using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{

    public event EventHandler<OnCardPlacedEventArgs> OnCardPlaced;
    public class OnCardPlacedEventArgs : EventArgs {
        public int cardNumber { get; set; }
    }

    [SerializeField] private TextMeshProUGUI cardNumberText;
    [SerializeField] private Button placeCardButton;

    private int cardNumber;

    private void Awake() {
        placeCardButton.onClick.AddListener(() => {
            OnCardPlaced?.Invoke(this, new OnCardPlacedEventArgs { cardNumber = cardNumber});
            Destroy(gameObject);
        });
    }

    public void SetCardNumber(int number) {
        cardNumber = number;
        cardNumberText.text = number.ToString();
    }
}
