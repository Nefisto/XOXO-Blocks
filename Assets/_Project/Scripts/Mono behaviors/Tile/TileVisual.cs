using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class TileVisual : MonoBehaviour
{
    [Header("Settings")]
    [Range(0.2f, 2f)]
    [SerializeField]
    private float fadeDuration = 1f;

    [FormerlySerializedAs("spriteRenderer")]
    [Header("References")]
    [SerializeField]
    private SpriteRenderer iconRenderer;

    [SerializeField]
    private TileBackground tileBackground;

    [SerializeField]
    private Sprite cross;

    [SerializeField]
    private Sprite circle;

    [SerializeField]
    private Sprite blank;

    public IEnumerator UpdateSprite (TileKind kind)
    {
        yield return DOTween.Sequence()
            .Append(iconRenderer.DOFade(0, fadeDuration)
                .OnComplete(() => iconRenderer.sprite = GetSprite(kind)))
            .Append(iconRenderer.DOFade(1, fadeDuration))
            .WaitForCompletion();
    }

    public void InstantlyUpdateSprite (TileKind kind) => iconRenderer.sprite = GetSprite(kind);

    private Sprite GetSprite (TileKind kind)
        => kind switch
        {
            TileKind.Bomb => blank,
            TileKind.Cross => cross,
            TileKind.Circle => circle,
            TileKind.Empty => null,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async UniTask SetupState (TileStateSettings settings)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        tileBackground.SetupState(settings);

        iconRenderer.DOKill(true);
        iconRenderer
            .DOColor(settings.tileColor, settings.colorTransitionDuration)
            .OnComplete(() => iconRenderer.color = settings.tileColor);
    }

    public void UpdateLayer (int gameStateCurrentTurn)
    {
        iconRenderer.sortingOrder = gameStateCurrentTurn;
        tileBackground.BackgroundRenderer.sortingOrder = gameStateCurrentTurn + 1;
    }

    public void SendToBackLayer() => iconRenderer.color = Color.gray;
    public void SendToFrontLayer() => iconRenderer.color = Color.white;

    public async UniTask Place()
    {
        await tileBackground.Place();
    }
}