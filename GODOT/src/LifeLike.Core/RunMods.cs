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
    // v0.21.49: procentowe premie (Szkolenia BHP i Kurs fachowy, Respekt), nagrody za odbiór
    /// <summary>+% zadawanych obrażeń.</summary>
    public int DmgPct;
    /// <summary>-% otrzymanych obrażeń.</summary>
    public int TakenPct;
    /// <summary>+ do rzutu na jakość sprzętu z paczek.</summary>
    public int GearPct;
    /// <summary>Unik +% (łącznie maks. GameData.DodgeMaxPct).</summary>
    public int Dodge;
    /// <summary>Kawa leczy +%.</summary>
    public int CoffeePct;
    /// <summary>Brygada taniej o %.</summary>
    public int BrigadePct;
    /// <summary>Hurtownia (zł) taniej o %.</summary>
    public int ShopPct;
    /// <summary>Materiały z problemów częściej o %.</summary>
    public int MatsPct;
    /// <summary>Druga szansa: raz na budowę 1 HP zamiast końca.</summary>
    public int SecondChance;
    /// <summary>Sloty sprzętu w dropach (bitmaska; nagrody: buty, pas).</summary>
    public int GearSlots;
    /// <summary>v0.21.49: Akt 0 (Papierologia) z nagrody za odbiór – budowa zaczyna się od niego (1 = tak).</summary>
    public int Act0;
    /// <summary>v0.21.50: Respekt Druga oferta – darmowe losowanie premii po etapie.</summary>
    public int Rerolls;
    /// <summary>v0.21.50 cz. 4: wyzwanie tygodnia + 1 (0 = zwykła budowa – domyślna wartość struktury).</summary>
    public int WeeklyPlus1;
    /// <summary>v0.21.51 cz. 2: Respekt Zaprawiony w boju – kawy w termosie na start.</summary>
    public int StartCoffee;

    /// <summary>Wyzwanie tygodnia (GameData.Weekly), -1 = zwykła budowa (jak run_mods::weekly na GBA).</summary>
    public int Weekly
    {
        readonly get => WeeklyPlus1 - 1;
        set => WeeklyPlus1 = value + 1;
    }

    public static RunMods Default(GameData d) => new() { Tools = d.StartToolsMask, Helpers = d.StartHelpersMask, GearSlots = d.GearBaseMask };

    /// <summary>Premia z rangi Respektu (add_respect; wartość łączna rangi).</summary>
    public void AddRespect(RespectEffect e, int v)
    {
        switch (e)
        {
            case RespectEffect.DmgPct: DmgPct += v; break;
            case RespectEffect.TakenPct: TakenPct += v; break;
            case RespectEffect.GearPct: GearPct += v; break;
            case RespectEffect.Crit: Crit += v; break;
            case RespectEffect.Dodge: Dodge += v; break;
            case RespectEffect.CoffeePct: CoffeePct += v; break;
            case RespectEffect.Thermos: Thermos += v; break;
            case RespectEffect.Cooldown: Cooldown += v; break;
            case RespectEffect.Cash: Cash += v; break;
            case RespectEffect.XpPct: XpPct += v; break;
            case RespectEffect.BrigadePct: BrigadePct += v; break;
            case RespectEffect.Sight: Sight += v; break;
            case RespectEffect.ShopPct: ShopPct += v; break;
            case RespectEffect.MatsPct: MatsPct += v; break;
            case RespectEffect.SecondChance: SecondChance += v; break;
            case RespectEffect.Reroll: Rerolls += v; break;
            case RespectEffect.Veteran: StartCoffee += v; break;
        }
    }

    /// <summary>Skutek rangi Respektu dla gracza, np. „+8% obrażeń” (respect_label).</summary>
    public static Message RespectLabel(Message m, RespectEffect e, int v) => e switch
    {
        RespectEffect.DmgPct => m.Add("+").Add(v).Add("% obrażeń"),
        RespectEffect.TakenPct => m.Add("-").Add(v).Add("% otrzymanych obrażeń"),
        RespectEffect.GearPct => m.Add("+").Add(v).Add(" do jakości sprzętu"),
        RespectEffect.Crit => m.Add("Kryt +").Add(v).Add("%"),
        RespectEffect.Dodge => m.Add("Unik +").Add(v).Add("%"),
        RespectEffect.CoffeePct => m.Add("Kawa leczy +").Add(v).Add("%"),
        RespectEffect.Thermos => m.Add("Termos +").Add(v).Add(v == 1 ? " miejsce" : " miejsca"),
        RespectEffect.Cooldown => m.Add("Moc -").Add(v).Add(" t. odnowienia"),
        RespectEffect.Cash => m.Add("+").Add(v).Add(" zł na start"),
        RespectEffect.XpPct => m.Add("+").Add(v).Add("% doświadczenia"),
        RespectEffect.BrigadePct => m.Add("Brygada -").Add(v).Add("% ceny"),
        RespectEffect.Sight => m.Add("Widzenie +").Add(v),
        RespectEffect.ShopPct => m.Add("Hurtownia -").Add(v).Add("% ceny"),
        RespectEffect.MatsPct => m.Add("Materiały +").Add(v).Add("% częściej"),
        RespectEffect.SecondChance => m.Add("Raz na budowę: 1 HP zamiast końca"),
        RespectEffect.Reroll => m.Add("Premie: +").Add(v).Add(" darmowe losowanie"),
        RespectEffect.Veteran => m.Add("Na start: ").Add(v).Add(v == 1 ? " kawa" : " kawy").Add(" w termosie"),
        _ => m,
    };

    public static string RespectLabel(RespectEffect e, int v) => RespectLabel(new Message(), e, v).Text;

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
