using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class BigIconManager : MonoBehaviour
{
    [FormerlySerializedAs("crossIcon")]
    [TitleGroup("References")]
    [SerializeField]
    private BigIcon bottomIcon;

    [FormerlySerializedAs("circleIcon")]
    [TitleGroup("References")]
    [SerializeField]
    private BigIcon topIcon;

    private readonly Dictionary<PlayerSide, BigIcon> playerSideToIcon = new();

    private void Awake()
    {
        GameEvents.GameplayEvents.OnStartingNewGame += NewGameHandle;
        GameEvents.GameplayEvents.OnStartingNewTurn += NewTurnHandle;
        GameEvents.GameplayEvents.OnSceneSetup += SceneSetupHandle;
    }

    private void OnDestroy()
    {
        GameEvents.GameplayEvents.OnStartingNewGame -= NewGameHandle;
        GameEvents.GameplayEvents.OnStartingNewTurn -= NewTurnHandle;
        GameEvents.GameplayEvents.OnSceneSetup -= SceneSetupHandle;
    }

    private IEnumerator NewTurnHandle (object arg1, EventArgs arg2)
    {
        var ctx = arg2 as GameEvents.OnStartNewTurnEventArgs;

        foreach (var (playerSide, bigIcon) in playerSideToIcon)
            if (playerSide == ctx.PlayerSide)
                bigIcon.IconIn.PlayFeedbacks();
            else
                bigIcon.IconOut.PlayFeedbacks();

        yield break;
    }

    private IEnumerator NewGameHandle (object arg1, EventArgs arg2)
    {
        playerSideToIcon.Clear();
        playerSideToIcon.TryAdd(ServiceLocator.PlayerSide, bottomIcon);
        playerSideToIcon.TryAdd(ServiceLocator.PlayerSide.GetInverseSide(), topIcon);

        bottomIcon.Setup(ServiceLocator.PlayerSide);
        topIcon.Setup(ServiceLocator.PlayerSide.GetInverseSide());

        yield break;
    }

    private IEnumerator SceneSetupHandle (object arg1, EventArgs arg2)
    {
        bottomIcon.IconOut.PlayFeedbacks();
        topIcon.IconOut.PlayFeedbacks();
        yield break;
    }
}