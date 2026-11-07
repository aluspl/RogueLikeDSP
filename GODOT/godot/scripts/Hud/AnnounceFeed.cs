using System;
using LifeLike.Core;
using LifeLike.Game.Session;

namespace LifeLike.Game.Hud;

/// <summary>
/// v0.23.0: treść ogłoszeń „z przytupem” ze zdarzeń sesji – awans (poziom bohatera), znaleziona Markowa / Mistrzowska
/// broń albo unikat, boss pokonany, sekretne zlecenie wykonane. AnnounceView tylko rysuje.
/// </summary>
public sealed class AnnounceFeed
{
    private readonly GameSession _s;
    private readonly AnnounceView _view;

    public AnnounceFeed(GameSession session, AnnounceView view)
    {
        _s = session;
        _view = view;
        view.DataSource = () => _s.Data; // dane przeładowane po zmianie języka
        var ev = session.Events;
        ev.RunStarted += view.Queue.Clear;
        ev.LevelUp += (level, _) => Push(AnnounceKind.LevelUp, Loc.T("ogl_awans"), Loc.F("ogl_poziom", level));
        ev.ToolFound += _ => OnToolFound();
        ev.BossDefeated += def => Push(AnnounceKind.Boss, Loc.T("ogl_boss"), _s.Data.Enemies[def].Name);
        ev.Achievements += (_, _, secrets) =>
        {
            for (var i = 0; i < _s.Data.Secrets.Length; i++)
            {
                if ((secrets & (1 << i)) != 0) Push(AnnounceKind.Secret, Loc.T("ogl_sekret"), _s.Data.Secrets[i].RewardText);
            }
        };
    }

    private void OnToolFound()
    {
        var g = _s.Game;
        var master = g.Weapon.Master;
        var r = master ? 3 : g.WeaponRarity;
        if (!AnnounceQueue.WeaponWorthy(r, master)) return;
        var kind = AnnounceQueue.WeaponKind(r, master);
        Push(kind, kind == AnnounceKind.Unique ? Loc.T("ogl_unikat") : Loc.T("ogl_markowa"), g.Weapon.Name);
    }

    /// <summary>Ogłoszenie z bronią bohatera (zamach Markowej / Mistrzowskiej albo ikona).</summary>
    private void Push(AnnounceKind kind, string title, string sub)
    {
        var g = _s.Game;
        var w = Array.IndexOf(g.D.Weapons, g.Weapon);
        _view.Push(new Announcement(kind, title, sub, w, g.Weapon.Master ? 3 : Gfx.WeaponSheet.Rarity(g)));
    }
}
