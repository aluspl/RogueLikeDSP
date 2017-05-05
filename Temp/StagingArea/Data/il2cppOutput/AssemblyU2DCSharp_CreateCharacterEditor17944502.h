#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "UnityEngine_UnityEngine_MonoBehaviour1158329972.h"
#include "AssemblyU2DCSharp_Characters_CharacterStatisticDat2398261370.h"

// UnityEngine.UI.Slider
struct Slider_t297367283;
// UnityEngine.UI.Text
struct Text_t356221433;
// UnityEngine.Canvas
struct Canvas_t209405766;
// UnityEngine.UI.Dropdown
struct Dropdown_t1985816271;
// UnityEngine.UI.InputField
struct InputField_t1631627530;
// Characters.BaseCharacter
struct BaseCharacter_t1958497842;
// System.Collections.Generic.Dictionary`2<System.String,System.String>
struct Dictionary_2_t3943999495;
// System.String
struct String_t;
// UnityEngine.UI.Button
struct Button_t2872111280;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// CreateCharacterEditor
struct  CreateCharacterEditor_t17944502  : public MonoBehaviour_t1158329972
{
public:
	// UnityEngine.UI.Slider CreateCharacterEditor::StrengthSlider
	Slider_t297367283 * ___StrengthSlider_2;
	// UnityEngine.UI.Slider CreateCharacterEditor::InteligenceSlider
	Slider_t297367283 * ___InteligenceSlider_3;
	// UnityEngine.UI.Slider CreateCharacterEditor::CharismaSlider
	Slider_t297367283 * ___CharismaSlider_4;
	// UnityEngine.UI.Slider CreateCharacterEditor::AgilitySlider
	Slider_t297367283 * ___AgilitySlider_5;
	// UnityEngine.UI.Slider CreateCharacterEditor::EnduranceSlider
	Slider_t297367283 * ___EnduranceSlider_6;
	// UnityEngine.UI.Slider CreateCharacterEditor::PerceptionSlider
	Slider_t297367283 * ___PerceptionSlider_7;
	// UnityEngine.UI.Text CreateCharacterEditor::StrengthValue
	Text_t356221433 * ___StrengthValue_8;
	// UnityEngine.UI.Text CreateCharacterEditor::InteligenceValue
	Text_t356221433 * ___InteligenceValue_9;
	// UnityEngine.UI.Text CreateCharacterEditor::CharismaValue
	Text_t356221433 * ___CharismaValue_10;
	// UnityEngine.UI.Text CreateCharacterEditor::AgilityValue
	Text_t356221433 * ___AgilityValue_11;
	// UnityEngine.UI.Text CreateCharacterEditor::EnduranceValue
	Text_t356221433 * ___EnduranceValue_12;
	// UnityEngine.UI.Text CreateCharacterEditor::PerceptionValue
	Text_t356221433 * ___PerceptionValue_13;
	// UnityEngine.Canvas CreateCharacterEditor::_characterUiCanvas
	Canvas_t209405766 * ____characterUiCanvas_14;
	// UnityEngine.UI.Dropdown CreateCharacterEditor::ClassListDropdown
	Dropdown_t1985816271 * ___ClassListDropdown_15;
	// UnityEngine.UI.InputField CreateCharacterEditor::CharacterName
	InputField_t1631627530 * ___CharacterName_16;
	// Characters.BaseCharacter CreateCharacterEditor::Character
	BaseCharacter_t1958497842 * ___Character_17;
	// UnityEngine.UI.Text CreateCharacterEditor::CharacterLeftPoint
	Text_t356221433 * ___CharacterLeftPoint_18;
	// System.Int32 CreateCharacterEditor::CharacterLeftPointValue
	int32_t ___CharacterLeftPointValue_19;
	// System.Collections.Generic.Dictionary`2<System.String,System.String> CreateCharacterEditor::_characterClasses
	Dictionary_2_t3943999495 * ____characterClasses_20;
	// System.String CreateCharacterEditor::<SelectedClass>k__BackingField
	String_t* ___U3CSelectedClassU3Ek__BackingField_21;
	// UnityEngine.UI.Button CreateCharacterEditor::SaveButton
	Button_t2872111280 * ___SaveButton_22;
	// Characters.CharacterStatisticDataModel CreateCharacterEditor::Statistic
	CharacterStatisticDataModel_t2398261370  ___Statistic_23;

public:
	inline static int32_t get_offset_of_StrengthSlider_2() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ___StrengthSlider_2)); }
	inline Slider_t297367283 * get_StrengthSlider_2() const { return ___StrengthSlider_2; }
	inline Slider_t297367283 ** get_address_of_StrengthSlider_2() { return &___StrengthSlider_2; }
	inline void set_StrengthSlider_2(Slider_t297367283 * value)
	{
		___StrengthSlider_2 = value;
		Il2CppCodeGenWriteBarrier(&___StrengthSlider_2, value);
	}

	inline static int32_t get_offset_of_InteligenceSlider_3() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ___InteligenceSlider_3)); }
	inline Slider_t297367283 * get_InteligenceSlider_3() const { return ___InteligenceSlider_3; }
	inline Slider_t297367283 ** get_address_of_InteligenceSlider_3() { return &___InteligenceSlider_3; }
	inline void set_InteligenceSlider_3(Slider_t297367283 * value)
	{
		___InteligenceSlider_3 = value;
		Il2CppCodeGenWriteBarrier(&___InteligenceSlider_3, value);
	}

	inline static int32_t get_offset_of_CharismaSlider_4() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ___CharismaSlider_4)); }
	inline Slider_t297367283 * get_CharismaSlider_4() const { return ___CharismaSlider_4; }
	inline Slider_t297367283 ** get_address_of_CharismaSlider_4() { return &___CharismaSlider_4; }
	inline void set_CharismaSlider_4(Slider_t297367283 * value)
	{
		___CharismaSlider_4 = value;
		Il2CppCodeGenWriteBarrier(&___CharismaSlider_4, value);
	}

	inline static int32_t get_offset_of_AgilitySlider_5() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ___AgilitySlider_5)); }
	inline Slider_t297367283 * get_AgilitySlider_5() const { return ___AgilitySlider_5; }
	inline Slider_t297367283 ** get_address_of_AgilitySlider_5() { return &___AgilitySlider_5; }
	inline void set_AgilitySlider_5(Slider_t297367283 * value)
	{
		___AgilitySlider_5 = value;
		Il2CppCodeGenWriteBarrier(&___AgilitySlider_5, value);
	}

	inline static int32_t get_offset_of_EnduranceSlider_6() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ___EnduranceSlider_6)); }
	inline Slider_t297367283 * get_EnduranceSlider_6() const { return ___EnduranceSlider_6; }
	inline Slider_t297367283 ** get_address_of_EnduranceSlider_6() { return &___EnduranceSlider_6; }
	inline void set_EnduranceSlider_6(Slider_t297367283 * value)
	{
		___EnduranceSlider_6 = value;
		Il2CppCodeGenWriteBarrier(&___EnduranceSlider_6, value);
	}

	inline static int32_t get_offset_of_PerceptionSlider_7() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ___PerceptionSlider_7)); }
	inline Slider_t297367283 * get_PerceptionSlider_7() const { return ___PerceptionSlider_7; }
	inline Slider_t297367283 ** get_address_of_PerceptionSlider_7() { return &___PerceptionSlider_7; }
	inline void set_PerceptionSlider_7(Slider_t297367283 * value)
	{
		___PerceptionSlider_7 = value;
		Il2CppCodeGenWriteBarrier(&___PerceptionSlider_7, value);
	}

	inline static int32_t get_offset_of_StrengthValue_8() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ___StrengthValue_8)); }
	inline Text_t356221433 * get_StrengthValue_8() const { return ___StrengthValue_8; }
	inline Text_t356221433 ** get_address_of_StrengthValue_8() { return &___StrengthValue_8; }
	inline void set_StrengthValue_8(Text_t356221433 * value)
	{
		___StrengthValue_8 = value;
		Il2CppCodeGenWriteBarrier(&___StrengthValue_8, value);
	}

	inline static int32_t get_offset_of_InteligenceValue_9() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ___InteligenceValue_9)); }
	inline Text_t356221433 * get_InteligenceValue_9() const { return ___InteligenceValue_9; }
	inline Text_t356221433 ** get_address_of_InteligenceValue_9() { return &___InteligenceValue_9; }
	inline void set_InteligenceValue_9(Text_t356221433 * value)
	{
		___InteligenceValue_9 = value;
		Il2CppCodeGenWriteBarrier(&___InteligenceValue_9, value);
	}

	inline static int32_t get_offset_of_CharismaValue_10() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ___CharismaValue_10)); }
	inline Text_t356221433 * get_CharismaValue_10() const { return ___CharismaValue_10; }
	inline Text_t356221433 ** get_address_of_CharismaValue_10() { return &___CharismaValue_10; }
	inline void set_CharismaValue_10(Text_t356221433 * value)
	{
		___CharismaValue_10 = value;
		Il2CppCodeGenWriteBarrier(&___CharismaValue_10, value);
	}

	inline static int32_t get_offset_of_AgilityValue_11() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ___AgilityValue_11)); }
	inline Text_t356221433 * get_AgilityValue_11() const { return ___AgilityValue_11; }
	inline Text_t356221433 ** get_address_of_AgilityValue_11() { return &___AgilityValue_11; }
	inline void set_AgilityValue_11(Text_t356221433 * value)
	{
		___AgilityValue_11 = value;
		Il2CppCodeGenWriteBarrier(&___AgilityValue_11, value);
	}

	inline static int32_t get_offset_of_EnduranceValue_12() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ___EnduranceValue_12)); }
	inline Text_t356221433 * get_EnduranceValue_12() const { return ___EnduranceValue_12; }
	inline Text_t356221433 ** get_address_of_EnduranceValue_12() { return &___EnduranceValue_12; }
	inline void set_EnduranceValue_12(Text_t356221433 * value)
	{
		___EnduranceValue_12 = value;
		Il2CppCodeGenWriteBarrier(&___EnduranceValue_12, value);
	}

	inline static int32_t get_offset_of_PerceptionValue_13() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ___PerceptionValue_13)); }
	inline Text_t356221433 * get_PerceptionValue_13() const { return ___PerceptionValue_13; }
	inline Text_t356221433 ** get_address_of_PerceptionValue_13() { return &___PerceptionValue_13; }
	inline void set_PerceptionValue_13(Text_t356221433 * value)
	{
		___PerceptionValue_13 = value;
		Il2CppCodeGenWriteBarrier(&___PerceptionValue_13, value);
	}

	inline static int32_t get_offset_of__characterUiCanvas_14() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ____characterUiCanvas_14)); }
	inline Canvas_t209405766 * get__characterUiCanvas_14() const { return ____characterUiCanvas_14; }
	inline Canvas_t209405766 ** get_address_of__characterUiCanvas_14() { return &____characterUiCanvas_14; }
	inline void set__characterUiCanvas_14(Canvas_t209405766 * value)
	{
		____characterUiCanvas_14 = value;
		Il2CppCodeGenWriteBarrier(&____characterUiCanvas_14, value);
	}

	inline static int32_t get_offset_of_ClassListDropdown_15() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ___ClassListDropdown_15)); }
	inline Dropdown_t1985816271 * get_ClassListDropdown_15() const { return ___ClassListDropdown_15; }
	inline Dropdown_t1985816271 ** get_address_of_ClassListDropdown_15() { return &___ClassListDropdown_15; }
	inline void set_ClassListDropdown_15(Dropdown_t1985816271 * value)
	{
		___ClassListDropdown_15 = value;
		Il2CppCodeGenWriteBarrier(&___ClassListDropdown_15, value);
	}

	inline static int32_t get_offset_of_CharacterName_16() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ___CharacterName_16)); }
	inline InputField_t1631627530 * get_CharacterName_16() const { return ___CharacterName_16; }
	inline InputField_t1631627530 ** get_address_of_CharacterName_16() { return &___CharacterName_16; }
	inline void set_CharacterName_16(InputField_t1631627530 * value)
	{
		___CharacterName_16 = value;
		Il2CppCodeGenWriteBarrier(&___CharacterName_16, value);
	}

	inline static int32_t get_offset_of_Character_17() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ___Character_17)); }
	inline BaseCharacter_t1958497842 * get_Character_17() const { return ___Character_17; }
	inline BaseCharacter_t1958497842 ** get_address_of_Character_17() { return &___Character_17; }
	inline void set_Character_17(BaseCharacter_t1958497842 * value)
	{
		___Character_17 = value;
		Il2CppCodeGenWriteBarrier(&___Character_17, value);
	}

	inline static int32_t get_offset_of_CharacterLeftPoint_18() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ___CharacterLeftPoint_18)); }
	inline Text_t356221433 * get_CharacterLeftPoint_18() const { return ___CharacterLeftPoint_18; }
	inline Text_t356221433 ** get_address_of_CharacterLeftPoint_18() { return &___CharacterLeftPoint_18; }
	inline void set_CharacterLeftPoint_18(Text_t356221433 * value)
	{
		___CharacterLeftPoint_18 = value;
		Il2CppCodeGenWriteBarrier(&___CharacterLeftPoint_18, value);
	}

	inline static int32_t get_offset_of_CharacterLeftPointValue_19() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ___CharacterLeftPointValue_19)); }
	inline int32_t get_CharacterLeftPointValue_19() const { return ___CharacterLeftPointValue_19; }
	inline int32_t* get_address_of_CharacterLeftPointValue_19() { return &___CharacterLeftPointValue_19; }
	inline void set_CharacterLeftPointValue_19(int32_t value)
	{
		___CharacterLeftPointValue_19 = value;
	}

	inline static int32_t get_offset_of__characterClasses_20() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ____characterClasses_20)); }
	inline Dictionary_2_t3943999495 * get__characterClasses_20() const { return ____characterClasses_20; }
	inline Dictionary_2_t3943999495 ** get_address_of__characterClasses_20() { return &____characterClasses_20; }
	inline void set__characterClasses_20(Dictionary_2_t3943999495 * value)
	{
		____characterClasses_20 = value;
		Il2CppCodeGenWriteBarrier(&____characterClasses_20, value);
	}

	inline static int32_t get_offset_of_U3CSelectedClassU3Ek__BackingField_21() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ___U3CSelectedClassU3Ek__BackingField_21)); }
	inline String_t* get_U3CSelectedClassU3Ek__BackingField_21() const { return ___U3CSelectedClassU3Ek__BackingField_21; }
	inline String_t** get_address_of_U3CSelectedClassU3Ek__BackingField_21() { return &___U3CSelectedClassU3Ek__BackingField_21; }
	inline void set_U3CSelectedClassU3Ek__BackingField_21(String_t* value)
	{
		___U3CSelectedClassU3Ek__BackingField_21 = value;
		Il2CppCodeGenWriteBarrier(&___U3CSelectedClassU3Ek__BackingField_21, value);
	}

	inline static int32_t get_offset_of_SaveButton_22() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ___SaveButton_22)); }
	inline Button_t2872111280 * get_SaveButton_22() const { return ___SaveButton_22; }
	inline Button_t2872111280 ** get_address_of_SaveButton_22() { return &___SaveButton_22; }
	inline void set_SaveButton_22(Button_t2872111280 * value)
	{
		___SaveButton_22 = value;
		Il2CppCodeGenWriteBarrier(&___SaveButton_22, value);
	}

	inline static int32_t get_offset_of_Statistic_23() { return static_cast<int32_t>(offsetof(CreateCharacterEditor_t17944502, ___Statistic_23)); }
	inline CharacterStatisticDataModel_t2398261370  get_Statistic_23() const { return ___Statistic_23; }
	inline CharacterStatisticDataModel_t2398261370 * get_address_of_Statistic_23() { return &___Statistic_23; }
	inline void set_Statistic_23(CharacterStatisticDataModel_t2398261370  value)
	{
		___Statistic_23 = value;
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
