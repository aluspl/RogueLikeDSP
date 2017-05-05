#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "AssemblyU2DCSharp_Controls_MovingObject1193331391.h"

// Characters.BaseCharacter
struct BaseCharacter_t1958497842;
// UnityEngine.Transform
struct Transform_t3275118058;
// UnityEngine.Light
struct Light_t494725636;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// Controls.Enemy
struct  Enemy_t2013604514  : public MovingObject_t1193331391
{
public:
	// Characters.BaseCharacter Controls.Enemy::<EnemyCharacter>k__BackingField
	BaseCharacter_t1958497842 * ___U3CEnemyCharacterU3Ek__BackingField_8;
	// UnityEngine.Transform Controls.Enemy::_target
	Transform_t3275118058 * ____target_9;
	// System.Boolean Controls.Enemy::<IsSelected>k__BackingField
	bool ___U3CIsSelectedU3Ek__BackingField_10;
	// UnityEngine.Light Controls.Enemy::_selectedLight
	Light_t494725636 * ____selectedLight_11;
	// System.Double Controls.Enemy::TOLERANCE
	double ___TOLERANCE_12;
	// System.Boolean Controls.Enemy::<IsHisTurn>k__BackingField
	bool ___U3CIsHisTurnU3Ek__BackingField_13;

public:
	inline static int32_t get_offset_of_U3CEnemyCharacterU3Ek__BackingField_8() { return static_cast<int32_t>(offsetof(Enemy_t2013604514, ___U3CEnemyCharacterU3Ek__BackingField_8)); }
	inline BaseCharacter_t1958497842 * get_U3CEnemyCharacterU3Ek__BackingField_8() const { return ___U3CEnemyCharacterU3Ek__BackingField_8; }
	inline BaseCharacter_t1958497842 ** get_address_of_U3CEnemyCharacterU3Ek__BackingField_8() { return &___U3CEnemyCharacterU3Ek__BackingField_8; }
	inline void set_U3CEnemyCharacterU3Ek__BackingField_8(BaseCharacter_t1958497842 * value)
	{
		___U3CEnemyCharacterU3Ek__BackingField_8 = value;
		Il2CppCodeGenWriteBarrier(&___U3CEnemyCharacterU3Ek__BackingField_8, value);
	}

	inline static int32_t get_offset_of__target_9() { return static_cast<int32_t>(offsetof(Enemy_t2013604514, ____target_9)); }
	inline Transform_t3275118058 * get__target_9() const { return ____target_9; }
	inline Transform_t3275118058 ** get_address_of__target_9() { return &____target_9; }
	inline void set__target_9(Transform_t3275118058 * value)
	{
		____target_9 = value;
		Il2CppCodeGenWriteBarrier(&____target_9, value);
	}

	inline static int32_t get_offset_of_U3CIsSelectedU3Ek__BackingField_10() { return static_cast<int32_t>(offsetof(Enemy_t2013604514, ___U3CIsSelectedU3Ek__BackingField_10)); }
	inline bool get_U3CIsSelectedU3Ek__BackingField_10() const { return ___U3CIsSelectedU3Ek__BackingField_10; }
	inline bool* get_address_of_U3CIsSelectedU3Ek__BackingField_10() { return &___U3CIsSelectedU3Ek__BackingField_10; }
	inline void set_U3CIsSelectedU3Ek__BackingField_10(bool value)
	{
		___U3CIsSelectedU3Ek__BackingField_10 = value;
	}

	inline static int32_t get_offset_of__selectedLight_11() { return static_cast<int32_t>(offsetof(Enemy_t2013604514, ____selectedLight_11)); }
	inline Light_t494725636 * get__selectedLight_11() const { return ____selectedLight_11; }
	inline Light_t494725636 ** get_address_of__selectedLight_11() { return &____selectedLight_11; }
	inline void set__selectedLight_11(Light_t494725636 * value)
	{
		____selectedLight_11 = value;
		Il2CppCodeGenWriteBarrier(&____selectedLight_11, value);
	}

	inline static int32_t get_offset_of_TOLERANCE_12() { return static_cast<int32_t>(offsetof(Enemy_t2013604514, ___TOLERANCE_12)); }
	inline double get_TOLERANCE_12() const { return ___TOLERANCE_12; }
	inline double* get_address_of_TOLERANCE_12() { return &___TOLERANCE_12; }
	inline void set_TOLERANCE_12(double value)
	{
		___TOLERANCE_12 = value;
	}

	inline static int32_t get_offset_of_U3CIsHisTurnU3Ek__BackingField_13() { return static_cast<int32_t>(offsetof(Enemy_t2013604514, ___U3CIsHisTurnU3Ek__BackingField_13)); }
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
