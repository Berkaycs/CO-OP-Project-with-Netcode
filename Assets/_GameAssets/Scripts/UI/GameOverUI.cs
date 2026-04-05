using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameOverUI : MonoBehaviour
{
    [Header("References")]
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
        _winnerTextTransform = _winnerText.GetComponent<RectTransform>();
        _mainMenuButtonTransform = _mainMenuButton.GetComponent<RectTransform>();
    }

    private void Start()
    {
        _scoreTableTransform.gameObject.SetActive(false);
        _scoreTableTransform.localScale = Vector3.zero;

        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
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
    }
}
