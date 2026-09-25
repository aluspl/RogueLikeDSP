using Godot;

namespace LifeLike.Game;

/// <summary>
/// Tło pod telefonem: mapa rozmyta (mipmapy tekstury ekranu) i przyciemniona fioletem marki, z płynnym wejściem.
/// </summary>
public partial class Backdrop : ColorRect
{
    private const string ShaderCode = @"
shader_type canvas_item;
uniform sampler2D screen_tex : hint_screen_texture, filter_linear_mipmap;
uniform float amount = 0.0;
void fragment() {
    vec3 c = textureLod(screen_tex, SCREEN_UV, 2.4 * amount).rgb;
    vec3 navy = vec3(0.06, 0.045, 0.15);
    COLOR = vec4(mix(c, navy, 0.6 * amount), 1.0);
}";

    private ShaderMaterial _mat;
    private float _amount;
    private float _target;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _mat = new ShaderMaterial { Shader = new Shader { Code = ShaderCode } };
        Material = _mat;
        Visible = false;
    }

    public void SetOn(bool on, bool instant = false)
    {
        _target = on ? 1f : 0f;
        if (instant) _amount = _target;
        Visible = _amount > 0.001f || on;
        _mat.SetShaderParameter("amount", _amount);
    }

    public override void _Process(double delta)
    {
        if (Mathf.IsEqualApprox(_amount, _target)) return;
        _amount = Mathf.MoveToward(_amount, _target, (float)delta * 6f);
        _mat.SetShaderParameter("amount", _amount);
        Visible = _amount > 0.001f;
    }
}
