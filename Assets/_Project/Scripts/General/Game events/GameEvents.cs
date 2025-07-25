using System;

public static partial class GameEvents
{
    /// <summary>
    ///     A player has changed his Ready status
    /// </summary>
    public static EventHandler<bool> OnPlayerUpdateReadyStatus { get; set; }

    /// <summary>
    ///     Disable players ability to change their ready status
    /// </summary>
    public static EventHandler OnDisableChangeStatus { get; set; }

    public static class NetworkEvents
    {
        public static EventHandler LoggingAsGuest { get; set; }
    }
}