using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// Teksty wydarzeń z wyborem i ulepszenia narzędzia (v0.21.50 cz. 3) – port core::choice_out_label, choice_label,
/// tool_level_label z core.h (te same słowa na GBA i w Godocie).
/// </summary>
public static class ChoiceText
{
    /// <summary>Skutek odpowiedzi słowami, np. „-10 zł”, „ciosy +2 na etap”, „30%: 2x Pleśń obok”.</summary>
    public static Message OutLabel(GameData d, Message m, ChoiceOut o)
    {
        var v = o.Value;
        if (o.Chance < 100) m.Add(o.Chance).Add("%: ");
        switch (o.Effect)
        {
            case ChoiceEffect.Cash: return m.Add(v > 0 ? "+" : "").Add(v).Add(Loc.T("zl"));
            case ChoiceEffect.Xp: return m.Add("+").Add(v).Add(Loc.T("dosw"));
            case ChoiceEffect.Hp: return m.Add(v > 0 ? "+" : "").Add(v).Add(" HP");
            case ChoiceEffect.MaxHp: return m.Add(v > 0 ? "+" : "").Add(v).Add(Loc.T("max_hp"));
            case ChoiceEffect.Mats:
                if (o.Arg >= 0) return m.Add(d.Materials[o.Arg].Name).Add(v > 0 ? " +" : " ").Add(v);
                return m.Add(Loc.T("materialy_2")).Add(v > 0 ? "+" : "").Add(v);
            case ChoiceEffect.StageDmg: return m.Add(Loc.T("ciosy")).Add(v > 0 ? "+" : "").Add(v).Add(Loc.T("na_etap"));
            case ChoiceEffect.StageDef: return m.Add(Loc.T("obr_4")).Add(v > 0 ? "+" : "").Add(v).Add(Loc.T("na_etap"));
            case ChoiceEffect.Boon: return m.Add(Loc.T("premia_1_z_3"));
            case ChoiceEffect.Gear:
                m.Add(o.Arg >= 0 ? d.GearSlots[o.Arg] : Loc.T("sprzet_2"));
                if (v > 0) m.Add(" (").Add(d.GearRarities[v]).Add(")");
                return m;
            case ChoiceEffect.Respect: return m.Add(Loc.T("respekt_2")).Add(v);
            case ChoiceEffect.Coffee: return m.Add(Loc.T("kawa_3")).Add(v);
            case ChoiceEffect.Spawn: return m.Add(v).Add("x ").Add(d.Enemies[o.Arg].Name).Add(Loc.T("obok"));
            case ChoiceEffect.Status: return m.Add(d.Statuses[o.Arg].Name).Add(" ").Add(v).Add(" t.");
            case ChoiceEffect.Upgrade: return m.Add(Loc.T("narzedzie")).Add(v);
            case ChoiceEffect.Power: return m.Add(Loc.T("moc_gotowa"));
            default: return m;
        }
    }

    public static string OutLabel(GameData d, ChoiceOut o) => OutLabel(d, new Message(), o).Text;

    /// <summary>Wszystkie skutki odpowiedzi po przecinku („bez skutków”, gdy brak).</summary>
    public static Message Label(GameData d, Message m, EventChoice c)
    {
        if (c.Outs.Length == 0) return m.Add(Loc.T("bez_skutkow"));
        for (var i = 0; i < c.Outs.Length; ++i)
        {
            if (i > 0) m.Add(", ");
            OutLabel(d, m, c.Outs[i]);
        }
        return m;
    }

    public static string Label(GameData d, EventChoice c) => Label(d, new Message(), c).Text;

    /// <summary>Koszt poziomu ulepszenia narzędzia, np. „20 zł + 2 Stal”.</summary>
    public static Message ToolLevelLabel(GameData d, Message m, int level, int cash)
    {
        var t = d.ToolLevels[level];
        return m.Add(cash).Add(Loc.T("zl_2")).Add(t.Count).Add(" ").Add(d.Materials[t.Material].Short);
    }
}
