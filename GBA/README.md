# PlanBudowlany RogueLike (GBA)

Marketingowe demo roguelike na Game Boy Advance / emulatory (Miyoo, mGBA). Zbudowane na rdzeniu logiki
przeniesionym z LifeLike (folder `../GODOT`): generator map z seedem, tury, walka, AI wrogów.

Zbuduj dom w 8 etapach: **Fundamenty → Mury parteru → Strop → Dach → Okna i drzwi → Instalacje → Tynki i wylewki → Wykończenie i odbiór**.
Pokonaj „problemy budowy” (Przeciek, Zwarcie, Pleśń, Kornik, Papierologia, Opóźniona dostawa, Ulewa,
Przekroczony budżet) i bossa **Nieprzekraczalny Termin**. Na końcu ekran z kodem QR do planbudowlany.online.

![ekran tytułowy](docs/preview_title.png) ![ekran końcowy z QR](docs/preview_end.png)

## Sterowanie
| Klawisz | Akcja |
|---|---|
| D-pad | ruch / atak przez wejście na wroga (przytrzymaj = szybki ruch) |
| A | krótko: atak w najbliższy cel; przytrzymaj: podgląd zasięgu i celownik (strzałki zmieniają cel), puść: atak |
| B | krótko: czekaj turę (co 4 tury +1 HP); przytrzymaj: podgląd widocznych wrogów (nazwa, HP, obrażenia, opis; strzałki zmieniają wroga) |
| START | w grze: menu akcji wokół bohatera – góra Atak, prawo Moc, dół Termos, lewo Czekaj (strzałka wybiera, A albo ta sama strzałka wykonuje, START/B zamyka); poza grą: dalej |
| SELECT | w grze: telefon z aplikacją PlanBudowlany (Zadania, Usterki, Start, Sprzęt, Koszty; L/R – zakładki, START – menu: jak grać, zapisz i wyjdź, porzuć budowę); na tytule: telefon profilu (Odznaki/Zlecenia/Pamiątki – przełączane A, Katalog, Osiedle, Zespół, Koszty = Szkolenia) |
| L/R (wybór zawodu) | pamiątka zabierana na budowę (albo „bez pamiątki”) |
| B (tytuł) | ekran „Jak grać” |
| L (przytrzymaj) | podgląd odkrytej mapy etapu |
| R | moc zawodu (Odprawa, Ścianka, Seria, Łańcuch, Zawór, Wirówka); ikona w prawym górnym rogu: szara z odliczaniem = ładuje się, pulsuje z „R” = gotowa; ranga II od 3. i III od 5. poziomu postaci |
| góra/dół (wybór zawodu) | poziom trudności: Łatwy / Normalny / Trudny |
| L+R+SELECT | skrót pokazowy: zalicz etap (do testów i prezentacji na stoisku) |

## Zawody (dane w `data/game.json`)
Kierownik budowy (Dziennik budowy, zasięg 2) · Murarz (Kielnia) · Cieśla-dekarz (Gwoździarka, zasięg 3) ·
Elektryk (Próbnik napięcia, zasięg 2) · Hydraulik (Klucz nastawny) · Glazurnik (Szlifierka).
Znajdźki: kawa (trafia do termosu – 3 miejsca, pije się z menu pod START za turę; przy pełnym termosie pije od razu), kask (+obrona), projekt wykonawczy (+obrażenia). Ikona termosu z liczbą kaw jest w HUD.
Sprzęt: z wrogów wypadają paczki (zwykły / solidny / markowy) – kask (+obrona), rękawice (+obrażenia), kamizelka (+max HP). Każdy przedmiot ma losową cechę (Szczęście +1, Kryt +5%, Odporność na zatrucie, Widzenie +1, Odnowienie mocy -1, Siła/Zręczność/Inteligencja +1; `equipment.traits`). Cechy SIŁ/ZRĘ/INT podnoszą statystykę, z którą skaluje się broń (obrażenia = rzut broni + statystyka/2 + premie). Do pustego slotu zakłada się sam; przy zajętym okno porównania (obecny vs nowy): A – zakładam, B – zostawiam (doświadczenie). W telefonie zakładka Sprzęt z cechami.
**Szczęście** (`luck` zawodu + cechy): kryt 5% + 3%/pkt (obrażenia x2, żółte „KRYT!”), unik przed ciosem wroga 2%/pkt (maks. 20%), częstsze i lepsze dropy – parametry w sekcji `luck` pliku `data/game.json`. Glazurnik ma najwięcej szczęścia, Murarz wcale.
Dropy: z pokonanych wrogów może wypaść kawa, kask, projekt albo skrzynka z narzędziem (Łom, Wkrętarka, Poziomica laserowa, Młot wyburzeniowy, Tablet z projektem, Miernik laserowy – zastępuje broń zawodu; kolejne narzędzia odblokowujesz w Szkoleniach; trzy ostatnie skalują się z INT).
Statystyki efektywne (baza zawodu + premie ze Szkoleń, pamiątki i sprzętu, np. „SIŁ 5+2”) są na wyborze zawodu i w telefonie (zakładka Start).
Powiadomienia push jak w aplikacji: awans, nowe narzędzie, drop, moc gotowa, zaliczony etap, pojawienie się Terminu.
Mgła wojny: widzisz na 7 pól (ściany zasłaniają), odkryte pola zostają przyciemnione, wrogowie poza polem widzenia są ukryci.

