using System;
using System.Threading.Tasks;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Screens;
using LifeLike.Game.Session;

namespace LifeLike.Game.Debug;

/// <summary>
/// Sceny zrzutów v0.21.52 cz. c (jak scenariusz GBA 71): drzewko Szkoleń (Koszty > Drzewko), kolekcje (Katalog >
/// Kolekcje / Bossowie / Album), zadania dnia i tygodnia z serią dni (Odznaki > Zadania), zadanie w telefonie w trakcie
/// budowy, banery celów na planszy końcowej i strona Jak grać.
/// </summary>
public sealed class GoalsStaging
{
    public static readonly string[] Names =
    [
        "tree", "tree-locked", "collections", "bosses", "album", "tasks", "phone-goals", "goals-end", "help-goals", "title-goals",
    ];

    private readonly App _app;

    public GoalsStaging(App app)
    {
        _app = app;
    }

    private ScreenFlow Flow => _app.Flow;

    public static bool Handles(string scene) => Array.IndexOf(Names, scene) >= 0;

    /// <summary>
    /// Profil po ~25 budowach: pień gałęzi Fach i połowa BHP, wybrana Siła rozpędu, kolekcja Stan surowy bez jednego
    /// problemu, 3 karty bossów, seria 2 dni (budowa dnia wczoraj i przedwczoraj), zadania: jedno wykonane, drugie prawie.
    /// </summary>
    public static void Prepare(GameSession s)
    {
        var d = s.Data;
        var p = s.Profile;
        s.FixedToday = Tuple.Create(2026, 10, 3);
        p.Runs = Math.Max(p.Runs, 25);
        p.Xp = 220;
        for (var i = 0; i < d.Upgrades.Length; ++i)
        {
            var b = SkillTree.BranchOf(d, i);
            p.Levels[i] = (byte)(b == 0 ? d.Upgrades[i].Levels : b == 1 ? 2 : i % 2);
        }
        p.Tree = 2; // Fach I: opcja B (Siła rozpędu)
        var set = d.Collections[0];
        for (var e = 0; e < d.Enemies.Length; ++e)
        {
            if (((set.Enemies >> e) & 1) != 0)
            {
                p.KillCount[e] = (byte)set.Count;
                Meta.CatalogAdd(p, e);
            }
            else if (e % 3 == 0 && !CollectionBook.EnemyBoss(d, e) && e % 7 != 0)
            {
                p.KillCount[e] = (byte)(e % 7);
                Meta.CatalogAdd(p, e);
            }
        }
        p.KillCount[Array.FindIndex(d.Enemies, e => e.Id == "woda")] = (byte)(set.Count - 1);
        for (var k = 0; k < 3; ++k)
        {
            var e = CollectionBook.BossAt(d, k);
            p.KillCount[e] = (byte)(2 + k);
            Meta.CatalogAdd(p, e);
        }
        var day = s.TodayNumber;
        Daily.Record(d, p, day - 2, 2100, false);
        Daily.Record(d, p, day - 1, 3300, true);
        DailyTasks.Roll(p, day, s.TodayWeek);
        p.TaskProgress[0] = (byte)(DailyTasks.Of(d, p, 0).Target - 1);
        p.TaskProgress[1] = (byte)DailyTasks.Of(d, p, 1).Target;
        p.TaskDone = 2;
        p.TasksTotal = 4;
        p.TaskProgress[3] = (byte)(DailyTasks.Of(d, p, 3).Target / 2);
    }

