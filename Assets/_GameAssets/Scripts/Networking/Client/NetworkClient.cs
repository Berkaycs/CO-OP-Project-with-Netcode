using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System;

public class NetworkClient : IDisposable // we use IDisposable to clean up the resources when the client is disconnected (it's like OnDestroy)
{
    private NetworkManager _networkManager;

    public NetworkClient(NetworkManager networkManager)
    {
        _networkManager = networkManager;

        _networkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        // if the clientId is not 0 (0 is the host) and the clientId is not the local clientId, return
        if (clientId != 0 && clientId != _networkManager.LocalClientId) return;

        Disconnect();
    }

    private void Disconnect()
    {
        if (SceneManager.GetActiveScene().name != Consts.Scenes.MENU_SCENE)
        {
            SceneManager.LoadScene(Consts.Scenes.MENU_SCENE);
        }

        if (_networkManager.IsConnectedClient)
        {
            _networkManager.Shutdown();
        }
    }

    public void Dispose()
    {
        if (_networkManager == null) return;

        _networkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;

        if (_networkManager.IsListening)
        {
            _networkManager.Shutdown();
        }
    }
}
