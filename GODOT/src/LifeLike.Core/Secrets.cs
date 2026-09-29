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

    public static bool CosmeticUnlocked(GameData d, Profile p, int k) => k >= 0 && Owned(d, p, SecretReward.Cosmetic, k);

    /// <summary>Wygląd na budowie: wybrany i odblokowany (złota kielnia – zawsze po odblokowaniu).</summary>
    public static bool CosmeticOn(GameData d, Profile p, int k) =>
        CosmeticUnlocked(d, p, k) && (k == d.CosmeticGold || ((p.Cosmetic >> k) & 1) != 0);

    public static void ToggleCosmetic(GameData d, Profile p, int k)
    {
        if (CosmeticUnlocked(d, p, k)) p.Cosmetic = (byte)(p.Cosmetic ^ (1 << k));
    }

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
