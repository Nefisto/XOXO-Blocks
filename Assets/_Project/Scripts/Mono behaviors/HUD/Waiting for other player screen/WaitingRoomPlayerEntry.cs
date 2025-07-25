using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitingRoomPlayerEntry : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private bool isBottom;

    [Header("References")]
    [SerializeField]
    private Image icon;

    [SerializeField]
    private TMP_Text nickLabel;

    [SerializeField]
    private Sprite crossIcon;

    [SerializeField]
    private Sprite circleIcon;

    private void Awake() => GameEvents.GameplayEvents.OnSetupReferencesNewGame += SetupHandle;

    private void OnDestroy() => GameEvents.GameplayEvents.OnSetupReferencesNewGame -= SetupHandle;

    public void SetupEntry (PlayerSide side, string nick)
    {
        icon.sprite = side == PlayerSide.Circle ? circleIcon : crossIcon;
        nickLabel.text = nick;
    }

    private IEnumerator SetupHandle (object arg1, EventArgs arg2)
    {
        if (isBottom)
            ServiceLocator.WaitingRoomReferences.BottomWaitingRoomPlayerEntry = this;
        else
            ServiceLocator.WaitingRoomReferences.TopWaitingRoomPlayerEntry = this;
        yield break;
    }
}