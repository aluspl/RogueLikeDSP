using System.Collections.Generic;
using LifeLike.Core;
using LifeLike.Core.Data;

namespace LifeLike.Game.Session;

/// <summary>
/// v0.21.52 cz. b: banery nowych poziomów na końcu budowy (push_progress na GBA) – inspektor (#44), mistrzostwo zawodu
/// (#45), stopnie inwestora (#48), każdy z nagrodą. Kilka poziomów naraz: dwa pierwsze z nagrodą, potem jeden zbiorczy.
/// </summary>
public static class ProgressBanners
{
    public static List<(string Title, string Body)> Of(GameData d, ProgressGain pg, int stakeBefore, int stakeAfter)
    {
        var list = new List<(string, string)>();
        for (var l = pg.InspBefore; l < pg.InspAfter; ++l)
        {
            if (l >= pg.InspBefore + 2 && l + 1 < pg.InspAfter) continue;
            list.Add(($"Inspektor: poziom {l + 1}", l >= pg.InspBefore + 2 ? "Nagrody: Profil > Odznaki" : Progress.RewardLabel(d, d.InspectorLevels[l], pg.Cls)));
        }
        if (pg.Cls >= 0 && pg.MasteryAfter > pg.MasteryBefore) // tylko ostatni poziom (nagrody w profilu)
        {
            var l = pg.MasteryAfter - 1;
            list.Add(($"Mistrzostwo: poziom {l + 1}", Progress.RewardLabel(d, d.MasteryLevels[l], pg.Cls)));
        }
        for (var l = System.Math.Max(stakeBefore, stakeAfter - 2); l < stakeAfter; ++l) // najwyżej dwa ostatnie stopnie
            list.Add(($"Stopień inwestora {d.StakeRanks[l].Xp}", Progress.RewardLabel(d, d.StakeRanks[l], pg.Cls)));
        return list;
    }
}
