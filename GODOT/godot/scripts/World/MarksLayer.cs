using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;

namespace LifeLike.Game;

/// <summary>
/// Znaczniki nad mapą (nad mgłą): strzałka nad celem krótkiego A, ikona stanu nad bohaterem (zatrucie / porażenie /
/// poślizg na zmianę), „z” nad ogłuszonymi i menu akcji wokół bohatera (Atak ↑, Moc →, Termos ↓, Czekaj ←).
/// </summary>
public partial class MarksLayer : Node2D
{
    private WorldView _w;
    private float _clock;

    public void Bind(WorldView w) => _w = w;

    public override void _Process(double delta)
    {
        _clock += (float)delta;
        QueueRedraw();
    }

    public override void _Draw()
    {
        try
        {
            DrawContent();
        }
        catch (System.Exception ex)
        {
            DrawErrors.Record("MarksLayer", ex);
        }
    }

    private void DrawContent()
    {
        var g = _w?.Game;
        if (g is null || !_w.HeroSprite.Visible) return;
        const int p = Assets.Particle;
        var ptex = Assets.Particles;
        var hero = _w.HeroSprite.Position;

        for (var i = 0; i < g.EnemiesCount; i++)
        {
            var sp = _w.EnemySprite(i);
            if (sp is null || !sp.Visible || sp.Dying || !g.Enemies[i].Alive) continue;
            if (g.Enemies[i].Stun > 0)
            {
                var zy = ((int)(_clock * 3 + i) & 1) == 1 ? -2 : 0;
                Assets.DrawFrame(this, ptex, Assets.PZzz, p, sp.Position + new Vector2(6, -34 + zy));
            }
        }

        var marked = _w.Marked;
        if (marked >= 0 && _w.MenuSel < -1)
        {
            var sp = _w.EnemySprite(marked);
            if (sp is not null && sp.Visible && !sp.Dying)
            {
                var by = ((int)(_clock * 6) & 1) == 1 ? 2 : 0;
                Assets.DrawFrame(this, ptex, Assets.PMarker, p, sp.Position + new Vector2(-p / 2, -46 - by));
            }
        }

        // ikona stanu nad bohaterem (co 40 klatek następny aktywny stan)
        StatusEffect[] order = [StatusEffect.Poison, StatusEffect.Shock, StatusEffect.Slip];
        var active = new System.Collections.Generic.List<int>();
        for (var k = 0; k < 3; k++)
        {
            if (g.StatusTurns(order[k]) > 0) active.Add(k);
        }
        if (active.Count > 0 && _w.MenuSel < -1)
        {
            var k = active[(int)(_clock / 0.66f) % active.Count];
            Assets.DrawFrame(this, ptex, Assets.PStatus + k, p, hero + new Vector2(6, -34));
        }

        if (_w.MenuSel >= -1) DrawMenu(g, hero);
    }

    private void DrawMenu(LifeLike.Core.Game g, Vector2 hero)
    {
        const int s = 32;
        for (var k = 0; k < 4; k++)
        {
            var c = hero + new Vector2(WorldView.MenuDirs[k].X, WorldView.MenuDirs[k].Y) * 44;
            var sel = k == _w.MenuSel;
            DrawCircle(c, 19, new Color(Pal.Navy, 0.82f));
            DrawArc(c, 19, 0, Mathf.Tau, 24, sel ? Pal.Prog : new Color(1, 1, 1, 0.35f), sel ? 2f : 1f);
            var tl = c - new Vector2(s / 2, s / 2);
            if (k == 1)
            {
                var ready = g.AbilityCd == 0;
                Assets.DrawFrame(this, Assets.AbilityIcons, g.Cls, s, tl, 1, ready ? Colors.White : new Color(0.55f, 0.55f, 0.6f));
            }
            else
            {
                Assets.DrawFrame(this, Assets.MenuIcons, k == 0 ? 0 : k == 2 ? 1 : 2, s, tl);
            }
            if (sel)
            {
                var pulse = ((int)(_clock * 5) & 1) == 1 ? 1 : 0;
                Assets.DrawFrame(this, Assets.MenuIcons, 3, s, tl - new Vector2(pulse, pulse), 1);
            }
        }
    }
}
