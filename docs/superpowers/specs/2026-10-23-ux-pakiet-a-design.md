# Projekt A – wygląd i przepływ ekranów (Godot) – dokument projektu

Data: 2026-10-23 · Wersja gry: v0.22.0 (otwarta, branch `dev/0.22`) · Ustalone z właścicielem w rozmowie (brainstorming).

## 1. Po co

Gra ma dużo treści (12 zawodów, 22 bronie, Szkolenia, Respekt, kontrakty kariery, odblokowania za odbiór), ale ekrany jej
nie pokazują i nie podpowiadają, co dalej. Właściciel: „jeśli gra ma tyle broni, to chujowo to pokazuje”. Cel: gra ma
**wyglądać sensownie i być użyteczna**. Gracz w każdej chwili wie, co może zrobić dalej, a po każdej budowie dostaje
powód, żeby zagrać kolejną.

To pierwszy z pięciu projektów. Kolejność ustalona z właścicielem: **A (ten) → D humor → C animacje walki → B więcej
treści (kontrakty, ulepszenia, bronie z umiejętnościami, pula odblokowań, przecinak do ścian) → E wezwanie wsparcia
z telefonu**.

## 2. Cele

1. Pełne nazwy zawodów. Bronie i moce zawodów widoczne jako osobne elementy, nie w zbitym tekście.
2. Moment między budowami zachęca do kolejnej: pokazuje zdobyte doświadczenie, co można za nie kupić i ile brakuje do następnej broni.
3. Tytuł z mniejszą liczbą pozycji i z widocznym doświadczeniem, które od razu da się wydać.
4. Usunięcie problemów znalezionych w teście (zrzuty: `docs/ux/pakiet-a/sheet_*.png`).
5. Porządny układ na dużym ekranie pod palec: rozłożony składak, iPad.

## 3. Czego nie robimy (non-goals)

- Żadnych zmian w logice gry, balansie, cenach ani danych rozgrywki. Test złoty C++↔C# i balans bota bez zmian.
- Nowa treść (bronie, umiejętności broni, kontrakty, ulepszenia, przecinak do ścian): to projekt B.
- Nowe teksty humorystyczne i postacie: to projekt D. Tutaj SMS-y mają ton obecnych wiadomości Kierownika Marka, z miejscem na późniejszy humor.
- Animacje walki: to projekt C.
- Warianty tytułu B (karta „Następna budowa”), C (ostatni SMS na tytule) i D (tablice na placu).
- **GBA: bez zmian w tym pakiecie.** Decyzja właściciela: „GBA poświęć mniej czasu i opcji, prio to Godot; GBA był eksperymentem, by rozreklamować resztę”. GBA po budowie i tak już przechodzi do Szkoleń (START → `scene::shop`) i pokazuje „Stać cię: X”.

## 4. Czego nie wolno zepsuć (anti-goals)

- Wygląd telefonu w grze, w tokenach appki (tło `#f6f5f8`, białe karty, fiolet `#6b4eff`, statusy z `Pal.cs`), zostaje. Nowe ekrany mają wyglądać jak jego część, a nie jak inna aplikacja.
- Wiadomości (SMS-y Marka i Anny) to mocna strona gry: przebudowa ich nie spłaszcza.
- Sterowanie klawiaturą i padem na desktopie dalej działa na każdym zmienionym ekranie (skróty mogą zniknąć z tekstu, nie z obsługi).
- Samouczek menu (`Coach`, dymki Marka) dalej wskazuje właściwe elementy po zmianie układu tytułu.
- Monkey test Godot (`--monkey`) i test dymny (`--smoke`) przechodzą bez wyjątków w `DrawErrors`.

## 5. Ograniczenia

- Godot 4.7.2 .NET, C#: jeden typ na plik, Allman, 4 spacje, bez `Type?`. Uwaga na taby wstawiane przez edytor w `App.cs`/`Main.cs`/`SceneNodes.cs`.
- Teksty tylko w `GBA/data/game.json` (`uiGodot`) i `GBA/data/lang/en.json`. Po zmianie `python3 GBA/tools/gen_data.py --check`. Każdy nowy ekran w PL i EN, sprawdzony zrzutem `--lang en`.
- Docelowe rozmiary do sprawdzenia zrzutami: telefon pion `--touch --portrait` (860×1864), desktop 1280×720, duży ekran `--touch --size 1640x1460` i `--size 1460x1640`.
- Bez cudzych marek. Przeciwnicy to problemy budowy, nie ludzie.
- Doświadczenie wydaje się w **Szkoleniach** (telefon kierownika, dziś zakładka „Koszty”, `TrainingTab`), a nie w Hurtowni. Hurtownia to sklep w trakcie budowy, za zł. Dokumenty UX z `master` w tym miejscu się mylą.
- Testy oszczędnie: po każdej części build, testy jednostkowe, smoke i zrzuty dotkniętych scen. Pełny `rogue-test` raz przed wydaniem.

## 6. Ustalenia (zdecydowane z właścicielem)

### 6.1 Wybór zawodu: siatka kafli (makieta `docs/ux/pakiet-a/class-select.html`, wariant B)

