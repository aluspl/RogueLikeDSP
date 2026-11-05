using Godot;
using LifeLike.Core;
using LifeLike.Game.Audio;
using LifeLike.Game.Hud;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;
using LifeLike.Game.Screens.Views;
using LifeLike.Game.Session;
using LifeLike.Game.Settings;

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

    /// <summary>v0.22.0 (UX A): powiadomienia pod statystykami – nie zasłaniają klucza ustawień.</summary>
    public override BannerSpot BannerSpot => BannerSpot.BelowTitleStats;

    public TitleScreen(App app) : base(app)
    {
    }

    public override ViewSet Views => ViewSet.Title;
    public override string Music => "title";
    public override bool ShowsSettings => true;

    /// <summary>v0.22.0 (UX A): pozycje menu (wariant A); Codzienna i Tydzień w jednym wierszu.</summary>
    private TitleItem[] Items => _hasRun
        ? [TitleItem.Continue, TitleItem.NewRun, TitleItem.Daily, TitleItem.Weekly, TitleItem.Phone, TitleItem.Help]
        : [TitleItem.NewRun, TitleItem.Daily, TitleItem.Weekly, TitleItem.Phone, TitleItem.Help];

    private static string Label(TitleItem i) => i switch
    {
        TitleItem.Continue => Loc.T("kontynuuj_budowe"),
        TitleItem.NewRun => Loc.T("nowa_budowa"),
        TitleItem.Daily => Loc.T("codzienna"),
        TitleItem.Weekly => Loc.T("tydzien"),
        TitleItem.Phone => Loc.T("telefon_kierownika"),
        _ => Loc.T("jak_grac"),
    };

    /// <summary>Wiersz pozycji: start (Kontynuuj, Nowa budowa), Dziennie (dwie obok siebie), Postęp.</summary>
    private int Row(int index)
    {
        var items = Items;
        var row = 0;
        for (var i = 1; i <= index; i++)
        {
            if (items[i] != TitleItem.Weekly) row++;
        }
        return row;
    }

    private static string Group(TitleItem i) => i switch
    {
        TitleItem.Daily => Loc.T("dziennie"),
        TitleItem.Phone => Loc.T("postep_grupa"),
        _ => "",
    };

    /// <summary>Indeks pozycji (test dymny, samouczek); -1 = brak.</summary>
    public int IndexOf(TitleItem item) => System.Array.IndexOf(Items, item);

    /// <summary>Zaznaczona pozycja (test dymny).</summary>
    public TitleItem Selected => Items[_sel];

    public void Open() => Flow.Go(this);

    public override void Enter(bool instant)
    {
        if (GameSettings.Persist && GodotDataSource.DataEnglish != Loc.English) // v0.21.53 cz. 2 (#40): dane w nowym języku
        {
            ((SceneTree)Engine.GetMainLoop()).CallDeferred(SceneTree.MethodName.ReloadCurrentScene);
            return;
        }
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
        var item = id switch
        {
            "phone" or "training" or "respect" => TitleItem.Phone, // Szkolenia i Respekt są w Telefonie kierownika
            "daily" => TitleItem.Daily,
            "help" => TitleItem.Help,
            _ => TitleItem.NewRun,
        };
        return N.TitleView.ItemRect(IndexOf(item));
    }

    /// <summary>Rekord, liczba budów, doświadczenie, wybór i notatka na planszy tytułu (także pod telefonem profilu).</summary>
    public void Populate()
    {
        var v = N.TitleView;
        var items = Items;
        v.Items = System.Array.ConvertAll(items, Label);
        v.ItemRows = new int[items.Length];
        var groups = new System.Collections.Generic.List<string>();
        for (var i = 0; i < items.Length; i++)
        {
            v.ItemRows[i] = Row(i);
            if (v.ItemRows[i] == groups.Count) groups.Add(Group(items[i]));
        }
        v.RowGroups = groups.ToArray();
        v.Best = S.Profile.Best;
        v.Runs = S.Profile.Runs;
        v.Xp = S.Profile.Xp;
        v.Sel = _sel;
        v.Note = S.Note;
        // v0.22.0 (UX A): karta „Twoje postępy” z przyciskiem do Szkoleń (aktywny, gdy stać na najbliższy zakup)
        var cost = Meta.NextUnlock(S.Data, S.Profile, out _, out _);
        v.CardOn = S.Profile.Runs > 0;
        v.CardLine = Loc.F("postepy_linia", S.Profile.Xp, S.Profile.Runs);
        v.CardActive = cost >= 0 && S.Profile.Xp >= cost;
        v.CardButton = v.CardActive || cost < 0 ? Loc.T("wydaj_doswiadczenie") : $"{Loc.T("wydaj_doswiadczenie")} · {Loc.F("brakuje_dosw", cost - S.Profile.Xp)}";
        v.Info = "";
        var p = S.Profile; // v0.21.52 cz. b (#44): poziom inspektora z paskiem (od pierwszej budowy)
        Progress.InspectorBar(S.Data, p, out var cur, out var need);
        v.InspLevel = p.Runs > 0 || p.InspectorXp > 0 ? Progress.InspectorLevel(S.Data, p) : -1;
        v.InspFill = need > 0 ? cur / (float)need : 1f;
        v.InspMax = need == 0;
        if (S.RollTasks()) S.Save(); // v0.21.52 cz. c: zadania dnia (data z systemu) i seria dni pod rekordem
        var streak = DayStreak.Now(p, S.TodayNumber);
        v.Goals = p.Runs > 0 ? Loc.F("zadania_4", DailyTasks.DoneToday(p), DailyTasks.DailySlots) : "";
        v.Streak = p.Runs > 0 && streak > 0 ? Loc.F("seria_2", streak, (streak == 1 ? Loc.T("dzien_3") : Loc.T("dni_5"))) : "";
    }

    public override bool HandleInput(InputCmd e)
    {
        if (App.Coach.HandleInput(e)) return true;
        var v = e.VDir;
        var hd = e.HDir;
        if (v != 0 || hd != 0)
        {
            _sel = Move(v, hd);
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
            if (N.TitleView.CardAt(e.Pointer))
            {
                Flow.Profile.Open(4); // „Wydaj doświadczenie” – Szkolenia
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

    /// <summary>↑/↓ po wierszach (w wierszu dwóch pozycji ta sama kolumna), ←/→ w wierszu.</summary>
    private int Move(int v, int h)
    {
        var n = Items.Length;
        var row = Row(_sel);
        var rows = Row(n - 1) + 1;
        int First(int r)
        {
            for (var i = 0; i < n; i++)
            {
                if (Row(i) == r) return i;
            }
            return 0;
        }
        var col = _sel - First(row);
        if (v != 0)
        {
            var nr = (row + v + rows) % rows;
            var f = First(nr);
            var last = f;
            while (last + 1 < n && Row(last + 1) == nr) last++;
            return System.Math.Min(f + col, last);
        }
        var j = _sel + h;
        return j >= 0 && j < n && Row(j) == row ? j : _sel;
    }

    private void Choose()
    {
        Sfx.Play("menu");
        switch (Items[_sel])
        {
            case TitleItem.Continue:
                if (S.ResumeRun())
                {
                    App.Refresh();
                    Flow.Game.Open();
                    App.Refresh();
                }
                else
                {
                    _hasRun = false;
                    _sel = 0;
                    S.Note = Loc.T("zapis_budowy_nie_pasuje_do_tej");
                    Populate();
                }
                break;
            case TitleItem.NewRun:
                if (Flow.Career.Available) Flow.Career.Open(); // v0.21.52 cz. d: mapa kariery po pierwszej budowie
                else Flow.ClassSelect.Open();
                break;
            case TitleItem.Daily:
                Flow.Daily.Open();
                break;
            case TitleItem.Weekly:
                Flow.Weekly.Open();
                break;
            case TitleItem.Phone:
                Flow.Profile.Open(0);
                break;
            default:
                Flow.Help.Open(true);
                break;
        }
    }
}
