namespace LifeLike.Game.Touch;

/// <summary>
/// Rodzaje gestów jednego palca: Down (dotknięcie), Drag (ruch), Swipe (przesunięcie o próg - krok w kierunku),
/// SwipeRepeat (palec dalej trzymany po przesunięciu - kolejny krok), LongPress (przytrzymanie w miejscu),
/// Tap (krótkie dotknięcie bez ruchu), Up (puszczenie - zawsze na końcu).
/// </summary>
public enum GestureKind
{
    Down,
    Drag,
    Swipe,
    SwipeRepeat,
    LongPress,
    Tap,
    Up,
}
