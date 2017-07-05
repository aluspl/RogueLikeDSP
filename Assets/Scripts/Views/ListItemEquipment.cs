using System;
using LifeLike.Enums.Equipment;
using LifeLike.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace LifeLike.Views{
    public class ListItemEquipment : ListItemBase
    {
        public IEquipment Object { get;  set; }
        public EquipmentType Type { get;  set; }


        [SerializeField]
        private Text _itemName;

        [SerializeField]
        private Text _itemType;
        [SerializeField]
        private Text _itemStat;
        // [SerializeField]
        // private Sprite _sprite;


	public void SetItem()
	{
	    SetDetails();
	}

	public void SetDetails()
	{
        _itemName.text = Object.Name;
        _itemType.text=string.Format("Type: {0}",Object.EquipmentType.ToString());
        _itemStat.text=Object.Stats;
        
		// if(_sprite != null)
		// {
		// 	Resources.UnloadAsset (_sprite.texture);
		// }

		// _sprite = Resources.Load<Sprite> ("weapontypes/" + Object.EquipmentType);

		// _image.sprite = _sprite;
	}

	public void Select(bool selected)
	{
       // GetComponent<Image>().color=selected ? Color.gray : Color.white;
	}

    }
}