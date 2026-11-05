using LifeLike.Core.Data;

namespace LifeLike.Core;

// v0.23.0: tryb B broni z odnowieniem, efekty trybów (ogłuszenie, unieruchomienie, odrzut, osłabienie, krwawienie / podpalenie,
// znak, odsłonięcie, otwarcie ściany, zamiana, łańcuch, osuszenie, kryt) i stany problemów.
public sealed partial class Game
{
    /// <summary>Tury do ponownego użycia trybu B (maleje na koniec tury bohatera; zostaje po zmianie broni).</summary>
    public int WeaponBCd;

    /// <summary>Ładunki broni jednorazowej (Przecinak); 0 = jeszcze nie zaczęta. Po zużyciu broń znika.</summary>
    public sbyte WeaponCharges;

    /// <summary>Pomiar (Miernik, Poziomica): premia do krytu bohatera i jej tury.</summary>
    public sbyte CritBuff, CritBuffTurns;

    /// <summary>Zmiana odnowienia trybu B od rzadkości broni (WeaponRarityDef.CooldownDelta); rzadkość broni w ręku dojdzie w zadaniu 5.</summary>
    private int WeaponCdDelta() => 0;

    /// <summary>
    /// Tryb B: wymaga braku odnowienia i ważnego celu wzoru B (żywy, widoczny, w zasięgu, na polu wzoru) – inaczej false bez kosztu.
    /// Atakuje wzorem B z efektami, ustawia odnowienie (ModeB.Cooldown + rzadkość, min 1). Zwraca true = tura zużyta.
    /// </summary>
    public bool PlayerWeaponB(int ei)
    {
        if (St != GameStatus.Playing || WeaponBCd > 0 || ei < 0 || ei >= EnemiesCount) return false;
        var m = Weapon.ModeB;
        var e = Enemies[ei];
        if (!e.Alive || !Visible(e.X, e.Y) || Cheb(Hero.X, Hero.Y, e.X, e.Y) > ModeRange(m) || !InPattern(m, ei)) return false;
        if (ShockedTurn()) return true;
        AttackWith(m, ei);
        SpendCharge(m);
        EndTurn();
        WeaponBCd = Math.Max(1, m.Cooldown + WeaponCdDelta()); // po EndTurn, by pierwsza tura nie zjadła odnowienia
        return true;
    }

    /// <summary>Broń z ładunkami (OpenWall z Value &gt; 0, np. Przecinak): każde użycie zużywa jeden; po ostatnim wraca broń zawodu.</summary>
    private void SpendCharge(WeaponMode m)
    {
        if (m.Effect != ModeEffect.OpenWall || m.Value <= 0 || WeaponOverride < 0) return;
        if (WeaponCharges <= 0) WeaponCharges = (sbyte)m.Value;
        if (--WeaponCharges > 0) return;
        WeaponCharges = 0;
        WeaponOverride = -1;
        ResetUpgrade();
    }

    /// <summary>Efekty przed ciosem (ten sam cios je już czuje): ogłuszenie, unieruchomienie, osłabienie, znak, kryt, krwawienie, osuszenie, mokry.</summary>
    private void ModeEffectBefore(ModeEffect eff, int val, int turns, int t)
    {
        ref var e = ref Enemies[t];
        if (!e.Alive) return;
        switch (eff)
        {
            case ModeEffect.Stun:
                if (t != Boss && (val <= 0 || val >= 100 || R.Range(1, 100) <= val))
                {
                    e.Stun = (sbyte)Math.Max((int)e.Stun, Math.Max(1, turns));
                    e.Awake = true;
                }
                break;
            case ModeEffect.Root:
                if (t != Boss) e.Root = (sbyte)Math.Max((int)e.Root, Math.Max(1, turns));
                break;
            case ModeEffect.DefDown:
                e.DefDown = (sbyte)Math.Max((int)e.DefDown, Math.Max(1, turns));
                e.DefDownVal = (sbyte)Math.Clamp(val, 0, 100);
                break;
            case ModeEffect.Mark:
                e.MarkTurns = (sbyte)Math.Max(1, turns);
                e.MarkPct = (byte)Math.Clamp(val, 0, 255);
                break;
            case ModeEffect.CritNext:
                e.Flags = (byte)(e.Flags | ActorFlag.CritNext);
                break;
            case ModeEffect.Dot:
                e.Dot = (sbyte)Math.Max(1, turns);
                e.DotDmg = (sbyte)(val > 0 ? val : D.HitTypes.DotPerTurn);
                e.DotKind = (byte)(HitCalc.Has(Weapon, HitType.Cut) && Weapon.Elem != Element.Fire ? 2 : 1); // cięcie krwawi, reszta podpala
                e.Awake = true;
                break;
            case ModeEffect.Dry:
                e.Wet = 0;
                if (D.Enemies[e.DefId].Elem == Element.Water && t != Boss) e.Stun = (sbyte)Math.Max((int)e.Stun, Math.Max(1, turns)); // zawór: problem wodny traci turę
                break;
            case ModeEffect.Wet:
                e.Wet = (sbyte)Math.Max((int)e.Wet, turns > 0 ? turns : D.WetTurns);
                break;
        }
    }

