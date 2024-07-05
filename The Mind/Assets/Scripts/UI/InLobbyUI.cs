using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
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
    [SerializeField] private TextMeshProUGUI startButtonText;
    [SerializeField] private TMP_InputField lobbyCodeInputField;

    private Lobby lobby;

    private void Start() {
        LobbyManager.Instance.OnLobbyAction += GameLobby_OnLobbyAction;
        Hide();
    }

    private void GameLobby_OnLobbyAction(object sender, System.EventArgs e) {
        lobby = LobbyManager.Instance.GetLobby();
        lobbyCodeInputField.text = lobby.LobbyCode;
        playerJoinedCountText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;

        if(MultiplayerManager.Instance.GetPlayerId() == 0) {
            MultiplayerManager.Instance.SetPlayerId(lobby.Players.Count);
        }

        if (!LobbyManager.Instance.IsLobbyHost()) {
            startButton.interactable = false;
            startButtonText.color = new Color(startButtonText.color.r, startButtonText.color.g, startButtonText.color.b, .5f);
        }
    }

    private void Awake() {
        backButton.onClick.AddListener(() => {
            Hide();
            lobbyUI.Show();

            if (LobbyManager.Instance.IsLobbyHost()) {
                LobbyManager.Instance.CloseLobby();
            } else {
                LobbyManager.Instance.LeaveLobby();
            }
        });

        startButton.onClick.AddListener(() => {
            MultiplayerManager.Instance.SetIsTimerOn(Settings.Instance.GetIsTimerOn());
            MultiplayerManager.Instance.SetGameTimer(Settings.Instance.GetGameTimer());
            MultiplayerManager.Instance.SetCardAmount(Settings.Instance.GetCardAmount());
            MultiplayerManager.Instance.SetNumPlayers(lobby.Players.Count);

            SceneLoader.LoadSceneNetwork(SceneLoader.Scene.Game);
        });
    }
}
