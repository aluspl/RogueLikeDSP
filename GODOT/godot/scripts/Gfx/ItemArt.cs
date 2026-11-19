using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;

namespace LifeLike.Game.Gfx;

/// <summary>
/// v0.23.2 (UI 2): obrazki przedmiotów w kaflach plecaka i slotach postaci – broń ikoną 32x32 w kolorze rzadkości, sprzęt
/// kawałkiem nakładki z art/gear (kask, kamizelka, buty, pas; rękawice – paczka sprzętu) powiększonym całkowicie do kafla; kolor rzadkości
/// wspólny dla broni i sprzętu (WeaponLook.RarityColor).
/// </summary>
public static class ItemArt
{
    private static readonly Dictionary<int, Rect2I> Crops = new();
    private static Image _gear;

    /// <summary>Kolor rzadkości przedmiotu (sprzęt: Zwykły / Solidny / Markowy jak bronie 0-2).</summary>
    public static Color Rarity(GameData d, BackpackItem it) => WeaponLook.RarityColor(d, it.Kind == BackpackKind.Weapon && d.Weapons[it.Id].Master ? d.WeaponRarities.Length - 1 : it.Rarity);

    /// <summary>Nazwa rzadkości przedmiotu.</summary>
    public static string RarityName(GameData d, BackpackItem it) =>
        it.Kind == BackpackKind.Weapon ? WeaponLook.RarityName(d, d.Weapons[it.Id].Master ? d.WeaponRarities.Length - 1 : it.Rarity)
        : d.GearRarities[Mathf.Clamp(it.Rarity, 0, d.GearRarities.Length - 1)];

    /// <summary>Kolor tekstu rzadkości na jasnej karcie.</summary>
    public static Ink RarityInk(GameData d, BackpackItem it) =>
        WeaponLook.PhoneInk(d, it.Kind == BackpackKind.Weapon && d.Weapons[it.Id].Master ? d.WeaponRarities.Length - 1 : it.Rarity);

    /// <summary>Przedmiot wyśrodkowany w box (bez tła); alpha &lt; 1 = wyszarzony.</summary>
    public static void Draw(CanvasItem c, GameData d, BackpackItem it, Rect2 box, float alpha = 1f)
    {
        if (it.Kind == BackpackKind.Weapon)
        {
            var s = box.Size.X >= 32 && box.Size.Y >= 32 ? 32 : 16;
            var r = new Rect2((box.GetCenter() - new Vector2(s, s) / 2).Round(), new Vector2(s, s));
            WeaponSheet.DrawIcon(c, d.Weapons[it.Id], it.Rarity, r, new Color(1, 1, 1, alpha));
            return;
        }
        if (it.Kind == BackpackKind.Gear) Gear(c, it.Id, it.Rarity, box, alpha);
    }

    /// <summary>Sprzęt slotu w jakości rarity jako ikona (wycinek nakładki), wyśrodkowany i powiększony całkowicie do box.</summary>
    public static void Gear(CanvasItem c, int slot, int rarity, Rect2 box, float alpha = 1f)
    {
        var row = slot * 3 + Mathf.Clamp(rarity, 0, 2);
        var crop = slot == 1 ? new Rect2I() : Crop(row, slot); // rękawice na postaci to dwa piksele – w kaflu paczka sprzętu
        if (crop.Size.X <= 0)
        {
            var bs = box.Size.X >= 32 && box.Size.Y >= 32 ? 32 : 24;
            c.DrawTextureRectRegion(Assets.Actors, new Rect2((box.GetCenter() - new Vector2(bs, bs) / 2).Round(), new Vector2(bs, bs)), Assets.Frame(Assets.FrameGear + Mathf.Clamp(rarity, 0, 2), Assets.Actor), new Color(1, 1, 1, alpha));
            return;
        }
        var k = Mathf.Max(1, Mathf.Min((int)(box.Size.X / crop.Size.X), (int)(box.Size.Y / crop.Size.Y)));
        k = Mathf.Min(k, box.Size.X >= 40 ? 3 : 2);
        var size = new Vector2(crop.Size.X, crop.Size.Y) * k;
        c.DrawTextureRectRegion(WeaponSheet.Gear, new Rect2((box.GetCenter() - size / 2).Round(), size), new Rect2(crop.Position, crop.Size), new Color(1, 1, 1, alpha));
    }

    /// <summary>Wycinek nakładki: kolumna 0 (stoi), obszar z pikselami; buty – tylko lewa połowa (jedna sztuka).</summary>
    private static Rect2I Crop(int row, int slot)
    {
        if (Crops.TryGetValue(row, out var r)) return r;
        if (_gear is null)
        {
            _gear = WeaponSheet.Gear?.GetImage();
            if (_gear is not null && _gear.IsCompressed()) _gear.Decompress();
        }
        r = new Rect2I();
        if (_gear is not null && (row + 1) * WeaponSheet.GearFrame <= _gear.GetHeight())
        {
            var half = slot == 3; // buty
            var frame = new Rect2I(0, row * WeaponSheet.GearFrame, half ? WeaponSheet.GearFrame / 2 : WeaponSheet.GearFrame, WeaponSheet.GearFrame);
            var used = _gear.GetRegion(frame).GetUsedRect();
            if (used.Size.X > 0) r = new Rect2I(frame.Position + used.Position, used.Size);
        }
        Crops[row] = r;
        return r;
    }

    public static void ClearCache()
    {
        Crops.Clear();
        _gear = null;
    }
}
