using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerDataManager : NetworkBehaviour
{
    private void Start() {
        GameLobby.Instance.OnLobbyAction += GameLobby_OnLobbyAction;
    }

    private void GameLobby_OnLobbyAction(object sender, System.EventArgs e) {
        throw new System.NotImplementedException();
    }
}
