using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class MysteryBoxCollectible : NetworkBehaviour, ICollectible
{
    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private Collider _collider;

    [Header("Settings")]
    [SerializeField] private float _respawnTimer = 4f;

    public void Collect()
    {
        Debug.Log("Collected Mystery Box");
        CollectRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void CollectRpc()
    {
        AnimateCollection();
        Invoke(nameof(RespawnBox), _respawnTimer);
    }

    private void AnimateCollection()
    {
        _collider.enabled = false;
        _animator.SetTrigger(Consts.BoxAnimations.IS_COLLECTED);
    }

    private void RespawnBox()
    {
        _animator.SetTrigger(Consts.BoxAnimations.IS_RESPAWNED);
        _collider.enabled = true;
    }
}
