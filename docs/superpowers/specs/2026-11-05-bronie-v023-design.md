# v0.23 – bronie: typy ciosu, dwa tryby, rzadkość, pas budowlańca (dokument projektu)

Status: szkic do zatwierdzenia przez właściciela. Źródło propozycji: `docs/v0.23/katalog_broni.pdf`
(generator `docs/v0.23/make_katalog_broni.py`). Decyzje właściciela z 2026-11-05 – sekcja 3.

## 1. Po co

Dziś bronie różnią się głównie liczbami (obrażenia, zasięg, statystyka, czasem żywioł prąd / iskra). Walka z każdym
problemem wygląda tak samo, a znalezienie nowej broni rzadko zmienia sposób gry. Cel v0.23: broń zmienia **jak**
walczysz – na ile pól bijesz, w kogo opłaca się bić czym, kiedy użyć ciosu specjalnego – i jest widoczna (kolor
rzadkości w ręce postaci). To ostatnia wersja open source (repo publiczne); v0.24 przenosi Godota do repo organizacji.

**Sukces:** gracz wybiera broń pod etap i problem (cement – obuch, drewno – cięcie, opancerzony boss – kłucie), trafia
mu się rzadka broń i to widać, a balans botów zostaje w obecnych celach przy broni domyślnej.

## 2. Cele, nie-cele, anty-cele

**Cele**
- 3 typy ciosu: obuch, cięcie, kłucie (+ żywioły prąd, iskra, woda i nowy ogień); skuteczność zależna od materiału problemu (pole `material` w `enemies`:
  stal 15, drewno 13, cement 10, bez materiału 9 – głównie bossowie). Żywioły prąd / iskra / woda i kombinacje stanów
  zostają i dokładają się do typu.
- Każda broń ma 2 tryby: A – zwykły atak, B – inny wzór pól albo efekt, z odnowieniem (jak moc zawodu).
- Wzory pól (10): pojedynczy, łuk, wokół, krzyż, linia 2, linia 3, strzał N, rzut 3x3, natrysk, pchnięcie
  (+ linia w obie strony i linia w 4 strony dla laserów).
