using System.Collections.Generic;
using LifeLike.Characters;
using LifeLike.Controls;
using LifeLike.Enums.Equipment;
using LifeLike.Interfaces;

namespace LifeLike.Inferfaces
{
    public interface IPlayerManager : IManager
    {
         Character Statistic {get; set;}

         Player Object  {get; set;}

        List<IEquipment> GetEquipments(EquipmentType weapon);
    }
}