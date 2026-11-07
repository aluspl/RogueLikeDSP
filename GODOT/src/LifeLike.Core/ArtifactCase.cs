using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// v0.23.0: Gablota artefaktów (profil v19) – artefakty to bronie Mistrzowskie (GameData.Artifacts); odkryty = wzięty do ręki
/// albo do kabury w którejś budowie (Game.ToolsFound / broń w kaburze). Profil trzyma maskę odkrytych i dzień pierwszego znalezienia.
/// </summary>
public static class ArtifactCase
{
    /// <summary>Pozycja broni w Gablocie (-1 = nie artefakt).</summary>
    public static int Index(GameData d, int weapon) => Array.IndexOf(d.Artifacts, weapon);

    public static bool Found(Profile p, int i) => i >= 0 && i < Profile.MaxArtifacts && ((p.Artifacts >> i) & 1) != 0;

    /// <summary>Dzień pierwszego znalezienia (dni od 1970-01-01; 0 = nieznany – znaleziony przed Gablotą).</summary>
    public static int Day(Profile p, int i) => i >= 0 && i < Profile.MaxArtifacts ? p.ArtifactDay[i] : 0;

    public static int FoundCount(GameData d, Profile p)
    {
        var n = 0;
        for (var i = 0; i < d.Artifacts.Length; ++i)
        {
            if (Found(p, i)) ++n;
        }
        return n;
    }

    /// <summary>Narzędzie (GameData.Tools) z bronią artefaktu i; -1 = brak.</summary>
    public static int Tool(GameData d, int i) => i >= 0 && i < d.Artifacts.Length ? Array.FindIndex(d.Tools, t => t.Weapon == d.Artifacts[i]) : -1;

    /// <summary>Czy artefakt i był w ręku albo kaburze w budowie g.</summary>
    public static bool InRun(GameData d, Game g, int i)
    {
        var t = Tool(d, i);
        return (t >= 0 && ((g.ToolsFound >> t) & 1) != 0) || g.Weapon2 == d.Artifacts[i] || g.WeaponOverride == d.Artifacts[i];
    }

    /// <summary>Zapisuje do Gabloty artefakty znalezione w budowie (dzień today); zwraca maskę odkrytych właśnie teraz. Można wołać wielokrotnie.</summary>
    public static int Bank(GameData d, Profile p, Game g, int today)
    {
        var got = 0;
        for (var i = 0; i < d.Artifacts.Length && i < Profile.MaxArtifacts; ++i)
        {
            if (Found(p, i) || !InRun(d, g, i)) continue;
            got |= 1 << i;
            p.ArtifactDay[i] = (ushort)Math.Clamp(today, 0, ushort.MaxValue);
        }
        p.Artifacts = (ushort)(p.Artifacts | got);
        return got;
    }

    /// <summary>v18 -> v19: unikaty już znalezione (Profile.ToolsFound) trafiają do Gabloty bez daty.</summary>
    public static void MigrateV19(GameData d, Profile p)
    {
        p.Artifacts = 0;
        Array.Clear(p.ArtifactDay);
        for (var i = 0; i < d.Artifacts.Length && i < Profile.MaxArtifacts; ++i)
        {
            var t = Tool(d, i);
            if (t >= 0 && ((p.ToolsFound >> t) & 1) != 0) p.Artifacts = (ushort)(p.Artifacts | (1 << i));
        }
    }
}
