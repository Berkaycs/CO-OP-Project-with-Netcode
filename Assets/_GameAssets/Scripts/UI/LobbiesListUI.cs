using UnityEngine;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using System.Collections.Generic;

public class LobbiesListUI : MonoBehaviour
{
    [SerializeField] private LobbyItemUI _lobbyItemPrefab;
    [SerializeField] private Transform _lobbyItemsParent;
    [SerializeField] private Button _refreshButton;

    private bool _isJoining;
    private bool _isRefreshing;

    private void Awake()
    {
        _refreshButton.onClick.AddListener(RefreshLobbies);
    }

    public async void JoinAsync(Lobby lobby)
    {
        if (_isJoining) return;
        _isJoining = true;

        try
        {
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
            string joinCode = joinedLobby.Data["JoinCode"].Value;
            ClientSingleton.Instance.ClientGameManager.SetJoinCode(joinCode);

            await ClientSingleton.Instance.ClientGameManager.StartClientAsync(joinCode);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to join lobby: {ex.Message}");
            RefreshLobbies();
        }

        _isJoining = false;
    }

    public async void RefreshLobbies()
    {
        if (_isRefreshing) return;
        _isRefreshing = true;

        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions();
            queryLobbiesOptions.Count = 20;
            queryLobbiesOptions.Filters = new List<QueryFilter>
            {
                // Only show lobbies with available slots (greater than 0 slots)
                new QueryFilter
                (
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0"
                ),
                // Only show lobbies that are not locked (0 means not locked) both of filters are same
                new QueryFilter
                (
                    field: QueryFilter.FieldOptions.IsLocked,
                    op: QueryFilter.OpOptions.EQ,
                    value: "0"
                )
            };

            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            foreach (Transform child in _lobbyItemsParent)
            {
                Destroy(child.gameObject);
            }

            foreach (Lobby lobby in lobbies.Results)
            {
                LobbyItemUI lobbyItemUI = Instantiate(_lobbyItemPrefab, _lobbyItemsParent);
                lobbyItemUI.SetupLobbyItem(this, lobby);
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to refresh lobbies: {ex.Message}");
        }

        _isRefreshing = false;
    }
}
