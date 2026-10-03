using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Opis statystyk (stats_page na GBA, #19): strona 1 - wartości i co dają (wybór zawodu) albo skąd są premie
/// (w trakcie budowy), strony 2-3 - rozpiska obrażeń broni (#26, jak w BG3: cios od-do i skąd, kryt i obrona),
/// strona 4 - wzory w prostych słowach (StatHelp). A / strzałki / przycisk: strona, B / Esc: wróć.
/// </summary>
public sealed class StatsPage : PhonePage
{
    /// <summary>Strona rozpiski obrażeń (Sprzęt: I / dotknięcie narzędzia).</summary>
    public const int DamagePage = 1;
    private const int Pages = 4;
    private static string[] Titles => [Loc.T("statystyki"), Loc.T("obrazenia_broni"), Loc.T("kryt_i_obrona"), Loc.T("jak_dzialaja")];

    private readonly GameData _d;
    private readonly int _cls;
    private readonly RunMods _m;
    private readonly CoreGame _g;
    private readonly List<DamageRow> _rows;
    private int _page;

    /// <summary>g = null: wybór zawodu (baza + premie z profilu m); inaczej bieżąca budowa. p: profil (źródła premii).</summary>
    public StatsPage(GameData d, int cls, RunMods m, CoreGame g, Profile p = null, int page = 0)
    {
        _d = d;
        _cls = cls;
        _m = m;
        _g = g;
        _rows = DamageRows.Build(d, DamageRows.ForHero(d, p, g, cls, m), g);
        _page = page is >= 0 and < Pages ? page : 0;
    }

    public int Page => _page;
    public override string Title => Titles[_page];
    public override string Sub => $"{_d.Classes[_cls].Name} {_page + 1}/{Pages}";
    public override string Hint => ButtonNames.Localize(Loc.T("a_dalej_b_wroc"));
    public override PageAction[] Actions => [new(_page == Pages - 1 ? Loc.T("wartosci") : Loc.T("dalej"), GameAction.A), new(Loc.T("wroc"), GameAction.B)];
    public override bool Closable => true;

    public override bool Input(InputCmd e)
    {
        if (e.Is(GameAction.A | GameAction.Right | GameAction.Down)) _page = (_page + 1) % Pages;
        else if (e.Is(GameAction.Left | GameAction.Up)) _page = (_page + Pages - 1) % Pages;
        else return false;
        return true;
    }

    private WeaponDef Weapon => _g is not null ? _g.Weapon : _d.Weapons[_d.Classes[_cls].Weapon];

    private static StatKind KindOf(Stat s) => s == Stat.Str ? StatKind.Str : s == Stat.Agi ? StatKind.Agi : StatKind.Intel;

    public override void Draw(PhonePainter p)
    {
        if (_page is 1 or 2)
        {
            DrawDamage(p);
            return;
        }
        var lines = _page == 3 ? Rules() : _g is null ? ClassValues() : RunSources();
        var y = p.Section(p.Top, _page == 3 ? Loc.T("wzory") : _g is null ? Loc.T("co_daja") : Loc.T("skad_premie"), Loc.T("a_dalej"));
        var card = p.Card(y, lines.Count);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        for (var i = 0; i < lines.Count; i++)
        {
            var (text, ink, stripe) = lines[i];
            if (i > 0) p.Divider(card, i);
            if (stripe) p.Stripe(card, i, Pal.Brand);
            p.Text(tx, p.RowY(card, i), text, ink, TextAlign.Left, right - tx);
        }
        y = p.Section(card.End.Y + 6, Loc.T("cios_3"));
        var c2 = p.Card(y, 2);
        p.Text(p.TextX(c2), p.RowY(c2, 0), Loc.T("rzut_broni_stat_2_premie"), Ink.Dark, TextAlign.Left, right - p.TextX(c2));
        p.Divider(c2, 1);
        p.Text(p.TextX(c2), p.RowY(c2, 1), Loc.T("obrona_problemu_2_min_1_kryt"), Ink.Dim, TextAlign.Left, right - p.TextX(c2));
    }

    /// <summary>Rozpiska: strona 2 - cios (od-do i skąd), strona 3 - kryt, obrona i unik; długie wiersze zawinięte.</summary>
    private void DrawDamage(PhonePainter p)
    {
        var hit = _page == 1;
        var y = p.Section(p.Top, hit ? Loc.T("cios_od_do") : Loc.T("kryt_i_obrona_2"), hit ? Loc.T("jak_w_walce") : "");
        var width = (int)(p.Width - 24);
        var lines = new List<(string Text, Ink Ink, Color Stripe)>();
        foreach (var r in _rows)
        {
            if (DamageRows.IsHit(r) != hit) continue;
            var wrapped = p.F.Wrap(r.Text, width);
            for (var k = 0; k < wrapped.Count; k++) lines.Add(((k > 0 ? "  " : "") + wrapped[k], r.Ink, k == 0 ? r.Stripe : Colors.Transparent));
        }
        var card = p.Card(y, lines.Count);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        for (var i = 0; i < lines.Count; i++)
        {
            if (i > 0 && !lines[i].Text.StartsWith("  ")) p.Divider(card, i);
            if (lines[i].Stripe.A > 0) p.Stripe(card, i, lines[i].Stripe);
            p.Text(tx, p.RowY(card, i), lines[i].Text, lines[i].Ink, TextAlign.Left, right - tx);
        }
        if (!hit) return;
        var help = _d.DamageHelpLines;
        if (help.Length == 0 || card.End.Y + 6 + (help.Length + 1) * PhonePainter.RowH > p.Bottom) return;
        y = p.Section(card.End.Y + 6, Loc.T("w_skrocie"));
        var c2 = p.Card(y, help.Length);
        for (var i = 0; i < help.Length; i++) p.Text(p.TextX(c2), p.RowY(c2, i), help[i], Ink.Dim, TextAlign.Left, right - p.TextX(c2));
    }

    /// <summary>Wybór zawodu: wartość (baza + premie z profilu) i co daje.</summary>
    private List<(string, Ink, bool)> ClassValues()
    {
        var c = _d.Classes[_cls];
        var w = Weapon;
        var list = new List<(string, Ink, bool)>();
        (StatKind Kind, int Value, bool Weapon)[] rows =
        [
            (StatKind.Hp, c.MaxHealth + _m.Hp, false),
            (StatKind.Str, c.Strength + RunMods.StatBonus(_d, _m, _cls, Stat.Str), w.ScalesWith == Stat.Str),
            (StatKind.Agi, c.Agility + RunMods.StatBonus(_d, _m, _cls, Stat.Agi), w.ScalesWith == Stat.Agi),
            (StatKind.Intel, c.Intelligence + RunMods.StatBonus(_d, _m, _cls, Stat.Intel), w.ScalesWith == Stat.Intel),
            (StatKind.Def, c.Defense + _m.Def, false),
            (StatKind.Luck, c.Luck + _m.Luck, false),
        ];
        foreach (var (kind, value, weapon) in rows)
        {
            var m = new Message().Add(StatHelp.Name(kind)).Add(" ").Add(value).Add(": ");
            StatHelp.Effect(_d, m, kind, value, weapon);
            var dim = kind is StatKind.Str or StatKind.Agi or StatKind.Intel && !weapon;
            list.Add((m.Text, weapon ? Ink.Brand : dim ? Ink.Dim : Ink.Dark, weapon));
        }
        return list;
    }

    /// <summary>W trakcie budowy: skąd są premie (statystyka broni, obrażenia, obrona, szczęście, HP).</summary>
    private List<(string, Ink, bool)> RunSources()
    {
        var g = _g;
        var c = g.CDef;
        var w = Weapon;
        var ws = g.HeroStat(w.ScalesWith);
        var list = new List<(string, Ink, bool)>
        {
            (Loc.F("bron_obrazen", UiText.StatShort(w.ScalesWith), ws, ws / 2), Ink.Brand, true),
        };
        var src = Loc.F("zawod_5", RunMods.ClassBaseStat(_d, _cls, w.ScalesWith));
        var craft = RunMods.StatBonus(_d, g.Bonus, _cls, w.ScalesWith);
        if (craft > 0) src += Loc.F("warsztaty_3", craft);
        var trait = g.TraitBonus(RunMods.StatTrait(w.ScalesWith));
        if (trait > 0) src += Loc.F("sprzet_7", trait);
        list.Add((src, Ink.Dim, false));
        var dmg = Loc.F("obrazenia_4", g.DmgBonus);
        if (g.GearBonus(GearStat.Dmg) > 0) dmg += Loc.F("rekawice_2", g.GearBonus(GearStat.Dmg));
        if (g.Bonus.DmgPct > 0) dmg += $", +{g.Bonus.DmgPct}%";
        list.Add((dmg, Ink.Dark, false));
        var def = g.HeroDefense();
        var ds = Loc.F("obr_zawod", def, c.Defense);
        if (g.DefBonus > 0) ds += Loc.F("premie_6", g.DefBonus);
        if (g.GearBonus(GearStat.Def) > 0) ds += Loc.F("kask_3", g.GearBonus(GearStat.Def));
        ds += Loc.F("obrazen_3", def / 2);
        if (g.Bonus.TakenPct > 0) ds += $", -{g.Bonus.TakenPct}%";
        list.Add((ds, Ink.Dark, false));
        list.Add((Loc.F("szcz_kryt_unik_lupy", g.Luck(), g.CritPct(), g.DodgePct(), _d.DropPerLuckPct * g.Luck()), Ink.Dark, false));
        var lvl = _d.HpPerLevel * (g.HeroLevel - 1);
        var gear = g.GearBonus(GearStat.Hp);
        var hp = Loc.F("hp_zawod", g.Hero.MaxHp, c.MaxHealth);
        if (g.Bonus.Hp > 0) hp += Loc.F("szkolenia_2", g.Bonus.Hp);
        if (lvl > 0) hp += Loc.F("poziomy_2", lvl);
        if (gear > 0) hp += Loc.F("kamizelka_2", gear);
        var rest = g.Hero.MaxHp - c.MaxHealth - g.Bonus.Hp - lvl - gear;
        if (rest > 0) hp += Loc.F("inne_2", rest);
        list.Add((hp, Ink.Dark, false));
        return list;
    }

    /// <summary>Wzory: statystyka broni, obrona, szczęście (3 wiersze), HP.</summary>
    private List<(string, Ink, bool)> Rules()
    {
        var ws = KindOf(Weapon.ScalesWith);
        (StatKind Kind, int Part)[] rows = [(ws, 0), (StatKind.Def, 0), (StatKind.Luck, 0), (StatKind.Luck, 1), (StatKind.Luck, 2), (StatKind.Hp, 0)];
        var list = new List<(string, Ink, bool)>();
        for (var i = 0; i < rows.Length; i++)
        {
            var m = StatHelp.Rule(_d, new Message(), rows[i].Kind, rows[i].Part);
            list.Add((m.Text, i == 0 ? Ink.Brand : i is 3 or 4 ? Ink.Dim : Ink.Dark, i == 0));
        }
        list.Add((Loc.T("inne_statystyki_nie_daja"), Ink.Dim, false));
        return list;
    }
}
