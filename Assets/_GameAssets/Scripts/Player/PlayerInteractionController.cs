using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Collections;

public class PlayerInteractionController : NetworkBehaviour
{
    [SerializeField] private PlayerHealthController _playerHealthController;
    [SerializeField] private CameraShake _cameraShake;

    private PlayerSkillController _playerSkillController;
    private PlayerVehicleController _playerVehicleController;
    private PlayerNetworkController _playerNetworkController;

    private bool _isVehicleCrashed;
    private bool _isShieldActive;
    private bool _isSpikeActive;
    private bool _deathHandled;

    override public void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        _playerSkillController = GetComponent<PlayerSkillController>();
        _playerVehicleController = GetComponent<PlayerVehicleController>();
        _playerNetworkController = GetComponent<PlayerNetworkController>();
        _playerVehicleController.OnVehicleCrashed += PlayerVehicleController_OnVehicleCrashed;
    }

    private void PlayerVehicleController_OnVehicleCrashed()
    {
        enabled = false;
        _isVehicleCrashed = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckCollision(other);
    }

    private void OnTriggerStay(Collider other)
    {
        CheckCollision(other);
    }

    private void CheckCollision(Collider other)
    {
        if (!IsOwner) return;
        if (_isVehicleCrashed) return;

        if (!GameManager.IsGamePlaying()) return;

        CheckCollectibleCollision(other);
        CheckDamageableCollision(other);
    }

    private void CheckCollectibleCollision(Collider other)
    {
        if (other.gameObject.TryGetComponent(out ICollectible collectible))
        {
            collectible.Collect(_playerSkillController, _cameraShake);
        }
    }

    private void CheckDamageableCollision(Collider other)
    {
        if (other.gameObject.TryGetComponent(out IDamageable damageable))
        {
            if (_isShieldActive)
            {
                Debug.Log("Shield is active");
                return;
            }

            CrashTheVehicle(damageable);
        }
    }

    private void CrashTheVehicle(IDamageable damageable)
    {
        if (_deathHandled) return;
        if (_playerHealthController.GetHealth() <= 0) return;

        var playerName = _playerNetworkController.PlayerName.Value;

        _cameraShake.ShakeCamera(3f, 0.8f);
        damageable.Damage(_playerVehicleController, damageable.GetKillerName());

        _playerHealthController.TakeDamage(damageable.GetDamageAmount());

        if (_playerHealthController.GetHealth() <= 0)
        {
            _deathHandled = true;
            KillScreenUI.Instance.SetSmashedUI(damageable.GetKillerName(), damageable.GetRespawnTimer());
            SetKillerUIRpc(damageable.GetKillerClientId(), playerName.ToString(),
                        RpcTarget.Single(damageable.GetKillerClientId() ,RpcTargetUse.Temp));

            SpawnerManager.Instance.RespawnPlayer(damageable.GetRespawnTimer(), OwnerClientId);
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SetKillerUIRpc(ulong killerClientId, FixedString32Bytes playerName,RpcParams rpcParams)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(killerClientId, out var killerClient))
        {
            KillScreenUI.Instance.SetSmashUI(playerName.ToString());
            killerClient.PlayerObject.GetComponent<PlayerScoreController>().AddScore(1);
        }
    }

    public void OnPlayerRespawned()
    {
        enabled = true;
        _isVehicleCrashed = false;
        _deathHandled = false;
        _playerHealthController.RestartHealth();
    }

    public void SetShieldActive(bool active) => _isShieldActive = active;
    public void SetSpikeActive(bool active) => _isSpikeActive = active;
}
