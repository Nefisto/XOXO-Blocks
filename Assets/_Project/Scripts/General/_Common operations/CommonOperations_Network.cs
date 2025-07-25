using Unity.Netcode;

public static partial class CommonOperations
{
    public static class Network
    {
        public static T GetNetworkObjectId<T> (ulong networkObjectId)
            => NetworkManager
                .Singleton
                .SpawnManager
                .SpawnedObjects[networkObjectId]
                .GetComponent<T>();

        public static ClientRpcParams SendTo (params ulong[] clientIds)
            => new() { Send = new ClientRpcSendParams { TargetClientIds = clientIds } };
    }
}