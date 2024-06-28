using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : BaseUI
{
    [Header("UIs")]
    [SerializeField] private InLobbyUI inLobbyUI;

    [Header("Buttons")]
    [SerializeField] private Button hostLobbyButton;
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private Button quitButton;

    [Header("TextFields")]
    [SerializeField] private TextMeshProUGUI joinLobbyButtonText;

    [Header("InputFields")]
    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private TMP_InputField playerNameInputField;

    private void Start() {
        GameLobby.Instance.OnLobbyAction += GameLobby_OnLobbyAction;

        playerNameInputField.text = MultiplayerGameManager.Instance.GetPlayerName();
        playerNameInputField.onValueChanged.AddListener((string newText) => {
            MultiplayerGameManager.Instance.SetPlayerName(newText);
        });
    }

    private void GameLobby_OnLobbyAction(object sender, System.EventArgs e) {
        Hide();
        inLobbyUI.Show();
    }

    private void Awake() {
        hostLobbyButton.onClick.AddListener(() => {
            GameLobby.Instance.CreateLobby();
        });
        
        joinLobbyButton.onClick.AddListener(() => {
            GameLobby.Instance.JoinWithCode(joinCodeInputField.text);
        });

        quitButton.onClick.AddListener(() => {
            Application.Quit();
        });
    }

    private void Update() {
        if(joinCodeInputField.text.Length == 6) {
            joinLobbyButton.interactable = true;
            joinLobbyButtonText.color = new Color(joinLobbyButtonText.color.r, joinLobbyButtonText.color.g, joinLobbyButtonText.color.b, 1.0f);
        } else {
            joinLobbyButton.interactable = false;
            joinLobbyButtonText.color = new Color(joinLobbyButtonText.color.r, joinLobbyButtonText.color.g, joinLobbyButtonText.color.b, 0.5f);
        }
    }
}
