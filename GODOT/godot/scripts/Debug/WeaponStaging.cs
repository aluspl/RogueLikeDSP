using System;
using System.Threading.Tasks;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Input;
using LifeLike.Game.Screens;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Debug;

/// <summary>
/// v0.23.0: sceny zrzutów broni – celowanie łukiem trybu A (weapon-arc: Kielnia, trzy problemy), celowanie wzorem B
/// (weapon-b: Gwoździarka, linia), pas monterski po zamianie (weapon-belt: przycisk „Zamień”, wpis w dzienniku), karta
/// problemu z mnożnikiem ciosu (weapon-card: Klucz na kamień – obuch ×125%), telefon Sprzęt z Markową bronią i kaburą
/// (weapon-rarity), wybór zawodu ze szczegółami broni (weapon-class), Jak grać – Bronie (help-weapons) i znaleziony
/// Nóż majstra seniora (weapon-legend-found – ogłoszenie z animacją w zadaniu 9), oferta wymiany z profitami (weapon-perks),
/// opis ataku pod przyciskiem: Fach Kielni (weapon-tip-kielnia, Markowa z Chwytem, odnowienie), Fach Noża majstra seniora
/// (weapon-tip-noz) i Cios Kielni (weapon-tip-cios); Sprzęt przewinięty o sekcję w poziomie (gear-scroll).
/// </summary>
public sealed class WeaponStaging
{
    public static readonly string[] Names =
        ["weapon-arc", "weapon-b", "weapon-belt", "weapon-card", "weapon-rarity", "weapon-class", "help-weapons", "weapon-legend-found", "weapon-perks", "offer-trait", "weapon-tip-kielnia", "weapon-tip-noz",
         "weapon-tip-cios", "gear-scroll"];

    private readonly App _app;

    public WeaponStaging(App app) => _app = app;

    private ScreenFlow Flow => _app.Flow;

    private CoreGame G => _app.Session.Game;

    public static bool Handles(string scene) => Array.IndexOf(Names, scene) >= 0;

