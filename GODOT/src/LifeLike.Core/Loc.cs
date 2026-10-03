using System.Text.Json;

namespace LifeLike.Core;

/// <summary>
/// v0.21.53 cz. 2 (#40): teksty interfejsu po polsku i po angielsku. Polskie w game.json („ui” – wspólne z GBA,
/// „uiGodot” – tylko Godot), angielskie w GBA/data/lang/en.json (te same klucze). T(klucz) – tekst w bieżącym
/// języku; F(klucz, ...) – szablon z {0}, {1}... (string.Format). Brak angielskiego tekstu – polski, brak klucza – klucz.
/// Teksty danych (nazwy, opisy, SMS-y) tłumaczy LangOverlay przy wczytaniu GameData.
/// </summary>
public static class Loc
{
    private static Dictionary<string, string> _pl = new();
    private static Dictionary<string, string> _en = new();

    /// <summary>Bieżący język: angielski (true) albo polski.</summary>
    public static bool English { get; set; }

    public static string Code => English ? "en" : "pl";

    /// <summary>Tablice tekstów z game.json (ui, uiGodot) i en.json (ui, uiGodot; null = sam polski).</summary>
    public static void Load(JsonElement game, JsonElement? en)
    {
        _pl = Table(game);
        _en = en is { } e ? Table(e) : new Dictionary<string, string>();
    }

    public static string T(string key)
    {
        if (English && _en.TryGetValue(key, out var e)) return e;
        return _pl.TryGetValue(key, out var p) ? p : key;
    }

    public static string F(string key, params object[] args) => string.Format(T(key), args);

    /// <summary>Czy klucz jest w tablicy (testy kompletności).</summary>
    public static bool Has(string key, bool english) => (english ? _en : _pl).ContainsKey(key);

    public static IEnumerable<string> Keys => _pl.Keys;

    private static Dictionary<string, string> Table(JsonElement root)
    {
        var t = new Dictionary<string, string>();
        foreach (var section in new[] { "ui", "uiGodot" })
        {
            if (!root.TryGetProperty(section, out var s) || s.ValueKind != JsonValueKind.Object) continue;
            foreach (var p in s.EnumerateObject()) t[p.Name] = p.Value.GetString() ?? "";
        }
        return t;
    }
}
