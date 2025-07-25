using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class EmojiDatabase : ScriptableObject
{
    [field: TitleGroup("Settings")]
    [field: SerializeField]
    public List<EmojiData> AllEmojis { get; set; }
}