#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "AssemblyU2DCSharp_Controls_MovingObject1193331391.h"

// System.String
struct String_t;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// Controls.Player
struct  Player_t109330733  : public MovingObject_t1193331391
{
public:
	// System.Boolean Controls.Player::IsButtonEnabled
	bool ___IsButtonEnabled_8;

public:
	inline static int32_t get_offset_of_IsButtonEnabled_8() { return static_cast<int32_t>(offsetof(Player_t109330733, ___IsButtonEnabled_8)); }
	inline bool get_IsButtonEnabled_8() const { return ___IsButtonEnabled_8; }
	inline bool* get_address_of_IsButtonEnabled_8() { return &___IsButtonEnabled_8; }
	inline void set_IsButtonEnabled_8(bool value)
	{
		___IsButtonEnabled_8 = value;
	}
};

struct Player_t109330733_StaticFields
{
public:
	// System.String Controls.Player::Tag
	String_t* ___Tag_9;
	// System.Double Controls.Player::TOLERANCE
	double ___TOLERANCE_10;

public:
	inline static int32_t get_offset_of_Tag_9() { return static_cast<int32_t>(offsetof(Player_t109330733_StaticFields, ___Tag_9)); }
	inline String_t* get_Tag_9() const { return ___Tag_9; }
	inline String_t** get_address_of_Tag_9() { return &___Tag_9; }
	inline void set_Tag_9(String_t* value)
	{
		___Tag_9 = value;
		Il2CppCodeGenWriteBarrier(&___Tag_9, value);
	}

	inline static int32_t get_offset_of_TOLERANCE_10() { return static_cast<int32_t>(offsetof(Player_t109330733_StaticFields, ___TOLERANCE_10)); }
	inline double get_TOLERANCE_10() const { return ___TOLERANCE_10; }
	inline double* get_address_of_TOLERANCE_10() { return &___TOLERANCE_10; }
	inline void set_TOLERANCE_10(double value)
	{
		___TOLERANCE_10 = value;
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
