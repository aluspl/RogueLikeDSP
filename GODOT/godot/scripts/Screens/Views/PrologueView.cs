using Godot;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;

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
        f.Draw(ci, new Vector2(w - 6, Layout.SafeTop + 4), ButtonNames.Pick("Spacja / Enter: pomiń", "Dotknij: pomiń"), Ink.MapDim, TextAlign.Right);
        if (Caption.Length == 0) return;
        const float ts = 1.5f;
        var lines = f.Wrap(Caption, (int)w - 16, ts);
        if (lines.Count > 3) lines = lines.GetRange(0, 3);
        var lh = PixelFont.LineHeight * ts;
        var y = h - Layout.SafeBottom - 24 - lines.Count * lh;
        ci.DrawRect(new Rect2(0, y - 6, w, lines.Count * lh + 16), new Color(Pal.Text, 0.6f * _fade));
        for (var i = 0; i < lines.Count; i++)
            f.Draw(ci, new Vector2(w / 2, y + i * lh), lines[i], Ink.Map.WithAlpha(_fade), TextAlign.Center, ts);
    }
}