- Rzadkość broni: Zwykła, Solidna, Markowa, Mistrzowska; widoczna kolorem w ręce postaci, w dropach, Hurtowni, na karcie.
- Wszystkie 22 obecne bronie dostają typ i tryb B; 17 nowych broni (14 z katalogu + laser krzyżowy, lutownica, opalarka).
- Pas budowlańca: rzadki przedmiot, który daje drugie miejsce na broń; zamiana broni kosztuje turę.
- GBA: pełny system (wspólna logika C++ / C#, test złoty).

**Inne cele v0.23 (osobne części wersji, własne dokumenty – nie w tym dokumencie)**
- Nowe zawody, nowi bossowie, zmiany mocy zawodów (moc zostaje osobno od trybu B broni; nowe zawody dostają broń
  domyślną z tego systemu).
- Easter egg „Zaplecze”.
- Humor (własne postacie, np. sąsiad z działki).

**Nie-cele**
- Konfiguracje chmurowe (Xcode Cloud, CI) i integracje z PlanBudowlany – v0.24, nie w tym repo.

**Anty-cele (porażka, nawet jeśli działa)**
- Broń domyślna zawodu jest „zła” na jakimś etapie i gracz czuje się zmuszony ją wymienić.
- Tryb B silniejszy od mocy zawodu albo używany co turę (bez odnowienia).
- Rzadkość widoczna tylko w tekście (bez koloru w ręce postaci).
- Cudze marki w nazwach (pas „jak u znanego bohatera” – nazwa własna).

## 3. Decyzje właściciela (2026-11-05)

1. Tryb B z **odnowieniem** (jak moc) – dla balansu.
2. **Jedna broń** naraz (zamiana przy znalezieniu jak dziś narzędzie); rzadki przedmiot **pas budowlańca** daje drugą
   broń; **zamiana między nimi kosztuje turę**.
3. Siła różnic (±50% jak w katalogu albo łagodniej ±25%) – **ustalić testami balansu botem**.
4. Rzadkość broni z **dropów** i ze **Szkoleń / Hurtowni**.
5. **Wszystkie** nowe bronie z katalogu; **żywioł ogień** dla broni, które grzeją (lutownica i inne). Poziomica laserowa w trybie B: **linia przód i tył**. Nowa broń **laser
   krzyżowy**: tryb B w **4 strony**.
6. **GBA: pełny system, ale niższy priorytet niż Godot.** Jeśli GBA wpadnie w ograniczenia sprzętowe (sprite'y,
   kafle, stos, SRAM, ROM, wydajność), wykonawca nie tnie systemu po cichu: zatrzymuje się i informuje właściciela,
   na czym dokładnie (co, ile jest, ile trzeba), a Godot idzie dalej.
7. **Stare zapisy i test złoty bez ograniczeń** – w v0.23 stare zapisy (profil GBA w SRAM, zapis budowy) mogą przestać
   działać (wyjątek od zasady z CLAUDE.md „nigdy nie łam starych zapisów”); test złoty regenerowany po zmianach logiki.

## 4. Projekt (jak, w tym co ustalone)

### 4.1 Typy i skuteczność
- Typ broni w danych (`weapons[].hit`: `blunt` | `cut` | `pierce`, opcjonalnie dwa typy dla broni mieszanych, np.
  młot udarowy obuch + kłucie – liczy się korzystniejszy).
- Efekty typu: obuch – szansa na ogłuszenie (cel traci turę; propozycja 20%), ciężkie bronie odrzucają o 1 pole;
  cięcie – +10% kryt; kłucie – ignoruje 2 OBR (jak dzisiejsza cecha narzędzia „Przebicie”) i w linii przebija do
  kolejnego celu.
- Mnożnik wg materiału (tabela startowa z katalogu, do strojenia):

  | | stal | drewno | cement | żywioł woda | bez materiału |
  |---|---|---|---|---|---|
  | Obuch | 75% | 100% | 150% | 50% | 100% |
  | Cięcie | 75% | 150% | 75% | 100% | 100% |
  | Kłucie | 125% | 100% | 100% | 100% | 100% (−2 OBR) |

  Iskra +50% na stal, prąd +50% na żywioł woda (z kombinacją Porażenie jak dziś). **Nowy żywioł ogień** (bronie,
  które grzeją: lutownica, opalarka, palnik): +50% na drewno, −50% na żywioł woda, ale osusza (zdejmuje stan Mokry),
  podpala (nowy stan: obrażenia przez 2 tury; propozycja 2 na turę). Iskra zostaje osobno (pył, stal). Wartości w `game.json` (sekcja
  np. `hitTypes`), nie w kodzie.
- Rozpiska obrażeń (#26, karta wroga, wybór zawodu) pokazuje typ i mnożnik („Cement: obuch ×1,5”).

### 4.2 Wzory pól i dwa tryby
- Wzór pól to dane (`weapons[].modeA` / `modeB`: wzór, zasięg, mnożnik obrażeń, efekt, odnowienie). Bohater patrzy w
  kierunku ostatniego ruchu / celu; wzór obraca się z kierunkiem.
- Tryb A – zwykły atak (dzisiejsze `range` → wzór „pojedynczy” / „strzał N”, żeby obecne liczby zostały jak dziś).
- Tryb B – osobny przycisk (Godot: pasek akcji obok Mocy; klawiatura / pad – nowy przypisany klawisz; GBA – kombinacja
  przycisków, do ustalenia przez wykonawcę), odnowienie w turach w danych (propozycje 3–5 z katalogu).
- Efekty trybu B z katalogu (ogłuszenie, unieruchomienie, odrzut, przyciągnięcie, −OBR na N tur, obrażenia przez N tur,
  znacznik +X% obrażeń, odsłonięcie pokoju, otwarcie pękniętej ściany, zamiana miejscami, przeskok na mokre cele).
  Część to nowe stany – dopisywane do `statuses` jak dziś.
- Lasery: poziomica laserowa B – linia przód i tył; laser krzyżowy B – linie w 4 strony.

### 4.3 Rzadkość
- Poziomy jak przy sprzęcie: Zwykła, Solidna, Markowa + Mistrzowska (unikaty: Młot Zenka, Poziomica mistrza, Pilarka
  tarczowa, broń mistrza zawodu). Propozycja efektów: Solidna +1 obrażeń albo cecha, Markowa +2 i cecha, odnowienie
  trybu B −1, Mistrzowska – własny efekt.
- Źródła: drop na budowie (losowanie jak sprzęt, z premią etapu), Hurtownia (za zł), Szkolenia (za doświadczenie –
  odblokowanie broni i/lub wyższej rzadkości). Proporcje do strojenia.
- Ulepszanie narzędzia (`toolUpgrade`, cechy Przebicie / Ostrze / Wyważenie) zostaje; wykonawca ustala, jak łączy się
  z rzadkością (np. rzadkość = startowy poziom ulepszenia + kolor).

### 4.4 Pas budowlańca
- Rzadki przedmiot w istniejącym slocie sprzętu „Pas” (dziś Pas narzędziowy / z kaburą / markowy – dokładają termos).
  Nazwa własna (propozycja: „Pas monterski z kaburą”). Z nim bohater nosi 2 bronie; zamiana = 1 tura (akcja
  na pasku akcji / klawisz). Źródło: rzadki drop / nagroda (do ustalenia przez wykonawcę w granicach „rzadki”).

### 4.5 Wygląd
- Broń w ręce postaci: mały sprite broni na 4 kierunki (najlepiej widoczny przy patrzeniu w dół), podbarwiony kolorem
  rzadkości (Zwykła – szary, Solidna – niebieski, Markowa – fiolet marki, Mistrzowska – złoty `Pal.EliteGold`),
  Markowa i Mistrzowska z poświatą. Ten sam kolor w dropach, Hurtowni, na karcie zawodu i w telefonie (Sprzęt).
- Grafika: nowe arkusze z eksportera (`GODOT/tools/export_godot_assets.py`) – jedno źródło dla GBA i Godota.
- GBA: limity 128 sprite'ów i 256 kafli sprite'ów – broń w ręce co najmniej u bohatera; u brygady / pomocników tylko
  jeśli mieści się w limitach (decyzja wykonawcy po pomiarze `-DPB_DEBUG_STATS`).

### 4.6 Nowe bronie (17)
Łopata, Siekiera ciesielska, Piła ręczna, Kilof, Przecinak z młotkiem (3 ładunki, otwiera pęknięte ściany), Nożyce do
blachy, Pistolet do silikonu, Myjka ciśnieniowa (moczy – kombinacja z prądem), Palnik dekarski, Taker, Młotek gumowy,
Pilarka tarczowa (Mistrzowska / sekret), Poziomica 2 m, Mieszadło do zaprawy, Laser krzyżowy, Lutownica (ogień,
kłucie), Opalarka (ogień, natrysk). Palnik dekarski – ogień zamiast iskry. Tryby i typy – katalog
(sekcja 6 PDF). Nazwy i opisy PL/EN w danych.

## 4.7 Decyzje projektanta (do sprawdzenia przez właściciela)

1. **Materiał + żywioł woda na jednym problemie** (13 problemów, np. Przeciek = cement + woda): **żywioł woda ma
   pierwszeństwo** – liczy się kolumna „żywioł woda”, nie materiał (problem wodny to najpierw woda). Bez mnożenia
   dwóch kolumn.
2. **Dla jakiego bota są cele balansu:** cele z sekcji 5 dotyczą bota, który gra jak gracz – podnosi i wymienia bronie,
   używa trybu B i pasa. Dodatkowo sprawdzamy bota **tylko z bronią domyślną** (bez wymiany): jego wygrane na Normalnym
   nie niżej niż dziś −5 punktów procentowych dla żadnego zawodu.
3. **Test „broń domyślna nie jest zła na etapie”:** pomiar botem z bronią domyślną – dla każdego zawodu i etapu udział
   porażek na tym etapie nie rośnie o więcej niż 10 punktów procentowych względem dziś. Jeśli rośnie, najpierw
   łagodzimy mnożniki (np. ±25%), potem dajemy broni domyślnej drugi typ. Bez sztucznego „neutralnego” wyjątku.
4. **Rzadkość:** Zwykła / Solidna / Markowa losowana przy każdym dropie i zakupie w Hurtowni; **Mistrzowska stała**
   dla unikatów (Młot Zenka, Poziomica mistrza, Pilarka tarczowa) i 12 broni mistrza zawodu – złoty kolor, efekty jak
   dziś (perk mistrzostwa), bez 12 nowych efektów. Drop i Hurtownia nigdy nie dają Mistrzowskiej. Zapis budowy:
   bajt rzadkości na broń.
5. **Szkolenia a rzadkość:** Szkolenia są stałe (jak dziś) – nowe Szkolenie podnosi rzadkość startową broni domyślnej
   (Zwykła → Solidna → Markowa) i odblokowuje bronie do dropów / Hurtowni. Wymaga nowej wersji profilu GBA (szersza
   maska broni + poziom Szkolenia), bez migracji starych zapisów (decyzja 7). Hurtownia: zakup tylko na bieżącą budowę.
6. **Pas budowlańca** zachowuje premię do termosu jak inne pasy (rzadka odmiana tego slotu).

## 5. Ograniczenia

- Wspólna logika: zmiany w `GBA/include/core.h` / `meta.h` i 1:1 w `GODOT/src/LifeLike.Core` (+ kopie `game.json`,
  `en.json` w `golden/`). Kolejność: najpierw Godot (rdzeń C#, UI, grafika), potem port na GBA; ograniczenia sprzętowe
  GBA – zgłosić właścicielowi z liczbami (decyzja 6), nie obchodzić po cichu.
- Zapisy: profil GBA w SRAM (dziś v17, 384 B; maska narzędzi `Tools` to 1 bajt – za mało na 37 broni) → nowa wersja
  profilu; migracja starych zapisów **nie jest wymagana** (decyzja 7) – nowa wersja może zacząć od czystego profilu.
  Zapis budowy (`PBRUN…`) – nowa wersja magic (rzadkość broni, druga broń, odnowienie trybu B), stary zapis budowy
  może zostać odrzucony.
- Test złoty (`golden_dump.cpp` → `golden/`) regenerowany po zmianach; C++ i C# muszą dalej dawać ten sam wynik.
- Balans (bot, 300 przebiegów / zawód): Łatwy 50–60%, Normalny 25–35%, Trudny 8–15%, pełne Szkolenia 50–60%,
  + Respekt 65–75%, Akt 0 + meta 60–70%, inwestor 5–15%, kontrakty 25–40 / 60–75. Bot musi umieć tryb B i zamianę
  broni (inaczej cele balansu nie znaczą nic).
- Teksty tylko w `game.json` / `en.json` (`gen_data.py --check`), C# jeden typ na plik, przeciwnicy to problemy budowy,
  bez cudzych marek, bez konfiguracji chmurowych w tym repo.
- GBA: stos ~12 KB, sprite'y / kafle jak wyżej, 4 warstwy BG.

## 6. Zostawione wykonawcy

- Dokładne liczby (mnożniki, szanse, odnowienia, ceny, proporcje rzadkości) – start z katalogu, strojenie botem.
- Przycisk trybu B na GBA i klawisz na desktopie / padzie.
- Format danych (`hit`, `modeA`, `modeB`) i kolejność implementacji (najpierw rdzeń + bot + test złoty, potem UI,
  grafika, GBA).
- Jak rzadkość łączy się z istniejącym ulepszaniem narzędzia.
- Źródło pasa budowlańca (drop / nagroda / sekret) w granicach „rzadki”.
- Kolejność wprowadzania 15 nowych broni do dropów / Hurtowni / Szkoleń.
