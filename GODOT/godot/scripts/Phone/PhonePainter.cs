using Godot;
using LifeLike.Game.Gfx;

namespace LifeLike.Game.Phone;

/// <summary>
/// Rysowanie treści aplikacji w telefonie: karty, wiersze list z paskiem statusu po lewej, pastylki statusów,
/// paski postępu, nagłówki sekcji (odpowiedniki phone_canvas / phone_text / phone_pill / stripe z GBA).
/// Wszystko w pikselach ekranu 640x360.
/// </summary>
public sealed class PhonePainter
{
    public const int RowH = 18;
    public const int Pad = 6;

    public readonly CanvasItem C;
    public readonly Rect2 Content;
    public readonly PixelFont F = PixelFont.I;

    public PhonePainter(CanvasItem c, Rect2 content)
    {
        C = c;
        Content = content;
    }

    public float Left => Content.Position.X + 4;
    public float Right => Content.End.X - 4;
    public float Width => Right - Left;
    public float Top => Content.Position.Y;
    public float Bottom => Content.End.Y;

    /// <summary>Biała karta z zaokrąglonymi rogami na `rows` wierszy od y; zwraca prostokąt karty.</summary>
    public Rect2 Card(float y, int rows) => CardH(y, rows * RowH + 8);

    public Rect2 CardH(float y, float h)
    {
        var r = new Rect2(Left, y, Width, h);
        C.DrawStyleBox(Ui.Box(Pal.Card, 6, Pal.Border), r);
        return r;
    }

    public float RowY(Rect2 card, int r) => card.Position.Y + 4 + r * RowH;

    /// <summary>Tekst wiersza (lewa krawędź za paskiem statusu).</summary>
    public float TextX(Rect2 card) => card.Position.X + 12;

    /// <summary>Pasek statusu przy lewej krawędzi wiersza (stripe na GBA).</summary>
    public void Stripe(Rect2 card, int r, Color c)
    {
        C.DrawRect(new Rect2(card.Position.X + 4, RowY(card, r) + 2, 3, RowH - 4), c);
    }

    /// <summary>Zaznaczony wiersz listy: jasny fiolet pod całą szerokością + pasek marki.</summary>
    public void Selected(Rect2 card, int r)
    {
        C.DrawRect(new Rect2(card.Position.X + 2, RowY(card, r), card.Size.X - 4, RowH), Pal.Group);
        Stripe(card, r, Pal.Brand);
    }

    public void Divider(Rect2 card, int r)
    {
        C.DrawRect(new Rect2(card.Position.X + 10, RowY(card, r) - 1, card.Size.X - 20, 1), Pal.Bg);
    }

    /// <summary>Tekst; y = górna krawędź wiersza 18 px (font 16 px wyśrodkowany). maxW &gt; 0 przycina.</summary>
    public float Text(float x, float y, string s, Ink ink, TextAlign a = TextAlign.Left, float maxW = 0)
    {
        if (maxW > 0) s = F.Fit(s, (int)maxW);
        return F.Draw(C, new Vector2(x, y + 1), s, ink, a);
    }

    /// <summary>Pogrubiony tekst (dwa przebiegi przesunięte o piksel) - tytuły jak w aplikacji.</summary>
    public void Bold(float x, float y, string s, Ink ink, TextAlign a = TextAlign.Left) => F.Draw(C, new Vector2(x, y), s, ink, a, 1, true);

    public static (Color Bg, Ink Ink) PillColors(PillKind k) => k switch
    {
        PillKind.Prog => (Pal.ProgBg, Ink.Prog),
        PillKind.Late => (Pal.LateBg, Ink.Late),
        PillKind.Done => (Pal.DoneBg, Ink.Done),
        PillKind.Brand => (Pal.Brand, Ink.White),
        PillKind.Group => (Pal.PillGroup, Ink.Brand),
        _ => (Pal.Border, Ink.Dim),
    };

    /// <summary>Pastylka przyklejona prawą krawędzią do `right`, w wierszu o górze y; zwraca jej szerokość.</summary>
    public float Pill(float right, float y, string s, PillKind k)
    {
        var (bg, ink) = PillColors(k);
        s = F.Normalize(s);
        var w = F.Measure(s) + 10;
        var r = new Rect2(right - w, y + 2, w, RowH - 4);
        C.DrawStyleBox(Ui.Box(bg, 7), r);
        F.Draw(C, new Vector2(r.Position.X + w / 2f, y), s, ink, TextAlign.Center);
        return w;
    }

    /// <summary>Pasek postępu (value z max) w kolorze c na torze w kolorze grupy.</summary>
    public void Bar(float x, float y, float w, int value, int max, Color c, float h = 6)
    {
        var r = new Rect2(x, y + (RowH - h) / 2, w, h);
        C.DrawStyleBox(Ui.Box(Pal.Group, 3), r);
        var fw = max > 0 ? Mathf.Round(w * Mathf.Clamp(value / (float)max, 0f, 1f)) : 0;
        if (fw >= 2) C.DrawStyleBox(Ui.Box(c, 3), new Rect2(r.Position, new Vector2(fw, h)));
    }

    /// <summary>Nagłówek sekcji nad kartą (wersaliki w kolorze textDim).</summary>
    public float Section(float y, string title, string right = "")
    {
        Text(Left + 4, y, title, Ink.Dim);
        if (right.Length > 0) Text(Right - 4, y, right, Ink.Brand, TextAlign.Right);
        return y + RowH;
    }

    public void Icon(Texture2D tex, int frame, int size, Vector2 topLeft, int scale = 1) => Assets.DrawFrame(C, tex, frame, size, topLeft, scale);

    public void IconTinted(Texture2D tex, int frame, int size, Vector2 topLeft, int scale, Color mod) => Assets.DrawFrame(C, tex, frame, size, topLeft, scale, mod);
}
