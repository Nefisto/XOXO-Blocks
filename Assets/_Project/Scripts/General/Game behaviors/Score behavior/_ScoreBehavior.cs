using System.Collections.Generic;
using System.Linq;

public abstract class ScoreBehavior
{
    public virtual ScoreResult HasScored (ScoreContext context)
    {
        HasScored(out var result);
        return result;
    }

    private static bool HasScored (out ScoreResult result)
    {
        var grid = ServiceLocator.GameReferences.GridManager;

        var lineScoreData = CheckLines(grid, TileKind.Circle, TileKind.Cross);
        var columnScoreData = CheckColumns(grid, TileKind.Circle, TileKind.Cross);
        var diagonalsScoreData = CheckDiagonals(grid, TileKind.Circle, TileKind.Cross);

        result = new ScoreResult();
        result.AddScore(lineScoreData);
        result.AddScore(columnScoreData);
        result.AddScore(diagonalsScoreData);

        return result.scoreData.Any();
    }

    protected static List<ScoreResult.ScoreData> CheckLines (GridManager grid, params TileKind[] validTiles)
    {
        var result = new List<ScoreResult.ScoreData>();
        var size = grid.GridSize;
        const string pointKeyTemplate = "h{0}";
        for (var i = 0; i < size; i++)
        {
            var firstSymbol = grid[i, 0].PeekTile()?.Kind;
            if (!firstSymbol.HasValue || !validTiles.Contains(firstSymbol.Value))
                continue;

            var tempList = new List<GridTile>();
            var completed = true;

            for (var j = 0; j < size && completed; j++)
                if (grid[i, j].PeekTile()?.Kind != firstSymbol)
                    completed = false;
                else
                    tempList.Add(grid[i, j]);

            if (completed)
                result.Add(new ScoreResult.ScoreData
                {
                    Kind = firstSymbol.Value,
                    scoreKey = string.Format(pointKeyTemplate, i),
                    debugMessage = $"Line {i}",
                    TilesThatScored = tempList.ToList()
                });
        }

        return result;
    }

    protected static List<ScoreResult.ScoreData> CheckColumns (GridManager grid, params TileKind[] validTiles)
    {
        var result = new List<ScoreResult.ScoreData>();
        var size = grid.GridSize;
        const string pointKeyTemplate = "v{0}";
        for (var i = 0; i < size; i++)
        {
            var firstSymbol = grid[0, i].PeekTile()?.Kind;
            if (!firstSymbol.HasValue || !validTiles.Contains(firstSymbol.Value))
                continue;

            var tempList = new List<GridTile>();
            var completed = true;

            for (var j = 0; j < size && completed; j++)
                if (grid[j, i].PeekTile()?.Kind != firstSymbol)
                    completed = false;
                else
                    tempList.Add(grid[j, i]);

            if (completed)
                result.Add(new ScoreResult.ScoreData
                {
                    Kind = firstSymbol.Value,
                    scoreKey = string.Format(pointKeyTemplate, i),
                    debugMessage = $"Column {i}",
                    TilesThatScored = tempList.ToList()
                });
        }

        return result;
    }

    protected static List<ScoreResult.ScoreData> CheckDiagonals (GridManager grid, params TileKind[] validTiles)
    {
        var result = new List<ScoreResult.ScoreData>();

        var firstSymbolOnMainDiagonal = grid[0, 0].PeekTile()?.Kind;
        var tempList = new List<GridTile>();
        const string pointKeyTemplate = "d{0}";
        var size = grid.GridSize;
        if (firstSymbolOnMainDiagonal != null)
            for (var i = 0; i < size; i++)
            {
                if (!validTiles.Contains(firstSymbolOnMainDiagonal.Value))
                    break;

                if (grid[i, i].PeekTile()?.Kind != firstSymbolOnMainDiagonal)
                    break;

                tempList.Add(grid[i, i]);
                if (i < size - 1)
                    continue;

                result.Add(new ScoreResult.ScoreData
                {
                    Kind = firstSymbolOnMainDiagonal.Value,
                    scoreKey = string.Format(pointKeyTemplate, "m"),
                    debugMessage = "Main diagonal",
                    TilesThatScored = tempList.ToList()
                });
            }

        tempList.Clear();
        var firstSymbolOnSecondaryDiagonal = grid[size - 1, 0].PeekTile()?.Kind;
        if (firstSymbolOnSecondaryDiagonal == null)
            return result;

        for (int i = 0, j = size - 1; i < size; i++, j--)
        {
            if (!validTiles.Contains(firstSymbolOnSecondaryDiagonal.Value))
                break;

            if (grid[i, j].PeekTile()?.Kind != firstSymbolOnSecondaryDiagonal)
                break;

            tempList.Add(grid[i, j]);
            if (i < size - 1)
                continue;

            result.Add(new ScoreResult.ScoreData
            {
                Kind = firstSymbolOnSecondaryDiagonal.Value,
                scoreKey = string.Format(pointKeyTemplate, "s"),
                debugMessage = "Sub diagonal",
                TilesThatScored = tempList.ToList()
            });
        }

        return result;
    }
}

public class ScoreContext { }