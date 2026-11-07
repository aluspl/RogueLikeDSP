using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Phone.Pages;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;

namespace LifeLike.Game.Screens.Views;

/// <summary>
/// Wybór zawodu (run_class_select na GBA). v0.22.0 (UX A): siatka kafli wszystkich zawodów z pełnymi nazwami –
/// odblokowane najpierw, zablokowane po nich jako sylwetki z kłódką i warunkiem; wybrany z animacją i ramką w fiolecie
/// marki. Pion: krótka karta pod siatką, „Szczegóły” = pełna karta; poziom: siatka po lewej, pełna karta po prawej.
/// Karta: moc i broń jako osobne bloki, statystyki słowami, trudność (↑/↓), pamiątka (Q/E), tryb inwestora.
/// </summary>
public partial class ClassSelectView : Control, Touch.ITapTargets
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
    /// <summary>
    /// Dymek z opisem statystyki (dotknięcie wiersza; na komputerze też najechanie myszą); -1 = brak,
    /// WeaponTip = rozpiska obrażeń broni (#26) nad wierszem narzędzia.
    /// </summary>
    public int TipStat { get; set; } = -1;
    public const int WeaponTip = 6;
    private int _hoverStat = -1;
    private readonly Rect2[] _statRects = new Rect2[7];
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

    public void TapTargets(List<Rect2> into)
    {
        foreach (var (r, _, _) in _hits) into.Add(r);
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
        if (_d != null) HelmetTint.Apply(this, _d, _p); // v0.21.52: kolor kasku na portretach (klatki do pokolorowania)
        QueueRedraw();
    }

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
        for (var k = 0; k < _statRects.Length; k++) _statRects[k] = new Rect2();
        var f = PixelFont.I;
        var w = Size.X;
        var h = Size.Y;
        var portrait = Layout.Portrait; // v0.22.0 (UX A): także duży ekran prawie kwadratowy
        var top = Layout.SafeTop;
        var bar = top + (portrait ? 34 : 24);
        DrawRect(new Rect2(0, 0, w, h), Pal.ScreenBg);
        Ui.VioletGradient(this, new Rect2(0, 0, w, bar));
        DrawRect(new Rect2(0, bar, w, 2), Pal.Accent);
        Ui.WarningStripe(this, new Rect2(0, h - 8, w, 8), _clock * 10f);
        f.Draw(this, new Vector2(w / 2, top + (portrait ? 9 : 4)), Loc.T("wybierz_zawod"), Ink.OnBrand, TextAlign.Center, 1, true);
        var unl = Meta.ClassUnlocked(_d, _p, Selected);
        var startLabel = unl ? Loc.T("start_budowy") : LockCondition(Selected);

        if (portrait)
        {
            // v0.22.0 (UX A): pion – siatka kafli (składak: 4 szerokie kolumny), krótka karta, „Szczegóły” = pełna karta
            var bh = 52f;
            var by = h - Mathf.Max(Layout.SafeBottom, 8) - bh - 6;
            if (Details)
            {
                DrawPanel(new Rect2(12 + _cardShift, bar + 10, w - 24, by - bar - 20), true);
                var bw0 = (w - 36) / 3;
                Button(new Rect2(12, by, bw0, bh), Loc.T("wroc"), false, ClassSelectHit.Details);
                Button(new Rect2(24 + bw0, by, w - 36 - bw0, bh), startLabel, unl, ClassSelectHit.Start, !unl);
                _coach["go"] = new Rect2(24 + bw0, by, w - 36 - bw0, bh);
                DrawStatTip();
                return;
            }
            var cols = Layout.Wide ? 4 : 3;
            var rows = (_order.Length + cols - 1) / cols;
            var cardH = ShortCardH(w - 24);
            var gridTop = bar + 10;
            var tileH = Mathf.Clamp((by - 12 - cardH - 8 - gridTop) / rows - 6, 52, 104);
            var gridH = rows * (tileH + 6) - 6;
            DrawGrid(new Rect2(12, gridTop, w - 24, gridH), cols, tileH);
            DrawPanel(new Rect2(12 + _cardShift, gridTop + gridH + 10, w - 24, by - 12 - (gridTop + gridH + 10)), false);
            var bw = (w - 48) / 4;
            Button(new Rect2(12, by, bw, bh), Loc.T("wroc"), false, ClassSelectHit.Back);
            Button(new Rect2(24 + bw, by, bw * 1.2f, bh), Loc.T("szczegoly"), false, ClassSelectHit.Details);
            var sr = new Rect2(36 + bw * 2.2f, by, w - 48 - bw * 2.2f, bh);
            Button(sr, startLabel, unl, ClassSelectHit.Start, !unl);
            _coach["go"] = sr;
            _coach["stats"] = _coach["investor"] = new Rect2(24 + bw, by, bw * 1.2f, bh); // statystyki i inwestor w Szczegółach
            DrawStatTip();
            return;
        }

        // poziom (komputer): siatka 3 kolumny po lewej, pełna karta po prawej
        var foot = h - 34;
        var gw = Mathf.Min(320f, w * 0.48f);
        var rows3 = (_order.Length + 2) / 3;
        var th = Mathf.Clamp((foot - bar - 14) / rows3 - 4, 44, 96);
        DrawGrid(new Rect2(10, bar + 8, gw, rows3 * (th + 4) - 4), 3, th);
        DrawPanel(new Rect2(gw + 20 + _cardShift, bar + 8, w - gw - 30, foot - bar - 14), true);
        DrawStatTip();
        if (Layout.Touch)
        {
            Button(new Rect2(10, h - 30, 120, 24), Loc.T("wroc"), false, ClassSelectHit.Back);
            Button(new Rect2(w - 170, h - 30, 160, 24), startLabel, unl, ClassSelectHit.Start, !unl);
            _coach["go"] = new Rect2(w - 170, h - 30, 160, 24);
        }
        else
        {
            _coach["go"] = new Rect2(10, h - 30, w - 20, 22);
            f.Draw(this, new Vector2(w / 2, h - 26), Loc.T("strzalki_zawod_trudnosc_q_e"), Ink.Dim, TextAlign.Center);
        }
    }

    /// <summary>v0.22.0 (UX A): pełna karta w pionie (przycisk „Szczegóły”).</summary>
    public bool Details { get; set; }

    /// <summary>v0.22.0 (UX A): siatka kafli – portret, pełna nazwa (do 2 wierszy), zablokowany: sylwetka, kłódka, warunek.</summary>
    private void DrawGrid(Rect2 area, int cols, float tileH)
    {
        var f = PixelFont.I;
        var tw = (area.Size.X - (cols - 1) * 6) / cols;
        for (var k = 0; k < _order.Length; k++)
        {
            var cls = _order[k];
            var unl = Meta.ClassUnlocked(_d, _p, cls);
            var sel = k == _pos;
            var r = new Rect2(area.Position.X + (k % cols) * (tw + 6), area.Position.Y + (k / cols) * (tileH + 6), tw, tileH);
            DrawStyleBox(Ui.Box(sel ? Pal.Card.Lerp(Pal.Brand, 0.12f) : Pal.Card, 10, sel ? Pal.Brand : Pal.Border), r);
            if (sel) DrawStyleBox(Ui.Box(Colors.Transparent, 10, Pal.Brand), r.Grow(1));
            var lines = unl ? NameLines(_d.Classes[cls].Name, (int)tw - 8) : [LockedShort(cls)];
            if (lines.Count > 1 && tileH < 32 + 32 + 4) lines = [f.Fit(_d.Classes[cls].Name, (int)tw - 8)]; // bardzo niski kafel
            var textH = lines.Count * 16;
            var sc = tileH - textH - 8 >= 64 ? 2 : 1;
            var size = 32 * sc;
            var anim = sel && unl && ((int)(_clock / 0.4f) & 1) == 1;
            var hf = Assets.HeroFrame(_d, _p, cls); // kask w paski (wygląd z sekretnego zlecenia)
            var frame = unl ? (anim ? Assets.AnimB(hf) : hf) : Assets.Silhouette(cls);
            var py = r.Position.Y + Mathf.Max(2, (tileH - textH - size - 4) / 2);
            var dst = new Rect2(Mathf.Round(r.GetCenter().X - size / 2f), Mathf.Round(py), size, size);
            DrawTextureRectRegion(Assets.Actors, dst, Assets.Frame(frame, Assets.Actor), unl ? Colors.White : new Color(1, 1, 1, 0.8f));
            if (!unl) DrawTextureRectRegion(Assets.Actors, new Rect2(dst.End.X - 14, dst.End.Y - 16, 16, 16), Assets.Frame(Assets.FrameLock, Assets.Actor));
            var ty = r.End.Y - 4 - textH;
            for (var l = 0; l < lines.Count; l++)
                f.Draw(this, new Vector2(r.GetCenter().X, ty + l * 16), lines[l], unl ? (sel ? Ink.Brand : Ink.Dark) : Ink.Dim, TextAlign.Center, 1, sel && unl);
            _hits.Add((r, ClassSelectHit.Portrait, k));
        }
        _coach["class"] = area.Grow(4);
    }

    /// <summary>Pełna nazwa zawodu w najwyżej 2 wierszach (podział po spacji albo po łączniku), bez wielokropka, gdy się mieści.</summary>
    private static List<string> NameLines(string name, int width)
    {
        var f = PixelFont.I;
        if (f.Measure(name, 1, true) <= width) return [name];
        var cut = name.LastIndexOf(' ', name.Length - 1);
        var dash = name.IndexOf('-');
        var at = cut > 0 ? cut : dash > 0 ? dash + 1 : -1;
        if (at < 0) return [f.Fit(name, width)];
        var a = name[..at].TrimEnd();
        var b = name[at..].TrimStart();
        return [f.Fit(a, width), f.Fit(b, width)];
    }

    /// <summary>Warunek odblokowania na szarym przycisku startu: „Za 5. wygraną”, „Szkolenia: 160 dośw.”, „Sekretne zlecenie”.</summary>
    private string LockCondition(int cls) => Meta.ClassSecret(_d, cls) ? Loc.T("sekretne_zlecenie_3")
        : Meta.ClassReward(_d, cls) ? Loc.F("za_wygrana_nr", RewardWinOf(cls)) : Loc.F("za_dosw_szkolenia", Meta.ClassCost(_d, _p));

    private void Button(Rect2 r, string label, bool primary, ClassSelectHit hit, bool gray = false)
    {
        var f = PixelFont.I;
        DrawStyleBox(Ui.Box(gray ? Pal.Group : primary ? Pal.Brand : Pal.Card, 10, gray ? Pal.Border : primary ? Pal.Accent : Pal.Border), r);
        f.Draw(this, new Vector2(r.GetCenter().X, Mathf.Round(r.GetCenter().Y - 9)), f.Fit(label, (int)r.Size.X - 8), gray ? Ink.Dim : primary ? Ink.White : Ink.Brand, TextAlign.Center, 1, true);
        _hits.Add((r, hit, 0));
    }

    private static float RowH => Layout.Touch ? Layout.TouchTarget : 20f;

    private static float StatStep => Layout.Touch && Layout.Portrait ? 22f : 15f;

    /// <summary>Poziom (komputer, 360 px wysokości): ciaśniejsza karta, żeby zmieściły się pamiątka i inwestor.</summary>
    private static bool Tight => !Layout.Portrait;

    /// <summary>Wysokość krótkiej karty (pion): nazwa, opis, moc, broń, trudność, pamiątka, linia notatki.</summary>
    private static float ShortCardH(float w) => 8 + 34 + 16 + 6 + 44 + 6 + 26 + 6 + 2 * (RowH + 4) + 18 + 6;

    /// <summary>
    /// v0.22.0 (UX A): karta zawodu – nazwa, opis, moc i broń jako osobne bloki; pełna (full) dokłada statystyki słowami
    /// („Siła 5 · +2 obrażeń broni”), tryb inwestora i uprawnienia. Trudność i pamiątka jako wiersze do dotknięcia.
    /// </summary>
    private void DrawPanel(Rect2 r, bool full)
    {
        var f = PixelFont.I;
        var cls = Selected;
        var c = _d.Classes[cls];
        var unl = Meta.ClassUnlocked(_d, _p, cls);
        var m = Meta.Mods(_d, _p, cls); // v0.21.52 cz. b: mistrzostwo zawodu (broń mistrza)
        DrawStyleBox(Ui.Box(new Color(0, 0, 0, 0.18f), 10), new Rect2(r.Position + new Vector2(0, 3), r.Size));
        DrawStyleBox(Ui.Box(Pal.Card, 10, Pal.Border), r);
        var x = r.Position.X + 12;
        var cw = (int)r.Size.X - 24;
        var y = r.Position.Y + 8;
        var badge = unl && Progress.MasteryXp(_p, cls) > 0 ? 84 : 0;
        var gap = Tight ? 4 : 6;
        var ns = !Tight && f.Measure(c.Name, 2) <= cw - badge ? 2f : f.Measure(c.Name, 1.5f) <= cw - badge ? 1.5f : 1f;
        f.Draw(this, new Vector2(x, y + (Tight ? 0 : (2 - ns) * 8)), c.Name, Ink.Dark, TextAlign.Left, ns);
        if (unl) MasteryBadge(new Vector2(x + cw, y + 2), cls);
        y += Tight ? 26 : 34;
        var desc = f.Wrap(c.Desc, cw);
        var dl = full && !Tight ? Mathf.Min(desc.Count, 2) : 1;
        for (var k = 0; k < dl; k++) f.Draw(this, new Vector2(x, y + k * 16), dl == 1 ? f.Fit(c.Desc, cw) : desc[k], Ink.Dim);
        y += dl * 16 + gap;

        // moc: osobny blok z ikoną
        var variant = unl && Progress.PowerVariantOn(_d, _p, cls); // v0.21.52 cz. b: wariant mocy z mistrzostwa
        var mcd = _d.MasteryClasses[cls];
        var ad = f.Wrap(variant ? mcd.PowerDesc : c.AbilityDesc, cw - 52);
        var pl = full && !Tight ? Mathf.Min(ad.Count, 2) : 1;
        var pr = new Rect2(x - 4, y, cw + 8, Mathf.Max(Tight ? 40 : 44, 22 + pl * 16));
        DrawStyleBox(Ui.Box(Pal.Group, 8), pr);
        Assets.DrawFrame(this, Assets.AbilityIcons, cls, 32, new Vector2(x + 2, pr.Position.Y + (pr.Size.Y - 32) / 2));
        f.Draw(this, new Vector2(x + 42, pr.Position.Y + 4), f.Fit(Loc.F("moc_7", variant ? mcd.PowerName : c.AbilityName), cw - 46), Ink.Brand, TextAlign.Left, 1, true);
        for (var k = 0; k < pl; k++) f.Draw(this, new Vector2(x + 42, pr.Position.Y + 21 + k * 16), pl == 1 ? f.Fit(variant ? mcd.PowerDesc : c.AbilityDesc, cw - 46) : ad[k], Ink.Dim);
        y = pr.End.Y + gap;

        // broń: osobny blok ze statystyką, od której zależy (dotknięcie = rozpiska obrażeń)
        var wpn = _d.Weapons[c.Weapon];
        var tag = $"{UiText.StatShort(wpn.ScalesWith)} · {WeaponLook.Hits(_d, wpn)}"; // v0.23.0: typ ciosu obok statystyki
        var tw = f.Measure(tag) + 10;
        var rarity = LifeLike.Core.Game.StartRarity(_d, m); // v0.23.0: rzadkość startowa (Szkolenie Narzędzia, broń mistrza – Mistrzowska)
        var wr = new Rect2(x - 4, y, cw + 8, full ? 42 : 26);
        DrawStyleBox(Ui.Box(Pal.Group, 8), wr);
        DrawRect(new Rect2(wr.Position.X + 2, wr.Position.Y + 5, 3, wr.Size.Y - 10), WeaponLook.RarityColor(_d, rarity)); // pasek koloru rzadkości
        WeaponRow(new Rect2(x, y + 1, cw - tw - 4, 24), cls, m, cw - tw - 14, 1);
        DrawStyleBox(Ui.Box(Pal.Card, 7), new Rect2(x + cw - tw, y + 6, tw, 14));
        f.Draw(this, new Vector2(x + cw - tw / 2f, y + 4), tag, Ink.Brand, TextAlign.Center);
        if (full) // v0.23.0: rzadkość i tryb B słowami (tryb A – w rozpisce pod dotknięciem)
        {
            var modes = $"{WeaponLook.RarityName(_d, rarity)} · B: {WeaponLook.Mode(_d, wpn, wpn.ModeB, true, WeaponLook.CdDelta(_d, rarity))}";
            var full2 = $"{WeaponLook.RarityName(_d, rarity)} · A: {WeaponLook.Mode(_d, wpn, wpn.ModeA, false)} · B: {WeaponLook.Mode(_d, wpn, wpn.ModeB, true, WeaponLook.CdDelta(_d, rarity))}";
            if (f.Measure(full2) <= cw - 8) modes = full2;
            f.Draw(this, new Vector2(x + 4, y + 22), f.Fit(modes, cw - 8), WeaponLook.PhoneInk(_d, rarity));
        }
        y = wr.End.Y + gap;

        if (full)
        {
            var statsTop = y;
            y = DrawStatWords(x, y, cw, cls, c, m, unl, StatStep) + gap;
            _coach["stats"] = new Rect2(x - 4, statsTop - 4, cw + 8, y - statsTop);
        }
        var rowH = RowH;
        var diff = _d.Difficulties[Difficulty];
        var diffLock = Meta.DifficultyUnlocked(_d, _p, Difficulty) ? "" : Loc.T("zablok_3");
        y = ChoiceRow(new Rect2(x - 4, y, cw + 8, rowH), Loc.T("trudnosc_2"), $"< {diff.Name}{diffLock} >", diffLock.Length > 0 ? Ink.Late : Ink.Brand, ClassSelectHit.Difficulty, "difficulty");
        var k2 = Meta.SelectedKeepsake(_d, _p);
        var keep = k2 < 0 ? Loc.T("bez_pamiatki") : $"{_d.Keepsakes[k2].Name} {UiText.Roman(Meta.KeepsakeRank(_d, _p, k2) - 1)}";
        if (y + rowH <= r.End.Y - 4) y = ChoiceRow(new Rect2(x - 4, y, cw + 8, rowH), Loc.T("pamiatka_3"), $"< {keep} >", k2 < 0 ? Ink.Dim : Ink.Done, ClassSelectHit.Keepsake, "keepsake");
        if (full && InvestorPage.Available(_d, _p, Selected) && y + rowH <= r.End.Y - 4) // tryb inwestora (v0.21.52: albo sam wygląd)
            y = ChoiceRow(new Rect2(x - 4, y, cw + 8, rowH), Meta.InvestorUnlocked(_p) ? Loc.T("tryb_inwestora") : Loc.T("wyglad"), InvestorLabel(), Ink.Brand, ClassSelectHit.Investor, "investor");
        var perkText = k2 >= 0 ? RunMods.PerkLabel(Meta.KeepsakePerk(_d, _p, k2)) : "";
        var perks = UiText.Perks(_d, _p);
        var bottom = Note.Length > 0 ? Note : !unl ? LockedText(cls, !Layout.Touch)
                   : !full ? "" : (perkText.Length > 0 ? Loc.F("pamiatka_4", perkText) : "") + Loc.T("uprawnienia") + (perks.Length > 0 ? perks : Loc.T("brak_zdobywaj_odznaki"));
        y += 2;
        foreach (var line in f.Wrap(bottom, cw))
        {
            if (y > r.End.Y - 18) break;
            f.Draw(this, new Vector2(x, y), line, Note.Length > 0 || !unl ? Ink.Late : Ink.Dim);
            y += 16;
        }
    }

    /// <summary>Wiersz wyboru (trudność, pamiątka, inwestor): etykieta po lewej, wartość po prawej; zwraca dół.</summary>
    private float ChoiceRow(Rect2 r, string label, string value, Ink ink, ClassSelectHit hit, string coach)
    {
        var f = PixelFont.I;
        DrawStyleBox(Ui.Box(Pal.Group, 8), r.Grow(-2));
        var ty = Mathf.Round(r.GetCenter().Y - 8);
        var lw = f.Measure(label);
        f.Draw(this, new Vector2(r.Position.X + 10, ty), label, Ink.Dim);
        f.Draw(this, new Vector2(r.End.X - 10, ty), f.Fit(value, (int)(r.Size.X - lw - 30)), ink, TextAlign.Right);
        _hits.Add((r, hit, 0));
        _coach[coach] = r;
        return r.End.Y + 4;
    }

    /// <summary>
    /// v0.21.52 cz. b (#45): „Mistrz N” z paskiem do kolejnego poziomu, prawy górny róg karty (topRight); bez dośw. – nic.
    /// </summary>
    private void MasteryBadge(Vector2 topRight, int cls)
    {
        if (Progress.MasteryXp(_p, cls) <= 0) return;
        var f = PixelFont.I;
        var lv = Progress.MasteryLevel(_d, _p, cls);
        Progress.MasteryBar(_d, _p, cls, out var cur, out var need);
        var label = need == 0 ? Loc.F("mistrz_max", lv) : Loc.F("mistrz_3", lv);
        var bw = Mathf.Max(72, f.Measure(label));
        f.Draw(this, topRight, label, need == 0 ? Ink.Done : Ink.Prog, TextAlign.Right);
        var bar = new Rect2(topRight.X - bw, topRight.Y + PixelFont.LineHeight + 2, bw, 5);
        DrawRect(bar, Pal.Border);
        DrawRect(new Rect2(bar.Position, new Vector2(Mathf.Round(bw * (need == 0 ? 1f : cur / (float)need)), 5)), need == 0 ? Pal.Done : Pal.Prog);
    }

    /// <summary>Stawka włączonych modyfikatorów i rekord zawodu, np. „stawka 5, rekord 3”.</summary>
    private string InvestorLabel()
    {
        if (!Meta.InvestorUnlocked(_p)) // v0.21.52: przed pierwszą wygraną – sam wygląd (kolor kasku)
        {
            var hk = Secrets.HelmetCosmetic(_d, _p, Selected);
            if (hk >= 0) return _d.Cosmetics[hk].Name;
            return Secrets.HelmetsUnlocked(_d, _p) > 0 ? Loc.T("kask_zawodu") : Loc.T("filtr") + _d.ScreenFilters[ScreenFilter.Resolve(_d, _p)].Name; // v0.21.53
        }
        return Loc.F("stawka_rekord", Investor.Stake(_d, Meta.InvestorMask(_d, _p)), Meta.BestStake(_p, Selected));
    }

    /// <summary>
    /// v0.22.0 (UX A): statystyki słowami – „Siła 5 · +2 obrażeń broni” (StatHelp.Effect: prawdziwy wzór), premia
    /// Szkoleń na zielono; cienki pasek jako tło wiersza. Zwraca dół.
    /// </summary>
    private float DrawStatWords(float sx, float sy, float width, int cls, ClassDef c, in RunMods m, bool unl, float step)
    {
        var f = PixelFont.I;
        (string Label, StatKind Kind, int Base, int Bonus, int Max)[] stats =
        [
            (Loc.T("stat_hp"), StatKind.Hp, c.MaxHealth, m.Hp, _d.Classes.Max(k2 => k2.MaxHealth) + m.Hp),
            (Loc.T("stat_str"), StatKind.Str, c.Strength, RunMods.StatBonus(_d, m, cls, Stat.Str), 10),
            (Loc.T("stat_agi"), StatKind.Agi, c.Agility, RunMods.StatBonus(_d, m, cls, Stat.Agi), 10),
            (Loc.T("stat_int"), StatKind.Intel, c.Intelligence, RunMods.StatBonus(_d, m, cls, Stat.Intel), 10),
            (Loc.T("stat_def"), StatKind.Def, c.Defense, m.Def, 6),
            (Loc.T("stat_luck"), StatKind.Luck, c.Luck, m.Luck, 8),
        ];
        var ws = _d.Weapons[c.Weapon].ScalesWith;
        var lw = 0f;
        foreach (var st in stats) lw = Mathf.Max(lw, f.Measure(st.Label));
        InfoButton(new Vector2(sx + width - 18, sy - 1));
        for (var i = 0; i < stats.Length; i++)
        {
            var (label, kind, b, bonus, max) = stats[i];
            max = Math.Max(max, b + bonus);
            var yy = sy + i * step;
            _statRects[i] = new Rect2(sx - 4, yy, width - 18, step);
            _hits.Add((_statRects[i], ClassSelectHit.Stat, i));
            if (i == ShownTip) DrawStyleBox(Ui.Box(new Color(Pal.Brand, 0.12f), 4), _statRects[i]);
            var bar = new Rect2(sx, yy + step - 3, (width - 24) * (b + bonus) / (float)max, 2);
            DrawRect(bar, new Color(unl ? Pal.Brand : Pal.Todo, 0.25f));
            var ty = yy + (step - 16) / 2;
            f.Draw(this, new Vector2(sx, ty), label, Ink.Dim);
            var val = UiText.StatText("", b, bonus).Trim();
            f.Draw(this, new Vector2(sx + lw + 8, ty), val, bonus > 0 ? Ink.Done : Ink.Dark, TextAlign.Left, 1, true);
            var weapon = (kind == StatKind.Str && ws == Stat.Str) || (kind == StatKind.Agi && ws == Stat.Agi) || (kind == StatKind.Intel && ws == Stat.Intel);
            var ex = sx + lw + 16 + f.Measure(val, 1, true);
            var eff = StatHelp.Effect(_d, new Message(), kind, b + bonus, weapon).Text;
            f.Draw(this, new Vector2(ex, ty), f.Fit(eff, (int)(sx + width - 24 - ex)), weapon ? Ink.Brand : Ink.Dim);
        }
        return sy + stats.Length * step;
    }

    private int ShownTip => TipStat >= 0 ? TipStat : _hoverStat;

    /// <summary>
    /// Wiersz narzędzia z zakresem ciosu i krytem jak w walce (rozpiska #26: „Kielnia 8-11, kryt 16-22 (21%)”, co się
    /// nie mieści – krócej); najechanie / dotknięcie = dymek z rozpiską.
    /// </summary>
    private void WeaponRow(Rect2 row, int cls, in RunMods m, int maxW, float scale)
    {
        var f = PixelFont.I;
        var b = DamageRows.ForHero(_d, _p, null, cls, m);
        var master = Meta.ClassUnlocked(_d, _p, cls) && Progress.MasteryHas(_d, _p, cls, ProgressReward.Weapon); // broń mistrza
        var name = $"{(master ? _d.MasteryClasses[cls].WeaponName : _d.Weapons[b.Weapon].Name)} {b.Min}-{b.Max}";
        string[] variants =
        [
            Loc.F("kryt_zasieg", name, b.CritMin, b.CritMax, b.CritChance(), b.Range),
            Loc.F("kryt_11", name, b.CritMin, b.CritMax, b.CritChance()),
            Loc.F("kr_linia", name, b.CritMin, b.CritMax, b.CritChance()),
        ];
        var text = "";
        foreach (var v in variants)
        {
            if (f.Measure(v, scale) > maxW) continue;
            text = v;
            break;
        }
        if (text.Length == 0 && scale > 1)   // w powiększeniu się nie mieści: zwykła wielkość, pełniejszy tekst
        {
            scale = 1;
            text = variants[^1];
            foreach (var v in variants)
            {
                if (f.Measure(v) > maxW) continue;
                text = v;
                break;
            }
        }
        if (text.Length == 0) text = variants[^1];
        _statRects[WeaponTip] = row;
        _hits.Add((row, ClassSelectHit.Stat, WeaponTip));
        if (ShownTip == WeaponTip) DrawStyleBox(Ui.Box(new Color(Pal.Brand, 0.12f), 4), row);
        var dy = scale < 1.5f && row.Size.Y > 20 ? 4 : 1;
        f.Draw(this, new Vector2(row.Position.X + 4, row.Position.Y + dy), f.Fit(text, maxW, scale), Ink.Dark, TextAlign.Left, scale);
    }

    /// <summary>Dymek z rozpiską obrażeń broni zawodu (wiersze jak strona Obrażenia w telefonie).</summary>
    private void DrawWeaponTip(Rect2 row)
    {
        var f = PixelFont.I;
        var cls = Selected;
        var rows = DamageRows.Build(_d, DamageRows.ForHero(_d, _p, null, cls, Meta.Mods(_d, _p, cls)), null);
        var maxW = (int)Size.X - 36;
        var lines = new List<(string Text, Ink Ink)>();
        foreach (var r in rows)
        {
            var wrapped = f.Wrap(r.Text, maxW);
            for (var k = 0; k < wrapped.Count; k++) lines.Add(((k > 0 ? "  " : "") + wrapped[k], r.Kind == DmgText.Total ? Ink.MapGood : r.Kind == DmgText.Crit ? Ink.MapLoot : r.Kind == DmgText.Weapon ? Ink.Map : Ink.MapDim));
        }
        var tw = 0f;
        foreach (var l in lines) tw = Mathf.Max(tw, f.Measure(l.Text));
        tw = Mathf.Min(Size.X - 16, tw + 20);
        var th = lines.Count * 16 + 8;
        var x = Mathf.Clamp(row.Position.X, 8, Size.X - tw - 8);
        var y = row.End.Y + 4 + th <= Size.Y - 4 ? row.End.Y + 4 : Mathf.Max(4, row.Position.Y - th - 4);
        var box = new Rect2(x, y, tw, th);
        DrawStyleBox(Ui.Box(new Color(0, 0, 0, 0.3f), 8), new Rect2(box.Position + new Vector2(0, 2), box.Size));
        DrawStyleBox(Ui.Box(Pal.Text, 8, Pal.Brand), box);
        for (var i = 0; i < lines.Count; i++) f.Draw(this, new Vector2(x + 10, y + 4 + i * 16), lines[i].Text, lines[i].Ink);
    }

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
        if (i == WeaponTip)
        {
            DrawWeaponTip(_statRects[i]);
            return;
        }
        var f = PixelFont.I;
        var cls = Selected;
        var c = _d.Classes[cls];
        var m = Meta.Mods(_d, _p, cls); // v0.21.52 cz. b: mistrzostwo zawodu (broń mistrza)
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

    private string LockedShort(int cls) => Meta.ClassSecret(_d, cls) ? Loc.T("sekret_2") : Meta.ClassReward(_d, cls) ? Loc.F("wygr_3", RewardWinOf(cls)) : Loc.F("dosw_5", Meta.ClassCost(_d, _p));

    /// <summary>Podpowiedź sekretnego zlecenia, które odblokowuje zawód (v0.21.51 cz. 2).</summary>
    private string SecretHint(int cls)
    {
        var si = Secrets.Of(_d, SecretReward.Cls, cls);
        return si >= 0 ? _d.Secrets[si].Hint : "???";
    }

    private string LockedText(int cls, bool keys) => Meta.ClassSecret(_d, cls)
        ? Loc.F("sekretne_zlecenie_telefon_2", SecretHint(cls))
        : Meta.ClassReward(_d, cls)
        ? Loc.F("nagroda_za_odbior_budowy_za", RewardWinOf(cls), _p.Wins)
        : Loc.F("zablokowany_dosw_w_szkoleniach", Meta.ClassCost(_d, _p)) + (keys ? " (K)" : "");
}
