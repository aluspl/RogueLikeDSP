using System;
using System.Threading.Tasks;
using LifeLike.Core;
using LifeLike.Game.Input;
using LifeLike.Game.Screens;

namespace LifeLike.Game.Debug;

/// <summary>
/// v0.22.0 (UX A): sceny zrzutów rozmowy z Kierownikiem Markiem między budowami – po wygranej (z ofertą „Kup”), po
/// porażce, po budowie dnia, bez doświadczenia i z wyborem w drzewku Szkoleń. Stare „end” i „endmsg” prowadzą tu.
/// </summary>
public sealed class BetweenStaging
{
    public static readonly string[] Names = ["between-win", "between-death", "between-daily", "between-broke", "between-tree"];

    private readonly App _app;

    public BetweenStaging(App app)
    {
        _app = app;
    }

    private ScreenFlow Flow => _app.Flow;

    public static bool Handles(string scene) => Array.IndexOf(Names, scene) >= 0;

    public async Task Setup(string scene)
    {
        var s = _app.Session;
        var d = s.Data;
        var p = s.Profile;
        var scenes = new DebugScenes(_app);
        if (scene == "between-daily")
        {
            s.FixedToday = Tuple.Create(2026, 9, 25);
            Flow.Daily.Open(true);
            await DebugRunner.Frames(_app.Root, 2);
            Flow.Daily.HandleInput(InputCmd.Of(GameAction.A));
        }
        else
        {
            s.ClassId = 0;
            _app.StartRun();
        }
        scenes.AdvanceMessages();
        var g = s.Game;
        if (scene == "between-win")
        {
            for (var guard = 0; guard < 60 && Flow.Current != Flow.HouseSchedule && Flow.Current != Flow.BetweenRuns; guard++)
            {
                scenes.AdvanceMessages();
                if (Flow.Current == Flow.Prologue) Flow.Prologue.HandleInput(InputCmd.Of(GameAction.A));
                if (Flow.Current == Flow.Offer) Flow.Offer.Decide(true);
                if (Flow.Current != Flow.Game) continue;
                g.DebugSkip();
                _app.AfterAction(true);
                await DebugRunner.Frames(_app.Root, 1);
            }
        }
        else
        {
            g.Hero.Hp = 0;
            g.Hero.Alive = false;
            g.St = GameStatus.Dead;
            _app.AfterAction(true);
        }
        if (scene == "between-broke") p.Xp = 0;
        if (scene == "between-tree")
        {
            p.Xp = 1000000;
            while (Spend.QuickBuy(d, p)) { }
            p.Xp = 1000000;
            var n = Spend.TreePick(d, p);
            p.Xp = n >= 0 ? d.TreeNodes[n].Cost : 0;
        }
        Flow.BetweenRuns.Open(true);
        await DebugRunner.Frames(_app.Root, 4);
        _app.Nodes.Phone.QueueRedraw();
    }
}
