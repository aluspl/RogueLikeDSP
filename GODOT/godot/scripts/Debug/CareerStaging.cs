using System;
using System.Threading.Tasks;
using LifeLike.Core;
using LifeLike.Game.Screens;
using LifeLike.Game.Session;

namespace LifeLike.Game.Debug;

/// <summary>
/// Sceny zrzutów v0.21.52 cz. d (#47, jak scenariusze GBA 72–75): mapa kariery (kontrakty, zaznaczony Domek / zablokowana
/// Kamienica), etap z bossem każdego nowego kontraktu (boss obok bohatera), bliźniak – druga połowa z problemami
/// z pierwszej, banery końca budowy (wygrany kontrakt, nowy kontrakt) i strona Jak grać.
/// </summary>
public sealed class CareerStaging
{
    public static readonly string[] Names =
    [
        "career", "career-locked", "career-letnisko", "career-blizniak", "career-poddasze", "career-kamienica", "career-end", "help-career",
    ];

    private readonly App _app;

    public CareerStaging(App app)
    {
        _app = app;
    }

    private ScreenFlow Flow => _app.Flow;

    public static bool Handles(string scene) => Array.IndexOf(Names, scene) >= 0;

    /// <summary>Profil po 30 budowach: 12 wygranych, inspektor 13 – wszystkie kontrakty; Dom, Domek i Bliźniak wygrane.</summary>
    public static void Prepare(GameSession s)
    {
        var d = s.Data;
        var p = s.Profile;
        p.Runs = Math.Max(p.Runs, 30);
        p.Wins = Math.Max(p.Wins, 12);
        p.Best = Math.Max(p.Best, 6400);
        while (Progress.InspectorLevel(d, p) < 13) p.InspectorXp += 100;
        p.CareerDone = 0x07;
        p.CareerSeen = 0x0F;
        p.CareerWins[0] = 9;
        p.CareerWins[1] = 2;
        p.CareerWins[2] = 1;
        p.CareerBest[0] = 10;
        p.CareerBest[1] = 6;
        p.CareerBest[2] = 10;
        p.CareerBest[3] = 7;
        p.CareerBest[4] = 0;
    }

    public async Task Setup(string scene)
    {
        var s = _app.Session;
        var d = s.Data;
        Prepare(s);
        switch (scene)
        {
            case "career": // mapa kariery: zaznaczony Domek letniskowy (wygrany), nagroda i boss pod listą
            case "career-locked": // profil z 1 wygraną: Bliźniak zablokowany (3 wygrane), Kamienica – inspektor
            {
                if (scene == "career-locked")
                {
                    s.Profile.Wins = 1;
                    s.Profile.InspectorXp = 0;
                    s.Profile.CareerDone = 1;
                    s.Profile.CareerWins[1] = s.Profile.CareerWins[2] = 0;
                }
                s.Profile.Contract = (byte)(scene == "career" ? 1 : 0);
                Flow.Career.Open(true);
                if (scene == "career-locked") Flow.Career.Page.Sel = 2;
                _app.Nodes.Phone.QueueRedraw();
                return;
            }
            case "help-career": // Jak grać, strona 11
                Flow.Help.Open(false, true);
                if (_app.Nodes.Phone.Current is Phone.Pages.HelpPage hp) hp.Page = 10;
                _app.Nodes.Phone.QueueRedraw();
                return;
        }
        var contract = scene switch
        {
            "career-letnisko" => 1,
            "career-blizniak" or "career-end" => 2,
            "career-poddasze" => 3,
            _ => 4,
        };
        s.Profile.Contract = (byte)contract;
        if (scene == "career-end") s.Profile.CareerDone = 0x03; // Bliźniak jeszcze bez wygranej
        s.ClassId = 1;
        _app.StartRun();
        Flow.StageCard.Advance();
        var g = s.Game;
        if (scene == "career-blizniak") // druga połowa (Dach, prawa): 3 problemy z pierwszej, ta sama pogoda
        {
            var ts = 1;
            while (!g.SDef(ts).Twin) ++ts;
            ts += 2;
            while (!g.SDef(ts).Twin) ++ts;
            g.StartStage(ts - 1);
            g.StartStage(ts);
        }
        else if (scene == "career-end") // ostatni etap, boss pokonany: wygrany kontrakt, baner nagrody
        {
            g.StartStage(g.RouteCount() - 1);
            g.DebugSkip();
        }
        else // etap z bossem kontraktu, boss obok bohatera
        {
            var bs = 0;
            while (bs < g.RouteCount() - 1 && g.SDef(bs).Boss != g.KDef.Boss) ++bs;
            g.StartStage(bs);
            if (g.Boss >= 0)
            {
                for (var r = 2; r <= 4; r++)
                {
                    var placed = false;
                    for (var dy = -r; dy <= r && !placed; dy++)
                    {
                        for (var dx = -r; dx <= r && !placed; dx++)
                        {
                            int x = g.Hero.X + dx, y = g.Hero.Y + dy;
                            if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != r || g.Lv.At(x, y) != LifeLike.Core.Tile.Floor || g.Occupied(x, y)) continue;
                            g.Enemies[g.Boss].X = (sbyte)x;
                            g.Enemies[g.Boss].Y = (sbyte)y;
                            g.Enemies[g.Boss].Awake = true;
                            placed = true;
                        }
                    }
                    if (placed) break;
                }
                g.UpdateFov();
            }
        }
        s.ResetWatch();
        _app.Refresh();
        if (scene == "career-end") _app.AfterAction(true);
        await Task.CompletedTask;
    }
}
