using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CardPile : NetworkBehaviour
{
    [SerializeField] private CardHolder cardHolder;
    [SerializeField] private Transform cardPrefab;

    private void Start() {
        cardHolder.OnPilePlaced += CardHolder_OnPilePlaced;
    }

    private void CardHolder_OnPilePlaced(object sender, CardHolder.OnPilePlacedEventArgs e) {
        SpawnCardServerRpc(e.cardNumber);
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
