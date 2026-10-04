using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// v0.21.52 cz. c – port z meta.h: kolekcje. Liczniki pokonanych problemów każdego rodzaju (Profile.KillCount, do 255;
/// znak wodny KillMark jak RunKills); komplet: każdy problem aktu x Count, każdy boss (karty bossów) albo wszystkie ozdoby
/// Osiedla (album). Nagroda: stała premia, tytuł albo kolor kasku.
/// </summary>
public static class CollectionBook
{
    public static void Bank(GameData d, Profile p, Game g)
    {
        for (var e = 0; e < d.Enemies.Length; ++e)
        {
            int v = g.KillsByType[e], add = v - p.KillMark[e];
            if (add > 0) p.KillCount[e] = Meta.AddSat8(p.KillCount[e], add);
            p.KillMark[e] = (byte)Math.Max(p.KillMark[e], v);
        }
    }

    public static bool EnemyBoss(GameData d, int e) => d.Enemies[e].Slam;

    /// <summary>Postęp kompletu: have / need (rodzaje problemów z licznikiem &gt;= Count, bossowie, ozdoby).</summary>
    public static (int Have, int Need) Progress(GameData d, Profile p, int i)
    {
        var cd = d.Collections[i];
        if (cd.Kind == CollectionKind.Decor) return (Story.EstateDecor(d, p), d.EstateDecor.Length);
        int have = 0, need = 0;
        for (var e = 0; e < d.Enemies.Length; ++e)
        {
            var inSet = ((cd.Enemies >> e) & 1) != 0; // v0.21.52 cz. d: bossowie też z maski (Dom / kariera)
            if (!inSet) continue;
            ++need;
            if (p.KillCount[e] >= cd.Count) ++have;
        }
        return (have, need);
    }

    public static bool Complete(GameData d, Profile p, int i)
    {
        var (have, need) = Progress(d, p, i);
        return need > 0 && have >= need;
    }

    public static int Done(GameData d, Profile p) => Enumerable.Range(0, d.Collections.Length).Count(i => Complete(d, p, i));

    /// <summary>Komplety ukończone, a jeszcze nieogłoszone (baner na końcu budowy): zaznacza je i zwraca bity.</summary>
    public static int Check(GameData d, Profile p)
    {
        var got = 0;
        for (var i = 0; i < d.Collections.Length; ++i)
        {
            if (Complete(d, p, i) && ((p.Collections >> i) & 1) == 0) got |= 1 << i;
        }
        p.Collections = (byte)(p.Collections | got);
        return got;
    }

    public static int BossesCount(GameData d) => Enumerable.Range(0, d.Enemies.Length).Count(e => EnemyBoss(d, e));

    /// <summary>Boss nr k (karty bossów, kolejność z listy problemów) – indeks w GameData.Enemies, -1 = brak.</summary>
    public static int BossAt(GameData d, int k)
    {
        for (var e = 0; e < d.Enemies.Length; ++e)
        {
            if (EnemyBoss(d, e) && k-- == 0) return e;
        }
        return -1;
    }

    /// <summary>Nagroda kompletu słowami: „+10 zł na start”, „Tytuł: Urzędnik”, „Ceglasty kask”.</summary>
    public static string RewardLabel(GameData d, int i)
    {
        var cd = d.Collections[i];
        return cd.Reward.Reward == ProgressReward.Perk ? RunMods.PerkLabel(cd.Bonus) : LifeLike.Core.Progress.RewardLabel(d, cd.Reward, -1);
    }
}
