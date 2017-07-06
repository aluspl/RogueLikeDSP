#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "AssemblyU2DCSharp_LifeLike_Window1496026134.h"

// LifeLike.Views.ListView
struct ListView_t2035967309;
// UnityEngine.UI.Text
struct Text_t356221433;
// LifeLike.Views.ListItemBase
struct ListItemBase_t3737471492;
// LifeLike.Views.ListItemEquipment
struct ListItemEquipment_t2400421815;
// System.Collections.Generic.IList`1<LifeLike.Interfaces.IEquipment>
struct IList_1_t130228914;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// LifeLike.EquipmentWindow
struct  EquipmentWindow_t3804619500  : public Window_t1496026134
{
public:
	// LifeLike.Views.ListView LifeLike.EquipmentWindow::_list
	ListView_t2035967309 * ____list_2;
	// UnityEngine.UI.Text LifeLike.EquipmentWindow::_selectedWeapon
	Text_t356221433 * ____selectedWeapon_3;
	// UnityEngine.UI.Text LifeLike.EquipmentWindow::_selectedArmor
	Text_t356221433 * ____selectedArmor_4;
	// UnityEngine.UI.Text LifeLike.EquipmentWindow::_selectedHead
	Text_t356221433 * ____selectedHead_5;
	// UnityEngine.UI.Text LifeLike.EquipmentWindow::_selectedGloves
	Text_t356221433 * ____selectedGloves_6;
	// UnityEngine.UI.Text LifeLike.EquipmentWindow::_selectedShoes
	Text_t356221433 * ____selectedShoes_7;
	// UnityEngine.UI.Text LifeLike.EquipmentWindow::_selectedPants
	Text_t356221433 * ____selectedPants_8;
	// LifeLike.Views.ListItemBase LifeLike.EquipmentWindow::_listItem
	ListItemBase_t3737471492 * ____listItem_9;
	// LifeLike.Views.ListItemEquipment LifeLike.EquipmentWindow::_selectedItem
	ListItemEquipment_t2400421815 * ____selectedItem_10;
	// System.Int32 LifeLike.EquipmentWindow::_selectedIndex
	int32_t ____selectedIndex_11;
	// System.Collections.Generic.IList`1<LifeLike.Interfaces.IEquipment> LifeLike.EquipmentWindow::<Equipments>k__BackingField
	Il2CppObject* ___U3CEquipmentsU3Ek__BackingField_12;

public:
	inline static int32_t get_offset_of__list_2() { return static_cast<int32_t>(offsetof(EquipmentWindow_t3804619500, ____list_2)); }
	inline ListView_t2035967309 * get__list_2() const { return ____list_2; }
	inline ListView_t2035967309 ** get_address_of__list_2() { return &____list_2; }
	inline void set__list_2(ListView_t2035967309 * value)
	{
		____list_2 = value;
		Il2CppCodeGenWriteBarrier(&____list_2, value);
	}

	inline static int32_t get_offset_of__selectedWeapon_3() { return static_cast<int32_t>(offsetof(EquipmentWindow_t3804619500, ____selectedWeapon_3)); }
	inline Text_t356221433 * get__selectedWeapon_3() const { return ____selectedWeapon_3; }
	inline Text_t356221433 ** get_address_of__selectedWeapon_3() { return &____selectedWeapon_3; }
	inline void set__selectedWeapon_3(Text_t356221433 * value)
	{
		____selectedWeapon_3 = value;
		Il2CppCodeGenWriteBarrier(&____selectedWeapon_3, value);
	}

	inline static int32_t get_offset_of__selectedArmor_4() { return static_cast<int32_t>(offsetof(EquipmentWindow_t3804619500, ____selectedArmor_4)); }
	inline Text_t356221433 * get__selectedArmor_4() const { return ____selectedArmor_4; }
	inline Text_t356221433 ** get_address_of__selectedArmor_4() { return &____selectedArmor_4; }
	inline void set__selectedArmor_4(Text_t356221433 * value)
	{
		____selectedArmor_4 = value;
		Il2CppCodeGenWriteBarrier(&____selectedArmor_4, value);
	}

	inline static int32_t get_offset_of__selectedHead_5() { return static_cast<int32_t>(offsetof(EquipmentWindow_t3804619500, ____selectedHead_5)); }
	inline Text_t356221433 * get__selectedHead_5() const { return ____selectedHead_5; }
	inline Text_t356221433 ** get_address_of__selectedHead_5() { return &____selectedHead_5; }
	inline void set__selectedHead_5(Text_t356221433 * value)
	{
		____selectedHead_5 = value;
		Il2CppCodeGenWriteBarrier(&____selectedHead_5, value);
	}

