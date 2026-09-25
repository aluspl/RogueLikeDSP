using System.Text.RegularExpressions;
using Godot;

namespace LifeLike.Game.Input;

/// <summary>
/// Nazwy przycisków w tekstach z game.json (pisanych pod GBA: A, B, L, R, START, SELECT) zamienione na klawisze
/// z bieżącej mapy wejścia (Spacja, Z, M, R, Enter, Tab). Przy sterowaniu ekranowym (dotyk: te same litery co
/// na GBA) wystarczy UseGbaNames = true.
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

    /// <summary>Tekst z nazwami przycisków GBA -> nazwy klawiszy (odmiana „Przytrzymaj Spację”, L = przełącznik M).</summary>
    public static string Localize(string text)
    {
        if (UseGbaNames || string.IsNullOrEmpty(text)) return text ?? "";
        text = text.Replace("Przytrzymaj L:", "L:"); // podgląd mapy w Godocie to przełącznik, nie przytrzymanie
        foreach (var (gba, action) in Tokens)
            text = Regex.Replace(text, $@"(?<![\p{{L}}\d]){gba}(?![\p{{L}}\d])", KeyName(action));
        return text.Replace("Przytrzymaj Spacja", "Przytrzymaj Spację");
    }

    /// <summary>Nazwa pierwszego klawisza akcji po polsku (Spacja, Enter, Tab, Esc...).</summary>
    public static string KeyName(GameAction a)
    {
        var name = GameInput.NameOf(a);
        if (!InputMap.HasAction(name)) return a.ToString();
        foreach (var ev in InputMap.ActionGetEvents(name))
        {
            if (ev is not InputEventKey k) continue;
            var code = k.PhysicalKeycode != Key.None ? DisplayServer.KeyboardGetKeycodeFromPhysical(k.PhysicalKeycode) : k.Keycode;
            return code switch
            {
                Key.Space => "Spacja",
                Key.Escape => "Esc",
                Key.Enter => "Enter",
                _ => OS.GetKeycodeString(code),
            };
        }
        return a.ToString();
    }
}
