using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

public class HiddenIcon : MonoBehaviour
{
    [field: TitleGroup("References")]
    [field: SerializeField]
    public MMF_Player CrossIn { get; private set; }

    [field: SerializeField]
    public MMF_Player CrossOut { get; set; }

    [field: SerializeField]
    public MMF_Player CircleIn { get; set; }

    [field: SerializeField]
    public MMF_Player CircleOut { get; set; }

    public static List<HiddenIcon> AllIcons { get; set; } = new();

    private void Awake() => AllIcons.Add(this);

    private void OnDestroy() => AllIcons.Remove(this);

    public async UniTask HidAllAsync()
    {
        await CrossOut.PlayFeedbacksTask().AsUniTask();
        await CircleOut.PlayFeedbacksTask().AsUniTask();
    }
}