using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;

namespace LifeLike.Game.Screens.Views;

/// <summary>
/// Wybór zawodu (run_class_select na GBA, nowy układ): u góry karuzela portretów wszystkich zawodów - odblokowane
/// najpierw, zablokowane po nich (kolejność z danych w każdej grupie); wybrany większy, z animacją i ramką w fiolecie
/// marki, zablokowane jako sylwetki z kłódką i ceną. Pod spodem karta: nazwa, moc z ikoną i opisem, narzędzie ze
/// statystyką, paski statystyk z premią Szkoleń, trudność (↑/↓) i pamiątka (Q/E), uprawnienia z odznak.
/// </summary>
public partial class ClassSelectView : Control
{
    private GameData _d;
    private Profile _p;
    private int[] _order = [];
    private int _pos;
    private float _clock;
    private float _slide;      // pozycja ramki wyboru (płynnie za _pos)
    private float _cardShift;  // wjazd karty przy zmianie zawodu
    private readonly Dictionary<int, float> _scale = new();
    private readonly List<(Rect2 Rect, ClassSelectHit Hit, int Arg)> _hits = new();
    private readonly Dictionary<string, Rect2> _coach = new();   // samouczek: prostokąty elementów (id kroku)

    public int Difficulty { get; set; }
    /// <summary>Dymek z opisem statystyki (dotknięcie wiersza; na komputerze też najechanie myszą); -1 = brak.</summary>
    public int TipStat { get; set; } = -1;
    private int _hoverStat = -1;
    private readonly Rect2[] _statRects = new Rect2[6];
    public string Note { get; set; } = "";

    /// <summary>Zawód (indeks w danych) pod ramką wyboru.</summary>
    public int Selected => _order.Length > 0 ? _order[_pos] : 0;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    /// <summary>Kolejność: odblokowane, potem zablokowane; cls = zawód, który ma być wybrany.</summary>
    public void Setup(GameData d, Profile p, int cls)
    {
        _d = d;
        _p = p;
        var all = Enumerable.Range(0, d.Classes.Length).ToList();
        _order = all.Where(i => Meta.ClassUnlocked(d, p, i)).Concat(all.Where(i => !Meta.ClassUnlocked(d, p, i))).ToArray();
        _pos = Math.Max(0, Array.IndexOf(_order, cls));
        _slide = _pos;
        foreach (var i in all) _scale[i] = i == Selected ? 2f : 1f;
        QueueRedraw();
    }

    public void Move(int d)
    {
        TipStat = -1;
        if (_order.Length == 0) return;
        _pos = (_pos + d + _order.Length) % _order.Length;
        _cardShift = d * 18f;
        Sfx.Play("menu");
    }

    /// <summary>Co jest pod punktem (dotyk / klik): portret (Arg = pozycja w karuzeli), trudność, pamiątka, przyciski.</summary>
    /// <summary>Samouczek: element omawiany w kroku id (zawód, trudność, pamiątka, statystyki, tryb inwestora, start).</summary>
    public Rect2 CoachRect(string id)
    {
        if (id is "new" or "class") id = "class";
        return _coach.TryGetValue(id, out var r) ? r : _coach.TryGetValue("class", out var c) ? c : new Rect2();
    }

    public (ClassSelectHit Hit, int Arg) HitAt(Vector2 p)
    {
        foreach (var (r, hit, arg) in _hits)
        {
            if (r.HasPoint(p)) return (hit, arg);
        }
        return (ClassSelectHit.None, 0);
    }

    /// <summary>Wybór portretu z karuzeli (pozycja k).</summary>
    public void MoveTo(int k)
    {
        if (k == _pos || k < 0 || k >= _order.Length) return;
        Move(k - _pos);
    }

