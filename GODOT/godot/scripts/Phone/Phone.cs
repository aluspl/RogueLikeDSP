using System;
using Godot;

namespace LifeLike.Game;

/// <summary>
/// Smartfon bohatera z aplikacją PlanBudowlany (KONCEPCJA, sekcja 3): pionowo na środku ekranu, wysuwa się z dołu.
/// Ramka, pasek statusu 09:41, nagłówek z tytułem i podtytułem, treść strony (PhonePage) i dolny pasek zakładek
/// z ikonami (aktywna w fiolecie marki). Zakładki przełączają Q/E (LB/RB) i strzałki w lewo/prawo, jak L/R na GBA.
/// Akcje w telefonie są poza czasem gry.
/// </summary>
public partial class Phone : Control
{
    public const int W = 256, H = 348;
    private const int Bezel = 5, StatusH = 14, HeaderH = 24, TabH = 36;

    private PhonePage[] _tabs = [];
    private string[] _labels = [];
    private PhonePage _single;
    private int _singleIcon = -1;
    private int _tab;
    private Tween _slide;

    public bool IsOpen { get; private set; }
    public int TabIndex => _tab;
    public PhonePage Current => _single ?? (_tabs.Length > 0 ? _tabs[_tab] : null);
    public bool HasTabs => _single is null && _tabs.Length > 1;

    /// <summary>Wywoływane po zmianie zakładki (np. zapamiętanie ostatniej, jak phone_tab na GBA).</summary>
    public Action<int> OnTabChanged;

