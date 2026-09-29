using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// Rdzeń gry – port 1:1 struktury core::game z GBA/include/core.h (ta sama kolejność wywołań RNG,
/// ta sama arytmetyka całkowita i rzutowania int8/int16). Ten sam seed daje identyczną grę co na GBA.
/// Stan: mapa, bohater, problemy budowy, znajdźki, dziennik, mgła wojny, akty, stany, sprzęt.
/// </summary>
public sealed partial class Game
{
    /// <summary>v0.21.49: 12 z etapu (z bossem i wezwanymi) + miejsce na podziały.</summary>
    public const int MaxEnemies = 16;
    /// <summary>Rodzaje problemów (katalog: 16 + 32 bity w profilu).</summary>
    public const int MaxEnemyTypes = 48;
    public const int MaxStages = 12;
    public const int MaxPickups = 10;
    public const int LogLines = 3;
    public const int FovRadius = 7;
    public const int MaxHits = 8;
    public const int MaxWalls = 5;
    public const int MaxBridges = 3;
    /// <summary>Kask, rękawice, kamizelka + sloty z nagród (buty, pas).</summary>
    public const int MaxGearSlots = 6;
    /// <summary>Stany bohatera (indeks = StatusEffect; v0.21.50: + Mokry).</summary>
    public const int StatusSlots = 6;

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
    public readonly byte[] KillsByType = new byte[MaxEnemyTypes];
    public int StageDamage;
    public int StageKills;
    public int StageStartTurn;
    public byte ToolsFound;
    // liczniki zleceń (przenoszone do profilu przez Meta.BankCounters; ile już przeniesiono – w profilu, Run*)
    /// <summary>Użycia mocy.</summary>
    public ushort PowersUsed;
    /// <summary>Założone markowe przedmioty.</summary>
    public byte BrandFound;
    /// <summary>Bossowie aktu bez obrażeń w walce z nimi.</summary>
    public byte CleanBosses;
    /// <summary>StageDamage w chwili dołączenia bossa do walki (-1 = jeszcze nie).</summary>
    public int BossWakeDamage = -1;
    /// <summary>Wydarzenie na placu na bieżącym etapie (GameData.SiteEvents, -1 = brak).</summary>
    public sbyte StageEvent = -1;
    /// <summary>Pogoda dnia na bieżącym etapie (GameData.Weather).</summary>
    public sbyte Weather;
    /// <summary>Brygada: fachowiec wezwany na tym etapie (-1 = jeszcze nie).</summary>
    public sbyte HelperCalled = -1;
    /// <summary>Ochrona BHP-owca: tury do końca.</summary>
    public sbyte GuardTurns;
    /// <summary>Pomocnik obok bohatera: tury pomocy i pole.</summary>
    public sbyte AllyTurns, AllyX = -1, AllyY = -1;
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
    /// <summary>Boss z wezwaniami: tury do kolejnego wezwania.</summary>
    public int SummonCounter;
    /// <summary>Ilu wezwano w tej walce (uśpione miejsca za bossem w Enemies).</summary>
    public int SummonsUsed;
    /// <summary>Tury aktywnych stanów bohatera (indeks = StatusEffect).</summary>
    public readonly sbyte[] HeroStatus = new sbyte[StatusSlots];
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
    public readonly sbyte[] Equipped = [-1, -1, -1, -1, -1, -1];
    /// <summary>Cecha przedmiotu w slocie (GameData.GearTraits).</summary>
    public readonly sbyte[] EquippedTrait = new sbyte[MaxGearSlots];
    /// <summary>Kawy w termosie (pije się z menu akcji).</summary>
    public int Thermos;
    /// <summary>Paczka czeka na decyzję (zakładam / zostawiam): slot (-1 = brak), jakość, cecha.</summary>
    public sbyte OfferSlot = -1, OfferRarity, OfferTrait;
    public int WeaponOverride = -1;
    public int LogSerial;
    // v0.21.48: wybór ścieżki, materiały, naprawy, codzienna budowa, harmonogram domu
    /// <summary>Seed budowy (oferta ścieżek na harmonogramie).</summary>
    public uint RunSeed;
    /// <summary>Ścieżka bieżącego etapu (GameData.Paths), -1 = bez wyboru (pierwszy etap).</summary>
    public sbyte StagePath = -1;
    /// <summary>Wybór na harmonogramie: 0/1 = pozycja w ofercie.</summary>
    public sbyte NextPath;
    /// <summary>Materiały: cement, stal, drewno (GameData.Materials).</summary>
    public readonly byte[] Mats = new byte[4];
    /// <summary>Kładki na etapie (kałuże w zasięgu bez poślizgu).</summary>
    public sbyte Bridges;
    public readonly sbyte[] BridgeX = new sbyte[MaxBridges], BridgeY = new sbyte[MaxBridges];
    /// <summary>Codzienna budowa (seed dnia) i numer dnia.</summary>
    public bool Daily;
    public ushort DailyDay;
    /// <summary>Tury na każdym etapie (harmonogram domu po wygranej).</summary>
    public readonly ushort[] StageDays = new ushort[MaxStages];
    // v0.21.49: Respekt za etapy, reszty procentów obrażeń, Druga szansa
    /// <summary>Respekt zdobyty w tej budowie (profil: Meta.BankCounters).</summary>
    public int Respect;
    /// <summary>Reszty z procentowych premii obrażeń (Pct.Part).</summary>
    public int DmgCarry, TakenCarry;
    /// <summary>Druga szansa zużyta.</summary>
    public bool SecondUsed;
    // v0.21.49 (część 2): wybuch po usunięciu problemu (czerwone pola), strzały z dystansu (efekty warstwy Godota)
    public sbyte BlastX = -1, BlastY = -1, BlastTimer, BlastDmg;
    /// <summary>Bitmaska: którzy wrogowie strzelili w tej turze (warstwa prezentacji czyta i zeruje).</summary>
    public uint ShotEvents;
    // v0.21.49 (część 3): Akt 0 – pierwszy etap budowy (0 z Aktem 0, inaczej za nim), zebrane dokumenty (pieczątki)
    public sbyte FirstStage;
    /// <summary>Bitmaska zebranych dokumentów (GameData.Documents).</summary>
    public byte Docs;

