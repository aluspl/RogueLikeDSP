using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;

namespace LifeLike.Game;

/// <summary>
/// Wybór zawodu (run_class_select na GBA, nowy układ): u góry karuzela portretów wszystkich zawodów - odblokowane
/// najpierw, zablokowane po nich (kolejność z danych w każdej grupie); wybrany większy, z animacją i ramką w fiolecie
/// marki, zablokowane jako sylwetki z kłódką i ceną. Pod spodem karta: nazwa, moc z ikoną i opisem, narzędzie ze
/// statystyką, paski statystyk z premią Szkoleń, trudność (↑/↓) i pamiątka (Q/E), uprawnienia z odznak.
/// </summary>
public partial class ClassSelectScreen : Control
{
    private GameData _d;
    private Profile _p;
    private int[] _order = [];
    private int _pos;
    private float _clock;
    private float _slide;      // pozycja ramki wyboru (płynnie za _pos)
    private float _cardShift;  // wjazd karty przy zmianie zawodu
    private readonly Dictionary<int, float> _scale = new();

    public int Difficulty { get; set; }
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
        _order = all.Where(i => Meta.ClassUnlocked(p, i)).Concat(all.Where(i => !Meta.ClassUnlocked(p, i))).ToArray();
        _pos = Math.Max(0, Array.IndexOf(_order, cls));
        _slide = _pos;
        foreach (var i in all) _scale[i] = i == Selected ? 2f : 1f;
        QueueRedraw();
    }

    public void Move(int d)
    {
        if (_order.Length == 0) return;
        _pos = (_pos + d + _order.Length) % _order.Length;
        _cardShift = d * 18f;
        Sfx.Play("menu");
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
        QueueRedraw();
    }

    private float SlotX(float pos) => Size.X / 2 + (pos - (_order.Length - 1) / 2f) * 76;

    public override void _Draw()
    {
        try
        {
            DrawContent();
        }
        catch (System.Exception ex)
        {
            DrawErrors.Record("ClassSelectScreen", ex);
        }
    }

    private void DrawContent()
    {
        if (_d is null) return;
        var f = PixelFont.I;
        var w = Size.X;
        var h = Size.Y;
        DrawRect(new Rect2(0, 0, w, h), Pal.ScreenBg);
        Ui.VioletGradient(this, new Rect2(0, 0, w, 112));
        DrawRect(new Rect2(0, 112, w, 2), Pal.Accent);
        Ui.WarningStripe(this, new Rect2(0, h - 8, w, 8), _clock * 10f);

        f.Draw(this, new Vector2(w / 2, 3), "Wybierz zawód", Ink.OnBrand, TextAlign.Center);
        f.Draw(this, new Vector2(w / 2 + 1, 3), "Wybierz zawód", Ink.OnBrand, TextAlign.Center);

        // karuzela portretów
        const float baseY = 70;
        var fx = SlotX(_slide);
        DrawStyleBox(Ui.Box(new Color(0, 0, 0, 0.25f), 10), new Rect2(fx - 38, baseY - 50, 76, 84));
        DrawStyleBox(Ui.Box(new Color(Pal.Card, 0.18f), 10, Pal.Card), new Rect2(fx - 38, baseY - 52, 76, 84));
        for (var k = 0; k < _order.Length; k++)
        {
            var cls = _order[k];
            var unl = Meta.ClassUnlocked(_p, cls);
            var sc = _scale[cls];
            var sel = k == _pos;
            var x = SlotX(k);
            var anim = sel && unl && ((int)(_clock / 0.4f) & 1) == 1;
            var bob = sel ? Mathf.Round(Mathf.Sin(_clock * 4f) * 1.5f) : 0;
            var frame = unl ? _d.Classes[cls].Frame + (anim ? Assets.FrameAnimB : 0) : Assets.FrameSilhouette + cls;
            var size = 32 * sc;
            var feet = baseY + 14;
            DrawTextureRect(Assets.Shadow, new Rect2(x - 12 * sc / 1.4f, feet - 3, 24 * sc / 1.4f, 7), false);
            var dst = new Rect2(Mathf.Round(x - size / 2), Mathf.Round(feet - size + 2 * sc + bob), Mathf.Round(size), Mathf.Round(size));
            DrawTextureRectRegion(Assets.Actors, dst, Assets.Frame(frame, Assets.Actor), unl ? Colors.White : new Color(1, 1, 1, sel ? 1f : 0.75f));
            if (!unl)
            {
                DrawTextureRectRegion(Assets.Actors, new Rect2(Mathf.Round(x + size / 2 - 18), Mathf.Round(feet - 18), 16, 16), Assets.Frame(Assets.FrameLock, Assets.Actor));
                f.Draw(this, new Vector2(x, feet + 6), $"{_d.ClassCost} dośw.", Ink.MapDim, TextAlign.Center);
            }
            else if (!sel)
            {
                f.Draw(this, new Vector2(x, feet + 6), f.Fit(_d.Classes[cls].Name.Split(' ')[0], 70), Ink.MapDim, TextAlign.Center);
            }
        }
        f.Draw(this, new Vector2(SlotX(0) - 50, baseY - 10), "<", Ink.OnBrand, TextAlign.Center);
        f.Draw(this, new Vector2(SlotX(_order.Length - 1) + 50, baseY - 10), ">", Ink.OnBrand, TextAlign.Center);

        DrawCard(new Rect2(40 + _cardShift, 122, w - 80, 204));
        f.Draw(this, new Vector2(w / 2, h - 26), "Strzałki: zawód / trudność   Q/E: pamiątka   Enter: start   Esc: wróć", Ink.MapDim, TextAlign.Center);
    }

    private void DrawCard(Rect2 r)
    {
        var f = PixelFont.I;
        var cls = Selected;
        var c = _d.Classes[cls];
        var unl = Meta.ClassUnlocked(_p, cls);
        var m = Meta.Mods(_d, _p);
        DrawStyleBox(Ui.Box(new Color(0, 0, 0, 0.35f), 10), new Rect2(r.Position + new Vector2(0, 3), r.Size));
        DrawStyleBox(Ui.Box(Pal.Card, 10), r);
        var x = r.Position.X + 14;
        var y = r.Position.Y + 8;

        // nazwa w 2x + opis
        f.Draw(this, new Vector2(x, y), c.Name, Ink.Dark, TextAlign.Left, 2);
        y += 32;
        var colW = (int)(r.Size.X / 2 - 24);
        f.Draw(this, new Vector2(x, y), f.Fit(c.Desc, colW), Ink.Dim);
        y += 20;

        // moc z ikoną
        DrawStyleBox(Ui.Box(Pal.Group, 6), new Rect2(x, y, 36, 36));
        Assets.DrawFrame(this, Assets.AbilityIcons, cls, 32, new Vector2(x + 2, y + 2));
        f.Draw(this, new Vector2(x + 44, y), $"Moc (R): {c.AbilityName}", Ink.Brand);
        f.Draw(this, new Vector2(x + 44, y + 17), f.Fit(c.AbilityDesc, colW - 44), Ink.Dim);
        y += 40;

        // narzędzie ze statystyką
        var wpn = _d.Weapons[c.Weapon];
        f.Draw(this, new Vector2(x, y), f.Fit($"{wpn.Name} {wpn.MinDamage}-{wpn.MaxDamage}, zasięg {wpn.Range}", colW - 40), Ink.Dark);
        var tag = UiText.StatShort(wpn.ScalesWith);
        var tw = f.Measure(tag) + 10;
        DrawStyleBox(Ui.Box(Pal.Group, 7), new Rect2(x + colW - tw, y + 2, tw, 14));
        f.Draw(this, new Vector2(x + colW - tw / 2f, y), tag, Ink.Brand, TextAlign.Center);

        // trudność i pamiątka
        y = r.Position.Y + 140;
        DrawRect(new Rect2(x, y, r.Size.X - 28, 1), Pal.Border);
        y += 6;
        var diff = _d.Difficulties[Difficulty];
        var diffLock = Meta.DifficultyUnlocked(_d, _p, Difficulty) ? "" : " (zablok.)";
        f.Draw(this, new Vector2(x, y), "Trudność:", Ink.Dim);
        f.Draw(this, new Vector2(x + 64, y), $"< {diff.Name}{diffLock} >", diffLock.Length > 0 ? Ink.Late : Ink.Dark);
        var k = Meta.SelectedKeepsake(_d, _p);
        var keep = k < 0 ? "bez pamiątki" : $"{_d.Keepsakes[k].Name} {UiText.Roman(Meta.KeepsakeRank(_d, _p, k) - 1)}: {RunMods.PerkLabel(Meta.KeepsakePerk(_d, _p, k))}";
        f.Draw(this, new Vector2(x + colW + 24, y), "Q/E: pamiątka", Ink.Dim);
        y += 18;
        f.Draw(this, new Vector2(x, y), "Pamiątka:", Ink.Dim);
        f.Draw(this, new Vector2(x + 64, y), f.Fit(keep, (int)r.Size.X - 92), k < 0 ? Ink.Dim : Ink.Done);
        y += 18;
        var perks = UiText.Perks(_d, _p);
        var bottom = Note.Length > 0 ? Note : !unl ? $"Zablokowany: {_d.ClassCost} dośw. w Szkoleniach (K)" : "Uprawnienia: " + (perks.Length > 0 ? perks : "brak - zdobywaj odznaki");
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
        for (var i = 0; i < stats.Length; i++)
        {
            var (label, b, bonus, max) = stats[i];
            max = Math.Max(max, b + bonus);
            var yy = sy + i * 17f;
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
}
