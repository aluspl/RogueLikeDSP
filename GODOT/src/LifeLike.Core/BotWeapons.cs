using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// v0.23.0: bronie w bocie testów balansu (Bot.Step): ocena broni wg materiałów najbliższych etapów (znajdźki, wymiana),
/// zamiana z pasa pod materiał celu, tryb B, gdy trafia więcej albo jego efekt coś daje, atak wzorem A i ustawienie pod wzór.
/// </summary>
public static class BotWeapons
{
    private static readonly int[,] Dirs = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };

    /// <summary>O ile (%) lepsza musi być broń, by bot ją podniósł / przełożył z pasa (bez przekładania tam i z powrotem).</summary>
    private const int BetterPct = 115;


    /// <summary>v0.23.0: premia oceny (%) za każdy profit broni – także za te, których nie widać w średnim ciosie (kryt, odnowienie B).</summary>
    private const int PerkScorePct = 6;

    /// <summary>Ile pól drogi (koszt 4 na pole) bot zboczy po lepszą broń.</summary>
    private const int ToolGoalCost = 28;

    /// <summary>
    /// Ocena broni na najbliższe etapy: średni cios (×10, z mnożnikiem materiału) w problemy z puli bieżącego i następnego etapu
    /// (boss ×2), z premią za zasięg i wzór wielopolowy. weapon -1 = broń w ręku (z ulepszeniem, rzadkością i profitami).
    /// v0.23.0: perks = profity broni weapon (średni cios je zawiera) i premia PerkScorePct za każdy profit.
    /// </summary>
    public static int Score(Game g, int weapon, int rarity, int perks = 0)
    {
        if (cacheGame != g) // ocena zależy od etapu, broni i premii bohatera – pamięć na jedną budowę (wątek)
        {
            cacheGame = g;
            cache = new Dictionary<long, int>();
        }
        if (weapon < 0) perks = g.WeaponPerks;
        var key = (((((long)g.Stage * 64 + weapon + 1) * 8 + rarity) * 64 + (weapon < 0 ? g.WeaponOverride + 1 : 0)) * 1024
                   + (weapon < 0 ? g.WeaponLvl * 128 + g.WeaponRarity * 32 : 0) + g.HeroLevel + g.DmgBonus * 40) * 256 + perks;
        if (cache.TryGetValue(key, out var v)) return v;
        v = ScoreRaw(g, weapon, rarity, perks) * (100 + PerkScorePct * System.Numerics.BitOperations.PopCount((uint)perks)) / 100;
        cache[key] = v;
        return v;
    }

    [ThreadStatic]
    private static Game cacheGame;

    [ThreadStatic]
    private static Dictionary<long, int> cache;

    private static int ScoreRaw(Game g, int weapon, int rarity, int perks)
    {
        long sum = 0, n = 0;
        var last = Math.Min(g.Stage + 1, g.RouteCount() - 1);
        for (var s = g.Stage; s <= last; ++s)
        {
            var sd = g.SDef(s);
            foreach (var e in sd.Pool)
            {
                sum += Avg(g, weapon, rarity, perks, e);
                ++n;
            }
            if (sd.Boss >= 0)
            {
                sum += 2 * Avg(g, weapon, rarity, perks, sd.Boss);
                n += 2;
            }
        }
        var avg = n > 0 ? (int)(sum / n) : Avg(g, weapon, rarity, perks, -1);
        return avg * Shape(g, weapon) / 100 * (100 + ProcPct(weapon < 0 ? g.Weapon : g.D.Weapons[weapon])) / 100;
    }

    /// <summary>Ocena broni przeciw jednemu problemowi (zamiana z pasa pod cel).</summary>
    private static int ScoreVs(Game g, int weapon, int rarity, int perks, int defId)
    {
        if (weapon < 0) perks = g.WeaponPerks;
        return Avg(g, weapon, rarity, perks, defId) * Shape(g, weapon) / 100 * (100 + PerkScorePct * System.Numerics.BitOperations.PopCount((uint)perks)) / 100;
    }

    private static int Avg(Game g, int weapon, int rarity, int perks, int defId) =>
        weapon < 0 ? g.WeaponBreakdown(defId).Avg10 : g.WeaponBreakdown(defId, weapon, -1, -1, 0, rarity, perks).Avg10;

    /// <summary>
    /// v0.23.0: wartość mechaniki artefaktu (%) – średnio na cios: dodatkowe obrażenia (DmgPct - 100) / N, efekt unieruchamiający
    /// albo podpalenie ~30% / N, przeskok (łańcuch, ogień) ~5% / N.
    /// </summary>
    public static int ProcPct(WeaponDef w)
    {
        var p = w.Proc;
        if (p is null) return 0;
        var n = Math.Max(1, p.Every);
        var v = (p.DmgPct - 100) / n;
        if (p.Effect is ModeEffect.Stun or ModeEffect.Root or ModeEffect.Burn or ModeEffect.Dot) v += 30 / n;
        if (p.Effect2 is ModeEffect.Chain or ModeEffect.Spread) v += 5 / n;
        return v;
    }

    /// <summary>Premia (%) za zasięg (+25% za pole ponad 1, do 3) i wzór trybu A trafiający kilka pól (+10%).</summary>
    private static int Shape(Game g, int weapon)
    {
        var w = weapon < 0 ? g.Weapon : g.D.Weapons[weapon];
        var multi = w.ModeA.Pattern is PatternKind.Arc or PatternKind.Line or PatternKind.LineBoth or PatternKind.Line4 or PatternKind.Spray
            or PatternKind.Around or PatternKind.Cross or PatternKind.Throw3x3;
        return 100 + 25 * (Math.Min(3, w.Range) - 1) + (multi ? 10 : 0);
    }

    /// <summary>Czy bot chce narzędzie t (rzadkość r) zamiast broni w ręku: wyraźnie lepsza ocena na najbliższe etapy.</summary>
    public static bool WantsTool(Game g, int t, int rarity, int perks = 0)
    {
        if (t < 0 || t >= g.D.Tools.Length) return false;
        var w = g.D.Tools[t].Weapon;
        if (g.HasBelt() && g.Weapon2 < 0) return true; // pusta kabura: broń trafi do drugiego miejsca
        var r = g.D.Weapons[w].Master ? g.D.WeaponRarities.Length - 1 : rarity;
        var hand = Score(g, -1, 0);
        if (g.HasBelt() && g.Weapon2 >= 0) hand = Math.Min(hand, Score(g, g.Weapon2, g.Weapon2Rarity, g.Weapon2Perks)); // nadepnięta zastąpi gorszą (bot przełoży)
        return Score(g, w, r, perks) * 100 > hand * BetterPct;
    }

    /// <summary>v0.23.2: broń z mapy w plecaku, gdy jej ocena to co najmniej BackpackPct% broni w ręku.</summary>
    private const int BackpackPct = 90;

    /// <summary>
    /// v0.23.2: karta broni – wyraźnie lepszą bierze (stara do plecaka), porównywalną chowa do plecaka (na Skup / zapas), resztę
    /// zostawia. Tylko broń domyślna: zawsze Zostaw.
    /// </summary>
    public static void ToolOffer(Game g, BotOptions o)
    {
        if (!g.HasToolOffer) return;
        if (o.DefaultWeaponOnly)
        {
            g.DeclineTool();
            return;
        }
        if (WantsTool(g, g.ToolOffer, g.ToolOfferRarity, g.ToolOfferPerks))
        {
            g.AcceptTool();
            return;
        }
        var w = g.D.Tools[g.ToolOffer].Weapon;
        var r = g.D.Weapons[w].Master ? g.D.WeaponRarities.Length - 1 : g.ToolOfferRarity;
        if (g.BackpackFull() || Score(g, w, r, g.ToolOfferPerks) * 100 < Score(g, -1, 0) * BackpackPct || !g.ToolToBackpack()) g.DeclineTool();
    }

    /// <summary>Pola z bronią, której bot nie chce (tylko domyślna: każda) – nadepnięcie podnosi ją bez pytania.</summary>
    public static bool[] AvoidCells(Game g, BotOptions o)
    {
        bool[] avoid = null;
        for (var i = 0; i < g.PickupsCount; ++i)
        {
            var p = g.Pickups[i];
            if (!p.Active || p.Type != PickupType.Tool) continue;
            if (!o.DefaultWeaponOnly && WantsTool(g, p.Arg, p.Trait, p.Perks)) continue;
            avoid ??= new bool[Level.W * Level.H];
            avoid[p.Y * Level.W + p.X] = true;
        }
        return avoid;
    }

    /// <summary>Najbliższa chciana broń na ziemi w zasięgu ToolGoalCost drogi.</summary>
    public static bool ToolGoal(Game g, int[] hd, out int gx, out int gy)
    {
        gx = gy = -1;
        var best = ToolGoalCost + 1;
        for (var i = 0; i < g.PickupsCount; ++i)
        {
            var p = g.Pickups[i];
            var dd = hd[p.Y * Level.W + p.X];
            if (!p.Active || p.Type != PickupType.Tool || dd < 0 || dd >= best || !WantsTool(g, p.Arg, p.Trait, p.Perks)) continue;
            best = dd;
            gx = p.X;
            gy = p.Y;
        }
        return gx >= 0;
    }

    /// <summary>
    /// Tura walki bronią: zamiana z pasa (lepszy mnożnik na najbliższy cel, gdy bezpiecznie), tryb B (lepszy od A albo jedyny
    /// w zasięgu), atak trybem A w najlepszy cel, ustawienie broni dystansowej pod wzór. True = tura zużyta.
    /// </summary>
    public static bool Fight(Game g, BotOptions o)
    {
        if (g.DangerCell(g.Hero.X, g.Hero.Y)) return false;
        var near = g.NearestVisibleEnemy();
        if (!o.DefaultWeaponOnly && TrySwap(g, near)) return true;
        if (near < 0 && g.CanWeaponBOpenWall() && g.PlayerWeaponBWall()) return true; // Łom / Przecinak / Młot: pęknięta ściana
        int ba = -1, va = 0;
        Span<int> hit = stackalloc int[HitPattern.MaxCells];
        for (var i = 0; i < g.EnemiesCount; ++i)
        {
            if (!g.Enemies[i].Alive || !g.Visible(g.Enemies[i].X, g.Enemies[i].Y)) continue;
            var v = Value(g, g.Weapon.ModeA, i, hit);
            if (v > va)
            {
                va = v;
                ba = i;
            }
        }
        if (g.WeaponBCd == 0)
        {
            int bb = -1, vb = 0;
            for (var i = 0; i < g.EnemiesCount; ++i)
            {
                if (!g.Enemies[i].Alive || !g.Visible(g.Enemies[i].X, g.Enemies[i].Y)) continue;
                var v = Value(g, g.Weapon.ModeB, i, hit);
                if (v > vb)
                {
                    vb = v;
                    bb = i;
                }
            }
            if (bb >= 0 && vb > va && g.PlayerWeaponB(bb)) return true;
        }
        if (ba >= 0 && g.Weapon.Range > 1) return g.PlayerAttack(ba); // wręcz: cios przy kroku na cel (jak dotąd)
        return g.Weapon.Range > 1 && Position(g);
    }

    /// <summary>
    /// Wartość trybu m skierowanego na cel ei (0 = cel poza wzorem): suma po trafionych mnożnika materiału × % trybu
    /// plus efekt (ogłuszenie, unieruchomienie, znak, osłabienie, obrażenia przez tury, osuszenie wodnego, pewny kryt).
    /// </summary>
    private static int Value(Game g, WeaponMode m, int ei, Span<int> hit)
    {
        var n = g.PatternTargets(m, ei, hit);
        if (n == 0) return 0;
        var w = g.Weapon;
        var v = 0;
        for (var i = 0; i < n; ++i)
        {
            var t = hit[i];
            var ed = g.D.Enemies[g.Enemies[t].DefId];
            v += HitCalc.Pct(g.D, w, ed) * m.DmgPct / 100;
            v += Effect(g, m.Effect, m.Value, m.Turns, t, ed) + Effect(g, m.Effect2, m.Value2, m.Turns2, t, ed);
        }
        return v;
    }

    private static int Effect(Game g, ModeEffect eff, int val, int turns, int t, EnemyDef ed)
    {
        var boss = t == g.Boss;
        var e = g.Enemies[t];
        switch (eff)
        {
            case ModeEffect.Stun:
                return boss || e.Stun > 0 ? 0 : (val <= 0 || val >= 100 ? 100 : val) * Math.Max(1, turns) * 6 / 10;
            case ModeEffect.Root:
                return boss || e.Root > 0 ? 0 : 15 * Math.Max(1, turns);
            case ModeEffect.Mark:
                return e.MarkTurns > 0 ? 0 : val * Math.Min(3, Math.Max(1, turns)) / 2;
            case ModeEffect.DefDown:
                return e.DefDown > 0 || ed.Defense <= 0 ? 0 : 10 * Math.Min(ed.Defense, val) * Math.Min(3, Math.Max(1, turns)) / 2;
            case ModeEffect.Dot:
                return e.Dot > 0 ? 0 : 12 * Math.Max(1, turns);
            case ModeEffect.Dry:
                return ed.Elem == Element.Water && !boss ? 80 : 0;
            case ModeEffect.CritNext:
                return 40;
            case ModeEffect.Wet:
                return e.Wet > 0 ? 0 : 10;
            default:
                return 0;
        }
    }

    /// <summary>
    /// Zamiana z pasa: druga broń lepsza o BetterPct na najbliższy widoczny problem (bez celu – na najbliższe etapy);
    /// tylko bez sąsiedniego problemu albo przy HP od połowy.
    /// </summary>
    private static bool TrySwap(Game g, int near)
    {
        if (!g.HasBelt() || g.Weapon2 < 0) return false;
        int hand, other;
        if (near >= 0)
        {
            var def = g.Enemies[near].DefId;
            hand = ScoreVs(g, -1, 0, 0, def);
            other = ScoreVs(g, g.Weapon2, g.Weapon2Rarity, g.Weapon2Perks, def);
            var adjacent = Game.Cheb(g.Hero.X, g.Hero.Y, g.Enemies[near].X, g.Enemies[near].Y) <= 1;
            if (adjacent && g.Hero.Hp * 2 < g.Hero.MaxHp) return false;
        }
        else
        {
            hand = Score(g, -1, 0);
            other = Score(g, g.Weapon2, g.Weapon2Rarity, g.Weapon2Perks);
        }
        return other * 100 > hand * BetterPct && g.PlayerSwapWeapon();
    }

    /// <summary>
    /// Broń dystansowa: problem w zasięgu, ale poza wzorem A (linia, strzał za rogiem) – krok na sąsiednie pole, z którego cel
    /// jest we wzorze i nie stoi tuż obok.
    /// </summary>
    private static bool Position(Game g)
    {
        var rg = g.WeaponRange();
        var t = -1;
        var bd = 99;
        for (var i = 0; i < g.EnemiesCount; ++i)
        {
            var e = g.Enemies[i];
            var d = Game.Cheb(g.Hero.X, g.Hero.Y, e.X, e.Y);
            if (e.Alive && g.Visible(e.X, e.Y) && d <= rg + 1 && d < bd)
            {
                bd = d;
                t = i;
            }
        }
        if (t < 0) return false;
        int hx = g.Hero.X, hy = g.Hero.Y;
        for (var k = 0; k < 4; ++k)
        {
            int nx = hx + Dirs[k, 0], ny = hy + Dirs[k, 1];
            if (!g.Lv.Passable(nx, ny) || g.Occupied(nx, ny) || g.DangerCell(nx, ny) || g.Mud(nx, ny)) continue;
            var d = Game.Cheb(nx, ny, g.Enemies[t].X, g.Enemies[t].Y);
            if (d < 2 || d > rg) continue;
            g.Hero.X = (sbyte)nx;
            g.Hero.Y = (sbyte)ny;
            var ok = g.InPattern(g.Weapon.ModeA, t);
            g.Hero.X = (sbyte)hx;
            g.Hero.Y = (sbyte)hy;
            if (ok) return g.PlayerMove(Dirs[k, 0], Dirs[k, 1]);
        }
        return false;
    }
}
