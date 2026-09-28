using LifeLike.Core.Data;

namespace LifeLike.Core;

// Rozpiska obrażeń broni (#26) – port game::weapon_breakdown, power_dmg_bonus, enemy_hit z core.h.
public sealed partial class Game
{
    /// <summary>
    /// Rozpiska obrażeń broni – wzory jak HeroAttack. enemyDefId: problem (jego obrona; -1 = bez), weaponIdx: inna broń
    /// (-1 = obecna), swap*: sprzęt w slocie po zamianie (porównanie paczki; -1 = bez zmian).
    /// </summary>
    public DmgBreakdown WeaponBreakdown(int enemyDefId = -1, int weaponIdx = -1, int swapSlot = -1, int swapRarity = -1, int swapTrait = 0)
    {
        var b = new DmgBreakdown { Weapon = weaponIdx >= 0 ? weaponIdx : (WeaponOverride >= 0 ? WeaponOverride : CDef.Weapon) };
        var w = D.Weapons[b.Weapon];
        b.WMin = w.MinDamage;
        b.WMax = w.MaxDamage;
        b.RangeBase = w.Range;
        b.Range = RangeOf(w);
        b.Scales = w.ScalesWith;
        var stTr = RunMods.StatTrait(w.ScalesWith);
        var luckT = 0;
        for (var i = 0; i < D.GearSlotsCount; ++i)
        {
            int rar = i == swapSlot ? swapRarity : Equipped[i], tr = i == swapSlot ? swapTrait : EquippedTrait[i];
            if (rar < 0) continue;
            var gd = D.Gear[i * 3 + rar];
            if (gd.Stat == GearStat.Dmg)
            {
                b.FlatGear += gd.Value;
                b.GearItem = i * 3 + rar;
            }
            var td = D.GearTraits[tr];
            if (td.Effect == TraitEffect.Luck) luckT += td.Value;
            else if (td.Effect == TraitEffect.Crit) b.CritTrait += td.Value;
            else if (td.Effect == stTr) b.StatTrait += td.Value;
        }
        b.StatClass = RunMods.ClassBaseStat(D, Cls, w.ScalesWith);
        b.StatCraft = RunMods.StatBonus(D, Bonus, Cls, w.ScalesWith);
        b.FlatMods = Bonus.Dmg;
        b.FlatLevel = DamageHelp.LevelDmg(D, HeroLevel);
        b.FlatFound = DmgBonus - Bonus.Dmg - b.FlatLevel;
        b.Pct = Bonus.DmgPct;
        b.FlatBoon = BoonSum(BoonEffect.Dmg);
        b.PctBoon = BoonSum(BoonEffect.DmgPct);
        b.CritBoon = BoonSum(BoonEffect.Crit);
        b.VsEnemy = enemyDefId >= 0;
        b.EnemyDef = b.VsEnemy ? D.Enemies[enemyDefId].Defense : 0;
        b.Luck = CDef.Luck + Bonus.Luck + luckT + BoonLuck();
        b.CritBonus = Bonus.Crit;
        b.PowerRank = AbilityRank();
        b.Power = PowerDmgBonus();
        b.Finish(D);
        return b;
    }

    /// <summary>Rozpiska przeciw konkretnemu problemowi na planszy (karta problemu): z obroną elity (Tarcza).</summary>
    public DmgBreakdown ActorBreakdown(int ei)
    {
        var b = WeaponBreakdown(Enemies[ei].DefId);
        b.EnemyElite = EnemyEliteDef(ei);
        b.Elite = Enemies[ei].Elite;
        b.Finish(D);
        return b;
    }

    /// <summary>Premia mocy do ciosu: Seria i Rynna +1 od rangi II, Taran +ranga; premie zawodów: Wirówka, Taran.</summary>
    public int PowerDmgBonus()
    {
        var rank = AbilityRank();
        switch (CDef.Ability)
        {
            case AbilityEffect.Volley:
            case AbilityEffect.Line:
                return rank >= 2 ? 1 : 0;
            case AbilityEffect.Ram:
                return rank + BoonPower();
            case AbilityEffect.Spin:
                return BoonPower();
            default:
                return 0;
        }
    }

    /// <summary>Cios problemu ei w bohatera (zakres po OBR i -%).</summary>
    public HitRange EnemyHit(int ei)
    {
        var e = Enemies[ei];
        var ed = D.Enemies[e.DefId];
        return DamageHelp.EnemyHitRange(ed.MinDamage, ed.MaxDamage, EnemyBonus(e), HeroDefense(), Bonus.TakenPct);
    }

    /// <summary>Premia obrażeń problemu: etap, trudność, wzrost, elita.</summary>
    public int EnemyBonus(in Actor e) => EnemyDmgBonus() + e.Grow / 2 + (e.Elite >= 0 ? D.EliteDmg : 0);
}