## Akty i bossowie
Etapy są pogrupowane w akty (`data/game.json`: `acts`, pole `act` etapu); każdy akt kończy się bossem:
Akt I Stan surowy – **Zepsuta Betoniarka**, Akt II Pod dachem – **Nawałnica**, Akt III Wykończenie – **Nieprzekraczalny Termin**.
Bossowie co kilka tur zapowiadają uderzenie – czerwone pola wokół bohatera; masz 2 tury, żeby z nich zejść.
Usunięte problemy dają budżet (zł), koniec aktu – premię; między aktami **Hurtownia** (kawa, paczka sprzętu,
narzędzie, siłownia, energetyk – oferta w `hurtownia`).

## Stany
Problemy budowy przy trafieniu mogą nałożyć stan (konfiguracja `onHit` w `data/game.json`): zatrucie (Pleśń: -1 HP
przez kilka tur, nie zabija), porażenie (Zwarcie: tracisz turę), poślizg (Ulewa, Przeciek, Nawałnica: ruch o 2 pola),
papierologia (Papierologia, Termin: moc odnawia się 3 tury dłużej). Ikona aktywnego stanu jest nad bohaterem, a w HUD (pod paskiem HP) ikony stanów z liczbą tur do końca; komunikat przy nałożeniu mówi skutek i czas, telefon (zakładka Start) ma wiersz Stany. Nazwy i skutki: `statuses`.

## Fabuła
Budujesz dom dla rodziny Nowaków. Pierwszą budowę otwiera prolog: pickup wjeżdża na działkę pełną porozrzucanych problemów (A pomija). Każdy etap otwiera wiadomość w telefonie od inwestorki Anny Nowak albo
kierownika Marka; po wygranej i porażce przychodzi wiadomość z morałem: plan pokonuje chaos budowy.
Teksty są w `data/game.json` (`story`).

## Motywacja
- **Odznaki** (9, np. Bez usterek, Przed terminem, Pełny zespół) - nagroda w doświadczeniu, baner przy zdobyciu.
- **Uprawnienia** (jak w Hades): każda zdobyta odznaka daje trwałą premię na każdą budowę (pole `perk` w `badges`):
  Bez usterek +2 max HP, Przed terminem moc -1 t., Seryjny +1 obrażeń, Zawodowiec kryt +5%, Twardziel +1 obrony,
  Pełny zespół +20 zł na start, Kolekcjoner +10% szans na narzędzie, Katalog +1 szczęścia, Osiedle +10% doświadczenia.
  Premia zaznaczonej odznaki jest w telefonie profilu (Odznaki).
- **Zlecenia** (jak lista przepowiedni, `contracts`): długofalowe cele z licznikami w profilu – Trzy fachy, Czysta robota
  (boss aktu bez obrażeń w walce z nim), Markowy styl (5 markowych przedmiotów), Mocarz (100 użyć mocy), Pogromca usterek
  (200 problemów), Stały klient (5 wygranych). Nagroda: doświadczenie i/lub pamiątka, baner przy ukończeniu.
  Telefon profilu: zakładka Odznaki, strona Zlecenia (A); w trakcie budowy zakładka Koszty pokazuje najbliższe zlecenie.
- **Pamiątki** (jak keepsakes, `keepsakes`): jedna zabierana na budowę, wybór L/R na ekranie zawodu. Termos babci
  (termos +1 miejsce, od początku – nowy profil zabiera go domyślnie), Kask ojca (+1 obrony, odznaka Bez usterek), Szczęśliwa kielnia (+2 szczęścia),
  Stara poziomica (widzenie +1), Notes kierownika (moc -1 t.) – trzy ostatnie za zlecenia. Ranga II po 3, III po 8
  budowach z pamiątką (`rankRuns`, wartości `values`). Strona Pamiątki w telefonie profilu.
- **Katalog usterek** - pokonane rodzaje problemów z opisami.
- **Osiedle** - dom za każdą wygraną budowę, wielkość zależy od wyniku.
- SELECT na tytule: telefon profilu (Odznaki/Zlecenia/Pamiątki, Katalog, Osiedle, Zespół, Koszty).

## Wydarzenia na placu
Na starcie etapu (nie pierwszego i nie z bossem) z szansą `siteEvents.chancePct` przychodzi SMS z modyfikatorem etapu:
Dostawa spóźniona (mniej znajdziek), Premia od inwestora (+20 zł), Inspekcja nadzoru (etap bez obrażeń = +10 dośw.),
Ulewa w nocy (ciosy częściej dają poślizg), Ekipa na kawie (pełny termos). Wiadomość na karcie etapu, wiersz w zakładce
Zadania telefonu. Teksty i wartości: `siteEvents` w `data/game.json`.

