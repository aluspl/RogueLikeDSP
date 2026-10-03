using System.Linq;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;

namespace LifeLike.Game.Phone.Pages;

/// <summary>Jak grać (page_help na GBA): cel budowy, sterowanie z nazwami klawiszy z bieżącej mapy wejścia, pogoda, brygada, tryb inwestora,
/// ścieżki, materiały, codzienna budowa; strona 2 - mechaniki aktów i zachowania problemów, strona 3 - statystyki,
/// strona 4 - obrażenia broni (rozpiska #26, teksty damageHelp z game.json jak na GBA) (A: dalej).</summary>
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
        ("Okna", "dotknij = zaznacz; Wybierz: prawo, Wróć: lewo"),   // v0.21.51: ten sam układ w każdym oknie
    ];

    private const int Pages = 12; // v0.21.52 cz. b: strona 9 – inspektor i mistrzostwo; cz. c: strona 10 – drzewko, kolekcje, zadania, seria; cz. d: 11 – mapa kariery; v0.21.53: 12 – filtry ekranu

    /// <summary>v0.21.53: strona filtrów ekranu (indeks od 0).</summary>
    public const int FiltersPage = 11;


    /// <summary>v0.21.52 cz. c: gdzie drzewko, kolekcje, zadania dnia i seria dni.</summary>
    private static readonly string[] GoalsWhereLines =
    [
        "Profil > Koszty > Tab: Drzewko",
        "Profil > Katalog > Spacja: Kolekcje",
        "Profil > Odznaki > Zadania",
        "Seria: budowa dnia w kolejne dni",
    ];
    /// <summary>v0.21.52 cz. d: gdzie mapa kariery i co dają kontrakty.</summary>
    private static readonly string[] CareerWhereLines =
    [
        "Tytuł > Nowa budowa: Mapa kariery",
        "Wybór zawodu > Esc: wróć do mapy",
        "Profil > Odznaki > Tytuły: tytuły kontraktów",
        "Budowa dnia i tygodnia: zawsze Dom",
    ];
    private readonly GameData _d;
    private int _page;

    /// <summary>Strona 0-6 (sceny zrzutów); 4 = kombinacje stanów, premie i elity (v0.21.50 cz. 2); 5 = wydarzenia, ulepszenia, magazyn (cz. 3);
    /// 6 = podsumowanie budowy, wyzwanie tygodnia, fabuła (cz. 4); 7 = sekretne zlecenia (v0.21.51 cz. 2).</summary>
    public int Page
    {
        get => _page;
        set => _page = System.Math.Clamp(value, 0, Pages - 1);
    }

    public HelpPage(GameData d, bool fromTitle = false)
    {
        _d = d;
        FromTitle = fromTitle;
    }

    /// <summary>Z menu tytułu: przycisk „Samouczek jeszcze raz” (SELECT / Tab) - dymki menu od nowa.</summary>
    public bool FromTitle { get; }

    public override string Title => "Jak grać";
    public override string Sub => $"{_page + 1}/{Pages}";
    public override string Hint => ButtonNames.Localize(FromTitle ? "A: dalej  SELECT: samouczek menu" : "A: dalej");
    public override PageAction[] Actions => FromTitle
        ? [new("Samouczek jeszcze raz", GameAction.Select), new("Dalej", GameAction.A)]
        : [new("Dalej", GameAction.A)];

    /// <summary>A / Start: kolejna strona (akty i problemy, statystyki); na ostatniej - wyjście (HelpScreen).</summary>
    public override bool Input(InputCmd e)
    {
        if (!e.Is(GameAction.A | GameAction.Start) || _page >= Pages - 1) return false;
        _page++;
        return true;
    }

    public override void Draw(PhonePainter p)
    {
        if (_page == 1)
        {
            DrawList(p, "AKTY: MECHANIKI", ActLines, "PROBLEMY: ZACHOWANIA", BehaviorLines);
            return;
        }
        if (_page == 2)
        {
            DrawList(p, "STATYSTYKI", StatLines(_d), "GDZIE OPIS", WhereLines);
            return;
        }
        if (_page == 3)
        {
            DrawList(p, "OBRAŻENIA", _d.DamageHelpLines, "GDZIE ROZPISKA", DamageWhereLines);
            return;
        }
        if (_page == 4)
        {
            DrawList(p, "KOMBINACJE STANÓW", ComboLines(_d), "SKĄD STANY, PREMIE, ELITY", ComboWhereLines(_d));
            return;
        }
        if (_page == FiltersPage) // v0.21.53: filtry ekranu i tryby dla daltonistów
        {
            var fwidth = (int)(p.Right - 6 - p.Left - 12);
            var flines = p.F.Wrap(string.Join(" ", _d.FiltersHelpLines.Skip(2)), fwidth); // dwie pierwsze linie: gdzie na GBA
            var fc = p.Card(p.Section(p.Top, _d.FilterText("helpSection")), flines.Count);
            var fx = p.TextX(fc);
            for (var i = 0; i < flines.Count; i++) p.Text(fx, p.RowY(fc, i), flines[i], Ink.Dark, TextAlign.Left, fc.End.X - 6 - fx);
            var fc2 = p.Card(p.Section(fc.End.Y + 6, _d.FilterText("helpWhere")), _d.FilterWhereLines.Length);
            for (var i = 0; i < _d.FilterWhereLines.Length; i++)
            {
                if (i > 0) p.Divider(fc2, i);
                p.Text(fx, p.RowY(fc2, i), _d.FilterWhereLines[i], Ink.Dim, TextAlign.Left, fc2.End.X - 6 - fx);
            }
            return;
        }
        if (_page == 10) // v0.21.52 cz. d (#47): mapa kariery
        {
            var cwidth = (int)(p.Right - 6 - p.Left - 12);
            var clines = p.F.Wrap(string.Join(" ", _d.CareerHelpLines), cwidth);
            var cc = p.Card(p.Section(p.Top, "MAPA KARIERY"), clines.Count);
            var cx = p.TextX(cc);
            for (var i = 0; i < clines.Count; i++) p.Text(cx, p.RowY(cc, i), clines[i], Ink.Dark, TextAlign.Left, cc.End.X - 6 - cx);
            var cc2 = p.Card(p.Section(cc.End.Y + 6, "GDZIE"), CareerWhereLines.Length);
            for (var i = 0; i < CareerWhereLines.Length; i++)
            {
                if (i > 0) p.Divider(cc2, i);
                p.Text(cx, p.RowY(cc2, i), ButtonNames.Localize(CareerWhereLines[i]), Ink.Dim, TextAlign.Left, cc2.End.X - 6 - cx);
            }
            return;
        }
        if (_page == 9) // v0.21.52 cz. c: drzewko Szkoleń, kolekcje, zadania dnia i tygodnia, seria dni
        {
            var gwidth = (int)(p.Right - 6 - p.Left - 12);
            var glines = p.F.Wrap(string.Join(" ", _d.GoalsHelpLines), gwidth);
            var gc = p.Card(p.Section(p.Top, "DRZEWKO, KOLEKCJE, ZADANIA"), glines.Count);
            var gx = p.TextX(gc);
            for (var i = 0; i < glines.Count; i++) p.Text(gx, p.RowY(gc, i), glines[i], Ink.Dark, TextAlign.Left, gc.End.X - 6 - gx);
            var gc2 = p.Card(p.Section(gc.End.Y + 6, "GDZIE"), GoalsWhereLines.Length);
            for (var i = 0; i < GoalsWhereLines.Length; i++)
            {
                if (i > 0) p.Divider(gc2, i);
                p.Text(gx, p.RowY(gc2, i), ButtonNames.Localize(GoalsWhereLines[i]), Ink.Dim, TextAlign.Left, gc2.End.X - 6 - gx);
            }
            return;
        }
        if (_page == 8) // v0.21.52 cz. b: poziom inspektora (#44), mistrzostwo zawodu (#45), stopnie inwestora (#48)
        {
            var pwidth = (int)(p.Right - 6 - p.Left - 12);
            var plines = p.F.Wrap(string.Join(" ", _d.ProgressHelpLines), pwidth);
            var pc = p.Card(p.Section(p.Top, "INSPEKTOR I MISTRZOSTWO"), plines.Count);
            var px = p.TextX(pc);
            for (var i = 0; i < plines.Count; i++) p.Text(px, p.RowY(pc, i), plines[i], Ink.Dark, TextAlign.Left, pc.End.X - 6 - px);
            var pc2 = p.Card(p.Section(pc.End.Y + 6, "GDZIE"), ProgressWhereLines.Length);
            for (var i = 0; i < ProgressWhereLines.Length; i++)
            {
                if (i > 0) p.Divider(pc2, i);
                p.Text(px, p.RowY(pc2, i), ProgressWhereLines[i], Ink.Dim, TextAlign.Left, pc2.End.X - 6 - px);
            }
            return;
        }
        if (_page == 7)
        {
            var width = (int)(p.Right - 6 - p.Left - 12);
            var lines = p.F.Wrap(ButtonNames.Localize(string.Join(" ", SecretsHelp)), width);
            var sc = p.Card(p.Section(p.Top, "SEKRETNE ZLECENIA"), lines.Count);
            var sx = p.TextX(sc);
            for (var i = 0; i < lines.Count; i++) p.Text(sx, p.RowY(sc, i), lines[i], Ink.Dark, TextAlign.Left, sc.End.X - 6 - sx);
            var y2 = p.Section(sc.End.Y + 6, "GDZIE");
            var c2 = p.Card(y2, SecretWhereLines.Length);
            for (var i = 0; i < SecretWhereLines.Length; i++)
            {
                if (i > 0) p.Divider(c2, i);
                p.Text(sx, p.RowY(c2, i), SecretWhereLines[i], Ink.Dim, TextAlign.Left, c2.End.X - 6 - sx);
            }
            return;
        }
        if (_page == 6)
        {
            DrawList(p, "PO BUDOWIE, TYDZIEŃ, FABUŁA", _d.MetaHelpLines, "GDZIE", MetaWhereLines);
            return;
        }
        if (_page == 5)
        {
            // krótkie linie z GBA złączone i zawinięte na szerokość telefonu; jedna karta, żeby zmieścić się w poziomie
            var width = (int)(p.Right - 6 - p.Left - 12);
            var a = p.F.Wrap(ExtrasText(_d, 0), width);
            var b = p.F.Wrap("Magazyn: " + ExtrasText(_d, 6), width);
            var ec = p.Card(p.Section(p.Top, "WYDARZENIA, ULEPSZENIA, MAGAZYN"), a.Count + b.Count);
            var ex = p.TextX(ec);
            for (var i = 0; i < a.Count; i++) p.Text(ex, p.RowY(ec, i), a[i], Ink.Dark, TextAlign.Left, ec.End.X - 6 - ex);
            p.Divider(ec, a.Count);
            for (var i = 0; i < b.Count; i++) p.Text(ex, p.RowY(ec, a.Count + i), b[i], Ink.Dim, TextAlign.Left, ec.End.X - 6 - ex);
            return;
        }
        if (!PhoneView.Full) // telefon w poziomie: nowości w karcie na górze zamiast osobnej sekcji (brak miejsca)
        {
            var top = p.Card(p.Top, 1 + ShortNews.Length);
            p.Stripe(top, 0, Pal.Brand);
            p.Text(p.TextX(top), p.RowY(top, 0), "3 akty z bossami, z nagrody Akt 0", Ink.Dark, TextAlign.Left, top.End.X - 6 - p.TextX(top));
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
        p.Text(p.TextX(intro), p.RowY(intro, 0), "10 etapów w 3 aktach (+ Akt 0 z nagrody),", Ink.Dark);
        p.Text(p.TextX(intro), p.RowY(intro, 1), "każdy kończy boss. Schody = dalej.", Ink.Dark);
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

    private static void DrawList(PhonePainter p, string t1, string[] l1, string t2, string[] l2)
    {
        var y = p.Section(p.Top, t1);
        var card = p.Card(y, l1.Length);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        for (var i = 0; i < l1.Length; i++)
        {
            if (i > 0) p.Divider(card, i);
            p.Text(tx, p.RowY(card, i), l1[i], Ink.Dark, TextAlign.Left, right - tx);
        }
        y = p.Section(card.End.Y + 6, t2);
        var c2 = p.Card(y, l2.Length);
        for (var i = 0; i < l2.Length; i++)
        {
            if (i > 0) p.Divider(c2, i);
            p.Text(tx, p.RowY(c2, i), l2[i], Ink.Dim, TextAlign.Left, right - tx);
        }
    }

    /// <summary>Kombinacje z danych: „Mokry + prąd! Porażenie”, skutek, skutek na bohaterze.</summary>
    private static string[] ComboLines(GameData d)
    {
        var l = new System.Collections.Generic.List<string>();
        foreach (var c in d.Combos)
        {
            l.Add($"{c.Short} {c.Name}");
            l.Add((PhoneView.Full ? "  " : "") + c.Info);
            if (c.Hero.Length > 0 && PhoneView.Full) l.Add("  " + c.Hero);
        }
        return l.ToArray();
    }

    private static string[] ComboWhereLines(GameData d)
    {
        var l = new System.Collections.Generic.List<string>(d.ComboSources);
        if (!PhoneView.Full) return l.ToArray(); // telefon w poziomie: bez miejsca na premie i elity (krótko na stronie 1)
        l.Add("Po etapie: premia 1 z 3 (2+ znaczniki = synergia)");
        l.Add("Złota ramka: elita, lepsza nagroda");
        return l.ToArray();
    }

    /// <summary>Tekst extrasHelp (wspólny z GBA): od 0 wydarzenia i ulepszanie narzędzia, od 6 magazyn.</summary>
    private static string ExtrasText(GameData d, int from)
    {
        var l = new System.Collections.Generic.List<string>();
        for (var i = from; i < from + 6 && i < d.ExtrasHelpLines.Length; i++) l.Add(d.ExtrasHelpLines[i]);
        return string.Join(" ", l);
    }

    private static string[] _secretsHelp;
    /// <summary>Tekst secretsHelp z game.json (wspólny z GBA), wczytany raz.</summary>
    private static string[] SecretsHelp => _secretsHelp ??= Session.GodotDataSource.LoadStrings("secretsHelp");

    private static readonly string[] SecretWhereLines =
    [
        "Profil > Odznaki > Sekrety",
        "Nowe zawody: wybór zawodu",
        "Kask w paski: Tryb inwestora",
    ];

    private static readonly string[] ProgressWhereLines =
    [
        "Tytuł: pasek inspektora",
        "Profil > Odznaki > Inspektor",
        "Wybór zawodu: Mistrz N, Wygląd",
        "Tryb inwestora: kolejny stopień",
    ];

    private static readonly string[] MetaWhereLines =
    [
        "Podsumowanie: po końcu budowy",
        "Tytuł: Wyzwanie tygodnia",
        "Profil > Osiedle: Wiadomości",
    ];

    private static readonly string[] ActLines =
    [
        "0: pieczątki - 3 dokumenty = schody",
        "I: błoto - wejście = tura",
        "II: porywy wiatru spychają",
        "III: pył - widzisz mniej",
        "Kładka działa też na błoto",
    ];

    private static readonly string[] BehaviorLines =
    [
        "Strzelają z 2-3 pól, dzielą się",
        "Łatają sąsiadów, rosną z czasem",
        "Wybuch: zejdź z czerwonych pól!",
        "Uciekają albo stoją jak mur",
        "Odpychają, wracają raz",
    ];

    /// <summary>Wzory statystyk z danych (StatHelp jak stat_rule w core.h).</summary>
    private static string[] StatLines(GameData d) =>
    [
        "SIŁ/ZRĘ/INT: +1 obr. co 2 pkt",
        "(liczy się tylko statystyka broni)",
        StatHelp.Rule(d, new Message(), StatKind.Def).Text,
        StatHelp.Rule(d, new Message(), StatKind.Luck).Text,
        StatHelp.Rule(d, new Message(), StatKind.Luck, 1).Text,
        StatHelp.Rule(d, new Message(), StatKind.Luck, 2).Text,
        StatHelp.Rule(d, new Message(), StatKind.Hp).Text,
    ];

    private static string[] WhereLines => Layout.Touch
        ? ["Wybór zawodu: dotknij statystyki", "Telefon > Start > Opis statystyk"]
        : ["Wybór zawodu: I albo mysz", "Telefon > Start > Spacja"];

    private static string[] DamageWhereLines => Layout.Touch
        ? ["Wybór zawodu: dotknij narzędzia", "Telefon > Sprzęt > dotknij narzędzia", "Przytrzymaj problem: obrażenia w obie strony"]
        : ["Wybór zawodu: mysz na narzędziu, I", "Telefon > Sprzęt > I", "Trzymaj Z: obrażenia w obie strony"];

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
    private static readonly string[] ShortNews =
        ["Okna: A bierze, B wraca (wszędzie)", "Respekt i nagrody: profil > Koszty", "Codzienna budowa: menu tytułu", "Pogoda, brygada: telefon > Sprzęt"];

    /// <summary>Pogoda, brygada i tryb inwestora (v0.21.47), ścieżki, materiały i codzienna budowa (v0.21.48), Respekt i nagrody (v0.21.49).</summary>
    private static string[] News => Layout.Touch
        ?
        [
            "Po etapie: premia 1 z 3, potem ścieżka etapu", "Złota ramka: elita (cecha, lepsza nagroda)", "Materiały z problemów: Hurtownia i naprawy (Brygada)",
            "Załataj (drewno): deski przed problemem", "Kładka (stal): kałuże bez poślizgu", "Codzienna budowa: menu tytułu, jedna na dzień",
            "Pogoda dnia: ikona w HUD, skutek w Zadaniach", "Brygada: Telefon > Sprzęt > Brygada (raz na etap)",
            "Po wygranej: Tryb inwestora na wyborze zawodu",
            "Respekt za każdy etap zostaje po porażce: profil > Koszty > Respekt",
            "Każda wygrana: nagroda za odbiór (sprzęt, narzędzia, zawody)",
        ]
        :
        [
            "Okna: strzałki zaznaczają, A (Spacja/Enter) bierze, B (Z/Esc) wraca", "Po etapie: premia 1 z 3, potem ścieżka etapu",
            "Złota ramka: elita (cecha, lepsza nagroda)", "Materiały z problemów: Hurtownia i naprawy (Brygada)",
            "Załataj (drewno): deski przed problemem", "Kładka (stal): kałuże bez poślizgu", "Codzienna budowa: menu tytułu, jedna na dzień",
            "Pogoda dnia: ikona w HUD, skutek w Zadaniach", "Brygada: Enter, Spacja (albo telefon > Sprzęt)",
            "Po wygranej: Tab na wyborze zawodu = tryb inwestora",
            "Respekt za każdy etap zostaje po porażce: profil (P) > Koszty > Tab",
            "Każda wygrana: nagroda za odbiór (sprzęt, narzędzia, zawody)",
        ];
}
