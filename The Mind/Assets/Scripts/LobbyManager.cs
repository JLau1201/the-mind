using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{

    public static LobbyManager Instance { get; private set; }

    public event EventHandler OnLobbyAction;

    private Lobby lobby;
    private float heartbeatTimer;

    private bool isNewPlayer;

    private void Start() {
        InitializeUnityAuthentication();
    }

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update() {
        HandleLobbyHeartbeat();
    }

    // Initialize Unity lobby authentication anonymously
    private async void InitializeUnityAuthentication() {
        if (UnityServices.State != ServicesInitializationState.Initialized) {

            InitializationOptions initializationOptions = new InitializationOptions();
            
            // Set profile for local testing
            initializationOptions.SetProfile(UnityEngine.Random.Range(0, 10000).ToString());

            await UnityServices.InitializeAsync(initializationOptions);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    // Lobby heartbeat to keep lobby active
    private async void HandleLobbyHeartbeat() {
        if(IsLobbyHost()) {
            heartbeatTimer -= Time.deltaTime;
            if(heartbeatTimer <= 0f) {
                float heartbeatTimerMax = 15;
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
            }
        }
    }

    public bool IsLobbyHost() {
        return lobby != null && lobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    public async void CreateLobby(string playerName) {
        try {
            // Set lobby parameters
            string lobbyName = "new lobby";
            int maxPlayers = 8;                                     
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = true;

            // Create lobby
            lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            UpdatePlayerData(playerName);
            
            // Callbacks for lobby events
            var callbacks = new LobbyEventCallbacks();
            callbacks.PlayerDataAdded += Callbacks_PlayerDataAdded;
            callbacks.PlayerLeft += Callbacks_PlayerLeft;
            try {
                var lobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }

            NetworkManager.Singleton.StartHost();

        }catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public async void JoinWithCode(string lobbyCode, string playerName) {
        try {
            // Join lobby with code
            lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            
            UpdatePlayerData(playerName);

            // Callback for lobby events
            var callbacks = new LobbyEventCallbacks();
            callbacks.PlayerDataAdded += Callbacks_PlayerDataAdded;
            callbacks.PlayerLeft += Callbacks_PlayerLeft;
            try {
                var lobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }

            NetworkManager.Singleton.StartClient();

        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private void Callbacks_PlayerDataAdded(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> obj) {
        UpdateLobby();
    }

    // Update lobby when players join/leave
    private void Callbacks_PlayerLeft(List<int> obj) {
        UpdateLobby();
    }

    public async void UpdatePlayerData(string playerName) {
        try {
            UpdatePlayerOptions options = new UpdatePlayerOptions();
            options.Data = new Dictionary<string, PlayerDataObject>(){
                {
                    "PlayerName", new PlayerDataObject(
                        visibility: PlayerDataObject.VisibilityOptions.Public,
                        value: playerName)
                },
            };

            string playerId = AuthenticationService.Instance.PlayerId;

            lobby = await LobbyService.Instance.UpdatePlayerAsync(lobby.Id, playerId, options);

            UpdateLobby();

        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }


    public async void CloseLobby() {
        try {
            await LobbyService.Instance.DeleteLobbyAsync(lobby.Id);
            lobby = null;
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public async void LeaveLobby() {
        try {
            string playerId = AuthenticationService.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync(lobby.Id, playerId);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public async void UpdateLobby() {
        try {
            lobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);

            OnLobbyAction?.Invoke(this, EventArgs.Empty);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public Lobby GetLobby() {
        return lobby;
    }
}
