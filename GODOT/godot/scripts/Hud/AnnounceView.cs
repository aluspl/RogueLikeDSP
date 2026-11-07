using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Gfx;
using LifeLike.Game.Settings;

namespace LifeLike.Game.Hud;

/// <summary>
/// v0.23.0: ogłoszenia „z przytupem” (awans, Markowa / Mistrzowska broń, boss pokonany, sekretne zlecenie): pas przez
/// ekran z pasami ostrzegawczymi, duży pikselowy napis wpada po skosie, przy lądowaniu krótki wstrząs i błysk; obok
/// zamach daną bronią (weapons_swing, ~70 ms/klatkę, bez wygładzania). ~1 s, nie przyjmuje wejścia (gra toczy się dalej),
/// kilka naraz – kolejka (AnnounceQueue). Ograniczony ruch: bez wstrząsu, błysku i wlotu – sam napis i statyczna klatka.
/// </summary>
public partial class AnnounceView : Control
{
    private const float FlyIn = 0.18f, ShakeTime = 0.16f, FadeOut = 0.2f;

    public AnnounceQueue Queue { get; } = new();

    /// <summary>Dane gry i broń bohatera (ogłoszenie z Weapon = -1).</summary>
    public System.Func<GameData> DataSource { get; set; } = () => null;

    private GameData Data => DataSource();

    /// <summary>Zrzuty ekranu: zatrzymany czas animacji (s; &lt; 0 – normalnie).</summary>
    public float FreezeAt { get; set; } = -1f;

    private float _clock;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    public void Push(Announcement a) => Queue.Push(a);

    public override void _Process(double delta)
    {
        _clock += (float)delta;
        if (FreezeAt >= 0f) Queue.Seek(FreezeAt);
        else Queue.Update((float)delta);
        Visible = Queue.Busy;
        if (Visible) QueueRedraw();
    }

    public override void _Draw()
    {
        try
        {
            DrawContent();
        }
        catch (System.Exception ex)
        {
            DrawErrors.Record("AnnounceView", ex);
        }
    }

    private static Color TitleColor(Announcement a) => a.Kind switch
    {
        AnnounceKind.Unique => new Color("ffd84a"),
        AnnounceKind.Branded => new Color("b2a0ff"),
        AnnounceKind.Secret => new Color("ffd84a"),
        _ => Pal.Accent,
    };

    private static Color EdgeColor(Announcement a) => a.Kind is AnnounceKind.Unique or AnnounceKind.Secret ? new Color("6b4a00") : Pal.Brand.Darkened(0.35f);

