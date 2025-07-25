using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileMouse : MonoBehaviour, IDraggable
{
    [Header("References")]
    [SerializeField]
    private EventTrigger eventTrigger;

    private void Start()
    {
        var beginDragEntry = new EventTrigger.Entry();
        beginDragEntry.eventID = EventTriggerType.BeginDrag;
        beginDragEntry.callback.AddListener(_ => OnDragStart?.Invoke());

        var endDragEntry = new EventTrigger.Entry();
        endDragEntry.eventID = EventTriggerType.EndDrag;
        endDragEntry.callback.AddListener(_ => OnDragEnd?.Invoke());

        eventTrigger.triggers.Add(beginDragEntry);
        eventTrigger.triggers.Add(endDragEntry);
    }

    public Action OnDragStart { get; set; }
    public Action OnDragEnd { get; set; }
}