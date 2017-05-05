#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "UnityEngine_UnityEngine_MonoBehaviour1158329972.h"

// System.Collections.Generic.List`1<UnityEngine.GameObject>
struct List_1_t1125654279;
// System.Collections.Generic.List`1<Controls.Enemy>
struct List_1_t1382725646;
// UnityEngine.GameObject
struct GameObject_t1756533147;
// Controls.Player
struct Player_t109330733;
// System.Random
struct Random_t1044426839;
// Enums.MapElement[0...,0...]
struct MapElementU5BU2CU5D_t632957514;
// UnityEngine.Transform
struct Transform_t3275118058;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// MapManager
struct  MapManager_t3593696545  : public MonoBehaviour_t1158329972
{
public:
	// System.Collections.Generic.List`1<UnityEngine.GameObject> MapManager::Walls
	List_1_t1125654279 * ___Walls_2;
	// System.Collections.Generic.List`1<UnityEngine.GameObject> MapManager::Floors
	List_1_t1125654279 * ___Floors_3;
	// System.Collections.Generic.List`1<UnityEngine.GameObject> MapManager::Doors
	List_1_t1125654279 * ___Doors_4;
	// System.Collections.Generic.List`1<Controls.Enemy> MapManager::Enemies
	List_1_t1382725646 * ___Enemies_5;
	// UnityEngine.GameObject MapManager::EndPoint
	GameObject_t1756533147 * ___EndPoint_6;
	// Controls.Player MapManager::Player
	Player_t109330733 * ___Player_7;
	// System.Int32 MapManager::MapSizeX
	int32_t ___MapSizeX_8;
	// System.Int32 MapManager::MapSizeY
	int32_t ___MapSizeY_9;
	// System.Int32 MapManager::MaxEnemies
	int32_t ___MaxEnemies_10;
	// System.Int32 MapManager::MaxHorizontalLines
	int32_t ___MaxHorizontalLines_11;
	// System.Int32 MapManager::MaxVerticalLines
	int32_t ___MaxVerticalLines_12;
	// System.Int32 MapManager::MinimalWallSize
	int32_t ___MinimalWallSize_13;
	// System.Random MapManager::_random
	Random_t1044426839 * ____random_14;
	// Enums.MapElement[0...,0...] MapManager::<Map>k__BackingField
	MapElementU5BU2CU5D_t632957514* ___U3CMapU3Ek__BackingField_15;
	// UnityEngine.Transform MapManager::<EnemiesCollection>k__BackingField
	Transform_t3275118058 * ___U3CEnemiesCollectionU3Ek__BackingField_16;
	// UnityEngine.Transform MapManager::<MapCollection>k__BackingField
	Transform_t3275118058 * ___U3CMapCollectionU3Ek__BackingField_17;

public:
	inline static int32_t get_offset_of_Walls_2() { return static_cast<int32_t>(offsetof(MapManager_t3593696545, ___Walls_2)); }
	inline List_1_t1125654279 * get_Walls_2() const { return ___Walls_2; }
	inline List_1_t1125654279 ** get_address_of_Walls_2() { return &___Walls_2; }
	inline void set_Walls_2(List_1_t1125654279 * value)
	{
		___Walls_2 = value;
		Il2CppCodeGenWriteBarrier(&___Walls_2, value);
	}

	inline static int32_t get_offset_of_Floors_3() { return static_cast<int32_t>(offsetof(MapManager_t3593696545, ___Floors_3)); }
	inline List_1_t1125654279 * get_Floors_3() const { return ___Floors_3; }
	inline List_1_t1125654279 ** get_address_of_Floors_3() { return &___Floors_3; }
	inline void set_Floors_3(List_1_t1125654279 * value)
	{
		___Floors_3 = value;
		Il2CppCodeGenWriteBarrier(&___Floors_3, value);
	}

