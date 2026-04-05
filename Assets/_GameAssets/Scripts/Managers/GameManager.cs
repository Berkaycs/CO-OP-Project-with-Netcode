using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using System;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<GameState> OnGameStateChanged;
    
    [SerializeField] private GameDataSO _gameData;
    [SerializeField] private GameState _currentGameState;

    private NetworkVariable<int> _gameTimer = new NetworkVariable<int>(0);

    private void Awake()
    {
        Instance = this;
    }

    override public void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _gameTimer.Value = _gameData.GameTimer;
            SetTimerTextRpc();
            InvokeRepeating(nameof(DecreaseTimer), 1f, 1f);
        }

        _gameTimer.OnValueChanged += OnValueChanged;
    }

    private void OnValueChanged(int previousValue, int newValue)
    {
        TimerUI.Instance.SetTimer(newValue);

        if (IsServer && newValue <= 0)
        {
            ChangeGameState(GameState.GameOver);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetTimerTextRpc()
    {
        TimerUI.Instance.SetTimer(_gameTimer.Value);
    }

    private void DecreaseTimer()
    {
        if (IsServer && _currentGameState == GameState.Playing)
        {
            _gameTimer.Value--;
            SetTimerTextRpc();

            if (_gameTimer.Value <= 0)
            {
                CancelInvoke(nameof(DecreaseTimer));    
            }
        }
    }

    public void ChangeGameState(GameState newGameState)
    {
        if (!IsServer) return;
        _currentGameState = newGameState;
        ChangeGameStateRpc(newGameState);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ChangeGameStateRpc(GameState newGameState)
    {
        _currentGameState = newGameState;
        OnGameStateChanged?.Invoke(newGameState);
        Debug.Log($"Game state changed to {newGameState}");
    }

    public GameState GetCurrentGameState()
    {
        return _currentGameState;
    }
}
