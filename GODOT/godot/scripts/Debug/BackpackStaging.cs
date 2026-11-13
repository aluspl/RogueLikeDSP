using System;
using System.Threading.Tasks;
using LifeLike.Core;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;
using LifeLike.Game.Screens;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Debug;

/// <summary>
/// v0.23.2: sceny zrzutów plecaka – karta paczki z trzema wyborami (bag-offer), karta broni (bag-weapon), ekran plecaka
/// (bag, bag-menu z akcjami), Hurtownia: zakupy z zakładkami (bag-shop), Skup (bag-skup), PSZOK (bag-pszok), dymek Marka
/// przy pierwszym przedmiocie (bag-tip). v0.23.2 (UI 2): pauza (pause, pause-quit z potwierdzeniem); Postać (char, bag = Postać
/// z kartą kasku, bag-menu = karta broni z plecaka, char-gear – karta noszonej kamizelki, char-hand – broń w ręku ze
/// szczegółami, char-empty – pusty plecak, char-helmet – karta noszonego kasku: Zmień / Do plecaka / Na PSZOK,
/// char-change – lista Zmień z podglądem, char-full – pełny plecak: „Do plecaka” wyszarzone); Hurtownia: Plecak (bag-hurt, bag-hurt-sheet), karta w Skupie (bag-skup-sheet).
/// </summary>
public sealed class BackpackStaging
{
    public static readonly string[] Names = ["bag-offer", "bag-weapon", "bag", "bag-menu", "bag-shop", "bag-skup", "bag-pszok", "bag-tip", "pause", "pause-quit",
        "char", "char-gear", "char-hand", "char-empty", "char-change", "char-full", "char-helmet", "bag-hurt", "bag-hurt-sheet", "bag-skup-sheet"];

    private readonly App _app;

    public BackpackStaging(App app) => _app = app;

    private ScreenFlow Flow => _app.Flow;

    public static bool Handles(string scene) => Array.IndexOf(Names, scene) >= 0;

    private static int Tool(CoreGame g, string id) => Array.FindIndex(g.D.Tools, t => g.D.Weapons[t.Weapon].Id == id);

    private static int Weapon(CoreGame g, string id) => Array.FindIndex(g.D.Weapons, w => w.Id == id);

    /// <summary>Plecak: Łom Markowy +1, Siekiera Zwykła, Kask Markowy, Rękawice Zwykłe, Kamizelka Solidna.</summary>
    private static void Fill(CoreGame g)
    {
        g.Backpack[0] = BackpackItem.Weapon(Weapon(g, "lom"), 2, 1, 1);
        g.Backpack[1] = BackpackItem.Weapon(Weapon(g, "siekiera"), 0, 0, 0);
        g.Backpack[2] = BackpackItem.Gear(0, 2, 3);
        g.Backpack[3] = BackpackItem.Gear(1, 0, 1);
        g.Backpack[4] = BackpackItem.Gear(2, 1, 2);
    }

