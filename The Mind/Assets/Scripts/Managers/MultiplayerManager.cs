using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class MultiplayerManager : NetworkBehaviour
{
    public static MultiplayerManager Instance { get; private set; }

    public event EventHandler OnConnectionApproved;

    private const int MAX_PLAYER_COUNT = 8;

    private int cardAmount = 1;

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    // *** NETWORK MANAGER ***
    public void StartHost() {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;

        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse) {
        if (SceneManager.GetActiveScene().name != SceneLoader.Scene.MainMenu.ToString()) {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game has already started!";
            return;
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_COUNT) {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Max numbers of players reached!";
        }
        connectionApprovalResponse.Approved = true;
        OnConnectionApproved?.Invoke(this, EventArgs.Empty);
    }

    public void StartClient() {
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;

        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId) {
        OnConnectionApproved?.Invoke(this, EventArgs.Empty);
    }

    public void Shutdown() {
        NetworkManager.Singleton.Shutdown();
        if (IsHost) {
            NetworkManager.Singleton.ConnectionApprovalCallback -= NetworkManager_ConnectionApprovalCallback;
        } else {
            NetworkManager.Singleton.OnClientConnectedCallback -= NetworkManager_OnClientConnectedCallback;
        }
    }

    // *** GAME MANAGER ***
    public int GetMaxPlayerCount() {
        return MAX_PLAYER_COUNT;
    }

    public int GetCardAmount() {
        return cardAmount;
    }

    public void SetCardAmount(int cardAmount) {
        this.cardAmount = cardAmount;
    }
}
