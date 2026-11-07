using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// v0.23.0: tryby broni słowami – wzór pól („łuk 3 pól”, „linia 2”), efekt („ogłuszenie (1 t.)”), cały tryb
/// („linia 2, -2 OBR (3 t.), odn. 4”) i nazwa ruchu Patentu (tryb B; pusta w danych – ogólne „Patent”).
/// Wspólne dla telefonu, wyboru zawodu, celowania i opisu ataku pod przyciskiem (AttackInfo).
/// </summary>
public static class ModeText
{
    /// <summary>Nazwa ruchu Patentu broni („Rzut zaprawą”); bez nazwy w danych – „Patent”.</summary>
    public static string BName(WeaponDef w) => w.ModeB.Name.Length > 0 ? w.ModeB.Name : Loc.T("cios_b");

    /// <summary>Typy ciosu broni: „Obuch”, „Cięcie/Kłucie”.</summary>
    public static string Hits(GameData d, WeaponDef w)
    {
        var parts = new List<string>();
        foreach (var h in w.Hit) parts.Add(d.HitTypes.Names[(int)h]);
        return string.Join("/", parts);
    }

    /// <summary>Wzór pól trybu: „łuk 3 pól”, „linia 2”, „strzał 3”, „rzut 3x3”.</summary>
    public static string Pattern(WeaponMode m) => m.Pattern switch
    {
        PatternKind.Single => Loc.T("wzor_single"),
        PatternKind.Arc => Loc.T("wzor_arc"),
        PatternKind.Around => Loc.T("wzor_around"),
        PatternKind.Cross => Loc.T("wzor_cross"),
        PatternKind.Line => Loc.F("wzor_line", m.Range),
        PatternKind.Shot => Loc.F("wzor_shot", m.Range),
        PatternKind.Throw3x3 => Loc.F("wzor_throw", m.Range),
        PatternKind.Spray => Loc.T("wzor_spray"),
        PatternKind.Push => Loc.T("wzor_push"),
        PatternKind.LineBoth => Loc.F("wzor_line_both", m.Range),
        PatternKind.Line4 => Loc.F("wzor_line4", m.Range),
        _ => "",
    };

    /// <summary>Efekt trybu słowami („ogłuszenie 20%”, „-2 OBR (3 t.)”, „otwiera ścianę”); pusty, gdy brak.</summary>
    public static string Effect(WeaponDef w, ModeEffect e, int val, int turns)
    {
        var t = turns > 0 ? Loc.F("ef_tury", turns) : "";
        return e switch
        {
            ModeEffect.Stun => (val is > 0 and < 100 ? Loc.F("ef_stun_szansa", val) : Loc.T("ef_stun")) + t,
            ModeEffect.Root => Loc.T("ef_root") + t,
            ModeEffect.Push => Loc.F("ef_push", Math.Max(1, val)),
            ModeEffect.Pull => Loc.T("ef_pull"),
            ModeEffect.DefDown => Loc.F("ef_defdown", val) + t,
            ModeEffect.Dot => (HitCalc.Has(w, HitType.Cut) && w.Elem != Element.Fire ? Loc.T("ef_bleed") : Loc.T("ef_burn")) + t,
            ModeEffect.Mark => Loc.F("ef_mark", val) + t,
            ModeEffect.Reveal => Loc.T("ef_reveal"),
            ModeEffect.OpenWall => val > 0 ? Loc.F("ef_openwall_ladunki", val) : Loc.T("ef_openwall"),
            ModeEffect.Swap => Loc.T("ef_swap"),
            ModeEffect.Chain => Loc.T("ef_chain"),
            ModeEffect.Dry => Loc.T("ef_dry"),
            ModeEffect.CritNext => Loc.T("ef_critnext"),
            ModeEffect.Wet => Loc.T("ef_wet"),
            ModeEffect.Ignite => Loc.T("ef_ignite"),
            _ => "",
        };
    }

    /// <summary>Oba efekty trybu po przecinku („odrzut 2, otwiera pękniętą ścianę”); pusty, gdy brak.</summary>
    public static string Effects(WeaponDef w, WeaponMode m)
    {
        var e1 = Effect(w, m.Effect, m.Value, m.Turns);
        var e2 = Effect(w, m.Effect2, m.Value2, m.Turns2);
        return e1.Length > 0 && e2.Length > 0 ? e1 + ", " + e2 : e1 + e2;
    }

    /// <summary>
    /// Tryb słowami: wzór[ 50%][ x2][, efekt][, efekt 2][, odn. N] – np. „linia 2, -2 OBR (3 t.), odn. 4” (bez nazwy ruchu).
    /// cooldown: dopisz odnowienie (Patent; rzadkość i profit Chwyt zmieniają je o delta).
    /// </summary>
    public static string Mode(WeaponDef w, WeaponMode m, bool cooldown, int cdDelta = 0)
    {
        var s = Pattern(m);
        if (m.DmgPct != 100) s += $" {m.DmgPct}%";
        if (m.Hits > 1) s += Loc.F("ciosy_x", m.Hits);
        var e = Effects(w, m);
        if (e.Length > 0) s += ", " + e;
        if (cooldown) s += Loc.F("odn_b", Math.Max(1, m.Cooldown + cdDelta));
        return s;
    }

    /// <summary>Pierwsza litera wielka („ogłuszenie” → „Ogłuszenie”).</summary>
    public static string Capital(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];
}
