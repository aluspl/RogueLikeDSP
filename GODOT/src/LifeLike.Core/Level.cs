namespace LifeLike.Core;

/// <summary>Mapa etapu 32x32: generator pokoje + korytarze w kształcie L (port core::level).</summary>
public sealed class Level
{
    public const int W = 32;
    public const int H = 32;
    public const int MaxRooms = 10;

    /// <summary>Pola mapy, indeks y * W + x.</summary>
    public readonly Tile[] T = new Tile[W * H];
    public readonly Room[] Rooms = new Room[MaxRooms];
    public int RoomsCount;

    public static bool In(int x, int y) => x >= 0 && y >= 0 && x < W && y < H;

    /// <summary>Bezpośredni dostęp do pola (bez sprawdzania granic).</summary>
    public Tile this[int x, int y]
    {
        get => T[y * W + x];
        set => T[y * W + x] = value;
    }

    public Tile At(int x, int y) => In(x, y) ? T[y * W + x] : Tile.Wall;

    public bool Passable(int x, int y) => At(x, y) != Tile.Wall;

    public void Carve(int x, int y)
    {
        if (In(x, y) && T[y * W + x] == Tile.Wall) T[y * W + x] = Tile.Floor;
    }

    public void Fill(Tile t) => Array.Fill(T, t);

    public void Generate(ref Rng r)
    {
        Array.Fill(T, Tile.Wall);
        RoomsCount = 0;
        for (var attempt = 0; attempt < MaxRooms * 12 && RoomsCount < MaxRooms; ++attempt)
        {
            var rm = new Room();
            rm.W = (sbyte)r.Range(4, 8);
            rm.H = (sbyte)r.Range(4, 7);
            rm.X = (sbyte)r.Range(1, W - rm.W - 2);
            rm.Y = (sbyte)r.Range(1, H - rm.H - 2);
            var ok = true;
            for (var i = 0; i < RoomsCount; ++i)
            {
                if (Rooms[i].Intersects(rm))
                {
                    ok = false;
                    break;
                }
            }
            if (!ok) continue;
            for (var y = rm.Y; y < rm.Y + rm.H; ++y)
                for (var x = rm.X; x < rm.X + rm.W; ++x)
                    Carve(x, y);
            if (RoomsCount > 0) // korytarz w kształcie L do poprzedniego pokoju
            {
                var p = Rooms[RoomsCount - 1];
                int ax = p.Cx, ay = p.Cy, bx = rm.Cx, by = rm.Cy;
                var hfirst = (r.Next() & 1) != 0;
                int kx = hfirst ? bx : ax, ky = hfirst ? ay : by;
                for (int x = ax, y = ay; ;)
                {
                    Carve(x, y);
                    if (x == kx && y == ky) break;
                    x += Math.Sign(kx - x);
                    y += Math.Sign(ky - y);
                }
                for (int x = kx, y = ky; ;)
                {
                    Carve(x, y);
                    if (x == bx && y == by) break;
                    x += Math.Sign(bx - x);
                    y += Math.Sign(by - y);
                }
            }
            Rooms[RoomsCount++] = rm;
        }
    }

    public void CopyFrom(Level o)
    {
        Array.Copy(o.T, T, T.Length);
        Array.Copy(o.Rooms, Rooms, Rooms.Length);
        RoomsCount = o.RoomsCount;
    }
}
