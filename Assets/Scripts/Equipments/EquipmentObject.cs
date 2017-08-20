using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LifeLike.Enums;
using LifeLike.Interfaces;
using LifeLike.Enums.Equipment;
using System;

namespace LifeLike.Equipments{

	public  class EquipmentObject : MonoBehaviour {

     
        public void SetEquipment(IEquipment equipment)
        {
            Equipment=equipment;

        
            if (Equipment!=null)
            {
               var  sprite = Resources.Load<Sprite>("Equipment/" + Equipment.IconImageName);
                Debug.Log(string.Format("Sprite is null: {0}? ",sprite==null));
                GetComponent<SpriteRenderer>().sprite=sprite;

            }
            else
                Debug.Log("Can't Load sprite ");    
        }
        public IEquipment Equipment {get;set;}
		public static string Tag = "EQUIPMENT";
        void OnTriggerEnter2D(Collider2D other) {
            
        	if (other.tag==Controls.Player.Tag)
			{
                if (Equipment==null) return;
                switch (Equipment.EquipmentType)
                {
                    case EquipmentType.Health: 
                        PlayerManager.Instance.RecoverHP(Equipment as IHealth);
                        Debug.Log("Restore Health");

                        Destroy(this.gameObject);
                        break;
                    case EquipmentType.Stamina: 
                        PlayerManager.Instance.RecoverStamina(Equipment as IStamina);
                        Debug.Log("Restore Stamina");

                        Destroy(this.gameObject);
                        break;
                    
                }
			}
		}
    }
}
