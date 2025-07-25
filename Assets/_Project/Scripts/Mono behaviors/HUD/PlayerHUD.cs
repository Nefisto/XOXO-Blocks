using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private bool isBottomHud;

    [Header("References")]
    [SerializeField]
    public ScoreHud scoreHud;

    [SerializeField]
    private HandHud handHud;

    [Tooltip("Used in some game modes")]
    [SerializeField]
    private HandHud subHandHud;

    [SerializeField]
    private TimerHud timerHud;

    [SerializeField]
    private TMP_Text nickname;

    [SerializeField]
    private TMP_Text nicknameShadow;

    [SerializeField]
    private BigEmoji bigEmoji;

    [Header("Debug")]
    [SerializeField]
    private PlayerSide playerSide;

    private void Awake()
    {
        GameEvents.GameplayEvents.OnSetupReferencesNewGame += ReferencePlayerHudHandle;
        GameEvents.GameplayEvents.OnPointScored += PointScoreHandle;
    }

    private void OnDestroy()
    {
        GameEvents.GameplayEvents.OnSetupReferencesNewGame -= ReferencePlayerHudHandle;
        GameEvents.GameplayEvents.OnPointScored -= PointScoreHandle;
    }

    public void SetupHUD (UserData userData, Hand hand, Timer timer, Hand subHand = null)
    {
        playerSide = userData.playerSide;

        nickname.text = userData.userName;
        nicknameShadow.text = userData.userName;

        scoreHud.Setup(playerSide);

        if (hand != null)
            handHud.Setup(hand, .75f);

        if (subHand != null && subHandHud != null)
            subHandHud.Setup(subHand, .4f);

        timerHud.Setup(timer);
    }

    public void WaitTurn()
    {
        handHud.WaitState();
    }

    public void ActiveTurn()
    {
        handHud.ActiveState();
    }

    public void ShowEmoji (EmojisKind kind) => bigEmoji.Animate(kind);

    private IEnumerator ReferencePlayerHudHandle (object caller, EventArgs args)
    {
        if (isBottomHud)
            ServiceLocator.GameReferences.BottomPlayerHud = this;
        else
            ServiceLocator.GameReferences.TopPlayerHud = this;

        yield break;
    }

    private IEnumerator PointScoreHandle (object _, EventArgs arg)
    {
        yield return scoreHud.PointScoreHandle(this, arg);
    }
}