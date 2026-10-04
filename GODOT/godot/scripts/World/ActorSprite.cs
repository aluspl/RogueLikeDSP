using Godot;
using LifeLike.Game.Gfx;

namespace LifeLike.Game.World;

/// <summary>
/// Postać, problem budowy albo znajdźka na mapie: cień pod spodem, 2 klatki animacji w miejscu (A/B jak na GBA)
/// albo oddech, 4 klatki chodu przy przejściu na sąsiednie pole, płynny ruch, „szturchnięcie” przy ataku,
/// biały błysk przy trafieniu, mruganie po obrażeniach, podskakiwanie znajdziek i zanikanie usuniętego problemu.
/// </summary>
public partial class ActorSprite : Node2D
{
    public int BaseFrame { get; set; }
    /// <summary>Druga klatka animacji (-1 = brak).</summary>
    public int AltFrame { get; set; } = -1;
    public float AnimPeriod { get; set; } = 0.4f;
    public float AnimPhase { get; set; }
    public bool Bob { get; set; }
    /// <summary>W miejscu oddycha (klatka oddechu co pół sekundy) zamiast przebierać klatkami A/B.</summary>
    public bool Breathes { get; set; }
    public bool Flip { get; set; }
    public bool HasShadow { get; set; } = true;
    /// <summary>Przesunięcie rysowania względem pozycji (znajdźki stoją „za” postaciami przy sortowaniu po Y).</summary>
    public Vector2 DrawOffset { get; set; }
    /// <summary>Mini pasek HP nad głową (0..1, &lt;0 = ukryty) i kolor (0 zielony, 1 żółty, 2 czerwony).</summary>
    public float HpFill { get; set; } = -1f;
    public int HpColor { get; set; }
    /// <summary>v0.21.50: elita – złota ramka i poświata.</summary>
    public bool Elite { get; set; }
    /// <summary>v0.21.50: stany nad głową (bity: 1 mokry, 2 zapylony, 4 zmrożony).</summary>
    public int States { get; set; }

    private Vector2 _bump;
    private float _flash;
    private Color _flashColor = Colors.White;
    private float _blink;
    private float _clock;
    private float _dying = -1f;
    private Tween _move;
    private float _walk;       // ile jeszcze trwa chód po ostatnim kroku
    private float _walkClock;  // faza cyklu chodu (ciągła przez kolejne kroki)

    private const float WalkStep = 0.07f, WalkHold = 0.26f;

    public bool Dying => _dying >= 0f;

    public override void _Process(double delta)
    {
        var dt = (float)delta;
        _clock += dt;
        if (_flash > 0) _flash = Mathf.Max(0, _flash - dt * 4f);
        if (_blink > 0) _blink = Mathf.Max(0, _blink - dt);
        if (_walk > 0)
        {
            _walk = Mathf.Max(0, _walk - dt);
            _walkClock += dt;
        }
        if (_dying >= 0f)
        {
            _dying += dt;
            if (_dying > 0.35f)
            {
                Visible = false;
                _dying = -1f;
            }
        }
        QueueRedraw();
    }

