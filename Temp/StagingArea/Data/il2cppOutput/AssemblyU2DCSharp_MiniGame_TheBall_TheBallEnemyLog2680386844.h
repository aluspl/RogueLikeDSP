#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "UnityEngine_UnityEngine_MonoBehaviour1158329972.h"

// MiniGame.TheBall.TheBallGameLogic
struct TheBallGameLogic_t1652829844;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// MiniGame.TheBall.TheBallEnemyLogic
struct  TheBallEnemyLogic_t2680386844  : public MonoBehaviour_t1158329972
{
public:
	// MiniGame.TheBall.TheBallGameLogic MiniGame.TheBall.TheBallEnemyLogic::_theBallGameLogic
	TheBallGameLogic_t1652829844 * ____theBallGameLogic_2;

public:
	inline static int32_t get_offset_of__theBallGameLogic_2() { return static_cast<int32_t>(offsetof(TheBallEnemyLogic_t2680386844, ____theBallGameLogic_2)); }
	inline TheBallGameLogic_t1652829844 * get__theBallGameLogic_2() const { return ____theBallGameLogic_2; }
	inline TheBallGameLogic_t1652829844 ** get_address_of__theBallGameLogic_2() { return &____theBallGameLogic_2; }
	inline void set__theBallGameLogic_2(TheBallGameLogic_t1652829844 * value)
	{
		____theBallGameLogic_2 = value;
		Il2CppCodeGenWriteBarrier(&____theBallGameLogic_2, value);
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
