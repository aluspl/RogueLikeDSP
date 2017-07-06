#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "UnityEngine_UnityEngine_MonoBehaviour1158329972.h"
#include "UnityEngine_UnityEngine_Vector22243707579.h"

// LifeLike.Views.ListView/OnItemLoadedHandler
struct OnItemLoadedHandler_t1656134054;
// LifeLike.Views.ListView/OnItemSelectedHandler
struct OnItemSelectedHandler_t321209286;
// UnityEngine.UI.ScrollRect
struct ScrollRect_t1199013257;
// UnityEngine.RectTransform
struct RectTransform_t3349966182;
// System.Collections.Generic.List`1<LifeLike.Views.ListItemBase>
struct List_1_t3106592624;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// LifeLike.Views.ListView
struct  ListView_t2035967309  : public MonoBehaviour_t1158329972
{
public:
	// LifeLike.Views.ListView/OnItemLoadedHandler LifeLike.Views.ListView::onItemLoaded
	OnItemLoadedHandler_t1656134054 * ___onItemLoaded_2;
	// LifeLike.Views.ListView/OnItemSelectedHandler LifeLike.Views.ListView::onItemSelected
	OnItemSelectedHandler_t321209286 * ___onItemSelected_3;
	// UnityEngine.UI.ScrollRect LifeLike.Views.ListView::_scrollRect
	ScrollRect_t1199013257 * ____scrollRect_4;
	// UnityEngine.RectTransform LifeLike.Views.ListView::_viewport
	RectTransform_t3349966182 * ____viewport_5;
	// UnityEngine.RectTransform LifeLike.Views.ListView::_content
	RectTransform_t3349966182 * ____content_6;
	// System.Single LifeLike.Views.ListView::_spacing
	float ____spacing_7;
	// System.Boolean LifeLike.Views.ListView::_fitItemToViewport
	bool ____fitItemToViewport_8;
	// System.Boolean LifeLike.Views.ListView::_centerOnItem
	bool ____centerOnItem_9;
	// System.Single LifeLike.Views.ListView::_changeItemDragFactor
	float ____changeItemDragFactor_10;
	// System.Collections.Generic.List`1<LifeLike.Views.ListItemBase> LifeLike.Views.ListView::_itemsList
	List_1_t3106592624 * ____itemsList_11;
	// System.Single LifeLike.Views.ListView::_itemSize
	float ____itemSize_12;
	// System.Single LifeLike.Views.ListView::_lastPosition
	float ____lastPosition_13;
	// System.Int32 LifeLike.Views.ListView::_itemsTotal
	int32_t ____itemsTotal_14;
	// System.Int32 LifeLike.Views.ListView::_itemsVisible
	int32_t ____itemsVisible_15;
	// System.Int32 LifeLike.Views.ListView::_itemsToRecycleBefore
	int32_t ____itemsToRecycleBefore_16;
	// System.Int32 LifeLike.Views.ListView::_itemsToRecycleAfter
	int32_t ____itemsToRecycleAfter_17;
	// System.Int32 LifeLike.Views.ListView::_currentItemIndex
	int32_t ____currentItemIndex_18;
	// System.Int32 LifeLike.Views.ListView::_lastItemIndex
	int32_t ____lastItemIndex_19;
	// UnityEngine.Vector2 LifeLike.Views.ListView::_dragInitialPosition
	Vector2_t2243707579  ____dragInitialPosition_20;

public:
	inline static int32_t get_offset_of_onItemLoaded_2() { return static_cast<int32_t>(offsetof(ListView_t2035967309, ___onItemLoaded_2)); }
	inline OnItemLoadedHandler_t1656134054 * get_onItemLoaded_2() const { return ___onItemLoaded_2; }
	inline OnItemLoadedHandler_t1656134054 ** get_address_of_onItemLoaded_2() { return &___onItemLoaded_2; }
	inline void set_onItemLoaded_2(OnItemLoadedHandler_t1656134054 * value)
	{
		___onItemLoaded_2 = value;
		Il2CppCodeGenWriteBarrier(&___onItemLoaded_2, value);
	}

