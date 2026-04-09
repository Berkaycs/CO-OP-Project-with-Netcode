using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;

public class MultiplayerGameManager : NetworkBehaviour
{
    public static MultiplayerGameManager Instance { get; private set; }

    public event Action OnPlayerDataNetworkListChanged;

    [SerializeField] private List<Color> _playerColorsList;

    // NetworkList is a list of objects that are synchronized between the server and the clients
    // We'll use this for lobby players data
    private NetworkList<PlayerDataSerializable> _playerDataList = new NetworkList<PlayerDataSerializable>();

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        _playerDataList.OnListChanged += PlayerDataNetworklist_OnListChanged;
    }

    override public void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _playerDataList.Clear();

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
        }
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        for (int i = 0; i < _playerDataList.Count; i++)
        {
            if (_playerDataList[i].ClientId == clientId)
            {
                _playerDataList.RemoveAt(i);
            }
        }

        _playerDataList.Add(new PlayerDataSerializable 
        { 
            ClientId = clientId,
            ColorId = GetFirstAvailableColorId()
        });
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        for (int i = 0; i < _playerDataList.Count; i++)
        {
            PlayerDataSerializable playerData = _playerDataList[i];
            if (playerData.ClientId == clientId)
            {
                _playerDataList.RemoveAt(i);
            }
        }
    }

    private void PlayerDataNetworklist_OnListChanged(NetworkListEvent<PlayerDataSerializable> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke();
    }

    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < _playerDataList.Count;
    }

    public PlayerDataSerializable GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return _playerDataList[playerIndex];
    }

    public void ChangePlayerColor(int colorId)
    {
        ChangePlayerColorRpc(colorId);
    }

    [Rpc(SendTo.Server)]
    private void ChangePlayerColorRpc(int colorId, RpcParams rpcParams = default)
    {
        if (IsColorIdInUse(colorId)) return;

        int playerDataIndex = GetPlayerDataIndexFromClientId(rpcParams.Receive.SenderClientId);
        PlayerDataSerializable playerData = _playerDataList[playerDataIndex];
        playerData.ColorId = colorId;
        _playerDataList[playerDataIndex] = playerData;
    }

    private int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < _playerDataList.Count; i++)
        {
            if (_playerDataList[i].ClientId == clientId)
            {
                return i;
            }
        }

        return -1;
    }

    public Color GetPlayerColor(int colorId)
    {
        return _playerColorsList[colorId];
    }

    public int GetFirstAvailableColorId()
    {
        for (int i = 0; i < _playerColorsList.Count; i++)
        {
            if (!IsColorIdInUse(i))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>Returns true if any connected player is already using this color.</summary>
    private bool IsColorIdInUse(int colorId)
    {
        for (int i = 0; i < _playerDataList.Count; i++)
        {
            if (_playerDataList[i].ColorId == colorId)
            {
                return true;
            }
        }

        return false;
    }

    public PlayerDataSerializable GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerDataSerializable playerData in _playerDataList)
        {
            if (playerData.ClientId == clientId)
            {
                return playerData;
            }
        }

        return default;
    }

    public PlayerDataSerializable GetPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    public void KickPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        OnClientDisconnectCallback(clientId);
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
    }
}
