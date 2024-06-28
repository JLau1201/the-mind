using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerGameManager : NetworkBehaviour
{

    public const int MAX_PLAYER_SIZE = 8;
    private const string PLAYER_PREFS_PLAYER_NAME = "PlayerName";

    public static MultiplayerGameManager Instance { get; private set; }

    public event EventHandler OnSettingsChanged;

    private string playerName;

    private NetworkVariable<float> gameTime = new NetworkVariable<float>(60);
    private NetworkVariable<float> cardAmount = new NetworkVariable<float>(1);

    private void Awake() {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME, "Player" + UnityEngine.Random.Range(1, 100));
    }

    public string GetPlayerName() {
        return playerName;
    }

    public void SetPlayerName(string playerName) {
        this.playerName = playerName;
        PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME, playerName);
    }

    public void StartHost() {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse) {
        if(SceneManager.GetActiveScene().name != SceneLoader.Scene.MainMenu.ToString()) {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = " Game has already started";
            return;
        }
        if(NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_SIZE) {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = " Game is full";
            return;
        }
        connectionApprovalResponse.Approved = true;
    }

    public void StartClient() {
        NetworkManager.Singleton.StartClient();
    }

    public void Disconnect() {
        NetworkManager.Singleton.ConnectionApprovalCallback -= NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.Shutdown();
    }

    public float GetGameTime() {
        return gameTime.Value;
    }

    public void SetGameTime(float value) {
        gameTime.Value = value;
        OnSettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public float GetCardAmount() {
        return cardAmount.Value;
    }

    public void SetCardAmount(float value) {
        cardAmount.Value = value;
        OnSettingsChanged?.Invoke(this, EventArgs.Empty);
    }
}
