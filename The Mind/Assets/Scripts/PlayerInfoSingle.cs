using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInfoSingle : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI cardsRemainingText;

    public void SetInfo(string playerName, int cardsRemaining) {
        playerNameText.text = playerName;
        cardsRemainingText.text = cardsRemaining.ToString();
    }
}
