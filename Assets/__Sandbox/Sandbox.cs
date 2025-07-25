using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class Sandbox : MonoBehaviour
{
    public TimerHud timerHud;

    [Button]
    public void TestCrazy()
    {
        var controller = ServiceLocator.ApplicationController;

        controller.MoveToScene("", new ApplicationController.MoveToSceneSettings
            {
                BeforeFadeOut = BeforeHandle,
                AfterFadeOut = AfterHandle
            })
            .Forget();
    }

    private async UniTask AfterHandle()
    {
        Debug.Log("AFTER HI");
        await UniTask.Delay(3000);
        Debug.Log("AFTER BYE");
    }

    private async UniTask BeforeHandle()
    {
        Debug.Log("BEFORE HI");
        await UniTask.Delay(3000);
        Debug.Log("BEFORE BYE");
        await UniTask.Delay(3000);
    }

    [Button]
    private async void StartTimer (float duration)
    {
        var timer = new Timer(duration)
        {
            OnTimerStart = () => Debug.Log("Start timer"),
            OnTimerEnd = () => Debug.Log("End timer"),
            OnTimerUpdate = () => Debug.Log("Tick timer")
        };
        timerHud.Setup(timer);

        await timer.StartTimer();
    }
}