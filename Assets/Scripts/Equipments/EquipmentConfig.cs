using System;
using System.Collections.Generic;

namespace LifeLike.Equipments
{
    public class EquipmentConfig
    {
        public List<string> Materials {get;set;}

        public List<EquipmentLevel> Types { get; set; }

        internal static EquipmentConfig SampleData()
        {
            return new EquipmentConfig{
                Materials=new List<string>{"Paper","Rusty", "Cotton",  "Poliester", "Metal", "Leather"  ,
                "Titan"} ,          
                Types=new List<EquipmentLevel>{
                    new EquipmentLevel {}, 
                    new EquipmentLevel {}, 
                    new EquipmentLevel {}, 
                     
                }                
            };
        }
    }
}