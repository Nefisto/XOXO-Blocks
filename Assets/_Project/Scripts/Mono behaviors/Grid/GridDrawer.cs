using System.Collections;
using System.Collections.Generic;
using NTools;
using Sirenix.OdinInspector;
using UnityEngine;

public class GridDrawer : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GridTile tile;

    [SerializeField]
    private Transform tiledFolder;

    [Button]
    [DisableInEditorMode]
    private void Test_CreateGrid (GridSetting settings = null) => StartCoroutine(CreateGrid(settings));

    public IEnumerator CreateGrid (GridSetting settings = null)
    {
        tiledFolder.DestroyChildren();

        settings ??= new GridSetting();
        var offset = settings.size * -.5f;

        var size = settings.size - 1;
        for (var x = 0; x < settings.size; x++)
        for (var y = 0; y < settings.size; y++)
        {
            var instance = Instantiate(tile, tiledFolder);
            instance.name = $"Tile ({x}, {y})";
            instance.transform.position = new Vector3(offset + y + .5f, offset + (size - x) + .5f, 0);

            settings.positionToTile.TryAdd(new Vector2Int(x, y), instance);
        }

        yield break;
    }

    public class GridSetting
    {
        public Dictionary<Vector2Int, GridTile> positionToTile = new();
        public int size = 3;
    }
}