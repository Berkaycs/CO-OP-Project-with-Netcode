using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

public class GameOverUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LeaderboardUI _leaderboardUI;
    [SerializeField] private ScoreTablePlayerUI _scoreTablePlayerUIPrefab;
    [SerializeField] private Transform _scoreTableParentTransform;
    [SerializeField] private Image _gameOverBGImage;
    [SerializeField] private RectTransform _gameOverTextTransform;
    [SerializeField] private RectTransform _scoreTableTransform;
    [SerializeField] private TMP_Text _winnerText;
    [SerializeField] private Button _mainMenuButton;

    [Header("Settings")]
    [SerializeField] private float _animationDuration;
    [SerializeField] private float _scaleDuration;

    private RectTransform _winnerTextTransform;
    private RectTransform _mainMenuButtonTransform;

    private void Awake()
    {
        _mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);

        _winnerTextTransform = _winnerText.GetComponent<RectTransform>();
        _mainMenuButtonTransform = _mainMenuButton.GetComponent<RectTransform>();
    }

    private void Start()
    {
        _scoreTableTransform.gameObject.SetActive(false);
        _scoreTableTransform.localScale = Vector3.zero;

        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
    }

    private void OnMainMenuButtonClicked()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            HostSingleton.Instance.HostGameManager.Shutdown();
        }

        ClientSingleton.Instance.ClientGameManager.Disconnect(); // host is also a client so we need to disconnect from both
    }

    private void GameManager_OnGameStateChanged(GameState gameState)
    {
        if (gameState == GameState.GameOver)
        {
            AnimateGameOver();
        }
    }

    private void AnimateGameOver()
    {
        _gameOverBGImage.DOFade(0.8f, _animationDuration / 2);
        _gameOverTextTransform.DOAnchorPosY(0f, _animationDuration).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            _gameOverTextTransform.GetComponent<TMP_Text>().DOFade(0f, _animationDuration / 2).SetDelay(1f).OnComplete(() =>
            {
                AnimateLeaderboardAndButtons();
            });
        });
    }

    private void AnimateLeaderboardAndButtons()
    {
        _scoreTableTransform.gameObject.SetActive(true);
        _scoreTableTransform.DOScale(0.8f, _scaleDuration).SetEase(Ease.OutBack).OnComplete(() =>
        {
            _mainMenuButtonTransform.DOScale(1f, _scaleDuration).SetEase(Ease.OutBack).OnComplete(() =>
            {
                _winnerTextTransform.DOScale(1f, _scaleDuration).SetEase(Ease.OutBack);
            });
        });

        PopulateScoreTable();
    }

    private void PopulateScoreTable()
    {
        List<LeaderboardEntitiesSerializable> leaderboardData = _leaderboardUI.GetLeaderboardData().OrderByDescending(x => x.Score).ToList();

        HashSet<ulong> existingClientIds = new HashSet<ulong>();

        for (int i = 0; i < leaderboardData.Count; i++)
        {
            var entry = leaderboardData[i];

            if (existingClientIds.Contains(entry.ClientId)) continue;

            ScoreTablePlayerUI scoreTablePlayerUI = Instantiate(_scoreTablePlayerUIPrefab, _scoreTableParentTransform);
            bool isOwner = entry.ClientId == NetworkManager.Singleton.LocalClientId;
            int rank = i + 1;

            scoreTablePlayerUI.SetScoreTableData(rank.ToString(), entry.PlayerName, entry.Score.ToString(), entry.ProfileIndex, isOwner);

            existingClientIds.Add(entry.ClientId);
        }

        SetWinnersName();
    }

    private void SetWinnersName()
    {
        string winnerName = _leaderboardUI.GetWinnerName();
        _winnerText.text = winnerName + "SMASHED Y'ALL!";
    }
}
