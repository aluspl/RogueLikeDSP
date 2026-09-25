using LifeLike.Game.Gfx;
using LifeLike.Game.Input;

namespace LifeLike.Game.Phone.Pages;

/// <summary>Jak grać (page_help na GBA): cel budowy i sterowanie z nazwami klawiszy z bieżącej mapy wejścia.</summary>
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
        var intro = p.Card(p.Top, 2);
        p.Stripe(intro, 0, Pal.Brand);
        p.Stripe(intro, 1, Pal.Brand);
        p.Text(p.TextX(intro), p.RowY(intro, 0), "8 etapów w 3 aktach, każdy", Ink.Dark);
        p.Text(p.TextX(intro), p.RowY(intro, 1), "kończy boss. Schody = dalej.", Ink.Dark);
        var y = p.Section(intro.End.Y + 6, "STEROWANIE");
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
    }
}