    public async Task Setup(string scene)
    {
        switch (scene)
        {
            case "help-weapons":
                Flow.Help.Open(true, true);
                if (_app.Nodes.Phone.Current is Phone.Pages.HelpPage hp) hp.Page = Phone.Pages.HelpPage.WeaponsPage;
                _app.Nodes.Phone.QueueRedraw();
                return;
            case "weapon-class": // karta zawodu: blok broni z typem, rzadkością i trybami
                _app.Session.ClassId = 1;
                Flow.ClassSelect.Open();
                _app.Nodes.ClassSelectView.Details = true;
                return;
        }
        var cls = scene switch { "weapon-arc" or "weapon-tip-kielnia" or "weapon-tip-cios" => 1, "weapon-b" => 2, "weapon-card" => 4, _ => 5 };
        _app.Session.ClassId = cls;
        _app.StartRun();
        new DebugScenes(_app).AdvanceMessages();
        await DebugRunner.Frames(_app.Root, 2);
        var g = G;
        Clear(g);
        switch (scene)
        {
            case "weapon-arc": // Kielnia: łuk 3 pól – trzy problemy przed bohaterem, celownik na środkowym
            {
                var (dx, dy) = OpenSide(g);
                Place(g, "plesn", dx, dy);
                Place(g, "kornik", dx - dy, dy + dx);
                Place(g, "plesn", dx + dy, dy - dx);
                Place(g, "kamien", -dx * 2, -dy * 2 + 1);
                Refresh(g);
                _app.Nodes.Touch.Bar.Pressed = Touch.BarButton.Attack;
                Flow.Game.HandleInput(InputCmd.Of(GameAction.A));
                Flow.Game.Aim.PointAt(new Godot.Vector2I(g.Hero.X + dx, g.Hero.Y + dy));
                break;
            }
            case "weapon-b": // Gwoździarka B: linia 3 przebija – dwa problemy w linii, trzeci obok
            {
                var (dx, dy) = OpenSide(g);
                Place(g, "plesn", dx * 2, dy * 2);
                Place(g, "kornik", dx * 3, dy * 3);
                Place(g, "zwarcie", dx - dy, dy + dx);
                Refresh(g);
                _app.Nodes.Touch.Bar.Pressed = Touch.BarButton.WeaponB;
                Flow.Game.HandleInput(InputCmd.Of(GameAction.WeaponB));
                Flow.Game.Aim.PointAt(new Godot.Vector2I(g.Hero.X + dx * 2, g.Hero.Y + dy * 2));
                break;
            }
            case "weapon-belt": // pas monterski: zamiana na Markowy Łom, Szlifierka w kaburze
            {
                g.Equip(g.D.BeltSlot, CoreGame.BeltTier, 0);
                g.Weapon2 = Array.FindIndex(g.D.Weapons, w => w.Id == "lom");
                g.Weapon2Rarity = 2;
                var (dx, dy) = OpenSide(g);
                Place(g, "kamien", dx * 2, dy * 2);
                Refresh(g);
                Flow.Game.HandleInput(InputCmd.Of(GameAction.Swap));
                g.WeaponBCd = 0;
                break;
            }
            case "weapon-card": // Klucz (obuch + kłucie) na kamień (cement): obuch ×125%
            {
                var (dx, dy) = OpenSide(g);
                Place(g, "kamien", dx, dy);
                Refresh(g);
                _app.Nodes.Touch.Bar.Pressed = Touch.BarButton.Wait;
                Flow.Game.HandleInput(InputCmd.Of(GameAction.B));
                Flow.Game.Look.Reveal();
                break;
            }
            case "weapon-rarity": // telefon Sprzęt: Markowa Siekiera, Solidna Poziomica 2 m w kaburze, odnowienie B
                g.TakeTool(Array.FindIndex(g.D.Tools, t => g.D.Weapons[t.Weapon].Id == "siekiera"), 2, false);
                g.Equip(g.D.BeltSlot, CoreGame.BeltTier, 0);
                g.Weapon2 = Array.FindIndex(g.D.Weapons, w => w.Id == "poziomica_2m");
                g.Weapon2Rarity = 1;
                g.Weapon2Perks = 1 << 3; // v0.23.0: Solidna – jeden profit (Głowica)
                g.WeaponBCd = 3;
                Refresh(g);
                Flow.Phone.Open(3, true);
                break;
            case "weapon-perks": // v0.23.0: oferta wymiany – ulepszona Siekiera z profitem, na ziemi Markowy Łom z dwoma profitami
            {
                g.TakeTool(Array.FindIndex(g.D.Tools, t => g.D.Weapons[t.Weapon].Id == "siekiera"), 1, false, 0, 1 << 1);
                g.WeaponLvl = 1;
                var (dx, dy) = OpenSide(g);
                var t = Array.FindIndex(g.D.Tools, x => g.D.Weapons[x.Weapon].Id == "lom");
                g.Pickups[g.PickupsCount++] = new Pickup(g.Hero.X + dx, g.Hero.Y + dy, PickupType.Tool, true, t, 2) { Perks = (1 << 0) | (1 << 4) };
                Refresh(g);
                _app.AfterAction(g.PlayerMove(dx, dy));
                break;
            }
            case "offer-trait": // v0.23.0 (#91): paczka – te same Solidne rękawice, cecha Szczęście +1 -> Odporność na zatrucie
            {
                g.Equip(1, 1, Array.FindIndex(g.D.GearTraits, t => t.Effect == TraitEffect.Luck));
                Refresh(g);
                g.Pickups[0] = new Pickup(g.Hero.X, g.Hero.Y, PickupType.GearBox, true, 1 * 3 + 1, Array.FindIndex(g.D.GearTraits, t => t.Effect == TraitEffect.PoisonRes));
                g.PickupsCount = 1;
                g.Collect();
                _app.AfterAction(true);
                break;
            }
            case "weapon-tip-kielnia": // Fach Kielni: Markowa z Chwytem, odnowienie 2 t., dymek przytrzymanego przycisku
            case "weapon-tip-cios":
            case "weapon-tip-noz":
            {
                var noz = scene == "weapon-tip-noz";
                if (noz) g.TakeTool(Array.FindIndex(g.D.Tools, t => g.D.Weapons[t.Weapon].Id == "noz_majstra"), 3, false);
                else // Kielnia – broń zawodu (Murarz): Markowa z Chwytem
                {
                    g.WeaponRarity = 2;
                    g.WeaponPerks = (byte)(1 << Array.FindIndex(g.D.WeaponPerkPool, p => p.Id == "chwyt"));
                }
                g.WeaponBCd = noz ? 0 : 2;
                var (dx, dy) = OpenSide(g);
                Place(g, "plesn", dx * 2, dy * 2);
                Place(g, "kornik", dx - dy, dy + dx);
                Refresh(g);
                var b = scene == "weapon-tip-cios" ? Touch.BarButton.Attack : Touch.BarButton.WeaponB;
                Flow.Game.Tips.Holding = true;
                if (_app.Nodes.Touch.Visible)
                {
                    _app.Nodes.Touch.Bar.Pressed = b;
                    Flow.Game.Tips.ShowOverButton(b);
                }
                else _app.Nodes.Hud.ShowAttackTipAtHud(g, b == Touch.BarButton.WeaponB);
                break;
            }
            case "gear-scroll": // telefon Sprzęt przewinięty o sekcję (w poziomie trzy sekcje się nie mieszczą)
                g.TakeTool(Array.FindIndex(g.D.Tools, t => g.D.Weapons[t.Weapon].Id == "siekiera"), 2, false);
                g.Equip(g.D.BeltSlot, CoreGame.BeltTier, 0);
                g.Weapon2 = Array.FindIndex(g.D.Weapons, w => w.Id == "poziomica_2m");
                g.Weapon2Rarity = 1;
                g.Weapon2Perks = 1 << 3;
                Refresh(g);
                Flow.Phone.Open(3, true);
                await DebugRunner.Frames(_app.Root, 2); // pierwsze rysowanie liczy, czy trzeba przewijać
                _app.Nodes.Phone.HandleInput(InputCmd.Of(GameAction.Down));
                break;
            case "weapon-legend-found": // Nóż majstra seniora podniesiony z placu (bohater na polu znajdźki, wpis w dzienniku)
            {
                var (dx, dy) = OpenSide(g);
                var t = Array.FindIndex(g.D.Tools, x => g.D.Weapons[x.Weapon].Id == "noz_majstra");
                g.Pickups[g.PickupsCount++] = new Pickup(g.Hero.X + dx, g.Hero.Y + dy, PickupType.Tool, true, t, 0);
                Place(g, "plesn", dx * 3, dy * 3);
                Refresh(g);
                _app.AfterAction(g.PlayerMove(dx, dy));
                break;
            }
        }
        await DebugRunner.Frames(_app.Root, 2);
    }

