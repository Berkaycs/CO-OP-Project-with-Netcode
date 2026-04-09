using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode.Components;
using Unity.Collections;

public class CharacterSelectPlayer : NetworkBehaviour
{
    [SerializeField] private int _playerIndex;
    [SerializeField] private Sprite[] _playerProfiles;
    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private GameObject _readyGameObject;
    [SerializeField] private Button _kickButton;
    [SerializeField] private Image _profileImage;
    [SerializeField] private CharacterSelectVisual _characterSelectVisual;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();
    public NetworkVariable<int> ProfileIndex = new NetworkVariable<int>();

    private void Awake()
    {
        _kickButton.onClick.AddListener(OnKickButtonClicked);
    }

    private void Start()
    {
        MultiplayerGameManager.Instance.OnPlayerDataNetworkListChanged += MultiplayerGameManager_OnPlayerDataNetworkListChanged;
        CharacterSelectReady.Instance.OnReadyChanged += CharacterSelectReady_OnReadyChanged;

        UpdatePlayer();
        PlayerName_OnValueChanged(string.Empty, PlayerName.Value);
        ProfileIndex_OnValueChanged(0, ProfileIndex.Value);
        PlayerName.OnValueChanged += PlayerName_OnValueChanged;
        ProfileIndex.OnValueChanged += ProfileIndex_OnValueChanged;
    }

    private void MultiplayerGameManager_OnPlayerDataNetworkListChanged()
    {
        UpdatePlayer();
    }

    private void CharacterSelectReady_OnReadyChanged()
    {
        UpdatePlayer();
    }

    private void PlayerName_OnValueChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        _playerNameText.text = newName.ToString();
    }

    private void ProfileIndex_OnValueChanged(int oldIndex, int newIndex)
    {
        _profileImage.sprite = _playerProfiles[newIndex];
    }

    private void UpdatePlayer()
    {
        if (MultiplayerGameManager.Instance.IsPlayerIndexConnected(_playerIndex))
        {
            gameObject.SetActive(true);

            PlayerDataSerializable playerData = MultiplayerGameManager.Instance.GetPlayerDataFromPlayerIndex(_playerIndex);

            _characterSelectVisual.SetPlayerColor(MultiplayerGameManager.Instance.GetPlayerColor(playerData.ColorId));

            _readyGameObject.SetActive(CharacterSelectReady.Instance.IsPlayerReady(playerData.ClientId));
            HideKickButton(playerData);
            SetOwner(playerData.ClientId);
            UpdatePlayerNameRpc();
            UpdateProfileIndexRpc();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void HideKickButton(PlayerDataSerializable playerData)
    {
        _kickButton.gameObject.SetActive(NetworkManager.Singleton.IsServer && 
                                        playerData.ClientId != NetworkManager.Singleton.LocalClientId);
    }

    private void OnKickButtonClicked()
    {
        PlayerDataSerializable playerData = MultiplayerGameManager.Instance.GetPlayerDataFromPlayerIndex(_playerIndex);
        MultiplayerGameManager.Instance.KickPlayer(playerData.ClientId);
    }

    private void SetOwner(ulong clientId)
    {
        if (IsServer)
        {
            var networkObject = GetComponent<NetworkObject>();

            if (networkObject.OwnerClientId != clientId)
            {
                networkObject.ChangeOwnership(clientId);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdatePlayerNameRpc()
    {
        if (IsServer)
        {
            UserData userData = HostSingleton.Instance.HostGameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            PlayerName.Value = userData.UserName;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateProfileIndexRpc()
    {
        if (IsServer)
        {
            UserData userData = HostSingleton.Instance.HostGameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            ProfileIndex.Value = userData.ProfileIndex;
        }
    }

    public override void OnDestroy()
    {
        PlayerName.OnValueChanged -= PlayerName_OnValueChanged;
        MultiplayerGameManager.Instance.OnPlayerDataNetworkListChanged -= MultiplayerGameManager_OnPlayerDataNetworkListChanged;
        CharacterSelectReady.Instance.OnReadyChanged -= CharacterSelectReady_OnReadyChanged;
    }
}
