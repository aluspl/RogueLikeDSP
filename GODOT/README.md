# PlanBudowlany RogueLike – Godot 4 + C#

Wersja Godot gry z demo GBA (`../GBA`): roguelike budowlany, w którym etapy budowy są piętrami lochu,
a wrogami są *problemy budowy*. Kierunek rozwoju: [`docs/KONCEPCJA.md`](docs/KONCEPCJA.md)
(telefon z aplikacją PlanBudowlany jako interfejs, oprawa 2.5D – kolejne kamienie milowe).

**Stan: zgodny z GBA v0.21.47** (logika, dane i test złoty z migawki GBA v0.21.47: pogoda dnia, brygada, tryb
inwestora, boss Inspekcja Pracy); oprawa (grafika, font, dźwięk, telefon, wybór zawodu) jak w GBA v0.21.47.

## Co jest

- **`src/LifeLike.Core`** – cała logika gry przeniesiona 1:1 z `GBA/include/core.h` i `meta.h`
  (bez zależności od Godota): generator map xorshift32 (pokoje + korytarze L), pole widzenia (shadowcasting),
  walka i AI problemów, moce zawodów z rangami, stany z danych (`statuses`: zatrucie, porażenie, poślizg,
  papierologia – komunikat ze skutkiem i czasem), szczęście zawodów (kryt x2, unik, częstsze i lepsze dropy – sekcja
  `luck`), sprzęt z cechami (`equipment.traits`) i porównaniem przy paczce (zakładam / zostawiam za `declineXp`),
  termos (`thermos`: kawa trafia do termosu, picie zużywa turę),
  narzędzia, dropy, akty z bossami i zapowiadanym uderzeniem, Hurtownia, poziomy postaci, NG+, dziennik
  (rodzaje, powtórzenia, bufor 48 bajtów UTF-8 jak na GBA), celowanie, profil gracza (Szkolenia, odblokowania,
  odznaki, Katalog usterek, Osiedle, bankowanie doświadczenia, migracje zapisu v1/v2/v3), zapis budowy w trakcie
  (format `PBRUN03`).
  Od v0.21.43: statystyki efektywne (zawód + Warsztaty na statystykę broni zawodu + cechy sprzętu SIŁ/ZRĘ/INT),
  Szkolenia Kurs BHP II i Warsztaty, **uprawnienia** (każda odznaka daje trwałą premię na budowę, `badges[].perk`),
  **pamiątki** (`keepsakes`: wybór na starcie, rangi I–III po 3 i 8 budowach, odblokowanie od startu / odznaką / zleceniem),
  **zlecenia** (`contracts`: liczniki w profilu – usunięte problemy, moce, markowy sprzęt, czyści bossowie, wygrane – nagroda
  w doświadczeniu i pamiątce, postęp na żywo w trakcie budowy), **pogoda dnia** (`weather`: upał, mróz, wiatr,
  kałuże w deszczu; `WeaponRange`), **brygada** (`brigade`: fachowiec raz na etap za zł – Geodeta, BHP-owiec, Pompa do
  betonu, pomocnik Elektryk-kolega; `CallHelper`), **tryb inwestora** (`investor`: modyfikatory po pierwszej wygranej,
  stawka i rekord stawki zawodu), **wydarzenia na placu** (`siteEvents`: SMS na starcie etapu
  bez bossa, 45% – mniej znajdziek, premia, inspekcja, ulewa z poślizgiem, pełny termos). Profil v6 (`PBRL006`, 88 bajtów,
  układ jak SRAM na GBA; migracje v1–v5 zachowują stare pola, nowe zerują), zapis budowy `PBRUN06`.
  **Ten sam seed daje identyczną grę co na GBA** (ta sama kolejność wywołań RNG, arytmetyka całkowita,
  rzutowania int8/int16).
- **`src/LifeLike.Core/Bot.cs`** – deterministyczny bot z testów GBA (testy balansu, test złoty, test dymny);
  bierze lepszą paczkę sprzętu (gorszą zostawia) i pije z termosu poniżej `thermos.botDrinkBelowPct` HP.
