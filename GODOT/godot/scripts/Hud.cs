using System.Collections.Generic;
using Godot;
using LifeLike.Core.Entities;

namespace LifeLike.Game;

/// <summary>HUD: statystyki gracza + log zdarzeń + baner. Budowany z kodu, żeby nie trzymać .tscn w szkielecie.</summary>
public partial class Hud : CanvasLayer
{
    private readonly Label _stats = new() { Position = new Vector2(12, 8) };
    private readonly Label _log = new() { Position = new Vector2(12, 520) };
    private readonly Label _banner = new() { Position = new Vector2(480, 320) };
    private readonly Label _help = new()
    {
        Position = new Vector2(12, 690),
        Text = "Ruch: WSAD/strzałki/D-pad/gałka · Czekaj: Spacja/A · Mysz: klik = krok/atak · Tab: klasa · R: restart"
    };

    public override void _Ready()
    {
        _banner.AddThemeFontSizeOverride("font_size", 28);
        foreach (var l in new[] { _stats, _log, _banner, _help }) AddChild(l);
    }

    public void Show(Actor hero, int round, int enemiesAlive, IEnumerable<string> log)
    {
        var s = hero.Stats;
        _stats.Text = $"{hero.Name}  HP {hero.Health}/{s.MaxHealth}  STR {s.Strength}  AGI {s.Agility}  INT {s.Intelligence}  DEF {s.Defense}\n" +
                      $"Broń: {hero.Weapon?.Name ?? "pięści"} (zasięg {hero.Weapon?.Range ?? 1})  ·  Runda {round}  ·  Wrogów: {enemiesAlive}";
        _log.Text = string.Join("\n", log);
        _banner.Text = "";
    }

    public void Banner(string text) => _banner.Text = text;
}
