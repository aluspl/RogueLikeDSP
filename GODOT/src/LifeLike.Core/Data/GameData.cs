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
    /// <summary>Indeks = slot * 3 + jakość.</summary>
    public GearDef[] Gear { get; private init; } = [];
    public string[] GearSlots { get; private init; } = [];
    public string[] GearRarities { get; private init; } = [];
    /// <summary>Wagi dropów w kolejności typów znajdziek: kawa, kask, projekt, narzędzie, sprzęt.</summary>
    public int[] DropWeights { get; private init; } = [];
    public int[] LevelThresholds { get; private init; } = [];

    public int SlamEvery { get; private init; }
    public int SlamDamageBonus { get; private init; }
    public int SlamRadius { get; private init; }
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

    public int MaxHeroLevel => LevelThresholds.Length + 1;
    public int GearSlotsCount => GearSlots.Length;

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
                Int(c, "agility"), Int(c, "intelligence"), Int(c, "defense"), Lookup(wid, Str(c, "weapon"), "broń"),
                Int(c, "frame"), Str(ab, "name"), Str(ab, "desc"), ParseEnum<AbilityEffect>(Str(ab, "effect")),
                Int(ab, "cooldown"));
        }).ToArray();

        var enemies = enemiesJson.Select(e =>
        {
            var hasHit = e.TryGetProperty("onHit", out var hit);
            return new EnemyDef(Str(e, "id"), Str(e, "name"), Str(e, "desc"), Int(e, "maxHealth"), Int(e, "minDamage"),
                Int(e, "maxDamage"), Int(e, "defense"), Int(e, "sight"), Int(e, "score"), Int(e, "frame"),
                e.TryGetProperty("slam", out var slam) && slam.GetBoolean(),
                hasHit && hit.TryGetProperty("status", out var s) ? ParseEnum<StatusEffect>(s.GetString() ?? "none") : StatusEffect.None,
                hasHit ? Int(hit, "chancePct", 0) : 0,
                hasHit ? Int(hit, "turns", 0) : 0);
        }).ToArray();

        var stages = d.GetProperty("stages").EnumerateArray().Select(st =>
        {
            var pool = st.GetProperty("enemies").EnumerateArray().Select(x => Lookup(eid, x.GetString() ?? "", "wróg")).ToArray();
            Require(pool.Length is >= 1 and <= 4, "etap: 1-4 rodzaje wrogów");
            var boss = st.TryGetProperty("boss", out var b) ? Lookup(eid, b.GetString() ?? "", "boss") : -1;
            return new StageDef(Str(st, "name"), pool, Int(st, "count"), boss, Int(st, "hpPct", 100), Int(st, "dmgBonus", 0), Int(st, "act"));
        }).ToArray();

        var difficulties = d.GetProperty("difficulties").EnumerateArray().Select(x => new DifficultyDef(
            Str(x, "id", ""), Str(x, "name"), Int(x, "hpPct"), Int(x, "dmgBonus"), Int(x, "scorePct"))).ToArray();

        var meta = d.GetProperty("meta");
        var upgrades = meta.GetProperty("upgrades").EnumerateArray().Select(u => new UpgradeDef(
            Str(u, "id", ""), Str(u, "name"), Str(u, "desc"), ParseEnum<UpgradeEffect>(Str(u, "effect")), Int(u, "value"),
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
        var badges = badgesJson.Select(b => new BadgeDef(Str(b, "id"), Str(b, "name"), Str(b, "desc"), Int(b, "xp"))).ToArray();
        Require(badges.Length <= 16 && enemies.Length <= 16, "maks. 16 odznak i 16 rodzajów wrogów");

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
            Tools = tools,
            Gear = gear.ToArray(),
            GearSlots = slots.ToArray(),
            GearRarities = rarities,
            DropWeights = [Int(weights, "coffee"), Int(weights, "helmet"), Int(weights, "plan"), Int(weights, "tool"), Int(weights, "gear")],
            LevelThresholds = hl.GetProperty("thresholds").EnumerateArray().Select(x => x.GetInt32()).ToArray(),
            SlamEvery = Int(slamJson, "every"),
            SlamDamageBonus = Int(slamJson, "damageBonus"),
            SlamRadius = Int(slamJson, "radius"),
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

    private static T ParseEnum<T>(string s) where T : struct, Enum =>
        Enum.TryParse<T>(s, ignoreCase: true, out var v) ? v : throw new GameDataException($"nieznana wartość {typeof(T).Name}: {s}");

    private static void Require(bool ok, string what)
    {
        if (!ok) throw new GameDataException(what);
    }
}
