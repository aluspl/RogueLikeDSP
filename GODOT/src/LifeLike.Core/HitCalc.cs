using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// v0.23.0: mnożnik obrażeń ciosu (w procentach) wg typu ciosu broni (obuch/cięcie/kłucie) i celu: kolumna „woda”, gdy problem
/// jest wodny (pierwszeństwo przed materiałem), inaczej jego materiał (cement/stal/drewno), inaczej „bez materiału” (boss bez
/// materiału = 100%, chyba że ma żywioł wody – Nawałnica i Zawilgocony strych liczą się z kolumny „woda”: słabość żywiołu, jak prąd
/// na mokre). Broń z dwoma typami bierze lepszy. Potem żywioł broni: iskra na stal, prąd na wodę, ogień na drewno / wodę.
/// </summary>
public static class HitCalc
{
    /// <summary>Kolumna w MaterialPct dla problemu: 0..2 materiał, 3 woda, 4 bez materiału.</summary>
    public static int Column(GameData d, EnemyDef e)
    {
        if (e.Elem == Element.Water) return 3;
        return e.Material >= 0 && e.Material < d.Materials.Length ? e.Material : 4;
    }

    public static bool Has(WeaponDef w, HitType t)
    {
        foreach (var h in w.Hit)
        {
            if (h == t) return true;
        }
        return false;
    }

    /// <summary>Lepszy z typów ciosu broni na kolumnie col (przy remisie wcześniejszy).</summary>
    public static HitType Best(GameData d, WeaponDef w, int col)
    {
        var best = w.Hit[0];
        foreach (var h in w.Hit)
        {
            if (d.HitTypes.MaterialPct[(int)h, col] > d.HitTypes.MaterialPct[(int)best, col]) best = h;
        }
        return best;
    }

    /// <summary>Procent obrażeń ciosu broni w problem (100 = bez zmian).</summary>
    public static int Pct(GameData d, WeaponDef w, EnemyDef e)
    {
        if (w.Neutral) return 100; // v0.23.0 (#84): czyste kłucie – materiał bez znaczenia
        var col = Column(d, e);
        var pct = d.HitTypes.MaterialPct[(int)Best(d, w, col), col];
        var steel = col == 1;
        var wood = col == 2;
        var water = col == 3;
        if (w.BonusPct != 0 && col < d.Materials.Length && d.Materials[col].Id == w.BonusMaterial) pct = pct * (100 + w.BonusPct) / 100; // v0.23.0: artefakt (Łom Ostatniej Deski)
        switch (w.Elem)
        {
            case Element.Spark:
                if (steel) pct = pct * d.HitTypes.SparkSteelPct / 100;
                break;
            case Element.Power:
                if (water) pct = pct * d.HitTypes.PowerWaterPct / 100;
                break;
            case Element.Fire:
                if (wood) pct = pct * d.HitTypes.FireWoodPct / 100;
                else if (water) pct = pct * d.HitTypes.FireWaterPct / 100;
                break;
        }
        return pct;
    }

    /// <summary>v0.23.0 (#84): ile OBR omija kłucie broni (własna wartość broni albo HitTypes.PierceDef; bez kłucia 0).</summary>
    public static int PierceDef(GameData d, WeaponDef w) => Has(w, HitType.Pierce) ? (w.Pierce > 0 ? w.Pierce : d.HitTypes.PierceDef) : 0;

    /// <summary>Obrażenia po mnożniku (najmniej 1).</summary>
    public static int Apply(int dmg, int pct) => Math.Max(1, dmg * pct / 100);
}