    /// <summary>Numer etapu dla gracza (1..).</summary>
    public int StageNumber() => Stage - FirstStage + 1;

    /// <summary>Liczba etapów tej budowy (bez Aktu 0, gdy nieodblokowany).</summary>
    public int StagesInRun() => D.Stages.Length - FirstStage;

    /// <summary>Numer aktu bieżącego etapu dla gracza ("0", "I", "II", "III").</summary>
    public string ActNumeral() => D.Acts[D.Stages[Stage].Act].Numeral;

    /// <summary>
    /// Etap we wzorach (błoto, kałuże, porywy, oferta ścieżek) liczony od Fundamentów: Akt 0 nie zmienia wzorów etapów
    /// budowy (Akt 0 ma ujemne numery).
    /// </summary>
    public int PatternStage() => Stage - D.PreludeStages;

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

    /// <summary>Zasięg broni z pogodą: wiatr skraca zasięg broni dalekiego zasięgu (nie mniej niż 1).</summary>
    public int WeaponRange() => RangeOf(Weapon);

    /// <summary>Zasięg danej broni z pogodą (range_of).</summary>
    public int RangeOf(WeaponDef w)
    {
        var rg = w.Range; // Dekarz: wiatr mu nie przeszkadza
        return WeatherIs(WeatherEffect.Wind) && rg > 1 && !HasPassive(ClassPassive.Windproof) ? Math.Max(1, rg - WDef.Value) : rg;
    }

    public bool HasPassive(ClassPassive p) => CDef.Passive == p;

    /// <summary>Unik: szczęście + Respekt + buty, łącznie najwyżej DodgeMaxPct.</summary>
    public int DodgePct()
    {
        return Math.Min(D.DodgeMaxPct, D.DodgePerLuckPct * Luck() + Bonus.Dodge + GearBonus(GearStat.Dodge) + BoonSum(BoonEffect.Dodge));
    }

    // ------------------------------------------------------------------ pogoda dnia
    public WeatherDef WDef => D.Weather[Weather];

    // ------------------------------------------------------------------ tryb inwestora
    /// <summary>Suma wartości włączonych modyfikatorów danego rodzaju.</summary>
    public int InvestorValue(InvestorEffect e)
    {
        var v = 0;
        for (var i = 0; i < D.Investor.Length; ++i)
        {
            if (((Bonus.Investor >> i) & 1) != 0 && D.Investor[i].Effect == e) v += D.Investor[i].Value;
        }
        return v;
    }