- **`tests/LifeLike.Core.Tests`** – testy xUnit przeniesione z `GBA/tests/core_tests.cpp` (łącznie z balansem
  na botach, z mniejszą liczbą przebiegów) + test złoty.
- **`godot/`** – grywalna wersja 2D z oprawą jak na GBA (kamienie milowe 2 i część 4 z KONCEPCJI):
  interfejs 640x360 pikseli UI przy 1280x720 (stretch `canvas_items`, aspect `expand`, skala całkowita – inne proporcje
  okna dają więcej miejsca, bez pasów), mapa przybliżona do ~9 pól w pionie jak na GBA, HUD w 1.5x, kafle etapów 32x32 w paletach z GBA, postacie, problemy budowy,
  bossowie i znajdźki z GBA (Scale2x) z cieniem, płynnym ruchem, szturchnięciem przy ataku i błyskiem trafienia,
  mgła wojny z miękkim światłem, czerwone pola zapowiedzianego ciosu bossa, znacznik celu, „!”, mini paski HP,
  liczby obrażeń / „KRYT!” / „Unik!”, cząsteczki (pył, iskry, moce zawodów, awans, dropy, konfetti), wstrząs i błyski
  ekranu, podgląd mapy (M) dopasowany do odkrytej części etapu, chód w 4 klatkach i oddech postaci, celowanie pod
  trzymanym A (zasięg, celownik, strzałki zmieniają cel) i karta wroga pod trzymanym B, prolog przy pierwszej budowie
  (pickup wjeżdża na plac, kamera jedzie przez działkę, SMS od inwestorki) i ekran „Jak grać”. HUD jak na GBA: pasek HP, poziom z doświadczeniem, etap, stany z turami, termos, ikona
  mocy (szara z odliczaniem / pulsująca), dziennik z gasnącymi kolorowymi liniami, menu akcji wokół bohatera.
  **Telefon z aplikacją PlanBudowlany** pionowo na środku ekranu (mapa rozmyta i przyciemniona, wysuwa się z dołu,
  akcje poza czasem gry): w grze Zadania, Usterki, Start, Sprzęt, Koszty; na tytule profil – Odznaki / Zlecenia /
  Pamiątki, Katalog, Osiedle, Zespół, Koszty = sklep Szkolenia; wiadomości (karta etapu z SMS-em wydarzenia),
  paczka sprzętu, harmonogram z radą kierownika (`tips` z game.json, przyciski GBA zamienione na klawisze), Hurtownia;
  powiadomienia push z dźwiękiem (na mapie w prawym górnym rogu pod HUD, kliknięcie otwiera zakładkę telefonu). Ekrany: tytuł z logo i wersją z game.json,
  wybór zawodu (karuzela portretów – odblokowane najpierw, karta z mocą, narzędziem, paskami statystyk z premią,
  trudnością i pamiątką), koniec gry z kodem QR i konfetti. Dźwięki z GBA na tych samych zdarzeniach, muzyka tytułu
  i gry. Profil zapisuje się w `user://profile.sav` (układ bajtów jak SRAM na GBA).

## Sterowanie (jak na GBA)

