using Godot;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.World;

/// <summary>
/// Nakładki na podłogę pod postaciami: pulsujące pola zapowiedzianego ciosu bossa (czerwona ramka z kreskami),
/// ramki pól w zasięgu broni (mignięcie, gdy atak nie ma celu), poświata schodów (pulsowanie palety na GBA)
/// kałuże w deszczu (wejście = poślizg), błoto w akcie I (wejście = tura) i pola wybuchu problemu. v0.21.53: przy filtrach
/// z wzorami (ScreenFilter.Cues) pola ciosu i wybuchu dostają ukośne paski i jasną ramkę. v0.21.54: pola wg Proj (widok płaski
/// albo 3/4); pęknięcie / drzwi magazynu rysuje WallLayer na licu muru.
/// </summary>
public partial class OverlayLayer : Node2D
{
    private CoreGame _g;
    private float _clock;
    private float _range;

    public void Bind(CoreGame g) => _g = g;

    /// <summary>Pokaż zasięg broni na chwilę (range_flash na GBA).</summary>
    public void FlashRange(float seconds = 0.45f) => _range = seconds;

    /// <summary>Zasięg widoczny, dopóki gracz trzyma A (celowanie).</summary>
    public bool RangeHeld { get; set; }

    public override void _Process(double delta)
    {
        _clock += (float)delta;
        if (_range > 0) _range = Mathf.Max(0, _range - (float)delta);
        QueueRedraw();
    }

    /// <summary>Kałuża: elipsa wody z odblaskiem (jak kafel 9 na GBA).</summary>
    private void DrawPuddle(Rect2 r, float pulse, bool lit)
    {
        var sy = r.Size.Y / Assets.Cell; // v0.21.54: w widoku 3/4 kałuża spłaszczona jak wiersz
        var center = r.GetCenter() + new Vector2(0, 2 * sy);
        var pts = new Vector2[20];
        for (var i = 0; i < pts.Length; i++)
        {
            var a = i * Mathf.Tau / pts.Length;
            pts[i] = center + new Vector2(Mathf.Cos(a) * 13f, Mathf.Sin(a) * 8f * sy);
        }
        DrawColoredPolygon(pts, new Color(0.25f, 0.44f, 0.69f, lit ? 0.85f : 0.3f));
        if (lit) DrawLine(center + new Vector2(-6, -3), center + new Vector2(3, -3), new Color(0.75f, 0.88f, 1f, 0.5f + 0.3f * pulse), 2f);
    }

    /// <summary>Błoto (akt I): płaska mokra plama w jednym z 3 wariantów (skrót pozycji), lekko obrócona odbiciem.</summary>
    private void DrawMud(Rect2 r, int x, int y)
    {
        var h = (int)(((uint)(x * 19349663) ^ (uint)(y * 83492791)) >> 4);
        var src = Assets.Frame(h % Assets.MudVariants, Assets.Cell);
        if ((h & 8) != 0) r = new Rect2(r.Position.X + r.Size.X, r.Position.Y, -r.Size.X, r.Size.Y); // odbicie w poziomie
        DrawTextureRectRegion(Assets.Mud, r, src, new Color(1, 1, 1, _g.Visible(x, y) ? 1f : 0.5f));
    }

    /// <summary>v0.21.53: ukośne paski (ciemne z jasnym brzegiem) na polu ciosu – czytelne bez rozróżniania czerwieni.</summary>
    private void DrawHatch(Rect2 r)
    {
        var c = r.Size.X;
        var sy = new Vector2(1, r.Size.Y / c); // v0.21.54: wiersz 3/4 niższy niż pole
        for (var k = 6f; k < 2 * c; k += 8f)
        {
            var a = r.Position + new Vector2(Mathf.Min(k, c), Mathf.Max(0, k - c)) * sy;
            var b = r.Position + new Vector2(Mathf.Max(0, k - c), Mathf.Min(k, c)) * sy;
            DrawLine(a, b, new Color(0.05f, 0.02f, 0.05f, 0.7f), 2.5f);
            DrawLine(a + new Vector2(1.5f, 1.5f), b + new Vector2(1.5f, 1.5f), new Color(1f, 0.95f, 0.6f, 0.55f), 1f);
        }
        DrawRect(r.Grow(-1.5f), new Color(1f, 0.95f, 0.6f, 0.8f), false, 2f);
    }

    /// <summary>
    /// v0.22.0 (#67): schody w dół – delikatna ciepła poświata u progu i dwie strzałki w dół, które co chwilę spływają
    /// w głąb otworu (zamiast świecącego prostokąta na całe pole, który robił z schodów skrzynkę).
    /// </summary>
    private void DrawStairsCue(Rect2 r)
    {
        var sy = r.Size.Y / Proj.W;
        var lip = r.Position + new Vector2(r.Size.X / 2, r.Size.Y - 3 * sy);
        var breathe = 0.5f + 0.5f * Mathf.Sin(_clock * 3f);
        DrawRect(new Rect2(r.Position.X + 4, lip.Y - 4 * sy, r.Size.X - 8, 4 * sy), new Color(Pal.StairsGlow, 0.10f + 0.10f * breathe));
        var phase = _clock * 0.9f % 1f;   // strzałki spływają w dół i gasną
        for (var k = 0; k < 2; k++)
        {
            var p = (phase + k * 0.5f) % 1f;
            var c = new Vector2(r.Position.X + r.Size.X / 2, r.Position.Y + (7 + 14 * p) * sy);
            var a = Mathf.Sin(p * Mathf.Pi) * 0.9f;
            var hw = 6f - 2f * p;
            var hh = (4f - 1f * p) * sy;
            DrawPolyline([c + new Vector2(-hw, -hh), c, c + new Vector2(hw, -hh)], new Color(0.05f, 0.04f, 0.08f, a * 0.8f), 4f);
            DrawPolyline([c + new Vector2(-hw, -hh), c, c + new Vector2(hw, -hh)], new Color(Pal.StairsGlow, a), 2f);
        }
    }

