using System.Collections.Generic;
using System.Text.Json;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;

namespace LifeLike.Game.Gfx;

/// <summary>
/// v0.23.0: arkusze broni i sprzętu z art/weapons i art/gear (kopie w assets/weapons, assets/gear – eksport
/// GODOT/tools/export_godot_assets.py). Manifest weapons.json: wiersz broni, piksel chwytu w kierunku, wiersze zamachu.
/// Broń w ręce 32x32 (kolumna = rzadkość * 4 + kierunek), ikony 32x32 / 16x16 (kolumna = rzadkość), zamach 64x64
/// (6 klatek, tylko Markowa i Mistrzowska), nakładki sprzętu 32x32 (wiersz = slot * 3 + rzadkość, kolumna = klatka chodu).
/// </summary>
public static class WeaponSheet
{
    public const int Frame = 32, SwingFrame = 64, SwingFrames = 6, GearFrame = 32;

    /// <summary>Kolejność rysowania nakładek sprzętu (gear.json drawOrder): buty, kamizelka, pas, rękawice, kask.</summary>
    public static readonly int[] GearDrawOrder = [3, 2, 4, 1, 0];

    public const int HelmetSlot = 0, GearSlots = 5;

    private static Dictionary<string, Entry> _byId;
    private static Dictionary<string, int> _swing;

    private sealed class Entry
    {
        public int Row;
        public Vector2I[] Grip = new Vector2I[4];
    }

    public static Texture2D InHand => Assets.Tex("weapons/inhand.png");
    public static Texture2D Icons => Assets.Tex("weapons/icons.png");
    public static Texture2D Icons1x => Assets.Tex("weapons/icons_1x.png");
    public static Texture2D Swing => Assets.Tex("weapons/swing.png");
    public static Texture2D Gear => Assets.Tex("gear/overlay.png");

    private static void Load()
    {
        if (_byId != null) return;
        _byId = new Dictionary<string, Entry>();
        _swing = new Dictionary<string, int>();
        var text = FileAccess.GetFileAsString("res://assets/weapons/weapons.json");
        if (string.IsNullOrEmpty(text)) return;
        using var doc = JsonDocument.Parse(text);
        string[] dirs = ["down", "up", "left", "right"];
        foreach (var w in doc.RootElement.GetProperty("weapons").EnumerateArray())
        {
            var e = new Entry { Row = w.GetProperty("row").GetInt32() };
            var g = w.GetProperty("gripMapGodot"); // assets/weapons/inhand.png = weapons_inhand_map_2x.png (broń 3/4)
            for (var d = 0; d < 4; d++)
            {
                var p = g.GetProperty(dirs[d]);
                e.Grip[d] = new Vector2I(p[0].GetInt32(), p[1].GetInt32());
            }
            _byId[w.GetProperty("id").GetString() ?? ""] = e;
        }
        var keys = new List<string>();
        foreach (var k in doc.RootElement.GetProperty("rarityKeys").EnumerateArray()) keys.Add(k.GetString());
        var i = 0;
        foreach (var s in doc.RootElement.GetProperty("swingRows").EnumerateArray())
            _swing[s.GetProperty("id").GetString() + ":" + keys.IndexOf(s.GetProperty("rarity").GetString())] = i++;
    }

    /// <summary>Wiersz broni w arkuszach (-1 = brak rysunku).</summary>
    public static int Row(WeaponDef w)
    {
        Load();
        return _byId.TryGetValue(w.Id, out var e) ? e.Row : -1;
    }

    /// <summary>Piksel chwytu klatki 32x32 w kierunku (WeaponPose.Down..Right).</summary>
    public static Vector2I Grip(WeaponDef w, int dir)
    {
        Load();
        return _byId.TryGetValue(w.Id, out var e) ? e.Grip[dir] : new Vector2I(16, 16);
    }

    /// <summary>Wiersz zamachu (Markowa / Mistrzowska; -1 = brak – Zwykła i Solidna nie mają animacji).</summary>
    public static int SwingRow(WeaponDef w, int rarity)
    {
        Load();
        var key = w.Id + ":" + (w.Master ? 3 : rarity);
        return _swing.TryGetValue(key, out var r) ? r : -1;
    }

    /// <summary>Rzadkość do rysowania: broń mistrza i broń z mistrzostwa zawodu – Mistrzowska.</summary>
    public static int Rarity(LifeLike.Core.Game g) => g.WeaponMaster() ? 3 : g.WeaponRarity;

    /// <summary>Region klatki broni w ręce.</summary>
    public static Rect2 InHandRegion(int row, int column) => new(column * Frame, row * Frame, Frame, Frame);

    /// <summary>Region klatki zamachu.</summary>
    public static Rect2 SwingRegion(int row, int k) => new(k * SwingFrame, row * SwingFrame, SwingFrame, SwingFrame);

    /// <summary>Region nakładki sprzętu: slot, rzadkość (0-2), klatka chodu 0-3 / oddech 4 (kolumna „down”).</summary>
    public static Rect2 GearRegion(int slot, int rarity, int anim) => new(anim * GearFrame, (slot * 3 + rarity) * GearFrame, GearFrame, GearFrame);

    /// <summary>Ikona broni w kolorze rzadkości (32x32 przy skali ≥ 2 / dużym polu, inaczej 16x16), w prostokącie dst.
    /// false – broń bez rysunku (wtedy rysuj starą ikonę ataku).</summary>
    public static bool DrawIcon(CanvasItem ci, WeaponDef w, int rarity, Rect2 dst, Color modulate)
    {
        var row = Row(w);
        if (row < 0) return false;
        var col = w.Master ? 3 : Mathf.Clamp(rarity, 0, 3);
        var big = dst.Size.X > 20;
        var s = big ? Frame : Assets.Icon;
        ci.DrawTextureRectRegion(big ? Icons : Icons1x, dst, new Rect2(col * s, row * s, s, s), modulate);
        return true;
    }

    public static bool DrawIcon(CanvasItem ci, WeaponDef w, int rarity, Rect2 dst) => DrawIcon(ci, w, rarity, dst, Colors.White);
}
