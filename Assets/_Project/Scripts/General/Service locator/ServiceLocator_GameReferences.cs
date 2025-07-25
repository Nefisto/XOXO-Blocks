public static partial class ServiceLocator
{
    public static class GameReferences
    {
        public static GridManager GridManager { get; set; }
        public static GameManager GameManager { get; set; }
        public static TurnController TurnController { get; set; }
        public static PieceSpawnManager PieceSpawnManager { get; set; }
        public static LinePointManager LinePointManager { get; set; }

        public static PlayerHUD BottomPlayerHud { get; set; }
        public static PlayerHUD TopPlayerHud { get; set; }

        // Clean: Remove
        public static Player CrossPlayer { get; set; }
        public static Player CirclePlayer { get; set; }

        public static EmojiManager EmojiManager { get; set; }
    }
}