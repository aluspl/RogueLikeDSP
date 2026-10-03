using Godot;
using LifeLike.Core.Data;
using LifeLike.Game.Settings;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.World;

/// <summary>
/// v0.21.54: tło za mapą z paralaksą (tylko wygląd). Widać je przygaszone przez ciemność nieznanych pól i poza mapą
/// (FogLayer przepuszcza trochę tła): niebo wg pogody dnia (deszcz – stalowe chmury, upał – pomarańczowy zmierzch ze
/// słońcem, mróz – blade, wiatr – zielonkawe) albo wg aktu przy pogodzie bez efektu, daleko sylwetki budynków
/// w budowie z kilkoma oknami, bliżej żuraw wieżowy i rusztowania. Warstwy przesuwają się wolniej niż kamera
/// (0,04–0,22 jej ruchu) – subtelnie; chmury dryfują tylko bez „ograniczonego ruchu”. Pikselowe prostokąty (2 px),
/// przerysowanie tylko po ruchu kamery albo zmianie nieba.
/// </summary>
public partial class SkyBackdrop : Node2D
{
    private const float Px = 2f;
    private WorldView _w;
    private Vector2 _lastCam = new(float.NaN, float.NaN);
    private int _lastKey = -1;
    private float _clock;

    public void Bind(WorldView w) => _w = w;

    private CoreGame G => _w?.Game;

    /// <summary>Niebo: góra, dół (horyzont); słońce przy upale.</summary>
    private (Color Top, Color Bottom, int Key) Sky()
    {
        var g = G;
        var effect = g is null ? WeatherEffect.None : g.WDef.Effect;
        switch (effect)
        {
            case WeatherEffect.Rain: return (new Color(0.09f, 0.11f, 0.17f), new Color(0.22f, 0.26f, 0.34f), 1);
            case WeatherEffect.Heat: return (new Color(0.18f, 0.09f, 0.12f), new Color(0.58f, 0.31f, 0.17f), 2);
            case WeatherEffect.Frost: return (new Color(0.13f, 0.15f, 0.24f), new Color(0.44f, 0.48f, 0.58f), 3);
            case WeatherEffect.Wind: return (new Color(0.09f, 0.15f, 0.19f), new Color(0.27f, 0.38f, 0.40f), 4);
        }
        var act = g is null ? 1 : g.SDef().Act;
        return act switch
        {
            0 => (new Color(0.12f, 0.08f, 0.20f), new Color(0.36f, 0.22f, 0.38f), 10),
            1 => (new Color(0.10f, 0.10f, 0.22f), new Color(0.44f, 0.29f, 0.30f), 11),
            2 => (new Color(0.08f, 0.12f, 0.24f), new Color(0.30f, 0.36f, 0.52f), 12),
            _ => (new Color(0.05f, 0.05f, 0.12f), new Color(0.17f, 0.17f, 0.30f), 13),
        };
    }

    public override void _Process(double delta)
    {
        if (_w is null) return;
        var drift = !GameSettings.ReduceMotion && G is { } g && g.WDef.Effect is WeatherEffect.Rain or WeatherEffect.Wind;
        if (drift) _clock += (float)delta;
        var cam = _w.Camera.GetScreenCenterPosition();
        var key = Sky().Key;
        if (!drift && cam.IsEqualApprox(_lastCam) && key == _lastKey) return;
        _lastCam = cam;
        _lastKey = key;
        QueueRedraw();
    }

    private static float Snap(float v) => Mathf.Floor(v / Px) * Px;

    private void Box(float x, float y, float w, float h, Color c) => DrawRect(new Rect2(Snap(x), Snap(y), Mathf.Max(Px, Snap(w)), Mathf.Max(Px, Snap(h))), c);

    public override void _Draw()
    {
        if (_w is null) return;
        var cam = _w.Camera.GetScreenCenterPosition();
        var zoom = _w.Camera.Zoom;
        var view = GetViewportRect().Size / new Vector2(Mathf.Max(0.05f, zoom.X), Mathf.Max(0.05f, zoom.Y));
        var tl = cam - view / 2 - new Vector2(8, 8);
        var size = view + new Vector2(16, 16);
        var (top, bottom, key) = Sky();
        DrawPolygon([tl, tl + new Vector2(size.X, 0), tl + size, tl + new Vector2(0, size.Y)], [top, top, bottom, bottom]);
        var horizon = cam.Y + view.Y * 0.18f - cam.Y * 0.05f; // horyzont trochę poniżej środka, prawie nieruchomy
        if (key == 2) // upał: duże, przygaszone słońce
        {
            var sun = new Vector2(cam.X + view.X * 0.22f - cam.X * 0.02f, horizon - 70);
            DrawCircle(sun, 34, new Color(0.95f, 0.62f, 0.3f, 0.35f));
            DrawCircle(sun, 24, new Color(1f, 0.75f, 0.42f, 0.45f));
        }
        if (key is 1 or 3 or 4) Clouds(cam, view, top, bottom);
        var far = bottom.Lerp(top, 0.55f).Darkened(0.25f);
        Skyline(cam, view, horizon, far);
        var mid = bottom.Darkened(0.55f);
        Crane(cam, view, horizon, mid);
        Scaffolding(cam, view, horizon, bottom.Darkened(0.42f), bottom.Lightened(0.08f));
        Box(tl.X, horizon, size.X, tl.Y + size.Y - horizon, bottom.Darkened(0.65f)); // ziemia pod horyzontem
    }

