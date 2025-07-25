using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public partial class GameManager : NetworkBehaviour
{
    private Dictionary<ulong, bool> clientIdToStatus = new();

    protected override void OnInSceneObjectsSpawned()
    {
        base.OnInSceneObjectsSpawned();

        clientIdToStatus = new Dictionary<ulong, bool>();
        ServiceLocator.GameReferences.GameManager = this;

        GameEvents.GameplayEvents.OnSetupNewGame += NewGameHandle;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        GameEvents.GameplayEvents.OnSetupNewGame -= NewGameHandle;
    }

    private IEnumerator NewGameHandle (object arg1, EventArgs arg2)
    {
        var ctx = arg2 as GameEvents.OnSetupNewGameEventArgs;

        Assert.IsNotNull(ctx);

        ServiceLocator.GameSettings = ctx.GameSettings;
        yield break;
    }

    public async UniTask AddClient (UserData userData, GameSettings gameSettings)
    {
        clientIdToStatus.Add(userData.clientId, false);

        RequestReferenceSetupClientRpc(CommonOperations.Network.SendTo(userData.clientId));
        await UniTask.WaitUntil(() => clientIdToStatus[userData.clientId]);

        ResetReadinessStatus();
        RequestSetupNewGameForAllClientRpc(gameSettings.BoardSize, gameSettings.StickyBomb, gameSettings.DraftDraw);
        await UniTask.WaitUntil(() => clientIdToStatus.All(t => t.Value));

        foreach (var ud in ServiceLocator.Network.ServerManager.ClientIdToUserData.Values)
            SetupPlayerInGameClientRpc(ud);

        if (userData.clientId == 0)
        {
            ServiceLocator.Network.LobbyInfoHud.Setup(ServiceLocator.Network.HostManager.JoinCode);
            ServiceLocator.Network.LobbyInfoHud.EnableRoomInfo();
        }

        ResetReadinessStatus();
        RequestSceneSetupForClientRpc(CommonOperations.Network.SendTo(userData.clientId));
        await UniTask.WaitUntil(() => clientIdToStatus[userData.clientId]);

        ResetReadinessStatus();
        RequestFadeOutClientRpc(1.5f, CommonOperations.Network.SendTo(userData.clientId));
        await UniTask.WaitUntil(() => clientIdToStatus[userData.clientId]);
    }

    // Clean: Change to Async
    public IEnumerator BeginGame()
    {
        ResetReadinessStatus();
        RequestCloseWaitingRoomForClientRpc();
        yield return new WaitUntil(() => clientIdToStatus.All(t => t.Value));
        yield return new WaitForSeconds(0.5f);

        // First draw phase
        yield return GameEvents.GameplayEvents.BeginningDrawPhase?.YieldableInvoke(this, EventArgs.Empty);

        ResetReadinessStatus();
        RequestNewGameStartingNotificationForClientRpc();
        yield return new WaitUntil(() => clientIdToStatus.All(t => t.Value));

        // Some animation for game start
        while (!GameHasFinished())
            yield return GameEvents.GameplayEvents.NewTurnSetup?.YieldableInvoke(this, EventArgs.Empty);

        NotifyGameEndClientRpc(CommonOperations.GetWinnerSide());
        yield return new WaitForSeconds(3f);

        ResetReadinessStatus();
        RequestFadeInClientRpc(1.5f);
        yield return new WaitUntil(() => clientIdToStatus.All(t => t.Value));

        if (IsServer)
            yield return new WaitForSeconds(.5f);

        NetworkManager.Singleton.SceneManager.LoadScene(GameConstants.LOBBY_SCENE_NAME, LoadSceneMode.Single);
    }

    private void ResetReadinessStatus()
    {
        clientIdToStatus = clientIdToStatus.ToDictionary(t => t.Key, t => false);
    }

    private bool GameHasFinished()
    {
        var gameState = ServiceLocator.GameState;
        if (gameState.PointsOfCircle < 3
            && gameState.PointsOfCross < 3)
            return false;

        if (gameState.PointsOfCross == gameState.PointsOfCircle)
            return false;

        return true;
    }

    public void CleanGameState()
    {
        ServiceLocator.GameState = new GameState();
    }

    #region Server RPCs

    // Clean: This READINESS status could be abstracted to a plain class, it`s a thing that will be used a lot

    [ServerRpc(RequireOwnership = false)]
    private void NotifyReadinessServerRpc (ulong clientId) => clientIdToStatus[clientId] = true;

    #endregion

    #region Client RPCs

    [ClientRpc]
    private void SetupPlayerInGameClientRpc (UserData userData)
    {
        var player = CommonOperations.Network.GetNetworkObjectId<Player>(userData.playerObjectId);
        if (player.HasAlreadySetup)
            return;

        var isMyData = userData.clientId == NetworkManager.Singleton.LocalClientId;
        var playerHud = isMyData
            ? ServiceLocator.GameReferences.BottomPlayerHud
            : ServiceLocator.GameReferences.TopPlayerHud;
        var waitingRoomPlayerEntry = isMyData
            ? ServiceLocator.WaitingRoomReferences.BottomWaitingRoomPlayerEntry
            : ServiceLocator.WaitingRoomReferences.TopWaitingRoomPlayerEntry;

        player.Setup(userData, playerHud, waitingRoomPlayerEntry);
    }

    [ClientRpc]
    private void RequestNewGameStartingNotificationForClientRpc()
    {
        StartCoroutine(Routine());

        IEnumerator Routine()
        {
            yield return GameEvents.GameplayEvents.OnStartingNewGame?.YieldableInvoke(this, EventArgs.Empty);

            NotifyReadinessServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ClientRpc]
    private void RequestReferenceSetupClientRpc (ClientRpcParams clientRpcParams = default)
    {
        StartCoroutine(Routine());

        IEnumerator Routine()
        {
            yield return GameEvents.GameplayEvents.OnSetupReferencesNewGame?.YieldableInvoke(this, EventArgs.Empty);

            NotifyReadinessServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ClientRpc]
    private void RequestFadeOutClientRpc (float fadeTime, ClientRpcParams clientRpcParams = default)
    {
        Routine().Forget();

        async UniTask Routine()
        {
            await ServiceLocator.FadingManager.FadeOutAsync(fadeTime);

            NotifyReadinessServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ClientRpc]
    private void RequestFadeInClientRpc (float fadeTime, ClientRpcParams clientRpcParams = default)
    {
        Routine().Forget();

        async UniTask Routine()
        {
            await ServiceLocator.FadingManager.FadeInAsync(fadeTime);

            NotifyReadinessServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ClientRpc]
    private void RequestSceneSetupForClientRpc (ClientRpcParams clientRpcParams = default)
    {
        StartCoroutine(Routine());

        IEnumerator Routine()
        {
            yield return GameEvents.GameplayEvents.OnSceneSetup?.YieldableInvoke(this, EventArgs.Empty);

            NotifyReadinessServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ClientRpc]
    private void RequestCloseWaitingRoomForClientRpc (ClientRpcParams clientRpcParams = default)
    {
        StartCoroutine(Routine());

        IEnumerator Routine()
        {
            yield return GameEvents.WaitingRoom.OnClosingWaitingRoom?.YieldableInvoke(this, EventArgs.Empty);

            NotifyReadinessServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ClientRpc]
    private void NotifyGameEndClientRpc (PlayerSide winner)
    {
        GameEvents.GameplayEvents.OnGameEnd?.Invoke(winner);
    }

    [ClientRpc]
    public void DisconnectedClientRpc()
    {
        StartCoroutine(Routine());

        IEnumerator Routine()
        {
            yield return GameEvents.GameplayEvents.OnClientDisconnected?.YieldableInvoke(this, EventArgs.Empty);

            SceneManager.LoadScene(GameConstants.LOBBY_SCENE_NAME);
        }
    }

    [ClientRpc]
    private void RequestSetupNewGameForAllClientRpc (int boardSize, bool isStickyMode, bool isDraftDrawMode)
    {
        CleanGameState();
        StartCoroutine(Routine());

        IEnumerator Routine()
        {
            yield return GameEvents.GameplayEvents.OnSetupNewGame?.YieldableInvoke(this,
                new GameEvents.OnSetupNewGameEventArgs
                {
                    GameSettings = new GameSettings
                    {
                        BoardSize = boardSize,
                        StickyBomb = isStickyMode,
                        DraftDraw = isDraftDrawMode,
                        HandSize = 3
                    }
                });

            NotifyReadinessServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ClientRpc]
    public void NotifyAllPlayersConnectedClientRpc()
    {
        StartCoroutine(Routine());

        IEnumerator Routine()
        {
            yield return GameEvents.GameplayEvents.AllPlayersConnected?.YieldableInvoke(this, EventArgs.Empty);

            NotifyReadinessServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    #endregion
}