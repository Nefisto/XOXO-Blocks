using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "PieceDatabase", menuName = "Scriptable Objects/PieceDatabase")]
public class PieceDatabase : ScriptableObject
{
    [field: TitleGroup("Settings")]
    [field: SerializeField]
    public List<PieceData> AllPieces { get; set; }
}