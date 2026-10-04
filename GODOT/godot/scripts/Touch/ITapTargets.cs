using System.Collections.Generic;
using Godot;

namespace LifeLike.Game.Touch;

/// <summary>
/// Widok z elementami do dotknięcia (przyciski, wiersze list, zakładki) – prostokąty we własnych współrzędnych
/// rysowania. Używa ich test małpy (Debug/MonkeyTest), żeby stukać częściej w przyciski niż w puste miejsca.
/// </summary>
public interface ITapTargets
{
    void TapTargets(List<Rect2> into);
}
