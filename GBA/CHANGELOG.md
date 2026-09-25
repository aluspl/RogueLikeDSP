# Changelog – PlanBudowlany RogueLike (GBA)

Wydania z plikiem ROM: https://github.com/aluspl/RogueLikeDSP/releases

## v0.2141 – 2026-09-25
### Nowe
- **Prolog przy pierwszej budowie:** pickup PlanBudowlany wjeżdża na działkę, bohater wysiada, kamera
  przejeżdża przez plac z porozrzucanymi problemami budowy, na koniec SMS od inwestorki. A pomija.
- Numer wersji na ekranie tytułowym.
### Zmiany
- Instrukcja „Jak grać” opisuje 8 etapów w 3 aktach i nowe sterowanie (celowanie, podgląd, telefon).

## v0.2140 – 2026-09-25 (poprawka)
### Poprawki
- Telefon (SELECT) w trakcie budowy wywracał grę w v0.2139 (brak wolnej warstwy tła).
- Komunikat o ciosie bossa nie jest już ucinany.
### Testy
- Scenariusze testowe playtestera (`-DPB_SCENARIO=N`): boss obok, wrogowie w zasięgu, moce wszystkich
  zawodów, sprzęt, stany. Pełna regresja: 92 zrzuty bez ekranu błędu.

## v0.2139 – 2026-09-25
### Czytelność
- Pikselowy font o zmiennej szerokości (Butano) z polskimi znakami.
- Dziennik: znikające komunikaty, powtórzenia jako „x2”, kolory według znaczenia, półprzezroczyste paski.
- Celowanie: przytrzymanie A pokazuje zasięg i celownik, strzałki zmieniają cel.
- Znacznik oznaczonego celu, mini paski HP wrogów, „!” gdy wróg Cię zauważy, karta wroga pod B.
### Mechaniki
- 3 akty zakończone bossami: Zepsuta Betoniarka, Nawałnica, Nieprzekraczalny Termin.
- Zapowiadane uderzenia bossów (czerwone pola, 2 tury na unik).
- Budżet za usunięte problemy, premia za akt, Hurtownia między aktami.
- Stany: zatrucie, porażenie, poślizg, papierologia.
- Konfiguracja aktów, bossów, Hurtowni i stanów w `data/game.json`.
### Znane problemy
- Otwarcie telefonu w trakcie budowy wywraca grę – naprawione w v0.2140.

## v0.2138 – 2026-09-25
### Poprawki
- Ścianka Murarza nie zamyka już bohatera (mur w poprzek drogi wroga zamiast kwadratu wokół).
### Nowe
- Rangi mocy II/III (od 3. i 5. poziomu postaci), Zawór Hydraulika jako strumień odpychający wrogów.
- Ikona mocy w HUD z odliczaniem, podpowiedź mocy na start budowy.
- 8 etapów budowy (nowe: Strop, Okna i drzwi, Tynki i wylewki).
- Sprzęt z dropów: kask, rękawice, kamizelka w 3 jakościach, zakładka Sprzęt w telefonie.

## v0.2137 – 2026-09-25
Pierwsze publiczne wydanie.
- 5 etapów budowy, 6 zawodów z mocami pod R, 9 problemów budowy i boss Nieprzekraczalny Termin.
- Trudność Łatwy / Normalny / Trudny, NG+, poziomy postaci, dropy i narzędzia.
- Telefon z aplikacją PlanBudowlany jako menu, powiadomienia push, fabuła w wiadomościach.
- Odznaki, katalog usterek, Osiedle, sklep Szkolenia, zapis w SRAM.
- Mgła wojny z miękkim światłem, cienie, animacje, cząsteczki, muzyka i efekty dźwiękowe.
