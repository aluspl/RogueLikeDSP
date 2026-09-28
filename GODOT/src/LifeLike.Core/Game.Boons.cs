using LifeLike.Core.Data;

namespace LifeLike.Core;

// v0.21.50 cz. 2: premie po etapie, elity, kombinacje stanów – port 1:1 z core.h.
public sealed partial class Game
{
    /// <summary>Wybrane premie (bity GameData.Boons).</summary>
    public ulong Boons;
    /// <summary>Oferta po etapie (1 z 3), -1 = brak.</summary>
    public readonly sbyte[] BoonOffer = [-1, -1, -1];
    /// <summary>Losowania oferty w tej budowie (1 płatne + darmowe z Respektu).</summary>
    public byte BoonRerolls;
    /// <summary>Żywioł ciosu z mocy zawodu (bity 1 prąd, 2 iskra) – tylko w trakcie mocy.</summary>
    public byte HitCtx;
    /// <summary>Bitmaska: kombinacje w tej turze (bit = ComboEffect, +3 = na bohaterze); warstwa prezentacji czyta i zeruje.</summary>
    public byte ComboEvents;

    // ------------------------------------------------------------------ premie po etapie
    public bool HasBoon(int b) => ((Boons >> b) & 1ul) != 0;

    public int BoonsOwned()
    {
        var n = 0;
        for (var b = 0; b < D.Boons.Length; ++b)
        {
            if (HasBoon(b)) n++;
        }
        return n;
    }

    /// <summary>Suma wartości wybranych premii danego rodzaju.</summary>
    public int BoonSum(BoonEffect e)
    {
        var v = 0;
        for (var b = 0; b < D.Boons.Length && (Boons >> b) != 0; ++b)
        {
            if (HasBoon(b) && D.Boons[b].Effect == e) v += D.Boons[b].Value;
        }
        return v;
    }

    /// <summary>Ile premii ze zbioru m ma znacznik t (GameData.BoonTags).</summary>
    public static int TagCountIn(GameData d, ulong m, int t)
    {
        var n = 0;
        for (var b = 0; b < d.Boons.Length && (m >> b) != 0; ++b)
        {
            if (((m >> b) & 1ul) != 0 && ((d.Boons[b].Tags >> t) & 1) != 0) n++;
        }
        return n;
    }

    public int TagCount(int t) => TagCountIn(D, Boons, t);

    /// <summary>Synergia s: jeden znacznik – 2+ premie z nim; dwa znaczniki – po jednej z każdego (razem 2+).</summary>
    public static bool SynergyActiveIn(GameData d, ulong m, int s)
    {
        var tg = d.Synergies[s].Tags;
        int any = 0, tags = 0;
        for (var t = 0; t < d.BoonTags.Length; ++t)
        {
            if (((tg >> t) & 1) == 0) continue;
            ++tags;
            if (TagCountIn(d, m, t) == 0) return false;
        }
        for (var b = 0; b < d.Boons.Length && (m >> b) != 0; ++b)
        {
            if (((m >> b) & 1ul) != 0 && (d.Boons[b].Tags & tg) != 0) any++;
        }
        return tags > 0 && any >= d.SynergyAt;
    }

    public bool SynergyActive(int s) => SynergyActiveIn(D, Boons, s);

    public static int SynergyMaskIn(GameData d, ulong m)
    {
        var r = 0;
        for (var s = 0; s < d.Synergies.Length; ++s)
        {
            if (SynergyActiveIn(d, m, s)) r |= 1 << s;
        }
        return r;
    }

    public int SynergyMask() => SynergyMaskIn(D, Boons);

    /// <summary>Wartość aktywnych synergii danego rodzaju (0 = żadna).</summary>
    public int SynergyValue(SynergyEffect e)
    {
        var v = 0;
        for (var s = 0; s < D.Synergies.Length; ++s)
        {
            if (D.Synergies[s].Effect == e && SynergyActive(s)) v += D.Synergies[s].Value;
        }
        return v;
    }

