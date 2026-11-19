using System;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Gfx;

namespace LifeLike.Game.Phone;

/// <summary>
/// v0.23.0: Gablota artefaktów – siatka wszystkich artefaktów (GameData.Artifacts): nieodkryty to ciemna sylwetka, odkryty to ikona
/// Mistrzowska w złotej ramce; pod siatką karta zaznaczonego (nazwa, opis, mechanika, baza, data pierwszego znalezienia albo „skąd”).
/// Wspólna dla Szkoleń (TrainingTab) i Profilu (CatalogTab).
/// </summary>
public sealed class ArtifactCaseView
{
    /// <summary>Indeks dotknięcia kafla: HitBase + pozycja w Gablocie.</summary>
    public const int HitBase = 900;

    private static readonly Color Gold = new("d9a400");
    private static readonly Color GoldBg = new("fff6d6");
    private static readonly Color Shadow = new(0.05f, 0.04f, 0.12f, 0.85f);

    private readonly GameData _d;
    private readonly Profile _p;
    private int _cols = 4;

    public ArtifactCaseView(GameData d, Profile p)
    {
        _d = d;
        _p = p;
    }

    public int Sel { get; set; }

    public int Count => _d.Artifacts.Length;

    public string Sub => Loc.F("gablota_odkryte", ArtifactCase.FoundCount(_d, _p), Count);

    /// <summary>Strzałki: w poziomie o 1, w pionie o rząd siatki. True = zmiana.</summary>
    public bool Move(int dx, int dy)
    {
        if (Count == 0 || (dx == 0 && dy == 0)) return false;
        var n = Math.Clamp(Sel + dx + dy * _cols, 0, Count - 1);
        if (n == Sel) return false;
        Sel = n;
        return true;
    }

    /// <summary>Dotknięcie kafla (index z PhonePainter.Hit); true = obsłużone.</summary>
    public bool Tap(int index)
    {
        if (index < HitBase || index >= HitBase + Count) return false;
        Sel = index - HitBase;
        return true;
    }

    public void Draw(PhonePainter p)
    {
        if (Count == 0) return;
        Sel = Math.Clamp(Sel, 0, Count - 1);
        _cols = Math.Clamp((int)(p.Width / 52), 2, 4);
        var rows = (Count + _cols - 1) / _cols;
        var gap = 4f;
        var tile = MathF.Floor(Math.Min(Math.Min(84f, (p.Width - 8 - gap * (_cols - 1)) / _cols), ((p.Bottom - p.Top) * 0.42f - 12 - gap * (rows - 1)) / rows));
        var gridW = tile * _cols + gap * (_cols - 1);
        var card = p.CardH(p.Top, rows * tile + gap * (rows - 1) + 12);
        var x0 = card.Position.X + MathF.Floor((card.Size.X - gridW) / 2);
        var y0 = card.Position.Y + 6;
        for (var i = 0; i < Count; i++)
        {
            var r = new Rect2(x0 + i % _cols * (tile + gap), y0 + i / _cols * (tile + gap), tile, tile);
            DrawTile(p, i, r);
            p.Hit(r, HitBase + i);
        }
        DrawDetails(p, card.End.Y + 6);
    }

    private void DrawTile(PhonePainter p, int i, Rect2 r)
    {
        var found = ArtifactCase.Found(_p, i);
        var sel = i == Sel;
        p.C.DrawStyleBox(Ui.Box(found ? GoldBg : Pal.Group, 5, sel ? Pal.Brand : found ? Gold : Colors.Transparent), r);
        if (sel) p.C.DrawStyleBox(Ui.Box(Colors.Transparent, 6, Pal.Brand), r.Grow(1));
        var w = _d.Weapons[_d.Artifacts[i]];
        var s = r.Size.X >= 76 ? 64f : r.Size.X >= 40 ? 32f : 16f;
        var ic = new Rect2(r.Position + (r.Size - new Vector2(s, s)) / 2, new Vector2(s, s));
        if (!WeaponSheet.DrawIcon(p.C, w, 3, ic, found ? Colors.White : Shadow))
            Assets.DrawFrame(p.C, Assets.UiMenu, 0, Assets.Icon, ic.Position, 1, found ? Colors.White : Shadow);
        if (!found) p.Text(r.End.X - 4, r.Position.Y + 2, "?", Ink.Dim, TextAlign.Right);
    }

