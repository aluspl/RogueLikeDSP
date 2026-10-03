# Changelog – PlanBudowlany RogueLike (GBA)

Wydania z plikiem ROM: https://github.com/aluspl/RogueLikeDSP/releases

## v0.21.53 – w przygotowaniu (filtry ekranu, tryby dla daltonistów, język angielski)
### In English
PlanBudowlany RogueLike now speaks English (GBA and Godot). Every enemy, boss, boon, event, text-message story,
tutorial bubble, How to Play page, phone tab, recap and tip has a natural English version – the Unmissable Deadline,
the Broken Concrete Mixer, Red Tape, the Builders' Merchant, Respect and Training are waiting. Pick the language on the
first boot of the GBA game (Polski / English; later: profile phone > Team > SELECT) or in the Godot settings (wrench >
Język / Language; default follows your system language). This release also adds screen filters – Noir, Retro LCD,
Night Neon and Acid to unlock – and colour-blind modes (Protanopia, Deuteranopia, Tritanopia, High Contrast) available
from the start, with patterns and letters so colour is never the only cue.

### Najważniejsze
Filtry ekranu – cztery do odblokowania postępem i cztery tryby dla daltonistów dostępne od pierwszego uruchomienia –
oraz cała gra po angielsku (GBA i Godot, wspólne dane i rdzeń):
1. **Filtry zabawowe (#53)** – **Noir** (czerń i biel, kontrast, ziarno, winieta; czerwień zagrożeń zostaje przygaszona,
   nie szara), **Retro LCD** (4 odcienie zieleni, dithering i kratka pikseli), **Neon nocy** (róż i błękit, przesunięcie
   kanałów, linie, poświata, drgnięcia taśmy), **Kwas** (tęcza w czasie, fale, rozlana barwa – z ostrzeżeniem o ruchu).
2. **Tryby dla daltonistów (#54)** – **Protanopia, Deuteranopia, Tritanopia** (daltonizacja: to, czego oko nie rozróżni,
   przenoszone na widoczne kanały – nie sama symulacja) i **Wysoki kontrast**; zawsze dostępne, nigdy za postępem.
   Przy tych trybach (i w Noir / Retro LCD) kolor nie jest jedyną wskazówką: pola ciosu i wybuchu w paski, litery
   rzadkości premii Z / R / L.
