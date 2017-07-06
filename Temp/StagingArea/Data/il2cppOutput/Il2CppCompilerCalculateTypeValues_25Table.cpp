#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <cstring>
#include <string.h>
#include <stdio.h>
#include <cmath>
#include <limits>
#include <assert.h>

#include "class-internals.h"
#include "codegen/il2cpp-codegen.h"
#include "AssemblyU2DCSharp_ExitGameOnClick4077405555.h"
#include "AssemblyU2DCSharp_LoadSceneOnClick982901957.h"
#include "AssemblyU2DCSharp_MainMenuScript4030058353.h"
#include "AssemblyU2DCSharp_LifeLike_MapElements_Door3091268286.h"
#include "AssemblyU2DCSharp_LifeLike_MapElements_Exit2143535946.h"
#include "AssemblyU2DCSharp_LifeLike_MapElements_StairsContr3583851268.h"
#include "AssemblyU2DCSharp_LifeLike_MapUtils_MapGenerator3725750701.h"
#include "AssemblyU2DCSharp_LifeLike_MapUtils_MapManager2333778437.h"
#include "AssemblyU2DCSharp_MiniGame_TheBall_BallMapElement1694755539.h"
#include "AssemblyU2DCSharp_MiniGame_TheBall_TheBallControlM3092626429.h"
#include "AssemblyU2DCSharp_MiniGame_TheBall_TheBallEnemyLog2680386844.h"
#include "AssemblyU2DCSharp_MiniGame_TheBall_TheBallGameLogi1652829844.h"
#include "AssemblyU2DCSharp_MiniGame_TheBall_TheBallMapGener1256188551.h"
#include "AssemblyU2DCSharp_MiniGame_TheBall_TheBallMapLogic1388539958.h"
#include "AssemblyU2DCSharp_MiniGame_TheBall_TheBallPlayer3718522733.h"
#include "AssemblyU2DCSharp_LifeLike_PlayerManager3892152706.h"
#include "AssemblyU2DCSharp_LifeLike_PlayerManager_U3CGetEqu1637715850.h"
#include "AssemblyU2DCSharp_LifeLike_UIManager1130753667.h"
#include "AssemblyU2DCSharp_LifeLike_Utils_EnemyUtils3894177059.h"
#include "AssemblyU2DCSharp_LifeLike_Utils_EnemyUtils_U3CEnemi23949920.h"
#include "AssemblyU2DCSharp_LifeLike_Utils_FightUtils2111988849.h"
#include "AssemblyU2DCSharp_LifeLike_Utils_GameLogUtils1646876099.h"
#include "AssemblyU2DCSharp_LifeLike_Utils_MathUtils1922421181.h"
#include "AssemblyU2DCSharp_LifeLike_Views_ListItemBase3737471492.h"
#include "AssemblyU2DCSharp_LifeLike_Views_ListItemBase_OnSe4141102032.h"
#include "AssemblyU2DCSharp_LifeLike_Views_ListItemEquipment2400421815.h"
#include "AssemblyU2DCSharp_LifeLike_Views_ListView2035967309.h"
#include "AssemblyU2DCSharp_LifeLike_Views_ListView_OnItemLo1656134054.h"
#include "AssemblyU2DCSharp_LifeLike_Views_ListView_OnItemSel321209286.h"
#include "AssemblyU2DCSharp_LifeLike_Views_ListView_ScrollDir500476937.h"
#include "AssemblyU2DCSharp_LifeLike_Views_ListView_U3CCenter430448123.h"
#include "AssemblyU2DCSharp_LifeLike_WindowManager1276248557.h"
#include "AssemblyU2DCSharp_LifeLike_CreateWindow1062208490.h"
#include "AssemblyU2DCSharp_LifeLike_DetailWindow4290421825.h"
#include "AssemblyU2DCSharp_LifeLike_EquipmentWindow3804619500.h"
#include "AssemblyU2DCSharp_LifeLike_Window1496026134.h"







