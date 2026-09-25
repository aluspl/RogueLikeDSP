using Godot;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using LifeLike.Game.Screens.Views;

namespace LifeLike.Game.Screens;

/// <summary>
/// Prolog przy pierwszej budowie (run_prologue na GBA, 420 klatek): pickup wjeżdża na plac, bohater wysiada,
/// kamera jedzie przez działkę z problemami budowy (każdy dostaje „!”), podpisy z game.json; potem SMS od
/// inwestorki (PrologueMessageScreen). A / START pomija.
/// </summary>
public sealed class PrologueScreen : Screen
{
    private const float Fps = 60f, Length = 420 / Fps;
    private float _t;
    private int _dustTick = -1;

    public PrologueScreen(App app) : base(app)
    {
    }

    public override ViewSet Views => ViewSet.Prologue;

    private PrologueView View => N.PrologueView;

    public void Open() => Flow.Go(this);

    public override void Enter(bool instant)
    {
        _t = 0;
        _dustTick = -1;
        View.Stage.Setup(S.Data, S.Data.Classes[S.Game.Cls].Frame);
        View.Caption = "";
        Apply();
    }

    /// <summary>Skok na chwilę prologu (sceny zrzutów).</summary>
    public void Seek(float seconds)
    {
        for (var t = 0f; t < seconds; t += 1 / Fps) Process(1 / Fps);
    }

    public override void Process(double delta)
    {
        _t += (float)delta;
        Apply();
        if (_t >= Length) Finish();
    }

    /// <summary>Oś czasu jak w run_prologue (klatki 60 Hz): wjazd 0-80, wysiadka 90, kamera 110-330, drugi podpis 220.</summary>
    private void Apply()
    {
        var f = _t * Fps;
        var stage = View.Stage;
        var captions = S.Data.PrologueCaptions;
        stage.TruckX = -2 + Mathf.Min(f, 80) / 80f * 7;
        stage.TruckFrame = f < 80 ? ((int)(f / 4) & 1) : 0;
        var tick = (int)(f / 5);
        if (f < 80 && tick != _dustTick) // pył spod kół
        {
            _dustTick = tick;
            stage.Fx.Spawn(PrologueStage.At(stage.TruckX, 9) + new Vector2(-32, 12), new Vector2(-1f, -0.4f), 0, 20, Assets.PDust, 3);
        }
        if (f >= 90 && !stage.HeroVisible)
        {
            stage.HeroVisible = true;
            for (var k = -1; k <= 1; k += 2)
                stage.Fx.Spawn(PrologueStage.At(7, 10) + new Vector2(k * 8, 12), new Vector2(k * 0.6f, -0.6f), 0, 16, Assets.PDust, 3);
        }
        var pan = Mathf.Clamp((f - 110) / 220f, 0f, 1f);
        View.Camera = PrologueStage.At(7 + 16 * pan, 8);
        if (f > 110) stage.AlertUpTo(View.Camera.X + 5.6f * PrologueStage.Cell);
        var caption = f >= 220 ? 1 : f >= 90 ? 0 : -1;
        View.Caption = caption >= 0 && caption < captions.Length ? captions[caption] : "";
    }

    public override bool HandleInput(InputCmd e)
    {
        if (!e.Is(GameAction.A | GameAction.Start) && !e.IsTap) return false;
        Finish();
        return true;
    }

    private void Finish()
    {
        if (Flow.Current != this) return;
        Flow.PrologueMessage.Open();
    }
}