    /// <summary>Karta zaznaczonego: nazwa (i data znalezienia po prawej), potem mechanika, baza i opis – ile się zmieści.</summary>
    private void DrawDetails(PhonePainter p, float y)
    {
        var i = Sel;
        var w = _d.Weapons[_d.Artifacts[i]];
        var found = ArtifactCase.Found(_p, i);
        var width = p.Width - 24;
        var lines = new System.Collections.Generic.List<(string Text, Ink Ink)>();
        void Add(string s, Ink ink)
        {
            foreach (var l in p.F.Wrap(s, (int)width)) lines.Add((l, ink));
        }
        var b = _d.ArtifactBase(_d.Artifacts[i]);
        if (found)
        {
            Add(Loc.F("gablota_mechanika", Mechanic(w)), Ink.Dark);
            if (b >= 0) Add(Loc.F("gablota_baza", _d.Weapons[b].Name), Ink.Dim);
            var day = ArtifactCase.Day(_p, i);
            Add(day > 0 ? Loc.F("gablota_znaleziony", Date(day)) : Loc.T("gablota_dawno"), Ink.Done);
            if (w.Desc.Length > 0) Add(w.Desc, Ink.Dim);
        }
        else
        {
            Add(Loc.F("gablota_skad", w.Hint), Ink.Dark);
            if (b >= 0) Add(Loc.F("gablota_baza", _d.Weapons[b].Name), Ink.Dim);
        }
        const int lh = 15;
        var room = (int)((p.Bottom - y - 2 - 26 - 4) / lh);
        var n = Math.Clamp(lines.Count, 0, Math.Max(1, room));
        var dc = p.CardH(y, 26 + n * lh + 4);
        var tx = dc.Position.X + 12;
        var right = dc.End.X - 8;
        WeaponSheet.DrawIcon(p.C, w, 3, new Rect2(tx - 2, dc.Position.Y + 4, 16, 16), found ? Colors.White : Shadow);
        p.Text(tx + 18, dc.Position.Y + 4, found ? w.Name : Loc.T("gablota_nieodkryty"), found ? Ink.Legend : Ink.Dim, TextAlign.Left, right - tx - 18);
        var ly = dc.Position.Y + 24;
        for (var k = 0; k < n; k++)
        {
            p.Text(tx, ly, lines[k].Text, lines[k].Ink);
            ly += lh;
        }
    }

    /// <summary>Mechanika artefaktu słowami: „co N-ty cios” z danych, Łom (ściana bez odnowienia, premia na materiał) albo ruch Patentu.</summary>
    private string Mechanic(WeaponDef w)
    {
        if (w.Proc is not null && w.Proc.Desc.Length > 0) return w.Proc.Desc;
        var parts = new System.Collections.Generic.List<string>();
        if (w.WallFree) parts.Add(Loc.T("tip_sciana_bez_odn"));
        var mat = w.BonusPct != 0 ? Array.FindIndex(_d.Materials, m => m.Id == w.BonusMaterial) : -1;
        if (mat >= 0) parts.Add(Loc.F("tip_material", w.BonusPct, _d.Materials[mat].Name));
        if (parts.Count == 0) parts.Add(Loc.T("tryb_fach") + ModeText.BName(w));
        return string.Join(", ", parts);
    }

    private static string Date(int day)
    {
        var (y, m, d) = Daily.CivilFromDays(day);
        return $"{d:00}.{m:00}.{y}";
    }
}
