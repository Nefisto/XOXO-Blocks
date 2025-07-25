using Unity.Netcode;

public struct UserData : INetworkSerializable
{
    // Filled by client
    public string userName;

    // Filled by server
    public ulong clientId;
    public ulong playerObjectId;
    public PlayerSide playerSide;

    public bool isBot;

    public void NetworkSerialize<T> (BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref userName);
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref playerObjectId);
        serializer.SerializeValue(ref playerSide);
    }
}