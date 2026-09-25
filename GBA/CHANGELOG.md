# Changelog – PlanBudowlany RogueLike (GBA)

Wydania z plikiem ROM: https://github.com/aluspl/RogueLikeDSP/releases

## v0.21.45 – 2026-09-25
### Nowe
- **Nowy ekran wyboru zawodu:** pasek portretów wszystkich zawodów u góry – wybrany powiększony na fioletowym polu,
  kołysze się i przebiera nogami, zablokowane jako ciemne sylwetki z małą kłódką. Pod nim karta zawodu: moc z ikoną
  i opisem, broń z obrażeniami, zasięgiem i statystyką skalowania, statystyki w siatce z premiami na zielono
  (statystyka broni wyróżniona), trudność jako kolorowa pastylka (Łatwy / Normalny / Trudny). Karta wjeżdża z boku
  przy zmianie zawodu.
### Zmiany
- Zawody posortowane: najpierw odblokowane, potem zablokowane (w obu grupach kolejność z danych). Zablokowany można
  obejrzeć, ale nie wystartować (A – portret kręci głową); pod kartą podpowiedź „Odblokuj w Kosztach (telefon)”
  z kosztem i posiadanym doświadczeniem.
- Pamiątka (L/R) pod kartą z pełnym opisem efektu; zmiana pamiątki od razu przelicza statystyki na karcie.
- Sterowanie bez zmian: lewo/prawo – zawód, góra/dół – trudność, L/R – pamiątka, A/START – start, B – powrót.

## v0.21.44 – 2026-09-25
### Nowe
- **Rada kierownika** na ekranie harmonogramu między etapami: krótka podpowiedź o sterowaniu i mechanikach
  (podgląd pod B, celownik pod A, termos, uniki przed bossem, mapa pod L…), co etap inna. Lista `tips` w `data/game.json`.
### Zmiany
- Nowy profil zaczyna z wybraną pamiątką Termos babci (nie trzeba jej wybierać L/R); profil przeniesiony bez wybranej
  pamiątki dostaje pierwszą odblokowaną.
- Profil w formacie v5 (migracja z v4/v3/v2/v1 bez utraty danych); nowy zapis budowy (PBRUN04) – przerwana budowa
  z v0.21.43 nie wznowi się.
- Telefon, zakładka Koszty w trakcie budowy: czytelniej – „Wydane na szkolenia X z Y dośw.” i „Do wydania po budowie”
  zamiast mylącego „Budżet / Pozostało”.
- Odznaki w telefonie profilu: nagroda za niezdobytą odznakę jako „+15 dośw.” zamiast samego „+15”.
- Harmonogram pokazuje zaliczony etap i trzy kolejne.
### Poprawki
- Liczniki zleceń z etapu nie liczą się drugi raz po wyłączeniu konsoli i wznowieniu budowy z autozapisu
  (profil pamięta, ile z bieżącej budowy już przeniesiono); postęp w telefonie też nie pokazuje etapu podwójnie.
- Ucinane teksty: nazwa etapu pod pastylką „W trakcie” (zakładka Start), opis paczki w Hurtowni, komunikaty dziennika
  (np. „Zostawiasz stary sprzęt: +2 dośw.”) – teksty mierzone są teraz w pikselach (font o zmiennej szerokości),
  a ucięte kończą się kropką zamiast wychodzić poza ramkę.
- ROM kompiluje się bez ostrzeżeń.

## v0.21.43 – 2026-09-25
### Nowe
- **Statystyki:** cechy sprzętu Siła/Zręczność/Inteligencja +1 (podnoszą statystykę broni), narzędzia skalowane INT
  do odblokowania w Szkoleniach (Tablet z projektem, Miernik laserowy), nowe Szkolenia Kurs BHP II (+1 szczęścia)
  i Warsztaty (+1 do statystyki broni zawodu). Statystyki efektywne („SIŁ 5+2”) na wyborze zawodu i w telefonie.
- **Uprawnienia:** każda odznaka daje trwałą premię na każdą budowę (np. Bez usterek +2 max HP, Seryjny +1 obrażeń,
  Przed terminem moc -1 t., Kolekcjoner +10% szans na narzędzie, Osiedle +10% doświadczenia). Premia widoczna
  przy odznace w telefonie profilu.
- **Zlecenia:** długofalowe cele z licznikami (Trzy fachy, Czysta robota, Markowy styl, Mocarz, Pogromca usterek,
  Stały klient) z nagrodą w doświadczeniu i pamiątkach. Strona Zlecenia w zakładce Odznaki (A), baner przy ukończeniu,
  najbliższe zlecenie z postępem w zakładce Koszty w trakcie budowy.
- **Pamiątki:** przedmiot zabierany na budowę (L/R na wyborze zawodu): Termos babci, Kask ojca, Szczęśliwa kielnia,
  Stara poziomica, Notes kierownika. Ranga II po 3 i III po 8 budowach z pamiątką. Strona Pamiątki w telefonie profilu.
- **Wydarzenia na placu:** na starcie etapu (nie pierwszego i nie z bossem) losowy SMS z modyfikatorem – Dostawa
  spóźniona, Premia od inwestora, Inspekcja nadzoru, Ulewa w nocy, Ekipa na kawie. Wiersz w zakładce Zadania.
- Scenariusze testowe 9 (statystyki), 10 (uprawnienia i zlecenia), 11 (pamiątki), 12 (wydarzenia).
### Zmiany
- Koniec etapu czeka, aż pokażą się banery odznak i zleceń (A pomija); banery także na ekranie końcowym.
- Ekran wyboru zawodu: trudność u góry, pamiątka obok postaci, statystyka broni przy narzędziu (np. „(SIŁ)”).
- Profil w formacie v4 (migracja z v3/v2/v1 bez utraty danych); nowy zapis budowy (PBRUN03) – przerwana budowa
  z v0.21.42 nie wznowi się.
- Konfiguracja w `data/game.json`: `badges[].perk`, `contracts`, `keepsakes`, `siteEvents`.

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
