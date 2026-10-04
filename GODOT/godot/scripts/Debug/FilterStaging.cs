using System;
using System.Threading.Tasks;
using LifeLike.Core;
using LifeLike.Game.Screens;

namespace LifeLike.Game.Debug;

/// <summary>
/// Sceny zrzutów v0.21.53 (filtry ekranu; filtr wybiera --filter ID): plac z czerwonymi polami, elitami i stanami
/// (filter-map), oferta premii z literami rzadkości (filter-boon = boon-pick), lista filtrów w ustawieniach (filters,
/// filters-locked), Wygląd na wyborze zawodu z wierszem filtra (filters-looks), banery odblokowania (filter-banner)
/// i strona Jak grać (help-filters).
/// </summary>
public sealed class FilterStaging
{
    public static readonly string[] Names = ["filter-map", "filter-boon", "filters", "filters-locked", "filters-looks", "filter-banner", "help-filters"];

    private readonly App _app;
    private readonly DemoStaging _stage;

    public FilterStaging(App app)
    {
        _app = app;
        _stage = new DemoStaging(app);
    }

    private ScreenFlow Flow => _app.Flow;

    public static bool Handles(string scene) => Array.IndexOf(Names, scene) >= 0 && scene != "filter-boon";

    /// <summary>Profil z odblokowanymi filtrami Noir i Retro LCD (inspektor 6, kolekcja Stan surowy), bez Neonu i Kwasu.</summary>
    public static void Prepare(LifeLike.Game.Session.GameSession s, bool all)
    {
        var d = s.Data;
        var p = s.Profile;
        p.Runs = Math.Max(p.Runs, 14);
        p.Wins = Math.Max(p.Wins, 4);
        while (Progress.InspectorLevel(d, p) < (all ? 20 : 6)) p.InspectorXp += 100;
        var retro = Array.FindIndex(d.ScreenFilters, f => f.Id == "retro");
        if (retro >= 0)
        {
            var col = d.Collections[d.ScreenFilters[retro].Unlock[0].Value];
            for (var e = 0; e < d.Enemies.Length; e++)
                if (((col.Enemies >> e) & 1) != 0) p.KillCount[e] = (byte)Math.Max(p.KillCount[e], col.Count);
        }
        if (all) p.CareerDone = 0x1F;
    }

    public async Task Setup(string scene)
    {
        var s = _app.Session;
        switch (scene)
        {
            case "filter-map":
                s.ClassId = 1;
                _app.StartRun();
                Flow.StageCard.Advance();
                _app.Nodes.Banners.Clear();
                _stage.FilterShowcase();
                s.ResetWatch();
                _app.Refresh();
                break;
            case "filters":
            case "filters-locked":
                Prepare(s, false);
                Flow.Settings.Open(Flow.Title, true);
                Flow.Settings.OpenFilters();
                if (_app.Nodes.Phone.Current is Phone.Pages.FiltersPage fp) fp.Sel = scene == "filters" ? 1 : 3;
                _app.Nodes.Phone.QueueRedraw();
                break;
            case "filters-looks":
                Prepare(s, false);
                Flow.ClassSelect.Open();
                Flow.Investor.Open(true);
                if (Flow.Investor.Page is { } ip) ip.SelectFilterRow();
                _app.Nodes.Phone.QueueRedraw();
                break;
            case "filter-banner":
                Prepare(s, true);
                Flow.Title.Open();
                _app.Banners.FilterUnlocks(ScreenFilters.Announce(s.Data, s.Profile));
                break;
            case "help-filters":
                Flow.Help.Open(false, true);
                if (_app.Nodes.Phone.Current is Phone.Pages.HelpPage hp) hp.Page = Phone.Pages.HelpPage.FiltersPage;
                _app.Nodes.Phone.QueueRedraw();
                break;
        }
        await Task.CompletedTask;
    }
}