    public override void _Process(double delta)
    {
        if (!Visible || _d is null) return;
        var dt = (float)delta;
        _clock += dt;
        _slide = Mathf.Lerp(_slide, _pos, Mathf.Min(1f, dt * 14f));
        if (Mathf.Abs(_slide - _pos) < 0.01f) _slide = _pos;
        _cardShift = Mathf.MoveToward(_cardShift, 0, dt * 140f);
        foreach (var i in _order) _scale[i] = Mathf.MoveToward(_scale[i], i == Selected ? 2f : 1f, dt * 8f);
        if (!Layout.Touch) // dymek statystyki pod kursorem myszy
        {
            var mouse = GetLocalMousePosition();
            _hoverStat = -1;
            for (var k = 0; k < _statRects.Length; k++)
            {
                if (_statRects[k].HasPoint(mouse)) _hoverStat = k;
            }
        }
        QueueRedraw();
    }

    private float Spacing => Mathf.Min(76, (Size.X - 24) / Mathf.Max(1, _order.Length));

    private float SlotX(float pos) => Size.X / 2 + (pos - (_order.Length - 1) / 2f) * Spacing;

    public override void _Draw()
    {
        try
        {
            DrawContent();
        }
        catch (System.Exception ex)
        {
            DrawErrors.Record("ClassSelectView", ex);
        }
    }

    private void DrawContent()
    {
        if (_d is null) return;
        _hits.Clear();
        _coach.Clear();
        var f = PixelFont.I;
        var w = Size.X;
        var h = Size.Y;
        var portrait = h > w;
        var top = Layout.SafeTop;
        var head = top + (portrait ? 150 : 112);
        DrawRect(new Rect2(0, 0, w, h), Pal.ScreenBg);
        Ui.VioletGradient(this, new Rect2(0, 0, w, head));
        DrawRect(new Rect2(0, head, w, 2), Pal.Accent);
        Ui.WarningStripe(this, new Rect2(0, h - 8, w, 8), _clock * 10f);

        f.Draw(this, new Vector2(w / 2, top + (portrait ? 10 : 3)), "Wybierz zawód", Ink.OnBrand, TextAlign.Center, 1, true);

        // karuzela portretów
        var baseY = top + (portrait ? 96 : 70);
        var fx = SlotX(_slide);
        DrawStyleBox(Ui.Box(new Color(0, 0, 0, 0.25f), 10), new Rect2(fx - 38, baseY - 50, 76, 84));
        DrawStyleBox(Ui.Box(new Color(Pal.Card, 0.18f), 10, Pal.Card), new Rect2(fx - 38, baseY - 52, 76, 84));
        for (var k = 0; k < _order.Length; k++)
        {
            var cls = _order[k];
            var unl = Meta.ClassUnlocked(_d, _p, cls);
            var sc = _scale[cls];
            var sel = k == _pos;
            var x = SlotX(k);
            var anim = sel && unl && ((int)(_clock / 0.4f) & 1) == 1;
            var bob = sel ? Mathf.Round(Mathf.Sin(_clock * 4f) * 1.5f) : 0;
            var frame = unl ? (anim ? Assets.AnimB(_d.Classes[cls].Frame) : _d.Classes[cls].Frame) : Assets.Silhouette(cls);
            var size = 32 * sc;
            var feet = baseY + 14;
            DrawTextureRect(Assets.Shadow, new Rect2(x - 12 * sc / 1.4f, feet - 3, 24 * sc / 1.4f, 7), false);
            var dst = new Rect2(Mathf.Round(x - size / 2), Mathf.Round(feet - size + 2 * sc + bob), Mathf.Round(size), Mathf.Round(size));
            DrawTextureRectRegion(Assets.Actors, dst, Assets.Frame(frame, Assets.Actor), unl ? Colors.White : new Color(1, 1, 1, sel ? 1f : 0.75f));
            if (!unl)
            {
                DrawTextureRectRegion(Assets.Actors, new Rect2(Mathf.Round(x + size / 2 - 18), Mathf.Round(feet - 18), 16, 16), Assets.Frame(Assets.FrameLock, Assets.Actor));
                if (!sel && f.Measure(LockedShort(cls)) <= Spacing - 2)   // wąski pasek (pion, 9 zawodów): bez podpisów, żeby się nie nakładały
                    f.Draw(this, new Vector2(x, feet + 6), LockedShort(cls), Ink.MapDim, TextAlign.Center);
            }
            else if (!sel)
            {
                f.Draw(this, new Vector2(x, feet + 6), f.Fit(_d.Classes[cls].Name.Split(' ')[0], (int)Math.Min(70, Spacing - 2)), Ink.MapDim, TextAlign.Center);
            }
        }
        for (var k = 0; k < _order.Length; k++)
            _hits.Add((new Rect2(SlotX(k) - Spacing / 2, baseY - 50, Spacing, 84), ClassSelectHit.Portrait, k));
        _coach["class"] = new Rect2(4, baseY - 54, w - 8, 88);
        if (!portrait)
        {
            f.Draw(this, new Vector2(SlotX(0) - 50, baseY - 10), "<", Ink.OnBrand, TextAlign.Center);
            f.Draw(this, new Vector2(SlotX(_order.Length - 1) + 50, baseY - 10), ">", Ink.OnBrand, TextAlign.Center);
        }

        if (portrait)
        {
            var bh = 52f;
            var by = h - Mathf.Max(Layout.SafeBottom, 8) - bh - 6;
            DrawPortraitCard(new Rect2(12 + _cardShift, head + 10, w - 24, by - head - 20));
            var bw = (w - 36) / 3;
            Button(new Rect2(12, by, bw, bh), "Wróć", false, ClassSelectHit.Back);
            Button(new Rect2(24 + bw, by, w - 36 - bw, bh), Meta.ClassUnlocked(_d, _p, Selected) ? "Start budowy" : "Zablokowany", true, ClassSelectHit.Start);
            _coach["go"] = new Rect2(24 + bw, by, w - 36 - bw, bh);
            DrawStatTip();
            return;
        }
        DrawCard(new Rect2(40 + _cardShift, 122, w - 80, 204));
        DrawStatTip();
        if (Layout.Touch)
        {
            Button(new Rect2(40, h - 30, 120, 24), "Wróć", false, ClassSelectHit.Back);
            Button(new Rect2(w - 160, h - 30, 120, 24), "Start budowy", true, ClassSelectHit.Start);
            _coach["go"] = new Rect2(w - 160, h - 30, 120, 24);
        }
        else
        {
            _coach["go"] = new Rect2(40, h - 30, w - 80, 22);
            f.Draw(this, new Vector2(w / 2, h - 26), "Strzałki: zawód / trudność   Q/E: pamiątka   I: statystyki   Enter: start   Esc: wróć", Ink.MapDim, TextAlign.Center);
        }
    }

