using System.Collections.Generic;
using Godot;
using LifeLike.Core.Data;
using LifeLike.Game.Phone;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Gfx;

/// <summary>
/// Wygląd premii po etapie (v0.21.50 cz. 2): kolor rzadkości (zwykła szara, rzadka niebieska, legendarna złota),
/// znaczniki jako tekst, stany problemu (mokry, zapylony, zmrożony) i opis cechy elity.
/// </summary>
public static class BoonLook
{
    public static Color RarityColor(int r) => r >= 2 ? Pal.Legend : r == 1 ? Pal.Rare : Pal.Todo;

    public static Ink RarityInk(int r) => r >= 2 ? Ink.Legend : r == 1 ? Ink.Rare : Ink.Dim;

    public static PillKind RarityPill(int r) => r >= 2 ? PillKind.Prog : r == 1 ? PillKind.Group : PillKind.Gray;

    public static string RarityName(GameData d, int r) => r >= 0 && r < d.BoonRarities.Length
        ? (ScreenFilter.Cues ? RarityLetter(r) + " " : "") + d.BoonRarities[r].Name
        : "";

    /// <summary>v0.21.53: litera rzadkości (Z zwykła, R rzadka, L legendarna) – wzór zamiast samego koloru przy filtrach
    /// dla daltonistów (ScreenFilter.Cues).</summary>
    public static string RarityLetter(int r) => r >= 2 ? "L" : r == 1 ? "R" : "Z";

    /// <summary>Nazwa premii z literą rzadkości na początku, gdy filtr wymaga wzorów.</summary>
    public static string CueName(string name, int r) => ScreenFilter.Cues ? $"[{RarityLetter(r)}] {name}" : name;

    /// <summary>Nazwy znaczników z maski, np. „Beton, BHP”.</summary>
    public static List<string> Tags(GameData d, int mask)
    {
        var l = new List<string>();
        for (var t = 0; t < d.BoonTags.Length; t++)
        {
            if (((mask >> t) & 1) != 0) l.Add(d.BoonTags[t]);
        }
        return l;
    }

    /// <summary>Premia z oferty włączy nową synergię (pierwsza nowa albo -1).</summary>
    public static int NewSynergy(CoreGame g, int boon)
    {
        var before = g.SynergyMask();
        var after = CoreGame.SynergyMaskIn(g.D, g.Boons | (1ul << boon));
        for (var s = 0; s < g.D.Synergies.Length; s++)
        {
            if (((after >> s) & 1) != 0 && ((before >> s) & 1) == 0) return s;
        }
        return -1;
    }

    /// <summary>Stany problemu ei, np. „mokry, zapylony” (pusty, gdy brak).</summary>
    public static string States(CoreGame g, int ei)
    {
        var l = new List<string>();
        if (g.EnemyWet(ei)) l.Add("mokry");
        if (g.EnemyDusty(ei)) l.Add("zapylony");
        if (g.EnemyFrozen(ei)) l.Add("zmrożony");
        return string.Join(", ", l);
    }

    /// <summary>Bity stanów do ikon nad problemem: 1 mokry, 2 zapylony, 4 zmrożony.</summary>
    public static int StateBits(CoreGame g, int ei) => (g.EnemyWet(ei) ? 1 : 0) | (g.EnemyDusty(ei) ? 2 : 0) | (g.EnemyFrozen(ei) ? 4 : 0);

    /// <summary>„Elita: Tarcza (+4 OBR)” albo pusty.</summary>
    public static string Elite(CoreGame g, int ei)
    {
        var e = g.Enemies[ei];
        return e.Elite < 0 ? "" : $"Elita: {g.D.Elites[e.Elite].Name} ({g.D.Elites[e.Elite].Info})";
    }

    /// <summary>Ikona stanu (kropla, pył, płatek śniegu) w punkcie c, rozmiar ~8 px.</summary>
    public static void DrawState(CanvasItem ci, int bit, Vector2 c)
    {
        switch (bit)
        {
            case 1:
                ci.DrawCircle(c + new Vector2(0, 1.5f), 3.2f, Pal.WetBlue);
                ci.DrawColoredPolygon([c + new Vector2(-2.8f, 0.5f), c + new Vector2(0, -4.5f), c + new Vector2(2.8f, 0.5f)], Pal.WetBlue);
                ci.DrawCircle(c + new Vector2(-1f, 1.5f), 0.9f, Colors.White);
                break;
            case 2:
                ci.DrawCircle(c + new Vector2(-2.5f, 1.5f), 2.2f, Pal.DustGray);
                ci.DrawCircle(c + new Vector2(2f, 0.5f), 2.6f, Pal.DustGray);
                ci.DrawCircle(c + new Vector2(0, -2.5f), 2f, new Color(Pal.DustGray, 0.8f));
                break;
            default:
                for (var k = 0; k < 3; k++)
                {
                    var a = k * Mathf.Pi / 3;
                    var v = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 4f;
                    ci.DrawLine(c - v, c + v, Pal.FrostCyan, 1.6f);
                }
                ci.DrawCircle(c, 1.2f, Colors.White);
                break;
        }
    }
}
