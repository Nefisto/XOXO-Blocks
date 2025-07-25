using System;
using System.Collections;
using NTools;
using Sirenix.OdinInspector;
using UnityEngine;

[SelectionBase]
public partial class LinePointManager : MonoBehaviour
{
    [TitleGroup("References")]
    [SerializeField]
    private NDictionary<string, LinePoint> mediumBoardLinePoints = new();

    [TitleGroup("References")]
    [SerializeField]
    private NDictionary<string, LinePoint> bigBoardLinePoints = new();

    [TitleGroup("Debug")]
    [ReadOnly]
    [ShowInInspector]
    private bool isMediumSize = true;

    public NDictionary<string, LinePoint> CorrectBoardLinePoints
        => isMediumSize ? mediumBoardLinePoints : bigBoardLinePoints;

    private void Awake()
    {
        ServiceLocator.GameReferences.LinePointManager = this;

        GameEvents.GameplayEvents.OnSetupNewGame += SetupHandle;
        GameEvents.GameplayEvents.OnPointScored += PointScoredHandle;
    }

    private void OnDestroy()
    {
        GameEvents.GameplayEvents.OnSetupNewGame -= SetupHandle;
        GameEvents.GameplayEvents.OnPointScored -= PointScoredHandle;
    }

    private IEnumerator PointScoredHandle (object arg1, EventArgs arg2)
    {
        var ctx = arg2 as GameEvents.OnPointScoredEventArgs;

        foreach (var scoreData in ctx.ScoreResult.scoreData)
            ShowFeedback(scoreData.scoreKey);

        yield break;
    }

    [Button]
    [DisableInEditorMode]
    public async void ShowFeedback (string key) => await CorrectBoardLinePoints[key].Play();

    private IEnumerator SetupHandle (object arg1, EventArgs arg2)
    {
        var ctx = arg2 as GameEvents.OnSetupNewGameEventArgs;
        isMediumSize = ctx.GameSettings.BoardSize == 3;

        foreach (var (_, line) in mediumBoardLinePoints)
            line.gameObject.SetActive(false);
        foreach (var (_, line) in bigBoardLinePoints)
            line.gameObject.SetActive(false);

        yield break;
    }
}