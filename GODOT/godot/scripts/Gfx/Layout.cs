using System;
using Godot;
using LifeLike.Game.Settings;

namespace LifeLike.Game.Gfx;

/// <summary>
/// Jedyne miejsce, które zna rozmiar ekranu i skale. Projekt: stretch „canvas_items”, aspect „expand”, skala
/// całkowita - piksel UI to ContentScale pikseli okna. Poziomo baza 640x360 (1280x720 -> 2, UI 640x360), pionowo
/// (telefon) baza 360x640 (iPhone 1290x2796 -> 3, UI 430x932: piksel UI = punkt iOS, 44 px = cel dotyku 44 pt).
/// Każde proporcje okna dają więcej miejsca w UI, bez pasów. Skale warstw są dobierane tak, żeby piksel grafiki był
/// zawsze całkowitą liczbą pikseli okna (ostre krawędzie), a układ liczył się od UiSize (kotwice), nie od stałych.
/// Tu też bezpieczny obszar (wycięcia ekranu, wyspa, pasek domowy) i tryb dotykowy.
/// </summary>
public static class Layout
{
    /// <summary>Rozmiar bazowy (project.godot: viewport_width / viewport_height) - krótszy i dłuższy bok.</summary>
    public const int BaseWidth = 640, BaseHeight = 360;

    /// <summary>Pola mapy widoczne na krótszym boku (GBA pokazuje 10 pól 16x16 na 160 px).</summary>
    private const float WorldTilesShort = 9f;

    /// <summary>Docelowa wielkość tekstu HUD względem pikseli UI (font 8x16 -> 24 px UI przy 1.5); pionowo 1.</summary>
    private const float HudTarget = 1.5f;

    /// <summary>Wysokość paska akcji dotykowych (bez bezpiecznego obszaru pod nim), piksele UI.</summary>
    public const float ActionBarHeight = 64f;

    /// <summary>Minimalny cel dotyku (44 pt na iOS = 44 px UI przy skali 3).</summary>
    public const float TouchTarget = 44f;

    private static Window _root;
    private static bool _applying;

    public static Vector2 UiSize { get; private set; } = new(BaseWidth, BaseHeight);

    /// <summary>Piksele okna na piksel UI (skala całkowita stretch).</summary>
    public static float ContentScale { get; private set; } = 2f;

    /// <summary>Bezpieczny obszar (bez wycięć ekranu i paska domowego) w pikselach UI.</summary>
    public static Rect2 SafeArea { get; private set; } = new(0, 0, BaseWidth, BaseHeight);

    /// <summary>Ekran pionowy (telefon): inne układy plansz, telefon na cały ekran, HUD w skali 1.</summary>
    public static bool Portrait => UiSize.Y > UiSize.X;

    /// <summary>Sterowanie dotykiem: pasek akcji, gesty, przyciski zamiast podpowiedzi klawiszy.</summary>
    public static bool Touch { get; set; }

    /// <summary>Symulowane wycięcia (góra, dół) w pikselach UI - --portrait na komputerze (iPhone: 59 / 34 pt).</summary>
    public static Vector2 SimulatedInsets { get; set; }

    public static float SafeTop => SafeArea.Position.Y;
    public static float SafeBottom => UiSize.Y - SafeArea.End.Y;
    public static float SafeLeft => SafeArea.Position.X;
    public static float SafeRight => UiSize.X - SafeArea.End.X;

    /// <summary>Dół wolnej części mapy: nad paskiem akcji (dotyk) i paskiem domowym, piksele UI.</summary>
    public static float BottomReserve => SafeBottom + (Touch ? ActionBarHeight : 0);

    /// <summary>Skala warstw HUD i banerów (większy tekst na mapie), wielokrotność 1/ContentScale.</summary>
    public static float HudScale => Snap(Portrait ? 1f : HudTarget);