| Akcja | Klawiatura | Pad | Mysz |
|---|---|---|---|
| Ruch / atak wroga na drodze | strzałki, WSAD, numpad | D-pad, lewa gałka | lewy klik obok = krok |
| A: atak najbliższego celu w zasięgu (bez celu miga zasięg); trzymaj: celownik, strzałki zmieniają cel, puść = atak | Spacja, X, J | A | lewy klik na wroga w zasięgu |
| B: czekaj turę; trzymaj: karta wroga (strzałki: następny), bez zużycia tury | Z, Kp5, `.` | B | prawy klik |
| R: moc zawodu | R, F | RB | |
| START: menu akcji (strzałka wybiera, ta sama strzałka / A wykonuje, Enter/B zamyka) | Enter | Start | |
| SELECT: telefon (zakładki: Q/E albo ←/→, zamknięcie: Tab/Esc/Z) | Tab | Select | |
| L: podgląd mapy etapu (dowolny klawisz wraca) | M | L3 | |
| Paczka sprzętu: A zakładam / B zostawiam | Spacja / Z | A / B | |
| Dalej (wiadomości, harmonogram, Hurtownia) | Enter / Spacja | Start / A | |
| Powiadomienie push: otwórz jego zakładkę w telefonie | | | lewy klik / dotknięcie |
| Prolog: pomiń | Spacja / Enter | A / Start | klik |
| Tytuł: menu (Nowa budowa, Profil, Szkolenia, Jak grać) | ↑/↓ + Enter | D-pad + Start | |
| Wybór zawodu: zawód / trudność / pamiątka / start / wróć | ←/→, ↑/↓, Q/E, Enter, Esc | D-pad, LB/RB, Start, B | |
| Profil w telefonie (odznaki, zlecenia, pamiątki; Spacja zmienia stronę) | P | X | |
| Brygada (menu akcji: Spacja bez kierunku; telefon: Sprzęt → Spacja) | Enter, Spacja | Start, A | |
| Tryb inwestora na wyborze zawodu (po pierwszej wygranej; Spacja włącza modyfikator) | Tab | Select | |
| Szkolenia (zakładka Koszty w telefonie profilu, Spacja kupuje) | K | Y | |
| Ustawienia (także klucz w prawym górnym rogu tytułu i mapy) | Esc | | klik na klucz |

## Telefon: pion i dotyk jedną ręką

Na iOS/Androidzie (albo z `--touch` na komputerze) gra jest pionowa i sterowana kciukiem – bez przycisków A/B:

| Gest | Co robi |
|---|---|
| Przesunięcie palcem po mapie | krok w tę stronę (atak problemu na drodze); palec trzymany dalej = kolejne kroki |
| Dotknięcie pola | marsz krok po kroku (każdy krok to tura); staje, gdy pojawi się problem, gdy jest obok albo gdy oberwiesz |
| Dotknięcie problemu | atak, jeśli w zasięgu, inaczej marsz do niego (do zasięgu) |
| Przytrzymanie problemu | jego karta (HP, obrażenia, opis), bez tury |
| Pasek akcji: **Atak** | dotknięcie = najbliższy cel; trzymanie = zasięg i celownik, palec przesuwa cel, puszczenie = atak |
| **Moc** | moc zawodu (szara z odliczaniem, pulsuje, gdy gotowa) |
| **Termos** | kawa z termosu (liczba kaw na przycisku) |
| **Czekaj** | dotknięcie = tura; trzymanie = karta najbliższego problemu (przesunięcie: następny) |
| **Telefon** | dotknięcie = aplikacja PlanBudowlany na cały ekran; trzymanie = podgląd mapy etapu |
| Telefon → **Sprzęt → Brygada** | fachowiec raz na etap (przycisk „Wezwij”, zużywa turę) |
| Wybór zawodu: **Tryb inwestora** | wiersz pod pamiątką po pierwszej wygranej – modyfikatory i stawka |
| Pasek HUD u góry | pulpit postaci w telefonie |
| Telefon | ikony zakładek, przesunięcie w bok = zakładka, w górę/dół = lista, przyciski na dole strony, krzyżyk zamyka |

Ustawienia (klucz 🔧 w prawym górnym rogu, poza czasem gry): muzyka, dźwięki, wibracje, sterowanie (gesty + pasek
albo gałka + pasek), pasek akcji dla prawej / lewej ręki, tekst normalny / duży, Jak grać, w trakcie budowy
Zapisz i wyjdź (na tytule „Kontynuuj budowę”) i Porzuć budowę, wersja i link planbudowlany.online. Zapis w
`user://settings.cfg` (osobno od profilu `user://profile.sav`); budowa w toku w `user://run.sav` (start etapu,
Zapisz i wyjdź, uśpienie aplikacji).

