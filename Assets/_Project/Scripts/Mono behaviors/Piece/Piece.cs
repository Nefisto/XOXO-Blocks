using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using QFSW.QC;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public class PieceStateSettings
{
    public Color backgroundColor;
    public float backgroundColorTransitionDuration = .5f;
    public TileStateSettings tileSettings;
}

[SelectionBase]
public class Piece : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private PieceStateSettings activeStateSettings;

    [SerializeField]
    private PieceStateSettings waitingStateSettings;

    [field: Header("References")]
    [field: SerializeField]
    public PiecePlacementValidator PlacementValidator { get; private set; }

    [field: SerializeField]
    public Tile TilePrefab { get; private set; }

    [field: SerializeField]
    public SpriteRenderer ContourSprite { get; private set; }

    [Header("Debug")]
    [SerializeField]
    private List<Tile> tiles;

    private EventTrigger eventTrigger;

    public BoxCollider2D PieceCollider { get; private set; }

    public Vector2 PieceSize => PieceCollider.size * transform.localScale.x;
    public Vector2 PieceColliderOffset => PieceCollider.offset * transform.localScale.x;

    public EventHandler OnBeingPlaced { get; set; }

    public bool CanBeDragged { get; set; }

    private void Awake()
    {
        PieceCollider = GetComponent<BoxCollider2D>();
        PlacementValidator = GetComponent<PiecePlacementValidator>();
        eventTrigger = GetComponent<EventTrigger>();
    }

    [Command("Piece.GetAllTilesPosition", MonoTargetType.Argument)]
    public List<Vector2> GetAllTileOffsets()
        => tiles
            .Select(x => (Vector2)x.transform.localPosition)
            .ToList();

    public void Setup (PieceConfiguration configuration)
    {
        gameObject.name = configuration.pieceCode;
        var (width, height) = (configuration.pieceWidth, configuration.pieceHeight);

        var initialPosition = new Vector2(width * -.5f, height * -.5f) // Half of the piece size 
                              + new Vector2(.5f, .5f); // half of the tile size

        tiles = new List<Tile>();
        var offSet = new Vector2(width.IsEven() ? -.5f : 0f, height.IsEven() ? -.5f : 0f);
        foreach (var (pos, side) in configuration.tiles)
        {
            if (side == TileSide.Empty)
                continue;

            var correctPosition = initialPosition + pos + offSet;
            var correctTileKind = side switch
            {
                TileSide.Bomb => TileKind.Bomb,
                TileSide.Player => configuration.playerSide == PlayerSide.Cross ? TileKind.Cross : TileKind.Circle,
                TileSide.Opponent => configuration.playerSide.GetInverseSide() == PlayerSide.Circle
                    ? TileKind.Circle
                    : TileKind.Cross,
                _ => throw new ArgumentOutOfRangeException()
            };

            var instance = Instantiate(TilePrefab, transform);
            instance.transform.localPosition = correctPosition;

            tiles.Add(instance);
            instance.Setup(correctTileKind);
        }

        SetupDetectionSize(width, height);
        SetupContourPosition(width, height);

        SetActiveState().Forget();
    }

    public async UniTask SetActiveState() => await SetupState(activeStateSettings);
    public async UniTask SetWaitingState() => await SetupState(waitingStateSettings);

    private async UniTask SetupState (PieceStateSettings stateSetting)
    {
        ContourSprite.DOKill(true);
        ContourSprite
            .DOColor(stateSetting.backgroundColor, stateSetting.backgroundColorTransitionDuration)
            .OnComplete(() => ContourSprite.color = stateSetting.backgroundColor);

        foreach (var tile in tiles)
            await tile.SetupState(stateSetting.tileSettings);
    }

    private void SetupContourPosition (int width, int height)
    {
        ContourSprite.transform.localPosition = new Vector2(width.IsEven() ? -.5f : 0f, height.IsEven() ? -.5f : 0f);
        ContourSprite.transform.localScale = new Vector2(width, height);
    }

    public void UpdateLayer (int gameStateCurrentTurn)
    {
        foreach (var tile in tiles)
            tile.UpdateLayer(gameStateCurrentTurn);
    }

    public bool CanBePlaced()
    {
        return InternalGetAllOverlapGridTile()
            .All(gt => gt != null);
    }

    public List<GridTile> GetAllValidOverlappedGridTile()
    {
        return InternalGetAllOverlapGridTile()
            .Where(gt => gt != null && gt.HasTile())
            .ToList();
    }

    private IEnumerable<GridTile> InternalGetAllOverlapGridTile()
    {
        return tiles
            .Select(t => t.GetGridTileBelow());
    }

    public async UniTask Place()
    {
        PlacePieceBehavior placeBehavior = ServiceLocator.GameSettings.StickyBomb
            ? new BombStickyBehavior()
            : new CommonPlacePieceBehavior();

        var placeTasks = new List<UniTask>();
        foreach (var tile in tiles)
        {
            var gridTile = tile.GetGridTileBelow();
            var placeTask = placeBehavior.PlacePiece(new PlacePieceBehaviorContext
            {
                tile = tile,
                gridTile = gridTile
            });

            placeTasks.Add(placeTask);
        }

        await UniTask.WhenAll(placeTasks);
    }

    public void DisableDetection() => PieceCollider.enabled = false;

    public void ScaleAnimation (float endValue, float duration)
    {
        DOTween.Kill(transform);
        transform.DOScale(endValue, duration);
    }

    public void FadeContourAnimation (float endValue, float duration)
    {
        DOTween.Kill(ContourSprite);
        ContourSprite.DOFade(endValue, duration);
    }

    private void SetupDetectionSize (int width, int height)
    {
        PieceCollider.offset = new Vector2(width.IsEven() ? -.5f : 0f, height.IsEven() ? -.5f : 0f);
        PieceCollider.size = new Vector2(width, height);
    }
}