    public bool SynergyOn(SynergyEffect e)
    {
        for (var s = 0; s < D.Synergies.Length; ++s)
        {
            if (D.Synergies[s].Effect == e && SynergyActive(s)) return true;
        }
        return false;
    }

    public int BoonLuck() => BoonSum(BoonEffect.Luck) + SynergyValue(SynergyEffect.Luck);

    /// <summary>Obrona z premii: Beton B30 i podobne + Zbrojenie (+1 za każdą premię Beton).</summary>
    public int BoonDefense()
    {
        var v = BoonSum(BoonEffect.Def);
        for (var s = 0; s < D.Synergies.Length; ++s)
        {
            if (D.Synergies[s].Effect != SynergyEffect.Armor || !SynergyActive(s)) continue;
            for (var t = 0; t < D.BoonTags.Length; ++t)
            {
                if (((D.Synergies[s].Tags >> t) & 1) != 0) v += D.Synergies[s].Value * TagCount(t);
            }
        }
        return v;
    }

    /// <summary>Premia zawodu: wzmocnienie mocy (opis w danych).</summary>
    public int BoonPower() => BoonSum(BoonEffect.Power);

    public bool HasBoonOffer => BoonOffer[0] >= 0;

    /// <summary>Losowania oferty: 1 płatne na budowę + darmowe z Respektu (Druga oferta, zużywane najpierw).</summary>
    public int RerollsLeft() => Math.Max(0, 1 + Bonus.Rerolls - BoonRerolls);

    public int RerollPrice() => BoonRerolls < Bonus.Rerolls ? 0 : D.BoonRerollCost;

    public bool CanReroll() => HasBoonOffer && RerollsLeft() > 0 && Cash >= RerollPrice();

    public bool BoonAvailable(int b)
    {
        if (HasBoon(b) || (D.Boons[b].Cls >= 0 && D.Boons[b].Cls != Cls)) return false;
        for (var k = 0; k < 3; ++k)
        {
            if (BoonOffer[k] == b) return false;
        }
        return true;
    }

    /// <summary>Wagi rzadkości: szczęście przesuwa trochę z zwykłych na rzadkie i legendarne.</summary>
    public int BoonWeight(int rarity)
    {
        var l = Math.Max(0, Luck());
        if (rarity == 1) return D.BoonRarities[1].Weight + l * D.BoonLuckRare;
        if (rarity == 2) return D.BoonRarities[2].Weight + l * D.BoonLuckLegend;
        return Math.Max(10, D.BoonRarities[0].Weight - l * (D.BoonLuckRare + D.BoonLuckLegend));
    }

    private static readonly int[,] RarityOrder = { { 0, 1, 2 }, { 1, 0, 2 }, { 2, 1, 0 } };

    /// <summary>Oferta 1 z 3 po etapie: osobny generator z seeda budowy, etapu i losowania (bez wpływu na RNG gry).</summary>
    public void RollBoons()
    {
        for (var k = 0; k < 3; ++k) BoonOffer[k] = -1;
        if (D.Boons.Length == 0) return;
        var br = new Rng();
        br.Seed((RunSeed ^ ((uint)(Stage + 1 + Tier * 16) * 2654435761u) ^ ((uint)(BoonRerolls + 1) * 40503u)) * 2246822519u);
        for (var k = 0; k < 3; ++k)
        {
            int total = BoonWeight(0) + BoonWeight(1) + BoonWeight(2), roll = br.Range(1, total), rar = 0;
            while (rar < 2 && roll > BoonWeight(rar)) roll -= BoonWeight(rar++);
            for (var o = 0; o < 3 && BoonOffer[k] < 0; ++o) // brak w rzadkości: najpierw niższa
            {
                int want = RarityOrder[rar, o], n = 0;
                for (var b = 0; b < D.Boons.Length; ++b)
                {
                    if (BoonAvailable(b) && D.Boons[b].Rarity == want) n++;
                }
                if (n == 0) continue;
                var pick = br.Range(0, n - 1);
                for (var b = 0; b < D.Boons.Length; ++b)
                {
                    if (BoonAvailable(b) && D.Boons[b].Rarity == want && pick-- == 0)
                    {
                        BoonOffer[k] = (sbyte)b;
                        break;
                    }
                }
            }
        }
    }

