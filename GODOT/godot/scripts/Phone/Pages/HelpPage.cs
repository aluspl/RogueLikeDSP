using LifeLike.Game.Gfx;
using LifeLike.Game.Input;

namespace LifeLike.Game.Phone.Pages;

/// <summary>Jak grać (page_help na GBA): cel budowy, sterowanie z nazwami klawiszy z bieżącej mapy wejścia, pogoda, brygada, tryb inwestora.</summary>
public sealed class HelpPage : PhonePage
{
    private static readonly (string Key, string What)[] Controls =
    [
        ("Strzałki", "ruch i atak wręcz (też WSAD)"),
        ("A", "atak (trzymaj: celuj)"),
        ("B", "czekaj (trzymaj: podgląd)"),
        ("R", "moc zawodu"),
        ("L", "mapa etapu"),
        ("START", "menu akcji (termos)"),
        ("SELECT", "telefon"),
    ];

    private static readonly (string Key, string What)[] TouchControls =
    [
        ("Przesuń palec", "krok (trzymaj: dalej)"),
        ("Dotknij pola", "idź tam krok po kroku"),
        ("Dotknij problemu", "atak albo podejdź"),
        ("Przytrzymaj go", "karta problemu"),
        ("Atak", "najbliższy (trzymaj: celuj)"),
        ("Moc", "moc zawodu"),
        ("Termos", "kawa leczy (tura)"),
        ("Czekaj", "tura (trzymaj: podgląd)"),
        ("Telefon", "aplikacja (trzymaj: mapa)"),
    ];

    public override string Title => "Jak grać";
    public override string Sub => "Kierownik budowy";
    public override string Hint => ButtonNames.Localize("A: dalej");
    public override PageAction[] Actions => [new("Dalej", GameAction.A)];

    public override void Draw(PhonePainter p)
    {
        if (!PhoneView.Full) // telefon w poziomie: nowości w karcie na górze zamiast osobnej sekcji (brak miejsca)
        {
            var top = p.Card(p.Top, 1 + ShortNews.Length);
            p.Stripe(top, 0, Pal.Brand);
            p.Text(p.TextX(top), p.RowY(top, 0), "8 etapów, 3 akty z bossami", Ink.Dark, TextAlign.Left, top.End.X - 6 - p.TextX(top));
            for (var i = 0; i < ShortNews.Length; i++)
            {
                p.Divider(top, i + 1);
                p.Text(p.TextX(top), p.RowY(top, i + 1), ShortNews[i], Ink.Dark, TextAlign.Left, top.End.X - 6 - p.TextX(top));
            }
            DrawControls(p, p.Section(top.End.Y + 6, "STEROWANIE"));
            return;
        }
        var intro = p.Card(p.Top, 2);
        p.Stripe(intro, 0, Pal.Brand);
        p.Stripe(intro, 1, Pal.Brand);
        p.Text(p.TextX(intro), p.RowY(intro, 0), "8 etapów w 3 aktach, każdy", Ink.Dark);
        p.Text(p.TextX(intro), p.RowY(intro, 1), "kończy boss. Schody = dalej.", Ink.Dark);
        var card = DrawControls(p, p.Section(intro.End.Y + 6, "STEROWANIE"));
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        var y = p.Section(card.End.Y + 6, "NA PLACU");
        var news = p.Card(y, News.Length);
        for (var i = 0; i < News.Length; i++)
        {
            if (i > 0) p.Divider(news, i);
            p.Text(tx, p.RowY(news, i), News[i], Ink.Dark, TextAlign.Left, right - tx);
        }
    }

    private static Godot.Rect2 DrawControls(PhonePainter p, float y)
    {
        var controls = Layout.Touch ? TouchControls : Controls;
        var card = p.Card(y, controls.Length);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        for (var i = 0; i < controls.Length; i++)
        {
            var (key, what) = controls[i];
            var row = p.RowY(card, i);
            if (i > 0) p.Divider(card, i);
            var pw = p.Pill(right, row, ButtonNames.Localize(key), PillKind.Group);
            p.Text(tx, row, what, Ink.Dark, TextAlign.Left, right - pw - 4 - tx);
        }
        return card;
    }

    /// <summary>Nowości w wąskim telefonie (poziomo): krótko.</summary>
    private static readonly string[] ShortNews = ["Pogoda dnia: ikona w HUD", "Brygada: Enter i Spacja", "Po wygranej: Tab = inwestor"];

    /// <summary>Pogoda dnia, brygada i tryb inwestora (v0.21.47).</summary>
    private static string[] News => Layout.Touch
        ? ["Pogoda dnia: ikona w HUD, skutek w Zadaniach", "Brygada: Telefon > Sprzęt > Brygada (raz na etap)", "Po wygranej: Tryb inwestora na wyborze zawodu"]
        : ["Pogoda dnia: ikona w HUD, skutek w Zadaniach", "Brygada: Enter, Spacja (albo telefon > Sprzęt)", "Po wygranej: Tab na wyborze zawodu = tryb inwestora"];
}
