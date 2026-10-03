using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;

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
    public const int FrameSilhouetteExt = 58;   // sylwetki zawodów z nagród (indeks 6+)
    public const int FrameToolbox = 26;
    public const int FrameAnimB = 27;
    public const int FrameGear = 42;
    public const int FrameReticle = 45;

    // particles.png (particle_pool w main.cpp)
    public const int PDust = 0, PSpark = 3, PConfetti = 5, PStar = 9, PRing = 10, PBrick = 12, PNail = 13, PBolt = 14;
    public const int PDrop = 16, PPlus = 17, PZzz = 18, PAlert = 19, PMarker = 20, PStatus = 21;

    // menu_icons.png: 0 atak, 1 termos, 2 czekaj, 3 ramka, 4 kłódka, 5 strzałki, 6-10 pogoda dnia (wg WeatherEffect)
    public const int MenuWeather = 6;
    // menu_icons.png 15-18: nagrody za odbiór (Młot udarowy, Pistolet do kotew, Buty, Pas), 19: Respekt, 14: kalendarz
    public const int MenuRewardTool = 15, MenuRewardTool2 = 16, MenuBoots = 17, MenuBelt = 18, MenuRespect = 19, MenuCalendar = 14;
    // menu_icons.png 20-22: mechaniki aktów (błoto, porywy, pył; + ActMechanic - 1), 23: statystyki ("i")
    public const int MenuAct = 20, MenuStats = 23;
    // menu_icons.png 24: mechanika Aktu 0 - pieczątki
    public const int MenuStamps = 24;

    // actors.png 61-80: problemy etapów (v0.21.49), 81-100 ich druga klatka
    public const int FrameStageEnemy = 61, StageEnemies = 20;
    // actors.png 101-109: problemy Aktu 0 i Decyzja odmowna, 110-118 ich druga klatka, 119-121 dokumenty (podpis, mapa, uzgodnienie)
    public const int FramePreludeEnemy = 101, PreludeEnemies = 9, FrameDocument = 119;
    // actors.png 122-126 (v0.21.50 cz. 3): pole wydarzenia, klucz do magazynu, skrzynia, pęknięcie muru, drzwi magazynu
    public const int FrameEvent = 122, FrameKey = 123, FrameChest = 124, FrameCrack = 125, FrameDoor = 126;
    // menu_icons.png 27-29: wydarzenie z wyborem, ulepszenie narzędzia, klucz do magazynu
    public const int MenuEvent = 27, MenuUpgrade = 28, MenuKey = 29;
    // v0.21.51 cz. 2: actors.png 127-129 zawody z sekretnych zleceń (Spawacz, Geodeta, Majster), 130-132 ich druga klatka,
    // 133-135 sylwetki; 136-159 kask w paski (zawód * 2 + klatka animacji A/B)
    public const int FrameSecretClass = 127, SecretClasses = 3, FrameSecretSilhouette = 133, FrameStripes = 136;
    /// <summary>v0.21.52: kask do pokolorowania (160 + zawód * 2 + klatka A/B) – kolor kasku z odznak i zleceń (HelmetTint).</summary>
    public const int FrameHelmet = 160;
    /// <summary>v0.21.52 cz. d (#47): problemy i bossowie kontraktów mapy kariery 184-195 (para klatek A/B).</summary>
    public const int FrameCareerEnemy = 184, CareerEnemies = 6;
    // menu_icons.png 30-34: sekretne zlecenie (koperta z „?”), Młot Zenka, Poziomica mistrza, Złota kielnia, Kask w paski
    public const int MenuSecret = 30, MenuZenka = 31, MenuLevel = 32, MenuGold = 33, MenuStripes = 34;

    // tiles/stage_N.png: 4 podłogi, 2 podłogi z cieniem muru, wierzch muru, lico muru, schody;
    // v0.21.51 autokafle muru - nakładki: krawędź wierzchu góra / lewa / prawa, lewy / prawy koniec lica, róg wewnętrzny
    public const int TileFloor = 0, TileFloorShadow = 4, TileWall = 6, TileWallFace = 7, TileStairs = 8;
    public const int TileEdgeTop = 9, TileEdgeLeft = 10, TileEdgeRight = 11, TileFaceLeft = 12, TileFaceRight = 13, TileInnerCorner = 14;

    /// <summary>Warianty plamy błota w fx/mud.png (pionowy pasek 32x32).</summary>
    public const int MudVariants = 3;

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
    public static Texture2D TouchIcons => Tex("ui/touch_icons.png");
    public static Texture2D Shadow => Tex("fx/shadow.png");
    public static Texture2D Danger => Tex("fx/danger.png");
    public static Texture2D Range => Tex("fx/range.png");
    public static Texture2D Mud => Tex("fx/mud.png");

    public static Texture2D StageTiles(int stage) => Tex($"tiles/stage_{Mathf.Max(0, stage)}.png");

    public const int AnimWalkFrames = 4, AnimBreath = 4;

    /// <summary>Region klatki animacji (chód 0..3, oddech 4) postaci o klatce bazowej row.</summary>
    public static Rect2 AnimFrame(int row, int k) => new(k * Actor, row * Actor, Actor, Actor);

    /// <summary>Czy klatka ma animację chodu (zawody, problemy budowy, bossowie).</summary>
    public static bool HasWalk(int frame) =>
        frame is >= 0 and < 15 or 46 or 47 or 50 or 52 or 53 or 54 || (frame >= FrameStageEnemy && frame < FrameStageEnemy + StageEnemies)
        || (frame >= FramePreludeEnemy && frame < FramePreludeEnemy + PreludeEnemies)
        || (frame >= FrameSecretClass && frame < FrameSecretClass + SecretClasses)
        || (frame >= FrameStripes && frame < FrameHelmet + 24 && (frame & 1) == 0)
        || (frame >= FrameCareerEnemy && frame < FrameCareerEnemy + 2 * CareerEnemies && (frame & 1) == 0);

    /// <summary>Ikona mechaniki aktu w menu_icons (błoto, porywy, pył; pieczątki Aktu 0).</summary>
    public static int ActIcon(ActMechanic m) => m == ActMechanic.Stamps ? MenuStamps : MenuAct + Mathf.Max(0, (int)m - 1);

    /// <summary>Region klatki size x size w pionowym pasku.</summary>
    public static Rect2 Frame(int index, int size) => new(0, index * size, size, size);

    /// <summary>Druga klatka animacji (anim_b z main.cpp): zawody i wrogowie 0..14 -> +27, bossowie 46-47 -> 48-49, 50 -> 51,
    /// zawody z nagród 52-54 -> 55-57, problemy etapów 61-80 -> 81-100, Akt 0 101-109 -> 110-118,
    /// zawody z sekretów 127-129 -> 130-132, kask w paski 136+2k -> 137+2k (też kontrakty mapy kariery 184+2k -> 185+2k).</summary>
    public static int AnimB(int frame)
    {
        if (frame >= FrameStripes) return frame | 1;
        if (frame >= FrameSecretClass) return frame + SecretClasses;
        if (frame >= FramePreludeEnemy)
        {
            return frame + PreludeEnemies;
        }
        if (frame >= FrameStageEnemy)
        {
            return frame + StageEnemies;
        }
        return frame < 15 ? frame + FrameAnimB : (frame < 48 ? frame + 2 : (frame < 52 ? frame + 1 : frame + 3));
    }

    /// <summary>Sylwetka zablokowanego zawodu (0-5: 20+, zawody z nagród: 58+, z sekretów: 133+).</summary>
    public static int Silhouette(int cls) =>
        cls < 6 ? FrameSilhouette + cls : (cls < 9 ? FrameSilhouetteExt + cls - 6 : FrameSecretSilhouette + cls - 9);

    /// <summary>Klatka bohatera (hero_base z main.cpp): zawód albo – wygląd z sekretnego zlecenia – ten sam zawód w kasku w paski,
    /// albo (v0.21.52) w kasku do pokolorowania (HelmetTint podmienia kolor).</summary>
    public static int HeroFrame(GameData d, Profile p, int cls)
    {
        if (p != null && d.CosmeticStripes >= 0 && Secrets.CosmeticOn(d, p, d.CosmeticStripes)) return FrameStripes + cls * 2;
        return HeroHelmet(d, p, cls) >= 0 ? FrameHelmet + cls * 2 : d.Classes[cls].Frame;
    }

    /// <summary>Kolor kasku bohatera (wygląd z odznaki / zlecenia; -1 = kask zawodu albo kask w paski); cls &gt;= 0: kask
    /// mistrza (v0.21.52 cz. b) tylko zawodem z poziomem mistrzostwa.</summary>
    public static int HeroHelmet(GameData d, Profile p, int cls = -1) =>
        p == null || (d.CosmeticStripes >= 0 && Secrets.CosmeticOn(d, p, d.CosmeticStripes)) ? -1 : Secrets.HelmetCosmetic(d, p, cls);

    /// <summary>Ikona nagrody sekretnego zlecenia w menu_icons (secret_icon z main.cpp; zawód – osobno portretem).</summary>
    public static int SecretIcon(GameData d, SecretDef sd) => sd.Reward switch
    {
        SecretReward.Tool => d.Weapons[d.Tools[sd.Index].Weapon].Knockback ? MenuZenka : MenuLevel,
        SecretReward.Cosmetic => sd.Index == d.CosmeticGold ? MenuGold : MenuStripes,
        SecretReward.Respect => MenuRespect,
        _ => MenuSecret,
    };

    /// <summary>Klatka domu na Osiedlu: wielkość * liczba zawodów + zawód (pusta działka: HouseEmpty).</summary>
    public static int HouseFrame(int house, int classes) => (house >> 4) * classes + (house & 15);

    public static int HouseEmpty(int classes) => 4 * classes;

    /// <summary>Ikona nagrody za odbiór w menu_icons (narzędzie, sprzęt).</summary>
    public static int RewardIcon(GameData d, RewardDef r)
    {
        if (r.Kind == RewardKind.Tool) return d.Weapons[d.Tools[r.Index].Weapon].Elem == Element.Spark ? MenuRewardTool2 : MenuRewardTool;   // Pistolet do kotew
        if (r.Kind == RewardKind.Gear) return d.Gear[r.Index * 3].Stat == GearStat.Thermos ? MenuBelt : MenuBoots;
        if (r.Kind == RewardKind.Act) return MenuStamps;
        return MenuCalendar;
    }

    /// <summary>Klatka znajdźki jak pickup_frame() w main.cpp.</summary>
    public static int PickupFrame(in Pickup p) => p.Type switch
    {
        PickupType.Tool => FrameToolbox,
        PickupType.GearBox => FrameGear + p.Arg % 3,
        PickupType.Document => FrameDocument + p.Arg,
        PickupType.EventTile => FrameEvent,
        PickupType.StoreKey => FrameKey,
        PickupType.Chest => FrameChest,
        _ => FrameCoffee + (int)p.Type,
    };

    public static void DrawFrame(CanvasItem ci, Texture2D tex, int frame, int size, Vector2 topLeft, int scale = 1) =>
        DrawFrame(ci, tex, frame, size, topLeft, scale, Colors.White);

    public static void DrawFrame(CanvasItem ci, Texture2D tex, int frame, int size, Vector2 topLeft, int scale, Color modulate) =>
        ci.DrawTextureRectRegion(tex, new Rect2(topLeft, new Vector2(size * scale, size * scale)), Frame(frame, size), modulate);
}
