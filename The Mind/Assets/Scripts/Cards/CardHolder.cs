using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CardHolder : MonoBehaviour
{
    public event EventHandler<OnPilePlacedEventArgs> OnPilePlaced;
    public class OnPilePlacedEventArgs : EventArgs {
        public int cardNumber { get; set; }
        public int playerId;
    }

    [Header("Card")]
    [SerializeField] private Transform cardPrefab;

    private List<int> playerHand = new List<int>();

    private void Start() {
        TheMindManager.Instance.OnPlayerHandGenerated += TheMindManager_OnPlayerHandGenerated;
    }

    private void TheMindManager_OnPlayerHandGenerated(object sender, System.EventArgs e) {
        playerHand = TheMindManager.Instance.GetPlayerHand();

        foreach (int cardNumber in playerHand) {
            Transform newCardTransform = Instantiate(cardPrefab, transform);
            Card newCard = newCardTransform.GetComponent<Card>();
            newCard.SetCardNumber(cardNumber);
            newCard.OnCardPlaced += NewCard_OnCardPlaced;
        }
    }

    private void NewCard_OnCardPlaced(object sender, Card.OnCardPlacedEventArgs e) {
        OnPilePlaced?.Invoke(this, new OnPilePlacedEventArgs { cardNumber = e.cardNumber, playerId = MultiplayerManager.Instance.GetPlayerId() });
    }
}
