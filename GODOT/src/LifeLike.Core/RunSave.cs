using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// Zapis budowy w trakcie (odpowiednik core::run_save): magia, rozmiar i suma kontrolna FNV-1a
/// odrzucają zapisy uszkodzone i z innej wersji gry.
/// </summary>
public sealed class RunSave
{
    public const string RunMagic = "PBRUN02"; // 02: szczęście, cechy sprzętu, termos

    public byte[] Magic = new byte[8];
    public uint Size;
    public uint Checksum;
    public byte[] Data = [];

    public static uint Fnv1a(ReadOnlySpan<byte> b)
    {
        var h = 2166136261u;
        foreach (var x in b)
        {
            h ^= x;
            h *= 16777619u;
        }
        return h;
    }

    public static int GameSize(GameData d) => new Game(d).ToBytes().Length;

    public static RunSave Make(Game g)
    {
        var data = g.ToBytes();
        return new RunSave { Magic = Profile.MagicBytes(RunMagic), Size = (uint)data.Length, Data = data, Checksum = Fnv1a(data) };
    }

    public bool Valid(GameData d) =>
        Magic.AsSpan().SequenceEqual(Profile.MagicBytes(RunMagic)) && Size == GameSize(d) && Data.Length == Size && Checksum == Fnv1a(Data);

    public void Clear() => Magic = new byte[8];

    public Game Load(GameData d)
    {
        var g = new Game(d);
        g.FromBytes(Data);
        return g;
    }

    public byte[] ToBytes()
    {
        using var ms = new MemoryStream();
        using (var w = new BinaryWriter(ms))
        {
            w.Write(Magic);
            w.Write(Size);
            w.Write(Checksum);
            w.Write(Data);
        }
        return ms.ToArray();
    }

    public static RunSave FromBytes(byte[] raw)
    {
        using var r = new BinaryReader(new MemoryStream(raw));
        var s = new RunSave { Magic = r.ReadBytes(8), Size = r.ReadUInt32(), Checksum = r.ReadUInt32() };
        s.Data = r.ReadBytes(raw.Length - 16);
        return s;
    }
}
