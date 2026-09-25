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
| A | atak narzędziem w najbliższy cel w zasięgu (zasięg zależy od narzędzia) |
| B | czekaj turę (co 4 tury +1 HP) |
| START | dalej |
| SELECT | w grze: telefon z aplikacją PlanBudowlany (Zadania, Usterki, Start, Zespół, Koszty; L/R – zakładki, START – menu: jak grać, zapisz i wyjdź, porzuć budowę); na tytule: Szkolenia (ekran Koszty) |
| B (tytuł) | ekran „Jak grać” |
| L (przytrzymaj) | podgląd odkrytej mapy etapu |
| R | moc zawodu (Odprawa, Ścianka, Seria, Łańcuch, Zawór, Wirówka); ikona w prawym górnym rogu: szara z odliczaniem = ładuje się, pulsuje z „R” = gotowa; ranga II od 3. i III od 5. poziomu postaci |
| góra/dół (wybór zawodu) | poziom trudności: Łatwy / Normalny / Trudny |
| L+R+SELECT | skrót pokazowy: zalicz etap (do testów i prezentacji na stoisku) |

## Zawody (dane w `data/game.json`)
Kierownik budowy (Dziennik budowy, zasięg 2) · Murarz (Kielnia) · Cieśla-dekarz (Gwoździarka, zasięg 3) ·
Elektryk (Próbnik napięcia, zasięg 2) · Hydraulik (Klucz nastawny) · Glazurnik (Szlifierka).
Znajdźki: kawa z termosu (+HP), kask (+obrona), projekt wykonawczy (+obrażenia).
Dropy: z pokonanych wrogów może wypaść kawa, kask, projekt albo skrzynka z narzędziem (Łom, Wkrętarka, Poziomica laserowa, Młot wyburzeniowy – zastępuje broń zawodu; kolejne narzędzia odblokowujesz w Szkoleniach).
Powiadomienia push jak w aplikacji: awans, nowe narzędzie, drop, moc gotowa, zaliczony etap, pojawienie się Terminu.
Mgła wojny: widzisz na 7 pól (ściany zasłaniają), odkryte pola zostają przyciemnione, wrogowie poza polem widzenia są ukryci.

## Fabuła
Budujesz dom dla rodziny Nowaków. Każdy etap otwiera wiadomość w telefonie od inwestorki Anny Nowak albo
kierownika Marka; po wygranej i porażce przychodzi wiadomość z morałem: plan pokonuje chaos budowy.
Teksty są w `data/game.json` (`story`).

## Motywacja
- **Odznaki** (9, np. Bez usterek, Przed terminem, Pełny zespół) - nagroda w doświadczeniu, baner przy zdobyciu.
- **Katalog usterek** - pokonane rodzaje problemów z opisami.
- **Osiedle** - dom za każdą wygraną budowę, wielkość zależy od wyniku.
- SELECT na tytule: telefon profilu (Odznaki, Katalog, Osiedle, Zespół, Koszty).

## Trudność i meta-progresja
- HP wrogów rośnie z etapem, poziom trudności (Łatwy/Normalny/Trudny) mnoży siłę wrogów i wynik.
- Poziomy postaci w trakcie budowy: awans za doświadczenie daje +HP, na 3. poziomie +obrona, na 5. +obrażenia.
- Po wygranej: „Kolejna budowa” (NG+) – ten sam zawód i premie, mocniejsi wrogowie.
- Doświadczenie za wrogów, etapy i bossa wydajesz w sklepie „Szkolenia” (po budowie i z tytułu):
  ulepszenia statystyk, więcej znajdziek, nowe zawody, poziom Trudny.
- Profil (rekord, doświadczenie, zakupy) zapisuje się w SRAM; starszy zapis z samym rekordem jest przenoszony.
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
src/main.cpp          warstwa GBA: sceny, mapa, kamera, HUD, SRAM
```
Grafiki są placeholderami generowanymi kodem: podmień pliki w `graphics/` pixel-artem z Aseprite
(te same nazwy i rozmiary) bez zmian w kodzie.

Gra nieoficjalna, niezwiązana z Nintendo ani przez nie licencjonowana. Silnik: Butano (zlib).
Font: DejaVu Sans Mono (licencja Bitstream Vera/DejaVu).
