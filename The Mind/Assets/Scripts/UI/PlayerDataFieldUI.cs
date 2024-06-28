using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataFieldUI : MonoBehaviour
{
    [Header("IDK WHAT TO CALL THIS HEADER BUT I LOVE HEADERS")]
    [SerializeField] private Transform playerFieldTemplate;

    private void Start() {
        // Subscribe to joined/leave events
        // Some event calls UpdatePlayerField
        GameLobby.Instance.OnLobbyAction += GameLobby_OnLobbyAction;
    }

    private void GameLobby_OnLobbyAction(object sender, System.EventArgs e) {
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

        for(int i = 0; i < GameLobby.Instance.GetLobby().Players.Count; i ++) {
            Transform playerFieldTransform = Instantiate(playerFieldTemplate, transform);
            playerFieldTransform.gameObject.SetActive(true);
            playerFieldTransform.GetComponent<PlayerDataFieldSingleUI>().SetPlayerDataField();
        }
    }
}
