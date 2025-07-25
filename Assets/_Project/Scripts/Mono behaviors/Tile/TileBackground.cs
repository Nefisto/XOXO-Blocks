using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class TileBackground : MonoBehaviour
{
    [TitleGroup("References")]
    [SerializeField]
    private Animator animator;

    [field: TitleGroup("References")]
    [field: SerializeField]
    public SpriteRenderer BackgroundRenderer { get; set; }

    private bool hasPopped;

    private void NotifyFinishPopAnimation() => hasPopped = true;

    public async UniTask Place()
    {
        animator.SetTrigger("Pop");
        await UniTask.WaitUntil(() => hasPopped);
    }

    public void SetupState (TileStateSettings settings)
    {
        BackgroundRenderer.DOKill(true);
        BackgroundRenderer
            .DOColor(settings.backgroundColor, settings.colorTransitionDuration)
            .OnComplete(() => BackgroundRenderer.color = settings.backgroundColor);
    }
}