    private void Button(Rect2 r, string label, bool primary, ClassSelectHit hit)
    {
        var f = PixelFont.I;
        DrawStyleBox(Ui.Box(primary ? Pal.Brand : Pal.Card, 10, primary ? Pal.Accent : Pal.Border), r);
        f.Draw(this, new Vector2(r.GetCenter().X, Mathf.Round(r.GetCenter().Y - 9)), f.Fit(label, (int)r.Size.X - 8), primary ? Ink.White : Ink.Brand, TextAlign.Center, 1, true);
        _hits.Add((r, hit, 0));
    }

    /// <summary>Karta zawodu na pionowym ekranie: jedna kolumna, trudność i pamiątka jako wiersze do dotknięcia.</summary>
    private void DrawPortraitCard(Rect2 r)
    {
        var f = PixelFont.I;
        var cls = Selected;
        var c = _d.Classes[cls];
        var unl = Meta.ClassUnlocked(_d, _p, cls);
        var m = Meta.Mods(_d, _p);
        DrawStyleBox(Ui.Box(new Color(0, 0, 0, 0.35f), 10), new Rect2(r.Position + new Vector2(0, 3), r.Size));
        DrawStyleBox(Ui.Box(Pal.Card, 10), r);
        var x = r.Position.X + 12;
        var cw = (int)r.Size.X - 24;
        var y = r.Position.Y + 8;
        f.Draw(this, new Vector2(x, y), f.Fit(c.Name, cw, 2), Ink.Dark, TextAlign.Left, 2);
        y += 34;
        foreach (var line in f.Wrap(c.Desc, cw))
        {
            f.Draw(this, new Vector2(x, y), line, Ink.Dim);
            y += 16;
        }
        y += 6;
        DrawStyleBox(Ui.Box(Pal.Group, 6), new Rect2(x, y + 2, 36, 36));
        Assets.DrawFrame(this, Assets.AbilityIcons, cls, 32, new Vector2(x + 2, y + 4));
        f.Draw(this, new Vector2(x + 44, y), f.Fit($"Moc: {c.AbilityName}", cw - 44), Ink.Brand);
        var ad = f.Wrap(c.AbilityDesc, cw - 44);
        for (var k = 0; k < ad.Count && k < 2; k++) f.Draw(this, new Vector2(x + 44, y + 17 + k * 16), ad[k], Ink.Dim);
        y += Mathf.Max(44, 17 + Mathf.Min(ad.Count, 2) * 16 + 4);
        var wpn = _d.Weapons[c.Weapon];
        var tag = UiText.StatShort(wpn.ScalesWith);
        var tw = f.Measure(tag) + 10;
        f.Draw(this, new Vector2(x, y), f.Fit($"{wpn.Name} {wpn.MinDamage}-{wpn.MaxDamage}, zasięg {wpn.Range}", cw - tw - 6), Ink.Dark);
        DrawStyleBox(Ui.Box(Pal.Group, 7), new Rect2(x + cw - tw, y + 2, tw, 14));
        f.Draw(this, new Vector2(x + cw - tw / 2f, y), tag, Ink.Brand, TextAlign.Center);
        y += 24;
        var statsTop = y;
        y = DrawStats(x, y, cw, cls, c, m, unl, 18) + 6;
        _coach["stats"] = new Rect2(x - 4, statsTop - 4, cw + 8, y - statsTop);

        DrawRect(new Rect2(x, y, cw, 1), Pal.Border);
        y += 4;
        var diff = _d.Difficulties[Difficulty];
        var diffLock = Meta.DifficultyUnlocked(_d, _p, Difficulty) ? "" : " (zablok.)";
        var rowH = Layout.TouchTarget;
        var dr = new Rect2(x - 4, y, cw + 8, rowH);
        DrawStyleBox(Ui.Box(Pal.Group, 8), dr.Grow(-2));
        f.Draw(this, new Vector2(x + 6, Mathf.Round(dr.GetCenter().Y - 8)), "Trudność", Ink.Dim);
        f.Draw(this, new Vector2(x + cw - 6, Mathf.Round(dr.GetCenter().Y - 8)), $"< {diff.Name}{diffLock} >", diffLock.Length > 0 ? Ink.Late : Ink.Brand, TextAlign.Right);
        _hits.Add((dr, ClassSelectHit.Difficulty, 0));
        _coach["difficulty"] = dr;
        y += rowH + 4;
        var k2 = Meta.SelectedKeepsake(_d, _p);
        var keep = k2 < 0 ? "bez pamiątki" : $"{_d.Keepsakes[k2].Name} {UiText.Roman(Meta.KeepsakeRank(_d, _p, k2) - 1)}";
        var kr = new Rect2(x - 4, y, cw + 8, rowH);
        DrawStyleBox(Ui.Box(Pal.Group, 8), kr.Grow(-2));
        f.Draw(this, new Vector2(x + 6, Mathf.Round(kr.GetCenter().Y - 8)), "Pamiątka", Ink.Dim);
        f.Draw(this, new Vector2(x + cw - 6, Mathf.Round(kr.GetCenter().Y - 8)), f.Fit($"< {keep} >", cw - 90), k2 < 0 ? Ink.Dim : Ink.Done, TextAlign.Right);
        _hits.Add((kr, ClassSelectHit.Keepsake, 0));
        _coach["keepsake"] = kr;
        y += rowH + 4;
        if (Meta.InvestorUnlocked(_p)) // tryb inwestora: stawka, dotknięcie = modyfikatory
        {
            var ir = new Rect2(x - 4, y, cw + 8, rowH);
            DrawStyleBox(Ui.Box(Pal.Group, 8), ir.Grow(-2));
            f.Draw(this, new Vector2(x + 6, Mathf.Round(ir.GetCenter().Y - 8)), "Tryb inwestora", Ink.Dim);
            f.Draw(this, new Vector2(x + cw - 6, Mathf.Round(ir.GetCenter().Y - 8)), InvestorLabel(), Ink.Brand, TextAlign.Right);
            _hits.Add((ir, ClassSelectHit.Investor, 0));
            _coach["investor"] = ir;
            y += rowH + 4;
        }
        y += 2;
        var perkText = k2 >= 0 ? RunMods.PerkLabel(Meta.KeepsakePerk(_d, _p, k2)) : "";
        var perks = UiText.Perks(_d, _p);
        var bottom = Note.Length > 0 ? Note : !unl ? LockedText(cls, false)
                   : (perkText.Length > 0 ? $"Pamiątka: {perkText}. " : "") + "Uprawnienia: " + (perks.Length > 0 ? perks : "brak - zdobywaj odznaki");
        foreach (var line in f.Wrap(bottom, cw))
        {
            if (y > r.End.Y - 18) break;
            f.Draw(this, new Vector2(x, y), line, Note.Length > 0 || !unl ? Ink.Late : Ink.Dim);
            y += 16;
        }
    }

