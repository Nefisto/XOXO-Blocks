using System;
using System.Collections;
using UnityEngine;

public class DefaultDrawBehavior : IDrawBehavior
{
    private readonly Action<bool> getNewPieceCallback;
    private readonly Hand hand;

    public DefaultDrawBehavior (Hand hand, Action<bool> getNewPieceCallback)
    {
        this.hand = hand;
        this.getNewPieceCallback = getNewPieceCallback;
    }

    public IEnumerator InitialDrawPhase()
    {
        while (hand.Pieces.Count < ServiceLocator.GameSettings.HandSize)
        {
            getNewPieceCallback?.Invoke(true);
            yield return new WaitForSeconds(1f);
        }
    }

    public IEnumerator DrawPhase()
    {
        getNewPieceCallback?.Invoke(true);
        yield break;
    }

    public IEnumerator DiscardPhase()
    {
        yield break;
    }
}