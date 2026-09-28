namespace LifeLike.Core.Data;

/// <summary>
/// „Problem budowy” (wróg). Slam = boss zapowiada uderzenie w obszar (Shape: kwadrat albo krzyż, SlamName w komunikatach).
/// Mechaniki bossów (domyślnie wyłączone): wezwania (Summon co SummonEvery tur, najwyżej SummonMax na walkę),
/// ogłuszenie na start walki przy pełnym sprzęcie (GearStun), nagroda za pokonanie (RewardCash, RewardTitle).
/// Material: materiał z usuniętego problemu (GameData.Materials), -1 = losowy.
/// Tags: zachowania (bity Behavior, pole "behaviors"), łączone dowolnie.
/// v0.21.50: Elem – woda (zawsze mokry, moczy bohatera), prąd (porażenie mokrego); Gender – 0 m, 1 ż, 2 n / l.mn. (przedrostek elity).
/// Druga faza bossa (pole "phase"): raz przy PhasePct% HP odzyskuje PhaseHeal% max HP i wzywa PhaseSummon problemów.
/// </summary>
public sealed record EnemyDef(
    string Id, string Name, string Desc, int MaxHealth, int MinDamage, int MaxDamage, int Defense, int Sight, int Score,
    int Frame, bool Slam, StatusEffect OnHit, int StatusChance, int StatusTurns,
    SlamShape Shape = SlamShape.Square, string SlamName = "", int Summon = -1, int SummonEvery = 0, int SummonMax = 0,
    int GearStun = 0, int RewardCash = 0, string RewardTitle = "", int Material = -1, int Tags = 0,
    int PhasePct = 0, int PhaseHeal = 0, int PhaseSummon = 0, string PhaseName = "", Element Elem = Element.None, int Gender = 0);
