using System;
using Godot;

namespace LifeLike.Game.Gfx;

/// <summary>
/// Jedyne miejsce, które zna rozmiar ekranu i skale. Projekt: stretch „canvas_items”, aspect „expand”, skala
/// całkowita - piksel UI to ContentScale pikseli okna (1280x720 -> 2, UI 640x360), a inne proporcje (np. telefon
/// pionowo) po prostu dają więcej miejsca w UI. Skale warstw są dobierane tak, żeby piksel grafiki był zawsze
/// całkowitą liczbą pikseli okna (ostre krawędzie), a układ liczył się od UiSize (kotwice), nie od stałych.
/// </summary>
public static class Layout
{
    /// <summary>Rozmiar bazowy (project.godot: viewport_width / viewport_height).</summary>
    public const int BaseWidth = 640, BaseHeight = 360;

    /// <summary>Pola mapy widoczne w pionie (GBA pokazuje 10 pól 16x16 na 160 px).</summary>
    private const float WorldTilesTall = 9f;

    /// <summary>Docelowa wielkość tekstu HUD względem pikseli UI (font 8x16 -> 24 px UI przy 1.5).</summary>
    private const float HudTarget = 1.5f;

    public static Vector2 UiSize { get; private set; } = new(BaseWidth, BaseHeight);

    /// <summary>Piksele okna na piksel UI (skala całkowita stretch).</summary>
    public static float ContentScale { get; private set; } = 2f;

    /// <summary>Marginesy bezpiecznego obszaru (wycięcia ekranu telefonu) w pikselach UI: lewo, góra, prawo, dół.</summary>
    public static Rect2 SafeArea { get; private set; } = new(0, 0, BaseWidth, BaseHeight);

    /// <summary>Skala warstw HUD i banerów (większy tekst na mapie), wielokrotność 1/ContentScale.</summary>
    public static float HudScale => Snap(HudTarget);

    /// <summary>Powiększenie kamery mapy: ~9 pól w pionie, piksel grafiki = całkowita liczba połówek piksela okna.</summary>
    public static float WorldZoom
    {
        get
        {
            var tiles = Mathf.Min(UiSize.Y / (WorldTilesTall * Assets.Cell), UiSize.X / (16f * Assets.Cell));
            var real = Mathf.Max(1f, Mathf.Round(tiles * ContentScale * 2f) / 2f); // pikseli okna na piksel mapy
            return real / ContentScale;
        }
    }

    /// <summary>Zmiana rozmiaru okna / orientacji.</summary>
    public static event Action Changed;

    /// <summary>Śledzi okno główne: rozmiar teraz i po każdej zmianie.</summary>
    public static void Track(Window root)
    {
        Update(root);
        root.SizeChanged += () => Update(root);
    }

    /// <summary>Skala najbliższa docelowej, dla której piksel UI to całkowita liczba pikseli okna.</summary>
    public static float Snap(float target) => Mathf.Max(1f, Mathf.Round(target * ContentScale)) / ContentScale;

    private static void Update(Window root)
    {
        var size = root.GetVisibleRect().Size;
        var scale = size.X > 0 ? root.Size.X / size.X : 1f;
        var safe = DisplayServer.GetDisplaySafeArea();
        var win = DisplayServer.WindowGetPosition();
        var safeRect = new Rect2(0, 0, size);
        if (safe.Size.X > 0 && OS.HasFeature("mobile")) // wycięcia ekranu tylko na telefonie
            safeRect = new Rect2((Vector2)(safe.Position - win) / scale, (Vector2)safe.Size / scale).Intersection(safeRect);
        if (size == UiSize && Mathf.IsEqualApprox(scale, ContentScale) && safeRect == SafeArea) return;
        UiSize = size;
        ContentScale = scale;
        SafeArea = safeRect;
        Changed?.Invoke();
    }
}
