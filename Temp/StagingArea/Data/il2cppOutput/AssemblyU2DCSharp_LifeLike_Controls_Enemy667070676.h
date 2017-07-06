#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "AssemblyU2DCSharp_LifeLike_Controls_MovingObject111237401.h"

// LifeLike.Characters.Character
struct Character_t2496620761;
// UnityEngine.Transform
struct Transform_t3275118058;
// UnityEngine.Light
struct Light_t494725636;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// LifeLike.Controls.Enemy
struct  Enemy_t667070676  : public MovingObject_t111237401
{
public:
	// LifeLike.Characters.Character LifeLike.Controls.Enemy::<EnemyCharacter>k__BackingField
	Character_t2496620761 * ___U3CEnemyCharacterU3Ek__BackingField_8;
	// UnityEngine.Transform LifeLike.Controls.Enemy::_target
	Transform_t3275118058 * ____target_9;
	// System.Boolean LifeLike.Controls.Enemy::<IsSelected>k__BackingField
	bool ___U3CIsSelectedU3Ek__BackingField_10;
	// UnityEngine.Light LifeLike.Controls.Enemy::_selectedLight
	Light_t494725636 * ____selectedLight_11;
	// System.Double LifeLike.Controls.Enemy::TOLERANCE
	double ___TOLERANCE_12;
	// System.Boolean LifeLike.Controls.Enemy::<IsHisTurn>k__BackingField
	bool ___U3CIsHisTurnU3Ek__BackingField_13;

public:
	inline static int32_t get_offset_of_U3CEnemyCharacterU3Ek__BackingField_8() { return static_cast<int32_t>(offsetof(Enemy_t667070676, ___U3CEnemyCharacterU3Ek__BackingField_8)); }
	inline Character_t2496620761 * get_U3CEnemyCharacterU3Ek__BackingField_8() const { return ___U3CEnemyCharacterU3Ek__BackingField_8; }
	inline Character_t2496620761 ** get_address_of_U3CEnemyCharacterU3Ek__BackingField_8() { return &___U3CEnemyCharacterU3Ek__BackingField_8; }
	inline void set_U3CEnemyCharacterU3Ek__BackingField_8(Character_t2496620761 * value)
	{
		___U3CEnemyCharacterU3Ek__BackingField_8 = value;
		Il2CppCodeGenWriteBarrier(&___U3CEnemyCharacterU3Ek__BackingField_8, value);
	}

	inline static int32_t get_offset_of__target_9() { return static_cast<int32_t>(offsetof(Enemy_t667070676, ____target_9)); }
	inline Transform_t3275118058 * get__target_9() const { return ____target_9; }
	inline Transform_t3275118058 ** get_address_of__target_9() { return &____target_9; }
	inline void set__target_9(Transform_t3275118058 * value)
	{
		____target_9 = value;
		Il2CppCodeGenWriteBarrier(&____target_9, value);
	}

	inline static int32_t get_offset_of_U3CIsSelectedU3Ek__BackingField_10() { return static_cast<int32_t>(offsetof(Enemy_t667070676, ___U3CIsSelectedU3Ek__BackingField_10)); }
	inline bool get_U3CIsSelectedU3Ek__BackingField_10() const { return ___U3CIsSelectedU3Ek__BackingField_10; }
	inline bool* get_address_of_U3CIsSelectedU3Ek__BackingField_10() { return &___U3CIsSelectedU3Ek__BackingField_10; }
	inline void set_U3CIsSelectedU3Ek__BackingField_10(bool value)
	{
		___U3CIsSelectedU3Ek__BackingField_10 = value;
	}

	inline static int32_t get_offset_of__selectedLight_11() { return static_cast<int32_t>(offsetof(Enemy_t667070676, ____selectedLight_11)); }
	inline Light_t494725636 * get__selectedLight_11() const { return ____selectedLight_11; }
	inline Light_t494725636 ** get_address_of__selectedLight_11() { return &____selectedLight_11; }
	inline void set__selectedLight_11(Light_t494725636 * value)
	{
		____selectedLight_11 = value;
		Il2CppCodeGenWriteBarrier(&____selectedLight_11, value);
	}

	inline static int32_t get_offset_of_TOLERANCE_12() { return static_cast<int32_t>(offsetof(Enemy_t667070676, ___TOLERANCE_12)); }
	inline double get_TOLERANCE_12() const { return ___TOLERANCE_12; }
	inline double* get_address_of_TOLERANCE_12() { return &___TOLERANCE_12; }
	inline void set_TOLERANCE_12(double value)
	{
		___TOLERANCE_12 = value;
	}

	inline static int32_t get_offset_of_U3CIsHisTurnU3Ek__BackingField_13() { return static_cast<int32_t>(offsetof(Enemy_t667070676, ___U3CIsHisTurnU3Ek__BackingField_13)); }
	inline bool get_U3CIsHisTurnU3Ek__BackingField_13() const { return ___U3CIsHisTurnU3Ek__BackingField_13; }
	inline bool* get_address_of_U3CIsHisTurnU3Ek__BackingField_13() { return &___U3CIsHisTurnU3Ek__BackingField_13; }
	inline void set_U3CIsHisTurnU3Ek__BackingField_13(bool value)
	{
		___U3CIsHisTurnU3Ek__BackingField_13 = value;
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
