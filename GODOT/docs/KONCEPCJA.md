# Koncepcja: PlanBudowlany Roguelike w Godot (ewolucja LifeLike)

Status: szkic z podjętymi decyzjami (2026-09-24, patrz sekcja 7). Źródło pomysłu: demo GBA (`../../GBA`) okazało się grywalne,
więc wersja Godot przejmuje jego założenia i rozwija je w pełnej rozdzielczości, z większą ilością detali.
Najważniejszy wyróżnik: **menu i interfejs gry to smartfon bohatera z aplikacją PlanBudowlany**.

## 1. Filary

1. **Budowa jako loch.** Etapy budowy (Fundamenty → Stan surowy → Dach → Instalacje → Wykończenie i odbiór)
   to kolejne piętra. Wrogami są *problemy budowy* (Przeciek, Zwarcie, Pleśń, Papierologia,
   Przekroczony budżet…), bossem *Nieprzekraczalny Termin*. Nigdy zawody ani ludzie.
2. **Telefon jako interfejs.** Wszystko poza samą mapą dzieje się w telefonie, który wygląda jak prawdziwa
   aplikacja PlanBudowlany (ta sama paleta, karty, pastylki statusów, dolny pasek zakładek, powiadomienia push).
   Gra jest więc jednocześnie grą i reklamą produktu, bez nachalnego brandingu.
3. **Roguelite z meta-progresją.** Każda budowa jest inna (generator z seedem), a doświadczenie z budów
   kupuje trwałe Szkolenia, nowe zawody i narzędzia.
4. **Czytelność ponad efekciarstwo.** Turowa walka, jasne informacje (paski HP, liczby obrażeń, mgła wojny),
   gra na klawiaturze, myszce i padzie.

## 2. Pętla rozgrywki

```
Tytuł ─▶ Wybór zawodu + trudność ─▶ Etap (eksploracja, walka, znajdźki) ─▶ Harmonogram ─▶ … ─▶ Termin
   ▲                                                                                            │
   └──────── Koszty / Szkolenia (wydaj doświadczenie) ◀── Odbiór zaliczony / Budowa wstrzymana ◀┘
```

Z demo GBA przenosimy 1:1 (dane w JSON, logika w `LifeLike.Core`, testy xUnit):
zawody z mocą pod jednym klawiszem, poziomy postaci w trakcie budowy, dropy i narzędzia, NG+
(„Kolejna budowa”), poziomy trudności, zapis budowy w trakcie, mgła wojny.

## 3. Telefon – projekt interfejsu

Telefon wysuwa się z dołu na **środek ekranu** (Tab / SELECT / prawy klik na postać). Gra jest turowa,
a wszystko, co robisz w telefonie, dzieje się **poza czasem gry**: przeglądanie zakładek, zakupy i
ustawienia nie zużywają tury.
Styl 1:1 z aplikacją mobilną (`planbudowlany-mobile`, `AppColors.kt`):

| Token | Wartość | Użycie w grze |
|---|---|---|
| brand | `#6B4EFF` | aktywna zakładka, wybrane elementy, przyciski |
| brandAccent | `#FF7A3D` | wyróżnienia, pasek ostrzegawczy na tytule |
| statusTodo / InProgress / Done / Delayed | `#94A3B8` / `#F59E0B` / `#10B981` / `#EF4444` | statusy etapów, usterek, zawodów |
| bgLight2 / card / textLight2 / textDim | `#F6F5F8` / `#FFFFFF` / `#0B0B0F` / `#636366` | tło, karty, tekst |

Zakładki (dolny pasek jak w aplikacji):

