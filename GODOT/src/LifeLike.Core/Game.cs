using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// Rdzeń gry – port 1:1 struktury core::game z GBA/include/core.h (ta sama kolejność wywołań RNG,
/// ta sama arytmetyka całkowita i rzutowania int8/int16). Ten sam seed daje identyczną grę co na GBA.
/// Stan: mapa, bohater, problemy budowy, znajdźki, dziennik, mgła wojny, akty, stany, sprzęt.
/// </summary>
public sealed partial class Game
{
    public const int MaxEnemies = 12;
    public const int MaxPickups = 10;
    public const int LogLines = 3;
    public const int FovRadius = 7;
    public const int MaxHits = 8;
    public const int MaxWalls = 5;

    public GameData D { get; }

    public readonly Level Lv = new();
    public Rng R = new();
    public Actor Hero = new();
    public readonly Actor[] Enemies = NewActors();
    public int EnemiesCount;
    public readonly Pickup[] Pickups = new Pickup[MaxPickups];
    public int PickupsCount;
    public int Cls;
    /// <summary>0..Stages.Length-1.</summary>
    public int Stage;
    public int Diff;
    /// <summary>NG+: ile razy budowa została już ukończona.</summary>
    public int Tier;
    public int DefBonus, DmgBonus;
    public int Turns, Kills, Score;
    /// <summary>Pokonane problemy wg rodzaju (zakładka Usterki).</summary>
    public readonly byte[] KillsByType = new byte[16];
    public int StageDamage;
    public int StageKills;
    public int StageStartTurn;
    public byte ToolsFound;
    // liczniki zleceń (przenoszone do profilu przez Meta.BankCounters; *Banked = już przeniesione)
    public int KillsBanked;
    /// <summary>Użycia mocy.</summary>
    public ushort PowersUsed, PowersBanked;
    /// <summary>Założone markowe przedmioty.</summary>
    public byte BrandFound, BrandBanked;
    /// <summary>Bossowie aktu bez obrażeń w walce z nimi.</summary>
    public byte CleanBosses, CleanBanked;
    /// <summary>StageDamage w chwili dołączenia bossa do walki (-1 = jeszcze nie).</summary>
    public int BossWakeDamage = -1;
    /// <summary>Wydarzenie na placu na bieżącym etapie (GameData.SiteEvents, -1 = brak).</summary>
    public sbyte StageEvent = -1;
    /// <summary>Budżet budowy (zł) – za usunięte problemy i premie aktów, wydawany w Hurtowni.</summary>
    public int Cash;
    public int ActKills;
    public int ActBonus;
    /// <summary>Pokonano bossa aktu – przed kolejnym etapem jest Hurtownia.</summary>
    public bool ActCleared;
    /// <summary>Uderzenie bossa: tury do ciosu (0 = brak zapowiedzi).</summary>
    public int SlamTimer;
    public sbyte SlamX = -1, SlamY = -1;
    public int SlamCounter;
    /// <summary>Tury aktywnych stanów bohatera (indeks = StatusEffect).</summary>
    public readonly sbyte[] HeroStatus = new sbyte[5];
    public RunMods Bonus;
    /// <summary>Doświadczenie x100 (mnożnik trudności bez gubienia ułamków).</summary>
    public int XpPct;
    public int XpBanked;
    public int RunXp;
    public int HeroLevel = 1;
    public int Boss = -1;
    public int StairsX = -1, StairsY = -1;
    public GameStatus St = GameStatus.Playing;
    public readonly Message[] Log = [new(), new(), new()];
    /// <summary>Mgła wojny, indeks y * Level.W + x.</summary>
    public readonly Sight[] Fov = new Sight[Level.W * Level.H];
    public uint TurnEvents;
    public bool HeroHit;
    public readonly Hit[] Hits = new Hit[MaxHits];
    public int HitsCount;
    public int LastTarget = -1;
    public int AbilityCd;
    public readonly TempWall[] Walls = new TempWall[MaxWalls];
    public int WallsCount;
    /// <summary>Sprzęt: jakość w slocie (kask, rękawice, kamizelka), -1 = brak.</summary>
    public readonly sbyte[] Equipped = [-1, -1, -1, -1];
    /// <summary>Cecha przedmiotu w slocie (GameData.GearTraits).</summary>
    public readonly sbyte[] EquippedTrait = new sbyte[4];
    /// <summary>Kawy w termosie (pije się z menu akcji).</summary>
    public int Thermos;
    /// <summary>Paczka czeka na decyzję (zakładam / zostawiam): slot (-1 = brak), jakość, cecha.</summary>
    public sbyte OfferSlot = -1, OfferRarity, OfferTrait;
    public int WeaponOverride = -1;
    public int LogSerial;

    public Game(GameData data)
    {
        D = data;
        Diff = data.DefaultDifficulty;
        Bonus = RunMods.Default(data);
    }

    private static Actor[] NewActors()
    {
        var a = new Actor[MaxEnemies];
        for (var i = 0; i < a.Length; i++) a[i] = new Actor();
        return a;
    }

    // ------------------------------------------------------------------ pomocnicze
    public static int Cheb(int ax, int ay, int bx, int by) => Math.Max(Math.Abs(ax - bx), Math.Abs(ay - by));

    public ClassDef CDef => D.Classes[Cls];
    public DifficultyDef DDef => D.Difficulties[Diff];
    public WeaponDef Weapon => D.Weapons[WeaponOverride >= 0 ? WeaponOverride : CDef.Weapon];

    public int StatusTurns(StatusEffect s) => HeroStatus[(int)s];

    /// <summary>Wiadomość fabularna na wejściu etapu (przy NG+ pierwszy etap ma własną).</summary>
    public StoryMsg StageStory => Tier > 0 && Stage == 0 ? D.StoryNgPlus : D.StoryStages[Stage];

    public bool Visible(int x, int y) => Level.In(x, y) && Fov[y * Level.W + x] == Sight.InView;
    public bool Explored(int x, int y) => Level.In(x, y) && Fov[y * Level.W + x] != Sight.Unknown;

    /// <summary>Pole w zasięgu zapowiedzianego uderzenia bossa (czerwone pola na mapie).</summary>
    public bool SlamCell(int x, int y) => SlamTimer > 0 && SlamCellAt(x, y);
    public bool SlamCellAt(int x, int y) => SlamX >= 0 && Cheb(x, y, SlamX, SlamY) <= D.SlamRadius;

    public int GearBonus(GearStat s)
    {
        var b = 0;
        for (var i = 0; i < D.GearSlotsCount; ++i)
        {
            if (Equipped[i] >= 0 && D.Gear[i * 3 + Equipped[i]].Stat == s) b += D.Gear[i * 3 + Equipped[i]].Value;
        }
        return b;
    }

    /// <summary>Szczęście: kryt (x2), mały unik przed ciosem wroga, częstsze i lepsze dropy.</summary>
    public int Luck() => CDef.Luck + Bonus.Luck + TraitBonus(TraitEffect.Luck);

    public int CritPct() => D.CritBasePct + D.CritPerLuckPct * Luck() + TraitBonus(TraitEffect.Crit) + Bonus.Crit;

    public int SightRadius() => FovRadius + TraitBonus(TraitEffect.Sight) + Bonus.Sight;

    /// <summary>Pojemność termosu (+ uprawnienia i pamiątka).</summary>
    public int ThermosCap() => D.ThermosCapacity + Bonus.Thermos;

    // ------------------------------------------------------------------ wydarzenia na placu
    public bool EventActive(EventEffect e) => StageEvent >= 0 && D.SiteEvents[StageEvent].Effect == e;

    /// <summary>Bieżące wydarzenie na placu albo null.</summary>
    public SiteEventDef CurrentEvent => StageEvent >= 0 ? D.SiteEvents[StageEvent] : null;

    /// <summary>Wydarzenie na placu: SMS na starcie etapu, efekt od razu (znajdźki, budżet, termos) albo w trakcie etapu.</summary>
    public void ApplyEvent(int e)
    {
        StageEvent = (sbyte)e;
        var ev = D.SiteEvents[e];
        switch (ev.Effect)
        {
            case EventEffect.FewerPickups: PickupsCount = Math.Max(Math.Min(1, PickupsCount), PickupsCount - ev.Value); break;
            case EventEffect.Cash: Cash += ev.Value; break;
            case EventEffect.Thermos: Thermos = ThermosCap(); break;
            // inspekcja: premia na koniec etapu; ulewa: poślizg przy ciosach
        }
        Push(Msg("SMS: ").Add(ev.Name).As(ev.Good ? LogKind.Good : LogKind.Bad));
    }

    /// <summary>Suma cech założonego sprzętu danego rodzaju.</summary>
    public int TraitBonus(TraitEffect e)
    {
        var b = 0;
        for (var i = 0; i < D.GearSlotsCount; ++i)
        {
            if (Equipped[i] >= 0 && D.GearTraits[EquippedTrait[i]].Effect == e) b += D.GearTraits[EquippedTrait[i]].Value;
        }
        return b;
    }

