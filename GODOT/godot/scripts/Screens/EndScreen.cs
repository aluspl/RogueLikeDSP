using LifeLike.Core;
using LifeLike.Game.Hud;
using LifeLike.Game.Input;
using LifeLike.Game.Session;

namespace LifeLike.Game.Screens;

/// <summary>Plansza końcowa z kodem QR (run_end na GBA): po odbiorze A = kolejna budowa (NG+), Enter = tytuł.</summary>
public sealed class EndScreen : Screen
{
    public EndScreen(App app) : base(app)
    {
    }

    public override ViewSet Views => ViewSet.End;
    public override string Music => "";

    public void Open() => Flow.Go(this);

    public override void Enter(bool instant)
    {
        var g = S.Game;
        var won = g.St == GameStatus.Won;
        var v = N.EndView;
        v.Won = won;
        v.CanContinue = won && !g.Daily && g.WeeklyWeek == 0; // budowa dnia / tygodnia: bez NG+
        v.Line1 = $"Wynik {g.Score}   Dni {g.Turns}   Etap {g.StageNumber()}/{g.StagesInRun()}   Dośw. +{S.LastGained}";
        var record = g.Score > S.PrevBest ? "Nowy rekord!" : $"Rekord {S.Profile.Best}";
        var daily = g.Daily ? (S.DailyRecord ? "   Rekord dnia!" : $"   Budowa dnia nr {g.DailyDay}") : "";
        if (g.WeeklyWeek != 0) daily = S.WeeklyRecord ? "   Rekord tygodnia!" : $"   Tydzień nr {g.WeeklyWeek}";
        v.Line2 = $"{record}   Doświadczenie w profilu {S.Profile.Xp}   Respekt +{g.Respect} (masz {S.Profile.Respect})" + (won ? "   Dom na Osiedlu!" : "") + daily;
        v.Note = S.Note;
        N.Banners.Clear();
        foreach (var (title, body) in ProgressBanners.Of(S.Data, S.LastProgress, S.LastStakeBefore, S.LastStakeAfter))
            N.Banners.Push(new PushBanner { Title = title, Body = body, Gold = true }); // v0.21.52 cz. b: nowe poziomy
        foreach (var (title, body) in GoalBanners.Of(S.Data, S.Profile, S.LastTasks, S.LastCollections, S.LastTasksBefore, S.LastStreakBefore, S.LastStreakAfter))
            N.Banners.Push(new PushBanner { Title = title, Body = body, Gold = true }); // cz. c: zadania, kolekcje, seria dni
        var stories = 0; // fabuła (#35): nowe wątki w Wiadomościach (Profil > Osiedle); więcej niż 2 – jeden zbiorczy baner
        for (var i = 0; i < S.Data.StoryArc.Length; i++) stories += (int)((S.LastStory >> i) & 1);
        for (var i = 0; i < S.Data.StoryArc.Length && stories <= 2; i++)
        {
            if (((S.LastStory >> i) & 1) != 0) N.Banners.Push("Nowa wiadomość", S.Data.StoryArc[i].Name);
        }
        if (stories > 2) N.Banners.Push($"Nowe wiadomości: {stories}", "Profil > Osiedle > Wiadomości");
        if (S.LastCareerFirst) // v0.21.52 cz. d (#47): kontrakt wygrany pierwszy raz, nowe kontrakty na mapie kariery
            N.Banners.Push(new PushBanner { Title = $"Wygrany kontrakt: {g.KDef.Name}", Body = Career.RewardLabel(S.Data, g.Contract), Gold = true });
        for (var k = 1; k < S.Data.Career.Length; k++)
        {
            if (((S.LastCareerNew >> k) & 1) != 0) N.Banners.Push(new PushBanner { Title = "Nowy kontrakt!", Body = $"{S.Data.Career[k].Name} – mapa kariery", Gold = true });
        }
        App.Banners.FilterUnlocks(S.LastFilterNew); // v0.21.53: nowe filtry ekranu
    }

    public override bool HandleInput(InputCmd e)
    {
        if (e.IsTap)
        {
            var b = N.EndView.ButtonAt(e.Pointer);
            if (b == 0) return false;
            e = InputCmd.Of(b == 1 ? GameAction.A : GameAction.Start);
        }
        if (S.Game.St == GameStatus.Won && !S.Game.Daily && S.Game.WeeklyWeek == 0 && e.Is(GameAction.A))
        {
            S.NewGamePlus();
            App.Refresh();
            Flow.StageCard.Open();
            return true;
        }
        if (!e.Is(GameAction.Start | GameAction.A)) return false;
        S.Note = "";
        Flow.Title.Open();
        return true;
    }
}
