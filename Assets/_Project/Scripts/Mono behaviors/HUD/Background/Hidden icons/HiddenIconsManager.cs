using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using NTools;
using UnityEngine;
using Random = UnityEngine.Random;

public class HiddenIconsManager : MonoBehaviour
{
    private NTask routine;

    private void Awake()
    {
        GameEvents.GameplayEvents.OnStartingNewGame += GameStartHandle;
        GameEvents.GameplayEvents.OnStartingNewTurn += NewTurnHandle;
    }

    private void OnDestroy()
    {
        GameEvents.GameplayEvents.OnStartingNewGame -= GameStartHandle;
        GameEvents.GameplayEvents.OnStartingNewTurn -= NewTurnHandle;
    }

    private IEnumerator GameStartHandle (object arg1, EventArgs arg2)
    {
        foreach (var hiddenIcon in HiddenIcon.AllIcons.ToList())
            StartCoroutine(hiddenIcon.HidAllAsync().ToCoroutine());

        yield break;
    }

    private IEnumerator NewTurnHandle (object arg1, EventArgs arg2)
    {
        var ctx = arg2 as GameEvents.OnStartNewTurnEventArgs;
        routine?.Stop();

        foreach (var hiddenIcon in HiddenIcon.AllIcons.ToList())
            if (ctx.PlayerSide == PlayerSide.Cross)
                hiddenIcon.CircleOut.PlayFeedbacks();
            else
                hiddenIcon.CrossOut.PlayFeedbacks();

        routine = new NTask(ShowIconsRoutine(ctx.PlayerSide));
        yield break;
    }

    private static IEnumerator ShowIconsRoutine (PlayerSide side)
    {
        foreach (var hiddenIcon in HiddenIcon.AllIcons.Shuffle())
        {
            if (side == PlayerSide.Cross)
                hiddenIcon.CrossIn.PlayFeedbacks();
            else
                hiddenIcon.CircleIn.PlayFeedbacks();

            yield return new WaitForSeconds(Random.Range(0f, 0.75f));
        }
    }
}