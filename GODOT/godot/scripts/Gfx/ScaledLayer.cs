using System;
using Godot;

namespace LifeLike.Game.Gfx;

/// <summary>
/// Warstwa z własną skalą (np. HUD z większym tekstem): węzły dzieci dodaje się do Root, który ma rozmiar
/// ekranu w jednostkach tej warstwy (UiSize / skala), więc kotwice FullRect działają jak zwykle.
/// Skala pochodzi z funkcji (zwykle z Layout) i jest liczona od nowa przy zmianie okna albo Refresh().
/// </summary>
public partial class ScaledLayer : CanvasLayer
{
    private readonly Func<float> _scale;

    public ScaledLayer() : this(() => 1f)
    {
    }

    public ScaledLayer(Func<float> scale)
    {
        _scale = scale;
        Root = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
        AddChild(Root);
    }

    public Control Root { get; }

    /// <summary>Bieżąca skala warstwy względem pikseli UI.</summary>
    public float UiScale => Scale.X;

    public override void _Ready()
    {
        Layout.Changed += Refresh;
        Refresh();
    }

    public override void _ExitTree() => Layout.Changed -= Refresh;

    /// <summary>Przelicz skalę i rozmiar Root (np. po zmianie trybu warstwy).</summary>
    public void Refresh()
    {
        var s = _scale();
        Scale = new Vector2(s, s);
        Root.Position = Vector2.Zero;
        Root.Size = (Layout.UiSize / s).Ceil();
    }

    /// <summary>Punkt w pikselach UI -> jednostki warstwy (np. klik myszy).</summary>
    public Vector2 FromUi(Vector2 ui) => ui / UiScale;
}
