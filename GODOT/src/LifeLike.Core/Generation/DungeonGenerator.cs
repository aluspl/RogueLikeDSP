using LifeLike.Core.Grid;

namespace LifeLike.Core.Generation;

public sealed record Room(int X, int Y, int W, int H)
{
    public GridPos Center => new(X + W / 2, Y + H / 2);
    public bool Intersects(Room o, int margin = 1) =>
        X - margin < o.X + o.W && X + W + margin > o.X && Y - margin < o.Y + o.H && Y + H + margin > o.Y;
}

public sealed record Dungeon(TileGrid Map, IReadOnlyList<Room> Rooms, GridPos PlayerStart);

/// <summary>
/// Klasyczny generator "pokoje + korytarze w kształcie L". Działa w przestrzeni logicznej,
/// więc zmiana renderu (2D -> 2.5D) nie wymaga zmian tutaj. Ten sam seed = ta sama mapa.
/// </summary>
public static class DungeonGenerator
{
    public static Dungeon Generate(int width, int height, int seed, int maxRooms = 12, int minSize = 4, int maxSize = 9)
    {
        var rng = new Random(seed);
        var map = new TileGrid(width, height);
        for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                map.SetWall(new GridPos(x, y));

        var rooms = new List<Room>();
        for (var attempt = 0; attempt < maxRooms * 10 && rooms.Count < maxRooms; attempt++)
        {
            var w = rng.Next(minSize, maxSize + 1);
            var h = rng.Next(minSize, maxSize + 1);
            var room = new Room(rng.Next(1, width - w - 1), rng.Next(1, height - h - 1), w, h);
            if (rooms.Any(r => r.Intersects(room))) continue;

            Carve(map, room);
            if (rooms.Count > 0) CarveCorridor(map, rooms[^1].Center, room.Center, rng.Next(2) == 0);
            rooms.Add(room);
        }

        if (rooms.Count == 0) throw new InvalidOperationException("Mapa za mała na choćby jeden pokój");
        return new Dungeon(map, rooms, rooms[0].Center);
    }

    private static void Carve(TileGrid map, Room r)
    {
        for (var x = r.X; x < r.X + r.W; x++)
            for (var y = r.Y; y < r.Y + r.H; y++)
                map.SetWall(new GridPos(x, y), false);
    }

    private static void CarveCorridor(TileGrid map, GridPos a, GridPos b, bool horizontalFirst)
    {
        var corner = horizontalFirst ? new GridPos(b.X, a.Y) : new GridPos(a.X, b.Y);
        CarveLine(map, a, corner);
        CarveLine(map, corner, b);
    }

    private static void CarveLine(TileGrid map, GridPos from, GridPos to)
    {
        var p = from;
        map.SetWall(p, false);
        while (p != to)
        {
            p = new GridPos(p.X + Math.Sign(to.X - p.X), p.Y + Math.Sign(to.Y - p.Y));
            map.SetWall(p, false);
        }
    }
}
