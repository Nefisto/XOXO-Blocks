public static partial class Extensions
{
    public static PlayerSide GetInverseSide (this PlayerSide side)
        => side == PlayerSide.Circle ? PlayerSide.Cross : PlayerSide.Circle;
}