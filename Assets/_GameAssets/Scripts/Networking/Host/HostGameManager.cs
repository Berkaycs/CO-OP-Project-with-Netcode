using Cysharp.Threading.Tasks;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Text;
using Unity.Services.Authentication;

public class HostGameManager : IDisposable
{
    private const int MAX_PLAYERS = 4;

    public NetworkServer NetworkServer { get; private set; }

    private Allocation _allocation;
    private string _joinCode;
    private string _lobbyId;

    public async UniTask StartHostAsync()
    {
        try
        {
            _allocation = await RelayService.Instance.CreateAllocationAsync(MAX_PLAYERS);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create allocation: {ex.Message}");
            return;
        }

        try
        {
            _joinCode = await RelayService.Instance.GetJoinCodeAsync(_allocation.AllocationId);
            Debug.Log($"Join Code: {_joinCode}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to get join code: {ex.Message}");
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(AllocationUtils.ToRelayServerData(_allocation, "dtls")); // dtls is the safe utp

        try
        {
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions();
            createLobbyOptions.IsPrivate = false;
            createLobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject
                    (
                        visibility: DataObject.VisibilityOptions.Member,
                        value: _joinCode
                    )
                }
            };

            string playerName = PlayerPrefs.GetString(Consts.PlayerData.PLAYER_NAME, "Noname");

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync
                    ($"{playerName}'s Lobby", MAX_PLAYERS, createLobbyOptions);

            _lobbyId = lobby.Id;

            HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15f));
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to create lobby: {ex.Message}");
            return;
        }
        
        NetworkServer = new NetworkServer(NetworkManager.Singleton);

        UserData userData = new UserData
        {
            UserName = PlayerPrefs.GetString(Consts.PlayerData.PLAYER_NAME, "Noname"),
            UserAuthId = AuthenticationService.Instance.PlayerId,
            ProfileIndex = PlayerPrefs.GetInt(Consts.PlayerData.PROFILE_INDEX, 0)
        };

        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartHost();

        NetworkManager.Singleton.SceneManager.LoadScene(Consts.Scenes.CHARACTER_SELECT_SCENE, LoadSceneMode.Single);
    }

    private IEnumerator HeartbeatLobby(float waitTimeSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(_lobbyId);
            yield return delay;
        }
    }

    public string GetJoinCode()
    {
        return _joinCode;
    }

    public async void Shutdown()
    {
        HostSingleton.Instance.StopCoroutine(HeartbeatLobby(15f));

        if (!string.IsNullOrEmpty(_lobbyId))
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(_lobbyId);
            }
            catch (LobbyServiceException ex)
            {
                Debug.Log($"Failed to delete lobby: {ex.Message}");
            }

            _lobbyId = string.Empty;
        }

        NetworkServer?.Dispose();
    }

    public void Dispose()
    {
        Shutdown();
    }
}
