using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Gfx;
using LifeLike.Game.Phone;
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
        ev.Dropped += () => _banners.Push("Coś wypadło!", "Sprawdź miejsce usterki", PhoneTabs.Issues);
        ev.GearEquipped += OnGearEquipped;
        ev.AbilityReady += () => _banners.Push("Moc gotowa", _s.Game.CDef.AbilityName, PhoneTabs.Start);
        ev.BossSpotted += def => _banners.Push("Przypisano Ci usterkę", _s.Data.Enemies[def].Name, PhoneTabs.Issues);
        ev.StageCleared += OnStageCleared;
        ev.Achievements += OnAchievements;
        ev.RespectGained += (gained, total) => _banners.Push($"Respekt +{gained}", $"Razem: {total} (Koszty)", PhoneTabs.Costs);
        ev.RewardUnlocked += i => _banners.Push("Nagroda: " + _s.Data.Rewards[i].Name, _s.Data.Rewards[i].Desc, PhoneTabs.Start);
        ev.SecondChance += () => _banners.Push("Druga szansa!", "Zostaje 1 HP - uważaj", PhoneTabs.Start);
        ev.DocumentFound += (doc, complete) => _banners.Push("Dokument: " + _s.Data.Documents[doc],
            complete ? "Komplet! Schody otwarte" : $"Pieczątki {_s.Game.DocsCount()}/{_s.Game.DocsNeeded()}", PhoneTabs.Tasks);
        ev.BossPhase += def =>
        {
            var bd = _s.Data.Enemies[def];
            var b = _s.Game.Enemies[_s.Game.Boss];
            _banners.Push($"Druga faza: {bd.PhaseName}!", $"{bd.Name} +{b.MaxHp * bd.PhaseHeal / 100} HP", PhoneTabs.Issues);
        };
    }

    private void OnLevelUp(int level, bool abilityUp)
    {
        var g = _s.Game;
        if (abilityUp) _banners.Push("Moc: " + UiText.AbilityLabel(g), "Silniejsza, szybciej gotowa", PhoneTabs.Start);
        var body = $"+{g.D.HpPerLevel} HP";
        if ((g.D.DefLevelsMask & (1 << level)) != 0) body += ", +1 obrona";
        if ((g.D.DmgLevelsMask & (1 << level)) != 0) body += ", +1 obrażenia";
        _banners.Push($"Awans! Poziom {level}", body, PhoneTabs.Start);
    }

    /// <summary>Nowe narzędzie: porównanie ciosu z poprzednią bronią (rozpiska #26), np. „6-8 -&gt; 9-13 (śr. +3)”.</summary>
    private void OnToolFound(int previous)
    {
        var g = _s.Game;
        var was = g.WeaponBreakdown(-1, previous >= 0 ? previous : g.CDef.Weapon);
        var now = g.WeaponBreakdown();
        var m = DamageHelp.AddRange(new Message(), was.Min, was.Max).Add(" -> ");
        DamageHelp.AddRange(m, now.Min, now.Max).Add(" (śr. ");
        DamageHelp.AddTenths(m, now.Avg10 - was.Avg10, true).Add(")");
        _banners.Push("Nowe: " + g.Weapon.Name, m.Text, PhoneTabs.Gear);
    }

    private void OnGearEquipped(int slot)
    {
        var g = _s.Game;
        var gd = g.D.Gear[slot * 3 + g.Equipped[slot]];
        _banners.Push("Sprzęt: " + g.D.GearRarities[g.Equipped[slot]], $"{gd.Name} +{gd.Value}", PhoneTabs.Gear);
    }

    private void OnStageCleared()
    {
        _banners.Clear();
        _banners.Push("Etap zaliczony", _s.Data.Stages[_s.Game.Stage].Name, PhoneTabs.Tasks);
    }

    /// <summary>Nowe odznaki i zlecenia (push_achievements na GBA).</summary>
    private void OnAchievements(int badges, int contracts)
    {
        var d = _s.Data;
        for (var i = 0; i < d.Badges.Length; i++)
        {
            if ((badges & (1 << i)) != 0) _banners.Push("Odznaka: " + d.Badges[i].Name, $"+{d.Badges[i].Xp} dośw.", PhoneTabs.Start);
        }
        for (var i = 0; i < d.Contracts.Length; i++)
        {
            if ((contracts & (1 << i)) == 0) continue;
            var c = d.Contracts[i];
            _banners.Push("Zlecenie: " + c.Name, c.Keepsake >= 0 ? $"+{c.Xp}, {d.Keepsakes[c.Keepsake].Name}" : $"Wykonane! +{c.Xp} dośw.", PhoneTabs.Start);
        }
    }

    /// <summary>Pierwszy etap aktu: baner z mechaniką aktu (błoto, porywy, pył) jak na GBA.</summary>
    public void ActHint()
    {
        var g = _s.Game;
        var d = _s.Data;
        var act = d.Stages[g.Stage].Act;
        if (g.ADef.Mechanic == ActMechanic.None || (g.Stage > g.FirstStage && d.Stages[g.Stage - 1].Act == act)) return;
        _banners.Push($"Akt {g.ActNumeral()}: {g.ADef.MechShort}", g.ADef.MechInfo, PhoneTabs.Tasks);
    }

    /// <summary>Podpowiedź na start budowy: moc pod R i zabrana pamiątka z rangą (jak na GBA).</summary>
    public void FirstStageHints()
    {
        var g = _s.Game;
        var d = _s.Data;
        var p = _s.Profile;
        _banners.Push("R: " + g.CDef.AbilityName, g.CDef.AbilityDesc, PhoneTabs.Start);
        var k = Meta.SelectedKeepsake(d, p);
        if (k < 0) return;
        var runs = p.KeepsakeRuns[k] - 1;
        var rank = 1 + (runs >= d.KeepsakeRankRuns[0] ? 1 : 0) + (runs >= d.KeepsakeRankRuns[1] ? 1 : 0);
        var kd = d.Keepsakes[k];
        _banners.Push($"{kd.Name} {UiText.Roman(rank - 1)}", RunMods.PerkLabel(new Perk(kd.Effect, kd.Values[rank - 1])), PhoneTabs.Start);
    }
}
