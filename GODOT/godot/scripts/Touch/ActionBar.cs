using System;
using Godot;
using LifeLike.Game.Gfx;
using LifeLike.Game.Settings;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Touch;

/// <summary>
/// Dolny pasek akcji dla kciuka (sterowanie dotykiem, jak menu akcji START z GBA): Atak (młotek), Moc (ikona zawodu:
/// szara z odliczaniem albo pulsująca, gdy gotowa), Termos (liczba kaw), Czekaj (klepsydra), Telefon. Dla prawej
/// ręki Atak jest najbliżej kciuka (prawa krawędź), dla lewej - odwrotnie. Nad paskiem domowym (bezpieczny obszar).
/// </summary>
public partial class ActionBar : Control
{
    private static readonly BarButton[] RightHand = [BarButton.Phone, BarButton.Wait, BarButton.Thermos, BarButton.Ability, BarButton.Attack];
    private static readonly BarButton[] LeftHand = [BarButton.Attack, BarButton.Ability, BarButton.Thermos, BarButton.Wait, BarButton.Phone];

    private CoreGame _g;
    private float _clock;

    /// <summary>Wciśnięty przycisk (podświetlony fioletem marki).</summary>
    public BarButton Pressed { get; set; } = BarButton.None;

    public static BarButton[] Order => GameSettings.LeftHanded ? LeftHand : RightHand;

    /// <summary>Pasek w pikselach UI (bez paska domowego pod spodem).</summary>
    public static Rect2 BarRect => new(0, Layout.UiSize.Y - Layout.SafeBottom - Layout.ActionBarHeight, Layout.UiSize.X, Layout.ActionBarHeight);

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    public void Bind(CoreGame g) => _g = g;

    public override void _Process(double delta)
    {
        if (!IsVisibleInTree()) return;
        _clock += (float)delta;
        QueueRedraw();
    }

    public static Rect2 ButtonRect(int slot)
    {
        var bar = BarRect;
        var left = Layout.SafeLeft + 4;
        var w = (Layout.UiSize.X - Layout.SafeLeft - Layout.SafeRight - 8) / 5f;
        return new Rect2(Mathf.Round(left + slot * w + 2), bar.Position.Y + 4, Mathf.Round(w - 4), bar.Size.Y - 8);
    }

    public static BarButton HitTest(Vector2 p)
    {
        var order = Order;
        for (var i = 0; i < order.Length; i++)
        {
            if (ButtonRect(i).Grow(3).HasPoint(p)) return order[i];
        }
        return BarButton.None;
    }

    /// <summary>Czy punkt jest na pasku (albo pod nim) - nie na mapie.</summary>
    public static bool Covers(Vector2 p) => p.Y >= BarRect.Position.Y - 4;

    public override void _Draw()
    {
        try
        {
            DrawContent();
        }
        catch (Exception ex)
        {
            DrawErrors.Record("ActionBar", ex);
        }
    }

    private void DrawContent()
    {
        if (_g is null) return;
        var bar = BarRect;
        DrawRect(new Rect2(0, bar.Position.Y - 4, Size.X, Size.Y - bar.Position.Y + 4), new Color(Pal.Text, 0.86f));
        DrawRect(new Rect2(0, bar.Position.Y - 5, Size.X, 1), new Color(Pal.Brand, 0.7f));
        var order = Order;
        for (var i = 0; i < order.Length; i++) DrawButton(order[i], ButtonRect(i));
    }

    private void DrawButton(BarButton b, Rect2 r)
    {
        var f = PixelFont.I;
        var g = _g;
        var on = Pressed == b;
        var enabled = b switch
        {
            BarButton.Thermos => g.Thermos > 0,
            BarButton.Ability => g.AbilityCd == 0,
            _ => true,
        };
        DrawStyleBox(Ui.Box(on ? Pal.Brand : new Color(1, 1, 1, 0.07f), 10, on ? Pal.Accent : new Color(1, 1, 1, 0.14f)), r);
        var icon = new Vector2(Mathf.Round(r.GetCenter().X - 16), r.Position.Y + 3);
        var dim = new Color(1, 1, 1, enabled ? 1f : 0.45f);
        string label;
        switch (b)
        {
            case BarButton.Attack:
                Assets.DrawFrame(this, Assets.MenuIcons, 0, 32, icon);
                label = "Atak";
                break;
            case BarButton.Ability:
                if (g.AbilityCd == 0)
                {
                    var bob = ((int)(_clock * 4) & 1) == 1 ? -2 : 0;
                    var pulse = 0.5f + 0.5f * Mathf.Sin(_clock * 6f);
                    DrawStyleBox(Ui.Box(new Color(Pal.Accent, 0.15f + 0.25f * pulse), 8), new Rect2(icon - new Vector2(3, 2), new Vector2(38, 36)));
                    Assets.DrawFrame(this, Assets.AbilityIcons, g.Cls, 32, icon + new Vector2(0, bob));
                    label = "Moc";
                }
                else
                {
                    Assets.DrawFrame(this, Assets.UiAbilityGray, g.Cls, Assets.Icon, icon, 2, new Color(1, 1, 1, 0.6f));
                    f.Draw(this, icon + new Vector2(16, 6), g.AbilityCd.ToString(), Ink.Map, TextAlign.Center, 1.5f);
                    label = $"Moc {g.AbilityCd}";
                }
                break;
            case BarButton.Thermos:
                Assets.DrawFrame(this, Assets.MenuIcons, 1, 32, icon, 1, dim);
                label = $"Termos {g.Thermos}";
                break;
            case BarButton.Wait:
                Assets.DrawFrame(this, Assets.MenuIcons, 2, 32, icon);
                label = "Czekaj";
                break;
            default:
                Assets.DrawFrame(this, Assets.TouchIcons, TouchIcon.Phone, Assets.Icon, icon, 2);
                label = "Telefon";
                break;
        }
        var ink = on ? Ink.White : enabled ? Ink.Map : Ink.MapDim;
        f.Draw(this, new Vector2(r.GetCenter().X, r.End.Y - 20), f.Fit(label, (int)r.Size.X - 2), ink, TextAlign.Center);
    }
}
