using System.Collections.Generic;
using System.Linq;

public class ScoreResult
{
    public List<ScoreData> scoreData = new();

    public bool HasCrossPlayerMadePoint => scoreData.Any(d => d.Kind == TileKind.Cross);
    public bool HasCirclePlayerMadePoint => scoreData.Any(d => d.Kind == TileKind.Circle);
    public bool HasBombConnectedPoint => scoreData.Any(d => d.Kind == TileKind.Bomb);

    public void AddScore (TileKind kind, string key, List<GridTile> tiles)
    {
        scoreData.Add(new ScoreData
        {
            Kind = kind,
            debugMessage = key,
            TilesThatScored = tiles,
            AmountOfPoints = kind != TileKind.Bomb ? 1 : -1
        });
    }

    public void AddScore (List<ScoreData> scoreData) => this.scoreData.AddRange(scoreData);

    public class ScoreData
    {
        public string debugMessage;
        public string scoreKey;
        public TileKind Kind { get; set; }
        public List<GridTile> TilesThatScored { get; set; }
        public int AmountOfPoints { get; set; } = 1;
    }
}