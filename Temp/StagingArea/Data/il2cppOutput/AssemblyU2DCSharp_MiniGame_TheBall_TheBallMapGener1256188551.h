#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "UnityEngine_UnityEngine_MonoBehaviour1158329972.h"

// UnityEngine.GameObject
struct GameObject_t1756533147;
// System.Collections.Generic.List`1<UnityEngine.TextAsset>
struct List_1_t3342280977;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// MiniGame.TheBall.TheBallMapGenerator
struct  TheBallMapGenerator_t1256188551  : public MonoBehaviour_t1158329972
{
public:
	// UnityEngine.GameObject MiniGame.TheBall.TheBallMapGenerator::Wall
	GameObject_t1756533147 * ___Wall_2;
	// UnityEngine.GameObject MiniGame.TheBall.TheBallMapGenerator::Corner
	GameObject_t1756533147 * ___Corner_3;
	// UnityEngine.GameObject MiniGame.TheBall.TheBallMapGenerator::FinishPoint
	GameObject_t1756533147 * ___FinishPoint_4;
	// UnityEngine.GameObject MiniGame.TheBall.TheBallMapGenerator::Enemy
	GameObject_t1756533147 * ___Enemy_5;
	// System.Collections.Generic.List`1<UnityEngine.TextAsset> MiniGame.TheBall.TheBallMapGenerator::Map
	List_1_t3342280977 * ___Map_6;
	// System.Single MiniGame.TheBall.TheBallMapGenerator::Size
	float ___Size_7;
	// UnityEngine.GameObject MiniGame.TheBall.TheBallMapGenerator::_player
	GameObject_t1756533147 * ____player_8;

public:
	inline static int32_t get_offset_of_Wall_2() { return static_cast<int32_t>(offsetof(TheBallMapGenerator_t1256188551, ___Wall_2)); }
	inline GameObject_t1756533147 * get_Wall_2() const { return ___Wall_2; }
	inline GameObject_t1756533147 ** get_address_of_Wall_2() { return &___Wall_2; }
	inline void set_Wall_2(GameObject_t1756533147 * value)
	{
		___Wall_2 = value;
		Il2CppCodeGenWriteBarrier(&___Wall_2, value);
	}

	inline static int32_t get_offset_of_Corner_3() { return static_cast<int32_t>(offsetof(TheBallMapGenerator_t1256188551, ___Corner_3)); }
	inline GameObject_t1756533147 * get_Corner_3() const { return ___Corner_3; }
	inline GameObject_t1756533147 ** get_address_of_Corner_3() { return &___Corner_3; }
	inline void set_Corner_3(GameObject_t1756533147 * value)
	{
		___Corner_3 = value;
		Il2CppCodeGenWriteBarrier(&___Corner_3, value);
	}

	inline static int32_t get_offset_of_FinishPoint_4() { return static_cast<int32_t>(offsetof(TheBallMapGenerator_t1256188551, ___FinishPoint_4)); }
	inline GameObject_t1756533147 * get_FinishPoint_4() const { return ___FinishPoint_4; }
	inline GameObject_t1756533147 ** get_address_of_FinishPoint_4() { return &___FinishPoint_4; }
	inline void set_FinishPoint_4(GameObject_t1756533147 * value)
	{
		___FinishPoint_4 = value;
		Il2CppCodeGenWriteBarrier(&___FinishPoint_4, value);
	}

	inline static int32_t get_offset_of_Enemy_5() { return static_cast<int32_t>(offsetof(TheBallMapGenerator_t1256188551, ___Enemy_5)); }
	inline GameObject_t1756533147 * get_Enemy_5() const { return ___Enemy_5; }
	inline GameObject_t1756533147 ** get_address_of_Enemy_5() { return &___Enemy_5; }
	inline void set_Enemy_5(GameObject_t1756533147 * value)
	{
		___Enemy_5 = value;
		Il2CppCodeGenWriteBarrier(&___Enemy_5, value);
	}

	inline static int32_t get_offset_of_Map_6() { return static_cast<int32_t>(offsetof(TheBallMapGenerator_t1256188551, ___Map_6)); }
	inline List_1_t3342280977 * get_Map_6() const { return ___Map_6; }
	inline List_1_t3342280977 ** get_address_of_Map_6() { return &___Map_6; }
	inline void set_Map_6(List_1_t3342280977 * value)
	{
		___Map_6 = value;
		Il2CppCodeGenWriteBarrier(&___Map_6, value);
	}

	inline static int32_t get_offset_of_Size_7() { return static_cast<int32_t>(offsetof(TheBallMapGenerator_t1256188551, ___Size_7)); }
	inline float get_Size_7() const { return ___Size_7; }
	inline float* get_address_of_Size_7() { return &___Size_7; }
	inline void set_Size_7(float value)
	{
		___Size_7 = value;
	}

	inline static int32_t get_offset_of__player_8() { return static_cast<int32_t>(offsetof(TheBallMapGenerator_t1256188551, ____player_8)); }
	inline GameObject_t1756533147 * get__player_8() const { return ____player_8; }
	inline GameObject_t1756533147 ** get_address_of__player_8() { return &____player_8; }
	inline void set__player_8(GameObject_t1756533147 * value)
	{
		____player_8 = value;
		Il2CppCodeGenWriteBarrier(&____player_8, value);
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
