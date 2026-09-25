using System.Buffers.Binary;
using System.Text;

namespace LifeLike.Core;

/// <summary>
/// Profil gracza (odpowiednik core::profile z meta.h): rekord, doświadczenie, zakupy, odznaki, Osiedle.
/// ToBytes/FromBytes zachowują układ zapisu SRAM z GBA (v6: 88 bajtów, little-endian, bajt 55 to wyrównanie),
/// więc migracje v1/v2/v3/v4/v5 działają tak samo.
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
    public const int Size = 88;
    public const int MaxKeepsakes = 8;
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
        };
        return p;
    }
}
