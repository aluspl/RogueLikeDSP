#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "UnityEngine_UnityEngine_MonoBehaviour1158329972.h"

// LifeLike.Inferfaces.IPlayerManager
struct IPlayerManager_t1795196509;
// LifeLike.Characters.Character
struct Character_t2496620761;
// System.Collections.Generic.List`1<LifeLike.Interfaces.IEquipment>
struct List_1_t3253376741;
// LifeLike.Controls.Player
struct Player_t2016570597;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// LifeLike.PlayerManager
struct  PlayerManager_t3892152706  : public MonoBehaviour_t1158329972
{
public:
	// LifeLike.Characters.Character LifeLike.PlayerManager::<Statistic>k__BackingField
	Character_t2496620761 * ___U3CStatisticU3Ek__BackingField_3;
	// System.Collections.Generic.List`1<LifeLike.Interfaces.IEquipment> LifeLike.PlayerManager::<Equipments>k__BackingField
	List_1_t3253376741 * ___U3CEquipmentsU3Ek__BackingField_4;
	// LifeLike.Controls.Player LifeLike.PlayerManager::<Object>k__BackingField
	Player_t2016570597 * ___U3CObjectU3Ek__BackingField_5;

public:
	inline static int32_t get_offset_of_U3CStatisticU3Ek__BackingField_3() { return static_cast<int32_t>(offsetof(PlayerManager_t3892152706, ___U3CStatisticU3Ek__BackingField_3)); }
	inline Character_t2496620761 * get_U3CStatisticU3Ek__BackingField_3() const { return ___U3CStatisticU3Ek__BackingField_3; }
	inline Character_t2496620761 ** get_address_of_U3CStatisticU3Ek__BackingField_3() { return &___U3CStatisticU3Ek__BackingField_3; }
	inline void set_U3CStatisticU3Ek__BackingField_3(Character_t2496620761 * value)
	{
		___U3CStatisticU3Ek__BackingField_3 = value;
		Il2CppCodeGenWriteBarrier(&___U3CStatisticU3Ek__BackingField_3, value);
	}

	inline static int32_t get_offset_of_U3CEquipmentsU3Ek__BackingField_4() { return static_cast<int32_t>(offsetof(PlayerManager_t3892152706, ___U3CEquipmentsU3Ek__BackingField_4)); }
	inline List_1_t3253376741 * get_U3CEquipmentsU3Ek__BackingField_4() const { return ___U3CEquipmentsU3Ek__BackingField_4; }
	inline List_1_t3253376741 ** get_address_of_U3CEquipmentsU3Ek__BackingField_4() { return &___U3CEquipmentsU3Ek__BackingField_4; }
	inline void set_U3CEquipmentsU3Ek__BackingField_4(List_1_t3253376741 * value)
	{
		___U3CEquipmentsU3Ek__BackingField_4 = value;
		Il2CppCodeGenWriteBarrier(&___U3CEquipmentsU3Ek__BackingField_4, value);
	}

	inline static int32_t get_offset_of_U3CObjectU3Ek__BackingField_5() { return static_cast<int32_t>(offsetof(PlayerManager_t3892152706, ___U3CObjectU3Ek__BackingField_5)); }
	inline Player_t2016570597 * get_U3CObjectU3Ek__BackingField_5() const { return ___U3CObjectU3Ek__BackingField_5; }
	inline Player_t2016570597 ** get_address_of_U3CObjectU3Ek__BackingField_5() { return &___U3CObjectU3Ek__BackingField_5; }
	inline void set_U3CObjectU3Ek__BackingField_5(Player_t2016570597 * value)
	{
		___U3CObjectU3Ek__BackingField_5 = value;
		Il2CppCodeGenWriteBarrier(&___U3CObjectU3Ek__BackingField_5, value);
	}
};

struct PlayerManager_t3892152706_StaticFields
{
public:
	// LifeLike.Inferfaces.IPlayerManager LifeLike.PlayerManager::Instance
	Il2CppObject * ___Instance_2;

public:
	inline static int32_t get_offset_of_Instance_2() { return static_cast<int32_t>(offsetof(PlayerManager_t3892152706_StaticFields, ___Instance_2)); }
	inline Il2CppObject * get_Instance_2() const { return ___Instance_2; }
	inline Il2CppObject ** get_address_of_Instance_2() { return &___Instance_2; }
	inline void set_Instance_2(Il2CppObject * value)
	{
		___Instance_2 = value;
		Il2CppCodeGenWriteBarrier(&___Instance_2, value);
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
