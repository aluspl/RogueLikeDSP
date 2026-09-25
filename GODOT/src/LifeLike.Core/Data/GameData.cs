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

        var weapons = weaponsJson.Select(w => new WeaponDef(
            Str(w, "id"), Str(w, "name"), Int(w, "minDamage"), Int(w, "maxDamage"), Int(w, "range"),
            ParseStat(Str(w, "scalesWith")))).ToArray();
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
                Int(ab, "cooldown"));
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
                hasReward ? Str(rw, "title", "") : "");
        }).ToArray();
        foreach (var e in enemies)
        {
            Require(e.Summon < 0 || (!enemies[e.Summon].Slam && e.SummonEvery > 0 && e.SummonMax is > 0 and <= 3),
                $"wróg {e.Id}: złe wezwania");
        }

        var stages = d.GetProperty("stages").EnumerateArray().Select(st =>
        {
            var pool = st.GetProperty("enemies").EnumerateArray().Select(x => Lookup(eid, x.GetString() ?? "", "wróg")).ToArray();
            Require(pool.Length is >= 1 and <= 4, "etap: 1-4 rodzaje wrogów");
            var boss = st.TryGetProperty("boss", out var b) ? Lookup(eid, b.GetString() ?? "", "boss") : -1;
            return new StageDef(Str(st, "name"), pool, Int(st, "count"), boss, Int(st, "hpPct", 100), Int(st, "dmgBonus", 0), Int(st, "act"));
        }).ToArray();
        foreach (var st in stages)
        {
            // boss z wezwaniami: etap + boss + wezwani mieszczą się w Game.MaxEnemies
            Require(st.Boss < 0 || st.EnemyCount + 1 + enemies[st.Boss].SummonMax <= 12, $"etap {st.Name}: za dużo wrogów z wezwanymi");
        }

        var difficulties = d.GetProperty("difficulties").EnumerateArray().Select(x => new DifficultyDef(
            Str(x, "id", ""), Str(x, "name"), Int(x, "hpPct"), Int(x, "dmgBonus"), Int(x, "scorePct"))).ToArray();

        var meta = d.GetProperty("meta");
        var upgrades = meta.GetProperty("upgrades").EnumerateArray().Select(u => new UpgradeDef(
            Str(u, "id", ""), Str(u, "name"), Str(u, "desc"), ParseUpgrade(Str(u, "effect")), Int(u, "value"),
            u.GetProperty("costs").EnumerateArray().Select(c => c.GetInt32()).ToArray())).ToArray();
        foreach (var u in upgrades)
        {
            Require(u.Costs.Length is >= 1 and <= 4, $"ulepszenie {u.Name}: 1-4 poziomy");
        }

        var classesJson = d.GetProperty("classes").EnumerateArray().ToArray();
        var cid = Index(classesJson);
        var tools = meta.GetProperty("tools").EnumerateArray().Select(t => new ToolDef(Lookup(wid, Str(t, "weapon"), "narzędzie"), Int(t, "cost"))).ToArray();

        var story = d.GetProperty("story");
        var storyStages = story.GetProperty("stages").EnumerateArray().Select(Story).ToArray();
        Require(storyStages.Length == stages.Length, "fabuła: tyle wiadomości, ile etapów");

        var acts = d.GetProperty("acts").EnumerateArray().Select(a => new ActDef(Str(a, "name"), Int(a, "bonusPerStage"), Int(a, "bonusPerKill"))).ToArray();
        for (var ai = 0; ai < acts.Length; ai++)
        {
            var last = Array.FindLastIndex(stages, s => s.Act == ai);
            Require(last >= 0 && stages[last].Boss >= 0, $"akt {ai} bez bossa");
        }

        var hurtownia = d.GetProperty("hurtownia").EnumerateArray().Select(it => new ShopItemDef(
            Str(it, "id", ""), Str(it, "name"), Str(it, "desc"), Int(it, "price"), ParseEnum<ShopEffect>(Str(it, "effect")))).ToArray();

        var badgesJson = d.GetProperty("badges").EnumerateArray().ToArray();
        var badges = badgesJson.Select(b => new BadgeDef(Str(b, "id"), Str(b, "name"), Str(b, "desc"), Int(b, "xp"),
            b.TryGetProperty("perk", out var pk) ? new Perk(ParsePerk(Str(pk, "effect")), Int(pk, "value")) : new Perk(PerkEffect.Unknown, 0))).ToArray();
        Require(badges.Length <= 16 && enemies.Length <= 16, "maks. 16 odznak i 16 rodzajów wrogów");
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
        foreach (var sl in eq.GetProperty("slots").EnumerateArray())
        {
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

        var startTools = 0;
        for (var i = 0; i < tools.Length; i++)
        {
            if (tools[i].Cost == 0) startTools |= 1 << i;
        }
        var startClasses = 0;
        foreach (var c in meta.GetProperty("startClasses").EnumerateArray()) startClasses |= 1 << Lookup(cid, c.GetString() ?? "", "zawód");

        int BadgeIdx(string id) => Array.FindIndex(badges, b => b.Id == id);

        return new GameData
        {
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
            DefaultDifficulty = Int(d, "defaultDifficulty"),
            NgHpPctPerTier = Int(ng, "hpPctPerTier"),
            NgDmgBonusPerTier = Int(ng, "dmgBonusPerTier"),
            NgScorePctPerTier = Int(ng, "scorePctPerTier"),
            Statuses = [new StatusDef("", "", ""), StatusOf("poison"), StatusOf("shock"), StatusOf("slip"), StatusOf("paper")],
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
        };
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

    private static UpgradeEffect ParseUpgrade(string s) =>
        Enum.TryParse<UpgradeEffect>(s, ignoreCase: true, out var v) && v != UpgradeEffect.Unknown ? v : UpgradeEffect.Unknown;

    private static T ParseEnum<T>(string s) where T : struct, Enum =>
        Enum.TryParse<T>(s, ignoreCase: true, out var v) ? v : throw new GameDataException($"nieznana wartość {typeof(T).Name}: {s}");

    private static void Require(bool ok, string what)
    {
        if (!ok) throw new GameDataException(what);
    }
}
