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
        ev.Dropped += () => _banners.Push(Loc.T("cos_wypadlo"), Loc.T("sprawdz_miejsce_usterki"), PhoneTabs.Issues);
        ev.GearEquipped += OnGearEquipped;
        ev.AbilityReady += () => _banners.Push(Loc.T("moc_gotowa_3"), _s.Game.PDef.AbilityName, PhoneTabs.Start);
        ev.BossSpotted += def => _banners.Push(Loc.T("przypisano_ci_usterke"), _s.Data.Enemies[def].Name, PhoneTabs.Issues);
        ev.StageCleared += OnStageCleared;
        ev.Achievements += OnAchievements;
        ev.Goals += (tasks, coll, before) => // v0.21.52 cz. c: zadania dnia / tygodnia, komplety kolekcji
        {
            foreach (var (title, body) in GoalBanners.Of(_s.Data, _s.Profile, tasks, coll, before))
                _banners.Push(new PushBanner { Title = title, Body = body, Gold = true });
        };
        ev.RespectGained += (gained, total) => _banners.Push(Loc.F("respekt_5", gained), Loc.F("razem_koszty", total), PhoneTabs.Costs);
        ev.RewardUnlocked += i => _banners.Push(Loc.T("nagroda") + _s.Data.Rewards[i].Name, _s.Data.Rewards[i].Desc, PhoneTabs.Start);
        ev.SecondChance += () => _banners.Push(Loc.T("druga_szansa"), Loc.T("zostaje_1_hp_uwazaj"), PhoneTabs.Start);
        ev.DocumentFound += (doc, complete) => _banners.Push(Loc.T("dokument") + _s.Data.Documents[doc],
            complete ? Loc.T("komplet_schody_otwarte") : Loc.F("pieczatki_2", _s.Game.DocsCount(), _s.Game.DocsNeeded()), PhoneTabs.Tasks);
        ev.BossPhase += def =>
        {
            var bd = _s.Data.Enemies[def];
            var b = _s.Game.Enemies[_s.Game.Boss];
            _banners.Push(Loc.F("druga_faza_2", bd.PhaseName), $"{bd.Name} +{b.MaxHp * bd.PhaseHeal / 100} HP", PhoneTabs.Issues);
        };
    }

    /// <summary>v0.21.53: odblokowane filtry ekranu (bity GameData.ScreenFilters) – złoty baner na każdy.</summary>
    public void FilterUnlocks(int bits)
    {
        for (var f = 0; f < _s.Data.ScreenFilters.Length; f++)
        {
            if (((bits >> f) & 1) != 0)
                _banners.Push(new PushBanner { Title = _s.Data.FilterText("banner"), Body = $"{_s.Data.ScreenFilters[f].Name} – {_s.Data.FilterText("bannerWhere")}", Gold = true });
        }
    }

    private void OnLevelUp(int level, bool abilityUp)
    {
        var g = _s.Game;
        if (abilityUp) _banners.Push(Loc.T("moc_4") + UiText.AbilityLabel(g), Loc.T("silniejsza_szybciej_gotowa"), PhoneTabs.Start);
        var body = $"+{g.D.HpPerLevel} HP";
        if ((g.D.DefLevelsMask & (1 << level)) != 0) body += Loc.T("n1_obrona");
        if ((g.D.DmgLevelsMask & (1 << level)) != 0) body += Loc.T("n1_obrazenia");
        _banners.Push(Loc.F("awans_poziom_3", level), body, PhoneTabs.Start);
    }

    /// <summary>Nowe narzędzie: porównanie ciosu z poprzednią bronią (rozpiska #26), np. „6-8 -&gt; 9-13 (śr. +3)”.</summary>
    private void OnToolFound(int previous)
    {
        var g = _s.Game;
        var was = g.WeaponBreakdown(-1, previous >= 0 ? previous : g.CDef.Weapon);
        var now = g.WeaponBreakdown();
        var m = DamageHelp.AddRange(new Message(), was.Min, was.Max).Add(" -> ");
        DamageHelp.AddRange(m, now.Min, now.Max).Add(Loc.T("sr"));
        DamageHelp.AddTenths(m, now.Avg10 - was.Avg10, true).Add(")");
        _banners.Push(Loc.T("nowe") + g.Weapon.Name, m.Text, PhoneTabs.Gear);
    }

    private void OnGearEquipped(int slot)
    {
        var g = _s.Game;
        var gd = g.D.Gear[slot * 3 + g.Equipped[slot]];
        _banners.Push(Loc.T("sprzet_3") + g.D.GearRarities[g.Equipped[slot]], $"{gd.Name} +{gd.Value}", PhoneTabs.Gear);
    }

    private void OnStageCleared()
    {
        _banners.Clear();
        _banners.Push(Loc.T("etap_zaliczony"), _s.Game.SDef().Name, PhoneTabs.Tasks);
    }

    /// <summary>Sekretne zlecenia, nowe odznaki i zlecenia (push_achievements na GBA).</summary>
    private void OnAchievements(int badges, int contracts, int secrets)
    {
        var d = _s.Data;
        for (var i = 0; i < d.Secrets.Length; i++)
        {
            if ((secrets & (1 << i)) != 0) _banners.Push(new PushBanner { Title = Loc.T("sekretne_zlecenie"), Body = d.Secrets[i].RewardText, Icon = Assets.MenuSecret, Gold = true });
        }
        for (var i = 0; i < d.Badges.Length; i++)
        {
            if ((badges & (1 << i)) != 0) _banners.Push(Loc.T("odznaka") + d.Badges[i].Name, Loc.F("tytul_dosw", d.Badges[i].Title, d.Badges[i].Xp), PhoneTabs.Start);
        }
        for (var i = 0; i < d.Contracts.Length; i++)
        {
            if ((contracts & (1 << i)) == 0) continue;
            var c = d.Contracts[i];
            _banners.Push(Loc.T("zlecenie") + c.Name, c.Keepsake >= 0 ? Loc.F("tytul_4", d.Keepsakes[c.Keepsake].Name, c.Title) : Loc.F("tytul_dosw", c.Title, c.Xp), PhoneTabs.Start);
        }
    }

    /// <summary>Pierwszy etap aktu: baner z mechaniką aktu (błoto, porywy, pył) jak na GBA.</summary>
    public void ActHint()
    {
        var g = _s.Game;
        var d = _s.Data;
        var act = g.SDef().Act;
        if (g.TwinCarry > 0) _banners.Push(Loc.T("wspolna_sciana_2"), Loc.F("problemy_z_1_polowy_ta_sama", g.TwinCarry), PhoneTabs.Tasks); // v0.21.52 cz. d
        if (g.ADef.Mechanic == ActMechanic.None || (g.Stage > g.FirstStage && g.SDef(g.Stage - 1).Act == act)) return;
        var info = g.KDef.Gust > 0 && g.ADef.Mechanic == ActMechanic.Gust ? Loc.F("poryw_co_tur_na_poddaszu", g.MechValue()) : g.ADef.MechInfo;
        _banners.Push(Loc.F("akt_3", g.ActNumeral(), g.ADef.MechShort), info, PhoneTabs.Tasks);
    }

    /// <summary>Podpowiedź na start budowy: moc pod R i zabrana pamiątka z rangą (jak na GBA).</summary>
    public void FirstStageHints()
    {
        var g = _s.Game;
        var d = _s.Data;
        var p = _s.Profile;
        _banners.Push("R: " + g.PDef.AbilityName, g.PDef.AbilityDesc, PhoneTabs.Start); // Majster: moc pożyczona na etap
        var k = Meta.SelectedKeepsake(d, p);
        if (k < 0) return;
        var runs = p.KeepsakeRuns[k] - 1;
        var rank = 1 + (runs >= d.KeepsakeRankRuns[0] ? 1 : 0) + (runs >= d.KeepsakeRankRuns[1] ? 1 : 0);
        var kd = d.Keepsakes[k];
        _banners.Push($"{kd.Name} {UiText.Roman(rank - 1)}", RunMods.PerkLabel(new Perk(kd.Effect, kd.Values[rank - 1])), PhoneTabs.Start);
    }
}
