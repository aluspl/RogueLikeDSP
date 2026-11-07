using LifeLike.Core;
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
    private int _level, _weapon, _weapon2 = -1, _pickups, _cd, _active;
    private readonly sbyte[] _equipped = new sbyte[CoreGame.MaxGearSlots];
    private bool _bossSeen, _second, _phase, _bossAlive;
    private int _docs;
    private int _stage = -1, _tier = -1;

    public TurnWatcher(SessionEvents events) => _events = events;

    /// <summary>Stan odniesienia bez zdarzeń (start etapu, wczytanie).</summary>
    public void Reset(CoreGame g)
    {
        _level = g.HeroLevel;
        _weapon = g.WeaponOverride;
        _weapon2 = g.Weapon2;
        _pickups = g.PickupsCount;
        _cd = g.AbilityCd;
        _active = ActivePickups(g);
        for (var i = 0; i < CoreGame.MaxGearSlots; i++) _equipped[i] = g.Equipped[i];
        _bossSeen = false;
        _second = g.SecondUsed;
        _docs = g.Docs;
        _phase = g.Boss >= 0 && (g.Enemies[g.Boss].Flags & ActorFlag.Phase) != 0;
        _stage = g.Stage;
        _tier = g.Tier;
        _bossAlive = g.Boss >= 0 && g.Enemies[g.Boss].Alive;
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
        // v0.23.0: zamiana z kabury to nie znalezisko (bez ogłoszenia „ARTEFAKT!” i banera nowej broni)
        var swapped = AnnounceQueue.IsHolsterSwap(_weapon >= 0 ? _weapon : g.CDef.Weapon, _weapon2, g.WeaponOverride >= 0 ? g.WeaponOverride : g.CDef.Weapon, g.Weapon2);
        if (g.WeaponOverride != _weapon && g.WeaponOverride >= 0 && !swapped) _events.RaiseToolFound(_weapon);
        // v0.23.0: artefakt prosto do kabury (bez zmiany broni w ręku – zamiana z kabury to nie znalezisko)
        if (g.Weapon2 != _weapon2 && g.Weapon2 >= 0 && g.WeaponOverride == _weapon && g.D.Weapons[g.Weapon2].Master) _events.RaiseArtifactHolstered(g.Weapon2);
        _weapon2 = g.Weapon2;
        if (g.PickupsCount > _pickups) _events.RaiseDropped();
        for (var i = 0; i < g.D.GearSlotsCount && i < CoreGame.MaxGearSlots; i++)
        {
            if (g.Equipped[i] != _equipped[i] && g.Equipped[i] >= 0) _events.RaiseGearEquipped(i);
            _equipped[i] = g.Equipped[i];
        }
        if (_cd > 0 && g.AbilityCd == 0) _events.RaiseAbilityReady();
        if (g.SecondUsed && !_second) _events.RaiseSecondChance();   // Druga szansa z Respektu
        _second = g.SecondUsed;
        if (g.Docs != _docs)   // Akt 0: nowy dokument (komplet otwiera schody)
        {
            for (var i = 0; i < g.D.Documents.Length; i++)
            {
                if ((g.Docs >> i & 1) != 0 && (_docs >> i & 1) == 0) _events.RaiseDocumentFound(i, !g.StairsLocked());
            }
            _docs = g.Docs;
        }
        if (!_phase && g.Boss >= 0 && g.Enemies[g.Boss].Alive && (g.Enemies[g.Boss].Flags & ActorFlag.Phase) != 0)   // druga faza bossa
        {
            _phase = true;
            _events.RaiseBossPhase(g.Enemies[g.Boss].DefId);
        }
        if (!_bossSeen && g.Boss >= 0 && g.Enemies[g.Boss].Alive && g.Visible(g.Enemies[g.Boss].X, g.Enemies[g.Boss].Y))
        {
            _bossSeen = true;
            _events.RaiseBossSpotted(g.Enemies[g.Boss].DefId);
        }
        var bossAlive = g.Boss >= 0 && g.Enemies[g.Boss].Alive;
        if (_bossAlive && !bossAlive) _events.RaiseBossDefeated(g.Enemies[g.Boss].DefId); // v0.23.0: ogłoszenie
        _bossAlive = bossAlive;
        _level = g.HeroLevel;
        _weapon = g.WeaponOverride;
        _weapon2 = g.Weapon2;
        _pickups = g.PickupsCount;
        _cd = g.AbilityCd;
    }
}
