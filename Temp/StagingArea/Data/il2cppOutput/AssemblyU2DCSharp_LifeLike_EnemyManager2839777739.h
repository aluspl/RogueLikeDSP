#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "UnityEngine_UnityEngine_MonoBehaviour1158329972.h"

// System.Collections.Generic.List`1<LifeLike.Controls.Enemy>
struct List_1_t36191808;
// LifeLike.EnemyManager
struct EnemyManager_t2839777739;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// LifeLike.EnemyManager
struct  EnemyManager_t2839777739  : public MonoBehaviour_t1158329972
{
public:
	// System.Collections.Generic.List`1<LifeLike.Controls.Enemy> LifeLike.EnemyManager::List
	List_1_t36191808 * ___List_2;

public:
	inline static int32_t get_offset_of_List_2() { return static_cast<int32_t>(offsetof(EnemyManager_t2839777739, ___List_2)); }
	inline List_1_t36191808 * get_List_2() const { return ___List_2; }
	inline List_1_t36191808 ** get_address_of_List_2() { return &___List_2; }
	inline void set_List_2(List_1_t36191808 * value)
	{
		___List_2 = value;
		Il2CppCodeGenWriteBarrier(&___List_2, value);
	}
};

struct EnemyManager_t2839777739_StaticFields
{
public:
	// LifeLike.EnemyManager LifeLike.EnemyManager::Instance
	EnemyManager_t2839777739 * ___Instance_3;

public:
	inline static int32_t get_offset_of_Instance_3() { return static_cast<int32_t>(offsetof(EnemyManager_t2839777739_StaticFields, ___Instance_3)); }
	inline EnemyManager_t2839777739 * get_Instance_3() const { return ___Instance_3; }
	inline EnemyManager_t2839777739 ** get_address_of_Instance_3() { return &___Instance_3; }
	inline void set_Instance_3(EnemyManager_t2839777739 * value)
	{
		___Instance_3 = value;
		Il2CppCodeGenWriteBarrier(&___Instance_3, value);
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
