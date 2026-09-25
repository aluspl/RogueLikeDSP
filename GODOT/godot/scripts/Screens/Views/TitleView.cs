using Godot;
using LifeLike.Game.Gfx;

namespace LifeLike.Game.Screens.Views;

/// <summary>
/// Ekran tytułowy jak na GBA: gradient fioletu marki, logo PB z napisem (ui/title.png z GBA w 2x), pas ostrzegawczy
/// placu budowy na dole, wersja z game.json w lewym górnym rogu, rekord w prawym, menu z wyborem.
/// </summary>
public partial class TitleView : Control
{
    public static readonly string[] Items = ["Nowa budowa", "Profil: odznaki, zlecenia", "Szkolenia (Koszty)"];

    public string Version { get; set; } = "";
    public int Best { get; set; }
    public int Runs { get; set; }
    public int Xp { get; set; }
    public int Sel { get; set; }
    public string Note { get; set; } = "";

    private const float MenuScale = 1.5f;
    private float _clock;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    public override void _Process(double delta)
    {
        if (!Visible) return;
        _clock += (float)delta;
        QueueRedraw();
    }

    public override void _Draw()
    {
        try
        {
            DrawContent();
        }
        catch (System.Exception ex)
        {
            DrawErrors.Record("TitleView", ex);
        }
    }

    private void DrawContent()
    {
        var f = PixelFont.I;
        var w = Size.X;
        var h = Size.Y;
        Ui.VioletGradient(this, new Rect2(0, 0, w, h));
        var logo = Assets.Tex("ui/title.png");
        var bob = Mathf.Round(Mathf.Sin(_clock * 1.6f) * 2f);
        var ls = h >= 440 ? 2f : 1.5f; // logo z GBA (240x128) - mniejsze, gdy menu potrzebuje miejsca
        var lsz = logo.GetSize() * ls;
        DrawTextureRect(logo, new Rect2(Mathf.Round((w - lsz.X) / 2), -6 * ls + bob, lsz.X, lsz.Y), false);
        Ui.WarningStripe(this, new Rect2(0, h - 14, w, 14), _clock * 12f);

        f.Draw(this, new Vector2(6, 4), Version, Ink.OnBrand);
        if (Best > 0) f.Draw(this, new Vector2(w - 6, 4), $"Rekord: {Best}", Ink.OnBrand, TextAlign.Right);

        const float ts = MenuScale;
        var rowH = Mathf.Round(PixelFont.LineHeight * ts) + 4;
        var y = Mathf.Round(lsz.Y - 6 * ls + 4);
        for (var i = 0; i < Items.Length; i++)
        {
            var sel = i == Sel;
            var tw = f.Measure(Items[i], ts) + 40;
            var r = new Rect2(Mathf.Round((w - tw) / 2), y + i * (rowH + 4), tw, rowH);
            var ty = r.Position.Y + 2;
            if (sel)
            {
                DrawStyleBox(Ui.Box(new Color(0, 0, 0, 0.25f), 11), new Rect2(r.Position + new Vector2(0, 2), r.Size));
                DrawStyleBox(Ui.Box(Pal.Card, 11), r);
                f.Draw(this, new Vector2(w / 2, ty), Items[i], Ink.Brand, TextAlign.Center, ts);
                var arrow = ((int)(_clock * 3) & 1) == 1 ? 1 : 0;
                f.Draw(this, new Vector2(r.Position.X + 8 + arrow, ty), ">", new Ink(Pal.Accent, Colors.Transparent), TextAlign.Left, ts);
            }
            else
            {
                f.Draw(this, new Vector2(w / 2, ty), Items[i], Ink.OnBrand, TextAlign.Center, ts);
            }
        }
        var info = Note.Length > 0 ? Note : $"Budowy: {Runs}   Doświadczenie: {Xp}   P: profil   K: Szkolenia";
        f.Draw(this, new Vector2(w / 2, h - 34), f.Fit(info, (int)w - 12), Note.Length > 0 ? Ink.NoteOnBrand : Ink.OnBrand, TextAlign.Center);
    }
}
