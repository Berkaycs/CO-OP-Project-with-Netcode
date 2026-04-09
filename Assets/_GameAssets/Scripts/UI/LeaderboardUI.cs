using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class LeaderboardUI : NetworkBehaviour // it's controlled by the server
{
    [SerializeField] private LeaderboardRankingUI _leaderboardRankingUIPrefab;
    [SerializeField] private Transform _rankingParent;
    [SerializeField] private TMP_Text _rankText;
    
    private NetworkList<LeaderboardEntitiesSerializable> _leaderboardEntitiesList = new NetworkList<LeaderboardEntitiesSerializable>();
    private List<LeaderboardRankingUI> _leaderboardRankingList = new List<LeaderboardRankingUI>();

    override public void OnNetworkSpawn()
    {
        if (IsClient)
        {
            _leaderboardEntitiesList.OnListChanged += LeaderboardEntitiesList_OnListChanged;

            foreach (LeaderboardEntitiesSerializable entity in _leaderboardEntitiesList)
            {
                LeaderboardEntitiesList_OnListChanged(new NetworkListEvent<LeaderboardEntitiesSerializable>
                {
                    Type = NetworkListEvent<LeaderboardEntitiesSerializable>.EventType.Add,
                    Value = entity
                });
            }
        }

        if (IsServer)
        {
            PlayerNetworkController[] players = FindObjectsByType<PlayerNetworkController>(FindObjectsSortMode.None);
            foreach (PlayerNetworkController player in players)
            {
                PlayerNetworkController_OnPlayerSpawned(player);
            }

            PlayerNetworkController.OnPlayerSpawned += PlayerNetworkController_OnPlayerSpawned;
            PlayerNetworkController.OnPlayerDespawned += PlayerNetworkController_OnPlayerDespawned;
        }
    }

    private void LeaderboardEntitiesList_OnListChanged(NetworkListEvent<LeaderboardEntitiesSerializable> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<LeaderboardEntitiesSerializable>.EventType.Add:
                if (!_leaderboardRankingList.Any(ui => ui.ClientId == changeEvent.Value.ClientId))
                {
                    LeaderboardRankingUI leaderboardRankingUI 
                    = Instantiate(_leaderboardRankingUIPrefab, _rankingParent);

                    leaderboardRankingUI.SetData(
                    changeEvent.Value.ClientId, 
                    changeEvent.Value.PlayerName, 
                    changeEvent.Value.Score, 
                    changeEvent.Value.ProfileIndex
                    );

                    _leaderboardRankingList.Add(leaderboardRankingUI);
                }

                UpdatePlayerRankText();

                break;
            case NetworkListEvent<LeaderboardEntitiesSerializable>.EventType.Value:
                LeaderboardRankingUI leaderboardRankingToUpdate
                    = _leaderboardRankingList.FirstOrDefault(ui => ui.ClientId == changeEvent.Value.ClientId);

                if (leaderboardRankingToUpdate != null)
                {
                    leaderboardRankingToUpdate.UpdateScore(changeEvent.Value.Score);
                }
                break;
            
            case NetworkListEvent<LeaderboardEntitiesSerializable>.EventType.Remove:
                LeaderboardRankingUI leaderboardRankingToRemove
                    = _leaderboardRankingList.FirstOrDefault(ui => ui.ClientId == changeEvent.Value.ClientId);
                
                if (leaderboardRankingToRemove != null)
                {
                    leaderboardRankingToRemove.transform.SetParent(null);
                    Destroy(leaderboardRankingToRemove.gameObject);
                    _leaderboardRankingList.Remove(leaderboardRankingToRemove);
                    UpdatePlayerRankText();
                }
                break;
        }

        UpdateSortingOrder();
    }

    private void PlayerNetworkController_OnPlayerSpawned(PlayerNetworkController playerNetworkController)
    {
        _leaderboardEntitiesList.Add(new LeaderboardEntitiesSerializable
        {
            ClientId = playerNetworkController.OwnerClientId,
            PlayerName = playerNetworkController.PlayerName.Value,
            Score = 0,
            ProfileIndex = playerNetworkController.ProfileIndex.Value
        });

        playerNetworkController.GetPlayerScoreController().PlayerScore.OnValueChanged 
            += (oldScore, newScore) => PlayerScoreController_OnPlayerScoreChanged(playerNetworkController.OwnerClientId, newScore);
    }

    private void PlayerScoreController_OnPlayerScoreChanged(ulong clientId, int newScore)
    {
        for (int i = 0; i < _leaderboardEntitiesList.Count; i++)
        {
            if (_leaderboardEntitiesList[i].ClientId != clientId) continue;

            _leaderboardEntitiesList[i] = new LeaderboardEntitiesSerializable
            {
                ClientId = _leaderboardEntitiesList[i].ClientId,
                PlayerName = _leaderboardEntitiesList[i].PlayerName,
                Score = newScore,
                ProfileIndex = _leaderboardEntitiesList[i].ProfileIndex
            };

            UpdatePlayerRankText();
            
            return;
        }
    }

    private void PlayerNetworkController_OnPlayerDespawned(PlayerNetworkController playerNetworkController)
    {
        if (_leaderboardEntitiesList == null) return;

        foreach (LeaderboardEntitiesSerializable entity in _leaderboardEntitiesList)
        {
            if (entity.ClientId != playerNetworkController.OwnerClientId) continue;

            _leaderboardEntitiesList.Remove(entity);
            break;
        }

        playerNetworkController.GetPlayerScoreController().PlayerScore.OnValueChanged 
            -= (oldScore, newScore) => PlayerScoreController_OnPlayerScoreChanged(playerNetworkController.OwnerClientId, newScore);
    }

    private void UpdateSortingOrder()
    {
        _leaderboardRankingList.Sort((x, y) => y.Score.CompareTo(x.Score));

        for (int i = 0; i < _leaderboardRankingList.Count; i++)
        {
            _leaderboardRankingList[i].transform.SetSiblingIndex(i);
            _leaderboardRankingList[i].UpdateRank();
        }

        UpdatePlayerRankText();
    }

    private void UpdatePlayerRankText()
    {
        LeaderboardRankingUI myRanking 
            = _leaderboardRankingList.FirstOrDefault(ui => ui.ClientId == NetworkManager.Singleton.LocalClientId);

        if (myRanking == null) return;

        int rank = myRanking.transform.GetSiblingIndex() + 1;
        string suffix = GetRankSuffix(rank);

        _rankText.text = $"{rank}<sup>{suffix}</sup>/{_leaderboardRankingList.Count}";
    }

    private string GetRankSuffix(int rank)
    {
        return rank switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
    }

    public List<LeaderboardEntitiesSerializable> GetLeaderboardData()
    {
        List<LeaderboardEntitiesSerializable> leaderboardData = new List<LeaderboardEntitiesSerializable>();

        foreach (LeaderboardEntitiesSerializable entity in _leaderboardEntitiesList)
        {
            leaderboardData.Add(entity);
        }

        return leaderboardData;
    }

    public string GetWinnerName()
    {
        if (_leaderboardRankingList.Count > 0)
        {
            return _leaderboardRankingList[0].GetPlayerName();
        }

        return "No Winner";
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            _leaderboardEntitiesList.OnListChanged -= LeaderboardEntitiesList_OnListChanged;
        }

        if (IsServer)
        {
            PlayerNetworkController.OnPlayerSpawned -= PlayerNetworkController_OnPlayerSpawned;
            PlayerNetworkController.OnPlayerDespawned -= PlayerNetworkController_OnPlayerDespawned;
        }
    }
}
