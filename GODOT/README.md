# PlanBudowlany RogueLike – Godot 4 + C#

Wersja Godot gry z demo GBA (`../GBA`): roguelike budowlany, w którym etapy budowy są piętrami lochu,
a wrogami są *problemy budowy*. Kierunek rozwoju: [`docs/KONCEPCJA.md`](docs/KONCEPCJA.md)
(telefon z aplikacją PlanBudowlany jako interfejs, oprawa 2.5D – kolejne kamienie milowe).

**Stan: zgodny z GBA v0.21.42** (logika, dane i test złoty z migawki GBA v0.21.42).

## Co jest

- **`src/LifeLike.Core`** – cała logika gry przeniesiona 1:1 z `GBA/include/core.h` i `meta.h`
  (bez zależności od Godota): generator map xorshift32 (pokoje + korytarze L), pole widzenia (shadowcasting),
  walka i AI problemów, moce zawodów z rangami, stany z danych (`statuses`: zatrucie, porażenie, poślizg,
  papierologia – komunikat ze skutkiem i czasem), szczęście zawodów (kryt x2, unik, częstsze i lepsze dropy – sekcja
  `luck`), sprzęt z cechami (`equipment.traits`) i porównaniem przy paczce (zakładam / zostawiam za `declineXp`),
  termos (`thermos`: kawa trafia do termosu, picie zużywa turę),
  narzędzia, dropy, akty z bossami i zapowiadanym uderzeniem, Hurtownia, poziomy postaci, NG+, dziennik
  (rodzaje, powtórzenia, bufor 48 bajtów UTF-8 jak na GBA), celowanie, profil gracza (Szkolenia, odblokowania,
  odznaki, Katalog usterek, Osiedle, bankowanie doświadczenia, migracje zapisu v1/v2), zapis budowy w trakcie
  (format `PBRUN02`).
  **Ten sam seed daje identyczną grę co na GBA** (ta sama kolejność wywołań RNG, arytmetyka całkowita,
  rzutowania int8/int16).
- **`src/LifeLike.Core/Bot.cs`** – deterministyczny bot z testów GBA (testy balansu, test złoty, test dymny);
  bierze lepszą paczkę sprzętu (gorszą zostawia) i pije z termosu poniżej `thermos.botDrinkBelowPct` HP.
- **`tests/LifeLike.Core.Tests`** – testy xUnit przeniesione z `GBA/tests/core_tests.cpp` (łącznie z balansem
  na botach, z mniejszą liczbą przebiegów) + test złoty.
- **`godot/`** – grywalna wersja 2D: kolorowe prostokąty i litery, mgła wojny przyciemnieniem, HUD tekstowy
  (szczęście, kryt %, unik %, wzrok, termos, stany z turami i skutkiem, sprzęt z cechami), liczby nad polami
  (obrażenia, żółte „KRYT! -N”, zielone „Unik!”), menu akcji pod Enter/START (ikony wokół bohatera: ↑ Atak,
  → Moc, ↓ Termos, ← Czekaj), okno porównania sprzętu przy paczce, ekrany tekstowe (tytuł z wyborem zawodu
  i poziomu, Szkolenia, harmonogram po etapie, Hurtownia po akcie, podgląd pod Tab, koniec gry z NG+). Profil zapisuje się w `user://profile.sav` (układ bajtów jak SRAM na GBA).

## Sterowanie (jak na GBA)

| Akcja | Klawiatura | Pad | Mysz |
|---|---|---|---|
| Ruch / atak wroga na drodze | strzałki, WSAD, numpad | D-pad, lewa gałka | lewy klik obok = krok |
| A: atak najbliższego celu w zasięgu | Spacja, X, J | A | lewy klik na wroga w zasięgu |
| B: czekaj turę | Z, Kp5, `.` | B | prawy klik |
| R: moc zawodu | R, F | RB | |
| Podgląd tekstowy | Tab | Select | |
| Menu akcji (strzałka wybiera, ta sama strzałka / A wykonuje, Enter/B zamyka) | Enter | Start | |
| Paczka sprzętu: A zakładam / B zostawiam | Spacja / Z | A / B | |
| Dalej / start (ekrany) | Enter | Start | |
| Szkolenia (na ekranie tytułowym) | K | Y | |

## Wymagania i uruchomienie

