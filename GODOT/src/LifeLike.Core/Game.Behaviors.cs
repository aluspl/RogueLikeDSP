using LifeLike.Core.Data;

namespace LifeLike.Core;

// v0.21.49 (część 2): mechaniki aktów (błoto, porywy, pył) i zachowania problemów (port core::game z core.h).
public sealed partial class Game
{
    /// <summary>Kierunki porywu: 0 prawo, 1 dół, 2 lewo, 3 góra.</summary>
    public static readonly int[,] GustVec = { { 1, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 } };

    private static readonly string[] DirNames = ["w prawo", "w dół", "w lewo", "w górę"];

    public static string DirName(int d) => DirNames[d & 3];

    // ------------------------------------------------------------------ mechanika aktu: błoto, porywy, pył
    public ActDef ADef => D.Acts[D.Stages[Stage].Act];

    public bool ActIs(ActMechanic m) => ADef.Mechanic == m;

    /// <summary>Błoto (akt I): stały wzór na podłodze zależny od etapu; wejście kosztuje dodatkową turę. Kładka też na błoto.</summary>
    public bool Mud(int x, int y)
    {
        if (!ActIs(ActMechanic.Mud) || Lv.At(x, y) != Tile.Floor || (x * 5 + y * 11 + PatternStage() * 3) % ADef.MechValue != 0) return false;
        for (var i = 0; i < Bridges; ++i)
        {
            if (Cheb(x, y, BridgeX[i], BridgeY[i]) <= BridgeReach()) return false;
        }
        return true;
    }

    /// <summary>Tury do kolejnego porywu (0 = brak porywów w tym akcie).</summary>
    public int GustIn()
    {
        if (!ActIs(ActMechanic.Gust)) return 0;
        int v = ADef.MechValue, t = Turns - StageStartTurn;
        return v - t % v;
    }

    /// <summary>Kierunek kolejnego porywu: 0 prawo, 1 dół, 2 lewo, 3 góra.</summary>
    public int GustDir()
    {
        var t = Turns - StageStartTurn + GustIn();
        return (t / Math.Max(1, ADef.MechValue) + PatternStage()) & 3;
    }

    public void GustTick()
    {
        int v = ADef.MechValue, t = Turns - StageStartTurn;
        if (t <= 0) return;
        if (t % v == v - 1)
        {
            Push(Msg("Poryw wiatru za 1 t. ").Add(DirName(GustDir())).As(LogKind.Bad));
            return;
        }
        if (t % v != 0) return;
        int d = (t / v + PatternStage()) & 3, nx = Hero.X + GustVec[d, 0], ny = Hero.Y + GustVec[d, 1];
        if (Lv.Passable(nx, ny) && !Occupied(nx, ny))
        {
            Hero.X = (sbyte)nx;
            Hero.Y = (sbyte)ny;
            Collect();
            UpdateFov();
            Push(Msg("Poryw! Spycha cię ").Add(DirName(d)).As(LogKind.Bad));
        }
        else
        {
            Push(Msg("Poryw - trzymasz się muru").As(LogKind.Good));
        }
    }

    public int DustSight() => ActIs(ActMechanic.Dust) ? ADef.MechValue : 0;

    /// <summary>Pieczątki (Akt 0): ile dokumentów otwiera schody na tym etapie (0 = bez pieczątek, np. etap z bossem).</summary>
    public int DocsNeeded() => ActIs(ActMechanic.Stamps) && D.Stages[Stage].Boss < 0 ? ADef.MechValue : 0;

    public int DocsCount()
    {
        var n = 0;
        for (var i = 0; i < D.Documents.Length; i++) n += (Docs >> i) & 1;
        return n;
    }

    /// <summary>Schody zamknięte, dopóki nie zebrano wszystkich dokumentów.</summary>
    public bool StairsLocked() => DocsCount() < DocsNeeded();

    // ------------------------------------------------------------------ zachowania problemów
    public bool HasTag(in Actor e, int t) => e.DefId >= 0 && (D.Enemies[e.DefId].Tags & t) != 0;

    /// <summary>Wybuch po usunięciu problemu: pola w promieniu wokół miejsca (czerwone), tura na zejście.</summary>
    public bool BlastCell(int x, int y) => BlastTimer > 0 && Cheb(x, y, BlastX, BlastY) <= D.BehaviorBlastRadius;

    /// <summary>Pole zagrożone: zapowiedziany cios bossa albo wybuch.</summary>
    public bool DangerCell(int x, int y) => SlamCell(x, y) || BlastCell(x, y);