    /// <summary>Powiększenie kamery mapy: ~9 pól na krótszym boku, piksel grafiki = całkowita liczba połówek piksela okna.</summary>
    public static float WorldZoom
    {
        get
        {
            var shortSide = Mathf.Min(UiSize.X, UiSize.Y);
            var longSide = Mathf.Max(UiSize.X, UiSize.Y);
            var tiles = Mathf.Min(shortSide / (WorldTilesShort * Assets.Cell), longSide / (16f * Assets.Cell));
            var real = Mathf.Max(1f, Mathf.Round(tiles * ContentScale * 2f) / 2f); // pikseli okna na piksel mapy
            return real / ContentScale;
        }
    }

    /// <summary>Zmiana rozmiaru okna / orientacji / skali tekstu.</summary>
    public static event Action Changed;

    /// <summary>Śledzi okno główne: rozmiar teraz i po każdej zmianie (także ustawienia „duży tekst”).</summary>
    public static void Track(Window root)
    {
        _root = root;
        Update(root);
        root.SizeChanged += () => Update(root);
        GameSettings.Changed += () => Update(root);
    }

    /// <summary>Skala najbliższa docelowej, dla której piksel UI to całkowita liczba pikseli okna.</summary>
    public static float Snap(float target) => Mathf.Max(1f, Mathf.Round(target * ContentScale)) / ContentScale;

    /// <summary>Punkt w pikselach UI (np. dotyk) w bezpiecznym obszarze.</summary>
    public static bool InSafe(Vector2 p) => SafeArea.HasPoint(p);

    /// <summary>
    /// Skala całkowita dla okna: poziomo od 640x360, pionowo od 360x640; „duży tekst” dokłada 1, jeśli UI zostaje
    /// co najmniej 270 px szerokie (pionowo) albo 300 px wysokie (poziomo).
    /// </summary>
    public static int ScaleFor(Vector2I win, bool large)
    {
        var portrait = win.Y > win.X;
        float bw = portrait ? BaseHeight : BaseWidth, bh = portrait ? BaseWidth : BaseHeight;
        var s = Math.Max(1, (int)Mathf.Floor(Mathf.Min(win.X / bw, win.Y / bh)));
        if (large && (portrait ? win.X / (s + 1) >= 270 : win.Y / (s + 1) >= 300)) s++;
        return s;
    }

    private static void Update(Window root)
    {
        if (_applying) return;
        _applying = true;
        try
        {
            ApplyStretch(root);
            Measure(root);
        }
        finally
        {
            _applying = false;
        }
    }

    /// <summary>Rozmiar bazowy = okno / skala: stretch „expand” daje wtedy dokładnie tę skalę całkowitą.</summary>
    private static void ApplyStretch(Window root)
    {
        var win = root.Size;
        if (win.X <= 0 || win.Y <= 0) return;
        var s = ScaleFor(win, GameSettings.LargeText);
        var want = new Vector2I(win.X / s, win.Y / s);
        if (root.ContentScaleSize != want) root.ContentScaleSize = want;
    }

    private static void Measure(Window root)
    {
        var size = root.GetVisibleRect().Size;
        var scale = size.X > 0 ? root.Size.X / size.X : 1f;
        var safeRect = new Rect2(0, 0, size);
        if (OS.HasFeature("mobile")) // wycięcia ekranu tylko na telefonie
        {
            var safe = DisplayServer.GetDisplaySafeArea();
            var win = DisplayServer.WindowGetPosition();
            if (safe.Size.X > 0)
                safeRect = new Rect2((Vector2)(safe.Position - win) / scale, (Vector2)safe.Size / scale).Intersection(safeRect);
        }
        else if (SimulatedInsets != Vector2.Zero && size.Y > size.X)
        {
            safeRect = new Rect2(0, SimulatedInsets.X, size.X, size.Y - SimulatedInsets.X - SimulatedInsets.Y);
        }
        if (size == UiSize && Mathf.IsEqualApprox(scale, ContentScale) && safeRect == SafeArea) return;
        UiSize = size;
        ContentScale = scale;
        SafeArea = safeRect;
        Changed?.Invoke();
    }

    /// <summary>Przelicz od nowa (np. po zmianie okna z linii poleceń).</summary>
    public static void Refresh()
    {
        if (_root is not null) Update(_root);
    }
}