    public int DodgePct() => Math.Min(D.DodgeMaxPct, D.DodgePerLuckPct * Luck());

    public void AddHit(int x, int y, int amount, bool onHero, HitKind kind = HitKind.Normal)
    {
        if (HitsCount < MaxHits) Hits[HitsCount++] = new Hit { X = (sbyte)x, Y = (sbyte)y, Amount = (short)amount, OnHero = onHero, Kind = kind };
    }

    public void Push(Message m)
    {
        ++LogSerial;
        var last = Log[LogLines - 1];
        if (last.Kind == m.Kind && last.SameText(m))
        {
            if (last.Repeat < 99) ++last.Repeat; // ten sam komunikat: licznik zamiast nowej linii
            return;
        }
        for (var i = 0; i < LogLines - 1; ++i) Log[i] = Log[i + 1];
        Log[LogLines - 1] = m;
    }

    private static Message Msg(string s) => new Message().Add(s);

    // ------------------------------------------------------------------ stany
    /// <summary>Nakłada stan; komunikat mówi skutek i czas, np. „Zatrucie: -1 HP/turę, 3 t.”.</summary>
    public void ApplyStatus(StatusEffect s, int t)
    {
        if (s == StatusEffect.None) return;
        var sd = D.Statuses[(int)s];
        if (s == StatusEffect.Paper)
        {
            AbilityCd = Math.Min(AbilityCooldown() + D.PaperDelay, AbilityCd + D.PaperDelay);
            Push(Msg(sd.Name).Add(": moc +").Add(D.PaperDelay).Add(" t.").As(LogKind.Bad));
            return;
        }
        if (s == StatusEffect.Poison && TraitBonus(TraitEffect.PoisonRes) > 0)
        {
            Push(Msg("Odporność: bez zatrucia").As(LogKind.Good));
            return;
        }
        HeroStatus[(int)s] = (sbyte)Math.Max(HeroStatus[(int)s], t);
        Push(Msg(sd.Name).Add(": ").Add(sd.Effect).Add(", ").Add(HeroStatus[(int)s]).Add(" t.").As(LogKind.Bad));
    }

    /// <summary>Porażenie: akcja bohatera przepada, mija tura.</summary>
    public bool ShockedTurn()
    {
        if (HeroStatus[(int)StatusEffect.Shock] <= 0) return false;
        --HeroStatus[(int)StatusEffect.Shock];
        Push(Msg("Porażenie: tura stracona").As(LogKind.Bad));
        EndTurn();
        return true;
    }

    // ------------------------------------------------------------------ pole widzenia
    private static readonly int[,] FovMult =
    {
        { 1, 0, 0, -1, -1, 0, 0, 1 }, { 0, 1, -1, 0, 0, -1, 1, 0 },
        { 0, 1, 1, 0, 0, -1, -1, 0 }, { 1, 0, 0, 1, -1, 0, 0, -1 },
    };

    /// <summary>Recursive shadowcasting (8 oktantów), ściany zasłaniają, same są widoczne.</summary>
    public void UpdateFov()
    {
        for (var i = 0; i < Fov.Length; i++)
        {
            if (Fov[i] == Sight.InView) Fov[i] = Sight.Remembered;
        }
        Fov[Hero.Y * Level.W + Hero.X] = Sight.InView;
        for (var o = 0; o < 8; ++o) CastLight(1, 1.0f, 0.0f, FovMult[0, o], FovMult[1, o], FovMult[2, o], FovMult[3, o]);
    }

    private void CastLight(int row, float start, float end, int xx, int xy, int yx, int yy)
    {
        if (start < end) return;
        float newStart = 0;
        var radius = SightRadius();
        for (var j = row; j <= radius; ++j)
        {
            var blocked = false;
            for (int dx = -j, dy = -j; dx <= 0; ++dx)
            {
                int x = Hero.X + dx * xx + dy * xy, y = Hero.Y + dx * yx + dy * yy;
                float lSlope = (dx - 0.5f) / (dy + 0.5f), rSlope = (dx + 0.5f) / (dy - 0.5f);
                if (start < rSlope) continue;
                if (end > lSlope) break;
                if (dx * dx + dy * dy <= radius * radius && Level.In(x, y)) Fov[y * Level.W + x] = Sight.InView;
                var opaque = !Lv.Passable(x, y);
                if (blocked)
                {
                    if (opaque)
                    {
                        newStart = rSlope;
                        continue;
                    }
                    blocked = false;
                    start = newStart;
                }
                else if (opaque && j < radius)
                {
                    blocked = true;
                    CastLight(j + 1, start, lSlope, xx, xy, yx, yy);
                    newStart = rSlope;
                }
            }
            if (blocked) break;
        }
    }

