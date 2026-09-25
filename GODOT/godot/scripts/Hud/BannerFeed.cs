using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Gfx;
using LifeLike.Game.Session;

namespace LifeLike.Game.Hud;

/// <summary>
/// Treść powiadomień push dla zdarzeń budowy (push_banner i push_achievements na GBA): obserwuje SessionEvents
/// i układa tytuł + treść banera; PushBanners tylko je rysuje.
/// </summary>
public sealed class BannerFeed
{
    private readonly GameSession _s;
    private readonly PushBanners _banners;

    public BannerFeed(GameSession session, PushBanners banners)
    {
        _s = session;
        _banners = banners;
        var ev = session.Events;
        ev.RunStarted += _banners.Clear;
        ev.LevelUp += OnLevelUp;
        ev.ToolFound += OnToolFound;
        ev.Dropped += () => _banners.Push("Coś wypadło!", "Sprawdź miejsce usterki");
        ev.GearEquipped += OnGearEquipped;
        ev.AbilityReady += () => _banners.Push("Moc gotowa", _s.Game.CDef.AbilityName);
        ev.BossSpotted += def => _banners.Push("Przypisano Ci usterkę", _s.Data.Enemies[def].Name);
        ev.StageCleared += OnStageCleared;
        ev.Achievements += OnAchievements;
    }

    private void OnLevelUp(int level, bool abilityUp)
    {
        var g = _s.Game;
        if (abilityUp) _banners.Push("Moc: " + UiText.AbilityLabel(g), "Silniejsza, szybciej gotowa");
        var body = $"+{g.D.HpPerLevel} HP";
        if ((g.D.DefLevelsMask & (1 << level)) != 0) body += ", +1 obrona";
        if ((g.D.DmgLevelsMask & (1 << level)) != 0) body += ", +1 obrażenia";
        _banners.Push($"Awans! Poziom {level}", body);
    }

    private void OnToolFound()
    {
        var w = _s.Game.Weapon;
        _banners.Push("Nowe narzędzie", $"{w.Name} {w.MinDamage}-{w.MaxDamage}");
    }

    private void OnGearEquipped(int slot)
    {
        var g = _s.Game;
        var gd = g.D.Gear[slot * 3 + g.Equipped[slot]];
        _banners.Push("Sprzęt: " + g.D.GearRarities[g.Equipped[slot]], $"{gd.Name} +{gd.Value}");
    }

    private void OnStageCleared()
    {
        _banners.Clear();
        _banners.Push("Etap zaliczony", _s.Data.Stages[_s.Game.Stage].Name);
    }

    /// <summary>Nowe odznaki i zlecenia (push_achievements na GBA).</summary>
    private void OnAchievements(int badges, int contracts)
    {
        var d = _s.Data;
        for (var i = 0; i < d.Badges.Length; i++)
        {
            if ((badges & (1 << i)) != 0) _banners.Push("Odznaka: " + d.Badges[i].Name, $"+{d.Badges[i].Xp} dośw.");
        }
        for (var i = 0; i < d.Contracts.Length; i++)
        {
            if ((contracts & (1 << i)) == 0) continue;
            var c = d.Contracts[i];
            _banners.Push("Zlecenie: " + c.Name, c.Keepsake >= 0 ? $"+{c.Xp}, {d.Keepsakes[c.Keepsake].Name}" : $"Wykonane! +{c.Xp} dośw.");
        }
    }

    /// <summary>Podpowiedź na start budowy: moc pod R i zabrana pamiątka z rangą (jak na GBA).</summary>
    public void FirstStageHints()
    {
        var g = _s.Game;
        var d = _s.Data;
        var p = _s.Profile;
        _banners.Push("R: " + g.CDef.AbilityName, g.CDef.AbilityDesc);
        var k = Meta.SelectedKeepsake(d, p);
        if (k < 0) return;
        var runs = p.KeepsakeRuns[k] - 1;
        var rank = 1 + (runs >= d.KeepsakeRankRuns[0] ? 1 : 0) + (runs >= d.KeepsakeRankRuns[1] ? 1 : 0);
        var kd = d.Keepsakes[k];
        _banners.Push($"{kd.Name} {UiText.Roman(rank - 1)}", RunMods.PerkLabel(new Perk(kd.Effect, kd.Values[rank - 1])));
    }
}
