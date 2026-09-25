using System;
using Godot;

namespace LifeLike.Game.Gfx;

/// <summary>Kontrolka na całą powierzchnię rodzica, która rysuje przez przekazaną funkcję (warstwa nad dziećmi rodzica).</summary>
public partial class DrawHook : Control
{
    private readonly Action<CanvasItem> _draw;

    public DrawHook(Action<CanvasItem> draw) => _draw = draw;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    public override void _Process(double delta)
    {
        if (IsVisibleInTree()) QueueRedraw();
    }

    public override void _Draw()
    {
        try
        {
            _draw(this);
        }
        catch (Exception ex)
        {
            DrawErrors.Record(GetParent()?.Name ?? "DrawHook", ex);
        }
    }
}
