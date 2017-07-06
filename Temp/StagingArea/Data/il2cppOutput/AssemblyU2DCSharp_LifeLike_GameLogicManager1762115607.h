#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "UnityEngine_UnityEngine_MonoBehaviour1158329972.h"

// LifeLike.MapUtils.MapManager
struct MapManager_t2333778437;
// LifeLike.Inferfaces.IGameLogicManager
struct IGameLogicManager_t3547714540;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// LifeLike.GameLogicManager
struct  GameLogicManager_t1762115607  : public MonoBehaviour_t1158329972
{
public:
	// LifeLike.MapUtils.MapManager LifeLike.GameLogicManager::MapManager
	MapManager_t2333778437 * ___MapManager_2;
	// System.Int32 LifeLike.GameLogicManager::_level
	int32_t ____level_4;
	// System.Boolean LifeLike.GameLogicManager::<IsDay>k__BackingField
	bool ___U3CIsDayU3Ek__BackingField_5;
	// System.Boolean LifeLike.GameLogicManager::<IsPlayerTurn>k__BackingField
	bool ___U3CIsPlayerTurnU3Ek__BackingField_6;
	// System.Boolean LifeLike.GameLogicManager::<IsEnemyTurn>k__BackingField
	bool ___U3CIsEnemyTurnU3Ek__BackingField_7;

public:
	inline static int32_t get_offset_of_MapManager_2() { return static_cast<int32_t>(offsetof(GameLogicManager_t1762115607, ___MapManager_2)); }
	inline MapManager_t2333778437 * get_MapManager_2() const { return ___MapManager_2; }
	inline MapManager_t2333778437 ** get_address_of_MapManager_2() { return &___MapManager_2; }
	inline void set_MapManager_2(MapManager_t2333778437 * value)
	{
		___MapManager_2 = value;
		Il2CppCodeGenWriteBarrier(&___MapManager_2, value);
	}

	inline static int32_t get_offset_of__level_4() { return static_cast<int32_t>(offsetof(GameLogicManager_t1762115607, ____level_4)); }
	inline int32_t get__level_4() const { return ____level_4; }
	inline int32_t* get_address_of__level_4() { return &____level_4; }
	inline void set__level_4(int32_t value)
	{
		____level_4 = value;
	}

	inline static int32_t get_offset_of_U3CIsDayU3Ek__BackingField_5() { return static_cast<int32_t>(offsetof(GameLogicManager_t1762115607, ___U3CIsDayU3Ek__BackingField_5)); }
	inline bool get_U3CIsDayU3Ek__BackingField_5() const { return ___U3CIsDayU3Ek__BackingField_5; }
	inline bool* get_address_of_U3CIsDayU3Ek__BackingField_5() { return &___U3CIsDayU3Ek__BackingField_5; }
	inline void set_U3CIsDayU3Ek__BackingField_5(bool value)
	{
		___U3CIsDayU3Ek__BackingField_5 = value;
	}

	inline static int32_t get_offset_of_U3CIsPlayerTurnU3Ek__BackingField_6() { return static_cast<int32_t>(offsetof(GameLogicManager_t1762115607, ___U3CIsPlayerTurnU3Ek__BackingField_6)); }
	inline bool get_U3CIsPlayerTurnU3Ek__BackingField_6() const { return ___U3CIsPlayerTurnU3Ek__BackingField_6; }
	inline bool* get_address_of_U3CIsPlayerTurnU3Ek__BackingField_6() { return &___U3CIsPlayerTurnU3Ek__BackingField_6; }
	inline void set_U3CIsPlayerTurnU3Ek__BackingField_6(bool value)
	{
		___U3CIsPlayerTurnU3Ek__BackingField_6 = value;
	}

	inline static int32_t get_offset_of_U3CIsEnemyTurnU3Ek__BackingField_7() { return static_cast<int32_t>(offsetof(GameLogicManager_t1762115607, ___U3CIsEnemyTurnU3Ek__BackingField_7)); }
	inline bool get_U3CIsEnemyTurnU3Ek__BackingField_7() const { return ___U3CIsEnemyTurnU3Ek__BackingField_7; }
	inline bool* get_address_of_U3CIsEnemyTurnU3Ek__BackingField_7() { return &___U3CIsEnemyTurnU3Ek__BackingField_7; }
	inline void set_U3CIsEnemyTurnU3Ek__BackingField_7(bool value)
	{
		___U3CIsEnemyTurnU3Ek__BackingField_7 = value;
	}
};

struct GameLogicManager_t1762115607_StaticFields
{
public:
	// LifeLike.Inferfaces.IGameLogicManager LifeLike.GameLogicManager::Instance
	Il2CppObject * ___Instance_3;

public:
	inline static int32_t get_offset_of_Instance_3() { return static_cast<int32_t>(offsetof(GameLogicManager_t1762115607_StaticFields, ___Instance_3)); }
	inline Il2CppObject * get_Instance_3() const { return ___Instance_3; }
	inline Il2CppObject ** get_address_of_Instance_3() { return &___Instance_3; }
	inline void set_Instance_3(Il2CppObject * value)
	{
		___Instance_3 = value;
		Il2CppCodeGenWriteBarrier(&___Instance_3, value);
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