    public bool InvestorHas(InvestorEffect e)
    {
        for (var i = 0; i < D.Investor.Length; ++i)
        {
            if (((Bonus.Investor >> i) & 1) != 0 && D.Investor[i].Effect == e) return true;
        }
        return false;
    }

    /// <summary>Przychód budowy (zł) z modyfikatorem budżetu.</summary>
    public int Income(int v) => v * (100 + InvestorValue(InvestorEffect.CashPct)) / 100;

    /// <summary>Co ile tur boss zapowiada cios (Kontrola częściej: krócej, nie mniej niż 2).</summary>
    public int SlamEvery() => Math.Max(2, D.SlamEvery - InvestorValue(InvestorEffect.Slam));

    /// <summary>Hurtownia zamknięta: po akcie od razu kolejny etap.</summary>
    public bool ShopClosed => InvestorHas(InvestorEffect.NoShop) || WeeklyHas(WeeklyRule.NoShop);

    public bool WeatherIs(WeatherEffect e) => D.Weather[Weather].Effect == e;

    private bool WeatherAllowed(int i, int s, bool badOnly) => (D.Weather[i].StagesMask & (1 << s)) != 0 && (!badOnly || D.Weather[i].Bad);

    /// <summary>Pogoda dnia: losowanie wagami spośród dozwolonych na etapie s (badOnly: tylko niekorzystne, jeśli są).</summary>
    public int RollWeather(int s, bool badOnly = false)
    {
        var total = 0;
        for (var i = 0; i < D.Weather.Length; ++i)
        {
            if (WeatherAllowed(i, s, badOnly)) total += D.Weather[i].Weight;
        }
        if (total == 0) return RollWeather(s, false);
        var roll = R.Range(1, total);
        for (var i = 0; i < D.Weather.Length; ++i)
        {
            if (!WeatherAllowed(i, s, badOnly)) continue;
            if (roll <= D.Weather[i].Weight) return i;
            roll -= D.Weather[i].Weight;
        }
        return 0;
    }

    /// <summary>
    /// Deszcz: kałuże na części pól podłogi (stały wzór zależny od etapu); wejście w kałużę = poślizg. Kładka: kałuże
    /// w jej zasięgu nie działają do końca etapu.
    /// </summary>
    public bool Puddle(int x, int y)
    {
        if (!WeatherIs(WeatherEffect.Rain) || Lv.At(x, y) != Tile.Floor || (x * 7 + y * 13 + PatternStage() * 5) % WDef.Value != 0) return false;
        for (var i = 0; i < Bridges; ++i)
        {
            if (Cheb(x, y, BridgeX[i], BridgeY[i]) <= BridgeReach()) return false;
        }
        return true;
    }

    public int StatusTurns(StatusEffect s) => HeroStatus[(int)s];

    /// <summary>Wiadomość fabularna na wejściu etapu (przy NG+ pierwszy etap ma własną).</summary>
    public StoryMsg StageStory => Tier > 0 && Stage == FirstStage ? D.StoryNgPlus : D.StoryStages[Stage];

    public bool Visible(int x, int y) => Level.In(x, y) && Fov[y * Level.W + x] == Sight.InView;
    public bool Explored(int x, int y) => Level.In(x, y) && Fov[y * Level.W + x] != Sight.Unknown;

    /// <summary>Pole w zasięgu zapowiedzianego uderzenia bossa (czerwone pola na mapie).</summary>
    public bool SlamCell(int x, int y) => SlamTimer > 0 && SlamCellAt(x, y);
    public bool SlamCellAt(int x, int y)
    {
        if (SlamX < 0) return false;
        if (Boss >= 0 && D.Enemies[Enemies[Boss].DefId].Shape == SlamShape.Cross) // Kontrola BHP: wiersz i kolumna
        {
            return (x == SlamX && Math.Abs(y - SlamY) <= D.SlamCrossReach) || (y == SlamY && Math.Abs(x - SlamX) <= D.SlamCrossReach);
        }
        return Cheb(x, y, SlamX, SlamY) <= D.SlamRadius;
    }

    /// <summary>Pełny sprzęt: założony przedmiot w każdym slocie bazowym (kask, rękawice, kamizelka).</summary>
    public bool FullGear()
    {
        for (var i = 0; i < D.GearSlotsCount; ++i)
        {
            if (((D.GearBaseMask >> i) & 1) != 0 && Equipped[i] < 0) return false;
        }
        return true;
    }