	inline static int32_t get_offset_of_Doors_4() { return static_cast<int32_t>(offsetof(MapManager_t3593696545, ___Doors_4)); }
	inline List_1_t1125654279 * get_Doors_4() const { return ___Doors_4; }
	inline List_1_t1125654279 ** get_address_of_Doors_4() { return &___Doors_4; }
	inline void set_Doors_4(List_1_t1125654279 * value)
	{
		___Doors_4 = value;
		Il2CppCodeGenWriteBarrier(&___Doors_4, value);
	}

	inline static int32_t get_offset_of_Enemies_5() { return static_cast<int32_t>(offsetof(MapManager_t3593696545, ___Enemies_5)); }
	inline List_1_t1382725646 * get_Enemies_5() const { return ___Enemies_5; }
	inline List_1_t1382725646 ** get_address_of_Enemies_5() { return &___Enemies_5; }
	inline void set_Enemies_5(List_1_t1382725646 * value)
	{
		___Enemies_5 = value;
		Il2CppCodeGenWriteBarrier(&___Enemies_5, value);
	}

	inline static int32_t get_offset_of_EndPoint_6() { return static_cast<int32_t>(offsetof(MapManager_t3593696545, ___EndPoint_6)); }
	inline GameObject_t1756533147 * get_EndPoint_6() const { return ___EndPoint_6; }
	inline GameObject_t1756533147 ** get_address_of_EndPoint_6() { return &___EndPoint_6; }
	inline void set_EndPoint_6(GameObject_t1756533147 * value)
	{
		___EndPoint_6 = value;
		Il2CppCodeGenWriteBarrier(&___EndPoint_6, value);
	}

	inline static int32_t get_offset_of_Player_7() { return static_cast<int32_t>(offsetof(MapManager_t3593696545, ___Player_7)); }
	inline Player_t109330733 * get_Player_7() const { return ___Player_7; }
	inline Player_t109330733 ** get_address_of_Player_7() { return &___Player_7; }
	inline void set_Player_7(Player_t109330733 * value)
	{
		___Player_7 = value;
		Il2CppCodeGenWriteBarrier(&___Player_7, value);
	}

	inline static int32_t get_offset_of_MapSizeX_8() { return static_cast<int32_t>(offsetof(MapManager_t3593696545, ___MapSizeX_8)); }
	inline int32_t get_MapSizeX_8() const { return ___MapSizeX_8; }
	inline int32_t* get_address_of_MapSizeX_8() { return &___MapSizeX_8; }
	inline void set_MapSizeX_8(int32_t value)
	{
		___MapSizeX_8 = value;
	}

	inline static int32_t get_offset_of_MapSizeY_9() { return static_cast<int32_t>(offsetof(MapManager_t3593696545, ___MapSizeY_9)); }
	inline int32_t get_MapSizeY_9() const { return ___MapSizeY_9; }
	inline int32_t* get_address_of_MapSizeY_9() { return &___MapSizeY_9; }
	inline void set_MapSizeY_9(int32_t value)
	{
		___MapSizeY_9 = value;
	}

	inline static int32_t get_offset_of_MaxEnemies_10() { return static_cast<int32_t>(offsetof(MapManager_t3593696545, ___MaxEnemies_10)); }
	inline int32_t get_MaxEnemies_10() const { return ___MaxEnemies_10; }
	inline int32_t* get_address_of_MaxEnemies_10() { return &___MaxEnemies_10; }
	inline void set_MaxEnemies_10(int32_t value)
	{
		___MaxEnemies_10 = value;
	}

	inline static int32_t get_offset_of_MaxHorizontalLines_11() { return static_cast<int32_t>(offsetof(MapManager_t3593696545, ___MaxHorizontalLines_11)); }
	inline int32_t get_MaxHorizontalLines_11() const { return ___MaxHorizontalLines_11; }
	inline int32_t* get_address_of_MaxHorizontalLines_11() { return &___MaxHorizontalLines_11; }
	inline void set_MaxHorizontalLines_11(int32_t value)
	{
		___MaxHorizontalLines_11 = value;
	}

