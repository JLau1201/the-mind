using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CardHolder : MonoBehaviour
{
    [Header("Card")]
    [SerializeField] private Transform cardPrefab;
    [SerializeField] private Button deselectButton;

    private List<int> playerHand = new List<int>();

    private void Start() {
        TheMindManager.Instance.OnPlayerHandGenerated += TheMindManager_OnPlayerHandGenerated;
    }

    private void Awake() {
        deselectButton.onClick.AddListener(() => {
            TheMindManager.Instance.SetSelectedCard(null);
        });
    }

    private void TheMindManager_OnPlayerHandGenerated(object sender, System.EventArgs e) {
        playerHand = TheMindManager.Instance.GetPlayerHand();

        foreach (int cardNumber in playerHand) {
            Transform newCardTransform = Instantiate(cardPrefab, transform);
            Card newCard = newCardTransform.GetComponent<Card>();
            newCard.SetCardNumber(cardNumber);
        }
    }
}
