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
    public int ClassCost { get; private init; }
    public int HardCost { get; private init; }
    public int HpPerLevel { get; private init; }
    public int DmgLevelsMask { get; private init; }
    public int DefLevelsMask { get; private init; }
    public string Version { get; private init; } = "";
    /// <summary>v0.21.50: Jak grać, strona Obrażenia – obrażenia broni w prostych słowach (rozpiska #26).</summary>
    public string[] DamageHelpLines { get; private init; } = [];
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
            ParseStat(Str(w, "scalesWith")), ParseElement(Str(w, "element", "none")))).ToArray();
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
                Int(ab, "cooldown"), ParseEnum<ClassPassive>(Str(c, "passive", "none")), Bool(c, "reward"));
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

        var stages = d.GetProperty("stages").EnumerateArray().Select(st =>
        {
            var pool = st.GetProperty("enemies").EnumerateArray().Select(x => Lookup(eid, x.GetString() ?? "", "wróg")).ToArray();
            Require(pool.Length is >= 1 and <= 4, "etap: 1-4 rodzaje wrogów");
            var boss = st.TryGetProperty("boss", out var b) ? Lookup(eid, b.GetString() ?? "", "boss") : -1;
            return new StageDef(Str(st, "name"), pool, Int(st, "count"), boss, Int(st, "hpPct", 100), Int(st, "dmgBonus", 0), Int(st, "act"),
                Int(st, "cost", 0));
        }).ToArray();
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
        var upgrades = meta.GetProperty("upgrades").EnumerateArray().Select(u => new UpgradeDef(
            Str(u, "id", ""), Str(u, "name"), Str(u, "desc"), ParseUpgrade(Str(u, "effect")), Int(u, "value"),
            u.GetProperty("costs").EnumerateArray().Select(c => c.GetInt32()).ToArray(), Int(u, "refund", 0), Int(u, "resetRefund", 0))).ToArray();
        foreach (var u in upgrades)
        {
            Require(u.Costs.Length is >= 1 and <= 4, $"ulepszenie {u.Name}: 1-4 poziomy");
        }

        var classesJson = d.GetProperty("classes").EnumerateArray().ToArray();
        var cid = Index(classesJson);
        var tools = meta.GetProperty("tools").EnumerateArray().Select(t => new ToolDef(Lookup(wid, Str(t, "weapon"), "narzędzie"), Int(t, "cost"),
            Bool(t, "reward"))).ToArray();
        Require(tools.Length <= 8, "narzędzia: maks. 8 (bitmaska w profilu)");

        var story = d.GetProperty("story");
        var storyStages = story.GetProperty("stages").EnumerateArray().Select(Story).ToArray();
        Require(storyStages.Length == stages.Length, "fabuła: tyle wiadomości, ile etapów");

        var acts = d.GetProperty("acts").EnumerateArray().Select(a => a.TryGetProperty("mechanic", out var mc)
            ? new ActDef(Str(a, "name"), Int(a, "bonusPerStage"), Int(a, "bonusPerKill"), ParseEnum<ActMechanic>(Str(mc, "effect")), Int(mc, "value", 0),
                Str(mc, "name", ""), Str(mc, "short", ""), Str(mc, "info", ""), Str(a, "numeral", ""), Bool(a, "prelude"))
            : new ActDef(Str(a, "name"), Int(a, "bonusPerStage"), Int(a, "bonusPerKill"), Numeral: Str(a, "numeral", ""), Prelude: Bool(a, "prelude"))).ToArray();
        for (var ai = 0; ai < acts.Length; ai++)
        {
            var last = Array.FindLastIndex(stages, s => s.Act == ai);
            Require(last >= 0 && stages[last].Boss >= 0, $"akt {ai} bez bossa");
        }
        // Akt wstępny (Akt 0): jego etapy na początku listy, za nimi etapy budowy.
        var preludeStages = stages.Count(st => acts[st.Act].Prelude);
        for (var i = 0; i < stages.Length; i++) Require(acts[stages[i].Act].Prelude == i < preludeStages, "Akt 0: etapy na początku listy");
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
        var badges = badgesJson.Select(b => new BadgeDef(Str(b, "id"), Str(b, "name"), Str(b, "desc"), Int(b, "xp"),
            b.TryGetProperty("perk", out var pk) ? new Perk(ParsePerk(Str(pk, "effect")), Int(pk, "value")) : new Perk(PerkEffect.Unknown, 0))).ToArray();
        Require(badges.Length <= 16 && enemies.Length <= Game.MaxEnemyTypes, "maks. 16 odznak i 48 rodzajów wrogów");
        var bid = Index(badgesJson);
        // pamiątki i zlecenia (od v0.21.43; starsze dane – puste listy)
        var keepsakesJson = d.TryGetProperty("keepsakes", out var ksj) ? ksj.GetProperty("list").EnumerateArray().ToArray() : [];
        var kid = Index(keepsakesJson);
        var contractsJson = d.TryGetProperty("contracts", out var cj) ? cj.EnumerateArray().ToArray() : [];
        var contracts = contractsJson.Select(c => new ContractDef(Str(c, "id"), Str(c, "name"), Str(c, "desc"), ParseContract(Str(c, "kind")),
            Int(c, "target"), Int(c, "xp"), c.TryGetProperty("keepsake", out var ck) ? Lookup(kid, ck.GetString() ?? "", "pamiątka") : -1)).ToArray();
        foreach (var c in contracts) Require(c.Target is > 0 and < 30000, $"zlecenie {c.Id}: zły cel");
        var keepsakes = keepsakesJson.Select(k => new KeepsakeDef(Str(k, "id"), Str(k, "name"), Str(k, "desc"), ParsePerk(Str(k, "effect")),
            k.GetProperty("values").EnumerateArray().Select(v => v.GetInt32()).ToArray(),
            k.TryGetProperty("badge", out var kb) ? Lookup(bid, kb.GetString() ?? "", "odznaka") : -1,
            k.TryGetProperty("start", out var kst) && kst.GetBoolean())).ToArray();
        for (var k = 0; k < keepsakes.Length; k++)
        {
            Require(keepsakes[k].Values.Length == 3, $"pamiątka {keepsakes[k].Id}: 3 rangi");
            var kk = k;
            Require(keepsakes[k].Start || keepsakes[k].Badge >= 0 || contracts.Any(c => c.Keepsake == kk), $"pamiątka {keepsakes[k].Id} bez sposobu odblokowania");
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

        var allStages = (1 << stages.Length) - 1;
        WeatherDef[] weather = [new WeatherDef("slonce", "Słonecznie", "Pogodnie", "", WeatherEffect.None, 0, 1, false, allStages)];
        var weatherNoBadStack = false;
        if (d.TryGetProperty("weather", out var wj))
        {
            weatherNoBadStack = wj.TryGetProperty("noBadStack", out var nb) && nb.GetBoolean();
            weather = wj.GetProperty("list").EnumerateArray().Select(w => new WeatherDef(Str(w, "id"), Str(w, "name"), Str(w, "short"),
                Str(w, "info"), ParseEnum<WeatherEffect>(Str(w, "effect")), Int(w, "value"), Int(w, "weight"), w.GetProperty("bad").GetBoolean(),
                w.TryGetProperty("stages", out var ws) ? ws.EnumerateArray().Aggregate(0, (m, x) => m | 1 << x.GetInt32()) : allStages)).ToArray();
            Require(weather.Length >= 1 && weather[0].Effect == WeatherEffect.None, "pogoda: pierwsza bez skutku");
            foreach (var w in weather)
            {
                Require(w.Weight is > 0 and < 128 && (w.Effect is not (WeatherEffect.Frost or WeatherEffect.Rain) || w.Value >= 2), $"pogoda {w.Id}: zła waga/wartość");
            }
            for (var si = 0; si < stages.Length; si++)
            {
                var bit = 1 << si;
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
            if (tools[i].Cost == 0 && !tools[i].Reward) startTools |= 1 << i;
        }
        var rewardClasses = 0;
        for (var i = 0; i < classes.Length; i++)
        {
            if (classes[i].Reward) rewardClasses |= 1 << i;
            else Require(i < 8, "zawody do kupienia: bitmaska uint8 w profilu");
        }
        Require(classes.Length <= 12, "maks. 12 zawodów");

        var hasRespect = d.TryGetProperty("respect", out var rsj);
        var respect = hasRespect
            ? rsj.GetProperty("upgrades").EnumerateArray().Select(x => new RespectDef(Str(x, "id"), Str(x, "name"), Str(x, "desc"),
                ParseRespect(Str(x, "effect")), x.GetProperty("values").EnumerateArray().Select(v => v.GetInt32()).ToArray(),
                x.GetProperty("costs").EnumerateArray().Select(v => v.GetInt32()).ToArray())).ToArray()
            : [];
        Require(respect.Length <= 16, "Respekt: maks. 16 ulepszeń");
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
            Require(tutUnlocks.Select(x => x.Id).SequenceEqual(new[] { "respect", "daily", "investor", "act0", "class" }), "samouczek: 5 dymków odblokowań");
        }

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
                x.TryGetProperty("class", out var bc) ? Lookup(cid, bc.GetString() ?? "", "zawód premii") : -1)).ToArray();
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

        return new GameData
        {
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
            ClassCost = Int(meta, "classCost"),
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