    /// <summary>Plac bez problemów i znajdziek wokół bohatera (pola obok – podłoga).</summary>
    private void Clear(CoreGame g)
    {
        if (g.Boss < 0) g.EnemiesCount = 0;
        for (var i = 0; i < g.EnemiesCount; i++)
        {
            if (i != g.Boss) g.Enemies[i].Alive = false;
        }
        g.PickupsCount = 0;
        for (var y = g.Hero.Y - 3; y <= g.Hero.Y + 3; y++)
        {
            for (var x = g.Hero.X - 3; x <= g.Hero.X + 3; x++)
            {
                if (x >= 1 && y >= 1 && x < Level.W - 1 && y < Level.H - 1 && g.Lv[x, y] == Tile.Wall) g.Lv[x, y] = Tile.Floor;
            }
        }
        _app.Nodes.Banners.Clear();
    }

    /// <summary>Kierunek z wolną podłogą na 3 pola (w prawo, w dół, w lewo, w górę).</summary>
    private static (int, int) OpenSide(CoreGame g)
    {
        foreach (var (dx, dy) in new[] { (1, 0), (0, 1), (-1, 0), (0, -1) })
        {
            var ok = true;
            for (var k = 1; k <= 3 && ok; k++) ok = g.Lv.At(g.Hero.X + dx * k, g.Hero.Y + dy * k) == Tile.Floor;
            if (ok) return (dx, dy);
        }
        return (1, 0);
    }

    private static void Place(CoreGame g, string id, int dx, int dy)
    {
        int x = g.Hero.X + dx, y = g.Hero.Y + dy;
        if (g.EnemiesCount >= CoreGame.MaxEnemies || g.Lv.At(x, y) != Tile.Floor || g.Occupied(x, y)) return;
        var ei = g.EnemiesCount;
        g.Spawn(Math.Max(0, Array.FindIndex(g.D.Enemies, e => e.Id == id)), x, y);
        g.Enemies[ei].Hp = (short)Math.Max(1, g.Enemies[ei].MaxHp * 3 / 4);
        g.Enemies[ei].Awake = true;
    }

    private void Refresh(CoreGame g)
    {
        g.Hero.Hp = g.Hero.MaxHp;
        g.UpdateFov();
        _app.Session.ResetWatch();
        _app.Nodes.World.Sync();
        _app.Refresh();
    }
}
