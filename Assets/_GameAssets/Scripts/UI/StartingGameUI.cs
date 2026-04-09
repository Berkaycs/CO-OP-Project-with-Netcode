using UnityEngine;
using Unity.Netcode;
using TMPro;
using DG.Tweening;
using System.Collections;
using System;

public class StartingGameUI : NetworkBehaviour
{
    public static StartingGameUI Instance { get; private set; }

    public event Action OnAllPlayersConnected;

    [Header("References")]
    [SerializeField] private TMP_Text _countdownText;

    [Header("Settings")]
    [SerializeField] private float _animationDuration;
    [SerializeField] private float _waitingDuration = 1f;

    private NetworkVariable<int> _playersLoaded = new NetworkVariable<int>
    (
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private void Awake()
    {
        Instance = this;
    }

    override public void OnNetworkSpawn()
    {
        if (IsClient)
        {
            SetPlayerLoadedRpc();
        }

        if (IsServer)
        {
            OnSinglePlayerConnected();
            _playersLoaded.OnValueChanged += OnPlayersLoadedChanged;
        }
    }

    private void OnPlayersLoadedChanged(int oldPlayerCount, int newPlayerCount)
    {
        if (IsServer && newPlayerCount == NetworkManager.Singleton.ConnectedClientsList.Count)
        {
            StartCountdownRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void SetPlayerLoadedRpc()
    {
        _playersLoaded.Value++;
        Debug.Log($"Players loaded: {_playersLoaded.Value}");
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartCountdownRpc()
    {
        OnAllPlayersConnected?.Invoke();
        StartCoroutine(CountdownCoroutine());
    }

    private void OnSinglePlayerConnected()
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 1)
        {
            StartCoroutine(CountdownCoroutine());
            WaitingForPlayersUI.Instance.Hide();
        }
    }

    private IEnumerator CountdownCoroutine()
    {
        _countdownText.gameObject.SetActive(true);

        for (int i = 3; i > 0; i--)
        {
            _countdownText.text = i.ToString();
            AnimateText();
            yield return new WaitForSeconds(_waitingDuration);
        }

        GameManager.Instance.ChangeGameState(GameState.Playing);
        _countdownText.text = "GO!";
        yield return new WaitForSeconds(_waitingDuration);

        _countdownText.transform.DOScale(0, _animationDuration / 2).SetEase(Ease.OutQuart).OnComplete(() =>
        {
            _countdownText.gameObject.SetActive(false);
        });
    }

    private void AnimateText()
    {
        _countdownText.transform.localScale = Vector3.zero;
        _countdownText.transform.localRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-30f, 30f));

        _countdownText.transform.DOScale(1, _animationDuration).SetEase(Ease.OutBack);
        _countdownText.transform.DORotate(Vector3.zero, _animationDuration).SetEase(Ease.OutBack);
    }
}
