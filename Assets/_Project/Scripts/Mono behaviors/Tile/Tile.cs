using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

[Serializable]
public class TileStateSettings
{
    public float colorTransitionDuration = .5f;
    public Color backgroundColor;
    public Color tileColor;
}

public partial class Tile : MonoBehaviour
{
    [field: Header("Settings")]
    [field: SerializeField]
    public TileKind Kind { get; private set; }

    private IEnumerator changeTileRoutine;

    private TileVisual visual;

    public TilePlacementValidator PlacementValidator { get; private set; }
    public TileNotification Notification { get; private set; }

    private void Awake()
    {
        visual = GetComponent<TileVisual>();

        PlacementValidator = GetComponent<TilePlacementValidator>();
        Notification = GetComponent<TileNotification>();
    }

    public void Setup (TileKind kind)
    {
        Kind = kind;
        visual.InstantlyUpdateSprite(Kind);
    }

    public async UniTask SetupState (TileStateSettings settings)
    {
        await visual.SetupState(settings);
    }

    public void SendToBackLayer()
    {
        visual.SendToBackLayer();
    }

    public void SendToFrontLayer()
    {
        visual.SendToFrontLayer();
    }

    public void ChangeTileKind (TileKind kind)
    {
        if (changeTileRoutine != null)
            return;

        changeTileRoutine = Routine();
        StartCoroutine(changeTileRoutine);

        IEnumerator Routine()
        {
            Kind = kind;
            yield return visual.UpdateSprite(Kind);
            changeTileRoutine = null;
        }
    }

    public void UpdateLayer (int gameStateCurrentTurn) => visual.UpdateLayer(gameStateCurrentTurn);

    public GridTile GetGridTileBelow()
        => Physics2D
            .OverlapCircle(transform.position, .1f, LayerMask.GetMask("Grid Tile"))
            ?.GetComponent<GridTile>();

    public async UniTask Place()
    {
        await visual.Place();
    }
}