Układ: `Layout` pionowo liczy od 360x640 (iPhone 14 Pro Max 1290x2796 -> UI 430x932 w skali 3, piksel UI = punkt
iOS, cele dotyku ≥ 44 pt), poziomo od 640x360; HUD pod wyspą (`DisplayServer.GetDisplaySafeArea()`), pasek akcji
nad paskiem domowym, mapa ~9 pól na szerokość, telefon jako aplikacja na cały ekran.

## Wymagania i uruchomienie

- Godot 4.7 w wersji .NET (np. `brew install --cask godot-mono`), .NET 8 SDK (lub nowszy z runtime 8).
- W Godot: *Import* → `GODOT/godot/project.godot`, uruchom scenę `scenes/Main.tscn`.
- Z terminala: `godot-mono --path GODOT/godot` (opcjonalnie `-- --seed 1234`).
- Test dymny bez okna (bot gra 3 etapy przez warstwę Godota, kod 0 = OK):
  `godot-mono --headless --path GODOT/godot -- --smoke`
- Podgląd wersji na telefon na komputerze: `godot-mono --path GODOT/godot -- --touch --portrait` (okno 430x932 jak
  iPhone w punktach, z symulowaną wyspą i paskiem domowym; `--touch` samo = dotyk myszą w poziomie,
  `--size 860x1864` = inny rozmiar okna). Działa też ze zrzutami (`--touch --portrait --screenshot ...`).
- Zrzut ekranu (1280x720) sceny pokazowej: `godot-mono --path GODOT/godot -- --screenshot /tmp/shot.png --scene game`.
  Sceny: `title`, `classselect`, `profile`, `catalog`, `estate`, `team`, `training` (telefon profilu), `game`, `combat`
  (KRYT!/Unik!), `offer` (paczka sprzętu), `menu` (menu akcji), `phone-tasks`, `phone-issues`, `phone-start`
  (= `overview`), `phone-gear`, `phone-costs`, `card` (karta etapu z wydarzeniem), `perks` (HUD z premiami i wydarzeniem),
  `schedule`, `hurtownia`, `boss` (boss z zapowiedzianym ciosem), `endmsg`, `end`, `banners`, `map`, `aim` (trzymane A),
  `preview` (trzymane B), `prologue`, `schedule-tip` (rada kierownika), `help` (Jak grać), `settings` (ustawienia
  w budowie), `settings-title`, `walk` (marsz po dotknięciu pola), `weather-rain`, `weather-snow`, `weather-wind`,
  `weather-heat` (pogoda dnia: nakładka, kałuże, ikona w HUD), `brigade` (telefon: Brygada), `ally` (pomocnik obok
  bohatera), `investor` (tryb inwestora nad wyborem zawodu).
  Sceny ustawiają stan ręcznie (profil pokazowy, skrót zaliczenia etapu jak L+R+SELECT na GBA); zrzuty i test dymny
  działają bez dźwięku.

### iPhone (iOS)

