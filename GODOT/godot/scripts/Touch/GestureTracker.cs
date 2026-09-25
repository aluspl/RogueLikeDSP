using System;
using Godot;

namespace LifeLike.Game.Touch;

/// <summary>
/// Rozpoznawanie gestów jednego palca z dotyku (na telefonie emulowanego jako lewy przycisk myszy) albo myszy
/// (--touch na komputerze): dotknięcie, przesunięcie o próg (krok w 4 kierunkach, zmiana kierunku w trakcie),
/// trzymanie po przesunięciu (powtarzanie kroków), przytrzymanie w miejscu, krótkie dotknięcie i puszczenie.
/// Czas liczy Process (Main._Process), więc przytrzymanie działa bez ruchu palca.
/// </summary>
public sealed class GestureTracker
{
    /// <summary>Próg przesunięcia w pikselach UI (punktach iOS).</summary>
    public const float SwipeDistance = 22f;
    public const float LongPressTime = 0.42f;
    public const float RepeatDelay = 0.3f, RepeatEvery = 0.16f;

    private bool _down, _moved, _long;
    private Vector2 _start, _pos, _anchor;
    private Vector2I _dir;
    private float _time, _repeat;

    /// <summary>Odbiorca gestów (Main przekazuje je ekranowi bieżącemu).</summary>
    public Action<Gesture> Emit;

    public bool IsDown => _down;

    /// <summary>Zdarzenie Godota; true = to był dotyk / lewy przycisk (połknięty).</summary>
    public bool Feed(InputEvent e)
    {
        switch (e)
        {
            case InputEventMouseButton { ButtonIndex: MouseButton.Left } mb:
                if (mb.Pressed) Begin(mb.Position);
                else End(mb.Position);
                return true;
            case InputEventMouseMotion mm when _down:
                Move(mm.Position);
                return true;
            default:
                return false;
        }
    }

    public void Process(double delta)
    {
        if (!_down) return;
        _time += (float)delta;
        if (!_moved && !_long && _time >= LongPressTime)
        {
            _long = true;
            Send(GestureKind.LongPress);
        }
        if (!_moved) return;
        _repeat -= (float)delta;
        if (_repeat > 0) return;
        _repeat = RepeatEvery;
        Send(GestureKind.SwipeRepeat);
    }

    /// <summary>Przerwanie (np. zmiana ekranu): bez Tap, tylko Up.</summary>
    public void Cancel()
    {
        if (!_down) return;
        _down = false;
        Send(GestureKind.Up);
    }

    private void Begin(Vector2 p)
    {
        _down = true;
        _moved = _long = false;
        _start = _pos = _anchor = p;
        _dir = Vector2I.Zero;
        _time = 0;
        Send(GestureKind.Down);
    }

    private void Move(Vector2 p)
    {
        _pos = p;
        Send(GestureKind.Drag);
        if (!_moved)
        {
            if (_long || p.DistanceTo(_start) < SwipeDistance) return;
            _moved = true;
            Step(p - _start, p);
            return;
        }
        var d = p - _anchor;
        if (d.Length() < SwipeDistance * 1.5f) return;
        var nd = Dir(d);
        _anchor = p;
        if (nd != _dir) Step(d, p); // palec zmienił kierunek: od razu krok w nowym
    }

    private void Step(Vector2 d, Vector2 p)
    {
        _dir = Dir(d);
        _anchor = p;
        _repeat = RepeatDelay;
        Send(GestureKind.Swipe);
    }

    private void End(Vector2 p)
    {
        if (!_down) return;
        _pos = p;
        _down = false;
        if (!_moved && !_long) Send(GestureKind.Tap);
        Send(GestureKind.Up);
    }

    private void Send(GestureKind k) => Emit?.Invoke(new Gesture(k, _pos, _start, _dir, _time));

    /// <summary>Kierunek przewagi osi: (±1, 0) albo (0, ±1).</summary>
    public static Vector2I Dir(Vector2 d) =>
        Mathf.Abs(d.X) >= Mathf.Abs(d.Y) ? new Vector2I(Math.Sign(d.X), 0) : new Vector2I(0, Math.Sign(d.Y));
}
