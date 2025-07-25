using Sirenix.OdinInspector;
using UnityEngine;
using Physics2D = UnityEngine.Physics2D;

public class TilePlacementValidator : MonoBehaviour
{
    [Button]
    [DisableInEditorMode]
    public bool CanBePlaced()
    {
        var gridTile = Physics2D.OverlapCircle(transform.position, .25f, LayerMask.GetMask("Grid Tile"));

        return gridTile != null;
    }
}