`GODOT/tools/ios_deploy.sh` – wersja z `GBA/data/game.json` do presetu, `dotnet build`, eksport projektu Xcode
(`godot-mono --headless --export-debug iOS`, preset w `godot/export_presets.cfg`), `xcodebuild` z automatycznym
podpisem (klucz API App Store Connect), instalacja i uruchomienie przez `xcrun devicectl` na iPhonie (USB albo Wi-Fi).
Opcje: `--export` (tylko projekt Xcode), `--no-launch`; zmienne `UDID` (domyślnie iPhone Szymona), `ASC_KEY`
(ścieżka do `.p8`, domyślnie w `~/Dev/PlanerBudowlany/Organizacja/Mobile/certs`; klucza nie ma w repo), `TEAM_ID`,
`BUNDLE_ID`. Wyniki w `GODOT/build/ios` (poza gitem). Wymaga szablonów eksportu 4.7.2.stable.mono, Xcode i:
`rendering/textures/vram_compression/import_etc2_astc=true` w project.godot (bez tego eksport iOS kończy się po cichu)
oraz `godot/LifeLike.Game.sln` obok project.godot (eksport C# szuka tam rozwiązania; testy dalej z `GODOT/LifeLike.sln`).
Ikona `godot/icon.png` (1024x1024) powstaje w `tools/export_godot_assets.py`. Preset „Android” jest bez sekretów podpisu
(klucz debug z ustawień edytora).

Przy pierwszym uruchomieniu z terminala najpierw `dotnet build GODOT/godot/LifeLike.Game.csproj`
i `godot-mono --headless --path GODOT/godot --import` (import grafik i dźwięków z `godot/assets`).

## Grafika, font i dźwięk z GBA

`python3 GODOT/tools/export_godot_assets.py` (Pillow; do muzyki `openmpt123` i `ffmpeg` z libmp3lame) czyta – tylko
czyta – `GBA/graphics/*.bmp`, `GBA/include/font_widths.h` i `GBA/audio/*` i zapisuje do `godot/assets/`:
`sprites/` (postacie, wrogowie, bossowie, znajdźki, paczki, celownik – klatki 32x32 powiększone algorytmem Scale2x
z 16x16, kolejność klatek jak `actors.bmp`; białe sylwetki do błysku; cząsteczki 16x16; domy Osiedla; ikony menu
i mocy), `ui/` (ikony telefonu aktywne i nieaktywne, ikony HUD 16x16 i szare do ładowania mocy, plansze tytułu
i końca z przezroczystym tłem), `tiles/stage_N.png` (8 etapów: 4 warianty podłogi, podłoga z cieniem muru, mur,
lico muru, schody – rysowane w 32x32 w paletach etapów z GBA, bogatsze niż kafle 8x8), `fx/` (cień, pole ciosu,
ramka zasięgu), `font/` (font 8x16 z polskimi znakami: litery i cień osobno + `font.json` z szerokościami;
rysuje go `scripts/Gfx/PixelFont.cs`), `audio/` (SFX `.wav` 1:1, muzyka `.mod` wyrenderowana do `.mp3`).
Wynik jest deterministyczny – po zmianie grafik GBA wystarczy uruchomić skrypt ponownie i zaimportować projekt.

## Skąd dane

Jedno źródło prawdy: **`GBA/data/game.json`** (ten sam plik, z którego GBA generuje `include/game_data.h`
skryptem `tools/gen_data.py`). `GameData` interpretuje go tak samo jak `gen_data.py` (identyfikatory → indeksy,
maski startowych zawodów/narzędzi i poziomów), a nieznane pola ignoruje.

- **Gra w Godot:** target MSBuild `CopySharedGameData` w `godot/LifeLike.Game.csproj` kopiuje przy każdym buildzie
  `GBA/data/game.json` do `godot/data/game.json` (plik w `.gitignore`), a gra czyta `res://data/game.json`.
  Przy eksporcie dodaj filtr zasobów `data/*.json`.
- **Testy:** używają zamrożonej kopii `tests/LifeLike.Core.Tests/golden/game.json` (z migawki GBA, z której zrobiono
  złote przebiegi), więc nie zmieniają się razem z bieżącymi pracami nad GBA. Osobny test sprawdza tylko,
  czy aktualny `GBA/data/game.json` da się wczytać.

## Test złoty (zgodność z GBA)

`GBA/tests/golden_dump.cpp` kompiluje się z nagłówkami GBA i rozgrywa 20 budów deterministycznym botem
(wszystkie zawody i poziomy, bot z `core_tests.cpp` oraz wariant z mocą i celowaniem, Hurtownia, pełne Szkolenia, NG+,
profile z odznakami – uprawnienia – i wybraną pamiątką w rangach I–III, zleceniami; wydarzenia na placu wypadają same;
oba boty piją z termosu i decydują o paczkach sprzętu – bot z testów bierze lepszą, wariant „smart” także tej samej
jakości, czyli wymienia cechę). Zapisuje `golden/run_XX.json`: skrót FNV stanu po każdym kroku (`StateDigest`, łącznie
z trafieniami tury – kryt/unik – cechami sprzętu, termosem i paczką czekającą na decyzję; po kroku lista trafień
jest zerowana jak w warstwie GBA), pełne zrzuty na starcie każdego etapu i na końcu (mapa, mgła, wrogowie, znajdźki
z cechami, dziennik bajt po bajcie, wydarzenie na placu, liczniki zleceń, statystyki efektywne, …) oraz profil po budowie
(pola i cały zapis bajt po bajcie jak w SRAM).
`GoldenTests` odtwarza to samo w C# i porównuje pole po polu.

Odtworzenie plików (z tej samej wersji nagłówków GBA co `golden/game.json`; obecnie migawka v0.21.47 – bot „smart”
wzywa też brygadę, dwa przebiegi z trybem inwestora,
np. `git show d02ba811:GBA/...` rozpakowane do osobnego katalogu `<gba_v43>`):

```bash
g++ -std=c++20 -O2 -I<gba_v43>/include GBA/tests/golden_dump.cpp -o /tmp/golden_dump
/tmp/golden_dump GODOT/tests/LifeLike.Core.Tests/golden
cp <gba_v43>/data/game.json GODOT/tests/LifeLike.Core.Tests/golden/game.json
```

Parser danych ignoruje nieznane pola, a nieznane wartości efektów Szkoleń i cech sprzętu (z nowszych wersji
`GBA/data/game.json`) wczytuje jako `Unknown` – bez działania, dopóki port ich nie obsłuży (tak samo nieznane
uprawnienia/pamiątki; dane bez sekcji `contracts`/`keepsakes`/`siteEvents` wczytują się z pustymi listami).

## Testy

```bash
dotnet test GODOT/LifeLike.sln
```

## Struktura

```
src/LifeLike.Core/          logika gry (Game*.cs, Level, Rng, Meta, Profile, RunSave, Bot, Data/GameData)
tests/LifeLike.Core.Tests/  testy xUnit + golden/ (dane z migawki GBA i złote przebiegi)
tools/                      export_godot_assets.py – grafiki, font i dźwięki z GBA do godot/assets
godot/                      projekt Godota (UI 640x360 przy 1280x720, dowolne proporcje okna)
godot/assets/               wynik eksportu z GBA (PNG, font, WAV, MP3) + pliki .import
godot/scripts/              prezentacja (C#, katalog = przestrzeń nazw LifeLike.Game.*) - patrz „Architektura”
godot/data/                 kopia GBA/data/game.json robiona przy buildzie (poza gitem)
```

## Architektura warstwy Godota (`godot/scripts`)

Katalog = przestrzeń nazw (`LifeLike.Game.<Katalog>`), jeden typ na plik. Logika gry jest wyłącznie w `LifeLike.Core`;
prezentacja tylko ją pokazuje i reaguje na zdarzenia.

```
Main.cs            korzeń sceny: dane + profil -> App; wejście (klawiatura/pad/mysz/wirtualny kontroler) do ekranu bieżącego
App.cs             kompozycja: GameSession, SceneNodes, ScreenFlow, obserwatorzy zdarzeń; Refresh / AfterAction / StartRun
SceneNodes.cs      drzewo węzłów: WorldView, HudLayer (1), plansze (2), telefon z tłem (3), banery (4)
LaunchOptions.cs   argumenty --seed / --smoke / --screenshot --scene / --touch / --portrait / --size
Input/             GameAction (A, B, L, R, START, SELECT, strzałki...), InputCmd (zdarzenie jako akcje, wciśnięte
                   i puszczone), GameInput (mapa klawiszy i pada, Translate, Press/Release dla przycisków
                   ekranowych, IsHeld), ButtonNames (A/B/START... w tekstach z game.json -> klawisze)
Session/           GameSession (budowa + profil: start, etap, NG+, odznaki, zlecenia, bankowanie, zapis),
                   SessionEvents (LevelUp, PickedUp, Dropped, ToolFound, GearEquipped, AbilityReady, BossSpotted,
                   StageCleared, RunEnded, Achievements), TurnWatcher (wykrywa zdarzenia tury), GodotDataSource
Screens/           Screen (Enter / Exit / HandleInput / Process + deklaracja warstw i muzyki), ScreenFlow (maszyna
                   stanów), ekrany: Title, Profile, ClassSelect, Prologue, PrologueMessage, Help, Game, Phone,
                   StageCard, Schedule, Hurtownia, Offer, EndMessage, End; Play/ (tryby mapy: ActionMenu, Aiming,
                   EnemyLook, PlayCommands, TouchPlay - gesty na mapie, AutoWalk + PathFinder -
                   marsz po dotknięciu pola); Settings (ustawienia nad bieżącym ekranem); Views/ (TitleView, ClassSelectView, EndView, PrologueView, PrologueStage)
World/             WorldView (sprite'y, synchronizacja), WorldFx (trafienia, moce, awans, konfetti), WorldCamera,
                   warstwy: MapLayer, OverlayLayer, FogLayer, FxLayer, MarksLayer, ActorSprite
Touch/             GestureTracker (dotyk -> gesty: Down, Drag, Swipe, SwipeRepeat, LongPress, Tap, Up), Gesture,
                   TouchControls (warstwa: ActionBar - pasek akcji, VirtualStick - gałka), BarButton, TouchIcon
Settings/          GameSettings (user://settings.cfg, Changed), ControlScheme, Haptics (wibracje przy dźwiękach)
Hud/               SettingsButton (klucz ustawień), HudLayer (HudTop, HudLog, EnemyCard, ScreenTint), PushBanners + PushBanner (rysowanie, trafienie
                   kliknięciem), BannerFeed (treść i zakładka telefonu z SessionEvents)
Phone/             PhoneView (telefon), PhonePage, PhonePainter, PhoneTabs, Backdrop; Tabs/ (w grze), ProfileTabs/,
                   Pages/ (wiadomość, harmonogram z radą, Hurtownia, paczka, Jak grać)
Gfx/               Pal (tokeny kolorów PlanBudowlany i GBA), Ink, Layout (rozmiar UI, skale HUD i mapy, bezpieczny
                   obszar - jedyne miejsce), ScaledLayer (warstwa z własną skalą), Assets, PixelFont (skala
                   ułamkowa), Ui, UiText, DrawHook, DrawErrors
Audio/             Sfx (dźwięki i muzyka), SoundCues (dźwięki zdarzeń sesji)
Debug/             DebugRunner (--smoke / --screenshot), SmokeTest, ScreenshotRunner, DebugScenes, DemoStaging, DemoProfile
```

Przepływ: `Main` tłumaczy zdarzenie na `InputCmd` -> `ScreenFlow.Current.HandleInput` -> ekran woła akcję rdzenia
(np. `PlayerMove`) -> `App.AfterAction` -> `Refresh` (widok mapy zużywa trafienia tury, `TurnWatcher` zgłasza
zdarzenia -> banery / dźwięki / efekty) -> `GameSession.Resolve` (etap zaliczony, koniec budowy, paczka) -> kolejny ekran.

Skalowanie: `Layout` liczy wszystko od widocznego prostokąta okna – `ContentScale` (piksele okna na piksel UI,
całkowita), `HudScale` (1.5x, zaokrąglone tak, by piksel był całkowitą liczbą pikseli okna), `WorldZoom` (~9 pól
w pionie), `SafeArea` (wycięcia ekranu na telefonie). Widoki układają się od `Size`/`UiSize` (kotwice), więc układ
pionowy (telefon) wymaga tylko własnych proporcji elementów, bez zmian w logice ekranów. Przyciski ekranowe
wstrzykują akcje przez `GameInput.Press/Release` (ta sama ścieżka co klawiatura, łącznie z trzymaniem A/B).
