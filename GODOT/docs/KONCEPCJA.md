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
