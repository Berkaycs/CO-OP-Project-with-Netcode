using UnityEngine;
using Unity.Netcode;

public class SpikeDamageable : NetworkBehaviour, IDamageable
{
    [SerializeField] private MysteryBoxSkillsSO _mysteryBoxSkillsSO;
    [SerializeField] private GameObject _explosionEffectPrefab;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        if (NetworkManager.Singleton == null) return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(OwnerClientId, out var client))
        {
            NetworkObject ownerNetworkObject = client.PlayerObject;
            if (ownerNetworkObject == null) return;
            if (!ownerNetworkObject.TryGetComponent(out PlayerVehicleController playerVehicleController)) return;
            playerVehicleController.OnVehicleCrashed += PlayerVehicleController_OnVehicleCrashed;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        if (NetworkManager.Singleton == null) return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(OwnerClientId, out var client))
        {
            NetworkObject ownerNetworkObject = client.PlayerObject;
            if (ownerNetworkObject == null) return;
            if (!ownerNetworkObject.TryGetComponent(out PlayerVehicleController playerVehicleController)) return;
            playerVehicleController.OnVehicleCrashed -= PlayerVehicleController_OnVehicleCrashed;
        }
    }

    private void PlayerVehicleController_OnVehicleCrashed()
    {
        DestroyRpc();
    }

    public void Damage(PlayerVehicleController playerVehicleController, string playerName)
    {
        PlayerHealthController health = playerVehicleController.GetComponent<PlayerHealthController>();
        if (health.GetHealth() - GetDamageAmount() <= 0)
        {
            playerVehicleController.CrashVehicle();
            ExplosionParticleRpc(playerVehicleController.transform.position);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DestroyRpc()
    {
        if (IsServer)
        {
            Destroy(gameObject);
        }
    }

    [Rpc(SendTo.Server)]
    private void ExplosionParticleRpc(Vector3 vehiclePosition = default)
    {
        if (!IsServer) return;
        
        GameObject explosionParticle = Instantiate(_explosionEffectPrefab, vehiclePosition, Quaternion.identity);
        explosionParticle.GetComponent<NetworkObject>().Spawn();
    }

    public int GetRespawnTimer()
    {
        return _mysteryBoxSkillsSO.SkillData.RespawnTimer;
    }

    public ulong GetKillerClientId()
    {
        return OwnerClientId;
    }

    public int GetDamageAmount()
    {
        return _mysteryBoxSkillsSO.SkillData.DamageAmount;
    }

    public string GetKillerName()
    {
        ulong killerClientId = GetKillerClientId();

        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.ConnectedClients.TryGetValue(killerClientId, out var client))
        {
            return string.Empty;
        }

        NetworkObject playerObject = client.PlayerObject;
        if (playerObject == null || !playerObject.TryGetComponent(out PlayerNetworkController playerNetworkController))
        {
            return string.Empty;
        }

        return playerNetworkController.PlayerName.Value.ToString();
    }
}
