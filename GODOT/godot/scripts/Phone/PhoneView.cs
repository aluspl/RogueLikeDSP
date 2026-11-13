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
public partial class PhoneView : Control, Touch.ITapTargets
{
    public const int W = 256, H = 348;
    private const int Bezel = 5, StatusH = 14, HeaderH = 24, TabH = 36;
    private const int FullHeaderH = 34, FullTabH = 40, FullButtonsH = 46;

    /// <summary>v0.23.2 (UI 2): najmniejsza szerokość telefonu na cały ekran w jego pikselach (jak telefon w poziomie, 256).</summary>
    private const float MinFullWidth = 248f;

    private PhonePage[] _tabs = [];
    private string[] _labels = [];
    private PhonePage _single;
    private int _singleIcon = -1;
    private int _tab;
    private Tween _slide;
    private List<(Rect2 Rect, int Index)> _hits = new();
    private readonly List<(Rect2 Rect, GameAction Action)> _buttons = new();
    private Rect2 _close;
    private int _scroll, _maxScroll;

    /// <summary>Tło pod nagłówkiem ramki telefonu przy przewiniętej treści (zaokrąglone tylko górne rogi).</summary>
    private static readonly StyleBoxFlat TopMask = new() { BgColor = Pal.Bg, AntiAliasing = false, CornerRadiusTopLeft = 15, CornerRadiusTopRight = 15 };

    /// <summary>Tło pod treścią (przyciski, pasek zakładek) w ramce telefonu – zaokrąglone tylko dolne rogi.</summary>
    private static readonly StyleBoxFlat BottomMask = new() { BgColor = Pal.Bg, AntiAliasing = false, CornerRadiusBottomLeft = 15, CornerRadiusBottomRight = 15 };
    private Rect2 _tabBar;

    public bool IsOpen { get; private set; }

    /// <summary>v0.23.2: ile pikseli treści strony wystaje pod przyciski (0 = mieści się; zrzuty i test dymny to sprawdzają).</summary>
    public float Overflow { get; private set; }
    public int TabIndex => _tab;
    public PhonePage Current => _single ?? (_tabs.Length > 0 ? _tabs[_tab] : null);
    public bool HasTabs => _single is null && _tabs.Length > 1;

    /// <summary>Telefon wjeżdża / zjeżdża (blokada wejścia w ScreenFlow).</summary>
    public bool Sliding => _slide is not null && _slide.IsValid() && _slide.IsRunning();

    /// <summary>Wywoływane po zmianie zakładki (np. zapamiętanie ostatniej, jak phone_tab na GBA).</summary>
    public Action<int> OnTabChanged;

    /// <summary>Pionowy ekran: aplikacja na cały ekran zamiast telefonu na środku.</summary>
    public static bool Full => Layout.Portrait;

    /// <summary>v0.22.0 (UX A): największa szerokość telefonu na dużym ekranie (składak, tablet) - wyśrodkowany, nie rozciągnięty.</summary>
    public const float MaxWidth = Layout.WideWidth;

    /// <summary>
    /// v0.23.2 (UI 2): powiększenie telefonu na cały ekran (pion) – tekst ~1,5x większy niż piksel UI. Wielokrotność
    /// 1/ContentScale (piksel fontu = całkowita liczba pikseli okna), ale telefon zostaje co najmniej MinFullWidth szeroki.
    /// W poziomie (telefon w ramce) 1.
    /// </summary>
    public static float Zoom
    {
        get
        {
            if (!Full) return 1f;
            var cs = Layout.ContentScale;
            var target = Mathf.Ceil(1.5f * cs) / cs;
            var fit = Mathf.Floor(Mathf.Min(Layout.UiSize.X, MaxWidth) / MinFullWidth * cs) / cs;
            return Mathf.Max(1f, Mathf.Min(target, fit));
        }
    }

    /// <summary>Rozmiar telefonu w jego pikselach (na ekranie razy Zoom).</summary>
    public static Vector2 PhoneSize => Full ? new Vector2(Mathf.Floor(Mathf.Min(Layout.UiSize.X, MaxWidth) / Zoom), Mathf.Ceil(Layout.UiSize.Y / Zoom)) : new Vector2(W, H);

