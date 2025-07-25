using System;

public interface IDraggable : IMonobehavior
{
    public Action OnDragStart { get; set; }
    public Action OnDragEnd { get; set; }
}