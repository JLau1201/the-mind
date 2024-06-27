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
    
    public static MultiplayerGameManager Instance { get; private set; }

    private void Awake() {
        Instance = this;

        DontDestroyOnLoad(gameObject);
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
}
