using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerDataFieldUI : MonoBehaviour
{
    [Header("IDK WHAT TO CALL THIS HEADER BUT I LOVE HEADERS")]
    [SerializeField] private Transform playerFieldTemplate;

    Lobby lobby;

    private void Start() {
        // Subscribe to joined/leave events
        // Some event calls UpdatePlayerField
        LobbyManager.Instance.OnLobbyAction += GameLobby_OnLobbyAction;
    }

    private void GameLobby_OnLobbyAction(object sender, System.EventArgs e) {
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
