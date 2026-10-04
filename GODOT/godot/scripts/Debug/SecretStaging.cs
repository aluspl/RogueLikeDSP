using System;
using System.Threading.Tasks;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Screens;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Debug;

/// <summary>
/// Sceny zrzutów v0.21.51 cz. 2 – sekretne zlecenia (jak scenariusze GBA 61-67): strona Sekrety w profilu
/// (część wykonana / nic), banery „Sekretne zlecenie!”, dymek Nowość na tytule, nowe zawody na wyborze zawodu
/// (odblokowane i zablokowany z podpowiedzią), ich moce na placu (Spaw, Tyczenie, Złota rączka z mocą etapu w HUD),
/// kask w paski (plac i Tryb inwestora), złoty błysk przy krycie, Poziomica mistrza na podglądzie mapy, zablokowany
/// Respekt „Zaprawiony w boju” i strona Jak grać.
/// </summary>
public sealed class SecretStaging
{
    public static readonly string[] Names =
    [
        "sekrety", "sekrety-locked", "sekret-banner", "sekret-news", "class-spawacz", "class-geodeta", "class-majster",
        "class-sekret-locked", "power-spaw", "power-tyczenie", "power-majster", "stripes", "stripes-investor", "gold-crit",
        "poziomica-map", "respect-sekret", "help-secrets",
    ];

    private readonly App _app;
    private readonly DemoStaging _stage;

    public SecretStaging(App app)
    {
        _app = app;
        _stage = new DemoStaging(app);
    }

    private ScreenFlow Flow => _app.Flow;

    public static bool Handles(string scene) => Array.IndexOf(Names, scene) >= 0;

    /// <summary>Sceny na profilu pokazowym (część odznak, wygrane – tryb inwestora).</summary>
    public static bool UsesDemoProfile(string scene) => scene is "sekrety" or "class-sekret-locked" or "stripes-investor" or "respect-sekret";

    /// <summary>Wykonane sekretne zlecenie o identyfikatorze id (bez dymka Nowość).</summary>
    public static void Grant(GameData d, Profile p, string id)
    {
        var i = Array.FindIndex(d.Secrets, s => s.Id == id);
        if (i >= 0) p.Secrets = (ushort)(p.Secrets | (1 << i));
    }

    /// <summary>Zawód z sekretnego zlecenia o tej mocy (Spaw, Tyczenie, Złota rączka).</summary>
    public static int ClassOf(GameData d, AbilityEffect e) => Array.FindIndex(d.Classes, c => c.Ability == e);

    public async Task Setup(string scene)
    {
        var s = _app.Session;
        var d = s.Data;
        var p = s.Profile;
        switch (scene)
        {
            case "sekrety": // połowa wykonana: zawód Spawacz (portret), Poziomica, Złota kielnia, Kask w paski
            case "sekrety-locked": // nic nie wykonane: same „???” z podpowiedziami
            {
                if (scene == "sekrety")
                {
                    foreach (var id in new[] { "bez_kawy", "szczur", "mokra_robota", "na_styk" }) Grant(d, p, id);
                }
                Flow.Profile.Open(0, true);
                if (_app.Nodes.Phone.Current is Phone.ProfileTabs.BadgesTab bt)
                {
                    bt.Page = Phone.ProfileTabs.BadgesTab.SecretsPage;
                    bt.Select(scene == "sekrety" ? 0 : 1);
                }
                _app.Nodes.Phone.QueueRedraw();
                return;
            }
            case "sekret-news": // tytuł: dymek „Nowość” po wykonanym sekretnym zleceniu (scenariusz GBA 61)
            {
                var i = Array.FindIndex(d.Secrets, x => x.Id == "bez_kawy");
                p.Secrets = (ushort)(1 << i);
                p.SecretsNew = (ushort)(1 << i);
                Flow.Title.Open();
                return;
            }
            case "class-spawacz":
            case "class-geodeta":
            case "class-majster":
            case "class-sekret-locked":
            {
                var e = scene switch { "class-geodeta" => AbilityEffect.Mark, "class-spawacz" => AbilityEffect.Weld, _ => AbilityEffect.Borrow };
                if (scene != "class-sekret-locked")
                {
                    foreach (var sd in d.Secrets)
                    {
                        if (sd.Reward == SecretReward.Cls) Grant(d, p, sd.Id);
                    }
                }
                s.ClassId = ClassOf(d, e);
                Flow.ClassSelect.Open();
                return;
            }
            case "stripes-investor": // Tryb inwestora: wiersz wyglądu „Kask w paski” (WŁ), w tle portret w pasach
                Grant(d, p, "na_styk");
                p.Cosmetic = (byte)(1 << d.CosmeticStripes);
                p.Wins = Math.Max(1, p.Wins);
                s.ClassId = 1;
                Flow.ClassSelect.Open();
                Flow.Investor.Open(true);
                Flow.Investor.Page.Sel = d.Investor.Length;
                return;
            case "respect-sekret": // Koszty > Respekt: „???” z pastylką Sekret, podpowiedź zlecenia w opisie
            {
                Flow.Profile.Open(4, true);
                if (_app.Nodes.Phone.Current is Phone.ProfileTabs.TrainingTab tt)
                {
                    tt.Page = Phone.ProfileTabs.TrainingTab.RespectPage;
                    tt.Select(Array.FindIndex(d.Respect, r => r.Secret >= 0));
                }
                _app.Nodes.Phone.QueueRedraw();
                return;
            }
            case "help-secrets":
                Flow.Help.Open(true, true);
                if (_app.Nodes.Phone.Current is Phone.Pages.HelpPage hp) hp.Page = 7;
                _app.Nodes.Phone.QueueRedraw();
                return;
        }
        await SetupRun(scene);
    }

