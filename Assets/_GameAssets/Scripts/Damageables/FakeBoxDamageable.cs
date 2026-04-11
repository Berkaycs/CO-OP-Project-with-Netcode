using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class FakeBoxDamageable : NetworkBehaviour, IDamageable
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
        DestroyRpc(false);
    }

    public void Damage(PlayerVehicleController playerVehicleController, string playerName)
    {
        PlayerHealthController health = playerVehicleController.GetComponent<PlayerHealthController>();
        if (health.GetHealth() - GetDamageAmount() <= 0)
        {
            playerVehicleController.CrashVehicle();
            DestroyRpc(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out ShieldController shieldController))
        {
            DestroyRpc(true);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DestroyRpc(bool isExploded)
    {
        if (IsServer)
        {
            if (isExploded)
            {
                GameObject explosionEffect = Instantiate(_explosionEffectPrefab, transform.position, Quaternion.identity);
                explosionEffect.GetComponent<NetworkObject>().Spawn();
            }
            
            Destroy(gameObject);
        }
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
