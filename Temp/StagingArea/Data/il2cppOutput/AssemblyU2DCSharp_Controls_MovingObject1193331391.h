#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "UnityEngine_UnityEngine_MonoBehaviour1158329972.h"
#include "UnityEngine_UnityEngine_LayerMask3188175821.h"

// UnityEngine.BoxCollider2D
struct BoxCollider2D_t948534547;
// UnityEngine.Rigidbody2D
struct Rigidbody2D_t502193897;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// Controls.MovingObject
struct  MovingObject_t1193331391  : public MonoBehaviour_t1158329972
{
public:
	// System.Single Controls.MovingObject::MoveTime
	float ___MoveTime_2;
	// UnityEngine.LayerMask Controls.MovingObject::BlockingLayer
	LayerMask_t3188175821  ___BlockingLayer_3;
	// UnityEngine.LayerMask Controls.MovingObject::CharacterLayer
	LayerMask_t3188175821  ___CharacterLayer_4;
	// UnityEngine.BoxCollider2D Controls.MovingObject::_boxCollider2D
	BoxCollider2D_t948534547 * ____boxCollider2D_5;
	// UnityEngine.Rigidbody2D Controls.MovingObject::_rb2D
	Rigidbody2D_t502193897 * ____rb2D_6;
	// System.Single Controls.MovingObject::_inverseMoveTime
	float ____inverseMoveTime_7;

public:
	inline static int32_t get_offset_of_MoveTime_2() { return static_cast<int32_t>(offsetof(MovingObject_t1193331391, ___MoveTime_2)); }
	inline float get_MoveTime_2() const { return ___MoveTime_2; }
	inline float* get_address_of_MoveTime_2() { return &___MoveTime_2; }
	inline void set_MoveTime_2(float value)
	{
		___MoveTime_2 = value;
	}

	inline static int32_t get_offset_of_BlockingLayer_3() { return static_cast<int32_t>(offsetof(MovingObject_t1193331391, ___BlockingLayer_3)); }
	inline LayerMask_t3188175821  get_BlockingLayer_3() const { return ___BlockingLayer_3; }
	inline LayerMask_t3188175821 * get_address_of_BlockingLayer_3() { return &___BlockingLayer_3; }
	inline void set_BlockingLayer_3(LayerMask_t3188175821  value)
	{
		___BlockingLayer_3 = value;
	}

	inline static int32_t get_offset_of_CharacterLayer_4() { return static_cast<int32_t>(offsetof(MovingObject_t1193331391, ___CharacterLayer_4)); }
	inline LayerMask_t3188175821  get_CharacterLayer_4() const { return ___CharacterLayer_4; }
	inline LayerMask_t3188175821 * get_address_of_CharacterLayer_4() { return &___CharacterLayer_4; }
	inline void set_CharacterLayer_4(LayerMask_t3188175821  value)
	{
		___CharacterLayer_4 = value;
	}

	inline static int32_t get_offset_of__boxCollider2D_5() { return static_cast<int32_t>(offsetof(MovingObject_t1193331391, ____boxCollider2D_5)); }
	inline BoxCollider2D_t948534547 * get__boxCollider2D_5() const { return ____boxCollider2D_5; }
	inline BoxCollider2D_t948534547 ** get_address_of__boxCollider2D_5() { return &____boxCollider2D_5; }
	inline void set__boxCollider2D_5(BoxCollider2D_t948534547 * value)
	{
		____boxCollider2D_5 = value;
		Il2CppCodeGenWriteBarrier(&____boxCollider2D_5, value);
	}

	inline static int32_t get_offset_of__rb2D_6() { return static_cast<int32_t>(offsetof(MovingObject_t1193331391, ____rb2D_6)); }
	inline Rigidbody2D_t502193897 * get__rb2D_6() const { return ____rb2D_6; }
	inline Rigidbody2D_t502193897 ** get_address_of__rb2D_6() { return &____rb2D_6; }
	inline void set__rb2D_6(Rigidbody2D_t502193897 * value)
	{
		____rb2D_6 = value;
		Il2CppCodeGenWriteBarrier(&____rb2D_6, value);
	}

	inline static int32_t get_offset_of__inverseMoveTime_7() { return static_cast<int32_t>(offsetof(MovingObject_t1193331391, ____inverseMoveTime_7)); }
	inline float get__inverseMoveTime_7() const { return ____inverseMoveTime_7; }
	inline float* get_address_of__inverseMoveTime_7() { return &____inverseMoveTime_7; }
	inline void set__inverseMoveTime_7(float value)
	{
		____inverseMoveTime_7 = value;
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
