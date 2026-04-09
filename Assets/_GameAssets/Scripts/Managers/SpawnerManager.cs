using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode.Components;

public class SpawnerManager : NetworkBehaviour
{
    public static SpawnerManager Instance { get; private set; }

    [Header("Player Prefab")]
    [SerializeField] private GameObject _playerPrefab;

    [Header("Transform Lists")]
    [SerializeField] private List<Transform> _spawnPointTransformList;
    [SerializeField] private List<Transform> _respawnPointTransformList;

    private List<int> _availableSpawnPointIndexList = new List<int>();
    private List<int> _availableRespawnPointIndexList = new List<int>();

    private void Awake()
    {
        Instance = this;
    }

    override public void OnNetworkSpawn()
    {
        if (!IsServer) return;

        for (int i = 0; i < _spawnPointTransformList.Count; i++)
        {
            _availableSpawnPointIndexList.Add(i);
        }

        for (int i = 0; i < _respawnPointTransformList.Count; i++)
        {
            _availableRespawnPointIndexList.Add(i);
        }

        //NetworkManager.OnClientConnectedCallback += SpawnPlayer; no need anymore

        SpawnAllPlayers();
    }

    private void SpawnAllPlayers()
    {
        if (!IsServer) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            SpawnPlayer(client.ClientId);
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (_availableSpawnPointIndexList.Count == 0)
        {
            Debug.LogError("No available spawn points!");
            return;
        }
        
        int randomIndex = Random.Range(0, _availableSpawnPointIndexList.Count);
        int spawnPointIndex = _availableSpawnPointIndexList[randomIndex];
        _availableSpawnPointIndexList.RemoveAt(randomIndex);

        Transform spawnPointTransform = _spawnPointTransformList[spawnPointIndex];
        GameObject playerInstance = Instantiate(_playerPrefab, spawnPointTransform.position, spawnPointTransform.rotation);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    public void RespawnPlayer(int respawnTimer, ulong clientId)
    {
        StartCoroutine(RespawnPlayerCoroutine(respawnTimer, clientId));
    }

    private IEnumerator RespawnPlayerCoroutine(int respawnTimer, ulong clientId)
    {
        yield return new WaitForSeconds(respawnTimer);

        if (GameManager.Instance.GetCurrentGameState() != GameState.Playing) { yield break; }

        if (_respawnPointTransformList.Count == 0)
        {
            Debug.LogError("No available respawn points!");
            yield break;
        }

        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            Debug.LogError($"Client {clientId} not found!");
            yield break;
        }

        if (_availableRespawnPointIndexList.Count == 0)
        {
            for (int i = 0; i < _respawnPointTransformList.Count; i++)
            {
                _availableRespawnPointIndexList.Add(i);
            }
        }

        int randomIndex = Random.Range(0, _availableRespawnPointIndexList.Count);
        int respawnPointIndex = _availableRespawnPointIndexList[randomIndex];
        _availableRespawnPointIndexList.RemoveAt(randomIndex);

        Transform respawnPointTransform = _respawnPointTransformList[respawnPointIndex];
        NetworkObject playerNetworkObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;

        if (playerNetworkObject == null)
        {
            Debug.LogError($"Player network object for client {clientId} not found!");
            yield break;
        }

        if (playerNetworkObject.TryGetComponent<Rigidbody>(out var playerRigidbody))
        {
            playerRigidbody.isKinematic = true;
        }

        if (playerNetworkObject.TryGetComponent<NetworkTransform>(out var playerNetworkTransform))
        {
            playerNetworkTransform.Interpolate = false;
            playerNetworkObject.GetComponent<PlayerVehicleVisualController>().SetVehicleVisualActive(0.1f);
        }

        playerNetworkObject.transform.SetPositionAndRotation(respawnPointTransform.position, respawnPointTransform.rotation);

        yield return new WaitForSeconds(0.1f);

        playerRigidbody.isKinematic = false;
        playerNetworkTransform.Interpolate = true;

        if (playerNetworkObject.TryGetComponent<PlayerNetworkController>(out var playerNetworkController))
        {
            playerNetworkController.OnPlayerRespawned();
        }
    }
}
