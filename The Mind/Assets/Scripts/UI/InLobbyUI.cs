using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class InLobbyUI : BaseUI
{

    [Header("UIs")]
    [SerializeField] private LobbyUI lobbyUI;

    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button startButton;

    [Header("TextFields")]
    [SerializeField] private TextMeshProUGUI playerJoinedCountText;
    [SerializeField] private TMP_InputField lobbyCodeInputField;

    private Lobby lobby;

    private void Start() {
        GameLobby.Instance.OnLobbyAction += GameLobby_OnLobbyAction;
        Hide();
    }

    private void GameLobby_OnLobbyAction(object sender, System.EventArgs e) {
        lobby = GameLobby.Instance.GetLobby();
        lobbyCodeInputField.text = lobby.LobbyCode;
        playerJoinedCountText.text = lobby.Players.Count + "/8";
    }

    private void Awake() {
        backButton.onClick.AddListener(() => {
            Hide();
            lobbyUI.Show();
            MultiplayerGameManager.Instance.Disconnect();

            if (GameLobby.Instance.IsLobbyHost()) {
                GameLobby.Instance.CloseLobby();
            } else {
                GameLobby.Instance.LeaveLobby();
            }
        });

        startButton.onClick.AddListener(() => {
            SceneLoader.LoadNetwork(SceneLoader.Scene.Game);
        });
    }
}