    private void Clouds(Vector2 cam, Vector2 view, Color top, Color bottom)
    {
        const float k = 0.04f, period = 520f;
        var col = top.Lerp(bottom, 0.35f).Darkened(0.2f);
        var shift = Mathf.PosMod(_clock * 6f, period);
        var left = cam.X - view.X / 2;
        var origin = cam.X - cam.X * k + shift;
        for (var i = Mathf.FloorToInt((left - 200 - origin) / period); ; i++)
        {
            var x = origin + i * period;
            if (x > left + view.X) break;
            var y = cam.Y - view.Y * 0.32f + (i & 1) * 26 - cam.Y * 0.03f;
            Box(x, y, 150, 14, col);
            Box(x + 24, y - 10, 90, 12, col);
            Box(x + 60, y + 12, 120, 8, col);
        }
    }

    private void Skyline(Vector2 cam, Vector2 view, float horizon, Color col)
    {
        const float k = 0.08f, period = 420f;
        int[] heights = [70, 110, 54, 140, 86, 62];
        var window = col.Lightened(0.35f);
        var left = cam.X - view.X / 2;
        var offset = cam.X * k;
        for (var i = Mathf.FloorToInt((left - cam.X + offset) / period) - 1; ; i++)
        {
            var bx = cam.X - offset + i * period;
            if (bx > left + view.X + period) break;
            for (var b = 0; b < heights.Length; b++)
            {
                var h = heights[(b + Mathf.PosMod(i, 3)) % heights.Length];
                var x = bx + b * 70;
                Box(x, horizon - h, 56, h, col);
                if (((b + i) & 1) == 0) Box(x + 10, horizon - h + 14, 6, 6, window);
                if (((b + i) % 3) == 0) Box(x + 34, horizon - h + 34, 6, 6, window);
            }
        }
    }

    private void Crane(Vector2 cam, Vector2 view, float horizon, Color col)
    {
        const float k = 0.14f, period = 1300f;
        var left = cam.X - view.X / 2;
        var offset = cam.X * k;
        for (var i = Mathf.FloorToInt((left - cam.X + offset - 400) / period); ; i++)
        {
            var mx = cam.X - offset + i * period + 260;
            if (mx - 300 > left + view.X) break;
            var top = horizon - 250;
            Box(mx, top, 4, 250, col); // maszt: dwa pasy i kratownica
            Box(mx + 14, top, 4, 250, col);
            for (var y = top + 6; y < horizon - 10; y += 20)
            {
                for (var t = 0; t < 7; t++) Box(mx + 4 + t * 2, y + t * 2, 2, 2, col);
            }
            Box(mx - 90, top - 10, 300, 6, col); // wysięgnik i przeciwwysięgnik
            Box(mx - 90, top - 4, 300, 2, col.Darkened(0.2f));
            Box(mx - 86, top - 4, 28, 18, col); // przeciwwaga
            Box(mx + 4, top - 34, 8, 24, col); // wieżyczka
            for (var t = 0; t < 12; t++) Box(mx + 12 + t * 16, top - 34 + t * 2, 4, 2, col); // odciąg
            var hook = mx + 150;
            Box(hook, top - 4, 2, 90, col.Darkened(0.1f));
            Box(hook - 4, top + 86, 10, 6, col);
        }
    }

    private void Scaffolding(Vector2 cam, Vector2 view, float horizon, Color col, Color rim)
    {
        const float k = 0.22f, period = 900f;
        var left = cam.X - view.X / 2;
        var offset = cam.X * k;
        for (var i = Mathf.FloorToInt((left - cam.X + offset - 300) / period); ; i++)
        {
            var sx = cam.X - offset + i * period + 520;
            if (sx > left + view.X) break;
            const int bays = 4, levels = 4;
            const float bw = 54, lh = 44;
            for (var b = 0; b <= bays; b++) Box(sx + b * bw, horizon - levels * lh, 4, levels * lh, col); // stojaki
            for (var l = 0; l <= levels; l++) // poręcze i pomosty
            {
                var y = horizon - l * lh;
                Box(sx, y - 4, bays * bw + 4, 4, col);
                Box(sx, y - 4, bays * bw + 4, 2, rim);
            }
            for (var b = 0; b < bays; b++) // stężenia po skosie co drugie pole
            {
                for (var l = (b & 1); l < levels; l += 2)
                {
                    for (var t = 0; t < 12; t++) Box(sx + b * bw + 4 + t * 4, horizon - l * lh - 6 - t * 3.3f, 2, 2, col);
                }
            }
        }
    }
}
