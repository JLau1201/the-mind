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

    private int cardsPerPlayer;
    private int numPlayers;

    private List<int> allCardsList = new List<int>();
    private List<int> playerHand = new List<int>();
    private List<int> cardsRemaining = new List<int>();

    private Dictionary<int, int> playerClientId = new Dictionary<int, int>();

    private int playerId;

    private Card selectedCard = null;

    [SerializeField] private Transform playerInfoPrefab;
    [SerializeField] private Transform playerInfoContainer;

    private void Start() {
        CardPile.Instance.onCardPlaced += CardPile_onCardPlaced;
    }

    private void CardPile_onCardPlaced(object sender, CardPile.OnCardPlacedEventArgs e) {
        int cardNumber = e.cardNumber;
        CheckCardPlacedServerRpc(cardNumber, playerId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckCardPlacedServerRpc(int cardNumber, int playerId) {
        if (cardNumber == allCardsList[0]) {
            allCardsList.RemoveAt(0);
            cardsRemaining[playerId]--;
            int[] cardsRemainingArray = cardsRemaining.ToArray();
            UpdatePlayerMenuClientRpc(cardsRemainingArray);
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

        foreach (Transform child in playerInfoContainer) {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < lobby.Players.Count; i++) {
            Player player = lobby.Players[i];
            Transform playerInfoSingle = Instantiate(playerInfoPrefab, playerInfoContainer);
            if (player.Data.TryGetValue("PlayerName", out PlayerDataObject playerDataObject)) {
                playerInfoSingle.gameObject.GetComponent<PlayerInfoSingleUI>().SetInfo(playerDataObject.Value, cardsRemaining[i]);
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

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        if (NetworkManager.Singleton.IsHost) {
            int id = 0;
            foreach (int clientId in NetworkManager.Singleton.ConnectedClientsIds) {
                playerClientId.Add(clientId, id);
                id++;
            }

            foreach(KeyValuePair<int, int> entry in playerClientId) {
                SetPlayerIdClientRpc(entry.Key, entry.Value);
            }

            SetGameVariables();
            GenerateAllCardsList();
            for(int i = 0; i < LobbyManager.Instance.GetLobby().Players.Count; i++) {
                cardsRemaining.Add(cardsPerPlayer);
            }
        }
    }

    [ClientRpc]
    private void SetPlayerIdClientRpc(int clientId, int playerId) {
        if((ulong)clientId == NetworkManager.Singleton.LocalClientId) {
            this.playerId = playerId;
        }
    }


    // Get game variables from settings
    private void SetGameVariables() {
        cardsPerPlayer = MultiplayerManager.Instance.GetCardAmount();
        numPlayers = LobbyManager.Instance.GetLobby().Players.Count;
    }

    // Get unique set of cards from 1 - 100
    private void GenerateAllCardsList() {
        while (allCardsList.Count < cardsPerPlayer * numPlayers) {
            int newCard = UnityEngine.Random.Range(1, 100);

            if (!allCardsList.Contains(newCard)) {
                allCardsList.Add(newCard);
            }
        }

        int[] allCardsArray = allCardsList.ToArray();
        GeneratePlayerHandsClientRpc(allCardsArray, cardsPerPlayer);
        allCardsList.Sort();
    }

    // Generate the players hand
    [ClientRpc]
    private void GeneratePlayerHandsClientRpc(int[] allCardsArray, int cardsPerPlayer) {
        int startInd = playerId * cardsPerPlayer;
        for(int i = startInd; i < startInd + cardsPerPlayer; i++) {
            playerHand.Add(allCardsArray[i]);
        }

        playerHand.Sort();
        playerHand.Reverse();

        StartCoroutine(WaitForSomething());
    }

    IEnumerator WaitForSomething() {
        yield return new WaitForSeconds(1);
        UpdatePlayerMenuClientRpc(cardsRemaining.ToArray());
        OnPlayerHandGenerated?.Invoke(this, EventArgs.Empty);
    }

    public List<int> GetPlayerHand() {
        return playerHand;
    }

    public void SetSelectedCard(Card card) {
        selectedCard = card;
    }

    public Card GetSelectedCard() {
        return selectedCard;
    }
}
