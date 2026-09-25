# PlanBudowlany RogueLike – Godot 4 + C#

Wersja Godot gry z demo GBA (`../GBA`): roguelike budowlany, w którym etapy budowy są piętrami lochu,
a wrogami są *problemy budowy*. Kierunek rozwoju: [`docs/KONCEPCJA.md`](docs/KONCEPCJA.md)
(telefon z aplikacją PlanBudowlany jako interfejs, oprawa 2.5D – kolejne kamienie milowe).

**Stan: zgodny z GBA v0.21.43** (logika, dane i test złoty z migawki GBA v0.21.43).

## Co jest

- **`src/LifeLike.Core`** – cała logika gry przeniesiona 1:1 z `GBA/include/core.h` i `meta.h`
  (bez zależności od Godota): generator map xorshift32 (pokoje + korytarze L), pole widzenia (shadowcasting),
  walka i AI problemów, moce zawodów z rangami, stany z danych (`statuses`: zatrucie, porażenie, poślizg,
  papierologia – komunikat ze skutkiem i czasem), szczęście zawodów (kryt x2, unik, częstsze i lepsze dropy – sekcja
  `luck`), sprzęt z cechami (`equipment.traits`) i porównaniem przy paczce (zakładam / zostawiam za `declineXp`),
  termos (`thermos`: kawa trafia do termosu, picie zużywa turę),
  narzędzia, dropy, akty z bossami i zapowiadanym uderzeniem, Hurtownia, poziomy postaci, NG+, dziennik
  (rodzaje, powtórzenia, bufor 48 bajtów UTF-8 jak na GBA), celowanie, profil gracza (Szkolenia, odblokowania,
  odznaki, Katalog usterek, Osiedle, bankowanie doświadczenia, migracje zapisu v1/v2/v3), zapis budowy w trakcie
  (format `PBRUN03`).
  Od v0.21.43: statystyki efektywne (zawód + Warsztaty na statystykę broni zawodu + cechy sprzętu SIŁ/ZRĘ/INT),
  Szkolenia Kurs BHP II i Warsztaty, **uprawnienia** (każda odznaka daje trwałą premię na budowę, `badges[].perk`),
  **pamiątki** (`keepsakes`: wybór na starcie, rangi I–III po 3 i 8 budowach, odblokowanie od startu / odznaką / zleceniem),
  **zlecenia** (`contracts`: liczniki w profilu – usunięte problemy, moce, markowy sprzęt, czyści bossowie, wygrane – nagroda
  w doświadczeniu i pamiątce, postęp na żywo w trakcie budowy), **wydarzenia na placu** (`siteEvents`: SMS na starcie etapu
  bez bossa, 45% – mniej znajdziek, premia, inspekcja, ulewa z poślizgiem, pełny termos). Profil v4 (`PBRL004`, 72 bajty,
  układ jak SRAM na GBA; migracja v3→v4 zachowuje stare pola, nowe zeruje).
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
  i poziomu, statystykami z premią „SIŁ 5+2”, wyborem pamiątki i listą uprawnień, Szkolenia, profil z odznakami, zleceniami
  i pamiątkami, harmonogram po etapie, karta etapu z SMS-em wydarzenia na placu, Hurtownia po akcie, podgląd pod Tab,
  koniec gry z NG+; HUD pokazuje wydarzenie na placu i postęp najbliższego zlecenia). Profil zapisuje się w `user://profile.sav` (układ bajtów jak SRAM na GBA).

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
| Pamiątka na budowę (na ekranie tytułowym, jak L/R na GBA) | Q / E | LB / RB | |
| Profil: odznaki z uprawnieniami, zlecenia, pamiątki (na ekranie tytułowym) | P | X | |

## Wymagania i uruchomienie

- Godot 4.7 w wersji .NET (np. `brew install --cask godot-mono`), .NET 8 SDK (lub nowszy z runtime 8).
- W Godot: *Import* → `GODOT/godot/project.godot`, uruchom scenę `scenes/Main.tscn`.
- Z terminala: `godot-mono --path GODOT/godot` (opcjonalnie `-- --seed 1234`).
- Test dymny bez okna (bot gra 3 etapy przez warstwę Godota, kod 0 = OK):
  `godot-mono --headless --path GODOT/godot -- --smoke`
- Zrzut ekranu po kilku turach bota: `godot-mono --path GODOT/godot -- --screenshot /tmp/shot.png`
  (opcjonalnie `--scene title|game|combat|offer|menu|overview` – ekran tytułowy, gra, liczby KRYT!/Unik!,
  porównanie sprzętu, menu akcji z termosem i stanami, podgląd; `profile|card|perks` – profil z uprawnieniami, zleceniami
  i pamiątkami, karta etapu z wydarzeniem na placu, HUD z premiami „SIŁ 5+3”, wydarzeniem i zleceniem;
  sceny pokazowe ustawiają stan ręcznie)

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

`GBA/tests/golden_dump.cpp` kompiluje się z nagłówkami GBA i rozgrywa 20 budów deterministycznym botem
(wszystkie zawody i poziomy, bot z `core_tests.cpp` oraz wariant z mocą i celowaniem, Hurtownia, pełne Szkolenia, NG+,
profile z odznakami – uprawnienia – i wybraną pamiątką w rangach I–III, zleceniami; wydarzenia na placu wypadają same;
oba boty piją z termosu i decydują o paczkach sprzętu – bot z testów bierze lepszą, wariant „smart” także tej samej
jakości, czyli wymienia cechę). Zapisuje `golden/run_XX.json`: skrót FNV stanu po każdym kroku (`StateDigest`, łącznie
z trafieniami tury – kryt/unik – cechami sprzętu, termosem i paczką czekającą na decyzję; po kroku lista trafień
jest zerowana jak w warstwie GBA), pełne zrzuty na starcie każdego etapu i na końcu (mapa, mgła, wrogowie, znajdźki
z cechami, dziennik bajt po bajcie, wydarzenie na placu, liczniki zleceń, statystyki efektywne, …) oraz profil po budowie
(pola i cały zapis bajt po bajcie jak w SRAM).
`GoldenTests` odtwarza to samo w C# i porównuje pole po polu.

Odtworzenie plików (z tej samej wersji nagłówków GBA co `golden/game.json`; obecnie migawka v0.21.43,
np. `git show d02ba811:GBA/...` rozpakowane do osobnego katalogu `<gba_v43>`):

```bash
g++ -std=c++20 -O2 -I<gba_v43>/include GBA/tests/golden_dump.cpp -o /tmp/golden_dump
/tmp/golden_dump GODOT/tests/LifeLike.Core.Tests/golden
cp <gba_v43>/data/game.json GODOT/tests/LifeLike.Core.Tests/golden/game.json
```

Parser danych ignoruje nieznane pola, a nieznane wartości efektów Szkoleń i cech sprzętu (z nowszych wersji
`GBA/data/game.json`) wczytuje jako `Unknown` – bez działania, dopóki port ich nie obsłuży (tak samo nieznane
uprawnienia/pamiątki; dane bez sekcji `contracts`/`keepsakes`/`siteEvents` wczytują się z pustymi listami).

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
