using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class EmojiData : ScriptableObject
{
    [field: TitleGroup("Settings")]
    [field: PreviewField]
    [field: SerializeField]
    public Sprite Icon { get; set; }

    [field: TitleGroup("Settings")]
    [field: SerializeField]
    public EmojisKind Kind { get; set; }
}