## Trudność i meta-progresja
- HP wrogów rośnie z etapem, poziom trudności (Łatwy/Normalny/Trudny) mnoży siłę wrogów i wynik.
- Poziomy postaci w trakcie budowy: awans za doświadczenie daje +HP, na 3. poziomie +obrona, na 5. +obrażenia.
- Po wygranej: „Kolejna budowa” (NG+) – ten sam zawód i premie, mocniejsi wrogowie.
- Doświadczenie za wrogów, etapy i bossa wydajesz w sklepie „Szkolenia” (po budowie i z tytułu):
  ulepszenia statystyk (m.in. Kurs BHP II: +1 szczęścia, Warsztaty: +1 do statystyki broni zawodu), więcej znajdziek,
  nowe zawody, narzędzia, poziom Trudny.
- Profil (rekord, doświadczenie, zakupy, odznaki, liczniki zleceń, pamiątki) zapisuje się w SRAM (format v5);
  starsze zapisy (v1-v4) są przenoszone bez utraty danych.
- Liczniki zleceń trafiają do profilu na końcu etapu; profil pamięta, ile z bieżącej budowy już przeniesiono, więc
  wznowienie budowy po wyłączeniu konsoli nie liczy etapu drugi raz.
- Harmonogram między etapami pokazuje radę kierownika (sterowanie i mechaniki; lista `tips` w `data/game.json`).
- Budowa zapisuje się sama na starcie każdego etapu i przez „Zapisz i wyjdź” w menu; na tytule START wznawia. Śmierć, wygrana i porzucenie kasują zapis.

## Budowanie
Wymagania: [Butano](https://github.com/GValiente/butano) 21.8.0, devkitARM **albo** Wonderful Toolchain
(+ `blocksds-toolchain`), Python 3. Do regeneracji grafik: `pip install pillow cairosvg qrcode`.

```bash
python3 tools/gen_data.py        # data/game.json -> include/game_data.h
python3 tools/make_assets.py     # (opcjonalnie) grafiki -> graphics/*.bmp
python3 tools/make_audio.py      # (opcjonalnie) dźwięk -> audio/*.wav, audio/*.mod (Maxmod)
make -j8 BUTANO_PATH=/ścieżka/do/butano/butano
```
Domyślnie Makefile szuka Butano w `../../butano/butano` (obok repo).

Testy rdzenia na PC (spójność 500 map, determinizm seedów, symulacja botem balansu każdego zawodu):
```bash
g++ -std=c++20 -O2 -Iinclude tests/core_tests.cpp -o core_tests && ./core_tests
```

Playtest bez okna (Docker + libmgba): skrypt klawiszy -> zrzuty ekranu PNG, np.
```bash
tools/playtest/run.sh moj_skrypt.txt /tmp/zrzuty --fresh   # opis komend w tools/playtest/playtest.c
```

Scenariusze testowe (sytuacje, do których skrypt nie dojdzie na ślepo – boss obok, wrogowie w zasięgu, moce,
sprzęt, stany, porównanie sprzętu, termos, kryt i unik, statystyki (9), uprawnienia i zlecenia (10), pamiątki (11),
wydarzenia na placu (12)): build z `-DPB_SCENARIO=N` (opis w `src/debug_scenarios.h`), np.
```bash
make TARGET=scn1 BUILD=build_scn1 USERFLAGS="-DPB_SCENARIO=1" BUTANO_PATH=...
ROM=scn1.gba tools/playtest/run.sh skrypt.txt /tmp/zrzuty --fresh
```

## Uruchomienie na Miyoo
Skopiuj `PlanBudowlanyRogue.gba` do `Roms/GBA/` (OnionOS) lub folderu GBA w MinUI. Działa na rdzeniach
mGBA i gpSP. Zapis rekordu: SRAM (emulator tworzy plik .sav/.srm).

## Struktura
```
data/game.json        zawody, narzędzia, wrogowie, etapy (źródło prawdy)
tools/gen_data.py     JSON -> include/game_data.h
tools/make_assets.py  proceduralne grafiki: font PL 8x16, sprite'y, kafelki+palety etapów, tytuł, ekran z QR
assets_src/pb_logo.svg  znak PlanBudowlany
include/core.h        logika gry (czyste C++, bez Butano) - testowalna na PC
include/meta.h        profil SRAM (v4), Szkolenia, odznaki i uprawnienia, zlecenia, pamiątki
src/main.cpp          warstwa GBA: sceny, mapa, kamera, HUD, SRAM
```
Grafiki są placeholderami generowanymi kodem: podmień pliki w `graphics/` pixel-artem z Aseprite
(te same nazwy i rozmiary) bez zmian w kodzie.

Gra nieoficjalna, niezwiązana z Nintendo ani przez nie licencjonowana. Silnik: Butano (zlib).
Font gry: pikselowy font o zmiennej szerokości z Butano (`common_variable_8x16_font`, licencja zlib) z dorysowanymi polskimi znakami (`assets_src/butano_variable_8x16_font.bmp`). Napisy na ekranie tytułowym: DejaVu Sans (licencja Bitstream Vera/DejaVu).
