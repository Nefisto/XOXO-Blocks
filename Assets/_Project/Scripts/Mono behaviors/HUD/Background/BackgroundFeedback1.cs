using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class BackgroundFeedback1 : MonoBehaviour
{
    [TitleGroup("Settings")]
    [SerializeField]
    private Color crossColor;

    [SerializeField]
    private Color circleColor;

    [TitleGroup("References")]
    [SerializeField]
    private Image background;

    private bool running;

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

        ChangeToPlayer(ctx.PlayerSide);
        yield return null;
    }

    [Button]
    [DisableInEditorMode]
    public async void ChangeToPlayer (PlayerSide side)
    {
        if (running)
            return;

        running = true;
        var color = side == PlayerSide.Cross ? crossColor : circleColor;
        await DOTween
            .Sequence()
            .Append(background.DOColor(color, 0.5f))
            .AsyncWaitForCompletion();
        running = false;
    }
}