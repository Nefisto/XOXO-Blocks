using System.Collections.Generic;
using System.Linq;
using NTools;
using Ricimi;
using UnityEngine;
using UnityEngine.UI;

public static partial class CommonOperations
{
    public static string GetUsername => PlayerPrefs.GetString(GameConstants.NICKNAME_PLAYERPREF_KEY);

    public static void EnabledStateButton (Button button, bool state)
    {
        button.interactable = state;

        if (state)
            button.GetComponentInChildren<ColorSwapper>().EnableColor();
        else
            button.GetComponentInChildren<ColorSwapper>().DisableColor();
    }

    public static void AddPieceToPlayerSide (PlayerSide playerSide, Piece piece, bool intoMainHand)
        => GetPlayerRefOf(playerSide).AddPiece(piece, intoMainHand);

    public static void RemovePieceFromPlayerSide (PlayerSide playerSide, Piece piece)
        => GetPlayerRefOf(playerSide).RemovePiece(piece);

    public static Player GetPlayerRef() => GetPlayerRefOf(ServiceLocator.PlayerSide);
    public static Player GetEnemyRef() => GetEnemyRefOf(ServiceLocator.PlayerSide);

    public static PlayerSide GetWinnerSide()
        => ServiceLocator.GameState.PointsOfCircle > ServiceLocator.GameState.PointsOfCross
            ? PlayerSide.Circle
            : PlayerSide.Cross;

    public static Player GetPlayerRefOf (PlayerSide side)
        => side == PlayerSide.Cross
            ? ServiceLocator.GameReferences.CrossPlayer
            : ServiceLocator.GameReferences.CirclePlayer;

    public static Player GetEnemyRefOf (PlayerSide side)
        => side != PlayerSide.Cross
            ? ServiceLocator.GameReferences.CrossPlayer
            : ServiceLocator.GameReferences.CirclePlayer;

    /// <summary>
    ///     Get all tiles that are on top in their respective grid tile
    /// </summary>
    /// <returns></returns>
    public static List<Tile> GetAllTopTilesOfKind (TileKind tileKind)
    {
        return GetAllTopLayerTiles()
            .Where(t => t.Kind == tileKind)
            .ToList();
    }

    public static void SetAllTilesAsFrontLayer()
        => GetAllTopLayerTiles()
            .ForEach(t => t.SendToFrontLayer());

    public static IEnumerable<Tile> GetAllTopLayerTiles()
        => ServiceLocator.GameReferences
            .GridManager
            .GridTiles
            .Values
            .Where(gt => gt.HasTile())
            .Select(gt => gt.PeekTile());
}