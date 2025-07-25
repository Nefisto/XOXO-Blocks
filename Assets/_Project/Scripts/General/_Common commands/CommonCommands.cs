using QFSW.QC;
using Unity.Netcode;

public static class CommonCommands
{
    [Command]
    private static void Shutdown() => NetworkManager.Singleton.Shutdown();
}