| Zakładka | W grze | Odpowiednik w aplikacji |
|---|---|---|
| **Zadania** | harmonogram budowy: etapy jako grupy zadań, w każdym *zadania etapu* (np. „Zbrojenie ław”, „Wylanie chudziaka”) odhaczane za pokonane problemy i znalezione elementy | lista zadań z filtrami Wszystkie / Do zrobienia / W trakcie / Gotowe |
| **Usterki** | problemy budowy jako zgłoszenia: numer, waga (NISKI / ŚREDNI / WYSOKI), status OTWARTA / ZAMKNIĘTA, „zdjęcie” = portret wroga, opis z humorem | raport usterek z odbioru |
| **Start** | pulpit: HP, poziom, moc, broń, wynik, dzień budowy, mini-mapa odkrytego etapu | ekran startowy |
| **Zespół** | zawody (odblokowane / zablokowane), w dalszej kolejności *najemni fachowcy* do wezwania raz na etap | zespół i wizytówki |
| **Koszty** | Szkolenia: „Całkowity koszt”, pasek budżetu, „Pozostało” = doświadczenie; lista ulepszeń z pastylkami ceny | ekran kosztów i budżetu |

Powiadomienia push (baner zjeżdżający z góry, ikona PB, „teraz”): awans, nowe narzędzie, drop,
moc gotowa, zaliczony etap, pojawienie się Terminu („Przypisano Ci usterkę: Nieprzekraczalny Termin”).
W wersji Godot dochodzą: dźwięk powiadomienia, stos kilku banerów, dotknięcie banera otwiera właściwą zakładkę.

Szkic ekranu (1280×720): telefon w pionie na środku, mapa etapu w tle rozmyta i przyciemniona:

```
┌──────────────────────────────────────────────────────────────┐
│                    ┌──────────────────────┐                  │
│   (mapa etapu,     │ 09:41        ▂▄▆ ▭   │   (rozmyta,      │
│    przyciemniona)  │ Zadania     Etap 3/5 │    przyciemniona)│
│                    │ ┌──────────────────┐ │                  │
│                    │ │● Stan surowy   4 │ │                  │
│                    │ │ Montaż okien     │ │                  │
│                    │ │     [W trakcie]  │ │                  │
│                    │ │ Docieplenie      │ │                  │
│                    │ │     [Do zrob.]   │ │                  │
│                    │ └──────────────────┘ │                  │
│                    │  ☑   ▤   ⌂   👥   ▭  │                  │
│                    └──────────────────────┘                  │
└──────────────────────────────────────────────────────────────┘
```

## 4. Więcej detali niż na GBA

- **Oprawa 2.5D:** siatka turowa bez zmian, ale rzut 3/4 z głębią (sprite'y lub proste modele na płaskiej
  siatce, kamera pod kątem), oświetlenie dynamiczne, animacje chodu i ataku, cząsteczki (pył, iskry, krople), dynamiczne światło latarki czołowej zamiast
  twardej mgły, pogoda na etapie Dach (Ulewa jako wróg i jako efekt).
- **Mapa budowy zamiast lochu:** generator etapów tematycznych – wykop z szalunkami, surowe mury z otworami
  okiennymi, dach z krokwiami, instalacje w bruzdach, wykończone wnętrza. Elementy otoczenia: rusztowania
  (wyższy teren), betoniarka (przeszkoda / broń środowiskowa), palety materiałów (osłona).
- **Zadania etapu jako cele poboczne:** zakładka Zadania to prawdziwa lista celów, a ich wykonanie daje
  dodatkowe doświadczenie i „Odbiór częściowy”.
- **Wydarzenia losowe z telefonu:** SMS od „kierownika” (modyfikator etapu: „dostawa spóźniona – mniej
  znajdziek”), inspekcja nadzoru (bonus za brak otwartych usterek).
- **Moce zawodów rozwinięte:** drzewko 2–3 ulepszeń mocy kupowanych w Koszty.
- **Dźwięk:** muzyka per etap, efekty narzędzi, dźwięk powiadomienia jak w telefonie.

## 5. Architektura (zgodna z obecnym projektem)

