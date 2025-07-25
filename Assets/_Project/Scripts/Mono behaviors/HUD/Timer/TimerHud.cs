using UnityEngine;
using UnityEngine.UI;

public class TimerHud : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private Image timerImage;

    private Timer timer;

    public void Setup (Timer timer)
    {
        this.timer = timer;

        this.timer.OnTimerStart += TimerStartHandle;
        this.timer.OnTimerUpdate += TickTimerHandle;

        timerImage.fillAmount = 1f;
    }

    private void TickTimerHandle() => timerImage.fillAmount = timer.RemainingNormalizedPercentage;

    private void TimerStartHandle() => timerImage.fillAmount = 1f;
}