    public async Task Setup(string scene)
    {
        var s = _app.Session;
        var d = s.Data;
        Prepare(s);
        switch (scene)
        {
            case "title-goals": // tytuł: Zadania 1/3 i seria dni pod rekordem
                Flow.Title.Open();
                return;
            case "tree": // Koszty > Drzewko: wybrana Siła rozpędu, zaznaczony węzeł II gałęzi Fach (otwarty)
            case "tree-locked": // zaznaczony zamknięty węzeł (BHP II – próg pnia)
            {
                Flow.Profile.Open(4, true);
                if (_app.Nodes.Phone.Current is Phone.ProfileTabs.TrainingTab tt)
                {
                    tt.Page = Phone.ProfileTabs.TrainingTab.TreePage;
                    if (scene == "tree") tt.SelectNode(0, 2);
                    else tt.SelectNode(1, 3);
                }
                _app.Nodes.Phone.QueueRedraw();
                return;
            }
            case "collections": // Katalog > Kolekcje: Stan surowy 12/13
            case "bosses": // Katalog > Bossowie: 3 karty z 5
            case "album": // Katalog > Album Osiedla
            {
                Flow.Profile.Open(1, true);
                if (_app.Nodes.Phone.Current is Phone.ProfileTabs.CatalogTab ct)
                {
                    ct.Page = scene == "collections" ? Phone.ProfileTabs.CatalogTab.CollectionsPage
                        : scene == "bosses" ? Phone.ProfileTabs.CatalogTab.BossesPage : Phone.ProfileTabs.CatalogTab.AlbumPage;
                    if (scene == "bosses") ct.Select(1);
                }
                _app.Nodes.Phone.QueueRedraw();
                return;
            }
            case "tasks": // Odznaki > Zadania: 3 dnia, 2 tygodnia, seria dni
            {
                Flow.Profile.Open(0, true);
                if (_app.Nodes.Phone.Current is Phone.ProfileTabs.BadgesTab bt)
                {
                    bt.Page = Phone.ProfileTabs.BadgesTab.TasksPage;
                    bt.Select(0);
                }
                _app.Nodes.Phone.QueueRedraw();
                return;
            }
            case "help-goals": // Jak grać, strona 10
                Flow.Help.Open(false, true);
                if (_app.Nodes.Phone.Current is Phone.Pages.HelpPage hp) hp.Page = 9;
                _app.Nodes.Phone.QueueRedraw();
                return;
        }
        // w budowie: zadanie dnia w Kosztach (postęp na żywo) albo koniec budowy z banerami celów
        s.ClassId = 1;
        _app.StartRun();
        Flow.StageCard.Advance();
        var g = s.Game;
        var task0 = DailyTasks.Of(d, s.Profile, 0);
        Bump(d, g, task0.Kind);
        g.KillsByType[Array.FindIndex(d.Enemies, e => e.Id == "woda")] = 1; // komplet Stan surowy
        s.ResetWatch();
        if (scene == "phone-goals")
        {
            Flow.Phone.Open(4, true);
            return;
        }
        g.EnemiesCount = 0;
        g.Hero.Hp = 1;
        g.Bonus.SecondChance = 0;
        var bud = Array.FindIndex(d.Enemies, e => e.Id == "budzet");
        for (var dx = 1; dx <= 2 && g.EnemiesCount == 0; dx++)
        {
            if (g.Lv.At(g.Hero.X + dx, g.Hero.Y) == Tile.Floor) g.Spawn(bud, g.Hero.X + dx, g.Hero.Y);
        }
        if (g.EnemiesCount == 0) g.Spawn(bud, g.Hero.X, g.Hero.Y + 1);
        for (var k = 0; k < 6 && g.St == GameStatus.Playing; k++) g.EnemyStrike(0, false);
        _app.AfterAction(true);
        await DebugRunner.Frames(_app.Root, 2);
        Flow.End.Open();
        await DebugRunner.Frames(_app.Root, 20);
    }

    /// <summary>Licznik budowy pod zadanie (wykonane na końcu budowy).</summary>
    private static void Bump(GameData d, LifeLike.Core.Game g, TaskKind k)
    {
        switch (k)
        {
            case TaskKind.Kills: g.Kills += 60; break;
            case TaskKind.Elites: g.ElitesKilled += 3; break;
            case TaskKind.Brigade: g.HelpersCalled += 3; break;
            case TaskKind.Powers: g.PowersUsed += 9; break;
            case TaskKind.Coffee: g.CoffeeDrunk += 5; break;
            case TaskKind.Combos: g.CombosRun += 4; break;
            case TaskKind.Storerooms: g.SecretsFound += 1; break;
            case TaskKind.Events: g.StageEventLog[0] = 4; g.StageEventLog[1] = 4; break;
            case TaskKind.Bosses: g.KillsByType[CollectionBook.BossAt(d, 0)] += 2; break;
        }
    }
}
