namespace LifeLike.Core;

/// <summary>
/// Skrót FNV-1a stanu gry po każdym kroku – liczony identycznie w GBA/tests/golden_dump.cpp (funkcja digest).
/// Kolejność pól jest częścią formatu testu złotego: zmiana tutaj wymaga zmiany w C++ i nowych plików golden/*.json.
/// </summary>
public static class StateDigest
{
    private sealed class Fnv
    {
        public uint H = 2166136261u;

        public void Add(int v)
        {
            var u = unchecked((uint)v);
            for (var i = 0; i < 4; i++)
            {
                H ^= (u >> (8 * i)) & 0xFF;
                H *= 16777619u;
            }
        }
    }

    public static uint Of(Game g)
    {
        var f = new Fnv();
        f.Add(g.Hero.X);
        f.Add(g.Hero.Y);
        f.Add(g.Hero.Hp);
        f.Add(g.Hero.MaxHp);
        f.Add(g.Hero.Alive ? 1 : 0);
        f.Add(g.Turns);
        f.Add(g.Score);
        f.Add(g.Cash);
        f.Add(g.XpPct);
        f.Add(g.RunXp);
        f.Add(g.HeroLevel);
        f.Add((int)g.St);
        f.Add(unchecked((int)g.R.S));
        f.Add(g.LogSerial);
        f.Add(g.AbilityCd);
        f.Add(g.DefBonus);
        f.Add(g.DmgBonus);
        f.Add(g.Stage);
        f.Add(g.Tier);
        f.Add(g.Kills);
        f.Add(g.SlamTimer);
        f.Add(g.SlamX);
        f.Add(g.SlamY);
        f.Add(g.SlamCounter);
        f.Add(g.WeaponOverride);
        f.Add(g.WallsCount);
        f.Add(g.EnemiesCount);
        for (var i = 0; i < g.EnemiesCount; i++)
        {
            var e = g.Enemies[i];
            f.Add(e.X);
            f.Add(e.Y);
            f.Add(e.Hp);
            f.Add(e.MaxHp);
            f.Add(e.DefId);
            f.Add(e.Alive ? 1 : 0);
            f.Add(e.Awake ? 1 : 0);
            f.Add(e.Stun);
        }
        f.Add(g.PickupsCount);
        for (var i = 0; i < g.PickupsCount; i++)
        {
            var p = g.Pickups[i];
            f.Add(p.X);
            f.Add(p.Y);
            f.Add((int)p.Type);
            f.Add(p.Active ? 1 : 0);
            f.Add(p.Arg);
            f.Add(p.Trait);
        }
        for (var i = 0; i < 5; i++) f.Add(g.HeroStatus[i]);
        for (var i = 0; i < 4; i++) f.Add(g.Equipped[i]);
        for (var i = 0; i < 4; i++) f.Add(g.EquippedTrait[i]);
        f.Add(g.Thermos);
        f.Add(g.OfferSlot);
        f.Add(g.OfferRarity);
        f.Add(g.OfferTrait);
        f.Add(g.HitsCount);
        for (var i = 0; i < g.HitsCount; i++)
        {
            var h = g.Hits[i];
            f.Add(h.X);
            f.Add(h.Y);
            f.Add(h.Amount);
            f.Add(h.OnHero ? 1 : 0);
            f.Add((int)h.Kind);
        }
        f.Add(g.ActCleared ? 1 : 0);
        f.Add(g.ActBonus);
        f.Add(g.StageDamage);
        f.Add(g.StageKills);
        f.Add(g.ToolsFound);
        foreach (var m in g.Log)
        {
            f.Add(m.N);
            f.Add((int)m.Kind);
            f.Add(m.Repeat);
            for (var i = 0; i < m.N; i++) f.Add(m.S[i]);
        }
        foreach (var t in g.Lv.T) f.Add((int)t);
        // v0.21.43: wydarzenie na placu, liczniki zleceń
        f.Add(g.StageEvent);
        f.Add(g.BossWakeDamage);
        f.Add(g.PowersUsed);
        f.Add(g.BrandFound);
        f.Add(g.CleanBosses);
        return f.H;
    }
}
