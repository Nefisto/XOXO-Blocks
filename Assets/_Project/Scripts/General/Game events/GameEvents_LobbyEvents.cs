using System;

public static partial class GameEvents
{
    public static class LobbyEvents
    {
        public static EventHandler LoadedLobbyScene { get; set; }
        public static EventHandler OnTryingMoveToGame { get; set; }
        public static EventHandler OnFailedToJoinGame { get; set; }
        public static EventHandler OpenChangeNickScreen { get; set; }
        public static EventHandler CloseChangeNickScreen { get; set; }
    }

    public class OpenChangeNickScreenEventArgs : EventArgs
    {
        public bool isFirstTime;
        public string title = "Select a new nick!";
    }
}