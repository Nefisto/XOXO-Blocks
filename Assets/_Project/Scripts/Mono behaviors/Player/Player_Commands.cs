#if UNITY_EDITOR
using QFSW.QC;

public partial class Player
{
    [Command("Player.RequestNewPieceFromBag", MonoTargetType.All)]
    private void Command_RequestNewPieceFromBag() => GetNewPiece(true);
}
#endif