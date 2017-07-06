#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "UnityEngine_UnityEngine_MonoBehaviour1158329972.h"
#include "AssemblyU2DCSharp_LifeLike_Enums_WindowState473668005.h"

// LifeLike.CreateWindow
struct CreateWindow_t1062208490;
// LifeLike.DetailWindow
struct DetailWindow_t4290421825;
// LifeLike.EquipmentWindow
struct EquipmentWindow_t3804619500;
// LifeLike.Inferfaces.IWindowManager
struct IWindowManager_t2527860612;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// LifeLike.WindowManager
struct  WindowManager_t1276248557  : public MonoBehaviour_t1158329972
{
public:
	// LifeLike.CreateWindow LifeLike.WindowManager::CreateWindowPrefab
	CreateWindow_t1062208490 * ___CreateWindowPrefab_2;
	// LifeLike.DetailWindow LifeLike.WindowManager::DetailWindowPrefab
	DetailWindow_t4290421825 * ___DetailWindowPrefab_3;
	// LifeLike.EquipmentWindow LifeLike.WindowManager::EquipmentWindowPrefab
	EquipmentWindow_t3804619500 * ___EquipmentWindowPrefab_4;
	// LifeLike.Enums.WindowState LifeLike.WindowManager::<Status>k__BackingField
	int32_t ___U3CStatusU3Ek__BackingField_6;

public:
	inline static int32_t get_offset_of_CreateWindowPrefab_2() { return static_cast<int32_t>(offsetof(WindowManager_t1276248557, ___CreateWindowPrefab_2)); }
	inline CreateWindow_t1062208490 * get_CreateWindowPrefab_2() const { return ___CreateWindowPrefab_2; }
	inline CreateWindow_t1062208490 ** get_address_of_CreateWindowPrefab_2() { return &___CreateWindowPrefab_2; }
	inline void set_CreateWindowPrefab_2(CreateWindow_t1062208490 * value)
	{
		___CreateWindowPrefab_2 = value;
		Il2CppCodeGenWriteBarrier(&___CreateWindowPrefab_2, value);
	}

	inline static int32_t get_offset_of_DetailWindowPrefab_3() { return static_cast<int32_t>(offsetof(WindowManager_t1276248557, ___DetailWindowPrefab_3)); }
	inline DetailWindow_t4290421825 * get_DetailWindowPrefab_3() const { return ___DetailWindowPrefab_3; }
	inline DetailWindow_t4290421825 ** get_address_of_DetailWindowPrefab_3() { return &___DetailWindowPrefab_3; }
	inline void set_DetailWindowPrefab_3(DetailWindow_t4290421825 * value)
	{
		___DetailWindowPrefab_3 = value;
		Il2CppCodeGenWriteBarrier(&___DetailWindowPrefab_3, value);
	}

	inline static int32_t get_offset_of_EquipmentWindowPrefab_4() { return static_cast<int32_t>(offsetof(WindowManager_t1276248557, ___EquipmentWindowPrefab_4)); }
	inline EquipmentWindow_t3804619500 * get_EquipmentWindowPrefab_4() const { return ___EquipmentWindowPrefab_4; }
	inline EquipmentWindow_t3804619500 ** get_address_of_EquipmentWindowPrefab_4() { return &___EquipmentWindowPrefab_4; }
	inline void set_EquipmentWindowPrefab_4(EquipmentWindow_t3804619500 * value)
	{
		___EquipmentWindowPrefab_4 = value;
		Il2CppCodeGenWriteBarrier(&___EquipmentWindowPrefab_4, value);
	}

	inline static int32_t get_offset_of_U3CStatusU3Ek__BackingField_6() { return static_cast<int32_t>(offsetof(WindowManager_t1276248557, ___U3CStatusU3Ek__BackingField_6)); }
	inline int32_t get_U3CStatusU3Ek__BackingField_6() const { return ___U3CStatusU3Ek__BackingField_6; }
	inline int32_t* get_address_of_U3CStatusU3Ek__BackingField_6() { return &___U3CStatusU3Ek__BackingField_6; }
	inline void set_U3CStatusU3Ek__BackingField_6(int32_t value)
	{
		___U3CStatusU3Ek__BackingField_6 = value;
	}
};

struct WindowManager_t1276248557_StaticFields
{
public:
	// LifeLike.Inferfaces.IWindowManager LifeLike.WindowManager::Instance
	Il2CppObject * ___Instance_5;

public:
	inline static int32_t get_offset_of_Instance_5() { return static_cast<int32_t>(offsetof(WindowManager_t1276248557_StaticFields, ___Instance_5)); }
	inline Il2CppObject * get_Instance_5() const { return ___Instance_5; }
	inline Il2CppObject ** get_address_of_Instance_5() { return &___Instance_5; }
	inline void set_Instance_5(Il2CppObject * value)
	{
		___Instance_5 = value;
		Il2CppCodeGenWriteBarrier(&___Instance_5, value);
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
