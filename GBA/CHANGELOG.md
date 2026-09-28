# Changelog – PlanBudowlany RogueLike (GBA)

Wydania z plikiem ROM: https://github.com/aluspl/RogueLikeDSP/releases

## v0.21.49 – 2026-09-28
### Nowe
- **Respekt:** stała waluta za każdy ukończony etap – 2 za zwykły etap, 4 za bossa w środku aktu (Inspekcja Pracy),
  6 za bossa aktu, 10 za odbiór (mnożnik jak wynik: Łatwy mniej, Trudny i NG+ więcej). Trafia do profilu od razu po
  etapie, więc po porażce zostaje. Baner „Respekt +N” po etapie, stan w zakładce Koszty w trakcie budowy, na końcu
  budowy „Respekt z budowy”. Sekcja `respect` w `data/game.json`.
- **Sklep Respektu** (telefon profilu → Koszty → SELECT = strona Respekt): 15 stałych ulepszeń z rangami i rosnącą
  ceną – Pewna ręka (obrażenia +1/+2%), Gruba skóra (otrzymane -1/-2%), Dobre źródła (lepszy sprzęt), Oko fachowca
  (kryt +1/+2%), Zwinność (unik +1/+2%, łącznie maks. 20%), Mocna kawa (+5/+10%), Duży termos (+1 miejsce), Rutyna
  (moc -1/-2 t.), Oszczędności (+10–50 zł na start), Nauka (+4–20% doświadczenia), Znajomości (brygada -6–30%),
  Czujność (widzenie +1), Rabat (Hurtownia -5–25%), Zapasy (materiały +10–50% częściej), Druga szansa (raz na budowę
  1 HP zamiast końca). Pełny Respekt to ok. 1150 Respektu (kilkadziesiąt wygranych budów). „Start z przedmiotem”
  pominięty – na razie nie ma przedmiotów do zabrania (pomysł #18).
- **Nagrody za odbiór** (jak w Slay the Spire): każda wygrana odblokowuje kolejną nagrodę z listy – Młot udarowy,
  Dekarz, Buty robocze, Pistolet do kotew, Tynkarz, Pas narzędziowy, Operator koparki, dalej „Akt 0: Papierologia –
  wkrótce”. Lista z postępem na stronie Nagrody (Koszty → SELECT → SELECT), baner nagrody po wygranej, SMS „Nagroda:
  …!”, na ekranie końcowym „Za kolejny odbiór: …”. Stare profile dostają nagrody za dotychczasowe wygrane.
  Sekcja `rewards`.
- **Nowe zawody** (z nagród; pixel-art jak pozostali fachowcy, 2 klatki, sylwetka, ikona mocy, domy na Osiedlu):
  **Dekarz** (Dachówki 2-3, zasięg 3, ZRĘ; wiatr nie skraca mu zasięgu) – moc **Rynna**: dachówki lecą linią przez
  najbliższy widoczny problem i trafiają wszystkich na linii (4/5/6 pól, mur zatrzymuje); **Tynkarz** (Agregat
  tynkarski 2-4, zasięg 2, SIŁ) – moc **Narzut**: tynk na obszar 3x3 wokół celu (5x5 na III, od II ogłusza);
  **Operator koparki** (Łyżka koparki 4-7, dużo HP i obrony, ZRĘ 1 i zero szczęścia – wolny, bez uników; cios wręcz
  w 20% odpycha problem o pole) – moc **Taran**: szarża do problemu, cios +ranga i odepchnięcie o 2 pola.
- **Nowe narzędzia** w dropach (z nagród): Młot udarowy (5-9, SIŁ) i Pistolet do kotew (3-5, zasięg 3, ZRĘ).
- **Nowy sprzęt** (z nagród): **Buty** (unik +3/+5/+8%) i **Pas** (termos +1/+1/+2 miejsca); nowa cecha **Bez
  poślizgu**. Zakładka Sprzęt pokazuje 5 slotów (materiały wtedy w nagłówku). Pełny sprzęt dla Inspekcji Pracy to
  nadal kask, rękawice i kamizelka.
- Wybór zawodu: pasek portretów przewija się (9 zawodów); zawód z nagrody pokazuje „Za N. wygraną”.
- „Jak grać” – czwarta strona (Respekt i nagrody). Scenariusze testowe 22–28.
### Balans
v0.21.48 miało z pełnymi Szkoleniami 89% wygranych bota (cel 50–60%). Nie tylko BHP (+1 obrony) i Kurs fachowy (+1
obrażeń) – bot reaguje mocno na każdą premię, więc Szkolenia są lżejsze: **Szkolenie BHP -2% otrzymanych obrażeń**,
**Kurs fachowy +2% obrażeń** (procent z przeniesieniem reszty, bez losowania), Kondycja 2 poziomy, Lepszy termos
(kawa +1 HP), Dostawy, Kurs BHP II i Warsztaty po 1 poziomie. Siła przechodzi do Respektu. Stare profile: kupione BHP, Kurs
fachowy i Lepszy termos wracają jako doświadczenie (15, 20 i 10), poziomy ponad nowe maksimum też (Kondycja 35, Lepszy termos 20,
Dostawy 30, Kurs BHP II i Warsztaty po 40). Tryb inwestora łagodniej: Problemy +25% HP, Termin goni +1.
Kawa ma znaczenie (#17): bot pije średnio 2 kawy na budowę (74% budów), a bez picia kawy wygrywa 20% zamiast 32%
(z pełnymi Szkoleniami 39% zamiast 57%).

| Wygrane bota (300 przebiegów na zawód) | v0.21.48 (6 zawodów) | v0.21.49 (9 zawodów) |
|---|---|---|
| Łatwy | 60% | 61% |
| Normalny | 30% | 32% |
| Trudny | 12% | 13% |
| Normalny, pełne Szkolenia | 89% | 57% |
| Normalny, pełne Szkolenia + pełny Respekt | – | 74% |
| Normalny, pełne Szkolenia (i Respekt) + wszystkie modyfikatory | 10% | 10% |
| Normalny bez picia kawy (bez meta / pełne Szkolenia) | – | 20% / 39% |

### Zmiany
- Profil w SRAM v8 (PBRL008, 152 bajty: Respekt, rangi, nagrody, wygrane i stawki zawodów 8–11) – starsze profile
  przenoszą się bez utraty danych (nagrody za wygrane, zwrot doświadczenia za zmienione Szkolenia).
- Nowy zapis budowy (PBRUN08) – przerwana budowa z v0.21.48 nie wznowi się.
- Zawody i narzędzia z nagród nie są na sprzedaż w Szkoleniach; ceny brygady i Hurtowni z rabatem Respektu.

## v0.21.48 – 2026-09-25
### Nowe
- **Wybór ścieżki:** na harmonogramie między etapami dwa warianty kolejnego etapu jako rozgałęzienie (lewo/prawo, A) –
  **Szybko i drogo** (-2 problemy, -15 zł, -1 znajdźka), **Tanio, ryzyko** (+1 problem, +2 znajdźki, tylko zła pogoda),
  **Po terminie** (bez wydarzenia na placu, -10 zł), **Z zapasem** (+1 problem, +3 materiały). Kolejność etapów bez
  zmian, oferta zależy od seeda budowy. Wybrana ścieżka w zakładce Zadania i na karcie etapu. Sekcja `paths`.
- **Materiały:** cement, stal i drewno wypadają z problemów (35%, każdy problem ma swój materiał), bossów (po 2 każdego)
  i paczek sprzętu. Ikony z liczbą w HUD, wiersz w zakładce Sprzęt. W Hurtowni towary za materiały: **Zbrojenie**
  (4 stal, +1 obrony), **Wylewka** (4 cement, +3 max HP), **Deskowanie** (3 drewno, 2 kawy do termosu).
- **Naprawy pola** (telefon → Sprzęt → A, pod Brygadą): **Załataj** (1 drewno – mur z desek w poprzek drogi problemu na
  6 tur) i **Kładka** (1 stal – kałuże w zasięgu 2 bez poślizgu do końca etapu, zdejmuje poślizg). Zużywają turę.
- **Codzienna budowa** (R na tytule): GBA nie ma zegara, więc datę ustawiasz strzałkami (pamięta ją profil). Z daty:
  „Budowa dnia nr N”, zawód dnia i dwa modyfikatory dnia – dla wszystkich takie same, bez Szkoleń i pamiątek.
  Najlepszy wynik i wygrana każdego z 5 ostatnich dni w profilu; po budowie „Rekord dnia!”, bez NG+. Sekcja `daily`.
- **Harmonogram domu po wygranej:** telefon z aplikacją PlanBudowlany – zdjęcie domu z Osiedla, etapy z datą
  rozpoczęcia, liczbą dni i kosztem (tys. zł, pole `cost` etapu), razem dni i koszt, na dole planbudowlany.online
  (kod QR na kolejnym ekranie).
- **Wyraźny awans:** złoty błysk, pierścień i unoszące się gwiazdki, napis „AWANS! Poziom N” nad bohaterem (~1,5 s),
  w banerze co się poprawiło (max HP, obrona, obrażenia, ranga mocy).
- **Koniec budowy motywuje:** na ekranie końcowym na zmianę rekord (albo „Nowy rekord!”), najbliższe zlecenie
  z postępem, najbliższe Szkolenie („Stać Cię” albo „brakuje N dośw.”) i stawka.
- „Jak grać” – trzecia strona (ścieżki, materiały i naprawy, budowa dnia). Scenariusze testowe 17–21.
### Balans
Bot testów balansu naprawiony: wybierał cel po odległości w linii prostej przez ścianę i w ~28% przebiegów krążył
między dwoma celami do limitu kroków (liczone jako porażka). Teraz cel i krok po odległości ścieżki, unik przed ciosem
bossa w stronę celu, mur Ścianki przeczekuje – 0 utkniętych przebiegów. Potem gra jest trudniejsza (jak w roguelike):
etapy aktów II–III mocniejsze (HP 95→132%, obrażenia +1/+2), więcej problemów (5–9), problemy wcześniej zauważają
bohatera, bossowie bez zmian w sile, broń wręcz (Kielnia, Klucz, Szlifierka) i HP Murarza/Hydraulika wyżej. Szkolenia:
BHP i Kurs fachowy po 1 poziomie (zwrot 30/40 dośw. za kupiony drugi poziom), Kondycja +2 HP, Lepszy termos +2 HP.
Tryb inwestora: Problemy +35% HP, Termin goni +2. Etapy 1–2 bez śmierci bota – giną u bossów aktów II i III.

| Wygrane bota (300 przebiegów na zawód) | v0.21.47 | bot bez pętli | v0.21.48 |
|---|---|---|---|
| Łatwy | 68% | 97% | 60% |
| Normalny | 49% | 64% | 30% |
| Trudny | 34% | 36% | 12% |
| Normalny, pełne Szkolenia | 70% | 99% | 89% |
| Normalny, pełne Szkolenia + wszystkie modyfikatory | 24% | 84% | 10% |
| Utknięte przebiegi (Normalny) | 28% | 0% | 0% |

### Zmiany
- Profil w SRAM v7 (PBRL007, 124 bajty: data i wyniki budowy dnia) – starsze profile przenoszą się bez utraty danych;
  poziomy Szkoleń ponad nowe maksimum wracają jako doświadczenie.
- Nowy zapis budowy (PBRUN07) – przerwana budowa z v0.21.47 nie wznowi się.
- Stawka trybu inwestora na ekranie końcowym w wierszu na zmianę (w rogu zasłaniała kod QR).

## v0.21.47 – 2026-09-25
### Nowe
- **Pogoda dnia:** każdy etap losuje pogodę – Słonecznie (bez skutku), **Upał** (moc odnawia się 1 turę dłużej),
  **Mróz** (problemy stoją co 3. turę), **Wiatr** (broń z dystansu: zasięg -1; etapy Mury parteru – Okna i drzwi),
  **Deszcz** (kałuże na mapie, wejście w kałużę = poślizg; etapy do Okien i drzwi). Wewnątrz domu (Instalacje,
  Tynki, Wykończenie) tylko słońce albo upał. Ikona pogody w HUD obok termosu, wiersz „Pogoda” w zakładce Zadania,
  linia na karcie etapu. Niekorzystna pogoda nie łączy się z niekorzystnym wydarzeniem na placu (wydarzenie przepada).
- **Brygada:** raz na etap najemny fachowiec za zł z budżetu budowy (wezwanie zużywa turę): **Geodeta** (8 zł, cała
  mapa etapu i schody), **BHP-owiec** (10 zł, zdejmuje stany, obrona +2 na 8 tur), **Pompa do betonu** (15 zł, -6 HP
  problemom w zasięgu 2) i **Elektryk-kolega** (15 zł, idzie obok bohatera przez 10 tur, bije sąsiadów za 3
  i zasłania drogę problemom). Dwóch pierwszych od razu, pozostałych kupisz w Szkoleniach. Telefon: zakładka Zespół
  (Sprzęt) – A otwiera Brygadę; w menu akcji (START) A bez kierunku. Baner i efekt przy przyjściu fachowca.
- **Tryb inwestora** (jak Heat w Hadesie): po pierwszej wygranej SELECT na wyborze zawodu otwiera modyfikatory –
  Budżet -30%, Bez przerwy na kawę, Problemy +20% HP, Hurtownia zamknięta, Kontrola częściej (cios bossa co 3 tury),
  Termin goni (+1 obrażeń problemów). Każdy daje % doświadczenia i punkty **stawki** (razem do 10). Rekord stawki
  każdego zawodu w profilu; stawka na karcie zawodu, w SMS-ie końca budowy i na ekranie końcowym.
- „Jak grać” ma drugą stronę (pogoda, brygada, tryb inwestora).
- Scenariusze testowe 14 (pogoda po kolei), 15 (brygada), 16 (tryb inwestora).
### Zmiany
- Profil w SRAM v6 (PBRL006: brygada, modyfikatory, rekord stawki) – starsze profile przenoszą się bez utraty danych.
- Nowy zapis budowy (PBRUN06) – przerwana budowa z v0.21.46 nie wznowi się.
- Dane w `data/game.json`: sekcje `weather`, `brigade`, `investor`.
- Balans (bot): Normalny 49% wygranych (z pełnymi Szkoleniami 70%); wszystkie modyfikatory inwestora: 4% bez
  Szkoleń, 24% z pełnymi.

## v0.21.46 – 2026-09-25
### Nowe
- **Nowy boss: Inspekcja Pracy** – kontrola BHP bez zapowiedzi w środku aktu III (etap Instalacje; Termin dalej
  czeka na końcu). Podkładka z protokołem o surowych brwiach, odlatującą kartką i pieczątką z czerwonym tuszem.
  SMS od kierownika przed etapem, powiadomienie „Przypisano Ci usterkę: Inspekcja Pracy”, wpis w katalogu usterek.
- **Kontrola BHP:** Inspekcja stempluje w **krzyż** (wiersz i kolumna bohatera, 2 pola w każdą stronę) zamiast
  kwadratu 3×3; 3 tury na zejście – najlepiej po skosie. Trafienie może nałożyć Papierologię.
- **Wezwania:** co 6 tur Inspekcja wzywa Papierologię (najwyżej 2 na walkę).
- **„Wszystko zgodnie z BHP!”** – w pełnym sprzęcie (kask, rękawice, kamizelka) Inspekcja na start walki traci 2 tury.
- Pokonana Inspekcja: baner **„Protokół bez uwag”** i premia +60 zł; potem zwykły harmonogram (Hurtownia zostaje
  tylko między aktami).
- Mechaniki bossów sterowane danymi (`data/game.json`): `slamShape`, `slamName`, `summon`, `gearStun`, `reward`
  wroga oraz `delay`, `crossReach`, `crossDelay` w `slam`.
- Scenariusz testowy 13: Inspekcja obok bohatera w pełnym sprzęcie.
### Zmiany
- Etap Instalacje: 7 problemów zamiast 8 (plus boss), bez wydarzenia na placu (jak każdy etap z bossem).
- Bot testów balansu schodzi z czerwonych pól na pole poza zasięgiem i nie wchodzi na nie przed ciosem
  (bez zmian dla kwadratu bossów aktów). Normalny: 52% wygranych (z pełnymi Szkoleniami 70%).
- Nowy zapis budowy (PBRUN05) – przerwana budowa z v0.21.45 nie wznowi się.

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
