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
#include "UnityEngine_UnityEngine_Color2020392075.h"

// UnityEngine.Camera
struct Camera_t189460977;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// LifeLike.CameraManager
struct  CameraManager_t2368271596  : public MonoBehaviour_t1158329972
{
public:
	// UnityEngine.Vector3 LifeLike.CameraManager::offset
	Vector3_t2243707580  ___offset_2;
	// System.Boolean LifeLike.CameraManager::isDay
	bool ___isDay_3;
	// UnityEngine.Camera LifeLike.CameraManager::_camera
	Camera_t189460977 * ____camera_4;
	// UnityEngine.Color LifeLike.CameraManager::AlmostDark
	Color_t2020392075  ___AlmostDark_5;
	// UnityEngine.Color LifeLike.CameraManager::ColorSteel
	Color_t2020392075  ___ColorSteel_6;

public:
	inline static int32_t get_offset_of_offset_2() { return static_cast<int32_t>(offsetof(CameraManager_t2368271596, ___offset_2)); }
	inline Vector3_t2243707580  get_offset_2() const { return ___offset_2; }
	inline Vector3_t2243707580 * get_address_of_offset_2() { return &___offset_2; }
	inline void set_offset_2(Vector3_t2243707580  value)
	{
		___offset_2 = value;
	}

	inline static int32_t get_offset_of_isDay_3() { return static_cast<int32_t>(offsetof(CameraManager_t2368271596, ___isDay_3)); }
	inline bool get_isDay_3() const { return ___isDay_3; }
	inline bool* get_address_of_isDay_3() { return &___isDay_3; }
	inline void set_isDay_3(bool value)
	{
		___isDay_3 = value;
	}

	inline static int32_t get_offset_of__camera_4() { return static_cast<int32_t>(offsetof(CameraManager_t2368271596, ____camera_4)); }
	inline Camera_t189460977 * get__camera_4() const { return ____camera_4; }
	inline Camera_t189460977 ** get_address_of__camera_4() { return &____camera_4; }
	inline void set__camera_4(Camera_t189460977 * value)
	{
		____camera_4 = value;
		Il2CppCodeGenWriteBarrier(&____camera_4, value);
	}

	inline static int32_t get_offset_of_AlmostDark_5() { return static_cast<int32_t>(offsetof(CameraManager_t2368271596, ___AlmostDark_5)); }
	inline Color_t2020392075  get_AlmostDark_5() const { return ___AlmostDark_5; }
	inline Color_t2020392075 * get_address_of_AlmostDark_5() { return &___AlmostDark_5; }
	inline void set_AlmostDark_5(Color_t2020392075  value)
	{
		___AlmostDark_5 = value;
	}

	inline static int32_t get_offset_of_ColorSteel_6() { return static_cast<int32_t>(offsetof(CameraManager_t2368271596, ___ColorSteel_6)); }
	inline Color_t2020392075  get_ColorSteel_6() const { return ___ColorSteel_6; }
	inline Color_t2020392075 * get_address_of_ColorSteel_6() { return &___ColorSteel_6; }
	inline void set_ColorSteel_6(Color_t2020392075  value)
	{
		___ColorSteel_6 = value;
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
