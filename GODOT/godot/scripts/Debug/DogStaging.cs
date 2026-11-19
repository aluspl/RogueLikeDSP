using System;
using System.Threading.Tasks;
using LifeLike.Core;
using LifeLike.Game.Screens;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Debug;

/// <summary>
/// v0.23.0: easter egg – sceny zrzutów Kawki (też do reklamy): Kawka w błocie obok bohatera na Fundamentach (dog-map),
/// ogłoszenie głaskania – dłoń, serduszka, „Kawka” (dog-pet, zatrzymane w PB_ANNOUNCE_AT, domyślnie 0,7 s) i dymek z oceną
/// postępów nad nią (dog-quip, PB_BUBBLE_AT, domyślnie 1,0 s), wpis „Spotkana Kawka” w Katalogu (dog-catalog).
/// </summary>
public sealed class DogStaging
{
    public static readonly string[] Names = ["dog-map", "dog-pet", "dog-quip", "dog-catalog"];

    private readonly App _app;

    public DogStaging(App app) => _app = app;

    private ScreenFlow Flow => _app.Flow;

    public static bool Handles(string scene) => Array.IndexOf(Names, scene) >= 0;

    public async Task Setup(string scene)
    {
        if (scene == "dog-catalog")
        {
            var p = _app.Session.Profile;
            p.DogBuilds = Math.Max(p.DogBuilds, (byte)2);
            Flow.Profile.Open(1, true);
            var ct = (Phone.ProfileTabs.CatalogTab)_app.Nodes.Phone.Current;
            ct.Select(ct.DogRow);
            _app.Nodes.Phone.QueueRedraw();
            return;
        }
        _app.Session.ClassId = 1; // Murarz z kielnią
        _app.StartRun();
        new DebugScenes(_app).AdvanceMessages();
        await DebugRunner.Frames(_app.Root, 2);
        var g = _app.Session.Game;
        var (dx, dy) = Place(g);
        g.UpdateFov();
        _app.Session.ResetWatch();
        _app.Nodes.World.Sync();
        _app.Refresh();
        if (scene == "dog-map") return;
        var reduce = Environment.GetEnvironmentVariable("PB_REDUCE_MOTION") == "1";
        if (reduce) Settings.GameSettings.ReduceMotion = true;
        _app.AfterAction(g.PlayerMove(dx, dy)); // prawdziwe głaskanie: dośw., ogłoszenie, dymek
        if (scene == "dog-pet")
        {
            _app.Nodes.Announce.FreezeAt = At("PB_ANNOUNCE_AT", 0.7f);
            _app.Nodes.DogBubble.HideBubble();
            return;
        }
        _app.Nodes.Announce.Queue.Clear(); // dog-quip: sam dymek nad Kawką
        _app.Nodes.DogBubble.FreezeAt = At("PB_BUBBLE_AT", 1.0f);
    }

    private static float At(string env, float fallback) =>
        float.TryParse(Environment.GetEnvironmentVariable(env), System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var t) ? t : fallback;

    /// <summary>Plac bez problemów i znajdziek wokół bohatera; Kawka na błocie tuż obok (bohater przesunięty na sąsiednie pole
    /// błota w pokoju startowym). Zwraca kierunek od bohatera do Kawki.</summary>
    private (int, int) Place(CoreGame g)
    {
        g.EnemiesCount = 0;
        g.PickupsCount = 0;
        g.Weather = 0; // bez deszczu (czytelny kadr do reklamy)
        _app.Nodes.Banners.Clear();
        int[,] dirs = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };
        for (var r = 1; r <= 6; r++)
        {
            for (var y = g.Hero.Y - r; y <= g.Hero.Y + r; y++)
            {
                for (var x = g.Hero.X - r; x <= g.Hero.X + r; x++)
                {
                    if (!g.Mud(x, y)) continue;
                    for (var k = 0; k < 4; k++)
                    {
                        int hx = x - dirs[k, 0], hy = y - dirs[k, 1];
                        if (g.Lv.At(hx, hy) != Tile.Floor || g.Mud(hx, hy) || dirs[k, 1] != 0) continue; // obok w poziomie: oba w kadrze
                        g.Hero.X = (sbyte)hx;
                        g.Hero.Y = (sbyte)hy;
                        g.DogX = (sbyte)x;
                        g.DogY = (sbyte)y;
                        g.DogFlags = CoreGame.DogOn | CoreGame.DogSpawned;
                        return (dirs[k, 0], dirs[k, 1]);
                    }
                }
            }
        }
        // bez błota w pobliżu (nie powinno się zdarzyć w Akcie I): zwykła podłoga obok
        g.DogX = (sbyte)(g.Hero.X + 1);
        g.DogY = g.Hero.Y;
        g.Lv[g.DogX, g.DogY] = Tile.Floor;
        g.DogFlags = CoreGame.DogOn | CoreGame.DogSpawned;
        return (1, 0);
    }
}