    /// <summary>Stawka włączonych modyfikatorów i rekord zawodu, np. „stawka 5, rekord 3”.</summary>
    private string InvestorLabel() => $"stawka {Investor.Stake(_d, Meta.InvestorMask(_d, _p))}, rekord {Meta.BestStake(_p, Selected)}";

    /// <summary>Paski statystyk (bazowa w kolorze marki, premia Szkoleń na zielono); zwraca dół.</summary>
    private float DrawStats(float sx, float sy, float width, int cls, ClassDef c, in RunMods m, bool unl, float step)
    {
        var f = PixelFont.I;
        (string Label, int Base, int Bonus, int Max)[] stats =
        [
            ("HP", c.MaxHealth, m.Hp, _d.Classes.Max(k2 => k2.MaxHealth) + m.Hp),
            ("SIŁ", c.Strength, RunMods.StatBonus(_d, m, cls, Stat.Str), 10),
            ("ZRĘ", c.Agility, RunMods.StatBonus(_d, m, cls, Stat.Agi), 10),
            ("INT", c.Intelligence, RunMods.StatBonus(_d, m, cls, Stat.Intel), 10),
            ("OBR", c.Defense, m.Def, 6),
            ("SZCZ", c.Luck, m.Luck, 8),
        ];
        var bw = width - 90;
        InfoButton(new Vector2(sx + width - 18, sy - 1));
        for (var i = 0; i < stats.Length; i++)
        {
            var (label, b, bonus, max) = stats[i];
            max = Math.Max(max, b + bonus);
            var yy = sy + i * step;
            _statRects[i] = new Rect2(sx, yy, width, step);
            _hits.Add((_statRects[i], ClassSelectHit.Stat, i));
            if (i == ShownTip) DrawStyleBox(Ui.Box(new Color(Pal.Brand, 0.12f), 4), _statRects[i]);
            f.Draw(this, new Vector2(sx, yy), label, Ink.Dim);
            var bar = new Rect2(sx + 40, yy + 5, bw, 7);
            DrawStyleBox(Ui.Box(Pal.Group, 3), bar);
            var bwBase = Mathf.Round(bw * b / (float)max);
            var bwAll = Mathf.Round(bw * (b + bonus) / (float)max);
            if (bwAll > 0) DrawStyleBox(Ui.Box(Pal.Done, 3), new Rect2(bar.Position, new Vector2(Mathf.Max(bwAll, 4), 7)));
            if (bwBase > 0) DrawStyleBox(Ui.Box(unl ? Pal.Brand : Pal.Todo, 3), new Rect2(bar.Position, new Vector2(Mathf.Max(bwBase, 4), 7)));
            f.Draw(this, new Vector2(bar.End.X + 6, yy), UiText.StatText("", b, bonus).Trim(), bonus > 0 ? Ink.Done : Ink.Dark);
        }
        return sy + stats.Length * step;
    }

