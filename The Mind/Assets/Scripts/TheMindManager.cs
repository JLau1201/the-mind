using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TheMindManager : NetworkBehaviour
{
    public static TheMindManager Instance { get; private set; }

    public event EventHandler OnPlayerHandGenerated;
    public event EventHandler<OnGameOverEventArgs> OnGameOver;

    public class OnGameOverEventArgs : EventArgs {
        public int gameWon;
    }

    [SerializeField] private CardHolder cardHolder;

    private float timer = 1;

    private float gameTime;

    private float gameTimeMax;
    private int cardsPerPlayer;
    private int numPlayers;
    private bool isUsingTimer;

    private List<int> allCardsList = new List<int>();
    private List<int> playerHand = new List<int>();
    private List<int> cardsRemaining = new List<int>();
    private int playerId;

    [SerializeField] private Transform playerInfoPrefab;
    [SerializeField] private Transform playerInfoContainer;

    private void Start() {
        cardHolder.OnPilePlaced += CardHolder_OnPilePlaced;
    }
    
    private void CardHolder_OnPilePlaced(object sender, CardHolder.OnPilePlacedEventArgs e) {
        CheckCardPlacedServerRpc(e.cardNumber, e.playerId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckCardPlacedServerRpc(int cardNumber, int playerId) {
        if (cardNumber == allCardsList[0]) {
            allCardsList.RemoveAt(0);
            cardsRemaining[playerId - 1]--;
            if (IsServer) {
                int[] cardsRemainingArray = cardsRemaining.ToArray();
                UpdatePlayerMenuClientRpc(cardsRemainingArray);
            }
        } else {
            Debug.Log("You Lose");
            OnGameOverClientRpc(0);
        }

        if (allCardsList.Count == 0) {
            Debug.Log("You Win");
            OnGameOverClientRpc(1);
        }
    }

    [ClientRpc]
    private void UpdatePlayerMenuClientRpc(int[] cardsRemaining) {
        Lobby lobby = LobbyManager.Instance.GetLobby();

        Debug.Log(lobby.Players.Count);
        Debug.Log(cardsRemaining.Length);

        foreach (Transform child in playerInfoContainer) {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < lobby.Players.Count; i++) {
            Player player = lobby.Players[i];
            Transform playerInfoSingle = Instantiate(playerInfoPrefab, playerInfoContainer);
            if (player.Data.TryGetValue("PlayerName", out PlayerDataObject playerDataObject)) {
                playerInfoSingle.gameObject.GetComponent<PlayerInfoSingle>().SetInfo(playerDataObject.Value, cardsRemaining[i]);
            }
        }
    }

    [ClientRpc]
    private void OnGameOverClientRpc(int gameWon) {
        OnGameOver?.Invoke(this, new OnGameOverEventArgs { gameWon = gameWon});
    }

    private void Awake() {
        Instance = this;
    }

    private bool isSpawned = false;

    private void Update() {
        timer -= Time.deltaTime;
        if (timer < 0) {
            if (!isSpawned) {
                if (IsServer) {
                    GenerateAllCardsList();
                    int[] allCardsArray = allCardsList.ToArray();
                    allCardsList.Sort();
                    GeneratePlayerHandsClientRpc(allCardsArray, cardsPerPlayer);

                    for (int i = 0; i < numPlayers; i++) { 
                        cardsRemaining.Add(cardsPerPlayer);
                    }

                    int[] arr = cardsRemaining.ToArray();
                    UpdatePlayerMenuClientRpc(arr);
                }
            }
            isSpawned = true;
        }
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        playerId = MultiplayerManager.Instance.GetPlayerId();
        SetGameVariables();
    }

    // Get unique set of cards from 1 - 100
    private void GenerateAllCardsList() {
        while (allCardsList.Count < cardsPerPlayer * numPlayers) {
            int newCard = UnityEngine.Random.Range(1, 100);

            if (!allCardsList.Contains(newCard)) {
                allCardsList.Add(newCard);
            }
        }
    }

    // Get game variables from settings
    private void SetGameVariables() {
        gameTimeMax = MultiplayerManager.Instance.GetGameTimer();
        cardsPerPlayer = MultiplayerManager.Instance.GetCardAmount();
        numPlayers = MultiplayerManager.Instance.GetNumPlayers();
        isUsingTimer = MultiplayerManager.Instance.GetIsTimerOn();
    }

    public float GetGameTimer() {
        return gameTime / gameTimeMax;
    }

    public List<int> GetPlayerHand() {
        return playerHand;
    }

    // Generate the players hand
    [ClientRpc]
    private void GeneratePlayerHandsClientRpc(int[] allCardsArray, int cardsPerPlayer) {
        int startIndex;
        if(playerId == 1) {
            startIndex = 0;
        } else {
            startIndex = cardsPerPlayer * (playerId - 1);
        }

        for(int i = startIndex; i < startIndex + cardsPerPlayer; i++) {
            playerHand.Add(allCardsArray[i]);
        }
        playerHand.Sort();
        playerHand.Reverse();
        OnPlayerHandGenerated?.Invoke(this, EventArgs.Empty);
    }
}
