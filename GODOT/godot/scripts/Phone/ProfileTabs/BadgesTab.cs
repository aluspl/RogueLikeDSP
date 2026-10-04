using System.Linq;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;

namespace LifeLike.Game.Phone.ProfileTabs;

/// <summary>
/// Profil: strony Odznaki / Zlecenia / Pamiątki / Sekrety przełączane A (jak zakładka 0 w run_shop na GBA): lista z pastylkami
/// (Zdobyta / +XP, Wykonane / postęp, Ranga / Zablok., Wykonane / ???) i opis zaznaczonej pozycji z premią lub nagrodą.
/// Sekrety (#39, v0.21.51 cz. 2): na liście podpowiedź z ikoną koperty, warunek i nagroda (z ikoną) dopiero po wykonaniu.
/// Tytuły (v0.21.52, #43): tytuły z odznak i zleceń – Tab / „Wybierz” (SELECT na GBA) albo drugie stuknięcie wybiera
/// tytuł widoczny w profilu i na końcu budowy. v0.21.52 cz. b: tytuły z poziomu inspektora i stopni inwestora; strona
/// Inspektor (#44) – poziomy z nagrodami (Masz / postęp bieżącego / Poz. N), opis: dośw. do kolejnego poziomu.
/// v0.21.52 cz. c: strona Zadania – 3 zadania dnia i 2 tygodnia (data z systemu) z postępem, nagroda, wykonane łącznie
/// i seria dni budowy dnia z kolejną nagrodą.
/// </summary>
public sealed class BadgesTab : PhonePage
{
    private const int Window = 7;
    private static string[] Pages => [Loc.T("odznaki_2"), Loc.T("zlecenia"), Loc.T("pamiatki"), Loc.T("sekrety"), Loc.T("tytuly"), Loc.T("inspektor_2"), Loc.T("zadania_2")];
    public const int SecretsPage = 3;
    public const int TitlesPage = 4;
    public const int InspectorPage = 5;
    public const int TasksPage = 6;
    private readonly GameData _d;
    private readonly Profile _p;
    private readonly ListState _list = new();
    private int _page;

    public BadgesTab(GameData d, Profile p, int page = 0)
    {
        _d = d;
        _p = p;
        _page = page;
    }

    public override string Title => Pages[_page];

    public override string Sub
    {
        get
        {
            var total = Count;
            if (_page == TasksPage) return Loc.F("dnia_2", DailyTasks.DoneToday(_p), DailyTasks.DailySlots);
            var n = _page == InspectorPage ? Progress.InspectorLevel(_d, _p)
                  : _page == TitlesPage ? Titles.OwnedCount(_d, _p)
                  : _page == SecretsPage ? Secrets.DoneCount(_d, _p)
                  : _page == 2 ? Enumerable.Range(0, total).Count(k => Meta.KeepsakeUnlocked(_d, _p, k))
                  : UiText.BitCount(_page == 1 ? _p.Contracts : _p.Badges);
            return $"{n}/{total}";
        }
    }

    public override string Hint => _page == TitlesPage
        ? Loc.F("tab_wybierz_tytul_spacja_q_e", Pages[(_page + 1) % Pages.Length])
        : Loc.F("spacja_q_e_zakladki", Pages[(_page + 1) % Pages.Length]);

    public override PageAction[] Actions => _page == TitlesPage
        ? [new(Loc.T("wybierz"), GameAction.Select), new(Pages[(_page + 1) % Pages.Length] + " >", GameAction.A)]
        : [new(Pages[(_page + 1) % Pages.Length] + " >", GameAction.A)];

    /// <summary>Bieżąca strona (0 Odznaki, 1 Zlecenia, 2 Pamiątki, 3 Sekrety) – test dymny i zrzuty.</summary>
    public int Page
    {
        get => _page;
        set
        {
            _page = System.Math.Clamp(value, 0, Pages.Length - 1);
            _list.Reset();
            if (_page == InspectorPage) Select(System.Math.Min(Progress.InspectorLevel(_d, _p), Count - 1)); // od bieżącego poziomu
        }
    }

    /// <summary>Zaznaczona pozycja listy (zrzuty).</summary>
    public void Select(int i)
    {
        _list.Reset();
        _list.Move(i, Count, Window);
    }

    public override bool TapRow(int index)
    {
        if (_page == TitlesPage && index == _list.Sel) ChooseTitle();
        _list.Sel = index;
        return true;
    }

    /// <summary>Wybór zaznaczonego tytułu (drugi raz – bez tytułu); tylko zdobyte.</summary>
    public bool ChooseTitle()
    {
        var t = _list.Sel;
        if (!Titles.Owned(_d, _p, t)) return false;
        _p.Title = (byte)(Titles.Selected(_d, _p) == t ? 0 : t + 1);
        Sfx.Play("buy");
        Saved?.Invoke();
        return true;
    }

