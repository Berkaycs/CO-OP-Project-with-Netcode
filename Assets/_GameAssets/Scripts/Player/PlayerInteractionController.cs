using UnityEngine;
using Unity.Netcode;

public class PlayerInteractionController : NetworkBehaviour
{
    private PlayerSkillController _playerSkillController;

    override public void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        _playerSkillController = GetComponent<PlayerSkillController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (other.gameObject.TryGetComponent(out ICollectible collectible))
        {
            collectible.Collect(_playerSkillController);
        }
    }
}
