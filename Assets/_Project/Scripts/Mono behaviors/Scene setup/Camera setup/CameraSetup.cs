using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public partial class CameraSetup : MonoBehaviour
{
    [TitleGroup("Animation")]
    [SerializeField]
    private float cameraGoingUpDuration = 2f;

    [SerializeField]
    private Ease animationEase = Ease.Linear;

    [TitleGroup("References")]
    [SerializeField]
    private Camera hudCamera;

    [SerializeField]
    private Transform cameraInitialPosition;

    [TitleGroup("References")]
    [SerializeField]
    private Transform cameraFinalPosition;

    private void Awake()
    {
        GameEvents.GameplayEvents.OnSceneSetup += SetupHandle;
        GameEvents.WaitingRoom.OnClosedWaitingRoom += CloseWaitingRoomHandle;
    }

    private void OnDestroy()
    {
        GameEvents.GameplayEvents.OnSceneSetup -= SetupHandle;
        GameEvents.WaitingRoom.OnClosedWaitingRoom -= CloseWaitingRoomHandle;
    }

    private IEnumerator CloseWaitingRoomHandle (object arg1, EventArgs arg2)
    {
        yield return hudCamera
            .transform
            .DOMove(cameraFinalPosition.position, cameraGoingUpDuration)
            .SetEase(animationEase)
            .WaitForCompletion();

        yield return GameEvents.WaitingRoom.OnFinishedMovingUpAnimation?.YieldableInvoke(this, EventArgs.Empty);
    }

    private IEnumerator SetupHandle (object arg1, EventArgs arg2)
    {
        hudCamera.transform.position = cameraInitialPosition.position;
        yield break;
    }
}