    /// <summary>Boss dołącza do walki (raz na etap): licznik Czystej roboty; Inspekcja przy pełnym sprzęcie traci turę.</summary>
    public void BossEngaged()
    {
        BossWakeDamage = StageDamage;
        var bd = D.Enemies[Enemies[Boss].DefId];
        if (bd.GearStun > 0 && FullGear())
        {
            Enemies[Boss].Stun = (sbyte)Math.Max(Enemies[Boss].Stun, bd.GearStun);
            Push(Msg("Wszystko zgodnie z BHP!").As(LogKind.Good));
        }
    }

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
    public int Luck() => CDef.Luck + Bonus.Luck + TraitBonus(TraitEffect.Luck) + BoonLuck();

    public int CritPct() => D.CritBasePct + D.CritPerLuckPct * Luck() + TraitBonus(TraitEffect.Crit) + Bonus.Crit + BoonSum(BoonEffect.Crit)
                            + ToolTraitValue(ToolTraitEffect.Crit);

    /// <summary>Pole widzenia; pył (akt III) zmniejsza, najmniej 3.</summary>
    public int SightRadius() => Math.Max(3, FovRadius + TraitBonus(TraitEffect.Sight) + Bonus.Sight + BoonSum(BoonEffect.Sight) - DustSight());

    /// <summary>Pojemność termosu (+ uprawnienia i pamiątka).</summary>
    public int ThermosCap() => D.ThermosCapacity + Bonus.Thermos + GearBonus(GearStat.Thermos) + BoonSum(BoonEffect.Thermos);

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
            case EventEffect.Cash: Cash += Income(ev.Value); break;
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


    /// <summary>Obrona bohatera: zawód + premie + sprzęt + ochrona BHP-owca z brygady.</summary>
    public int HeroDefense() =>
        CDef.Defense + DefBonus + GearBonus(GearStat.Def) + (GuardTurns > 0 ? D.Brigade[HelperCalled].Value : 0) + BoonDefense()
        + EventDef; // wydarzenie (#30): OBR na etap

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
        if ((s == StatusEffect.Poison || s == StatusEffect.Shock) && SynergyOn(SynergyEffect.Safety)) // synergia Pełne BHP
        {
            Push(Msg("Pełne BHP: bez stanu").As(LogKind.Good));
            return;
        }
        if (s != StatusEffect.Wet && BoonSum(BoonEffect.StatusRes) > 0) // Instrukcja BHP: stany krócej
        {
            t -= BoonSum(BoonEffect.StatusRes);
            if (t <= 0)
            {
                Push(Msg("Instrukcja BHP: bez stanu").As(LogKind.Good));
                return;
            }
        }
        if (s == StatusEffect.Slip && TraitBonus(TraitEffect.SlipRes) > 0)
        {
            Push(Msg("Odporność: bez poślizgu").As(LogKind.Good));
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
    public int EnemyHpPct() => D.Stages[Stage].HpPct * DDef.HpPct / 100 * (100 + Tier * D.NgHpPctPerTier) / 100
                               * (100 + InvestorValue(InvestorEffect.EnemyHp)) / 100;

    public int EnemyDmgBonus() => D.Stages[Stage].DmgBonus + DDef.DmgBonus + Tier * D.NgDmgBonusPerTier + InvestorValue(InvestorEffect.EnemyDmg);

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
        RunSeed = seed;
        Hero.MaxHp = Hero.Hp = (short)(CDef.MaxHealth + mods.Hp);
        Hero.Alive = true;
        Cash = mods.Cash;
        FirstStage = (sbyte)(mods.Act0 != 0 ? 0 : D.PreludeStages); // bez nagrody Akt 0 budowa zaczyna się od Fundamentów
        StartStage(FirstStage);
    }

