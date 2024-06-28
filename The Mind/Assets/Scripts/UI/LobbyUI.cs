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

    private void Awake() {
        hostLobbyButton.onClick.AddListener(() => {
            LobbyManager.Instance.CreateLobby(playerNameInputField.text);

            Hide();
            inLobbyUI.Show();
        });
        
        joinLobbyButton.onClick.AddListener(() => {
            LobbyManager.Instance.JoinWithCode(joinCodeInputField.text, playerNameInputField.text);

            Hide();
            inLobbyUI.Show();
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
