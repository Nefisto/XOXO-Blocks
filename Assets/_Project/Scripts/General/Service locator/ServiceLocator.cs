public static partial class ServiceLocator
{
    public static ApplicationController ApplicationController { get; set; }

    public static GameState GameState { get; set; } = new();

    public static GameSettings GameSettings { get; set; } = new();

    public static DraggingController DraggingController { get; set; }
    public static PlayerSide PlayerSide { get; set; }

    public static FadingManager FadingManager { get; set; }

    public static TurnState TurnState { get; set; }
}

public enum TurnFinishedReason
{
    TimedOut = 0,
    PiecePlaced = 1
}

public enum TurnPhase
{
    DrawPhase,
    SelectPhase,
    AnimatingPhase,
    DiscardPhase
}

public class TurnState
{
    public bool HasFinishedSelectPhase { get; set; }
    public bool HasFinishedDiscardPhase { get; set; }

    public TurnPhase TurnPhase { get; set; }
    public TurnFinishedReason FinishReason { get; set; }

    // Even when not playing, the opponent need to do some operations
    public bool HasPlayerFinishedTheirStep { get; set; }
    public bool HasOpponentFinishedTheirStep { get; set; }
}

public class GameSettings
{
    public int BoardSize { get; set; }
    public bool StickyBomb { get; set; }
    public bool DraftDraw { get; set; }
    public int HandSize { get; set; }
}