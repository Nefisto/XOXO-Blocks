using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NTools;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

public class GridManager : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform grid3Folder;

    [SerializeField]
    private Transform grid4Folder;

    private ScoreBehavior scoreBehavior;
    public Dictionary<Vector2Int, GridTile> GridTiles { get; private set; } = new();

    public GridTile this [int i, int j]
    {
        get => GridTiles[new Vector2Int(i, j)];
        set => GridTiles[new Vector2Int(i, j)] = value;
    }

    public int GridSize { get; private set; }

    private void Awake()
    {
        ServiceLocator.GameReferences.GridManager = this;

        GameEvents.GameplayEvents.OnSetupNewGame += NewGameHandle;
    }

    private void OnDisable()
    {
        GameEvents.GameplayEvents.OnSetupNewGame -= NewGameHandle;
    }

    private IEnumerator NewGameHandle (object arg1, EventArgs arg2)
    {
        var ctx = arg2 as GameEvents.OnSetupNewGameEventArgs;
        Assert.IsNotNull(ctx);

        scoreBehavior = ctx.GameSettings.StickyBomb ? new StickyBombsScoreBehavior() : new CommonScoreBehavior();

        var gameSettings = ctx.GameSettings;
        var settings = new GridDrawer.GridSetting { size = gameSettings.BoardSize };
        StartCoroutine(GetComponent<GridDrawer>().CreateGrid(settings));

        GridSize = gameSettings.BoardSize;
        grid3Folder.gameObject.SetActive(false);
        grid4Folder.gameObject.SetActive(false);
        if (gameSettings.BoardSize == 3)
            grid3Folder.gameObject.SetActive(true);
        else
            grid4Folder.gameObject.SetActive(true);

        GridTiles = settings.positionToTile;
        yield break;
    }

    public List<Vector2> GetAllGridPositions()
    {
        var positions = new List<Vector2>();

        for (var x = 0; x < GridSize; x++)
        for (var y = 0; y < GridSize; y++)
            positions.Add(new Vector2(x, y));

        return positions;
    }

    public bool CanPlacePiece (Vector2 positionToPlace, List<Vector2> pieceConfiguration)
        => pieceConfiguration
            .Select(offset => positionToPlace + new Vector2(offset.y * -1f, offset.x))
            .All(IsWithinBounds);

    private bool IsWithinBounds (Vector2 position)
        => position.x >= 0 && position.x < GridSize && position.y >= 0 && position.y < GridSize;

    public IEnumerator PlacePiece (Piece piece)
    {
        yield return piece.Place().ToCoroutine();

        piece.DisableDetection();
        var scoreResult = scoreBehavior.HasScored(new ScoreContext());
        if (scoreResult.scoreData.Any())
        {
            if (scoreResult.HasCrossPlayerMadePoint)
            {
                foreach (var tile in CommonOperations.GetAllTopTilesOfKind(TileKind.Cross))
                    tile.ChangeTileKind(TileKind.Empty);

                ServiceLocator.GameState.PointsOfCross++;
                yield return GameEvents.GameplayEvents.OnPointScored?.YieldableInvoke(this,
                    new GameEvents.OnPointScoredEventArgs
                    {
                        PlayerSide = PlayerSide.Cross,
                        ScoreResult = scoreResult
                    });
            }

            if (scoreResult.HasCirclePlayerMadePoint)
            {
                foreach (var tile in CommonOperations.GetAllTopTilesOfKind(TileKind.Circle))
                    tile.ChangeTileKind(TileKind.Empty);

                ServiceLocator.GameState.PointsOfCircle++;
                yield return GameEvents.GameplayEvents.OnPointScored?.YieldableInvoke(this,
                    new GameEvents.OnPointScoredEventArgs
                    {
                        PlayerSide = PlayerSide.Circle,
                        ScoreResult = scoreResult
                    });
            }

            if (scoreResult.HasBombConnectedPoint)
            {
                foreach (var scoreData in scoreResult.scoreData)
                {
                    if (scoreData.Kind != TileKind.Bomb)
                        continue;

                    foreach (var gridTile in scoreData.TilesThatScored)
                        gridTile.Tile.ChangeTileKind(TileKind.Empty);
                }

                if (ServiceLocator.GameReferences.TurnController.CurrentPlayerSide == PlayerSide.Cross)
                {
                    ServiceLocator.GameState.PointsOfCross =
                        Mathf.Max(ServiceLocator.GameState.PointsOfCross - 1, 0);
                    yield return GameEvents.GameplayEvents.OnPointScored?.YieldableInvoke(this,
                        new GameEvents.OnPointScoredEventArgs
                        {
                            PlayerSide = PlayerSide.Cross,
                            ScoreResult = scoreResult
                        });
                }
                else
                {
                    ServiceLocator.GameState.PointsOfCircle =
                        Mathf.Max(ServiceLocator.GameState.PointsOfCircle - 1, 0);
                    yield return GameEvents.GameplayEvents.OnPointScored?.YieldableInvoke(this,
                        new GameEvents.OnPointScoredEventArgs
                        {
                            PlayerSide = PlayerSide.Circle,
                            ScoreResult = scoreResult
                        });
                }
            }
        }

        ServiceLocator.GameState.CurrentTurn++;
    }

    public Vector2 GetWorldPositionOfVector (Vector2 targetPosition)
        => GridTiles[targetPosition.ToVector2Int()].transform.position;
}