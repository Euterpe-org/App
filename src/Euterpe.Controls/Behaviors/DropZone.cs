using Avalonia.Interactivity;

namespace Euterpe.Controls;

/// <summary>
///     Attached behavior that adds the <c>DragOver</c> style class to a control while files are dragged over it,
///     so an overlay can be revealed purely through a style selector. The drop itself is handled separately
///     (e.g. FilesDropBehavior); this never touches the control's content.
///     An enter/leave depth counter is used because <see cref="DragDrop.AllowDropProperty" /> inherits to
///     children, so the drag events fire repeatedly as the pointer crosses child controls.
/// </summary>
public sealed partial class DropZone : AvaloniaObject
{
    private const string DragOverClass = "DragOver";

    static DropZone() => IsActiveProperty.Changed.AddClassHandler<Control>(OnIsActiveChanged);

    private DropZone()
    {
    }

    [GeneratedAttachedProperty]
    public static partial bool GetIsActive(Control control);

    [GeneratedAttachedProperty]
    private static partial int GetDragDepth(Control control);

    private static void OnIsActiveChanged(Control control, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.GetNewValue<bool>())
        {
            DragDrop.SetAllowDrop(control, true);
            control.AddHandler(DragDrop.DragEnterEvent, OnDragEnter, RoutingStrategies.Bubble, true);
            control.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave, RoutingStrategies.Bubble, true);
            control.AddHandler(DragDrop.DropEvent, OnDrop, RoutingStrategies.Bubble, true);
        }
        else
        {
            control.RemoveHandler(DragDrop.DragEnterEvent, OnDragEnter);
            control.RemoveHandler(DragDrop.DragLeaveEvent, OnDragLeave);
            control.RemoveHandler(DragDrop.DropEvent, OnDrop);
        }
    }

    private static void OnDragEnter(object? sender, DragEventArgs e)
    {
        if (sender is not Control control)
        {
            return;
        }

        var depth = GetDragDepth(control) + 1;
        SetDragDepth(control, depth);
        if (depth is 1)
        {
            control.Classes.Add(DragOverClass);
        }
    }

    private static void OnDragLeave(object? sender, DragEventArgs e)
    {
        if (sender is not Control control)
        {
            return;
        }

        var depth = GetDragDepth(control) - 1;
        if (depth > 0)
        {
            SetDragDepth(control, depth);
            return;
        }

        SetDragDepth(control, 0);
        control.Classes.Remove(DragOverClass);
    }

    private static void OnDrop(object? sender, DragEventArgs e)
    {
        if (sender is not Control control)
        {
            return;
        }

        SetDragDepth(control, 0);
        control.Classes.Remove(DragOverClass);
    }
}
