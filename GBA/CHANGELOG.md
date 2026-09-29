# Changelog – PlanBudowlany RogueLike (GBA)

Wydania z plikiem ROM: https://github.com/aluspl/RogueLikeDSP/releases

## v0.21.50 – 2026-09-28
### Nowe
- **Rozpiska obrażeń broni (#26, jak w D&D / Baldur's Gate 3):** zakres ciosu od-do, kryt i skąd się biorą –
  liczone tymi samymi wzorami co walka (`dmg_breakdown` w `core.h`, test: zakres z rozpiski = to, co zadaje walka
  na tysiącach rzutów). Cios = rzut broni + statystyka broni / 2 (w dół) + premie stałe (Szkolenia, odznaki), z budowy
  (poziom, projekty) i rękawice - OBR problemu / 2 (w dół), najmniej 1; potem +% (Kurs fachowy, Respekt) – część
  procentowa w dół z resztą przenoszoną na następny cios, więc pojedynczy cios dostaje ją w dół albo w górę (zakres
  ma oba skraje, średnia dokładna); kryt (5% + 3%/pkt SZCZ + cechy + premie) mnoży wynik po procencie.
- **Wybór zawodu:** wiersz broni „Kielnia 8-11, kr 16-22 21%” (z premiami profilu); START = Statystyki, dalej
  strony „Obrażenia broni” (broń, statystyka, premie z podziałem na Szkolenia / Respekt / odznaki / pamiątkę,
  procent, cios i średnia, kryt z częściami, moc), na końcu „Jak działają”.
- **Telefon > Sprzęt:** narzędzie z zakresem i krytem, przy każdym przedmiocie co daje („+1 OBR”, „+2 obrażeń”,
  „unik +8%”); A = rozpiska (w budowie też obrona i unik), **Brygada i naprawy – teraz dół** (START i A bez kierunku
  jak dawniej).
- **Porównanie:** paczka sprzętu pokazuje „teraz 12-15 -> 13-16 (średnio +1,1)”, zmianę krytu i OBR (na zmianę);
  nowe narzędzie – baner „Nowe: Młot udarowy / 13-16 -> 14-19 (śr. +2)”.
- **Karta problemu** (przytrzymane B): „Zadasz 11-14 (kryt 22-28), on Tobie 1” – po obronie w obie strony i procentach
  (za długie: osobno z unikiem), OBR problemu w pierwszej linii.
- „Jak grać” – strona 7 „Obrażenia” (sekcja `damageHelp` w `data/game.json`, wspólna z Godotem). Scenariusz 42.
- **Premia 1 z 3 po etapie (#27, jak w Hades / Slay the Spire, cz. 2):** po każdym zaliczonym etapie (przed
  Hurtownią i harmonogramem, jeden ciąg ekranów) wybierasz 1 z 3 premii na bieżącą budowę. 41 premii w danych
  (sekcja `boons`): 30 ogólnych i 11 premii zawodów (każdy zawód 1–2, np. Twarda odprawa – Odprawa ogłusza +1 t.,
  Gruba ścianka, Pełny magazynek, Mocny łańcuch, Mocny strumień, Szybka wirówka, Długa rynna, Gęsty tynk, Ciężka
  łyżka). Rzadkość **zwykła / rzadka / legendarna** (wagi 70/24/6, każdy punkt SZCZ przesuwa trochę ku rzadszym),
  znaczniki **Woda, Prąd, Beton, BHP, Szczęście, Kawa, Brygada, Materiały, Iskra**. **Synergie** (2+ premie z tym
  samym znacznikiem, baner „Synergia: …”): Przepięcie (woda + prąd – ciosy z prądem, porażenie dalej), Zbrojenie
  (beton – +1 OBR za każdą premię Beton), Espresso (kawa ładuje moc -3 t.), Pełne BHP (bez zatrucia i porażenia),
  Fart (+2 SZCZ), Stała ekipa (brygada -50%), Magazyn (materiały 2x częściej), Iskrzenie (wybuch pyłu +3 i szerzej).
  Raz na budowę można wylosować ofertę jeszcze raz za 25 zł; nowe ulepszenie Respektu **Druga oferta** daje darmowe
  losowanie. Oferta pochodzi z osobnego generatora (seed budowy, etap, losowanie) – ten sam seed = te same premie.
  Ekran wyboru: 3 karty w kolorze rzadkości, znaczniki, znacznik „Synergia!”, góra/dół i A, SELECT = losuj. Wybrane
  premie z opisami i aktywne synergie są w telefonie (Sprzęt → góra = strona Premie), premie do ciosu mają w rozpisce
  obrażeń własny wiersz „Premie etapów: +2, +6%, kryt +4%”.
- **Elity (#28):** część problemów (akt I 7%, akt II 11%, akt III 15%, Akt 0 5%; Łatwy -3, Trudny +5, NG+ +4 pkt)
  pojawia się wzmocniona: złota ramka, przedrostek nazwy zgodny z rodzajem („Zbrojony Przeciek”, „Uparta Pleśń”,
  „Szybkie Zwarcie”), +50% HP, +1 obrażeń i jedna cecha – **Tarcza** (+4 OBR – rozpiska i „Zadasz” na karcie liczą
  z nią), **Szybki** (2 kroki na turę), **Regeneracja** (+1 HP co turę), **Wybuchowy** (wybucha po usunięciu),
  **Wzywa pomoc** (przy 50% HP raz wzywa słabszy problem). Nagroda: pewny drop (paczka sprzętu co najmniej solidna),
  2 materiały i Respekt +1. Sekcja `elites`.
- **Kombinacje stanów (#29):** problemy i bohater mają stany z otoczenia – **mokry** (kałuże w Deszczu, problemy
  wodne jak Przeciek czy Ulewa, Zawór Hydraulika, premia Wąż ogrodowy), **zapylony** (akt III), **zmrożony** (Mróz,
  premia Suchy lód) – a ciosy żywioł: **prąd** (Próbnik, Łańcuch, premia Przedłużacz; po stronie problemów Zwarcie,
  Brak uziemienia, Kolizja z kablem), **iskra** (Szlifierka, Pistolet do kotew, Wirówka, premia Krzesiwo),
  **uderzenie** (cios wręcz). Kombinacje: **Mokry + prąd = Porażenie** (+3 celowi i mokrym obok), **Pył + iskra =
  Wybuch pyłu** (4 wokół celu, pył znika), **Zamróz + uderzenie = Pęknięcie** (+50% ciosu). Na bohaterze: mokry
  (nowy stan „Mokry”) i trafiony prądem – +2 i tracisz turę; wybuch w pyle aktu III +2. Ikony stanów nad problemami,
  napis „Mokry + prąd!” przy kombinacji, stany na karcie problemu, ikona Mokry w HUD. „Jak grać” ma 10 stron (nowe:
  Kombinacje, Skąd stany, Premie i elity). Sekcja `combos`, żywioły broni i problemów (`element`), rodzaj nazwy
  problemu (`gender`). Scenariusze testowe 43–51 (oferty każdej rzadkości, synergia, lista premii, elity, każda
  kombinacja, mokry bohater). Godot: te same ekrany (sceny `boon-*`, `elite-*`, `combo-*`, `help-combos`).

- **Wydarzenia z wyborem (#30, cz. 3):** na etapie (nie pierwszym budowy i nie z bossem, szansa 55%) leży pole
  z SMS-em (ikona telefonu z „!”). Wejście otwiera wiadomość, potem 2–3 odpowiedzi ze skutkami i wynik: co zaszło,
  a co „nie tym razem”. 12 wydarzeń w danych (sekcja `choiceEvents`), bez powtórek w budowie: Betoniarka sąsiada
  (-20 zł, ciosy +2 na etap / odmów), Tańszy dostawca (materiały +2, 30%: 2x Pleśń obok), Nadgodziny (+15 dośw.,
  +10 zł, -4 HP / +4 HP), Znaleziony projekt (premia 1 z 3 od razu za -4 HP / +6 dośw.), Zagubiony kask (kask
  solidny / Respekt +1 / +12 zł), Ekipa obok prosi, Automat z kawą (75%: kawa +2), Stara ostrzałka (narzędzie +1,
  35%: -3 HP), Stal przed czasem, Szybka kontrola BHP (OBR +1 na etap / dośw. z ryzykiem), Energetyk od Marka (moc
  gotowa), Premia za tempo (+25 zł, OBR -1 na etap). Skutki: zł, dośw., HP, max HP, materiały, ciosy / OBR do końca
  etapu, premia 1 z 3, sprzęt, Respekt, kawa, problemy obok, stan, ulepszenie narzędzia, moc. Pole, wydarzenie
  i los szansy pochodzą z osobnego generatora (seed budowy, etap) – ten sam seed = te same wydarzenia i wyniki.
  Wpis w dzienniku („SMS: …”, „Odpowiedź: …”, skutki) i w telefonie (Zadania: wydarzenie z odpowiedzią).
- **Ulepszanie narzędzia (#31):** Hurtownia ma pierwszy wiersz „Ulepsz narzędzie” – +1 obrażeń za poziom, maks. +3
  (koszt 40/60/80 zł + 3/4/5 stali, Rabat z Respektu działa na zł; także z wydarzenia Stara ostrzałka). Przy +2
  wybór cechy: **Przebicie** (ignoruje 2 OBR problemu), **Ostrze** (kryt +5%), **Wyważenie** (najsłabszy rzut +1).
  Nazwa „Kielnia+2” w telefonie (Sprzęt) i rozpisce, wiersz „Ulepszenie +2: +2 obr., Przebicie OBR -2”, przebicie
  w wierszu OBR problemu, ostrze w krycie – zakres z rozpiski = walka (test). Nowe narzędzie przy ulepszonym nie
  zakłada się samo: okno z porównaniem ciosu i „Uwaga: ulepszenie +2 przepadnie!” (A zamieniam, B zostaję);
  Hurtownia „Nowe narzędzie” ostrzega i też kasuje ulepszenie.
- **Ukryte pomieszczenia (#32):** 35% etapów bez bossa ma magazyn 3x3 za **pękniętą ścianą** (otworzy ją klucz,
  Operator koparki – wejście albo Taran – i każdy wybuch obok: wybuchowy problem, wybuch pyłu) albo **drzwiami
  magazynu** (tylko klucz). Klucz ma jeden problem – najpierw elita (karta pod B: „Ma klucz do magazynu!”), wypada po
  jego usunięciu; w połowie magazynów śpi elita-strażnik. W środku skrzynia: Respekt +2, +10 zł, po 1 każdego
  materiału i sprzęt co najmniej solidny. Pęknięcie / drzwi widać na polu muru, a na podglądzie mapy (L) – znacznik
  magazynu i skrzyni, gdy odkryte; w telefonie (Zadania) wiersz Magazyn. Bez klucza wejście w ścianę daje tylko
  podpowiedź (bez tury). Sekcja `hiddenRooms`.
- „Jak grać” ma 12 stron (nowe: Wydarzenia, Magazyn; sekcja `extrasHelp`, wspólna z Godotem). Skrót L+R+SELECT zalicza
  etap od razu (problem obok schodów nie spycha już bohatera w tej samej turze). Scenariusze testowe 52–56
  (wydarzenia, Hurtownia z ulepszeniem i cechą, zamiana ulepszonego narzędzia, pęknięta ściana z kluczem, drzwi).
  Godot: te same ekrany (sceny `event-*`, `upgrade-*`, `tool-swap`, `secret-*`, `help-extras`).

### Balans
Premie po etapie są mocne (bot bez nich: Normalny 1%, pełne Szkolenia 8%), więc problemy rosną z etapem: HP etapów
budowy +4 pkt proc. za każdy kolejny etap (Fundamenty 95% … Wykończenie 168%), obrażenia +1 od Stropu i kolejne +1 od
Okien i drzwi, etapy Aktu 0 +20% HP, Trudny 140% HP (było 130%). Premie osłabione przed balansem (+1 obrażeń tylko
w rzadkich, max HP +4/+8, procenty 6–10%). Nagroda za elitę to pewny drop, nie zawsze sprzęt (inaczej elity
ułatwiały grę o 10 pkt). Bot wybiera premię prostą kolejnością (obrażenia, %, OBR, HP, moc…, rzadsza wyżej,
+20 za nową synergię) i losuje ponownie tylko za darmo przy samych zwykłych. Kawa ma znaczenie: bot pije średnio
2,75 kawy na budowę (80% budów), bez picia kawy wygrywa 20% zamiast 30% (z pełnymi Szkoleniami 36% zamiast 52%).
Na budowę średnio 6 kombinacji stanów i 2,8 elity (Normalny, bez meta).
Część 3 dokłada graczowi siły (bot sam: ulepszenie +13 pkt, wydarzenia +7, magazyny +6 wygranych), więc problemy
znów rosną: HP etapów budowy +12 pkt proc. i +3 pkt za każdy kolejny etap (Fundamenty 107% … Wykończenie 207%),
+1 obrażeń od Ścian działowych, etapy Aktu 0 +4 pkt HP, Trudny 145% HP, inwestor „Problemy +20% HP” (było +25%).
Bot w części 3: pole wydarzenia i klucz / magazyn / skrzynia jako cel zamiast schodów, odpowiedź wg prostej wartości
skutków, w Hurtowni jedno ulepszenie na wizytę (cecha: Przebicie), nowe narzędzie tylko gdy lepsze niż ulepszone.
Na budowę (Normalny, bez meta): 2,2 wydarzenia, 0,8 magazynu, narzędzie +1,6; kawa 3,1 na budowę (82% budów).

| Wygrane bota (300 przebiegów na zawód) | v0.21.49 | v0.21.50 cz. 2 (premie, elity, kombinacje) | v0.21.50 cz. 3 (wydarzenia, ulepszenia, magazyn) |
|---|---|---|---|
| Łatwy | 58% | 54% | 53% |
| Normalny | 30% | 30% | 32% |
| Trudny | 12% | 11% | 10% |
| Normalny, pełne Szkolenia | 57% | 52% | 54% |
| Normalny, pełne Szkolenia + pełny Respekt | 71% | 68% | 70% |
| Normalny, pełne Szkolenia (i Respekt) + wszystkie modyfikatory | 10% | 14% | 9% |
| Normalny bez picia kawy (bez meta / pełne Szkolenia) | 17% / 38% | 20% / 36% | 22% / 40% |
| Normalny, pełne Szkolenia + pełny Respekt + Akt 0 (wszystkie nagrody) | 66% | 68% | 66% |
| Normalny bez premii po etapie (bez meta / pełne Szkolenia) | – | 1% / 8% | 4% / 14% |

### Zmiany
- Nowy zapis budowy (PBRUN12; w cz. 2 PBRUN11) – przerwana budowa z wcześniejszej wersji nie wznowi się. Profil bez
  zmian (PBRL010, 160 bajtów; Druga oferta to 16. ranga Respektu w istniejącej tablicy).
- Karta etapu pokazuje wyższy procent HP problemów na późnych etapach; 3 nowe rady kierownika (premie, mokry + prąd,
  złota ramka elity).

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
  Dekarz, Buty robocze, Pistolet do kotew, Tynkarz, Pas narzędziowy, Operator koparki i (8. wygrana) **Akt 0:
  Papierologia**. Lista z postępem na stronie Nagrody (Koszty → SELECT → SELECT), baner nagrody po wygranej, SMS „Nagroda:
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
- **Wrogowie pasujący do etapu (#24):** 20 nowych problemów, każdy etap ma własny zestaw (2 nowe + znajomi):
  Fundamenty – Woda gruntowa, Kamień w wykopie; Izolacja fundamentów – Osuwisko skarpy, Dziurawa folia; Mury parteru –
  Krzywy mur, Mostek termiczny; Strop – Ugięcie stropu, Brak zbrojenia; Dach – Przeciekająca papa, Zapchana rynna;
  Ściany działowe – Pęknięty pustak, Zła wymiarówka; Okna i drzwi – Nieszczelna ramka, Przeciąg; Instalacje –
  Zapowietrzenie, Brak uziemienia; Tynki i wylewki – Rysa skurczowa, Wilgoć w ścianie; Wykończenie – Odpryski płytek,
  Poprawki na odbiorze. Zachowania z danych (pole `behaviors`, parametry `behaviorParams`), łączone dowolnie:
  **strzela z dystansu** (2–3 pola w linii, prosto albo po skosie; iskry lecą do bohatera), **dzieli się** (dwa słabsze
  z połową HP), **łata innych** (+3 HP rannemu sąsiadowi), **wybucha** (czerwone pola wokół, tura na zejście),
  **rośnie** (co 4 tury +2 HP, co drugi stopień +1 obrażeń), **ucieka** (odskakuje, gdy stoisz obok), **nie rusza się**
  (za to twardy), **odpycha** (cios przesuwa o pole, co 3 tury), **wraca raz** (po 4 turach z połową HP). Pixel-art
  w stylu reszty (klatki 61–100), opisy z humorem w Katalogu usterek, „Cechy: …” na karcie wroga (przytrzymane B,
  na zmianę z opisem) i w Katalogu. Bot omija czerwone pola wybuchu.
- **10 etapów i różne akty (#14):** nowe etapy Izolacja fundamentów (akt I) i Ściany działowe (akt II), własne palety
  i wiadomości. Każdy akt ma własne kafle – akt I ziemia i bloczki betonowe, akt II deski i cegła, akt III płytki
  i tynk – oraz mechanikę (sekcja `mechanic` aktu): **akt I błoto** (wejście w błoto = stracona tura; Kładka działa
  też na błoto; bot omija błoto, jeśli się da), **akt II porywy wiatru** (co 6 tur poryw spycha o pole, zapowiedź turę
  wcześniej, licznik w HUD pod ikoną mocy), **akt III pył** (widzenie -2, pył w powietrzu). Baner na początku aktu,
  wiersz w zakładce Zadania („Poryw za N t. w lewo”), mechanika na karcie etapu.
- **Opis statystyk (#19):** na wyborze zawodu START = strona Statystyki (wartość i co daje, np. „SIŁ 5: +2 obrażeń
  broni”, „SZCZ 2: kryt 11%, unik 4%”) i strona „Jak działają” (wzory: +1 obr. co 2 pkt statystyki broni, -1
  obrażeń co 2 pkt OBR, kryt 5% +3%/pkt, unik +2%/pkt do 20%, łupy +2%/pkt). W telefonie zakładka Start → A:
  skąd są premie (zawód, Warsztaty, sprzęt, poziomy, kask, rękawice, kamizelka). „Jak grać” – strony 5 (akty
  i problemy) i 6 (statystyki). Scenariusze testowe 29–36.
- **Akt 0 „Papierologia” (#20)** – nagroda za 8. odbiór; od tej chwili każda nowa budowa (poza budową dnia) zaczyna się
  od dwóch etapów przed stanem surowym: **Pozwolenie** (biuro: segregatory, kartki) i **Przyłącza** (wykop
  z rurami). 8 nowych problemów – zawsze przedmioty, nigdy ludzie: Brakujący podpis (ucieka), Zaginiony wniosek (wraca
  raz), Termin na odwołanie (rośnie), Niezgodność z planem (strzela uwagami), Pieczątka nie ta (odpycha, papierologia),
  Pęknięta rura (dzieli się, poślizg), Brak ciśnienia (stoi i łata innych), Kolizja z kablem (wybucha, porażenie).
  Boss **Decyzja odmowna** – stos pism z pieczątką ODMOWA (Stempel ODMOWA w obszar, wzywa Zaginione wnioski); przy 50%
  HP **druga faza Odwołanie**: raz odzyskuje 30% HP i od razu wzywa wniosek (baner, czerwony błysk). Pokonana:
  „Pozwolenie wydane” +40 zł, premia za Akt 0 i Hurtownia. Mechanika aktu **pieczątki**: na etapie leżą 3 dokumenty
  (podpis, mapa, uzgodnienie) – schody widać, ale są zamknięte (kłódka) do kompletu; licznik 0/3 w HUD pod ikoną mocy,
  wiersz w Zadaniach, banery „Dokument: …” i „Komplet! Schody otwarte”. Anna pisze, że najpierw papiery; Akt 0 ma
  numer „0” na kartach etapów, a numeracja etapów liczy się od pierwszego etapu budowy (12 z Aktem 0, 10 bez). Wzory
  błota, kałuż, porywów i oferta ścieżek liczone od Fundamentów – Akt 0 nie zmienia etapów budowy.
- **Samouczek menu (#25)** – przy pierwszym uruchomieniu ekran przygasa, a Kierownik Marek w dymku jak powiadomienie
  PlanBudowlany opisuje po kolei: Nowa budowa, telefon profilu, Szkolenia, Respekt, codzienna budowa, Jak grać
  (tytuł) oraz zawód, trudność, pamiątkę, statystyki (START otwiera ich opis), tryb inwestora (po odblokowaniu) i start
  (wybór zawodu; omawiany element podświetlony). A – dalej, B – pomiń. Później jeden dymek „Nowość” przy pierwszym
  odblokowaniu: Respekt, codzienna budowa (po pierwszej budowie), tryb inwestora, Akt 0 i każdy nowy zawód z nagrody.
  „Jak grać” z tytułu kończy się stroną „pokaż samouczek jeszcze raz”. Teksty wspólne z Godotem (sekcja `tutorial`
  w `data/game.json`). Scenariusze testowe 37–41.
### Balans
v0.21.48 miało z pełnymi Szkoleniami 89% wygranych bota (cel 50–60%). Nie tylko BHP (+1 obrony) i Kurs fachowy (+1
obrażeń) – bot reaguje mocno na każdą premię, więc Szkolenia są lżejsze: **Szkolenie BHP -2% otrzymanych obrażeń**,
**Kurs fachowy +2% obrażeń** (procent z przeniesieniem reszty, bez losowania), Kondycja 2 poziomy, Lepszy termos
(kawa +1 HP), Dostawy, Kurs BHP II i Warsztaty po 1 poziomie. Siła przechodzi do Respektu. Stare profile: kupione BHP, Kurs
fachowy i Lepszy termos wracają jako doświadczenie (15, 20 i 10), poziomy ponad nowe maksimum też (Kondycja 35, Lepszy termos 20,
Dostawy 30, Kurs BHP II i Warsztaty po 40). Tryb inwestora łagodniej: Problemy +25% HP, Termin goni +1.
Kawa ma znaczenie (#17): bot pije średnio 2 kawy na budowę (74% budów), a bez picia kawy wygrywa 20% zamiast 32%
(z pełnymi Szkoleniami 39% zamiast 57%).

| Wygrane bota (300 przebiegów na zawód) | v0.21.48 (6 zawodów) | v0.21.49 cz. 1 (9 zawodów) | v0.21.49 (10 etapów, nowi wrogowie) |
|---|---|---|---|
| Łatwy | 60% | 61% | 58% |
| Normalny | 30% | 32% | 30% |
| Trudny | 12% | 13% | 12% |
| Normalny, pełne Szkolenia | 89% | 57% | 57% |
| Normalny, pełne Szkolenia + pełny Respekt | – | 74% | 71% |
| Normalny, pełne Szkolenia (i Respekt) + wszystkie modyfikatory | 10% | 10% | 10% |
| Normalny bez picia kawy (bez meta / pełne Szkolenia) | – | 20% / 39% | 17% / 38% |
| Normalny, pełne Szkolenia + pełny Respekt + Akt 0 (wszystkie nagrody) | – | – | 66% |

Akt 0 jest zablokowany dla nowych graczy, więc wyniki bez meta liczone są bez niego (Normalny 30%). Z Aktem 0 budowa
ma 12 etapów: przy pełnym meta 66% wygranych (bez Aktu 0 71%) – bot ginie głównie u Decyzji odmownej (etapy Aktu 0:
HP problemów 106/110%, obrażenia +2/+1, boss 34 HP, 3–5 obrażeń).

Po dodaniu 2 etapów, nowych wrogów i mechanik aktów: odepchnięcie działa co 3 tury (bez tego Krzywy mur zamykał
walkę wręcz – bot ginął na Murach parteru), Krzywy mur i Mostek termiczny słabsze, Trudny: problemy 130% HP (było 125%).
Bot szuka drogi z kosztem (błoto droższe) – 0 utkniętych przebiegów na Normalnym. Kawa: bot pije średnio 2,6 kawy
na budowę (80% budów), bez kawy wygrywa 17% zamiast 30%. Etapy 1–2 dalej bez śmierci bota.

### Zmiany
- Profil w SRAM v10 (PBRL010, 160 bajtów: v10 – obejrzane dymki samouczka i zawody z nagród, o których był dymek;
  profile v9 dostają Akt 0 za 8+ dotychczasowych wygranych, a kto już grał, nie ogląda głównego samouczka). Wcześniej v9 (PBRL009, 156 bajtów: v8 – Respekt, rangi, nagrody, wygrane i stawki zawodów 8–11; v9 – Katalog
  usterek dla problemów 17–48) – starsze profile przenoszą się bez utraty danych (nagrody za wygrane, zwrot
  doświadczenia za zmienione Szkolenia).
- Nowy zapis budowy (PBRUN10) – przerwana budowa z wcześniejszej wersji nie wznowi się.
- Do 16 problemów naraz na etapie (miejsce na podział), harmonogram domu i zakładka Zadania z 10 (12 z Aktem 0) etapami.
- Zawody i narzędzia z nagród nie są na sprzedaż w Szkoleniach; ceny brygady i Hurtowni z rabatem Respektu.
- Dymki samouczka menu: długi klawisz (np. „SELECT, Koszty”) nie nachodzi na podpowiedź „A: dalej  B: pomiń”,
  portret wybranego zawodu widoczny pod dymkiem; etap Aktu 0 nazywa się krótko **Pozwolenie** (mieści się w telefonie).

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
