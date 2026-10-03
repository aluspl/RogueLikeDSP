using LifeLike.Core.Data;

namespace LifeLike.Core;

// v0.21.51 cz. 2: sekretne zlecenia (#39) – liczniki budowy sprawdzane w Meta.CheckSecrets, nowe zawody (Spawacz, Geodeta,
// Majster) – port 1:1 z core.h.
public sealed partial class Game
{
    /// <summary>Bity SecretFlags: boss pokonany ciosem brygady, Akt 0 bez obrażeń od papierów.</summary>
    public const byte SecretHelperBossFlag = 1, SecretPaperCleanFlag = 2;

    /// <summary>Kawy wypite w budowie (termos, pełny termos, Hurtownia).</summary>
    public byte CoffeeDrunk;
    /// <summary>Mokry + prąd wywołane przez bohatera.</summary>
    public byte ShockCombos;
    /// <summary>Ciosy problemów papierowych (GameData.SecretPaperMask) w Akcie 0.</summary>
    public byte PaperHits;
    public byte SecretFlags;
    /// <summary>Cios brygady (pompa, pomocnik) – tylko w trakcie ciosu.</summary>
    public byte HelperCtx;
    /// <summary>Majster (Złota rączka): zawód, którego moc ma na tym etapie (-1 = własna).</summary>
    public sbyte BorrowCls = -1;
    /// <summary>Geodeta (Tyczenie): oznaczony problem i tury znaku.</summary>
    public sbyte MarkTarget = -1, MarkTurns;

    /// <summary>Zawód, którego moc działa (Majster: pożyczony na ten etap) – moc, ikona, odnowienie, premia do ciosu.</summary>
    public int PowerCls() => CDef.Ability == AbilityEffect.Borrow && BorrowCls >= 0 ? BorrowCls : Cls;

    public ClassDef PDef => D.Classes[PowerCls()];

    /// <summary>Geodeta (Tyczenie): cios w oznaczony problem +1 + ranga mocy (+ premia zawodu).</summary>
    public int MarkBonus(int ei) => ei == MarkTarget && MarkTurns > 0 ? 1 + AbilityRank() + BoonPower() : 0;

    /// <summary>Dni budowy jak w harmonogramie domu, bez Aktu 0 (sekretne zlecenie Szybka ekipa).</summary>
    public int BuildDays()
    {
        var t = 0;
        for (var s = Math.Max(FirstStage, PreludeCount()); s < RouteCount(); ++s) t += D.ScheduleMinDays + StageDays[s] / D.ScheduleTurnsPerDay;
        return t;
    }

    /// <summary>Mapa etapu odkryta (Geodeta z brygady, zawód Geodeta na starcie etapu): podłoga, schody i mury przy nich.</summary>
    public void RevealMap()
    {
        for (var y = 0; y < Level.H; ++y)
        {
            for (var x = 0; x < Level.W; ++x)
            {
                if (Fov[y * Level.W + x] != Sight.Unknown) continue;
                var near = false;
                for (var dy = -1; dy <= 1 && !near; ++dy)
                {
                    for (var dx = -1; dx <= 1; ++dx)
                    {
                        if (!Lv.Passable(x + dx, y + dy)) continue;
                        near = true;
                        break;
                    }
                }
                if (near) Fov[y * Level.W + x] = Sight.Remembered;
            }
        }
    }

    /// <summary>Majster: moc innego fachu na ten etap (osobny generator, nigdy własna).</summary>
    private void RollBorrow()
    {
        if (CDef.Ability != AbilityEffect.Borrow) return;
        var br = SideRng(7);
        var open = Cls < D.OpenClassesCount;
        var k = br.Range(0, D.OpenClassesCount - (open ? 2 : 1));
        if (open && k >= Cls) ++k;
        BorrowCls = (sbyte)k;
    }

    /// <summary>Cios w bohatera od problemu papierowego w Akcie 0 (sekretne zlecenie Papierologia? Nie tym razem).</summary>
    private void NotePaperHit(int src)
    {
        if (Stage < PreludeCount() && src >= 0 && ((D.SecretPaperMask >> src) & 1) != 0 && PaperHits < 255) ++PaperHits;
    }

    // Moce nowych zawodów (PlayerAbility).
    private bool AbilityWeld(ClassDef c, int rank)
    {
        var t = NearestVisibleEnemy();
        if (t < 0) return false;
        int dx = Enemies[t].X - Hero.X, dy = Enemies[t].Y - Hero.Y, len = Math.Max(Math.Abs(dx), Math.Abs(dy));
        uint done = 0;
        var ok = false;
        HitCtx = 2; // iskra
        for (var k = 1; k <= 2 + rank + BoonPower() && St == GameStatus.Playing; ++k)
        {
            int x = Hero.X + Pct.DivRound(dx * k, len), y = Hero.Y + Pct.DivRound(dy * k, len);
            if (!Lv.Passable(x, y)) break; // mur zatrzymuje iskry
            var ei = EnemyAt(x, y);
            if (ei < 0 || (done & (1u << ei)) != 0) continue;
            HeroAttack(ei);
            done |= 1u << ei;
            ok = true;
            if (Enemies[ei].Alive) Enemies[ei].Flags = (byte)(Enemies[ei].Flags | ActorFlag.Dusty); // dym spawalniczy
        }
        HitCtx = 0;
        if (ok) Push(Msg(c.AbilityName).Add(": iskry i dym!"));
        return ok;
    }

    private bool AbilityMark(ClassDef c)
    {
        var t = NearestVisibleEnemy();
        if (t < 0) return false;
        MarkTarget = (sbyte)t;
        MarkTurns = (sbyte)D.MarkTurns;
        Enemies[t].Stun = (sbyte)Math.Max((int)Enemies[t].Stun, 1);
        Enemies[t].Awake = true;
        Push(Msg(c.AbilityName).Add(": ").Add(D.Enemies[Enemies[t].DefId].Name).Add(" +").Add(MarkBonus(t)));
        return true;
    }
}