    private async Task SetupRun(string scene)
    {
        var s = _app.Session;
        var d = s.Data;
        var p = s.Profile;
        foreach (var sd in d.Secrets) Grant(d, p, sd.Id); // wszystko odblokowane (zawody, narzędzia, wygląd)
        p.Cosmetic = 0;
        if (scene is "stripes" or "gold-crit") p.Cosmetic = (byte)(1 << d.CosmeticStripes);
        if (scene != "gold-crit") p.Secrets = (ushort)(p.Secrets & ~(1 << Array.FindIndex(d.Secrets, x => x.Id == "mokra_robota")));
        s.ClassId = scene switch
        {
            "power-spaw" => ClassOf(d, AbilityEffect.Weld),
            "power-tyczenie" => ClassOf(d, AbilityEffect.Mark),
            "power-majster" => ClassOf(d, AbilityEffect.Borrow),
            _ => 1,
        };
        _app.StartRun();
        Flow.StageCard.Advance();
        var g = s.Game;
        var banners = _app.Nodes.Banners;
        banners.Clear();
        switch (scene)
        {
            case "power-spaw":
            case "power-tyczenie":
            case "power-majster":
            {
                _stage.NewClassShowcase(scene == "power-spaw" ? AbilityEffect.Line : AbilityEffect.Stun);
                if (scene == "power-majster")
                {
                    banners.Push("R: " + g.PDef.AbilityName, g.PDef.AbilityDesc, Phone.PhoneTabs.Start); // moc etapu
                    _app.Refresh();
                    break;
                }
                var acted = Screens.Play.PlayCommands.UseAbility(g, _app.Nodes.World);
                _app.AfterAction(acted);
                break;
            }
            case "sekret-banner": // koniec budowy: nagroda, sekretne zlecenia, odznaka (kolejka 8 banerów)
                p.Secrets = 0;
                _stage.BringEnemies();
                _app.Session.Events.RaiseRewardUnlocked(0);
                _app.Session.Events.RaiseAchievements(1 << d.BadgeBezUsterek, 0,
                    (1 << Array.FindIndex(d.Secrets, x => x.Id == "na_styk")) | (1 << Array.FindIndex(d.Secrets, x => x.Id == "szczur")));
                break;
            case "stripes":
                _stage.BringEnemies();
                break;
            case "gold-crit":
                await DebugRunner.Frames(_app.Root, 30);
                _stage.Combat();
                _app.Root.GetTree().CreateTimer(0.6).Timeout += () => _app.Nodes.World.Effects.GoldGlint(_app.Nodes.World.HeroSprite.Position + new Godot.Vector2(32, 0));
                break;
            case "poziomica-map":
                PoziomicaStage(g);
                Flow.Game.ToggleOverview();
                break;
        }
        await DebugRunner.Frames(_app.Root, 4);
    }

    /// <summary>Etap z magazynem daleko od bohatera (pod mgłą) i Poziomica mistrza w ręku.</summary>
    private void PoziomicaStage(CoreGame g)
    {
        var bas = g.RunSeed;
        for (uint k = 1; k < 4000; k++)
        {
            g.RunSeed = bas + k * 7919u;
            g.StartStage(g.D.PreludeStages + 1);
            if (g.HasSecret && !g.Explored(g.SecretX, g.SecretY) && CoreGame.Cheb(g.SecretX, g.SecretY, g.Hero.X, g.Hero.Y) > 12) break;
        }
        g.WeaponOverride = g.D.Tools[Array.FindIndex(g.D.Tools, t => g.D.Weapons[t.Weapon].Reveal)].Weapon;
        g.StageEvent = -1;
        _app.Session.ResetWatch();
        _app.Nodes.World.Sync();
        _app.Refresh();
    }
}
