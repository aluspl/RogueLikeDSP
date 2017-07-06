#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "UnityEngine_UnityEngine_MonoBehaviour1158329972.h"

// UnityEngine.EventSystems.EventSystem
struct EventSystem_t3466835263;
// UnityEngine.GameObject
struct GameObject_t1756533147;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// MainMenuScript
struct  MainMenuScript_t4030058353  : public MonoBehaviour_t1158329972
{
public:
	// UnityEngine.EventSystems.EventSystem MainMenuScript::eventSystem
	EventSystem_t3466835263 * ___eventSystem_2;
	// UnityEngine.GameObject MainMenuScript::selectedObject
	GameObject_t1756533147 * ___selectedObject_3;
	// System.Boolean MainMenuScript::buttonSelected
	bool ___buttonSelected_4;

public:
	inline static int32_t get_offset_of_eventSystem_2() { return static_cast<int32_t>(offsetof(MainMenuScript_t4030058353, ___eventSystem_2)); }
	inline EventSystem_t3466835263 * get_eventSystem_2() const { return ___eventSystem_2; }
	inline EventSystem_t3466835263 ** get_address_of_eventSystem_2() { return &___eventSystem_2; }
	inline void set_eventSystem_2(EventSystem_t3466835263 * value)
	{
		___eventSystem_2 = value;
		Il2CppCodeGenWriteBarrier(&___eventSystem_2, value);
	}

	inline static int32_t get_offset_of_selectedObject_3() { return static_cast<int32_t>(offsetof(MainMenuScript_t4030058353, ___selectedObject_3)); }
	inline GameObject_t1756533147 * get_selectedObject_3() const { return ___selectedObject_3; }
	inline GameObject_t1756533147 ** get_address_of_selectedObject_3() { return &___selectedObject_3; }
	inline void set_selectedObject_3(GameObject_t1756533147 * value)
	{
		___selectedObject_3 = value;
		Il2CppCodeGenWriteBarrier(&___selectedObject_3, value);
	}

	inline static int32_t get_offset_of_buttonSelected_4() { return static_cast<int32_t>(offsetof(MainMenuScript_t4030058353, ___buttonSelected_4)); }
	inline bool get_buttonSelected_4() const { return ___buttonSelected_4; }
	inline bool* get_address_of_buttonSelected_4() { return &___buttonSelected_4; }
	inline void set_buttonSelected_4(bool value)
	{
		___buttonSelected_4 = value;
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
