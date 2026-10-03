using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// v0.21.52 cz. c – port z meta.h: drzewko Szkoleń. Gałęzie Fach / BHP / Logistyka: pień = Szkolenia (poziomy kupowane
/// jak dotąd), węzeł otwiera się po Depth poziomach pnia gałęzi; w węźle wybór 1 z 2 opcji za dośw. (Cost), zmiana wyboru
/// za GameData.TreeRespecCost. Wybór: Profile.Tree, 2 bity na węzeł (0 brak, 1 = A, 2 = B).
/// </summary>
public static class SkillTree
{
    public static int BranchLevels(GameData d, Profile p, int b)
    {
        var n = 0;
        for (var i = 0; i < d.Upgrades.Length; ++i)
        {
            if (((d.TreeBranches[b].Upgrades >> i) & 1) != 0) n += p.Levels[i];
        }
        return n;
    }

    public static int BranchMax(GameData d, int b)
    {
        var n = 0;
        for (var i = 0; i < d.Upgrades.Length; ++i)
        {
            if (((d.TreeBranches[b].Upgrades >> i) & 1) != 0) n += d.Upgrades[i].Levels;
        }
        return n;
    }

    /// <summary>Gałąź, do której należy Szkolenie (pień).</summary>
    public static int BranchOf(GameData d, int upgrade)
    {
        for (var b = 0; b < d.TreeBranches.Length; ++b)
        {
            if (((d.TreeBranches[b].Upgrades >> upgrade) & 1) != 0) return b;
        }
        return 0;
    }

    /// <summary>Wybrana opcja węzła n: 0 = brak, 1 = A, 2 = B.</summary>
    public static int Pick(Profile p, int n)
    {
        var v = (p.Tree >> (2 * n)) & 3;
        return v <= 2 ? v : 0;
    }

    public static bool Open(GameData d, Profile p, int n) => BranchLevels(d, p, d.TreeNodes[n].Branch) >= d.TreeNodes[n].Depth;

    /// <summary>Koszt wybrania opcji o (0/1) węzła n: pierwszy wybór – Cost, zmiana – opłata; -1 = już wybrana.</summary>
    public static int Cost(GameData d, Profile p, int n, int o)
    {
        var pk = Pick(p, n);
        if (pk == o + 1) return -1;
        return pk == 0 ? d.TreeNodes[n].Cost : d.TreeRespecCost;
    }

    public static bool Choose(GameData d, Profile p, int n, int o)
    {
        if (n < 0 || n >= d.TreeNodes.Length || o < 0 || o > 1) return false;
        var c = Cost(d, p, n, o);
        if (c < 0 || !Open(d, p, n) || p.Xp < c) return false;
        p.Xp -= c;
        p.Tree = (ushort)((p.Tree & ~(3u << (2 * n))) | ((uint)(o + 1) << (2 * n)));
        return true;
    }

    public static int Picked(GameData d, Profile p) => Enumerable.Range(0, d.TreeNodes.Length).Count(n => Pick(p, n) > 0);

    /// <summary>Premie wybranych węzłów (add_tree).</summary>
    public static void Add(GameData d, ref RunMods m, Profile p)
    {
        for (var n = 0; n < d.TreeNodes.Length; ++n)
        {
            var pk = Pick(p, n);
            if (pk > 0) m.AddUpgrade(d.TreeNodes[n].Options[pk - 1].Effect, d.TreeNodes[n].Options[pk - 1].Value);
        }
    }
}
