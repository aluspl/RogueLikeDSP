using System;
using System.Threading.Tasks;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Screens;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Debug;

/// <summary>
/// Sceny zrzutów v0.21.50 cz. 4 (jak scenariusze GBA 57-60): podsumowanie po porażce i po wygranej (oś czasu, ciosy,
/// cel i rada), baner „Nowa wiadomość” na planszy końcowej, wyzwanie tygodnia (strona i budowa z zasadą), archiwum
/// Wiadomości i Osiedle z ozdobami.
/// </summary>
public sealed class RecapStaging
{
    private readonly App _app;

    public RecapStaging(App app) => _app = app;

    private ScreenFlow Flow => _app.Flow;

    public static bool Handles(string scene) =>
        scene.StartsWith("recap-") || scene.StartsWith("weekly") || scene.StartsWith("story-") || scene == "estate-grow";

    /// <summary>Oś czasu jak po kilku etapach: dni, usunięte, premie, SMS-y, magazyn, elita, boss (scenariusze GBA 57/58).</summary>
    private static void FillTimeline(CoreGame g, int last)
    {
        var d = g.D;
        g.StartStage(last);
        g.Boons = 0;
        for (var s = g.FirstStage; s < last; ++s)
        {
            g.StageDays[s] = (ushort)(14 + s * 7 % 11);
            g.StageKillLog[s] = (byte)(5 + s % 4);
            g.StageBoon[s] = (sbyte)(s * 5 % d.Boons.Length);
            g.Boons |= 1ul << g.StageBoon[s];
            if (s % 3 == 1) g.StageEventLog[s] = (byte)(s / 3 % d.ChoiceEvents.Length * 4 + s % 2);
        }
        g.StageFlags[g.FirstStage + 1] = RecapFlag.Secret | RecapFlag.Elite;
        g.StageFlags[g.FirstStage + 3] = RecapFlag.Boss | RecapFlag.Combo | RecapFlag.Upgrade | RecapFlag.Synergy;
        g.Kills = 30;
        g.ElitesKilled = 2;
        g.CombosRun = 5;
        g.SecretsFound = 1;
        var bet = Array.FindIndex(d.Enemies, e => e.Id == "betoniarka");
        g.BestHit = 21;
        g.BestHitDef = (sbyte)bet;
        g.BestHitCrit = true;
        g.WorstHit = new RecapHit { Src = (sbyte)bet, Elite = -1, Kind = (byte)RecapKind.Slam, Stage = (sbyte)(g.FirstStage + 3), Amount = 8 };
        g.Respect = 14;
        g.Score = 3100;
    }

    public async Task Setup(string scene)
    {
        var s = _app.Session;
        var d = s.Data;
        var p = s.Profile;
        s.FixedToday = Tuple.Create(2026, 9, 29);
        switch (scene)
        {
            case "weekly": // wyzwanie tygodnia: zasady, zawód, wyniki dwóch wcześniejszych tygodni, notatka po „Wyślij wynik”
            {
                var w = s.TodayWeek;
                Weekly.Record(d, p, w - 1, 2890, true);
                Weekly.Record(d, p, w - 2, 1320, false);
                Weekly.Record(d, p, w, 1760, false);
                Flow.Title.Open();
                Flow.Weekly.Open(true);
                Flow.Weekly.Submit();
                return;
            }
            case "weekly-run": // budowa tygodnia z zasadą (Mokry tydzień: deszcz), telefon > Zadania z wierszem Tydzień
            case "weekly-card":
            {
                var w = s.TodayWeek;
                while (d.Weekly[Weekly.Index(d, w)].Rules[0].Rule != WeeklyRule.Weather) w++;
                _app.StartWeekly(w);
                if (scene == "weekly-card") return;
                Flow.StageCard.Advance();
                _app.Nodes.Banners.Clear();
                Flow.Phone.Open(0);
                return;
            }
            case "story-archive":
            case "story-thread":
            case "estate-grow":
            {
                p.Wins = 6;
                p.Runs = 9;
                p.HousesCount = 0;
                for (var i = 0; i < 6; i++) p.Houses[p.HousesCount++] = (byte)(i % d.Classes.Length | i % 4 << 4);
                var bet = Array.FindIndex(d.Enemies, e => e.Id == "betoniarka");
                p.Catalog |= (ushort)(1 << bet);
                p.Story = 0;
                p.StoryNew = 0;
                Story.Check(d, p, null);
                p.StoryNew = p.Story & ~3u;
                Flow.Profile.Open(2, true);
                if (scene == "estate-grow" || _app.Nodes.Phone.Current is not Phone.ProfileTabs.EstateTab et) return;
                et.OpenMessages();
                if (scene == "story-thread") et.OpenThread(et.Sel);
                _app.Nodes.Phone.QueueRedraw();
                return;
            }
        }
        // podsumowanie: budowa Kierownikiem, oś czasu kilku etapów, porażka od Zwarcia na mokro albo odbiór
        s.ClassId = 0;
        _app.StartRun();
        Flow.StageCard.Advance();
        var g = s.Game;
        var won = scene == "recap-win";
        FillTimeline(g, won ? d.Stages.Length - 1 : d.PreludeStages + 4);
        s.ResetWatch();
        if (won)
        {
            g.DebugSkip();
        }
        else
        {
            var kornik = Array.FindIndex(d.Enemies, e => e.Id == "kornik");
            g.LogHit(kornik, 2, RecapKind.Melee, 4);
            g.EnemiesCount = 0;
            g.Hero.Hp = 3;
            g.Bonus.SecondChance = 0;
            g.ApplyStatus(StatusEffect.Wet, 6);
            var zw = Array.FindIndex(d.Enemies, e => e.Id == "zwarcie");
            for (var dx = 1; dx <= 2 && g.EnemiesCount == 0; dx++)
            {
                if (g.Lv.At(g.Hero.X + dx, g.Hero.Y) == Tile.Floor) g.Spawn(zw, g.Hero.X + dx, g.Hero.Y);
            }
            if (g.EnemiesCount == 0) g.Spawn(zw, g.Hero.X, g.Hero.Y + 1);
            for (var k = 0; k < 6 && g.St == GameStatus.Playing; k++) g.EnemyStrike(0, false);
        }
        _app.AfterAction(true);
        await DebugRunner.Frames(_app.Root, 2);
        if (scene == "recap-endmsg") return;
        Flow.Recap.Open(true);
        if (scene == "recap-death-scroll") Flow.Recap.Page.TapRow(-2);
        if (scene == "recap-end")
        {
            Flow.End.Open();
            await DebugRunner.Frames(_app.Root, 20);
        }
        _app.Nodes.Phone.QueueRedraw();
    }
}
