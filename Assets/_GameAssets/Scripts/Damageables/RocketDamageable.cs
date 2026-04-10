using UnityEngine;
using Unity.Netcode;

public class RocketDamageable : NetworkBehaviour, IDamageable
{
    [SerializeField] private MysteryBoxSkillsSO _mysteryBoxSkillsSO;
    [SerializeField] private GameObject _explosionEffectPrefab;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(OwnerClientId, out var client))
        {
            NetworkObject ownerNetworkObject = client.PlayerObject;
            PlayerVehicleController playerVehicleController = ownerNetworkObject.GetComponent<PlayerVehicleController>();
            playerVehicleController.OnVehicleCrashed += PlayerVehicleController_OnVehicleCrashed;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(OwnerClientId, out var client))
        {
            NetworkObject ownerNetworkObject = client.PlayerObject;
            PlayerVehicleController playerVehicleController = ownerNetworkObject.GetComponent<PlayerVehicleController>();
            playerVehicleController.OnVehicleCrashed -= PlayerVehicleController_OnVehicleCrashed;
        }
    }

    private void PlayerVehicleController_OnVehicleCrashed()
    {
        DestroyRpc(false);
    }
    
    public void Damage(PlayerVehicleController playerVehicleController, string playerName)
    {
        PlayerHealthController health = playerVehicleController.GetComponent<PlayerHealthController>();
        if (health.GetHealth() - GetDamageAmount() <= 0)
        {
            playerVehicleController.CrashVehicle();
            DestroyRpc(true, playerVehicleController.transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out ShieldController shieldController))
        {
            DestroyRpc(true, shieldController.transform.position);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DestroyRpc(bool isExploded, Vector3 vehiclePosition = default)
    {
        if (IsServer)
        {
            if (isExploded)
            {
                GameObject explosionEffect = Instantiate(_explosionEffectPrefab, vehiclePosition, Quaternion.identity);
                explosionEffect.GetComponent<NetworkObject>().Spawn();
            }
            
            Destroy(gameObject);
        }
    }

    public ulong GetKillerClientId()
    {
        return OwnerClientId;
    }

    public int GetRespawnTimer()
    {
        return _mysteryBoxSkillsSO.SkillData.RespawnTimer;
    }

    public int GetDamageAmount()
    {
        return _mysteryBoxSkillsSO.SkillData.DamageAmount;
    }

    public string GetKillerName()
    {
        ulong killerClientId = GetKillerClientId();

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(killerClientId, out var client))
        {
            string playerName = client.PlayerObject.GetComponent<PlayerNetworkController>().PlayerName.Value.ToString();
            return playerName;
        }

        return string.Empty;
    }
}
