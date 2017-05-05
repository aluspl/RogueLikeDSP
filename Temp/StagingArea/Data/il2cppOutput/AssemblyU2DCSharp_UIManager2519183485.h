#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "UnityEngine_UnityEngine_MonoBehaviour1158329972.h"

// UnityEngine.UI.Text
struct Text_t356221433;
// System.Text.StringBuilder
struct StringBuilder_t1221177846;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// UIManager
struct  UIManager_t2519183485  : public MonoBehaviour_t1158329972
{
public:
	// UnityEngine.UI.Text UIManager::GameLog
	Text_t356221433 * ___GameLog_2;
	// UnityEngine.UI.Text UIManager::SelectedEnemyStatistic
	Text_t356221433 * ___SelectedEnemyStatistic_3;
	// UnityEngine.UI.Text UIManager::SelectedEnemyDetail
	Text_t356221433 * ___SelectedEnemyDetail_4;
	// UnityEngine.UI.Text UIManager::PlayerStatistic
	Text_t356221433 * ___PlayerStatistic_5;
	// UnityEngine.UI.Text UIManager::PlayerDetail
	Text_t356221433 * ___PlayerDetail_6;
	// System.Text.StringBuilder UIManager::_stringLog
	StringBuilder_t1221177846 * ____stringLog_7;
	// System.Int32 UIManager::MaxLines
	int32_t ___MaxLines_8;

public:
	inline static int32_t get_offset_of_GameLog_2() { return static_cast<int32_t>(offsetof(UIManager_t2519183485, ___GameLog_2)); }
	inline Text_t356221433 * get_GameLog_2() const { return ___GameLog_2; }
	inline Text_t356221433 ** get_address_of_GameLog_2() { return &___GameLog_2; }
	inline void set_GameLog_2(Text_t356221433 * value)
	{
		___GameLog_2 = value;
		Il2CppCodeGenWriteBarrier(&___GameLog_2, value);
	}

	inline static int32_t get_offset_of_SelectedEnemyStatistic_3() { return static_cast<int32_t>(offsetof(UIManager_t2519183485, ___SelectedEnemyStatistic_3)); }
	inline Text_t356221433 * get_SelectedEnemyStatistic_3() const { return ___SelectedEnemyStatistic_3; }
	inline Text_t356221433 ** get_address_of_SelectedEnemyStatistic_3() { return &___SelectedEnemyStatistic_3; }
	inline void set_SelectedEnemyStatistic_3(Text_t356221433 * value)
	{
		___SelectedEnemyStatistic_3 = value;
		Il2CppCodeGenWriteBarrier(&___SelectedEnemyStatistic_3, value);
	}

	inline static int32_t get_offset_of_SelectedEnemyDetail_4() { return static_cast<int32_t>(offsetof(UIManager_t2519183485, ___SelectedEnemyDetail_4)); }
	inline Text_t356221433 * get_SelectedEnemyDetail_4() const { return ___SelectedEnemyDetail_4; }
	inline Text_t356221433 ** get_address_of_SelectedEnemyDetail_4() { return &___SelectedEnemyDetail_4; }
	inline void set_SelectedEnemyDetail_4(Text_t356221433 * value)
	{
		___SelectedEnemyDetail_4 = value;
		Il2CppCodeGenWriteBarrier(&___SelectedEnemyDetail_4, value);
	}

	inline static int32_t get_offset_of_PlayerStatistic_5() { return static_cast<int32_t>(offsetof(UIManager_t2519183485, ___PlayerStatistic_5)); }
	inline Text_t356221433 * get_PlayerStatistic_5() const { return ___PlayerStatistic_5; }
	inline Text_t356221433 ** get_address_of_PlayerStatistic_5() { return &___PlayerStatistic_5; }
	inline void set_PlayerStatistic_5(Text_t356221433 * value)
	{
		___PlayerStatistic_5 = value;
		Il2CppCodeGenWriteBarrier(&___PlayerStatistic_5, value);
	}

	inline static int32_t get_offset_of_PlayerDetail_6() { return static_cast<int32_t>(offsetof(UIManager_t2519183485, ___PlayerDetail_6)); }
	inline Text_t356221433 * get_PlayerDetail_6() const { return ___PlayerDetail_6; }
	inline Text_t356221433 ** get_address_of_PlayerDetail_6() { return &___PlayerDetail_6; }
	inline void set_PlayerDetail_6(Text_t356221433 * value)
	{
		___PlayerDetail_6 = value;
		Il2CppCodeGenWriteBarrier(&___PlayerDetail_6, value);
	}

	inline static int32_t get_offset_of__stringLog_7() { return static_cast<int32_t>(offsetof(UIManager_t2519183485, ____stringLog_7)); }
	inline StringBuilder_t1221177846 * get__stringLog_7() const { return ____stringLog_7; }
	inline StringBuilder_t1221177846 ** get_address_of__stringLog_7() { return &____stringLog_7; }
	inline void set__stringLog_7(StringBuilder_t1221177846 * value)
	{
		____stringLog_7 = value;
		Il2CppCodeGenWriteBarrier(&____stringLog_7, value);
	}

	inline static int32_t get_offset_of_MaxLines_8() { return static_cast<int32_t>(offsetof(UIManager_t2519183485, ___MaxLines_8)); }
	inline int32_t get_MaxLines_8() const { return ___MaxLines_8; }
	inline int32_t* get_address_of_MaxLines_8() { return &___MaxLines_8; }
	inline void set_MaxLines_8(int32_t value)
	{
		___MaxLines_8 = value;
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
