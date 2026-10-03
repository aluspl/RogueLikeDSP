using System;
using System.Linq;
using System.Threading.Tasks;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.ProfileTabs;
using LifeLike.Game.Screens;
using LifeLike.Game.Screens.Play;

namespace LifeLike.Game.Debug;

/// <summary>
/// Test dymny sekretnych zleceń (v0.21.51 cz. 2) na świeżym profilu: strona Sekrety w profilu (A przełącza strony,
/// dotknięcie wiersza), zawody z sekretów zablokowane, wygrana bez kawy (skrót) – sekretne zlecenie, baner i dymek
/// Nowość na tytule, start każdym zawodem z sekretów z mocą (Majster – moc innego fachu w HUD), kask w paski
/// z Trybu inwestora (klatka bohatera), złoty błysk przy krycie, Poziomica mistrza na podglądzie mapy i ranga
/// Respektu zablokowana do sekretu. Profil sprzed testu wraca na koniec.
/// </summary>
public sealed class SmokeSecrets
{
    private readonly App _app;

    public SmokeSecrets(App app) => _app = app;

    private ScreenFlow Flow => _app.Flow;

    public int Done { get; private set; }
    public int Classes { get; private set; }
    public int Banners { get; private set; }

    public async Task Run()
    {
        var s = _app.Session;
        var d = s.Data;
        var old = s.Profile;
        var p = Meta.NewProfile(d);
        p.Tutorial = 0x3F;
        p.ClassesSeen = 0xFFFF;
        p.SetFlag(Profile.FlagPrologueSeen | Profile.FlagHelpSeen);
        s.Profile = p;
        try
        {
            await Page(p, d);
            await WinWithoutCoffee(p, d);
            await NewsBubble(p, d);
            await SecretClasses(p, d);
            await Cosmetics(p, d);
            await LevelOnMap(p, d);
            await RespectLock(p, d);
        }
        finally
        {
            s.Profile = old;
            Flow.Title.Open();
        }
    }

    private async Task Page(Profile p, GameData d)
    {
        Flow.Profile.Open(0, true);
        await DebugRunner.Frames(_app.Root, 2);
        if (_app.Nodes.Phone.Current is not BadgesTab bt) throw new Exception("sekrety: brak zakładki Odznaki");
        for (var k = 0; k < BadgesTab.SecretsPage; k++) Flow.Profile.HandleInput(InputCmd.Of(GameAction.A));
        if (bt.Page != BadgesTab.SecretsPage) throw new Exception($"sekrety: A nie dochodzi do strony Sekrety (strona {bt.Page})");
        if (bt.Sub != $"0/{d.Secrets.Length}") throw new Exception($"sekrety: licznik {bt.Sub}");
        bt.TapRow(d.Secrets.Length - 1);
        Flow.Profile.HandleInput(InputCmd.Of(GameAction.Down));
        await DebugRunner.Frames(_app.Root, 2);
        Flow.Profile.HandleInput(InputCmd.Of(GameAction.A));
        if (bt.Page != BadgesTab.TitlesPage) throw new Exception("sekrety: A na stronie Sekrety nie przechodzi do Tytułów");
        // v0.21.52: tytuł z odznaki – Tab („Wybierz”) wybiera zdobyty, drugi raz zdejmuje; niezdobytego nie da się wybrać
        var tp = _app.Session.Profile;
        var badges0 = tp.Badges;
        bt.Select(1);
        Flow.Profile.HandleInput(InputCmd.Of(GameAction.Select));
        if (Titles.Selected(d, tp) != -1 && (badges0 & 2) == 0) throw new Exception("tytuły: wybrano niezdobyty tytuł");
        tp.Badges = (ushort)(badges0 | 2);
        Flow.Profile.HandleInput(InputCmd.Of(GameAction.Select));
        if (Titles.Selected(d, tp) != 1) throw new Exception($"tytuły: Tab nie wybiera tytułu ({tp.Title})");
        Flow.Profile.HandleInput(InputCmd.Of(GameAction.Select));
        if (tp.Title != 0) throw new Exception("tytuły: drugi Tab nie zdejmuje tytułu");
        tp.Badges = badges0;
        Flow.Profile.HandleInput(InputCmd.Of(GameAction.A)); // v0.21.52 cz. b: Tytuły -> Inspektor -> Odznaki
        if (bt.Page != Phone.ProfileTabs.BadgesTab.InspectorPage) throw new Exception("inspektor: A na stronie Tytuły nie przechodzi do Inspektora");
        Flow.Profile.HandleInput(InputCmd.Of(GameAction.A));
        if (bt.Page != 0) throw new Exception("sekrety: A na stronie Inspektor nie wraca do Odznak");
        for (var c = 0; c < d.Classes.Length; c++)
        {
            if (Meta.ClassSecret(d, c) && Meta.ClassUnlocked(d, p, c)) throw new Exception($"sekrety: {d.Classes[c].Name} odblokowany od startu");
        }
        Flow.Title.Open();
    }

