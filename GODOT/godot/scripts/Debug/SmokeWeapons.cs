using System;
using System.Threading.Tasks;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using LifeLike.Game.Hud;
using LifeLike.Game.Input;
using LifeLike.Game.Screens;
using LifeLike.Game.Touch;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Debug;

/// <summary>
/// v0.23.0: test dymny broni przez prawdziwe wejście ekranu budowy – Cios B (F: celowanie wzorem B, puszczenie = cios,
/// odnowienie rusza; w odnowieniu bez tury), Zamień (X: bez pasa nic, z pasem i bronią w kaburze – zamiana za turę,
/// przycisk „Zamień” na pasku dotyku), karta problemu z wierszem mnożnika ciosu (Loc „mnoznik_ciosu”).
/// </summary>
public sealed class SmokeWeapons
{
    private readonly App _app;

    public SmokeWeapons(App app) => _app = app;

    private ScreenFlow Flow => _app.Flow;

    /// <summary>Opis wyniku do linii SMOKE.</summary>
    public string Summary { get; private set; } = "-";

    public async Task Run()
    {
        var s = _app.Session;
        s.ClassId = 1; // Murarz: Kielnia, tryb B – strzał 2 z ogłuszeniem
        s.Difficulty = 0;
        _app.StartRun();
        new DebugScenes(_app).AdvanceMessages();
        var g = s.Game;
        if (Flow.Current != Flow.Game) throw new Exception($"bronie: brak ekranu budowy ({Flow.Current.GetType().Name})");
        var target = Arena(g);

        // Cios B: F wciśnięte – celowanie wzorem B, puszczone – cios i odnowienie
        var turns = g.Turns;
        Flow.Game.HandleInput(InputCmd.Of(GameAction.WeaponB));
        if (!Flow.Game.Aim.Active || !Flow.Game.Aim.ModeB || Flow.Game.Aim.Target != target) throw new Exception("Cios B: brak celowania wzorem B");
        Flow.Game.HandleInput(InputCmd.Release(GameAction.WeaponB));
        if (g.WeaponBCd <= 0 || g.Turns != turns + 1) throw new Exception($"Cios B: odnowienie {g.WeaponBCd}, tury {g.Turns - turns}");
        var cd = g.WeaponBCd;
        turns = g.Turns;
        Flow.Game.HandleInput(InputCmd.Of(GameAction.WeaponB)); // w odnowieniu: tylko wpis, bez celowania i tury
        Flow.Game.HandleInput(InputCmd.Release(GameAction.WeaponB));
        if (Flow.Game.Aim.Active || g.Turns != turns || g.WeaponBCd != cd) throw new Exception("Cios B w odnowieniu zużył turę");
        await DebugRunner.Frames(_app.Root, 2);

        // Zamień bez pasa: nic (bez tury)
        var d = g.D;
        var lom = Array.FindIndex(d.Weapons, w => w.Id == "lom");
        g.Weapon2 = lom;
        var hand = g.WeaponOverride;
        Flow.Game.HandleInput(InputCmd.Of(GameAction.Swap));
        if (g.Turns != turns || g.WeaponOverride != hand) throw new Exception("Zamień bez pasa zmienił broń albo zużył turę");
        Layout.Touch = true;
        try
        {
            if (Array.IndexOf(ActionBar.Order, BarButton.Swap) >= 0) throw new Exception("pasek dotyku: „Zamień” bez pasa");
            if (Array.IndexOf(ActionBar.Order, BarButton.WeaponB) < 0) throw new Exception("pasek dotyku: brak „Cios B”");
            // z pasem: przycisk i zamiana za turę
            g.Equip(d.BeltSlot, CoreGame.BeltTier, 0);
            g.Weapon2 = lom;
            if (Array.IndexOf(ActionBar.Order, BarButton.Swap) < 0) throw new Exception("pasek dotyku: brak „Zamień” z pasem");
        }
        finally
        {
            Layout.Touch = false;
        }
        g.Hero.Hp = g.Hero.MaxHp;
        Flow.Game.HandleInput(InputCmd.Of(GameAction.Swap));
        if (g.WeaponOverride != lom || g.Turns != turns + 1 || g.Weapon2 != g.CDef.Weapon) throw new Exception("Zamień z pasem nie zamienił broni za turę");
        await DebugRunner.Frames(_app.Root, 2);

        // karta problemu: wiersz mnożnika broni w ręku
        if (g.St != GameStatus.Playing || Flow.Current != Flow.Game) throw new Exception("bronie: budowa skończona przed kartą");
        target = Arena(g);
        Flow.Game.HandleInput(InputCmd.Of(GameAction.B));
        Flow.Game.Look.Reveal();
        await DebugRunner.Frames(_app.Root, 2);
        var card = _app.Nodes.Hud.Card;
        var ed = d.Enemies[g.Enemies[target].DefId];
        var col = HitCalc.Column(d, ed);
        var mat = col < d.Materials.Length ? d.Materials[col].Name : col == 3 ? Loc.T("zywiol_woda") : Loc.T("bez_materialu");
        var want = Loc.F("mnoznik_ciosu", mat, d.HitTypes.Names[(int)HitCalc.Best(d, g.Weapon, col)], HitCalc.Pct(d, g.Weapon, ed));
        if (!card.Visible || card.HitLine != want) throw new Exception($"karta problemu: „{card.HitLine}” zamiast „{want}”");
        Flow.Game.Look.Cancel();
        Summary = $"cios B (odn. {cd}), zamiana, karta „{card.HitLine}”";
    }

    /// <summary>Plac bez innych problemów: jeden wytrzymały problem tuż obok bohatera (w linii – każdy wzór go sięga).</summary>
    private int Arena(CoreGame g)
    {
        if (g.Boss < 0) g.EnemiesCount = 0;
        for (var i = 0; i < g.EnemiesCount; i++)
        {
            if (i != g.Boss) g.Enemies[i].Alive = false;
        }
        foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
        {
            int x = g.Hero.X + dx, y = g.Hero.Y + dy;
            if (g.Lv.At(x, y) != Tile.Floor || g.Occupied(x, y)) continue;
            var ei = g.EnemiesCount;
            g.Spawn(0, x, y);
            g.Enemies[ei].Hp = g.Enemies[ei].MaxHp = 500;
            g.Enemies[ei].Awake = false;
            g.Hero.Hp = g.Hero.MaxHp;
            g.UpdateFov();
            _app.Refresh();
            return ei;
        }
        throw new Exception("bronie: brak wolnego pola obok bohatera");
    }
}
