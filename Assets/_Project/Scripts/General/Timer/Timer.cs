using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

[Serializable]
public class Timer
{
    [field: Header("Settings")]
    [field: SerializeField]
    public float OriginalDuration { get; private set; }

    [field: SerializeField]
    public float Current { get; private set; }

    private CancellationTokenSource cancellationToken;

    private UniTask timerRoutine;

    public Timer (float duration) => OriginalDuration = duration;
    public Action OnTimerStart { get; set; }
    public Action OnTimerEnd { get; set; }
    public Action OnTimerStopped { get; set; }
    public Action OnTimerUpdate { get; set; }

    public float RemainingNormalizedPercentage => Current / OriginalDuration;

    public async UniTask StartTimer()
    {
        cancellationToken = new CancellationTokenSource();
        await TimerRoutine(cancellationToken.Token);
    }

    public void StopTimer()
    {
        cancellationToken.Cancel();
        OnTimerStopped?.Invoke();
    }

    private async UniTask TimerRoutine (CancellationToken token)
    {
        Current = OriginalDuration;
        OnTimerStart?.Invoke();
        while (Current >= 0f)
        {
            await UniTask.Yield(token);
            Current -= Time.deltaTime;
            OnTimerUpdate?.Invoke();
        }

        Current = 0f;
        OnTimerEnd?.Invoke();
    }
}