using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.Netcode;
using UnityEngine;

public class DraggingController : NetworkBehaviour
{
    private bool canDrag;

    [Header("Debug")]
    [ShowInInspector]
    private Vector2 distanceFromClickedPieceToClickedPoint;

    private Piece holdingPiece;
    private Collider2D lastCollider;
    public Action<Piece> OnSelectPiece { get; set; }
    public Action<Piece> OnPlaceFailPiece { get; set; }
    public Action<Piece> OnPlaceSuccessfullyPiece { get; set; }

    private void Start() => ServiceLocator.DraggingController = this;

    private void Update()
    {
        if (!canDrag)
            return;

        if (Input.GetMouseButtonDown(0))
            MousePressClick();

        if (Input.GetMouseButtonUp(0))
            MouseReleaseClick();

        if (holdingPiece == null)
            return;

        var mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        var hit = Physics2D.CircleCast(mouseWorldPosition, 0.1f, Vector2.zero, 0f, LayerMask.GetMask("Grid Tile"));
        if (hit.collider != null)
        {
            if (hit.collider == lastCollider)
                return;

            SnapPiece(hit.collider);

            // Select all pieces on top and set them as top layer
            CommonOperations.SetAllTilesAsFrontLayer();

            holdingPiece
                .GetAllValidOverlappedGridTile()
                .Select(gt => gt.PeekTile())
                .ForEach(t => t.SendToBackLayer());

            holdingPiece.PlacementValidator.UpdateNotification();
            lastCollider = hit.collider;
        }
        else
        {
            if (lastCollider != null)
                CommonOperations.SetAllTilesAsFrontLayer();

            FreeMovement(mouseWorldPosition);
            holdingPiece.PlacementValidator.CancelNotifications();
            lastCollider = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (holdingPiece == null)
            return;

        var pieceTransform = holdingPiece.transform;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pieceTransform.position, 0.1f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere((Vector2)pieceTransform.position + distanceFromClickedPieceToClickedPoint, 0.1f);
        Gizmos.color = Color.magenta;
        var nearestCenter = (Vector2)pieceTransform.position
                            + new Vector2(
                                Mathf.Round(distanceFromClickedPieceToClickedPoint.x + 0.5f),
                                Mathf.Round(distanceFromClickedPieceToClickedPoint.y + 0.5f))
                            - new Vector2(0.5f, 0.5f);
        Gizmos.DrawWireSphere(nearestCenter, 0.1f);
    }

    protected override void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();

        canDrag = true;
    }

    public void DisableDrag() => canDrag = false;
    public void EnableDrag() => canDrag = true;

    private void MousePressClick()
    {
        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var hit = Physics2D.OverlapCircle(mousePosition, 0.25f, LayerMask.GetMask("Piece"));
        if (hit == null)
            return;

        if (!hit.TryGetComponent<Piece>(out var piece) || !piece.NetworkObject.IsOwner)
            return;

        if (!piece.CanBeDragged)
            return;

        SetDraggable(hit.GetComponent<Piece>());
    }

    private void MouseReleaseClick()
    {
        if (holdingPiece == null)
            return;

        holdingPiece.PlacementValidator.CancelNotifications();

        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var hit = Physics2D.OverlapCircle(mousePosition, 0.25f, LayerMask.GetMask("Grid Tile"));
        if (hit == null)
        {
            ReleasePiece();
            return;
        }

        // Iterate over piece tiles and check if all of them are on the valid place
        if (!holdingPiece.CanBePlaced())
        {
            ReleasePiece();
            return;
        }

        GameEvents.GameplayEvents.OnPlacePiece?.Invoke(holdingPiece);
        PlacePieceServerRpc(ServiceLocator.PlayerSide, holdingPiece.NetworkObjectId,
            holdingPiece.transform.position);

        PlacePiece();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlacePieceServerRpc (PlayerSide side, ulong pieceId, Vector3 targetPosition)
    {
        var playerId = CommonOperations.GetPlayerRefOf(side).OwnerClientId;
        var enemyId = CommonOperations.GetEnemyRefOf(side).OwnerClientId;

        PlacePieceOnCurrentPlayerClientRpc(side, pieceId, targetPosition,
            new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { playerId } } });

