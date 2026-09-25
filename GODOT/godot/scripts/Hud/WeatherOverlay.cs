using Godot;
using LifeLike.Core.Data;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Hud;

/// <summary>
/// Pogoda dnia na ekranie (warstwa HUD, pod paskami): deszcz – ukośne krople, mróz – płatki śniegu,
/// wiatr – smugi powietrza, upał – drgające ciepłe pasma i lekko cieplejsze światło. Słonecznie: nic.
/// Cząsteczki są rysowane w _Draw z tablicy (bez węzłów), ruch zależy tylko od czasu.
/// </summary>
public partial class WeatherOverlay : Control
{
    private const int Count = 90;
    private readonly Vector2[] _p = new Vector2[Count];
    private readonly float[] _v = new float[Count];
    private CoreGame _g;
    private float _clock;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        var rnd = new RandomNumberGenerator { Seed = 4711 };
        for (var i = 0; i < Count; i++)
        {
            _p[i] = new Vector2(rnd.Randf(), rnd.Randf());
            _v[i] = rnd.RandfRange(0.7f, 1.3f);
        }
    }

    public void SetGame(CoreGame g) => _g = g;

    private WeatherEffect Effect => _g is null ? WeatherEffect.None : _g.WDef.Effect;

    public override void _Process(double delta)
    {
        _clock += (float)delta;
        if (Effect != WeatherEffect.None && IsVisibleInTree()) QueueRedraw();
    }

    public override void _Draw()
    {
        var w = Size.X;
        var h = Size.Y;
        switch (Effect)
        {
            case WeatherEffect.Rain:
                for (var i = 0; i < Count; i++)
                {
                    var y = Mathf.PosMod(_p[i].Y * h + _clock * 260f * _v[i], h + 20) - 10;
                    var x = Mathf.PosMod(_p[i].X * w - _clock * 60f * _v[i], w + 20) - 10;
                    DrawLine(new Vector2(x, y), new Vector2(x - 2.5f, y + 9), new Color(0.72f, 0.84f, 1f, 0.38f), 1f);
                }
                DrawRect(new Rect2(0, 0, w, h), new Color(0.2f, 0.28f, 0.45f, 0.10f));
                break;
            case WeatherEffect.Frost:
                for (var i = 0; i < Count / 2; i++)
                {
                    var y = Mathf.PosMod(_p[i].Y * h + _clock * 22f * _v[i], h + 10) - 5;
                    var x = Mathf.PosMod(_p[i].X * w + Mathf.Sin(_clock * 1.3f + i) * 10f, w);
                    var s = _v[i] > 1f ? 2f : 1f;
                    DrawRect(new Rect2(x, y, s, s), new Color(1f, 1f, 1f, 0.75f));
                }
                DrawRect(new Rect2(0, 0, w, h), new Color(0.6f, 0.8f, 1f, 0.07f));
                break;
            case WeatherEffect.Wind:
                for (var i = 0; i < Count / 6; i++)
                {
                    var x = Mathf.PosMod(_p[i].X * w - _clock * 320f * _v[i], w + 80) - 40;
                    var y = _p[i].Y * h + Mathf.Sin(_clock * 2f + i) * 3f;
                    DrawLine(new Vector2(x, y), new Vector2(x + 26f * _v[i], y), new Color(0.92f, 0.95f, 1f, 0.28f), 1f);
                }
                break;
            case WeatherEffect.Heat:
                DrawRect(new Rect2(0, 0, w, h), new Color(1f, 0.55f, 0.15f, 0.07f));
                for (var i = 0; i < 6; i++)
                {
                    var y = Mathf.PosMod(_p[i].Y * h - _clock * 14f, h);
                    for (var x = 0f; x < w; x += 6f)
                    {
                        var dy = Mathf.Sin(x * 0.08f + _clock * 3f + i) * 1.5f;
                        DrawRect(new Rect2(x, y + dy, 5f, 1f), new Color(1f, 0.85f, 0.6f, 0.10f));
                    }
                }
                break;
        }
    }
}