- Zamiast paska 12 małych portretów: siatka kafli, każdy z portretem i **pełną nazwą** (np. „Kierownik budowy”, „Operator koparki”; w razie potrzeby dwie linie, nigdy wielokropek).
- Telefon pion: 3 kolumny. Pod siatką krótka karta wybranego zawodu: nazwa, opis, broń (nazwa + styl + obrażenia). Przyciski „Szczegóły” i „Start budowy”.
- Zablokowany kafel: przyciemniony, z warunkiem wprost na kafelku („5 wygr.”, „60 dośw.”, „Akt 0”, „?” dla sekretu). Przycisk startu przy zablokowanym zawodzie nie wygląda jak aktywny.
- Pełna karta zawodu (po „Szczegóły” na telefonie, stale na dużym ekranie): broń i moc jako **osobne bloki** z ikoną. Statystyki opisane słowami („Siła · obrażenia wręcz”, „Szczęście · lepszy sprzęt”) zamiast „0+3” / „34+4”. Dalej trudność, pamiątka, tryb inwestora, mistrzostwo.

### 6.2 Między budowami: SMS od Kierownika Marka (makieta `between-runs.html`, wariant C)

- Po budowie (wygrana i przegrana, także budowa dnia i tygodnia) gracz dostaje w telefonie SMS-y od Kierownika Marka:
  - wynik („Szefie, odbiór zaliczony!” / przy przegranej odpowiednik);
  - zdobyte doświadczenie i stan konta;
  - co warto kupić: najtańsza rzecz, na którą stać, z ceną, albo ile brakuje do najbliższej;
  - ile brakuje do następnej broni lub odblokowania za odbiór.
- Pod SMS-ami **szybkie odpowiedzi** jako przyciski:
  - „Kup [nazwa]”: kupuje od razu, bez wchodzenia do Szkoleń, gdy stać;
  - „Pokaż Szkolenia”: otwiera telefon kierownika na Szkoleniach;
  - „Kolejna budowa”: dalej do wyboru zawodu albo mapy kariery, jak dziś;
  - przy wygranej zostaje opcja NG+, jak dziś.
- Pod rozmową **podgląd następnej broni / odblokowania** (z ikoną broni), zaczerpnięty z wariantu B. Forma: patrz decyzja 7 w sekcji 7.
- To zastępuje dzisiejszy ciąg plansz, w którym doświadczenie ginie. Obecny ciąg: SMS końca → harmonogram domu → podsumowanie → plansza końcowa z kodem QR. Szczegółowe podsumowanie budowy zostaje dostępne („Szczegóły budowy”), ale nie zasłania głównej decyzji.
- **Promocja PlanBudowlany (decyzja właściciela, 2026-10-23):** po wygranej **harmonogram domu dalej pojawia się automatycznie** przed SMS-ami Marka. **Kod QR do planbudowlany.online przenosi się do harmonogramu domu** (obok jednego „Zaplanuj swoją budowę”). Stara plansza końcowa (`EndView`, fioletowa z kodem QR) znika z przepływu.

### 6.3 Tytuł: pogrupowane menu + karta postępów (makieta `title.html`, wariant A)

- Na górze „Nowa budowa” (i „Kontynuuj budowę”, gdy jest zapis) jako główny przycisk.
- Grupa **Dziennie**: „Codzienna” i „Tydzień” obok siebie.
- Grupa **Postęp**: „Telefon kierownika” (jedna pozycja zamiast dzisiejszych „Profil: odznaki, zlecenia” i „Szkolenia (Koszty)”) oraz „Jak grać”.
- Na dole karta **„Twoje postępy”**: Doświadczenie, Budowy (i rekord), przycisk **„Wydaj doświadczenie”** otwierający Szkolenia. Stopka „P: profil K: Szkolenia” znika z tekstu (klawisze dalej działają).
- Inspektor i rekord zostają na górze ekranu.

### 6.4 Poprawki z testu (właściciel: „zgodzę się z problemami potwierdzonymi w teście”, „póki co wszystko ok”)