        // For bot
        if (enemyId == playerId)
        {
            NotifyEndOfTurnForOpponentServerRpc();
            return;
        }

        PlacePieceOnCurrentEnemyClientRpc(side, pieceId, targetPosition,
            new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { enemyId } } });
    }

    [ClientRpc]
    private void PlacePieceOnCurrentPlayerClientRpc (PlayerSide side, ulong pieceId, Vector3 targetPosition,
        ClientRpcParams rpcParams = default)
    {
        StartCoroutine(Routine());

        IEnumerator Routine()
        {
            var piece = CommonOperations.Network.GetNetworkObjectId<Piece>(pieceId);
            piece.transform.position = targetPosition;

            yield return ServiceLocator.GameReferences.GridManager.PlacePiece(piece);
            CommonOperations.RemovePieceFromPlayerSide(side, piece);
            GameEvents.GameplayEvents.OnFinishPlacementAnimation?.Invoke();
            NotifyEndOfTurnForPlayerServerRpc();
        }
    }

    [ClientRpc]
    private void PlacePieceOnCurrentEnemyClientRpc (PlayerSide side, ulong pieceId, Vector3 targetPosition,
        ClientRpcParams rpcParams = default)
    {
        PlacingPieceFromEnemyPovAsync(side, pieceId, targetPosition).Forget();
    }

    private async UniTask PlacingPieceFromEnemyPovAsync (PlayerSide side, ulong pieceId, Vector3 targetPosition)
    {
        var piece = CommonOperations.Network.GetNetworkObjectId<Piece>(pieceId);
        piece.OnBeingPlaced?.Invoke(piece, EventArgs.Empty);

        await piece
            .transform
            .DOMoveY(piece.transform.position.y - 1f, 0.5f)
            .SetEase(Ease.Linear)
            .AsyncWaitForCompletion();

        piece.UpdateLayer(ServiceLocator.GameState.CurrentTurn);

        await piece
            .transform
            .DOMove(targetPosition, .75f)
            .SetEase(Ease.OutBounce)
            .AsyncWaitForCompletion();
        piece.transform.position = targetPosition;

        StartCoroutine(ServiceLocator.GameReferences.GridManager.PlacePiece(piece));
        CommonOperations.RemovePieceFromPlayerSide(side, piece);
        NotifyEndOfTurnForOpponentServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyEndOfTurnForPlayerServerRpc()
    {
        ServiceLocator.TurnState.HasPlayerFinishedTheirStep = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyEndOfTurnForOpponentServerRpc()
    {
        ServiceLocator.TurnState.HasOpponentFinishedTheirStep = true;
    }

    private void SnapPiece (Collider2D hitCollider)
    {
        var nearestCenter = new Vector2(
            Mathf.Round(distanceFromClickedPieceToClickedPoint.x),
            Mathf.Round(distanceFromClickedPieceToClickedPoint.y));

        holdingPiece.transform.position = (Vector2)hitCollider.transform.position - nearestCenter;
    }

    private void FreeMovement (Vector2 mousePosition)
    {
        holdingPiece.transform.position = mousePosition - distanceFromClickedPieceToClickedPoint;
    }

    public void SetDraggable (Piece pieceToHold)
    {
        pieceToHold.UpdateLayer(ServiceLocator.GameState.CurrentTurn);
        CalculateOffsetFromObjectPivot(pieceToHold.transform);

        holdingPiece = pieceToHold;
        OnSelectPiece?.Invoke(pieceToHold);
    }

    public void ReleasePiece()
    {
        holdingPiece.UpdateLayer(ServiceLocator.GameState.CurrentTurn);
        OnPlaceFailPiece?.Invoke(holdingPiece);
        holdingPiece = null;
        lastCollider = null;
    }

    public void PlacePiece()
    {
        OnPlaceSuccessfullyPiece?.Invoke(holdingPiece);
        holdingPiece = null;
        lastCollider = null;
    }

    private void CalculateOffsetFromObjectPivot (Transform objectToHold)
    {
        var exactClickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        distanceFromClickedPieceToClickedPoint = exactClickPosition - objectToHold.position;
    }
}