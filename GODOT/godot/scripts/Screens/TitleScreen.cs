using Godot;
using LifeLike.Core;
using LifeLike.Game.Audio;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;
using LifeLike.Game.Screens.Views;

namespace LifeLike.Game.Screens;

/// <summary>
/// Tytuł (run_title na GBA): menu Kontynuuj budowę (gdy jest zapis) / Nowa budowa / Codzienna budowa / Wyzwanie tygodnia / Profil /
/// Szkolenia / Jak grać,
/// rekord i doświadczenie z profilu, link planbudowlany.online, klucz ustawień (także Esc). Dotyk: przyciski menu.
/// </summary>
public sealed class TitleScreen : Screen
{
    private int _sel;
    private bool _hasRun;

    public TitleScreen(App app) : base(app)
    {
    }

    public override ViewSet Views => ViewSet.Title;
    public override string Music => "title";
    public override bool ShowsSettings => true;

    private string[] Items => _hasRun
        ? ["Kontynuuj budowę", "Nowa budowa", "Codzienna budowa", "Wyzwanie tygodnia", "Profil: odznaki, zlecenia", "Szkolenia (Koszty)", "Jak grać"]
        : ["Nowa budowa", "Codzienna budowa", "Wyzwanie tygodnia", "Profil: odznaki, zlecenia", "Szkolenia (Koszty)", "Jak grać"];

    public void Open() => Flow.Go(this);

    public override void Enter(bool instant)
    {
        _hasRun = S.HasRun;
        _sel = 0;
        Populate();
        App.Coach.Begin(this, 0, CoachHole, () => { });   // samouczek menu przy pierwszym uruchomieniu, potem dymki nowości
        var filters = ScreenFilters.Announce(S.Data, S.Profile); // v0.21.53: filtry odblokowane przed aktualizacją (profil v16)
        if (filters == 0) return;
        App.Banners.FilterUnlocks(filters);
        S.Save();
    }

    public override void Process(double delta) => App.Coach.Update();

    /// <summary>Samouczek: element tytułu omawiany w dymku (pozycja menu albo klucz ustawień).</summary>
    private Rect2 CoachHole(string id)
    {
        if (id == "options") return Hud.SettingsButton.Rect;
        var label = id switch
        {
            "phone" => "Profil",
            "training" or "respect" => "Szkolenia",
            "daily" => "Codzienna",
            "help" => "Jak grać",
            _ => "Nowa budowa",
        };
        var items = Items;
        for (var i = 0; i < items.Length; i++)
        {
            if (items[i].StartsWith(label)) return N.TitleView.ItemRect(i);
        }
        return new Rect2();
    }

    /// <summary>Rekord, liczba budów, doświadczenie, wybór i notatka na planszy tytułu (także pod telefonem profilu).</summary>
    public void Populate()
    {
        var v = N.TitleView;
        v.Items = Items;
        v.Best = S.Profile.Best;
        v.Runs = S.Profile.Runs;
        v.Xp = S.Profile.Xp;
        v.Sel = _sel;
        v.Note = S.Note;
        v.Info = $"Budowy: {S.Profile.Runs}   Doświadczenie: {S.Profile.Xp}" + ButtonNames.Pick("   P: profil   K: Szkolenia", "");
        var p = S.Profile; // v0.21.52 cz. b (#44): poziom inspektora z paskiem (od pierwszej budowy)
        Progress.InspectorBar(S.Data, p, out var cur, out var need);
        v.InspLevel = p.Runs > 0 || p.InspectorXp > 0 ? Progress.InspectorLevel(S.Data, p) : -1;
        v.InspFill = need > 0 ? cur / (float)need : 1f;
        v.InspMax = need == 0;
        if (S.RollTasks()) S.Save(); // v0.21.52 cz. c: zadania dnia (data z systemu) i seria dni pod rekordem
        var streak = DayStreak.Now(p, S.TodayNumber);
        v.Goals = p.Runs > 0 ? $"Zadania {DailyTasks.DoneToday(p)}/{DailyTasks.DailySlots}" : "";
        v.Streak = p.Runs > 0 && streak > 0 ? $"Seria {streak} {(streak == 1 ? "dzień" : "dni")}" : "";
    }

    public override bool HandleInput(InputCmd e)
    {
        if (App.Coach.HandleInput(e)) return true;
        var n = Items.Length;
        var v = e.VDir;
        if (v != 0)
        {
            _sel = (_sel + v + n) % n;
            S.Note = "";
            Sfx.Play("menu");
            Populate();
            return true;
        }
        if (e.IsTap)
        {
            if (N.TitleView.LinkAt(e.Pointer))
            {
                LifeLike.Game.Session.ExternalLinks.Open(SettingsPage.Url);
                return true;
            }
            var i = N.TitleView.ItemAt(e.Pointer);
            if (i < 0) return false;
            _sel = i;
            Populate();
            Choose();
            return true;
        }
        if (e.Is(GameAction.Cancel))
        {
            Flow.Settings.Open(this);
            return true;
        }
        if (e.Is(GameAction.Profile))
        {
            Flow.Profile.Open(0);
            return true;
        }
        if (e.Is(GameAction.Shop))
        {
            Flow.Profile.Open(4);
            return true;
        }
        if (!e.Is(GameAction.A | GameAction.Start)) return false;
        Choose();
        return true;
    }

    private void Choose()
    {
        Sfx.Play("menu");
        var k = _hasRun ? _sel - 1 : _sel;
        switch (k)
        {
            case -1:
                if (S.ResumeRun())
                {
                    App.Refresh();
                    Flow.Game.Open();
                    App.Refresh();
                }
                else
                {
                    _hasRun = false;
                    S.Note = "Zapis budowy nie pasuje do tej wersji gry";
                    Populate();
                }
                break;
            case 0:
                if (Flow.Career.Available) Flow.Career.Open(); // v0.21.52 cz. d: mapa kariery po pierwszej budowie
                else Flow.ClassSelect.Open();
                break;
            case 1:
                Flow.Daily.Open();
                break;
            case 2:
                Flow.Weekly.Open();
                break;
            case 3:
                Flow.Profile.Open(0);
                break;
            case 4:
                Flow.Profile.Open(4);
                break;
            default:
                Flow.Help.Open(true);
                break;
        }
    }
}
