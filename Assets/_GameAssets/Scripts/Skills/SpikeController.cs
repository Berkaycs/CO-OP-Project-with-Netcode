using UnityEngine;
using Unity.Netcode;

public class SpikeController : NetworkBehaviour
{
    [SerializeField] private Collider _spikeCollider;

    public override void OnNetworkSpawn()
    {
        PlayerSkillController.OnTimerFinished += PlayerSkillController_OnTimerFinished;

        if (IsOwner)
        {
            SetOwnerVisualsRpc();
        }
    }

    public override void OnNetworkDespawn()
    {
        PlayerSkillController.OnTimerFinished -= PlayerSkillController_OnTimerFinished;
    }

    private void PlayerSkillController_OnTimerFinished(ulong clientId)
    {
        if (clientId != OwnerClientId) return;
        DestroyRpc();
    }

    [Rpc(SendTo.Owner)]
    private void SetOwnerVisualsRpc()
    {
        _spikeCollider.enabled = false;
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
