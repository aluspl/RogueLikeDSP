using System;
using Godot;

namespace LifeLike.Game.Gfx;

/// <summary>
/// Jedyne miejsce, które zna rozmiar ekranu: rozmiar interfejsu w pikselach UI (widoczny prostokąt widoku po
/// skalowaniu okna). Warstwy i widoki układają się względem UiSize (kotwice), nie stałych 640x360.
/// </summary>
public static class Layout
{
    /// <summary>Rozmiar bazowy (project.godot: viewport_width / viewport_height).</summary>
    public const int BaseWidth = 640, BaseHeight = 360;

    public static Vector2 UiSize { get; private set; } = new(BaseWidth, BaseHeight);

    /// <summary>Zmiana rozmiaru okna / orientacji.</summary>
    public static event Action Changed;

    /// <summary>Śledzi widok główny: rozmiar teraz i po każdej zmianie okna.</summary>
    public static void Track(Viewport vp)
    {
        Update(vp.GetVisibleRect().Size);
        vp.SizeChanged += () => Update(vp.GetVisibleRect().Size);
    }

    private static void Update(Vector2 size)
    {
        if (size == UiSize) return;
        UiSize = size;
        Changed?.Invoke();
    }
}
