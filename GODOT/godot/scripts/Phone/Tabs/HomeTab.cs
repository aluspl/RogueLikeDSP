using System;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Tabs;

/// <summary>
/// Start = pulpit postaci (tab_home na GBA): etap, HP z paskiem, poziom z doświadczeniem, moc z gotowością, stany,
/// statystyki (A: opis statystyk i skąd premie); do tego wynik, dzień, budżet i najbliższe zlecenie z postępem na żywo.
/// </summary>
public sealed class HomeTab : PhonePage
{
    private readonly CoreGame _g;
    private readonly Profile _p;
    private readonly Action _stats;

    public HomeTab(CoreGame g, Profile p, Action stats = null)
    {
        _g = g;
        _p = p;
        _stats = stats;
    }

    /// <summary>Opis statystyk i skąd są premie (Spacja / „Opis”).</summary>
    public override PageAction[] Actions => _stats is null ? [] : [new(Loc.T("opis_statystyk"), GameAction.A)];

    public override bool Input(InputCmd e)
    {
        if (_stats is null || !e.Is(GameAction.A)) return false;
        _stats();
        return true;
    }

    public override string Title => Loc.T("start");
    public override string Sub => Loc.F("zl_6", _g.DDef.Name, (_g.Tier > 0 ? $" NG+{_g.Tier}" : ""), _g.Cash);

    public override void Draw(PhonePainter p)
    {
        var g = _g;
        var d = g.D;
        var y = p.Section(p.Top, g.CDef.Name.ToUpperInvariant(), Loc.F("dzien_2", g.Turns));
        var card = p.Card(y, 7);
        var tx = p.TextX(card);
        var right = card.End.X - 6;

        var r = p.RowY(card, 0);
        p.Stripe(card, 0, Pal.Prog);
        var pw = p.Pill(right, r, Loc.T("w_trakcie"), PillKind.Prog);
        p.Text(tx, r, Loc.F("etap_3", g.StageNumber(), g.SDef().Name), Ink.Dark, TextAlign.Left, right - pw - 4 - tx);

        r = p.RowY(card, 1);
        p.Divider(card, 1);
        p.Text(tx, r, $"HP {g.Hero.Hp}/{g.Hero.MaxHp}", Ink.Dark);
        var hpc = g.Hero.Hp * 2 > g.Hero.MaxHp ? Pal.Done : g.Hero.Hp * 4 > g.Hero.MaxHp ? Pal.Prog : Pal.Late;
        p.Bar(tx + 80, r, right - tx - 80, g.Hero.Hp, g.Hero.MaxHp, hpc);

        r = p.RowY(card, 2);
        p.Divider(card, 2);
        p.Text(tx, r, Loc.F("poziom_5", g.HeroLevel), Ink.Dark);
        var prev = g.HeroLevel >= 2 ? d.LevelThresholds[g.HeroLevel - 2] : 0;
        if (g.XpToNext() < 0) p.Bar(tx + 80, r, right - tx - 80, 1, 1, Pal.Brand);
        else p.Bar(tx + 80, r, right - tx - 80, g.RunXp - prev, d.LevelThresholds[g.HeroLevel - 1] - prev, Pal.Brand);

        r = p.RowY(card, 3);
        p.Divider(card, 3);
        var ready = g.AbilityCd == 0;
        pw = p.Pill(right, r, ready ? Loc.T("gotowa") : $"za {g.AbilityCd}", ready ? PillKind.Done : PillKind.Gray);
        p.Text(tx, r, Loc.T("moc_4") + UiText.AbilityLabel(g), Ink.Dark, TextAlign.Left, right - pw - 4 - tx);

        r = p.RowY(card, 4);
        p.Divider(card, 4);
        var sl = UiText.StatusLine(g, out var bad);
        p.Text(tx, r, sl, bad ? Ink.Late : Ink.Dim, TextAlign.Left, right - tx);

        r = p.RowY(card, 5);
        p.Divider(card, 5);
        pw = _stats is null ? 0 : p.Pill(right, r, ButtonNames.Localize(Loc.T("a_opis")), PillKind.Group);
        p.Text(tx, r, UiText.HeroStatsLine(g), Ink.Dim, TextAlign.Left, right - pw - 4 - tx);

        r = p.RowY(card, 6);
        p.Divider(card, 6);
        var w = g.Weapon;
        p.Text(tx, r, $"{w.Name} {w.MinDamage}-{w.MaxDamage} z{w.Range}", Ink.Dim, TextAlign.Left, right - tx);

        y = card.End.Y + 4;
        var c2 = p.Card(y, 2);
        tx = p.TextX(c2);
        r = p.RowY(c2, 0);
        p.Text(tx, r, Loc.F("wynik_5", g.Score), Ink.Dark);
        p.Text(right, r, Loc.F("usuniete_2", g.Kills), Ink.Dim, TextAlign.Right);
        r = p.RowY(c2, 1);
        p.Divider(c2, 1);
        var ci = Meta.NextContract(d, _p, g);
        if (ci >= 0)
        {
            var c = d.Contracts[ci];
            p.Stripe(c2, 1, Pal.Brand);
            pw = p.Pill(right, r, $"{Math.Min(c.Target, Meta.ContractProgressLive(d, _p, g, ci))}/{c.Target}", PillKind.Prog);
            p.Text(tx, r, Loc.T("zlecenie") + c.Name, Ink.Dim, TextAlign.Left, right - pw - 4 - tx);
        }
        else
        {
            p.Text(tx, r, Loc.T("wszystkie_zlecenia_wykonane"), Ink.Done);
        }
    }
}
