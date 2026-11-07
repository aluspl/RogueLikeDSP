using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;

namespace LifeLike.Game.Gfx;

/// <summary>
/// v0.23.0: broń słowami i kolorem – rzadkość (kolor z danych weaponRarities), typy ciosu („Obuch/Kłucie”), tryby A i B
/// („łuk 3 pól”, „linia 2, -2 OBR (3 t.), odn. 4”). Wspólne dla HUD, telefonu, wyboru zawodu, oferty narzędzia i Jak grać.
/// Ikony broni w ręku – zadanie 9; tu tylko tekst i kolor.
/// </summary>
public static class WeaponLook
{
    /// <summary>Kolor rzadkości z danych (Zwykła szara, Solidna niebieska, Markowa fiolet, Mistrzowska złota).</summary>
    public static Color RarityColor(GameData d, int r) =>
        r >= 0 && r < d.WeaponRarities.Length && d.WeaponRarities[r].Color.Length > 0 ? new Color(d.WeaponRarities[r].Color) : Pal.Dim;

    public static string RarityName(GameData d, int r) => r >= 0 && r < d.WeaponRarities.Length ? d.WeaponRarities[r].Name : "";

    /// <summary>Tekst w kolorze rzadkości na jasnej karcie telefonu (przyciemniony – czytelny na bieli).</summary>
    public static Ink PhoneInk(GameData d, int r) => new(RarityColor(d, r).Darkened(r == 3 ? 0.35f : 0.15f), Colors.Transparent);

    /// <summary>Podbarwienie ikony broni: Zwykła bez zmian (szary ściemniłby ikonę), reszta – jasny kolor rzadkości.</summary>
    public static Color IconTint(GameData d, int r) => r <= 0 ? Colors.White : RarityColor(d, r).Lightened(0.35f);

    /// <summary>Tekst w kolorze rzadkości na mapie / ciemnym tle (rozjaśniony, z cieniem).</summary>
    public static Ink MapInk(GameData d, int r) => new(RarityColor(d, r).Lightened(r == 0 ? 0.35f : 0.25f), Ink.Map.Edge);

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
    public static string Effect(GameData d, WeaponDef w, ModeEffect e, int val, int turns)
    {
        var t = turns > 0 ? Loc.F("ef_tury", turns) : "";
        return e switch
        {
            ModeEffect.Stun => (val is > 0 and < 100 ? Loc.F("ef_stun_szansa", val) : Loc.T("ef_stun")) + t,
            ModeEffect.Root => Loc.T("ef_root") + t,
            ModeEffect.Push => Loc.F("ef_push", System.Math.Max(1, val)),
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

    /// <summary>
    /// Tryb słowami: [nazwa: ]wzór[ 50%][ x2][, efekt][, efekt 2][, odn. N] – np. „linia 2, -2 OBR (3 t.), odn. 4”.
    /// cooldown: dopisz odnowienie (tryb B; rzadkość broni zmienia je o delta).
    /// </summary>
    public static string Mode(GameData d, WeaponDef w, WeaponMode m, bool cooldown, int cdDelta = 0)
    {
        var s = Pattern(m);
        if (m.Name.Length > 0) s = $"{m.Name}: {s}";
        if (m.DmgPct != 100) s += $" {m.DmgPct}%";
        if (m.Hits > 1) s += Loc.F("ciosy_x", m.Hits);
        var e1 = Effect(d, w, m.Effect, m.Value, m.Turns);
        if (e1.Length > 0) s += ", " + e1;
        var e2 = Effect(d, w, m.Effect2, m.Value2, m.Turns2);
        if (e2.Length > 0) s += ", " + e2;
        if (cooldown) s += Loc.F("odn_b", System.Math.Max(1, m.Cooldown + cdDelta));
        return s;
    }

    /// <summary>Zmiana odnowienia trybu B od rzadkości (Markowa -1).</summary>
    public static int CdDelta(GameData d, int r) => r >= 0 && r < d.WeaponRarities.Length ? d.WeaponRarities[r].CooldownDelta : 0;
}