	inline static int32_t get_offset_of_onItemSelected_3() { return static_cast<int32_t>(offsetof(ListView_t2035967309, ___onItemSelected_3)); }
	inline OnItemSelectedHandler_t321209286 * get_onItemSelected_3() const { return ___onItemSelected_3; }
	inline OnItemSelectedHandler_t321209286 ** get_address_of_onItemSelected_3() { return &___onItemSelected_3; }
	inline void set_onItemSelected_3(OnItemSelectedHandler_t321209286 * value)
	{
		___onItemSelected_3 = value;
		Il2CppCodeGenWriteBarrier(&___onItemSelected_3, value);
	}

	inline static int32_t get_offset_of__scrollRect_4() { return static_cast<int32_t>(offsetof(ListView_t2035967309, ____scrollRect_4)); }
	inline ScrollRect_t1199013257 * get__scrollRect_4() const { return ____scrollRect_4; }
	inline ScrollRect_t1199013257 ** get_address_of__scrollRect_4() { return &____scrollRect_4; }
	inline void set__scrollRect_4(ScrollRect_t1199013257 * value)
	{
		____scrollRect_4 = value;
		Il2CppCodeGenWriteBarrier(&____scrollRect_4, value);
	}

	inline static int32_t get_offset_of__viewport_5() { return static_cast<int32_t>(offsetof(ListView_t2035967309, ____viewport_5)); }
	inline RectTransform_t3349966182 * get__viewport_5() const { return ____viewport_5; }
	inline RectTransform_t3349966182 ** get_address_of__viewport_5() { return &____viewport_5; }
	inline void set__viewport_5(RectTransform_t3349966182 * value)
	{
		____viewport_5 = value;
		Il2CppCodeGenWriteBarrier(&____viewport_5, value);
	}

	inline static int32_t get_offset_of__content_6() { return static_cast<int32_t>(offsetof(ListView_t2035967309, ____content_6)); }
	inline RectTransform_t3349966182 * get__content_6() const { return ____content_6; }
	inline RectTransform_t3349966182 ** get_address_of__content_6() { return &____content_6; }
	inline void set__content_6(RectTransform_t3349966182 * value)
	{
		____content_6 = value;
		Il2CppCodeGenWriteBarrier(&____content_6, value);
	}

	inline static int32_t get_offset_of__spacing_7() { return static_cast<int32_t>(offsetof(ListView_t2035967309, ____spacing_7)); }
	inline float get__spacing_7() const { return ____spacing_7; }
	inline float* get_address_of__spacing_7() { return &____spacing_7; }
	inline void set__spacing_7(float value)
	{
		____spacing_7 = value;
	}

	inline static int32_t get_offset_of__fitItemToViewport_8() { return static_cast<int32_t>(offsetof(ListView_t2035967309, ____fitItemToViewport_8)); }
	inline bool get__fitItemToViewport_8() const { return ____fitItemToViewport_8; }
	inline bool* get_address_of__fitItemToViewport_8() { return &____fitItemToViewport_8; }
	inline void set__fitItemToViewport_8(bool value)
	{
		____fitItemToViewport_8 = value;
	}

	inline static int32_t get_offset_of__centerOnItem_9() { return static_cast<int32_t>(offsetof(ListView_t2035967309, ____centerOnItem_9)); }
	inline bool get__centerOnItem_9() const { return ____centerOnItem_9; }
	inline bool* get_address_of__centerOnItem_9() { return &____centerOnItem_9; }
	inline void set__centerOnItem_9(bool value)
	{
		____centerOnItem_9 = value;
	}

	inline static int32_t get_offset_of__changeItemDragFactor_10() { return static_cast<int32_t>(offsetof(ListView_t2035967309, ____changeItemDragFactor_10)); }
	inline float get__changeItemDragFactor_10() const { return ____changeItemDragFactor_10; }
	inline float* get_address_of__changeItemDragFactor_10() { return &____changeItemDragFactor_10; }
	inline void set__changeItemDragFactor_10(float value)
	{
		____changeItemDragFactor_10 = value;
	}

	inline static int32_t get_offset_of__itemsList_11() { return static_cast<int32_t>(offsetof(ListView_t2035967309, ____itemsList_11)); }
	inline List_1_t3106592624 * get__itemsList_11() const { return ____itemsList_11; }
	inline List_1_t3106592624 ** get_address_of__itemsList_11() { return &____itemsList_11; }
	inline void set__itemsList_11(List_1_t3106592624 * value)
	{
		____itemsList_11 = value;
		Il2CppCodeGenWriteBarrier(&____itemsList_11, value);
	}

