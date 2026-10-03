using LifeLike.Core;
using System.Text.RegularExpressions;
using Godot;
using LifeLike.Game.Gfx;

namespace LifeLike.Game.Input;

/// <summary>
/// Nazwy przycisków w tekstach z game.json (pisanych pod GBA: A, B, L, R, START, SELECT) zamienione na klawisze
/// z bieżącej mapy wejścia (Spacja, Z, M, R, Enter, Tab). Przy sterowaniu ekranowym (dotyk: te same litery co
/// na GBA) wystarczy UseGbaNames = true. Przy sterowaniu dotykiem (Layout.Touch) - nazwy przycisków paska akcji
/// (Atak, Moc, Termos, Czekaj, Telefon), bez klawiszy: „Przytrzymaj Atak: celownik”.
/// </summary>
public static class ButtonNames
{
    /// <summary>Zostaw nazwy z GBA (wirtualny pad ma przyciski A, B, L, R, START, SELECT).</summary>
    public static bool UseGbaNames { get; set; }

    private static readonly (string Gba, GameAction Action)[] Tokens =
    [
        ("SELECT", GameAction.Select), ("START", GameAction.Start), ("A", GameAction.A), ("B", GameAction.B),
        ("L", GameAction.L), ("R", GameAction.R),
    ];

    private static (string Gba, string Touch)[] TouchPhrases =>
    [
        (Loc.T("menu_start"), Loc.T("przycisk_termos")), (Loc.T("telefon_select"), Loc.T("telefon")), (Loc.T("moc_pod_r"), Loc.T("moc_6")),
        (Loc.T("paczka_a_zakladam_b_zostawiam"), Loc.T("paczka_zakladam_albo_zostawiam")), (Loc.T("a_dalej"), Loc.T("dalej")),
    ];

    private static (string Gba, string Touch)[] TouchTokens =>
    [
        ("SELECT", Loc.T("telefon")), ("START", Loc.T("termos_3")), ("A", Loc.T("atak")), ("B", Loc.T("czekaj")), ("L", Loc.T("telefon")), ("R", Loc.T("moc_6")),
    ];

    /// <summary>Tekst dla bieżącego sterowania: klawisze albo dotyk (np. podpowiedzi ekranów pisane w kodzie).</summary>
    public static string Pick(string keys, string touch) => Layout.Touch ? touch : keys;

    /// <summary>Tekst z nazwami przycisków GBA -> nazwy klawiszy (odmiana „Przytrzymaj Spację”, L = przełącznik M)
    /// albo przycisków paska akcji przy dotyku.</summary>
    public static string Localize(string text)
    {
        if (string.IsNullOrEmpty(text)) return text ?? "";
        if (Layout.Touch) return ForTouch(text);
        if (UseGbaNames) return text;
        text = text.Replace(Loc.T("przytrzymaj_l"), "L:"); // podgląd mapy w Godocie to przełącznik, nie przytrzymanie
        foreach (var (gba, action) in Tokens)
            text = Regex.Replace(text, $@"(?<![\p{{L}}\d]){gba}(?![\p{{L}}\d])", KeyName(action));
        return text.Replace(Loc.T("przytrzymaj_spacja"), Loc.T("przytrzymaj_spacje"));
    }

    private static string ForTouch(string text)
    {
        foreach (var (gba, touch) in TouchPhrases) text = text.Replace(gba, touch);
        foreach (var (gba, touch) in TouchTokens)
            text = Regex.Replace(text, $@"(?<![\p{{L}}\d]){gba}(?![\p{{L}}\d])", touch);
        return text;
    }

    /// <summary>Nazwa pierwszego klawisza akcji po polsku (Spacja, Enter, Tab, Esc...).</summary>
    public static string KeyName(GameAction a)
    {
        var name = GameInput.NameOf(a);
        if (!InputMap.HasAction(name)) return a.ToString();
        foreach (var ev in InputMap.ActionGetEvents(name))
        {
            if (ev is not InputEventKey k) continue;
            var code = k.PhysicalKeycode != Key.None ? k.PhysicalKeycode : k.Keycode; // nazwa wg układu QWERTY
            return code switch
            {
                Key.Space => Loc.T("spacja"),
                Key.Escape => Loc.T("esc"),
                Key.Enter => Loc.T("enter"),
                _ => OS.GetKeycodeString(code),
            };
        }
        return a.ToString();
    }
}
