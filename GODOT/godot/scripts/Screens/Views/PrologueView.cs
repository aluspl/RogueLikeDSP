using Godot;
using LifeLike.Game.Gfx;

namespace LifeLike.Game.Screens.Views;

/// <summary>
/// Plansza prologu: plac (PrologueStage) przesuwany kamerą i powiększany jak mapa, podpis u dołu (napisy
/// z game.json story.prologueCaptions) i podpowiedź pominięcia.
/// </summary>
public partial class PrologueView : Control
{
    private float _fade;

    public PrologueStage Stage { get; } = new();

    /// <summary>Środek kamery w pikselach placu.</summary>
    public Vector2 Camera { get; set; }

    public string Caption { get; set; } = "";

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        ClipContents = true;
        AddChild(Stage);
        AddChild(new DrawHook(DrawOverlay));
    }

    public override void _Process(double delta)
    {
        if (!Visible) return;
        _fade = Caption.Length > 0 ? Mathf.MoveToward(_fade, 1f, (float)delta * 3f) : 0f;
        var zoom = Mathf.Min(Layout.WorldZoom, Layout.Snap(Size.Y / (12f * PrologueStage.Cell)));
        Stage.Scale = Vector2.One * zoom;
        Stage.Position = (Size / 2 - Camera * zoom).Round();
        QueueRedraw();
    }

    public override void _Draw() => DrawRect(new Rect2(Vector2.Zero, Size), Pal.Void);

    /// <summary>Podpis i podpowiedź nad placem (DrawHook - dziecko rysuje się po placu).</summary>
    private void DrawOverlay(CanvasItem ci)
    {
        var f = PixelFont.I;
        var w = Size.X;
        var h = Size.Y;
        f.Draw(ci, new Vector2(w - 6, 4), "Spacja / Enter: pomiń", Ink.MapDim, TextAlign.Right);
        if (Caption.Length == 0) return;
        const float ts = 1.5f;
        var y = h - 48;
        ci.DrawRect(new Rect2(0, y - 6, w, 40), new Color(Pal.Text, 0.6f * _fade));
        f.Draw(ci, new Vector2(w / 2, y), f.Fit(Caption, (int)w - 16, ts), Ink.Map.WithAlpha(_fade), TextAlign.Center, ts);
    }
}
