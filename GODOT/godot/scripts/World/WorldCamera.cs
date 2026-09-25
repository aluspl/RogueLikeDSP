using Godot;
using LifeLike.Core;

namespace LifeLike.Game.World;

/// <summary>Kamera mapy: podąża za bohaterem z wygładzaniem, wstrząs po obrażeniach, podgląd całego etapu.</summary>
public partial class WorldCamera : Camera2D
{
    private const int Cell = WorldView.Cell;
    private float _shake;

    public bool Overview { get; private set; }

    public override void _Ready()
    {
        PositionSmoothingEnabled = true;
        PositionSmoothingSpeed = 9f;
        LimitLeft = -Cell;
        LimitTop = -Cell * 2;
        LimitRight = Level.W * Cell + Cell;
        LimitBottom = Level.H * Cell + Cell * 2;
    }

    /// <summary>Od razu na pozycji (nowy etap).</summary>
    public void SnapTo(Vector2 pos)
    {
        Position = pos;
        ResetSmoothing();
    }

    public void Shake(float seconds) => _shake = seconds;

    /// <summary>Podgląd całego etapu (L na GBA): kamera oddala się 4x.</summary>
    public void ToggleOverview()
    {
        Overview = !Overview;
        Zoom = Overview ? new Vector2(0.25f, 0.25f) : Vector2.One;
        PositionSmoothingEnabled = !Overview;
        if (Overview) Position = new Vector2(Level.W * Cell / 2, Level.H * Cell / 2);
    }

    /// <summary>Co klatkę: za bohaterem (poza podglądem) i wstrząs.</summary>
    public void Follow(Vector2 hero, double delta)
    {
        if (Overview) return;
        Position = hero;
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
