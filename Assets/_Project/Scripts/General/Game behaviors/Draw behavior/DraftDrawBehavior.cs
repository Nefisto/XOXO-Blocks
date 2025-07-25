using System;
using System.Collections;
using UnityEngine;

public class DraftDrawBehavior : IDrawBehavior
{
    private readonly Action<bool> getNewPieceCallback;
    private readonly Hand hand;
    private readonly Player player;
    private Hand subHand;

    public DraftDrawBehavior (Player player, Hand hand, Hand subHand, Action<bool> getNewPieceCallback)
    {
        this.player = player;
        this.hand = hand;
        this.subHand = subHand;
        this.getNewPieceCallback = getNewPieceCallback;
    }

    public IEnumerator InitialDrawPhase()
    {
        while (hand.Pieces.Count < ServiceLocator.GameSettings.HandSize)
        {
            getNewPieceCallback?.Invoke(true);
            getNewPieceCallback?.Invoke(false);

            yield return new WaitForSeconds(1f);
        }
    }

    public IEnumerator DrawPhase()
    {
        yield break;
    }

    public IEnumerator DiscardPhase()
    {
        player.GetNewDraftHandServerRpc();
        yield return new WaitForSeconds(0.75f);
    }
}