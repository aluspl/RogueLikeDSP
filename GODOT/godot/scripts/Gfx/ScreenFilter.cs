using System;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Settings;

namespace LifeLike.Game.Gfx;

/// <summary>
/// v0.21.53: filtr ekranu nad mapą, HUD i planszami – prostokąt na cały ekran z shaderem czytającym obraz pod spodem
/// (shaders/screen_filter.gdshader). Wybór, siła, telefon i ograniczony ruch z GameSettings; zablokowany w profilu filtr
/// działa jak klasyczny. Klasyczny = węzeł ukryty (bez kopii ekranu). Telefon bez filtra: warstwa 2 tuż nad planszami
/// (pod telefonem, banerami i dymkami), inaczej nad wszystkim. Cues – wzory zamiast samego koloru (paski na polach ciosu,
/// litery rzadkości) dla trybów z polem „cues”.
/// </summary>
public partial class ScreenFilter : CanvasLayer
{
    public const int UnderPhoneLayer = 2;
    public const int TopLayer = 6;

    private readonly ColorRect _rect = new() { MouseFilter = Control.MouseFilterEnum.Ignore };
    private ShaderMaterial _mat;

    /// <summary>Dane i profil (odblokowanie); bez nich – zawsze klasyczny.</summary>
    public Func<(GameData Data, Profile Profile)> Source
    {
        get => _source;
        set
        {
            _source = value;
            if (_mat is not null) Apply();
        }
    }

    private Func<(GameData Data, Profile Profile)> _source;

    /// <summary>Aktywny filtr (indeks w GameData.ScreenFilters, 0 = klasyczny).</summary>
    public int Active { get; private set; }

    /// <summary>Filtr wymuszony (--filter ID: zrzuty i test) – bez względu na ustawienia i odblokowanie.</summary>
    public static string Force { get; set; } = "";

    /// <summary>Wzory zamiast samego koloru (paski na polach ciosu, litery rzadkości) – czytają je warstwy rysujące.</summary>
    public static bool Cues { get; private set; }

    /// <summary>Po zmianie filtra (np. przerysowanie mapy z wzorami).</summary>
    public static event Action Changed;

    public override void _Ready()
    {
        Layer = UnderPhoneLayer;
        _mat = new ShaderMaterial { Shader = GD.Load<Shader>("res://shaders/screen_filter.gdshader") };
        _rect.Material = _mat;
        _rect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(_rect);
        GameSettings.Changed += Apply;
        Layout.Changed += Apply;
        Apply();
    }

    private double _recheck;

    /// <summary>Co pół sekundy: profil mógł się zmienić (odblokowanie, inny profil w scenach pokazowych).</summary>
    public override void _Process(double delta)
    {
        _recheck += delta;
        if (_recheck < 0.5) return;
        _recheck = 0;
        Apply();
    }

    public override void _ExitTree()
    {
        GameSettings.Changed -= Apply;
        Layout.Changed -= Apply;
    }

    /// <summary>Tryb shadera dla filtra o tym id (0 = bez efektu).</summary>
    public static int ModeOf(string id) => id switch
    {
        "noir" => 1,
        "retro" => 2,
        "neon" => 3,
        "kwas" => 4,
        "protanopia" => 5,
        "deuteranopia" => 6,
        "tritanopia" => 7,
        "kontrast" => 8,
        _ => 0,
    };

    /// <summary>Indeks filtra z ustawień (po id), odblokowany w profilu – inaczej klasyczny.</summary>
    public static int Resolve(GameData d, Profile p)
    {
        if (Force.Length > 0) return Math.Max(0, Array.FindIndex(d.ScreenFilters, x => x.Id == Force));
        var f = Array.FindIndex(d.ScreenFilters, x => x.Id == GameSettings.Filter);
        return p is null ? Math.Max(0, f) : ScreenFilters.Valid(d, p, f);
    }

    /// <summary>Wybiera filtr f (zapis ustawień i odświeżenie).</summary>
    public static void Select(GameData d, int f)
    {
        GameSettings.Filter = d.ScreenFilters[Math.Clamp(f, 0, d.ScreenFilters.Length - 1)].Id;
        GameSettings.Save();
    }

    public void Apply()
    {
        var src = Source?.Invoke();
        var f = src is { } s && s.Data is not null ? Resolve(s.Data, s.Profile) : 0;
        var def = src is { } s2 && s2.Data is not null ? s2.Data.ScreenFilters[f] : null;
        var mode = def is null ? 0 : ModeOf(def.Id);
        var cues = def?.Cues ?? false;
        var changed = f != Active || cues != Cues;
        Active = f;
        Cues = cues;
        _rect.Visible = mode != 0;
        Layer = GameSettings.FilterPhone ? TopLayer : UnderPhoneLayer;
        _mat.SetShaderParameter("mode", mode);
        _mat.SetShaderParameter("strength", GameSettings.FilterStrength / (float)GameSettings.FilterSteps);
        _mat.SetShaderParameter("px", Mathf.Max(1f, Layout.ContentScale));
        _mat.SetShaderParameter("motion", !GameSettings.ReduceMotion);
        if (changed) Changed?.Invoke();
    }
}
