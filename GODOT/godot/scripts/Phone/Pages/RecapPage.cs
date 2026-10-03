using System;
using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using LifeLike.Game.Session;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Podsumowanie budowy (#33) – jedna przewijana strona w telefonie (na GBA 3 strony): co zatrzymało budowę (albo
/// odbiór), ostatnie ciosy, najmocniejsze ciosy, oś czasu etapów (dni, usunięte, SMS, magazyn, premie), nagrody z budowy
/// (doświadczenie, Respekt, zlecenie, rekord dnia / tygodnia), v0.21.52: karta „Postęp” z paskami (najbliższe Szkolenie,
/// mistrzostwo zawodu i poziom inspektora z dośw. z tej budowy i nowym poziomem), najbliższy cel i rada. Góra/dół albo dotknięcie górnej /
/// dolnej połowy przewija, Enter – dalej.
/// </summary>
public sealed class RecapPage : PhonePage
{
    private readonly List<RecapRow> _rows;
    private int _top;
    private int _window = 10;

    public RecapPage(GameSession s)
    {
        _rows = Build(s);
    }

    public int Top => _top;
    public int Count => _rows.Count;

    public override string Title => Loc.T("podsumowanie");
    public override string Sub => _rows.Count > _window ? $"{Math.Min(_rows.Count, _top + _window)}/{_rows.Count}" : "";
    public override string Hint => Loc.T("gora_dol_przewin_enter_dalej");
    public override PageAction[] Actions => [new(Loc.T("wyzej"), GameAction.Up), new(Loc.T("nizej"), GameAction.Down), new(Loc.T("dalej"), GameAction.Start)];

    private static Ink InkOf(LogKind k) => k switch
    {
        LogKind.Bad => Ink.Late,
        LogKind.Good => Ink.Done,
        LogKind.Loot => Ink.Brand,
        _ => Ink.Dark,
    };

    private static List<RecapRow> Build(GameSession s)
    {
        var g = s.Game;
        var d = s.Data;
        var p = s.Profile;
        var won = g.St == GameStatus.Won;
        var none = Colors.Transparent;
        var rows = new List<RecapRow>();
        void Head(string t) => rows.Add(new RecapRow(t, "", Ink.Dim, none, true));
        void Row(string t, string tail, Ink ink, Color stripe) => rows.Add(new RecapRow(t, tail, ink, stripe));

        var where = g.RecapWhere(new Message()).Text;
        if (won) Row(Loc.T("odbior_zaliczony"), Loc.F("dni_4", HouseSchedule.TotalDays(g)), Ink.Done, Pal.Done);
        else Row(g.RecapKiller(new Message()).Text, "", Ink.Late, Pal.Late);
        Row(g.SDef().Name, where, Ink.Dark, won ? Pal.Done : Pal.Late);
        Row(Loc.F("usuniete_elity_kombinacje", g.Kills, g.ElitesKilled, g.CombosRun), "", Ink.Dim, none);
        if (!won)
        {
            Head(Loc.T("ostatnie_ciosy_2"));
            for (var i = 0; i < CoreGame.RecapHitsN; i++)
            {
                var h = g.LastHits[i];
                if (h.Amount <= 0) break;
                Row(g.RecapHitText(h), "", i == 0 ? Ink.Late : Ink.Dark, i == 0 ? Pal.Late : none);
            }
        }
        Head(Loc.T("najmocniejsze_ciosy"));
        if (g.WorstHit.Amount > 0) Row(Loc.T("w_ciebie") + g.RecapHitText(g.WorstHit), "", Ink.Dark, none);
        if (g.BestHit > 0) Row(Loc.F("twoj_w", g.BestHit, (g.BestHitCrit ? Loc.T("kryt_13") : ""), d.Enemies[g.BestHitDef].Name), "", Ink.Brand, none);

        Head(Loc.T("os_czasu_2"));
        foreach (var l in g.RecapTimeline())
        {
            var head = !l.Text.Text.StartsWith(' ');
            rows.Add(new RecapRow(l.Text.Text.TrimStart(), l.Tail.Text, InkOf(l.Ink), head ? (l.Ink == LogKind.Bad ? Pal.Late : Pal.Done) : none));
        }

        Head(Loc.T("nagrody_z_budowy"));
        Row(Loc.F("doswiadczenie", s.LastGained), Loc.F("masz_4", p.Xp), Ink.Dark, none);
        Row(Loc.F("respekt_5", g.Respect), Loc.F("masz_4", p.Respect), Ink.Brand, none);
        var ci = Meta.NextContract(d, p, g);
        if (ci >= 0)
        {
            var c = d.Contracts[ci];
            Row(Loc.F("zlecenie_3", c.Name), $"{Math.Min(c.Target, Meta.ContractProgress(d, p, ci))}/{c.Target}", Ink.Dark, Pal.Prog);
        }
        if (g.Daily) Row(s.DailyRecord ? Loc.T("rekord_dnia_3") : Loc.F("budowa_dnia_nr_2", g.DailyDay), "", s.DailyRecord ? Ink.Done : Ink.Dim, none);
        if (g.WeeklyWeek != 0) Row(s.WeeklyRecord ? Loc.T("rekord_tygodnia_3") : Loc.F("wyzwanie", d.Weekly[g.Bonus.Weekly].Name), "", s.WeeklyRecord ? Ink.Done : Ink.Dim, none);
        var ti = Titles.Selected(d, p);
        if (ti >= 0) Row(Loc.T("tytul") + Titles.Name(d, ti), "", Ink.Brand, none);

        // v0.21.52 (#52): paski postępu – najbliższe Szkolenie za doświadczenie, mistrzostwo zawodu i poziom inspektora
        Head(Loc.T("postep_2"));
        void Bar(float v, Color c) => rows.Add(new RecapRow("", "", Ink.Dim, none, false, Math.Clamp(v, 0f, 1f), c));
        var cost = Meta.NextUnlock(d, p, out var kind, out var idx);
        if (cost < 0)
        {
            Row(Loc.T("szkolenia_wszystko_kupione"), "MAX", Ink.Done, Pal.Done);
            Bar(1f, Pal.Done);
        }
        else
        {
            var ready = p.Xp >= cost;
            Row(UnlockName(d, p, kind, idx), ready ? Loc.T("stac_cie") : $"{p.Xp}/{cost}", Ink.Dark, ready ? Pal.Done : Pal.Brand);
            Bar(ready ? 1f : p.Xp / (float)cost, ready ? Pal.Done : Pal.Brand);
        }
        var pg = s.LastProgress;
        void Level(string label, int level, int cur, int need, bool up)
        {
            var full = up || need == 0;
            Row($"{label} {level}", up ? Loc.F("poziom_3", level) : (need == 0 ? "MAX" : $"+{pg.Gained}  {cur}/{need}"), Ink.Dark, full ? Pal.Done : Pal.Brand);
            Bar(need == 0 ? 1f : cur / (float)need, full ? Pal.Done : Pal.Brand);
        }
        Progress.MasteryBar(d, p, g.Cls, out var mc, out var mn);
        Level(Loc.T("mistrzostwo") + d.Classes[g.Cls].Name, Progress.MasteryLevel(d, p, g.Cls), mc, mn, pg.MasteryAfter > pg.MasteryBefore);
        Progress.InspectorBar(d, p, out var ic, out var inn);
        Level(Loc.T("inspektor_2"), Progress.InspectorLevel(d, p), ic, inn, pg.InspAfter > pg.InspBefore);

        Head(Loc.T("najblizszy_cel"));
        if (Recap.Goal(d, p, out var lead, out var name)) Row($"{lead} {name}", "", Ink.Brand, Pal.Brand);
        var tip = d.RecapTips[Recap.TipIndex(d, g)];
        Row(tip.Lines[0], "", Ink.Prog, Pal.Prog);
        if (tip.Lines[1].Length > 0) Row(tip.Lines[1], "", Ink.Prog, Pal.Prog);
        return rows;
    }

    /// <summary>Nazwa najbliższego zakupu w Szkoleniach (next_unlock): „Kondycja III”, „Zawód: Elektryk”...</summary>
    private static string UnlockName(GameData d, Profile p, int kind, int i) => kind switch
    {
        0 => $"{d.Upgrades[i].Name} {UiText.Roman(p.Levels[i])}",
        1 => Loc.T("zawod_3") + d.Classes[i].Name,
        2 => d.Weapons[d.Tools[i].Weapon].Name,
        3 => Loc.T("brygada_2") + d.Brigade[i].Name,
        5 => Loc.T("drzewko") + d.TreeBranches[d.TreeNodes[i].Branch].Name, // v0.21.52 cz. c
        _ => Loc.T("trudnosc") + d.Difficulties[^1].Name,
    };

    /// <summary>Przewiń do nagłówka sekcji (sceny zrzutów: „POSTĘP”).</summary>
    public void ScrollTo(string header)
    {
        var i = _rows.FindIndex(r => r.Header && r.Text == header);
        if (i >= 0) _top = i;
        Redraw();
    }

    public override bool TapRow(int index)
    {
        Scroll(index == -1 ? -_window / 2 : _window / 2);
        return true;
    }

    public override bool Input(InputCmd e)
    {
        if (e.VDir == 0) return false;
        Scroll(e.VDir * 3);
        return true;
    }

    private void Scroll(int d)
    {
        var top = Math.Clamp(_top + d, 0, Math.Max(0, _rows.Count - _window));
        if (top == _top) return;
        _top = top;
        Sfx.Play("menu");
        Redraw();
    }

    public override void Draw(PhonePainter p)
    {
        _window = Math.Max(4, (int)((p.Bottom - p.Top - 12) / PhonePainter.RowH));
        _top = Math.Clamp(_top, 0, Math.Max(0, _rows.Count - _window));
        var n = Math.Min(_window, _rows.Count - _top);
        var card = p.Card(p.Top, n);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        for (var r = 0; r < n; r++)
        {
            var row = _rows[_top + r];
            var y = p.RowY(card, r);
            if (row.Header)
            {
                if (r > 0) p.Divider(card, r);
                p.Text(card.Position.X + 6, y, row.Text, Ink.Dim);
                continue;
            }
            if (row.Bar >= 0f) // pasek postępu: tor i wypełnienie
            {
                var track = new Rect2(tx, y + PhonePainter.RowH / 2f - 5, right - tx, 10);
                p.C.DrawRect(track, Pal.Border);
                if (row.Bar > 0f) p.C.DrawRect(new Rect2(track.Position, new Vector2(Mathf.Max(3, track.Size.X * row.Bar), track.Size.Y)), row.BarColor);
                continue;
            }
            if (row.Stripe.A > 0) p.Stripe(card, r, row.Stripe);
            var room = right - tx;
            if (row.Tail.Length > 0) room -= p.Pill(right, y, row.Tail, PillKind.Gray) + 4;
            p.Text(tx, y, row.Text, row.Ink, TextAlign.Left, room);
        }
        var half = card.Size.Y / 2;
        p.Hit(new Rect2(card.Position, new Vector2(card.Size.X, half)), -1);
        p.Hit(new Rect2(card.Position + new Vector2(0, half), new Vector2(card.Size.X, half)), -2);
        if (_top > 0) p.Text(right, card.Position.Y - 2, "^", Ink.Dim, TextAlign.Right);
        if (_top + n < _rows.Count) p.Text(right, card.End.Y - 14, "v", Ink.Dim, TextAlign.Right);
    }
}
