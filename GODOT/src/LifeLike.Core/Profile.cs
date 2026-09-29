using System.Buffers.Binary;
using System.Text;

namespace LifeLike.Core;

/// <summary>
/// Profil gracza (odpowiednik core::profile z meta.h): rekord, doświadczenie, zakupy, odznaki, Osiedle.
/// ToBytes/FromBytes zachowują układ zapisu SRAM z GBA (v11: 188 bajtów, little-endian, bajt 55 to wyrównanie),
/// więc migracje v1–v10 działają tak samo.
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
    public const int Size = 188;
    public const int WeeklySlots = 3;
    public const int MaxRespect = 16;
    /// <summary>Zawody 0-7: bitmaska Classes (Szkolenia), 8-11: tylko z nagród za odbiór.</summary>
    public const int MaxClasses = 12;
    public const int MaxKeepsakes = 8;
    public const int DailySlots = 5;
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
        };
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
