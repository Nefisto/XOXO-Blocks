using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

public class TurnController : NetworkBehaviour
{
    private TurnState currentTurnState;
    private bool finishedTurnByTimeout;
    private bool finishTurnBySelectingPiece;
    private GameState gameState;

    private int initialDrawingOnGoing;

    public NetworkVariable<int> CurrentTurn { get; } = new();

    public PlayerSide CurrentPlayerSide => CurrentTurn.Value % 2 != 0 ? PlayerSide.Cross : PlayerSide.Circle;

    public override void OnNetworkSpawn()
    {
        ServiceLocator.GameReferences.TurnController = this;

        GameEvents.GameplayEvents.OnSetupReferencesNewGame += SetupHandle;
        GameEvents.GameplayEvents.BeginningDrawPhase += BeginningDrawHandle;
        GameEvents.GameplayEvents.NewTurnSetup += NewTurnHandle;
    }

    public override void OnNetworkDespawn()
    {
        GameEvents.GameplayEvents.OnSetupReferencesNewGame -= SetupHandle;
        GameEvents.GameplayEvents.BeginningDrawPhase -= BeginningDrawHandle;
        GameEvents.GameplayEvents.NewTurnSetup -= NewTurnHandle;
    }

    private IEnumerator NewTurnHandle (object _, EventArgs args)
    {
        Assert.IsTrue(IsServer);

        currentTurnState = ServiceLocator.TurnState = new TurnState();

        currentTurnState.TurnPhase = TurnPhase.SelectPhase;
        CurrentTurn.Value++;
        RunSelectPhaseClientRpc(CurrentPlayerSide);
        yield return new WaitUntil(() => ServiceLocator.TurnState.HasFinishedSelectPhase);

        currentTurnState.TurnPhase = TurnPhase.AnimatingPhase;
        if (ServiceLocator.TurnState.FinishReason == TurnFinishedReason.PiecePlaced)
            yield return new WaitUntil(() => ServiceLocator.TurnState.HasOpponentFinishedTheirStep
                                             && ServiceLocator.TurnState.HasPlayerFinishedTheirStep);

        currentTurnState.TurnPhase = TurnPhase.DiscardPhase;
        RunDiscardPhaseClientRpc(CurrentPlayerSide);
        yield return new WaitUntil(() => currentTurnState.HasFinishedDiscardPhase);

        yield return new WaitForSeconds(1f);
    }

    [ClientRpc]
    private void RunDiscardPhaseClientRpc (PlayerSide playerSide)
    {
        StartCoroutine(Routine());

        IEnumerator Routine()
        {
            var player = CommonOperations.GetPlayerRefOf(playerSide);

            yield return player.DiscardPhase();
        }
    }

    private IEnumerator BeginningDrawHandle (object arg1, EventArgs arg2)
    {
        initialDrawingOnGoing = 0;
        InitialDrawClientRpc();
        while (true)
        {
            if (initialDrawingOnGoing >= 2)
                break;
            yield return null;
        }
    }

    [ClientRpc]
    private void InitialDrawClientRpc()
    {
        var player = CommonOperations.GetPlayerRefOf(ServiceLocator.PlayerSide);
        if (!player.IsOwner)
            return;

        StartCoroutine(DrawHand(player));

        var opponent = CommonOperations.GetPlayerRefOf(player.PlayerSide.GetInverseSide());
        if (opponent.IsOwner) // Playing against bot
            StartCoroutine(DrawHand(opponent));

        IEnumerator DrawHand (Player target)
        {
            yield return target.InitialDraw();

            FinishBeginningDrawServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void FinishBeginningDrawServerRpc() => initialDrawingOnGoing++;

    [ClientRpc]
    private void RunSelectPhaseClientRpc (PlayerSide playerSide)
    {
        StartCoroutine(Routine());

        IEnumerator Routine()
        {
            var player = CommonOperations.GetPlayerRefOf(playerSide);
            var enemy = CommonOperations.GetEnemyRefOf(playerSide);

            yield return GameEvents.GameplayEvents.OnStartingNewTurn?.YieldableInvoke(this,
                new GameEvents.OnStartNewTurnEventArgs { PlayerSide = playerSide });

            yield return enemy.WaitTurn();
            yield return player.PlayTurn();
        }
    }

    private IEnumerator SetupHandle (object _, EventArgs args)
    {
        if (IsServer)
            CurrentTurn.Value = 0;

        gameState = ServiceLocator.GameState;
        yield break;
    }
}