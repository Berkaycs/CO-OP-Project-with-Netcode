using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using Unity.Collections;
using TMPro;
using System;

public class PlayerNetworkController : NetworkBehaviour
{
    public static event Action<PlayerNetworkController> OnPlayerSpawned;
    public static event Action<PlayerNetworkController> OnPlayerDespawned;

    [SerializeField] private CinemachineCamera _playerCamera;
    [SerializeField] private PlayerVehicleController _playerVehicleController;
    [SerializeField] private PlayerSkillController _playerSkillController;
    [SerializeField] private PlayerInteractionController _playerInteractionController;
    [SerializeField] private PlayerScoreController _playerScoreController;
    [SerializeField] private TMP_Text _playerNameText;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();
    public NetworkVariable<int> ProfileIndex = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
        _playerCamera.gameObject.SetActive(IsOwner);

        if (IsServer)
        {
            UserData userData = HostSingleton.Instance.HostGameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            PlayerName.Value = userData.UserName;
            ProfileIndex.Value = userData.ProfileIndex;

            SetPlayerNameRpc();

            OnPlayerSpawned?.Invoke(this);
        }
    }

    public void OnPlayerRespawned()
    {
        _playerVehicleController.OnPlayerRespawned();
        _playerSkillController.OnPlayerRespawned();
        _playerInteractionController.OnPlayerRespawned();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetPlayerNameRpc()
    {
        _playerNameText.text = PlayerName.Value.ToString();
    }

    public PlayerScoreController GetPlayerScoreController()
    {
        return _playerScoreController;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            OnPlayerDespawned?.Invoke(this);
        }
    }
}
