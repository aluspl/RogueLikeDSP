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
        var page = new MessagePage(won ? "Odbiór" : "Budowa");
        page.Add(won ? d.StoryWin : d.StoryLose);
        page.Info($"Wynik {g.Score}  Dośw. +{S.LastGained} (w profilu {p.Xp})", Ink.Dark);
        if (g.WeeklyWeek != 0) page.Info(S.WeeklyRecord ? $"Rekord tygodnia! {S.Data.Weekly[g.Bonus.Weekly].Name}" : $"Wyzwanie tygodnia: najlepszy {Weekly.Best(d, p, g.WeeklyWeek)}", S.WeeklyRecord ? Ink.Done : Ink.Dim);
        if (g.Daily) page.Info(S.DailyRecord ? $"Rekord dnia! Budowa dnia nr {g.DailyDay}" : $"Budowa dnia nr {g.DailyDay}: najlepszy {Daily.Best(d, p, g.DailyDay)}", S.DailyRecord ? Ink.Done : Ink.Dim);
        if (g.Score > S.PrevBest) page.Info($"Nowy rekord! (poprzedni {S.PrevBest})", Ink.Done);
        else page.Info(won ? "Dom na Osiedlu!" : $"Rekord {p.Best} - do pobicia", won ? Ink.Done : Ink.Dim);
        var stake = Investor.Stake(d, g.Bonus.Investor);
        if (stake > 0) page.Info($"Tryb inwestora: stawka {stake}" + (won ? $" (rekord zawodu {Meta.BestStake(p, g.Cls)})" : ""), Ink.Brand);
        if (S.LastReward >= 0) page.Info($"Nagroda: {d.Rewards[S.LastReward].Name}! {d.Rewards[S.LastReward].Desc}", Ink.Done);
        page.Info($"Respekt z budowy +{g.Respect}, masz {p.Respect}", Ink.Brand);
        var nr = p.Rewards;
        if (nr < d.Rewards.Length)
            page.Info(d.Rewards[nr].Kind == LifeLike.Core.Data.RewardKind.Soon ? $"{d.Rewards[nr].Name} - wkrótce" : $"Za kolejny odbiór: {d.Rewards[nr].Name}", Ink.Dim);
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
            page.Info($"Zlecenie {c.Name}: {System.Math.Min(c.Target, Meta.ContractProgress(d, p, ci))}/{c.Target}", Ink.Prog);
        }
        var cost = Meta.NextUnlock(d, p, out var kind, out var idx);
        if (cost < 0) return;
        var name = kind switch
        {
            0 => $"{d.Upgrades[idx].Name} {UiText.Roman(p.Levels[idx])}",
            1 => $"zawód {d.Classes[idx].Name}",
            2 => d.Weapons[d.Tools[idx].Weapon].Name,
            3 => $"brygada: {d.Brigade[idx].Name}",
            5 => $"węzeł drzewka {d.TreeBranches[d.TreeNodes[idx].Branch].Name}", // v0.21.52 cz. c
            _ => $"poziom {d.Difficulties[d.Difficulties.Length - 1].Name}",
        };
        page.Info(p.Xp >= cost ? $"Stać Cię: {name} ({cost} dośw.)" : $"Najbliżej: {name}, brakuje {cost - p.Xp} dośw.", p.Xp >= cost ? Ink.Done : Ink.Brand);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (!e.Is(GameAction.A | GameAction.Start)) return false;
        if (S.Game.St == GameStatus.Won && S.Data.Stages.Length > 0) Flow.HouseSchedule.Open();
        else Flow.Recap.Open(); // podsumowanie budowy (#33)
        return true;
    }
}
