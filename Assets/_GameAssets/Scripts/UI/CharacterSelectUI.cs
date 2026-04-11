using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _readyButton;
    [SerializeField] private Button _copyButton;
    [SerializeField] private TMP_Text _readyText;
    [SerializeField] private TMP_Text _joinCodeText;
    [SerializeField] private Image _copiedImage;

    [Header("Sprites")]
    [SerializeField] private Sprite _greenButtonSprite;
    [SerializeField] private Sprite _redButtonSprite;
    [SerializeField] private Sprite _tickSprite;

    private bool _isPlayerReady;

    private void OnEnable()
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

    private void Awake()
    {
        _mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        _readyButton.onClick.AddListener(OnReadyButtonClicked);
        _startButton.onClick.AddListener(OnStartButtonClicked);
        _copyButton.onClick.AddListener(OnCopyButtonClicked);
    }

    private void Start()
    {
        _startButton.gameObject.SetActive(NetworkManager.Singleton.IsServer);
        SetStartButtonInteractable(false);

        CharacterSelectReady.Instance.OnAllPlayersReady += CharacterSelectReady_OnAllPlayersReady;
        CharacterSelectReady.Instance.OnUnreadyChanged += CharacterSelectReady_OnUnreadyChanged;
        MultiplayerGameManager.Instance.OnPlayerDataNetworkListChanged += MultiplayerGameManager_OnPlayerDataNetworkListChanged;
    }

    private void CharacterSelectReady_OnAllPlayersReady()
    {
        SetStartButtonInteractable(true);
    }

    private void CharacterSelectReady_OnUnreadyChanged()
    {
        SetStartButtonInteractable(false);
    }

    private void MultiplayerGameManager_OnPlayerDataNetworkListChanged()
    {
        if (CharacterSelectReady.Instance.AreAllPlayersReady())
        {
            SetStartButtonInteractable(true);
        }
        else
        {
            SetStartButtonInteractable(false);
        }
    }

    private void OnCopyButtonClicked()
    {
        _copiedImage.sprite = _tickSprite;
        GUIUtility.systemCopyBuffer = _joinCodeText.text;
    }

    private void OnMainMenuButtonClicked()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            HostSingleton.Instance.HostGameManager.Shutdown();
        }
        else
        {
            ClientSingleton.Instance.ClientGameManager.Disconnect();
        }
    }

    private void OnReadyButtonClicked()
    {
        _isPlayerReady = !_isPlayerReady;

        if (_isPlayerReady)
        {
            SetPlayerReady();
        }
        else
        {
            SetPlayerUnready();
        }
    }

    private void OnStartButtonClicked()
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null)
        {
            Debug.LogError("[CharacterSelect] OnStartButtonClicked: NetworkManager.Singleton is null.");
            return;
        }

        if (!nm.IsServer)
        {
            Debug.LogWarning(
                "[CharacterSelect] OnStartButtonClicked ignored: caller is not server " +
                $"(IsServer={nm.IsServer} IsClient={nm.IsClient} IsHost={nm.IsHost}).");
            return;
        }

        if (nm.SceneManager == null)
        {
            Debug.LogError("[CharacterSelect] OnStartButtonClicked: SceneManager is null. Enable Scene Management on NetworkManager.");
            return;
        }

        string activeScene = SceneManager.GetActiveScene().name;
        int connected = nm.ConnectedClientsList.Count;
        Debug.Log(
            $"[CharacterSelect] Requesting networked load of '{Consts.Scenes.GAME_SCENE}' (Single). " +
            $"activeScene={activeScene} connectedClients={connected} LocalClientId={nm.LocalClientId}");

        SceneEventProgressStatus status = nm.SceneManager.LoadScene(Consts.Scenes.GAME_SCENE, LoadSceneMode.Single);
        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogError($"[CharacterSelect] LoadScene did not start successfully. status={status}");
        }
        else
        {
            Debug.Log($"[CharacterSelect] LoadScene accepted by Netcode. status={status}");
        }
    }

    private void SetPlayerReady()
    {
        CharacterSelectReady.Instance.SetPlayerReady();
        _readyText.text = "Ready";
        _readyButton.image.sprite = _greenButtonSprite;
    }

    private void SetPlayerUnready()
    {
        CharacterSelectReady.Instance.SetPlayerUnready();
        _readyText.text = "Not Ready";
        _readyButton.image.sprite = _redButtonSprite;
    }

    private void SetStartButtonInteractable(bool isActive)
    {
        if (_startButton != null)
        {
            _startButton.interactable = isActive;
        }
    }

    private void OnDestroy()
    {
        CharacterSelectReady.Instance.OnAllPlayersReady -= CharacterSelectReady_OnAllPlayersReady;
        CharacterSelectReady.Instance.OnUnreadyChanged -= CharacterSelectReady_OnUnreadyChanged;
        MultiplayerGameManager.Instance.OnPlayerDataNetworkListChanged -= MultiplayerGameManager_OnPlayerDataNetworkListChanged;
    }
}
