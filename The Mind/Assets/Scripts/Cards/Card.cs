using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour 
{
    [SerializeField] private List<TextMeshProUGUI> cardNumberTexts;
    [SerializeField] private Button selectCardButton;

    private int cardNumber;

    private void Awake() {
        selectCardButton.onClick.AddListener(() => {
            TheMindManager.Instance.SetSelectedCard(this);
        });
    }

    public void SetCardNumber(int number) {
        cardNumber = number;
        foreach (TextMeshProUGUI text in cardNumberTexts) {
            text.text = number.ToString();
        }
    }

    public int GetCardNumber() {
        return cardNumber;
    }
}