    /// <summary>Wybór premii k z oferty: skutki natychmiastowe od razu; nowa synergia – komunikat (baner w warstwie gry).</summary>
    public bool PickBoon(int k)
    {
        if (k < 0 || k > 2 || BoonOffer[k] < 0) return false;
        int b = BoonOffer[k];
        var bd = D.Boons[b];
        var before = SynergyMask();
        Boons |= 1ul << b;
        for (var i = 0; i < 3; ++i) BoonOffer[i] = -1;
        switch (bd.Effect)
        {
            case BoonEffect.MaxHp:
                Hero.MaxHp = (short)(Hero.MaxHp + bd.Value);
                Hero.Hp = (short)(Hero.Hp + bd.Value);
                break;
            case BoonEffect.Cash: Cash += Income(bd.Value); break;
            case BoonEffect.Mats:
                for (var m = 0; m < D.Materials.Length; ++m) AddMaterial(m, bd.Value);
                break;
            case BoonEffect.Thermos: Thermos = Math.Min(ThermosCap(), Thermos + 1); break;
            case BoonEffect.Sight: UpdateFov(); break;
        }
        Push(Msg("Premia: ").Add(bd.Name).As(LogKind.Loot));
        var now = SynergyMask();
        for (var s = 0; s < D.Synergies.Length; ++s)
        {
            if (((now >> s) & 1) != 0 && ((before >> s) & 1) == 0) Push(Msg("Synergia: ").Add(D.Synergies[s].Name).Add("!").As(LogKind.Good));
        }
        return true;
    }

    public bool RerollBoons()
    {
        if (!CanReroll()) return false;
        Cash -= RerollPrice();
        ++BoonRerolls;
        RollBoons();
        Push(Msg("Nowa oferta premii"));
        return true;
    }

    public void SkipBoons()
    {
        for (var i = 0; i < 3; ++i) BoonOffer[i] = -1;
    }

    private static readonly int[] BoonPrio = [60, 58, 40, 50, 55, 28, 30, 25, 35, 8, 5, 38, 15, 15, 18, 18, 3, 30, 30, 20, 45, 5, 3, 10];

    /// <summary>Wybór bota (testy balansu, test złoty): prosta kolejność skutków, rzadkość, nowa synergia.</summary>
    public int BoonPriority(int b)
    {
        var bd = D.Boons[b];
        var v = bd.Rarity * 100 + ((int)bd.Effect < BoonPrio.Length ? BoonPrio[(int)bd.Effect] : 0);
        if (SynergyMaskIn(D, Boons | (1ul << b)) != SynergyMask()) v += 20; // włączy synergię
        return v;
    }

    public int BotBoonChoice()
    {
        int best = -1, bv = -1;
        for (var k = 0; k < 3; ++k)
        {
            if (BoonOffer[k] >= 0 && BoonPriority(BoonOffer[k]) > bv)
            {
                bv = BoonPriority(BoonOffer[k]);
                best = k;
            }
        }
        return best;
    }

    /// <summary>Bot losuje ofertę jeszcze raz tylko za darmo (Druga oferta) i gdy same zwykłe premie.</summary>
    public bool BotWantsReroll() => HasBoonOffer && RerollPrice() == 0 && CanReroll() && BoonPriority(BoonOffer[BotBoonChoice()]) < 100;

    // ------------------------------------------------------------------ elity
    public bool IsElite(int ei) => Enemies[ei].Elite >= 0;

    public bool EliteIs(in Actor e, EliteEffect x) => e.Elite >= 0 && D.Elites[e.Elite].Effect == x;

    public int EliteChance() =>
        D.EliteActPct.Length == 0 ? 0 : Math.Max(0, D.EliteActPct[D.Stages[Stage].Act] + D.EliteDiffPct[Diff] + Tier * D.EliteTierPct);

    public void MakeElite(int i, int trait)
    {
        ref var a = ref Enemies[i];
        a.Elite = (sbyte)trait;
        a.Hp = a.MaxHp = (short)Math.Max(1, a.MaxHp * D.EliteHpPct / 100);
    }