- Godot 4.7 w wersji .NET (np. `brew install --cask godot-mono`), .NET 8 SDK (lub nowszy z runtime 8).
- W Godot: *Import* → `GODOT/godot/project.godot`, uruchom scenę `scenes/Main.tscn`.
- Z terminala: `godot-mono --path GODOT/godot` (opcjonalnie `-- --seed 1234`).
- Test dymny bez okna (bot gra 3 etapy przez warstwę Godota, kod 0 = OK):
  `godot-mono --headless --path GODOT/godot -- --smoke`
- Zrzut ekranu po kilku turach bota: `godot-mono --path GODOT/godot -- --screenshot /tmp/shot.png`
  (opcjonalnie `--scene title|game|combat|offer|menu|overview` – ekran tytułowy, gra, liczby KRYT!/Unik!,
  porównanie sprzętu, menu akcji z termosem i stanami, podgląd; sceny pokazowe ustawiają stan ręcznie)

Przy pierwszym uruchomieniu z terminala najpierw `dotnet build GODOT/godot/LifeLike.Game.csproj`
i `godot-mono --headless --path GODOT/godot --import`.

## Skąd dane

Jedno źródło prawdy: **`GBA/data/game.json`** (ten sam plik, z którego GBA generuje `include/game_data.h`
skryptem `tools/gen_data.py`). `GameData` interpretuje go tak samo jak `gen_data.py` (identyfikatory → indeksy,
maski startowych zawodów/narzędzi i poziomów), a nieznane pola ignoruje.

- **Gra w Godot:** target MSBuild `CopySharedGameData` w `godot/LifeLike.Game.csproj` kopiuje przy każdym buildzie
  `GBA/data/game.json` do `godot/data/game.json` (plik w `.gitignore`), a gra czyta `res://data/game.json`.
  Przy eksporcie dodaj filtr zasobów `data/*.json`.
- **Testy:** używają zamrożonej kopii `tests/LifeLike.Core.Tests/golden/game.json` (z migawki GBA, z której zrobiono
  złote przebiegi), więc nie zmieniają się razem z bieżącymi pracami nad GBA. Osobny test sprawdza tylko,
  czy aktualny `GBA/data/game.json` da się wczytać.

## Test złoty (zgodność z GBA)

`GBA/tests/golden_dump.cpp` kompiluje się z nagłówkami GBA i rozgrywa 15 budów deterministycznym botem
(wszystkie zawody i poziomy, bot z `core_tests.cpp` oraz wariant z mocą i celowaniem, Hurtownia, pełne Szkolenia, NG+;
oba boty piją z termosu i decydują o paczkach sprzętu – bot z testów bierze lepszą, wariant „smart” także tej samej
jakości, czyli wymienia cechę). Zapisuje `golden/run_XX.json`: skrót FNV stanu po każdym kroku (`StateDigest`, łącznie
z trafieniami tury – kryt/unik – cechami sprzętu, termosem i paczką czekającą na decyzję; po kroku lista trafień
jest zerowana jak w warstwie GBA), pełne zrzuty na starcie każdego etapu i na końcu (mapa, mgła, wrogowie, znajdźki
z cechami, dziennik bajt po bajcie, …) oraz profil po budowie.
`GoldenTests` odtwarza to samo w C# i porównuje pole po polu.

Odtworzenie plików (z tej samej wersji nagłówków GBA co `golden/game.json`; obecnie migawka v0.21.42,
np. `git show 4163dfd2:GBA/...` rozpakowane do osobnego katalogu `<gba_v42>`):

```bash
g++ -std=c++20 -O2 -I<gba_v42>/include GBA/tests/golden_dump.cpp -o /tmp/golden_dump
/tmp/golden_dump GODOT/tests/LifeLike.Core.Tests/golden
cp <gba_v42>/data/game.json GODOT/tests/LifeLike.Core.Tests/golden/game.json
```

Parser danych ignoruje nieznane pola, a nieznane wartości efektów Szkoleń i cech sprzętu (z nowszych wersji
`GBA/data/game.json`) wczytuje jako `Unknown` – bez działania, dopóki port ich nie obsłuży.

## Testy

```bash
dotnet test GODOT/LifeLike.sln
```

## Struktura

```
src/LifeLike.Core/          logika gry (Game*.cs, Level, Rng, Meta, Profile, RunSave, Bot, Data/GameData)
tests/LifeLike.Core.Tests/  testy xUnit + golden/ (dane z migawki GBA i złote przebiegi)
godot/                      projekt Godota (Main.cs – ekrany i wejście, WorldView.cs – render, Hud.cs, GameInput.cs)
godot/data/                 kopia GBA/data/game.json robiona przy buildzie (poza gitem)
```
