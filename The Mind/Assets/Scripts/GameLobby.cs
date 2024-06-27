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

public class GameLobby : MonoBehaviour
{

    public static GameLobby Instance { get; private set; }

    public event EventHandler OnLobbyAction;

    private Lobby lobby;
    private float heartbeatTimer;

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

    private async void InitializeUnityAuthentication() {
        if (UnityServices.State != ServicesInitializationState.Initialized) {

            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(UnityEngine.Random.Range(0, 10000).ToString());

            await UnityServices.InitializeAsync(initializationOptions);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

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

    public async void CreateLobby() {
        try {
            string lobbyName = "new lobby";
            int maxPlayers = 8;
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = true;

            lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            var callbacks = new LobbyEventCallbacks();
            callbacks.PlayerJoined += Callbacks_PlayerJoined;
            callbacks.PlayerLeft += Callbacks_PlayerLeft;
            try {
                var lobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }

            MultiplayerGameManager.Instance.StartHost();
            
            OnLobbyAction?.Invoke(this, EventArgs.Empty);
        }catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private void Callbacks_PlayerLeft(List<int> obj) {
        UpdateLobby();
    }

    private void Callbacks_PlayerJoined(List<LobbyPlayerJoined> obj) {
        UpdateLobby();
    }

    public async void JoinWithCode(string lobbyCode) {
        try {
            lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            var callbacks = new LobbyEventCallbacks();
            callbacks.PlayerJoined += Callbacks_PlayerJoined;
            callbacks.PlayerLeft += Callbacks_PlayerLeft;
            try {
                var lobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }

            MultiplayerGameManager.Instance.StartClient();

            OnLobbyAction?.Invoke(this, EventArgs.Empty);
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