    /// <summary>Obrona elity (Tarcza) problemu ei.</summary>
    public int EnemyEliteDef(int ei) => EliteIs(Enemies[ei], EliteEffect.Shield) ? D.Elites[Enemies[ei].Elite].Value : 0;

    /// <summary>Obrona problemu ei: z danych + Tarcza elity.</summary>
    public int EnemyDefense(int ei) => D.Enemies[Enemies[ei].DefId].Defense + EnemyEliteDef(ei);

    /// <summary>Nazwa problemu z przedrostkiem elity („Zbrojony Przeciek”, „Uparta Pleśń”).</summary>
    public Message EnemyName(Message m, int ei)
    {
        var e = Enemies[ei];
        var ed = D.Enemies[e.DefId];
        if (e.Elite >= 0) m.Add(D.Elites[e.Elite].Prefix[ed.Gender]).Add(" ");
        return m.Add(ed.Name);
    }

    public string EnemyName(int ei) => EnemyName(new Message(), ei).Text;

    // ------------------------------------------------------------------ kombinacje stanów
    public bool EnemyWet(int ei) => Enemies[ei].Wet > 0 || D.Enemies[Enemies[ei].DefId].Elem == Element.Water;

    public bool EnemyDusty(int ei) => (Enemies[ei].Flags & ActorFlag.Dusty) != 0;

    public bool EnemyFrozen(int ei) => (Enemies[ei].Flags & ActorFlag.Frozen) != 0;

    public bool HeroWet() => HeroStatus[(int)StatusEffect.Wet] > 0;

    /// <summary>Bohater mokry (kałuża, cios wody): komunikat tylko, gdy był suchy.</summary>
    public void SoakHero()
    {
        if (HeroWet()) HeroStatus[(int)StatusEffect.Wet] = (sbyte)Math.Max(HeroStatus[(int)StatusEffect.Wet], D.HeroWetTurns);
        else ApplyStatus(StatusEffect.Wet, D.HeroWetTurns);
    }

    /// <summary>Żywioł ciosu bohatera: broń (Próbnik – prąd), premie, synergia Przepięcie, moc zawodu (Łańcuch).</summary>
    public bool HitPower() =>
        Weapon.Elem == Element.Power || (HitCtx & 1) != 0 || BoonSum(BoonEffect.Electric) > 0 || SynergyOn(SynergyEffect.Conduct);

    /// <summary>Iskra: broń (Szlifierka, Pistolet do kotew), premie, moc zawodu (Wirówka).</summary>
    public bool HitSpark() => Weapon.Elem == Element.Spark || (HitCtx & 2) != 0 || BoonSum(BoonEffect.Spark) > 0;

    /// <summary>Mokry + prąd: porażenie celu i mokrych problemów obok (Przepięcie: dalej).</summary>
    public void ComboShock(int ei, int x, int y)
    {
        var c = D.Combos[(int)ComboEffect.ShockArea];
        var rad = c.Radius + SynergyValue(SynergyEffect.Conduct);
        ComboEvents = (byte)(ComboEvents | (1 << (int)ComboEffect.ShockArea));
        Push(Msg(c.Short).Add(" ").Add(c.Name).As(LogKind.Good));
        for (var i = 0; i < EnemiesCount && St == GameStatus.Playing; ++i)
        {
            var e = Enemies[i];
            if (e.Alive && (i == ei || (EnemyWet(i) && Cheb(x, y, e.X, e.Y) <= rad))) DamageEnemy(i, c.Value, false, c.Name);
        }
    }

