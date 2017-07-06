#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "UnityEngine_UnityEngine_MonoBehaviour1158329972.h"
#include "mscorlib_System_DateTime693205669.h"
#include "mscorlib_System_TimeSpan3430258949.h"

// UnityEngine.GameObject
struct GameObject_t1756533147;
// UnityEngine.UI.Text
struct Text_t356221433;
// MiniGame.TheBall.TheBallMapGenerator
struct TheBallMapGenerator_t1256188551;
// System.String
struct String_t;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// MiniGame.TheBall.TheBallGameLogic
struct  TheBallGameLogic_t1652829844  : public MonoBehaviour_t1158329972
{
public:
	// UnityEngine.GameObject MiniGame.TheBall.TheBallGameLogic::Player
	GameObject_t1756533147 * ___Player_2;
	// UnityEngine.GameObject MiniGame.TheBall.TheBallGameLogic::FinishMap
	GameObject_t1756533147 * ___FinishMap_3;
	// UnityEngine.UI.Text MiniGame.TheBall.TheBallGameLogic::MapCounterText
	Text_t356221433 * ___MapCounterText_4;
	// UnityEngine.UI.Text MiniGame.TheBall.TheBallGameLogic::MapTimeText
	Text_t356221433 * ___MapTimeText_5;
	// UnityEngine.UI.Text MiniGame.TheBall.TheBallGameLogic::MapTotalText
	Text_t356221433 * ___MapTotalText_6;
	// MiniGame.TheBall.TheBallMapGenerator MiniGame.TheBall.TheBallGameLogic::Generator
	TheBallMapGenerator_t1256188551 * ___Generator_7;
	// System.Int32 MiniGame.TheBall.TheBallGameLogic::_map
	int32_t ____map_8;
	// System.DateTime MiniGame.TheBall.TheBallGameLogic::CurrentTime
	DateTime_t693205669  ___CurrentTime_12;
	// System.TimeSpan MiniGame.TheBall.TheBallGameLogic::_totalTime
	TimeSpan_t3430258949  ____totalTime_13;

public:
	inline static int32_t get_offset_of_Player_2() { return static_cast<int32_t>(offsetof(TheBallGameLogic_t1652829844, ___Player_2)); }
	inline GameObject_t1756533147 * get_Player_2() const { return ___Player_2; }
	inline GameObject_t1756533147 ** get_address_of_Player_2() { return &___Player_2; }
	inline void set_Player_2(GameObject_t1756533147 * value)
	{
		___Player_2 = value;
		Il2CppCodeGenWriteBarrier(&___Player_2, value);
	}

	inline static int32_t get_offset_of_FinishMap_3() { return static_cast<int32_t>(offsetof(TheBallGameLogic_t1652829844, ___FinishMap_3)); }
	inline GameObject_t1756533147 * get_FinishMap_3() const { return ___FinishMap_3; }
	inline GameObject_t1756533147 ** get_address_of_FinishMap_3() { return &___FinishMap_3; }
	inline void set_FinishMap_3(GameObject_t1756533147 * value)
	{
		___FinishMap_3 = value;
		Il2CppCodeGenWriteBarrier(&___FinishMap_3, value);
	}

	inline static int32_t get_offset_of_MapCounterText_4() { return static_cast<int32_t>(offsetof(TheBallGameLogic_t1652829844, ___MapCounterText_4)); }
	inline Text_t356221433 * get_MapCounterText_4() const { return ___MapCounterText_4; }
	inline Text_t356221433 ** get_address_of_MapCounterText_4() { return &___MapCounterText_4; }
	inline void set_MapCounterText_4(Text_t356221433 * value)
	{
		___MapCounterText_4 = value;
		Il2CppCodeGenWriteBarrier(&___MapCounterText_4, value);
	}

	inline static int32_t get_offset_of_MapTimeText_5() { return static_cast<int32_t>(offsetof(TheBallGameLogic_t1652829844, ___MapTimeText_5)); }
	inline Text_t356221433 * get_MapTimeText_5() const { return ___MapTimeText_5; }
	inline Text_t356221433 ** get_address_of_MapTimeText_5() { return &___MapTimeText_5; }
	inline void set_MapTimeText_5(Text_t356221433 * value)
	{
		___MapTimeText_5 = value;
		Il2CppCodeGenWriteBarrier(&___MapTimeText_5, value);
	}

	inline static int32_t get_offset_of_MapTotalText_6() { return static_cast<int32_t>(offsetof(TheBallGameLogic_t1652829844, ___MapTotalText_6)); }
	inline Text_t356221433 * get_MapTotalText_6() const { return ___MapTotalText_6; }
	inline Text_t356221433 ** get_address_of_MapTotalText_6() { return &___MapTotalText_6; }
	inline void set_MapTotalText_6(Text_t356221433 * value)
	{
		___MapTotalText_6 = value;
		Il2CppCodeGenWriteBarrier(&___MapTotalText_6, value);
	}

	inline static int32_t get_offset_of_Generator_7() { return static_cast<int32_t>(offsetof(TheBallGameLogic_t1652829844, ___Generator_7)); }
	inline TheBallMapGenerator_t1256188551 * get_Generator_7() const { return ___Generator_7; }
	inline TheBallMapGenerator_t1256188551 ** get_address_of_Generator_7() { return &___Generator_7; }
	inline void set_Generator_7(TheBallMapGenerator_t1256188551 * value)
	{
		___Generator_7 = value;
		Il2CppCodeGenWriteBarrier(&___Generator_7, value);
	}

	inline static int32_t get_offset_of__map_8() { return static_cast<int32_t>(offsetof(TheBallGameLogic_t1652829844, ____map_8)); }
	inline int32_t get__map_8() const { return ____map_8; }
	inline int32_t* get_address_of__map_8() { return &____map_8; }
	inline void set__map_8(int32_t value)
	{
		____map_8 = value;
	}

	inline static int32_t get_offset_of_CurrentTime_12() { return static_cast<int32_t>(offsetof(TheBallGameLogic_t1652829844, ___CurrentTime_12)); }
	inline DateTime_t693205669  get_CurrentTime_12() const { return ___CurrentTime_12; }
	inline DateTime_t693205669 * get_address_of_CurrentTime_12() { return &___CurrentTime_12; }
	inline void set_CurrentTime_12(DateTime_t693205669  value)
	{
		___CurrentTime_12 = value;
	}

	inline static int32_t get_offset_of__totalTime_13() { return static_cast<int32_t>(offsetof(TheBallGameLogic_t1652829844, ____totalTime_13)); }
	inline TimeSpan_t3430258949  get__totalTime_13() const { return ____totalTime_13; }
	inline TimeSpan_t3430258949 * get_address_of__totalTime_13() { return &____totalTime_13; }
	inline void set__totalTime_13(TimeSpan_t3430258949  value)
	{
		____totalTime_13 = value;
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
