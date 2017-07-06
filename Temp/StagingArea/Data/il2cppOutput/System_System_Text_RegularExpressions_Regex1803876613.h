#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "mscorlib_System_Object2689449295.h"
#include "System_System_Text_RegularExpressions_RegexOptions2418259727.h"

// System.Text.RegularExpressions.FactoryCache
struct FactoryCache_t2051534610;
// System.Text.RegularExpressions.IMachineFactory
struct IMachineFactory_t633643314;
// System.Collections.IDictionary
struct IDictionary_t596158605;
// System.String[]
struct StringU5BU5D_t1642385972;
// System.Int32[]
struct Int32U5BU5D_t3030399641;
// System.String
struct String_t;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// System.Text.RegularExpressions.Regex
struct  Regex_t1803876613  : public Il2CppObject
{
public:
	// System.Text.RegularExpressions.IMachineFactory System.Text.RegularExpressions.Regex::machineFactory
	Il2CppObject * ___machineFactory_2;
	// System.Collections.IDictionary System.Text.RegularExpressions.Regex::mapping
	Il2CppObject * ___mapping_3;
	// System.Int32 System.Text.RegularExpressions.Regex::group_count
	int32_t ___group_count_4;
	// System.Int32 System.Text.RegularExpressions.Regex::gap
	int32_t ___gap_5;
	// System.String[] System.Text.RegularExpressions.Regex::group_names
	StringU5BU5D_t1642385972* ___group_names_6;
	// System.Int32[] System.Text.RegularExpressions.Regex::group_numbers
	Int32U5BU5D_t3030399641* ___group_numbers_7;
	// System.String System.Text.RegularExpressions.Regex::pattern
	String_t* ___pattern_8;
	// System.Text.RegularExpressions.RegexOptions System.Text.RegularExpressions.Regex::roptions
	int32_t ___roptions_9;

public:
	inline static int32_t get_offset_of_machineFactory_2() { return static_cast<int32_t>(offsetof(Regex_t1803876613, ___machineFactory_2)); }
	inline Il2CppObject * get_machineFactory_2() const { return ___machineFactory_2; }
	inline Il2CppObject ** get_address_of_machineFactory_2() { return &___machineFactory_2; }
	inline void set_machineFactory_2(Il2CppObject * value)
	{
		___machineFactory_2 = value;
		Il2CppCodeGenWriteBarrier(&___machineFactory_2, value);
	}

	inline static int32_t get_offset_of_mapping_3() { return static_cast<int32_t>(offsetof(Regex_t1803876613, ___mapping_3)); }
	inline Il2CppObject * get_mapping_3() const { return ___mapping_3; }
	inline Il2CppObject ** get_address_of_mapping_3() { return &___mapping_3; }
	inline void set_mapping_3(Il2CppObject * value)
	{
		___mapping_3 = value;
		Il2CppCodeGenWriteBarrier(&___mapping_3, value);
	}

	inline static int32_t get_offset_of_group_count_4() { return static_cast<int32_t>(offsetof(Regex_t1803876613, ___group_count_4)); }
	inline int32_t get_group_count_4() const { return ___group_count_4; }
	inline int32_t* get_address_of_group_count_4() { return &___group_count_4; }
	inline void set_group_count_4(int32_t value)
	{
		___group_count_4 = value;
	}

	inline static int32_t get_offset_of_gap_5() { return static_cast<int32_t>(offsetof(Regex_t1803876613, ___gap_5)); }
	inline int32_t get_gap_5() const { return ___gap_5; }
	inline int32_t* get_address_of_gap_5() { return &___gap_5; }
	inline void set_gap_5(int32_t value)
	{
		___gap_5 = value;
	}

	inline static int32_t get_offset_of_group_names_6() { return static_cast<int32_t>(offsetof(Regex_t1803876613, ___group_names_6)); }
	inline StringU5BU5D_t1642385972* get_group_names_6() const { return ___group_names_6; }
	inline StringU5BU5D_t1642385972** get_address_of_group_names_6() { return &___group_names_6; }
	inline void set_group_names_6(StringU5BU5D_t1642385972* value)
	{
		___group_names_6 = value;
		Il2CppCodeGenWriteBarrier(&___group_names_6, value);
	}

	inline static int32_t get_offset_of_group_numbers_7() { return static_cast<int32_t>(offsetof(Regex_t1803876613, ___group_numbers_7)); }
	inline Int32U5BU5D_t3030399641* get_group_numbers_7() const { return ___group_numbers_7; }
	inline Int32U5BU5D_t3030399641** get_address_of_group_numbers_7() { return &___group_numbers_7; }
	inline void set_group_numbers_7(Int32U5BU5D_t3030399641* value)
	{
		___group_numbers_7 = value;
		Il2CppCodeGenWriteBarrier(&___group_numbers_7, value);
	}

	inline static int32_t get_offset_of_pattern_8() { return static_cast<int32_t>(offsetof(Regex_t1803876613, ___pattern_8)); }
	inline String_t* get_pattern_8() const { return ___pattern_8; }
	inline String_t** get_address_of_pattern_8() { return &___pattern_8; }
	inline void set_pattern_8(String_t* value)
	{
		___pattern_8 = value;
		Il2CppCodeGenWriteBarrier(&___pattern_8, value);
	}

	inline static int32_t get_offset_of_roptions_9() { return static_cast<int32_t>(offsetof(Regex_t1803876613, ___roptions_9)); }
	inline int32_t get_roptions_9() const { return ___roptions_9; }
	inline int32_t* get_address_of_roptions_9() { return &___roptions_9; }
	inline void set_roptions_9(int32_t value)
	{
		___roptions_9 = value;
	}
};

struct Regex_t1803876613_StaticFields
{
public:
	// System.Text.RegularExpressions.FactoryCache System.Text.RegularExpressions.Regex::cache
	FactoryCache_t2051534610 * ___cache_0;
	// System.Boolean System.Text.RegularExpressions.Regex::old_rx
	bool ___old_rx_1;

public:
	inline static int32_t get_offset_of_cache_0() { return static_cast<int32_t>(offsetof(Regex_t1803876613_StaticFields, ___cache_0)); }
	inline FactoryCache_t2051534610 * get_cache_0() const { return ___cache_0; }
	inline FactoryCache_t2051534610 ** get_address_of_cache_0() { return &___cache_0; }
	inline void set_cache_0(FactoryCache_t2051534610 * value)
	{
		___cache_0 = value;
		Il2CppCodeGenWriteBarrier(&___cache_0, value);
	}

	inline static int32_t get_offset_of_old_rx_1() { return static_cast<int32_t>(offsetof(Regex_t1803876613_StaticFields, ___old_rx_1)); }
	inline bool get_old_rx_1() const { return ___old_rx_1; }
	inline bool* get_address_of_old_rx_1() { return &___old_rx_1; }
	inline void set_old_rx_1(bool value)
	{
		___old_rx_1 = value;
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
