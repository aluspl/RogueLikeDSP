using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// Premie z meta-progresji, stałe przez całą budowę (core::run_mods): Szkolenia, uprawnienia z odznak, pamiątka.
/// </summary>
public struct RunMods
{
    public int Hp, Def, Dmg, Coffee, Pickups;
    /// <summary>Kurs BHP II: + szczęście.</summary>
    public int Luck;
    /// <summary>Warsztaty: + do statystyki, z którą skaluje się broń zawodu.</summary>
    public int Craft;
    // uprawnienia z odznak i pamiątka (PerkEffect)
    /// <summary>Odnowienie mocy krótsze o tyle tur.</summary>
    public int Cooldown;
    /// <summary>Widzenie +.</summary>
    public int Sight;
    /// <summary>Dodatkowe miejsca w termosie.</summary>
    public int Thermos;
    /// <summary>% szansy, że drop zamieni się w narzędzie.</summary>
    public int ToolPct;
    /// <summary>% więcej doświadczenia.</summary>
    public int XpPct;
    /// <summary>Budżet na start budowy (zł).</summary>
    public int Cash;
    /// <summary>Kryt +%.</summary>
    public int Crit;
    /// <summary>Narzędzia, które mogą wypaść z wrogów (bitmaska).</summary>
    public int Tools;
    /// <summary>Brygada: fachowcy do wezwania (bitmaska GameData.Brigade).</summary>
    public int Helpers;
    /// <summary>Tryb inwestora: włączone modyfikatory (bitmaska GameData.Investor).</summary>
    public int Investor;

    public static RunMods Default(GameData d) => new() { Tools = d.StartToolsMask, Helpers = d.StartHelpersMask };

    /// <summary>Dodaje premię (add_perk).</summary>
    public void AddPerk(Perk p)
    {
        switch (p.Effect)
        {
            case PerkEffect.Hp: Hp += p.Value; break;
            case PerkEffect.Def: Def += p.Value; break;
            case PerkEffect.Dmg: Dmg += p.Value; break;
            case PerkEffect.Luck: Luck += p.Value; break;
            case PerkEffect.Cooldown: Cooldown += p.Value; break;
            case PerkEffect.Sight: Sight += p.Value; break;
            case PerkEffect.Thermos: Thermos += p.Value; break;
            case PerkEffect.ToolPct: ToolPct += p.Value; break;
            case PerkEffect.XpPct: XpPct += p.Value; break;
            case PerkEffect.Cash: Cash += p.Value; break;
            case PerkEffect.Crit: Crit += p.Value; break;
            case PerkEffect.Coffee: Coffee += p.Value; break;
        }
    }

    /// <summary>Opis premii dla gracza, np. „+2 max HP”, „Moc -1 t. odnowienia” (perk_label).</summary>
    public static Message PerkLabel(Message m, Perk p)
    {
        var v = p.Value;
        return p.Effect switch
        {
            PerkEffect.Hp => m.Add("+").Add(v).Add(" max HP"),
            PerkEffect.Def => m.Add("+").Add(v).Add(" obrony"),
            PerkEffect.Dmg => m.Add("+").Add(v).Add(" obrażeń"),
            PerkEffect.Luck => m.Add("+").Add(v).Add(" szczęścia"),
            PerkEffect.Cooldown => m.Add("Moc -").Add(v).Add(" t. odnowienia"),
            PerkEffect.Sight => m.Add("Widzenie +").Add(v),
            PerkEffect.Thermos => m.Add("Termos +").Add(v).Add(v == 1 ? " miejsce" : " miejsca"),
            PerkEffect.ToolPct => m.Add("+").Add(v).Add("% szans na narzędzie"),
            PerkEffect.XpPct => m.Add("+").Add(v).Add("% doświadczenia"),
            PerkEffect.Cash => m.Add("+").Add(v).Add(" zł na start"),
            PerkEffect.Crit => m.Add("Kryt +").Add(v).Add("%"),
            PerkEffect.Coffee => m.Add("Kawa +").Add(v).Add(" HP"),
            _ => m,
        };
    }

    public static string PerkLabel(Perk p) => PerkLabel(new Message(), p).Text;

    /// <summary>Statystyka bazowa zawodu (class_base_stat).</summary>
    public static int ClassBaseStat(GameData d, int cls, Stat s)
    {
        var c = d.Classes[cls];
        return s == Stat.Str ? c.Strength : (s == Stat.Agi ? c.Agility : c.Intelligence);
    }

    /// <summary>Premia z meta-progresji do statystyki zawodu: Warsztaty działają na statystykę broni zawodu (mods_stat_bonus).</summary>
    public static int StatBonus(GameData d, in RunMods m, int cls, Stat s) =>
        d.Weapons[d.Classes[cls].Weapon].ScalesWith == s ? m.Craft : 0;

    /// <summary>Cecha sprzętu odpowiadająca statystyce (stat_trait).</summary>
    public static TraitEffect StatTrait(Stat s) =>
        s == Stat.Str ? TraitEffect.Str : (s == Stat.Agi ? TraitEffect.Agi : TraitEffect.Intel);
}
