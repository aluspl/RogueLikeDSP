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

    /// <summary>Typy ciosu broni: „Obuch”, „Cięcie/Kłucie” (ModeText w rdzeniu).</summary>
    public static string Hits(GameData d, WeaponDef w) => ModeText.Hits(d, w);

    /// <summary>v0.23.0 (#91): profity z maski pełnymi nazwami, po przecinku („Przebicie, Ostrze”) – do jednego wiersza.</summary>
    public static string PerkNames(GameData d, int perks)
    {
        var parts = new List<string>();
        for (var i = 0; i < d.WeaponPerkPool.Length; i++)
        {
            if ((perks & (1 << i)) != 0) parts.Add(d.WeaponPerkPool[i].Name);
        }
        return string.Join(", ", parts);
    }

    /// <summary>v0.23.0 (#91): po wierszu na profit – pełna nazwa i opis („Profit Ostrze: Kryt +5%”); Mistrzowska – „Unikat ...”.</summary>
    public static List<string> PerkLines(GameData d, int perks, bool master)
    {
        var lines = new List<string>();
        for (var i = 0; i < d.WeaponPerkPool.Length; i++)
        {
            if ((perks & (1 << i)) == 0) continue;
            var pk = d.WeaponPerkPool[i];
            lines.Add(master ? Loc.F("unikat_linia", pk.Name, pk.Desc) : Loc.F("profit_linia", pk.Name, pk.Desc));
        }
        return lines;
    }

    /// <summary>
    /// Wiersze profitów na karcie: w pełnym telefonie (pion) po wierszu na profit z opisem, w poziomie (brak miejsca) jeden wiersz
    /// z pełnymi nazwami („Profity: Ostrze, Głowica”). Bez profitów – pusta lista.
    /// </summary>
    public static List<string> PerkRows(GameData d, int perks, bool master, bool full)
    {
        if (perks == 0) return [];
        return full ? PerkLines(d, perks, master) : [(master ? Loc.T("unikat") : Loc.T("profity")) + ": " + PerkNames(d, perks)];
    }

    /// <summary>Cecha sprzętu na karcie: pełna nazwa („Odporność na zatrucie”), skrót tylko gdy nazwa się nie mieści.</summary>
    public static string TraitText(GameData d, int trait, PixelFont f, float room) =>
        f.Measure(d.GearTraits[trait].Name) <= room ? d.GearTraits[trait].Name : d.GearTraits[trait].Short;

    /// <summary>Wzór pól trybu: „łuk 3 pól”, „linia 2”, „strzał 3”, „rzut 3x3” (ModeText w rdzeniu).</summary>
    public static string Pattern(WeaponMode m) => ModeText.Pattern(m);

    /// <summary>Tryb słowami bez nazwy ruchu: „linia 2, -2 OBR (3 t.), odn. 4” (ModeText w rdzeniu).</summary>
    public static string Mode(WeaponDef w, WeaponMode m, bool cooldown, int cdDelta = 0) => ModeText.Mode(w, m, cooldown, cdDelta);

    /// <summary>v0.23.0: wiersz Cios („Cios: łuk 3 pól”).</summary>
    public static string LineA(WeaponDef w) => Loc.T("tryb_cios") + Mode(w, w.ModeA, false);

    /// <summary>v0.23.0: wiersz Patentu z nazwą ruchu („Rzut zaprawą: strzał 2 50%, ogłuszenie (1 t.), odn. 7”).</summary>
    public static string LineB(WeaponDef w, int cdDelta) => ModeText.BName(w) + ": " + Mode(w, w.ModeB, true, cdDelta);

    /// <summary>Zmiana odnowienia Patentu (tryb B) od rzadkości (Markowa -1).</summary>
    public static int CdDelta(GameData d, int r) => r >= 0 && r < d.WeaponRarities.Length ? d.WeaponRarities[r].CooldownDelta : 0;
}
