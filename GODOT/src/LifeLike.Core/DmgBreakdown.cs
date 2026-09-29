using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// Rozpiska obrażeń broni (#26, jak w D&amp;D / BG3; port core::dmg_breakdown). Zakres ciosu liczony tymi samymi wzorami
/// co Game.HeroAttack, bez losowania: rzut broni + statystyka broni / 2 (w dół) + premie płaskie - obrona problemu / 2
/// (w dół), najmniej 1; potem +% przez Pct.Part (część w dół, reszta przechodzi na następny cios – pojedynczy cios dostaje
/// ją w dół albo w górę, więc zakres ma oba skraje); kryt mnoży wynik po procencie.
/// </summary>
public sealed class DmgBreakdown
{
    public const int Sources = 4;

    /// <summary>Indeks w GameData.Weapons.</summary>
    public int Weapon;
    public int WMin, WMax;
    /// <summary>Zasięg z pogodą i zasięg broni.</summary>
    public int Range = 1, RangeBase = 1;
    public Stat Scales = Stat.Str;
    /// <summary>Statystyka broni = zawód + Warsztaty + cechy sprzętu.</summary>
    public int StatClass, StatCraft, StatTrait;
    /// <summary>Premie stałe z profilu (Szkolenia, odznaki: RunMods.Dmg).</summary>
    public int FlatMods;
    /// <summary>Awanse (DmgLevelsMask).</summary>
    public int FlatLevel;
    /// <summary>Z budowy: projekty wykonawcze (reszta DmgBonus).</summary>
    public int FlatFound;
    /// <summary>Sprzęt +obrażenia (rękawice) i przedmiot w GameData.Gear (-1 = brak).</summary>
    public int FlatGear, GearItem = -1;
    /// <summary>+% (Kurs fachowy, Respekt).</summary>
    public int Pct;
    /// <summary>v0.21.50: premie po etapach (osobno od premii profilu): płaskie, %, kryt.</summary>
    public int FlatBoon, PctBoon, CritBoon;
    public bool VsEnemy;
    public int EnemyDef;
    /// <summary>v0.21.50: + obrona elity (Tarcza) i cecha elity (GameData.Elites, -1 = brak).</summary>
    public int EnemyElite, Elite = -1;
    public int Luck, CritTrait, CritBonus;
    /// <summary>Moc dodaje do ciosu (Seria, Rynna od II, Taran +ranga) i ranga mocy.</summary>
    public int Power, PowerRank = 1;
    /// <summary>v0.21.50 cz. 3: ulepszenie narzędzia (#31) – poziom, +obrażeń, cecha (GameData.ToolTraits, -1 = brak).</summary>
    public int UpgLevel, FlatUpgrade, UpgTrait = -1;
    /// <summary>Cecha ulepszenia: -OBR problemu (Przebicie), +najsłabszy rzut (Wyważenie), +kryt (Ostrze).</summary>
    public int Pierce, Steady, CritUpg;
    /// <summary>v0.21.51 cz. 2: kryt broni (Poziomica mistrza).</summary>
    public int CritWeapon;
    /// <summary>Wydarzenie z wyborem (#30): ciosy +N na etap.</summary>
    public int FlatEvent;
    /// <summary>Źródła premii profilu znane (Src*): Szkolenia, Respekt, odznaki, pamiątka.</summary>
    public bool Split;
    public readonly int[] SrcDmg = new int[Sources], SrcPct = new int[Sources], SrcCrit = new int[Sources];

    // wyliczone w Finish()
    public int StatValue, StatDmg, Flat, DefCut, RollMin;
    /// <summary>Procent łącznie (profil + premie po etapach).</summary>
    public int PctTotal;
    public int BaseMin, BaseMax;
    public int Min, Max;
    /// <summary>Średni cios bez krytu x10.</summary>
    public int Avg10;
    public int CritBase, CritLuck, CritPct, CritMult = 1, CritMin, CritMax;

    public void Finish(GameData d)
    {
        StatValue = StatClass + StatCraft + StatTrait;
        StatDmg = StatValue / 2;
        Flat = FlatMods + FlatLevel + FlatFound + FlatGear + FlatBoon + FlatUpgrade + FlatEvent;
        DefCut = Math.Max(0, EnemyDef + EnemyElite - Pierce) / 2;
        PctTotal = Pct + PctBoon;
        RollMin = Math.Min(WMax, WMin + Steady); // Wyważenie: najsłabszy rzut wyżej
        var add = StatDmg + Flat - DefCut;
        BaseMin = Math.Max(1, RollMin + add);
        BaseMax = Math.Max(1, WMax + add);
        Min = BaseMin + DamageHelp.PctFloor(BaseMin, PctTotal);
        Max = BaseMax + DamageHelp.PctCeil(BaseMax, PctTotal);
        int sum = 0, n = 0;
        for (var r = RollMin; r <= WMax; ++r, ++n) sum += Math.Max(1, r + add) * (100 + Math.Max(0, PctTotal));
        Avg10 = n > 0 ? LifeLike.Core.Pct.DivRound(sum, 10 * n) : 0;
        CritBase = d.CritBasePct;
        CritLuck = d.CritPerLuckPct * Luck;
        CritPct = CritBase + CritLuck + CritTrait + CritBonus + CritBoon + CritUpg + CritWeapon;
        CritMult = d.CritMultiplier;
        CritMin = Min * CritMult;
        CritMax = Max * CritMult;
    }

    public int CritChance() => Math.Max(0, Math.Min(100, CritPct));

    /// <summary>Źródła premii profilu (Meta.ModsParts); tylko gdy sumy się zgadzają (nie budowa dnia).</summary>
    public void SetSources(RunMods[] parts)
    {
        int dm = 0, p = 0, c = 0;
        for (var s = 0; s < Sources; ++s)
        {
            SrcDmg[s] = parts[s].Dmg;
            SrcPct[s] = parts[s].DmgPct;
            SrcCrit[s] = parts[s].Crit;
            dm += parts[s].Dmg;
            p += parts[s].DmgPct;
            c += parts[s].Crit;
        }
        Split = dm == FlatMods && p == Pct && c == CritBonus;
    }

    /// <summary>Rozpiska dla zawodu przed budową (wybór zawodu): broń zawodu, premie z profilu, bez sprzętu i awansów.</summary>
    public static DmgBreakdown ForClass(GameData d, int cls, RunMods m, int enemyDef = -1)
    {
        var c = d.Classes[cls];
        var w = d.Weapons[c.Weapon];
        var b = new DmgBreakdown
        {
            Weapon = c.Weapon,
            WMin = w.MinDamage,
            WMax = w.MaxDamage,
            Range = w.Range,
            RangeBase = w.Range,
            Scales = w.ScalesWith,
            StatClass = RunMods.ClassBaseStat(d, cls, w.ScalesWith),
            StatCraft = RunMods.StatBonus(d, m, cls, w.ScalesWith),
            FlatMods = m.Dmg,
            Pct = m.DmgPct,
            VsEnemy = enemyDef >= 0,
            EnemyDef = Math.Max(0, enemyDef),
            Luck = c.Luck + m.Luck,
            CritBonus = m.Crit,
            CritWeapon = w.Crit,
        };
        b.Finish(d);
        return b;
    }
}
