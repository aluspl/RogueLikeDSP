#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "UnityEngine_UnityEngine_MonoBehaviour1158329972.h"

// LifeLike.GameManager
struct GameManager_t3670217055;
// UnityEngine.GameObject
struct GameObject_t1756533147;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// LifeLike.GameManager
struct  GameManager_t3670217055  : public MonoBehaviour_t1158329972
{
public:
	// UnityEngine.GameObject LifeLike.GameManager::GameLogicManager
	GameObject_t1756533147 * ___GameLogicManager_3;
	// UnityEngine.GameObject LifeLike.GameManager::UIManager
	GameObject_t1756533147 * ___UIManager_4;
	// UnityEngine.GameObject LifeLike.GameManager::WindowManager
	GameObject_t1756533147 * ___WindowManager_5;

public:
	inline static int32_t get_offset_of_GameLogicManager_3() { return static_cast<int32_t>(offsetof(GameManager_t3670217055, ___GameLogicManager_3)); }
	inline GameObject_t1756533147 * get_GameLogicManager_3() const { return ___GameLogicManager_3; }
	inline GameObject_t1756533147 ** get_address_of_GameLogicManager_3() { return &___GameLogicManager_3; }
	inline void set_GameLogicManager_3(GameObject_t1756533147 * value)
	{
		___GameLogicManager_3 = value;
		Il2CppCodeGenWriteBarrier(&___GameLogicManager_3, value);
	}

	inline static int32_t get_offset_of_UIManager_4() { return static_cast<int32_t>(offsetof(GameManager_t3670217055, ___UIManager_4)); }
	inline GameObject_t1756533147 * get_UIManager_4() const { return ___UIManager_4; }
	inline GameObject_t1756533147 ** get_address_of_UIManager_4() { return &___UIManager_4; }
	inline void set_UIManager_4(GameObject_t1756533147 * value)
	{
		___UIManager_4 = value;
		Il2CppCodeGenWriteBarrier(&___UIManager_4, value);
	}

	inline static int32_t get_offset_of_WindowManager_5() { return static_cast<int32_t>(offsetof(GameManager_t3670217055, ___WindowManager_5)); }
	inline GameObject_t1756533147 * get_WindowManager_5() const { return ___WindowManager_5; }
	inline GameObject_t1756533147 ** get_address_of_WindowManager_5() { return &___WindowManager_5; }
	inline void set_WindowManager_5(GameObject_t1756533147 * value)
	{
		___WindowManager_5 = value;
		Il2CppCodeGenWriteBarrier(&___WindowManager_5, value);
	}
};

struct GameManager_t3670217055_StaticFields
{
public:
	// LifeLike.GameManager LifeLike.GameManager::Instance
	GameManager_t3670217055 * ___Instance_2;

public:
	inline static int32_t get_offset_of_Instance_2() { return static_cast<int32_t>(offsetof(GameManager_t3670217055_StaticFields, ___Instance_2)); }
	inline GameManager_t3670217055 * get_Instance_2() const { return ___Instance_2; }
	inline GameManager_t3670217055 ** get_address_of_Instance_2() { return &___Instance_2; }
	inline void set_Instance_2(GameManager_t3670217055 * value)
	{
		___Instance_2 = value;
		Il2CppCodeGenWriteBarrier(&___Instance_2, value);
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
