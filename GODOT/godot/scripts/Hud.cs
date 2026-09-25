using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game;

/// <summary>
/// HUD tekstowy: etap/akt, HP, poziom, moc z odliczaniem, broń, budżet, szczęście (kryt/unik), termos,
/// stany z turami, sprzęt z cechami, dziennik (kolory jak na GBA), podpowiedź menu akcji
/// oraz panel ekranów tekstowych (tytuł, harmonogram, Hurtownia, podgląd, porównanie sprzętu, koniec gry).
/// </summary>
public partial class Hud : CanvasLayer
{
    private readonly Label _stats = new() { Position = new Vector2(12, 8) };
    private readonly RichTextLabel _log = new()
    {
        Position = new Vector2(12, 600), Size = new Vector2(900, 80), BbcodeEnabled = true, ScrollActive = false,
    };
    private readonly ColorRect _panelBg = new() { Color = new Color(0.04f, 0.03f, 0.08f, 0.94f), Position = new Vector2(40, 16), Size = new Vector2(1200, 668) };
    private readonly Label _panel = new() { Position = new Vector2(64, 30), Size = new Vector2(1152, 640), AutowrapMode = TextServer.AutowrapMode.WordSmart };
    /// <summary>Druga kolumna panelu (profil: zlecenia obok odznak).</summary>
    private readonly Label _panelRight = new() { Position = new Vector2(652, 30), Size = new Vector2(564, 640), AutowrapMode = TextServer.AutowrapMode.WordSmart };
    private readonly Label _help = new()
    {
        Position = new Vector2(12, 690),
        Text = "Strzałki/WSAD: ruch i atak · Spacja/X (A): atak celu · Z (B): czekaj · R: moc · Enter (START): menu akcji / termos · Tab: podgląd",
    };
    /// <summary>Podpowiedź menu akcji (nad dziennikiem), jak linia menu na GBA.</summary>
    private readonly RichTextLabel _hint = new()
    {
        Position = new Vector2(12, 548), Size = new Vector2(900, 50), BbcodeEnabled = true, ScrollActive = false, Visible = false,
    };

    public override void _Ready()
    {
        _panel.AddThemeFontSizeOverride("font_size", 18);
        _log.AddThemeFontSizeOverride("normal_font_size", 16);
        _hint.AddThemeFontSizeOverride("normal_font_size", 16);
        foreach (var n in new Control[] { _stats, _log, _hint, _help, _panelBg, _panel, _panelRight }) AddChild(n);
        HidePanel();
    }

    /// <summary>HUD w trakcie budowy; contract = linia najbliższego zlecenia (postęp na żywo), null = brak.</summary>
    public void ShowGame(CoreGame g, string contract = null)
    {
        var d = g.D;
        var st = d.Stages[g.Stage];
        var act = d.Acts[st.Act];
        var w = g.Weapon;
        var power = g.AbilityCd == 0 ? "gotowa" : $"za {g.AbilityCd}";
        var next = g.XpToNext() < 0 ? "max" : $"do awansu {g.XpToNext()}";
        var ng = g.Tier > 0 ? $" · Budowa {g.Tier + 1}" : "";
        _stats.Text =
            $"Etap {g.Stage + 1}/{d.Stages.Length}: {st.Name} · Akt {Roman(st.Act)} {act.Name}{ng} · Dzień {g.Turns} · {d.Difficulties[g.Diff].Name}\n" +
            $"{g.CDef.Name}  HP {g.Hero.Hp}/{g.Hero.MaxHp} · {Stats(g)} · Poziom {g.HeroLevel} ({next}) · Wynik {g.Score} · Budżet {g.Cash} zł\n" +
            $"Moc: {g.CDef.AbilityName} {Roman(g.AbilityRank() - 1)} ({g.CDef.AbilityDesc}) – {power} · " +
            $"Broń: {w.Name} {w.MinDamage}-{w.MaxDamage}, zasięg {w.Range}\n" +
            $"{Luck(g)} · Termos {g.Thermos}/{g.ThermosCap()} (kawa +{g.CoffeeHeal()} HP)\n" +
            $"Stany: {Statuses(g)}\nSprzęt: {Gear(g)}" +
            (g.CurrentEvent is { } ev ? $"\nPlac: {ev.Name} ({ev.Short}) – {ev.Info}" : "") +
            (contract is not null ? $"\n{contract}" : "") +
            (g.SlamTimer > 0 ? $"\nUWAGA: cios bossa za {g.SlamTimer}!" : "");
        _log.Text = string.Join("\n", g.Log.Where(m => m.N > 0).Select(LogLine));
    }

    public static string Roman(int i) => i switch { 0 => "I", 1 => "II", 2 => "III", 3 => "IV", 4 => "V", _ => (i + 1).ToString() };

    private static string LogLine(Message m)
    {
        var color = m.Kind switch { LogKind.Bad => "#ef4444", LogKind.Good => "#10b981", LogKind.Loot => "#f59e0b", _ => "#e5e7eb" };
        var text = m.Text.Replace("[", "[lb]");
        return $"[color={color}]{text}{(m.Repeat > 1 ? $" x{m.Repeat}" : "")}[/color]";
    }

