using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// Sekretne zlecenia (#39, v0.21.51 cz. 2) – port z meta.h: ukryte cele profilu („???” z podpowiedzią do wykonania),
/// warunki z liczników budowy (Game) i profilu, nagrody (zawód, narzędzie, wygląd, ranga Respektu), migracja v11 -> v12.
/// </summary>
public static class Secrets
{
    public static bool Done(Profile p, int i) => ((p.Secrets >> i) & 1) != 0;

    /// <summary>Nagroda z wykonanego sekretnego zlecenia.</summary>
    public static bool Owned(GameData d, Profile p, SecretReward k, int index)
    {
        for (var i = 0; i < d.Secrets.Length; ++i)
        {
            if (d.Secrets[i].Reward == k && d.Secrets[i].Index == index && Done(p, i)) return true;
        }
        return false;
    }

    /// <summary>Sekretne zlecenie, które daje tę nagrodę (-1 = żadne).</summary>
    public static int Of(GameData d, SecretReward k, int index) =>
        Array.FindIndex(d.Secrets, s => s.Reward == k && s.Index == index);

    /// <summary>Wygląd: z sekretnego zlecenia albo (v0.21.52) z odznaki / zlecenia – kolory kasku.</summary>
    public static bool CosmeticUnlocked(GameData d, Profile p, int k)
    {
        if (k < 0) return false;
        if (Owned(d, p, SecretReward.Cosmetic, k)) return true;
        for (var i = 0; i < d.Badges.Length; ++i)
        {
            if (d.Badges[i].Cosmetic == k && (p.Badges & (1u << i)) != 0) return true;
        }
        for (var i = 0; i < d.Contracts.Length; ++i)
        {
            if (d.Contracts[i].Cosmetic == k && (p.Contracts & (1u << i)) != 0) return true;
        }
        // v0.21.52 cz. b: poziom inspektora, stopnie inwestora, kask mistrza (dowolny zawód na poziomie z nagrodą „helmet”)
        for (var l = 0; l < d.InspectorLevels.Length; ++l)
        {
            if (d.InspectorLevels[l].Reward == ProgressReward.Helmet && d.InspectorLevels[l].Index == k) return Progress.InspectorLevel(d, p) > l;
        }
        for (var l = 0; l < d.StakeRanks.Length; ++l)
        {
            if (d.StakeRanks[l].Reward == ProgressReward.Helmet && d.StakeRanks[l].Index == k) return Progress.StakeRank(d, p) > l;
        }
        for (var l = 0; l < d.MasteryLevels.Length; ++l)
        {
            if (d.MasteryLevels[l].Reward != ProgressReward.Helmet || d.MasteryLevels[l].Index != k) continue;
            for (var c = 0; c < d.Classes.Length; ++c)
            {
                if (Progress.MasteryLevel(d, p, c) > l) return true;
            }
            return false;
        }
        return false;
    }

    /// <summary>Kask mistrza zawodu (nagroda mistrzostwa): działa tylko zawodem z tym poziomem.</summary>
    public static bool MasteryHelmet(GameData d, int k) => d.MasteryLevels.Any(l => l.Reward == ProgressReward.Helmet && l.Index == k);

    public static bool CosmeticHelmet(GameData d, int k) => k >= 0 && k < d.Cosmetics.Length && d.Cosmetics[k].IsHelmet;

    /// <summary>Wygląd na budowie: wybrany i odblokowany (złota kielnia – zawsze po odblokowaniu); kolory kasku osobno.</summary>
    public static bool CosmeticOn(GameData d, Profile p, int k) =>
        !CosmeticHelmet(d, k) && CosmeticUnlocked(d, p, k) && (k == d.CosmeticGold || ((p.Cosmetic >> k) & 1) != 0);

    public static void ToggleCosmetic(GameData d, Profile p, int k)
    {
        if (!CosmeticHelmet(d, k) && CosmeticUnlocked(d, p, k)) p.Cosmetic = (byte)(p.Cosmetic ^ (1 << k));
    }

    /// <summary>
    /// Kolor kasku na budowie: wybrany i odblokowany wygląd (-1 = kask zawodu); kask w paski ma pierwszeństwo.
    /// cls &gt;= 0: kask mistrza tylko zawodem, który ma ten poziom mistrzostwa (inny zawód – kask zawodu).
    /// </summary>
    public static int HelmetCosmetic(GameData d, Profile p, int cls = -1)
    {
        var k = p.Helmet - 1;
        if (!CosmeticHelmet(d, k) || !CosmeticUnlocked(d, p, k)) return -1;
        if (cls >= 0 && MasteryHelmet(d, k) && !Progress.MasteryHas(d, p, cls, ProgressReward.Helmet)) return -1;
        return k;
    }

    /// <summary>Wybór koloru kasku: kolejny odblokowany albo kask zawodu.</summary>
    public static void CycleHelmet(GameData d, Profile p, int dir)
    {
        var n = d.Cosmetics.Length + 1;
        int k = p.Helmet;
        for (var i = 0; i < n; ++i)
        {
            k = (k + dir + n) % n;
            if (k == 0 || (CosmeticHelmet(d, k - 1) && CosmeticUnlocked(d, p, k - 1))) break;
        }
        p.Helmet = (byte)k;
    }

    public static int HelmetsUnlocked(GameData d, Profile p) =>
        Enumerable.Range(0, d.Cosmetics.Length).Count(k => CosmeticHelmet(d, k) && CosmeticUnlocked(d, p, k));

    /// <summary>Warunek (g = null: tylko profil – migracja, np. wygrane każdym zawodem).</summary>
    public static bool Condition(GameData d, Profile p, Game g, int i)
    {
        var sd = d.Secrets[i];
        var won = g != null && g.St == GameStatus.Won;
        return sd.Kind switch
        {
            SecretKind.NoCoffeeWin => won && g.CoffeeDrunk == 0,
            SecretKind.HelperBoss => g != null && (g.SecretFlags & Game.SecretHelperBossFlag) != 0,
            SecretKind.Storerooms => g != null && g.SecretsFound >= sd.Value,
            SecretKind.ClassWins => Meta.OpenClassesWon(d, p) >= sd.Value,
            SecretKind.PaperClean => g != null && (g.SecretFlags & Game.SecretPaperCleanFlag) != 0,
            SecretKind.ShockCombos => g != null && g.ShockCombos >= sd.Value,
            SecretKind.LowHpWin => won && g.Hero.Hp >= 1 && g.Hero.Hp <= sd.Value,
            SecretKind.FastWin => won && g.BuildDays() <= sd.Value,
            _ => false,
        };
    }

    /// <summary>Sprawdza zlecenia; zwraca bitmaskę wykonanych właśnie teraz (baner, dymek „Nowość”).</summary>
    public static int Check(GameData d, Profile p, Game g)
    {
        var got = 0;
        for (var i = 0; i < d.Secrets.Length; ++i)
        {
            if (!Done(p, i) && Condition(d, p, g, i)) got |= 1 << i;
        }
        p.Secrets = (ushort)(p.Secrets | got);
        p.SecretsNew = (ushort)(p.SecretsNew | got);
        return got;
    }

    public static int DoneCount(GameData d, Profile p) => Enumerable.Range(0, d.Secrets.Length).Count(i => Done(p, i));

    /// <summary>v11 -> v12: sekrety za to, co już jest w profilu – czekają na dymek jak nowe.</summary>
    public static void MigrateV12(GameData d, Profile p)
    {
        p.Secrets = 0;
        p.SecretsNew = 0;
        p.Cosmetic = 0;
        Array.Clear(p.RespectRanksHi);
        Check(d, p, null);
    }
}
