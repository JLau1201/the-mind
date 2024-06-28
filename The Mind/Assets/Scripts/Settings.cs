using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Settings : NetworkBehaviour
{
    [Header("TextFields")]
    [SerializeField] private TextMeshProUGUI timerNumberText;
    [SerializeField] private TextMeshProUGUI cardNumberText;

    [Header("Buttons")]
    [SerializeField] private Button timerMinusButton;
    [SerializeField] private Button timerAddButton;
    [SerializeField] private Button cardMinusButton;
    [SerializeField] private Button cardAddButton;
    [SerializeField] private Toggle timerOnToggle;

    private float gameTimer = 60;
    private float cardAmount = 1;
    private float timerIncrement = 10f;

    private void Start() {
        LobbyManager.Instance.OnLobbyAction += GameLobby_OnLobbyAction;
    }

    private void GameLobby_OnLobbyAction(object sender, System.EventArgs e) {
        if (!LobbyManager.Instance.IsLobbyHost()) {
            timerMinusButton.interactable = false;
            timerAddButton.interactable = false;
            cardMinusButton.interactable = false;
            cardAddButton.interactable = false;
            timerOnToggle.interactable = false;
        }
    }

    private void Awake() {
        timerMinusButton.onClick.AddListener(() => {
            UpdateTimeClientRpc(-1);
        });
        
        timerAddButton.onClick.AddListener(() => {
            UpdateTimeClientRpc(1);
        });
        
        cardMinusButton.onClick.AddListener(() => {
            UpdateCardAmountClientRpc(-1);
        });
        
        cardAddButton.onClick.AddListener(() => {
            UpdateCardAmountClientRpc(1);
        });

        timerOnToggle.onValueChanged.AddListener(delegate {
            if (LobbyManager.Instance.IsLobbyHost()) {
                timerMinusButton.interactable = !timerMinusButton.interactable;
                timerAddButton.interactable = !timerAddButton.interactable;
            }
            UpdateTimerToggleClientRpc();
        });
    }

    [ClientRpc]
    private void UpdateTimeClientRpc(int sign) {
        if(sign < 0 && gameTimer > 10) {
            gameTimer -= timerIncrement;
        } else if(sign > 0){
            gameTimer += timerIncrement;
        }
        timerNumberText.text = gameTimer.ToString();
    }

    [ClientRpc]
    private void UpdateCardAmountClientRpc(int sign) {
        if(sign < 0 && cardAmount > 1) {
            cardAmount --;
        } else if(sign > 0 && cardAmount < 8) {
            cardAmount++;
        }
        cardNumberText.text = cardAmount.ToString();
    }

    [ClientRpc]
    private void UpdateTimerToggleClientRpc() {
        if (!LobbyManager.Instance.IsLobbyHost()) {
            timerOnToggle.isOn = !timerOnToggle.isOn;
        }
        if (timerOnToggle.isOn) {
            timerNumberText.color = new Color(0, 0, 0, 1f);
        } else {
            timerNumberText.color = new Color(0, 0, 0, .5f);
        }
    }
}
