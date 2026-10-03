using System.Text.Json;

namespace LifeLike.Core.Data;

/// <summary>
/// Dane gry wczytane z game.json (tego samego pliku, z którego GBA generuje include/game_data.h
/// skryptem tools/gen_data.py). Interpretacja pól 1:1 z gen_data.py: identyfikatory zamieniane na indeksy,
/// maski bitowe liczone tak samo. Nieznane pola są ignorowane (plik rozwija się razem z wersją GBA).
/// </summary>
public sealed class GameData
{
    public WeaponDef[] Weapons { get; private init; } = [];
    public ClassDef[] Classes { get; private init; } = [];
    public EnemyDef[] Enemies { get; private init; } = [];
    public StageDef[] Stages { get; private init; } = [];
    public DifficultyDef[] Difficulties { get; private init; } = [];
    public UpgradeDef[] Upgrades { get; private init; } = [];
    public StoryMsg[] StoryStages { get; private init; } = [];
    public StoryMsg StoryWin { get; private init; } = new("", ["", "", ""]);
    public StoryMsg StoryLose { get; private init; } = new("", ["", "", ""]);
    public StoryMsg StoryNgPlus { get; private init; } = new("", ["", "", ""]);
    public StoryMsg StoryPrologue { get; private init; } = new("", ["", "", ""]);
    public string[] PrologueCaptions { get; private init; } = [];
    public ActDef[] Acts { get; private init; } = [];
    public ShopItemDef[] Hurtownia { get; private init; } = [];
    public BadgeDef[] Badges { get; private init; } = [];
    public ToolDef[] Tools { get; private init; } = [];
    /// <summary>Pamiątki (sekcja "keepsakes"): wybierane na start budowy, ranga rośnie z budowami.</summary>
    public KeepsakeDef[] Keepsakes { get; private init; } = [];
    /// <summary>Po ilu budowach z pamiątką ranga II i III.</summary>
    public int[] KeepsakeRankRuns { get; private init; } = [3, 8];
    /// <summary>Zlecenia: długofalowe cele z licznikami w profilu.</summary>
    public ContractDef[] Contracts { get; private init; } = [];
    /// <summary>Wydarzenia na placu (sekcja "siteEvents").</summary>
    public SiteEventDef[] SiteEvents { get; private init; } = [];
    public int SiteEventChancePct { get; private init; }
    /// <summary>Pogoda dnia (sekcja "weather"); bez sekcji – jedna pogoda bez skutku.</summary>
    public WeatherDef[] Weather { get; private init; } = [];
    /// <summary>Niekorzystna pogoda i niekorzystne wydarzenie na placu naraz: wydarzenie przepada.</summary>
    public bool WeatherNoBadStack { get; private init; }
    /// <summary>Brygada: najemni fachowcy (sekcja "brigade"); bez sekcji – pusta lista.</summary>
    public HelperDef[] Brigade { get; private init; } = [];
    /// <summary>Fachowcy dostępni od początku (koszt 0).</summary>
    public int StartHelpersMask { get; private init; }
    /// <summary>Tryb inwestora: modyfikatory trudności (sekcja "investor"); bez sekcji – pusta lista.</summary>
    public InvestorDef[] Investor { get; private init; } = [];
    /// <summary>Materiały (sekcja "materials"): cement, stal, drewno; bez sekcji – pusta lista (bez dropów).</summary>
    public MaterialDef[] Materials { get; private init; } = [];
    /// <summary>Naprawy pola za materiał (Załataj, Kładka).</summary>
    public RepairDef[] Repairs { get; private init; } = [];
    public int MaterialDropPct { get; private init; }
    public int MaterialBossDrop { get; private init; }
    public int MaterialGearBox { get; private init; }
    public int MaterialMax { get; private init; } = 9;
    /// <summary>Wybór ścieżki między etapami (sekcja "paths"); bez sekcji – pusta lista (bez wyboru).</summary>
    public PathDef[] Paths { get; private init; } = [];
    /// <summary>Codzienna budowa: dzień nr 1 (rok, miesiąc, dzień), domyślna data na GBA, trudność, ile modyfikatorów dnia,
    /// ile dni w historii wyników.</summary>
    public int[] DailyEpoch { get; private init; } = [2026, 1, 1];
    public int[] DailyDefaultDate { get; private init; } = [2026, 10, 1];
    public int DailyDifficulty { get; private init; } = 1;
    public int DailyInvestorMods { get; private init; }
    public int DailyHistory { get; private init; } = 5;
    /// <summary>Harmonogram domu po wygranej: dni etapu = ScheduleMinDays + tury / ScheduleTurnsPerDay.</summary>
    public int ScheduleMinDays { get; private init; } = 4;
    public int ScheduleTurnsPerDay { get; private init; } = 3;
    public string ScheduleUrl { get; private init; } = "planbudowlany.online";
    /// <summary>Indeks = slot * 3 + jakość.</summary>
    public GearDef[] Gear { get; private init; } = [];
    public string[] GearSlots { get; private init; } = [];
    public string[] GearRarities { get; private init; } = [];
    /// <summary>Wagi dropów w kolejności typów znajdziek: kawa, kask, projekt, narzędzie, sprzęt.</summary>
    public int[] DropWeights { get; private init; } = [];
    public int[] LevelThresholds { get; private init; } = [];
    /// <summary>Opisy stanów, indeks = StatusEffect (0 = brak).</summary>
    public StatusDef[] Statuses { get; private init; } = [];
    /// <summary>Cechy sprzętu (losowane do każdego przedmiotu).</summary>
    public TraitDef[] GearTraits { get; private init; } = [];

    public int SlamEvery { get; private init; }
    public int SlamDamageBonus { get; private init; }
    public int SlamRadius { get; private init; }
    /// <summary>Tury od zapowiedzi do ciosu (kwadrat) i dla krzyża (Kontrola BHP), zasięg ramion krzyża.</summary>
    public int SlamDelay { get; private init; } = 2;
    public int SlamCrossDelay { get; private init; } = 3;
    public int SlamCrossReach { get; private init; } = 2;
    public int CashPerScore { get; private init; }
    public int StartToolsMask { get; private init; }
    public int DropChancePct { get; private init; }
    public int GearSolidFrom { get; private init; }
    public int GearBrandFrom { get; private init; }
    public int GearStageBonus { get; private init; }
    public int XpPerKill { get; private init; }
    public int XpPerStage { get; private init; }
    public int XpBoss { get; private init; }
    public int StartClassesMask { get; private init; }
    /// <summary>v0.21.52: cena kolejnego kupionego zawodu (rośnie z każdym zakupem; ostatnia dla następnych).</summary>
    public int[] ClassCosts { get; private init; } = [];
    /// <summary>v0.21.52: cena kolejnego kupionego narzędzia.</summary>
    public int[] ToolCosts { get; private init; } = [];
    public int HardCost { get; private init; }
    public int HpPerLevel { get; private init; }
    public int DmgLevelsMask { get; private init; }
    public int DefLevelsMask { get; private init; }
    public string Version { get; private init; } = "";
    /// <summary>v0.21.50: Jak grać, strona Obrażenia – obrażenia broni w prostych słowach (rozpiska #26).</summary>
    public string[] DamageHelpLines { get; private init; } = [];
    /// <summary>v0.21.50 cz. 3: Jak grać – wydarzenia z wyborem, ulepszanie narzędzia, magazyn (sekcja "extrasHelp").</summary>
    public string[] ExtrasHelpLines { get; private init; } = [];
    /// <summary>v0.21.50 cz. 4: Jak grać – podsumowanie budowy, wyzwanie tygodnia, fabuła (sekcja "metaHelp").</summary>
    public string[] MetaHelpLines { get; private init; } = [];
    /// <summary>Wydarzenia z wyborem (#30, sekcja "choiceEvents") i szansa na pole wydarzenia na etapie.</summary>
    public ChoiceEventDef[] ChoiceEvents { get; private init; } = [];
    public int ChoiceEventChancePct { get; private init; }
    /// <summary>Ulepszanie narzędzia (#31, sekcja "toolUpgrade"): koszt poziomów, cechy, maks., +obrażeń, poziom cechy.</summary>
    public ToolLevelDef[] ToolLevels { get; private init; } = [];
    public ToolTraitDef[] ToolTraits { get; private init; } = [];
    public int ToolUpgradeMax { get; private init; }
    public int ToolUpgradeDmg { get; private init; }
    public int ToolTraitAt { get; private init; }
    /// <summary>Ukryte pomieszczenia (#32, sekcja "hiddenRooms"): rodzaje, szansa, strażnik, zawartość skrzyni.</summary>
    public SecretKindDef[] SecretKinds { get; private init; } = [];
    public int SecretChancePct { get; private init; }
    public int SecretGuardPct { get; private init; }
    public int ChestRespect { get; private init; }
    public int ChestMats { get; private init; }
    public int ChestCash { get; private init; }
    public int ChestGearMin { get; private init; }
    /// <summary>v0.21.50 cz. 4: podsumowanie budowy (#33) – rodzaje ciosów (= RecapKind), czasowniki wg rodzaju, rady.</summary>
    public string[] RecapKindNames { get; private init; } = [];
    public string[] RecapVerbs { get; private init; } = ["Pokonał", "Pokonała", "Pokonało"];
    public RecapTipDef[] RecapTips { get; private init; } = [];
    /// <summary>Wyzwania tygodnia (#34, sekcja "weekly"): lista zasad, tydzień nr 1 (poniedziałek), trudność, historia, kawa na wynos.</summary>
    public WeeklyDef[] Weekly { get; private init; } = [];
    public int[] WeeklyEpoch { get; private init; } = [2026, 1, 5];
    public int WeeklyDifficulty { get; private init; } = 1;
    public int WeeklyHistory { get; private init; } = 3;
    public int WeeklyCoffeeCash { get; private init; }
    /// <summary>Fabuła odkrywana z budowami (#35, story.arc) i ozdoby Osiedla (estate.decor).</summary>
    public StoryThread[] StoryArc { get; private init; } = [];
    public DecorDef[] EstateDecor { get; private init; } = [];

    // v0.21.52 cz. b: poziom inspektora (#44), mistrzostwo zawodu (#45), stopnie inwestora (#48)
    /// <summary>Poziomy inspektora: dośw. na poziom i nagroda (sekcja "inspector").</summary>
    public ProgressLevel[] InspectorLevels { get; private init; } = [];
    /// <summary>Dośw. inspektora z budowy: budowa, etap, boss, elita, magazyn, wygrana; x procent trudności.</summary>
    public int InspectorXpRun { get; private init; }
    public int InspectorXpStage { get; private init; }
    public int InspectorXpBoss { get; private init; }
    public int InspectorXpElite { get; private init; }
    public int InspectorXpStoreroom { get; private init; }
    public int InspectorXpWin { get; private init; }
    public int[] InspectorDiffPct { get; private init; } = [];
    /// <summary>Migracja profilu v13 -> v14: dośw. inspektora za budowę, wygraną i % Respektu łącznie.</summary>
    public int InspectorMigrateRun { get; private init; }
    public int InspectorMigrateWin { get; private init; }
    public int InspectorMigrateRespectPct { get; private init; }
    /// <summary>Mistrzostwo zawodu 1-10: dośw. na poziom i nagroda (sekcja "mastery").</summary>
    public ProgressLevel[] MasteryLevels { get; private init; } = [];
    /// <summary>Wariant mocy, broń mistrza i premia mistrzostwa (indeks = zawód).</summary>
    public MasteryClassDef[] MasteryClasses { get; private init; } = [];
    /// <summary>Migracja: dośw. mistrzostwa za dom zawodu na Osiedlu i za wygraną zawodem.</summary>
    public int MasteryMigrateWin { get; private init; }
    public int MasteryMigrateClassWin { get; private init; }
    /// <summary>Stopnie inwestora: nagroda za nowy najwyższy próg stawki (Xp = stawka; investor.ranks).</summary>
    public ProgressLevel[] StakeRanks { get; private init; } = [];
    /// <summary>Tytuły z poziomu inspektora i stopni inwestora (po tytułach z odznak i zleceń).</summary>
    public ProgressTitle[] ProgressTitles { get; private init; } = [];
    /// <summary>Jak grać: poziom inspektora i mistrzostwo zawodu.</summary>
    public string[] ProgressHelpLines { get; private init; } = [];

