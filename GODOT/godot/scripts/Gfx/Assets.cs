using System.Collections.Generic;
using Godot;
using LifeLike.Core;

namespace LifeLike.Game.Gfx;

/// <summary>
/// Tekstury z GODOT/godot/assets (eksport z GBA: GODOT/tools/export_godot_assets.py) i numery klatek
/// jak w GBA/src/main.cpp (frame_coffee, frame_fx, frame_lock, ...). Arkusze to pionowe paski klatek.
/// </summary>
public static class Assets
{
    public const int Cell = 32;        // pole mapy (16x16 na GBA, tu Scale2x)
    public const int Actor = 32;       // klatka postaci / wroga / znajdźki
    public const int Particle = 16;    // klatka cząsteczki (8x8 na GBA)
    public const int Icon = 16;        // ikony HUD i telefonu (1:1 z GBA)

    // actors.png (kolejność jak GBA/tools/make_assets.py make_actors)
    public const int FrameCoffee = 15;
    public const int FrameFx = 18;
    public const int FrameLock = 19;
    public const int FrameSilhouette = 20;
    public const int FrameToolbox = 26;
    public const int FrameAnimB = 27;
    public const int FrameGear = 42;
    public const int FrameReticle = 45;

    // particles.png (particle_pool w main.cpp)
    public const int PDust = 0, PSpark = 3, PConfetti = 5, PStar = 9, PRing = 10, PBrick = 12, PNail = 13, PBolt = 14;
    public const int PDrop = 16, PPlus = 17, PZzz = 18, PAlert = 19, PMarker = 20, PStatus = 21;

    // tiles/stage_N.png: 4 podłogi, 2 podłogi z cieniem muru, mur, lico muru, schody
    public const int TileFloor = 0, TileFloorShadow = 4, TileWall = 6, TileWallFace = 7, TileStairs = 8;

    private static readonly Dictionary<string, Texture2D> Cache = new();

    public static Texture2D Tex(string rel)
    {
        if (Cache.TryGetValue(rel, out var t)) return t;
        t = GD.Load<Texture2D>("res://assets/" + rel);
        Cache[rel] = t;
        return t;
    }

    /// <summary>Zwalnia bufor tekstur (wyjście z gry - bez ostrzeżeń o zasobach w użyciu).</summary>
    public static void ClearCache() => Cache.Clear();

    public static Texture2D Actors => Tex("sprites/actors.png");
    public static Texture2D ActorsWhite => Tex("sprites/actors_white.png");
    /// <summary>Chód (4 klatki) i oddech postaci: wiersz = klatka z actors.png, kolumna = AnimWalk0..3 / AnimBreath.</summary>
    public static Texture2D ActorsAnim => Tex("sprites/actors_anim.png");
    public static Texture2D ActorsAnimWhite => Tex("sprites/actors_anim_white.png");
    public static Texture2D Truck => Tex("sprites/truck.png");
    public static Texture2D Particles => Tex("sprites/particles.png");
    public static Texture2D Houses => Tex("sprites/houses.png");
    public static Texture2D MenuIcons => Tex("sprites/menu_icons.png");
    public static Texture2D AbilityIcons => Tex("sprites/ability_icons.png");
    public static Texture2D UiAbility => Tex("ui/ability_icons.png");
    public static Texture2D UiAbilityGray => Tex("ui/ability_icons_gray.png");
    public static Texture2D UiMenu => Tex("ui/menu_icons.png");
    public static Texture2D PhoneIcons => Tex("ui/phone_icons.png");
    public static Texture2D PhoneIconsDim => Tex("ui/phone_icons_dim.png");
    public static Texture2D Shadow => Tex("fx/shadow.png");
    public static Texture2D Danger => Tex("fx/danger.png");
    public static Texture2D Range => Tex("fx/range.png");

    public static Texture2D StageTiles(int stage) => Tex($"tiles/stage_{Mathf.Clamp(stage, 0, 7)}.png");

    public const int AnimWalkFrames = 4, AnimBreath = 4;

    /// <summary>Region klatki animacji (chód 0..3, oddech 4) postaci o klatce bazowej row.</summary>
    public static Rect2 AnimFrame(int row, int k) => new(k * Actor, row * Actor, Actor, Actor);

    /// <summary>Czy klatka ma animację chodu (zawody, problemy budowy, bossowie).</summary>
    public static bool HasWalk(int frame) => frame is >= 0 and < 15 or 46 or 47;

    /// <summary>Region klatki size x size w pionowym pasku.</summary>
    public static Rect2 Frame(int index, int size) => new(0, index * size, size, size);

    /// <summary>Druga klatka animacji (anim_b z main.cpp): zawody i wrogowie 0..14 -> +27, bossowie 46-47 -> 48-49.</summary>
    public static int AnimB(int frame) => frame < 15 ? frame + FrameAnimB : frame + 2;

    /// <summary>Klatka znajdźki jak pickup_frame() w main.cpp.</summary>
    public static int PickupFrame(in Pickup p) => p.Type switch
    {
        PickupType.Tool => FrameToolbox,
        PickupType.GearBox => FrameGear + p.Arg % 3,
        _ => FrameCoffee + (int)p.Type,
    };

    public static void DrawFrame(CanvasItem ci, Texture2D tex, int frame, int size, Vector2 topLeft, int scale = 1) =>
        DrawFrame(ci, tex, frame, size, topLeft, scale, Colors.White);

    public static void DrawFrame(CanvasItem ci, Texture2D tex, int frame, int size, Vector2 topLeft, int scale, Color modulate) =>
        ci.DrawTextureRectRegion(tex, new Rect2(topLeft, new Vector2(size * scale, size * scale)), Frame(frame, size), modulate);
}
