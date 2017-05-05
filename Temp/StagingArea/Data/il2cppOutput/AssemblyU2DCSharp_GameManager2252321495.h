#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "UnityEngine_UnityEngine_MonoBehaviour1158329972.h"
#include "UnityEngine_UnityEngine_KeyCode2283395152.h"

// MapManager
struct MapManager_t3593696545;
// GameManager
struct GameManager_t2252321495;
// Characters.BaseCharacter
struct BaseCharacter_t1958497842;
// UnityEngine.GameObject
struct GameObject_t1756533147;
// Utils.FightSystemUtils
struct FightSystemUtils_t1337952792;
// CreateCharacterEditor
struct CreateCharacterEditor_t17944502;
// CharacterDetailView
struct CharacterDetailView_t772176927;
// UIManager
struct UIManager_t2519183485;
// UnityEngine.Canvas
struct Canvas_t209405766;
// System.Collections.Generic.List`1<Controls.Enemy>
struct List_1_t1382725646;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// GameManager
struct  GameManager_t2252321495  : public MonoBehaviour_t1158329972
{
public:
	// MapManager GameManager::_mapManager
	MapManager_t3593696545 * ____mapManager_2;
	// System.Int32 GameManager::_level
	int32_t ____level_4;
	// Characters.BaseCharacter GameManager::PlayerStatistic
	BaseCharacter_t1958497842 * ___PlayerStatistic_5;
	// UnityEngine.KeyCode GameManager::KeyUp
	int32_t ___KeyUp_6;
	// UnityEngine.KeyCode GameManager::KeyDown
	int32_t ___KeyDown_7;
	// UnityEngine.KeyCode GameManager::KeyLeft
	int32_t ___KeyLeft_8;
	// UnityEngine.KeyCode GameManager::KeyRight
	int32_t ___KeyRight_9;
	// UnityEngine.KeyCode GameManager::FightNormalKey
	int32_t ___FightNormalKey_10;
	// UnityEngine.KeyCode GameManager::FightSpecial
	int32_t ___FightSpecial_11;
	// UnityEngine.KeyCode GameManager::ReloadWeapon
	int32_t ___ReloadWeapon_12;
	// UnityEngine.KeyCode GameManager::SelectEnemyKey
	int32_t ___SelectEnemyKey_13;
	// UnityEngine.KeyCode GameManager::ExitKey
	int32_t ___ExitKey_14;
	// UnityEngine.KeyCode GameManager::OpenDetailKey
	int32_t ___OpenDetailKey_15;
	// UnityEngine.KeyCode GameManager::LightKey
	int32_t ___LightKey_16;
	// UnityEngine.GameObject GameManager::PlayerObject
	GameObject_t1756533147 * ___PlayerObject_17;
	// System.Boolean GameManager::IsDay
	bool ___IsDay_18;
	// Utils.FightSystemUtils GameManager::FightSystem
	FightSystemUtils_t1337952792 * ___FightSystem_19;
	// CreateCharacterEditor GameManager::CharacterEditorPrefab
	CreateCharacterEditor_t17944502 * ___CharacterEditorPrefab_20;
	// CharacterDetailView GameManager::PlayerDetailPrefab
	CharacterDetailView_t772176927 * ___PlayerDetailPrefab_21;
	// UIManager GameManager::UiUtils
	UIManager_t2519183485 * ___UiUtils_22;
	// UnityEngine.Canvas GameManager::_gameUI
	Canvas_t209405766 * ____gameUI_23;
	// System.Collections.Generic.List`1<Controls.Enemy> GameManager::Enemies
	List_1_t1382725646 * ___Enemies_24;
	// System.Boolean GameManager::OpenedWindow
	bool ___OpenedWindow_25;
	// System.Boolean GameManager::IsPlayerTurn
	bool ___IsPlayerTurn_26;

public:
	inline static int32_t get_offset_of__mapManager_2() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ____mapManager_2)); }
	inline MapManager_t3593696545 * get__mapManager_2() const { return ____mapManager_2; }
	inline MapManager_t3593696545 ** get_address_of__mapManager_2() { return &____mapManager_2; }
	inline void set__mapManager_2(MapManager_t3593696545 * value)
	{
		____mapManager_2 = value;
		Il2CppCodeGenWriteBarrier(&____mapManager_2, value);
	}

	inline static int32_t get_offset_of__level_4() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ____level_4)); }
	inline int32_t get__level_4() const { return ____level_4; }
	inline int32_t* get_address_of__level_4() { return &____level_4; }
	inline void set__level_4(int32_t value)
	{
		____level_4 = value;
	}

	inline static int32_t get_offset_of_PlayerStatistic_5() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ___PlayerStatistic_5)); }
	inline BaseCharacter_t1958497842 * get_PlayerStatistic_5() const { return ___PlayerStatistic_5; }
	inline BaseCharacter_t1958497842 ** get_address_of_PlayerStatistic_5() { return &___PlayerStatistic_5; }
	inline void set_PlayerStatistic_5(BaseCharacter_t1958497842 * value)
	{
		___PlayerStatistic_5 = value;
		Il2CppCodeGenWriteBarrier(&___PlayerStatistic_5, value);
	}

	inline static int32_t get_offset_of_KeyUp_6() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ___KeyUp_6)); }
	inline int32_t get_KeyUp_6() const { return ___KeyUp_6; }
	inline int32_t* get_address_of_KeyUp_6() { return &___KeyUp_6; }
	inline void set_KeyUp_6(int32_t value)
	{
		___KeyUp_6 = value;
	}

	inline static int32_t get_offset_of_KeyDown_7() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ___KeyDown_7)); }
	inline int32_t get_KeyDown_7() const { return ___KeyDown_7; }
	inline int32_t* get_address_of_KeyDown_7() { return &___KeyDown_7; }
	inline void set_KeyDown_7(int32_t value)
	{
		___KeyDown_7 = value;
	}

	inline static int32_t get_offset_of_KeyLeft_8() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ___KeyLeft_8)); }
	inline int32_t get_KeyLeft_8() const { return ___KeyLeft_8; }
	inline int32_t* get_address_of_KeyLeft_8() { return &___KeyLeft_8; }
	inline void set_KeyLeft_8(int32_t value)
	{
		___KeyLeft_8 = value;
	}

	inline static int32_t get_offset_of_KeyRight_9() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ___KeyRight_9)); }
	inline int32_t get_KeyRight_9() const { return ___KeyRight_9; }
	inline int32_t* get_address_of_KeyRight_9() { return &___KeyRight_9; }
	inline void set_KeyRight_9(int32_t value)
	{
		___KeyRight_9 = value;
	}

	inline static int32_t get_offset_of_FightNormalKey_10() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ___FightNormalKey_10)); }
	inline int32_t get_FightNormalKey_10() const { return ___FightNormalKey_10; }
	inline int32_t* get_address_of_FightNormalKey_10() { return &___FightNormalKey_10; }
	inline void set_FightNormalKey_10(int32_t value)
	{
		___FightNormalKey_10 = value;
	}

	inline static int32_t get_offset_of_FightSpecial_11() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ___FightSpecial_11)); }
	inline int32_t get_FightSpecial_11() const { return ___FightSpecial_11; }
	inline int32_t* get_address_of_FightSpecial_11() { return &___FightSpecial_11; }
	inline void set_FightSpecial_11(int32_t value)
	{
		___FightSpecial_11 = value;
	}

	inline static int32_t get_offset_of_ReloadWeapon_12() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ___ReloadWeapon_12)); }
	inline int32_t get_ReloadWeapon_12() const { return ___ReloadWeapon_12; }
	inline int32_t* get_address_of_ReloadWeapon_12() { return &___ReloadWeapon_12; }
	inline void set_ReloadWeapon_12(int32_t value)
	{
		___ReloadWeapon_12 = value;
	}

	inline static int32_t get_offset_of_SelectEnemyKey_13() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ___SelectEnemyKey_13)); }
	inline int32_t get_SelectEnemyKey_13() const { return ___SelectEnemyKey_13; }
	inline int32_t* get_address_of_SelectEnemyKey_13() { return &___SelectEnemyKey_13; }
	inline void set_SelectEnemyKey_13(int32_t value)
	{
		___SelectEnemyKey_13 = value;
	}

	inline static int32_t get_offset_of_ExitKey_14() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ___ExitKey_14)); }
	inline int32_t get_ExitKey_14() const { return ___ExitKey_14; }
	inline int32_t* get_address_of_ExitKey_14() { return &___ExitKey_14; }
	inline void set_ExitKey_14(int32_t value)
	{
		___ExitKey_14 = value;
	}

	inline static int32_t get_offset_of_OpenDetailKey_15() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ___OpenDetailKey_15)); }
	inline int32_t get_OpenDetailKey_15() const { return ___OpenDetailKey_15; }
	inline int32_t* get_address_of_OpenDetailKey_15() { return &___OpenDetailKey_15; }
	inline void set_OpenDetailKey_15(int32_t value)
	{
		___OpenDetailKey_15 = value;
	}

	inline static int32_t get_offset_of_LightKey_16() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ___LightKey_16)); }
	inline int32_t get_LightKey_16() const { return ___LightKey_16; }
	inline int32_t* get_address_of_LightKey_16() { return &___LightKey_16; }
	inline void set_LightKey_16(int32_t value)
	{
		___LightKey_16 = value;
	}

	inline static int32_t get_offset_of_PlayerObject_17() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ___PlayerObject_17)); }
	inline GameObject_t1756533147 * get_PlayerObject_17() const { return ___PlayerObject_17; }
	inline GameObject_t1756533147 ** get_address_of_PlayerObject_17() { return &___PlayerObject_17; }
	inline void set_PlayerObject_17(GameObject_t1756533147 * value)
	{
		___PlayerObject_17 = value;
		Il2CppCodeGenWriteBarrier(&___PlayerObject_17, value);
	}

	inline static int32_t get_offset_of_IsDay_18() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ___IsDay_18)); }
	inline bool get_IsDay_18() const { return ___IsDay_18; }
	inline bool* get_address_of_IsDay_18() { return &___IsDay_18; }
	inline void set_IsDay_18(bool value)
	{
		___IsDay_18 = value;
	}

	inline static int32_t get_offset_of_FightSystem_19() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ___FightSystem_19)); }
	inline FightSystemUtils_t1337952792 * get_FightSystem_19() const { return ___FightSystem_19; }
	inline FightSystemUtils_t1337952792 ** get_address_of_FightSystem_19() { return &___FightSystem_19; }
	inline void set_FightSystem_19(FightSystemUtils_t1337952792 * value)
	{
		___FightSystem_19 = value;
		Il2CppCodeGenWriteBarrier(&___FightSystem_19, value);
	}

	inline static int32_t get_offset_of_CharacterEditorPrefab_20() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ___CharacterEditorPrefab_20)); }
	inline CreateCharacterEditor_t17944502 * get_CharacterEditorPrefab_20() const { return ___CharacterEditorPrefab_20; }
	inline CreateCharacterEditor_t17944502 ** get_address_of_CharacterEditorPrefab_20() { return &___CharacterEditorPrefab_20; }
	inline void set_CharacterEditorPrefab_20(CreateCharacterEditor_t17944502 * value)
	{
		___CharacterEditorPrefab_20 = value;
		Il2CppCodeGenWriteBarrier(&___CharacterEditorPrefab_20, value);
	}

	inline static int32_t get_offset_of_PlayerDetailPrefab_21() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ___PlayerDetailPrefab_21)); }
	inline CharacterDetailView_t772176927 * get_PlayerDetailPrefab_21() const { return ___PlayerDetailPrefab_21; }
	inline CharacterDetailView_t772176927 ** get_address_of_PlayerDetailPrefab_21() { return &___PlayerDetailPrefab_21; }
	inline void set_PlayerDetailPrefab_21(CharacterDetailView_t772176927 * value)
	{
		___PlayerDetailPrefab_21 = value;
		Il2CppCodeGenWriteBarrier(&___PlayerDetailPrefab_21, value);
	}

	inline static int32_t get_offset_of_UiUtils_22() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ___UiUtils_22)); }
	inline UIManager_t2519183485 * get_UiUtils_22() const { return ___UiUtils_22; }
	inline UIManager_t2519183485 ** get_address_of_UiUtils_22() { return &___UiUtils_22; }
	inline void set_UiUtils_22(UIManager_t2519183485 * value)
	{
		___UiUtils_22 = value;
		Il2CppCodeGenWriteBarrier(&___UiUtils_22, value);
	}

	inline static int32_t get_offset_of__gameUI_23() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ____gameUI_23)); }
	inline Canvas_t209405766 * get__gameUI_23() const { return ____gameUI_23; }
	inline Canvas_t209405766 ** get_address_of__gameUI_23() { return &____gameUI_23; }
	inline void set__gameUI_23(Canvas_t209405766 * value)
	{
		____gameUI_23 = value;
		Il2CppCodeGenWriteBarrier(&____gameUI_23, value);
	}

	inline static int32_t get_offset_of_Enemies_24() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ___Enemies_24)); }
	inline List_1_t1382725646 * get_Enemies_24() const { return ___Enemies_24; }
	inline List_1_t1382725646 ** get_address_of_Enemies_24() { return &___Enemies_24; }
	inline void set_Enemies_24(List_1_t1382725646 * value)
	{
		___Enemies_24 = value;
		Il2CppCodeGenWriteBarrier(&___Enemies_24, value);
	}

	inline static int32_t get_offset_of_OpenedWindow_25() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ___OpenedWindow_25)); }
	inline bool get_OpenedWindow_25() const { return ___OpenedWindow_25; }
	inline bool* get_address_of_OpenedWindow_25() { return &___OpenedWindow_25; }
	inline void set_OpenedWindow_25(bool value)
	{
		___OpenedWindow_25 = value;
	}

	inline static int32_t get_offset_of_IsPlayerTurn_26() { return static_cast<int32_t>(offsetof(GameManager_t2252321495, ___IsPlayerTurn_26)); }
	inline bool get_IsPlayerTurn_26() const { return ___IsPlayerTurn_26; }
	inline bool* get_address_of_IsPlayerTurn_26() { return &___IsPlayerTurn_26; }
	inline void set_IsPlayerTurn_26(bool value)
	{
		___IsPlayerTurn_26 = value;
	}
};

struct GameManager_t2252321495_StaticFields
{
public:
	// GameManager GameManager::Instance
	GameManager_t2252321495 * ___Instance_3;

public:
	inline static int32_t get_offset_of_Instance_3() { return static_cast<int32_t>(offsetof(GameManager_t2252321495_StaticFields, ___Instance_3)); }
	inline GameManager_t2252321495 * get_Instance_3() const { return ___Instance_3; }
	inline GameManager_t2252321495 ** get_address_of_Instance_3() { return &___Instance_3; }
	inline void set_Instance_3(GameManager_t2252321495 * value)
	{
		___Instance_3 = value;
		Il2CppCodeGenWriteBarrier(&___Instance_3, value);
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
