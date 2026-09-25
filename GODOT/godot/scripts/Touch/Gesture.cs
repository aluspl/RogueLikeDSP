using Godot;

namespace LifeLike.Game.Touch;

/// <summary>Gest w pikselach UI: rodzaj, bieżąca pozycja, początek dotyku, kierunek przesunięcia (4 kierunki), czas trzymania.</summary>
public readonly struct Gesture
{
    public readonly GestureKind Kind;
    public readonly Vector2 Pos;
    public readonly Vector2 Start;
    public readonly Vector2I Dir;
    public readonly float Held;

    public Gesture(GestureKind kind, Vector2 pos, Vector2 start, Vector2I dir, float held)
    {
        Kind = kind;
        Pos = pos;
        Start = start;
        Dir = dir;
        Held = held;
    }

    public bool IsStep => Kind is GestureKind.Swipe or GestureKind.SwipeRepeat;

    public override string ToString() => $"{Kind} {Pos} dir {Dir}";
}
