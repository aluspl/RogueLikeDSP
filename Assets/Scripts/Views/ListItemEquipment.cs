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
	private Image _image;

	[SerializeField]
	private Text _itemName;

	[SerializeField]
	private Text _labelCode;


	private Sprite _sprite;


	public void SetItem()
	{
	    SetDetails();
	}

	public void SetDetails()
	{
        _itemName.text = Object.Name;
    
		if(_sprite != null)
		{
			Resources.UnloadAsset (_sprite.texture);
		}

		_sprite = Resources.Load<Sprite> ("flags/" + Object.SpriteImageName);

		_image.sprite = _sprite;
	}

	public void Select(bool selected)
	{
		_itemName.color = selected ? Color.green : Color.black;
	}

    }
}