3. Zablokowany filtr widać jako „???” z podpowiedzią, po odblokowaniu – baner „Nowy filtr ekranu!”.
4. **Język angielski (#40)** – cała gra po polsku albo po angielsku (GBA i Godot, wspólne dane): wrogowie i bossowie,
   premie, wydarzenia, wątki SMS, samouczek, Jak grać, telefon, podsumowanie, rady, tytuły, sekrety, filtry. Tłumaczenie
   pisane, nie dosłowne (Nieprzekraczalny Termin → The Unmissable Deadline, Papierologia → Red Tape, Hurtownia →
   Builders' Merchant); imiona (Anna, Marek, Zenek, Ewa, Kowal, Lis) bez zmian. Wybór języka: GBA – przy pierwszym
   uruchomieniu (Polski / English), potem telefon profilu > Zespół > SELECT (od razu); Godot – Ustawienia (klucz) >
   Język / Language, domyślnie z języka systemu (polski – Polski, inny – English).

### Nowe
- **Odblokowanie (sekcja `screenFilters` w `game.json`, dowolny z warunków):**

  | Filtr | Warunek |
  |---|---|
  | Noir | poziom inspektora 5 |
  | Retro LCD | kolekcja Stan surowy (komplet problemów aktu I) |
  | Neon nocy | wygrana Kamienica albo sekret Szybka ekipa |
  | Kwas | poziom inspektora 20 albo sekret Mokra robota |
  | Protanopia, Deuteranopia, Tritanopia, Wysoki kontrast | zawsze |

  Warunki z danych (`unlock`: `inspector`, `collection`, `career`, `secret`, `wins`), teksty interfejsu w `screenFilters.ui`
  (pod tłumaczenie).
- **GBA:** wybór zawodu > SELECT – strona Wygląd zawsze dostępna (podpowiedź „SELECT: wygląd”), wiersz **Ekran** (A – kolejny
  odblokowany filtr, opis, „???” z podpowiedzią zablokowanego). Efekt własny palet Butano: każdy kolor tła i sprite'ów
  po ściemnianiu przechodzi przez `include/screen_filter.h` (pamięć podręczna 1024 kolorów), Kwas – wbudowany obrót barwy
  co 4 klatki. Jak grać str. 19 „Filtry ekranu”. Scenariusz 76 (każde uruchomienie z zapisem – kolejny filtr; plac
  z polami wybuchu, elitami i stanami).
- **Godot:** shader `shaders/screen_filter.gdshader` (jeden przebieg na teksturze ekranu, kilka próbek na piksel; klasyczny =
  bez kopii ekranu). Ustawienia (klucz) > **Filtr ekranu**: lista z „???”, pastylka Dostępność, **siła efektu** (-/+),
  **filtr na telefonie** (wył. – sam plac, HUD i plansze) i **ograniczony ruch** (bez fal, drgnięć i animowanego ziarna).
  Wiersz filtra także w Wyglądzie na wyborze zawodu (Tab). Jak grać str. 12. Ustawienia w `settings.cfg`.
- Baner „Nowy filtr ekranu!” na końcu budowy (Godot także na tytule po aktualizacji – filtry odblokowane wcześniej).

### GBA – co inaczej niż w Godocie
Paleta nie wie, gdzie jest piksel, więc na GBA nie ma ziarna, winiety, ditheringu, kratki, linii, przesunięcia kanałów ani
fal – filtry to samo przekształcenie kolorów (Retro LCD: 4 odcienie bez ditheringu, Kwas: sam obrót barwy, bez opcji
ograniczonego ruchu – ostrzeżenie w opisie). Daltonizacja i Wysoki kontrast działają jak w Godocie (ta sama macierz).
Pola ciosu i wybuchu na GBA zawsze mają wzór (czerwona ramka z ukośnymi kreskami); rzadkość premii jest opisana słowem.

### Sprawdzenie trybów dla daltonistów
Zrzut tej samej sceny w każdym trybie, potem symulacja wady innym modelem (Viénot, LMS) niż ten w filtrze (Machado 2009);
dE (CIELAB) między wskazówkami po symulacji, klasyczny -> tryb:

| Para | Protanopia | Deuteranopia | Tritanopia |
|---|---|---|---|
| Pole ciosu / podłoga | 9,2 -> 40,9 | 23,9 -> 43,7 | 25,1 -> 53,0 |
| Pasek HP / tor | 99 -> 110 | 104 -> 110 | 114 -> 76 |
| Elita (złoto) / podłoga | 74 -> 83 | 77 -> 84 | 68 -> 83 |
| Rzadkość: zwykła / rzadka / legendarna | 57 / 139 / 83 -> 51 / 134 / 85 | 62 / 150 / 90 -> 60 / 146 / 87 | 56 / 167 / 113 -> 75 / 70 / 20 |

Najsłabsza wskazówka bez filtra – czerwone pole ciosu przy protanopii (9,2) – staje się wyraźna; pozostałe zostają
rozróżnialne (dE ≥ 20), a rzadkość ma dodatkowo literę. Test rdzenia (GBA) sprawdza to samo na parach kolorów gry.

### Język angielski (#40)
- **Dane:** polski zostaje bazą w `game.json`, angielska warstwa w `GBA/data/lang/en.json`: `fields` (które pola są
  tekstem dla gracza; „?” – odwołanie po nazwie, np. slot sprzętu w nagrodzie), `data` (polski tekst → angielski; ten sam
  tekst tłumaczy się raz – 1356 tekstów), `context` (wyjątek dla pola: skrót kontraktu Bliźniak → Semi), `ui` (799
  tekstów interfejsu GBA – wcześniej na sztywno w `main.cpp`, `core.h`, `meta.h`, teraz `game.json` „ui” i w kodzie
  `UI(klucz)`), `uiGodot` (661 tekstów skryptów Godota i rdzenia C#, w kodzie `Loc.T` / `Loc.F` z {0}, {1}),
  `fit` (własne limity szerokości).
- **GBA:** teksty z danych są typu `core::ltext` (wersja polska i angielska, wybór przy każdym użyciu – zmiana języka
  działa od razu, także w telefonie). `gen_data.py` buduje tablice dwa razy – z polskich i z angielskich danych – z tymi
  samymi asercjami (długości, linie dymków, tytuły bez powtórzeń) i scala wynik. Tytuł i plansza końcowa mają angielskie
  hasło (`title_en`, `end_en`). Profil v17: język w bajcie 378 (0 = jeszcze nie wybrano – ekran wyboru przy starcie,
  1 polski, 2 angielski; migracja v16 → v17 zeruje). ROM 779 KB (było 672 KB).
- **Godot:** `Loc` (teksty interfejsu) i `LangOverlay` (dane po angielsku przy wczytaniu, 1:1 z `gen_data.py`); kopia
  `en.json` przy buildzie do `res://data/lang_en.json`. Zmiana języka w ustawieniach: interfejs od razu, nazwy i opisy
  z danych – po powrocie na tytuł (przeładowanie). `--lang pl|en` do zrzutów i testów. Tytuł z hasłem po angielsku.
- **Sprawdzenia (`gen_data.py --check`):** każdy tekst ma tłumaczenie i nie ma zbędnych wpisów; font GBA (ASCII + polskie
  litery) dla obu języków; szerokość w pikselach fontu GBA – linia angielska nie szersza niż najszersza polska w tym polu
  (tekst interfejsu: do 25% / 16 px zapasu, wyjątki w `fit` – dymki SMS do 190 px, podpowiedzi przewijane); te same {0}, {1}
  w obu językach; brak polskich napisów na sztywno w `main.cpp`, `core.h`, `meta.h` i skryptach Godota (słownik polskich
  słów z tekstów gry, bez identyfikatorów danych, logów i walidacji). Za szerokie po angielsku skrócone (np. Jack of
  Trades → Odd Jobs, Safety Training → Safety Basics, Something dropped! → Loot dropped!).
- **Testy:** core_tests i test złoty bez zmian (polski domyślny), `LanguageTests` (kompletność danych i interfejsu, te same
  {0}, dane po angielsku z tymi samymi liczbami i odwołaniami, tekst interfejsu w obu językach), test dymny Godota
  po polsku i po angielsku (`--lang en`). Monkey test po angielsku: GBA 10 seedów zwykłej gry x 20000 klatek (szczyt kafli
  122/256, sprite'ów 102/128, stosu ~7,6 KB), Godot 10 klawiatura + mysz i 10 pion + dotyk x 3000 akcji – bez błędów.
  Scenariusze testowe GBA z `-DPB_LANG=2` (wyszukiwanie po polskich nazwach niezależnie od języka).
- Karta sklepu (EN): bez zdania „gra jest na razie po polsku”.

### Zmiany
- **Profil v17** (PBRL017, 384 B bez zmian): wybrany filtr (bajt 363) i ogłoszone filtry (376–377) w dawnym wyrównaniu
  v16. Migracja: filtr klasyczny, filtry odblokowane wcześniej czekają na baner. Zapis budowy bez zmian (PBRUN16).
  Godot trzyma wybór filtra w ustawieniach urządzenia, pole w profilu tylko dla zgodności zapisu.
- Test złoty z migawki v0.21.53 (74 przebiegi; zmieniła się tylko wersja profilu w zapisie).
- Testy: core_tests 54 (odblokowanie, wybór tylko odblokowanych, baner raz, migracja v16 -> v17) i 55 (tryb każdego
  filtra, Retro 4 odcienie, Noir, pary mylonych kolorów w symulacji wady), `ScreenFiltersTests`; test dymny Godota –
  każdy filtr nad i pod telefonem, lista w ustawieniach, Wygląd przed pierwszą wygraną.
- Monkey test GBA: 20 seedów zwykłej gry + scenariusz 76 (5) x 20000 klatek – bez błędów (szczyt kafli sprite'ów
  129/256, sprite'ów 105/128, stosu ~7,5 KB). Godot: 20 seedów klawiatura + mysz i 20 pion + dotyk x 3000 akcji,
  z losowym filtrem, filtrem na telefonie i ograniczonym ruchem – bez błędów.
- Balans bez zmian (filtry to sama oprawa).

## v0.21.52 – w przygotowaniu (razem z niewydanym osobno v0.21.51; cz. a: tempo postępu, cz. b: inspektor, mistrzostwo, stopnie inwestora, cz. c: drzewko Szkoleń, kolekcje, zadania dnia, seria dni, cz. d: mapa kariery)
### Najważniejsze
Jedno duże wydanie: poprawki po graniu na iPhonie (dawne v0.21.51), sekretne zlecenia i nowy, dłuższy postęp z wieloma
celami, a na koniec mapa kariery z nowymi budynkami (GBA i Godot, wspólny rdzeń):
1. **Mur i błoto (#36, #37)** – autokafle ścian (mur wygląda jak mur, miękkie przejście w ciemność), błoto dwa razy
   rzadziej jako płaska mokra plama.
2. **Spójne sterowanie (#38)** – A / „Wybierz” zawsze po prawej, B / „Wróć” po lewej, chwila blokady po otwarciu okna,
   nieodwracalne wybory przez zaznaczenie; HUD z pełną nazwą etapu w pionie.
3. **Sekretne zlecenia (#39)** – 8 ukrytych celów („???” z podpowiedzią), za nie zawody Spawacz, Geodeta, Majster,
   Młot Zenka, Poziomica mistrza, Złota kielnia, kask w paski i ranga Respektu.
4. **Stabilność** – poprawiony crash przy skrzyni w magazynie (stos i limit kafli sprite'ów), błędy znalezione przez
   nowe testy małpy (GBA: losowe klawisze, Godot: klawiatura, mysz i dotyk) – każda część przechodzi je bez błędów.
5. **Tempo postępu (#41–#43, #52)** – Szkolenia po 4 poziomy, rosnące ceny zawodów i narzędzi, odznaki i zlecenia dają
   tytuły i kolory kasku; koniec budowy z paskami postępu. Wykupienie wszystkiego to ~25 budów (było 3–4).
6. **Poziom inspektora (#44), mistrzostwo zawodu (#45), stopnie inwestora (#48)** – nagroda co poziom (Respekt, tytuły,
   kaski, SMS-y, ozdoby Osiedla, druga pamiątka), wariant mocy, broń mistrza, premia mistrzostwa, kask mistrza.
7. **Drzewko Szkoleń (#46), kolekcje (#49), zadania dnia i tygodnia (#50), seria dni (#51)** – wybór węzłów w gałęziach
   Fach / BHP / Logistyka (np. Precyzja albo Siła rozpędu, Hartowany albo Apteczka), komplety problemów i karty bossów,
   3 + 2 zadania z daty za Respekt, nagrody za kolejne dni budowy dnia.
8. **Mapa kariery (#47, cz. d)** – po wygranych kolejne budynki z własnymi etapami, wyglądem i bossem: **Domek
   letniskowy** (krótki, z drewna), **Bliźniak** (wspólna ściana: co zostawisz w lewej połowie, przejdzie na prawą),
   **Dom z poddaszem** (silniejszy wiatr) i **Kamienica** (remont zabytku); pierwsza wygrana każdego – Respekt, tytuł
   i kolor kasku.

### Nowe
- **Szkolenia (sekcja `meta.upgrades`, pole `steps`):** poziom = działanie + przyrost + koszt (z danych, różne działania
  w jednym Szkoleniu: HP, % obrażeń, kryt, termos, materiały, jakość sprzętu, budżet). W Kosztach wiersz „Kondycja 2/4”,
  opis „Poziom III: +1 HP na start”, na maksimum „Razem: …” (Godot: też „teraz”).

  | Szkolenie | I | II | III | IV | Ceny | Razem (v0.21.51) |
  |---|---|---|---|---|---|---|
  | Kondycja | +1 HP | +1 HP | +1 HP | +1 HP | 25/50/90/150 | +4 HP (+4 HP) |
  | Szkolenie BHP | -1% otrzym. | Sprzęt +1 | -1% otrzym. | Budżet +5 zł | 30/60/110/180 | -2% (-2%) |
  | Kurs fachowy | +1% obrażeń | Sprzęt +1 | +1% obrażeń | Kryt +1% | 35/70/125/200 | +2% (+2%) |
  | Lepszy termos | Termos +1 | Kawa +1 HP | Termos +1 | Termos +1 | 25/50/90/150 | kawa +1 HP (+1 HP) |
  | Dostawy | Materiały +5% | +1 znajdźka | Budżet +5 zł | Materiały +5% | 25/50/90/150 | +1 znajdźka (+1) |
  | Kurs BHP II | Sprzęt +1 | +1 szczęścia | Sprzęt +1 | Sprzęt +1 | 30/60/110/180 | +1 SZCZ (+1) |
  | Warsztaty | Sprzęt +1 | +1 stat. broni | Sprzęt +1 | Sprzęt +1 | 35/70/125/200 | +1 stat. broni (+1) |

  „Sprzęt +1” = +1 do rzutu na jakość paczek sprzętu (jak Respekt Dobre źródła).
- **Ceny rosnące z każdym zakupem:** zawody 60 / 100 / 150 dośw. (`meta.classCosts`), narzędzia 60 / 80 / 100 / 125 / 150
  (`meta.toolCosts`, narzędzie na sprzedaż: `shop`), Trudny 100 (było 40), brygada: Pompa 50, Elektryk-kolega 60. Cena
  widoczna w Kosztach, na wyborze zawodu i w Zespole. Cały sklep: 3600 dośw. (było 460).
- **Odznaki i zlecenia:** odznaki 5–15 dośw. (razem 85, było 295), zlecenia 10–15 (razem 70, było 250). Każda daje
  **tytuł** (pole `title`, np. Bez skazy, Terminowy, Seryjny pogromca, Twarda sztuka, Deweloper, Siłacz, Stały bywalec),
  a pięć – **kolor kasku** (`cosmetic`: niebieski – Przed terminem, czarny – Twardziel, zielony – Katalog usterek,
  złoty – Osiedle, biały – Stały klient). Pamiątki ze zleceń bez zmian.
- **Tytuły:** profil > Odznaki, strona 5 **Tytuły** (A przełącza strony; GBA: SELECT wybiera, Godot: Tab / „Wybierz” albo
  drugie stuknięcie). Wybrany tytuł: GBA – w wierszu na zmianę na planszy końcowej, Godot – na Osiedlu i w podsumowaniu.
  Banery odznak i zleceń pokazują tytuł.
- **Kolor kasku:** wybór zawodu > SELECT (Godot: Tab) – strona Wygląd za modyfikatorami trybu inwestora (przed pierwszą
  wygraną – sam Wygląd), A zmienia na kolejny odblokowany. Kask w paski ma pierwszeństwo. GBA: klatki 160–183 z kaskiem
  w indeksie 10 palety i osobna paleta bohatera; Godot: te klatki z eksportu i shader podmieniający kolor (mapa, wybór
  zawodu, prolog).
- **Koniec budowy – paski postępu (#52):** GBA – strona 4/4 podsumowania „Postęp” (najbliższe Szkolenie z paskiem
  dośw./koszt albo „Stać Cię!”, Mistrzostwo i Poziom inspektora – „wkrótce”); Godot – karta POSTĘP w podsumowaniu.
- Scenariusz 69 (GBA) i sceny zrzutów `titles`, `looks`, `recap-progress` (Godot).

### Balans
Pełne Szkolenia mają tę samą moc co wcześniej plus drobne dodatki (sprzęt, budżet, termos, materiały, kryt +1%).

| Wygrane bota (300 przebiegów na zawód) | v0.21.51 | v0.21.52 |
|---|---|---|
| Łatwy | 54% | 54% |
| Normalny | 32% | 32% |
| Trudny | 10% | 10% |
| Normalny, pełne Szkolenia | 54% | 57% |
| Normalny, pełne Szkolenia + pełny Respekt | 69% | 72% |
| Normalny, pełne Szkolenia (i Respekt) + wszystkie modyfikatory | 10% | 13% |
| Normalny bez picia kawy (bez meta / pełne Szkolenia) | 23% / 39% | 23% / 38% |
| Normalny, pełne Szkolenia + pełny Respekt + Akt 0 (wszystkie nagrody) | 67% | 68% |

### Tempo postępu
Bot: kariera od pustego profilu na Normalnym (kolejne odblokowane zawody, odznaki, zlecenia i sekrety jak w grze, po
każdej budowie kupuje najtańsze), 40 karier.

| | v0.21.51 | v0.21.52 |
|---|---|---|
| Doświadczenie z budowy bez meta (średnio / wygrana / porażka) | 115 / 165 / 91 | 115 / 165 / 91 |
| Doświadczenie z odznak i zleceń (wszystkie) | 545 | 155 |
| Koszt całego sklepu | 460 | 3600 |
| Doświadczenie na budowę w karierze (z odznakami, rosnące z meta) | – | 160 |
| Budowy do wykupienia wszystkiego | 3–4 | 23 (20–27) |

### Zmiany
- **Profil v13** (200 B): wybrany tytuł i kolor kasku. Migracja v12 (i starszych): kupione poziomy Szkoleń wracają jako
  doświadczenie po starej cenie (`legacyCosts`; poziom ponad stare maksimum – `refund`), poziomy od zera; zawody,
  narzędzia, Trudny i brygada zostają. Zapis budowy bez zmian (PBRUN14) – przerwana budowa się wznowi.
- Test złoty (Godot) z migawki v0.21.52 (55 przebiegów, + tytuł, kask i ceny w profilu).
- Monkey test GBA: 20 seedów zwykłej gry + scenariusze 57, 58, 69 (po 5) x 20000 klatek – bez błędów (szczyt kafli
  sprite'ów 134/256, sprite'ów 103/128, stosu ~8,1 KB).

### Godot
- Port rdzenia (UpgradeStep, ceny, Titles, kolory kasku, MigrateV13), te same ekrany: Koszty z poziomami, Tytuły,
  Wygląd z próbką koloru kasku, karta POSTĘP, banery z tytułem; test dymny tytułów.
- Test małpy: Ustawienia ~10% akcji zamiast ~38% (klucz rzadziej w celach stuknięć, Z / Esc na tytule i mapie zwykle
  zamienione, w Ustawieniach częściej wyjście). Seria pion + dotyk (50 seedów x 3000 akcji) znalazła dwa błędy –
  poprawione: marsz po dotknięciu (AutoWalk: NullReferenceException, gdy akcja zmieniła ekran) i podgląd problemów pod B
  (wybór spoza nowej listy). Po poprawkach: pion + dotyk 28 seedów, klawiatura + mysz 40 seedów x 3000 – bez błędów.

### Cz. b – poziom inspektora, mistrzostwo zawodu, stopnie inwestora
- **Poziom inspektora (#44)** – sekcja `inspector`: dośw. z budowy = 8 za budowę + 6 za ukończony etap + 10 za bossa
  + 3 za elitę + 4 za magazyn + 25 za wygraną, x 80 / 100 / 125% (Łatwy / Normalny / Trudny); liczone na końcu budowy
  (śmierć, wygrana, porzucenie) ze znakiem wodnym w profilu – „Kolejna budowa” (NG+) dolicza tylko nowe etapy.
  35 poziomów, próg 100 + 14 x poziom (114…590, razem 12 320). Nagroda co poziom z listy w danych:

  | Nagroda | Poziomy |
  |---|---|
  | Respekt (10–40) | 1, 6, 11, 16, 21, 26, 31, 34 |
  | Tytuł (Praktykant, Stażysta, Rzeczoznawca, Inspektor, Kierownik robót, Nadzór budowlany, Główny inspektor) | 2, 8, 14, 19, 24, 29, 35 |
  | Wątek SMS od Inspektora Kowala (7 nowych, Wiadomości 26) | 3, 7, 9, 15, 20, 25, 30 |
  | Kolor kasku (czerwony, fioletowy, turkusowy, różowy, srebrny, brązowy) | 4, 13, 18, 23, 28, 33 |
  | Ozdoba Osiedla (Nowa betoniarka, Rusztowanie, Paleta cegieł, Żuraw, Piaskownica, Altana – pixel art) | 5, 12, 17, 22, 27, 32 |
  | Druga pamiątka (inna niż pierwsza, zawsze na randze I) | 10 |

  Respekt przychodzi raz, w chwili osiągnięcia poziomu; reszta działa od poziomu. GBA: pasek pod wersją na tytule, profil >
  Odznaki > strona **Inspektor** (lista 35 poziomów: Masz / postęp / Poz. N), banery „Inspektor: poziom N” z nagrodą;
  Godot: to samo (złote banery na planszy końcowej).
- **Mistrzostwo zawodu 1–10 (#45)** – sekcja `mastery`: dośw. mistrzostwa = dośw. inspektora z budowy tym zawodem; progi
  80/120/160/200/260/320/400/480/560/640 (razem 3220). Poziomy 1, 2, 4, 6, 8, 9 – Respekt (5–10), 3 – **wariant mocy**
  (dane: + siła mocy i tury odnowienia, np. Długa odprawa +1 t. ogłuszenia / +2 t. odnowienia, Mur na zakładkę +3 t.
  muru, Szybki zawór -2 HP / -4 t., Lekki taran, Szybkie tyczenie; włącza się sam, wybór zawodu > SELECT: Wygląd – wł. /
  wył.), 5 – **broń mistrza** (Kielnia mistrza itd.: kryt +2–4% tylko z bronią zawodu, złoty błysk przy krycie, nazwa
  na karcie i w rozpisce), 7 – **premia mistrzostwa** w ofercie po etapie (12 nowych premii zawodów, np. Mur oporowy +2 OBR,
  Wysokie napięcie +2 cele Łańcucha; na końcu listy – oferty bez mistrzostwa bez zmian), 10 – **kask mistrza** (złoty,
  tylko tym zawodem). Wybór zawodu: „Mistrz N” obok nazwy i pasek pod portretem; Zespół: „Mistrz N”.
- **Stopnie inwestora (#48)** – `investor.ranks`: każda nowa najwyższa stawka wygranej budowy (dowolny zawód, 1–10) daje
  nagrodę: Respekt 15/20/25/30 (stawki 1, 4, 6, 9), tytuły Ryzykant / Negocjator / Pupil inwestora / Budowa marzeń
  (2, 5, 8, 10), kask miedziany i granatowy (3, 7). Strona trybu inwestora pokazuje nagrodę za kolejny stopień.
- **Koniec budowy (#52)**: strona Postęp (GBA 4/4, Godot karta POSTĘP) – trzy paski: Szkolenie, mistrzostwo zawodu,
  inspektor („+dośw.” albo „Poziom N!”).
- Kolory kasku: 16 wyglądów (było 7; przełączniki tylko wśród pierwszych 8, kolory w `p.helmet`). Tytuły: 26 (było 15).
  Jak grać: GBA str. 16, Godot str. 9. Scenariusz 70 (GBA), sceny `inspector`, `help-progress` (Godot).

### Balans cz. b
Mistrzostwo i inspektor to drobne premie (bot nie używa mocy – wariant mocy bez wpływu).

| Wygrane bota (Normalny, 300 przebiegów na zawód) | |
|---|---|
| pełne Szkolenia + Respekt + Akt 0 (jak wyżej) | 68% |
| + mistrzostwo 10 każdym zawodem i maks. inspektor | 70% |
| ... + odznaka Bez usterek (sama: 72%) i 2. pamiątka Kask ojca (I) | 86% |
| ... z Kaskiem ojca jako pierwszą pamiątką (bez drugiej) | 84% |

Druga pamiątka nie daje więcej niż wybór najlepszej jako pierwszej (Kask ojca: +1 OBR jest dla bota bardzo mocny); cele
Łatwy 54%, Normalny 32%, Trudny 10%, Szkolenia 57%, + Respekt 72%, modyfikatory 13%, Akt 0 68% – bez zmian.

### Tempo cz. b
Bot: kariera od pustego profilu (40 karier x 300 budów, jak tabela tempa wyżej).

| | |
|---|---|
| Dośw. inspektora na budowę (średnio) | 143 |
| Maksymalny poziom inspektora (35) | po 88 budowach (79–98) |
| Poziom inspektora po wykupieniu Szkoleń (~23 budowy) | 13 |
| Mistrzostwo 3 / 5 / 7 / 10 (budowy tym zawodem) | 3 / 6 / 11 / 22 |

### Zmiany cz. b
- **Profil v14** (240 B, zapis budowy dalej od 256): dośw. inspektora (uint32 @200), mistrzostwo 12 zawodów (uint16 @204),
  włączone warianty mocy (@228), znak wodny postępu budowy (@230), druga pamiątka (@232), 7 B zapasu. Migracja v13
  (i starszych): inspektor = 20 x budowy + 60 x wygrane + 50% Respektu łącznie, mistrzostwo = 120 x dom zawodu na
  Osiedlu + 60 za wygraną zawodem; Respekt za osiągnięte poziomy, wątki SMS inspektora od razu.
- Zapis budowy GBA bez zmian (pole mistrzostwa w miejscu wyrównania `run_mods` – stara przerwana budowa się wznowi);
  Godot: nowy rozmiar stanu, stara przerwana budowa przepada.
- GBA: klatka między scenami (ekran wygaszony) – kafle sprite'ów poprzedniej sceny zwalniają się przed rysowaniem nowej
  (monkey test: tytuł z paskiem inspektora i pierwsza karta wyboru zawodu = brak VRAM).
- Monkey test GBA: 20 seedów zwykłej gry + scenariusze 58, 69, 70 (po 6) x 20000 klatek – bez błędów (szczyt kafli
  sprite'ów 129/256, sprite'ów 105/128, stosu ~8,1 KB).
- Test złoty: 62 przebiegi (+ mistrzostwo z bota „smart” – Odprawa, Ścianka, Zawór, Taran, Tyczenie, Złota rączka –
  i druga pamiątka).

### Godot cz. b
- Port rdzenia (`Progress`, `ProgressGain`, `MasteryBit`, `Game.Mastery`, profil v14 `MigrateV14`), `InspectorMasteryTests`;
  `dotnet test` 238/238, build 0 ostrzeżeń.
- Te same ekrany: pasek inspektora na tytule, „Mistrz N” z paskiem na karcie zawodu, strona Wygląd / Tryb inwestora
  z wariantem mocy, drugą pamiątką i kolejnym stopniem, karta POSTĘP z trzema paskami, złote banery nowych poziomów
  (wątki fabuły zbiorczo, gdy więcej niż 2), Odznaki > Inspektor, „Mistrz N” w Zespole, ozdoby z inspektora na Osiedlu,
  Jak grać str. 9; sceny zrzutów `inspector`, `help-progress`; test dymny (9 stron Jak grać, Tytuły -> Inspektor).
- Test małpy: klawiatura + mysz 20 seedów i pion + dotyk 20 seedów x 3000 akcji – bez błędów.

### Cz. c – drzewko Szkoleń, kolekcje, zadania dnia i tygodnia, seria dni
- **Drzewko Szkoleń (#46, `meta.tree`)** – 7 Szkoleń to pień trzech gałęzi; w gałęzi dwa węzły otwierane po N poziomach
  pnia gałęzi, w węźle wybór 1 z 2 (pierwszy wybór 60 / 120 dośw., zmiana na drugą opcję 20 dośw. – pozostaje wybrana
  jedna). Węzły w danych (`nodes`: gałąź, głębokość, koszt, 2 opcje: nazwa, krótka nazwa na GBA, działanie, wartość):

  | Gałąź (pień) | Węzeł I | Węzeł II |
  |---|---|---|
  | Fach (Kurs fachowy, Warsztaty) | pień 3, 60: **Precyzja** kryt +1% / **Siła rozpędu** pierwszy cios w nietknięty problem +1 | pień 6, 120: **Rzemieślnik** +2% obrażeń / **Szybka ręka** moc -1 t. |
  | BHP (Szkolenie BHP, Kurs BHP II, Kondycja) | pień 4, 60: **Hartowany** -2% otrzymanych / **Apteczka** kawa +1 HP | pień 9, 120: **Refleks** unik +1% / **Zapas sił** +1 HP |
  | Logistyka (Lepszy termos, Dostawy) | pień 3, 60: **Hurtownik** Hurtownia -10% / **Magazynier** materiały +10% | pień 6, 120: **Księgowa** +15 zł / **Brygadzista** brygada -15% |

  Ta sama moc co wcześniej: z pnia zeszły kawa (Lepszy termos II), kryt (Kurs fachowy IV), +1 stat. broni (Warsztaty II)
  i 1 HP (Kondycja III) – w ich miejscu „Sprzęt +1”; wróciły jako opcje węzłów (stat. broni był w długiej budowie
  z Aktem 0 za mocny – zamiast niego Rzemieślnik +2% obrażeń). Lepszy termos: termos +3; Warsztaty: same „Sprzęt +1”.
  Siła rozpędu: premia w bitach 8-11 `run_mods::mastery` (zapis budowy bez zmiany rozmiaru). GBA: Koszty > SELECT:
  **Drzewko** (3 kolumny, wybrana opcja zielona, zaznaczona fioletowa, zamknięta szara, pastylka: koszt / zmiana / pień N;
  strzałki po węzłach, L/R zakładki, A wybiera); Godot: Koszty > Tab: Drzewko (kolumny z pniem i paskiem, opis z pniem
  gałęzi; strzałki / stuknięcie, Spacja / drugie stuknięcie wybiera).
- **Kolekcje (#49, `collections`)** – profil liczy pokonanych każdego rodzaju (do 255; znak wodny `kill_mark` jak liczniki
  zleceń – wznowienie budowy, NG+ i bank na końcu etapu bez podwójnego liczenia). Komplety:

  | Komplet | Warunek | Nagroda |
  |---|---|---|
  | Stan surowy | każdy problem aktu I x10 (13 rodzajów) | +10 zł na start (stała premia) |
  | Pod dachem | każdy problem aktu II x10 | tytuł Łowca usterek |
  | Wykończenie | każdy problem aktu III x10 | ceglasty kask |
  | Papierologia | każdy problem Aktu 0 x10 | tytuł Urzędnik |
  | Karty bossów | każdy z 5 bossów pokonany | +5% doświadczenia (stała premia) |
  | Album Osiedla | wszystkie 12 ozdób | tytuł Architekt |

  Telefon profilu > Katalog (licznik „x12” zamiast „ZAMKNIĘTA”) > A (Godot: Spacja): **Kolekcje** (postęp, nagroda;
  Godot: najbliższy brakujący problem), **Bossowie** (karta z portretem i licznikiem), **Album** (ozdoba: stoi / skąd).
  Baner „Komplet: …” raz (bity `collections` w profilu), na końcu etapu albo budowy.
- **Zadania dnia i tygodnia (#50, `tasks`)** – 3 zadania z puli dnia (10) i 2 z puli tygodnia (6), wybór z seeda numeru
  dnia / tygodnia (te same dla wszystkich; GBA: data budowy dnia, Godot: data z systemu). Postęp z każdej budowy (też
  budowy dnia i tygodnia; znak wodny `task_mark`, bank na końcu etapu, budowy i przy porzuceniu), nowy dzień / tydzień –
  od zera. Wykonane: Respekt od razu (dnia 4–5, tygodnia 12–20); za 5 / 15 / 40 wykonanych: Respekt +15, limonkowy kask,
  tytuł Pracowity. Liczniki: problemy, elity, bossowie, etapy, wezwania brygady, moce, kawy, kombinacje, magazyny,
  SMS-y, wygrana, wygrana bez zakupów w Hurtowni (nowe liczniki budowy w miejscu wyrównania stanu – rozmiar bez zmian).
  Odznaki > **Zadania** (A na GBA; postęp, „Gotowe”, nagroda i seria dni w opisie), tytuł „Zadania 1/3”, w budowie
  telefon > Koszty – najbliższe zadanie z postępem na żywo; banery „Zadanie dnia +5 Respektu” (kilka – zbiorczo)
  i „Zadania: 5 wykonanych”.
- **Seria dni (#51, `daily.streak`)** – budowa dnia w kolejne dni: liczy się tylko dzień zaraz po ostatnim (GBA: data
  wpisana ręcznie – ten sam dzień albo wstecz nic nie zmienia, przerwa = od nowa). Za najdłuższą serię: 3 dni – pamiątka
  **Kalendarz majstra** (kawa +1/2/3 HP), 7 – oranżowy kask, 14 – tytuł Niezawodny. Tytuł: „Seria N dni”, opis w Zadaniach.
- **Kask ojca** – -5/8/10% otrzymanych obrażeń (ranga I/II/III) zamiast +1/2/3 OBR (jako jedyna pamiątka na pełnym meta
  dawał +12 pkt wygranych, teraz +4 – porównywalnie z innymi).
- Tytuły z kolekcji, serii dni i zadań (Tytuły: skąd tytuł), pamiątka z serii (Pamiątki: „Seria 3 dni budowy dnia”).
  Jak grać: GBA str. 17 „Cele”, Godot str. 10 (sekcja `goalsHelp`). Scenariusz 71 (GBA), sceny zrzutów Godota (niżej).

### Balans cz. c
Bot: 300 przebiegów na zawód (9 zwykłych), Normalny.

| Wygrane bota | v0.21.52 cz. b | cz. c |
|---|---|---|
| Łatwy / Normalny / Trudny (bez meta) | 54% / 32% / 10% | 54% / 32% / 10% |
| Pełne Szkolenia (cz. c: pień + najlepsze wybory drzewka; sam pień) | 57% | 58% (pień 50%) |
| + pełny Respekt | 72% | 73% |
| + wszystkie modyfikatory trybu inwestora | 13% | 12% |
| Pełne meta z Aktem 0 | 68% | 67% |
| + mistrzostwo 10 i maks. inspektor | 70% | 70% |

Opcje drzewka osobno (sam pień 50,5%; Szybka ręka, Hurtownik, Księgowa, Brygadzista – bez wpływu na bota, który nie
używa mocy i nie kupuje w Hurtowni): Precyzja 50,3%, Siła rozpędu 52,2%, Rzemieślnik 52,1%, Hartowany 52,3%, Apteczka
53,7%, Refleks 50,1%, Zapas sił 51,0%, Magazynier 50,7%. „Najlepsze wybory” w tabeli = Siła rozpędu, Rzemieślnik,
Apteczka, Zapas sił, Magazynier, Księgowa.

| Pamiątka jako jedyna (pełne meta + mistrzostwo i inspektor, 100 przebiegów na zawód) | ranga I | ranga III |
|---|---|---|
| bez pamiątki | 72% | |
| Termos babci | 72% | 72% |
| Kask ojca (cz. b: +1 / +3 OBR – ok. 84–87%) | 76% | 80% |
| Szczęśliwa kielnia | 79% | 85% |
| Stara poziomica | 72% | 72% |
| Notes kierownika | 72% | 72% |
| Kalendarz majstra (nowa) | 74% | 77% |

Poziomica i Notes nie działają na bota (widzenie, moc). Szczęśliwa kielnia (+2 szczęścia) jest teraz najmocniejsza.

### Tempo cz. c
Kariera bota jak wyżej (kupuje też węzły drzewka, opcje na przemian): cały sklep 4140 dośw. (było 3600), wszystko
wykupione po **25 budowach** (22–31; było 23), dośw. na budowę w karierze 188; inspektor maks. po 90 budowach (81–103).

### Zmiany cz. c
- **Profil v15** (384 B): wybór w drzewku (@240, 2 bity na węzeł), liczniki kolekcji i znak wodny (@242, @290, po 48 B),
  zadania (dzień, tydzień, postęp i znak wodny 5 zadań, wykonane, łącznie), seria (dni, ostatni dzień, rekord), ogłoszone
  komplety, 24 B zapasu. Migracja v14 (i starszych): drzewko puste, kolekcje – rodzaje z Katalogu jako 1 pokonany, seria
  z wyników ostatnich dni budowy dnia (kolejne dni do najnowszego), komplety już osiągnięte bez banera.
- **Zapis budowy GBA od bajtu 512** (było 256 – profil nie mieści się przed 256). Przy migracji profilu warstwa GBA najpierw
  przenosi przerwaną budowę spod 256 pod 512, dopiero potem zapisuje profil (wyłączenie konsoli w trakcie: zostaje stary
  profil i stara budowa). Zapis PBRUN15: nowe liczniki zadań w miejscu wyrównania – rozmiar stanu bez zmian, więc PBRUN14
  się wczytuje (liczniki od zera) – przerwana budowa z cz. b się wznowi.
- Test złoty: 66 przebiegów (+ drzewko z Siłą rozpędu i wyborami, zadania z bankiem na końcu etapu, seria dni z budowy dnia).
- Monkey test GBA: 20 seedów zwykłej gry + scenariusz 71 (10 seedów) x 20000 klatek – bez błędów (szczyt kafli sprite'ów
  131/256, sprite'ów 112/128, stosu ~8,2 KB).

### Godot cz. c
- Port rdzenia (`SkillTree`, `CollectionBook`, `DailyTasks`, `DayStreak`, `Goals.MigrateV15`, `Game.HelpersCalled` /
  `ShopBuys`, `RunMods.FirstHitBonus`), `GoalsTests`; `dotnet test` 254/254, build 0 ostrzeżeń. Zapis budowy z cz. b
  (o 2 bajty krótszy) wczytuje się z zerami w nowych licznikach.
- Te same ekrany: Koszty > Drzewko, Katalog > Kolekcje / Bossowie / Album, Odznaki > Zadania, tytuł z zadaniami i serią,
  telefon w budowie > Koszty – zadanie dnia, złote banery celów (koniec etapu i plansza końcowa), Jak grać str. 10;
  sceny zrzutów `title-goals`, `tree`, `tree-locked`, `collections`, `bosses`, `album`, `tasks`, `phone-goals`,
  `goals-end`, `help-goals`; test dymny (drzewko: zamknięty węzeł, wybór, zmiana za opłatą; cele; 10 stron Jak grać).
- Test małpy: klawiatura + mysz 20 seedów i pion + dotyk 20 seedów x 3000 akcji – bez błędów.

### Cz. d – mapa kariery (#47)
- **Kontrakty** (sekcja `career` w `data/game.json`): budynek z własną listą etapów (etap jak w Domu + `look` – paleta,
  `tiles` – zestaw kafli, `twin` – druga połowa bliźniaka, `like` – pogoda jak na etapie Domu, `story` – SMS na start),
  warunkiem odblokowania i nagrodą za pierwszą wygraną. W rdzeniu etap budowy to etap kontraktu (`sdef`, `route_count`,
  `prelude_count`, `stage_id`); Dom jednorodzinny bez zmian (te same indeksy, test złoty starych przebiegów bez różnic).

  | Kontrakt | Odblokowanie | Etapy (akty) | Nowy boss | Co inaczej | Nagroda za 1. wygraną |
  |---|---|---|---|---|---|
  | Dom jednorodzinny | od początku | 10 + 2 Aktu 0 (0, I–III) | – | – | – |
  | Domek letniskowy | 1 wygrana | 6 (I, II) | **Zawilgocony strych** (pleśń, kapanie, wzywa Pleśń) | drewno: kafle pokładu i bali, paleta sosny; lżejsze problemy, Betoniarka przy tarasie | Respekt 20, tytuł Letnik, Sosnowy kask |
  | Bliźniak | 3 wygrane | 10 (I–III) | **Pęknięta dylatacja** (krzyż „Rozwarcie”, druga faza „Druga połowa”, wzywa Rysy) | pary etapów lewa / prawa: **wspólna ściana** – prawa połowa ma pogodę i wydarzenie na placu lewej, a do 3 niedokończonych problemów przechodzi (baner „Wspólna ściana”) | Respekt 30, tytuł Dobry sąsiad |
  | Dom z poddaszem | inspektor 8 | 12 (I–III) | **Zerwana połać** („Podmuch”, poślizg, wzywa Przeciąg) | ścianki kolankowe, więźba, okna dachowe, ocieplenie; **porywy co 4 tury** (zamiast 6); Inspekcja i Termin | Respekt 30, tytuł Pod samym dachem, Grafitowy kask |
  | Kamienica | inspektor 12 | 10 (I–III) | **Pęknięty strop** (krzyż „Osypisko tynku”, faza „Podpory”, wzywa Grzyb) | remont zabytku: nowe problemy **Grzyb domowy** (leczy innych, zatrucie) i **Stara instalacja** (prąd z dystansu), Decyzja odmowna konserwatora; kafle kamienicy (parkiet w jodełkę, stary mur z gzymsem) | Respekt 40, tytuł Konserwator, Kremowy kask |

- **Mapa kariery** (GBA: po pierwszej budowie tytuł > A / START; Godot: Nowa budowa): kontrakty z ikoną bossa
  (zablokowane – kłódka / sylwetka i warunek), pastylka „Wygrane N” / „Etapy N/M” (najlepszy wynik) / „3 wygrane” /
  „Inspektor 8”, opis, liczba etapów, boss i nagroda. A / Spacja wybiera i przechodzi do wyboru zawodu, B z wyboru
  zawodu wraca do mapy. Budowa dnia i tygodnia zawsze na Domu jednorodzinnym. Karta etapu z nazwą kontraktu („Bliźniak,
  Akt II, 5/10”), baner „Wspólna ściana: +N z 1. połowy”, porywy „co 4 tury”; na końcu budowy złote banery „Wygrany
  kontrakt” z nagrodą i „Nowy kontrakt!”.
- 6 nowych rodzajów problemów (47/48 w Katalogu): 2 problemy i 4 bossów, pixel art klatki 184–195 (Godot: z chodem
  i oddechem); 10 nowych palet etapów (22 wyglądy), zestawy kafli 5 (drewno) i 6 (kamienica); Godot: kafle 32 px
  z nowymi rodzajami (parkiet, bale, kamień, sztukateria).
- **Fabuła:** 4 wątki SMS po pokonaniu bossa kontraktu (Letnisko, Sąsiedzi, Pokój pod dachem, Zabytek; 30/32).
- **Kolekcje:** nowy komplet „Bossowie kariery” (4 nowi bossowie – tytuł Budowniczy); „Karty bossów” to bossowie Domu
  (jak dotąd 5). Kolory kasku: 22. Tytuły z kontraktów w Odznaki > Tytuły.
- Sekretne zlecenia z wygraną (bez kawy, na styk, szybka ekipa) liczą się tylko w pełnym budynku (co najmniej 10 etapów –
  nie w krótkim Domku letniskowym).
- Jak grać: GBA str. 18 „Kariera”, Godot str. 11 (sekcja `careerHelp`). Scenariusze GBA 72–75, sceny zrzutów Godota
  (`career`, `career-locked`, `career-letnisko`, `career-blizniak`, `career-poddasze`, `career-kamienica`, `career-end`,
  `help-career`).
- Węzeł drzewka Szkoleń „Twardziel” nazywa się teraz **„Hartowany”** (nie myli się z odznaką Twardziel).

### Balans cz. d
Bot: 300 przebiegów na zawód (9 zwykłych), Normalny; „pełne meta” = pełne Szkolenia z najlepszymi wyborami drzewka,
pełny Respekt i wszystkie nagrody za odbiór (Dom: z Aktem 0).

| Kontrakt | Etapy | Bez meta | Pełne meta | + mistrzostwo 10 i maks. inspektor |
|---|---|---|---|---|
| Dom jednorodzinny | 10 (+2) | 33% | 68% | 71% |
| Domek letniskowy (krótki – łatwiejszy) | 6 | 35% | 75% | 77% |
| Bliźniak | 10 | 28% | 68% | 70% |
| Dom z poddaszem | 12 | 27% | 67% | 70% |
| Kamienica | 10 | 26% | 70% | 71% |

Dom bez zmian (Łatwy 54%, Normalny 32%, Trudny 10%, Szkolenia 58%, + Respekt 73%, modyfikatory 12%, Akt 0 67%,
mistrzostwo i inspektor 70%). Kontrakty strojone mnożnikiem HP i premią obrażeń etapów (+1 obrażeń problemów to
~20 pkt wygranych); w teście `core_tests` asercje 20–42% (Domek do 50%) bez meta i 55–82% z pełnym meta.

### Zmiany cz. d
- **Profil v16** (384 B): wybrany kontrakt (@360), ogłoszone odblokowania (@361), wygrane kontrakty (@362), wygrane
  w każdym kontrakcie (@364, 6 B), najlepszy etap (@370, 6 B), 8 B zapasu. Migracja v15 (i starszych): Dom jednorodzinny
  wygrany, jeśli były wygrane (liczba wygranych, najlepszy etap – cała budowa); już odblokowane kontrakty dostają baner
  jak nowe.
- **Zapis budowy PBRUN16:** kontrakt i problemy z 1. połowy bliźniaka w miejscu wyrównania stanu (rozmiar bez zmian) –
  PBRUN14 / PBRUN15 wczytują się jako Dom jednorodzinny. Godot: dwa bajty na końcu stanu (`RunSave.V15Tail`), stary zapis
  się wczytuje.
- Tablice etapów stanu budowy dalej po 12 (najdłuższy kontrakt ma 12 etapów); `data::stages` ma 50 etapów (Dom pierwszy),
  maska pogody 64-bitowa; klatka problemu w danych `int16`.
- Test złoty: 74 przebiegi (+ każdy kontrakt botem z testów i „smart”, NG+ w Domku). `core_tests`: trasy kontraktów
  (skrót przez cały budynek, Hurtownia przy zmianie aktu), bliźniak, porywy, odblokowanie, nagroda raz, kolekcje bossów,
  migracja v15 → v16, PBRUN15 → 16, balans kontraktów.
- Monkey test GBA: 20 seedów zwykłej gry + scenariusze 72–75 (po 5) x 20000 klatek – bez błędów (szczyt kafli sprite'ów
  134/256, sprite'ów 106/128, stosu ~8,2 KB).

### Godot cz. d
- Port rdzenia (`CareerDef`, `CareerUnlock`, `GameData.Career` / `StagesCount`, `Game.Career`, `Career`, profil v16,
  `MigrateV16`), `CareerTests`; `dotnet test` 268/268, build 0 ostrzeżeń.
- Ekrany: Mapa kariery (`CareerScreen`, `CareerPage`), karta etapu, banery, Jak grać str. 11; kafle wg wyglądu etapu
  (`MapLayer`), eksport klatek i kafli kontraktów; test dymny mapy kariery (zablokowany kontrakt, wybór, powrót z wyboru
  zawodu, budowa w kontrakcie, nagroda raz).
- Test małpy: klawiatura + mysz 20 seedów i pion + dotyk 20 seedów x 3000 akcji – bez błędów (mapa kariery w 19 z 20 przebiegów).

### Dawne v0.21.51 (nie wydane osobno – w tym wydaniu): poprawki po graniu na iPhonie, sekretne zlecenia

#### Nowe
- **Autokafle ścian (#36):** widok 3/4 ze światłem z lewej góry. Pole muru z murem poniżej to ciemny **wierzch masy
  muru** (bez pasów i fug), pole z podłogą poniżej to **lico** – krawędź wierzchu u góry i wzór materiału (bloczki,
  papa, cegła, deski, płytki) z cieniem przy podłodze. Godot: kafle generowane w eksporcie dla palety każdego etapu
  (`export_godot_assets.py`: wierzch, lico i 6 nakładek – jasna krawędź od podłogi u góry i z lewej, ciemna z prawej,
  końce lica, róg wewnętrzny), dobierane w `MapLayer` wg sąsiadów; ciemne palety mają wierzch rozjaśniony, żeby nie
  zlewał się z tłem. Skraj odkrytej części etapu gaśnie miękko w ciemność (mgła 4 teksele na pole, zanikanie ~pół
  pola, także po skosie) zamiast twardych schodków. GBA (wersja lżejsza): 12 kafli na zestaw – nowy wierzch masy muru
  i dolna połowa lica; górna połowa lica ma jedną jasną krawędź zamiast dwóch pasków na polu.
- **Błoto (#37):** mechanika aktu I co 14. pole zamiast co 7. (`acts[0].mechanic.value` w `data/game.json` – gęstość
  z danych). Wygląd: płaska mokra plama o nierównym brzegu – jaśniejszy brąz z połyskiem, mniejsza niż pole (Godot:
  3 warianty, odbicia; GBA: nowy kafel i kolory 12-13 palet etapów).
- **Spójne sterowanie (#38):** w każdym oknie i przejściu (premia po etapie, Hurtownia, SMS z wyborem, harmonogram
  i wybór ścieżki, paczka sprzętu, zamiana narzędzia, cecha narzędzia, karta etapu, podsumowanie, harmonogram domu)
  A = wybierz / dalej, B = wróć / zostaw. Godot: Spacja i Enter = A, Z i Esc = B (Hurtownia: Enter kupuje zaznaczone,
  Z/Esc – dalej; paczka: Enter zakłada; harmonogram domu: Enter – dalej, Tab / dotknięcie linku – planbudowlany.online);
  na dotyku główny przycisk („Wybierz”, „Biorę”, „Dalej”, „Kup”) zawsze w prawym dolnym rogu w fiolecie, powrót
  („Wróć”, „Zostaw”, „Zostaję”) po lewej, sam jeden przycisk zajmuje prawą połowę (rola przycisku w `PageAction`).
- **Blokada wejścia:** Godot – 0,4 s po otwarciu okna i dopóki telefon wjeżdża wciśnięcia i dotknięcia są ignorowane
  (stuknięcie w mapę tuż przed końcem etapu nie wybiera premii, nie pomija SMS-a); GBA – ~20 klatek po otwarciu okna
  (i po zmianie strony SMS-a), A liczy się dopiero wciśnięte na nowo.
- **Nieodwracalne wybory przez zaznaczenie:** premia, zakup, odpowiedź na SMS, cecha – strzałka / pierwsze dotknięcie
  zaznacza, A / „Wybierz” / drugie dotknięcie tego samego zatwierdza. **Zamiana ulepszonego narzędzia** ma teraz dwa
  wiersze: zaznaczone na start „Zostaję: Kielnia+2”, strzałka – „Zamieniam na …”, A zatwierdza, B zostaje (A z rozpędu
  nie zabiera ulepszenia).
- „Jak grać”: GBA – nowa strona 2 „Okna i wybory” (14 stron); Godot – wiersz „Okna” w sterowaniu dotykiem i zasada na
  stronie 1.
- **HUD (Godot, pion):** gdy „Etap 2/10: Izolacja fundamentów” nie mieści się w pierwszym rzędzie, jest „Etap 2/10,
  Normalny”, a pełna nazwa etapu w drugim rzędzie po prawej (mierzona po ikonach; pastylka wydarzenia tylko, gdy
  starczy miejsca, ostrzeżenie bossa skraca się do „Cios za N!”).

#### Nowe – cz. 2: sekretne zlecenia (#39)
- **8 sekretnych zleceń** (sekcja `secrets` w `data/game.json`): w profilu (Odznaki > A na GBA, Spacja w Godocie) strona
  **Sekrety** – niewykonane jako „???” z podpowiedzią, wykonane z warunkiem i nagrodą (ikona / portret). Sprawdzane po
  etapie, po porzuceniu i na końcu budowy; baner „Sekretne zlecenie!” w złotej ramce (kolejka końca budowy do 8 banerów:
  nagroda, sekrety, odznaki, zlecenia, fabuła) i dymek „Nowość” na tytule z treścią sekretu.

  | Podpowiedź | Warunek | Nagroda |
  |---|---|---|
  | Bez kofeiny też się da | wygraj bez picia kawy | zawód **Spawacz** |
  | Szef tylko dzwoni | Termin pokonany przez brygadę | narzędzie **Młot Zenka** |
  | Szczur magazynowy | 5 magazynów w serii budów | narzędzie **Poziomica mistrza** |
  | Każdy fach się przyda | wygraj każdym z 9 zawodów | zawód **Majster** |
  | Papierologia? Nie tym razem | Akt 0 bez ciosu od papierów | zawód **Geodeta** |
  | Mokra robota | 20x mokry + prąd w jednej budowie | wygląd **Złota kielnia** |
  | Na styk | wygraj z 1–3 HP | wygląd **Kask w paski** |
  | Szybka ekipa | wygraj w 150 dni (bez Aktu 0) | Respekt **Zaprawiony w boju** |

- **Nowe zawody:** **Spawacz** (Spawarka, iskra; moc **Spaw** – iskry w linii i dym), **Geodeta** (Tyczka geodezyjna;
  widzi cały plac od startu etapu, moc **Tyczenie** – oznaczony cel dostaje mocniejsze ciosy i jest ogłuszony, znak
  nad celem), **Majster** „złota rączka” (Młotek; moc **Złota rączka** – co etap moc innego fachu, ikona w HUD).
  Na wyborze zawodu i w Zespole zablokowane jako „Sekret” z podpowiedzią. Pixel art klatek 127–135 (z chodem
  i oddechem), sylwetki.
- **Bronie:** **Młot Zenka** (4–7, cios odpycha problem) i **Poziomica mistrza** (2–5, zasięg 3, kryt +10%, magazyny
  i skrzynie widać na podglądzie mapy) – w dropach dopiero po sekrecie, poza Szkoleniami.
- **Wygląd:** **Złota kielnia** – złoty błysk broni przy krycie; **Kask w paski** – biało-czerwony kask bohatera
  i portretów 12 zawodów (klatki 136–159), włączany na wyborze zawodu (SELECT: Wygląd, Godot: Tryb inwestora).
- **Respekt Zaprawiony w boju** – kawa w termosie na start budowy; na liście Respektu zablokowany do sekretu.
- Liczniki budowy dla sekretów (wypite kawy, mokry + prąd, ciosy papierów w Akcie 0, boss od brygady). Odznaka Pełny
  zespół, zlecenia i budowa dnia liczą tylko 9 zwykłych zawodów; sekretne narzędzia nie liczą się do Kolekcjonera.
- **Profil v12** (196 B): sekrety, nieogłoszone sekrety, wygląd, Respekt 16–18. Migracja z v11 – sekret „Każdy fach”
  od razu, jeśli w profilu są już wygrane każdym zawodem (dymek jak przy nowym). Zapis budowy **PBRUN14** (nowe liczniki
  i zawody) – przerwana budowa z v0.21.50 / v0.21.51 cz. 1 się nie wznowi.
- „Jak grać”: GBA strona 15 „Sekrety”, Godot strona 8 (sekcja `secretsHelp`). Scenariusze 61–67 (dymek Nowość,
  strona Sekrety, nowe zawody, moc Majstra, kask w paski, Poziomica na mapie, 1 HP + awans).
- Balans nowych zawodów (bot, Normalny, bez meta): Spawacz 35%, Geodeta 36%, Majster 23%.

#### Balans
Rzadsze błoto ledwie rusza bota (omija je i tak): wyniki w granicach szumu.

| Wygrane bota (300 przebiegów na zawód) | v0.21.50 | v0.21.51 |
|---|---|---|
| Łatwy | 53% | 54% |
| Normalny | 32% | 32% |
| Trudny | 10% | 10% |
| Normalny, pełne Szkolenia | 54% | 54% |
| Normalny, pełne Szkolenia + pełny Respekt | 70% | 69% |
| Normalny, pełne Szkolenia (i Respekt) + wszystkie modyfikatory | 9% | 10% |
| Normalny bez picia kawy (bez meta / pełne Szkolenia) | 22% / 40% | 23% / 39% |
| Normalny, pełne Szkolenia + pełny Respekt + Akt 0 (wszystkie nagrody) | 66% | 67% |

Wyzwania tygodnia: Glazurnik bez kawy 10%, Elity x2 30%, Mokry tydzień 36%, Bez Hurtowni 17%, Szklany kask 29%,
Kierownik na placu 32%.

#### Poprawki
- **Crash przy otwieraniu skrzyni w magazynie** („No more sprite tiles items available” na Miyoo): dwie przyczyny.
  1. **Przepełniony stos.** Stos GBA (IWRAM, ~12 KB) był w samej grze zajęty w 97% (scena gry 7,8 KB, od kolejki
     8 banerów jeszcze więcej). Okno porównania sprzętu ze skrzyni/paczki przy zajętym slocie albo telefon → Start →
     A (statystyki) przepełniały go i nadpisywały dane w IWRAM (tablice nazw, stan dźwięku) – zawieszenie albo błąd.
     Duże obiekty sceny (napisy HUD, baner z kolejką, cząsteczki, liczby obrażeń, rozpiska obrażeń) są teraz na
     stercie: szczyt stosu ~8 KB.
  2. **Limit pozycji kafli sprite'ów.** Butano liczy w limicie 128 także wolne kawałki VRAM i kafle do zwolnienia –
     przy pełnym HUD (stany, premie, elity, materiały) skrzynia z banerami dochodziła do 132. Limit 256.
  Scenariusz 68: skrzynia przy pełnym HUD i zajętych slotach sprzętu.
- **Znalezione przez monkey test** (nowy bot losowych klawiszy, `tools/playtest/monkey.sh`):
  - wybór zawodu: L/R razem z lewo/prawo (albo START) w tej samej klatce – brak VRAM na sprite'y (błąd Butano);
  - telefon: zmiana zakładki + START, góra/dół + B – podwójne przerysowanie w jednej klatce (to samo ryzyko);
  - ekran końcowy w trybie inwestora: stawka jako 6. wiersz motywacji – „Vector is full”;
  - ciężka walka (wiele trafień, kryty, awans, banery, cząsteczki) dochodziła do 128 sprite'ów – liczby obrażeń,
    napis awansu i cząsteczki pojawiają się tylko przy zapasie sprite'ów i VRAM.
- Monkey test: losowe klawisze (d-pad, A, B, START, SELECT, L, R, przytrzymania, pary, L+R+SELECT) od pustego
  profilu i ze scenariuszy, wykrywa ekran błędu, zawieszenie i zastygły obraz; buildy testowe raportują zapas
  sprite'ów, kafli i stosu. Opis w README.

#### Zmiany
- Cz. 1 nie zmieniała zapisów; cz. 2: profil v12 z migracją, zapis budowy PBRUN14 (patrz wyżej).
- Test złoty (Godot) z migawki v0.21.51 cz. 2 (55 przebiegów).


#### Godot (cz. 2: sekretne zlecenia, mgła, test małpy)
- **Sekretne zlecenia (#39) na ekranie:** profil > Odznaki – strona Sekrety („???” z podpowiedzią, po wykonaniu warunek
  i nagroda z ikoną), sprawdzanie po etapie / porzuceniu / końcu budowy, baner „Sekretne zlecenie!” (złota ramka, kolejka
  8 banerów), dymek Nowość; Spawacz, Geodeta, Majster na wyborze zawodu (moc Majstra w HUD), kask w paski (Tryb inwestora),
  złoty błysk przy krycie, Poziomica mistrza na podglądzie mapy, Respekt Zaprawiony w boju zablokowany; klatki 127–159.
- **Mgła pikselowa:** skraj odkrytej części gaśnie w 3 stopniach kraty Bayera w pikselach grafiki zamiast rozmycia.
- **Test małpy** `--monkey SEED KROKI`: losowe klawisze i dotyk przez prawdziwe wejście, wykrywa wyjątki, błędy logu
  i zawieszenia; 50 seedów x 3000 akcji (klawiatura + mysz) bez błędów. Znalezione: profil pokazowy z Warsztatami 2/1
  (wyjątek w Kosztach), brak sprawdzania sekretów w sesji, sekretne narzędzia w Szkoleniach, ikona nagrody Pistoletu.

## v0.21.50 – 2026-09-29
### Najważniejsze
Regrywalność – 10 nowości w jednym wydaniu (GBA i Godot, wspólny rdzeń i test złoty):
1. **Rozpiska obrażeń broni (#26)** – cios od-do, kryt i skąd się biorą, porównanie przy zmianie sprzętu, karta problemu.
2. **Premia 1 z 3 po etapie (#27)** – 41 premii, rzadkość, znaczniki, 8 synergii, premie zawodów.
3. **Elity (#28)** – złota ramka, przedrostek nazwy, jedna z 5 cech, lepsza nagroda.
4. **Kombinacje stanów (#29)** – mokry + prąd, pył + iskra, zamróz + uderzenie.
5. **Wydarzenia z wyborem (#30)** – 12 SMS-ów na placu, ryzyko albo zysk.
6. **Ulepszanie narzędzia (#31)** – Hurtownia do +3, od +2 cecha (Przebicie, Ostrze, Wyważenie).
7. **Ukryte pomieszczenia (#32)** – magazyn za pękniętą ścianą albo drzwiami, klucz, strażnik, skrzynia.
8. **Podsumowanie budowy (#33)** – co zatrzymało budowę, ostatnie ciosy, oś czasu, nagrody, najbliższy cel i rada.
9. **Wyzwania tygodnia (#34)** – seed tygodnia i zasady z danych, osobne wyniki, miejsce na tabelę tygodnia.
10. **Fabuła odkrywana z budowami (#35)** – 19 wątków SMS za kamienie milowe, Wiadomości w profilu, Osiedle rośnie.

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

- **Podsumowanie budowy (#33, cz. 4):** po porażce i po wygranej (po SMS-ie i harmonogramie domu) – na GBA 3 strony
  w telefonie, w Godocie jedna przewijana strona. **Co zatrzymało budowę:** „Pokonało Cię: Zwarcie” (czasownik wg rodzaju,
  przedrostek elity: „Uparty Kornik”), etap i akt („5/10, Akt II”), ostatnie 3 ciosy („Zwarcie: -2 (mokry + prąd)”,
  „Zepsuta Betoniarka: -7 (cios bossa)”, „Inspekcja Pracy: -5 (Kontrola BHP)”, wybuch, wybuch pyłu, z dystansu),
  najmocniejszy cios w Ciebie i Twój („Twój cios: 21 (kryt) w Zepsuta Betoniarka”). **Oś czasu:** każdy etap z dniami
  i usuniętymi, pod nim SMS z placu, magazyn / ulepszenie / elita / boss / kombinacje / synergia / premia z SMS-a
  i wybrana premia po etapie, na końcu „Tu stanęła budowa”. **Nagrody i cele:** doświadczenie i Respekt z budowy,
  najbliższe zlecenie, rekord dnia / tygodnia, najbliższy cel („Jeszcze 3 Respektu do: Pewna ręka II”, potem Szkolenia)
  i rada – pierwsza pasująca z listy (sekcja `recap`): mokry + prąd, cios bossa, wybuch, niewypita kawa, strzelec, elita,
  boss, bez kombinacji („Mokry + prąd zadaje +3: spróbuj premii Przepięcie”), wygrana (tryb inwestora / Trudny).
- **Wyzwania tygodnia (#34):** tydzień od poniedziałku (nr 1 = 5.01.2026), seed z numeru tygodnia, zasady po kolei
  z listy (sekcja `weekly`, do 3 zasad: zawód, bez kawy – kawa na wynos za zł, elity %, pogoda na każdym etapie, bez
  Hurtowni, materiały %, HP %, ciosy %, budżet): **Tylko Glazurnik, bez kawy** (HP +25%), **Elity x2**, **Mokry tydzień**
  (deszcz na każdym etapie), **Bez Hurtowni, x2 materiały**, **Szklany kask** (HP -50%, ciosy +15%), **Kierownik na
  placu** (więcej elit, HP -25%). Bez Szkoleń i pamiątek, Normalny, bez NG+; profil pamięta najlepszy wynik 3 ostatnich
  tygodni. GBA: L na tytule albo SELECT na budowie dnia (tydzień z daty budowy dnia, góra/dół – inny tydzień); Godot:
  „Wyzwanie tygodnia” w menu tytułu (data z systemu), „Wyślij wynik” – zaślepka tabeli tygodnia (`ILeaderboard`:
  `WeeklyBoardId`, bez sieci). Zasada tygodnia w telefonie (Zadania), rekord tygodnia na końcu budowy.
- **Fabuła odkrywana z budowami (#35):** 19 wątków SMS (1–2 wiadomości, sekcja `story.arc`) od Anny, kierownika Marka,
  sąsiada Zenka i inwestorki Ewy – za 1., 5. i 10. budowę, 1., 3., 5. i 10. wygraną, każdego bossa (pierwsze
  pokonanie), pierwszą elitę, magazyn, SMS na placu, synergię, budowę dnia, wyzwanie tygodnia i Akt 0. Na końcu budowy
  baner „Nowa wiadomość”, w telefonie profilu Osiedle > A (Godot: „Wiadomości”) – archiwum: odblokowane z pastylką „Nowa”
  do przeczytania, zablokowane „???” z podpowiedzią, jak je zdobyć. Osiedle rośnie z wygranymi: Lipa (1), Ławka Zenka
  (3), Latarnia (4), Plac zabaw (5), Tablica PlanBudowlany (8), Fontanna (10) – nowe klatki pixel-art w `houses.bmp`
  (Godot: te same, eksport).
- „Jak grać”: GBA 13 stron (nowa „Po budowie”), Godot 7 (sekcja `metaHelp`, wspólna). Scenariusze testowe 57–60
  (podsumowanie po porażce i po wygranej, wyzwanie tygodnia, Wiadomości i Osiedle). Godot: sceny `recap-*`, `weekly*`,
  `story-*`, `estate-grow`, `help-meta`; test dymny przechodzi wyzwanie tygodnia, podsumowania i archiwum.

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

Część 4 nie zmienia logiki gry poza zapisem podsumowania (ciosy, oś czasu – bez losowania) i zasadami wyzwania tygodnia
(działają tylko w budowie tygodnia): wyniki bota są identyczne jak w części 3, a stare przebiegi testu złotego dają ten sam
stan (poza nowymi polami).

| Wygrane bota (300 przebiegów na zawód) | v0.21.49 | v0.21.50 cz. 2 (premie, elity, kombinacje) | v0.21.50 cz. 3 (wydarzenia, ulepszenia, magazyn) | v0.21.50 (wydanie, cz. 4) |
|---|---|---|---|---|
| Łatwy | 58% | 54% | 53% | 53% |
| Normalny | 30% | 30% | 32% | 32% |
| Trudny | 12% | 11% | 10% | 10% |
| Normalny, pełne Szkolenia | 57% | 52% | 54% | 54% |
| Normalny, pełne Szkolenia + pełny Respekt | 71% | 68% | 70% | 70% |
| Normalny, pełne Szkolenia (i Respekt) + wszystkie modyfikatory | 10% | 14% | 9% | 9% |
| Normalny bez picia kawy (bez meta / pełne Szkolenia) | 17% / 38% | 20% / 36% | 22% / 40% | 22% / 40% |
| Normalny, pełne Szkolenia + pełny Respekt + Akt 0 (wszystkie nagrody) | 66% | 68% | 66% | 66% |
| Normalny bez premii po etapie (bez meta / pełne Szkolenia) | – | 1% / 8% | 4% / 14% | 4% / 14% |

Wyzwania tygodnia (bot, Normalny bez meta; 100 przebiegów na zawód, a przy zasadzie zawodu 900 tym zawodem) – każde
do przejścia, żadne nie łatwiejsze niż zwykły Normalny o więcej niż kilka punktów:

| Wyzwanie | Wygrane bota | Uwagi |
|---|---|---|
| Tylko Glazurnik, bez kawy (HP +25%) | 11% | bez HP +25%: 7%; Glazurnik zwykle 25% |
| Elity x2 | 31% | elita = lepsza nagroda, trudność prawie bez zmian |
| Mokry tydzień (deszcz na każdym etapie) | 35% | kałuże: poślizg, ale też mokry + prąd dla bohatera |
| Bez Hurtowni, x2 materiały | 18% | bez ulepszeń narzędzia i siłowni |
| Szklany kask (HP -50%, ciosy +15%) | 24% | wersja -30% / +30% dawała 42% |
| Kierownik na placu (więcej elit, HP -25%) | 33% | Kierownik zwykle 38% |

### Zmiany
- Nowy zapis budowy (PBRUN13; w cz. 3 PBRUN12, w cz. 2 PBRUN11) – przerwana budowa z wcześniejszej wersji nie wznowi
  się. Profil v11 (PBRL011, 188 bajtów): wyniki 3 tygodni i wątki fabuły; migracja z v10 zachowuje wszystko, a wątki za
  to, co już osiągnięte (liczba budów i wygranych, bossowie z Katalogu, Akt 0), czekają w Wiadomościach jako nowe.
- Menu tytułu (Godot): nowa pozycja „Wyzwanie tygodnia”; GBA: podpowiedź „R: budowa dnia  L: tydzień”.
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
