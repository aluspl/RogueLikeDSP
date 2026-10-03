using System.Buffers.Binary;
using System.Text;

namespace LifeLike.Core;

/// <summary>
/// Profil gracza (odpowiednik core::profile z meta.h): rekord, doświadczenie, zakupy, odznaki, Osiedle.
/// ToBytes/FromBytes zachowują układ zapisu SRAM z GBA (v15: 384 bajty, little-endian, bajt 55 to wyrównanie),
/// więc migracje v1–v14 działają tak samo.
/// </summary>
public sealed class Profile
{
    public const int MaxUpgrades = 8;
    public const int MaxHouses = 12;
    public const int V2Size = 36;
    /// <summary>v4 = v3 + zlecenia i pamiątki od tego offsetu.</summary>
    public const int V3Size = 56;
    /// <summary>v5 = v4 + liczniki zleceń przeniesione z bieżącej budowy od tego offsetu.</summary>
    public const int V4Size = 72;
    /// <summary>v6 = v5 + brygada i tryb inwestora od tego offsetu.</summary>
    public const int V5Size = 78;
    /// <summary>v7 = v6 + codzienna budowa (data, najlepsze wyniki dni) od tego offsetu.</summary>
    public const int V6Size = 88;
    /// <summary>v8 = v7 + Respekt, nagrody za odbiór, wygrane i stawki zawodów 8-11 od tego offsetu.</summary>
    public const int V7Size = 124;
    /// <summary>v9 = v8 + katalog usterek 16-47 od tego offsetu.</summary>
    public const int V8Size = 152;
    /// <summary>v10 = v9 + samouczek menu (obejrzane dymki) od tego offsetu.</summary>
    public const int V9Size = 156;
    /// <summary>v11 = v10 + wyzwania tygodnia i fabuła od tego offsetu.</summary>
    public const int V10Size = 160;
    /// <summary>v12 = v11 + sekretne zlecenia (#39), wygląd, Respekt 16-18 od tego offsetu.</summary>
    public const int V11Size = 188;
    /// <summary>v13 = v12 + tytuł i kolor kasku (v0.21.52) od tego offsetu.</summary>
    public const int V12Size = 196;
    /// <summary>v14 = v13 + poziom inspektora, mistrzostwo zawodów, druga pamiątka (v0.21.52 cz. b) od tego offsetu.</summary>
    public const int V13Size = 200;
    /// <summary>v15 = v14 + drzewko Szkoleń, kolekcje, zadania dnia, seria dni (v0.21.52 cz. c) od tego offsetu.</summary>
    public const int V14Size = 240;
    public const int Size = 384;
    /// <summary>Rodzaje problemów (liczniki kolekcji, jak core::max_enemy_types).</summary>
    public const int MaxEnemyTypes = 48;
    /// <summary>3 zadania dnia + 2 tygodnia.</summary>
    public const int TaskSlots = 5;
    public const int DailyTaskSlots = 3;
    public const int WeeklySlots = 3;
    public const int MaxRespect = 16;
    /// <summary>Rangi Respektu 16-18 (v12, dalszy ciąg RespectRanks).</summary>
    public const int MaxRespectHi = 3;
    /// <summary>Zawody 0-7: bitmaska Classes (Szkolenia), 8-11: tylko z nagród za odbiór.</summary>
    public const int MaxClasses = 12;
    public const int MaxKeepsakes = 8;
    public const int DailySlots = 5;
    /// <summary>Bieżący format (v15, v0.21.52 cz. c).</summary>
    public const string MagicCurrent = "PBRL015";
    public const string MagicV14 = "PBRL014";
    public const string MagicV13 = "PBRL013";
    public const string MagicV12 = "PBRL012";
    public const string MagicV11 = "PBRL011";
    public const string MagicV10 = "PBRL010";
    public const string MagicV9 = "PBRL009";
    public const string MagicV8 = "PBRL008";
    public const string MagicV7 = "PBRL007";
    public const string MagicV6 = "PBRL006";
    public const string MagicV5 = "PBRL005";
    public const string MagicV4 = "PBRL004";
    public const string MagicV3 = "PBRL003";
    public const string MagicV2 = "PBRL002";
    public const string MagicV1 = "PBRL001";

    public const byte FlagHelpSeen = 1;
    public const byte FlagPrologueSeen = 2;

