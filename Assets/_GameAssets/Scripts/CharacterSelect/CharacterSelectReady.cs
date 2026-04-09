using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;

public class CharacterSelectReady : NetworkBehaviour
{
    public static CharacterSelectReady Instance { get; private set; }

    public event Action OnReadyChanged;
    public event Action OnUnreadyChanged;
    public event Action OnAllPlayersReady;

    private Dictionary<ulong, bool> _playerReadyDictionary = new Dictionary<ulong, bool>();

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
    }

    private void OnClientConnectedCallback(ulong connectedClientId)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (IsPlayerReady(clientId))
            {
                SetPlayerReadyToAllRpc(clientId);
            }
        }
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        if (_playerReadyDictionary.ContainsKey(clientId))
        {
            _playerReadyDictionary.Remove(clientId);
            OnUnreadyChanged?.Invoke();
        }
    }

    [Rpc(SendTo.Server)]
    private void SetPlayerReadyRpc(RpcParams rpcParams = default)
    {
        SetPlayerReadyToAllRpc(rpcParams.Receive.SenderClientId);
        _playerReadyDictionary[rpcParams.Receive.SenderClientId] = true;

        bool allPlayersReady = true;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!_playerReadyDictionary.ContainsKey(clientId) || !_playerReadyDictionary[clientId])
            {
                allPlayersReady = false;
                break;
            }
        }

        if (allPlayersReady)
        {
            OnAllPlayersReady?.Invoke();
        }
    }

    [Rpc(SendTo.Server)]
    private void SetPlayerUnreadyRpc(RpcParams rpcParams = default)
    {
        SetPlayerUnreadyToAllRpc(rpcParams.Receive.SenderClientId);

        if (_playerReadyDictionary.ContainsKey(rpcParams.Receive.SenderClientId))
        {
            _playerReadyDictionary[rpcParams.Receive.SenderClientId] = false;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetPlayerReadyToAllRpc(ulong clientId)
    {
        _playerReadyDictionary[clientId] = true;
        OnReadyChanged?.Invoke();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetPlayerUnreadyToAllRpc(ulong clientId)
    {
        _playerReadyDictionary[clientId] = false;
        OnReadyChanged?.Invoke();
        OnUnreadyChanged?.Invoke();
    }

    public bool IsPlayerReady(ulong clientId)
    {
        return _playerReadyDictionary.ContainsKey(clientId) && _playerReadyDictionary[clientId];
    }

    public bool AreAllPlayersReady()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!_playerReadyDictionary.ContainsKey(clientId) || !_playerReadyDictionary[clientId])
            {
                return false;
            }
        }
        return true;
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyRpc();
    }

    public void SetPlayerUnready()
    {
        SetPlayerUnreadyRpc();
    }
}
