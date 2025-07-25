using Cysharp.Threading.Tasks;

public class BombStickyBehavior : PlacePieceBehavior
{
    public override async UniTask PlacePiece (PlacePieceBehaviorContext context)
    {
        var gridTile = context.gridTile;
        var tile = context.tile;

        await tile.Place();
        if (gridTile.HasTile())
        {
            if (ShouldBlowTile(gridTile.Tile, tile))
            {
                gridTile.Tile.ChangeTileKind(TileKind.Empty);
                tile.ChangeTileKind(TileKind.Empty);
                return;
            }

            gridTile.PeekTile().ChangeTileKind(TileKind.Empty);
        }

        gridTile.SetTile(tile);
    }

    private static bool ShouldBlowTile (Tile gridTile, Tile tile)
        => gridTile.Kind == TileKind.Bomb
           || (gridTile.Kind != TileKind.Empty
               && tile.Kind == TileKind.Bomb);
}