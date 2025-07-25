using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class GameButton : MonoBehaviour
{
    [TitleGroup("Settings")]
    [SerializeField]
    private Color32 disabledColor;

    [TitleGroup("References")]
    [SerializeField]
    private Button button;

    [SerializeField]
    private Image image;

    [TitleGroup("Debug")]
    [ReadOnly]
    [ShowInInspector]
    private Color32 originalColor;

    // ReSharper disable once InconsistentNaming
    public Button.ButtonClickedEvent onClick
    {
        get => button.onClick;
        set => button.onClick = value;
    }

    [Button]
    [DisableInEditorMode]
    public void EnableButton()
    {
        button.interactable = true;
        image.color = Color.white;
    }

    [Button]
    [DisableInEditorMode]
    public void DisableButton()
    {
        button.interactable = false;
        image.color = disabledColor;
    }

    public async void FadOut()
    {
        DisableButton();

        await UniTask.WhenAll(GetComponentsInChildren<Graphic>()
            .Select(g => g.DOFade(0f, 1.5f)
                .AsyncWaitForCompletion()
                .AsUniTask()));
    }
}