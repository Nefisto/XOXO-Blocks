using System.Collections;
using DG.Tweening;
using NTools;
using Sirenix.OdinInspector;
using UnityEngine;

public class TileNotification : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private float blinkDuration = 1f;

    [Header("References")]
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private Sprite impossibleSprite;

    private NTask notificationRoutine;

    [Button]
    [DisableInEditorMode]
    public void CancelNotification()
    {
        spriteRenderer.gameObject.SetActive(false);
        notificationRoutine?.Stop();
    }

    [Button]
    [DisableInEditorMode]
    public void ShowNotification()
    {
        if (notificationRoutine != null)
            CancelNotification();

        spriteRenderer.gameObject.SetActive(true);
        notificationRoutine = new NTask(NotifyRoutine());
    }

    private IEnumerator NotifyRoutine()
    {
        spriteRenderer.sprite = impossibleSprite;
        while (true)
            yield return DOTween
                .Sequence()
                .Append(spriteRenderer.DOFade(0, blinkDuration * 0.5f))
                .Append(spriteRenderer.DOFade(1, blinkDuration * 0.5f))
                .WaitForCompletion();
    }
}