using Godot;
using LifeLike.Game.Audio;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;
using LifeLike.Game.Screens.Views;

namespace LifeLike.Game.Screens;

/// <summary>
/// Tytuł (run_title na GBA): menu Kontynuuj budowę (gdy jest zapis) / Nowa budowa / Profil / Szkolenia / Jak grać,
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
        ? ["Kontynuuj budowę", "Nowa budowa", "Profil: odznaki, zlecenia", "Szkolenia (Koszty)", "Jak grać"]
        : ["Nowa budowa", "Profil: odznaki, zlecenia", "Szkolenia (Koszty)", "Jak grać"];

    public void Open() => Flow.Go(this);

    public override void Enter(bool instant)
    {
        _hasRun = S.HasRun;
        _sel = 0;
        Populate();
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
    }

    public override bool HandleInput(InputCmd e)
    {
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
                OS.ShellOpen(SettingsPage.Url);
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
                Flow.ClassSelect.Open();
                break;
            case 1:
                Flow.Profile.Open(0);
                break;
            case 2:
                Flow.Profile.Open(4);
                break;
            default:
                Flow.Help.Open(true);
                break;
        }
    }
}
