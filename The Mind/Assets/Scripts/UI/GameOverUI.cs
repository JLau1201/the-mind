using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : BaseUI
{
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TextMeshProUGUI gameOverText;

    private void Start() {
        TheMindManager.Instance.OnGameOver += Instance_OnGameOver; ;
        Hide();
    }

    private void Instance_OnGameOver(object sender, TheMindManager.OnGameOverEventArgs e) {
        Show();
        if(e.gameWon == 1) {
            gameOverText.text = "nice. you won.";
        } else {
            gameOverText.text = "YOU LOSE LMAOOOOOOOOOOO!!!!!!!";
        }
    }

    private void Awake() {
        if (!LobbyManager.Instance.IsLobbyHost()) {
            playAgainButton.interactable = false;
        }

        playAgainButton.onClick.AddListener(() => {
            SceneLoader.LoadSceneNetwork(SceneLoader.Scene.Game);
        });
        
        quitButton.onClick.AddListener(() => {
            Application.Quit();
        });
    }
}
