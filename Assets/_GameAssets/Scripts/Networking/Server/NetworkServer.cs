using Unity.Netcode;
using System;
using UnityEngine;
using System.Collections.Generic;

public class NetworkServer : IDisposable
{
    private NetworkManager _networkManager;

    private Dictionary<ulong, string> _clientIdToAuthDictionary = new Dictionary<ulong, string>(); // for server
    private Dictionary<string, UserData> _authIdToUserDataDictionary = new Dictionary<string, UserData>(); // for UGS

    public NetworkServer(NetworkManager networkManager)
    {
        _networkManager = networkManager;

        _networkManager.ConnectionApprovalCallback += OnConnectionApproval;
        _networkManager.OnServerStarted += OnServerReady;
    }

    private void OnServerReady()
    {
        _networkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        if (_clientIdToAuthDictionary.TryGetValue(clientId, out string authId))
        {
            _clientIdToAuthDictionary.Remove(clientId);
            _authIdToUserDataDictionary.Remove(authId);
        }
    }

    private void OnConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonUtility.FromJson<UserData>(payload);

        _clientIdToAuthDictionary[request.ClientNetworkId] = userData.UserAuthId;
        _authIdToUserDataDictionary[userData.UserAuthId] = userData;

        response.Approved = true;
        response.CreatePlayerObject = true;
    }

    public UserData GetUserDataByClientId(ulong clientId)
    {
        if (_clientIdToAuthDictionary.TryGetValue(clientId, out string authId))
        {
            if (_authIdToUserDataDictionary.TryGetValue(authId, out UserData userData))
            {
                return userData;
            }

            return null;
        }

        return null;
    }

    public void Dispose()
    {
        if (_networkManager == null) return;

        _networkManager.ConnectionApprovalCallback -= OnConnectionApproval;
        _networkManager.OnServerStarted -= OnServerReady;
        _networkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;

        if (_networkManager.IsListening)
        {
            _networkManager.Shutdown();
        }
    }
}

[Serializable]
public class UserData
{
    public string UserName;
    public string UserAuthId;
    public int ProfileIndex;
}
