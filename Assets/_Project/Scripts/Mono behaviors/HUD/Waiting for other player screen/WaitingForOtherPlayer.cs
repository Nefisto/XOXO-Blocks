using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Febucci.UI;
using NTools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[SelectionBase]
public class WaitingForOtherPlayer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private bool skipWaitingPart;

    [Header("References")]
    [SerializeField]
    private TMP_Text textLabel;

    [SerializeField]
    private TypewriterByCharacter typewriter;

    [SerializeField]
    private WaitingRoomPlayerEntry bottomEntry;

    [SerializeField]
    private WaitingRoomPlayerEntry topEntry;

    private List<Image> childrenImages;

    private List<TMP_Text> childrenTexts;

    private NTask showingRoutine;

    private void Awake()
    {
        ServiceLocator.WaitingRoomReferences.WaitingForOtherScreen = this;

        gameObject.SetActive(true);
        showingRoutine = new NTask(CharacterAnimator());
        GameEvents.WaitingRoom.OnClosingWaitingRoom += WaitingSetupHandle;
        GameEvents.WaitingRoom.UpdatePlayerNicks += RefreshPlayerNickHandle;

        childrenTexts = GetComponentsInChildren<TMP_Text>().ToList();
        childrenImages = GetComponentsInChildren<Image>().ToList();
    }

    private void OnDestroy()
    {
        showingRoutine?.Stop();
        showingRoutine = null;

        GameEvents.WaitingRoom.OnClosingWaitingRoom -= WaitingSetupHandle;
        GameEvents.WaitingRoom.UpdatePlayerNicks -= RefreshPlayerNickHandle;
    }

    private IEnumerator WaitingSetupHandle (object arg1, EventArgs arg2)
    {
        showingRoutine?.Stop();
        typewriter.StopAllCoroutines();
        if (skipWaitingPart)
        {
            gameObject.SetActive(false);
            yield break;
        }

        textLabel.text = "Player connected";

        yield return new WaitForSeconds(2f);
        textLabel.text = "Starting game...";
        yield return new WaitForSeconds(2f);

        var duration = 2;
        childrenTexts.ForEach(x => x.DOFade(0f, duration));
        childrenImages.ForEach(image => image.DOFade(0f, duration));
        yield return new WaitForSeconds(2f);

        yield return GameEvents.WaitingRoom.OnClosedWaitingRoom?.YieldableInvoke(this, EventArgs.Empty);
        gameObject.SetActive(false);
    }

    private void RefreshPlayerNickHandle (object sender, EventArgs e)
    {
        var args = e as GameEvents.WaitingRoom.UpdatePlayerNicksEventArgs;

        SetupNicknameOnScreen(args.crossPlayerNickname, args.circlePlayerNickname);
    }

    private void SetupNicknameOnScreen (string crossPlayer = "-", string circlePlayer = "-")
    {
        var me = CommonOperations.GetPlayerRef();
        bottomEntry.SetupEntry(me.PlayerSide, crossPlayer);
        topEntry.SetupEntry(me.PlayerSide.GetInverseSide(), circlePlayer);
    }

    private IEnumerator CharacterAnimator()
    {
        while (true)
        {
            yield return new WaitForSeconds(4f);
            typewriter.StartDisappearingText();
            yield return new WaitForSeconds(4f);
            typewriter.StartShowingText();
        }
    }
}