    /// <summary>Cios problemu (wręcz albo z dystansu): unik ze szczęścia, obrażenia po obronie, stan, odepchnięcie.</summary>
    public void EnemyStrike(int i, bool ranged)
    {
        ref var e = ref Enemies[i];
        var ed = D.Enemies[e.DefId];
        if (DodgePct() > 0 && R.Range(1, 100) <= DodgePct()) // szczęście: unik
        {
            AddHit(Hero.X, Hero.Y, 0, true, HitKind.Dodge);
            Push(Msg("Unik! ").Add(ed.Name).Add(" chybia").As(LogKind.Good));
            return;
        }
        var wasWet = HeroWet();
        var dmg = TakenDamage(R.Range(ed.MinDamage, ed.MaxDamage) + EnemyBonus(e) - HeroDefense() / 2);
        Hero.Hp = (short)(Hero.Hp - dmg);
        StageDamage += dmg;
        HeroHit = true;
        LogHit(e.DefId, e.Elite, ranged ? RecapKind.Ranged : RecapKind.Melee, dmg);
        AddHit(Hero.X, Hero.Y, dmg, true);
        Push(Msg(ed.Name).Add(ranged ? " z dystansu: -" : ": -").Add(dmg).Add(" HP").As(LogKind.Bad));
        if (ed.OnHit != StatusEffect.None && Hero.Hp > 0 && R.Range(1, 100) <= ed.StatusChance)
            ApplyStatus(ed.OnHit, ed.StatusTurns);
        if (ed.Elem == Element.Water && Hero.Hp > 0) SoakHero(); // woda moczy
        if (ed.Elem == Element.Power && wasWet && Hero.Hp > 0) // mokry + prąd: porażenie bohatera
        {
            var c = D.Combos[(int)ComboEffect.ShockArea];
            Hero.Hp = (short)(Hero.Hp - c.HeroValue);
            StageDamage += c.HeroValue;
            LogHit(e.DefId, e.Elite, RecapKind.Shock, c.HeroValue);
            AddHit(Hero.X, Hero.Y, c.HeroValue, true);
            ComboEvents = (byte)(ComboEvents | (8 << (int)ComboEffect.ShockArea));
            Push(Msg(c.Short).Add(" -").Add(c.HeroValue).Add(" HP").As(LogKind.Bad));
            if (Hero.Hp > 0) ApplyStatus(StatusEffect.Shock, 1);
        }
        if (EventActive(EventEffect.Rain) && Hero.Hp > 0 && R.Range(1, 100) <= D.SiteEvents[StageEvent].Value)
            ApplyStatus(StatusEffect.Slip, 2); // Ulewa w nocy: błoto na placu
        if ((ed.Tags & Behavior.Pushes) != 0 && !ranged && Hero.Hp > 0 && e.Timer == 0) // odepchnięcie o pole (co kilka tur)
        {
            e.Timer = (sbyte)D.BehaviorPushCooldown;
            int nx = Hero.X + Math.Sign(Hero.X - e.X), ny = Hero.Y + Math.Sign(Hero.Y - e.Y);
            if (Lv.Passable(nx, ny) && !Occupied(nx, ny))
            {
                Hero.X = (sbyte)nx;
                Hero.Y = (sbyte)ny;
                Collect();
                UpdateFov();
                Push(Msg(ed.Name).Add(" odpycha cię!").As(LogKind.Bad));
            }
        }
        if (Hero.Hp <= 0) HeroDown();
    }

    /// <summary>Linia strzału z (x, y) do bohatera: odległość 2..zasięg, prosto albo po skosie, bez murów i postaci po drodze.</summary>
    public bool ShotLine(int x, int y)
    {
        int dx = Hero.X - x, dy = Hero.Y - y, d = Cheb(x, y, Hero.X, Hero.Y);
        if (d < 2 || d > D.BehaviorRangedReach || !(dx == 0 || dy == 0 || Math.Abs(dx) == Math.Abs(dy))) return false;
        for (var k = 1; k < d; ++k)
        {
            int cx = x + Math.Sign(dx) * k, cy = y + Math.Sign(dy) * k;
            if (!Lv.Passable(cx, cy) || Occupied(cx, cy)) return false;
        }
        return true;
    }

    /// <summary>Strzelec ustawia się w linii: krok na pole, z którego ma czysty strzał.</summary>
    public bool RangedStep(int i)
    {
        ref var e = ref Enemies[i];
        for (var k = 0; k < 4; ++k) // problemy chodzą tylko prosto (pierwsze 4 kierunki Around)
        {
            int nx = e.X + Around[k, 0], ny = e.Y + Around[k, 1];
            if (Lv.At(nx, ny) == Tile.Floor && !Occupied(nx, ny) && ShotLine(nx, ny))
            {
                e.X = (sbyte)nx;
                e.Y = (sbyte)ny;
                return true;
            }
        }
        return false;
    }

