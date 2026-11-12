using System;
using System.Threading.Tasks;
using LifeLike.Core;
using LifeLike.Game.Phone.Pages;
using LifeLike.Game.Screens;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Debug;

/// <summary>
/// v0.23.2: sceny zrzutów plecaka – karta paczki z trzema wyborami (bag-offer), karta broni (bag-weapon), ekran plecaka
/// (bag, bag-menu z akcjami), Hurtownia: zakupy z zakładkami (bag-shop), Skup (bag-skup), PSZOK (bag-pszok), dymek Marka
/// przy pierwszym przedmiocie (bag-tip).
/// </summary>
public sealed class BackpackStaging
{
    public static readonly string[] Names = ["bag-offer", "bag-weapon", "bag", "bag-menu", "bag-shop", "bag-skup", "bag-pszok", "bag-tip"];

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
                Fill(g);
                _app.Refresh();
                Flow.Backpack.Open(null, true);
                if (scene == "bag-menu")
                {
                    Flow.Backpack.Page.Sel = 0;
                    Flow.Backpack.Page.Confirm();
                }
                else Flow.Backpack.Page.Sel = 2;
                break;
            case "bag-shop":
            case "bag-skup":
            case "bag-pszok":
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
                Flow.Hurtownia.OpenTab(scene == "bag-shop" ? HurtTabs.Shop : scene == "bag-skup" ? HurtTabs.Sell : HurtTabs.Recycle, true);
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