    // ------------------------------------------------------------------ trudność, doświadczenie, poziomy
    /// <summary>Trudność = etap x poziom x NG+. Mnożniki w procentach, premie sumowane.</summary>
    public int EnemyHpPct() => D.Stages[Stage].HpPct * DDef.HpPct / 100 * (100 + Tier * D.NgHpPctPerTier) / 100;

    public int EnemyDmgBonus() => D.Stages[Stage].DmgBonus + DDef.DmgBonus + Tier * D.NgDmgBonusPerTier;

    public int ScorePct() => DDef.ScorePct * (100 + Tier * D.NgScorePctPerTier) / 100;

    public int Xp => XpPct / 100;

    public void GainXp(int b)
    {
        XpPct += b * ScorePct() * (100 + Bonus.XpPct) / 100;
        RunXp += b;
        while (HeroLevel < D.MaxHeroLevel && RunXp >= D.LevelThresholds[HeroLevel - 1]) LevelUp();
    }

    /// <summary>Ile brakuje do kolejnego poziomu; -1 = maksymalny.</summary>
    public int XpToNext() => HeroLevel < D.MaxHeroLevel ? D.LevelThresholds[HeroLevel - 1] - RunXp : -1;

    /// <summary>Awans: +HP; wybrane poziomy dają +1 obrażenia / +1 obrona.</summary>
    public void LevelUp()
    {
        ++HeroLevel;
        Hero.MaxHp = (short)(Hero.MaxHp + D.HpPerLevel);
        Hero.Hp = (short)(Hero.Hp + D.HpPerLevel);
        if ((D.DmgLevelsMask & (1 << HeroLevel)) != 0) ++DmgBonus;
        if ((D.DefLevelsMask & (1 << HeroLevel)) != 0) ++DefBonus;
        Push(Msg("Awans! Poziom ").Add(HeroLevel).As(LogKind.Good));
    }

    // ------------------------------------------------------------------ budowa i etapy
    public void NewRun(int classIndex, uint seed) => NewRun(classIndex, seed, D.DefaultDifficulty, RunMods.Default(D));

    public void NewRun(int classIndex, uint seed, int difficulty) => NewRun(classIndex, seed, difficulty, RunMods.Default(D));

    public void NewRun(int classIndex, uint seed, int difficulty, RunMods mods)
    {
        CopyFrom(new Game(D));
        Cls = classIndex;
        Diff = difficulty;
        Bonus = mods;
        DefBonus = mods.Def;
        DmgBonus = mods.Dmg;
        R.Seed(seed);
        Hero.MaxHp = Hero.Hp = (short)(CDef.MaxHealth + mods.Hp);
        Hero.Alive = true;
        Cash = mods.Cash;
        StartStage(0);
    }

    public bool Occupied(int x, int y)
    {
        if (Hero.Alive && Hero.X == x && Hero.Y == y) return true;
        for (var i = 0; i < EnemiesCount; ++i)
        {
            if (Enemies[i].Alive && Enemies[i].X == x && Enemies[i].Y == y) return true;
        }
        return false;
    }

    public void RandomFreeCellInRoom(in Room rm, out int ox, out int oy)
    {
        for (var k = 0; k < 40; ++k)
        {
            int x = R.Range(rm.X, rm.X + rm.W - 1), y = R.Range(rm.Y, rm.Y + rm.H - 1);
            if (Lv.At(x, y) == Tile.Floor && !Occupied(x, y))
            {
                ox = x;
                oy = y;
                return;
            }
        }
        ox = rm.Cx;
        oy = rm.Cy;
    }

