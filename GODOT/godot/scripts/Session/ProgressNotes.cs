using System.Linq;
using LifeLike.Core;
using LifeLike.Core.Data;

namespace LifeLike.Game.Session;

/// <summary>Teksty o zdobytych odznakach i zleceniach (notatka na harmonogramie i planszy końcowej).</summary>
public static class ProgressNotes
{
    public static string Badges(GameData d, int got) =>
        got == 0 ? "" : "Odznaki: " + string.Join(", ", d.Badges.Where((_, i) => (got & (1 << i)) != 0)
            .Select(b => $"{b.Name} (+{b.Xp}, uprawnienie: {RunMods.PerkLabel(b.Bonus)})"));

    public static string Contracts(GameData d, int got) =>
        got == 0 ? "" : "Zlecenia wykonane: " + string.Join(", ", d.Contracts.Where((_, i) => (got & (1 << i)) != 0)
            .Select(c => $"{c.Name} (+{c.Xp}{(c.Keepsake >= 0 ? ", pamiątka " + d.Keepsakes[c.Keepsake].Name : "")})"));

    public static string Join(GameData d, int badges, int contracts) =>
        string.Join("\n", new[] { Badges(d, badges), Contracts(d, contracts) }.Where(s => s.Length > 0));
}
