using Sirenix.OdinInspector;
using UnityEngine;

public class TileOfflineTools : MonoBehaviour
{
    [Button]
    public void UpdateVisual()
    {
        StartCoroutine(GetComponent<TileVisual>().UpdateSprite(GetComponent<Tile>().Kind));
    }
}