using System;
using System.Collections.Generic;
using System.Linq;
using NTools;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class BagEntry
{
    [HorizontalGroup]
    public PieceData pieceData;

    [HorizontalGroup]
    public int amount = 1;
}

[Serializable]
public class RemainingPiece
{
    [HideInInspector]
    public PieceData pieceData;

    [HorizontalGroup]
    public string pieceCode;

    [HorizontalGroup]
    public int amount = 1;
}

public class Bag : ScriptableObject
{
    [Header("Settings")]
    [SerializeField]
    private PieceData defaultPieceData;

    [SerializeField]
    private List<BagEntry> possibleEntries;

    [field: SerializeField]
    public List<RemainingPiece> RemainingPieces { get; private set; }

    [TitleGroup("Debug")]
    [HideInEditorMode]
    [ShowInInspector]
    private List<PieceData> historyOfDrawPieces = new();

    public Bag GetInstance => Instantiate(this);

    public bool IsEmpty => RemainingPieces.All(be => be.amount == 0);

    public string GetPieceCode()
    {
        var canBeBomb = historyOfDrawPieces
                            .FirstOrDefault()
                            ?.IsBomb
                        != true;

        var selectedEntry = RandomizeNewPiece(canBeBomb);
        if (selectedEntry == null)
        {
            ResetAllRemainingPieces();
            selectedEntry = RandomizeNewPiece(canBeBomb);
        }

        Assert.IsNotNull(selectedEntry);

        selectedEntry.amount--;
        historyOfDrawPieces.Insert(0, selectedEntry.pieceData);
        return selectedEntry.pieceCode;
    }

    private RemainingPiece RandomizeNewPiece (bool canBeBomb)
        => RemainingPieces
            .Shuffle()
            .FirstOrDefault(be =>
            {
                var isValid = true;
                if (!canBeBomb)
                    isValid = !be.pieceData.IsBomb;

                return be.amount > 0 && isValid;
            });

    public void ResetAllRemainingPieces()
    {
        RemainingPieces.Clear();
        foreach (var entry in possibleEntries)
        {
            var allCodes = entry.pieceData.GetAllPieceCodes();
            var piece = entry.pieceData;

            RemainingPieces.AddRange(allCodes.Select(code => new RemainingPiece
            {
                pieceData = piece,
                amount = 1,
                pieceCode = code
            }));
        }
    }
}