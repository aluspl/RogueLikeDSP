using Godot;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.World;

/// <summary>
/// Kamera mapy: przybliżenie z Layout (~9 pól w pionie jak na GBA, ostre piksele), podąża za bohaterem
/// z wygładzaniem, trzyma go w środku wolnego pola między paskami HUD, wstrząs po obrażeniach, podgląd
/// odkrytej części etapu dopasowany do ekranu.
/// </summary>
public partial class WorldCamera : Camera2D
{
    private const int Cell = WorldView.Cell;
    /// <summary>Wysokość górnego paska HUD i dolnego pasa podpowiedzi w jednostkach HUD (HudTop / HudLog).</summary>
    private const float HudTopH = 39, HudBottomH = 20;
    private float _shake;

    public bool Overview { get; private set; }

    public override void _Ready()
    {
        PositionSmoothingEnabled = true;
        PositionSmoothingSpeed = 9f;
        SetLimits(false);
        ApplyZoom();
        Layout.Changed += ApplyZoom;
    }

    public override void _ExitTree() => Layout.Changed -= ApplyZoom;

    /// <summary>Granice kamery: na mapie przy krawędzi etapu, w podglądzie wyłączone (widok dopasowany do odkrytej części).</summary>
    private void SetLimits(bool free)
    {
        const int far = 100000;
        LimitLeft = free ? -far : -Cell;
        LimitTop = free ? -far : -Cell * 3;
        LimitRight = free ? far : Level.W * Cell + Cell;
        LimitBottom = free ? far : Level.H * Cell + Cell * 2;
    }

    private void ApplyZoom()
    {
        if (!Overview) Zoom = Vector2.One * Layout.WorldZoom;
    }

    /// <summary>Przesunięcie, które stawia bohatera w środku pola między górnym paskiem HUD a dolnym pasem.</summary>
    private Vector2 HudBias => new(0, -(HudTopH - HudBottomH) * Layout.HudScale / 2f / Zoom.Y);

    /// <summary>Od razu na pozycji (nowy etap).</summary>
    public void SnapTo(Vector2 pos)
    {
        Position = pos + HudBias;
        ResetSmoothing();
    }

    public void Shake(float seconds) => _shake = seconds;

    /// <summary>Podgląd etapu (L na GBA): cała odkryta część placu dopasowana do ekranu.</summary>
    public void ToggleOverview(CoreGame g)
    {
        Overview = !Overview;
        PositionSmoothingEnabled = !Overview;
        SetLimits(Overview);
        if (!Overview)
        {
            ApplyZoom();
            return;
        }
        var r = ExploredBounds(g);
        var room = Layout.UiSize - new Vector2(24, (HudTopH + HudBottomH) * Layout.HudScale + 16);
        var z = Mathf.Min(room.X / r.Size.X, room.Y / r.Size.Y);
        Zoom = Vector2.One * Mathf.Clamp(z, 0.2f, Layout.WorldZoom);
        Position = r.GetCenter() + HudBias;
        Offset = Vector2.Zero;
    }

    /// <summary>Prostokąt odkrytych pól (w pikselach mapy) z marginesem jednego pola.</summary>
    private static Rect2 ExploredBounds(CoreGame g)
    {
        int x0 = Level.W, y0 = Level.H, x1 = -1, y1 = -1;
        for (var y = 0; y < Level.H; y++)
        {
            for (var x = 0; x < Level.W; x++)
            {
                if (!g.Explored(x, y)) continue;
                x0 = Mathf.Min(x0, x);
                y0 = Mathf.Min(y0, y);
                x1 = Mathf.Max(x1, x);
                y1 = Mathf.Max(y1, y);
            }
        }
        if (x1 < 0) return new Rect2(0, 0, Level.W * Cell, Level.H * Cell);
        return new Rect2((x0 - 1) * Cell, (y0 - 1) * Cell, (x1 - x0 + 3) * Cell, (y1 - y0 + 3) * Cell);
    }

    /// <summary>Co klatkę: za bohaterem (poza podglądem) i wstrząs.</summary>
    public void Follow(Vector2 hero, double delta)
    {
        if (Overview) return;
        Position = hero + HudBias;
        if (_shake > 0)
        {
            _shake = Mathf.Max(0, _shake - (float)delta);
            Offset = new Vector2(((int)(_shake * 60) & 1) == 1 ? 4 : -4, 0);
        }
        else
        {
            Offset = Vector2.Zero;
        }
    }
}
