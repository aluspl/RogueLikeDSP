using System;
using System.Collections.Generic;
using Godot;
using LifeLike.Core.Data;
using LifeLike.Game.Gfx;
using LifeLike.Game.World;

namespace LifeLike.Game.Screens.Views;

/// <summary>
/// Plac z prologu (run_prologue na GBA) w pikselach mapy: droga dojazdowa z lewej, działka z płotem u góry
/// i u dołu (kafle etapu 1), pickup, bohater i porozrzucane problemy budowy; pył i „!” w FxLayer.
/// Stan (pozycja auta, kamera, widoczność bohatera) ustawia PrologueScreen.
/// </summary>
public partial class PrologueStage : Node2D
{
    public const int Cell = Assets.Cell;
    private const int FieldTop = 4, FieldBottom = 13, FenceFrom = 4, Cols = 34;

    /// <summary>Problemy na działce: identyfikator z game.json i pole (props w run_prologue).</summary>
    private static readonly (string Id, int X, int Y)[] Props =
    [
        ("przeciek", 11, 6), ("zwarcie", 14, 11), ("papierologia", 17, 7), ("plesn", 20, 11),
        ("kornik", 22, 6), ("budzet", 25, 9), ("ulewa", 27, 5), ("termin", 29, 10),
    ];

    private readonly List<(int Frame, Vector2 Pos)> _props = new();
    private readonly bool[] _alerted = new bool[Props.Length];
    private float _clock;

    public FxLayer Fx { get; } = new();
    public float TruckX { get; set; }
    public int TruckFrame { get; set; }
    public bool HeroVisible { get; set; }
    public int HeroFrame { get; set; }

    public static Vector2 At(float x, float y) => new(x * Cell + Cell / 2, y * Cell + Cell / 2);

    public void Setup(GameData d, int heroFrame)
    {
        HeroFrame = heroFrame;
        HeroVisible = false;
        _props.Clear();
        Array.Clear(_alerted);
        Fx.Clear();
        foreach (var (id, x, y) in Props)
        {
            var i = Array.FindIndex(d.Enemies, e => e.Id == id);
            if (i >= 0) _props.Add((d.Enemies[i].Frame, At(x, y)));
        }
    }

    public override void _Ready() => AddChild(Fx);

    /// <summary>„!” nad problemami, które kamera już pokazała (alerted w run_prologue).</summary>
    public void AlertUpTo(float worldX)
    {
        for (var i = 0; i < _props.Count; i++)
        {
            if (_alerted[i] || _props[i].Pos.X >= worldX) continue;
            _alerted[i] = true;
            Fx.Spawn(_props[i].Pos + new Vector2(0, -30), new Vector2(0, -0.3f), 0, 50, Assets.PAlert);
        }
    }

    public override void _Process(double delta)
    {
        _clock += (float)delta;
        QueueRedraw();
    }

    public override void _Draw()
    {
        var tiles = Assets.StageTiles(0);
        for (var y = FieldTop - 1; y <= FieldBottom + 1; y++)
        {
            for (var x = -6; x < Cols; x++)
            {
                int idx;
                if (y >= FieldTop && y <= FieldBottom) idx = Assets.TileFloor + (((x * 73856093) ^ (y * 19349663)) >> 3 & 3);
                else if (x >= FenceFrom) idx = Assets.TileWallFace;
                else continue;
                DrawTextureRectRegion(tiles, new Rect2(x * Cell, y * Cell, Cell, Cell), new Rect2(idx * Cell, 0, Cell, Cell));
            }
        }
        for (var i = 0; i < _props.Count; i++)
        {
            var (frame, pos) = _props[i];
            var alt = ((int)(_clock * 3) + i & 1) == 1 ? Assets.AnimB(frame) : frame;
            DrawActor(Assets.Actors, Assets.Frame(alt, Assets.Actor), pos);
        }
        if (HeroVisible)
        {
            var breath = ((int)(_clock / 0.5f) & 1) == 1;
            DrawActor(breath ? Assets.ActorsAnim : Assets.Actors,
                      breath ? Assets.AnimFrame(HeroFrame, Assets.AnimBreath) : Assets.Frame(HeroFrame, Assets.Actor), At(7, 10));
        }
        var truck = At(TruckX, 9);
        DrawTextureRect(Assets.Shadow, new Rect2(truck + new Vector2(-28, 10), new Vector2(56, 10)), false);
        DrawTextureRectRegion(Assets.Truck, new Rect2(truck - new Vector2(32, 18), new Vector2(64, 32)), new Rect2(0, TruckFrame * 32, 64, 32));
    }

    private void DrawActor(Texture2D tex, Rect2 src, Vector2 pos)
    {
        const int s = Assets.Actor;
        DrawTextureRect(Assets.Shadow, new Rect2(pos + new Vector2(-12, 9), new Vector2(24, 8)), false);
        DrawTextureRectRegion(tex, new Rect2(pos - new Vector2(s / 2, s / 2 + 2), new Vector2(s, s)), src);
    }
}
