using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;

namespace LifeLike.Game.Gfx;

/// <summary>
/// v0.21.52: kolor kasku bohatera (wygląd z odznak i zleceń). Klatki 160-183 mają kask w kolorze indeksu D palety GBA
/// (ciemna zieleń, poza kaskiem nieużywana przez fachowców) – shader podmienia ten kolor na wybrany (na GBA: osobna paleta
/// bohatera). Materiał na węźle rysującym bohatera (mapa, wybór zawodu, prolog); bez koloru – bez materiału.
/// </summary>
public static class HelmetTint
{
    /// <summary>Kolor kasku w klatkach do pokolorowania (SPR_PAL[10] z make_assets.py).</summary>
    public static readonly Color Mask = Color.Color8(46, 107, 48);

    private static Shader _shader;

    private static Shader Shader => _shader ??= new Shader
    {
        Code = """
            shader_type canvas_item;
            uniform vec3 mask_color;
            uniform vec3 helmet_color;
            void fragment() {
                vec4 t = texture(TEXTURE, UV);
                if (t.a > 0.5 && distance(t.rgb, mask_color) < 0.03) {
                    vec3 mod_rgb = COLOR.rgb / max(t.rgb, vec3(0.004));
                    COLOR.rgb = helmet_color * mod_rgb;
                }
            }
            """,
    };

    /// <summary>Materiał z kolorem kasku k (data Cosmetics[k].Helmet, 0xRRGGBB).</summary>
    public static ShaderMaterial For(GameData d, int k)
    {
        var c = d.Cosmetics[k].Helmet;
        var m = new ShaderMaterial { Shader = Shader };
        m.SetShaderParameter("mask_color", new Vector3(Mask.R, Mask.G, Mask.B));
        m.SetShaderParameter("helmet_color", new Vector3(((c >> 16) & 255) / 255f, ((c >> 8) & 255) / 255f, (c & 255) / 255f));
        return m;
    }

    /// <summary>Ustawia (albo zdejmuje) materiał koloru kasku na węźle, który rysuje bohatera profilu p.</summary>
    public static void Apply(CanvasItem node, GameData d, Profile p)
    {
        var k = Assets.HeroHelmet(d, p);
        if (k < 0)
        {
            if (node.Material != null) node.Material = null;
            return;
        }
        if (node.GetMeta("helmet", -1).AsInt32() == k && node.Material != null) return;
        node.SetMeta("helmet", k);
        node.Material = For(d, k);
    }
}
