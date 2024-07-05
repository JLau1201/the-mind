using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Settings : NetworkBehaviour
{
    public static Settings Instance { get; private set; }

    [Header("TextFields")]
    [SerializeField] private TextMeshProUGUI timerNumberText;
    [SerializeField] private TextMeshProUGUI cardNumberText;

    [Header("Buttons")]
    [SerializeField] private Button timerMinusButton;
    [SerializeField] private Button timerAddButton;
    [SerializeField] private Button cardMinusButton;
    [SerializeField] private Button cardAddButton;
    [SerializeField] private Toggle timerOnToggle;

    private NetworkVariable<int> gameTimer = new NetworkVariable<int>(60);
    private NetworkVariable<int> cardAmount = new NetworkVariable<int>(1);
    private NetworkVariable<bool> isTimerOn = new NetworkVariable<bool>(false);
    private int timerIncrement = 10;

    private void Start() {
        LobbyManager.Instance.OnLobbyAction += GameLobby_OnLobbyAction;
    }

    private void GameLobby_OnLobbyAction(object sender, System.EventArgs e) {
        if (!LobbyManager.Instance.IsLobbyHost()) {
            timerMinusButton.interactable = false;
            timerAddButton.interactable = false;
            cardMinusButton.interactable = false;
            cardAddButton.interactable = false;
            timerOnToggle.interactable = false;
        }
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        if (IsServer) {
            timerMinusButton.onClick.AddListener(() => {
                if (gameTimer.Value > 10) {
                    gameTimer.Value -= timerIncrement;
                }
                timerNumberText.text = gameTimer.Value.ToString();
            });

            timerAddButton.onClick.AddListener(() => {
                gameTimer.Value += timerIncrement;
                timerNumberText.text = gameTimer.Value.ToString();
            });

            cardMinusButton.onClick.AddListener(() => {
                if(cardAmount.Value > 1) {
                    cardAmount.Value--;
                }
                cardNumberText.text = cardAmount.Value.ToString();
            });

            cardAddButton.onClick.AddListener(() => {
                if(cardAmount.Value < 8) {
                    cardAmount.Value++;
                }
                cardNumberText.text = cardAmount.Value.ToString();
            });

            timerOnToggle.onValueChanged.AddListener(delegate {
                if (LobbyManager.Instance.IsLobbyHost()) {
                    timerMinusButton.interactable = !timerMinusButton.interactable;
                    timerAddButton.interactable = !timerAddButton.interactable;
                }
                isTimerOn.Value = timerOnToggle.isOn;
            });
        } else {
            gameTimer.OnValueChanged += (oldVal, newVal) => {
                timerNumberText.text = newVal.ToString();
            };

            cardAmount.OnValueChanged += (oldVal, newVal) => {
                cardNumberText.text = newVal.ToString();
            };
            isTimerOn.OnValueChanged += (oldVal, newVal) => {
                timerOnToggle.isOn = newVal;
            };
        }
    }

    private void Awake() {
        Instance = this;
    }

    public int GetGameTimer() {
        return gameTimer.Value;
    }

    public int GetCardAmount() {
        return cardAmount.Value;
    }

    public bool GetIsTimerOn() {
        return isTimerOn.Value;
    }
}
