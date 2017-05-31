using LifeLike.Enums.Equipment;

namespace LifeLike.Interfaces
{
    public interface IEquipment
    {
         string Name { get; set; }
		 EquipmentType EquipmentType {get; set;}
		 string IconImageName {get; set;}
         string SpriteImageName {get; set;}
    }
}