    /// <summary>Przesuwa na pozycję (środek pola). snap = bez animacji.</summary>
    public void MoveTo(Vector2 pos, bool snap)
    {
        _move?.Kill();
        if (snap || Position.DistanceTo(pos) > Assets.Cell * 3)
        {
            Position = pos;
            return;
        }
        if (Position == pos) return;
        if (_walk <= 0) _walkClock = WalkStep; // pierwszy krok zaczyna od klatki „w kroku”
        _walk = WalkHold;
        _move = CreateTween();
        _move.TweenProperty(this, "position", pos, 0.11f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
    }

    /// <summary>Szturchnięcie w stronę celu (atak).</summary>
    public void Bump(Vector2 dir)
    {
        if (dir == Vector2.Zero) return;
        var t = CreateTween();
        t.TweenMethod(Callable.From<float>(v => _bump = dir.Normalized() * v), 0f, 7f, 0.05f);
        t.TweenMethod(Callable.From<float>(v => _bump = dir.Normalized() * v), 7f, 0f, 0.09f);
    }

    public void Flash(Color c, float strength = 1f)
    {
        _flashColor = c;
        _flash = strength;
    }

    /// <summary>Mruganie po otrzymaniu obrażeń (hurt_timer na GBA).</summary>
    public void Blink(float seconds) => _blink = seconds;

    public void Die()
    {
        _dying = 0f;
        Flash(Colors.White);
    }

    public void Revive()
    {
        _dying = -1f;
        Visible = true;
    }

    public override void _Draw()
    {
        const int s = Assets.Actor;
        var bobY = Bob && ((int)((_clock + AnimPhase) / 0.27f) & 1) == 1 ? -2f : 0f;
        var frame = AltFrame >= 0 && ((int)((_clock + AnimPhase) / AnimPeriod) & 1) == 1 ? AltFrame : BaseFrame;
        var alpha = 1f;
        var scale = 1f;
        if (_dying >= 0f)
        {
            var t = _dying / 0.35f;
            alpha = 1f - t;
            scale = 1f - t * 0.4f;
        }
        var at = DrawOffset + new Vector2(0, Proj.SpriteLift); // v0.21.54: w widoku 3/4 stopy w dolnej części wiersza
        if (HasShadow)
        {
            if (Proj.ThreeQuarter) // v0.21.54: cień rzucany w prawo w dół (światło z lewej góry) i ciemniejszy pod stopami
                DrawTextureRect(Assets.Shadow, new Rect2(at + new Vector2(-8, 9), new Vector2(30, 9)), false, new Color(1, 1, 1, 0.6f * alpha));
            DrawTextureRect(Assets.Shadow, new Rect2(at + new Vector2(-12, 9), new Vector2(24, 8)), false, new Color(1, 1, 1, alpha));
        }
        if (Elite && _dying < 0f) // elita: złota poświata pod stopami (pulsuje)
        {
            var pulse = 0.55f + 0.25f * Mathf.Sin(_clock * 4f);
            DrawArc(at + new Vector2(0, 13), 13f, 0, Mathf.Tau, 24, new Color(Pal.EliteGold, pulse), 2f);
        }
        if (_blink > 0 && ((int)(_blink * 16) & 1) == 1) return;
        var off = at + _bump + new Vector2(0, bobY);
        DrawSetTransform(off + new Vector2(0, (1 - scale) * s / 2), 0, new Vector2(Flip ? -scale : scale, scale));
        var dst = new Rect2(-s / 2, -s / 2 - 2, s, s);
        var (tex, white, src) = Source(frame);
        DrawTextureRectRegion(tex, dst, src, new Color(1, 1, 1, alpha));
        if (_flash > 0) DrawTextureRectRegion(white, dst, src, new Color(_flashColor, _flash * 0.85f * alpha));
        if (Elite) // elita: złoty odcień i narożniki ramki
        {
            DrawTextureRectRegion(white, dst, src, new Color(Pal.EliteGold, 0.2f * alpha));
            var gc = new Color(Pal.EliteGold, alpha);
            var r = dst.Grow(1);
            const float k = 6f;
            foreach (var (c, dx, dy) in new[] { (r.Position, 1, 1), (new Vector2(r.End.X, r.Position.Y), -1, 1), (new Vector2(r.Position.X, r.End.Y), 1, -1), (r.End, -1, -1) })
            {
                DrawLine(c, c + new Vector2(dx * k, 0), gc, 2f);
                DrawLine(c, c + new Vector2(0, dy * k), gc, 2f);
            }
        }
        DrawSetTransform(Vector2.Zero, 0, Vector2.One);
        if (HpFill >= 0f && _dying < 0f) DrawMiniHp(off + new Vector2(0, -22));
        if (States != 0 && _dying < 0f) // stany: kropla, pył, płatek (kombinacje stanów)
        {
            var n = 0;
            for (var b = 1; b <= 4; b <<= 1) n += (States & b) != 0 ? 1 : 0;
            var x = off.X - (n - 1) * 5f;
            for (var b = 1; b <= 4; b <<= 1)
            {
                if ((States & b) == 0) continue;
                DrawCircle(new Vector2(x, off.Y - 31), 5.5f, new Color(0.08f, 0.08f, 0.12f, 0.7f));
                BoonLook.DrawState(this, b, new Vector2(x, off.Y - 31));
                x += 10f;
            }
        }
    }

    /// <summary>Tekstura i region klatki: chód z actors_anim w trakcie kroku, oddech w miejscu, inaczej klatka A/B.</summary>
    private (Texture2D Tex, Texture2D White, Rect2 Src) Source(int frame)
    {
        if (Assets.HasWalk(BaseFrame) && _dying < 0f)
        {
            if (_walk > 0)
                return (Assets.ActorsAnim, Assets.ActorsAnimWhite, Assets.AnimFrame(BaseFrame, (int)(_walkClock / WalkStep) % Assets.AnimWalkFrames));
            if (Breathes && ((int)((_clock + AnimPhase) / 0.5f) & 1) == 1)
                return (Assets.ActorsAnim, Assets.ActorsAnimWhite, Assets.AnimFrame(BaseFrame, Assets.AnimBreath));
        }
        return (Assets.Actors, Assets.ActorsWhite, Assets.Frame(frame, Assets.Actor));
    }

    /// <summary>Mini pasek HP jak mini_hp.bmp na GBA (obrys, tło, wypełnienie w kolorze progu), w 2x.</summary>
    private void DrawMiniHp(Vector2 c)
    {
        var r = new Rect2(c.X - 8, c.Y, 16, 4);
        DrawRect(r.Grow(1), Pal.HpEdge);
        DrawRect(r, Pal.HpBack);
        var w = Mathf.Max(1f, Mathf.Round(16 * HpFill));
        DrawRect(new Rect2(r.Position, new Vector2(w, 4)), Pal.HpMain[HpColor]);
        DrawRect(new Rect2(r.Position, new Vector2(w, 1)), new Color(Pal.HpShine, 0.55f));
    }
}