    public static Vector2 Origin => new((640 - W) / 2, (360 - H) / 2);

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        Size = new Vector2(W, H);
        Position = Origin + new Vector2(0, 400);
        Visible = false;
    }

    /// <summary>Telefon z zakładkami (w grze: Zadania, Usterki, Start, Sprzęt, Koszty; na tytule: profil).</summary>
    public void OpenTabs(PhonePage[] tabs, string[] labels, int index, bool instant = false)
    {
        _tabs = tabs;
        _labels = labels;
        _single = null;
        _tab = Mathf.Clamp(index, 0, tabs.Length - 1);
        foreach (var t in tabs) t.Owner = this;
        Current?.Enter();
        Present(instant);
    }

    /// <summary>Pojedyncza strona (wiadomość, paczka, Hurtownia, harmonogram); icon = podświetlona ikona paska (-1 brak).</summary>
    public void OpenSingle(PhonePage page, int icon, bool instant = false)
    {
        _single = page;
        _singleIcon = icon;
        page.Owner = this;
        page.Enter();
        Present(instant);
    }

    private void Present(bool instant)
    {
        var wasOpen = IsOpen;
        IsOpen = true;
        Visible = true;
        _slide?.Kill();
        if (instant || wasOpen)
        {
            Position = Origin;
        }
        else
        {
            Position = Origin + new Vector2(0, 380);
            _slide = CreateTween();
            _slide.TweenProperty(this, "position", Origin, 0.24f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        }
        QueueRedraw();
    }

    public void Close(bool instant = false)
    {
        if (!IsOpen) return;
        IsOpen = false;
        _slide?.Kill();
        if (instant)
        {
            Visible = false;
            return;
        }
        _slide = CreateTween();
        _slide.TweenProperty(this, "position", Origin + new Vector2(0, 380), 0.16f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        _slide.TweenCallback(Callable.From(() => Visible = IsOpen));
    }

    public void SwitchTab(int d)
    {
        if (!HasTabs) return;
        _tab = (_tab + d + _tabs.Length) % _tabs.Length;
        Current.Enter();
        OnTabChanged?.Invoke(_tab);
        Sfx.Play("menu");
        QueueRedraw();
    }

    /// <summary>Wejście: najpierw strona, potem przełączanie zakładek. true = obsłużone.</summary>
    public bool HandleInput(InputEvent e)
    {
        if (!IsOpen || Current is null) return false;
        if (Current.Input(e))
        {
            QueueRedraw();
            return true;
        }
        if (!HasTabs) return false;
        var d = e.IsActionPressed(GameInput.TabNext) || e.IsActionPressed(GameInput.Right) ? 1
              : e.IsActionPressed(GameInput.TabPrev) || e.IsActionPressed(GameInput.Left) ? -1 : 0;
        if (d == 0) return false;
        SwitchTab(d);
        return true;
    }

    public override void _Draw()
    {
        try
        {
            DrawContent();
        }
        catch (System.Exception ex)
        {
            DrawErrors.Record("Phone", ex);
        }
    }

    private void DrawContent()
    {
        var page = Current;
        if (page is null) return;
        var f = PixelFont.I;
        var outer = new Rect2(0, 0, W, H);
        DrawStyleBox(Ui.Box(new Color(0, 0, 0, 0.4f), 20), new Rect2(4, 6, W, H));    // cień telefonu
        DrawStyleBox(Ui.Box(Pal.Text, 20, new Color("3a3550")), outer);                 // obudowa
        var scr = new Rect2(Bezel, Bezel, W - 2 * Bezel, H - 2 * Bezel);
        DrawStyleBox(Ui.Box(Pal.Bg, 15), scr);

        // pasek statusu: 09:41, zasięg, bateria, wyspa z kamerą
        var sx = scr.Position.X;
        var sy = scr.Position.Y;
        f.Draw(this, new Vector2(sx + 12, sy), "09:41", Ink.Dark);
        DrawStyleBox(Ui.Box(Pal.Text, 5), new Rect2(W / 2 - 22, sy + 3, 44, 9));
        var bx = scr.End.X - 14;
        DrawRect(new Rect2(bx - 14, sy + 4, 14, 8), Pal.Text, false, 1);
        DrawRect(new Rect2(bx - 12, sy + 6, 9, 4), Pal.Done);
        DrawRect(new Rect2(bx, sy + 6, 2, 4), Pal.Text);
        for (var k = 0; k < 4; k++) DrawRect(new Rect2(bx - 34 + k * 4, sy + 11 - k * 2 - 2, 3, k * 2 + 2), Pal.Text);

        // nagłówek
        var hy = sy + StatusH + 2;
        var title = f.Fit(page.Title, 130);
        f.Draw(this, new Vector2(sx + 10, hy), title, Ink.Dark, TextAlign.Left, 1, true);
        var sub = page.Sub;
        if (sub.Length > 0)
        {
            var room = (int)(scr.Size.X - 24 - f.Measure(title, 1, true));
            f.Draw(this, new Vector2(scr.End.X - 10, hy), f.Fit(sub, room), Ink.Dim, TextAlign.Right);
        }

        // treść
        var tabTop = scr.End.Y - TabH;
        var hint = page.Hint;
        var contentBottom = tabTop - (hint.Length > 0 ? 18 : 4);
        var content = new Rect2(sx + 2, sy + StatusH + HeaderH, scr.Size.X - 4, contentBottom - (sy + StatusH + HeaderH));
        page.Draw(new PhonePainter(this, content));
        if (hint.Length > 0) f.Draw(this, new Vector2(scr.Position.X + scr.Size.X / 2, contentBottom), f.Fit(hint, (int)scr.Size.X - 12), Ink.Dim, TextAlign.Center);

        // pasek zakładek
        DrawRect(new Rect2(sx, tabTop, scr.Size.X, 1), Pal.Border);
        DrawStyleBox(Ui.Box(Pal.Card, 15), new Rect2(sx, tabTop + 1, scr.Size.X, TabH - 1));
        DrawRect(new Rect2(sx, tabTop + 1, scr.Size.X, 12), Pal.Card);
        var active = _single is not null ? _singleIcon : _tab;
        var slot = scr.Size.X / 5f;
        for (var i = 0; i < 5; i++)
        {
            var cx = sx + slot * i + slot / 2;
            var on = i == active;
            Assets.DrawFrame(this, on ? Assets.PhoneIcons : Assets.PhoneIconsDim, i, Assets.Icon, new Vector2(Mathf.Round(cx - 8), tabTop + 3));
            var label = _single is null && i < _labels.Length ? _labels[i] : "";
            if (label.Length > 0) f.Draw(this, new Vector2(Mathf.Round(cx), tabTop + 18), f.Fit(label, (int)slot), on ? Ink.Brand : Ink.Dim, TextAlign.Center);
        }
    }
}
