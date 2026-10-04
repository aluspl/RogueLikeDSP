# CLAUDE.md – RogueLikeDSP (Plan Budowlany: Roguelike / Project Builder: Roguelike)

Roguelike o budowie domu w dwóch wersjach ze **wspólną logiką i danymi**: GBA (Butano, C++20, `GBA/`) i Godot 4.7 .NET
(C#, `GODOT/`, iOS/Android/desktop). Stan prac i plan: [`TODO.md`](TODO.md). Historia: [`GBA/CHANGELOG.md`](GBA/CHANGELOG.md).

## Sposób pracy (ustalony z właścicielem)
- **Wersje:** seria 0.21 zamknięta na v0.21.54 (branch `release/0.21`). Teraz seria **0.22** (branch `dev/0.22`).
  Wszystkie pomysły i poprawki trafiają do **otwartej wersji**; po skończonym pakiecie pokaż changelog i **zapytaj „wydajemy?”**.
  Nie rób osobnego wydania po każdej części.
- **Testy oszczędnie:** pełny zestaw (skill `rogue-test`) **raz przed wydaniem**. Po pojedynczej zmianie tylko szybkie
  kroki: build, testy jednostkowe, test dymny, kilka seedów małpy dla dotkniętej platformy. Nie czekaj na CI między częściami.
- **Agenci:** większe zadania deleguj do jednego agenta na raz na te same pliki (GBA i `GODOT/godot` mogą iść równolegle);
  agent robi małe commity po każdym kawałku (limity sesji potrafią go przerwać), bez push/tagów/`--force`; po przerwie
  wznawiaj go (SendMessage), nie zaczynaj od nowa. Koordynator pushuje. Bez fan-outu (sub-agentów) – zjada limit.
- **Push** bez pytania (bez `--force`); hook pluginu blokuje przepisywanie historii, `--force` i usuwanie tagów –
  takie rzeczy robi właściciel.
- Gdy właściciel mówi „pauza” – nic nie zaczynaj, tylko dopisuj pomysły do `TODO.md`.

## Testy i repro – skill `rogue-test`
Przy update, przed pushem zmian w `GBA/`/`GODOT/`, przy wydaniu i przy zgłoszonym błędzie używaj skilla **`rogue-test`**
(`~/.claude/skills/rogue-test/SKILL.md`, `/rogue-test`): dane i teksty PL/EN, rdzeń + cele balansu botów, test złoty
C++↔C#, buildy, test dymny Godot PL/EN, testy małpy GBA i Godot, zrzuty do obejrzenia, kontrola sekretów i tabów,
repro (scenariusze GBA, `monkey_replay.py`, `--monkey SEED`), kroki wydania.

## Zasady projektu
- Repo **publiczne**: ścieżki do kluczy, ID kluczy API, zespół Apple, UDID, hasła tylko w `GODOT/.env.local`
  (w .gitignore, wzór `GODOT/.env.example`). Przed pushem `git grep` na te wartości.
- Przeciwnicy to **problemy budowy** – nigdy zawody, ludzie, urzędnicy (ludzie = nadawcy SMS-ów, brygada, zawody gracza).
- Bez nazw/znaków konsol i cudzych marek w grze i sklepach („wersja retro”, „Neon nocy”, „Retro LCD”).
- Logika wspólna: zmiana w `GBA/include/core.h`/`meta.h` → ta sama w `GODOT/src/LifeLike.Core` + test złoty
  (`GBA/tests/golden_dump.cpp` → `GODOT/tests/LifeLike.Core.Tests/golden/` + kopie `game.json`, `en.json`).
- Teksty dla gracza tylko w `GBA/data/game.json` (PL, sekcje danych + `ui`/`uiGodot`) i `GBA/data/lang/en.json` (EN);
  `gen_data.py --check` pilnuje fontu, kompletności, szerokości i polskich literałów w kodzie.
- C#: jeden typ na plik, Allman, 4 spacje, bez `Type?`. Otwarty edytor Godota przepisuje `App.cs`/`Main.cs`/
  `SceneNodes.cs` na taby – porównaj `git diff -w --stat` z `git diff --stat`, nie commituj samych tabów.
- Balans (bot, 300 przebiegów/zawód): Łatwy 50–60%, Normalny 25–35%, Trudny 8–15%, pełne Szkolenia 50–60%,
  + Respekt 65–75%, Akt 0 + meta 60–70%, inwestor wszystkie modyfikatory 5–15%, kontrakty kariery 25–40 / 60–75.
  Uwaga: człowiek wygrywa łatwiej niż bot (feedback 2026-10-04) – patrz TODO #73.

## Pułapki techniczne (nauczone na błędach)
- **GBA:** stos ~12 KB – duże obiekty scen na stercie; limit kafli sprite'ów podniesiony do 256 (`USERFLAGS` w Makefile);
  max 128 sprite'ów i 4 warstwy BG; jedna pusta klatka między scenami zwalnia sprite'y; nie rysuj okna dwa razy w klatce.
  Profil w SRAM ma wersje z migracjami (teraz v17, 384 B; zapis budowy od offsetu 512, magic `PBRUN16`) – nowe pola =
  nowa wersja + migracja, nigdy nie łam starych zapisów.
- **Godot/iOS:** eksport wymaga `import_etc2_astc=true` i `godot/LifeLike.Game.sln`; szablony 4.7.2.stable.mono;
  podpis TestFlight lokalnym certyfikatem „Apple Distribution” + profil z `asc_profile.py` (klucz API nie ma cloud signing).
  Apple po akceptacji umowy potrafi jeszcze chwilę zwracać 403.
- **Android:** gradle, target SDK 36, versionCode = MINOR*10000 + PATCH*100 + BUILD (0.21.54 → 215400); klucz uploadu
  jak w aplikacji PlanBudowlany (hasło w `.env.local`).
- **macOS bash 3.2:** pusta tablica przy `set -u` → `${ARR[@]+"${ARR[@]}"}`.
- **CI:** `gba-build` z pełnym balansem trwa ~26 min; `core-tests` ~2 min.

## Narzędzia
| Co | Gdzie |
|---|---|
| iPhone (instalacja przez kabel/Wi-Fi) | `GODOT/tools/ios_deploy.sh` |
| TestFlight | `GODOT/tools/testflight_upload.sh` (+ `asc_profile.py`) |
| Google Play (AAB, ścieżka internal) | `GODOT/tools/android_release.sh --upload` (+ `play_upload.py`) |
| Karty sklepów (opisy PL/EN, zrzuty) | `GODOT/tools/asc_listing.py`, `GODOT/tools/play_listing.py`, materiały w `Promo/store/` |
| Grafiki z GBA do Godota | `GODOT/tools/export_godot_assets.py` |
| ROM na kartę Miyoo | właściciel: `sudo GBA/tools/sd_copy.sh` (Miyoo: Y → Refresh) |
| Social media | `Promo/<wersja>/` (infografiki, tekst posta, skrypt) |
| Testy małpy | GBA `GBA/tools/playtest/monkey.sh`, Godot `--monkey SEED KROKI [--touch --portrait] [--lang en]` |

Wydanie (po „tak” właściciela): CHANGELOG + wersja w `game.json` → GitHub Release z ROM-em → Drive
`DEV/Game/RogueLikeDSP/roms/<data>_vX.gba` + CHANGELOG → TestFlight → Google Play → karty sklepów.
Planowane CI: iOS przez Xcode Cloud, Android przez Bitrise (GitHub Actions nie używamy).

## Otwarte sprawy zewnętrzne
- Strony polityki prywatności i wsparcia na planbudowlany.online (treść: `Promo/store/prywatnosc.md`) – publikuje sesja/zespół PB.
- Formularze Play Console (ocena treści, bezpieczeństwo danych) – przez Chrome z rozszerzeniem Claude.
- Konto PlanBudowlany w grze (logowanie, kosmetyka za subskrypcję) – do uzgodnienia API z zespołem PB (TODO #55–#61).
