using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerManager : MonoBehaviour
{

    private const int MAX_PLAYER_COUNT = 8;

    private bool isTimerOn = true;
    private int gameTimer = 60;
    private int cardAmount = 1;
    private int numPlayers = 1;

    private int playerId = 0;

    public static MultiplayerManager Instance { get; private set; }

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public int GetMaxPlayerCount() {
        return MAX_PLAYER_COUNT;
    }

    public bool GetIsTimerOn() {
        return isTimerOn;
    }

    public int GetGameTimer() {
        return gameTimer;
    }

    public int GetCardAmount() {
        return cardAmount;
    }

    public int GetNumPlayers() {
        return numPlayers;
    }

    public int GetPlayerId() {
        return playerId;
    }

    public void SetIsTimerOn(bool newIsUsingTimer) {
        isTimerOn = newIsUsingTimer;
    }

    public void SetGameTimer(int newGameTimer) {
        gameTimer = newGameTimer;
    }

    public void SetCardAmount(int newCardAmount) {
        cardAmount = newCardAmount;
    }

    public void SetNumPlayers(int newNumPlayers) {
        numPlayers = newNumPlayers;
    }

    public void SetPlayerId(int newPlayerId) {
        playerId = newPlayerId;
    }
}