    /// <summary>8 bajtów (7 znaków + zero).</summary>
    public byte[] Magic = new byte[8];
    public int Best, Runs, Wins;
    /// <summary>Doświadczenie do wydania.</summary>
    public int Xp;
    public byte[] Levels = new byte[MaxUpgrades];
    public byte Classes;
    public byte Hard;
    public byte Flags;
    public byte Tools;
    public ushort Badges;
    public ushort Catalog;
    public byte ClassWins;
    public byte ToolsFound;
    public byte HousesCount;
    /// <summary>Osiedle: zawód (4 bity) | wielkość domu &lt;&lt; 4.</summary>
    public byte[] Houses = new byte[MaxHouses];
    // --- v4: zlecenia i pamiątki
    /// <summary>Problemy usunięte we wszystkich budowach.</summary>
    public ushort KillsTotal;
    /// <summary>Użycia mocy zawodu.</summary>
    public ushort PowersTotal;
    /// <summary>Założone markowe przedmioty.</summary>
    public byte BrandTotal;
    /// <summary>Bossowie aktu pokonani bez obrażeń w walce z nimi.</summary>
    public byte CleanBosses;
    /// <summary>Ukończone zlecenia (bitmaska GameData.Contracts).</summary>
    public byte Contracts;
    /// <summary>Wybrana pamiątka + 1 (0 = bez pamiątki).</summary>
    public byte Keepsake;
    /// <summary>Budowy z każdą pamiątką (ranga).</summary>
    public byte[] KeepsakeRuns = new byte[MaxKeepsakes];
    // --- v5: ile liczników zleceń bieżącej budowy już przeniesiono do *Total (znak wodny; zapisywany razem
    // z sumami, więc wznowienie budowy z autozapisu na starcie etapu nie liczy etapu drugi raz)
    public ushort RunKills;
    public ushort RunPowers;
    public byte RunBrand;
    public byte RunClean;
    // --- v6: brygada i tryb inwestora
    /// <summary>Kupieni w Szkoleniach fachowcy brygady (bitmaska; startowi zawsze dostępni).</summary>
    public byte Brigade;
    /// <summary>Włączone modyfikatory trybu inwestora (bitmaska GameData.Investor).</summary>
    public byte Investor;
    /// <summary>Najwyższa stawka wygranej budowy na każdy zawód.</summary>
    public byte[] BestStake = new byte[8];
    // --- v7: codzienna budowa (GBA: data wpisana ręcznie; Godot: data z systemu)
    /// <summary>Ostatnio wpisana data (0 = domyślna z danych).</summary>
    public byte DailyD, DailyM;
    public ushort DailyY;
    /// <summary>Numery dni z wynikiem (0 = pusty).</summary>
    public ushort[] DailyDay = new ushort[DailySlots];
    /// <summary>Bity: wygrana tego dnia.</summary>
    public byte DailyWon;
    /// <summary>Rozegrane codzienne budowy (do 255).</summary>
    public byte DailyRuns;
    /// <summary>Najlepszy wynik dnia.</summary>
    public int[] DailyScore = new int[DailySlots];
    // --- v8: Respekt (stała waluta za etapy) i nagrody za odbiór (każda wygrana odblokowuje kolejną)
    /// <summary>Respekt do wydania.</summary>
    public ushort Respect;
    /// <summary>Respekt zdobyty łącznie.</summary>
    public ushort RespectTotal;
    /// <summary>Ile Respektu bieżącej budowy już przeniesiono (znak wodny jak RunKills).</summary>
    public ushort RunRespect;
    /// <summary>Odblokowane nagrody za odbiór (pierwsze N z GameData.Rewards).</summary>
    public byte Rewards;
    /// <summary>Zawody 8-15, którymi wygrano (dalszy ciąg ClassWins).</summary>
    public byte ClassWinsHi;
    /// <summary>Kupione rangi Respektu.</summary>
    public byte[] RespectRanks = new byte[MaxRespect];
    /// <summary>Rekord stawki zawodów 8-11.</summary>
    public byte[] BestStakeHi = new byte[4];
    /// <summary>v9: katalog usterek – rodzaje problemów 16-47 (dalszy ciąg Catalog).</summary>
    public uint CatalogHi;
    // --- v10: samouczek menu
    /// <summary>Obejrzane dymki samouczka (bity Tutorial).</summary>
    public ushort Tutorial;
    /// <summary>Zawody z nagród, o których już był dymek odblokowania (bitmaska).</summary>
    public ushort ClassesSeen;
    // --- v11: wyzwania tygodnia (#34) i fabuła odkrywana z budowami (#35)
    /// <summary>Numery tygodni z wynikiem (0 = pusty).</summary>
    public ushort[] WeeklyWeek = new ushort[WeeklySlots];
    /// <summary>Bity: wygrana w tym tygodniu.</summary>
    public byte WeeklyWon;
    /// <summary>Rozegrane wyzwania tygodnia (do 255).</summary>
    public byte WeeklyRuns;
    /// <summary>Najlepszy wynik tygodnia.</summary>
    public int[] WeeklyScore = new int[WeeklySlots];
    /// <summary>Odblokowane wątki fabuły (bity GameData.StoryArc).</summary>
    public uint Story;
    /// <summary>Jeszcze nieprzeczytane wątki.</summary>
    public uint StoryNew;
    // --- v12: sekretne zlecenia (#39)
    /// <summary>Wykonane sekretne zlecenia (bity GameData.Secrets).</summary>
    public ushort Secrets;
    /// <summary>Wykonane, dymek „Nowość” na tytule jeszcze nie pokazany.</summary>
    public ushort SecretsNew;
    /// <summary>Wybrany wygląd (bity GameData.Cosmetics; działa tylko odblokowany).</summary>
    public byte Cosmetic;
    /// <summary>Kupione rangi Respektu 16-18.</summary>
    public byte[] RespectRanksHi = new byte[MaxRespectHi];
    // --- v13 (v0.21.52): tytuł z odznaki / zlecenia, kolor kasku
    /// <summary>Wybrany tytuł + 1 (0 = bez tytułu): odznaki 0..N-1, potem zlecenia.</summary>
    public byte Title;
    /// <summary>Wybrany kolor kasku: wygląd + 1 (0 = kask zawodu).</summary>
    public byte Helmet;
    // --- v14 (v0.21.52 cz. b): poziom inspektora (#44), mistrzostwo zawodów (#45), druga pamiątka
    /// <summary>Dośw. inspektora łącznie (poziom z progów GameData.InspectorLevels).</summary>
    public uint InspectorXp;
    /// <summary>Dośw. mistrzostwa każdego zawodu (poziom 1-10 z GameData.MasteryLevels).</summary>
    public ushort[] MasteryXp = new ushort[MaxClasses];
    /// <summary>Bity: wariant mocy włączony (zawód; działa od poziomu mistrzostwa z nagrodą „power”).</summary>
    public ushort PowerAlt;
    /// <summary>Ile dośw. inspektora z bieżącej budowy już przeniesiono (znak wodny jak RunKills).</summary>
    public ushort RunProgress;
    /// <summary>Druga pamiątka + 1 (0 = bez; slot z poziomu inspektora).</summary>
    public byte Keepsake2;
    // --- v15 (v0.21.52 cz. c): drzewko Szkoleń, kolekcje, zadania dnia i tygodnia, seria dni
    /// <summary>Wybór w węzłach drzewka: 2 bity na węzeł (0 brak, 1 = opcja A, 2 = opcja B).</summary>
    public ushort Tree;
    /// <summary>Kolekcje: pokonane każdego rodzaju problemu (do 255).</summary>
    public byte[] KillCount = new byte[MaxEnemyTypes];
    /// <summary>Ile z bieżącej budowy już doliczono do KillCount (znak wodny jak RunKills).</summary>
    public byte[] KillMark = new byte[MaxEnemyTypes];
    /// <summary>Dzień zadań dnia (numer budowy dnia), 0 = jeszcze żadnych.</summary>
    public ushort TaskDay;
    /// <summary>Tydzień zadań tygodnia (numer wyzwania tygodnia).</summary>
    public ushort TaskWeek;
    /// <summary>Postęp zadań: 3 dnia, 2 tygodnia.</summary>
    public byte[] TaskProgress = new byte[TaskSlots];
    /// <summary>Ile z bieżącej budowy już doliczono (znak wodny).</summary>
    public byte[] TaskMark = new byte[TaskSlots];
    /// <summary>Bity: zadanie wykonane (Respekt wydany).</summary>
    public byte TaskDone;
    /// <summary>Seria dni budowy dnia (kolejne dni).</summary>
    public byte Streak;
    /// <summary>Wykonane zadania łącznie (nagrody za liczbę).</summary>
    public ushort TasksTotal;
    /// <summary>Ostatni dzień serii (numer budowy dnia).</summary>
    public ushort StreakDay;
    /// <summary>Najdłuższa seria (nagrody za 3 / 7 / 14 dni).</summary>
    public byte StreakBest;
    /// <summary>Bity: ogłoszone komplety kolekcji (baner raz).</summary>
    public byte Collections;

