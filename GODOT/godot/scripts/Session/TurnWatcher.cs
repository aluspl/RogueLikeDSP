using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Session;

/// <summary>
/// Wykrywa ważne zdarzenia tury (detect_events w GBA/src/main.cpp) przez porównanie stanu rdzenia z poprzednim:
/// zebrana znajdźka, awans, nowe narzędzie, drop, założony sprzęt, moc gotowa, boss w polu widzenia.
/// Rdzeń (port 1:1 z GBA) nie zgłasza zdarzeń sam, więc to jedyne miejsce, które „diffuje” stan.
/// </summary>
public sealed class TurnWatcher
{
    private readonly SessionEvents _events;
    private int _level, _weapon, _pickups, _cd, _active;
    private readonly sbyte[] _equipped = new sbyte[4];
    private bool _bossSeen;
    private int _stage = -1, _tier = -1;

    public TurnWatcher(SessionEvents events) => _events = events;

    /// <summary>Stan odniesienia bez zdarzeń (start etapu, wczytanie).</summary>
    public void Reset(CoreGame g)
    {
        _level = g.HeroLevel;
        _weapon = g.WeaponOverride;
        _pickups = g.PickupsCount;
        _cd = g.AbilityCd;
        _active = ActivePickups(g);
        for (var i = 0; i < 4; i++) _equipped[i] = g.Equipped[i];
        _bossSeen = false;
        _stage = g.Stage;
        _tier = g.Tier;
    }

    private static int ActivePickups(CoreGame g)
    {
        var n = 0;
        for (var i = 0; i < g.PickupsCount; i++)
        {
            if (g.Pickups[i].Active) n++;
        }
        return n;
    }

    public void Check(CoreGame g)
    {
        if (g.Stage != _stage || g.Tier != _tier || g.PickupsCount < _pickups) Reset(g);
        var active = ActivePickups(g);
        if (active < _active) _events.RaisePickedUp();
        _active = active;
        if (g.HeroLevel > _level)
        {
            var oldRank = 1 + (_level >= 3 ? 1 : 0) + (_level >= 5 ? 1 : 0);
            _events.RaiseLevelUp(g.HeroLevel, g.AbilityRank() > oldRank);
        }
        if (g.WeaponOverride != _weapon && g.WeaponOverride >= 0) _events.RaiseToolFound();
        if (g.PickupsCount > _pickups) _events.RaiseDropped();
        for (var i = 0; i < g.D.GearSlotsCount && i < 4; i++)
        {
            if (g.Equipped[i] != _equipped[i] && g.Equipped[i] >= 0) _events.RaiseGearEquipped(i);
            _equipped[i] = g.Equipped[i];
        }
        if (_cd > 0 && g.AbilityCd == 0) _events.RaiseAbilityReady();
        if (!_bossSeen && g.Boss >= 0 && g.Enemies[g.Boss].Alive && g.Visible(g.Enemies[g.Boss].X, g.Enemies[g.Boss].Y))
        {
            _bossSeen = true;
            _events.RaiseBossSpotted(g.Enemies[g.Boss].DefId);
        }
        _level = g.HeroLevel;
        _weapon = g.WeaponOverride;
        _pickups = g.PickupsCount;
        _cd = g.AbilityCd;
    }
}
