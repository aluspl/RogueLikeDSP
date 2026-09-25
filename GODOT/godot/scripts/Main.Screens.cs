using LifeLike.Core;
using LifeLike.Core.Data;

namespace LifeLike.Game;

/// <summary>Ekrany i telefon: tytuł, wybór zawodu, profil, telefon w grze, harmonogram, karta etapu, Hurtownia, koniec.</summary>
public partial class Main
{
    private static readonly string[] GameTabLabels = ["Zadania", "Usterki", "Start", "Sprzęt", "Koszty"];
    private static readonly string[] ProfileTabLabels = ["Odznaki", "Katalog", "Osiedle", "Zespół", "Koszty"];

    private void ShowTitle()
    {
        _title.Best = _profile.Best;
        _title.Runs = _profile.Runs;
        _title.Xp = _profile.Xp;
        _title.Sel = _titleSel;
        _title.Note = _note;
        ApplyScreen(Screen.Title);
    }

    private void ShowClassSelect()
    {
        _note = "";
        _classSelect.Setup(_d, _profile, _cls);
        _classSelect.Difficulty = _diff;
        _classSelect.Note = "";
        ApplyScreen(Screen.ClassSelect);
    }

    /// <summary>Telefon profilu (run_shop na GBA): Odznaki/Zlecenia/Pamiątki, Katalog, Osiedle, Zespół, Koszty = Szkolenia.</summary>
    private void ShowProfile(int tab = 0, bool instant = false)
    {
        _note = "";
        ShowTitle();
        _screen = Screen.Profile;
        _backdrop.SetOn(true, instant);
        PhonePage[] tabs =
        [
            new BadgesTab(_d, _profile),
            new CatalogTab(_d, _profile),
            new EstateTab(_d, _profile),
            new TeamTab(_d, _profile),
            new TrainingTab(_d, _profile, SaveProfile),
        ];
        _phone.OpenTabs(tabs, ProfileTabLabels, tab, instant);
    }

    /// <summary>Telefon w trakcie budowy (SELECT na GBA): Zadania, Usterki, Start, Sprzęt, Koszty. Poza czasem gry.</summary>
    private void ShowPhone(int tab = -1, bool instant = false)
    {
        CloseMenu();
        _banners.Clear(); // telefon zasłania mapę - banery znikają jak na GBA (banner.hide)
        ApplyScreen(Screen.Phone);
        _backdrop.SetOn(true, instant);
        PhonePage[] tabs = [new TasksTab(_g), new IssuesTab(_g), new HomeTab(_g, _profile), new GearTab(_g), new CostsTab(_g, _profile)];
        _phone.OnTabChanged = i => _phoneTab = i;
        _phone.OpenTabs(tabs, GameTabLabels, tab >= 0 ? tab : _phoneTab, instant);
    }

    private void ClosePhone()
    {
        _phone.OnTabChanged = null;
        ApplyScreen(Screen.Playing);
        Refresh();
    }

    /// <summary>Karta etapu (stage_card na GBA): SMS od inwestorki / kierownika i drugi SMS z wydarzeniem na placu.</summary>
    private void ShowStageCard(bool instant = false)
    {
        ApplyScreen(Screen.StageCard);
        _backdrop.SetOn(true, instant);
        var sd = _d.Stages[_g.Stage];
        var page = new MessagePage($"Akt {UiText.Roman(sd.Act)}, {_g.Stage + 1}/{_d.Stages.Length}", "Enter: do roboty");
        page.Add(_g.StageStory);
        var ev = _g.CurrentEvent;
        if (ev is not null && _g.Turns == _g.StageStartTurn) page.Add(ev.Msg);
        if (sd.Boss >= 0) page.Info($"Uwaga: {_d.Enemies[sd.Boss].Name}!", Ink.Late);
        else page.Info($"{sd.Name}: problemy {_g.EnemyHpPct()}%", Ink.Dim);
        if (ev is not null) page.Info($"{ev.Name}: {ev.Info}", ev.Good ? Ink.Done : Ink.Late);
        page.Info(_g.DDef.Name + (_g.Tier > 0 ? $" NG+{_g.Tier}" : ""), Ink.Dim);
        _phone.OpenSingle(page, 2, instant);
        Sfx.Play("notify", 0.7f);
    }

    private void ShowStageClear(bool instant = false)
    {
        ApplyScreen(Screen.StageClear);
        _backdrop.SetOn(true, instant);
        _phone.OpenSingle(new SchedulePage(_g, _note), 0, instant);
    }

    private void ShowHurtownia(bool instant = false)
    {
        ApplyScreen(Screen.Hurtownia);
        _backdrop.SetOn(true, instant);
        _hurtownia = new HurtowniaPage(_g);
        _phone.OpenSingle(_hurtownia, 4, instant);
        Sfx.Play("notify", 0.7f);
    }