    // v0.21.52 cz. c: drzewko Szkoleń, kolekcje, zadania dnia i tygodnia, seria dni
    /// <summary>Gałęzie drzewka (meta.tree.branches): pień = Szkolenia (maska bitów Upgrades).</summary>
    public TreeBranch[] TreeBranches { get; private init; } = [];
    /// <summary>Węzły drzewka: głębokość pnia gałęzi, koszt, 1 z 2 opcji.</summary>
    public TreeNode[] TreeNodes { get; private init; } = [];
    /// <summary>Dośw. za zmianę wyboru w węźle.</summary>
    public int TreeRespecCost { get; private init; }
    /// <summary>Komplety kolekcji (sekcja "collections").</summary>
    public CollectionDef[] Collections { get; private init; } = [];
    /// <summary>Pule zadań dnia i tygodnia (sekcja "tasks").</summary>
    public TaskDef[] DailyTasks { get; private init; } = [];
    public TaskDef[] WeeklyTasks { get; private init; } = [];
    /// <summary>Nagrody za wykonane zadania łącznie (Xp = liczba zadań).</summary>
    public ProgressLevel[] TaskRewards { get; private init; } = [];
    /// <summary>Nagrody za serię dni budowy dnia (Xp = dni; daily.streak).</summary>
    public ProgressLevel[] StreakRewards { get; private init; } = [];
    /// <summary>Jak grać: drzewko, kolekcje, zadania, seria dni.</summary>
    public string[] GoalsHelpLines { get; private init; } = [];
    /// <summary>v0.21.52 cz. d: Jak grać – mapa kariery (7 linii, GBA str. 18).</summary>
    public string[] CareerHelpLines { get; private init; } = [];
    /// <summary>v0.21.52 cz. d (#47): kontrakty mapy kariery (0 = Dom jednorodzinny, etapy w Stages od First).</summary>
    public CareerDef[] Career { get; private init; } = [];
    /// <summary>v0.21.53 (#53, #54): filtry ekranu (0 = klasyczny; zabawowe do odblokowania, dla daltonistów zawsze).</summary>
    public ScreenFilterDef[] ScreenFilters { get; private init; } = [new("klasyczny", "Klasyczny", "Klasyk", "", "", FilterKind.Classic, false, false, [])];
    /// <summary>v0.21.53: teksty interfejsu filtrów (screenFilters.ui – do tłumaczenia); brak klucza = sam klucz.</summary>
    public IReadOnlyDictionary<string, string> FilterUi { get; private init; } = new Dictionary<string, string>();
    /// <summary>v0.21.53: Jak grać (Godot) – gdzie filtry i wzory (screenFilters.where).</summary>
    public string[] FilterWhereLines { get; private init; } = [];
    /// <summary>v0.21.53: litery rzadkości premii przy wzorach (zwykła, rzadka, legendarna).</summary>
    public string[] RarityLetters { get; private init; } = ["Z", "R", "L"];

    /// <summary>Tekst interfejsu filtrów o kluczu key.</summary>
    public string FilterText(string key) => FilterUi.TryGetValue(key, out var v) ? v : key;

    /// <summary>v0.21.53: Jak grać – filtry ekranu (7 linii, GBA str. 19).</summary>
    public string[] FiltersHelpLines { get; private init; } = [];
    /// <summary>Bliźniak: ile problemów z pierwszej połowy przechodzi na drugą.</summary>
    public int CareerTwinCarryMax { get; private init; }
    /// <summary>Etapy Domu jednorodzinnego (kontrakt 0) – Stages ma też etapy kolejnych kontraktów (jak data::stages_count).</summary>
    public int StagesCount { get; private init; }
    /// <summary>Palety etapów (tiles/stage_N.png).</summary>
    public int StageLooksCount { get; private init; }
    public int DefaultDifficulty { get; private init; }
    public int NgHpPctPerTier { get; private init; }
    public int NgDmgBonusPerTier { get; private init; }
    public int NgScorePctPerTier { get; private init; }
    /// <summary>Papierologia: o ile tur później moc.</summary>
    public int PaperDelay { get; private init; }
    // Szczęście (sekcja "luck"): kryt, unik, dropy, jakość sprzętu.
    public int CritBasePct { get; private init; }
    public int CritPerLuckPct { get; private init; }
    public int CritMultiplier { get; private init; }
    public int DropPerLuckPct { get; private init; }
    public int RarityPerLuck { get; private init; }
    public int DodgePerLuckPct { get; private init; }
    public int DodgeMaxPct { get; private init; }
    // Termos (sekcja "thermos").
    public int ThermosCapacity { get; private init; }
    public int CoffeeHeal { get; private init; }
    public int BotDrinkBelowPct { get; private init; }
    /// <summary>Doświadczenie za odrzucenie paczki sprzętu (plus jakość paczki).</summary>
    public int GearDeclineXp { get; private init; }

    // v0.21.49: Respekt, nagrody za odbiór, cechy zawodów
    /// <summary>Ulepszenia za Respekt (sekcja "respect"); bez sekcji – pusta lista.</summary>
    public RespectDef[] Respect { get; private init; } = [];
    /// <summary>Respekt za etap: zwykły, boss w środku aktu, boss aktu, ostatni.</summary>
    public int RespectStage { get; private init; }
    public int RespectBoss { get; private init; }
    public int RespectActBoss { get; private init; }
    public int RespectFinal { get; private init; }
    /// <summary>Nagrody za odbiór (sekcja "rewards"): każda wygrana odblokowuje kolejną.</summary>
    public RewardDef[] Rewards { get; private init; } = [];
    /// <summary>Operator koparki: szansa (%), że cios wręcz odepchnie problem.</summary>
    public int PushChancePct { get; private init; }
    /// <summary>Sloty sprzętu z nagród (buty, pas) i sloty bazowe (pełny sprzęt BHP).</summary>
    public int GearRewardMask { get; private init; }
    public int GearBaseMask { get; private init; } = 7;
    /// <summary>Zawody odblokowywane tylko nagrodą za odbiór.</summary>
    public int RewardClassesMask { get; private init; }

    // v0.21.51 cz. 2: sekretne zlecenia (#39)
    /// <summary>Sekretne zlecenia (sekcja "secrets"); bez sekcji – pusta lista.</summary>
    public SecretDef[] Secrets { get; private init; } = [];
    /// <summary>Wygląd z sekretnych zleceń (tylko oprawa).</summary>
    public CosmeticDef[] Cosmetics { get; private init; } = [];
    /// <summary>Zawody z sekretnych zleceń (na końcu listy) i liczba zwykłych zawodów (budowa dnia, balans, Pełny zespół).</summary>
    public int SecretClassesMask { get; private init; }
    public int OpenClassesCount { get; private init; }
    /// <summary>Narzędzia z sekretnych zleceń (w dropach dopiero po wykonaniu).</summary>
    public int SecretToolsMask { get; private init; }
    /// <summary>Problemy papierowe (Akt 0 bez obrażeń od papierów), bity indeksów GameData.Enemies.</summary>
    public ulong SecretPaperMask { get; private init; }
    /// <summary>Boss pokonany ciosem brygady (Szef tylko dzwoni), -1 = brak.</summary>
    public int SecretHelperBoss { get; private init; } = -1;
    /// <summary>Wygląd: złoty błysk broni przy krycie, kask w paski (-1 = brak).</summary>
    public int CosmeticGold { get; private init; } = -1;
    public int CosmeticStripes { get; private init; } = -1;
    /// <summary>Geodeta: Tyczenie trwa tyle tur.</summary>
    public int MarkTurns { get; private init; } = 6;

    // v0.21.49 (część 2): zachowania problemów (sekcja "behaviorParams"), nazwy zachowań (indeks = bit)
    public int BehaviorRangedReach { get; private init; } = 3;
    public int BehaviorSplitHpPct { get; private init; } = 50;
    public int BehaviorHealValue { get; private init; } = 3;
    public int BehaviorHealEvery { get; private init; } = 2;
    public int BehaviorBlastDamage { get; private init; } = 4;
    public int BehaviorBlastRadius { get; private init; } = 1;
    public int BehaviorBlastDelay { get; private init; } = 2;
    public int BehaviorGrowEvery { get; private init; } = 4;
    public int BehaviorGrowHp { get; private init; } = 2;
    public int BehaviorGrowMax { get; private init; } = 4;
    public int BehaviorFleeCooldown { get; private init; } = 3;
    public int BehaviorReturnTurns { get; private init; } = 4;
    public int BehaviorReturnHpPct { get; private init; } = 50;
    public int BehaviorPushCooldown { get; private init; } = 3;
    public string[] BehaviorNames { get; private init; } = Behavior.Ids;

    // v0.21.49 (część 3): Akt 0 (pieczątki, druga faza bossa), samouczek menu
    /// <summary>Etapy aktu wstępnego (Akt 0) na początku listy: bez nagrody za odbiór budowa zaczyna się za nimi.</summary>
    public int PreludeStages { get; private init; }
    /// <summary>Dokumenty etapu z pieczątkami (podpis, mapa, uzgodnienie).</summary>
    public string[] Documents { get; private init; } = [];
    /// <summary>Samouczek menu: kroki na tytule i wyborze zawodu, nadawca dymków.</summary>
    public TutorialStep[] TutorialSteps { get; private init; } = [];
    /// <summary>Dymki przy pierwszym odblokowaniu; kolejność = TutorialUnlock.</summary>
    public TutorialStep[] TutorialUnlocks { get; private init; } = [];

    // v0.21.50 cz. 2: premie po etapie (#27), elity (#28), kombinacje stanów (#29)
    /// <summary>Rzadkości premii (zwykła, rzadka, legendarna) z wagami.</summary>
    public BoonRarityDef[] BoonRarities { get; private init; } = [];
    /// <summary>Nazwy znaczników premii (indeks = bit w BoonDef.Tags).</summary>
    public string[] BoonTags { get; private init; } = [];
    public BoonDef[] Boons { get; private init; } = [];
    public SynergyDef[] Synergies { get; private init; } = [];
    public int BoonRerollCost { get; private init; }
    /// <summary>Ile premii ze znacznikiem włącza synergię.</summary>
    public int SynergyAt { get; private init; } = 2;
    /// <summary>Szczęście: +waga rzadkich i legendarnych za punkt (tyle mniej zwykłych).</summary>
    public int BoonLuckRare { get; private init; }
    public int BoonLuckLegend { get; private init; }
    public EliteDef[] Elites { get; private init; } = [];
    /// <summary>Szansa na elitę wg aktu (GameData.Acts) i trudności, +za NG+.</summary>
    public int[] EliteActPct { get; private init; } = [];
    public int[] EliteDiffPct { get; private init; } = [];
    public int EliteTierPct { get; private init; }
    public int EliteHpPct { get; private init; } = 100;
    public int EliteDmg { get; private init; }
    public int EliteRespect { get; private init; }
    public int EliteMats { get; private init; }
    public int EliteGearMin { get; private init; }
    /// <summary>Kombinacje stanów (indeks = ComboEffect).</summary>
    public ComboDef[] Combos { get; private init; } = [];
    /// <summary>Jak grać: skąd stany (mokry, prąd, pył, zamróz).</summary>
    public string[] ComboSources { get; private init; } = [];
    public int WetTurns { get; private init; } = 3;
    public int HeroWetTurns { get; private init; } = 2;

    public int MaxHeroLevel => LevelThresholds.Length + 1;
    public int GearSlotsCount => GearSlots.Length;
    public int GearTraitsCount => GearTraits.Length;

    /// <summary>Indeks wroga po identyfikatorze (odpowiednik data::enemy_*), -1 gdy brak.</summary>
    public int EnemyIndex(string id) => Array.FindIndex(Enemies, e => e.Id == id);

    /// <summary>Indeks odznaki po identyfikatorze (odpowiednik data::badge_*), -1 gdy brak.</summary>
    public int BadgeIndex(string id) => Array.FindIndex(Badges, b => b.Id == id);

    public int BadgeBezUsterek { get; private init; } = -1;
    public int BadgePrzedTerminem { get; private init; } = -1;
    public int BadgeSeryjny { get; private init; } = -1;
    public int BadgeZawodowiec { get; private init; } = -1;
    public int BadgeTwardziel { get; private init; } = -1;
    public int BadgePelnyZespol { get; private init; } = -1;
    public int BadgeKolekcjoner { get; private init; } = -1;
    public int BadgeKatalog { get; private init; } = -1;
    public int BadgeOsiedle { get; private init; } = -1;

    public static GameData LoadFile(string path) => Parse(File.ReadAllText(path, System.Text.Encoding.UTF8));

    public static GameData Parse(string json)
    {
        using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
        return FromJson(doc.RootElement);
    }

