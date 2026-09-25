using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Godot;

namespace LifeLike.Game.Gfx;

/// <summary>
/// Pikselowy font 8x16 o zmiennej szerokości z GBA (Butano common_variable_8x16 + polskie znaki), wyeksportowany
/// przez GODOT/tools/export_godot_assets.py: font/glyphs.png (litery), font/glyphs_edge.png (cień), font/font.json
/// (kolejność znaków i szerokości). Rysuje własnoręcznie (DrawTextureRectRegion), więc kolory liter i cienia
/// są dokładnie jak palety font_*.bmp na GBA, a skala jest zawsze całkowita (ostre piksele).
/// </summary>
public sealed class PixelFont
{
    public const int LineHeight = 16;
    private const int CellW = 8, CellH = 16;

    private static PixelFont _instance;

    public static PixelFont I => _instance ??= new PixelFont();

    public static void Release() => _instance = null;

    private readonly Texture2D _letters;
    private readonly Texture2D _edges;
    private readonly Dictionary<char, int> _index = new();
    private readonly int[] _widths;
    private readonly int _cols;
    private readonly int _space;

    private static readonly Dictionary<char, string> Fallback = new()
    {
        ['–'] = "-", ['—'] = "-", ['·'] = "-", ['„'] = "\"", ['”'] = "\"", ['“'] = "\"", ['…'] = "...",
        ['→'] = ">", ['←'] = "<", ['↑'] = "^", ['↓'] = "v", ['×'] = "x", ['’'] = "'", [' '] = " ",
    };

    private PixelFont()
    {
        _letters = GD.Load<Texture2D>("res://assets/font/glyphs.png");
        _edges = GD.Load<Texture2D>("res://assets/font/glyphs_edge.png");
        using var doc = JsonDocument.Parse(FileAccess.GetFileAsString("res://assets/font/font.json"));
        var root = doc.RootElement;
        var chars = root.GetProperty("chars").GetString() ?? "";
        _cols = root.GetProperty("cols").GetInt32();
        _space = root.GetProperty("space").GetInt32();
        var w = new List<int>();
        foreach (var e in root.GetProperty("widths").EnumerateArray()) w.Add(e.GetInt32());
        _widths = w.ToArray();
        for (var i = 0; i < chars.Length; i++) _index[chars[i]] = i;
    }

    /// <summary>Zamienia znaki spoza fontu (półpauzy, strzałki, cudzysłowy) na odpowiedniki ASCII.</summary>
    public string Normalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        StringBuilder sb = null;
        for (var i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (c == ' ' || _index.ContainsKey(c))
            {
                sb?.Append(c);
                continue;
            }
            sb ??= new StringBuilder(s, 0, i, s.Length + 8);
            sb.Append(Fallback.TryGetValue(c, out var f) ? f : "?");
        }
        return sb?.ToString() ?? s;
    }

    private int Advance(char c) => c == ' ' ? _space : _index.TryGetValue(c, out var i) ? _widths[i] : _space;

    public int Measure(string s, int scale = 1) => Measure(s, scale, false);

    /// <summary>Szerokość tekstu; bold = pogrubienie (każdy znak o 1 piksel szerszy).</summary>
    public int Measure(string s, int scale, bool bold)
    {
        s = Normalize(s);
        var w = 0;
        foreach (var c in s) w += Advance(c) + (bold && c != ' ' ? 1 : 0);
        return w * scale;
    }

    /// <summary>Tekst przycięty do szerokości (z „..” na końcu), jak clip() na GBA, ale w pikselach.</summary>
    public string Fit(string s, int maxWidth, int scale = 1)
    {
        s = Normalize(s);
        if (Measure(s, scale) <= maxWidth) return s;
        var dots = Measure("..", scale);
        var w = 0;
        for (var i = 0; i < s.Length; i++)
        {
            w += Advance(s[i]) * scale;
            if (w + dots > maxWidth) return s[..i].TrimEnd() + "..";
        }
        return s;
    }

    /// <summary>Łamanie wierszy po słowach do szerokości maxWidth (znak nowej linii wymusza łamanie).</summary>
    public List<string> Wrap(string s, int maxWidth, int scale = 1)
    {
        var lines = new List<string>();
        foreach (var para in Normalize(s).Split('\n'))
        {
            var line = "";
            foreach (var word in para.Split(' '))
            {
                var candidate = line.Length == 0 ? word : line + " " + word;
                if (Measure(candidate, scale) <= maxWidth || line.Length == 0)
                {
                    line = candidate;
                    continue;
                }
                lines.Add(line);
                line = word;
            }
            lines.Add(Fit(line, maxWidth, scale));
        }
        return lines;
    }

    /// <summary>
    /// Rysuje linię tekstu: pos = lewy górny róg 16-pikselowej linii (dla Center/Right: środek/prawa krawędź).
    /// Zwraca szerokość w pikselach.
    /// </summary>
    public int Draw(CanvasItem ci, Vector2 pos, string s, Ink ink, TextAlign align = TextAlign.Left, int scale = 1) =>
        Draw(ci, pos, s, ink, align, scale, false);

    /// <summary>Jak Draw; bold = pogrubienie jak w nagłówkach aplikacji (znak rysowany dwa razy, odstępy zachowane).</summary>
    public int Draw(CanvasItem ci, Vector2 pos, string s, Ink ink, TextAlign align, int scale, bool bold)
    {
        s = Normalize(s);
        var width = Measure(s, scale, bold);
        var x = Mathf.Round(pos.X - (align == TextAlign.Center ? width / 2 : align == TextAlign.Right ? width : 0));
        var y = Mathf.Round(pos.Y);
        var drawEdge = ink.Edge.A > 0.01f;
        for (var pass = drawEdge ? 0 : 1; pass < 2; pass++)
        {
            var tex = pass == 0 ? _edges : _letters;
            var col = pass == 0 ? ink.Edge : ink.Fill;
            var cx = x;
            foreach (var c in s)
            {
                if (c != ' ' && _index.TryGetValue(c, out var g))
                {
                    var src = new Rect2((g % _cols) * CellW, (g / _cols) * CellH, CellW, CellH);
                    ci.DrawTextureRectRegion(tex, new Rect2(cx, y, CellW * scale, CellH * scale), src, col);
                    if (bold) ci.DrawTextureRectRegion(tex, new Rect2(cx + scale, y, CellW * scale, CellH * scale), src, col);
                }
                cx += (Advance(c) + (bold && c != ' ' ? 1 : 0)) * scale;
            }
        }
        return width;
    }
}
