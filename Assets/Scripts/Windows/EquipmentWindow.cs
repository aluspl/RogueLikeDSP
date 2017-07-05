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
        private Text _selectedWeapon;
        [SerializeField]
        private Text _selectedArmor;
        [SerializeField]
        private Text _selectedHead;
        [SerializeField]
        private Text _selectedGloves;
        [SerializeField]
        private Text _selectedShoes;
        [SerializeField]
        private Text _selectedPants;
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
            Debug.Log(string.Format("Selected Equipment: {0} Type {1}", SelectedEquipment.Name ,SelectedEquipment.EquipmentType));
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
            Equipment.Object=Equipments[item.Index];
            Equipment.SetDetails();
        }
		void FixedUpdate()
        {
            KeyBinding();
            UpdateControls();
        }
        public Character Player {get {return PlayerManager.Instance.Statistic;} }
        private void UpdateControls()
        {
            if (_selectedWeapon != null)  _selectedWeapon.text = SetupText(EquipmentType.Weapon, Player.SelectedWeapon);
            if (_selectedArmor!=null) _selectedArmor.text= SetupText(EquipmentType.Armor, Player.SelectedArmor);
            if (_selectedPants!=null) _selectedPants.text= SetupText(EquipmentType.Pants, Player.SelectedPants);
            if (_selectedGloves!=null) _selectedGloves.text=SetupText(EquipmentType.Gloves, Player.SelectedGloves);
            if (_selectedShoes!=null) _selectedShoes.text=SetupText(EquipmentType.Shoes, Player.SelectedShoes);
            if (_selectedHead!=null) _selectedHead.text=SetupText(EquipmentType.Head, Player.SelectedHead);

        }

        private string SetupText(EquipmentType type, IEquipment equipment)
        {
            var name=equipment!=null ? equipment.Name : "none";
            return string.Format("{0}: {1}", type,name);
         }

        private void KeyBinding()
        {
            if (Input.GetKeyDown(InputManager.Instance.ExitKey)) CloseWindow();
            if (Input.GetKeyDown(InputManager.Instance.OpenEquipmentWindowKey)) CloseWindow();
        }
    }
}