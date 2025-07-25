using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using NTools;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

public class ServerManager : IDisposable
{
    private readonly NetworkManager networkManager;

    public ServerManager (GameSettings gameSettings)
    {
        GameSettings = gameSettings;
        networkManager = NetworkManager.Singleton;

        networkManager.ConnectionApprovalCallback += ConnectionApprovalHandle;
        networkManager.OnServerStarted += OnServerStartHandle;
    }

    public bool IsBusy { get; private set; }

    public GameSettings GameSettings { get; set; }

    public int AmountOfClients => ClientIdToUserData.Count;
    public Dictionary<ulong, UserData> ClientIdToUserData { get; } = new();

    public Action OnServerFull { get; set; }
    public Action<ulong> OnClientLeft { get; set; }

    public void Dispose()
    {
        networkManager.ConnectionApprovalCallback -= ConnectionApprovalHandle;
        networkManager.OnServerStarted -= OnServerStartHandle;
        networkManager.OnClientConnectedCallback -= ClientConnectHandle;
        networkManager.OnClientDisconnectCallback -= ClientDisconnectHandle;
    }

    private void OnServerStartHandle()
    {
        networkManager.OnClientConnectedCallback += ClientConnectHandle;
        networkManager.OnClientDisconnectCallback += ClientDisconnectHandle;
    }

    private void ConnectionApprovalHandle (NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        if (ClientIdToUserData.Count == 2)
        {
            Debug.Log("Room is full");
            response.Approved = false;
            return;
        }

        var payload = Encoding.UTF8.GetString(request.Payload);
        var userData = JsonUtility.FromJson<UserData>(payload);

        ClientIdToUserData[request.ClientNetworkId] = userData;

        response.Approved = true;

        if (ClientIdToUserData.Count == 2)
            OnServerFull?.Invoke();
    }

    private async void ClientConnectHandle (ulong clientId)
    {
        if (IsBusy)
            await UniTask.WaitWhile(() => IsBusy);

        IsBusy = true;
        if (clientId == 0)
            await UniTask.Delay(1000);

        var playerInstance = Object.Instantiate(ServiceLocator.ApplicationController.PlayerPrefab);

        var networkObject = playerInstance.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, playerInstance);

        await UniTask.WaitUntil(() => networkObject.IsSpawned);

        var userData = ClientIdToUserData[clientId];
        userData.clientId = clientId;
        userData.playerObjectId = playerInstance.NetworkObjectId;
        userData.playerSide = AmountOfClients == 1 ? PlayerSide.Cross : PlayerSide.Circle;
        ClientIdToUserData[userData.clientId] = userData;

        if (AmountOfClients == 2)
            ServiceLocator.GameReferences.GameManager.NotifyAllPlayersConnectedClientRpc();

        await ServiceLocator.GameReferences.GameManager.AddClient(userData, GameSettings);
        IsBusy = false;

        if (AmountOfClients < 2)
            return;

        _ = new NTask(ServiceLocator.GameReferences.GameManager.BeginGame());
    }

    public async void AddBotAsClient (UserData botData)
    {
        if (IsBusy)
            await UniTask.WaitWhile(() => IsBusy);

        var botInstance = Object.Instantiate(ServiceLocator.ApplicationController.BotPrefab);
        var serverId = 0u;
        var networkObject = botInstance.GetComponent<NetworkObject>();
        networkObject.Spawn();
        await UniTask.WaitUntil(() => networkObject.IsSpawned);
        networkObject.ChangeOwnership(serverId);

        await UniTask.WaitUntil(() => networkObject.IsSpawned);

        var userData = botData;
        userData.clientId = serverId;
        userData.playerObjectId = botInstance.NetworkObjectId;
        userData.playerSide = PlayerSide.Circle;
        ClientIdToUserData.TryAdd(1u, userData);

        botInstance.Setup(userData, ServiceLocator.GameReferences.TopPlayerHud,
            ServiceLocator.WaitingRoomReferences.TopWaitingRoomPlayerEntry);

        ServiceLocator.GameReferences.GameManager.NotifyAllPlayersConnectedClientRpc();
    }

    private void ClientDisconnectHandle (ulong clientId)
    {
        Debug.Log($"Client Disconnected: {clientId}");
        OnClientLeft?.Invoke(clientId);
    }
}