using LifeLike.Core;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game;

/// <summary>
/// Powiadomienia push i dźwięki o ważnych zdarzeniach tury (detect_events w GBA/src/main.cpp): zebrana znajdźka,
/// awans (z gwiazdkami i silniejszą mocą), nowe narzędzie, drop, założony sprzęt, moc gotowa, pojawienie się bossa.
/// </summary>
public sealed class EventWatcher
{
    private readonly PushBanners _banners;
    private readonly WorldView _world;
    private int _level, _weapon, _pickups, _cd, _active;
    private readonly sbyte[] _equipped = new sbyte[4];
    private bool _bossSeen;
    private int _stage = -1, _tier = -1;

    public EventWatcher(PushBanners banners, WorldView world)
    {
        _banners = banners;
        _world = world;
    }

    /// <summary>Stan odniesienia bez powiadomień (start etapu, wczytanie).</summary>
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
        if (active < _active) Sfx.Play("pickup");
        _active = active;
        if (g.HeroLevel > _level)
        {
            Sfx.Play("level");
            var oldRank = 1 + (_level >= 3 ? 1 : 0) + (_level >= 5 ? 1 : 0);
            if (g.AbilityRank() > oldRank) _banners.Push("Moc: " + UiText.AbilityLabel(g), "Silniejsza, szybciej gotowa");
            _world.LevelUpFx();
            var body = $"+{g.D.HpPerLevel} HP";
            if ((g.D.DefLevelsMask & (1 << g.HeroLevel)) != 0) body += ", +1 obrona";
            if ((g.D.DmgLevelsMask & (1 << g.HeroLevel)) != 0) body += ", +1 obrażenia";
            _banners.Push($"Awans! Poziom {g.HeroLevel}", body);
        }
        if (g.WeaponOverride != _weapon && g.WeaponOverride >= 0)
        {
            var w = g.Weapon;
            _banners.Push("Nowe narzędzie", $"{w.Name} {w.MinDamage}-{w.MaxDamage}");
        }
        if (g.PickupsCount > _pickups) _banners.Push("Coś wypadło!", "Sprawdź miejsce usterki");
        for (var i = 0; i < g.D.GearSlotsCount && i < 4; i++)
        {
            if (g.Equipped[i] != _equipped[i] && g.Equipped[i] >= 0)
            {
                var gd = g.D.Gear[i * 3 + g.Equipped[i]];
                _banners.Push("Sprzęt: " + g.D.GearRarities[g.Equipped[i]], $"{gd.Name} +{gd.Value}");
            }
            _equipped[i] = g.Equipped[i];
        }
        if (_cd > 0 && g.AbilityCd == 0) _banners.Push("Moc gotowa", g.CDef.AbilityName);
        if (!_bossSeen && g.Boss >= 0 && g.Enemies[g.Boss].Alive && g.Visible(g.Enemies[g.Boss].X, g.Enemies[g.Boss].Y))
        {
            _bossSeen = true;
            _banners.Push("Przypisano Ci usterkę", g.D.Enemies[g.Enemies[g.Boss].DefId].Name);
        }
        _level = g.HeroLevel;
        _weapon = g.WeaponOverride;
        _pickups = g.PickupsCount;
        _cd = g.AbilityCd;
    }
}