- `LifeLike.Core` (C#, bez Godota): dochodzą moduły z demo GBA – `Progression` (poziomy, doświadczenie),
  `Meta` (profil, sklep, koszty – odpowiednik `GBA/include/meta.h`), `Abilities`, `Loot`, `Fov`
  (shadowcasting), `Save` (serializacja stanu do JSON). Każdy z testami xUnit; przypadki testowe
  można przenieść z `GBA/tests/core_tests.cpp`.
- **Dane wspólne z GBA:** statystyki, zawody, przedmioty, narzędzia, wrogowie, etapy, ulepszenia i moce
  mają jedno źródło prawdy (dziś `GBA/data/game.json`, docelowo katalog wspólny dla obu projektów).
  GBA generuje z niego nagłówek (`tools/gen_data.py`), Godot czyta ten sam JSON przez `GodotDataSource`.
  Rzeczy specyficzne dla platformy (grafiki, dźwięki, układ UI) zostają osobno.
- **Telefon:** osobna scena `Phone.tscn` (`CanvasLayer` → `Control`), motyw Godot (`Theme`) z tokenami
  z tabeli wyżej; zakładki jako sceny `TasksTab`, `IssuesTab`, `HomeTab`, `TeamTab`, `CostsTab`;
  animacje wysuwania przez `Tween`. Powiadomienia jako `PushBanner.tscn` z kolejką.
- **Wejście:** istniejący `GameInput` (klawiatura / mysz / pad) + akcje `phone_toggle`, `tab_prev`, `tab_next`.

## 6. Kamienie milowe

1. Rdzeń: przeniesienie logiki z demo GBA do `LifeLike.Core` (testy), wspólny plik danych z GBA.
2. Telefon: scena, motyw, zakładka Start i Koszty, powiadomienia.
3. Pozostałe zakładki (Zadania z celami etapu, Usterki, Zespół).
4. Oprawa: pixel-art, animacje, światło, cząsteczki, dźwięk.
5. Mapy tematyczne etapów i elementy otoczenia.
6. Wydarzenia z telefonu, drzewka mocy, balans na botach (jak w demo GBA).

## 7. Decyzje

| Pytanie | Decyzja |
|---|---|
| Gdzie telefon? | Na środku ekranu, w pionie; mapa w tle przyciemniona. |
| Rzut? | 2.5D (siatka turowa, kamera 3/4, dynamiczne światło). |
| Czas a telefon? | Gra turowa; akcje w telefonie są poza czasem i nie zużywają tury. |
| Dane? | Statystyki, zawody, przedmioty itp. wspólne z GBA (jeden JSON); grafika i UI osobno. |

## 8. Postęp

### Kamień milowy 1 – rdzeń (zrobione, 2026-09-25)

- `LifeLike.Core` zastąpił stary fantasy-rdzeń (mag/wojownik/szczur, `DungeonGenerator`). Logika z `GBA/include/core.h`
  i `meta.h` przeniesiona 1:1: generator xorshift32, shadowcasting, walka, AI, moce z rangami, stany, sprzęt,
  narzędzia, dropy, akty z bossami (zapowiadane uderzenie), Hurtownia, poziomy postaci, NG+, dziennik, celowanie,
  profil (Szkolenia, odblokowania, odznaki, katalog, Osiedle, bankowanie XP, migracje zapisu) i zapis budowy.
- Dane: gra czyta wspólny `GBA/data/game.json` (kopiowany przy buildzie do `godot/data/`), parser ignoruje nieznane pola.
- Testy: przypadki z `GBA/tests/core_tests.cpp` jako xUnit (z balansem na botach) + **test złoty**:
  `GBA/tests/golden_dump.cpp` zapisuje 15 przebiegów bota, C# odtwarza je krok po kroku (skrót stanu po każdym
  kroku, pełne zrzuty na etapach) – ten sam seed daje identyczną grę.
- Godot: grywalna wersja 2D (prostokąty i litery, mgła wojny, HUD tekstowy, ekrany tekstowe: tytuł, Szkolenia,
  harmonogram, Hurtownia, podgląd, koniec gry z NG+), sterowanie klawiatura/pad/mysz jak na GBA,
  test dymny `--smoke` (bot gra 3 etapy w headless Godot).