    public static byte[] MagicBytes(string s)
    {
        var b = new byte[8];
        Encoding.ASCII.GetBytes(s, b);
        return b;
    }

    public bool MagicIs(string s) => Magic.AsSpan().SequenceEqual(MagicBytes(s));

    public string MagicText => Encoding.ASCII.GetString(Magic).TrimEnd('\0');

    public bool HasFlag(byte f) => (Flags & f) != 0;

    public void SetFlag(byte f) => Flags = (byte)(Flags | f);

    public byte[] ToBytes()
    {
        var b = new byte[Size];
        Magic.CopyTo(b, 0);
        BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(8), Best);
        BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(12), Runs);
        BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(16), Wins);
        BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(20), Xp);
        Levels.CopyTo(b, 24);
        b[32] = Classes;
        b[33] = Hard;
        b[34] = Flags;
        b[35] = Tools;
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(36), Badges);
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(38), Catalog);
        b[40] = ClassWins;
        b[41] = ToolsFound;
        b[42] = HousesCount;
        Houses.CopyTo(b, 43);
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(56), KillsTotal);
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(58), PowersTotal);
        b[60] = BrandTotal;
        b[61] = CleanBosses;
        b[62] = Contracts;
        b[63] = Keepsake;
        KeepsakeRuns.CopyTo(b, 64);
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(72), RunKills);
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(74), RunPowers);
        b[76] = RunBrand;
        b[77] = RunClean;
        b[78] = Brigade;
        b[79] = Investor;
        BestStake.CopyTo(b, 80);
        b[88] = DailyD;
        b[89] = DailyM;
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(90), DailyY);
        for (var i = 0; i < DailySlots; i++) BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(92 + i * 2), DailyDay[i]);
        b[102] = DailyWon;
        b[103] = DailyRuns;
        for (var i = 0; i < DailySlots; i++) BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(104 + i * 4), DailyScore[i]);
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(124), Respect);
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(126), RespectTotal);
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(128), RunRespect);
        b[130] = Rewards;
        b[131] = ClassWinsHi;
        RespectRanks.CopyTo(b, 132);
        BestStakeHi.CopyTo(b, 148);
        BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(152), CatalogHi);
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(156), Tutorial);
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(158), ClassesSeen);
        for (var i = 0; i < WeeklySlots; i++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(160 + i * 2), WeeklyWeek[i]);
            BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(168 + i * 4), WeeklyScore[i]);
        }
        b[166] = WeeklyWon;
        b[167] = WeeklyRuns;
        BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(180), Story);
        BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(184), StoryNew);
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(188), Secrets);
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(190), SecretsNew);
        b[192] = Cosmetic;
        RespectRanksHi.CopyTo(b, 193);
        b[196] = Title;
        b[197] = Helmet;
        BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(200), InspectorXp);
        for (var i = 0; i < MaxClasses; i++) BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(204 + i * 2), MasteryXp[i]);
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(228), PowerAlt);
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(230), RunProgress);
        b[232] = Keepsake2;
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(240), Tree);
        KillCount.CopyTo(b, 242);
        KillMark.CopyTo(b, 290);
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(338), TaskDay);
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(340), TaskWeek);
        TaskProgress.CopyTo(b, 342);
        TaskMark.CopyTo(b, 347);
        b[352] = TaskDone;
        b[353] = Streak;
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(354), TasksTotal);
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(356), StreakDay);
        b[358] = StreakBest;
        b[359] = Collections;
        return b;
    }

    /// <summary>Wczytuje surowe bajty (bez naprawy – do tego służy Meta.ProfileFix). Krótszy bufor = zera na końcu.</summary>
    public static Profile FromBytes(ReadOnlySpan<byte> src)
    {
        Span<byte> b = stackalloc byte[Size];
        src[..Math.Min(src.Length, Size)].CopyTo(b);
        var p = new Profile
        {
            Magic = b[..8].ToArray(),
            Best = BinaryPrimitives.ReadInt32LittleEndian(b[8..]),
            Runs = BinaryPrimitives.ReadInt32LittleEndian(b[12..]),
            Wins = BinaryPrimitives.ReadInt32LittleEndian(b[16..]),
            Xp = BinaryPrimitives.ReadInt32LittleEndian(b[20..]),
            Levels = b.Slice(24, MaxUpgrades).ToArray(),
            Classes = b[32],
            Hard = b[33],
            Flags = b[34],
            Tools = b[35],
            Badges = BinaryPrimitives.ReadUInt16LittleEndian(b[36..]),
            Catalog = BinaryPrimitives.ReadUInt16LittleEndian(b[38..]),
            ClassWins = b[40],
            ToolsFound = b[41],
            HousesCount = b[42],
            Houses = b.Slice(43, MaxHouses).ToArray(),
            KillsTotal = BinaryPrimitives.ReadUInt16LittleEndian(b[56..]),
            PowersTotal = BinaryPrimitives.ReadUInt16LittleEndian(b[58..]),
            BrandTotal = b[60],
            CleanBosses = b[61],
            Contracts = b[62],
            Keepsake = b[63],
            KeepsakeRuns = b.Slice(64, MaxKeepsakes).ToArray(),
            RunKills = BinaryPrimitives.ReadUInt16LittleEndian(b[72..]),
            RunPowers = BinaryPrimitives.ReadUInt16LittleEndian(b[74..]),
            RunBrand = b[76],
            RunClean = b[77],
            Brigade = b[78],
            Investor = b[79],
            BestStake = b.Slice(80, 8).ToArray(),
            DailyD = b[88],
            DailyM = b[89],
            DailyY = BinaryPrimitives.ReadUInt16LittleEndian(b[90..]),
            DailyWon = b[102],
            DailyRuns = b[103],
            Respect = BinaryPrimitives.ReadUInt16LittleEndian(b[124..]),
            RespectTotal = BinaryPrimitives.ReadUInt16LittleEndian(b[126..]),
            RunRespect = BinaryPrimitives.ReadUInt16LittleEndian(b[128..]),
            Rewards = b[130],
            ClassWinsHi = b[131],
            RespectRanks = b.Slice(132, MaxRespect).ToArray(),
            BestStakeHi = b.Slice(148, 4).ToArray(),
            CatalogHi = BinaryPrimitives.ReadUInt32LittleEndian(b[152..]),
            Tutorial = BinaryPrimitives.ReadUInt16LittleEndian(b[156..]),
            ClassesSeen = BinaryPrimitives.ReadUInt16LittleEndian(b[158..]),
            WeeklyWon = b[166],
            WeeklyRuns = b[167],
            Story = BinaryPrimitives.ReadUInt32LittleEndian(b[180..]),
            StoryNew = BinaryPrimitives.ReadUInt32LittleEndian(b[184..]),
            Secrets = BinaryPrimitives.ReadUInt16LittleEndian(b[188..]),
            SecretsNew = BinaryPrimitives.ReadUInt16LittleEndian(b[190..]),
            Cosmetic = b[192],
            RespectRanksHi = b.Slice(193, MaxRespectHi).ToArray(),
            Title = b[196],
            Helmet = b[197],
            InspectorXp = BinaryPrimitives.ReadUInt32LittleEndian(b[200..]),
            PowerAlt = BinaryPrimitives.ReadUInt16LittleEndian(b[228..]),
            RunProgress = BinaryPrimitives.ReadUInt16LittleEndian(b[230..]),
            Keepsake2 = b[232],
            Tree = BinaryPrimitives.ReadUInt16LittleEndian(b[240..]),
            KillCount = b.Slice(242, MaxEnemyTypes).ToArray(),
            KillMark = b.Slice(290, MaxEnemyTypes).ToArray(),
            TaskDay = BinaryPrimitives.ReadUInt16LittleEndian(b[338..]),
            TaskWeek = BinaryPrimitives.ReadUInt16LittleEndian(b[340..]),
            TaskProgress = b.Slice(342, TaskSlots).ToArray(),
            TaskMark = b.Slice(347, TaskSlots).ToArray(),
            TaskDone = b[352],
            Streak = b[353],
            TasksTotal = BinaryPrimitives.ReadUInt16LittleEndian(b[354..]),
            StreakDay = BinaryPrimitives.ReadUInt16LittleEndian(b[356..]),
            StreakBest = b[358],
            Collections = b[359],
        };
        for (var i = 0; i < MaxClasses; i++) p.MasteryXp[i] = BinaryPrimitives.ReadUInt16LittleEndian(b[(204 + i * 2)..]);
        for (var i = 0; i < WeeklySlots; i++)
        {
            p.WeeklyWeek[i] = BinaryPrimitives.ReadUInt16LittleEndian(b[(160 + i * 2)..]);
            p.WeeklyScore[i] = BinaryPrimitives.ReadInt32LittleEndian(b[(168 + i * 4)..]);
        }
        for (var i = 0; i < DailySlots; i++)
        {
            p.DailyDay[i] = BinaryPrimitives.ReadUInt16LittleEndian(b[(92 + i * 2)..]);
            p.DailyScore[i] = BinaryPrimitives.ReadInt32LittleEndian(b[(104 + i * 4)..]);
        }
        return p;
    }
}