    /// <summary>v0.22.0 (#67): schody zamknięte (Akt 0) – taśma ostrzegawcza w poprzek otworu i kłódka nad nią.</summary>
    private void DrawStairsLock(Rect2 r, float pulse)
    {
        var sy = r.Size.Y / Proj.W;
        DrawRect(new Rect2(r.Position + new Vector2(3, 3 * sy), new Vector2(r.Size.X - 6, r.Size.Y - 6 * sy)), new Color(0.05f, 0.04f, 0.1f, 0.35f));
        var band = new Rect2(r.Position.X + 1, r.Position.Y + 17 * sy, r.Size.X - 2, 7 * sy);
        DrawRect(band.Grow(1), new Color(0.06f, 0.05f, 0.08f, 0.9f));
        DrawRect(band, new Color(1f, 0.82f, 0.18f));
        for (var sx = -band.Size.Y; sx < band.Size.X; sx += 8f)   // czarne ukośne pasy taśmy
        {
            var x0 = band.Position.X + Mathf.Max(0, sx);
            var x1 = band.Position.X + Mathf.Min(band.Size.X, sx + band.Size.Y);
            if (x1 - x0 < 1) continue;
            DrawLine(new Vector2(x0, band.End.Y - (x0 - band.Position.X - sx)), new Vector2(x1, band.End.Y - (x1 - band.Position.X - sx)), new Color(0.08f, 0.07f, 0.1f), 3f);
        }
        DrawTextureRectRegion(Assets.Actors, new Rect2(r.Position + new Vector2(6, -6 * sy - 2 * pulse), new Vector2(20, 20)), Assets.Frame(Assets.FrameLock, Assets.Actor));
    }

    public override void _Draw()
    {
        if (_g is null) return;
        var pulse = 0.5f + 0.5f * Mathf.Sin(_clock * 7f);
        for (var y = 0; y < Level.H; y++)
        {
            for (var x = 0; x < Level.W; x++)
            {
                if (!_g.Explored(x, y)) continue;
                var r = Proj.CellRect(x, y);
                var t = _g.Lv[x, y];
                if (_g.Puddle(x, y)) DrawPuddle(r, pulse, _g.Visible(x, y));
                else if (_g.Mud(x, y)) DrawMud(r, x, y);
                var locked = t == Tile.Stairs && _g.StairsLocked();   // pieczątki (Akt 0): schody zamknięte do kompletu dokumentów
                if (t == Tile.Stairs && _g.Visible(x, y) && !locked && !(x == _g.Hero.X && y == _g.Hero.Y)) DrawStairsCue(r);
                if (locked && !(x == _g.Hero.X && y == _g.Hero.Y)) DrawStairsLock(r, pulse);
                if (t != Tile.Wall && _g.DangerCell(x, y))
                {
                    DrawTextureRect(Assets.Danger, r, false, new Color(1, 1, 1, 0.55f + 0.45f * pulse));
                    if (ScreenFilter.Cues) DrawHatch(r); // v0.21.53: wzór zamiast samego koloru (filtry dla daltonistów)
                }
                if ((_range > 0 || RangeHeld) && t != Tile.Wall && _g.Visible(x, y) && !(x == _g.Hero.X && y == _g.Hero.Y)
                    && CoreGame.Cheb(_g.Hero.X, _g.Hero.Y, x, y) <= _g.WeaponRange())
                    DrawTextureRect(Assets.Range, r, false, new Color(1, 1, 1, RangeHeld ? 0.75f + 0.25f * pulse : Mathf.Min(1f, _range * 4f)));
            }
        }
        for (var i = 0; i < _g.PickupsCount; i++)   // pole wydarzenia: poświata jak powiadomienie
        {
            var p = _g.Pickups[i];
            if (!p.Active || p.Type != PickupType.EventTile || !_g.Explored(p.X, p.Y)) continue;
            DrawCircle(Proj.Center(p.X, p.Y) + new Vector2(0, 2), 14f, new Color(0.42f, 0.31f, 1f, 0.12f + 0.14f * pulse));
        }
        for (var i = 0; i < _g.PickupsCount; i++)   // dokumenty Aktu 0: złota poświata pod kartką
        {
            var p = _g.Pickups[i];
            if (!p.Active || p.Type != PickupType.Document || !_g.Explored(p.X, p.Y)) continue;
            var center = Proj.Center(p.X, p.Y) + new Vector2(0, 2);
            DrawCircle(center, 13f, new Color(1f, 0.85f, 0.3f, 0.10f + 0.12f * pulse));
            DrawCircle(center, 8f, new Color(1f, 0.92f, 0.5f, 0.10f + 0.10f * pulse));
        }
    }
}
