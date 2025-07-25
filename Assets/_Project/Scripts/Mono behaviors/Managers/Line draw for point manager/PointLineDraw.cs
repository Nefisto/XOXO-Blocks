using System;
using System.Collections;
using System.Linq;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

public class PointLineDraw : MonoBehaviour
{
    [TitleGroup("References")]
    [SerializeField]
    private MMF_Player line;

    private void Awake()
    {
        GameEvents.GameplayEvents.OnPointScored += DrawLineHandle;
    }

    private void OnDestroy()
    {
        GameEvents.GameplayEvents.OnPointScored -= DrawLineHandle;
    }

    private IEnumerator DrawLineHandle (object arg1, EventArgs arg2)
    {
        var ctx = arg2 as GameEvents.OnPointScoredEventArgs;

        if (ctx.ScoreResult.HasBombConnectedPoint)
        {
            foreach (var scoreData in ctx.ScoreResult.scoreData.Where(sd => sd.Kind == TileKind.Bomb))
            {
                // Show bomb
            }
        }

        yield break;
    }
}