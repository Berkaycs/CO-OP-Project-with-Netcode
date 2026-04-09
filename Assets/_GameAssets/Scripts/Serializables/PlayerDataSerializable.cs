using UnityEngine;
using Unity.Netcode;
using System;

public struct PlayerDataSerializable : INetworkSerializeByMemcpy, IEquatable<PlayerDataSerializable> // IEquatable is used to compare two PlayerDataSerializable objects
{
    public ulong ClientId;
    public int ColorId;

    public PlayerDataSerializable(ulong clientId, int colorId)
    {
        ClientId = clientId;
        ColorId = colorId;
    }

    public bool Equals(PlayerDataSerializable other)
    {
        return ClientId == other.ClientId && ColorId == other.ColorId;
    }
}