	inline static int32_t get_offset_of__itemSize_12() { return static_cast<int32_t>(offsetof(ListView_t2035967309, ____itemSize_12)); }
	inline float get__itemSize_12() const { return ____itemSize_12; }
	inline float* get_address_of__itemSize_12() { return &____itemSize_12; }
	inline void set__itemSize_12(float value)
	{
		____itemSize_12 = value;
	}

	inline static int32_t get_offset_of__lastPosition_13() { return static_cast<int32_t>(offsetof(ListView_t2035967309, ____lastPosition_13)); }
	inline float get__lastPosition_13() const { return ____lastPosition_13; }
	inline float* get_address_of__lastPosition_13() { return &____lastPosition_13; }
	inline void set__lastPosition_13(float value)
	{
		____lastPosition_13 = value;
	}

	inline static int32_t get_offset_of__itemsTotal_14() { return static_cast<int32_t>(offsetof(ListView_t2035967309, ____itemsTotal_14)); }
	inline int32_t get__itemsTotal_14() const { return ____itemsTotal_14; }
	inline int32_t* get_address_of__itemsTotal_14() { return &____itemsTotal_14; }
	inline void set__itemsTotal_14(int32_t value)
	{
		____itemsTotal_14 = value;
	}

	inline static int32_t get_offset_of__itemsVisible_15() { return static_cast<int32_t>(offsetof(ListView_t2035967309, ____itemsVisible_15)); }
	inline int32_t get__itemsVisible_15() const { return ____itemsVisible_15; }
	inline int32_t* get_address_of__itemsVisible_15() { return &____itemsVisible_15; }
	inline void set__itemsVisible_15(int32_t value)
	{
		____itemsVisible_15 = value;
	}

	inline static int32_t get_offset_of__itemsToRecycleBefore_16() { return static_cast<int32_t>(offsetof(ListView_t2035967309, ____itemsToRecycleBefore_16)); }
	inline int32_t get__itemsToRecycleBefore_16() const { return ____itemsToRecycleBefore_16; }
	inline int32_t* get_address_of__itemsToRecycleBefore_16() { return &____itemsToRecycleBefore_16; }
	inline void set__itemsToRecycleBefore_16(int32_t value)
	{
		____itemsToRecycleBefore_16 = value;
	}

	inline static int32_t get_offset_of__itemsToRecycleAfter_17() { return static_cast<int32_t>(offsetof(ListView_t2035967309, ____itemsToRecycleAfter_17)); }
	inline int32_t get__itemsToRecycleAfter_17() const { return ____itemsToRecycleAfter_17; }
	inline int32_t* get_address_of__itemsToRecycleAfter_17() { return &____itemsToRecycleAfter_17; }
	inline void set__itemsToRecycleAfter_17(int32_t value)
	{
		____itemsToRecycleAfter_17 = value;
	}

	inline static int32_t get_offset_of__currentItemIndex_18() { return static_cast<int32_t>(offsetof(ListView_t2035967309, ____currentItemIndex_18)); }
	inline int32_t get__currentItemIndex_18() const { return ____currentItemIndex_18; }
	inline int32_t* get_address_of__currentItemIndex_18() { return &____currentItemIndex_18; }
	inline void set__currentItemIndex_18(int32_t value)
	{
		____currentItemIndex_18 = value;
	}

	inline static int32_t get_offset_of__lastItemIndex_19() { return static_cast<int32_t>(offsetof(ListView_t2035967309, ____lastItemIndex_19)); }
	inline int32_t get__lastItemIndex_19() const { return ____lastItemIndex_19; }
	inline int32_t* get_address_of__lastItemIndex_19() { return &____lastItemIndex_19; }
	inline void set__lastItemIndex_19(int32_t value)
	{
		____lastItemIndex_19 = value;
	}

	inline static int32_t get_offset_of__dragInitialPosition_20() { return static_cast<int32_t>(offsetof(ListView_t2035967309, ____dragInitialPosition_20)); }
	inline Vector2_t2243707579  get__dragInitialPosition_20() const { return ____dragInitialPosition_20; }
	inline Vector2_t2243707579 * get_address_of__dragInitialPosition_20() { return &____dragInitialPosition_20; }
	inline void set__dragInitialPosition_20(Vector2_t2243707579  value)
	{
		____dragInitialPosition_20 = value;
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
