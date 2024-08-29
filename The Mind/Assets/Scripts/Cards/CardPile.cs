using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CardPile : NetworkBehaviour
{
    public static CardPile Instance { get; private set; }

    public event EventHandler<OnCardPlacedEventArgs> onCardPlaced;
    public class OnCardPlacedEventArgs : EventArgs {
        public int cardNumber;
    }

    [SerializeField] private CardHolder cardHolder;
    [SerializeField] private Transform cardPrefab;
    [SerializeField] private Button placeCardButton;

    private Card placedCard;

    private void Awake() {
        Instance = this;
        placeCardButton.onClick.AddListener(() => {
            placedCard = TheMindManager.Instance.GetSelectedCard();
            if(placedCard != null) {
                int cardNumber = placedCard.GetCardNumber();

                Destroy(placedCard.gameObject);
                TheMindManager.Instance.SetSelectedCard(null);
                SpawnCardServerRpc(cardNumber);

                onCardPlaced?.Invoke(this, new OnCardPlacedEventArgs {
                    cardNumber = cardNumber,
                });
            }
        });
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnCardServerRpc(int cardNumber) {
        SpawnCardClientRpc(cardNumber);
    }
    
    [ClientRpc]
    private void SpawnCardClientRpc(int cardNumber) {
        Transform newCardTransform = Instantiate(cardPrefab, transform);
        Card newCard = newCardTransform.GetComponent<Card>();
        newCard.SetCardNumber(cardNumber);
        Destroy(newCardTransform.GetComponentInChildren<Button>().gameObject);
    }
}
