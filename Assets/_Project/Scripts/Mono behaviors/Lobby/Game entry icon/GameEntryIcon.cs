using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class GameEntryIcon : MonoBehaviour
{
    [TitleGroup("Settings")]
    [SerializeField]
    private Sprite enabledBackground;

    [SerializeField]
    private Sprite disabledBackground;

    [TitleGroup("References")]
    [SerializeField]
    private Image background;

    [SerializeField]
    private Image icon;

    [Button]
    [DisableInEditorMode]
    public void EnableEntry()
    {
        background.sprite = enabledBackground;
        icon.color = Color.white;
    }

    [Button]
    [DisableInEditorMode]
    public void DisableEntry()
    {
        background.sprite = disabledBackground;
        icon.color = Color.gray;
    }
}