	inline static int32_t get_offset_of_MaxVerticalLines_12() { return static_cast<int32_t>(offsetof(MapManager_t3593696545, ___MaxVerticalLines_12)); }
	inline int32_t get_MaxVerticalLines_12() const { return ___MaxVerticalLines_12; }
	inline int32_t* get_address_of_MaxVerticalLines_12() { return &___MaxVerticalLines_12; }
	inline void set_MaxVerticalLines_12(int32_t value)
	{
		___MaxVerticalLines_12 = value;
	}

	inline static int32_t get_offset_of_MinimalWallSize_13() { return static_cast<int32_t>(offsetof(MapManager_t3593696545, ___MinimalWallSize_13)); }
	inline int32_t get_MinimalWallSize_13() const { return ___MinimalWallSize_13; }
	inline int32_t* get_address_of_MinimalWallSize_13() { return &___MinimalWallSize_13; }
	inline void set_MinimalWallSize_13(int32_t value)
	{
		___MinimalWallSize_13 = value;
	}

	inline static int32_t get_offset_of__random_14() { return static_cast<int32_t>(offsetof(MapManager_t3593696545, ____random_14)); }
	inline Random_t1044426839 * get__random_14() const { return ____random_14; }
	inline Random_t1044426839 ** get_address_of__random_14() { return &____random_14; }
	inline void set__random_14(Random_t1044426839 * value)
	{
		____random_14 = value;
		Il2CppCodeGenWriteBarrier(&____random_14, value);
	}

	inline static int32_t get_offset_of_U3CMapU3Ek__BackingField_15() { return static_cast<int32_t>(offsetof(MapManager_t3593696545, ___U3CMapU3Ek__BackingField_15)); }
	inline MapElementU5BU2CU5D_t632957514* get_U3CMapU3Ek__BackingField_15() const { return ___U3CMapU3Ek__BackingField_15; }
	inline MapElementU5BU2CU5D_t632957514** get_address_of_U3CMapU3Ek__BackingField_15() { return &___U3CMapU3Ek__BackingField_15; }
	inline void set_U3CMapU3Ek__BackingField_15(MapElementU5BU2CU5D_t632957514* value)
	{
		___U3CMapU3Ek__BackingField_15 = value;
		Il2CppCodeGenWriteBarrier(&___U3CMapU3Ek__BackingField_15, value);
	}

	inline static int32_t get_offset_of_U3CEnemiesCollectionU3Ek__BackingField_16() { return static_cast<int32_t>(offsetof(MapManager_t3593696545, ___U3CEnemiesCollectionU3Ek__BackingField_16)); }
	inline Transform_t3275118058 * get_U3CEnemiesCollectionU3Ek__BackingField_16() const { return ___U3CEnemiesCollectionU3Ek__BackingField_16; }
	inline Transform_t3275118058 ** get_address_of_U3CEnemiesCollectionU3Ek__BackingField_16() { return &___U3CEnemiesCollectionU3Ek__BackingField_16; }
	inline void set_U3CEnemiesCollectionU3Ek__BackingField_16(Transform_t3275118058 * value)
	{
		___U3CEnemiesCollectionU3Ek__BackingField_16 = value;
		Il2CppCodeGenWriteBarrier(&___U3CEnemiesCollectionU3Ek__BackingField_16, value);
	}

	inline static int32_t get_offset_of_U3CMapCollectionU3Ek__BackingField_17() { return static_cast<int32_t>(offsetof(MapManager_t3593696545, ___U3CMapCollectionU3Ek__BackingField_17)); }
	inline Transform_t3275118058 * get_U3CMapCollectionU3Ek__BackingField_17() const { return ___U3CMapCollectionU3Ek__BackingField_17; }
	inline Transform_t3275118058 ** get_address_of_U3CMapCollectionU3Ek__BackingField_17() { return &___U3CMapCollectionU3Ek__BackingField_17; }
	inline void set_U3CMapCollectionU3Ek__BackingField_17(Transform_t3275118058 * value)
	{
		___U3CMapCollectionU3Ek__BackingField_17 = value;
		Il2CppCodeGenWriteBarrier(&___U3CMapCollectionU3Ek__BackingField_17, value);
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
