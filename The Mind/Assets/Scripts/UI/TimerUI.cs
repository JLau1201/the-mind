using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TimerUI : NetworkBehaviour
{
    [Header("Image")]
    [SerializeField] private Image timerImage;

    private void Update() {
        UpdateTimerClientRpc();
    }

    [ClientRpc]
    private void UpdateTimerClientRpc() {
        timerImage.fillAmount = TheMindManager.Instance.GetGameTimer();
    }
}
