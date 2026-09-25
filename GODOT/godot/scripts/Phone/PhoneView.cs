using System;
using System.Collections.Generic;
using Godot;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using LifeLike.Game.Touch;

namespace LifeLike.Game.Phone;

/// <summary>
/// Smartfon bohatera z aplikacją PlanBudowlany (KONCEPCJA, sekcja 3). Poziomo: telefon pionowo na środku ekranu,
/// wysuwa się z dołu. Pionowo (gra na telefonie): aplikacja na cały ekran - pasek statusu w miejscu wyspy,
/// pasek zakładek nad paskiem domowym. Ramka, pasek statusu 09:41, nagłówek z tytułem i podtytułem, treść strony
/// (PhonePage) i dolny pasek zakładek z ikonami (aktywna w fiolecie marki). Zakładki przełączają Q/E (LB/RB)
/// i strzałki w lewo/prawo jak L/R na GBA, a przy dotyku - przesunięcie palcem w bok i ikony paska. Przy dotyku
/// zamiast podpowiedzi klawiszy są przyciski strony (PhonePage.Actions) i krzyżyk w nagłówku.
/// Akcje w telefonie są poza czasem gry.
/// </summary>
public partial class PhoneView : Control
{
    public const int W = 256, H = 348;
    private const int Bezel = 5, StatusH = 14, HeaderH = 24, TabH = 36;
    private const int FullHeaderH = 36, FullTabH = 52;

    private PhonePage[] _tabs = [];
    private string[] _labels = [];
    private PhonePage _single;
    private int _singleIcon = -1;
    private int _tab;
    private Tween _slide;
    private List<(Rect2 Rect, int Index)> _hits = new();
    private readonly List<(Rect2 Rect, GameAction Action)> _buttons = new();
    private Rect2 _close;
    private Rect2 _tabBar;

    public bool IsOpen { get; private set; }
    public int TabIndex => _tab;
    public PhonePage Current => _single ?? (_tabs.Length > 0 ? _tabs[_tab] : null);
    public bool HasTabs => _single is null && _tabs.Length > 1;

    /// <summary>Wywoływane po zmianie zakładki (np. zapamiętanie ostatniej, jak phone_tab na GBA).</summary>
    public Action<int> OnTabChanged;

    /// <summary>Pionowy ekran: aplikacja na cały ekran zamiast telefonu na środku.</summary>
    public static bool Full => Layout.Portrait;

    public static Vector2 PhoneSize => Full ? Layout.UiSize : new Vector2(W, H);

    public static Vector2 Origin => Full ? Vector2.Zero : ((Layout.UiSize - new Vector2(W, H)) / 2).Floor();

    /// <summary>Pozycja schowanego telefonu (pod dolną krawędzią ekranu).</summary>
    private static Vector2 Stowed => new(Origin.X, Layout.UiSize.Y + 20);