    /// <summary>Wygrana skrótem bez kawy: sekretne zlecenie „Bez kofeiny…”, baner, dymek czeka na tytuł.</summary>
    private async Task WinWithoutCoffee(Profile p, GameData d)
    {
        var s = _app.Session;
        s.ClassId = 1;
        _app.StartRun();
        var scenes = new DebugScenes(_app);
        for (var guard = 0; guard < 60 && Flow.Current != Flow.EndMessage; guard++)
        {
            scenes.AdvanceMessages();
            if (Flow.Current == Flow.Prologue) Flow.Prologue.HandleInput(InputCmd.Of(GameAction.A));
            if (Flow.Current == Flow.Offer) Flow.Offer.Decide(true);
            if (Flow.Current != Flow.Game) continue;
            s.Game.DebugSkip();
            _app.AfterAction(true);
            await DebugRunner.Frames(_app.Root, 1);
        }
        if (Flow.Current != Flow.EndMessage || s.Game.St != GameStatus.Won) throw new Exception("sekrety: skrót nie doprowadził do odbioru");
        var noCoffee = Array.FindIndex(d.Secrets, x => x.Kind == SecretKind.NoCoffeeWin);
        if (!Secrets.Done(p, noCoffee) || (s.LastSecrets & (1 << noCoffee)) == 0) throw new Exception("sekrety: wygrana bez kawy nie dała sekretnego zlecenia");
        if ((p.SecretsNew & (1 << noCoffee)) == 0) throw new Exception("sekrety: brak dymka Nowość do pokazania");
        Banners = _app.Nodes.Banners.All.Count(b => b.Title == "Sekretne zlecenie!");
        if (Banners == 0) throw new Exception("sekrety: brak banera Sekretne zlecenie!");
        if (!s.Note.Contains("Sekretne zlecenie")) throw new Exception("sekrety: brak notatki na planszy końcowej");
        Done = Secrets.DoneCount(d, p);
        await DebugRunner.Frames(_app.Root, 3);
    }

    private async Task NewsBubble(Profile p, GameData d)
    {
        Flow.Title.Open();
        await DebugRunner.Frames(_app.Root, 2);
        var coach = _app.Coach;
        var shown = 0;
        for (var k = 0; k < 20 && coach.Active; k++)
        {
            if (coach.CurrentId == "secret")
            {
                shown++;
                if (_app.Nodes.Coach.Pill != "Nowość") throw new Exception($"sekrety: pastylka dymka {_app.Nodes.Coach.Pill}");
            }
            Flow.Title.HandleInput(InputCmd.Of(GameAction.A));
            await DebugRunner.Frames(_app.Root, 1);
        }
        if (shown == 0 || p.SecretsNew != 0) throw new Exception($"sekrety: dymek Nowość {shown}x, czeka {p.SecretsNew}");
    }

    /// <summary>Każdy zawód z sekretów: start, moc na mapie (Majster – moc etapu innego fachu, ikona w HUD).</summary>
    private async Task SecretClasses(Profile p, GameData d)
    {
        var s = _app.Session;
        foreach (var sd in d.Secrets.Where(x => x.Reward == SecretReward.Cls)) SecretStaging.Grant(d, p, sd.Id);
        var scenes = new DebugScenes(_app);
        var staging = new DemoStaging(_app);
        foreach (var eff in new[] { AbilityEffect.Weld, AbilityEffect.Mark, AbilityEffect.Borrow })
        {
            var cls = SecretStaging.ClassOf(d, eff);
            if (!Meta.ClassUnlocked(d, p, cls)) throw new Exception($"sekrety: {d.Classes[cls].Name} nie odblokował się");
            Flow.ClassSelect.Open();
            await DebugRunner.Frames(_app.Root, 1);
            s.ClassId = cls;
            _app.StartRun();
            scenes.AdvanceMessages();
            if (Flow.Current != Flow.Game) throw new Exception($"sekrety: {d.Classes[cls].Name} – brak mapy");
            var g = s.Game;
            staging.NewClassShowcase(eff == AbilityEffect.Weld ? AbilityEffect.Line : AbilityEffect.Stun);
            if (eff == AbilityEffect.Borrow && (g.PowerCls() == g.Cls || g.PowerCls() < 0)) throw new Exception("sekrety: Majster bez mocy innego fachu");
            var used = PlayCommands.UseAbility(g, _app.Nodes.World);
            if (!used && eff != AbilityEffect.Borrow) throw new Exception($"sekrety: moc {d.Classes[cls].AbilityName} nie zadziałała");
            if (eff == AbilityEffect.Mark && (g.MarkTarget < 0 || g.MarkTurns <= 0)) throw new Exception("sekrety: Tyczenie nie oznaczyło problemu");
            _app.AfterAction(used);
            for (var k = 0; k < 12 && Flow.Current == Flow.Game && g.St == GameStatus.Playing; k++)
            {
                Bot.StepSmart(g);
                _app.AfterAction(true);
            }
            await DebugRunner.Frames(_app.Root, 2);
            Classes++;
        }
    }

