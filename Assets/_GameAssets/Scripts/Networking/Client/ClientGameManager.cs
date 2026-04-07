using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using System;
using UnityEngine;
using Unity.Services.Relay;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using System.Text;
using Unity.Services.Authentication;

public class ClientGameManager : IDisposable
{
    private JoinAllocation _joinAllocation;
    private NetworkClient _networkClient;

    public async UniTask<bool> InitAsync()
    {
        // AUTHENTICATE PLAYER
        await UnityServices.InitializeAsync();

        _networkClient = new NetworkClient(NetworkManager.Singleton);

        AuthenticationState authenticationState = await AuthenticationHandler.DoAuth();

        if (authenticationState == AuthenticationState.Authenticated)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(Consts.Scenes.MENU_SCENE);
    }

    public async UniTask StartClientAsync(string joinCode)
    {
        try
        {
            _joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to join allocation: {ex.Message}");
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(AllocationUtils.ToRelayServerData(_joinAllocation, "dtls"));

        UserData userData = new UserData
        {
            UserName = PlayerPrefs.GetString(Consts.PlayerData.PLAYER_NAME, "Noname"),
            UserAuthId = AuthenticationService.Instance.PlayerId
        };

        // Set the connection data for the client
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartClient();
    }

    public void Dispose()
    {
        _networkClient?.Dispose();
    }
}