    private bool Closable => _single is null || _single.Closable;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        Size = PhoneSize;
        Position = Stowed;
        Visible = false;
        Layout.Changed += Relayout;
    }

    public override void _ExitTree() => Layout.Changed -= Relayout;

    private void Relayout()
    {
        Size = PhoneSize;
        if (IsOpen)
        {
            _slide?.Kill();
            Position = Origin;
        }
        QueueRedraw();
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
        Size = PhoneSize;
        _slide?.Kill();
        if (instant || wasOpen)
        {
            Position = Origin;
        }
        else
        {
            Position = Stowed;
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
        _slide.TweenProperty(this, "position", Stowed, 0.16f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        _slide.TweenCallback(Callable.From(() => Visible = IsOpen));
    }

    /// <summary>Przejście na zakładkę o numerze (np. z kliknięcia banera).</summary>
    public void ShowTab(int index)
    {
        if (!HasTabs || index < 0 || index >= _tabs.Length) return;
        SwitchTab(index - _tab);
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
    public bool HandleInput(InputCmd e)
    {
        if (!IsOpen || Current is null) return false;
        if (Current.Input(e))
        {
            QueueRedraw();
            return true;
        }
        if (!HasTabs) return false;
        var d = e.Is(GameAction.TabNext | GameAction.Right) ? 1 : e.Is(GameAction.TabPrev | GameAction.Left) ? -1 : 0;
        if (d == 0) return false;
        SwitchTab(d);
        return true;
    }

    /// <summary>
    /// Gest dotyku na telefonie: dotknięcie ikony paska zakładek, krzyżyka, przycisku strony albo wiersza listy;
    /// przesunięcie w bok = zakładka, w górę/dół = lista. true = obsłużone.
    /// </summary>
    public bool HandleGesture(in Gesture g)
    {
        if (!IsOpen || Current is null) return false;
        var p = g.Pos - Position;
        switch (g.Kind)
        {
            case GestureKind.Swipe when g.Dir.X != 0:
                if (HasTabs) SwitchTab(-g.Dir.X);
                return true;
            case GestureKind.Swipe:
            case GestureKind.SwipeRepeat when g.Dir.Y != 0:
                var a = g.Dir.Y < 0 ? GameAction.Down : GameAction.Up;
                if (Current.Input(InputCmd.Of(a))) QueueRedraw();
                return true;
            case GestureKind.Tap:
                return Tap(p);
            default:
                return false;
        }
    }

    private bool Tap(Vector2 p)
    {
        if (Layout.Touch && Closable && _close.Grow(6).HasPoint(p))
        {
            Press(GameAction.Cancel);
            return true;
        }
        foreach (var (rect, action) in _buttons)
        {
            if (!rect.Grow(3).HasPoint(p)) continue;
            Press(action);
            return true;
        }
        if (HasTabs && _tabBar.HasPoint(p))
        {
            var i = (int)((p.X - _tabBar.Position.X) / (_tabBar.Size.X / 5f));
            if (i >= 0 && i < _tabs.Length && i != _tab) SwitchTab(i - _tab);
            return true;
        }
        foreach (var (rect, index) in _hits)
        {
            if (!rect.GrowIndividual(0, 2, 0, 2).HasPoint(p)) continue;
            if (Current.TapRow(index))
            {
                Sfx.Play("menu");
                QueueRedraw();
            }
            return true;
        }
        return false;
    }

    /// <summary>Przycisk ekranowy = wciśnięcie i puszczenie akcji (ta sama droga co klawiatura).</summary>
    private static void Press(GameAction a)
    {
        GameInput.Press(a);
        GameInput.Release(a);
    }

    public override void _Draw()
    {
        try
        {
            DrawContent();
        }
        catch (Exception ex)
        {
            DrawErrors.Record("PhoneView", ex);
        }
    }

    private void DrawContent()
    {
        var page = Current;
        if (page is null) return;
        var f = PixelFont.I;
        var full = Full;
        var size = PhoneSize;
        Rect2 scr;
        float statusH, headerH, tabH;
        if (full)
        {
            scr = new Rect2(Vector2.Zero, size);
            DrawRect(scr, Pal.Bg);
            statusH = Mathf.Max(Layout.SafeTop, 20);
            headerH = FullHeaderH;
            tabH = FullTabH + Layout.SafeBottom;
        }
        else
        {
            var outer = new Rect2(Vector2.Zero, size);
            DrawStyleBox(Ui.Box(new Color(0, 0, 0, 0.4f), 20), new Rect2(4, 6, W, H)); // cień telefonu
            DrawStyleBox(Ui.Box(Pal.Text, 20, Pal.PhoneBezel), outer);                // obudowa
            scr = new Rect2(Bezel, Bezel, W - 2 * Bezel, H - 2 * Bezel);
            DrawStyleBox(Ui.Box(Pal.Bg, 15), scr);
            statusH = StatusH;
            headerH = HeaderH;
            tabH = TabH;
        }
        DrawStatus(scr, statusH, full);

        // nagłówek (przy dotyku z krzyżykiem zamykania)
        var sx = scr.Position.X;
        var hy = scr.Position.Y + statusH + (full ? 8 : 2);
        var closeW = Layout.Touch && Closable ? (full ? 44 : 30) : 0;
        _close = new Rect2(scr.End.X - closeW - 2, scr.Position.Y + statusH, closeW, headerH);
        if (closeW > 0)
        {
            DrawStyleBox(Ui.Box(Pal.Group, 8), new Rect2(_close.GetCenter() - new Vector2(14, 13), new Vector2(28, 26)));
            Assets.DrawFrame(this, Assets.TouchIcons, TouchIcon.Close, Assets.Icon, (_close.GetCenter() - new Vector2(8, 8)).Round(), 1, new Color(Pal.Brand, 1));
        }
        var title = f.Fit(page.Title, full ? 200 : 130);
        f.Draw(this, new Vector2(sx + (full ? 16 : 10), hy), title, Ink.Dark, TextAlign.Left, 1, true);
        var sub = page.Sub;
        if (sub.Length > 0)
        {
            var room = (int)(scr.Size.X - 24 - closeW - f.Measure(title, 1, true));
            f.Draw(this, new Vector2(scr.End.X - (full ? 16 : 10) - closeW, hy), f.Fit(sub, room), Ink.Dim, TextAlign.Right);
        }
        if (full) DrawRect(new Rect2(sx, scr.Position.Y + statusH + headerH - 1, scr.Size.X, 1), Pal.Border);

        // przyciski strony (dotyk) albo podpowiedź klawiszy
        var tabTop = scr.End.Y - tabH;
        var actions = Layout.Touch ? page.Actions : [];
        var hint = Layout.Touch ? "" : page.Hint;
        var buttonsH = actions.Length > 0 ? (full ? 56 : 30) : 0;
        var contentBottom = tabTop - (buttonsH > 0 ? buttonsH : hint.Length > 0 ? 18 : 4);
        var top = scr.Position.Y + statusH + headerH + (full ? 6 : 0);
        var content = new Rect2(sx + (full ? 6 : 2), top, scr.Size.X - (full ? 12 : 4), contentBottom - top);
        var painter = new PhonePainter(this, content);
        page.Draw(painter);
        _hits = painter.Hits;
        if (hint.Length > 0) f.Draw(this, new Vector2(scr.Position.X + scr.Size.X / 2, contentBottom), f.Fit(hint, (int)scr.Size.X - 12), Ink.Dim, TextAlign.Center);
        DrawButtons(actions, new Rect2(sx + 8, contentBottom + 4, scr.Size.X - 16, buttonsH - 8));

        // pasek zakładek
        DrawRect(new Rect2(sx, tabTop, scr.Size.X, 1), Pal.Border);
        if (full) DrawRect(new Rect2(sx, tabTop + 1, scr.Size.X, tabH - 1), Pal.Card);
        else
        {
            DrawStyleBox(Ui.Box(Pal.Card, 15), new Rect2(sx, tabTop + 1, scr.Size.X, TabH - 1));
            DrawRect(new Rect2(sx, tabTop + 1, scr.Size.X, 12), Pal.Card);
        }
        _tabBar = new Rect2(sx, tabTop, scr.Size.X, full ? FullTabH : TabH);
        var active = _single is not null ? _singleIcon : _tab;
        var slot = scr.Size.X / 5f;
        var iconScale = full ? 2 : 1;
        for (var i = 0; i < 5; i++)
        {
            var cx = sx + slot * i + slot / 2;
            var on = i == active;
            var isz = Assets.Icon * iconScale;
            Assets.DrawFrame(this, on ? Assets.PhoneIcons : Assets.PhoneIconsDim, i, Assets.Icon, new Vector2(Mathf.Round(cx - isz / 2f), tabTop + (full ? 4 : 3)), iconScale);
            var label = _single is null && i < _labels.Length ? _labels[i] : "";
            if (label.Length > 0) f.Draw(this, new Vector2(Mathf.Round(cx), tabTop + (full ? 35 : 18)), f.Fit(label, (int)slot), on ? Ink.Brand : Ink.Dim, TextAlign.Center);
        }
    }

    /// <summary>Pasek statusu: 09:41, zasięg, bateria; w ramce telefonu także wyspa z kamerą (pionowo jest prawdziwa).</summary>
    private void DrawStatus(Rect2 scr, float statusH, bool full)
    {
        var f = PixelFont.I;
        var sx = scr.Position.X;
        var sy = scr.Position.Y + (full ? Mathf.Round((statusH - 16) / 2) : 0);
        f.Draw(this, new Vector2(sx + (full ? 28 : 12), sy), "09:41", Ink.Dark);
        if (!full) DrawStyleBox(Ui.Box(Pal.Text, 5), new Rect2(W / 2 - 22, sy + 3, 44, 9));
        var bx = scr.End.X - (full ? 28 : 14);
        DrawRect(new Rect2(bx - 14, sy + 4, 14, 8), Pal.Text, false, 1);
        DrawRect(new Rect2(bx - 12, sy + 6, 9, 4), Pal.Done);
        DrawRect(new Rect2(bx, sy + 6, 2, 4), Pal.Text);
        for (var k = 0; k < 4; k++) DrawRect(new Rect2(bx - 34 + k * 4, sy + 11 - k * 2 - 2, 3, k * 2 + 2), Pal.Text);
    }

    /// <summary>Przyciski strony w rzędzie: pierwszy w fiolecie marki (główna akcja), kolejne jasne.</summary>
    private void DrawButtons(PageAction[] actions, Rect2 area)
    {
        _buttons.Clear();
        if (actions.Length == 0) return;
        var f = PixelFont.I;
        var gap = 8f;
        var w = (area.Size.X - gap * (actions.Length - 1)) / actions.Length;
        for (var i = 0; i < actions.Length; i++)
        {
            var r = new Rect2(Mathf.Round(area.Position.X + i * (w + gap)), area.Position.Y, Mathf.Round(w), area.Size.Y);
            var primary = i == 0;
            DrawStyleBox(Ui.Box(primary ? Pal.Brand : Pal.Card, 10, primary ? Pal.Brand : Pal.Border), r);
            f.Draw(this, new Vector2(r.GetCenter().X, Mathf.Round(r.GetCenter().Y - 9)), f.Fit(actions[i].Label, (int)r.Size.X - 8), primary ? Ink.White : Ink.Brand, TextAlign.Center, 1, true);
            _buttons.Add((r, actions[i].Action));
        }
    }
}
