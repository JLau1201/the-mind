using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class GameOverSettings : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cardAmountText;
    [SerializeField] private Button minusButton;
    [SerializeField] private Button plusButton;

    private int cardAmount;

    private void Awake() {
        if (!LobbyManager.Instance.IsLobbyHost()) {
            minusButton.interactable = false;
            plusButton.interactable = false;
        }

        cardAmount = MultiplayerManager.Instance.GetCardAmount();
        cardAmountText.text = cardAmount.ToString();

        minusButton.onClick.AddListener(() => {
            MinusButtonClickedClientRpc();
        });
        
        plusButton.onClick.AddListener(() => {
            PlusButtonClickedClientRpc();
        });
    }

    [ClientRpc]
    private void MinusButtonClickedClientRpc() {
        if (cardAmount > 1) {
            cardAmount--;
            cardAmountText.text = cardAmount.ToString();
            MultiplayerManager.Instance.SetCardAmount(cardAmount);
        }
    }

    [ClientRpc]
    private void PlusButtonClickedClientRpc() {
        if (cardAmount < 8) {
            cardAmount++;
            cardAmountText.text = cardAmount.ToString();
            MultiplayerManager.Instance.SetCardAmount(cardAmount);
        }
    }
}