#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2500 = { 0, -1, 0, 0 };
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2501 = { 0, -1, 0, 0 };
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2502 = { sizeof (ExitGameOnClick_t4077405555), -1, 0, 0 };
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2503 = { sizeof (LoadSceneOnClick_t982901957), -1, 0, 0 };
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2504 = { sizeof (MainMenuScript_t4030058353), -1, 0, 0 };
extern const int32_t g_FieldOffsetTable2504[3] = 
{
	MainMenuScript_t4030058353::get_offset_of_eventSystem_2(),
	MainMenuScript_t4030058353::get_offset_of_selectedObject_3(),
	MainMenuScript_t4030058353::get_offset_of_buttonSelected_4(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2505 = { sizeof (Door_t3091268286), -1, 0, 0 };
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2506 = { sizeof (Exit_t2143535946), -1, sizeof(Exit_t2143535946_StaticFields), 0 };
extern const int32_t g_FieldOffsetTable2506[1] = 
{
	Exit_t2143535946_StaticFields::get_offset_of_Tag_2(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2507 = { sizeof (StairsController_t3583851268), -1, 0, 0 };
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2508 = { sizeof (MapGenerator_t3725750701), -1, 0, 0 };
extern const int32_t g_FieldOffsetTable2508[3] = 
{
	MapGenerator_t3725750701::get_offset_of_MinimumWallSize_0(),
	MapGenerator_t3725750701::get_offset_of_U3CMapSizeYU3Ek__BackingField_1(),
	MapGenerator_t3725750701::get_offset_of_U3CMapSizeXU3Ek__BackingField_2(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2509 = { sizeof (MapManager_t2333778437), -1, 0, 0 };
extern const int32_t g_FieldOffsetTable2509[16] = 
{
	MapManager_t2333778437::get_offset_of_Walls_2(),
	MapManager_t2333778437::get_offset_of_Floors_3(),
	MapManager_t2333778437::get_offset_of_Doors_4(),
	MapManager_t2333778437::get_offset_of_Enemies_5(),
	MapManager_t2333778437::get_offset_of_EndPoint_6(),
	MapManager_t2333778437::get_offset_of_Player_7(),
	MapManager_t2333778437::get_offset_of_MapSizeX_8(),
	MapManager_t2333778437::get_offset_of_MapSizeY_9(),
	MapManager_t2333778437::get_offset_of_MaxEnemies_10(),
	MapManager_t2333778437::get_offset_of_MaxHorizontalLines_11(),
	MapManager_t2333778437::get_offset_of_MaxVerticalLines_12(),
	MapManager_t2333778437::get_offset_of_MinimalWallSize_13(),
	MapManager_t2333778437::get_offset_of__random_14(),
	MapManager_t2333778437::get_offset_of_U3CMapU3Ek__BackingField_15(),
	MapManager_t2333778437::get_offset_of_U3CEnemiesCollectionU3Ek__BackingField_16(),
	MapManager_t2333778437::get_offset_of_U3CMapCollectionU3Ek__BackingField_17(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2510 = { sizeof (BallMapElement_t1694755539)+ sizeof (Il2CppObject), sizeof(int32_t), 0, 0 };
extern const int32_t g_FieldOffsetTable2510[10] = 
{
	BallMapElement_t1694755539::get_offset_of_value___1() + static_cast<int32_t>(sizeof(Il2CppObject)),
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2511 = { sizeof (TheBallControlMap_t3092626429), -1, 0, 0 };
extern const int32_t g_FieldOffsetTable2511[7] = 
{
	TheBallControlMap_t3092626429::get_offset_of_RotAmount_2(),
	TheBallControlMap_t3092626429::get_offset_of_Speed_3(),
	TheBallControlMap_t3092626429::get_offset_of_BackgroundColorVector_4(),
	TheBallControlMap_t3092626429::get_offset_of_SizeFloat_5(),
	TheBallControlMap_t3092626429::get_offset_of_Camera_6(),
	TheBallControlMap_t3092626429::get_offset_of__currEuler_7(),
	TheBallControlMap_t3092626429::get_offset_of_U3CCharCtrlU3Ek__BackingField_8(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2512 = { sizeof (TheBallEnemyLogic_t2680386844), -1, 0, 0 };
extern const int32_t g_FieldOffsetTable2512[1] = 
{
	TheBallEnemyLogic_t2680386844::get_offset_of__theBallGameLogic_2(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2513 = { sizeof (TheBallGameLogic_t1652829844), -1, 0, 0 };
extern const int32_t g_FieldOffsetTable2513[12] = 
{
	TheBallGameLogic_t1652829844::get_offset_of_Player_2(),
	TheBallGameLogic_t1652829844::get_offset_of_FinishMap_3(),
	TheBallGameLogic_t1652829844::get_offset_of_MapCounterText_4(),
	TheBallGameLogic_t1652829844::get_offset_of_MapTimeText_5(),
	TheBallGameLogic_t1652829844::get_offset_of_MapTotalText_6(),
	TheBallGameLogic_t1652829844::get_offset_of_Generator_7(),
	TheBallGameLogic_t1652829844::get_offset_of__map_8(),
	0,
	0,
	0,
	TheBallGameLogic_t1652829844::get_offset_of_CurrentTime_12(),
	TheBallGameLogic_t1652829844::get_offset_of__totalTime_13(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2514 = { sizeof (TheBallMapGenerator_t1256188551), -1, 0, 0 };
extern const int32_t g_FieldOffsetTable2514[7] = 
{
	TheBallMapGenerator_t1256188551::get_offset_of_Wall_2(),
	TheBallMapGenerator_t1256188551::get_offset_of_Corner_3(),
	TheBallMapGenerator_t1256188551::get_offset_of_FinishPoint_4(),
	TheBallMapGenerator_t1256188551::get_offset_of_Enemy_5(),
	TheBallMapGenerator_t1256188551::get_offset_of_Map_6(),
	TheBallMapGenerator_t1256188551::get_offset_of_Size_7(),
	TheBallMapGenerator_t1256188551::get_offset_of__player_8(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2515 = { sizeof (TheBallMapLogic_t1388539958), -1, 0, 0 };
extern const int32_t g_FieldOffsetTable2515[1] = 
{
	TheBallMapLogic_t1388539958::get_offset_of__theBallGameLogic_2(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2516 = { sizeof (TheBallPlayer_t3718522733), -1, 0, 0 };
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2517 = { sizeof (PlayerManager_t3892152706), -1, sizeof(PlayerManager_t3892152706_StaticFields), 0 };
extern const int32_t g_FieldOffsetTable2517[4] = 
{
	PlayerManager_t3892152706_StaticFields::get_offset_of_Instance_2(),
	PlayerManager_t3892152706::get_offset_of_U3CStatisticU3Ek__BackingField_3(),
	PlayerManager_t3892152706::get_offset_of_U3CEquipmentsU3Ek__BackingField_4(),
	PlayerManager_t3892152706::get_offset_of_U3CObjectU3Ek__BackingField_5(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2518 = { sizeof (U3CGetEquipmentsU3Ec__AnonStorey0_t1637715850), -1, 0, 0 };
extern const int32_t g_FieldOffsetTable2518[1] = 
{
	U3CGetEquipmentsU3Ec__AnonStorey0_t1637715850::get_offset_of_type_0(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2519 = { sizeof (UIManager_t1130753667), -1, sizeof(UIManager_t1130753667_StaticFields), 0 };
extern const int32_t g_FieldOffsetTable2519[9] = 
{
	UIManager_t1130753667_StaticFields::get_offset_of_Instance_2(),
	UIManager_t1130753667::get_offset_of_GameLog_3(),
	UIManager_t1130753667::get_offset_of_SelectedEnemyStatistic_4(),
	UIManager_t1130753667::get_offset_of_SelectedEnemyDetail_5(),
	UIManager_t1130753667::get_offset_of_PlayerStatistic_6(),
	UIManager_t1130753667::get_offset_of_PlayerDetail_7(),
	UIManager_t1130753667::get_offset_of__stringLog_8(),
	UIManager_t1130753667::get_offset_of_MaxLines_9(),
	UIManager_t1130753667::get_offset_of_U3CGameUIU3Ek__BackingField_10(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2520 = { sizeof (EnemyUtils_t3894177059), -1, sizeof(EnemyUtils_t3894177059_StaticFields), 0 };
extern const int32_t g_FieldOffsetTable2520[7] = 
{
	EnemyUtils_t3894177059_StaticFields::get_offset_of__enemyCount_0(),
	EnemyUtils_t3894177059_StaticFields::get_offset_of_MAXDISTANCE_1(),
	EnemyUtils_t3894177059_StaticFields::get_offset_of_WaitTime_2(),
	EnemyUtils_t3894177059_StaticFields::get_offset_of_U3CEnemyIndexU3Ek__BackingField_3(),
	EnemyUtils_t3894177059_StaticFields::get_offset_of_U3CU3Ef__amU24cache0_4(),
	EnemyUtils_t3894177059_StaticFields::get_offset_of_U3CU3Ef__amU24cache1_5(),
	EnemyUtils_t3894177059_StaticFields::get_offset_of_U3CU3Ef__amU24cache2_6(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2521 = { sizeof (U3CEnemiesMoveU3Ec__Iterator0_t23949920), -1, 0, 0 };
extern const int32_t g_FieldOffsetTable2521[5] = 
{
	U3CEnemiesMoveU3Ec__Iterator0_t23949920::get_offset_of_U3CiU3E__1_0(),
	U3CEnemiesMoveU3Ec__Iterator0_t23949920::get_offset_of_U3CenemyU3E__2_1(),
	U3CEnemiesMoveU3Ec__Iterator0_t23949920::get_offset_of_U24current_2(),
	U3CEnemiesMoveU3Ec__Iterator0_t23949920::get_offset_of_U24disposing_3(),
	U3CEnemiesMoveU3Ec__Iterator0_t23949920::get_offset_of_U24PC_4(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2522 = { sizeof (FightUtils_t2111988849), -1, sizeof(FightUtils_t2111988849_StaticFields), 0 };
extern const int32_t g_FieldOffsetTable2522[1] = 
{
	FightUtils_t2111988849_StaticFields::get_offset_of_U3CInstanceU3Ek__BackingField_2(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2523 = { sizeof (GameLogUtils_t1646876099), -1, sizeof(GameLogUtils_t1646876099_StaticFields), 0 };
extern const int32_t g_FieldOffsetTable2523[1] = 
{
	GameLogUtils_t1646876099_StaticFields::get_offset_of_NoSelectedAttack_0(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2524 = { sizeof (MathUtils_t1922421181), -1, 0, 0 };
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2525 = { sizeof (ListItemBase_t3737471492), -1, 0, 0 };
extern const int32_t g_FieldOffsetTable2525[3] = 
{
	ListItemBase_t3737471492::get_offset_of_onSelected_2(),
	ListItemBase_t3737471492::get_offset_of__rectTransform_3(),
	ListItemBase_t3737471492::get_offset_of_U3CIndexU3Ek__BackingField_4(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2526 = { sizeof (OnSelectedHandler_t4141102032), sizeof(Il2CppMethodPointer), 0, 0 };
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2527 = { sizeof (ListItemEquipment_t2400421815), -1, 0, 0 };
extern const int32_t g_FieldOffsetTable2527[6] = 
{
	ListItemEquipment_t2400421815::get_offset_of_U3CObjectU3Ek__BackingField_5(),
	ListItemEquipment_t2400421815::get_offset_of_U3CTypeU3Ek__BackingField_6(),
	ListItemEquipment_t2400421815::get_offset_of__itemName_7(),
	ListItemEquipment_t2400421815::get_offset_of__itemType_8(),
	ListItemEquipment_t2400421815::get_offset_of__itemStat_9(),
	ListItemEquipment_t2400421815::get_offset_of__sprite_10(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2528 = { sizeof (ListView_t2035967309), -1, 0, 0 };
extern const int32_t g_FieldOffsetTable2528[19] = 
{
	ListView_t2035967309::get_offset_of_onItemLoaded_2(),
	ListView_t2035967309::get_offset_of_onItemSelected_3(),
	ListView_t2035967309::get_offset_of__scrollRect_4(),
	ListView_t2035967309::get_offset_of__viewport_5(),
	ListView_t2035967309::get_offset_of__content_6(),
	ListView_t2035967309::get_offset_of__spacing_7(),
	ListView_t2035967309::get_offset_of__fitItemToViewport_8(),
	ListView_t2035967309::get_offset_of__centerOnItem_9(),
	ListView_t2035967309::get_offset_of__changeItemDragFactor_10(),
	ListView_t2035967309::get_offset_of__itemsList_11(),
	ListView_t2035967309::get_offset_of__itemSize_12(),
	ListView_t2035967309::get_offset_of__lastPosition_13(),
	ListView_t2035967309::get_offset_of__itemsTotal_14(),
	ListView_t2035967309::get_offset_of__itemsVisible_15(),
	ListView_t2035967309::get_offset_of__itemsToRecycleBefore_16(),
	ListView_t2035967309::get_offset_of__itemsToRecycleAfter_17(),
	ListView_t2035967309::get_offset_of__currentItemIndex_18(),
	ListView_t2035967309::get_offset_of__lastItemIndex_19(),
	ListView_t2035967309::get_offset_of__dragInitialPosition_20(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2529 = { sizeof (OnItemLoadedHandler_t1656134054), sizeof(Il2CppMethodPointer), 0, 0 };
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2530 = { sizeof (OnItemSelectedHandler_t321209286), sizeof(Il2CppMethodPointer), 0, 0 };
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2531 = { sizeof (ScrollDirection_t500476937)+ sizeof (Il2CppObject), sizeof(int32_t), 0, 0 };
extern const int32_t g_FieldOffsetTable2531[3] = 
{
	ScrollDirection_t500476937::get_offset_of_value___1() + static_cast<int32_t>(sizeof(Il2CppObject)),
	0,
	0,
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2532 = { sizeof (U3CCenterOnItemCoroutineU3Ec__Iterator0_t430448123), -1, 0, 0 };
extern const int32_t g_FieldOffsetTable2532[5] = 
{
	U3CCenterOnItemCoroutineU3Ec__Iterator0_t430448123::get_offset_of_index_0(),
	U3CCenterOnItemCoroutineU3Ec__Iterator0_t430448123::get_offset_of_U24this_1(),
	U3CCenterOnItemCoroutineU3Ec__Iterator0_t430448123::get_offset_of_U24current_2(),
	U3CCenterOnItemCoroutineU3Ec__Iterator0_t430448123::get_offset_of_U24disposing_3(),
	U3CCenterOnItemCoroutineU3Ec__Iterator0_t430448123::get_offset_of_U24PC_4(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2533 = { sizeof (WindowManager_t1276248557), -1, sizeof(WindowManager_t1276248557_StaticFields), 0 };
extern const int32_t g_FieldOffsetTable2533[5] = 
{
	WindowManager_t1276248557::get_offset_of_CreateWindowPrefab_2(),
	WindowManager_t1276248557::get_offset_of_DetailWindowPrefab_3(),
	WindowManager_t1276248557::get_offset_of_EquipmentWindowPrefab_4(),
	WindowManager_t1276248557_StaticFields::get_offset_of_Instance_5(),
	WindowManager_t1276248557::get_offset_of_U3CStatusU3Ek__BackingField_6(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2534 = { sizeof (CreateWindow_t1062208490), -1, 0, 0 };
extern const int32_t g_FieldOffsetTable2534[21] = 
{
	CreateWindow_t1062208490::get_offset_of_StrengthSlider_2(),
	CreateWindow_t1062208490::get_offset_of_InteligenceSlider_3(),
	CreateWindow_t1062208490::get_offset_of_CharismaSlider_4(),
	CreateWindow_t1062208490::get_offset_of_AgilitySlider_5(),
	CreateWindow_t1062208490::get_offset_of_EnduranceSlider_6(),
	CreateWindow_t1062208490::get_offset_of_PerceptionSlider_7(),
	CreateWindow_t1062208490::get_offset_of_StrengthValue_8(),
	CreateWindow_t1062208490::get_offset_of_InteligenceValue_9(),
	CreateWindow_t1062208490::get_offset_of_CharismaValue_10(),
	CreateWindow_t1062208490::get_offset_of_AgilityValue_11(),
	CreateWindow_t1062208490::get_offset_of_EnduranceValue_12(),
	CreateWindow_t1062208490::get_offset_of_PerceptionValue_13(),
	CreateWindow_t1062208490::get_offset_of_ClassListDropdown_14(),
	CreateWindow_t1062208490::get_offset_of_CharacterName_15(),
	CreateWindow_t1062208490::get_offset_of_Character_16(),
	CreateWindow_t1062208490::get_offset_of_CharacterLeftPoint_17(),
	CreateWindow_t1062208490::get_offset_of_CharacterLeftPointValue_18(),
	CreateWindow_t1062208490::get_offset_of__characterClasses_19(),
	CreateWindow_t1062208490::get_offset_of_U3CSelectedClassU3Ek__BackingField_20(),
	CreateWindow_t1062208490::get_offset_of_SaveButton_21(),
	CreateWindow_t1062208490::get_offset_of_Statistic_22(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2535 = { sizeof (DetailWindow_t4290421825), -1, 0, 0 };
extern const int32_t g_FieldOffsetTable2535[7] = 
{
	DetailWindow_t4290421825::get_offset_of_CharacterName_2(),
	DetailWindow_t4290421825::get_offset_of_StrengthValaue_3(),
	DetailWindow_t4290421825::get_offset_of_InteligenceValaue_4(),
	DetailWindow_t4290421825::get_offset_of_CharismaValaue_5(),
	DetailWindow_t4290421825::get_offset_of_AgilityValaue_6(),
	DetailWindow_t4290421825::get_offset_of_EnduranceValaue_7(),
	DetailWindow_t4290421825::get_offset_of_PerceptionValaue_8(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2536 = { sizeof (EquipmentWindow_t3804619500), -1, 0, 0 };
extern const int32_t g_FieldOffsetTable2536[11] = 
{
	EquipmentWindow_t3804619500::get_offset_of__list_2(),
	EquipmentWindow_t3804619500::get_offset_of__selectedWeapon_3(),
	EquipmentWindow_t3804619500::get_offset_of__selectedArmor_4(),
	EquipmentWindow_t3804619500::get_offset_of__selectedHead_5(),
	EquipmentWindow_t3804619500::get_offset_of__selectedGloves_6(),
	EquipmentWindow_t3804619500::get_offset_of__selectedShoes_7(),
	EquipmentWindow_t3804619500::get_offset_of__selectedPants_8(),
	EquipmentWindow_t3804619500::get_offset_of__listItem_9(),
	EquipmentWindow_t3804619500::get_offset_of__selectedItem_10(),
	EquipmentWindow_t3804619500::get_offset_of__selectedIndex_11(),
	EquipmentWindow_t3804619500::get_offset_of_U3CEquipmentsU3Ek__BackingField_12(),
};
extern const Il2CppTypeDefinitionSizes g_typeDefinitionSize2537 = { sizeof (Window_t1496026134), -1, 0, 0 };
#ifdef __clang__
#pragma clang diagnostic pop
#endif