    /// <summary>Paczka sprzętu przy zajętym slocie: porównanie obecny / nowy z cechami (jak telefon na GBA).</summary>
    private void ShowOffer(bool instant = false)
    {
        ApplyScreen(Screen.GearOffer);
        _backdrop.SetOn(true, instant);
        _phone.OpenSingle(new OfferPage(_g), 3, instant);
        Sfx.Play("notify", 0.7f);
    }

    private void CloseOffer(bool accept)
    {
        if (accept)
        {
            _g.AcceptOffer();
            Sfx.Play("buy");
        }
        else
        {
            _g.DeclineOffer();
            Sfx.Play("menu");
        }
        ApplyScreen(Screen.Playing);
        AfterAction(true); // decyzja nie zużywa tury, ale mogła dać awans (doświadczenie)
    }

    /// <summary>Koniec budowy: SMS z odbioru / przerwania (phone_message na GBA), potem plansza końcowa.</summary>
    private void ShowEndMessage(bool instant = false)
    {
        var won = _g.St == GameStatus.Won;
        ApplyScreen(Screen.EndMessage);
        _backdrop.SetOn(true, instant);
        var page = new MessagePage(won ? "Odbiór" : "Budowa");
        page.Add(won ? _d.StoryWin : _d.StoryLose);
        page.Info($"Wynik {_g.Score}  Dośw. +{_lastGained}", Ink.Dark);
        page.Info(won ? "Dom na Osiedlu!" : "Doświadczenie zostaje", won ? Ink.Done : Ink.Dim);
        _phone.OpenSingle(page, 2, instant);
    }

    private void ShowEnd()
    {
        var won = _g.St == GameStatus.Won;
        _end.Won = won;
        _end.Line1 = $"Wynik {_g.Score}   Dni {_g.Turns}   Etap {_g.Stage + 1}/{_d.Stages.Length}   Dośw. +{_lastGained}";
        _end.Line2 = $"Rekord {_profile.Best}   Doświadczenie w profilu {_profile.Xp}" + (won ? "   Dom na Osiedlu!" : "");
        _end.Note = _note;
        _banners.Clear();
        ApplyScreen(Screen.End);
    }

    // ------------------------------------------------------------------ menu akcji (Enter / START)
    /// <summary>Menu akcji wokół bohatera: góra Atak, prawo Moc, dół Termos, lewo Czekaj (jak GBA v0.21.42).</summary>
    private void OpenMenu()
    {
        _screen = Screen.ActionMenu;
        _view.MenuSel = -1;
        MenuHint();
        Refresh();
    }

    private void CloseMenu()
    {
        _view.MenuSel = -2;
        _hud.ShowHint(null);
        _hud.SilenceLog();
        if (_screen == Screen.ActionMenu) _screen = Screen.Playing;
    }

    private void MenuHint()
    {
        var label = _view.MenuSel switch
        {
            0 => $"Atak: najbliższy cel (z{_g.Weapon.Range})",
            1 => $"Moc: {UiText.AbilityLabel(_g)}" + (_g.AbilityCd > 0 ? $" - za {_g.AbilityCd} t." : ""),
            2 => $"Termos {_g.Thermos}/{_g.ThermosCap()}: kawa +{_g.CoffeeHeal()} HP (zużywa turę)",
            3 => "Czekaj turę",
            _ => "Akcje: wybierz strzałką (góra Atak, prawo Moc, dół Termos, lewo Czekaj)",
        };
        _hud.ShowHint(label, _view.MenuSel < 0 ? "Enter/Z: zamknij" : "Ta sama strzałka lub Spacja: wykonaj   Enter/Z: zamknij");
    }

    /// <summary>Strzałka w menu: pierwszy raz wybiera, drugi raz tą samą - wykonuje.</summary>
    private void MenuPick(int k)
    {
        if (_view.MenuSel != k)
        {
            _view.MenuSel = k;
            MenuHint();
            Sfx.Play("menu");
            return;
        }
        MenuExecute();
    }

    private void MenuExecute()
    {
        var sel = _view.MenuSel;
        CloseMenu();
        var acted = false;
        switch (sel)
        {
            case 0:
                acted = AttackNearest();
                if (!acted) _view.FlashRange();
                break;
            case 1:
                acted = UseAbility();
                break;
            case 2:
                acted = _g.PlayerDrink();
                if (acted) Sfx.Play("pickup");
                break;
            case 3:
                acted = _g.PlayerWait();
                break;
        }
        AfterAction(acted);
    }

    /// <summary>Moc zawodu z dźwiękiem i efektami (ability_fx na GBA) - przed odświeżeniem, póki są trafienia tury.</summary>
    private bool UseAbility()
    {
        var acted = _g.PlayerAbility();
        if (acted)
        {
            Sfx.Play("ability");
            _view.AbilityFx();
        }
        return acted;
    }
}