    /// <summary>Ucieczka: krok prosto na pole dalej od bohatera (bez miejsca – nie ucieka).</summary>
    public bool FleeStep(int i)
    {
        ref var e = ref Enemies[i];
        int bd = Cheb(e.X, e.Y, Hero.X, Hero.Y), bx = -1, by = -1;
        for (var k = 0; k < 4; ++k)
        {
            int nx = e.X + Around[k, 0], ny = e.Y + Around[k, 1];
            if (Lv.At(nx, ny) != Tile.Floor || Occupied(nx, ny)) continue;
            var d = Cheb(nx, ny, Hero.X, Hero.Y);
            if (d > bd)
            {
                bd = d;
                bx = nx;
                by = ny;
            }
        }
        if (bx < 0) return false;
        e.X = (sbyte)bx;
        e.Y = (sbyte)by;
        if (Visible(bx, by)) Push(Msg(D.Enemies[e.DefId].Name).Add(" ucieka"));
        return true;
    }

    /// <summary>Łatanie: najbardziej ranny problem w zasięgu 2 (bez bossa) dostaje HP.</summary>
    public bool HealNear(int i)
    {
        var e = Enemies[i];
        int best = -1, lack = 0;
        for (var j = 0; j < EnemiesCount; ++j)
        {
            var o = Enemies[j];
            if (j == i || j == Boss || !o.Alive || Cheb(e.X, e.Y, o.X, o.Y) > 2 || o.MaxHp - o.Hp <= lack) continue;
            best = j;
            lack = o.MaxHp - o.Hp;
        }
        if (best < 0) return false;
        ref var t = ref Enemies[best];
        var h = Math.Min(D.BehaviorHealValue, lack);
        t.Hp = (short)(t.Hp + h);
        if (Visible(e.X, e.Y) || Visible(t.X, t.Y))
            Push(Msg(D.Enemies[e.DefId].Name).Add(" łata: ").Add(D.Enemies[t.DefId].Name).Add(" +").Add(h).As(LogKind.Bad));
        return true;
    }

    /// <summary>Wzrost: co kilka tur (w walce) +HP, co drugi stopień +1 obrażeń.</summary>
    public void GrowTick(int i)
    {
        ref var e = ref Enemies[i];
        if (e.Grow >= D.BehaviorGrowMax || Turns % D.BehaviorGrowEvery != 0) return;
        ++e.Grow;
        e.MaxHp = (short)(e.MaxHp + D.BehaviorGrowHp);
        e.Hp = (short)(e.Hp + D.BehaviorGrowHp);
        if (Visible(e.X, e.Y)) Push(Msg(D.Enemies[e.DefId].Name).Add(" rośnie!").As(LogKind.Bad));
    }

    /// <summary>Wybuch po usunięciu: czerwone pola wokół, spada po BehaviorBlastDelay turach (tura na zejście).</summary>
    public void ArmBlast(int x, int y, EnemyDef ed)
    {
        BlastX = (sbyte)x;
        BlastY = (sbyte)y;
        BlastTimer = (sbyte)D.BehaviorBlastDelay;
        BlastDmg = (sbyte)(D.BehaviorBlastDamage + EnemyDmgBonus());
        BlastSrc = (sbyte)Array.IndexOf(D.Enemies, ed); // podsumowanie: źródło wybuchu
        Push(Msg(ed.Name).Add(": wybuch za ").Add(D.BehaviorBlastDelay - 1).Add(" t.! Odejdź").As(LogKind.Bad));
    }

    /// <summary>Miejsce na nowy problem (podział): wolny slot na końcu albo po usuniętym (nie boss, nie wezwani, nie czekający).</summary>
    public int FreeSlot()
    {
        if (EnemiesCount < MaxEnemies) return EnemiesCount++;
        var reserve = Boss >= 0 ? D.Enemies[Enemies[Boss].DefId].SummonMax : 0;
        for (var j = 0; j < EnemiesCount; ++j)
        {
            if (!Enemies[j].Alive && (Enemies[j].Flags & ActorFlag.Reviving) == 0 && j != Boss && !(Boss >= 0 && j > Boss && j <= Boss + reserve))
                return j;
        }
        return -1;
    }

    /// <summary>Podział: dwa słabsze problemy (połowa max HP) na polu usuniętego i obok; same się już nie dzielą.</summary>
    public void Split(int ei)
    {
        var p = Enemies[ei];
        var made = 0;
        for (var k = 0; k < 2; ++k)
        {
            int x = p.X, y = p.Y;
            if (k == 1 || Occupied(x, y))
            {
                if (!FreeAround(p.X, p.Y, Hero.X, Hero.Y, out x, out y)) break;
            }
            var slot = FreeSlot();
            if (slot < 0) break;
            ref var c = ref Enemies[slot];
            c = new Actor
            {
                X = (sbyte)x,
                Y = (sbyte)y,
                DefId = p.DefId,
            };
            c.Hp = c.MaxHp = (short)Math.Max(1, p.MaxHp * D.BehaviorSplitHpPct / 100);
            c.Alive = true;
            c.Awake = true;
            c.Stun = 1;
            c.Flags = ActorFlag.Child;
            ++made;
        }
        if (made > 0) Push(Msg(D.Enemies[p.DefId].Name).Add(" dzieli się!").As(LogKind.Bad));
    }
}
