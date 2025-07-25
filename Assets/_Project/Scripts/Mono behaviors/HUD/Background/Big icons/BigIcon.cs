using System;
using System.Collections;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

public class BigIcon : MonoBehaviour
{
    [field: TitleGroup("References")]
    [field: SerializeField]
    public MMF_Player IconIn { get; private set; }

    [field: SerializeField]
    public MMF_Player IconOut { get; set; }

    [SerializeField]
    private SpriteRenderer crossIcon;

    [SerializeField]
    private SpriteRenderer circleIcon;

    private void Awake()
    {
        GameEvents.GameplayEvents.OnSceneSetup += SceneSetupHandle;
        GameEvents.WaitingRoom.OnFinishedMovingUpAnimation += AfterMovingUpAnimationHandle;
    }

    private void OnDestroy()
    {
        GameEvents.GameplayEvents.OnSceneSetup -= SceneSetupHandle;
        GameEvents.WaitingRoom.OnFinishedMovingUpAnimation -= AfterMovingUpAnimationHandle;
    }

    private IEnumerator SceneSetupHandle (object arg1, EventArgs arg2)
    {
        crossIcon.DOFade(0f, 0f);
        circleIcon.DOFade(0f, 0f);
        yield break;
    }

    private IEnumerator AfterMovingUpAnimationHandle (object arg1, EventArgs arg2)
    {
        crossIcon.DOFade(1f, 0f);
        circleIcon.DOFade(1f, 0f);
        yield break;
    }

    public void Setup (PlayerSide playerSide)
    {
        crossIcon.gameObject.SetActive(false);
        circleIcon.gameObject.SetActive(false);

        if (playerSide == PlayerSide.Cross)
            crossIcon.gameObject.SetActive(true);
        else
            circleIcon.gameObject.SetActive(true);
    }
}