    /// <summary>Efekty po ciosie na przeżywającym celu: odrzut, przyciągnięcie, zamiana miejsc.</summary>
    private void ModeEffectAfter(ModeEffect eff, int val, int t, bool knockbackDone)
    {
        ref var e = ref Enemies[t];
        if (!e.Alive || t == Boss) return;
        int dx = Math.Sign(e.X - Hero.X), dy = Math.Sign(e.Y - Hero.Y);
        switch (eff)
        {
            case ModeEffect.Push:
                if (!knockbackDone) Shove(t, dx, dy, val);
                break;
            case ModeEffect.Pull:
                Shove(t, -dx, -dy, Math.Min(val, Cheb(Hero.X, Hero.Y, e.X, e.Y) - 1));
                break;
            case ModeEffect.Swap:
                (Hero.X, e.X) = (e.X, Hero.X);
                (Hero.Y, e.Y) = (e.Y, Hero.Y);
                Collect();
                UpdateFov();
                break;
        }
    }

    /// <summary>Efekty całego ataku (raz, nie na cel): odsłonięcie pokoju + premia kryt, otwarcie ściany, łańcuch na mokrych, wybuch pyłu.</summary>
    private void ModeEffectOnce(ModeEffect eff, int val, int turns, WeaponMode m, ReadOnlySpan<int> hit, ReadOnlySpan<int> px, ReadOnlySpan<int> py,
        ReadOnlySpan<bool> wasWet, ReadOnlySpan<bool> wasDusty)
    {
        switch (eff)
        {
            case ModeEffect.Reveal:
                RevealRoom();
                if (val > 0 && turns > 0)
                {
                    CritBuff = (sbyte)val;
                    CritBuffTurns = (sbyte)turns;
                }
                break;
            case ModeEffect.OpenWall:
                // Value > 0 (Przecinak): każda zamknięta ściana magazynu; inaczej tylko pęknięta
                if (SecretClosed() && Cheb(Hero.X, Hero.Y, SecretX, SecretY) <= Math.Max(1, ModeRange(m)) && (val > 0 || SecretDef.Breakable))
                {
                    OpenSecret(Weapon.Name);
                }
                break;
            case ModeEffect.Chain:
                ChainShock(hit, px, py, wasWet);
                break;
            case ModeEffect.Ignite:
                for (var i = 0; i < hit.Length && St == GameStatus.Playing; ++i)
                {
                    if (wasDusty[i] && (Enemies[hit[i]].Flags & ActorFlag.Dusty) != 0) ComboDust(px[i], py[i]);
                }
                break;
        }
    }

    /// <summary>Łańcuch (Próbnik): prąd przeskakuje z trafionego, niemokrego celu na mokre problemy obok (mokre cele rażą się combo ciosu).</summary>
    private void ChainShock(ReadOnlySpan<int> hit, ReadOnlySpan<int> px, ReadOnlySpan<int> py, ReadOnlySpan<bool> wasWet)
    {
        var c = D.Combos[(int)ComboEffect.ShockArea];
        var announced = false;
        for (var i = 0; i < hit.Length && St == GameStatus.Playing; ++i)
        {
            if (wasWet[i]) continue;
            for (var j = 0; j < EnemiesCount && St == GameStatus.Playing; ++j)
            {
                if (!Enemies[j].Alive || !EnemyWet(j) || Cheb(px[i], py[i], Enemies[j].X, Enemies[j].Y) > 1 || hit.Contains(j)) continue;
                if (!announced)
                {
                    announced = true;
                    ComboEvents = (byte)(ComboEvents | (1 << (int)ComboEffect.ShockArea));
                    NoteCombo();
                    Push(Msg(c.Short).Add(" ").Add(c.Name).As(LogKind.Good));
                }
                DamageEnemy(j, c.Value, false, c.Name);
            }
        }
    }

    /// <summary>Odsłania pokój bohatera z murami oraz wnętrze zamkniętego magazynu (Miernik, Poziomica).</summary>
    private void RevealRoom()
    {
        for (var r = 0; r < Lv.RoomsCount; ++r)
        {
            var rm = Lv.Rooms[r];
            if (Hero.X < rm.X || Hero.X >= rm.X + rm.W || Hero.Y < rm.Y || Hero.Y >= rm.Y + rm.H) continue;
            RevealRect(rm.X - 1, rm.Y - 1, rm.W + 2, rm.H + 2);
        }
        if (SecretClosed()) RevealRect(SecretRx - 1, SecretRy - 1, SecretRw + 2, SecretRh + 2);
        UpdateFov();
    }

    private void RevealRect(int x0, int y0, int w, int h)
    {
        for (var y = y0; y < y0 + h; ++y)
        {
            for (var x = x0; x < x0 + w; ++x)
            {
                if (Level.In(x, y) && Fov[y * Level.W + x] == Sight.Unknown) Fov[y * Level.W + x] = Sight.Remembered;
            }
        }
    }

    /// <summary>
    /// Początek tury problemu: krwawienie / podpalenie zadaje DotDmg (podpalenie osusza), osłabienie wygasa. False = problem padł
    /// (albo budowa się skończyła) i nie działa w tej turze.
    /// </summary>
    private bool EnemyStatusTick(int i)
    {
        ref var e = ref Enemies[i];
        if (e.DefDown > 0 && --e.DefDown == 0) e.DefDownVal = 0;
        if (e.Dot <= 0) return true;
        --e.Dot;
        var burn = e.DotKind != 2;
        var dmg = Math.Max(1, (int)e.DotDmg);
        if (e.Dot == 0) e.DotDmg = 0;
        if (burn) e.Wet = 0;
        DamageEnemy(i, dmg, false, D.Statuses[(int)(burn ? StatusEffect.Burn : StatusEffect.Bleed)].Name);
        return Enemies[i].Alive && St == GameStatus.Playing;
    }
}
