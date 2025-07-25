using System;
using System.Linq;
using NTools;
using QFSW.QC;
using Sirenix.OdinInspector;
using UnityEngine;

public class CustomCommands : MonoBehaviour
{
    [TitleGroup("References")]
    [SerializeField]
    private Piece piecePrefab;

    [SerializeField]
    private PieceDatabase pieceDatabase;

    private void Awake()
    {
        AddCommands();
    }

    private void AddCommands()
    {
        foreach (var pieceData in pieceDatabase.AllPieces)
        {
            var pieceName = pieceData.name;
            var commandName = pieceData
                .name
                .Replace(" ", "_");
            var command = new LambdaCommandData(new Action(() => SpawnAllVersionsOfPiece(pieceName, PlayerSide.Cross)),
                $"Spawn_{commandName}");
            if (!QuantumConsoleProcessor.TryAddCommand(command))
                Debug.Log($"Failed to add command for {pieceName}");
        }
    }

    private void SpawnAllVersionsOfPiece (string pieceName, PlayerSide playerSide)
    {
        var pieceData = pieceDatabase.AllPieces.FirstOrDefault(p => p.name == pieceName);
        var folder = GameObject.Find("Debug_folder") ?? new GameObject("Debug_folder");
        folder.transform.DestroyChildren();

        foreach (var (pieceCode, i) in pieceData.GetAllPieceCodes().Select((c, i) => (c, i)))
        {
            var currentPosition = new Vector3(i * 5f, -10f, 0f);
            var instance = Instantiate(piecePrefab, currentPosition, Quaternion.identity, folder.transform);

            var pieceConfiguration = pieceData.GetPieceConfiguration(pieceCode);
            pieceConfiguration.playerSide = playerSide;

            instance.Setup(pieceConfiguration);
        }
    }
}