    public async Task Setup(string scene)
    {
        _app.Session.ClassId = 1; // Murarz z kielnią
        _app.StartRun();
        new DebugScenes(_app).AdvanceMessages();
        await DebugRunner.Frames(_app.Root, 2);
        var g = _app.Session.Game;
        if (scene != "bag-tip") _app.Session.Profile.Tutorial |= Tutorial.Backpack;
        _app.Nodes.Banners.Clear();
        g.EnemiesCount = 0;
        g.Equip(0, 1, 1);
        g.Equip(2, 1, 0);
        _app.Session.ResetWatch();
        _app.Refresh();
        _app.Nodes.Banners.Clear();
        switch (scene)
        {
            case "bag-offer":
                g.Backpack[0] = BackpackItem.Weapon(Weapon(g, "lom"), 2, 1, 0);
                g.Backpack[1] = BackpackItem.Gear(3, 0, 0);
                g.Pickups[0] = new Pickup(g.Hero.X, g.Hero.Y, PickupType.GearBox, true, 0 * 3 + 2, 3);
                g.PickupsCount = Math.Max(1, g.PickupsCount);
                g.Collect();
                _app.AfterAction(true);
                break;
            case "bag-weapon":
                g.Backpack[0] = BackpackItem.Gear(3, 0, 0);
                g.WeaponLvl = 1;
                g.Pickups[0] = new Pickup(g.Hero.X, g.Hero.Y, PickupType.Tool, true, Tool(g, "siekiera"), 2) { Perks = 1 };
                g.PickupsCount = Math.Max(1, g.PickupsCount);
                g.Collect();
                _app.AfterAction(true);
                break;
            case "bag":
            case "bag-menu":
            case "char":
            case "char-gear":
            case "char-hand":
            case "char-empty":
            case "char-change":
            case "char-full":
            case "char-helmet":
                if (scene != "char-empty") Fill(g);
                if (scene == "char-full") g.Backpack[5] = BackpackItem.Gear(1, 1, 4);
                g.Equip(4, 1, 0); // pas monterski z kaburą
                g.Weapon2 = Weapon(g, "mlotek");
                g.Hero.Hp = (short)(g.Hero.MaxHp - 7);
                _app.Refresh();
                Flow.Character.Open(null, true);
                var page = Flow.Character.Page;
                if (scene == "bag") page.OpenSheet(Phone.Pages.CharacterPage.BagBase + 2);
                else if (scene == "bag-menu") page.OpenSheet(Phone.Pages.CharacterPage.BagBase);
                else if (scene == "char-gear") page.OpenSheet(2);
                else if (scene is "char-helmet" or "char-full") page.OpenSheet(0);
                else if (scene == "char-change")
                {
                    page.OpenSheet(0);
                    page.Sheet.Input(InputCmd.Of(GameAction.A)); // główny przycisk: Zmień
                }
                else if (scene == "char-hand")
                {
                    page.OpenSheet(Phone.Pages.CharacterPage.HandId);
                    page.Sheet.Tap(Phone.ItemSheet.HitBase + page.Sheet.Buttons().IndexOf(Phone.SheetAct.Details)); // szczegóły trybów
                }
                else page.Focus = Phone.Pages.CharacterPage.BagBase + 1;
                break;
            case "bag-shop":
            case "bag-skup":
            case "bag-pszok":
            case "bag-hurt":
            case "bag-hurt-sheet":
            case "bag-skup-sheet":
                Fill(g);
                g.Mats[0] = 2;
                g.Mats[1] = 3;
                g.Mats[2] = 1;
                g.Cash = 140;
                g.ActBonus = 120;
                g.Hero.Hp = (short)(g.Hero.MaxHp - 5);
                g.St = GameStatus.StageClear;
                g.ActCleared = true;
                _app.Refresh();
                Flow.Hurtownia.OpenTab(scene == "bag-shop" ? HurtTabs.Shop : scene is "bag-skup" or "bag-skup-sheet" ? HurtTabs.Sell
                    : scene is "bag-hurt" or "bag-hurt-sheet" ? HurtTabs.Bag : HurtTabs.Recycle, true);
                if (scene == "bag-skup-sheet") Flow.Hurtownia.Bag.TapRow(0);
                if (scene == "bag-hurt-sheet") Flow.Hurtownia.Character.OpenSheet(Phone.Pages.CharacterPage.BagBase + 4);
                break;
            case "pause":
            case "pause-quit":
                Fill(g);
                g.Hero.Hp = (short)(g.Hero.MaxHp * 2 / 3);
                _app.Refresh();
                Flow.Pause.Open(true);
                if (scene == "pause-quit") Flow.Pause.Pick(PausePage.Quit);
                break;
            case "bag-tip":
                _app.Session.Profile.Tutorial = (ushort)(_app.Session.Profile.Tutorial & ~Tutorial.Backpack);
                g.Pickups[0] = new Pickup(g.Hero.X, g.Hero.Y, PickupType.GearBox, true, 0 * 3 + 2, 3);
                g.PickupsCount = Math.Max(1, g.PickupsCount);
                g.Collect();
                g.OfferToBackpack();
                _app.AfterAction(true);
                break;
        }
        _app.Nodes.Phone.QueueRedraw();
    }
}
