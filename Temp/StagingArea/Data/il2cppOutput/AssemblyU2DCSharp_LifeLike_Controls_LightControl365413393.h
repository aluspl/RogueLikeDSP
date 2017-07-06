#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "UnityEngine_UnityEngine_MonoBehaviour1158329972.h"

// UnityEngine.Light
struct Light_t494725636;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// LifeLike.Controls.LightControl
struct  LightControl_t365413393  : public MonoBehaviour_t1158329972
{
public:
	// System.Boolean LifeLike.Controls.LightControl::_isDay
	bool ____isDay_2;
	// UnityEngine.Light LifeLike.Controls.LightControl::_light
	Light_t494725636 * ____light_3;

public:
	inline static int32_t get_offset_of__isDay_2() { return static_cast<int32_t>(offsetof(LightControl_t365413393, ____isDay_2)); }
	inline bool get__isDay_2() const { return ____isDay_2; }
	inline bool* get_address_of__isDay_2() { return &____isDay_2; }
	inline void set__isDay_2(bool value)
	{
		____isDay_2 = value;
	}

	inline static int32_t get_offset_of__light_3() { return static_cast<int32_t>(offsetof(LightControl_t365413393, ____light_3)); }
	inline Light_t494725636 * get__light_3() const { return ____light_3; }
	inline Light_t494725636 ** get_address_of__light_3() { return &____light_3; }
	inline void set__light_3(Light_t494725636 * value)
	{
		____light_3 = value;
		Il2CppCodeGenWriteBarrier(&____light_3, value);
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
