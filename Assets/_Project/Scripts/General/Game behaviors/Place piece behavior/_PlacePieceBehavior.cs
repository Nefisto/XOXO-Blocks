using Cysharp.Threading.Tasks;

public abstract class PlacePieceBehavior
{
    public abstract UniTask PlacePiece (PlacePieceBehaviorContext context);
}

public class PlacePieceBehaviorContext
{
    public GridTile gridTile;
    public Tile tile;
}