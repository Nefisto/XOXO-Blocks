using QFSW.QC;
using Sirenix.OdinInspector;

public partial class HandHud
{
    [Command("Main_Hand.Wait_State", MonoTargetType.Argument)]
    [Button]
    [DisableInEditorMode]
    private void ToggleHandState()
    {
        if (waitingState == WaitingState.Waiting)
            ActiveState();
        else
            WaitState();
    }
}