	inline static int32_t get_offset_of__selectedGloves_6() { return static_cast<int32_t>(offsetof(EquipmentWindow_t3804619500, ____selectedGloves_6)); }
	inline Text_t356221433 * get__selectedGloves_6() const { return ____selectedGloves_6; }
	inline Text_t356221433 ** get_address_of__selectedGloves_6() { return &____selectedGloves_6; }
	inline void set__selectedGloves_6(Text_t356221433 * value)
	{
		____selectedGloves_6 = value;
		Il2CppCodeGenWriteBarrier(&____selectedGloves_6, value);
	}

	inline static int32_t get_offset_of__selectedShoes_7() { return static_cast<int32_t>(offsetof(EquipmentWindow_t3804619500, ____selectedShoes_7)); }
	inline Text_t356221433 * get__selectedShoes_7() const { return ____selectedShoes_7; }
	inline Text_t356221433 ** get_address_of__selectedShoes_7() { return &____selectedShoes_7; }
	inline void set__selectedShoes_7(Text_t356221433 * value)
	{
		____selectedShoes_7 = value;
		Il2CppCodeGenWriteBarrier(&____selectedShoes_7, value);
	}

	inline static int32_t get_offset_of__selectedPants_8() { return static_cast<int32_t>(offsetof(EquipmentWindow_t3804619500, ____selectedPants_8)); }
	inline Text_t356221433 * get__selectedPants_8() const { return ____selectedPants_8; }
	inline Text_t356221433 ** get_address_of__selectedPants_8() { return &____selectedPants_8; }
	inline void set__selectedPants_8(Text_t356221433 * value)
	{
		____selectedPants_8 = value;
		Il2CppCodeGenWriteBarrier(&____selectedPants_8, value);
	}

	inline static int32_t get_offset_of__listItem_9() { return static_cast<int32_t>(offsetof(EquipmentWindow_t3804619500, ____listItem_9)); }
	inline ListItemBase_t3737471492 * get__listItem_9() const { return ____listItem_9; }
	inline ListItemBase_t3737471492 ** get_address_of__listItem_9() { return &____listItem_9; }
	inline void set__listItem_9(ListItemBase_t3737471492 * value)
	{
		____listItem_9 = value;
		Il2CppCodeGenWriteBarrier(&____listItem_9, value);
	}

	inline static int32_t get_offset_of__selectedItem_10() { return static_cast<int32_t>(offsetof(EquipmentWindow_t3804619500, ____selectedItem_10)); }
	inline ListItemEquipment_t2400421815 * get__selectedItem_10() const { return ____selectedItem_10; }
	inline ListItemEquipment_t2400421815 ** get_address_of__selectedItem_10() { return &____selectedItem_10; }
	inline void set__selectedItem_10(ListItemEquipment_t2400421815 * value)
	{
		____selectedItem_10 = value;
		Il2CppCodeGenWriteBarrier(&____selectedItem_10, value);
	}

	inline static int32_t get_offset_of__selectedIndex_11() { return static_cast<int32_t>(offsetof(EquipmentWindow_t3804619500, ____selectedIndex_11)); }
	inline int32_t get__selectedIndex_11() const { return ____selectedIndex_11; }
	inline int32_t* get_address_of__selectedIndex_11() { return &____selectedIndex_11; }
	inline void set__selectedIndex_11(int32_t value)
	{
		____selectedIndex_11 = value;
	}

	inline static int32_t get_offset_of_U3CEquipmentsU3Ek__BackingField_12() { return static_cast<int32_t>(offsetof(EquipmentWindow_t3804619500, ___U3CEquipmentsU3Ek__BackingField_12)); }
	inline Il2CppObject* get_U3CEquipmentsU3Ek__BackingField_12() const { return ___U3CEquipmentsU3Ek__BackingField_12; }
	inline Il2CppObject** get_address_of_U3CEquipmentsU3Ek__BackingField_12() { return &___U3CEquipmentsU3Ek__BackingField_12; }
	inline void set_U3CEquipmentsU3Ek__BackingField_12(Il2CppObject* value)
	{
		___U3CEquipmentsU3Ek__BackingField_12 = value;
		Il2CppCodeGenWriteBarrier(&___U3CEquipmentsU3Ek__BackingField_12, value);
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
