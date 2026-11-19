using System.Collections.Generic;
using LifeLike.Core;

namespace LifeLike.Game.Session;

/// <summary>
/// v0.22.0 (UX A): krótkie wiersze „Do tego:” w rozmowie między budowami – to, co do v0.21 pokazywały banery i plansza
/// końcowa (rekordy, nagroda za odbiór, nowe poziomy, zadania, kolekcje, wątki fabuły, kontrakty, filtry, inwestor).
/// </summary>
public static class EndNotes
{
    public static List<string> Of(GameSession s)
    {
        var d = s.Data;
        var p = s.Profile;
        var g = s.Game;
        var won = g.St == GameStatus.Won;
        var list = new List<string>();
        if (g.Score > s.PrevBest) list.Add(Loc.F("nowy_rekord_poprzedni", s.PrevBest));
        if (g.Daily && s.DailyRecord) list.Add(Loc.F("rekord_dnia_budowa_dnia_nr", g.DailyDay));
        if (g.WeeklyWeek != 0 && s.WeeklyRecord) list.Add(Loc.F("rekord_tygodnia_4", d.Weekly[g.Bonus.Weekly].Name));
        if (g.Daily && !s.DailyRecord) list.Add(Loc.F("budowa_dnia_nr_najlepszy", g.DailyDay, Daily.Best(d, p, g.DailyDay)));
        if (g.WeeklyWeek != 0 && !s.WeeklyRecord) list.Add(Loc.F("wyzwanie_tygodnia_najlepszy", Weekly.Best(d, p, g.WeeklyWeek)));
        if (s.LastReward >= 0) list.Add(Loc.F("nagroda_3", d.Rewards[s.LastReward].Name, d.Rewards[s.LastReward].Desc));
        foreach (var line in s.Note.Split('\n')) // sekretne zlecenia, odznaki, zlecenia (ProgressNotes)
        {
            if (line.Trim().Length > 0) list.Add(line.Trim());
        }
        if (g.Respect > 0) list.Add(Loc.F("respekt_z_budowy_masz", g.Respect, p.Respect));
        if (s.LastCareerFirst) list.Add($"{Loc.F("wygrany_kontrakt", g.KDef.Name)} – {Career.RewardLabel(d, g.Contract)}");
        foreach (var (title, body) in ProgressBanners.Of(d, s.LastProgress, s.LastStakeBefore, s.LastStakeAfter)) list.Add($"{title} – {body}");
        foreach (var (title, body) in GoalBanners.Of(d, p, s.LastTasks, s.LastCollections, s.LastTasksBefore, s.LastStreakBefore, s.LastStreakAfter))
            list.Add($"{title} – {body}");
        for (var k = 1; k < d.Career.Length; k++)
        {
            if (((s.LastCareerNew >> k) & 1) != 0) list.Add($"{Loc.T("nowy_kontrakt")} – {Loc.F("mapa_kariery_3", d.Career[k].Name)}");
        }
        var stories = 0;
        for (var i = 0; i < d.StoryArc.Length; i++) stories += (int)((s.LastStory >> i) & 1);
        if (stories > 2) list.Add($"{Loc.F("nowe_wiadomosci", stories)} – {Loc.T("profil_osiedle_wiadomosci_2")}");
        else
        {
            for (var i = 0; i < d.StoryArc.Length; i++)
            {
                if (((s.LastStory >> i) & 1) != 0) list.Add($"{Loc.T("nowa_wiadomosc")} – {d.StoryArc[i].Name}");
            }
        }
        for (var f = 0; f < d.ScreenFilters.Length; f++)
        {
            if (((s.LastFilterNew >> f) & 1) != 0) list.Add($"{d.FilterText("banner")} – {d.ScreenFilters[f].Name}");
        }
        var stake = Investor.Stake(d, g.Bonus.Investor);
        if (stake > 0) list.Add(Loc.F("tryb_inwestora_stawka_3", stake) + (won ? Loc.F("rekord_zawodu", Meta.BestStake(p, g.Cls)) : ""));
        return list;
    }
}
