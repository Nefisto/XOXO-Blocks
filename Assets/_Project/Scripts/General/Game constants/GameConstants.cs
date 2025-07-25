public static class GameConstants
{
    public const string LOBBY_SCENE_NAME = "Lobby new";

    public const string NICKNAME_PLAYERPREF_KEY = "nickname";

    public static class LobbyConstants
    {
        public const int NICKNAME_MIN_LENGTH = 3;
        public const int NICKNAME_MAX_LENGTH = 8;
    }

    public static class GameplayCostants
    {
#if UNITY_EDITOR
        public const float TURN_TIMER = 300f;
#else
        public const float TURN_TIMER = 30f;
#endif
    }

    public static class NetworkConstants
    {
        public const float GAP_BETWEEN_FETCH_LOBBIES = 5f;
        public const string CONNECTION_TYPE = "wss";

        /// <summary>
        ///     Info that we public share when creating the lobby
        /// </summary>
        public static class LobbyConstants
        {
            public const string GAME_VERSION = "GameVersion";
            public const string JOIN_CODE = "JoinCode";
            public const string GRID_SIZE = "GridSize";
            public const string STICKY_BOMB_MODE = "StickyBomb";
            public const string DRAFT_DRAW_MODE = "DraftDraw";
        }
    }
}