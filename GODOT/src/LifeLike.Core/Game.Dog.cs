using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// v0.23.0: easter egg – Kawka (core.h: dog_*). Raz na budowę (GameData.DogChancePct) siedzi w błocie na jednym etapie Aktu I
/// bez bossa; nie problem: nie da się jej trafić, problemy ją omijają (Occupied). Pierwsze wejście = głaskanie i dośw. (raz),
/// kolejne – Kawka robi miejsce (zamiana pól, bez blokady przejścia). Losowanie z osobnego skrótu seeda budowy (RNG gry bez zmian).
/// </summary>
public sealed partial class Game
{
    public const byte DogOn = 1, DogPetted = 2, DogSpawned = 4, DogBanked = 8;

    /// <summary>Pole Kawki na etapie (ważne tylko z DogOn).</summary>
    public sbyte DogX = -1, DogY = -1;

    /// <summary>Bity DogOn, DogPetted (dośw. wydane), DogSpawned (już była w tej budowie), DogBanked (doliczona do profilu).</summary>
    public byte DogFlags;

    /// <summary>Ile razy pogłaskana w tej budowie (warstwa gry: ogłoszenie, dymek z oceną).</summary>
    public byte DogPets;

    public bool DogAt(int x, int y) => (DogFlags & DogOn) != 0 && DogX == x && DogY == y;

    public bool DogHere => (DogFlags & DogOn) != 0;

    public uint DogHash()
    {
        var h = unchecked((RunSeed ^ 0x4B41574Bu) * 2246822519u + 0x9E3779B9u);
        h ^= h >> 15;
        h = unchecked(h * 2654435761u);
        h ^= h >> 13;
        return h;
    }

    public bool DogStageOk(int s) => D.Acts[SDef(s).Act].Mechanic == ActMechanic.Mud && SDef(s).Boss < 0;

    /// <summary>Etap z Kawką w tej budowie (-1 = nie tym razem).</summary>
    public int DogStage()
    {
        var h = DogHash();
        if ((int)(h % 100u) >= D.DogChancePct) return -1;
        var n = 0;
        for (var s = (int)FirstStage; s < RouteCount(); ++s)
        {
            if (DogStageOk(s)) n++;
        }
        if (n == 0) return -1;
        var k = (int)((h >> 8) % (uint)n);
        for (var s = (int)FirstStage; s < RouteCount(); ++s)
        {
            if (DogStageOk(s) && k-- == 0) return s;
        }
        return -1;
    }

    private bool DogCellOk(int x, int y, bool needMud) =>
        Lv.At(x, y) == Tile.Floor && (!needMud || Mud(x, y)) && !Occupied(x, y) && !PickupAt(x, y) && !InSecret(x, y)
        && Cheb(x, y, Hero.X, Hero.Y) > 3;

    public void PlaceDog()
    {
        if (Tier > 0 || (DogFlags & DogSpawned) != 0 || Lv.RoomsCount < 2 || Stage != DogStage()) return;
        var dr = SideRng(131);
        for (var pass = 0; pass < 2; ++pass) // błoto we wnętrzu pokoju (nie w przejściu); bez błota – dowolna podłoga wnętrza
        {
            var n = 0;
            for (var i = 1; i < Lv.RoomsCount; ++i)
            {
                var rm = Lv.Rooms[i];
                for (var y = rm.Y + 1; y < rm.Y + rm.H - 1; ++y)
                {
                    for (var x = rm.X + 1; x < rm.X + rm.W - 1; ++x)
                    {
                        if (DogCellOk(x, y, pass == 0)) n++;
                    }
                }
            }
            if (n == 0) continue;
            var k = dr.Range(0, n - 1);
            for (var i = 1; i < Lv.RoomsCount; ++i)
            {
                var rm = Lv.Rooms[i];
                for (var y = rm.Y + 1; y < rm.Y + rm.H - 1; ++y)
                {
                    for (var x = rm.X + 1; x < rm.X + rm.W - 1; ++x)
                    {
                        if (!DogCellOk(x, y, pass == 0) || k-- != 0) continue;
                        DogX = (sbyte)x;
                        DogY = (sbyte)y;
                        DogFlags = (byte)(DogFlags | DogOn | DogSpawned);
                        return;
                    }
                }
            }
        }
    }

    /// <summary>Wejście na Kawkę: pierwsze = głaskanie z dośw. (raz na budowę), kolejne – zamiana pól (Kawka przepuszcza).</summary>
    public void PetDog()
    {
        if (DogPets < 255) DogPets++;
        if ((DogFlags & DogPetted) == 0)
        {
            DogFlags = (byte)(DogFlags | DogPetted);
            Push(Msg(Loc.T("kawka_glaszczesz")).Add(D.DogXp).Add(Loc.T("dosw")).As(LogKind.Good));
            GainXp(D.DogXp);
            return;
        }
        sbyte ox = Hero.X, oy = Hero.Y;
        Hero.X = DogX;
        Hero.Y = DogY;
        DogX = ox;
        DogY = oy;
        Push(Msg(Loc.T("kawka_przepuszcza")));
        Collect();
    }
}
