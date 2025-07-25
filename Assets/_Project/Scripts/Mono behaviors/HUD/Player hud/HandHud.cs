using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using QFSW.QC;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class HandHudSettings
{
    public float spaceBetweenPieces = .5f;
    public float heightPosition = -3.5f;
}

public enum WaitingState
{
    None,
    Waiting,
    Active
}

public partial class HandHud : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private HandHudSettings waitingSettings;

    [SerializeField]
    private HandHudSettings activeSettings;

    [SerializeField]
    private bool draggableHand = true;

    [Header("References")]
    [SerializeField]
    private Transform outOfScreenPosition;

    private readonly Dictionary<Piece, PositionInformation> pieceToPositions = new();
    private Hand hand;
    private float handScaleMultiplier;

    private WaitingState waitingState;

    private HandHudSettings CorrectSettings
        => waitingState == WaitingState.Active
            ? activeSettings
            : waitingSettings;

    private void OnDestroy()
    {
        if (hand == null)
            return;

        hand.OnAddPiece -= AddPiece;
        hand.OnDetachPiece -= DetachPiece;
        hand = null;
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var info in pieceToPositions.Values)
            Gizmos.DrawWireSphere(info.position, .2f);
    }

    public void Setup (Hand hand, float handScale)
    {
        waitingState = WaitingState.None;
        handScaleMultiplier = handScale;
        name = $"{ServiceLocator.PlayerSide} hand";

        this.hand = hand;
        hand.OnAddPiece += AddPiece;
        hand.OnDetachPiece += DetachPiece;
        hand.OnRemovePiece += RemovePiece;

        if (hand.DraggingController == null)
            return;

        hand.DraggingController.OnSelectPiece += DraggingPieceHandle;
        hand.DraggingController.OnPlaceFailPiece += FailPlaceHandle;
        hand.DraggingController.OnPlaceSuccessfullyPiece += SuccessPlaceHandle;
    }

    public async void WaitState()
    {
        if (waitingState == WaitingState.Waiting)
        {
            Debug.Log("Trying to wait for an already waiting state");
            return;
        }

        waitingState = WaitingState.Waiting;

        foreach (var (piece, _) in pieceToPositions)
            piece.SetWaitingState().Forget();
        await RecalculatePositions();
    }

    public async void ActiveState()
    {
        if (waitingState == WaitingState.Active)
        {
            Debug.Log("Trying to wait for an already waiting state");
            return;
        }

        waitingState = WaitingState.Active;

        foreach (var (piece, _) in pieceToPositions)
            piece.SetActiveState().Forget();
        await RecalculatePositions();
    }

    private void RemovePiece (Piece piece)
    {
        MoveToOutside().Forget();

        async UniTask MoveToOutside()
        {
            await piece
                .transform
                .DOMove(outOfScreenPosition.position, 0.5f)
                .AsyncWaitForCompletion();

            piece.OnBeingPlaced -= OnPlacePieceHandle;
            pieceToPositions.Remove(piece);
        }
    }

    private void SuccessPlaceHandle (Piece piece) { }

    private async void FailPlaceHandle (Piece piece)
    {
        pieceToPositions[piece].isDisabled = false;
        ReleasePieceAnimation(piece);

        await RecalculatePositions();
    }

    private async void DraggingPieceHandle (Piece piece)
    {
        pieceToPositions[piece].isDisabled = true;
        DraggingPieceAnimation(piece);

        await RecalculatePositions();
    }

    private static void DraggingPieceAnimation (Piece piece)
    {
        piece.ScaleAnimation(1f, .5f);
        piece.FadeContourAnimation(0f, .5f);
    }

    private static void ReleasePieceAnimation (Piece piece)
    {
        piece.ScaleAnimation(0.75f, .5f);
        piece.FadeContourAnimation(.5f, .5f);
    }

    private async void AddPiece (Piece piece)
    {
        piece.OnBeingPlaced += OnPlacePieceHandle;

        piece.CanBeDragged = draggableHand;

        piece.transform.position = outOfScreenPosition.position;
        piece.transform.localScale = Vector3.one * handScaleMultiplier;
        pieceToPositions.Add(piece, new PositionInformation { position = piece.transform.position });

        await RecalculatePositions();
    }

    private async void DetachPiece (Piece piece)
    {
        piece.OnBeingPlaced -= OnPlacePieceHandle;
        pieceToPositions.Remove(piece);

        await RecalculatePositions();
    }

    [Command]
    private async UniTask RecalculatePositions()
    {
        var validPieces = pieceToPositions
            .Where(t => t.Value.isDisabled == false)
            .ToDictionary(t => t.Key, t => t.Value);

        var correctSettings = CorrectSettings;
        var rootTransform = transform;
        var fullWidth = validPieces.Sum(t => t.Key.PieceSize.x)
                        + CorrectSettings.spaceBetweenPieces * Mathf.Max(hand.Pieces.Count - 2, 0);
        var currentPosition = rootTransform.position.x - fullWidth * 0.5f;
        foreach (var (piece, _) in validPieces)
        {
            var pieceSize = piece.PieceCollider.size;
            var offset = piece.PieceCollider.offset * handScaleMultiplier;

            currentPosition += pieceSize.x * (.5f * handScaleMultiplier);
            pieceToPositions[piece].position =
                new Vector3(currentPosition - offset.x, correctSettings.heightPosition, 0f);
            currentPosition += pieceSize.x * (.5f * handScaleMultiplier) + CorrectSettings.spaceBetweenPieces;
        }

        var sequence = DOTween.Sequence();
        foreach (var (piece, info) in validPieces)
            sequence.Join(piece.transform.DOMove(info.position, .5f));

        await sequence.AsyncWaitForCompletion();
    }

    private static void OnPlacePieceHandle (object caller, EventArgs args)
    {
        var piece = (Piece)caller;

        Assert.IsNotNull(piece);

        DraggingPieceAnimation(piece);
    }

    private class PositionInformation
    {
        public bool isDisabled;
        public Vector3 position;
    }
}