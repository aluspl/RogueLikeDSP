# Changelog – PlanBudowlany RogueLike (GBA)

Wydania z plikiem ROM: https://github.com/aluspl/RogueLikeDSP/releases

## v0.21.42 – 2026-09-25
### Nowe
- **Szczęście:** nowa statystyka zawodu (Glazurnik 4, Hydraulik/Kierownik/Cieśla 2, Elektryk 1, Murarz 0),
  widoczna na wyborze zawodu i w telefonie. Daje kryt (5% + 3%/pkt, obrażenia x2, żółte „KRYT!” i błysk),
  mały unik przed ciosem wroga („Unik!”), częstsze i lepsze dropy.
- **Cechy sprzętu:** każdy przedmiot ma losową cechę (Szczęście +1, Kryt +5%, Odporność na zatrucie,
  Widzenie +1, Odnowienie mocy -1). Paczka przy zajętym slocie otwiera okno porównania
  (A – zakładam, B – zostawiam za doświadczenie). Cechy w zakładce Sprzęt.
- **Menu akcji pod START:** ikony wokół bohatera – Atak, Moc, Termos, Czekaj.
- **Termos:** kawa trafia do termosu (3 miejsca), pije się z menu (zużywa turę); pełny termos – pije od razu.
  Ikona termosu z liczbą w HUD.
- Scenariusze testowe 6 (porównanie sprzętu), 7 (termos), 8 (kryt i unik).
### Zmiany
- Stany czytelniejsze: liczba tur przy ikonach w HUD, komunikat mówi skutek i czas
  (np. „Zatrucie: -1 HP/turę, 3 t.”), wiersz Stany w telefonie.
- Zakładka Start: szczęście, kryt i unik; budżet w nagłówku; wynik w nagłówku Zadań.
- Balans: HP problemów na etapach +8–13 pp (bot na Normalnym ~51% wygranych).
- Nowy format zapisu budowy (PBRUN02) – przerwana budowa z v0.21.41 nie wznowi się.
- Konfiguracja w `data/game.json`: `luck`, `thermos`, `statuses`, `equipment.traits`.

## v0.21.41 – 2026-09-25
### Nowe
- **Prolog przy pierwszej budowie:** pickup PlanBudowlany wjeżdża na działkę, bohater wysiada, kamera
  przejeżdża przez plac z porozrzucanymi problemami budowy, na koniec SMS od inwestorki. A pomija.
- Numer wersji na ekranie tytułowym.
### Zmiany
- Instrukcja „Jak grać” opisuje 8 etapów w 3 aktach i nowe sterowanie (celowanie, podgląd, telefon).

## v0.21.40 – 2026-09-25 (poprawka)
### Poprawki
- Telefon (SELECT) w trakcie budowy wywracał grę w v0.21.39 (brak wolnej warstwy tła).
- Komunikat o ciosie bossa nie jest już ucinany.
### Testy
- Scenariusze testowe playtestera (`-DPB_SCENARIO=N`): boss obok, wrogowie w zasięgu, moce wszystkich
  zawodów, sprzęt, stany. Pełna regresja: 92 zrzuty bez ekranu błędu.

## v0.21.39 – 2026-09-25
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
- Otwarcie telefonu w trakcie budowy wywraca grę – naprawione w v0.21.40.

## v0.21.38 – 2026-09-25
### Poprawki
- Ścianka Murarza nie zamyka już bohatera (mur w poprzek drogi wroga zamiast kwadratu wokół).
### Nowe
- Rangi mocy II/III (od 3. i 5. poziomu postaci), Zawór Hydraulika jako strumień odpychający wrogów.
- Ikona mocy w HUD z odliczaniem, podpowiedź mocy na start budowy.
- 8 etapów budowy (nowe: Strop, Okna i drzwi, Tynki i wylewki).
- Sprzęt z dropów: kask, rękawice, kamizelka w 3 jakościach, zakładka Sprzęt w telefonie.

## v0.21.37 – 2026-09-25
Pierwsze publiczne wydanie.
- 5 etapów budowy, 6 zawodów z mocami pod R, 9 problemów budowy i boss Nieprzekraczalny Termin.
- Trudność Łatwy / Normalny / Trudny, NG+, poziomy postaci, dropy i narzędzia.
- Telefon z aplikacją PlanBudowlany jako menu, powiadomienia push, fabuła w wiadomościach.
- Odznaki, katalog usterek, Osiedle, sklep Szkolenia, zapis w SRAM.
- Mgła wojny z miękkim światłem, cienie, animacje, cząsteczki, muzyka i efekty dźwiękowe.
