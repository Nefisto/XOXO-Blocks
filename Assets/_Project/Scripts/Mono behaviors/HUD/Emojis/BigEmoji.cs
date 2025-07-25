using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class BigEmoji : MonoBehaviour
{
    [TitleGroup("Animation")]
    [SerializeField]
    private float growingDuration = 0.75f;

    [SerializeField]
    private float rotateRange = 35f;

    [SerializeField]
    private float rotateDuration = 1f;

    [SerializeField]
    private bool isOnTop;

    [TitleGroup("References")]
    [SerializeField]
    private Image emojiIcon;

    [TitleGroup("References")]
    [SerializeField]
    private RectTransform root;

    [TitleGroup("References")]
    [SerializeField]
    private EmojiDatabase emojiDatabase;

    private bool isRunning;

    private void Awake() => root.gameObject.SetActive(false);

    [Button]
    [DisableInEditorMode]
    public async void Animate (EmojisKind kind)
    {
        if (isRunning)
            return;

        isRunning = true;

        root.gameObject.SetActive(true);

        emojiIcon.sprite = emojiDatabase
            .AllEmojis
            .First(e => e.Kind == kind)
            .Icon;

        var correctGrownMethod = isOnTop
            ? root.DOAnchorMin(new Vector2(0f, 0f), growingDuration)
            : root.DOAnchorMax(new Vector2(1f, 1f), growingDuration);
        var correctShrinkMethod = isOnTop
            ? root.DOAnchorMin(new Vector2(1, 1), growingDuration)
            : root.DOAnchorMax(new Vector2(0f, 0f), growingDuration);

        if (isOnTop)
            root.anchorMin = new Vector2(1f, 1f);
        else
            root.anchorMax = new Vector2(0f, 0f);

        await DOTween
            .Sequence()
            .Append(correctGrownMethod)
            .Append(emojiIcon.transform.DORotate(new Vector3(0f, 0f, -rotateRange), rotateDuration * .5f))
            .Append(emojiIcon.transform.DORotate(new Vector3(0f, 0f, rotateRange), rotateDuration)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.Linear))
            .Append(emojiIcon.transform.DORotate(new Vector3(0f, 0f, rotateRange), rotateDuration)
                .SetEase(Ease.Linear))
            .Append(emojiIcon.transform.DORotate(new Vector3(0f, 0f, 0), rotateDuration * .5f))
            .Append(correctShrinkMethod)
            .AsyncWaitForCompletion();

        root.gameObject.SetActive(false);
        isRunning = false;
    }
}