    /// <summary>Lewy górny róg telefonu w pikselach UI.</summary>
    public static Vector2 Origin => Full ? new Vector2(Mathf.Floor((Layout.UiSize.X - PhoneSize.X * Zoom) / 2), 0) : ((Layout.UiSize - new Vector2(W, H)) / 2).Floor();

    /// <summary>v0.22.0 (UX A): dolna krawędź banerów przy telefonie na cały ekran – nad paskiem zakładek i przyciskami strony (piksele UI).</summary>
    public static float BannerFloor => Layout.UiSize.Y - Layout.SafeBottom - (FullTabH + FullButtonsH + 8) * Zoom;

    /// <summary>Pozycja schowanego telefonu (pod dolną krawędzią ekranu).</summary>
    private static Vector2 Stowed => new(Origin.X, Layout.UiSize.Y + 20);

    private bool Closable => _single is null || _single.Closable;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        Size = PhoneSize;
        Scale = Vector2.One * Zoom;
        Position = Stowed;
        Visible = false;
        Layout.Changed += Relayout;
    }

    public override void _ExitTree() => Layout.Changed -= Relayout;

    private void Relayout()
    {
        Size = PhoneSize;
        Scale = Vector2.One * Zoom;
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
        _scroll = 0;
        Current?.Enter();
        Present(instant);
    }

    /// <summary>Pojedyncza strona (wiadomość, paczka, Hurtownia, harmonogram) bez paska zakładek; icon zostaje dla wołających (v0.22.0, UX A: nie rysuje się).</summary>
    public void OpenSingle(PhonePage page, int icon, bool instant = false)
    {
        _single = page;
        _singleIcon = icon;
        page.Owner = this;
        _scroll = 0;
        page.Enter();
        Present(instant);
    }

    private void Present(bool instant)
    {
        var wasOpen = IsOpen;
        IsOpen = true;
        Visible = true;
        Size = PhoneSize;
        Scale = Vector2.One * Zoom;
        _slide?.Kill();
        if (instant || wasOpen || Current is { Overlay: true })
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
        if (instant || Current is { Overlay: true })
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
        _scroll = 0;
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
        if (e.VDir != 0 && ScrollBy(e.VDir * PhonePainter.RowH * 3)) return true;
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
        var p = (g.Pos - Position) / Scale.X;
        switch (g.Kind)
        {
            case GestureKind.Swipe when g.Dir.X != 0:
                if (HasTabs) SwitchTab(-g.Dir.X);
                return true;
            case GestureKind.Swipe:
            case GestureKind.SwipeRepeat when g.Dir.Y != 0:
                var a = g.Dir.Y < 0 ? GameAction.Down : GameAction.Up;
                if (Current.Input(InputCmd.Of(a))) QueueRedraw();
                else ScrollBy(-g.Dir.Y * PhonePainter.RowH * 3);
                return true;
            case GestureKind.Tap:
                return Tap(p);
            default:
                return false;
        }
    }

    public void TapTargets(System.Collections.Generic.List<Rect2> into)
    {
        if (!IsOpen) return;
        if (Layout.Touch && Closable) into.Add(_close);
        foreach (var (rect, _) in _buttons) into.Add(rect);
        if (HasTabs)
        {
            for (var i = 0; i < 5; i++) into.Add(new Rect2(_tabBar.Position.X + i * _tabBar.Size.X / 5f, _tabBar.Position.Y, _tabBar.Size.X / 5f, _tabBar.Size.Y));
        }
        foreach (var (rect, _) in _hits) into.Add(rect);
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

    /// <summary>v0.23.2: przewinięcie strony Scrolls o dy pikseli; false = nie ma dokąd.</summary>
    public bool ScrollBy(float dy)
    {
        if (Current is not { Scrolls: true } || _maxScroll <= 0) return false;
        var next = Mathf.Clamp(_scroll + (int)dy, 0, _maxScroll);
        if (next == _scroll) return false;
        _scroll = next;
        QueueRedraw();
        return true;
    }

    /// <summary>Strona od góry (np. kolejna strona Jak grać).</summary>
    public void ResetScroll()
    {
        _scroll = 0;
        QueueRedraw();
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
        if (page.Overlay)
        {
            DrawOverlay(page);
            return;
        }
        var f = PixelFont.I;
        var full = Full;
        var size = PhoneSize;
        Rect2 scr;
        float statusH, headerH, tabH;
        var z = Zoom;
        if (full)
        {
            scr = new Rect2(Vector2.Zero, size);
            DrawRect(scr, Pal.Bg);
            statusH = Mathf.Max(Mathf.Ceil(Layout.SafeTop / z), 8); // v0.23.2 (UI 2): bez udawanego paska 09:41 – prawdziwy telefon ma swój
            headerH = FullHeaderH;
            tabH = (HasTabs ? FullTabH : 4) + Mathf.Ceil(Layout.SafeBottom / z); // v0.22.0 (UX A): pasek zakładek tylko przy zakładkach
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
            tabH = HasTabs ? TabH : 8;
        }
        var sx = scr.Position.X;

        // treść strony (przyciski strony przy dotyku albo podpowiedź klawiszy pod nią); v0.23.2: strona Scrolls przewija się
        var tabTop = scr.End.Y - tabH;
        var actions = Layout.Touch ? page.Actions : [];
        var hint = Layout.Touch ? "" : page.Hint;
        var buttonsH = actions.Length > 0 ? (full ? FullButtonsH : 30) : 0;
        var contentBottom = tabTop - (buttonsH > 0 ? buttonsH : hint.Length > 0 ? 18 : 4);
        var top = scr.Position.Y + statusH + headerH + (full ? 6 : 0);
        var content = new Rect2(sx + (full ? 4 : 2), top, scr.Size.X - (full ? 8 : 4), contentBottom - top);
        var painter = new PhonePainter(this, content);
        var scroll = page.Scrolls ? _scroll : 0;
        if (scroll > 0) DrawSetTransform(new Vector2(0, -scroll));
        page.Draw(painter);
        if (scroll > 0) DrawSetTransform(Vector2.Zero);
        _hits = new List<(Rect2 Rect, int Index)>();
        foreach (var (rect, index) in painter.Hits)
        {
            var r = new Rect2(rect.Position - new Vector2(0, scroll), rect.Size).Intersection(new Rect2(scr.Position.X, top, scr.Size.X, contentBottom - top));
            if (r.Size.Y >= 4) _hits.Add((r, index));
        }
        var over = painter.Extent - content.End.Y;
        _maxScroll = page.Scrolls ? (int)Mathf.Max(0, Mathf.Ceil(over)) : 0;
        if (_scroll > _maxScroll) _scroll = _maxScroll;
        Overflow = page.Scrolls ? 0 : Mathf.Max(0, over);
        if (over > 0) // v0.23.2: nic nie wchodzi pod przyciski
        {
            var below = new Rect2(sx, contentBottom, scr.Size.X, scr.End.Y - contentBottom);
            if (full) DrawRect(below, Pal.Bg);
            else DrawStyleBox(BottomMask, below);
        }
        if (scroll > 0) // przewinięta treść nie wchodzi pod nagłówek
        {
            if (full) DrawRect(new Rect2(sx, scr.Position.Y, scr.Size.X, top - scr.Position.Y), Pal.Bg);
            else DrawStyleBox(TopMask, new Rect2(scr.Position, new Vector2(scr.Size.X, top - scr.Position.Y)));
        }
        if (_maxScroll > 0) // pasek przewijania przy prawej krawędzi
        {
            var trackH = contentBottom - top - 8;
            var barH = Mathf.Max(16, trackH * trackH / (trackH + _maxScroll));
            var barY = top + 4 + (trackH - barH) * scroll / _maxScroll;
            DrawStyleBox(Ui.Box(new Color(Pal.Dim, 0.5f), 2), new Rect2(scr.End.X - 5, barY, 3, barH));
        }
        if (!full) DrawStatus(scr, statusH);

        // nagłówek (przy dotyku z krzyżykiem zamykania – v0.23.2: duży, zawsze w prawym górnym rogu)
        var hy = scr.Position.Y + statusH + (full ? 9 : 2);
        var closeW = Layout.Touch && Closable ? (full ? 40 : 30) : 0;
        _close = new Rect2(scr.End.X - closeW - (full ? 4 : 2), scr.Position.Y + statusH, closeW, headerH);
        if (closeW > 0)
        {
            var box = full ? new Vector2(32, 28) : new Vector2(28, 26);
            DrawStyleBox(Ui.Box(Pal.Group, 8), new Rect2((_close.GetCenter() - box / 2).Round(), box));
            Assets.DrawFrame(this, Assets.TouchIcons, TouchIcon.Close, Assets.Icon, (_close.GetCenter() - new Vector2(8, 8)).Round(), 1, new Color(Pal.Brand, 1));
        }
        var title = f.Fit(page.Title, (int)(scr.Size.X * 0.6f));
        f.Draw(this, new Vector2(sx + (full ? 12 : 10), hy), title, Ink.Dark, TextAlign.Left, 1, true);
        var sub = page.Sub;
        if (sub.Length > 0)
        {
            var room = (int)(scr.Size.X - 30 - closeW - f.Measure(title, 1, true));
            f.Draw(this, new Vector2(scr.End.X - (full ? 12 : 10) - closeW, hy), f.Fit(sub, room), Ink.Dim, TextAlign.Right);
        }
        if (full) DrawRect(new Rect2(sx, scr.Position.Y + statusH + headerH - 1, scr.Size.X, 1), Pal.Border);
        if (hint.Length > 0) f.Draw(this, new Vector2(scr.Position.X + scr.Size.X / 2, contentBottom), f.Fit(hint, (int)scr.Size.X - 12), Ink.Dim, TextAlign.Center);
        DrawButtons(actions, new Rect2(sx + 8, contentBottom + 4, scr.Size.X - 16, buttonsH - 8));

        // pasek zakładek (v0.22.0, UX A: pojedyncza strona go nie ma – ikony nic tam nie robiły)
        if (!HasTabs)
        {
            _tabBar = new Rect2();
            return;
        }
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
        var iconScale = 1;
        for (var i = 0; i < 5; i++)
        {
            var cx = sx + slot * i + slot / 2;
            var on = i == active;
            var isz = Assets.Icon * iconScale;
            Assets.DrawFrame(this, on ? Assets.PhoneIcons : Assets.PhoneIconsDim, i, Assets.Icon, new Vector2(Mathf.Round(cx - isz / 2f), tabTop + (full ? 5 : 3)), iconScale);
            var label = _single is null && i < _labels.Length ? _labels[i] : "";
            if (label.Length > 0) f.Draw(this, new Vector2(Mathf.Round(cx), tabTop + (full ? 21 : 18)), f.Fit(label, (int)slot), on ? Ink.Brand : Ink.Dim, TextAlign.Center);
        }
    }

    /// <summary>v0.23.2 (UI 2): strona-nakładka (pauza) w bezpiecznym obszarze, bez telefonu; przyciski rysuje sama.</summary>
    private void DrawOverlay(PhonePage page)
    {
        var z = Zoom;
        var size = PhoneSize;
        var area = Full
            ? new Rect2(12, Mathf.Ceil(Layout.SafeTop / z) + 8, size.X - 24, size.Y - Mathf.Ceil((Layout.SafeTop + Layout.SafeBottom) / z) - 16)
            : new Rect2(4, (H - Layout.UiSize.Y) / 2 + 8, W - 8, Layout.UiSize.Y - 16);
        var painter = new PhonePainter(this, area);
        page.Draw(painter);
        _hits = painter.Hits;
        _buttons.Clear();
        _tabBar = new Rect2();
        _close = new Rect2();
        _maxScroll = 0;
        Overflow = Mathf.Max(0, painter.Extent - area.End.Y);
        var hint = Layout.Touch ? "" : page.Hint;
        if (hint.Length > 0) PixelFont.I.Draw(this, new Vector2(area.GetCenter().X, area.End.Y - 16), PixelFont.I.Fit(hint, (int)area.Size.X), Ink.MapDim, TextAlign.Center);
    }

    /// <summary>Pasek statusu w ramce telefonu (poziomo): 09:41, wyspa z kamerą, zasięg, bateria. Pionowo jest prawdziwy.</summary>
    private void DrawStatus(Rect2 scr, float statusH)
    {
        var f = PixelFont.I;
        var sx = scr.Position.X;
        var sy = scr.Position.Y;
        f.Draw(this, new Vector2(sx + 12, sy), "09:41", Ink.Dark);
        DrawStyleBox(Ui.Box(Pal.Text, 5), new Rect2(W / 2 - 22, sy + 3, 44, 9));
        var bx = scr.End.X - 14;
        DrawRect(new Rect2(bx - 14, sy + 4, 14, 8), Pal.Text, false, 1);
        DrawRect(new Rect2(bx - 12, sy + 6, 9, 4), Pal.Done);
        DrawRect(new Rect2(bx, sy + 6, 2, 4), Pal.Text);
        for (var k = 0; k < 4; k++) DrawRect(new Rect2(bx - 34 + k * 4, sy + 11 - k * 2 - 2, 3, k * 2 + 2), Pal.Text);
    }

    /// <summary>
    /// Przyciski strony w rzędzie (v0.21.51 - ten sam układ wszędzie): główny (wybierz / dalej) w fiolecie marki zawsze
    /// po prawej, powrót (wróć / zostaw) po lewej, pozostałe między nimi; sam jeden przycisk zajmuje prawą połowę, żeby
    /// stuknięcie z lewej nigdy niczego nie zatwierdzało.
    /// </summary>
    private void DrawButtons(PageAction[] actions, Rect2 area)
    {
        _buttons.Clear();
        if (actions.Length == 0) return;
        var ordered = Arrange(actions);
        var f = PixelFont.I;
        var gap = 8f;
        var slots = Mathf.Max(2, ordered.Length);
        var w = (area.Size.X - gap * (slots - 1)) / slots;
        var first = slots - ordered.Length;
        for (var i = 0; i < ordered.Length; i++)
        {
            var a = ordered[i];
            var r = new Rect2(Mathf.Round(area.Position.X + (first + i) * (w + gap)), area.Position.Y, Mathf.Round(w), area.Size.Y);
            var primary = a.Resolved == PageRole.Primary;
            DrawStyleBox(Ui.Box(primary ? Pal.Brand : Pal.Card, 10, primary ? Pal.Brand : Pal.Border), r);
            f.Draw(this, new Vector2(r.GetCenter().X, Mathf.Round(r.GetCenter().Y - 9)), f.Fit(a.Label, (int)r.Size.X - 10 - a.Label.Length), primary ? Ink.White : Ink.Brand, TextAlign.Center, 1, true);
            _buttons.Add((r, a.Action));
        }
    }

    /// <summary>Kolejność od lewej: powrót, zwykłe, główny (pierwszy główny z listy; kolejne główne jak zwykłe).</summary>
    public static PageAction[] Arrange(PageAction[] actions)
    {
        var list = new List<PageAction>();
        foreach (var a in actions) if (a.Resolved == PageRole.Back) list.Add(a);
        var primaryAt = Array.FindIndex(actions, a => a.Resolved == PageRole.Primary);
        for (var i = 0; i < actions.Length; i++)
        {
            var r = actions[i].Resolved;
            if (r == PageRole.Other || (r == PageRole.Primary && i != primaryAt)) list.Add(actions[i] with { Role = PageRole.Other });
        }
        if (primaryAt >= 0) list.Add(actions[primaryAt]);
        return list.ToArray();
    }

    /// <summary>Prostokąt wiersza listy z indeksem w pikselach UI (test dymny: stuknięcie w kartę).</summary>
    public Rect2? RowRect(int index)
    {
        foreach (var (rect, i) in _hits) if (i == index) return ToUi(rect);
        return null;
    }

    /// <summary>Prostokąt przycisku strony z akcją (test dymny: stuknięcie w przycisk po lewej / prawej).</summary>
    public Rect2? ButtonRect(GameAction a)
    {
        foreach (var (rect, action) in _buttons) if (action == a) return ToUi(rect);
        return null;
    }

    /// <summary>Prostokąt w pikselach telefonu -> piksele UI (pozycja i powiększenie).</summary>
    public Rect2 ToUi(Rect2 r) => new(Position + r.Position * Scale.X, r.Size * Scale.X);
}
