using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Collections;

public struct LeaderboardEntitiesSerializable : INetworkSerializeByMemcpy, IEquatable<LeaderboardEntitiesSerializable>
{
    public ulong ClientId;
    public FixedString32Bytes PlayerName;
    public int Score;
    public int ProfileIndex;

    public LeaderboardEntitiesSerializable(ulong clientId, FixedString32Bytes playerName, int score, int profileIndex)
    {
        ClientId = clientId;
        PlayerName = playerName;
        Score = score;
        ProfileIndex = profileIndex;
    }
    
    public bool Equals(LeaderboardEntitiesSerializable other)
    {
        return ClientId == other.ClientId 
            && PlayerName.Equals(other.PlayerName) 
            && Score == other.Score
            && ProfileIndex == other.ProfileIndex;
    }
}
