using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour {
    // Const string to reference for RelayJoinCode
    private const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";
    public static LobbyManager Instance { get; private set; }

    public event EventHandler OnLobbyUpdated;
    public event EventHandler OnLobbyJoinError;

    private Lobby lobby;
    // Heartbeat timer to keep lobby active
    private float heartbeatTimer;

    private void Start() {
        InitializeUnityAuthentication();
    }

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update() {
        // Keep lobby active with heartbeat
        HandleLobbyHeartbeat();
    }

    // ***LOBBY CODE***

    // Initialize Unity lobby authentication anonymously
    private async void InitializeUnityAuthentication() {
        if (UnityServices.State != ServicesInitializationState.Initialized) {

            InitializationOptions initializationOptions = new InitializationOptions();

            // Set profile for local testing
            // Lobby read by ip address --> set different profile for same ip testing
            initializationOptions.SetProfile(UnityEngine.Random.Range(0, 10000).ToString());

            await UnityServices.InitializeAsync(initializationOptions);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    // Lobby heartbeat to keep lobby active
    private async void HandleLobbyHeartbeat() {
        // Only keep lobby active if user is lobby host
        if (IsLobbyHost()) {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0f) {
                float heartbeatTimerMax = 15;
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
            }
        }
    }

    public bool IsLobbyHost() {
        return lobby != null && lobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    // Create Lobby
    public async void CreateLobby(string playerName) {
        try {
            // Set lobby parameters
            string lobbyName = "new lobby";
            int maxPlayers = MultiplayerManager.Instance.GetMaxPlayerCount();

            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = true;

            // Create lobby
            lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            UpdatePlayerData(playerName);

            // Create Relay allocation
            // Get the joincode from allocated Relay
            Allocation allocation = await AllocateRelay();
            string relayJoinCode = await GetRelayJoinCode(allocation);

            // Relay Server Stuff
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
            UpdateLobbyOptions newOptions = new UpdateLobbyOptions();
            newOptions.Data = new Dictionary<string, DataObject> {
                {KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
            };
            await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, newOptions);

            // Callback for lobby events
            var callbacks = new LobbyEventCallbacks();
            callbacks.PlayerDataAdded += Callbacks_PlayerDataAdded;
            callbacks.PlayerLeft += Callbacks_PlayerLeft;
            try {
                var lobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }

            // Start Host connection as Server/Client
            MultiplayerManager.Instance.StartHost();
        } catch (LobbyServiceException e) {
            OnLobbyJoinError?.Invoke(this, EventArgs.Empty);
            Debug.Log(e);
        }
    }

    // Join lobby using lobby code
    public async void JoinWithCode(string lobbyCode, string playerName) {
        try {
            // Join lobby with code
            lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            UpdatePlayerData(playerName);

            // Get Relay join code
            // Join relay
            string relayJoinCode = lobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            // Relay server stuff
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            // Callback for lobby events
            var callbacks = new LobbyEventCallbacks();
            callbacks.PlayerDataAdded += Callbacks_PlayerDataAdded;
            callbacks.PlayerLeft += Callbacks_PlayerLeft;
            try {
                var lobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }

            // Start Client connection to Server
            MultiplayerManager.Instance.StartClient();
        } catch (LobbyServiceException e) {
            OnLobbyJoinError?.Invoke(this, EventArgs.Empty);
            Debug.Log(e);
        }
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

    private void Callbacks_PlayerDataAdded(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> obj) {
        UpdateLobby();
    }

    // Update lobby when players join/leave
    private void Callbacks_PlayerLeft(List<int> obj) {
        UpdateLobby();
    }

    public async void UpdateLobby() {
        try {
            // Update lobby by repolling for lobby data
            lobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);

            OnLobbyUpdated?.Invoke(this, EventArgs.Empty);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public async void CloseLobby() {
        try {
            // Delete Lobby
            await LobbyService.Instance.DeleteLobbyAsync(lobby.Id);
            lobby = null;

            // Shutdown Network Connection
            MultiplayerManager.Instance.Shutdown();
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public async void LeaveLobby() {
        try {
            // Poll for playerId
            // Remove playerId from lobby
            string playerId = AuthenticationService.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync(lobby.Id, playerId);

            OnLobbyUpdated?.Invoke(this, EventArgs.Empty);

            // Shutdown NetworkConnection
            MultiplayerManager.Instance.Shutdown();
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public Lobby GetLobby() {
        return lobby;
    }

    // ***RELAY CODE***
    private async Task<Allocation> AllocateRelay() {
        try {
            int maxPlayers = MultiplayerManager.Instance.GetMaxPlayerCount() - 1;

            // Allocate Relay Service
            // Parameters are max connections to relay (not including relay host)
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);

            return allocation;
        } catch (RelayServiceException e) {
            Debug.Log(e);

            return default;
        }
    }

    private async Task<string> GetRelayJoinCode(Allocation allocation) {
        try {
            // Fetch for allocated Relay service code
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayJoinCode;
        } catch (RelayServiceException e) {
            Debug.Log(e);
            return default;
        }
    }

    private async Task<JoinAllocation> JoinRelay(string joinCode) {
        try {
            // Join Relay connection with Relay code
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            return joinAllocation;
        } catch (RelayServiceException e) {
            Debug.Log(e);
            return default;
        }
    }
}
