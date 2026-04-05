using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

public class PlayerNetworkController : NetworkBehaviour
{
    [SerializeField] private CinemachineCamera _playerCamera;
    [SerializeField] private PlayerVehicleController _playerVehicleController;
    [SerializeField] private PlayerSkillController _playerSkillController;
    [SerializeField] private PlayerInteractionController _playerInteractionController;

    public override void OnNetworkSpawn()
    {
        _playerCamera.gameObject.SetActive(IsOwner);
    }

    public void OnPlayerRespawned()
    {
        _playerVehicleController.OnPlayerRespawned();
        _playerSkillController.OnPlayerRespawned();
        _playerInteractionController.OnPlayerRespawned();
    }
}