- Różnice względem GBA: zapis budowy to serializacja pól (nie `memcpy` struktury), celowanie pod A wybiera najbliższy
  cel (bez przełączania strzałkami), brak telefonu, dźwięku, prologu i animacji – to kolejne kamienie milowe.

### Kamień milowy 2 + część 4 – telefon i oprawa pikselowa (zrobione, 2026-09-25)

- Grafika, font i dźwięk z GBA: `GODOT/tools/export_godot_assets.py` (deterministyczny, czyta tylko `GBA/`) zapisuje
  do `godot/assets/` postacie i wrogów 32x32 (Scale2x), bogatsze kafle 32x32 dla 8 etapów w paletach z GBA,
  cząsteczki, ikony, plansze tytułu/końca, pikselowy font z polskimi znakami i dźwięki (muzyka `.mod` -> `.mp3`).
- Obraz 640x360 skalowany 2x (ostre piksele, więcej pól na ekranie niż na GBA): mapa z kafli, cienie, płynny ruch,
  szturchnięcie przy ataku, błysk trafienia, mgła z miękkim światłem (tekstura mgły z filtrowaniem liniowym),
  pola ciosu bossa, znacznik celu, „!”, mini paski HP, liczby KRYT!/Unik!, cząsteczki mocy, wstrząs i błyski ekranu.
- HUD 1:1 z GBA (pasek HP, poziom, etap, stany z turami, termos, ikona mocy z odliczaniem, gasnący dziennik).
- Telefon pionowo na środku (tło rozmyte i przyciemnione, wysuwa się z dołu, `Tab`/SELECT): zakładki w grze
  Zadania, Usterki (portrety problemów), Start, Sprzęt, Koszty; profil na tytule: Odznaki/Zlecenia/Pamiątki, Katalog,
  Osiedle, Zespół, Koszty = Szkolenia; wiadomości SMS, paczka sprzętu, harmonogram, Hurtownia. Powiadomienia push
  (stos banerów z dźwiękiem; przy otwartym telefonie w kolumnie obok).
- Ekrany: tytuł (logo, wersja z game.json), wybór zawodu jak GBA v0.21.45 (karuzela portretów, odblokowane najpierw,
  karta z paskami statystyk), koniec z kodem QR i konfetti. Zrzuty `--screenshot --scene` (26 scen), test dymny
  otwiera wszystkie zakładki i ekrany i kończy się błędem, gdy któryś się nie rysuje.
- Jeszcze nie (stan po kamieniu 2): 2.5D i dynamiczne światło (kamień 4); resztę zrobił kolejny krok (niżej).

### Architektura i parytet z GBA (zrobione, 2026-09-25)

- Warstwa Godota podzielona na klasy (katalog = przestrzeń nazw): `Main` to cienki korzeń, `App` składa sesję,
  węzły i ekrany; `Screens/` – maszyna stanów `ScreenFlow` i ekrany (`Screen`: Enter / Exit / HandleInput / Process,
  deklaracja warstw i muzyki), `Session/GameSession` z `SessionEvents` (awans, drop, moc gotowa, boss, etap, koniec
  budowy, odznaki) – banery, dźwięki i efekty mapy tylko obserwują; `Input/` – akcje jak przyciski GBA (`GameAction`,
  `InputCmd`), wstrzykiwanie z przycisków ekranowych; `Debug/` – zrzuty i test dymny poza kodem produkcyjnym;
  kolory w `Pal`/`Ink`, skale i rozmiar ekranu tylko w `Layout`. Zachowanie bez zmian (ten sam wynik testu
  dymnego, zrzuty 26 scen różnią się tylko fazą animacji).
