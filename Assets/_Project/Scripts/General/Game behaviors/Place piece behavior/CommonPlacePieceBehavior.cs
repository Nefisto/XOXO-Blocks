using Cysharp.Threading.Tasks;

public class CommonPlacePieceBehavior : PlacePieceBehavior
{
    public override async UniTask PlacePiece (PlacePieceBehaviorContext context)
    {
        var gridTile = context.gridTile;
        var tile = context.tile;

        if (gridTile.HasTile())
            gridTile.PeekTile().ChangeTileKind(TileKind.Empty);

        gridTile.SetTile(tile);

        if (tile.Kind == TileKind.Bomb)
            tile.ChangeTileKind(TileKind.Empty);

        await tile.Place();
    }
}