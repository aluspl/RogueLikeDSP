#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "AssemblyU2DCSharp_LifeLike_Controls_MovingObject111237401.h"

// System.String
struct String_t;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// LifeLike.Controls.Player
struct  Player_t2016570597  : public MovingObject_t111237401
{
public:

public:
};

struct Player_t2016570597_StaticFields
{
public:
	// System.String LifeLike.Controls.Player::Tag
	String_t* ___Tag_8;
	// System.Double LifeLike.Controls.Player::TOLERANCE
	double ___TOLERANCE_9;

public:
	inline static int32_t get_offset_of_Tag_8() { return static_cast<int32_t>(offsetof(Player_t2016570597_StaticFields, ___Tag_8)); }
	inline String_t* get_Tag_8() const { return ___Tag_8; }
	inline String_t** get_address_of_Tag_8() { return &___Tag_8; }
	inline void set_Tag_8(String_t* value)
	{
		___Tag_8 = value;
		Il2CppCodeGenWriteBarrier(&___Tag_8, value);
	}

	inline static int32_t get_offset_of_TOLERANCE_9() { return static_cast<int32_t>(offsetof(Player_t2016570597_StaticFields, ___TOLERANCE_9)); }
	inline double get_TOLERANCE_9() const { return ___TOLERANCE_9; }
	inline double* get_address_of_TOLERANCE_9() { return &___TOLERANCE_9; }
	inline void set_TOLERANCE_9(double value)
	{
		___TOLERANCE_9 = value;
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
