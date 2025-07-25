using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class JoinByCodeAnimation : MonoBehaviour
{
    [TitleGroup("References")]
    [SerializeField]
    private RectTransform requestFolder;

    [SerializeField]
    private RectTransform codeFolder;

    private bool isRunning;

    [Button]
    [DisableInEditorMode]
    private async void MoveToCodeAnimation()
    {
        if (isRunning)
            return;

        requestFolder.localScale = Vector3.one;
        codeFolder.localScale = Vector3.zero;

        isRunning = true;
        await DOTween
            .Sequence()
            .Append(requestFolder.DOScale(Vector3.zero, 1f))
            .Append(codeFolder.DOScale(Vector3.one, 1f))
            .AsyncWaitForCompletion();
        // Activate buttons
        isRunning = false;
    }

    [Button]
    [DisableInEditorMode]
    private async void CancelAnimation()
    {
        if (isRunning)
            return;

        requestFolder.localScale = Vector3.zero;
        codeFolder.localScale = Vector3.one;

        isRunning = true;
        await DOTween
            .Sequence()
            .Append(codeFolder.DOScale(Vector3.zero, 1f))
            .Append(requestFolder.DOScale(Vector3.one, 1f))
            .AsyncWaitForCompletion();
        // Activate buttons
        isRunning = false;
    }
}