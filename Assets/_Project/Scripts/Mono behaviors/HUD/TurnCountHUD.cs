using System;
using System.Collections;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[SelectionBase]
public class TurnCountHUD : MonoBehaviour
{
    [TitleGroup("Settings")]
    [SerializeField]
    private Color crossTurnColor;

    [SerializeField]
    private Color circleTurnColor;

    [Header("References")]
    [SerializeField]
    private TMP_Text turnCountLabel;

    [SerializeField]
    private Image sideIcon;

    [SerializeField]
    private Image background;

    [SerializeField]
    private Sprite crossIcon;

    [SerializeField]
    private Sprite circleIcon;

    private TurnController turnController;

    private void Awake()
    {
        GameEvents.GameplayEvents.OnSetupNewGame += NewGameHandle;
        GameEvents.GameplayEvents.OnStartingNewTurn += NewTurnHandle;
        GameEvents.GameplayEvents.OnSetupReferencesNewGame += SetupReferencesHandle;
    }

    private void OnDestroy()
    {
        GameEvents.GameplayEvents.OnSetupNewGame -= NewGameHandle;
        GameEvents.GameplayEvents.OnStartingNewTurn -= NewTurnHandle;
        GameEvents.GameplayEvents.OnSetupReferencesNewGame -= SetupReferencesHandle;

        if (ServiceLocator.GameReferences.TurnController == null)
            return;

        ServiceLocator.GameReferences.TurnController.CurrentTurn.OnValueChanged -= OnTurnValueChanged;
    }

    private IEnumerator SetupReferencesHandle (object arg1, EventArgs arg2)
    {
        if (ServiceLocator.GameReferences.TurnController == null)
            yield break;

        ServiceLocator.GameReferences.TurnController.CurrentTurn.OnValueChanged += OnTurnValueChanged;
    }

    private IEnumerator NewGameHandle (object arg1, EventArgs arg2)
    {
        turnCountLabel.text = "Turn 01";
        yield break;
    }

    private IEnumerator NewTurnHandle (object arg1, EventArgs arg2)
    {
        var ctx = arg2 as GameEvents.OnStartNewTurnEventArgs;

        Assert.IsNotNull(ctx);

        background.color = ctx.PlayerSide == PlayerSide.Cross ? crossTurnColor : circleTurnColor;
        sideIcon.sprite = ctx.PlayerSide == PlayerSide.Cross ? crossIcon : circleIcon;
        yield break;
    }

    private void OnTurnValueChanged (int _, int newValue)
    {
        turnCountLabel.text = $"Turn {newValue:D2}";
    }
}