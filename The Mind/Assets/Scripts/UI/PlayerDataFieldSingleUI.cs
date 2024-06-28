using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerDataFieldSingleUI : MonoBehaviour
{

    [Header("PlayerDataField")]
    [SerializeField] private TextMeshProUGUI playerNameText;

    public void SetPlayerDataField(string playerName) {
        playerNameText.text = playerName;
    }
}
