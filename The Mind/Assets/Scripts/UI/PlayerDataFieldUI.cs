using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerDataFieldUI : MonoBehaviour {
    [Header("Transforms")]
    [SerializeField] private Transform playerFieldTemplate;

    Lobby lobby;

    private void Start() {
        LobbyManager.Instance.OnLobbyUpdated += LobbyManager_OnLobbyUpdated;
    }

    private void LobbyManager_OnLobbyUpdated(object sender, System.EventArgs e) {
        lobby = LobbyManager.Instance.GetLobby();
        UpdatePlayerField();
    }

    private void Awake() {
        playerFieldTemplate.gameObject.SetActive(false);
    }

    private void UpdatePlayerField() {
        foreach (Transform child in transform) {
            if (child == playerFieldTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (Player player in lobby.Players) {
            if (player.Data.TryGetValue("PlayerName", out PlayerDataObject playerDataObject)) {
                string playerName;
                playerName = playerDataObject.Value;

                Transform playerFieldTransform = Instantiate(playerFieldTemplate, transform);
                playerFieldTransform.gameObject.SetActive(true);
                playerFieldTransform.GetComponent<PlayerDataFieldSingleUI>().SetPlayerDataField(playerName);
            }

        }
    }
}