    /// <summary>Pył + iskra: wybuch pyłu wokół zapylonego celu (pył znika z trafionych).</summary>
    public void ComboDust(int x, int y)
    {
        var c = D.Combos[(int)ComboEffect.DustBlast];
        var sp = SynergyValue(SynergyEffect.Sparks);
        int rad = c.Radius + (sp > 0 ? 1 : 0), dmg = c.Value + sp;
        ComboEvents = (byte)(ComboEvents | (1 << (int)ComboEffect.DustBlast));
        Push(Msg(c.Short).Add(" ").Add(c.Name).As(LogKind.Good));
        for (var i = 0; i < EnemiesCount; ++i)
        {
            if (Cheb(x, y, Enemies[i].X, Enemies[i].Y) <= rad) Enemies[i].Flags = (byte)(Enemies[i].Flags & ~ActorFlag.Dusty);
        }
        for (var i = 0; i < EnemiesCount && St == GameStatus.Playing; ++i)
        {
            if (Enemies[i].Alive && Cheb(x, y, Enemies[i].X, Enemies[i].Y) <= rad) DamageEnemy(i, dmg, false, c.Name);
        }
    }

    /// <summary>Zamróz + uderzenie: cios wręcz w zmrożony pęka go (+Value% ciosu, osobno).</summary>
    public void ComboCrack(int ei, int dmg)
    {
        var c = D.Combos[(int)ComboEffect.Crack];
        Enemies[ei].Flags = (byte)(Enemies[ei].Flags & ~ActorFlag.Frozen);
        ComboEvents = (byte)(ComboEvents | (1 << (int)ComboEffect.Crack));
        Push(Msg(c.Short).Add(" ").Add(c.Name).As(LogKind.Good));
        DamageEnemy(ei, Math.Max(1, dmg * c.Value / 100), false, c.Name);
    }

    /// <summary>Elita „wzywa pomoc”: raz, przy Value% HP – słabszy problem tego samego rodzaju obok (połowa HP, bez cechy).</summary>
    public void EliteCall(int ei)
    {
        Enemies[ei].Flags = (byte)(Enemies[ei].Flags | ActorFlag.Called);
        if (!FreeAround(Enemies[ei].X, Enemies[ei].Y, Hero.X, Hero.Y, out var x, out var y)) return;
        var slot = FreeSlot();
        if (slot < 0) return;
        ref var c = ref Enemies[slot];
        c = new Actor();
        c.X = (sbyte)x;
        c.Y = (sbyte)y;
        c.DefId = Enemies[ei].DefId;
        c.Hp = c.MaxHp = (short)Math.Max(1, D.Enemies[c.DefId].MaxHealth * EnemyHpPct() / 100 / 2);
        c.Alive = true;
        c.Awake = true;
        c.Stun = 1;
        c.Flags = ActorFlag.Child;
        Push(Msg(D.Enemies[c.DefId].Name).Add(": wzywa pomoc!").As(LogKind.Bad));
    }

    /// <summary>Nagroda za elitę: pewny drop (paczka sprzętu co najmniej solidna), materiały, Respekt.</summary>
    public void EliteReward(int ei)
    {
        int ex = Enemies[ei].X, ey = Enemies[ei].Y;
        Respect += D.EliteRespect;
        AddMaterial(R.Range(0, D.Materials.Length - 1), D.EliteMats);
        if (PickupsCount < MaxPickups && !PickupAt(ex, ey)) DropAt(ex, ey, D.EliteGearMin);
        Push(Msg("Elita usunięta! Respekt +").Add(D.EliteRespect).As(LogKind.Loot));
    }

    /// <summary>Krok problemu prosto w stronę bohatera (najpierw oś z większą odległością).</summary>
    public bool ChaseStep(int i)
    {
        ref var e = ref Enemies[i];
        int dx = Math.Sign(Hero.X - e.X), dy = Math.Sign(Hero.Y - e.Y);
        var xfirst = Math.Abs(Hero.X - e.X) >= Math.Abs(Hero.Y - e.Y);
        Span<int> tries = [xfirst ? dx : 0, xfirst ? 0 : dy, xfirst ? 0 : dx, xfirst ? dy : 0];
        for (var t = 0; t < 2; t++)
        {
            int tx = tries[t * 2], ty = tries[t * 2 + 1];
            if (tx == 0 && ty == 0) continue;
            int nx = e.X + tx, ny = e.Y + ty;
            if (Lv.At(nx, ny) == Tile.Floor && !Occupied(nx, ny))
            {
                e.X = (sbyte)nx;
                e.Y = (sbyte)ny;
                return true;
            }
        }
        return false;
    }
}
