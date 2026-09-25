# RogueLikeDSP – PlanBudowlany RogueLike (dawniej LifeLike)

Roguelike o budowie domu: przechodzisz etapy budowy (Fundamenty → … → Wykończenie i odbiór), a przeciwnikami są
problemy budowy (Przeciek, Zwarcie, Pleśń, Papierologia…) i bossowie aktów (Zepsuta Betoniarka, Nawałnica,
Nieprzekraczalny Termin). Menu gry to smartfon bohatera z aplikacją [PlanBudowlany](https://planbudowlany.online).

Projekt zaczął się w 2017 roku jako LifeLike (Unity, konkurs DSP2017), a w 2026 wrócił w dwóch nowych wersjach.

## Projekty w repozytorium

| Folder | Co to jest | Stan |
|---|---|---|
| [`GBA/`](GBA/README.md) | PlanBudowlany RogueLike na konsole GBA i emulatory (Butano, C++20) | grywalne, wydania ROM co wersję |
| [`GODOT/`](GODOT/README.md) | ta sama gra w Godot 4.7 + C#: logika 1:1 z GBA, bogatsza oprawa, telefon jako interfejs, wersja mobilna (iOS/Android) | w rozwoju |
| `Assets/`, `ProjectSettings/`, … | oryginalny LifeLike w Unity 2017 (archiwum) | nierozwijany |

Wspólne dane gry (zawody, przedmioty, wrogowie, etapy, odznaki, teksty) są w jednym pliku
[`GBA/data/game.json`](GBA/data/game.json) – GBA generuje z niego nagłówek C++, Godot czyta go bezpośrednio.

## Pobierz i zagraj

- **GBA:** ROM-y z changelogiem w [Releases](https://github.com/aluspl/RogueLikeDSP/releases) (np. mGBA, Miyoo;
  na Miyoo po wgraniu nowego ROM-u: lista gier → Y → Refresh). Zmiany: [`GBA/CHANGELOG.md`](GBA/CHANGELOG.md).
- **Godot (desktop):** `godot-mono --path GODOT/godot` (wymaga Godot 4.7 .NET i .NET SDK).
- **Godot (iPhone):** `GODOT/tools/ios_deploy.sh` – buduje, podpisuje i instaluje na podłączonym iPhonie
  (szczegóły w [`GODOT/README.md`](GODOT/README.md)).

## Budowanie i testy

| Co | Komenda |
|---|---|
| Dane GBA (walidacja fontu) | `python3 GBA/tools/gen_data.py --check` |
| ROM GBA | Docker `devkitpro/devkitarm:20241104`, patrz [`GBA/README.md`](GBA/README.md) |
| Testy rdzenia GBA (PC, boty balansu) | `GBA/tests/` |
| Test w emulatorze (zrzuty ekranu) | `GBA/tools/playtest/run.sh skrypt.txt katalog --fresh` |
| Testy rdzenia C# (w tym test złoty C++ ↔ C#) | `dotnet test GODOT/LifeLike.sln` |
| Test dymny Godota | `godot-mono --headless --path GODOT/godot -- --smoke` |
| Zrzuty ekranów Godota | `godot-mono --path GODOT/godot -- --screenshot out.png --scene title` |

CI (GitHub Actions): `gba-build` (ROM) i `core-tests` (testy C# + build projektu Godot).

## Zasady projektu

- Przeciwnicy to problemy budowy – nigdy zawody, ludzie ani urzędnicy.
- Bez znaków towarowych konsol w brandingu.
- Logika gry jest wspólna: zmiana w `GBA/include/core.h` / `meta.h` → ta sama zmiana w `GODOT/src/LifeLike.Core`
  i odświeżony test złoty.
- Wydania: wersja `v0.21.N` w `game.json`, sekcja w `GBA/CHANGELOG.md`, GitHub Release z ROM-em.

Plan dalszych prac: [`TODO.md`](TODO.md).

---

## Historia: LifeLike (Unity 2017)

Opis projektu na [Szymon Motyka](http://szymonmotyka.pl) i [DSP2017](http://szymonmotyka.pl/tag/dsp).
Kontakt: [Facebook](https://facebook.com/szymonmotykapl), twitter @AlusPL.

Wersje WebGL:
1. [Version 1](https://aluspl.github.io/RogueLikeDSP/Versions/)
2. [Version 2 – światła](https://aluspl.github.io/RogueLikeDSP/Versions/light)
3. [Version 3 – GUI, kreator postaci, przeciwnicy](https://aluspl.github.io/RogueLikeDSP/Versions/GUIAndCharacterCreator)
4. [Version 4 – walka](https://aluspl.github.io/RogueLikeDSP/Versions/fight)
5. [Version 5 – przeciwnicy nie stoją w miejscu](https://aluspl.github.io/RogueLikeDSP/Versions/version5contrattack)
6. [Version 6 – śmierć](https://aluspl.github.io/RogueLikeDSP/Versions/version6)
7. [Version 7 – ataki specjalne](https://aluspl.github.io/RogueLikeDSP/Versions/version7)
8. [Version 8 – ekwipunek](https://aluspl.github.io/RogueLikeDSP/Versions/version8)

Sterowanie (Unity): Tab – przełączanie przeciwników, F – światło, Spacja – atak, I – okno postaci, O – ekwipunek.
