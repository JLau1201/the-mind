using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PreLobbyUI : BaseUI
{
    [Header("UIs")]
    [SerializeField] private LobbyUI lobbyUI;
    [SerializeField] private ConnectingUI connectingUI;

    [Header("Buttons")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button quitButton;

    [Header("TextFields")]
    [SerializeField] private TextMeshProUGUI joinLobbyButtonText;

    [Header("InputFields")]
    [SerializeField] private TMP_InputField lobbyCodeInputField;
    [SerializeField] private TMP_InputField playerNameInputField;

    private void Start() {
        MultiplayerManager.Instance.OnConnectionApproved += MultiplayerManager_OnConnectionApproved;
        LobbyManager.Instance.OnLobbyJoinError += LobbyManager_OnLobbyJoinError;
    }

    private void LobbyManager_OnLobbyJoinError(object sender, System.EventArgs e) {
        connectingUI.Hide();
    }

    private void MultiplayerManager_OnConnectionApproved(object sender, System.EventArgs e) {
        Hide();
        connectingUI.Hide();
        lobbyUI.Show();
    }

    private void Awake() {
        hostButton.onClick.AddListener(() => {
            string playerName = playerNameInputField.text;
            connectingUI.Show();
            LobbyManager.Instance.CreateLobby(playerNameInputField.text);
        });
        
        joinButton.onClick.AddListener(() => {
            string playerName = playerNameInputField.text;
            string lobbyCode = lobbyCodeInputField.text;
            connectingUI.Show();
            LobbyManager.Instance.JoinWithCode(lobbyCode, playerNameInputField.text);
        });

        quitButton.onClick.AddListener(() => {
            Application.Quit();
        });

        // Check for valid lobby code
        lobbyCodeInputField.onValueChanged.AddListener(delegate {
            string lobbyCode = lobbyCodeInputField.text;

            if (lobbyCode.Length == 6) {
                joinButton.interactable = true;
            } else {
                joinButton.interactable = false;
            }
        });
    }

    public override void OnDestroy() {
        base.OnDestroy();
        MultiplayerManager.Instance.OnConnectionApproved -= MultiplayerManager_OnConnectionApproved;
        LobbyManager.Instance.OnLobbyJoinError -= LobbyManager_OnLobbyJoinError;
    }
}
