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




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// Controls.FollowPlayer
struct  FollowPlayer_t1953906986  : public MonoBehaviour_t1158329972
{
public:
	// UnityEngine.Vector3 Controls.FollowPlayer::offset
	Vector3_t2243707580  ___offset_2;
	// System.Boolean Controls.FollowPlayer::isDay
	bool ___isDay_3;
	// UnityEngine.Camera Controls.FollowPlayer::_camera
	Camera_t189460977 * ____camera_4;

public:
	inline static int32_t get_offset_of_offset_2() { return static_cast<int32_t>(offsetof(FollowPlayer_t1953906986, ___offset_2)); }
	inline Vector3_t2243707580  get_offset_2() const { return ___offset_2; }
	inline Vector3_t2243707580 * get_address_of_offset_2() { return &___offset_2; }
	inline void set_offset_2(Vector3_t2243707580  value)
	{
		___offset_2 = value;
	}

	inline static int32_t get_offset_of_isDay_3() { return static_cast<int32_t>(offsetof(FollowPlayer_t1953906986, ___isDay_3)); }
	inline bool get_isDay_3() const { return ___isDay_3; }
	inline bool* get_address_of_isDay_3() { return &___isDay_3; }
	inline void set_isDay_3(bool value)
	{
		___isDay_3 = value;
	}

	inline static int32_t get_offset_of__camera_4() { return static_cast<int32_t>(offsetof(FollowPlayer_t1953906986, ____camera_4)); }
	inline Camera_t189460977 * get__camera_4() const { return ____camera_4; }
	inline Camera_t189460977 ** get_address_of__camera_4() { return &____camera_4; }
	inline void set__camera_4(Camera_t189460977 * value)
	{
		____camera_4 = value;
		Il2CppCodeGenWriteBarrier(&____camera_4, value);
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
