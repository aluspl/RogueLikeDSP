using LifeLike.Game.Gfx;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Touch;

/// <summary>Warstwa sterowania dotykiem na mapie (w pikselach UI): pasek akcji i opcjonalna gałka.</summary>
public partial class TouchControls : ScaledLayer
{
    public ActionBar Bar { get; } = new();
    public VirtualStick Stick { get; } = new();

    public override void _Ready()
    {
        Layer = 1;
        Root.AddChild(Stick);
        Root.AddChild(Bar);
        Visible = false;
        base._Ready();
    }

    public void Bind(CoreGame g) => Bar.Bind(g);

    public void Redraw()
    {
        Bar.QueueRedraw();
        Stick.QueueRedraw();
    }
}
