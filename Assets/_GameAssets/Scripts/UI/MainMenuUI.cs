using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MainMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LobbiesListUI _lobbiesListUI;
    [SerializeField] private GameObject _lobbiesParent;
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _clientButton;
    [SerializeField] private Button _lobbiesButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private RectTransform _lobbiesBGTransform;
    [SerializeField] private TMP_InputField _joinCodeInputField;

    [Header("Settings")]
    [SerializeField] private float _animationDuration;

    private void Awake()
    {
        _hostButton.onClick.AddListener(StartHost);
        _clientButton.onClick.AddListener(StartClient);
        _lobbiesButton.onClick.AddListener(ShowLobbies);
        _closeButton.onClick.AddListener(HideLobbies);
    }

    private void Start()
    {
        _lobbiesParent.SetActive(false);
    }

    private async void StartHost()
    {
        await HostSingleton.Instance.HostGameManager.StartHostAsync();
    }

    private async void StartClient()
    {
        await ClientSingleton.Instance.ClientGameManager.StartClientAsync(_joinCodeInputField.text);
    }

    private void ShowLobbies()
    {
        _lobbiesParent.SetActive(true);
        _lobbiesBGTransform.DOAnchorPosX(-650f, _animationDuration).SetEase(Ease.OutBack);

        _lobbiesListUI.RefreshLobbies();
    }

    private void HideLobbies()
    {
        _lobbiesBGTransform.DOAnchorPosX(900f, _animationDuration).SetEase(Ease.InBack).OnComplete(() =>
        {
            _lobbiesParent.SetActive(false);
        });
    }
}
