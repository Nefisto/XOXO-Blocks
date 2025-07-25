using System.Collections;
using DG.Tweening;
using NTools;
using Unity.Collections;
using UnityEngine;

public class BotPlayer : Player
{
    public override void Setup (UserData userData, PlayerHUD playerHUD, WaitingRoomPlayerEntry waitingEntry)
    {
        UserData = userData;
        PlayerHUD = playerHUD;
        WaitingRoomPlayerEntry = waitingEntry;

        HasAlreadySetup = true;

        PlayerSide = userData.playerSide;
        Nickname = userData.userName;

        gameObject.name = $"-{userData.playerSide.ToString()} - BOT";

        ServiceLocator.GameReferences.CirclePlayer = this;

        Hand = new Hand();
        SubHand = new Hand();

        Timer = new Timer(GameConstants.GameplayCostants.TURN_TIMER);
        Timer.OnTimerEnd += () =>
        {
            ServiceLocator.TurnState.HasFinishedSelectPhase = true;
            ServiceLocator.TurnState.FinishReason = TurnFinishedReason.TimedOut;
            NotifyClientsThatTimerFinishedClientRpc();
        };

        waitingEntry.SetupEntry(UserData.playerSide, userData.userName);
        ServiceLocator.GameReferences.TopPlayerHud.SetupHUD(userData, Hand, Timer, SubHand);

        currentBag = CreateNewBag();
        GameEvents.GameplayEvents.OnPlacePiece += piece => { Hand.DetachPiece(piece); };
        // // Clean: CAN I REMOVE THIS AND ACCESS SIDE THROUGH MY REF IN SERVICE LOCATOR?
        IsSelectingPiece = false;

        drawBehavior = ServiceLocator.GameSettings.DraftDraw
            ? new DraftDrawBehavior(this, Hand, SubHand, GetNewPiece)
            : new DefaultDrawBehavior(Hand, GetNewPiece);
    }

    public override void GetNewPiece (bool intoMainHand)
    {
        if (currentBag.IsEmpty)
            currentBag = CreateNewBag();

        if (IsHandFull(intoMainHand))
            return;

        var randomizedPieceCode = currentBag.GetPieceCode();
        ServiceLocator.GameReferences.PieceSpawnManager.RequestSpawnPieceServerRpc(PlayerSide.Circle,
            new FixedString32Bytes(randomizedPieceCode), OwnerClientId, intoMainHand, false);
    }

    public override IEnumerator PlayTurn()
    {
        var gridManager = ServiceLocator.GameReferences.GridManager;

        var randomizedPiece = Hand.Pieces.GetRandom();
        var pieceConfiguration = randomizedPiece.GetAllTileOffsets();
        Vector2? targetGrid = null;
        while (true)
        {
            var allGridPositions = gridManager
                .GetAllGridPositions()
                .Shuffle();

            foreach (var gridPosition in allGridPositions)
            {
                if (!gridManager.CanPlacePiece(gridPosition, pieceConfiguration))
                    continue;

                targetGrid = gridPosition;
                break;
            }

            if (targetGrid == null)
            {
                yield return null;
                continue;
            }

            var worldPosition = gridManager.GetWorldPositionOfVector(targetGrid.Value);
            Hand.DetachPiece(randomizedPiece);

            randomizedPiece.ScaleAnimation(1f, .5f);
            randomizedPiece.FadeContourAnimation(0f, .5f);
            randomizedPiece.UpdateLayer(ServiceLocator.GameState.CurrentTurn);

            yield return DOTween
                .Sequence()
                .Append(randomizedPiece.transform.DOMoveY(randomizedPiece.transform.position.y - 1f, 0.2f))
                .Append(randomizedPiece.transform.DOMove(worldPosition, .75f))
                .AppendInterval(0.5f)
                .WaitForCompletion();

            randomizedPiece.transform.position = worldPosition;
            yield return gridManager.PlacePiece(randomizedPiece);

            break;
        }

        yield return null;

        gridManager.CanPlacePiece(targetGrid.Value, pieceConfiguration);

        FinishTurn();
    }

    public override IEnumerator WaitTurn()
    {
        yield break;
    }

    private static void FinishTurn()
    {
        ServiceLocator.TurnState.FinishReason = TurnFinishedReason.PiecePlaced;
        ServiceLocator.TurnState.HasFinishedSelectPhase = true;
        ServiceLocator.TurnState.HasPlayerFinishedTheirStep = true;
        ServiceLocator.TurnState.HasOpponentFinishedTheirStep = true;
    }
}