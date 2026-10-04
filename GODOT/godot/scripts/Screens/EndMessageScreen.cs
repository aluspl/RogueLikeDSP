using LifeLike.Core;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>Koniec budowy: SMS z odbioru / przerwania (phone_message na GBA), potem plansza końcowa.</summary>
public sealed class EndMessageScreen : Screen
{
    public EndMessageScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;

    public void Open(bool instant = false) => Flow.Go(this, instant);

    public override void Enter(bool instant)
    {
        var g = S.Game;
        var d = S.Data;
        var p = S.Profile;
        var won = g.St == GameStatus.Won;
        var page = new MessagePage(won ? Loc.T("odbior_2") : Loc.T("budowa"));
        page.Add(won ? d.StoryWin : d.StoryLose);
        page.Info(Loc.F("wynik_dosw_w_profilu", g.Score, S.LastGained, p.Xp), Ink.Dark);
        if (g.WeeklyWeek != 0) page.Info(S.WeeklyRecord ? Loc.F("rekord_tygodnia_4", S.Data.Weekly[g.Bonus.Weekly].Name) : Loc.F("wyzwanie_tygodnia_najlepszy", Weekly.Best(d, p, g.WeeklyWeek)), S.WeeklyRecord ? Ink.Done : Ink.Dim);
        if (g.Daily) page.Info(S.DailyRecord ? Loc.F("rekord_dnia_budowa_dnia_nr", g.DailyDay) : Loc.F("budowa_dnia_nr_najlepszy", g.DailyDay, Daily.Best(d, p, g.DailyDay)), S.DailyRecord ? Ink.Done : Ink.Dim);
        if (g.Score > S.PrevBest) page.Info(Loc.F("nowy_rekord_poprzedni", S.PrevBest), Ink.Done);
        else page.Info(won ? Loc.T("dom_na_osiedlu") : Loc.F("rekord_do_pobicia", p.Best), won ? Ink.Done : Ink.Dim);
        var stake = Investor.Stake(d, g.Bonus.Investor);
        if (stake > 0) page.Info(Loc.F("tryb_inwestora_stawka_3", stake) + (won ? Loc.F("rekord_zawodu", Meta.BestStake(p, g.Cls)) : ""), Ink.Brand);
        if (S.LastReward >= 0) page.Info(Loc.F("nagroda_3", d.Rewards[S.LastReward].Name, d.Rewards[S.LastReward].Desc), Ink.Done);
        page.Info(Loc.F("respekt_z_budowy_masz", g.Respect, p.Respect), Ink.Brand);
        var nr = p.Rewards;
        if (nr < d.Rewards.Length)
            page.Info(d.Rewards[nr].Kind == LifeLike.Core.Data.RewardKind.Soon ? Loc.F("wkrotce_3", d.Rewards[nr].Name) : Loc.F("za_kolejny_odbior_2", d.Rewards[nr].Name), Ink.Dim);
        if (!won) Motivation(page);
        N.Phone.OpenSingle(page, 2, instant);
    }

    /// <summary>Po porażce: najbliższe zlecenie i najbliższy zakup w Szkoleniach (coś zostaje na kolejną próbę).</summary>
    private void Motivation(MessagePage page)
    {
        var d = S.Data;
        var p = S.Profile;
        var ci = Meta.NextContract(d, p, S.Game);
        if (ci >= 0)
        {
            var c = d.Contracts[ci];
            page.Info(Loc.F("zlecenie_5", c.Name, System.Math.Min(c.Target, Meta.ContractProgress(d, p, ci)), c.Target), Ink.Prog);
        }
        var cost = Meta.NextUnlock(d, p, out var kind, out var idx);
        if (cost < 0) return;
        var name = kind switch
        {
            0 => $"{d.Upgrades[idx].Name} {UiText.Roman(p.Levels[idx])}",
            1 => Loc.F("zawod_5", d.Classes[idx].Name),
            2 => d.Weapons[d.Tools[idx].Weapon].Name,
            3 => Loc.F("brygada_4", d.Brigade[idx].Name),
            5 => Loc.F("wezel_drzewka_2", d.TreeBranches[d.TreeNodes[idx].Branch].Name), // v0.21.52 cz. c
            _ => Loc.F("poziom_6", d.Difficulties[d.Difficulties.Length - 1].Name),
        };
        page.Info(p.Xp >= cost ? Loc.F("stac_cie_dosw", name, cost) : Loc.F("najblizej_brakuje_dosw", name, cost - p.Xp), p.Xp >= cost ? Ink.Done : Ink.Brand);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (!e.Is(GameAction.A | GameAction.Start)) return false;
        if (S.Game.St == GameStatus.Won && S.Game.RouteCount() > 0) Flow.HouseSchedule.Open();
        else Flow.Recap.Open(); // podsumowanie budowy (#33)
        return true;
    }
}
