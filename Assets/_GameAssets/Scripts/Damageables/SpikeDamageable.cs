using UnityEngine;
using Unity.Netcode;

public class SpikeDamageable : NetworkBehaviour, IDamageable
{
    [SerializeField] private MysteryBoxSkillsSO _mysteryBoxSkillsSO;

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
        DestroyRpc();
    }

    public void Damage(PlayerVehicleController playerVehicleController)
    {
        playerVehicleController.CrashVehicle();
        KillScreenUI.Instance.SetSmashedUI("Kyle", _mysteryBoxSkillsSO.SkillData.RespawnTimer);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DestroyRpc()
    {
        if (IsServer)
        {
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
}
