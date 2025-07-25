using UnityEngine;

public class GridTile : MonoBehaviour
{
    public Tile Tile { get; private set; }

    public Tile PeekTile() => Tile;

    public bool HasTile() => Tile != null;

    public void SetTile (Tile tile) => Tile = tile;
}