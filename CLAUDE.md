# CLAUDE.md – RogueLikeDSP (Plan Budowlany: Roguelike / Project Builder: Roguelike)

## Testy i repro – skill `rogue-test`
Przy **każdym update**, przed pushem zmian w `GBA/` lub `GODOT/`, przy **każdym wydaniu** i przy **zgłoszonym błędzie z gry**
(crash na Miyoo/iPhonie, „repro”) używaj skilla **`rogue-test`** (`~/.claude/skills/rogue-test/SKILL.md`, wywołanie `/rogue-test`).
Zawiera kolejność testów (dane i teksty PL/EN, rdzeń + cele balansu botów, test złoty C++↔C#, buildy, test dymny Godot PL/EN,
testy małpy GBA i Godot, zrzuty do obejrzenia, kontrola sekretów i tabów), repro błędów (scenariusze GBA, `monkey_replay.py`,
`--monkey SEED`) i kroki wydania.

## Zasady projektu
- Repo jest **publiczne**: żadnych ścieżek do kluczy, ID kluczy API, ID zespołu Apple, UDID, haseł w commitach – tylko
  w `GODOT/.env.local` (w .gitignore, wzór `GODOT/.env.example`).
- Przeciwnicy to **problemy budowy** – nigdy zawody, ludzie ani urzędnicy (ludzie mogą być nadawcami SMS-ów i pomocnikami).
- Bez nazw i znaków konsol w brandingu (starsza wersja = „wersja retro”).
- Logika gry jest wspólna: zmiana w `GBA/include/core.h`/`meta.h` → ta sama w `GODOT/src/LifeLike.Core` + odświeżony test złoty.
- Teksty dla gracza tylko w `GBA/data/game.json` (PL) i `GBA/data/lang/en.json` (EN) – nie wpisuj ich w kod.
- C#: jeden typ na plik, nawiasy Allman, 4 spacje (edytor Godota potrafi zamienić je na taby – nie commituj samych tabów).
- Wydania: wersja `v0.21.N` w `game.json`, sekcja w `GBA/CHANGELOG.md`, GitHub Release z ROM-em, Drive, TestFlight, Google Play.
- Plan prac i stan: [`TODO.md`](TODO.md).