    /// <summary>Kask w paski z Trybu inwestora (klatka bohatera na mapie) i złoty błysk przy krycie.</summary>
    private async Task Cosmetics(Profile p, GameData d)
    {
        var s = _app.Session;
        SecretStaging.Grant(d, p, d.Secrets.First(x => x.Reward == SecretReward.Cosmetic && x.Index == d.CosmeticStripes).Id);
        SecretStaging.Grant(d, p, d.Secrets.First(x => x.Reward == SecretReward.Cosmetic && x.Index == d.CosmeticGold).Id);
        if (p.Wins == 0) p.Wins = 1;
        s.ClassId = 1;
        Flow.ClassSelect.Open();
        Flow.ClassSelect.HandleInput(InputCmd.Of(GameAction.Select));
        if (Flow.Current != Flow.Investor) throw new Exception("sekrety: Tab nie otwiera Trybu inwestora");
        Flow.Investor.Page.Sel = d.Investor.Length;
        Flow.Investor.HandleInput(InputCmd.Of(GameAction.A));
        if (!Secrets.CosmeticOn(d, p, d.CosmeticStripes)) throw new Exception("sekrety: kask w paski się nie włączył");
        await DebugRunner.Frames(_app.Root, 2);
        Flow.Investor.HandleInput(InputCmd.Of(GameAction.B));
        _app.StartRun();
        new DebugScenes(_app).AdvanceMessages();
        _app.Refresh();
        if (_app.Nodes.World.HeroSprite.BaseFrame != Assets.FrameStripes + 2) throw new Exception($"sekrety: klatka bohatera {_app.Nodes.World.HeroSprite.BaseFrame} bez kasku w paski");
        if (!_app.Nodes.World.GoldGlint) throw new Exception("sekrety: złota kielnia nie działa");
        new DemoStaging(_app).Combat(); // kryt -> złoty błysk
        await DebugRunner.Frames(_app.Root, 3);
        Secrets.ToggleCosmetic(d, p, d.CosmeticStripes);
    }

    /// <summary>Poziomica mistrza: narzędzie w dropach po sekrecie, magazyn widać na podglądzie mapy.</summary>
    private async Task LevelOnMap(Profile p, GameData d)
    {
        var tool = Array.FindIndex(d.Tools, t => d.Weapons[t.Weapon].Reveal);
        if (Meta.ToolUnlocked(d, p, tool)) throw new Exception("sekrety: Poziomica dostępna przed sekretem");
        SecretStaging.Grant(d, p, d.Secrets.First(x => x.Reward == SecretReward.Tool && x.Index == tool).Id);
        if (!Meta.ToolUnlocked(d, p, tool)) throw new Exception("sekrety: Poziomica nie trafiła do dropów");
        var g = _app.Session.Game;
        g.WeaponOverride = d.Tools[tool].Weapon;
        Flow.Game.ToggleOverview();
        await DebugRunner.Frames(_app.Root, 2);
        Flow.Game.ToggleOverview();
    }

    private async Task RespectLock(Profile p, GameData d)
    {
        var ri = Array.FindIndex(d.Respect, r => r.Secret >= 0);
        if (ri < 0) return;
        p.Respect = 999;
        p.Secrets = (ushort)(p.Secrets & ~(1 << d.Respect[ri].Secret)); // skrót wygrywa szybko – Szybka ekipa mogła już wpaść
        Flow.Profile.Open(4, true);
        await DebugRunner.Frames(_app.Root, 1);
        if (_app.Nodes.Phone.Current is not TrainingTab tt) throw new Exception("sekrety: brak zakładki Koszty");
        tt.Page = 1;
        tt.Select(ri);
        if (tt.BuyRespect() || Meta.RespectRank(d, p, ri) != 0) throw new Exception("sekrety: ranga Respektu kupiona przed sekretem");
        SecretStaging.Grant(d, p, d.Secrets[d.Respect[ri].Secret].Id);
        if (!tt.BuyRespect()) throw new Exception("sekrety: ranga Respektu po sekrecie się nie kupuje");
        await DebugRunner.Frames(_app.Root, 2);
    }
}
