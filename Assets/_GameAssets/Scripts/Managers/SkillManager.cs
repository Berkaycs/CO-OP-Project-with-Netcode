using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using Cysharp.Threading.Tasks;
using System;

public class SkillManager : NetworkBehaviour
{
    public static SkillManager Instance { get; private set; }

    public event Action OnMineCountReduced;

    [SerializeField] private MysteryBoxSkillsSO[] _mysteryBoxSkills;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _hillLayer;

    private Dictionary<SkillType, MysteryBoxSkillsSO> _skillsDictionary;

    private void Awake()
    {
        Instance = this;

        _skillsDictionary = new Dictionary<SkillType, MysteryBoxSkillsSO>();

        foreach (MysteryBoxSkillsSO skill in _mysteryBoxSkills)
        {
            _skillsDictionary.Add(skill.SkillType, skill);
        }
    }

    public void ActivateSkill(SkillType skillType, Transform playerTransform,ulong spawnerClientId)
    {
        SkillTransformDataSerializable skillTransformData = new SkillTransformDataSerializable(
            playerTransform.position, playerTransform.rotation, skillType, playerTransform.GetComponent<NetworkObject>());
        
        if (!IsServer)
        {
            RequestSpawnRpc(skillTransformData, spawnerClientId);
            return;
        }

        SpawnSkill(skillTransformData, spawnerClientId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RequestSpawnRpc(SkillTransformDataSerializable skillTransformDataSerializable, 
            ulong spawnerClientId)
    {
        SpawnSkill(skillTransformDataSerializable, spawnerClientId);
    }

    private async void SpawnSkill(SkillTransformDataSerializable skillTransformDataSerializable, 
            ulong spawnerClientId)
    {
        if (!_skillsDictionary.TryGetValue(skillTransformDataSerializable.SkillType, out var mysteryBoxSkillsSO))
        {
            Debug.LogError($"Skill type {skillTransformDataSerializable.SkillType} not found!");
            return;
        }

        if (skillTransformDataSerializable.SkillType == SkillType.Mine)
        {
            Vector3 spawnPosition = skillTransformDataSerializable.Position;
            Vector3 spawnDirection = skillTransformDataSerializable.Rotation * Vector3.forward;

            for (int i = 0; i < mysteryBoxSkillsSO.SkillData.SpawnAmountOrTimer; i++)
            {
                Vector3 offset = spawnDirection * (i * 3f);
                skillTransformDataSerializable.Position = spawnPosition + offset;

                Spawn(skillTransformDataSerializable, spawnerClientId, mysteryBoxSkillsSO);
                await UniTask.Delay(200);
                OnMineCountReduced?.Invoke();
            }
        }
        else
        {
            Spawn(skillTransformDataSerializable, spawnerClientId, mysteryBoxSkillsSO);
        }
    }

    private void Spawn(SkillTransformDataSerializable skillTransformDataSerializable, 
            ulong spawnerClientId, MysteryBoxSkillsSO mysteryBoxSkillsSO)
    {
        if (IsServer)
        {
            Transform skillInstance = Instantiate(mysteryBoxSkillsSO.SkillData.SkillPrefab);
            skillInstance.SetPositionAndRotation(skillTransformDataSerializable.Position, skillTransformDataSerializable.Rotation);
            var networkObject = skillInstance.GetComponent<NetworkObject>();
            networkObject.SpawnWithOwnership(spawnerClientId);

            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(spawnerClientId, out var client))
            {
                if (mysteryBoxSkillsSO.SkillType != SkillType.Rocket)
                {
                    networkObject.TrySetParent(client.PlayerObject);
                }
                else
                {
                    // ROKET ÖZEL
                    PlayerSkillController playerSkillController = client.PlayerObject.GetComponent<PlayerSkillController>();
                    networkObject.transform.localPosition = playerSkillController.GetRocketLaunchPoint();
                    return;
                }

                if (mysteryBoxSkillsSO.SkillData.ShouldBeAttachedToParent)
                {
                    networkObject.transform.localPosition = Vector3.zero;
                }

                PositionDataSerializable positionDataSerializable = new PositionDataSerializable(
                    skillInstance.transform.localPosition + mysteryBoxSkillsSO.SkillData.SkillOffset);

                UpdateSkillPositionRpc(networkObject.NetworkObjectId, positionDataSerializable, false);

                if (!mysteryBoxSkillsSO.SkillData.ShouldBeAttachedToParent)
                {
                    networkObject.TryRemoveParent();
                }

                if (mysteryBoxSkillsSO.SkillType == SkillType.FakeBox)
                {
                    float groundHeight = GetGroundHeight(mysteryBoxSkillsSO, skillInstance.position);
                    
                    positionDataSerializable = new PositionDataSerializable(new Vector3(
                                            skillInstance.transform.position.x,
                                            groundHeight,
                                            skillInstance.transform.position.z));
                    
                    UpdateSkillPositionRpc(networkObject.NetworkObjectId, positionDataSerializable, true);
                }
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateSkillPositionRpc(ulong objectId, PositionDataSerializable positionDataSerializable, bool isSpecialPosition)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var networkObject))
        {
            if (isSpecialPosition)
            {
                // FAKE BOX SPECIAL
                networkObject.transform.position = positionDataSerializable.Position;
            }
            else
            {
                networkObject.transform.localPosition = positionDataSerializable.Position;
            }
        }
    }

    private float GetGroundHeight(MysteryBoxSkillsSO mysteryBoxSkillsSO, Vector3 position)
    {
        if (Physics.Raycast(new Vector3(position.x, position.y, position.z), Vector3.down, 
                            out RaycastHit hit2, 10f, _hillLayer))
        {
            return 3f;
        }

        if (Physics.Raycast(new Vector3(position.x, position.y, position.z), Vector3.down, 
                            out RaycastHit hit, 10f, _groundLayer))
        {
            return mysteryBoxSkillsSO.SkillData.SkillOffset.y;
        }

        return mysteryBoxSkillsSO.SkillData.SkillOffset.y;
    }
}