    public bool Occupied(int x, int y)
    {
        if (Hero.Alive && Hero.X == x && Hero.Y == y) return true;
        if (AllyTurns > 0 && AllyX == x && AllyY == y) return true; // pomocnik z brygady
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

    public void StartStage(int s, int path = -1)
    {
        Stage = s;
        St = GameStatus.Playing;
        StagePath = (sbyte)path;
        var pd = path >= 0 ? D.Paths[path] : null;
        Lv.Generate(ref R);
        WallsCount = 0;
        Bridges = 0;
        StageDamage = 0;
        StageKills = 0;
        StageStartTurn = Turns;
        BossWakeDamage = -1;
        ActCleared = false;
        SlamTimer = 0;
        SlamX = SlamY = -1;
        SlamCounter = 0;
        SummonCounter = 0;
        SummonsUsed = 0;
        HelperCalled = -1; // brygada: raz na etap
        GuardTurns = 0;
        AllyTurns = 0;
        AllyX = AllyY = -1;
        BlastTimer = 0;
        BlastX = BlastY = -1;
        ShotEvents = 0;
        Docs = 0;
        PendingEvent = -1; // v0.21.50 cz. 3
        StageChoice = StageChoicePick = -1;
        ChoiceDone = 0;
        EventDmg = EventDef = 0;
        ToolOffer = ToolOfferPickup = -1;
        SecretX = SecretY = -1;
        SecretOpen = false;
        SecretKind = SecretDir = 0;
        KeyHolder = -1;
        Keys = 0;
        SecretRx = SecretRy = SecretRw = SecretRh = 0;
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
        var count = Math.Max(1, sd.EnemyCount + (pd?.Enemies ?? 0)); // ścieżka: więcej / mniej problemów
        for (var i = 0; i < count && EnemiesCount < MaxEnemies; ++i)
        {
            var roomI = 1 + R.Range(0, Lv.RoomsCount - 2 > 0 ? Lv.RoomsCount - 2 : 0);
            if (roomI >= Lv.RoomsCount) roomI = Lv.RoomsCount - 1;
            RandomFreeCellInRoom(Lv.Rooms[roomI], out var x, out var y);
            Spawn(sd.Pool[R.Range(0, sd.Pool.Length - 1)], x, y);
            if (R.Range(1, 100) <= EliteChance()) MakeElite(EnemiesCount - 1, R.Range(0, D.Elites.Length - 1)); // elita
        }
        if (sd.Boss >= 0)
        {
            int x = last.Cx, y = last.Cy;
            if (Occupied(x, y)) RandomFreeCellInRoom(last, out x, out y);
            Boss = EnemiesCount;
            Spawn(sd.Boss, x, y);
            var bd = D.Enemies[sd.Boss];
            for (var k = 0; k < bd.SummonMax && EnemiesCount < MaxEnemies; ++k) // uśpione miejsca na wezwanych
            {
                Spawn(bd.Summon, x, y);
                Enemies[EnemiesCount - 1].Alive = false;
            }
        }

        PickupsCount = 0;
        var pickupsN = Math.Max(1, 3 + Bonus.Pickups + (pd?.Pickups ?? 0));
        for (var i = 0; i < pickupsN && i < MaxPickups && Lv.RoomsCount > 1; ++i)
        {
            var rm = Lv.Rooms[R.Range(1, Lv.RoomsCount - 1)];
            RandomFreeCellInRoom(rm, out var x, out var y);
            Pickups[PickupsCount++] = new Pickup(x, y, i == 0 ? PickupType.Coffee : (PickupType)R.Range(0, 2), true);
        }
        Push(Msg("Etap ").Add(StageNumber()).Add(": ").Add(sd.Name));
        if (pd != null) // ścieżka z harmonogramu: budżet i materiały od razu
        {
            Push(Msg("Ścieżka: ").Add(pd.Name));
            if (pd.Cash != 0) Cash = Math.Max(0, Cash + Income(pd.Cash));
            for (var k = 0; k < pd.Materials; ++k) AddMaterial(R.Range(0, D.Materials.Length - 1));
        }
        Weather = (sbyte)RollWeather(s, pd != null && pd.BadWeather); // pogoda dnia
        if (WeeklyHas(WeeklyRule.Weather)) Weather = (sbyte)WeeklyValue(WeeklyRule.Weather); // wyzwanie: Mokry tydzień
        if (WDef.Effect != WeatherEffect.None)
            Push(Msg("Pogoda: ").Add(WDef.Name).Add(" (").Add(WDef.Short).Add(")").As(WDef.Bad ? LogKind.Bad : LogKind.Good));
        StageEvent = -1; // wydarzenie na placu: nie na pierwszym etapie i nie u bossa
        if (s > FirstStage && sd.Boss < 0 && !(pd != null && pd.NoEvent) && R.Range(1, 100) <= D.SiteEventChancePct)
        {
            var e = R.Range(0, D.SiteEvents.Length - 1);
            // niekorzystna pogoda i niekorzystne wydarzenie naraz to za dużo: wydarzenie przepada
            if (!(D.WeatherNoBadStack && WDef.Bad && !D.SiteEvents[e].Good)) ApplyEvent(e);
        }
        PlaceDocuments();
        PlaceEvent(); // v0.21.50 cz. 3: pole wydarzenia z wyborem (#30)
        PlaceSecret(); // ukryte pomieszczenie (#32): magazyn, skrzynia, klucz, strażnik
        // kombinacje stanów: w pyle (akt III) problemy są zapylone, w Mróz – zmrożone (boss nie)
        for (var i = 0; i < EnemiesCount; ++i)
        {
            if (i == Boss) continue;
            if (DustSight() > 0) Enemies[i].Flags = (byte)(Enemies[i].Flags | ActorFlag.Dusty);
            if (WeatherIs(WeatherEffect.Frost)) Enemies[i].Flags = (byte)(Enemies[i].Flags | ActorFlag.Frozen);
        }
        UpdateFov();
    }

    /// <summary>Pieczątki (Akt 0): dokumenty w różnych pokojach (bez pierwszego), na wolnych polach bez znajdziek.</summary>
    public void PlaceDocuments()
    {
        var n = DocsNeeded();
        if (n == 0 || Lv.RoomsCount < 2) return;
        int span = Lv.RoomsCount - 1, bas = R.Range(0, span - 1);
        for (var k = 0; k < n && PickupsCount < MaxPickups; ++k)
        {
            var rm = Lv.Rooms[1 + (bas + k * Math.Max(1, span / n)) % span];
            int x = rm.Cx, y = rm.Cy;
            for (var t = 0; t < 40; ++t)
            {
                int cx = R.Range(rm.X, rm.X + rm.W - 1), cy = R.Range(rm.Y, rm.Y + rm.H - 1);
                if (Lv.At(cx, cy) == Tile.Floor && !Occupied(cx, cy) && !PickupAt(cx, cy))
                {
                    x = cx;
                    y = cy;
                    break;
                }
            }
            Pickups[PickupsCount++] = new Pickup(x, y, PickupType.Document, true, k);
        }
        Push(Msg("Pieczątki: zbierz ").Add(n).Add(" dokumenty").As(LogKind.Bad));
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

    /// <summary>Przejście do kolejnego etapu (po ekranie harmonogramu) wybraną ścieżką. Przerwa na kawę: +5 HP.</summary>
    public void NextStage()
    {
        if (!InvestorHas(InvestorEffect.NoBreak)) Hero.Hp = (short)Math.Min(Hero.MaxHp, Hero.Hp + 5); // tryb inwestora: bez przerwy
        if (BoonSum(BoonEffect.RegenStage) > 0) Hero.Hp = (short)Math.Min(Hero.MaxHp, Hero.Hp + BoonSum(BoonEffect.RegenStage));
        SkipBoons(); // oferta bez wyboru przepada
        var path = D.Paths.Length >= 2 ? PathOffer(NextPath) : -1;
        NextPath = 0;
        StartStage(Stage + 1, path);
    }

    /// <summary>
    /// NG+ („Kolejna budowa”): po wygranej ten sam zawód i poziom, premie i wynik zostają,
    /// wrogowie mocniejsi o kolejny stopień. Zwraca false, jeśli budowa nie została ukończona.
    /// </summary>
    public bool NewGamePlus()
    {
        if (St != GameStatus.Won) return false;
        ++Tier;
        EventsSeen = 0; // nowa budowa: wydarzenia od nowa
        ClearTimeline(); // podsumowanie: oś czasu nowej budowy
        for (var i = 0; i < Enemies.Length; i++) Enemies[i] = new Actor();
        Hero.Hp = Hero.MaxHp;
        StartStage(FirstStage);
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
            Enemies[Boss].Flags = (byte)(Enemies[Boss].Flags | ActorFlag.Phase);
            HeroAttack(Boss);
        }
        else if (StairsX >= 0) // na schody i od razu zaliczony (problem obok mógłby zepchnąć bohatera w tej turze)
        {
            Docs = (byte)((1 << DocsNeeded()) - 1);
            Hero.X = (sbyte)StairsX;
            Hero.Y = (sbyte)StairsY;
            ++Turns;
            ClearStage();
        }
    }
}