    /// <summary>Zapis profilu po wyborze tytułu (ustawia ekran profilu).</summary>
    public System.Action Saved { get; set; }

    /// <summary>Dzisiejszy dzień budowy dnia (seria dni) – ustawia ekran profilu (data z systemu).</summary>
    public int Today { get; set; }

    private int Count => _page == TasksPage ? DailyTasks.Slots : _page == InspectorPage ? _d.InspectorLevels.Length : _page == TitlesPage ? Titles.Count(_d) : _page == SecretsPage ? _d.Secrets.Length : _page == 2 ? _d.Keepsakes.Length : _page == 1 ? _d.Contracts.Length : _d.Badges.Length;

    public override bool Input(InputCmd e)
    {
        var v = e.VDir;
        if (v != 0)
        {
            _list.Move(v, Count, Window);
            return true;
        }
        if (_page == TitlesPage && e.Is(GameAction.Select))
        {
            ChooseTitle();
            return true;
        }
        if (e.Is(GameAction.A))
        {
            Page = (_page + 1) % Pages.Length;
            Sfx.Play("menu");
            return true;
        }
        return false;
    }

    public override void Draw(PhonePainter p)
    {
        var n = Count;
        _list.Clamp(n, Window);
        var rows = System.Math.Min(Window, n - _list.Top);
        var card = p.Card(p.Top, rows);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        for (var r = 0; r < rows; r++)
        {
            var i = _list.Top + r;
            var y = p.RowY(card, r);
            var sel = i == _list.Sel;
            if (sel) p.Selected(card, r);
            else if (r > 0) p.Divider(card, r);
            p.HitRow(card, r, i);
            string name, pill;
            PillKind kind;
            bool on;
            if (_page == TasksPage) // v0.21.52 cz. c: zadanie dnia / tygodnia z postępem
            {
                var td = DailyTasks.Of(_d, _p, i);
                var done = DailyTasks.Done(_p, i);
                var prog = DailyTasks.ProgressLive(_d, _p, null, i);
                var tpw = p.Pill(right, y, done ? Loc.T("gotowe") : $"{prog}/{td.Target}", done ? PillKind.Done : prog > 0 ? PillKind.Prog : PillKind.Gray);
                p.Text(tx, y, (i >= DailyTasks.DailySlots ? Loc.T("tydzien_2") : "") + td.Name, sel ? Ink.Brand : done ? Ink.Done : Ink.Dark, TextAlign.Left, right - tpw - 4 - tx);
                continue;
            }
            if (_page == InspectorPage) // v0.21.52 cz. b: poziom i nagroda
            {
                var lv = Progress.InspectorLevel(_d, _p);
                on = i < lv;
                Progress.InspectorBar(_d, _p, out var cur, out var need);
                var ipw = p.Pill(right, y, on ? Loc.T("masz_3") : i == lv ? $"{cur}/{need}" : Loc.F("poz_2", i + 1), on ? PillKind.Done : i == lv ? PillKind.Prog : PillKind.Gray);
                p.Text(tx, y, $"{i + 1}. {Progress.RewardLabel(_d, _d.InspectorLevels[i], -1)}", sel ? Ink.Brand : on ? Ink.Dark : Ink.Dim, TextAlign.Left, right - ipw - 4 - tx);
                continue;
            }
            if (_page == TitlesPage)
            {
                on = Titles.Owned(_d, _p, i);
                var chosen = Titles.Selected(_d, _p) == i;
                var tpw = p.Pill(right, y, chosen ? Loc.T("wybrany") : on ? Loc.T("masz_3") : Loc.T("zablok"), chosen ? PillKind.Prog : on ? PillKind.Done : PillKind.Gray);
                p.Text(tx, y, Titles.Name(_d, i), sel ? Ink.Brand : on ? Ink.Dark : Ink.Dim, TextAlign.Left, right - tpw - 4 - tx);
                continue;
            }
            if (_page == SecretsPage)
            {
                var sd = _d.Secrets[i];
                on = Secrets.Done(_p, i);
                name = sd.Hint;
                pill = on ? Loc.T("wykonane_2") : "???";
                kind = on ? PillKind.Done : PillKind.Gray;
                var ic = new Rect2(tx - 2, y + (PhonePainter.RowH - 16) / 2f, 16, 16);
                if (on && sd.Reward == SecretReward.Cls) // nowy zawód: mały portret
                    p.C.DrawTextureRectRegion(Assets.Actors, ic, Assets.Frame(_d.Classes[sd.Index].Frame, Assets.Actor));
                else p.Icon(Assets.UiMenu, on ? Assets.SecretIcon(_d, sd) : Assets.MenuSecret, Assets.Icon, ic.Position);
                var spw = p.Pill(right, y, pill, kind);
                p.Text(tx + 18, y, name, sel ? Ink.Brand : on ? Ink.Dark : Ink.Dim, TextAlign.Left, right - spw - 4 - tx - 18);
                continue;
            }
            if (_page == 2)
            {
                on = Meta.KeepsakeUnlocked(_d, _p, i);
                name = _d.Keepsakes[i].Name;
                pill = on ? Loc.T("ranga") + UiText.Roman(Meta.KeepsakeRank(_d, _p, i) - 1) : Loc.T("zablok");
                kind = !on ? PillKind.Gray : Meta.SelectedKeepsake(_d, _p) == i ? PillKind.Prog : PillKind.Group;
            }
            else if (_page == 1)
            {
                on = Meta.ContractDone(_p, i);
                var c = _d.Contracts[i];
                var pr = System.Math.Min(Meta.ContractProgress(_d, _p, i), c.Target);
                name = c.Name;
                pill = on ? Loc.T("wykonane_2") : $"{pr}/{c.Target}";
                kind = on ? PillKind.Done : pr > 0 ? PillKind.Prog : PillKind.Gray;
                on = true;
            }
            else
            {
                on = (_p.Badges & (1 << i)) != 0;
                name = _d.Badges[i].Name;
                pill = on ? Loc.T("zdobyta") : $"+{_d.Badges[i].Xp}";
                kind = on ? PillKind.Done : PillKind.Gray;
            }
            var pw = p.Pill(right, y, pill, kind);
            p.Text(tx, y, name, sel ? Ink.Brand : on ? Ink.Dark : Ink.Dim, TextAlign.Left, right - pw - 4 - tx);
        }
        if (_list.Top > 0) p.Text(card.End.X - 4, card.Position.Y - 14, "^", Ink.Dim, TextAlign.Right);
        if (_list.Top + rows < n) p.Text(card.End.X - 4, card.End.Y - 6, "v", Ink.Dim, TextAlign.Right);

        var dc = p.Card(card.End.Y + 6, 3);
        var (desc, extra, ink) = Describe(_list.Sel);
        var lines = p.F.Wrap(desc, (int)(right - tx));
        for (var k = 0; k < 2 && k < lines.Count; k++) p.Text(tx, p.RowY(dc, k), lines[k], Ink.Dim);
        p.Divider(dc, 2);
        p.Stripe(dc, 2, ink.Fill);
        p.Text(tx, p.RowY(dc, 2), extra, ink, TextAlign.Left, right - tx);
    }

