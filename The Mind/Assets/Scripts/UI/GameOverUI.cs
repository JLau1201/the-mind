using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : BaseUI
{
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private GameObject victoryImage;
    [SerializeField] private GameObject defeatImage;

    [SerializeField] private TextMeshProUGUI numCardsText;
    [SerializeField] private Slider slider;

    private void Start() {
        TheMindManager.Instance.OnGameOver += Instance_OnGameOver;
        Hide();
    }

    private void Instance_OnGameOver(object sender, TheMindManager.OnGameOverEventArgs e) {
        numCardsText.text = MultiplayerManager.Instance.GetCardAmount().ToString();
        slider.value = MultiplayerManager.Instance.GetCardAmount();

        Show();
        if(e.gameWon == 1) {
            victoryImage.SetActive(true);
        } else {
            defeatImage.SetActive(true);    
        }
    }

    private void Awake() {
        if (!LobbyManager.Instance.IsLobbyHost()) {
            playAgainButton.interactable = false;
        }

        playAgainButton.onClick.AddListener(() => {
            MultiplayerManager.Instance.SetCardAmount((int)slider.value);
            SceneLoader.LoadSceneNetwork(SceneLoader.Scene.Game);
        });

        quitButton.onClick.AddListener(() => {
            Application.Quit();
        });

        slider.onValueChanged.AddListener(delegate { UpdateNumCardsTextClientRpc(slider.value); });
    }

    [ClientRpc]
    private void UpdateNumCardsTextClientRpc(float numCards) {
        numCardsText.text = numCards.ToString();
    }
}
