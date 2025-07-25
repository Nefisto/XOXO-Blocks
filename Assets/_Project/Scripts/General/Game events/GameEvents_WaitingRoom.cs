using System;
using NTools;

public static partial class GameEvents
{
    public static class WaitingRoom
    {
        public static EntryPoint OnClosingWaitingRoom { get; set; } = new();
        public static EntryPoint OnClosedWaitingRoom { get; set; } = new();
        public static EntryPoint OnFinishedMovingUpAnimation { get; set; } = new();
        public static EventHandler UpdatePlayerNicks { get; set; }

        public class UpdatePlayerNicksEventArgs : EventArgs
        {
            public string circlePlayerNickname;
            public string crossPlayerNickname;
        }
    }
}