# v0.23.2 – Plecak (projekt)

Decyzje właściciela (2026-11-09…12): plecak w publicznym 0.23.x (Godot + GBA), ok. 6 miejsc, bronie i sprzęt z mapy,
po bossie sprzedaż w Hurtowni za materiały (więcej za rzadsze) albo oddanie na PSZOK (za darmo, mały bonus),
nowa wersja zapisu budowy z migracją, rebalans; łatwa zamiana akcesoriów. v0.23.1 (termosy, drzwi magazynu) nie była
wydana osobno – wchodzi do v0.23.2.

Wzorce z innych roguelike'ów: mały, twardo ograniczony plecak (Brogue, Shiren, Dead Cells – mało miejsc = decyzje),
zamiana z plecaka kosztuje turę (jak kabura), porównanie z tym, co noszę, bez wchodzenia w menu (strzałki +/−),
przedmioty z plecaka mają wartość po etapie (sklep/skup), wyrzucenie kładzie przedmiot na ziemi.

## 1. Co trafia do plecaka
- **6 miejsc.** Każde: broń (narzędzie, rzadkość, profity, artefakt, ulepszenia z Hurtowni – poziom/cecha zostają przy
  broni jak w kaburze) albo sprzęt (slot, rzadkość, cecha). Kawa, klucze, materiały – bez zmian (własne liczniki).
- Plecak jest na jedną budowę (jak sprzęt); profil bez zmian.

## 2. Zbieranie z mapy
- **Paczka sprzętu przy zajętym slocie** – karta ma trzy wybory: **Załóż** (stary przedmiot idzie do plecaka; przy
  pełnym plecaku zostaje na ziemi w tym polu), **Do plecaka**, **Zostaw** (jak dziś: +XP, przedmiot zostaje na ziemi).
- **Broń** (gdy dziś pojawia się pytanie o zamianę albo ręka jest zajęta) – to samo: **Weź** (stara broń do plecaka /
  na ziemię), **Do plecaka**, **Zostaw**. Pusta kabura nadal łapie broń automatycznie.
- Pełny plecak: „Do plecaka” wyszarzone z dopiskiem „Plecak pełny (6/6)”.
- Karta pokazuje porównanie z noszonym (statystyka +/−, rzadkość, cecha) – podstawa pod kartę broni z v0.24.

## 3. Szybka zamiana akcesoriów
- **Ekran Plecak:** Godot – nowy przycisk w telefonie + skrót (klawisz I, w pionie dotknięcie paska sprzętu w HUD);
  GBA – aplikacja Plecak w telefonie.
- Lista 6 miejsc; przy każdym strzałka **▲ lepsze / ▼ słabsze / =** względem noszonego w tym slocie (dla broni: średni
  cios). A / dotknięcie: **Załóż** – zamiana miejscami z noszonym (1 tura, jak kabura); drugi wybór: **Wyrzuć** (kładzie
  na ziemi pod bohaterem, jeśli pole wolne, inaczej najbliższe wolne). Broń z plecaka można też włożyć do kabury.
- Zmiana pasa z kaburą działa jak dziś (broń z kabury → plecak, a gdy pełny – na ziemię).

## 4. Po bossie: Hurtownia (między aktami)
- **Skup** – nowa zakładka Hurtowni: sprzedaż przedmiotu z plecaka za materiały. Punkt wyjścia (do balansu):
  Zwykła 1, Solidna 2, Markowa 3, Mistrzowska/artefakt 5 sztuk; broń → stal, sprzęt → drewno, co druga sztuka cement.
- **PSZOK** – oddanie przedmiotu za darmo, mały bonus: +XP (3 + 2 × rzadkość) i +2 HP. Krótki humorystyczny tekst
  („Segregacja zaliczona.”).
- W Hurtowni dostępny też ekran Plecak (zamiana przed kolejnym aktem bez kosztu tury).

## 5. Zapis i rdzeń
- Logika wspólna: `GBA/include/core.h` / `meta.h` ↔ `GODOT/src/LifeLike.Core` + test złoty.
- Zapis budowy: nowa wersja (C# RunSave i GBA `PBRUN20`) z migracją – zapis PBRUN19 wczytuje się z pustym plecakiem.
  Sprawdzić miejsce w SRAM (raport bajtów).
- Pierwszy przedmiot w plecaku: dymek Marka (nowa flaga samouczka 4096), jedna linijka.

## 6. Bot i balans
- Bot: zakłada lepsze, chowa do plecaka porównywalne, w Skupie sprzedaje wszystko oprócz najlepszej zapasowej broni.
- Pomiar 300 budów/zawód, cele z CLAUDE.md (bez zmian HP przeciwników). Plecak i Skup dają więcej materiałów –
  w razie potrzeby obniżyć ceny Skupu, nie cele.

## 7. Poza zakresem (v0.24 w repo organizacji)
- Karta broni z atakami Cios/Patent i oknem porównania ataków, schody po bossie areny.
