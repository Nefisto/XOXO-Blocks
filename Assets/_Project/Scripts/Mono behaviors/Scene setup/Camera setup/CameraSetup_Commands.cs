using System;
using QFSW.QC;

public partial class CameraSetup
{
    [Command("Camera_GoingUpAnimation", "Simulate the initial animation when closing waiting room")]
    private void Camera_GoingUpAnimation()
    {
        hudCamera.transform.position = cameraInitialPosition.position;
        StartCoroutine(CloseWaitingRoomHandle(this, EventArgs.Empty));
    }
}