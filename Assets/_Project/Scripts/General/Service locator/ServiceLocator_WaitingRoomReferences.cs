public static partial class ServiceLocator
{
    public static class WaitingRoomReferences
    {
        public static WaitingForOtherPlayer WaitingForOtherScreen { get; set; }
        public static WaitingRoomPlayerEntry BottomWaitingRoomPlayerEntry { get; set; }
        public static WaitingRoomPlayerEntry TopWaitingRoomPlayerEntry { get; set; }
    }
}