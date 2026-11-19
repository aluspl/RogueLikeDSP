using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// Zapis budowy w trakcie (odpowiednik core::run_save): magia, rozmiar i suma kontrolna FNV-1a
/// odrzucają zapisy uszkodzone i z innej wersji gry.
/// </summary>
public sealed class RunSave
{
    public const string RunMagic = "PBRUN20"; // 20: plecak (v0.23.2; PBRUN19 wczytuje się z pustym plecakiem); 19: artefakty (licznik ciosów, Gablota w budowie); 18: profity broni (v0.23.0); 17: tryb B broni, stany problemów (v0.23.0); 13: podsumowanie budowy, wyzwanie tygodnia; 12: wydarzenia z wyborem, ulepszenie narzędzia, magazyn; 11: premie po etapie, elity, kombinacje stanów; 10: Akt 0; 09: 10 etapów, zachowania

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

    /// <summary>v0.21.52 cz. c: stan sprzed liczników zadań (2 bajty krócej, na końcu) – wczytuje się z zerami.</summary>
    public const int V14Tail = 2;
    /// <summary>v0.21.52 cz. d: stan sprzed mapy kariery (kontrakt i bliźniak – 2 bajty na końcu) – Dom jednorodzinny.</summary>
    public const int V15Tail = 2;

    /// <summary>v0.23.0: stan sprzed Kawki (4 bajty na końcu) – wczytuje się z zerami (bez Kawki); ta sama magia PBRUN19.</summary>
    public const int DogTail = 4;

    /// <summary>v0.23.2: plecak (6 x 7 bajtów) i znajdźka oferty sprzętu na końcu stanu – zapis PBRUN19 nie ma ich wcale.</summary>
    public const int BackpackTail = Game.MaxBackpack * 7 + 1;

    /// <summary>v0.23.2: poprzednia magia (stan bez plecaka) – wczytywana z migracją.</summary>
    public const string PrevMagic = "PBRUN19";

    private bool IsPrev => Magic.AsSpan().SequenceEqual(Profile.MagicBytes(PrevMagic));

    private bool IsCurrent => Magic.AsSpan().SequenceEqual(Profile.MagicBytes(RunMagic));

    public bool Valid(GameData d)
    {
        if (!(IsCurrent || IsPrev) || Data.Length != Size || Checksum != Fnv1a(Data)) return false;
        var full = GameSize(d);
        if (IsCurrent) return Size == full;
        foreach (var cut in new[] { 0, DogTail, DogTail + V15Tail, DogTail + V15Tail + V14Tail }) // PBRUN19: bez plecaka (i starsze ogony)
        {
            if (Size == full - BackpackTail - cut) return true;
        }
        return false;
    }

    public void Clear() => Magic = new byte[8];

    /// <summary>
    /// Wczytuje stan; zapis z innej wersji (inna magia, np. PBRUN13) = null, bez wyjątku (v0.23.0, decyzja 7).
    /// v0.23.2: PBRUN19 (bez plecaka) – migracja: pusty plecak, oferta sprzętu bez znajdźki.
    /// </summary>
    public Game Load(GameData d)
    {
        var prev = IsPrev;
        if (!IsCurrent && !prev) return null;
        var g = new Game(d);
        var size = GameSize(d);
        if (Data.Length < size) // v0.21.52 cz. c / d, v0.23.2: starszy stan bez ogona (zadania, kontrakt, Kawka, plecak) – zera
        {
            var padded = new byte[size];
            Data.AsSpan(0, Math.Min(Data.Length, size)).CopyTo(padded);
            g.FromBytes(padded);
        }
        else
        {
            g.FromBytes(Data);
        }
        if (prev)
        {
            Array.Clear(g.Backpack);
            g.OfferPickup = -1;
        }
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
