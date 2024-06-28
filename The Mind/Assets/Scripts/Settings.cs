using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [Header("TextFields")]
    [SerializeField] private TextMeshProUGUI timeNumberText;
    [SerializeField] private TextMeshProUGUI cardNumberText;

    [Header("Buttons")]
    [SerializeField] private Button timeMinusButton;
    [SerializeField] private Button timeAddButton;
    [SerializeField] private Button cardMinusButton;
    [SerializeField] private Button cardAddButton;

    private float timeIncrement = 10f;

    private void Start() {
        GameLobby.Instance.OnLobbyAction += GameLobby_OnLobbyAction;
        MultiplayerGameManager.Instance.OnSettingsChanged += MultiPlayerGameManager_OnSettingsChanged;
        timeNumberText.text = MultiplayerGameManager.Instance.GetGameTime().ToString();
        cardNumberText.text = MultiplayerGameManager.Instance.GetCardAmount().ToString();
    }

    private void MultiPlayerGameManager_OnSettingsChanged(object sender, System.EventArgs e) {
        timeNumberText.text = MultiplayerGameManager.Instance.GetGameTime().ToString();
        cardNumberText.text = MultiplayerGameManager.Instance.GetCardAmount().ToString();
    }

    private void GameLobby_OnLobbyAction(object sender, System.EventArgs e) {
        if (!GameLobby.Instance.IsLobbyHost()) {
            timeMinusButton.interactable = false;
            timeAddButton.interactable = false;
            cardMinusButton.interactable = false;
            cardAddButton.interactable = false;
        }
    }

    private void Awake() {
        timeMinusButton.onClick.AddListener(() => {
            AdjustTime(-1);
        });
        
        timeAddButton.onClick.AddListener(() => {
            AdjustTime(1);
        });
        
        cardMinusButton.onClick.AddListener(() => {
            AdjustCardAmount(-1);
        });
        
        cardAddButton.onClick.AddListener(() => {
            AdjustCardAmount(1);
        });
    }

    private void AdjustTime(int sign) {
        float gameTime = MultiplayerGameManager.Instance.GetGameTime();
        if(gameTime > 10 || sign == 1) {
            gameTime += sign * timeIncrement;
            MultiplayerGameManager.Instance.SetGameTime(gameTime);
        }
    }

    private void AdjustCardAmount(int sign) {
        float cardAmount = MultiplayerGameManager.Instance.GetCardAmount();
        if(cardAmount > 1 || sign == 1) {
            cardAmount += sign * 1;
            MultiplayerGameManager.Instance.SetCardAmount(cardAmount);
        }
    }
}