- Ekran: stretch `canvas_items` + `expand` (dowolne proporcje bez pasów), mapa ~9 pól w pionie jak na GBA
  (2.5 px okna na piksel grafiki), bohater w środku pola między paskami HUD, HUD w 1.5x, podgląd mapy dopasowany do
  odkrytej części; menu tytułu i karta zawodu w większym foncie (skala ułamkowa fontu).
- Celowanie pod trzymanym A (zasięg + celownik z GBA, strzałki zmieniają cel, puszczenie atakuje) i podgląd pod
  trzymanym B (karta wroga: nazwa, HP, obrażenia z premią, opis; krótkie B = czekaj) – jak na GBA.
- Prolog przy pierwszej budowie (`story.prologue`, `prologueCaptions`, pickup z GBA, flaga `prologue_seen`) i ekran
  „Jak grać” (`help_seen`, też z menu tytułu).
- Rada kierownika na harmonogramie (`tips`, kolejna z każdym etapem; przyciski GBA w tekście zamieniane na klawisze
  z mapy wejścia – dane w game.json bez zmian).
- Powiadomienia push w prawym górnym rogu pod HUD (nie zasłaniają „UWAGA: cios za…”), kliknięcie / dotknięcie
  otwiera powiązaną zakładkę telefonu.
- Eksporter: 4 klatki chodu i klatka oddechu dla zawodów, problemów i bossów (`actors_anim.png`), pickup z prologu.
- Dalej: układ pionowy z przyciskami ekranowymi (telefon), 2.5D i światło (kamień 4) – układ pionowy zrobiony (niżej).

### Wersja na telefon: pion, dotyk jedną ręką, ustawienia, iOS (zrobione, 2026-09-25)

- Na iOS/Androidzie gra działa pionowo (komputer bez zmian, 1280x720 poziomo). `Layout` wybiera bazę 360x640
  pionowo (iPhone 14 Pro Max: UI 430x932, piksel UI = punkt iOS, skala 3) i 640x360 poziomo, skala całkowita
  z opcją „duży tekst”, bezpieczny obszar z `DisplayServer.GetDisplaySafeArea()`: HUD pod wyspą, pasek akcji
  nad paskiem domowym, mapa ~9 pól na szerokość, telefon PlanBudowlany jako aplikacja na cały ekran.
- Sterowanie jedną ręką zamiast A/B: pasek akcji w strefie kciuka z ikonami menu akcji z GBA (Atak – trzymanie:
  celownik pod palcem; Moc – szara z odliczaniem / pulsująca; Termos z liczbą kaw; Czekaj – trzymanie: karta
  problemu; Telefon – trzymanie: mapa etapu), przesunięcie palcem = krok (trzymanie – kolejne kroki), dotknięcie
  pola = marsz krok po kroku (staje przy nowym problemie, obrażeniach, problemie obok), dotknięcie problemu = atak
  albo podejście, przytrzymanie = karta; opcjonalnie gałka i pasek dla lewej ręki; wibracje przy trafieniu,
  obrażeniach i powiadomieniu. W telefonie: przyciski stron zamiast podpowiedzi klawiszy, dotyk wierszy,
  przesunięcie w bok = zakładka. Teksty z GBA (rady kierownika) mówią o przyciskach paska („Przytrzymaj Atak”).
- Ustawienia pod kluczem w rogu (tytuł i mapa, Esc): głośności, wibracje, sterowanie, ręka, tekst, Jak grać,
  Zapisz i wyjdź / Porzuć budowę (zapis budowy jak `run_save` na GBA, „Kontynuuj budowę” na tytule), wersja
  i link planbudowlany.online (także na tytule). `user://settings.cfg` osobno od profilu.
- Eksport iOS (projekt Xcode) i Android w `export_presets.cfg`, ikona aplikacji z eksportera,
  `tools/ios_deploy.sh` – build, podpis i instalacja na iPhonie jednym poleceniem.
- Dalej: 2.5D i światło (kamień 4), zrzut ekranu z urządzenia w automatycznych testach.