- **Powiadomienia (banery):** nie zasłaniają nagłówków, przycisków, klucza ustawień, SMS-ów ani kodu QR. Dziś do trzech naraz przykrywa Hurtownię, koniec budowy i tytuł.
- **Pasek zakładek telefonu** tylko tam, gdzie zakładki działają. Bez niego: mapa kariery, ustawienia, Codzienna, Tydzień, wiadomości / karta etapu i inne ekrany jednej strony.
- **Podsumowanie budowy:** bez licznika „10/48” (pozycja przewijania) i bez przycisków „Wyżej/Niżej” na dotyku, przewijanie palcem. Wynik i doświadczenie na górze.
- **HUD w grze:** ikony (apteczka 0/4, pogoda, błoto) z krótkim podpisem.
- **Legenda kolorów** pasków w wiadomościach / karcie etapu i na placu budowy (premia, zagrożenie, ścieżka, info).
- **Harmonogram domu:** jedno „Zaplanuj swoją budowę” zamiast dwóch (TODO #71).
- **Ustawienia w widoku poziomym:** te same grupy co w pionie (Dźwięk / Sterowanie / Gra).

### 6.5 Duży ekran (makieta `class-select-fold.html`)

- Trzeci układ obok „pion telefonu” i „poziom desktop”: duży ekran dotykowy (rozłożony składak, iPad). Dziś rozłożony poziomo dostaje drobny układ desktopowy, a pionowo rozciągnięty układ telefonu (`sheet_fold.png`).
- Na pewno: wybór zawodu w dwóch panelach (siatka 4 kolumny po lewej, pełna karta po prawej) i czytelne, duże pod palec menu tytułu.

## 7. Decyzje podjęte przeze mnie (do sprawdzenia przy czytaniu)

1. **Bez modala „Masz N doświadczenia” przy Nowej budowie** (był w planie z 2026-10-22). Karta postępów na tytule i SMS między budowami pokrywają ten cel bez dodatkowego okna. Plan `docs/superpowers/plans/2026-10-22-ux-xp-flow.md` zostanie zastąpiony nowym planem.
2. **Powiadomienia:** naraz najwyżej jedno. Na tytule pod rekordem i statystykami, w telefonie przy dole nad przyciskami, reszta czeka w kolejce.
3. Zakładka **„Koszty” w telefonie kierownika zmienia nazwę na „Szkolenia”**. Podstrony Drzewko / Respekt / Nagrody zostają.
4. **Ikona głośnika znika z HUD**. Dźwięk jest w ustawieniach.
5. **Duży ekran w tym pakiecie:** wybór zawodu i tytuł, a z telefonu (każdy ekran-telefon) tylko to, żeby się nie rozciągał. Na dużym ekranie telefon ma rozsądną maksymalną szerokość i jest wyśrodkowany. Gra (mapa) bez zmian poza większym HUD-em, jeśli wychodzi za mały.
6. „Kup [nazwa]” w SMS-ie kupuje **najtańszą rzecz w Szkoleniach, na którą stać** (ta sama, którą dziś liczy `Meta.NextUnlock`). Respekt jest osobną walutą, dalej kupowany na stronie Respekt.
   Jednym stuknięciem tylko rzeczy bez wyboru: ulepszenie, zawód, narzędzie, brygada, poziom Trudny (rodzaje 0–4). Węzeł drzewka (rodzaj 5) wymaga wyboru 1 z 2, więc zamiast „Kup” jest „Pokaż Szkolenia” otwierające Drzewko. Po zakupie Marek odpisuje krótko („Zrobione!”) i podsuwa kolejną rzecz albo pisze, ile brakuje.
7. **Pasek postępu do broni zmienia się w wiersz „Za następny odbiór: [nazwa]”** z ikoną broni, gdy nagroda jest narzędziem. Powód: nagrody za odbiór (`d.Rewards`) przychodzą po jednej na każdą wygraną, więc pasek „3/5 wygranych” z makiety nie pasuje do danych. Jeśli wszystkie nagrody są odebrane, wiersza nie ma.
8. **Nagrody z końca budowy** (nowe poziomy, zadania, kolekcje, seria dni, wątki fabuły, kontrakty, filtry), które dziś lecą jako 5–10 złotych banerów, stają się częścią rozmowy: jeden SMS Marka „Do tego:” z krótką listą (najwyżej 4 pozycje, potem „i N więcej”, pełna lista w szczegółach budowy). Na ekranie między budowami nie ma banerów.
9. **„Kolejna budowa”** prowadzi od razu do mapy kariery (gdy `Flow.Career.Available`) albo do wyboru zawodu, z pominięciem tytułu. Tak samo po budowie dnia i tygodnia (zaczyna zwykłą budowę). Powrót na tytuł: krzyżyk lub „Menu”.

## 8. Zostawione wykonawcy

- Dokładne wymiary, odstępy, wielkości fontu i ikon (w zgodzie z tokenami telefonu i `Layout`).
- Próg „duży ekran” (np. krótszy bok w punktach UI) i sposób wykrycia składaka.
- Treść SMS-ów (PL/EN) w tonie obecnych wiadomości Marka. Dowcipy przyjdą w projekcie D.
- Kolejność i podział pracy na części (plan: `superpowers:writing-plans`).
- Jak technicznie zastąpić obecny ciąg plansz po budowie (nowy ekran czy przebudowa `EndMessageScreen` / `RecapScreen` / `EndScreen`), byle spełnić 6.2.
- Nowe sceny zrzutów w `DebugScenes` dla zmienionych ekranów.

## 9. Materiały

- Makiety: `docs/ux/pakiet-a/*.html` (fragmenty HTML z towarzysza wizualnego; style pomocnicze ramki nie są dołączone).
- Zrzuty z testu: `docs/ux/pakiet-a/sheet_p0..p5.png` (pion), `sheet_land.png` (1280×720), `sheet_fold.png` (duży ekran).
- Dokumenty UX z `master`: `docs/ux/ux-flow.html`, `docs/ux/review/ux-review.html`, `docs/ux/concepts/game-style-concepts-v2.html`, `docs/ux/title-variants-infographic.html`.
