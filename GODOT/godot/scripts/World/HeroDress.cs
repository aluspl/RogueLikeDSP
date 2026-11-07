using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Gfx;
using LifeLike.Game.Settings;

namespace LifeLike.Game.World;

/// <summary>
/// v0.23.0: co bohater ma na sobie i w ręce – nakładki sprzętu (art/gear, kolejność buty → kamizelka → pas → rękawice →
/// kask, z klatką chodu / oddechu bohatera) i broń w prawej dłoni (art/weapons: chwyt na dłoni, kierunek ostatniego ruchu
/// albo ataku, przy „up” za postacią) z krótkim wymachem przy ataku (bez niego przy ograniczonym ruchu).
/// Rysuje ActorSprite w transformacji postaci (odbicie, przysiad przy zanikaniu, szturchnięcie).
/// </summary>
public sealed class HeroDress
{
    /// <summary>Jakość w slocie sprzętu (0-2), -1 = brak (indeks jak equipment.slots).</summary>
    public readonly int[] Gear = [-1, -1, -1, -1, -1];

    public WeaponDef Weapon { get; set; }
    public int Rarity { get; set; }
    public int Dir { get; set; } = WeaponPose.Right;

    private float _swing = -1f;

    public bool HasHelmet => Gear[WeaponSheet.HelmetSlot] >= 0;

    public bool WeaponBehind => WeaponPose.Behind(Dir);

    /// <summary>Wymach przy ataku (WeaponPose.SwingAngle).</summary>
    public void Swing() => _swing = 0f;

    public void Update(float dt)
    {
        if (_swing >= 0f) _swing = _swing > 1f ? -1f : _swing + dt;
    }

    /// <summary>Ustawia z rdzenia: broń, rzadkość i założony sprzęt.</summary>
    public void Sync(LifeLike.Core.Game g)
    {
        Weapon = g.Weapon;
        Rarity = WeaponSheet.Rarity(g);
        for (var s = 0; s < Gear.Length; s++) Gear[s] = s < g.D.GearSlotsCount ? g.Equipped[s] : -1;
    }

    /// <summary>Nakładki sprzętu na klatce bohatera (dst: prostokąt klatki 32x32, anim: kolumna chodu 0-3 / oddech 4).</summary>
    public void DrawGear(CanvasItem ci, Rect2 dst, int anim, Color mod)
    {
        foreach (var s in WeaponSheet.GearDrawOrder)
        {
            if (Gear[s] >= 0) ci.DrawTextureRectRegion(WeaponSheet.Gear, dst, WeaponSheet.GearRegion(s, Mathf.Clamp(Gear[s], 0, 2), anim), mod);
        }
    }

    /// <summary>Broń w dłoni: klatka 32x32 z pikselem chwytu na dłoni (WeaponPose.HandX/Y w klatce bohatera), obrót wymachu
    /// wokół dłoni. baseXf: transformacja postaci (ActorSprite); dst – klatka bohatera w tej transformacji.</summary>
    public void DrawWeapon(CanvasItem ci, Transform2D baseXf, Rect2 dst, Color mod)
    {
        if (Weapon == null) return;
        var row = WeaponSheet.Row(Weapon);
        if (row < 0) return;
        var local = WeaponPose.LocalDir(Dir);
        var grip = WeaponSheet.Grip(Weapon, local);
        var (ox, oy) = WeaponPose.Origin(grip.X, grip.Y);
        var hand = dst.Position + new Vector2(WeaponPose.HandX, WeaponPose.HandY);
        var angle = WeaponPose.SwingAngle(_swing, GameSettings.ReduceMotion);
        if (Dir == WeaponPose.Up || Dir == WeaponPose.Down) angle = -angle * 0.5f; // pionowo – mniejszy łuk
        var xf = baseXf * new Transform2D(Mathf.DegToRad(angle), hand) * new Transform2D(0f, -hand);
        ci.DrawSetTransformMatrix(xf);
        var at = dst.Position + new Vector2(ox, oy);
        ci.DrawTextureRectRegion(WeaponSheet.InHand, new Rect2(at, new Vector2(WeaponSheet.Frame, WeaponSheet.Frame)),
            WeaponSheet.InHandRegion(row, WeaponPose.Column(Rarity, Weapon.Master, Dir)), mod);
        ci.DrawSetTransformMatrix(baseXf);
    }
}