    private static GameData FromJson(JsonElement d)
    {
        var weaponsJson = d.GetProperty("weapons").EnumerateArray().ToArray();
        var wid = Index(weaponsJson);
        var enemiesJson = d.GetProperty("enemies").EnumerateArray().ToArray();
        var eid = Index(enemiesJson);
        var hasMaterials = d.TryGetProperty("materials", out var matj);
        var materialsJson = hasMaterials ? matj.GetProperty("list").EnumerateArray().ToArray() : [];
        var mid = Index(materialsJson);

        var weapons = weaponsJson.Select(w => new WeaponDef(
            Str(w, "id"), Str(w, "name"), Int(w, "minDamage"), Int(w, "maxDamage"), Int(w, "range"),
            ParseStat(Str(w, "scalesWith")), ParseElement(Str(w, "element", "none")), Int(w, "crit", 0), Bool(w, "knockback"),
            Bool(w, "reveal"))).ToArray();
        foreach (var w in weapons)
        {
            Require(w.MinDamage <= w.MaxDamage && w.Range >= 1, $"broń {w.Id}: złe obrażenia/zasięg");
        }

        var classes = d.GetProperty("classes").EnumerateArray().Select(c =>
        {
            var ab = c.GetProperty("ability");
            return new ClassDef(Str(c, "id"), Str(c, "name"), Str(c, "desc"), Int(c, "maxHealth"), Int(c, "strength"),
                Int(c, "agility"), Int(c, "intelligence"), Int(c, "defense"), Int(c, "luck", 0), Lookup(wid, Str(c, "weapon"), "broń"),
                Int(c, "frame"), Str(ab, "name"), Str(ab, "desc"), ParseEnum<AbilityEffect>(Str(ab, "effect")),
                Int(ab, "cooldown"), ParseEnum<ClassPassive>(Str(c, "passive", "none")), Bool(c, "reward"), Bool(c, "secret"));
        }).ToArray();

        var enemies = enemiesJson.Select(e =>
        {
            var hasHit = e.TryGetProperty("onHit", out var hit);
            var hasSummon = e.TryGetProperty("summon", out var sm);
            var hasReward = e.TryGetProperty("reward", out var rw);
            return new EnemyDef(Str(e, "id"), Str(e, "name"), Str(e, "desc"), Int(e, "maxHealth"), Int(e, "minDamage"),
                Int(e, "maxDamage"), Int(e, "defense"), Int(e, "sight"), Int(e, "score"), Int(e, "frame"),
                e.TryGetProperty("slam", out var slam) && slam.GetBoolean(),
                hasHit && hit.TryGetProperty("status", out var s) ? ParseEnum<StatusEffect>(s.GetString() ?? "none") : StatusEffect.None,
                hasHit ? Int(hit, "chancePct", 0) : 0,
                hasHit ? Int(hit, "turns", 0) : 0,
                ParseEnum<SlamShape>(Str(e, "slamShape", "square")),
                Str(e, "slamName", ""),
                hasSummon ? Lookup(eid, Str(sm, "enemy"), "wezwany wróg") : -1,
                hasSummon ? Int(sm, "every") : 0,
                hasSummon ? Int(sm, "max") : 0,
                Int(e, "gearStun", 0),
                hasReward ? Int(rw, "cash", 0) : 0,
                hasReward ? Str(rw, "title", "") : "",
                e.TryGetProperty("material", out var em) ? Lookup(mid, em.GetString() ?? "", "materiał") : -1,
                BehaviorTags(e),
                e.TryGetProperty("phase", out var ph) ? Int(ph, "atPct") : 0,
                e.TryGetProperty("phase", out var ph2) ? Int(ph2, "healPct") : 0,
                e.TryGetProperty("phase", out var ph3) ? Int(ph3, "summon", 0) : 0,
                e.TryGetProperty("phase", out var ph4) ? Str(ph4, "name") : "",
                ParseElement(Str(e, "element", "none")),
                Str(e, "gender", "m") switch { "f" => 1, "n" => 2, _ => 0 });
        }).ToArray();
        foreach (var e in enemies)
        {
            Require(e.PhasePct == 0 || (e.Slam && e.PhasePct is >= 10 and <= 90 && e.PhaseSummon <= e.SummonMax), $"wróg {e.Id}: zła druga faza");
            Require(e.Summon < 0 || (!enemies[e.Summon].Slam && e.SummonEvery > 0 && e.SummonMax is > 0 and <= 3),
                $"wróg {e.Id}: złe wezwania");
        }

        // v0.21.52 cz. d (#47): mapa kariery - Stages = etapy Domu jednorodzinnego (kontrakt 0), potem etapy kolejnych kontraktów
        // (career.list[].stages); kontrakt = pierwszy etap + liczba (jak gen_data.py).
        var careerJson = d.TryGetProperty("career", out var carj) ? carj.GetProperty("list").EnumerateArray().ToArray() : [];
        var domJson = d.GetProperty("stages").EnumerateArray().ToArray();
        var stagesJson = domJson.Concat(careerJson.Skip(1).SelectMany(c => c.GetProperty("stages").EnumerateArray())).ToArray();
        var actsJson = d.GetProperty("acts").EnumerateArray().ToArray();
        var preludeN = domJson.Count(st => Bool(actsJson[Int(st, "act")], "prelude"));
        var stages = stagesJson.Select((st, i) =>
        {
            var pool = st.GetProperty("enemies").EnumerateArray().Select(x => Lookup(eid, x.GetString() ?? "", "wróg")).ToArray();
            Require(pool.Length is >= 1 and <= 4, "etap: 1-4 rodzaje wrogów");
            var boss = st.TryGetProperty("boss", out var b) ? Lookup(eid, b.GetString() ?? "", "boss") : -1;
            var act = Int(st, "act");
            var look = i < domJson.Length ? i : Int(st, "look");
            var tiles = st.TryGetProperty("tiles", out var tj) ? tj.GetInt32() : (i < preludeN ? 3 + i : act);
            return new StageDef(Str(st, "name"), pool, Int(st, "count"), boss, Int(st, "hpPct", 100), Int(st, "dmgBonus", 0), act,
                Int(st, "cost", 0), look, tiles, Bool(st, "twin"));
        }).ToArray();
        Require(stages.Length <= 64, "etapy: maks. 64 (maska pogody)");
        var paths = d.TryGetProperty("paths", out var pj)
            ? pj.GetProperty("list").EnumerateArray().Select(x => new PathDef(Str(x, "id"), Str(x, "name"), Str(x, "short"), Str(x, "desc"),
                Int(x, "enemies", 0), Int(x, "pickups", 0), Int(x, "cash", 0), Int(x, "materials", 0),
                x.TryGetProperty("badWeather", out var bw) && bw.GetBoolean(), x.TryGetProperty("noEvent", out var ne) && ne.GetBoolean())).ToArray()
            : [];
        Require(paths.Length == 0 || paths.Length is >= 2 and <= 8, "ścieżki: 2-8 wariantów");
        var pathMore = Math.Max(0, paths.Length > 0 ? paths.Max(x => x.Enemies) : 0);
        foreach (var st in stages)
        {
            // boss z wezwaniami: etap + ścieżka + boss + wezwani mieszczą się w Game.MaxEnemies
            Require(st.Boss < 0 || st.EnemyCount + pathMore + 1 + enemies[st.Boss].SummonMax <= 12, $"etap {st.Name}: za dużo wrogów z wezwanymi");
        }

        var difficulties = d.GetProperty("difficulties").EnumerateArray().Select(x => new DifficultyDef(
            Str(x, "id", ""), Str(x, "name"), Int(x, "hpPct"), Int(x, "dmgBonus"), Int(x, "scorePct"))).ToArray();

        var meta = d.GetProperty("meta");
        // v0.21.52: poziomy Szkoleń (steps: działanie, przyrost, koszt), stare koszty do zwrotu przy migracji profilu v12
        var upgrades = meta.GetProperty("upgrades").EnumerateArray().Select(u => new UpgradeDef(
            Str(u, "id", ""), Str(u, "name"), Str(u, "desc"), ParseUpgrade(Str(u, "effect")),
            u.GetProperty("steps").EnumerateArray().Select(x => new UpgradeStep(ParseUpgrade(Str(x, "effect")), Int(x, "value"), Int(x, "cost"))).ToArray(),
            u.TryGetProperty("legacyCosts", out var lc) ? lc.EnumerateArray().Select(c => c.GetInt32()).ToArray() : [],
            Int(u, "refund", 0))).ToArray();
        foreach (var u in upgrades)
        {
            Require(u.Levels is >= 1 and <= 5 && u.LegacyCosts.Length <= 4, $"ulepszenie {u.Name}: 1-5 poziomów");
        }
        var classCosts = meta.GetProperty("classCosts").EnumerateArray().Select(c => c.GetInt32()).ToArray();
        var toolCosts = meta.GetProperty("toolCosts").EnumerateArray().Select(c => c.GetInt32()).ToArray();
        Require(classCosts.Length >= 1 && toolCosts.Length >= 1, "ceny zawodów i narzędzi");

        var classesJson = d.GetProperty("classes").EnumerateArray().ToArray();
        var cid = Index(classesJson);
        var tools = meta.GetProperty("tools").EnumerateArray().Select(t => new ToolDef(Lookup(wid, Str(t, "weapon"), "narzędzie"), Bool(t, "shop"),
            Bool(t, "reward"), Bool(t, "secret"))).ToArray();
        Require(tools.Count(t => t.Shop) == toolCosts.Length, "ceny narzędzi: tyle, ile narzędzi na sprzedaż");
        Require(tools.Length <= 12 && tools.Select((t, i) => t.Secret || i < 8).All(x => x), "narzędzia: bitmaska uint8 w profilu (sekretne za nią)");

        var story = d.GetProperty("story");
        var storyStages = story.GetProperty("stages").EnumerateArray().Select(Story)
            .Concat(stagesJson.Skip(domJson.Length).Select(st => Story(st.GetProperty("story")))).ToArray();
        Require(storyStages.Length == stages.Length, "fabuła: tyle wiadomości, ile etapów");

        var acts = d.GetProperty("acts").EnumerateArray().Select(a => a.TryGetProperty("mechanic", out var mc)
            ? new ActDef(Str(a, "name"), Int(a, "bonusPerStage"), Int(a, "bonusPerKill"), ParseEnum<ActMechanic>(Str(mc, "effect")), Int(mc, "value", 0),
                Str(mc, "name", ""), Str(mc, "short", ""), Str(mc, "info", ""), Str(a, "numeral", ""), Bool(a, "prelude"))
            : new ActDef(Str(a, "name"), Int(a, "bonusPerStage"), Int(a, "bonusPerKill"), Numeral: Str(a, "numeral", ""), Prelude: Bool(a, "prelude"))).ToArray();
        var domStages = stages.Take(domJson.Length).ToArray();
        for (var ai = 0; ai < acts.Length; ai++)
        {
            var last = Array.FindLastIndex(domStages, s => s.Act == ai);
            Require(last >= 0 && domStages[last].Boss >= 0, $"akt {ai} bez bossa");
        }
        // Akt wstępny (Akt 0): jego etapy na początku listy, za nimi etapy budowy.
        var preludeStages = domStages.Count(st => acts[st.Act].Prelude);
        for (var i = 0; i < domStages.Length; i++) Require(acts[domStages[i].Act].Prelude == i < preludeStages, "Akt 0: etapy na początku listy");
        var documents = d.TryGetProperty("documents", out var docj) ? docj.EnumerateArray().Select(x => x.GetString() ?? "").ToArray() : [];
        Require(documents.Length <= 4, "pieczątki: maks. 4 dokumenty");
        foreach (var a in acts) Require(a.Mechanic != ActMechanic.Stamps || a.MechValue == documents.Length, "pieczątki: tyle dokumentów, ile w mechanice");

        var hurtownia = d.GetProperty("hurtownia").EnumerateArray().Select(it => new ShopItemDef(
            Str(it, "id", ""), Str(it, "name"), Str(it, "desc"), Int(it, "price"), ParseEnum<ShopEffect>(Str(it, "effect")),
            it.TryGetProperty("material", out var im) ? Lookup(mid, im.GetString() ?? "", "materiał") : -1, Int(it, "matCost", 0))).ToArray();
        var materials = materialsJson.Select(x => new MaterialDef(Str(x, "id"), Str(x, "name"), Str(x, "short"))).ToArray();
        Require(materials.Length is 0 or 3, "materiały: 3 rodzaje");
        var repairs = hasMaterials && matj.TryGetProperty("repairs", out var rj)
            ? rj.EnumerateArray().Select(x => new RepairDef(Str(x, "id"), Str(x, "name"), Str(x, "desc"), Str(x, "info"),
                ParseEnum<RepairEffect>(Str(x, "effect")), Lookup(mid, Str(x, "material"), "materiał"), Int(x, "cost"), Int(x, "value"))).ToArray()
            : [];

        var badgesJson = d.GetProperty("badges").EnumerateArray().ToArray();
        // v0.21.52: tytuł i wygląd (kolor kasku) z odznak i zleceń
        var cosmeticIds = d.TryGetProperty("secrets", out var sec0) ? Index(sec0.GetProperty("cosmetics").EnumerateArray().ToArray()) : new Dictionary<string, int>();
        int CosmeticOf(JsonElement x) => x.TryGetProperty("cosmetic", out var xc) ? Lookup(cosmeticIds, xc.GetString() ?? "", "wygląd") : -1;
        var badges = badgesJson.Select(b => new BadgeDef(Str(b, "id"), Str(b, "name"), Str(b, "desc"), Int(b, "xp"),
            b.TryGetProperty("perk", out var pk) ? new Perk(ParsePerk(Str(pk, "effect")), Int(pk, "value")) : new Perk(PerkEffect.Unknown, 0),
            Str(b, "title", ""), CosmeticOf(b))).ToArray();
        Require(badges.Length <= 16 && enemies.Length <= Game.MaxEnemyTypes, "maks. 16 odznak i 48 rodzajów wrogów");
        var bid = Index(badgesJson);
        // pamiątki i zlecenia (od v0.21.43; starsze dane – puste listy)
        var keepsakesJson = d.TryGetProperty("keepsakes", out var ksj) ? ksj.GetProperty("list").EnumerateArray().ToArray() : [];
        var kid = Index(keepsakesJson);
        var contractsJson = d.TryGetProperty("contracts", out var cj) ? cj.EnumerateArray().ToArray() : [];
        var contracts = contractsJson.Select(c => new ContractDef(Str(c, "id"), Str(c, "name"), Str(c, "desc"), ParseContract(Str(c, "kind")),
            Int(c, "target"), Int(c, "xp"), c.TryGetProperty("keepsake", out var ck) ? Lookup(kid, ck.GetString() ?? "", "pamiątka") : -1,
            Str(c, "title", ""), CosmeticOf(c))).ToArray();
        foreach (var c in contracts) Require(c.Target is > 0 and < 30000, $"zlecenie {c.Id}: zły cel");
        var keepsakes = keepsakesJson.Select(k => new KeepsakeDef(Str(k, "id"), Str(k, "name"), Str(k, "desc"), ParsePerk(Str(k, "effect")),
            k.GetProperty("values").EnumerateArray().Select(v => v.GetInt32()).ToArray(),
            k.TryGetProperty("badge", out var kb) ? Lookup(bid, kb.GetString() ?? "", "odznaka") : -1,
            k.TryGetProperty("start", out var kst) && kst.GetBoolean(), Int(k, "streak", 0))).ToArray();
        for (var k = 0; k < keepsakes.Length; k++)
        {
            Require(keepsakes[k].Values.Length == 3, $"pamiątka {keepsakes[k].Id}: 3 rangi");
            var kk = k;
            Require(keepsakes[k].Start || keepsakes[k].Badge >= 0 || keepsakes[k].Streak > 0 || contracts.Any(c => c.Keepsake == kk),
                $"pamiątka {keepsakes[k].Id} bez sposobu odblokowania");
        }
        Require(contracts.Length <= 8 && keepsakes.Length <= 8, "maks. 8 zleceń i 8 pamiątek");
        var rankRuns = ksj.ValueKind == JsonValueKind.Object ? ksj.GetProperty("rankRuns").EnumerateArray().Select(x => x.GetInt32()).ToArray() : new[] { 3, 8 };
        Require(rankRuns.Length == 2 && rankRuns[0] < rankRuns[1], "pamiątki: 2 progi rang");
        var siteEvents = Array.Empty<SiteEventDef>();
        var siteEventChance = 0;
        if (d.TryGetProperty("siteEvents", out var sej))
        {
            siteEventChance = Int(sej, "chancePct");
            siteEvents = sej.GetProperty("list").EnumerateArray().Select(e => new SiteEventDef(Str(e, "id"), Str(e, "name"), Str(e, "short"),
                Str(e, "info"), Story(e), ParseEvent(Str(e, "effect")), Int(e, "value"), e.GetProperty("good").GetBoolean())).ToArray();
        }

        var allStages = stages.Length >= 64 ? ulong.MaxValue : (1UL << stages.Length) - 1;
        // etapy kontraktów: pogoda jak na etapie Domu, który przypominają ("like"; bez - każda)
        ulong WeatherMask(JsonElement w)
        {
            if (!w.TryGetProperty("stages", out var ws)) return allStages;
            var allowed = ws.EnumerateArray().Select(x => x.GetInt32()).ToArray();
            var m = allowed.Aggregate(0UL, (acc, x) => acc | 1UL << x);
            for (var i = domJson.Length; i < stagesJson.Length; i++)
                if (!stagesJson[i].TryGetProperty("like", out var lk) || allowed.Contains(lk.GetInt32())) m |= 1UL << i;
            return m;
        }
        WeatherDef[] weather = [new WeatherDef("slonce", "Słonecznie", "Pogodnie", "", WeatherEffect.None, 0, 1, false, allStages)];
        var weatherNoBadStack = false;
        if (d.TryGetProperty("weather", out var wj))
        {
            weatherNoBadStack = wj.TryGetProperty("noBadStack", out var nb) && nb.GetBoolean();
            weather = wj.GetProperty("list").EnumerateArray().Select(w => new WeatherDef(Str(w, "id"), Str(w, "name"), Str(w, "short"),
                Str(w, "info"), ParseEnum<WeatherEffect>(Str(w, "effect")), Int(w, "value"), Int(w, "weight"), w.GetProperty("bad").GetBoolean(),
                WeatherMask(w))).ToArray();
            Require(weather.Length >= 1 && weather[0].Effect == WeatherEffect.None, "pogoda: pierwsza bez skutku");
            foreach (var w in weather)
            {
                Require(w.Weight is > 0 and < 128 && (w.Effect is not (WeatherEffect.Frost or WeatherEffect.Rain) || w.Value >= 2), $"pogoda {w.Id}: zła waga/wartość");
            }
            for (var si = 0; si < stages.Length; si++)
            {
                var bit = 1UL << si;
                Require(weather.Any(w => (w.StagesMask & bit) != 0), $"etap {si}: brak pogody do wylosowania");
            }
        }

        var brigade = d.TryGetProperty("brigade", out var bj)
            ? bj.GetProperty("list").EnumerateArray().Select(h => new HelperDef(Str(h, "id"), Str(h, "name"), Str(h, "desc"),
                ParseEnum<HelperEffect>(Str(h, "effect")), Int(h, "value"), Int(h, "turns"), Int(h, "reach", 0), Int(h, "price"),
                Int(h, "cost"), Int(h, "frame", -1))).ToArray()
            : [];
        Require(brigade.Length <= 8, "brygada: maks. 8 fachowców");
        foreach (var h in brigade)
        {
            Require(h.Price > 0 && (h.Effect != HelperEffect.Pump || h.Reach >= 1) && (h.Effect != HelperEffect.Ally || (h.Turns > 0 && h.Frame >= 0)),
                $"brygada {h.Id}: złe dane");
        }
        var startHelpers = 0;
        for (var i = 0; i < brigade.Length; i++)
        {
            if (brigade[i].Cost == 0) startHelpers |= 1 << i;
        }

        var investor = d.TryGetProperty("investor", out var ij)
            ? ij.GetProperty("list").EnumerateArray().Select(x => new InvestorDef(Str(x, "id"), Str(x, "name"), Str(x, "desc"),
                ParseInvestor(Str(x, "effect")), Int(x, "value"), Int(x, "xpPct"), Int(x, "stake"))).ToArray()
            : [];
        Require(investor.Length <= 8, "tryb inwestora: maks. 8 modyfikatorów");

        var drops = d.GetProperty("drops");
        var weights = drops.GetProperty("weights");
        var eq = d.GetProperty("equipment");
        var rarities = eq.GetProperty("rarities").EnumerateArray().Select(r => r.GetString() ?? "").ToArray();
        var gear = new List<GearDef>();
        var slots = new List<string>();
        int gearReward = 0, gearBase = 0;
        foreach (var sl in eq.GetProperty("slots").EnumerateArray())
        {
            if (Bool(sl, "reward")) gearReward |= 1 << slots.Count;
            else gearBase |= 1 << slots.Count;
            slots.Add(Str(sl, "name"));
            var gs = ParseEnum<GearStat>(Str(sl, "stat"));
            var items = sl.GetProperty("items").EnumerateArray().ToArray();
            Require(items.Length == 3 && rarities.Length == 3, "sprzęt: 3 jakości w slocie");
            foreach (var item in items)
            {
                var arr = item.EnumerateArray().ToArray();
                gear.Add(new GearDef(arr[0].GetString() ?? "", gs, arr[1].GetInt32()));
            }
        }
        var rr = eq.GetProperty("rarityRoll");
        var traits = eq.GetProperty("traits").EnumerateArray().Select(t => new TraitDef(Str(t, "id", ""), Str(t, "name"), Str(t, "short"),
            ParseTrait(Str(t, "effect")), Int(t, "value"))).ToArray();
        Require(traits.Length >= 1, "sprzęt: co najmniej jedna cecha");
        var stt = d.GetProperty("statuses");
        StatusDef StatusOf(string k)
        {
            var x = stt.GetProperty(k);
            return new StatusDef(Str(x, "name"), Str(x, "short"), Str(x, "effect"));
        }
        var lk = d.GetProperty("luck");
        var th = d.GetProperty("thermos");
        var hl = d.GetProperty("heroLevels");
        var ng = d.GetProperty("newGamePlus");
        var slamJson = d.GetProperty("slam");

        Require(slots.Count is >= 3 and <= Game.MaxGearSlots, "sprzęt: 3-6 slotów");
        var startTools = 0;
        for (var i = 0; i < tools.Length; i++)
        {
            if (!tools[i].Shop && !tools[i].Reward && !tools[i].Secret) startTools |= 1 << i;
        }
        int rewardClasses = 0, secretClasses = 0, secretTools = 0;
        for (var i = 0; i < classes.Length; i++)
        {
            if (classes[i].Reward) rewardClasses |= 1 << i;
            else if (classes[i].Secret) secretClasses |= 1 << i;
            else Require(i < 8, "zawody do kupienia: bitmaska uint8 w profilu");
        }
        Require(classes.Length <= 12, "maks. 12 zawodów");
        var openClasses = classes.Count(c => !c.Secret);
        Require(classes.Skip(openClasses).All(c => c.Secret), "zawody z sekretnych zleceń na końcu listy");
        for (var i = 0; i < tools.Length; i++)
        {
            if (tools[i].Secret) secretTools |= 1 << i;
        }

        var hasSecrets = d.TryGetProperty("secrets", out var secj);
        var secretsJson = hasSecrets ? secj.GetProperty("list").EnumerateArray().ToArray() : [];
        var sid = Index(secretsJson);
        var hasRespect = d.TryGetProperty("respect", out var rsj);
        var respect = hasRespect
            ? rsj.GetProperty("upgrades").EnumerateArray().Select(x => new RespectDef(Str(x, "id"), Str(x, "name"), Str(x, "desc"),
                ParseRespect(Str(x, "effect")), x.GetProperty("values").EnumerateArray().Select(v => v.GetInt32()).ToArray(),
                x.GetProperty("costs").EnumerateArray().Select(v => v.GetInt32()).ToArray(),
                x.TryGetProperty("secret", out var rsec) ? Lookup(sid, rsec.GetString() ?? "", "sekretne zlecenie Respektu") : -1)).ToArray()
            : [];
        Require(respect.Length <= 19, "Respekt: maks. 19 ulepszeń (16 + 3 w profilu v12)");
        foreach (var x in respect) Require(x.Values.Length is >= 1 and <= 5 && x.Values.Length == x.Costs.Length, $"Respekt {x.Id}: 1-5 rang");
        var rewards = new List<RewardDef>();
        if (d.TryGetProperty("rewards", out var rwj))
        {
            var tid = new Dictionary<string, int>();
            var tj = meta.GetProperty("tools").EnumerateArray().ToArray();
            for (var i = 0; i < tj.Length; i++) tid[Str(tj[i], "weapon")] = i;
            foreach (var x in rwj.GetProperty("list").EnumerateArray())
            {
                var kind = Str(x, "kind");
                var id = Str(x, "id");
                int idx;
                string name;
                RewardKind rk;
                switch (kind)
                {
                    case "tool":
                        rk = RewardKind.Tool;
                        idx = Lookup(tid, id, "narzędzie nagrody");
                        name = weapons[tools[idx].Weapon].Name;
                        break;
                    case "gear":
                        rk = RewardKind.Gear;
                        idx = slots.IndexOf(id);
                        Require(idx >= 0, $"nagroda: nieznany slot {id}");
                        name = slots[idx];
                        break;
                    case "class":
                        rk = RewardKind.Cls;
                        idx = Lookup(cid, id, "zawód nagrody");
                        name = classes[idx].Name;
                        break;
                    case "act":
                        rk = RewardKind.Act;
                        idx = Array.FindIndex(acts, a => a.Prelude);
                        Require(idx >= 0, "nagroda: brak aktu wstępnego");
                        name = acts[idx].Name;
                        break;
                    default:
                        rk = RewardKind.Soon;
                        idx = -1;
                        name = "";
                        break;
                }
                rewards.Add(new RewardDef(rk, idx, Str(x, "name", name), Str(x, "desc")));
            }
        }
        var startClasses = 0;
        foreach (var c in meta.GetProperty("startClasses").EnumerateArray()) startClasses |= 1 << Lookup(cid, c.GetString() ?? "", "zawód");

        int BadgeIdx(string id) => Array.FindIndex(badges, b => b.Id == id);
        var hasDaily = d.TryGetProperty("daily", out var dj);
        var hasBp = d.TryGetProperty("behaviorParams", out var bpj);
        var behaviorNames = d.TryGetProperty("behaviorNames", out var bnj)
            ? Behavior.Ids.Select(id => Str(bnj, id, id)).ToArray()
            : Behavior.Ids;
        var hasSchedule = d.TryGetProperty("schedule", out var scj);
        var difficultiesJson = d.GetProperty("difficulties").EnumerateArray().ToArray();
        TutorialStep[] tutSteps = [], tutUnlocks = [];
        if (d.TryGetProperty("tutorial", out var tuj))
        {
            var from = Str(tuj, "from");
            TutorialStep Tut(JsonElement x)
            {
                var lines = Str(x, "text").Split('|').ToList();
                while (lines.Count < 3) lines.Add("");
                return new TutorialStep(Str(x, "id"), Str(x, "title"), new StoryMsg(from, lines.ToArray()), Str(x, "gba", ""),
                    Str(x, "screen") == "class" ? 1 : 0, Bool(x, "godotOnly"), Str(x, "requires", "") == "investor", Str(x, "link", ""));
            }
            tutSteps = tuj.GetProperty("steps").EnumerateArray().Select(Tut).ToArray();
            tutUnlocks = tuj.GetProperty("unlocks").EnumerateArray().Select(Tut).ToArray();
            var unlockIds = tutUnlocks.Select(x => x.Id).ToArray();
            Require(unlockIds.SequenceEqual(new[] { "respect", "daily", "investor", "act0", "class" })
                    || unlockIds.SequenceEqual(new[] { "respect", "daily", "investor", "act0", "class", "secret" }), "samouczek: 5-6 dymków odblokowań");
        }
        // v0.21.51 cz. 2: sekretne zlecenia (#39) – nagroda: zawód, narzędzie, wygląd, ranga Respektu
        var cosmeticsJson = hasSecrets ? secj.GetProperty("cosmetics").EnumerateArray().ToArray() : [];
        var cosmetics = cosmeticsJson.Select(x => new CosmeticDef(Str(x, "id"), Str(x, "name"), Str(x, "desc"),
            x.TryGetProperty("helmet", out var hj) ? hj.EnumerateArray().Aggregate(0, (acc, v) => (acc << 8) | v.GetInt32()) : -1)).ToArray();
        var coid = Index(cosmeticsJson);
        var rsid = new Dictionary<string, int>();
        for (var i = 0; i < respect.Length; i++) rsid[respect[i].Id] = i;
        var tid2 = new Dictionary<string, int>();
        var toolsJson = meta.GetProperty("tools").EnumerateArray().ToArray();
        for (var i = 0; i < toolsJson.Length; i++) tid2[Str(toolsJson[i], "weapon")] = i;
        var tutFrom = d.TryGetProperty("tutorial", out var tfj) ? Str(tfj, "from") : "";
        var secrets = secretsJson.Select(x =>
        {
            var kind = ParseSnake<SecretKind>(Str(x, "kind"));
            var rw = x.GetProperty("reward");
            var rk = Str(rw, "kind");
            var id = Str(rw, "id");
            var (reward, idx) = rk switch
            {
                "class" => (SecretReward.Cls, Lookup(cid, id, "zawód sekretu")),
                "tool" => (SecretReward.Tool, Lookup(tid2, id, "narzędzie sekretu")),
                "cosmetic" => (SecretReward.Cosmetic, Lookup(coid, id, "wygląd sekretu")),
                _ => (SecretReward.Respect, Lookup(rsid, id, "Respekt sekretu")),
            };
            var value = kind == SecretKind.HelperBoss ? Lookup(eid, Str(x, "enemy"), "boss sekretu") : Int(x, "value", 0);
            var lines = Str(x, "news").Split('|').ToList();
            while (lines.Count < 3) lines.Add("");
            return new SecretDef(Str(x, "id"), Str(x, "hint"), Str(x, "desc"), kind, value, reward, idx, Str(x, "rewardText"),
                new StoryMsg(tutFrom, lines.ToArray()));
        }).ToArray();
        Require(secrets.Length <= 16 && cosmetics.Length <= 24 && cosmetics.Skip(8).All(x => x.IsHelmet),
            "sekrety: maks. 16 zleceń i 24 wyglądy (przełączniki tylko wśród pierwszych 8)");
        var paperMask = 0UL;
        if (hasSecrets)
        {
            foreach (var pe in secj.GetProperty("paper").EnumerateArray()) paperMask |= 1UL << Lookup(eid, pe.GetString() ?? "", "problem papierowy");
        }
        var helperBoss = secrets.FirstOrDefault(x => x.Kind == SecretKind.HelperBoss)?.Value ?? -1;

        // v0.21.50 cz. 2: premie po etapie (#27), elity (#28), kombinacje stanów (#29); bez sekcji – puste listy
        BoonRarityDef[] boonRarities = [];
        string[] boonTagNames = [];
        BoonDef[] boons = [];
        SynergyDef[] synergies = [];
        int rerollCost = 0, synergyAt = 2, luckRare = 0, luckLegend = 0;
        if (d.TryGetProperty("boons", out var boj))
        {
            boonRarities = boj.GetProperty("rarities").EnumerateArray().Select(x => new BoonRarityDef(Str(x, "id"), Str(x, "name"), Int(x, "weight"))).ToArray();
            var rarId = boonRarities.Select(x => x.Id).ToList();
            var tagIds = boj.GetProperty("tags").EnumerateArray().Select(x => Str(x, "id")).ToList();
            boonTagNames = boj.GetProperty("tags").EnumerateArray().Select(x => Str(x, "name")).ToArray();
            int TagMask(JsonElement x) => x.GetProperty("tags").EnumerateArray().Aggregate(0, (m, t) =>
            {
                var i = tagIds.IndexOf(t.GetString() ?? "");
                Require(i >= 0, $"premia: nieznany znacznik {t.GetString()}");
                return m | 1 << i;
            });
            boons = boj.GetProperty("list").EnumerateArray().Select(x => new BoonDef(Str(x, "id"), Str(x, "name"), Str(x, "desc"),
                rarId.IndexOf(Str(x, "rarity")), TagMask(x), ParseSnakeOr(Str(x, "effect"), BoonEffect.Unknown), Int(x, "value"),
                x.TryGetProperty("class", out var bc) ? Lookup(cid, bc.GetString() ?? "", "zawód premii") : -1, Bool(x, "mastery"))).ToArray();
            synergies = boj.GetProperty("synergies").EnumerateArray().Select(x => new SynergyDef(Str(x, "id"), Str(x, "name"), Str(x, "desc"),
                TagMask(x), ParseSnake<SynergyEffect>(Str(x, "effect")), Int(x, "value"))).ToArray();
            Require(boonRarities.Length == 3 && boons.Length <= 64 && boons.All(b => b.Rarity >= 0), "premie: 3 rzadkości, maks. 64 premie");
            rerollCost = Int(boj, "rerollCost");
            synergyAt = Int(boj, "synergyAt", 2);
            luckRare = Int(boj, "luckRare", 0);
            luckLegend = Int(boj, "luckLegend", 0);
        }
        EliteDef[] elites = [];
        int[] eliteActPct = new int[acts.Length], eliteDiffPct = new int[difficulties.Length];
        int eliteTierPct = 0, eliteHpPct = 100, eliteDmg = 0, eliteRespect = 0, eliteMats = 0, eliteGearMin = 0;
        if (d.TryGetProperty("elites", out var elj))
        {
            elites = elj.GetProperty("traits").EnumerateArray().Select(x => new EliteDef(Str(x, "id"), Str(x, "name"),
                x.GetProperty("prefix").EnumerateArray().Select(p => p.GetString() ?? "").ToArray(), Str(x, "info"),
                ParseSnake<EliteEffect>(Str(x, "effect")), Int(x, "value"))).ToArray();
            eliteActPct = elj.GetProperty("actPct").EnumerateArray().Select(x => x.GetInt32()).ToArray();
            eliteDiffPct = elj.GetProperty("diffPct").EnumerateArray().Select(x => x.GetInt32()).ToArray();
            Require(eliteActPct.Length == acts.Length && eliteDiffPct.Length == difficulties.Length && elites.All(e => e.Prefix.Length == 3), "elity: złe dane");
            eliteTierPct = Int(elj, "tierPct");
            eliteHpPct = Int(elj, "hpPct");
            eliteDmg = Int(elj, "dmg");
            var erw = elj.GetProperty("reward");
            eliteRespect = Int(erw, "respect");
            eliteMats = Int(erw, "mats");
            eliteGearMin = Int(erw, "gearMin");
        }
        // v0.21.50 cz. 3: wydarzenia z wyborem, ulepszanie narzędzia, ukryte pomieszczenia
        var slotNames = slots.ToArray();
        ChoiceEventDef[] choiceEvents = [];
        var choiceChance = 0;
        if (d.TryGetProperty("choiceEvents", out var cej))
        {
            choiceChance = Int(cej, "chancePct");
            choiceEvents = cej.GetProperty("list").EnumerateArray().Select(e => new ChoiceEventDef(Str(e, "id"), Str(e, "name"), Story(e),
                e.GetProperty("choices").EnumerateArray().Select(c => new EventChoice(Str(c, "label"), Str(c, "result"),
                    c.GetProperty("effects").EnumerateArray().Select(x => ParseChoiceOut(x, mid, eid, slotNames)).ToArray())).ToArray())).ToArray();
            Require(choiceEvents.Length <= 16 && choiceEvents.All(e => e.Choices.Length is >= 2 and <= 3 && e.Choices.All(c => c.Outs.Length <= 3)),
                "wydarzenia: maks. 16, 2-3 odpowiedzi, 0-3 skutki");
        }
        ToolLevelDef[] toolLevels = [];
        ToolTraitDef[] toolTraits = [];
        int toolMax = 0, toolDmg = 0, toolTraitAt = 0;
        if (d.TryGetProperty("toolUpgrade", out var tupj))
        {
            toolMax = Int(tupj, "max");
            toolDmg = Int(tupj, "dmg");
            toolTraitAt = Int(tupj, "traitAt");
            toolLevels = tupj.GetProperty("levels").EnumerateArray().Select(x => new ToolLevelDef(Int(x, "cash"),
                Lookup(mid, Str(x, "material"), "materiał"), Int(x, "count"))).ToArray();
            toolTraits = tupj.GetProperty("traits").EnumerateArray().Select(x => new ToolTraitDef(Str(x, "id"), Str(x, "name"), Str(x, "short"),
                Str(x, "desc"), ParseEnum<ToolTraitEffect>(Str(x, "effect")), Int(x, "value"))).ToArray();
            Require(toolLevels.Length == toolMax && toolTraits.Length == 3, "ulepszenie narzędzia: poziomy = maks., 3 cechy");
        }
        SecretKindDef[] secretKinds = [];
        int secretChance = 0, secretGuard = 0, chestRespect = 0, chestMats = 0, chestCash = 0, chestGearMin = 0;
        if (d.TryGetProperty("hiddenRooms", out var hrj))
        {
            secretChance = Int(hrj, "chancePct");
            secretGuard = Int(hrj, "guardPct");
            secretKinds = hrj.GetProperty("kinds").EnumerateArray().Select(x => new SecretKindDef(Str(x, "id"), Str(x, "name"), Str(x, "info"),
                Bool(x, "breakable"))).ToArray();
            var chj = hrj.GetProperty("chest");
            chestRespect = Int(chj, "respect");
            chestMats = Int(chj, "mats");
            chestCash = Int(chj, "cash");
            chestGearMin = Int(chj, "gearMin");
        }
        ComboDef[] combos = [];
        string[] comboSources = [];
        int wetTurns = 3, heroWetTurns = 2;
        if (d.TryGetProperty("combos", out var coj))
        {
            combos = coj.GetProperty("list").EnumerateArray().Select(x => new ComboDef(Str(x, "id"), Str(x, "name"), Str(x, "short"), Str(x, "info"),
                Str(x, "hero", ""), ParseSnake<ComboEffect>(Str(x, "effect")), Int(x, "value"), Int(x, "radius", 0), Int(x, "heroValue", 0))).ToArray();
            for (var i = 0; i < combos.Length; i++) Require((int)combos[i].Effect == i, "kombinacje: kolejność = ComboEffect");
            comboSources = coj.TryGetProperty("sources", out var csj) ? csj.EnumerateArray().Select(x => x.GetString() ?? "").ToArray() : [];
            wetTurns = Int(coj, "wetTurns");
            heroWetTurns = Int(coj, "heroWetTurns");
        }

        // v0.21.50 cz. 4: podsumowanie budowy (#33), wyzwania tygodnia (#34), fabuła odkrywana z budowami (#35)
        string[] recapKinds = [], recapVerbs = ["Pokonał", "Pokonała", "Pokonało"];
        RecapTipDef[] recapTips = [];
        if (d.TryGetProperty("recap", out var rcj))
        {
            recapKinds = rcj.GetProperty("kinds").EnumerateArray().Select(x => x.GetString() ?? "").ToArray();
            recapVerbs = rcj.GetProperty("verbs").EnumerateArray().Select(x => x.GetString() ?? "").ToArray();
            recapTips = rcj.GetProperty("tips").EnumerateArray().Select(x =>
            {
                var lines = Str(x, "text").Split('|').ToList();
                while (lines.Count < 2) lines.Add("");
                return new RecapTipDef(ParseSnake<RecapTip>(Str(x, "when")), lines.ToArray());
            }).ToArray();
            Require(recapKinds.Length == 6 && recapVerbs.Length == 3 && recapTips.Length > 0 && recapTips[^1].When == RecapTip.Any,
                "podsumowanie: 6 rodzajów ciosów, 3 czasowniki, ostatnia rada \"any\"");
        }
        WeeklyDef[] weekly = [];
        int[] weeklyEpoch = [2026, 1, 5];
        int weeklyDiff = 1, weeklyHistory = 3, weeklyCoffee = 0;
        if (d.TryGetProperty("weekly", out var wkj))
        {
            var weatherIds = weather.Select(w => w.Id).ToList();
            WeeklyRuleDef Rule(JsonElement x)
            {
                var r = Str(x, "rule");
                if (r == "class") return new WeeklyRuleDef(WeeklyRule.Cls, Lookup(cid, Str(x, "class"), "zawód tygodnia"));
                var rule = ParseSnake<WeeklyRule>(r);
                if (rule == WeeklyRule.Weather)
                {
                    var wi = weatherIds.IndexOf(Str(x, "weather"));
                    Require(wi >= 0, "tydzień: nieznana pogoda");
                    return new WeeklyRuleDef(rule, wi);
                }
                return new WeeklyRuleDef(rule, Int(x, "value", 0));
            }
            weekly = wkj.GetProperty("list").EnumerateArray().Select(x => new WeeklyDef(Str(x, "id"), Str(x, "name"), Str(x, "short"),
                Str(x, "desc").Split('|'), x.GetProperty("rules").EnumerateArray().Select(Rule).ToArray())).ToArray();
            weeklyEpoch = wkj.GetProperty("epoch").EnumerateArray().Select(x => x.GetInt32()).ToArray();
            weeklyDiff = Lookup(Index(difficultiesJson), Str(wkj, "difficulty"), "trudność tygodnia");
            weeklyHistory = Int(wkj, "history");
            weeklyCoffee = Int(wkj, "coffeeCash", 0);
            Require(weekly.Length is >= 1 and <= 16 && weekly.All(w => w.Rules.Length is >= 1 and <= 3 && w.Desc.Length == 2) && weeklyHistory is >= 1 and <= 3,
                "wyzwania tygodnia: 1-16, 1-3 zasady, opis w 2 liniach, historia 1-3");
        }
        StoryThread[] arc = [];
        if (story.TryGetProperty("arc", out var arj))
        {
            arc = arj.EnumerateArray().Select(x =>
            {
                var trig = ParseSnake<StoryTrigger>(Str(x, "trigger"));
                var v = trig == StoryTrigger.Boss ? Lookup(eid, Str(x, "value"), "boss wątku") : Int(x, "value");
                return new StoryThread(Str(x, "id"), Str(x, "name"), Str(x, "hint"), trig, v, x.GetProperty("messages").EnumerateArray().Select(Story).ToArray());
            }).ToArray();
            Require(arc.Length <= 32 && arc.All(t => t.Messages.Length is >= 1 and <= 2), "fabuła: maks. 32 wątki po 1-2 wiadomości");
        }
        var decor = d.TryGetProperty("estate", out var esj)
            ? esj.GetProperty("decor").EnumerateArray().Select(x => new DecorDef(Str(x, "id"), Str(x, "name"), Int(x, "wins", 0), Int(x, "inspector", 0))).ToArray()
            : [];
        // v0.21.52 cz. b: poziom inspektora (#44), mistrzostwo zawodu (#45), stopnie inwestora (#48)
        var arcIds = arc.Select((t, i) => (t.Id, i)).ToDictionary(x => x.Id, x => x.i);
        var decorIds = decor.Select((t, i) => (t.Id, i)).ToDictionary(x => x.Id, x => x.i);
        ProgressLevel Level(JsonElement x, int xp)
        {
            var reward = ParseSnake<ProgressReward>(Str(x, "reward"));
            var id = Str(x, "id", "");
            var index = reward switch
            {
                ProgressReward.Helmet => Lookup(coid, id, "kolor kasku"),
                ProgressReward.Story => Lookup(arcIds, id, "wątek fabuły"),
                ProgressReward.Decor => Lookup(decorIds, id, "ozdoba Osiedla"),
                _ => -1,
            };
            return new ProgressLevel(xp, reward, index, Int(x, "value", 0), Str(x, "title", ""));
        }
        ProgressLevel[] inspLevels = [], masteryLevels = [], stakeRanks = [];
        MasteryClassDef[] masteryClasses = [];
        int[] inspDiffPct = [];
        int ixRun = 0, ixStage = 0, ixBoss = 0, ixElite = 0, ixStore = 0, ixWin = 0, imRun = 0, imWin = 0, imResp = 0, mmWin = 0, mmClassWin = 0;
        if (d.TryGetProperty("inspector", out var insj))
        {
            inspLevels = insj.GetProperty("levels").EnumerateArray().Select(x => Level(x, Int(x, "xp"))).ToArray();
            var ix = insj.GetProperty("xp");
            ixRun = Int(ix, "run");
            ixStage = Int(ix, "stage");
            ixBoss = Int(ix, "boss");
            ixElite = Int(ix, "elite");
            ixStore = Int(ix, "storeroom");
            ixWin = Int(ix, "win");
            inspDiffPct = insj.GetProperty("diffPct").EnumerateArray().Select(v => v.GetInt32()).ToArray();
            var im = insj.GetProperty("migrate");
            imRun = Int(im, "run");
            imWin = Int(im, "win");
            imResp = Int(im, "respectPct");
            Require(inspLevels.Length is >= 20 and <= 40 && inspDiffPct.Length == difficulties.Length, "inspektor: 20-40 poziomów, % na każdą trudność");
        }
        if (d.TryGetProperty("mastery", out var maj))
        {
            masteryLevels = maj.GetProperty("levels").EnumerateArray().Select(x => Level(x, Int(x, "xp"))).ToArray();
            var boonIds = boons.Select((b, i) => (b.Id, i)).ToDictionary(x => x.Id, x => x.i);
            masteryClasses = maj.GetProperty("classes").EnumerateArray().Select(x =>
            {
                var pw = x.GetProperty("power");
                var wp = x.GetProperty("weapon");
                var pk = wp.GetProperty("perk");
                return new MasteryClassDef(Str(pw, "name"), Str(pw, "desc"), Int(pw, "power"), Int(pw, "cooldown"), Str(wp, "name"),
                    new Perk(ParsePerk(Str(pk, "effect")), Int(pk, "value")), Lookup(boonIds, Str(x, "boon"), "premia mistrzostwa"));
            }).ToArray();
            var mm = maj.GetProperty("migrate");
            mmWin = Int(mm, "win");
            mmClassWin = Int(mm, "classWin");
            Require(masteryLevels.Length == 10 && masteryClasses.Length == classes.Length, "mistrzostwo: 10 poziomów, wiersz na każdy zawód");
        }
        if (d.TryGetProperty("investor", out var invj2) && invj2.TryGetProperty("ranks", out var rkj))
        {
            stakeRanks = rkj.EnumerateArray().Select(x => Level(x, Int(x, "stake"))).ToArray();
        }
        // v0.21.52 cz. c: drzewko Szkoleń, kolekcje, zadania, seria dni
        TreeBranch[] treeBranches = [];
        TreeNode[] treeNodes = [];
        var treeRespec = 0;
        if (meta.TryGetProperty("tree", out var trj))
        {
            var upIds = upgrades.Select((u, i) => (u.Id, i)).ToDictionary(x => x.Id, x => x.i);
            treeBranches = trj.GetProperty("branches").EnumerateArray().Select(b => new TreeBranch(Str(b, "id"), Str(b, "name"),
                b.GetProperty("upgrades").EnumerateArray().Aggregate(0, (m, u) => m | 1 << Lookup(upIds, u.GetString() ?? "", "Szkolenie gałęzi")))).ToArray();
            var brIds = treeBranches.Select((b, i) => (b.Id, i)).ToDictionary(x => x.Id, x => x.i);
            treeNodes = trj.GetProperty("nodes").EnumerateArray().Select(x => new TreeNode(Lookup(brIds, Str(x, "branch"), "gałąź"), Int(x, "depth"),
                Int(x, "cost"), x.GetProperty("options").EnumerateArray().Select(o => new TreeOption(Str(o, "name"), Str(o, "short", Str(o, "name")), Str(o, "desc"),
                    ParseUpgrade(Str(o, "effect")), Int(o, "value"))).ToArray())).ToArray();
            treeRespec = Int(trj, "respecCost");
            Require(treeBranches.Length == 3 && treeNodes.Length is >= 1 and <= 8 && treeNodes.All(n => n.Options.Length == 2), "drzewko: 3 gałęzie, 1-8 węzłów po 2 opcje");
        }
        var kpIds = keepsakesJson.Select((k, i) => (Str(k, "id"), i)).ToDictionary(x => x.Item1, x => x.i);
        ProgressLevel Goal(JsonElement x, int xp)
        {
            var reward = ParseSnake<ProgressReward>(Str(x, "reward"));
            var id = Str(x, "id", "");
            var index = reward switch
            {
                ProgressReward.Helmet => Lookup(coid, id, "kolor kasku"),
                ProgressReward.Keepsake => kpIds.TryGetValue(id, out var ki) ? ki : -1, // dane bez pamiątek: nagroda bez działania
                _ => -1,
            };
            return new ProgressLevel(xp, reward, index, Int(x, "value", 0), Str(x, "title", ""));
        }
        CollectionDef[] collections = [];
        if (d.TryGetProperty("collections", out var colj))
        {
            collections = colj.GetProperty("sets").EnumerateArray().Select(x =>
            {
                var kind = Str(x, "kind") switch
                {
                    "act" => CollectionKind.Kills,
                    "bosses" => CollectionKind.Bosses,
                    "decor" => CollectionKind.Decor,
                    var s => throw new GameDataException($"nieznany rodzaj kolekcji: {s}"),
                };
                var mask = 0ul;
                if (kind == CollectionKind.Kills)
                {
                    var act = Int(x, "act");
                    foreach (var st in domStages)   // v0.21.52 cz. d: komplety aktów - problemy Domu jednorodzinnego
                    {
                        if (st.Act != act) continue;
                        foreach (var e in st.Pool) mask |= 1ul << e;
                    }
                }
                else if (kind == CollectionKind.Bosses)   // v0.21.52 cz. d: bossowie Domu albo ("career") nowych kontraktów
                {
                    var bosses = Bool(x, "career")
                        ? careerJson.Skip(1).Select(c => Lookup(eid, Str(c, "boss"), "boss kontraktu"))
                        : domStages.Where(st => st.Boss >= 0).Select(st => st.Boss);
                    foreach (var bi in bosses) mask |= 1ul << bi;
                }
                var rw = x.GetProperty("reward");
                var bonus = rw.TryGetProperty("perk", out var rpk) ? new Perk(ParsePerk(Str(rpk, "effect")), Int(rpk, "value")) : new Perk(PerkEffect.Hp, 0);
                return new CollectionDef(Str(x, "id"), Str(x, "name"), Str(x, "desc"), kind, mask, Int(x, "count"), Goal(rw, 0), bonus);
            }).ToArray();
            Require(collections.Length <= 8, "kolekcje: maks. 8 kompletów");
        }
        TaskDef[] dailyTasks = [], weeklyTasks = [];
        ProgressLevel[] taskRewards = [];
        if (d.TryGetProperty("tasks", out var tkj))
        {
            TaskDef[] Pool(string key) => tkj.GetProperty(key).EnumerateArray().Select(x => new TaskDef(Str(x, "id"), Str(x, "name"),
                ParseSnake<TaskKind>(Str(x, "kind")), Int(x, "target"), Int(x, "respect"))).ToArray();
            dailyTasks = Pool("daily");
            weeklyTasks = Pool("weekly");
            taskRewards = tkj.GetProperty("rewards").EnumerateArray().Select(x => Goal(x, Int(x, "count"))).ToArray();
            Require(dailyTasks.Length >= 3 && weeklyTasks.Length >= 2 && dailyTasks.Concat(weeklyTasks).All(x => x.Target is >= 1 and <= 255),
                "zadania: min. 3 dnia i 2 tygodnia, cel 1-255");
        }
        var streakRewards = d.TryGetProperty("daily", out var dsj) && dsj.TryGetProperty("streak", out var srj)
            ? srj.EnumerateArray().Select(x => Goal(x, Int(x, "days"))).ToArray()
            : [];
        // v0.21.52 cz. d (#47): kontrakty mapy kariery (bez sekcji - sam Dom jednorodzinny)
        var career = new List<CareerDef>();
        var careerFirst = 0;
        for (var i = 0; i < Math.Max(1, careerJson.Length); i++)
        {
            if (careerJson.Length == 0)
            {
                career.Add(new CareerDef("dom", "Dom jednorodzinny", "Dom", "", 0, domJson.Length, preludeStages, CareerUnlock.None, 0, 0, false, -1, 0, "", -1));
                break;
            }
            var c = careerJson[i];
            var n = i == 0 ? domJson.Length : c.GetProperty("stages").GetArrayLength();
            var u = c.GetProperty("unlock");
            var rw = c.TryGetProperty("reward", out var crw) ? crw : default;
            var hasRw = rw.ValueKind == JsonValueKind.Object;
            var helmet = hasRw && rw.TryGetProperty("helmet", out var chj) ? Lookup(coid, chj.GetString() ?? "", "kask kontraktu") : -1;
            var def = new CareerDef(Str(c, "id"), Str(c, "name"), Str(c, "short"), Str(c, "desc"), careerFirst, n, i == 0 ? preludeStages : 0,
                ParseEnum<CareerUnlock>(Str(u, "kind")), Int(u, "value"), Int(c, "gust", 0), Bool(c, "twins"),
                c.TryGetProperty("boss", out var cbj) ? Lookup(eid, cbj.GetString() ?? "", "boss kontraktu") : -1,
                hasRw ? Int(rw, "respect", 0) : 0, hasRw ? Str(rw, "title", "") : "", helmet);
            Require(def.Count is >= 1 and <= Game.MaxStages && (i == 0) == (def.Unlock == CareerUnlock.None), $"kontrakt {def.Id}: etapy / odblokowanie");
            career.Add(def);
            careerFirst += n;
        }
        Require(careerFirst == stages.Length && career.Count <= 6, "kontrakty: etapy wszystkich kontraktów, maks. 6");
        var twinCarryMax = d.TryGetProperty("career", out var ctm) ? Int(ctm, "twinCarryMax", 0) : 0;
        // v0.21.53 (#53, #54): filtry ekranu (bez sekcji - sam klasyczny)
        ScreenFilterDef[] screenFilters = [new("klasyczny", "Klasyczny", "Klasyk", "", "", FilterKind.Classic, false, false, [])];
        if (d.TryGetProperty("screenFilters", out var sfj))
        {
            var colIds = collections.Select((c, i) => (c.Id, i)).ToDictionary(x => x.Id, x => x.i);
            var carIds = career.Select((c, i) => (c.Id, i)).ToDictionary(x => x.Id, x => x.i);
            var secIds = secrets.Select((c, i) => (c.Id, i)).ToDictionary(x => x.Id, x => x.i);
            screenFilters = sfj.GetProperty("list").EnumerateArray().Select(x =>
            {
                var kind = ParseEnum<FilterKind>(Str(x, "kind"));
                var conds = x.TryGetProperty("unlock", out var uj)
                    ? uj.EnumerateArray().Select(u =>
                    {
                        var k = ParseEnum<FilterUnlock>(Str(u, "kind"));
                        var v = k switch
                        {
                            FilterUnlock.Collection => Lookup(colIds, Str(u, "id"), "kolekcja filtra"),
                            FilterUnlock.Career => Lookup(carIds, Str(u, "id"), "kontrakt filtra"),
                            FilterUnlock.Secret => Lookup(secIds, Str(u, "id"), "sekret filtra"),
                            _ => Int(u, "value"),
                        };
                        return new FilterCond(k, v);
                    }).ToArray()
                    : [];
                Require((kind == FilterKind.Fun) == (conds.Length > 0) && conds.Length <= 2, $"filtr {Str(x, "id")}: warunki tylko dla zabawowych (1-2)");
                return new ScreenFilterDef(Str(x, "id"), Str(x, "name"), Str(x, "short"), Str(x, "desc"), Str(x, "hint", ""), kind,
                    Bool(x, "cues"), Bool(x, "motion"), conds);
            }).ToArray();
            Require(screenFilters.Length is >= 2 and <= 16 && screenFilters[0].Kind == FilterKind.Classic
                && screenFilters.Count(f => f.Kind == FilterKind.Classic) == 1, "filtry ekranu: 2-16, pierwszy klasyczny");
        }

        var progressTitles = inspLevels.Select((l, i) => (l, i)).Where(x => x.l.Reward == ProgressReward.Title)
            .Select(x => new ProgressTitle(x.l.Title, 0, x.i + 1))
            .Concat(stakeRanks.Where(x => x.Reward == ProgressReward.Title).Select(x => new ProgressTitle(x.Title, 1, x.Xp)))
            .Concat(collections.Select((c, i) => (c, i)).Where(x => x.c.Reward.Reward == ProgressReward.Title).Select(x => new ProgressTitle(x.c.Reward.Title, 2, x.i + 1)))
            .Concat(streakRewards.Where(x => x.Reward == ProgressReward.Title).Select(x => new ProgressTitle(x.Title, 3, x.Xp)))
            .Concat(taskRewards.Where(x => x.Reward == ProgressReward.Title).Select(x => new ProgressTitle(x.Title, 4, x.Xp)))
            .Concat(career.Select((c, i) => (c, i)).Where(x => x.c.Title != "").Select(x => new ProgressTitle(x.c.Title, 5, x.i))).ToArray();

        return new GameData
        {
            RecapKindNames = recapKinds,
            RecapVerbs = recapVerbs,
            RecapTips = recapTips,
            Weekly = weekly,
            WeeklyEpoch = weeklyEpoch,
            WeeklyDifficulty = weeklyDiff,
            WeeklyHistory = weeklyHistory,
            WeeklyCoffeeCash = weeklyCoffee,
            StoryArc = arc,
            EstateDecor = decor,
            InspectorLevels = inspLevels,
            InspectorXpRun = ixRun,
            InspectorXpStage = ixStage,
            InspectorXpBoss = ixBoss,
            InspectorXpElite = ixElite,
            InspectorXpStoreroom = ixStore,
            InspectorXpWin = ixWin,
            InspectorDiffPct = inspDiffPct,
            InspectorMigrateRun = imRun,
            InspectorMigrateWin = imWin,
            InspectorMigrateRespectPct = imResp,
            MasteryLevels = masteryLevels,
            MasteryClasses = masteryClasses,
            MasteryMigrateWin = mmWin,
            MasteryMigrateClassWin = mmClassWin,
            StakeRanks = stakeRanks,
            ProgressTitles = progressTitles,
            ProgressHelpLines = d.TryGetProperty("progressHelp", out var phj) ? phj.EnumerateArray().Select(x => x.GetString() ?? "").ToArray() : [],
            TreeBranches = treeBranches,
            TreeNodes = treeNodes,
            TreeRespecCost = treeRespec,
            Collections = collections,
            DailyTasks = dailyTasks,
            WeeklyTasks = weeklyTasks,
            TaskRewards = taskRewards,
            StreakRewards = streakRewards,
            GoalsHelpLines = d.TryGetProperty("goalsHelp", out var ghj) ? ghj.EnumerateArray().Select(x => x.GetString() ?? "").ToArray() : [],
            CareerHelpLines = d.TryGetProperty("careerHelp", out var chl) ? chl.EnumerateArray().Select(x => x.GetString() ?? "").ToArray() : [],
            Career = career.ToArray(),
            CareerTwinCarryMax = twinCarryMax,
            ScreenFilters = screenFilters,
            FilterUi = d.TryGetProperty("screenFilters", out var sfu) && sfu.TryGetProperty("ui", out var sfuj)
                ? sfuj.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.GetString() ?? "")
                : new Dictionary<string, string>(),
            FilterWhereLines = d.TryGetProperty("screenFilters", out var sfw) && sfw.TryGetProperty("where", out var sfwj)
                ? sfwj.EnumerateArray().Select(x => x.GetString() ?? "").ToArray() : [],
            RarityLetters = d.TryGetProperty("screenFilters", out var sfr) && sfr.TryGetProperty("rarityLetters", out var sfrj)
                ? sfrj.EnumerateArray().Select(x => x.GetString() ?? "").ToArray() : ["Z", "R", "L"],
            FiltersHelpLines = d.TryGetProperty("filtersHelp", out var fhl) ? fhl.EnumerateArray().Select(x => x.GetString() ?? "").ToArray() : [],
            StagesCount = domJson.Length,
            StageLooksCount = stages.Max(x => x.Look) + 1,
            BoonRarities = boonRarities,
            BoonTags = boonTagNames,
            Boons = boons,
            Synergies = synergies,
            BoonRerollCost = rerollCost,
            SynergyAt = synergyAt,
            BoonLuckRare = luckRare,
            BoonLuckLegend = luckLegend,
            Elites = elites,
            EliteActPct = eliteActPct,
            EliteDiffPct = eliteDiffPct,
            EliteTierPct = eliteTierPct,
            EliteHpPct = eliteHpPct,
            EliteDmg = eliteDmg,
            EliteRespect = eliteRespect,
            EliteMats = eliteMats,
            EliteGearMin = eliteGearMin,
            ChoiceEvents = choiceEvents,
            ChoiceEventChancePct = choiceChance,
            ToolLevels = toolLevels,
            ToolTraits = toolTraits,
            ToolUpgradeMax = toolMax,
            ToolUpgradeDmg = toolDmg,
            ToolTraitAt = toolTraitAt,
            SecretKinds = secretKinds,
            SecretChancePct = secretChance,
            SecretGuardPct = secretGuard,
            ChestRespect = chestRespect,
            ChestMats = chestMats,
            ChestCash = chestCash,
            ChestGearMin = chestGearMin,
            MetaHelpLines = d.TryGetProperty("metaHelp", out var mhj) ? mhj.EnumerateArray().Select(x => x.GetString() ?? "").ToArray() : [],
            ExtrasHelpLines = d.TryGetProperty("extrasHelp", out var ehj) ? ehj.EnumerateArray().Select(x => x.GetString() ?? "").ToArray() : [],
            Combos = combos,
            ComboSources = comboSources,
            WetTurns = wetTurns,
            HeroWetTurns = heroWetTurns,
            Weapons = weapons,
            Classes = classes,
            Enemies = enemies,
            Stages = stages,
            Difficulties = difficulties,
            Upgrades = upgrades,
            StoryStages = storyStages,
            StoryWin = Story(story.GetProperty("win")),
            StoryLose = Story(story.GetProperty("lose")),
            StoryNgPlus = Story(story.GetProperty("ngplus")),
            StoryPrologue = story.TryGetProperty("prologue", out var pro) ? Story(pro) : new StoryMsg("", ["", "", ""]),
            PrologueCaptions = story.TryGetProperty("prologueCaptions", out var pc) ? pc.EnumerateArray().Select(x => x.GetString() ?? "").ToArray() : [],
            Acts = acts,
            Hurtownia = hurtownia,
            Badges = badges,
            Keepsakes = keepsakes,
            KeepsakeRankRuns = rankRuns,
            Contracts = contracts,
            SiteEvents = siteEvents,
            SiteEventChancePct = siteEventChance,
            Weather = weather,
            WeatherNoBadStack = weatherNoBadStack,
            Brigade = brigade,
            StartHelpersMask = startHelpers,
            Investor = investor,
            Materials = materials,
            Repairs = repairs,
            MaterialDropPct = hasMaterials ? Int(matj, "dropPct") : 0,
            MaterialBossDrop = hasMaterials ? Int(matj, "bossDrop") : 0,
            MaterialGearBox = hasMaterials ? Int(matj, "gearBox") : 0,
            MaterialMax = hasMaterials ? Int(matj, "max") : 9,
            Paths = paths,
            DailyEpoch = hasDaily ? dj.GetProperty("epoch").EnumerateArray().Select(x => x.GetInt32()).ToArray() : [2026, 1, 1],
            DailyDefaultDate = hasDaily ? dj.GetProperty("defaultDate").EnumerateArray().Select(x => x.GetInt32()).ToArray() : [2026, 10, 1],
            DailyDifficulty = hasDaily ? Lookup(Index(difficultiesJson), Str(dj, "difficulty"), "trudność") : Int(d, "defaultDifficulty"),
            DailyInvestorMods = hasDaily ? Int(dj, "investorMods") : 0,
            DailyHistory = hasDaily ? Int(dj, "history") : 5,
            ScheduleMinDays = hasSchedule ? Int(scj, "minDays") : 4,
            ScheduleTurnsPerDay = hasSchedule ? Int(scj, "turnsPerDay") : 3,
            ScheduleUrl = hasSchedule ? Str(scj, "url") : "planbudowlany.online",
            Tools = tools,
            Gear = gear.ToArray(),
            GearSlots = slots.ToArray(),
            GearRarities = rarities,
            DropWeights = [Int(weights, "coffee"), Int(weights, "helmet"), Int(weights, "plan"), Int(weights, "tool"), Int(weights, "gear")],
            LevelThresholds = hl.GetProperty("thresholds").EnumerateArray().Select(x => x.GetInt32()).ToArray(),
            SlamEvery = Int(slamJson, "every"),
            SlamDamageBonus = Int(slamJson, "damageBonus"),
            SlamRadius = Int(slamJson, "radius"),
            SlamDelay = Int(slamJson, "delay", 2),
            SlamCrossDelay = Int(slamJson, "crossDelay", 3),
            SlamCrossReach = Int(slamJson, "crossReach", 2),
            CashPerScore = Int(d.GetProperty("cash"), "perScore"),
            StartToolsMask = startTools,
            DropChancePct = Int(drops, "chancePct"),
            GearSolidFrom = Int(rr, "solidFrom"),
            GearBrandFrom = Int(rr, "brandFrom"),
            GearStageBonus = Int(rr, "stageBonus"),
            XpPerKill = Int(meta, "xpPerKill"),
            XpPerStage = Int(meta, "xpPerStage"),
            XpBoss = Int(meta, "xpBoss"),
            StartClassesMask = startClasses,
            ClassCosts = classCosts,
            ToolCosts = toolCosts,
            HardCost = Int(meta, "hardCost"),
            HpPerLevel = Int(hl, "hpPerLevel"),
            DmgLevelsMask = hl.GetProperty("dmgLevels").EnumerateArray().Aggregate(0, (m, l) => m | 1 << l.GetInt32()),
            DefLevelsMask = hl.GetProperty("defLevels").EnumerateArray().Aggregate(0, (m, l) => m | 1 << l.GetInt32()),
            Version = Str(d, "version", ""),
            DamageHelpLines = d.TryGetProperty("damageHelp", out var dhj) ? dhj.EnumerateArray().Select(x => x.GetString() ?? "").ToArray() : [],
            DefaultDifficulty = Int(d, "defaultDifficulty"),
            NgHpPctPerTier = Int(ng, "hpPctPerTier"),
            NgDmgBonusPerTier = Int(ng, "dmgBonusPerTier"),
            NgScorePctPerTier = Int(ng, "scorePctPerTier"),
            Statuses = [new StatusDef("", "", ""), StatusOf("poison"), StatusOf("shock"), StatusOf("slip"), StatusOf("paper"),
                stt.TryGetProperty("wet", out _) ? StatusOf("wet") : new StatusDef("Mokry", "Mokry", "prąd boli bardziej")],
            PaperDelay = Int(stt.GetProperty("paper"), "delay"),
            GearTraits = traits,
            GearDeclineXp = Int(eq, "declineXp"),
            CritBasePct = Int(lk, "critBasePct"),
            CritPerLuckPct = Int(lk, "critPerLuckPct"),
            CritMultiplier = Int(lk, "critMultiplier"),
            DropPerLuckPct = Int(lk, "dropPerLuckPct"),
            RarityPerLuck = Int(lk, "rarityPerLuck"),
            DodgePerLuckPct = Int(lk, "dodgePerLuckPct"),
            DodgeMaxPct = Int(lk, "dodgeMaxPct"),
            ThermosCapacity = Int(th, "capacity"),
            CoffeeHeal = Int(th, "heal"),
            BotDrinkBelowPct = Int(th, "botDrinkBelowPct"),
            BadgeBezUsterek = BadgeIdx("bez_usterek"),
            BadgePrzedTerminem = BadgeIdx("przed_terminem"),
            BadgeSeryjny = BadgeIdx("seryjny"),
            BadgeZawodowiec = BadgeIdx("zawodowiec"),
            BadgeTwardziel = BadgeIdx("twardziel"),
            BadgePelnyZespol = BadgeIdx("pelny_zespol"),
            BadgeKolekcjoner = BadgeIdx("kolekcjoner"),
            BadgeKatalog = BadgeIdx("katalog"),
            BadgeOsiedle = BadgeIdx("osiedle"),
            Respect = respect,
            RespectStage = hasRespect ? Int(rsj, "stage") : 0,
            RespectBoss = hasRespect ? Int(rsj, "boss") : 0,
            RespectActBoss = hasRespect ? Int(rsj, "actBoss") : 0,
            RespectFinal = hasRespect ? Int(rsj, "final") : 0,
            Rewards = rewards.ToArray(),
            PushChancePct = d.TryGetProperty("passives", out var psj) ? Int(psj, "pushChancePct", 0) : 0,
            GearRewardMask = gearReward,
            GearBaseMask = gearBase,
            RewardClassesMask = rewardClasses,
            BehaviorRangedReach = hasBp ? Int(bpj, "rangedReach", 3) : 3,
            BehaviorSplitHpPct = hasBp ? Int(bpj, "splitHpPct", 50) : 50,
            BehaviorHealValue = hasBp ? Int(bpj, "healValue", 3) : 3,
            BehaviorHealEvery = hasBp ? Int(bpj, "healEvery", 2) : 2,
            BehaviorBlastDamage = hasBp ? Int(bpj, "blastDamage", 4) : 4,
            BehaviorBlastRadius = hasBp ? Int(bpj, "blastRadius", 1) : 1,
            BehaviorBlastDelay = hasBp ? Int(bpj, "blastDelay", 2) : 2,
            BehaviorGrowEvery = hasBp ? Int(bpj, "growEvery", 4) : 4,
            BehaviorGrowHp = hasBp ? Int(bpj, "growHp", 2) : 2,
            BehaviorGrowMax = hasBp ? Int(bpj, "growMax", 4) : 4,
            BehaviorFleeCooldown = hasBp ? Int(bpj, "fleeCooldown", 3) : 3,
            BehaviorReturnTurns = hasBp ? Int(bpj, "returnTurns", 4) : 4,
            BehaviorReturnHpPct = hasBp ? Int(bpj, "returnHpPct", 50) : 50,
            BehaviorPushCooldown = hasBp ? Int(bpj, "pushCooldown", 3) : 3,
            BehaviorNames = behaviorNames,
            PreludeStages = preludeStages,
            Documents = documents,
            TutorialSteps = tutSteps,
            TutorialUnlocks = tutUnlocks,
            Secrets = secrets,
            Cosmetics = cosmetics,
            SecretClassesMask = secretClasses,
            OpenClassesCount = openClasses,
            SecretToolsMask = secretTools,
            SecretPaperMask = paperMask,
            SecretHelperBoss = helperBoss,
            CosmeticGold = coid.TryGetValue("zlota_kielnia", out var cg) ? cg : -1,
            CosmeticStripes = coid.TryGetValue("kask_paski", out var cs) ? cs : -1,
            MarkTurns = d.TryGetProperty("passives", out var mtj) ? Int(mtj, "markTurns", 6) : 6,
        };
    }

    // Zachowania problemu: lista "behaviors" -> bitmaska (nieznane ignorowane - nowsza wersja danych).
    private static int BehaviorTags(JsonElement e)
    {
        if (!e.TryGetProperty("behaviors", out var b)) return 0;
        var tags = 0;
        foreach (var x in b.EnumerateArray())
        {
            var i = Array.IndexOf(Behavior.Ids, x.GetString() ?? "");
            if (i >= 0) tags |= 1 << i;
        }
        return tags;
    }

    /// <summary>Skutek odpowiedzi na wydarzenie: argument z materiału, problemu, stanu albo slotu sprzętu.</summary>
    private static ChoiceOut ParseChoiceOut(JsonElement x, Dictionary<string, int> mid, Dictionary<string, int> eid, string[] slots)
    {
        var effect = ParseSnake<ChoiceEffect>(Str(x, "effect"));
        var arg = effect switch
        {
            ChoiceEffect.Mats => x.TryGetProperty("material", out var m) ? Lookup(mid, m.GetString() ?? "", "materiał") : -1,
            ChoiceEffect.Spawn => Lookup(eid, Str(x, "enemy"), "problem"),
            ChoiceEffect.Status => (int)ParseEnum<StatusEffect>(Str(x, "status")),
            ChoiceEffect.Gear => x.TryGetProperty("slot", out var sl) ? Array.IndexOf(slots, sl.GetString() ?? "") : -1,
            _ => -1,
        };
        return new ChoiceOut(effect, Int(x, "value"), Int(x, "chance", 100), arg);
    }

    private static StoryMsg Story(JsonElement m)
    {
        var lines = Str(m, "text").Split('|').ToList();
        Require(lines.Count <= 3, "dymek: maks. 3 linie");
        while (lines.Count < 3) lines.Add("");
        return new StoryMsg(Str(m, "from"), lines.ToArray());
    }

    private static Dictionary<string, int> Index(JsonElement[] items)
    {
        var map = new Dictionary<string, int>();
        for (var i = 0; i < items.Length; i++) map[Str(items[i], "id")] = i;
        return map;
    }

    private static int Lookup(Dictionary<string, int> map, string id, string what) =>
        map.TryGetValue(id, out var i) ? i : throw new GameDataException($"nieznany identyfikator ({what}): {id}");

    private static string Str(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) ? v.GetString() ?? "" : throw new GameDataException($"brak pola '{name}'");

    private static string Str(JsonElement e, string name, string fallback) =>
        e.TryGetProperty(name, out var v) ? v.GetString() ?? fallback : fallback;

    private static int Int(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) ? v.GetInt32() : throw new GameDataException($"brak pola '{name}'");

    private static bool Bool(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.True;

    private static int Int(JsonElement e, string name, int fallback) =>
        e.TryGetProperty(name, out var v) ? v.GetInt32() : fallback;

    private static Stat ParseStat(string s) => s switch
    {
        "strength" => Stat.Str,
        "agility" => Stat.Agi,
        "intelligence" => Stat.Intel,
        _ => throw new GameDataException($"nieznana cecha: {s}"),
    };

    private static TraitEffect ParseTrait(string s) => s switch
    {
        "luck" => TraitEffect.Luck,
        "crit" => TraitEffect.Crit,
        "poison_res" => TraitEffect.PoisonRes,
        "sight" => TraitEffect.Sight,
        "cooldown" => TraitEffect.Cooldown,
        "str" => TraitEffect.Str,
        "agi" => TraitEffect.Agi,
        "intel" => TraitEffect.Intel,
        "slip_res" => TraitEffect.SlipRes,
        _ => TraitEffect.Unknown, // nowsza wersja danych: cecha bez działania
    };

    private static PerkEffect ParsePerk(string s) => s switch
    {
        "hp" => PerkEffect.Hp,
        "def" => PerkEffect.Def,
        "dmg" => PerkEffect.Dmg,
        "luck" => PerkEffect.Luck,
        "cooldown" => PerkEffect.Cooldown,
        "sight" => PerkEffect.Sight,
        "thermos" => PerkEffect.Thermos,
        "tool_pct" => PerkEffect.ToolPct,
        "xp_pct" => PerkEffect.XpPct,
        "cash" => PerkEffect.Cash,
        "crit" => PerkEffect.Crit,
        "coffee" => PerkEffect.Coffee,
        "taken_pct" => PerkEffect.TakenPct,
        _ => PerkEffect.Unknown, // nowsza wersja danych: premia bez działania
    };

    private static ContractKind ParseContract(string s) => s switch
    {
        "kills" => ContractKind.Kills,
        "powers" => ContractKind.Powers,
        "brand" => ContractKind.Brand,
        "clean_boss" => ContractKind.CleanBoss,
        "class_wins" => ContractKind.ClassWins,
        "wins" => ContractKind.Wins,
        _ => throw new GameDataException($"nieznany rodzaj zlecenia: {s}"),
    };

    private static EventEffect ParseEvent(string s) => s switch
    {
        "fewer_pickups" => EventEffect.FewerPickups,
        "cash" => EventEffect.Cash,
        "inspection" => EventEffect.Inspection,
        "rain" => EventEffect.Rain,
        "thermos" => EventEffect.Thermos,
        _ => throw new GameDataException($"nieznany skutek wydarzenia: {s}"),
    };

    private static InvestorEffect ParseInvestor(string s) => s switch
    {
        "cash_pct" => InvestorEffect.CashPct,
        "no_break" => InvestorEffect.NoBreak,
        "enemy_hp" => InvestorEffect.EnemyHp,
        "no_shop" => InvestorEffect.NoShop,
        "slam" => InvestorEffect.Slam,
        "enemy_dmg" => InvestorEffect.EnemyDmg,
        _ => throw new GameDataException($"nieznany modyfikator inwestora: {s}"),
    };

    private static UpgradeEffect ParseUpgrade(string s) => s switch
    {
        "hp" => UpgradeEffect.Hp,
        "def" => UpgradeEffect.Def,
        "dmg" => UpgradeEffect.Dmg,
        "coffee" => UpgradeEffect.Coffee,
        "pickups" => UpgradeEffect.Pickups,
        "luck" => UpgradeEffect.Luck,
        "craft" => UpgradeEffect.Craft,
        "dmg_pct" => UpgradeEffect.DmgPct,
        "taken_pct" => UpgradeEffect.TakenPct,
        "crit" => UpgradeEffect.Crit,
        "dodge" => UpgradeEffect.Dodge,
        "thermos" => UpgradeEffect.Thermos,
        "mats_pct" => UpgradeEffect.MatsPct,
        "gear_pct" => UpgradeEffect.GearPct,
        "cash" => UpgradeEffect.Cash,
        "shop_pct" => UpgradeEffect.ShopPct,
        "brigade_pct" => UpgradeEffect.BrigadePct,
        "cooldown" => UpgradeEffect.Cooldown,
        "first_hit" => UpgradeEffect.FirstHit,
        _ => UpgradeEffect.Unknown, // nowsza wersja danych: ulepszenie bez działania
    };

    private static RespectEffect ParseRespect(string s) => s switch
    {
        "dmg_pct" => RespectEffect.DmgPct,
        "taken_pct" => RespectEffect.TakenPct,
        "gear_pct" => RespectEffect.GearPct,
        "crit" => RespectEffect.Crit,
        "dodge" => RespectEffect.Dodge,
        "coffee_pct" => RespectEffect.CoffeePct,
        "thermos" => RespectEffect.Thermos,
        "cooldown" => RespectEffect.Cooldown,
        "cash" => RespectEffect.Cash,
        "xp_pct" => RespectEffect.XpPct,
        "brigade_pct" => RespectEffect.BrigadePct,
        "sight" => RespectEffect.Sight,
        "shop_pct" => RespectEffect.ShopPct,
        "mats_pct" => RespectEffect.MatsPct,
        "second_chance" => RespectEffect.SecondChance,
        "reroll" => RespectEffect.Reroll,
        "veteran" => RespectEffect.Veteran,
        _ => RespectEffect.Unknown,
    };

    // "wet_hits" -> WetHits (skutki premii, synergii, elit i kombinacji w danych)
    private static T ParseSnake<T>(string s) where T : struct, Enum =>
        ParseEnum<T>(string.Concat(s.Split('_').Select(p => p.Length > 0 ? char.ToUpperInvariant(p[0]) + p[1..] : p)));

    private static T ParseSnakeOr<T>(string s, T fallback) where T : struct, Enum =>
        Enum.TryParse<T>(string.Concat(s.Split('_').Select(p => p.Length > 0 ? char.ToUpperInvariant(p[0]) + p[1..] : p)), true, out var v) ? v : fallback;

    private static Element ParseElement(string s) => s switch
    {
        "woda" => Element.Water,
        "prad" => Element.Power,
        "iskra" => Element.Spark,
        "none" => Element.None,
        _ => throw new GameDataException($"nieznany żywioł: {s}"),
    };

    private static T ParseEnum<T>(string s) where T : struct, Enum =>
        Enum.TryParse<T>(s, ignoreCase: true, out var v) ? v : throw new GameDataException($"nieznana wartość {typeof(T).Name}: {s}");

    private static void Require(bool ok, string what)
    {
        if (!ok) throw new GameDataException(what);
    }
}
