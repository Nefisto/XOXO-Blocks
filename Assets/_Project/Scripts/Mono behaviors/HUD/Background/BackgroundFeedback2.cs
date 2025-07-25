using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class BackgroundFeedback2 : MonoBehaviour
{
    [TitleGroup("Settings")]
    [SerializeField]
    private float colorTransitionDuration = 1.5f;

    [TitleGroup("References")]
    [SerializeField]
    private Image crossImage;

    [SerializeField]
    private Image circleImage;

    [SerializeField]
    private Color crossColor;

    [SerializeField]
    private Color circleColor;

    [SerializeField]
    private Color neutralColor;

    private bool isRunning;

    private void Awake()
    {
        GameEvents.GameplayEvents.OnStartingNewTurn += NewTurnHandle;
    }

    private void OnDestroy()
    {
        GameEvents.GameplayEvents.OnStartingNewTurn -= NewTurnHandle;
    }

    private IEnumerator NewTurnHandle (object arg1, EventArgs arg2)
    {
        var ctx = arg2 as GameEvents.OnStartNewTurnEventArgs;

        Assert.IsNotNull(ctx);

        ChangeTurnTo(ctx.PlayerSide);
        yield return null;
    }


    [Button]
    [DisableInEditorMode]
    private async void ChangeTurnTo (PlayerSide side)
    {
        if (isRunning)
            return;

        isRunning = true;
        var currentPlayer = (
            side == PlayerSide.Cross ? crossImage : circleImage,
            side == PlayerSide.Cross ? crossColor : circleColor);
        var waitingPlayer = side.GetInverseSide() == PlayerSide.Cross ? crossImage : circleImage;
        var playingTask = DOTween
            .Sequence()
            .Append(currentPlayer.Item1.DOColor(currentPlayer.Item2, colorTransitionDuration))
            .AsyncWaitForCompletion()
            .AsUniTask();
        var waitingTask = DOTween
            .Sequence()
            .Append(waitingPlayer.DOColor(neutralColor, colorTransitionDuration))
            .AsyncWaitForCompletion()
            .AsUniTask();

        await UniTask.WhenAll(playingTask, waitingTask);
        isRunning = false;
    }
}