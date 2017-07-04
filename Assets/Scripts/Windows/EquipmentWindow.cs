using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LifeLike.Characters;
using LifeLike.Enums;
using LifeLike.Enums.Equipment;
using LifeLike.Inferfaces;
using LifeLike.Interfaces;
using LifeLike.Views;
using UnityEngine;
using UnityEngine.UI;

namespace LifeLike
{
	public class EquipmentWindow : Window
	{

        [SerializeField]
        private ListView _list;
    
        [SerializeField]
        private ListItemBase _listItem;
        private ListItemEquipment _selectedItem;
    	private int _selectedIndex;


        public IList<IEquipment> Equipments { get; set; }

        // Use this for initialization
      
		 
		public override void SetupView()
		{
            Equipments=PlayerManager.Instance.Equipments;
			enabled = true;
            _list.onItemLoaded = HandleOnItemLoadedHandler;		// called when an item is recycled
		    _list.onItemSelected = HandleOnItemSelectedHandler;	// called when the item is selected

    		_list.Create (Equipments.Count(), _listItem); // create the list with a number and a prefab
	    	_list.gameObject.SetActive (true);
		
		}

        private void OnEquipmentSelect(IEquipment SelectedEquipment)
        {
			switch (SelectedEquipment.EquipmentType)
            {
                case EquipmentType.Weapon:
    				PlayerManager.Instance.Statistic.SelectedWeapon=SelectedEquipment as IWeapon;
                
                break;
            }
        }

        public void OnDestroy()
		{
            WindowManager.Instance.Status = WindowState.Close;;
		}

        void HandleOnItemSelectedHandler (ListItemBase item) // reference to the selected list item
        {
            if(_selectedItem != null)
            {
                _selectedItem.Select (false);
            }
    
            _selectedItem = (ListItemEquipment)item;
            _selectedItem.Select (true);
            OnEquipmentSelect(_selectedItem.Object);
           
            _selectedIndex = _selectedItem.Index;
    
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Selected Country | " + Equipments[item.Index].Name);
            #endif
        }
    
        void HandleOnItemLoadedHandler (ListItemBase item) // reference to the loaded list item
        {
            if(item == (ListItemEquipment)_selectedItem)
            {
                _selectedItem.Select (_selectedIndex == _selectedItem.Index);	
            }
                
            ListItemEquipment Equipment = (ListItemEquipment)item;	// cast to your own ListItem
            Equipment.Type = Equipments[item.Index].EquipmentType;
            Equipment.Object=Equipments[item.Index];
        }
		// Update is called once per frame
		void FixUpdate()
        {
            KeyBinding();
        }

        private void KeyBinding()
        {
            if (Input.GetKeyDown(InputManager.Instance.ExitKey)) CloseWindow();
            if (Input.GetKeyDown(InputManager.Instance.OpenDetailWindowKey)) CloseWindow();
        }
    }
}