using System;
using NTools;

public static partial class GameEvents
{
    public static class GameplayEvents
    {
        public static EntryPoint OnSetupReferencesNewGame { get; set; } = new();

        public static EntryPoint OnSceneSetup { get; set; } = new();

        public static EntryPoint OnSetupNewGame { get; set; } = new();
        public static EntryPoint OnPointScored { get; set; } = new();
        public static Action<Piece> OnPlacePiece { get; set; }
        public static Action OnFinishPlacementAnimation { get; set; }

        public static EntryPoint OnStartingNewGame { get; set; } = new();

        public static EntryPoint NewTurnSetup { get; set; } = new();

        public static EntryPoint OnStartingNewTurn { get; set; } = new();

        public static EntryPoint BeginningDrawPhase { get; set; } = new();

        // End game passing the winner side
        public static Action<PlayerSide> OnGameEnd { get; set; }

        public static EntryPoint OnClientDisconnected { get; set; } = new();

        public static EntryPoint AllPlayersConnected { get; set; } = new();
    }

    public class OnSetupNewGameEventArgs : EventArgs
    {
        public GameSettings GameSettings { get; set; }
    }

    public class OnPointScoredEventArgs : EventArgs
    {
        public PlayerSide PlayerSide { get; set; }
        public ScoreResult ScoreResult { get; set; }
    }

    public class OnStartNewTurnEventArgs : EventArgs
    {
        public PlayerSide PlayerSide { get; set; }
    }
}