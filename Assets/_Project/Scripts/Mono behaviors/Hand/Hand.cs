using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Hand
{
    [field: SerializeField]
    public List<Piece> Pieces { get; set; } = new();

    public Action<Piece> OnAddPiece { get; set; }
    public Action<Piece> OnDetachPiece { get; set; }
    public Action<Piece> OnRemovePiece { get; set; }

    public DraggingController DraggingController { get; private set; }
    public bool IsFull => Pieces.Count >= ServiceLocator.GameSettings.HandSize;

    public void SetupAsMain (DraggingController draggingController)
    {
        DraggingController = draggingController;
    }

    public void AddPiece (Piece piece)
    {
        if (Pieces.Count >= ServiceLocator.GameSettings.HandSize)
        {
            Debug.Log("Trying to add more than max hand size");
            return;
        }

        Pieces.Add(piece);
        OnAddPiece?.Invoke(piece);
    }

    public void DetachPiece (Piece piece)
    {
        Pieces.Remove(piece);
        OnDetachPiece?.Invoke(piece);
    }

    public void RemovePiece (Piece piece)
    {
        Pieces.Remove(piece);
        OnRemovePiece?.Invoke(piece);
    }

    public void ClearHand (bool deletePieces = false)
    {
        foreach (var piece in Pieces.ToList())
            if (deletePieces)
                RemovePiece(piece);
            else
                DetachPiece(piece);
    }
}