    private void DrawContent()
    {
        var a = Queue.Current;
        if (a == null || Data == null) return;
        var t = Queue.Elapsed;
        var calm = GameSettings.ReduceMotion;
        var f = PixelFont.I;
        var w = Size.X;
        var u = Mathf.Clamp(Mathf.Floor(w / 320f), 1f, 4f); // piksel pasów i odstępów
        var title = a.Title;
        var scale = Mathf.Clamp(Mathf.Floor(Mathf.Min(w / 240f, Size.Y / 250f)), 2f, 4f);
        while (scale > 1.5f && f.Measure(title, scale) > w * 0.6f) scale -= 0.5f;
        var subScale = Mathf.Max(1f, Mathf.Floor(scale / 2f));
        var size = 32f * Mathf.Floor(scale); // zamach (klatka 32 px GBA) x2-x4 bez wygładzania
        size = Mathf.Max(size, 48f);
        var bandH = Mathf.Max(size, 16f * scale + 16f * subScale + 6f * u) + 8f * u;
        var top = Mathf.Round(Size.Y * (w > Size.Y ? 0.38f : 0.34f) - bandH / 2); // w poziomie pod banerem push
        var fade = calm ? 1f : Mathf.Clamp((AnnounceQueue.Duration - t) / FadeOut, 0f, 1f);
        var shake = Vector2.Zero;
        if (!calm && t > FlyIn && t < FlyIn + ShakeTime)
        {
            var k = 1f - (t - FlyIn) / ShakeTime;
            shake = new Vector2(Mathf.Sin(t * 90f), Mathf.Cos(t * 70f)) * 3f * u * k;
        }
        // pas: ciemny fiolet z pasami ostrzegawczymi (przewijanymi) u góry i u dołu
        var band = new Rect2(0, top, w, bandH);
        var open = calm ? 1f : Mathf.Clamp(t / 0.08f, 0f, 1f);
        var bandR = new Rect2(band.Position.X, band.GetCenter().Y - bandH * open / 2, w, bandH * open);
        DrawRect(bandR, new Color(Pal.Navy, 0.88f));
        var sh = 3f * u;
        var scroll = calm ? 0f : _clock * 40f;
        Modulate = new Color(1, 1, 1, fade);
        Ui.WarningStripe(this, new Rect2(0, bandR.Position.Y - sh, w, sh), scroll);
        Ui.WarningStripe(this, new Rect2(0, bandR.End.Y, w, sh), -scroll);
        // napis: wpada z lewego dołu po skosie, ląduje z wstrząsem
        var e = calm ? 1f : Mathf.Clamp(t / FlyIn, 0f, 1f);
        e = 1f - (1f - e) * (1f - e) * (1f - e);
        var cx = Mathf.Round(w / 2 + size * 0.5f);
        var landY = top + 4f * u + Mathf.Max(0f, (bandH - 8f * u - 16f * scale - 16f * subScale - 6f * u) / 2);
        var fly = new Vector2(-(1f - e) * w * 0.6f, (1f - e) * bandH * 1.2f);
        var skew = calm ? 0f : 0.22f * (1f - e) + 0.06f; // lekki skos zostaje (pochylenie napisu)
        var at = new Vector2(cx, landY) + fly + shake;
        DrawSetTransformMatrix(new Transform2D(0f, Vector2.One, -skew, at));
        f.Draw(this, new Vector2(0, 0), title, new Ink(TitleColor(a), EdgeColor(a)), TextAlign.Center, scale);
        DrawSetTransformMatrix(Transform2D.Identity);
        if (a.Sub.Length > 0)
        {
            var sy = landY + 16f * scale + 4f * u;
            f.Draw(this, new Vector2(cx, sy) + shake * 0.5f, f.Fit(a.Sub, (int)(w * 0.6f), subScale), new Ink(Colors.White, Pal.Text), TextAlign.Center, subScale);
        }
        // broń: zamach (Markowa / Mistrzowska) albo duża ikona
        var wd = a.Weapon >= 0 && a.Weapon < Data.Weapons.Length ? Data.Weapons[a.Weapon] : null;
        if (wd != null)
        {
            var titleW = f.Measure(title, scale);
            var wx = Mathf.Max(4f, cx - titleW / 2 - size - 4f * u);
            var wy = band.GetCenter().Y - size / 2;
            var row = WeaponSheet.SwingRow(wd, a.Rarity);
            var pos = new Vector2(wx, wy) + shake;
            if (row >= 0)
            {
                var loop = AnnounceQueue.SwingStep * (WeaponSheet.SwingFrames + 2); // zamach i krótka pauza, w kółko
                var k = calm ? 2 : AnnounceQueue.SwingFrame(Mathf.Max(0f, t - FlyIn * 0.5f) % loop, WeaponSheet.SwingFrames);
                DrawTextureRectRegion(WeaponSheet.Swing, new Rect2(pos, new Vector2(size, size)), WeaponSheet.SwingRegion(row, k));
            }
            else
            {
                var bob = calm ? 0f : Mathf.Round(Mathf.Sin(t * 12f) * u);
                WeaponSheet.DrawIcon(this, wd, a.Rarity, new Rect2(pos + new Vector2(size * 0.15f, size * 0.15f + bob), new Vector2(size * 0.7f, size * 0.7f)));
            }
        }
        // błysk przy lądowaniu
        if (!calm && t > FlyIn && t < FlyIn + 0.14f)
        {
            var k = 1f - (t - FlyIn) / 0.14f;
            var flash = a.Kind is AnnounceKind.Unique or AnnounceKind.Secret ? new Color(1f, 0.9f, 0.4f) : new Color(1f, 0.95f, 1f);
            DrawRect(bandR, new Color(flash, 0.55f * k));
        }
    }
}
