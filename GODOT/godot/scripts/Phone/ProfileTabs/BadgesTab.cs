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
    private static readonly string[] Pages = ["Odznaki", "Zlecenia", "Pamiątki", "Sekrety", "Tytuły", "Inspektor", "Zadania"];
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
            if (_page == TasksPage) return $"Dnia {DailyTasks.DoneToday(_p)}/{DailyTasks.DailySlots}";
            var n = _page == InspectorPage ? Progress.InspectorLevel(_d, _p)
                  : _page == TitlesPage ? Titles.OwnedCount(_d, _p)
                  : _page == SecretsPage ? Secrets.DoneCount(_d, _p)
                  : _page == 2 ? Enumerable.Range(0, total).Count(k => Meta.KeepsakeUnlocked(_d, _p, k))
                  : UiText.BitCount(_page == 1 ? _p.Contracts : _p.Badges);
            return $"{n}/{total}";
        }
    }

    public override string Hint => _page == TitlesPage
        ? $"Tab: wybierz tytuł  Spacja: {Pages[(_page + 1) % Pages.Length]}  Q/E: zakładki"
        : $"Spacja: {Pages[(_page + 1) % Pages.Length]}  Q/E: zakładki";

    public override PageAction[] Actions => _page == TitlesPage
        ? [new("Wybierz", GameAction.Select), new(Pages[(_page + 1) % Pages.Length] + " >", GameAction.A)]
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
                var tpw = p.Pill(right, y, done ? "Gotowe" : $"{prog}/{td.Target}", done ? PillKind.Done : prog > 0 ? PillKind.Prog : PillKind.Gray);
                p.Text(tx, y, (i >= DailyTasks.DailySlots ? "Tydzień: " : "") + td.Name, sel ? Ink.Brand : done ? Ink.Done : Ink.Dark, TextAlign.Left, right - tpw - 4 - tx);
                continue;
            }
            if (_page == InspectorPage) // v0.21.52 cz. b: poziom i nagroda
            {
                var lv = Progress.InspectorLevel(_d, _p);
                on = i < lv;
                Progress.InspectorBar(_d, _p, out var cur, out var need);
                var ipw = p.Pill(right, y, on ? "Masz" : i == lv ? $"{cur}/{need}" : $"Poz. {i + 1}", on ? PillKind.Done : i == lv ? PillKind.Prog : PillKind.Gray);
                p.Text(tx, y, $"{i + 1}. {Progress.RewardLabel(_d, _d.InspectorLevels[i], -1)}", sel ? Ink.Brand : on ? Ink.Dark : Ink.Dim, TextAlign.Left, right - ipw - 4 - tx);
                continue;
            }
            if (_page == TitlesPage)
            {
                on = Titles.Owned(_d, _p, i);
                var chosen = Titles.Selected(_d, _p) == i;
                var tpw = p.Pill(right, y, chosen ? "Wybrany" : on ? "Masz" : "Zablok.", chosen ? PillKind.Prog : on ? PillKind.Done : PillKind.Gray);
                p.Text(tx, y, Titles.Name(_d, i), sel ? Ink.Brand : on ? Ink.Dark : Ink.Dim, TextAlign.Left, right - tpw - 4 - tx);
                continue;
            }
            if (_page == SecretsPage)
            {
                var sd = _d.Secrets[i];
                on = Secrets.Done(_p, i);
                name = sd.Hint;
                pill = on ? "Wykonane" : "???";
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
                pill = on ? "Ranga " + UiText.Roman(Meta.KeepsakeRank(_d, _p, i) - 1) : "Zablok.";
                kind = !on ? PillKind.Gray : Meta.SelectedKeepsake(_d, _p) == i ? PillKind.Prog : PillKind.Group;
            }
            else if (_page == 1)
            {
                on = Meta.ContractDone(_p, i);
                var c = _d.Contracts[i];
                var pr = System.Math.Min(Meta.ContractProgress(_d, _p, i), c.Target);
                name = c.Name;
                pill = on ? "Wykonane" : $"{pr}/{c.Target}";
                kind = on ? PillKind.Done : pr > 0 ? PillKind.Prog : PillKind.Gray;
                on = true;
            }
            else
            {
                on = (_p.Badges & (1 << i)) != 0;
                name = _d.Badges[i].Name;
                pill = on ? "Zdobyta" : $"+{_d.Badges[i].Xp}";
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
        ProgressReward.Respect => $"Respekt +{l.Value}",
        ProgressReward.Title => $"tytuł {l.Title}",
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
            var head = $"{(i < DailyTasks.DailySlots ? "Zadanie dnia" : "Zadanie tygodnia")}: +{td.Respect} Respektu. Wykonane łącznie: {_p.TasksTotal}"
                       + (nr >= 0 ? $", za {_d.TaskRewards[nr].Xp}: {GoalLabel(_d.TaskRewards[nr])}." : ".");
            var ns = DayStreak.NextReward(_d, _p);
            var streak = DayStreak.Now(_p, Today);
            var extra = ns >= 0 ? $"Seria {streak}/{_d.StreakRewards[ns].Xp} dni budowy dnia: {GoalLabel(_d.StreakRewards[ns])}"
                                : $"Seria {streak} dni budowy dnia (rekord {_p.StreakBest})";
            return (head, extra, Ink.Brand);
        }
        if (_page == InspectorPage)
        {
            var lv = Progress.InspectorLevel(_d, _p);
            Progress.InspectorBar(_d, _p, out var cur, out var need);
            var head = need > 0 ? $"Inspektor {lv}: {cur}/{need} dośw. do poziomu {lv + 1}." : $"Inspektor {lv}: wszystkie poziomy!";
            return (head + " Dośw. z każdej budowy, też porażki: etapy, bossowie, elity, magazyny, wygrana.",
                    $"Poziom {i + 1}: {Progress.RewardLabel(_d, _d.InspectorLevels[i], -1)}", i < lv ? Ink.Done : Ink.Brand);
        }
        if (_page == TitlesPage && i >= Titles.ProgressFrom(_d)) // v0.21.52 cz. b: tytuł z inspektora / stopnia inwestora
        {
            var pt = _d.ProgressTitles[i - Titles.ProgressFrom(_d)];
            var psel = Titles.Selected(_d, _p);
            var src = pt.Source switch
            {
                0 => $"Poziom inspektora {pt.Level} (dośw. z każdej budowy).",
                1 => $"Stopień inwestora: wygraj ze stawką {pt.Level}.",
                2 => $"Kolekcja „{_d.Collections[pt.Level - 1].Name}” w komplecie (Katalog > Kolekcje).", // v0.21.52 cz. c
                3 => $"Seria {pt.Level} dni budowy dnia.",
                _ => $"{pt.Level} wykonanych zadań dnia i tygodnia.",
            };
            var pextra = psel >= 0 ? "Twój tytuł: " + Titles.Name(_d, psel) : Titles.Owned(_d, _p, i) ? "Wybierz: Tab / „Wybierz”" : "Bez tytułu";
            return (src, pextra, psel >= 0 ? Ink.Done : Ink.Brand);
        }
        if (_page == TitlesPage)
        {
            var fromBadge = i < _d.Badges.Length;
            var si = fromBadge ? i : i - _d.Badges.Length;
            var src = fromBadge ? $"Odznaka „{_d.Badges[si].Name}”: {_d.Badges[si].Desc}" : $"Zlecenie „{_d.Contracts[si].Name}”: {_d.Contracts[si].Desc}";
            var ck = fromBadge ? _d.Badges[si].Cosmetic : _d.Contracts[si].Cosmetic;
            var own = Titles.Owned(_d, _p, i);
            var sel = Titles.Selected(_d, _p);
            var extra = sel >= 0 ? "Twój tytuł: " + Titles.Name(_d, sel) : own ? "Wybierz: Tab / „Wybierz”" : "Bez tytułu";
            if (ck >= 0) src += $" Też: {_d.Cosmetics[ck].Name}.";
            return (src, extra, sel >= 0 ? Ink.Done : Ink.Brand);
        }
        if (_page == SecretsPage)
        {
            var sd = _d.Secrets[i];
            var done = Secrets.Done(_p, i);
            return (done ? $"{sd.Hint}. Warunek: {sd.Desc}." : $"{sd.Hint}. Warunek: ??? – poznasz go po wykonaniu.",
                    "Nagroda: " + (done ? sd.RewardText : "???"), done ? Ink.Done : Ink.Dim);
        }
        if (_page == 2)
        {
            var kd = _d.Keepsakes[i];
            var unl = Meta.KeepsakeUnlocked(_d, _p, i);
            var rank = Meta.KeepsakeRank(_d, _p, i);
            string how;
            if (!unl)
                how = kd.Badge >= 0 ? "Odznaka: " + _d.Badges[kd.Badge].Name
                    : kd.Streak > 0 ? $"Seria {kd.Streak} dni budowy dnia"
                    : "Zlecenie: " + (_d.Contracts.FirstOrDefault(c => c.Keepsake == i)?.Name ?? "?");
            else if (rank < 3) how = $"Ranga {UiText.Roman(rank)} po {_d.KeepsakeRankRuns[rank - 1]} bud. (ma {_p.KeepsakeRuns[i]})";
            else how = "Ranga maksymalna";
            return ($"{kd.Desc} Premia: {RunMods.PerkLabel(Meta.KeepsakePerk(_d, _p, i))}", how, unl ? Ink.Done : Ink.Brand);
        }
        if (_page == 1)
        {
            var c = _d.Contracts[i];
            var reward = $"Nagroda: +{c.Xp} dośw., tytuł „{c.Title}”" + (c.Keepsake >= 0 ? ", " + _d.Keepsakes[c.Keepsake].Name
                       : c.Cosmetic >= 0 ? ", " + _d.Cosmetics[c.Cosmetic].Name : "");
            return (c.Desc, reward, Meta.ContractDone(_p, i) ? Ink.Done : Ink.Brand);
        }
        var b = _d.Badges[i];
        var got = (_p.Badges & (1 << i)) != 0;
        return ($"{b.Desc}. Tytuł „{b.Title}”" + (b.Cosmetic >= 0 ? ", " + _d.Cosmetics[b.Cosmetic].Name : ""),
                "Premia: " + RunMods.PerkLabel(b.Bonus), got ? Ink.Done : Ink.Brand);
    }
}
