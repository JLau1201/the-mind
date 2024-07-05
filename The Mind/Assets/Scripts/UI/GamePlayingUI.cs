using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayingUI : BaseUI
{
    private void Start() {
        TheMindManager.Instance.OnGameOver += TheMindManager_OnGameOver;
    }

    private void TheMindManager_OnGameOver(object sender, System.EventArgs e) {
        Hide();
    }
}
