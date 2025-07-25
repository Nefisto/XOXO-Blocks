using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class HostManager : IDisposable
{
    private CancellationTokenSource cts;

    private GameSettings gameSettings;

    private ServerManager serverManager;
    public string JoinCode { get; private set; }
    private string LobbyId { get; set; }

    public void Dispose() => Shutdown();

    public async UniTask StartHostAsync (GameSettings gameSettings)
    {
        this.gameSettings = gameSettings;

        await SetupRelayAllocation();
        await SetupLobby();

        serverManager = new ServerManager(gameSettings);
        serverManager.OnServerFull += ServerFilledHandle;
        serverManager.OnClientLeft += ClientLeftHandle;
        ServiceLocator.Network.ServerManager = serverManager;

        await ServiceLocator.FadingManager.FadeInAsync(1.5f);

        var userData = new UserData { userName = CommonOperations.GetUsername };
        var json = JsonUtility.ToJson(userData);
        var payloadBytes = Encoding.UTF8.GetBytes(json);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.OnLoad += LoadSceneHandle;

        NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    private void LoadSceneHandle (ulong clientId, string sceneName, LoadSceneMode _, AsyncOperation asyncOperation)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId)
            return;

        if (sceneName != GameConstants.LOBBY_SCENE_NAME)
            return;

        BackToLobby();
        Shutdown();
    }

    private async void BackToLobby()
    {
        await UniTask.WaitForSeconds(0.5f);
        GameEvents.LobbyEvents.LoadedLobbyScene?.Invoke(this, EventArgs.Empty);
        await ServiceLocator.FadingManager.FadeOutAsync(1.5f);
    }

    private void ServerFilledHandle() => cts.Cancel();

    public async void Shutdown()
    {
        cts?.Cancel();
        serverManager?.Dispose();

        await LobbyService.Instance.DeleteLobbyAsync(LobbyId);
        NetworkManager.Singleton.Shutdown();
    }

    public async void UnexpectedDisconnect()
    {
        cts.Cancel();
        serverManager?.Dispose();

        await LobbyService.Instance.DeleteLobbyAsync(LobbyId);
        NetworkManager.Singleton.Shutdown();
        ServiceLocator.GameReferences.GameManager.DisconnectedClientRpc();
    }

    private void ClientLeftHandle (ulong clientId)
    {
        UnexpectedDisconnect();
    }

    private async UniTask SetupLobby()
    {
        var lobbyOptions = new CreateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                {
                    GameConstants.NetworkConstants.LobbyConstants.GAME_VERSION,
                    new DataObject(DataObject.VisibilityOptions.Public, $"{Application.version}")
                },
                {
                    GameConstants.NetworkConstants.LobbyConstants.JOIN_CODE,
                    new DataObject(DataObject.VisibilityOptions.Public, $"{JoinCode}")
                },
                {
                    GameConstants.NetworkConstants.LobbyConstants.GRID_SIZE,
                    new DataObject(DataObject.VisibilityOptions.Public, $"{gameSettings.BoardSize}")
                },
                {
                    GameConstants.NetworkConstants.LobbyConstants.STICKY_BOMB_MODE,
                    new DataObject(DataObject.VisibilityOptions.Public, $"{gameSettings.StickyBomb}")
                },
                {
                    GameConstants.NetworkConstants.LobbyConstants.DRAFT_DRAW_MODE,
                    new DataObject(DataObject.VisibilityOptions.Public, $"{gameSettings.DraftDraw}")
                }
            },
            IsPrivate = false
        };
        var lobby = await LobbyService
            .Instance
            .CreateLobbyAsync($"{CommonOperations.GetUsername} lobby", 2, lobbyOptions);
        LobbyId = lobby.Id;
        cts = new CancellationTokenSource();
        HeartbeatLobby(cts.Token).Forget();
    }

    private async UniTask SetupRelayAllocation()
    {
        var allocation = await RelayService.Instance.CreateAllocationAsync(2);
        JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(allocation.ToRelayServerData(GameConstants.NetworkConstants.CONNECTION_TYPE));
    }

    private async UniTask HeartbeatLobby (CancellationToken ct = default)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();

            await LobbyService.Instance.SendHeartbeatPingAsync(LobbyId);
            await new WaitForSecondsRealtime(15f)
                .ToUniTask(cancellationToken: ct);
        }
    }
}