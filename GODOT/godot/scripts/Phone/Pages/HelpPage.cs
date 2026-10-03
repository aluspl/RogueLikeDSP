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
    private static (string Key, string What)[] Controls =>
    [
        (Loc.T("strzalki"), Loc.T("ruch_i_atak_wrecz_tez_wsad")),
        ("A", Loc.T("atak_trzymaj_celuj")),
        ("B", Loc.T("czekaj_trzymaj_podglad")),
        ("R", Loc.T("moc_zawodu")),
        ("L", Loc.T("mapa_etapu")),
        ("START", Loc.T("menu_akcji_termos")),
        ("SELECT", Loc.T("telefon_2")),
    ];

    private static (string Key, string What)[] TouchControls =>
    [
        (Loc.T("przesun_palec"), Loc.T("krok_trzymaj_dalej")),
        (Loc.T("dotknij_pola"), Loc.T("idz_tam_krok_po_kroku")),
        (Loc.T("dotknij_problemu"), Loc.T("atak_albo_podejdz")),
        (Loc.T("przytrzymaj_go"), Loc.T("karta_problemu")),
        (Loc.T("atak"), Loc.T("najblizszy_trzymaj_celuj")),
        (Loc.T("moc_6"), Loc.T("moc_zawodu")),
        (Loc.T("termos_3"), Loc.T("kawa_leczy_tura")),
        (Loc.T("czekaj"), Loc.T("tura_trzymaj_podglad")),
        (Loc.T("telefon"), Loc.T("aplikacja_trzymaj_mapa")),
        (Loc.T("okna"), Loc.T("dotknij_zaznacz_wybierz_prawo")),   // v0.21.51: ten sam układ w każdym oknie
    ];

    private const int Pages = 13; // v0.21.52 cz. b: strona 9 – inspektor i mistrzostwo; cz. c: strona 10 – drzewko, kolekcje, zadania, seria; cz. d: 11 – mapa kariery; v0.21.53: 12 – filtry ekranu; v0.21.54: 13 – widok i światło

    /// <summary>v0.21.54: strona „Widok i światło” (indeks od 0).</summary>
    public const int ViewPage = 12;

    private static string[] ViewWhereLines => [Loc.T("ustawienia_widok_mapy"), Loc.T("ustawienia_efekty_swietlne")];

    /// <summary>v0.21.53: strona filtrów ekranu (indeks od 0).</summary>
    public const int FiltersPage = 11;


    /// <summary>v0.21.52 cz. c: gdzie drzewko, kolekcje, zadania dnia i seria dni.</summary>
    private static string[] GoalsWhereLines =>
    [
        Loc.T("profil_koszty_tab_drzewko"),
        Loc.T("profil_katalog_spacja_kolekcje"),
        Loc.T("profil_odznaki_zadania"),
        Loc.T("seria_budowa_dnia_w_kolejne"),
    ];
    /// <summary>v0.21.52 cz. d: gdzie mapa kariery i co dają kontrakty.</summary>
    private static string[] CareerWhereLines =>
    [
        Loc.T("tytul_nowa_budowa_mapa_kariery"),
        Loc.T("wybor_zawodu_esc_wroc_do_mapy"),
        Loc.T("profil_odznaki_tytuly_tytuly"),
        Loc.T("budowa_dnia_i_tygodnia_zawsze"),
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

    public override string Title => Loc.T("jak_grac");
    public override string Sub => $"{_page + 1}/{Pages}";
    public override string Hint => ButtonNames.Localize(FromTitle ? Loc.T("a_dalej_select_samouczek_menu") : Loc.T("a_dalej"));
    public override PageAction[] Actions => FromTitle
        ? [new(Loc.T("samouczek_jeszcze_raz"), GameAction.Select), new(Loc.T("dalej"), GameAction.A)]
        : [new(Loc.T("dalej"), GameAction.A)];

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
            DrawList(p, Loc.T("akty_mechaniki"), ActLines, Loc.T("problemy_zachowania"), BehaviorLines);
            return;
        }
        if (_page == 2)
        {
            DrawList(p, Loc.T("statystyki_3"), StatLines(_d), Loc.T("gdzie_opis"), WhereLines);
            return;
        }
        if (_page == 3)
        {
            DrawList(p, Loc.T("obrazenia_3"), _d.DamageHelpLines, Loc.T("gdzie_rozpiska"), DamageWhereLines);
            return;
        }
        if (_page == 4)
        {
            DrawList(p, Loc.T("kombinacje_stanow"), ComboLines(_d), Loc.T("skad_stany_premie_elity"), ComboWhereLines(_d));
            return;
        }
        if (_page == ViewPage) // v0.21.54: widok mapy (płaski / 3/4) i efekty świetlne
        {
            var vwidth = (int)(p.Right - 6 - p.Left - 12);
            var vlines = p.F.Wrap(Loc.T("pomoc_widok_mapy") + " " + Loc.T("pomoc_efekty_swietlne"), vwidth);
            var vc = p.Card(p.Section(p.Top, Loc.T("widok_i_swiatlo")), vlines.Count);
            var vx = p.TextX(vc);
            for (var i = 0; i < vlines.Count; i++) p.Text(vx, p.RowY(vc, i), vlines[i], Ink.Dark, TextAlign.Left, vc.End.X - 6 - vx);
            if (!PhoneView.Full) return; // telefon w poziomie: bez karty „gdzie” (wiersze ustawień mówią to same)
            var vc2 = p.Card(p.Section(vc.End.Y + 6, Loc.T("gdzie")), ViewWhereLines.Length);
            for (var i = 0; i < ViewWhereLines.Length; i++)
            {
                if (i > 0) p.Divider(vc2, i);
                p.Text(vx, p.RowY(vc2, i), ViewWhereLines[i], Ink.Dim, TextAlign.Left, vc2.End.X - 6 - vx);
            }
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
            var cc = p.Card(p.Section(p.Top, Loc.T("mapa_kariery_2")), clines.Count);
            var cx = p.TextX(cc);
            for (var i = 0; i < clines.Count; i++) p.Text(cx, p.RowY(cc, i), clines[i], Ink.Dark, TextAlign.Left, cc.End.X - 6 - cx);
            var cc2 = p.Card(p.Section(cc.End.Y + 6, Loc.T("gdzie")), CareerWhereLines.Length);
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
            var gc = p.Card(p.Section(p.Top, Loc.T("drzewko_kolekcje_zadania")), glines.Count);
            var gx = p.TextX(gc);
            for (var i = 0; i < glines.Count; i++) p.Text(gx, p.RowY(gc, i), glines[i], Ink.Dark, TextAlign.Left, gc.End.X - 6 - gx);
            var gc2 = p.Card(p.Section(gc.End.Y + 6, Loc.T("gdzie")), GoalsWhereLines.Length);
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
            var pc = p.Card(p.Section(p.Top, Loc.T("inspektor_i_mistrzostwo")), plines.Count);
            var px = p.TextX(pc);
            for (var i = 0; i < plines.Count; i++) p.Text(px, p.RowY(pc, i), plines[i], Ink.Dark, TextAlign.Left, pc.End.X - 6 - px);
            var pc2 = p.Card(p.Section(pc.End.Y + 6, Loc.T("gdzie")), ProgressWhereLines.Length);
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
            var sc = p.Card(p.Section(p.Top, Loc.T("sekretne_zlecenia")), lines.Count);
            var sx = p.TextX(sc);
            for (var i = 0; i < lines.Count; i++) p.Text(sx, p.RowY(sc, i), lines[i], Ink.Dark, TextAlign.Left, sc.End.X - 6 - sx);
            var y2 = p.Section(sc.End.Y + 6, Loc.T("gdzie"));
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
            DrawList(p, Loc.T("po_budowie_tydzien_fabula"), _d.MetaHelpLines, Loc.T("gdzie"), MetaWhereLines);
            return;
        }
        if (_page == 5)
        {
            // krótkie linie z GBA złączone i zawinięte na szerokość telefonu; jedna karta, żeby zmieścić się w poziomie
            var width = (int)(p.Right - 6 - p.Left - 12);
            var a = p.F.Wrap(ExtrasText(_d, 0), width);
            var b = p.F.Wrap(Loc.T("magazyn_3") + ExtrasText(_d, 6), width);
            var ec = p.Card(p.Section(p.Top, Loc.T("wydarzenia_ulepszenia_magazyn")), a.Count + b.Count);
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
            p.Text(p.TextX(top), p.RowY(top, 0), Loc.T("n3_akty_z_bossami_z_nagrody_akt"), Ink.Dark, TextAlign.Left, top.End.X - 6 - p.TextX(top));
            for (var i = 0; i < ShortNews.Length; i++)
            {
                p.Divider(top, i + 1);
                p.Text(p.TextX(top), p.RowY(top, i + 1), ShortNews[i], Ink.Dark, TextAlign.Left, top.End.X - 6 - p.TextX(top));
            }
            DrawControls(p, p.Section(top.End.Y + 6, Loc.T("sterowanie_2")));
            return;
        }
        var intro = p.Card(p.Top, 2);
        p.Stripe(intro, 0, Pal.Brand);
        p.Stripe(intro, 1, Pal.Brand);
        p.Text(p.TextX(intro), p.RowY(intro, 0), Loc.T("n10_etapow_w_3_aktach_akt_0_z"), Ink.Dark);
        p.Text(p.TextX(intro), p.RowY(intro, 1), Loc.T("kazdy_konczy_boss_schody_dalej"), Ink.Dark);
        var card = DrawControls(p, p.Section(intro.End.Y + 6, Loc.T("sterowanie_2")));
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        var y = p.Section(card.End.Y + 6, Loc.T("na_placu_2"));
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
        l.Add(Loc.T("po_etapie_premia_1_z_3_2"));
        l.Add(Loc.T("zlota_ramka_elita_lepsza"));
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
    private static bool _secretsHelpEn;
    private static string[] SecretsHelp // v0.21.53 cz. 2: w bieżącym języku danych
    {
        get
        {
            if (_secretsHelp is null || _secretsHelpEn != Session.GodotDataSource.DataEnglish)
            {
                _secretsHelp = Session.GodotDataSource.LoadStrings("secretsHelp");
                _secretsHelpEn = Session.GodotDataSource.DataEnglish;
            }
            return _secretsHelp;
        }
    }

    private static string[] SecretWhereLines =>
    [
        Loc.T("profil_odznaki_sekrety"),
        Loc.T("nowe_zawody_wybor_zawodu"),
        Loc.T("kask_w_paski_tryb_inwestora"),
    ];

    private static string[] ProgressWhereLines =>
    [
        Loc.T("tytul_pasek_inspektora"),
        Loc.T("profil_odznaki_inspektor"),
        Loc.T("wybor_zawodu_mistrz_n_wyglad"),
        Loc.T("tryb_inwestora_kolejny_stopien"),
    ];

    private static string[] MetaWhereLines =>
    [
        Loc.T("podsumowanie_po_koncu_budowy"),
        Loc.T("tytul_wyzwanie_tygodnia"),
        Loc.T("profil_osiedle_wiadomosci"),
    ];

    private static string[] ActLines =>
    [
        Loc.T("n0_pieczatki_3_dokumenty_schody"),
        Loc.T("i_bloto_wejscie_tura"),
        Loc.T("ii_porywy_wiatru_spychaja"),
        Loc.T("iii_pyl_widzisz_mniej"),
        Loc.T("kladka_dziala_tez_na_bloto"),
    ];

    private static string[] BehaviorLines =>
    [
        Loc.T("strzelaja_z_2_3_pol_dziela_sie"),
        Loc.T("lataja_sasiadow_rosna_z_czasem"),
        Loc.T("wybuch_zejdz_z_czerwonych_pol"),
        Loc.T("uciekaja_albo_stoja_jak_mur"),
        Loc.T("odpychaja_wracaja_raz"),
    ];

    /// <summary>Wzory statystyk z danych (StatHelp jak stat_rule w core.h).</summary>
    private static string[] StatLines(GameData d) =>
    [
        Loc.T("sil_zre_int_1_obr_co_2_pkt"),
        Loc.T("liczy_sie_tylko_statystyka"),
        StatHelp.Rule(d, new Message(), StatKind.Def).Text,
        StatHelp.Rule(d, new Message(), StatKind.Luck).Text,
        StatHelp.Rule(d, new Message(), StatKind.Luck, 1).Text,
        StatHelp.Rule(d, new Message(), StatKind.Luck, 2).Text,
        StatHelp.Rule(d, new Message(), StatKind.Hp).Text,
    ];

    private static string[] WhereLines => Layout.Touch
        ? [Loc.T("wybor_zawodu_dotknij"), Loc.T("telefon_start_opis_statystyk")]
        : [Loc.T("wybor_zawodu_i_albo_mysz"), Loc.T("telefon_start_spacja")];

    private static string[] DamageWhereLines => Layout.Touch
        ? [Loc.T("wybor_zawodu_dotknij_narzedzia"), Loc.T("telefon_sprzet_dotknij"), Loc.T("przytrzymaj_problem_obrazenia")]
        : [Loc.T("wybor_zawodu_mysz_na_narzedziu"), Loc.T("telefon_sprzet_i"), Loc.T("trzymaj_z_obrazenia_w_obie")];

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
    private static string[] ShortNews =>
        [Loc.T("okna_a_bierze_b_wraca_wszedzie"), Loc.T("respekt_i_nagrody_profil"), Loc.T("codzienna_budowa_menu_tytulu"), Loc.T("pogoda_brygada_telefon_sprzet")];

    /// <summary>Pogoda, brygada i tryb inwestora (v0.21.47), ścieżki, materiały i codzienna budowa (v0.21.48), Respekt i nagrody (v0.21.49).</summary>
    private static string[] News => Layout.Touch
        ?
        [
            Loc.T("po_etapie_premia_1_z_3_potem"), Loc.T("zlota_ramka_elita_cecha_lepsza"), Loc.T("materialy_z_problemow_2"),
            Loc.T("zalataj_drewno_deski_przed"), Loc.T("kladka_stal_kaluze_bez"), Loc.T("codzienna_budowa_menu_tytulu_2"),
            Loc.T("pogoda_dnia_ikona_w_hud_skutek"), Loc.T("brygada_telefon_sprzet_brygada"),
            Loc.T("po_wygranej_tryb_inwestora_na"),
            Loc.T("respekt_za_kazdy_etap_zostaje"),
            Loc.T("kazda_wygrana_nagroda_za"),
        ]
        :
        [
            Loc.T("okna_strzalki_zaznaczaja_a"), Loc.T("po_etapie_premia_1_z_3_potem"),
            Loc.T("zlota_ramka_elita_cecha_lepsza"), Loc.T("materialy_z_problemow_2"),
            Loc.T("zalataj_drewno_deski_przed"), Loc.T("kladka_stal_kaluze_bez"), Loc.T("codzienna_budowa_menu_tytulu_2"),
            Loc.T("pogoda_dnia_ikona_w_hud_skutek"), Loc.T("brygada_enter_spacja_albo"),
            Loc.T("po_wygranej_tab_na_wyborze"),
            Loc.T("respekt_za_kazdy_etap_zostaje_2"),
            Loc.T("kazda_wygrana_nagroda_za"),
        ];
}
