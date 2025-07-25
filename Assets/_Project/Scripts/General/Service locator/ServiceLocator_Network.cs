public static partial class ServiceLocator
{
    public static class Network
    {
        public static ClientManager ClientManager { get; set; }
        public static HostManager HostManager { get; set; }
        public static ServerManager ServerManager { get; set; }
        public static LobbyInfoHUD LobbyInfoHud { get; set; }
    }
}