    /// <summary>Krótka nagroda za próg (zadania łącznie, seria dni): „Respekt +15”, nazwa kasku / pamiątki, „tytuł X”.</summary>
    private string GoalLabel(ProgressLevel l) => l.Reward switch
    {
        ProgressReward.Respect => Loc.F("respekt_5", l.Value),
        ProgressReward.Title => Loc.F("tytul_5", l.Title),
        ProgressReward.Helmet => _d.Cosmetics[l.Index].Name,
        ProgressReward.Keepsake => _d.Keepsakes[l.Index].Name,
        _ => "",
    };

    /// <summary>Opis zaznaczonej pozycji: (opis, wiersz premii / nagrody / rangi, jego kolor).</summary>
    private (string, string, Ink) Describe(int i)
    {
        if (_page == TasksPage)
        {
            var td = DailyTasks.Of(_d, _p, i);
            var nr = DailyTasks.NextReward(_d, _p);
            var head = Loc.F("respektu_wykonane_lacznie", (i < DailyTasks.DailySlots ? Loc.T("zadanie_dnia_2") : Loc.T("zadanie_tygodnia")), td.Respect, _p.TasksTotal)
                       + (nr >= 0 ? $", za {_d.TaskRewards[nr].Xp}: {GoalLabel(_d.TaskRewards[nr])}." : ".");
            var ns = DayStreak.NextReward(_d, _p);
            var streak = DayStreak.Now(_p, Today);
            var extra = ns >= 0 ? Loc.F("seria_dni_budowy_dnia_2", streak, _d.StreakRewards[ns].Xp, GoalLabel(_d.StreakRewards[ns]))
                                : Loc.F("seria_dni_budowy_dnia_rekord", streak, _p.StreakBest);
            return (head, extra, Ink.Brand);
        }
        if (_page == InspectorPage)
        {
            var lv = Progress.InspectorLevel(_d, _p);
            Progress.InspectorBar(_d, _p, out var cur, out var need);
            var head = need > 0 ? Loc.F("inspektor_dosw_do_poziomu", lv, cur, need, lv + 1) : Loc.F("inspektor_wszystkie_poziomy", lv);
            return (head + Loc.T("dosw_z_kazdej_budowy_tez_2"),
                    Loc.F("poziom_4", i + 1, Progress.RewardLabel(_d, _d.InspectorLevels[i], -1)), i < lv ? Ink.Done : Ink.Brand);
        }
        if (_page == TitlesPage && i >= Titles.ProgressFrom(_d)) // v0.21.52 cz. b: tytuł z inspektora / stopnia inwestora
        {
            var pt = _d.ProgressTitles[i - Titles.ProgressFrom(_d)];
            var psel = Titles.Selected(_d, _p);
            var src = pt.Source switch
            {
                0 => Loc.F("poziom_inspektora_dosw_z", pt.Level),
                1 => Loc.F("stopien_inwestora_wygraj_ze", pt.Level),
                2 => Loc.F("kolekcja_w_komplecie_katalog", _d.Collections[pt.Level - 1].Name), // v0.21.52 cz. c
                3 => Loc.F("seria_dni_budowy_dnia_3", pt.Level),
                _ => Loc.F("wykonanych_zadan_dnia_i", pt.Level),
            };
            var pextra = psel >= 0 ? Loc.T("twoj_tytul") + Titles.Name(_d, psel) : Titles.Owned(_d, _p, i) ? Loc.T("wybierz_tab_wybierz") : Loc.T("bez_tytulu");
            return (src, pextra, psel >= 0 ? Ink.Done : Ink.Brand);
        }
        if (_page == TitlesPage)
        {
            var fromBadge = i < _d.Badges.Length;
            var si = fromBadge ? i : i - _d.Badges.Length;
            var src = fromBadge ? Loc.F("odznaka_2", _d.Badges[si].Name, _d.Badges[si].Desc) : Loc.F("zlecenie_4", _d.Contracts[si].Name, _d.Contracts[si].Desc);
            var ck = fromBadge ? _d.Badges[si].Cosmetic : _d.Contracts[si].Cosmetic;
            var own = Titles.Owned(_d, _p, i);
            var sel = Titles.Selected(_d, _p);
            var extra = sel >= 0 ? Loc.T("twoj_tytul") + Titles.Name(_d, sel) : own ? Loc.T("wybierz_tab_wybierz") : Loc.T("bez_tytulu");
            if (ck >= 0) src += Loc.F("tez_2", _d.Cosmetics[ck].Name);
            return (src, extra, sel >= 0 ? Ink.Done : Ink.Brand);
        }
        if (_page == SecretsPage)
        {
            var sd = _d.Secrets[i];
            var done = Secrets.Done(_p, i);
            return (done ? Loc.F("warunek_2", sd.Hint, sd.Desc) : Loc.F("warunek_poznasz_go_po", sd.Hint),
                    Loc.T("nagroda") + (done ? sd.RewardText : "???"), done ? Ink.Done : Ink.Dim);
        }
        if (_page == 2)
        {
            var kd = _d.Keepsakes[i];
            var unl = Meta.KeepsakeUnlocked(_d, _p, i);
            var rank = Meta.KeepsakeRank(_d, _p, i);
            string how;
            if (!unl)
                how = kd.Badge >= 0 ? Loc.T("odznaka") + _d.Badges[kd.Badge].Name
                    : kd.Streak > 0 ? Loc.F("seria_dni_budowy_dnia_4", kd.Streak)
                    : Loc.T("zlecenie") + (_d.Contracts.FirstOrDefault(c => c.Keepsake == i)?.Name ?? "?");
            else if (rank < 3) how = Loc.F("ranga_po_bud_ma", UiText.Roman(rank), _d.KeepsakeRankRuns[rank - 1], _p.KeepsakeRuns[i]);
            else how = Loc.T("ranga_maksymalna");
            return (Loc.F("premia_5", kd.Desc, RunMods.PerkLabel(Meta.KeepsakePerk(_d, _p, i))), how, unl ? Ink.Done : Ink.Brand);
        }
        if (_page == 1)
        {
            var c = _d.Contracts[i];
            var reward = Loc.F("nagroda_dosw_tytul", c.Xp, c.Title) + (c.Keepsake >= 0 ? ", " + _d.Keepsakes[c.Keepsake].Name
                       : c.Cosmetic >= 0 ? ", " + _d.Cosmetics[c.Cosmetic].Name : "");
            return (c.Desc, reward, Meta.ContractDone(_p, i) ? Ink.Done : Ink.Brand);
        }
        var b = _d.Badges[i];
        var got = (_p.Badges & (1 << i)) != 0;
        return (Loc.F("tytul_6", b.Desc, b.Title) + (b.Cosmetic >= 0 ? ", " + _d.Cosmetics[b.Cosmetic].Name : ""),
                Loc.T("premia") + RunMods.PerkLabel(b.Bonus), got ? Ink.Done : Ink.Brand);
    }
}
