#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "UnityEngine_UnityEngine_MonoBehaviour1158329972.h"

// LifeLike.Inferfaces.IUIManager
struct IUIManager_t2286403184;
// UnityEngine.UI.Text
struct Text_t356221433;
// System.Text.StringBuilder
struct StringBuilder_t1221177846;
// UnityEngine.Canvas
struct Canvas_t209405766;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// LifeLike.UIManager
struct  UIManager_t1130753667  : public MonoBehaviour_t1158329972
{
public:
	// UnityEngine.UI.Text LifeLike.UIManager::GameLog
	Text_t356221433 * ___GameLog_3;
	// UnityEngine.UI.Text LifeLike.UIManager::SelectedEnemyStatistic
	Text_t356221433 * ___SelectedEnemyStatistic_4;
	// UnityEngine.UI.Text LifeLike.UIManager::SelectedEnemyDetail
	Text_t356221433 * ___SelectedEnemyDetail_5;
	// UnityEngine.UI.Text LifeLike.UIManager::PlayerStatistic
	Text_t356221433 * ___PlayerStatistic_6;
	// UnityEngine.UI.Text LifeLike.UIManager::PlayerDetail
	Text_t356221433 * ___PlayerDetail_7;
	// System.Text.StringBuilder LifeLike.UIManager::_stringLog
	StringBuilder_t1221177846 * ____stringLog_8;
	// System.Int32 LifeLike.UIManager::MaxLines
	int32_t ___MaxLines_9;
	// UnityEngine.Canvas LifeLike.UIManager::<GameUI>k__BackingField
	Canvas_t209405766 * ___U3CGameUIU3Ek__BackingField_10;

public:
	inline static int32_t get_offset_of_GameLog_3() { return static_cast<int32_t>(offsetof(UIManager_t1130753667, ___GameLog_3)); }
	inline Text_t356221433 * get_GameLog_3() const { return ___GameLog_3; }
	inline Text_t356221433 ** get_address_of_GameLog_3() { return &___GameLog_3; }
	inline void set_GameLog_3(Text_t356221433 * value)
	{
		___GameLog_3 = value;
		Il2CppCodeGenWriteBarrier(&___GameLog_3, value);
	}

	inline static int32_t get_offset_of_SelectedEnemyStatistic_4() { return static_cast<int32_t>(offsetof(UIManager_t1130753667, ___SelectedEnemyStatistic_4)); }
	inline Text_t356221433 * get_SelectedEnemyStatistic_4() const { return ___SelectedEnemyStatistic_4; }
	inline Text_t356221433 ** get_address_of_SelectedEnemyStatistic_4() { return &___SelectedEnemyStatistic_4; }
	inline void set_SelectedEnemyStatistic_4(Text_t356221433 * value)
	{
		___SelectedEnemyStatistic_4 = value;
		Il2CppCodeGenWriteBarrier(&___SelectedEnemyStatistic_4, value);
	}

	inline static int32_t get_offset_of_SelectedEnemyDetail_5() { return static_cast<int32_t>(offsetof(UIManager_t1130753667, ___SelectedEnemyDetail_5)); }
	inline Text_t356221433 * get_SelectedEnemyDetail_5() const { return ___SelectedEnemyDetail_5; }
	inline Text_t356221433 ** get_address_of_SelectedEnemyDetail_5() { return &___SelectedEnemyDetail_5; }
	inline void set_SelectedEnemyDetail_5(Text_t356221433 * value)
	{
		___SelectedEnemyDetail_5 = value;
		Il2CppCodeGenWriteBarrier(&___SelectedEnemyDetail_5, value);
	}

	inline static int32_t get_offset_of_PlayerStatistic_6() { return static_cast<int32_t>(offsetof(UIManager_t1130753667, ___PlayerStatistic_6)); }
	inline Text_t356221433 * get_PlayerStatistic_6() const { return ___PlayerStatistic_6; }
	inline Text_t356221433 ** get_address_of_PlayerStatistic_6() { return &___PlayerStatistic_6; }
	inline void set_PlayerStatistic_6(Text_t356221433 * value)
	{
		___PlayerStatistic_6 = value;
		Il2CppCodeGenWriteBarrier(&___PlayerStatistic_6, value);
	}

	inline static int32_t get_offset_of_PlayerDetail_7() { return static_cast<int32_t>(offsetof(UIManager_t1130753667, ___PlayerDetail_7)); }
	inline Text_t356221433 * get_PlayerDetail_7() const { return ___PlayerDetail_7; }
	inline Text_t356221433 ** get_address_of_PlayerDetail_7() { return &___PlayerDetail_7; }
	inline void set_PlayerDetail_7(Text_t356221433 * value)
	{
		___PlayerDetail_7 = value;
		Il2CppCodeGenWriteBarrier(&___PlayerDetail_7, value);
	}

	inline static int32_t get_offset_of__stringLog_8() { return static_cast<int32_t>(offsetof(UIManager_t1130753667, ____stringLog_8)); }
	inline StringBuilder_t1221177846 * get__stringLog_8() const { return ____stringLog_8; }
	inline StringBuilder_t1221177846 ** get_address_of__stringLog_8() { return &____stringLog_8; }
	inline void set__stringLog_8(StringBuilder_t1221177846 * value)
	{
		____stringLog_8 = value;
		Il2CppCodeGenWriteBarrier(&____stringLog_8, value);
	}

	inline static int32_t get_offset_of_MaxLines_9() { return static_cast<int32_t>(offsetof(UIManager_t1130753667, ___MaxLines_9)); }
	inline int32_t get_MaxLines_9() const { return ___MaxLines_9; }
	inline int32_t* get_address_of_MaxLines_9() { return &___MaxLines_9; }
	inline void set_MaxLines_9(int32_t value)
	{
		___MaxLines_9 = value;
	}

	inline static int32_t get_offset_of_U3CGameUIU3Ek__BackingField_10() { return static_cast<int32_t>(offsetof(UIManager_t1130753667, ___U3CGameUIU3Ek__BackingField_10)); }
	inline Canvas_t209405766 * get_U3CGameUIU3Ek__BackingField_10() const { return ___U3CGameUIU3Ek__BackingField_10; }
	inline Canvas_t209405766 ** get_address_of_U3CGameUIU3Ek__BackingField_10() { return &___U3CGameUIU3Ek__BackingField_10; }
	inline void set_U3CGameUIU3Ek__BackingField_10(Canvas_t209405766 * value)
	{
		___U3CGameUIU3Ek__BackingField_10 = value;
		Il2CppCodeGenWriteBarrier(&___U3CGameUIU3Ek__BackingField_10, value);
	}
};

struct UIManager_t1130753667_StaticFields
{
public:
	// LifeLike.Inferfaces.IUIManager LifeLike.UIManager::Instance
	Il2CppObject * ___Instance_2;

public:
	inline static int32_t get_offset_of_Instance_2() { return static_cast<int32_t>(offsetof(UIManager_t1130753667_StaticFields, ___Instance_2)); }
	inline Il2CppObject * get_Instance_2() const { return ___Instance_2; }
	inline Il2CppObject ** get_address_of_Instance_2() { return &___Instance_2; }
	inline void set_Instance_2(Il2CppObject * value)
	{
		___Instance_2 = value;
		Il2CppCodeGenWriteBarrier(&___Instance_2, value);
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
