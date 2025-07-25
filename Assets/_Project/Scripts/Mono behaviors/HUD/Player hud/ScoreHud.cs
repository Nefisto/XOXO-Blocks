using System;
using System.Collections;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreHud : MonoBehaviour
{
    [TitleGroup("Settings")]
    [SerializeField]
    private Color type;

    [Header("References")]
    [SerializeField]
    private TMP_Text scorePointsLabel;

    [SerializeField]
    private Sprite crossIcon;

    [SerializeField]
    private Sprite circleIcon;

    [SerializeField]
    private Material crossMaterial;

    [SerializeField]
    private Material circleMaterial;

    [SerializeField]
    private Image playerSideIcon;

    [Header("Debug")]
    [SerializeField]
    private PlayerSide playerSide;

    public void Setup (PlayerSide playerSide)
    {
        this.playerSide = playerSide;

        playerSideIcon.sprite = playerSide == PlayerSide.Cross ? crossIcon : circleIcon;
        scorePointsLabel.fontSharedMaterial = playerSide == PlayerSide.Cross ? crossMaterial : circleMaterial;
        scorePointsLabel.text = "0";
    }

    public IEnumerator PointScoreHandle (object _, EventArgs arg)
    {
        var ctx = arg as GameEvents.OnPointScoredEventArgs;

        if (ctx.PlayerSide != playerSide)
            yield break;

        var amountOfPoints = playerSide == PlayerSide.Cross
            ? ServiceLocator.GameState.PointsOfCross
            : ServiceLocator.GameState.PointsOfCircle;

        scorePointsLabel.text = $"{amountOfPoints}";
    }
}