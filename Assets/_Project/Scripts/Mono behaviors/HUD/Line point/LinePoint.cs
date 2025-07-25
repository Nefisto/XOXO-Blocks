using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoreMountains.Feedbacks;
using NTools;
using Sirenix.OdinInspector;
using UnityEngine;

public enum LinePointSize
{
    Medium,
    Big
}

public enum LinePointDirection
{
    Horizontal,
    Vertical,
    MainDiagonal,
    SubDiagonal
}

public struct LinePointSettings
{
    public LinePointSize size;
    public Vector2Int initialPoint;
    public LinePointDirection direction;
}

public class LinePoint : MonoBehaviour
{
    [TitleGroup("References")]
    [SerializeField]
    private Transform root;

    [TitleGroup("References")]
    [SerializeField]
    private MMF_Player feedback;

    private SpriteRenderer model;

    private void Awake() => model = GetComponentInChildren<SpriteRenderer>();

    public async UniTask Play()
    {
        feedback.gameObject.SetActive(true);
        model.DOFade(1f, 0f);

        await feedback
            .PlayFeedbacksTask()
            .AsUniTask();

        await UniTask
            .WaitForSeconds(0.5f);

        await model
            .DOFade(0f, 0.5f)
            .AsyncWaitForCompletion();

        feedback.gameObject.SetActive(false);
    }

    [Button]
    [DisableInEditorMode]
    public async UniTask Play (LinePointSettings settings)
    {
        await Setup(settings);

        feedback.gameObject.SetActive(true);
        model.DOFade(1f, 0f);

        await feedback
            .PlayFeedbacksTask()
            .AsUniTask();

        await UniTask
            .WaitForSeconds(0.5f);

        await model
            .DOFade(0f, 0.5f)
            .AsyncWaitForCompletion();
    }

    private async UniTask Setup (LinePointSettings settings)
    {
        root.position = settings.initialPoint.ToVector3();
        root.rotation = GetRotation(settings.direction);
        root.localScale = GetScale(settings.size,
            settings.direction is LinePointDirection.MainDiagonal or LinePointDirection.SubDiagonal);
        
        // Set point color

        await UniTask.Yield();
    }

    private static Quaternion GetRotation (LinePointDirection settingsDirection)
        => settingsDirection switch
        {
            LinePointDirection.Horizontal => Quaternion.Euler(0f, 0f, 0f),
            LinePointDirection.Vertical => Quaternion.Euler(0f, 0f, 90f),
            LinePointDirection.MainDiagonal => Quaternion.Euler(0f, 0f, 45f),
            LinePointDirection.SubDiagonal => Quaternion.Euler(0f, 0f, -45f)
        };

    private static Vector3 GetScale (LinePointSize settingsSize, bool isDiagonal)
    {
        if (isDiagonal)
            return new Vector3(Mathf.Sqrt(settingsSize == LinePointSize.Medium ? 18 : 32), 0.2f, 1f);

        return settingsSize switch
        {
            LinePointSize.Medium => new Vector3(3f, 0.2f, 1f),
            LinePointSize.Big => new Vector3(4f, 0.2f, 1f)
        };
    }
}