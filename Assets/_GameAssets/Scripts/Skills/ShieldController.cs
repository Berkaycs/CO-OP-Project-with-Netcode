using UnityEngine;
using Unity.Netcode;

public class ShieldController : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            PlayerSkillController.OnTimerFinished += PlayerSkillController_OnTimerFinished;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            PlayerSkillController.OnTimerFinished -= PlayerSkillController_OnTimerFinished;
        }
    }

    private void PlayerSkillController_OnTimerFinished(ulong clientId)
    {
        if (clientId != OwnerClientId) return;
        DestroyRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DestroyRpc()
    {
        if (IsServer)
        {
            Destroy(gameObject);
        }
    }
}
