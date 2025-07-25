using System.Collections.Generic;
using UnityEngine;

public class PieceConfiguration
{
    public int amountOfRotations;
    public bool isMirrored;
    public string pieceCode;
    public int pieceHeight;
    public int pieceWidth;

    public PlayerSide playerSide;

    public Dictionary<Vector2, TileSide> tiles = new();
}