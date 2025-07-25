public class StickyBombsScoreBehavior : ScoreBehavior
{
    public override ScoreResult HasScored (ScoreContext context)
    {
        var result = base.HasScored(context);

        var grid = ServiceLocator.GameReferences.GridManager;
        var lineScoreData = CheckLines(grid, TileKind.Bomb);
        var columnScoreData = CheckColumns(grid, TileKind.Bomb);
        var diagonalsScoreData = CheckDiagonals(grid, TileKind.Bomb);

        result.AddScore(lineScoreData);
        result.AddScore(columnScoreData);
        result.AddScore(diagonalsScoreData);

        return result;
    }
}