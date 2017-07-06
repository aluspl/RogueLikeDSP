#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "UnityEngine_UnityEngine_MonoBehaviour1158329972.h"
#include "UnityEngine_UnityEngine_Vector32243707580.h"

// UnityEngine.Camera
struct Camera_t189460977;
// UnityEngine.CharacterController
struct CharacterController_t4094781467;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// MiniGame.TheBall.TheBallControlMap
struct  TheBallControlMap_t3092626429  : public MonoBehaviour_t1158329972
{
public:
	// System.Single MiniGame.TheBall.TheBallControlMap::RotAmount
	float ___RotAmount_2;
	// System.Single MiniGame.TheBall.TheBallControlMap::Speed
	float ___Speed_3;
	// System.Single MiniGame.TheBall.TheBallControlMap::BackgroundColorVector
	float ___BackgroundColorVector_4;
	// System.Single MiniGame.TheBall.TheBallControlMap::SizeFloat
	float ___SizeFloat_5;
	// UnityEngine.Camera MiniGame.TheBall.TheBallControlMap::Camera
	Camera_t189460977 * ___Camera_6;
	// UnityEngine.Vector3 MiniGame.TheBall.TheBallControlMap::_currEuler
	Vector3_t2243707580  ____currEuler_7;
	// UnityEngine.CharacterController MiniGame.TheBall.TheBallControlMap::<CharCtrl>k__BackingField
	CharacterController_t4094781467 * ___U3CCharCtrlU3Ek__BackingField_8;

public:
	inline static int32_t get_offset_of_RotAmount_2() { return static_cast<int32_t>(offsetof(TheBallControlMap_t3092626429, ___RotAmount_2)); }
	inline float get_RotAmount_2() const { return ___RotAmount_2; }
	inline float* get_address_of_RotAmount_2() { return &___RotAmount_2; }
	inline void set_RotAmount_2(float value)
	{
		___RotAmount_2 = value;
	}

	inline static int32_t get_offset_of_Speed_3() { return static_cast<int32_t>(offsetof(TheBallControlMap_t3092626429, ___Speed_3)); }
	inline float get_Speed_3() const { return ___Speed_3; }
	inline float* get_address_of_Speed_3() { return &___Speed_3; }
	inline void set_Speed_3(float value)
	{
		___Speed_3 = value;
	}

	inline static int32_t get_offset_of_BackgroundColorVector_4() { return static_cast<int32_t>(offsetof(TheBallControlMap_t3092626429, ___BackgroundColorVector_4)); }
	inline float get_BackgroundColorVector_4() const { return ___BackgroundColorVector_4; }
	inline float* get_address_of_BackgroundColorVector_4() { return &___BackgroundColorVector_4; }
	inline void set_BackgroundColorVector_4(float value)
	{
		___BackgroundColorVector_4 = value;
	}

	inline static int32_t get_offset_of_SizeFloat_5() { return static_cast<int32_t>(offsetof(TheBallControlMap_t3092626429, ___SizeFloat_5)); }
	inline float get_SizeFloat_5() const { return ___SizeFloat_5; }
	inline float* get_address_of_SizeFloat_5() { return &___SizeFloat_5; }
	inline void set_SizeFloat_5(float value)
	{
		___SizeFloat_5 = value;
	}

	inline static int32_t get_offset_of_Camera_6() { return static_cast<int32_t>(offsetof(TheBallControlMap_t3092626429, ___Camera_6)); }
	inline Camera_t189460977 * get_Camera_6() const { return ___Camera_6; }
	inline Camera_t189460977 ** get_address_of_Camera_6() { return &___Camera_6; }
	inline void set_Camera_6(Camera_t189460977 * value)
	{
		___Camera_6 = value;
		Il2CppCodeGenWriteBarrier(&___Camera_6, value);
	}

	inline static int32_t get_offset_of__currEuler_7() { return static_cast<int32_t>(offsetof(TheBallControlMap_t3092626429, ____currEuler_7)); }
	inline Vector3_t2243707580  get__currEuler_7() const { return ____currEuler_7; }
	inline Vector3_t2243707580 * get_address_of__currEuler_7() { return &____currEuler_7; }
	inline void set__currEuler_7(Vector3_t2243707580  value)
	{
		____currEuler_7 = value;
	}

	inline static int32_t get_offset_of_U3CCharCtrlU3Ek__BackingField_8() { return static_cast<int32_t>(offsetof(TheBallControlMap_t3092626429, ___U3CCharCtrlU3Ek__BackingField_8)); }
	inline CharacterController_t4094781467 * get_U3CCharCtrlU3Ek__BackingField_8() const { return ___U3CCharCtrlU3Ek__BackingField_8; }
	inline CharacterController_t4094781467 ** get_address_of_U3CCharCtrlU3Ek__BackingField_8() { return &___U3CCharCtrlU3Ek__BackingField_8; }
	inline void set_U3CCharCtrlU3Ek__BackingField_8(CharacterController_t4094781467 * value)
	{
		___U3CCharCtrlU3Ek__BackingField_8 = value;
		Il2CppCodeGenWriteBarrier(&___U3CCharCtrlU3Ek__BackingField_8, value);
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
