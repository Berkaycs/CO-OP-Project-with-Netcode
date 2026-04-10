using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine.Audio;

public class SettingsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button _settingsButton;

    [Header("Settings Menu")]
    [SerializeField] private RectTransform _settingsMenuTransform;
    [SerializeField] private Image _bgImage;
    [SerializeField] private Button _vsyncButton;
    [SerializeField] private Button _musicButton;
    [SerializeField] private GameObject _vsyncTick;
    [SerializeField] private GameObject _musicTick;
    [SerializeField] private Button _leaveGameButton;
    [SerializeField] private Button _keepPlayingButton;
    [SerializeField] private Button _copyCodeButton;
    [SerializeField] private Image _copiedImage;
    [SerializeField] private TMP_Text _joinCodeText;

    [Header("Sprites")]
    [SerializeField] private Sprite _tickSprite;
    [SerializeField] private Sprite _crossSprite;

    [Header("Settings")]
    [SerializeField] private float _animationDuration;

    private bool _isAnimating;
    private bool _isVsyncEnabled;
    private bool _isMusicEnabled;
    private bool _isCopyingCode;

    private void Awake()
    {
        _settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        _vsyncButton.onClick.AddListener(OnVsyncButtonClicked);
        _leaveGameButton.onClick.AddListener(OnLeaveGameButtonClicked);
        _keepPlayingButton.onClick.AddListener(OnKeepPlayingButtonClicked);
        _copyCodeButton.onClick.AddListener(OnCopyCodeButtonClicked);
        _musicButton.onClick.AddListener(OnMusicButtonClicked);
    }

    private void Start()
    {
        _settingsMenuTransform.localScale = Vector3.zero;
        _settingsMenuTransform.gameObject.SetActive(false);
        _vsyncTick.SetActive(false);
        _musicTick.SetActive(true);
        _isMusicEnabled = true;
    }

    private void OnSettingsButtonClicked()
    {
        if (_isAnimating) return;

        SetJoinCode();

        _isAnimating = true;
        _settingsMenuTransform.gameObject.SetActive(true);

        _bgImage.DOFade(0.8f, _animationDuration);

        _settingsMenuTransform.DOScale(1, _animationDuration).SetEase(Ease.OutBack).OnComplete(() =>
        {
            _isAnimating = false;
        });
    }

    private void OnVsyncButtonClicked()
    {
        _isVsyncEnabled = !_isVsyncEnabled;
        QualitySettings.vSyncCount = _isVsyncEnabled ? 1 : 0;
        _vsyncTick.SetActive(_isVsyncEnabled);
    }

    private void OnLeaveGameButtonClicked()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            HostSingleton.Instance.HostGameManager.Shutdown();
        }

        ClientSingleton.Instance.ClientGameManager.Disconnect();
    }

    private void OnKeepPlayingButtonClicked()
    {
        if (_isAnimating) return;

        _isAnimating = true;

        _bgImage.DOFade(0f, _animationDuration);
        _settingsMenuTransform.DOScale(0, _animationDuration).SetEase(Ease.InBack).OnComplete(() =>
        {
            _isAnimating = false;
            _settingsMenuTransform.gameObject.SetActive(false);
            _isCopyingCode = false;
            _copiedImage.sprite = _crossSprite;
        });
    }

    private void OnCopyCodeButtonClicked()
    {
        if (_isCopyingCode) return;

        _isCopyingCode = true;

        _copiedImage.sprite = _tickSprite;
        _joinCodeText.text = HostSingleton.Instance.HostGameManager.GetJoinCode();
        GUIUtility.systemCopyBuffer = _joinCodeText.text;
    }

    private void SetJoinCode()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            _joinCodeText.text = HostSingleton.Instance.HostGameManager.GetJoinCode();
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            _joinCodeText.text = ClientSingleton.Instance.ClientGameManager.GetJoinCode();
        }
    }

    private void OnMusicButtonClicked()
    {
        _isMusicEnabled = !_isMusicEnabled;
        BGMusicController.Instance.SetMusicVolume(_isMusicEnabled ? .6f : 0f);
        _musicTick.SetActive(_isMusicEnabled);
    }
}
