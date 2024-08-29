using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : BaseUI
{

    [Header("UIs")]
    [SerializeField] private PreLobbyUI preLobbyUI;

    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button startButton;

    [Header("TextFields")]
    [SerializeField] private TextMeshProUGUI playerJoinedCountText;
    [SerializeField] private TextMeshProUGUI numCardsText;

    [Header("InputFields")]
    [SerializeField] private TMP_InputField lobbyCode;

    [Header("Transforms")]
    [SerializeField] private Transform playerDataSingle;
    [SerializeField] private Transform playerDataField;

    [Header("Slieders")]
    [SerializeField] private Slider slider;

    private Lobby lobby;

    private void Start() {
        LobbyManager.Instance.OnLobbyUpdated += LobbyManager_OnLobbyUpdated;
        Hide();
    }

    private void Awake() {
        backButton.onClick.AddListener(() => {
            LobbyManager.Instance.LeaveLobby();
            preLobbyUI.Show();
            Hide();
        });

        startButton.onClick.AddListener(() => {
            MultiplayerManager.Instance.SetCardAmount((int)slider.value);
            SceneLoader.LoadSceneNetwork(SceneLoader.Scene.Game);
        });

        slider.onValueChanged.AddListener(delegate { UpdateNumCardsTextClientRpc(slider.value); });
    }

    [ClientRpc]
    private void UpdateNumCardsTextClientRpc(float numCards) {
        numCardsText.text = numCards.ToString();
    }

    private void LobbyManager_OnLobbyUpdated(object sender, System.EventArgs e) {
        lobby = LobbyManager.Instance.GetLobby();
        lobbyCode.text = lobby.LobbyCode;

        if (!LobbyManager.Instance.IsLobbyHost()) {
            startButton.interactable = false;
            slider.interactable = false;
        }

        playerJoinedCountText.text = lobby.Players.Count + "/" + MultiplayerManager.Instance.GetMaxPlayerCount();
    }

    public override void OnDestroy() {
        base.OnDestroy();

        LobbyManager.Instance.OnLobbyUpdated -= LobbyManager_OnLobbyUpdated;
    }
}
