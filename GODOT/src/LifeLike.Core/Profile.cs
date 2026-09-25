using System.Buffers.Binary;
using System.Text;

namespace LifeLike.Core;

/// <summary>
/// Profil gracza (odpowiednik core::profile z meta.h): rekord, doświadczenie, zakupy, odznaki, Osiedle.
/// ToBytes/FromBytes zachowują układ zapisu SRAM z GBA (56 bajtów, little-endian), więc migracje v1/v2 działają tak samo.
/// </summary>
public sealed class Profile
{
    public const int MaxUpgrades = 8;
    public const int MaxHouses = 12;
    public const int V2Size = 36;
    public const int Size = 56;
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
        };
        return p;
    }
}