    private void DrawCard(Rect2 r)
    {
        var f = PixelFont.I;
        var cls = Selected;
        var c = _d.Classes[cls];
        var unl = Meta.ClassUnlocked(_d, _p, cls);
        var m = Meta.Mods(_d, _p);
        DrawStyleBox(Ui.Box(new Color(0, 0, 0, 0.35f), 10), new Rect2(r.Position + new Vector2(0, 3), r.Size));
        DrawStyleBox(Ui.Box(Pal.Card, 10), r);
        var x = r.Position.X + 14;
        var y = r.Position.Y + 8;

        // nazwa w 2x + opis (tekst karty w 1.5x - czytelny na dużym ekranie)
        const float ts = 1.5f;
        f.Draw(this, new Vector2(x, y), c.Name, Ink.Dark, TextAlign.Left, 2);
        y += 32;
        var colW = (int)(r.Size.X / 2 - 24);
        f.Draw(this, new Vector2(x, y), f.Fit(c.Desc, colW, ts), Ink.Dim, TextAlign.Left, ts);
        y += 26;

        // moc z ikoną
        DrawStyleBox(Ui.Box(Pal.Group, 6), new Rect2(x, y + 4, 36, 36));
        Assets.DrawFrame(this, Assets.AbilityIcons, cls, 32, new Vector2(x + 2, y + 6));
        f.Draw(this, new Vector2(x + 44, y), f.Fit($"Moc (R): {c.AbilityName}", colW - 44, ts), Ink.Brand, TextAlign.Left, ts);
        f.Draw(this, new Vector2(x + 44, y + 22), f.Fit(c.AbilityDesc, colW - 44, ts), Ink.Dim, TextAlign.Left, ts);
        y += 48;

        // narzędzie ze statystyką
        var wpn = _d.Weapons[c.Weapon];
        f.Draw(this, new Vector2(x, y), f.Fit($"{wpn.Name} {wpn.MinDamage}-{wpn.MaxDamage}, zasięg {wpn.Range}", colW - 40, ts), Ink.Dark, TextAlign.Left, ts);
        var tag = UiText.StatShort(wpn.ScalesWith);
        var tw = f.Measure(tag) + 10;
        DrawStyleBox(Ui.Box(Pal.Group, 7), new Rect2(x + colW - tw, y + 5, tw, 14));
        f.Draw(this, new Vector2(x + colW - tw / 2f, y + 3), tag, Ink.Brand, TextAlign.Center);

        // trudność i pamiątka
        y = r.Position.Y + 140;
        DrawRect(new Rect2(x, y, r.Size.X - 28, 1), Pal.Border);
        y += 6;
        var diff = _d.Difficulties[Difficulty];
        var diffLock = Meta.DifficultyUnlocked(_d, _p, Difficulty) ? "" : " (zablok.)";
        f.Draw(this, new Vector2(x, y), "Trudność:", Ink.Dim);
        _coach["difficulty"] = new Rect2(x - 4, y - 1, r.Size.X / 2 - 16, 19);
        f.Draw(this, new Vector2(x + 64, y), $"< {diff.Name}{diffLock} >", diffLock.Length > 0 ? Ink.Late : Ink.Dark);
        if (Meta.InvestorUnlocked(_p)) // tryb inwestora (Tab) w tym samym wierszu
        {
            var ix = r.Position.X + r.Size.X / 2 + 10;
            f.Draw(this, new Vector2(ix, y), "Inwestor:", Ink.Dim);
            _coach["investor"] = new Rect2(ix - 4, y - 1, r.End.X - ix - 6, 19);
            f.Draw(this, new Vector2(ix + 62, y), f.Fit($"{InvestorLabel()} (Tab)", (int)(r.End.X - ix - 76)), Ink.Brand);
        }
        var k = Meta.SelectedKeepsake(_d, _p);
        var keep = k < 0 ? "bez pamiątki" : $"{_d.Keepsakes[k].Name} {UiText.Roman(Meta.KeepsakeRank(_d, _p, k) - 1)}: {RunMods.PerkLabel(Meta.KeepsakePerk(_d, _p, k))}";
        y += 18;
        f.Draw(this, new Vector2(x, y), "Pamiątka:", Ink.Dim);
        _coach["keepsake"] = new Rect2(x - 4, y - 1, r.Size.X - 20, 19);
        f.Draw(this, new Vector2(x + 64, y), f.Fit(keep, (int)r.Size.X - 92), k < 0 ? Ink.Dim : Ink.Done);
        y += 18;
        var perks = UiText.Perks(_d, _p);
        var bottom = Note.Length > 0 ? Note : !unl ? LockedText(cls, true) : "Uprawnienia: " + (perks.Length > 0 ? perks : "brak - zdobywaj odznaki");
        f.Draw(this, new Vector2(x, y), f.Fit(bottom, (int)r.Size.X - 28), Note.Length > 0 || !unl ? Ink.Late : Ink.Dim);

        // statystyki jako paski (prawa kolumna)
        var sx = r.Position.X + r.Size.X / 2 + 10;
        var sy = r.Position.Y + 36;
        (string Label, int Base, int Bonus, int Max)[] stats =
        [
            ("HP", c.MaxHealth, m.Hp, _d.Classes.Max(k2 => k2.MaxHealth) + m.Hp),
            ("SIŁ", c.Strength, RunMods.StatBonus(_d, m, cls, Stat.Str), 10),
            ("ZRĘ", c.Agility, RunMods.StatBonus(_d, m, cls, Stat.Agi), 10),
            ("INT", c.Intelligence, RunMods.StatBonus(_d, m, cls, Stat.Intel), 10),
            ("OBR", c.Defense, m.Def, 6),
            ("SZCZ", c.Luck, m.Luck, 8),
        ];
        var bw = r.Size.X / 2 - 110;
        InfoButton(new Vector2(r.End.X - 30, r.Position.Y + 8));
        _coach["stats"] = new Rect2(sx - 6, r.Position.Y + 6, r.End.X - sx, sy - r.Position.Y + 6 * 17f + 2);   // z przyciskiem „i”
        for (var i = 0; i < stats.Length; i++)
        {
            var (label, b, bonus, max) = stats[i];
            max = Math.Max(max, b + bonus);
            var yy = sy + i * 17f;
            _statRects[i] = new Rect2(sx - 4, yy, r.End.X - sx - 10, 17);
            _hits.Add((_statRects[i], ClassSelectHit.Stat, i));
            if (i == ShownTip) DrawStyleBox(Ui.Box(new Color(Pal.Brand, 0.12f), 4), _statRects[i]);
            f.Draw(this, new Vector2(sx, yy), label, Ink.Dim);
            var bar = new Rect2(sx + 40, yy + 5, bw, 7);
            DrawStyleBox(Ui.Box(Pal.Group, 3), bar);
            var bwBase = Mathf.Round(bw * b / (float)max);
            var bwAll = Mathf.Round(bw * (b + bonus) / (float)max);
            if (bwAll > 0) DrawStyleBox(Ui.Box(Pal.Done, 3), new Rect2(bar.Position, new Vector2(Mathf.Max(bwAll, 4), 7)));
            if (bwBase > 0) DrawStyleBox(Ui.Box(unl ? Pal.Brand : Pal.Todo, 3), new Rect2(bar.Position, new Vector2(Mathf.Max(bwBase, 4), 7)));
            f.Draw(this, new Vector2(bar.End.X + 6, yy), UiText.StatText("", b, bonus).Trim(), bonus > 0 ? Ink.Done : Ink.Dark);
        }
    }

