using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace LifeLike.Core;

/// <summary>
/// v0.21.53 cz. 2 (#40): angielska warstwa danych gry (1:1 z GBA/tools/gen_data.py: overlay). en.json „fields” – wzorce
/// ścieżek pól z tekstem dla gracza („?” = odwołanie po nazwie, tłumaczone tylko, gdy jest wpis), „context” – wyjątki
/// dla pola, „data” – polski tekst -> angielski. Wynik: kopia game.json z angielskimi tekstami (ten sam kształt).
/// </summary>
public static class LangOverlay
{
    /// <summary>Teksty bez tłumaczenia z ostatniego Apply (test kompletności).</summary>
    public static List<string> Missing { get; } = new();

    public static string Apply(string gameJson, string enJson)
    {
        Missing.Clear();
        using var en = JsonDocument.Parse(enJson);
        var root = JsonNode.Parse(gameJson, documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
        var fields = en.RootElement.GetProperty("fields").EnumerateArray()
            .Select(f => f.GetString() ?? "")
            .Select(f => (Rx: new Regex("^" + Regex.Escape(f.TrimStart('?')).Replace(@"\*", @"[^.\[]+") + "$"), Optional: f.StartsWith('?')))
            .ToArray();
        var data = Dict(en.RootElement, "data");
        var context = new Dictionary<string, Dictionary<string, string>>();
        if (en.RootElement.TryGetProperty("context", out var ctx))
            foreach (var p in ctx.EnumerateObject()) context[p.Name] = p.Value.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.GetString() ?? "");
        Walk(root, "", fields, data, context);
        return root!.ToJsonString();
    }

    private static Dictionary<string, string> Dict(JsonElement e, string name) =>
        e.TryGetProperty(name, out var d) ? d.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.GetString() ?? "") : new Dictionary<string, string>();

    private static void Walk(JsonNode node, string path, (Regex Rx, bool Optional)[] fields, Dictionary<string, string> data,
        Dictionary<string, Dictionary<string, string>> context)
    {
        if (node is JsonObject o)
        {
            foreach (var key in o.Select(p => p.Key).ToList())
            {
                if (path.Length == 0 && key is "ui" or "uiGodot") continue;
                var child = o[key];
                var p = path.Length == 0 ? key : path + "." + key;
                if (child is JsonValue v && v.TryGetValue<string>(out var s)) o[key] = Translate(s, p, fields, data, context);
                else if (child is not null) Walk(child, p, fields, data, context);
            }
        }
        else if (node is JsonArray a)
        {
            for (var i = 0; i < a.Count; i++)
            {
                var child = a[i];
                if (child is JsonValue v && v.TryGetValue<string>(out var s)) a[i] = Translate(s, path + "[]", fields, data, context);
                else if (child is not null) Walk(child, path + "[]", fields, data, context);
            }
        }
    }

    private static string Translate(string s, string path, (Regex Rx, bool Optional)[] fields, Dictionary<string, string> data,
        Dictionary<string, Dictionary<string, string>> context)
    {
        foreach (var (rx, optional) in fields)
        {
            if (!rx.IsMatch(path)) continue;
            if (s.Length == 0) return s;
            if (context.TryGetValue(path, out var c) && c.TryGetValue(s, out var ct)) return ct;
            if (data.TryGetValue(s, out var e)) return e;
            if (!optional) Missing.Add(path + ": " + s);
            return s;
        }
        return s;
    }
}