    private static readonly (Stat S, string Name)[] StatNames = [(Stat.Str, "SIŁ"), (Stat.Agi, "ZRĘ"), (Stat.Intel, "INT")];

    /// <summary>Statystyka z premią, np. „SIŁ 5+2” (baza zawodu + Warsztaty/cechy sprzętu), bez premii „SIŁ 5”.</summary>
    public static string StatText(string name, int baseValue, int bonus) => bonus > 0 ? $"{name} {baseValue}+{bonus}" : $"{name} {baseValue}";

    /// <summary>Statystyki efektywne bohatera: „SIŁ 5+2 ZRĘ 3 INT 4+1”.</summary>
    public static string Stats(CoreGame g) =>
        string.Join(" ", StatNames.Select(x => StatText(x.Name, RunMods.ClassBaseStat(g.D, g.Cls, x.S), g.StatBonus(x.S))));

    /// <summary>Statystyki zawodu na ekranie tytułowym z premią z profilu (Warsztaty na statystykę broni).</summary>
    public static string ClassStats(GameData d, int cls, in RunMods m)
    {
        var mods = m;
        return string.Join(" ", StatNames.Select(x => StatText(x.Name, RunMods.ClassBaseStat(d, cls, x.S), RunMods.StatBonus(d, mods, cls, x.S))));
    }

    /// <summary>Szczęście i jego skutki: „Szczęście 4 · Kryt 17% · Unik 8% · Wzrok 7”.</summary>
    public static string Luck(CoreGame g) => $"Szczęście {g.Luck()} · Kryt {g.CritPct()}% · Unik {g.DodgePct()}% · Wzrok {g.SightRadius()}";

    /// <summary>Stany z turami i skutkiem z danych, np. „Zatrucie 3 t. (-1 HP/turę)”.</summary>
    public static string Statuses(CoreGame g)
    {
        var parts = new List<string>();
        foreach (var s in new[] { StatusEffect.Poison, StatusEffect.Shock, StatusEffect.Slip })
        {
            var t = g.StatusTurns(s);
            if (t <= 0) continue;
            var sd = g.D.Statuses[(int)s];
            parts.Add($"{sd.Name} {t} t. ({sd.Effect})");
        }
        return parts.Count == 0 ? "brak" : string.Join(", ", parts);
    }

    /// <summary>Założony sprzęt z cechami, np. „Kask budowlany [Kryt+5%]”.</summary>
    public static string Gear(CoreGame g)
    {
        var parts = new List<string>();
        for (var s = 0; s < g.D.GearSlotsCount; s++)
        {
            if (g.Equipped[s] >= 0) parts.Add($"{g.D.Gear[s * 3 + g.Equipped[s]].Name} [{g.D.GearTraits[g.EquippedTrait[s]].Short}]");
        }
        return parts.Count == 0 ? "brak" : string.Join(", ", parts);
    }

    public static string StatName(GearStat s) => s switch { GearStat.Def => "obrona", GearStat.Dmg => "obrażenia", _ => "max HP" };

    /// <summary>Podpowiedź pod mapą (menu akcji); null chowa.</summary>
    public void ShowHint(string text)
    {
        _hint.Visible = text is not null;
        if (text is not null) _hint.Text = $"[color=#f59e0b]{text.Replace("[", "[lb]")}[/color]";
    }

    /// <summary>Panel ekranu tekstowego; right = druga kolumna (null = jedna szeroka). Długi tekst – mniejsza czcionka.</summary>
    public void ShowPanel(string text, string right = null)
    {
        var lines = Math.Max(text.Split('\n').Length, right?.Split('\n').Length ?? 0);
        var size = right is not null ? 15 : lines > 26 ? 14 : lines > 22 ? 16 : 18;
        var full = right is not null || lines > 14; // krótkie ekrany (paczka, harmonogram): okno na środku, HUD widoczny
        _panel.AddThemeFontSizeOverride("font_size", size);
        _panelRight.AddThemeFontSizeOverride("font_size", size);
        _panelBg.Position = full ? new Vector2(40, 16) : new Vector2(240, 60);
        _panelBg.Size = full ? new Vector2(1200, 668) : new Vector2(800, 560);
        _panel.Position = full ? new Vector2(64, 30) : new Vector2(264, 76);
        _panel.Size = full ? new Vector2(right is null ? 1152 : 564, 640) : new Vector2(760, 530);
        _panel.Text = text;
        _panelRight.Text = right ?? "";
        _panel.Visible = _panelBg.Visible = true;
        _panelRight.Visible = right is not null;
        _stats.Visible = _log.Visible = !full; // pełnoekranowy panel zasłania HUD gry
    }

    public void HidePanel()
    {
        _panel.Visible = _panelBg.Visible = _panelRight.Visible = false;
        _stats.Visible = _log.Visible = true;
    }

    public bool PanelVisible => _panel.Visible;
}