    public void StartStage(int s)
    {
        Stage = s;
        St = GameStatus.Playing;
        Lv.Generate(ref R);
        WallsCount = 0;
        StageDamage = 0;
        StageKills = 0;
        StageStartTurn = Turns;
        BossWakeDamage = -1;
        ActCleared = false;
        SlamTimer = 0;
        SlamX = SlamY = -1;
        SlamCounter = 0;
        Array.Fill(Fov, Sight.Unknown);
        var sd = D.Stages[Stage];
        var first = Lv.Rooms[0];
        var last = Lv.Rooms[Lv.RoomsCount - 1];
        Hero.X = (sbyte)first.Cx;
        Hero.Y = (sbyte)first.Cy;
        Boss = -1;
        StairsX = StairsY = -1;
        if (sd.Boss < 0)
        {
            StairsX = last.Cx;
            StairsY = last.Cy;
            Lv[StairsX, StairsY] = Tile.Stairs;
        }

        EnemiesCount = 0;
        for (var i = 0; i < sd.EnemyCount && EnemiesCount < MaxEnemies; ++i)
        {
            var roomI = 1 + R.Range(0, Lv.RoomsCount - 2 > 0 ? Lv.RoomsCount - 2 : 0);
            if (roomI >= Lv.RoomsCount) roomI = Lv.RoomsCount - 1;
            RandomFreeCellInRoom(Lv.Rooms[roomI], out var x, out var y);
            Spawn(sd.Pool[R.Range(0, sd.Pool.Length - 1)], x, y);
        }
        if (sd.Boss >= 0)
        {
            int x = last.Cx, y = last.Cy;
            if (Occupied(x, y)) RandomFreeCellInRoom(last, out x, out y);
            Boss = EnemiesCount;
            Spawn(sd.Boss, x, y);
        }

        PickupsCount = 0;
        for (var i = 0; i < 3 + Bonus.Pickups && i < MaxPickups && Lv.RoomsCount > 1; ++i)
        {
            var rm = Lv.Rooms[R.Range(1, Lv.RoomsCount - 1)];
            RandomFreeCellInRoom(rm, out var x, out var y);
            Pickups[PickupsCount++] = new Pickup(x, y, i == 0 ? PickupType.Coffee : (PickupType)R.Range(0, 2), true);
        }
        Push(Msg("Etap ").Add(Stage + 1).Add(": ").Add(sd.Name));
        StageEvent = -1; // wydarzenie na placu: nie na pierwszym etapie i nie u bossa
        if (s > 0 && sd.Boss < 0 && R.Range(1, 100) <= D.SiteEventChancePct) ApplyEvent(R.Range(0, D.SiteEvents.Length - 1));
        UpdateFov();
    }

    public void Spawn(int defId, int x, int y)
    {
        var ed = D.Enemies[defId];
        ref var a = ref Enemies[EnemiesCount++];
        a = new Actor();
        a.X = (sbyte)x;
        a.Y = (sbyte)y;
        a.DefId = (sbyte)defId;
        a.Hp = a.MaxHp = (short)Math.Max(1, ed.MaxHealth * EnemyHpPct() / 100);
        a.Alive = true;
    }

    public int EnemyAt(int x, int y)
    {
        for (var i = 0; i < EnemiesCount; ++i)
        {
            if (Enemies[i].Alive && Enemies[i].X == x && Enemies[i].Y == y) return i;
        }
        return -1;
    }

    /// <summary>Statystyka efektywna: zawód + Warsztaty + cechy sprzętu (SIŁ/ZRĘ/INT +1).</summary>
    public int HeroStat(Stat s) => RunMods.ClassBaseStat(D, Cls, s) + StatBonus(s);

    public int StatBonus(Stat s) => RunMods.StatBonus(D, Bonus, Cls, s) + TraitBonus(RunMods.StatTrait(s));

    /// <summary>Przejście do kolejnego etapu (po ekranie harmonogramu). Przerwa na kawę: +5 HP.</summary>
    public void NextStage()
    {
        Hero.Hp = (short)Math.Min(Hero.MaxHp, Hero.Hp + 5);
        StartStage(Stage + 1);
    }

    /// <summary>
    /// NG+ („Kolejna budowa”): po wygranej ten sam zawód i poziom, premie i wynik zostają,
    /// wrogowie mocniejsi o kolejny stopień. Zwraca false, jeśli budowa nie została ukończona.
    /// </summary>
    public bool NewGamePlus()
    {
        if (St != GameStatus.Won) return false;
        ++Tier;
        for (var i = 0; i < Enemies.Length; i++) Enemies[i] = new Actor();
        Hero.Hp = Hero.MaxHp;
        StartStage(0);
        Push(Msg("Kolejna budowa! Poziom ").Add(Tier + 1));
        return true;
    }

    /// <summary>Skrót pokazowy (L+R+SELECT): zalicza etap albo pokonuje bossa.</summary>
    public void DebugSkip()
    {
        if (St != GameStatus.Playing) return;
        if (Boss >= 0 && Enemies[Boss].Alive)
        {
            Enemies[Boss].Hp = 1;
            HeroAttack(Boss);
        }
        else if (StairsX >= 0)
        {
            Hero.X = (sbyte)StairsX;
            Hero.Y = (sbyte)StairsY;
            EndTurn();
        }
    }
}
