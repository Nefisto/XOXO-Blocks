using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUDSetup : MonoBehaviour
{
    [TitleGroup("References")]
    [SerializeField]
    private Transform gameHUDRoot;

    [TitleGroup("Debug")]
    [ReadOnly]
    [ShowInInspector]
    private List<Graphic> thingsToHide = new();

    private void Awake()
    {
        CacheTextsAndImages();

        GameEvents.WaitingRoom.OnFinishedMovingUpAnimation += AfterFinishedCameraAnimationHandle;
        GameEvents.GameplayEvents.OnSceneSetup += SceneSetupHandle;
    }

    private void OnDestroy()
    {
        GameEvents.WaitingRoom.OnFinishedMovingUpAnimation -= AfterFinishedCameraAnimationHandle;
        GameEvents.GameplayEvents.OnSceneSetup -= SceneSetupHandle;
    }

    private void CacheTextsAndImages()
    {
        thingsToHide.Clear();

        thingsToHide.AddRange(gameHUDRoot.GetComponentsInChildren<TMP_Text>());
        thingsToHide.AddRange(gameHUDRoot.GetComponentsInChildren<Image>());
    }

    private IEnumerator SceneSetupHandle (object arg1, EventArgs arg2)
    {
        thingsToHide.ForEach(x => x.CrossFadeAlpha(0f, 0f, true));

        yield break;
    }

    private IEnumerator AfterFinishedCameraAnimationHandle (object arg1, EventArgs arg2)
    {
        thingsToHide.ForEach(x => x.CrossFadeAlpha(1f, 1f, true));

        yield return new WaitForSeconds(2f);
    }
}