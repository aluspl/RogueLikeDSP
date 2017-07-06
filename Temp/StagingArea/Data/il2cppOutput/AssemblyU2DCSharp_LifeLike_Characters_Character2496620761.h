#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "mscorlib_System_Object2689449295.h"
#include "AssemblyU2DCSharp_LifeLike_Enums_Status2904171214.h"

// System.String
struct String_t;
// System.Random
struct Random_t1044426839;
// LifeLike.Inferfaces.IWeapon
struct IWeapon_t3772527613;
// LifeLike.Interfaces.IHead
struct IHead_t2064629019;
// LifeLike.Interfaces.IArmor
struct IArmor_t144687040;
// LifeLike.Interfaces.IPants
struct IPants_t340395971;
// LifeLike.Interfaces.IGloves
struct IGloves_t1078915127;
// LifeLike.Interfaces.IShoes
struct IShoes_t3098197477;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// LifeLike.Characters.Character
struct  Character_t2496620761  : public Il2CppObject
{
public:
	// System.Random LifeLike.Characters.Character::random
	Random_t1044426839 * ___random_1;
	// System.String LifeLike.Characters.Character::<SelectedSpecialAttack>k__BackingField
	String_t* ___U3CSelectedSpecialAttackU3Ek__BackingField_2;
	// System.String LifeLike.Characters.Character::<Name>k__BackingField
	String_t* ___U3CNameU3Ek__BackingField_3;
	// System.Int32 LifeLike.Characters.Character::<Strength>k__BackingField
	int32_t ___U3CStrengthU3Ek__BackingField_4;
	// System.Int32 LifeLike.Characters.Character::<Level>k__BackingField
	int32_t ___U3CLevelU3Ek__BackingField_5;
	// System.Int64 LifeLike.Characters.Character::<CurrentExperience>k__BackingField
	int64_t ___U3CCurrentExperienceU3Ek__BackingField_6;
	// System.Int32 LifeLike.Characters.Character::<Inteligence>k__BackingField
	int32_t ___U3CInteligenceU3Ek__BackingField_7;
	// System.Int32 LifeLike.Characters.Character::<Perception>k__BackingField
	int32_t ___U3CPerceptionU3Ek__BackingField_8;
	// System.Int32 LifeLike.Characters.Character::<Charisma>k__BackingField
	int32_t ___U3CCharismaU3Ek__BackingField_9;
	// System.Int32 LifeLike.Characters.Character::<Agility>k__BackingField
	int32_t ___U3CAgilityU3Ek__BackingField_10;
	// System.Int32 LifeLike.Characters.Character::<Endurance>k__BackingField
	int32_t ___U3CEnduranceU3Ek__BackingField_11;
	// System.Int32 LifeLike.Characters.Character::<HealthPoint>k__BackingField
	int32_t ___U3CHealthPointU3Ek__BackingField_12;
	// LifeLike.Enums.Status LifeLike.Characters.Character::<Status>k__BackingField
	int32_t ___U3CStatusU3Ek__BackingField_13;
	// System.String LifeLike.Characters.Character::<SelectedClass>k__BackingField
	String_t* ___U3CSelectedClassU3Ek__BackingField_14;
	// System.Int32 LifeLike.Characters.Character::<MaxHealthPoint>k__BackingField
	int32_t ___U3CMaxHealthPointU3Ek__BackingField_15;
	// System.Boolean LifeLike.Characters.Character::<isEnemy>k__BackingField
	bool ___U3CisEnemyU3Ek__BackingField_16;
	// System.Int32 LifeLike.Characters.Character::<KilledEnemies>k__BackingField
	int32_t ___U3CKilledEnemiesU3Ek__BackingField_17;
	// System.Int32 LifeLike.Characters.Character::SpecialActionIndex
	int32_t ___SpecialActionIndex_18;
	// LifeLike.Inferfaces.IWeapon LifeLike.Characters.Character::<SelectedWeapon>k__BackingField
	Il2CppObject * ___U3CSelectedWeaponU3Ek__BackingField_19;
	// LifeLike.Interfaces.IHead LifeLike.Characters.Character::<SelectedHead>k__BackingField
	Il2CppObject * ___U3CSelectedHeadU3Ek__BackingField_20;
	// LifeLike.Interfaces.IArmor LifeLike.Characters.Character::<SelectedArmor>k__BackingField
	Il2CppObject * ___U3CSelectedArmorU3Ek__BackingField_21;
	// LifeLike.Interfaces.IPants LifeLike.Characters.Character::<SelectedPants>k__BackingField
	Il2CppObject * ___U3CSelectedPantsU3Ek__BackingField_22;
	// LifeLike.Interfaces.IGloves LifeLike.Characters.Character::<SelectedGloves>k__BackingField
	Il2CppObject * ___U3CSelectedGlovesU3Ek__BackingField_23;
	// LifeLike.Interfaces.IShoes LifeLike.Characters.Character::<SelectedShoes>k__BackingField
	Il2CppObject * ___U3CSelectedShoesU3Ek__BackingField_24;

public:
	inline static int32_t get_offset_of_random_1() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___random_1)); }
	inline Random_t1044426839 * get_random_1() const { return ___random_1; }
	inline Random_t1044426839 ** get_address_of_random_1() { return &___random_1; }
	inline void set_random_1(Random_t1044426839 * value)
	{
		___random_1 = value;
		Il2CppCodeGenWriteBarrier(&___random_1, value);
	}

	inline static int32_t get_offset_of_U3CSelectedSpecialAttackU3Ek__BackingField_2() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CSelectedSpecialAttackU3Ek__BackingField_2)); }
	inline String_t* get_U3CSelectedSpecialAttackU3Ek__BackingField_2() const { return ___U3CSelectedSpecialAttackU3Ek__BackingField_2; }
	inline String_t** get_address_of_U3CSelectedSpecialAttackU3Ek__BackingField_2() { return &___U3CSelectedSpecialAttackU3Ek__BackingField_2; }
	inline void set_U3CSelectedSpecialAttackU3Ek__BackingField_2(String_t* value)
	{
		___U3CSelectedSpecialAttackU3Ek__BackingField_2 = value;
		Il2CppCodeGenWriteBarrier(&___U3CSelectedSpecialAttackU3Ek__BackingField_2, value);
	}

	inline static int32_t get_offset_of_U3CNameU3Ek__BackingField_3() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CNameU3Ek__BackingField_3)); }
	inline String_t* get_U3CNameU3Ek__BackingField_3() const { return ___U3CNameU3Ek__BackingField_3; }
	inline String_t** get_address_of_U3CNameU3Ek__BackingField_3() { return &___U3CNameU3Ek__BackingField_3; }
	inline void set_U3CNameU3Ek__BackingField_3(String_t* value)
	{
		___U3CNameU3Ek__BackingField_3 = value;
		Il2CppCodeGenWriteBarrier(&___U3CNameU3Ek__BackingField_3, value);
	}

	inline static int32_t get_offset_of_U3CStrengthU3Ek__BackingField_4() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CStrengthU3Ek__BackingField_4)); }
	inline int32_t get_U3CStrengthU3Ek__BackingField_4() const { return ___U3CStrengthU3Ek__BackingField_4; }
	inline int32_t* get_address_of_U3CStrengthU3Ek__BackingField_4() { return &___U3CStrengthU3Ek__BackingField_4; }
	inline void set_U3CStrengthU3Ek__BackingField_4(int32_t value)
	{
		___U3CStrengthU3Ek__BackingField_4 = value;
	}

	inline static int32_t get_offset_of_U3CLevelU3Ek__BackingField_5() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CLevelU3Ek__BackingField_5)); }
	inline int32_t get_U3CLevelU3Ek__BackingField_5() const { return ___U3CLevelU3Ek__BackingField_5; }
	inline int32_t* get_address_of_U3CLevelU3Ek__BackingField_5() { return &___U3CLevelU3Ek__BackingField_5; }
	inline void set_U3CLevelU3Ek__BackingField_5(int32_t value)
	{
		___U3CLevelU3Ek__BackingField_5 = value;
	}

	inline static int32_t get_offset_of_U3CCurrentExperienceU3Ek__BackingField_6() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CCurrentExperienceU3Ek__BackingField_6)); }
	inline int64_t get_U3CCurrentExperienceU3Ek__BackingField_6() const { return ___U3CCurrentExperienceU3Ek__BackingField_6; }
	inline int64_t* get_address_of_U3CCurrentExperienceU3Ek__BackingField_6() { return &___U3CCurrentExperienceU3Ek__BackingField_6; }
	inline void set_U3CCurrentExperienceU3Ek__BackingField_6(int64_t value)
	{
		___U3CCurrentExperienceU3Ek__BackingField_6 = value;
	}

	inline static int32_t get_offset_of_U3CInteligenceU3Ek__BackingField_7() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CInteligenceU3Ek__BackingField_7)); }
	inline int32_t get_U3CInteligenceU3Ek__BackingField_7() const { return ___U3CInteligenceU3Ek__BackingField_7; }
	inline int32_t* get_address_of_U3CInteligenceU3Ek__BackingField_7() { return &___U3CInteligenceU3Ek__BackingField_7; }
	inline void set_U3CInteligenceU3Ek__BackingField_7(int32_t value)
	{
		___U3CInteligenceU3Ek__BackingField_7 = value;
	}

	inline static int32_t get_offset_of_U3CPerceptionU3Ek__BackingField_8() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CPerceptionU3Ek__BackingField_8)); }
	inline int32_t get_U3CPerceptionU3Ek__BackingField_8() const { return ___U3CPerceptionU3Ek__BackingField_8; }
	inline int32_t* get_address_of_U3CPerceptionU3Ek__BackingField_8() { return &___U3CPerceptionU3Ek__BackingField_8; }
	inline void set_U3CPerceptionU3Ek__BackingField_8(int32_t value)
	{
		___U3CPerceptionU3Ek__BackingField_8 = value;
	}

	inline static int32_t get_offset_of_U3CCharismaU3Ek__BackingField_9() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CCharismaU3Ek__BackingField_9)); }
	inline int32_t get_U3CCharismaU3Ek__BackingField_9() const { return ___U3CCharismaU3Ek__BackingField_9; }
	inline int32_t* get_address_of_U3CCharismaU3Ek__BackingField_9() { return &___U3CCharismaU3Ek__BackingField_9; }
	inline void set_U3CCharismaU3Ek__BackingField_9(int32_t value)
	{
		___U3CCharismaU3Ek__BackingField_9 = value;
	}

	inline static int32_t get_offset_of_U3CAgilityU3Ek__BackingField_10() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CAgilityU3Ek__BackingField_10)); }
	inline int32_t get_U3CAgilityU3Ek__BackingField_10() const { return ___U3CAgilityU3Ek__BackingField_10; }
	inline int32_t* get_address_of_U3CAgilityU3Ek__BackingField_10() { return &___U3CAgilityU3Ek__BackingField_10; }
	inline void set_U3CAgilityU3Ek__BackingField_10(int32_t value)
	{
		___U3CAgilityU3Ek__BackingField_10 = value;
	}

	inline static int32_t get_offset_of_U3CEnduranceU3Ek__BackingField_11() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CEnduranceU3Ek__BackingField_11)); }
	inline int32_t get_U3CEnduranceU3Ek__BackingField_11() const { return ___U3CEnduranceU3Ek__BackingField_11; }
	inline int32_t* get_address_of_U3CEnduranceU3Ek__BackingField_11() { return &___U3CEnduranceU3Ek__BackingField_11; }
	inline void set_U3CEnduranceU3Ek__BackingField_11(int32_t value)
	{
		___U3CEnduranceU3Ek__BackingField_11 = value;
	}

	inline static int32_t get_offset_of_U3CHealthPointU3Ek__BackingField_12() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CHealthPointU3Ek__BackingField_12)); }
	inline int32_t get_U3CHealthPointU3Ek__BackingField_12() const { return ___U3CHealthPointU3Ek__BackingField_12; }
	inline int32_t* get_address_of_U3CHealthPointU3Ek__BackingField_12() { return &___U3CHealthPointU3Ek__BackingField_12; }
	inline void set_U3CHealthPointU3Ek__BackingField_12(int32_t value)
	{
		___U3CHealthPointU3Ek__BackingField_12 = value;
	}

	inline static int32_t get_offset_of_U3CStatusU3Ek__BackingField_13() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CStatusU3Ek__BackingField_13)); }
	inline int32_t get_U3CStatusU3Ek__BackingField_13() const { return ___U3CStatusU3Ek__BackingField_13; }
	inline int32_t* get_address_of_U3CStatusU3Ek__BackingField_13() { return &___U3CStatusU3Ek__BackingField_13; }
	inline void set_U3CStatusU3Ek__BackingField_13(int32_t value)
	{
		___U3CStatusU3Ek__BackingField_13 = value;
	}

	inline static int32_t get_offset_of_U3CSelectedClassU3Ek__BackingField_14() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CSelectedClassU3Ek__BackingField_14)); }
	inline String_t* get_U3CSelectedClassU3Ek__BackingField_14() const { return ___U3CSelectedClassU3Ek__BackingField_14; }
	inline String_t** get_address_of_U3CSelectedClassU3Ek__BackingField_14() { return &___U3CSelectedClassU3Ek__BackingField_14; }
	inline void set_U3CSelectedClassU3Ek__BackingField_14(String_t* value)
	{
		___U3CSelectedClassU3Ek__BackingField_14 = value;
		Il2CppCodeGenWriteBarrier(&___U3CSelectedClassU3Ek__BackingField_14, value);
	}

	inline static int32_t get_offset_of_U3CMaxHealthPointU3Ek__BackingField_15() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CMaxHealthPointU3Ek__BackingField_15)); }
	inline int32_t get_U3CMaxHealthPointU3Ek__BackingField_15() const { return ___U3CMaxHealthPointU3Ek__BackingField_15; }
	inline int32_t* get_address_of_U3CMaxHealthPointU3Ek__BackingField_15() { return &___U3CMaxHealthPointU3Ek__BackingField_15; }
	inline void set_U3CMaxHealthPointU3Ek__BackingField_15(int32_t value)
	{
		___U3CMaxHealthPointU3Ek__BackingField_15 = value;
	}

	inline static int32_t get_offset_of_U3CisEnemyU3Ek__BackingField_16() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CisEnemyU3Ek__BackingField_16)); }
	inline bool get_U3CisEnemyU3Ek__BackingField_16() const { return ___U3CisEnemyU3Ek__BackingField_16; }
	inline bool* get_address_of_U3CisEnemyU3Ek__BackingField_16() { return &___U3CisEnemyU3Ek__BackingField_16; }
	inline void set_U3CisEnemyU3Ek__BackingField_16(bool value)
	{
		___U3CisEnemyU3Ek__BackingField_16 = value;
	}

	inline static int32_t get_offset_of_U3CKilledEnemiesU3Ek__BackingField_17() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CKilledEnemiesU3Ek__BackingField_17)); }
	inline int32_t get_U3CKilledEnemiesU3Ek__BackingField_17() const { return ___U3CKilledEnemiesU3Ek__BackingField_17; }
	inline int32_t* get_address_of_U3CKilledEnemiesU3Ek__BackingField_17() { return &___U3CKilledEnemiesU3Ek__BackingField_17; }
	inline void set_U3CKilledEnemiesU3Ek__BackingField_17(int32_t value)
	{
		___U3CKilledEnemiesU3Ek__BackingField_17 = value;
	}

	inline static int32_t get_offset_of_SpecialActionIndex_18() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___SpecialActionIndex_18)); }
	inline int32_t get_SpecialActionIndex_18() const { return ___SpecialActionIndex_18; }
	inline int32_t* get_address_of_SpecialActionIndex_18() { return &___SpecialActionIndex_18; }
	inline void set_SpecialActionIndex_18(int32_t value)
	{
		___SpecialActionIndex_18 = value;
	}

	inline static int32_t get_offset_of_U3CSelectedWeaponU3Ek__BackingField_19() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CSelectedWeaponU3Ek__BackingField_19)); }
	inline Il2CppObject * get_U3CSelectedWeaponU3Ek__BackingField_19() const { return ___U3CSelectedWeaponU3Ek__BackingField_19; }
	inline Il2CppObject ** get_address_of_U3CSelectedWeaponU3Ek__BackingField_19() { return &___U3CSelectedWeaponU3Ek__BackingField_19; }
	inline void set_U3CSelectedWeaponU3Ek__BackingField_19(Il2CppObject * value)
	{
		___U3CSelectedWeaponU3Ek__BackingField_19 = value;
		Il2CppCodeGenWriteBarrier(&___U3CSelectedWeaponU3Ek__BackingField_19, value);
	}

	inline static int32_t get_offset_of_U3CSelectedHeadU3Ek__BackingField_20() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CSelectedHeadU3Ek__BackingField_20)); }
	inline Il2CppObject * get_U3CSelectedHeadU3Ek__BackingField_20() const { return ___U3CSelectedHeadU3Ek__BackingField_20; }
	inline Il2CppObject ** get_address_of_U3CSelectedHeadU3Ek__BackingField_20() { return &___U3CSelectedHeadU3Ek__BackingField_20; }
	inline void set_U3CSelectedHeadU3Ek__BackingField_20(Il2CppObject * value)
	{
		___U3CSelectedHeadU3Ek__BackingField_20 = value;
		Il2CppCodeGenWriteBarrier(&___U3CSelectedHeadU3Ek__BackingField_20, value);
	}

	inline static int32_t get_offset_of_U3CSelectedArmorU3Ek__BackingField_21() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CSelectedArmorU3Ek__BackingField_21)); }
	inline Il2CppObject * get_U3CSelectedArmorU3Ek__BackingField_21() const { return ___U3CSelectedArmorU3Ek__BackingField_21; }
	inline Il2CppObject ** get_address_of_U3CSelectedArmorU3Ek__BackingField_21() { return &___U3CSelectedArmorU3Ek__BackingField_21; }
	inline void set_U3CSelectedArmorU3Ek__BackingField_21(Il2CppObject * value)
	{
		___U3CSelectedArmorU3Ek__BackingField_21 = value;
		Il2CppCodeGenWriteBarrier(&___U3CSelectedArmorU3Ek__BackingField_21, value);
	}

	inline static int32_t get_offset_of_U3CSelectedPantsU3Ek__BackingField_22() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CSelectedPantsU3Ek__BackingField_22)); }
	inline Il2CppObject * get_U3CSelectedPantsU3Ek__BackingField_22() const { return ___U3CSelectedPantsU3Ek__BackingField_22; }
	inline Il2CppObject ** get_address_of_U3CSelectedPantsU3Ek__BackingField_22() { return &___U3CSelectedPantsU3Ek__BackingField_22; }
	inline void set_U3CSelectedPantsU3Ek__BackingField_22(Il2CppObject * value)
	{
		___U3CSelectedPantsU3Ek__BackingField_22 = value;
		Il2CppCodeGenWriteBarrier(&___U3CSelectedPantsU3Ek__BackingField_22, value);
	}

	inline static int32_t get_offset_of_U3CSelectedGlovesU3Ek__BackingField_23() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CSelectedGlovesU3Ek__BackingField_23)); }
	inline Il2CppObject * get_U3CSelectedGlovesU3Ek__BackingField_23() const { return ___U3CSelectedGlovesU3Ek__BackingField_23; }
	inline Il2CppObject ** get_address_of_U3CSelectedGlovesU3Ek__BackingField_23() { return &___U3CSelectedGlovesU3Ek__BackingField_23; }
	inline void set_U3CSelectedGlovesU3Ek__BackingField_23(Il2CppObject * value)
	{
		___U3CSelectedGlovesU3Ek__BackingField_23 = value;
		Il2CppCodeGenWriteBarrier(&___U3CSelectedGlovesU3Ek__BackingField_23, value);
	}

	inline static int32_t get_offset_of_U3CSelectedShoesU3Ek__BackingField_24() { return static_cast<int32_t>(offsetof(Character_t2496620761, ___U3CSelectedShoesU3Ek__BackingField_24)); }
	inline Il2CppObject * get_U3CSelectedShoesU3Ek__BackingField_24() const { return ___U3CSelectedShoesU3Ek__BackingField_24; }
	inline Il2CppObject ** get_address_of_U3CSelectedShoesU3Ek__BackingField_24() { return &___U3CSelectedShoesU3Ek__BackingField_24; }
	inline void set_U3CSelectedShoesU3Ek__BackingField_24(Il2CppObject * value)
	{
		___U3CSelectedShoesU3Ek__BackingField_24 = value;
		Il2CppCodeGenWriteBarrier(&___U3CSelectedShoesU3Ek__BackingField_24, value);
	}
};

struct Character_t2496620761_StaticFields
{
public:
	// System.String LifeLike.Characters.Character::ClassName
	String_t* ___ClassName_0;

public:
	inline static int32_t get_offset_of_ClassName_0() { return static_cast<int32_t>(offsetof(Character_t2496620761_StaticFields, ___ClassName_0)); }
	inline String_t* get_ClassName_0() const { return ___ClassName_0; }
	inline String_t** get_address_of_ClassName_0() { return &___ClassName_0; }
	inline void set_ClassName_0(String_t* value)
	{
		___ClassName_0 = value;
		Il2CppCodeGenWriteBarrier(&___ClassName_0, value);
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif
