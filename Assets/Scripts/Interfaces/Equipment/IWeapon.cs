using LifeLike.Enums.Equipment;
using LifeLike.Interfaces;

namespace LifeLike.Inferfaces
{
    public interface IWeapon : IEquipment
    {
         int Attack { get; set; }
         WeaponType WeaponType {get; set;}
         bool IsRange {get; set; }
         int Range {get; set; }
         
    }
}