using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// v0.23.0: easter egg – Kawka w profilu i ocenach (meta.h: dog_bank, dog_quip): budowy z pogłaskaniem (odznaka Psi przyjaciel)
/// i sarkastyczna ocena postępów zależna od stanu budowy i profilu.
/// </summary>
public static class Dog
{
    /// <summary>Budowa z pogłaskaną Kawką: +1 w profilu (raz na budowę – bit Game.DogBanked). Zwraca true, gdy doliczono teraz.</summary>
    public static bool Bank(Profile p, Game g)
    {
        if ((g.DogFlags & Game.DogPetted) == 0 || (g.DogFlags & Game.DogBanked) != 0) return false;
        g.DogFlags = (byte)(g.DogFlags | Game.DogBanked);
        if (p.DogBuilds < 255) p.DogBuilds++;
        return true;
    }

    public static bool Met(Profile p) => p.DogBuilds > 0;

    /// <summary>Czy ocena q pasuje do stanu budowy i profilu.</summary>
    public static bool Fits(Profile p, Game g, DogQuip q)
    {
        var hpPct = g.Hero.MaxHp > 0 ? g.Hero.Hp * 100 / g.Hero.MaxHp : 100;
        var st = g.StageNumber();
        var v = q.Value;
        return q.When switch
        {
            DogCond.Any => true,
            DogCond.First => st == 1 && g.Kills == 0,
            DogCond.HelmetStage => st >= v && g.Equipped[0] >= 0,
            DogCond.HpLow => hpPct <= v,
            DogCond.NoHelmet => st >= v && g.Equipped[0] < 0,
            DogCond.Master => g.WeaponRarity >= 3 || g.Weapon.Master,
            DogCond.HpFull => st >= v && g.Hero.Hp >= g.Hero.MaxHp,
            DogCond.NoKills => st >= v && g.Kills == 0,
            DogCond.KillsGe => g.Kills >= v,
            DogCond.LossesGe => p.Runs - p.Wins >= v,
            DogCond.RunsLe => p.Runs <= v,
            DogCond.WinsGe => p.Wins >= v,
            DogCond.CoffeeGe => g.CoffeeDrunk >= v,
            DogCond.LevelGe => g.HeroLevel >= v,
            _ => false,
        };
    }

    /// <summary>Ocena po pogłaskaniu (indeks GameData.DogQuips): pasujące z warunkiem mają pierwszeństwo przed ogólnymi; kolejne
    /// głaskanie w tej budowie – następna z pasujących (od miejsca ze skrótu seeda).</summary>
    public static int Quip(GameData d, Profile p, Game g)
    {
        int n = 0, any = 0;
        foreach (var q in d.DogQuips)
        {
            if (q.When != DogCond.Any && Fits(p, g, q)) n++;
            if (q.When == DogCond.Any) any++;
        }
        var special = n > 0;
        var count = special ? n : any;
        if (count == 0) return 0;
        var k = (int)(((g.DogHash() >> 16) + (uint)Math.Max(0, g.DogPets - 1)) % (uint)count);
        for (var i = 0; i < d.DogQuips.Length; i++)
        {
            var q = d.DogQuips[i];
            var ok = special ? q.When != DogCond.Any && Fits(p, g, q) : q.When == DogCond.Any;
            if (ok && k-- == 0) return i;
        }
        return 0;
    }
}
