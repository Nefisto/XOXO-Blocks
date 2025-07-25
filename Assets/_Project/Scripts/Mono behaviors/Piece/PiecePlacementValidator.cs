using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PiecePlacementValidator : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField]
    private List<Tile> tiles;

    private void Awake()
    {
        tiles = GetComponentsInChildren<Tile>().ToList();
    }

    public void CancelNotifications()
    {
        tiles.ForEach(t => t.Notification.CancelNotification());
    }

    public void UpdateNotification()
    {
        tiles.ForEach(t =>
        {
            if (t.PlacementValidator.CanBePlaced())
                t.Notification.CancelNotification();
            else
                t.Notification.ShowNotification();
        });
    }
}