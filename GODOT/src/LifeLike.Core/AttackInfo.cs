using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// v0.23.0: opis ataku pod przyciskiem Cios / Patent (przytrzymanie, najechanie myszą, LB na padzie) dla broni w ręku:
/// tytuł (Cios albo nazwa ruchu Patentu), nagłówek („Patent · Kielnia”, przy Ciosie sama broń), wiersze (typ ciosu, obrażenia z % trybu, ciosy w cel,
/// wzór i zasięg, efekt, odnowienie, rzadkość i profity, które zmieniają ruch) oraz mini-siatka wzoru: bohater w środku
/// zwrócony w prawo, pola trafione (Hit) i zasięg trybu (Reach). Siatka z HitPattern.Cells – tych samych pól, co atak.
/// </summary>
public sealed class AttackInfo
{
    public const byte Empty = 0, Reach = 1, Hit = 2, Hero = 3;

    /// <summary>Największy promień siatki (9x9 pól).</summary>
    public const int MaxRadius = 4;

    private byte[] _grid = [];

    public string Title { get; private set; } = "";

    public string Header { get; private set; } = "";

    public bool ModeB { get; private set; }

    public List<string> Lines { get; } = new();

    /// <summary>Promień siatki: od -Radius do +Radius pól wokół bohatera (2..4).</summary>
    public int Radius { get; private set; } = 2;

    public int Side => 2 * Radius + 1;

    /// <summary>Pole siatki względem bohatera (dx w prawo = kierunek ataku); poza siatką – Empty.</summary>
    public byte Cell(int dx, int dy) =>
        Math.Abs(dx) > Radius || Math.Abs(dy) > Radius ? Empty : _grid[(dy + Radius) * Side + dx + Radius];

    public static AttackInfo For(Game g, bool modeB)
    {
        var d = g.D;
        var w = g.Weapon;
        var m = modeB ? w.ModeB : w.ModeA;
        var b = g.WeaponBreakdown();
        var a = new AttackInfo { ModeB = modeB };
        var kind = modeB ? Loc.T("cios_b") : Loc.T("tip_cios");
        a.Title = modeB ? ModeText.BName(w) : kind;
        a.Header = modeB ? $"{kind} · {g.WeaponTitle()}" : g.WeaponTitle(); // Cios: tytuł już mówi „Cios”

        a.Lines.Add(Loc.T("typ_ciosu") + ModeText.Hits(d, w));
        a.Lines.Add(m.DmgPct == 100
            ? Loc.F("tip_obrazenia", b.Min, b.Max)
            : Loc.F("tip_obrazenia_pct", HitCalc.Apply(b.Min, m.DmgPct), HitCalc.Apply(b.Max, m.DmgPct), m.DmgPct));
        if (m.Hits > 1) a.Lines.Add(Loc.F("tip_ciosy", m.Hits));
        var range = g.ModeRange(m);
        a.Lines.Add(Loc.F("tip_wzor", ModeText.Pattern(m with { Range = range }), range));
        var eff = ModeText.Effects(w, m);
        if (eff.Length > 0) a.Lines.Add(ModeText.Capital(eff));
        var pr = w.Proc; // v0.23.0: artefakt – co N-ty cios i licznik
        if (pr is not null) a.Lines.Add(pr.Every <= 1 ? Loc.F("tip_proc_zawsze", pr.Name) : Loc.F("tip_proc", pr.Name, pr.Every, g.ProcCount));
        if (modeB && w.WallFree && (m.Effect == ModeEffect.OpenWall || m.Effect2 == ModeEffect.OpenWall)) a.Lines.Add(Loc.T("tip_sciana_bez_odn"));
        var mat = w.BonusPct != 0 ? Array.FindIndex(d.Materials, x => x.Id == w.BonusMaterial) : -1;
        if (mat >= 0) a.Lines.Add(Loc.F("tip_material", w.BonusPct, d.Materials[mat].Name));
        if (modeB)
        {
            var cd = Math.Max(1, m.Cooldown + g.WeaponBCdDelta());
            a.Lines.Add(g.WeaponBCd > 0 ? Loc.F("tip_odnowienie_za", cd, g.WeaponBCd) : Loc.F("tip_odnowienie", cd));
            var rd = g.WeaponRarity < d.WeaponRarities.Length ? d.WeaponRarities[g.WeaponRarity].CooldownDelta : 0;
            if (rd != 0) a.Lines.Add(Loc.F("tip_rzadkosc_odn", d.WeaponRarities[g.WeaponRarity].Name, rd > 0 ? $"+{rd}" : rd.ToString()));
        }
        for (var i = 0; i < d.WeaponPerkPool.Length; i++)
        {
            var pk = d.WeaponPerkPool[i];
            if ((b.Perks & (1 << i)) == 0 || (!modeB && pk.Effect == ToolTraitEffect.CooldownB)) continue;
            a.Lines.Add($"{pk.Name}: {pk.Short}");
        }
        a.BuildGrid(m, range);
        return a;
    }

    private void BuildGrid(WeaponMode m, int range)
    {
        Span<int> xs = stackalloc int[HitPattern.MaxCells];
        Span<int> ys = stackalloc int[HitPattern.MaxCells];
        int hx = Level.W / 2, hy = Level.H / 2;
        var n = HitPattern.Cells(m.Pattern, range, hx, hy, 1, 0, xs, ys);
        var r = Math.Max(2, range);
        for (var i = 0; i < n; i++) r = Math.Max(r, Math.Max(Math.Abs(xs[i] - hx), Math.Abs(ys[i] - hy)));
        Radius = Math.Min(MaxRadius, r);
        _grid = new byte[Side * Side];
        for (var dy = -Radius; dy <= Radius; dy++)
        {
            for (var dx = -Radius; dx <= Radius; dx++)
            {
                if (Math.Max(Math.Abs(dx), Math.Abs(dy)) <= range) _grid[(dy + Radius) * Side + dx + Radius] = Reach;
            }
        }
        for (var i = 0; i < n; i++)
        {
            int dx = xs[i] - hx, dy = ys[i] - hy;
            if (Math.Abs(dx) <= Radius && Math.Abs(dy) <= Radius) _grid[(dy + Radius) * Side + dx + Radius] = Hit;
        }
        _grid[Radius * Side + Radius] = Hero;
    }
}
