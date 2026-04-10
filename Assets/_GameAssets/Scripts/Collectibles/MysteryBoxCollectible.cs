using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class MysteryBoxCollectible : NetworkBehaviour, ICollectible
{
    [Header("References")]
    [SerializeField] private MysteryBoxSkillsSO[] _mysteryBoxSkills;
    [SerializeField] private Animator _animator;
    [SerializeField] private Collider _collider;

    [Header("Settings")]
    [SerializeField] private float _respawnTimer = 4f;

    public void Collect(PlayerSkillController playerSkillController, CameraShake cameraShake)
    {
        if (playerSkillController.HasSkillAlready()) return;

        MysteryBoxSkillsSO skill = GetRandomMysteryBoxSkill();
        SkillsUI.Instance.SetSkill(skill.SkillName, skill.SkillIcon, 
                                   skill.SkillUsageType, skill.SkillData.SpawnAmountOrTimer);
        playerSkillController.SetupSkill(skill);

        cameraShake.ShakeCamera(0.8f, 0.4f);

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

    private MysteryBoxSkillsSO GetRandomMysteryBoxSkill()
    {
        int randomIndex = Random.Range(0, _mysteryBoxSkills.Length);
        return _mysteryBoxSkills[randomIndex];
    }
}