    private int ShownTip => TipStat >= 0 ? TipStat : _hoverStat;

    /// <summary>Przycisk „i” (menu_icons 23): strona opisu statystyk.</summary>
    private void InfoButton(Vector2 pos)
    {
        var r = new Rect2(pos, new Vector2(20, 20));
        Assets.DrawFrame(this, Assets.UiMenu, Assets.MenuStats, Assets.Icon, pos + new Vector2(2, 2));
        _hits.Add((r.Grow(8), ClassSelectHit.Stats, 0));
    }

    /// <summary>Dymek nad wierszem statystyki: wartość i co daje (StatHelp.Effect) oraz ogólny wzór (StatHelp.Rule).</summary>
    private void DrawStatTip()
    {
        var i = ShownTip;
        if (i < 0 || i >= _statRects.Length || _statRects[i].Size.X <= 0) return;
        var f = PixelFont.I;
        var cls = Selected;
        var c = _d.Classes[cls];
        var m = Meta.Mods(_d, _p);
        var ws = _d.Weapons[c.Weapon].ScalesWith;
        StatKind[] kinds = [StatKind.Hp, StatKind.Str, StatKind.Agi, StatKind.Intel, StatKind.Def, StatKind.Luck];
        int[] values =
        [
            c.MaxHealth + m.Hp, c.Strength + RunMods.StatBonus(_d, m, cls, Stat.Str), c.Agility + RunMods.StatBonus(_d, m, cls, Stat.Agi),
            c.Intelligence + RunMods.StatBonus(_d, m, cls, Stat.Intel), c.Defense + m.Def, c.Luck + m.Luck,
        ];
        var weapon = (i == 1 && ws == Stat.Str) || (i == 2 && ws == Stat.Agi) || (i == 3 && ws == Stat.Intel);
        var l1 = StatHelp.Effect(_d, new Message().Add(StatHelp.Name(kinds[i])).Add(" ").Add(values[i]).Add(": "), kinds[i], values[i], weapon).Text;
        var rule = StatHelp.Rule(_d, new Message(), kinds[i]).Text;
        var l2 = kinds[i] == StatKind.Luck ? rule + ", " + StatHelp.Rule(_d, new Message(), kinds[i], 1).Text : rule;
        var tw = Mathf.Min(Size.X - 16, Mathf.Max(f.Measure(l1), f.Measure(l2)) + 20);
        var row = _statRects[i];
        var x = Mathf.Clamp(row.Position.X + row.Size.X - tw, 8, Size.X - tw - 8);
        var y = row.Position.Y - 44 < 4 ? row.End.Y + 4 : row.Position.Y - 44;
        var box = new Rect2(x, y, tw, 40);
        DrawStyleBox(Ui.Box(new Color(0, 0, 0, 0.3f), 8), new Rect2(box.Position + new Vector2(0, 2), box.Size));
        DrawStyleBox(Ui.Box(Pal.Text, 8, Pal.Brand), box);
        f.Draw(this, new Vector2(x + 10, y + 3), f.Fit(l1, (int)tw - 20), weapon ? Ink.MapLoot : Ink.Map);
        f.Draw(this, new Vector2(x + 10, y + 20), f.Fit(l2, (int)tw - 20), Ink.MapDim);
    }

    // Zawód z nagrody za odbiór: numer wygranej, która go odblokuje (-1 = brak na liście).
    private int RewardWinOf(int cls)
    {
        for (var i = 0; i < _d.Rewards.Length; i++)
            if (_d.Rewards[i].Kind == RewardKind.Cls && _d.Rewards[i].Index == cls) return Meta.RewardWin(_d, _p, i);
        return -1;
    }

    private string LockedShort(int cls) => Meta.ClassReward(_d, cls) ? $"{RewardWinOf(cls)}. wygr." : $"{_d.ClassCost} dośw.";

    private string LockedText(int cls, bool keys) => Meta.ClassReward(_d, cls)
        ? $"Nagroda za odbiór budowy: za {RewardWinOf(cls)}. wygraną, masz {_p.Wins}"
        : $"Zablokowany: {_d.ClassCost} dośw. w Szkoleniach" + (keys ? " (K)" : "");
}
