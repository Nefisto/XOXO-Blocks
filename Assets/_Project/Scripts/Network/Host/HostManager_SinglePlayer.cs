using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using NTools;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class HostManager
{
    public async UniTask StartSinglePlayerHostAsync (GameSettings gameSettings)
    {
        this.gameSettings = gameSettings;

        await SetupRelayAllocationForSinglePlayer();
        await SetupLobbyForSinglePlayer();

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

        await UniTask.WaitWhile(() => serverManager.IsBusy);

        serverManager.AddBotAsClient(new UserData
        {
            userName = "Botson",
            isBot = true
        });

        await UniTask.WaitForSeconds(.2f);

        _ = new NTask(ServiceLocator.GameReferences.GameManager.BeginGame());
    }

    private async UniTask SetupRelayAllocationForSinglePlayer()
    {
        var allocation = await RelayService.Instance.CreateAllocationAsync(1);
        JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(allocation.ToRelayServerData(GameConstants.NetworkConstants.CONNECTION_TYPE));
    }

    private async UniTask SetupLobbyForSinglePlayer()
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
            IsPrivate = true
        };
        var lobby = await LobbyService
            .Instance
            .CreateLobbyAsync($"{CommonOperations.GetUsername} lobby", 1, lobbyOptions);
        LobbyId = lobby.Id;
    }
}