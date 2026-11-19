# Projekt A – wygląd i przepływ ekranów (Godot) – plan wdrożenia

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ekrany Godota pokazują treść gry (pełne nazwy zawodów, bronie, doświadczenie) i po każdej budowie prowadzą do kolejnej. Robią to SMS-y od Kierownika Marka z szybkimi odpowiedziami, tytuł z kartą postępów i porządny układ na dużym ekranie.

**Architecture:** Logika „co kupić / co dalej” trafia do nowej klasy rdzenia `Spend` (testy xUnit). Rdzeń jest bez zmian w symulacji, więc test złoty się nie zmienia. Ciąg plansz po budowie (`EndMessageScreen` → `HouseScheduleScreen` → `RecapScreen` → `EndScreen`) zastępuje: harmonogram domu (tylko po wygranej, z kodem QR) → nowy `BetweenRunsScreen` z rozmową w telefonie. Podsumowanie budowy jest dostępne z rozmowy. Duży ekran to tryb `Layout.Wide` na bazie układu pionowego.

**Tech Stack:** Godot 4.7.2 .NET (C#, net10.0), xUnit, dane `GBA/data/game.json` (`uiGodot`) + `GBA/data/lang/en.json`.

**Spec:** `docs/superpowers/specs/2026-10-23-ux-pakiet-a-design.md` (zatwierdzony 2026-10-23). Makiety: `docs/ux/pakiet-a/*.html`, zrzuty „przed”: `docs/ux/pakiet-a/sheet_*.png`.
Zastępuje plan `docs/superpowers/plans/2026-10-22-ux-xp-flow.md` (bez modala przy starcie, bez zmian w GBA).

## Global Constraints

- Branch `dev/0.22`, wersja v0.22.0. Po pakiecie changelog i pytanie „wydajemy?”, bez osobnego wydania.
- **Tylko Godot.** `GBA/` bez zmian (poza tekstami w `GBA/data/*.json`, które są wspólnym źródłem). Logika gry, balans i ceny bez zmian: `dotnet test` z testem złotym przechodzi bez aktualizacji `golden/`.
- Teksty tylko w `GBA/data/game.json` → `uiGodot` (PL) i `GBA/data/lang/en.json` (EN). Po każdej zmianie: `python3 GBA/tools/gen_data.py --check` → OK.
- C#: jeden typ na plik, Allman, 4 spacje, bez `Type?`, komentarze `/// v0.22.0 (UX A): …` jak w plikach obok. Przed commitem: `git diff -w --stat` vs `git diff --stat`. Same zmiany tabów z otwartego edytora w `App.cs`/`Main.cs`/`SceneNodes.cs` cofnąć.
- Tokeny wyglądu: tło `#f6f5f8`, białe karty, fiolet `#6b4eff`, statusy z `Pal.cs`. Nowe ekrany korzystają z `PhonePainter` / `Ui` / `Pal`, bez nowych kolorów na sztywno.
- Klawiatura i pad działają na każdym zmienionym ekranie. Skróty mogą zniknąć z tekstu, nie z obsługi.
- Doświadczenie wydaje się w Szkoleniach (`Flow.Profile.Open(4)`, `TrainingTab`), nigdy w Hurtowni.
- Weryfikacja po każdym zadaniu: `dotnet test GODOT/LifeLike.sln`, build Godota, `--smoke`, zrzuty dotkniętych scen w czterech rozmiarach (niżej), `--monkey` dla 1–2 seedów. Pełny `rogue-test` dopiero przed wydaniem.
- Commity małe, po każdym zadaniu, bez push/tagów/`--force` (pushuje koordynator).

Polecenia (z katalogu repo, `$O` = `/private/tmp/claude-501/-Users-szymonmotyka-Dev-Gamedev-RogueLikeDSP/3e5ad036-1c8d-4d9f-b234-8c7b630db62c/scratchpad/ux2`):

```bash
godot-mono --headless --path GODOT/godot --build-solutions --quit          # build
godot-mono --headless --path GODOT/godot -- --smoke                        # test dymny
shot() { godot-mono --path GODOT/godot -- --screenshot $O/$1.png --scene $1;                                   # 1280x720
         godot-mono --path GODOT/godot -- --touch --portrait --screenshot $O/$1-p.png --scene $1;              # telefon
         godot-mono --path GODOT/godot -- --touch --size 1640x1460 --screenshot $O/$1-fl.png --scene $1;      # składak poziomo
         godot-mono --path GODOT/godot -- --touch --portrait --lang en --screenshot $O/$1-en.png --scene $1; } # EN
godot-mono --path GODOT/godot -- --monkey 11 600 --touch --portrait        # małpa
```

## Review Focus

1. **Budowa dnia i tygodnia kończą się na nowym ekranie bez NG+.** „Kolejna budowa” prowadzi do zwykłej budowy (mapa kariery / wybór zawodu), a nie do kolejnej codziennej. → asercje w teście dymnym (Task 5, kroki 4–5).
2. **Gracz, którego nie stać na nic, i gracz z wszystkim kupionym** (`NextUnlock == -1`): rozmowa bez „Kup”, bez pustych linii, bez wyjątku. → testy `Spend` (Task 1) + scena `between-broke` (Task 5).
3. **Węzeł drzewka jako najtańsza rzecz:** brak zakupu jednym stuknięciem, w zamian „Pokaż Szkolenia”. → test `QuickBuyKindSkipsTreeNode` (Task 1).
4. **Kilka zakupów pod rząd w rozmowie:** po każdym „Kup” Marek odpisuje, oferta przechodzi na następną rzecz, doświadczenie nie schodzi poniżej 0, profil zapisany. → test `QuickBuyTwiceBuysNextCheapest` (Task 1) + asercja w teście dymnym (Task 5).
5. **EN i składak:** dłuższe teksty (EN) i układ `Wide` bez ucinania i nachodzenia. → zrzuty `-en` i `-fl` w każdym zadaniu UI, jawnie w Taskach 5, 7, 8.

---

### Task 1: Rdzeń – `Spend` (co kupić, szybki zakup, następna nagroda) i wspólna nazwa pozycji

**Files:**
- Create: `GODOT/src/LifeLike.Core/Spend.cs`
- Modify: `GODOT/src/LifeLike.Core/Recap.cs:37-73` (wydzielenie nazwy do `public static string UnlockName(...)`)
- Modify: `GODOT/godot/scripts/Phone/Pages/RecapPage.cs:135` i `GODOT/godot/scripts/Screens/EndMessageScreen.cs:55-66` (wołają `Recap.UnlockName`, prywatne duble usunięte)
- Test: `GODOT/tests/LifeLike.Core.Tests/SpendTests.cs` (nowy)

**Interfaces:**
- Produces:
  - `Recap.UnlockName(GameData d, Profile p, int kind, int idx) : string` (dokładnie dzisiejszy switch z `Recap.Goal`)
  - `Spend.QuickKind(GameData d, Profile p, out int kind, out int idx) : int`: koszt najtańszej pozycji z `Meta.NextUnlock`, którą stać i którą da się kupić bez wyboru (kind 0–4), albo -1. Gdy najtańsza jest węzłem drzewka (kind 5), szuka najtańszej w kind 0–4 (to ten sam rachunek co `NextUnlock`, ale z pominięciem 5).
  - `Spend.QuickBuy(GameData d, Profile p) : bool`: kupuje to, co wskazuje `QuickKind`, przez istniejące `Meta.BuyUpgrade/BuyClass/BuyTool/BuyHelper/BuyHard`.
  - `Spend.TreePick(GameData d, Profile p) : int`: indeks otwartego, niewybranego węzła drzewka, na który stać, albo -1 (do linii „W drzewku czeka wybór”).
  - `Spend.NextReward(GameData d, Profile p, out int toolWeapon) : int`: indeks w `d.Rewards` nagrody za następny odbiór (`p.Rewards`, gdy < `RewardsAvailable`), inaczej -1. `toolWeapon` = indeks broni (`d.Tools[r.Index].Weapon`), gdy `Kind == RewardKind.Tool`, inaczej -1.

- [ ] **Step 1: Testy (failing)** w `SpendTests.cs` (wzór: `ProfileAndShopTests`, `TestData.D`):

```csharp
[Fact]
public void QuickKindNothingWhenBroke()
{
    var p = Meta.NewProfile(D);
    p.Xp = 0;
    Assert.Equal(-1, Spend.QuickKind(D, p, out _, out _));
    Assert.False(Spend.QuickBuy(D, p));
    Assert.Equal(0, p.Xp);
}

[Fact]
public void QuickBuyTwiceBuysNextCheapest()
{
    var p = Meta.NewProfile(D);
    p.Xp = 10000;
    var c1 = Spend.QuickKind(D, p, out var k1, out var i1);
    Assert.True(c1 >= 0 && k1 is >= 0 and <= 4);
    Assert.True(Spend.QuickBuy(D, p));
    Assert.Equal(10000 - c1, p.Xp);
    var c2 = Spend.QuickKind(D, p, out var k2, out var i2);
    Assert.True(c2 >= c1 || (k2, i2) != (k1, i1));
    Assert.True(Spend.QuickBuy(D, p));
    Assert.Equal(10000 - c1 - c2, p.Xp);
}

[Fact]
public void QuickKindSkipsTreeNode()
{
    // Wszystko w rodzajach 0-4 wykupione/za drogie, otwarty węzeł drzewka tani: brak szybkiego zakupu, jest TreePick.
    var p = Meta.NewProfile(D);
    p.Xp = 100000;
    while (Spend.QuickBuy(D, p)) { }   // wykupione rodzaje 0-4
    p.Xp = 100000;
    var node = Spend.TreePick(D, p);
    Assert.True(node >= 0);
    p.Xp = D.TreeNodes[node].Cost;
    Assert.Equal(-1, Spend.QuickKind(D, p, out _, out _));
    Assert.True(Meta.NextUnlock(D, p, out var k, out _) >= 0);
    Assert.Equal(5, k);
}

[Fact]
public void NextRewardToolHasWeapon()
{
    var p = Meta.NewProfile(D);
    var r = Spend.NextReward(D, p, out var w);
    Assert.Equal(0, r);
    Assert.Equal(D.Rewards[0].Kind == RewardKind.Tool ? D.Tools[D.Rewards[0].Index].Weapon : -1, w);
    p.Rewards = (byte)Meta.RewardsAvailable(D);
    Assert.Equal(-1, Spend.NextReward(D, p, out _));
}
```

(Typ pola `p.Rewards` sprawdź w `Profile.cs` i dopasuj rzutowanie.)

- [ ] **Step 2:** `dotnet test GODOT/LifeLike.sln --filter "FullyQualifiedName~SpendTests"` → FAIL (brak `Spend`).
- [ ] **Step 3:** `Spend.cs` (statyczna klasa, jeden typ) + `Recap.UnlockName` + przepięcie `RecapPage`/`EndMessageScreen` na `Recap.UnlockName`.
- [ ] **Step 4:** `dotnet test GODOT/LifeLike.sln` → PASS (wszystkie, test złoty bez zmian).
- [ ] **Step 5: Commit** `"v0.22.0 UX A: rdzeń – Spend (szybki zakup, następna nagroda), wspólna nazwa pozycji Szkoleń"`

### Task 2: Duży ekran – tryb `Layout.Wide` i telefon bez rozciągania

**Files:**
- Modify: `GODOT/godot/scripts/Gfx/Layout.cs` (`ScaleFor`, `Portrait`, nowe `Wide`)
- Modify: `GODOT/godot/scripts/Phone/PhoneView.cs:49-53` (`Full`, `PhoneSize`, `Origin`)

**Interfaces:**
- Produces: `Layout.NearSquare : bool` (dotyk i dłuższy bok / krótszy < 1.4), `Layout.Portrait` = `UiSize.Y > UiSize.X || NearSquare`, `Layout.Wide : bool` = `Portrait && UiSize.X >= 480`. `PhoneView.MaxWidth = 480f`: na `Wide` telefon ma szerokość `min(UiSize.X, MaxWidth)`, pełną wysokość i jest wyśrodkowany. `Origin` uwzględnia przesunięcie, a `Full` zostaje `Portrait`.
- Skala dla `NearSquare`: `s = max(1, floor(min(win.X, win.Y) / 480))` (+1 przy „dużym tekście”, gdy krótszy bok UI zostaje ≥ 400). Przykłady do sprawdzenia: okno 1640×1460 → UI 546×486 (`Wide`); 1460×1640 → 486×546 (`Wide`); iPhone 1179×2556 (proporcje 2.17) → bez zmian względem dziś.

- [ ] **Step 1:** Zmiany w `Layout` i `PhoneView`.
- [ ] **Step 2:** Build + `shot title`, `shot settings-title`, `shot game`, `shot phone-tasks`. Oczekiwane: `-p` i 1280×720 identyczne jak przed zmianą (porównaj z `docs/ux/pakiet-a/sheet_p*.png` / `sheet_land.png`). `-fl`: układ pionowy, telefon wyśrodkowany, nie szerszy niż 480 UI, przyciski duże pod palec.
- [ ] **Step 3:** `--smoke` → PASS. `--monkey 12 400 --touch --size 1640x1460` bez wyjątków w `DrawErrors`.
- [ ] **Step 4: Commit** `"v0.22.0 UX A: duży ekran dotykowy (składak, iPad) – układ pionowy, telefon bez rozciągania"`

### Task 3: Powiadomienia nie zasłaniają treści

**Files:**
- Modify: `GODOT/godot/scripts/Hud/PushBanners.cs` (najwyżej jeden aktywny, kolejka; tryb dolny)
- Modify: `GODOT/godot/scripts/SceneNodes.cs:70-74` (`SetBannerMode`)
- Modify: `GODOT/godot/scripts/Screens/Screen.cs` (nowa wirtualna właściwość), `GODOT/godot/scripts/Screens/ScreenFlow.cs:104-118` (przekazanie)
- Modify: `GODOT/godot/scripts/Screens/TitleScreen.cs`, `GODOT/godot/scripts/Screens/Views/TitleView.cs` (dolna krawędź statystyk)

**Interfaces:**
- Produces: `PushBanners.MaxActive = 1` (stała). `PushBanners.Bottom : bool`: banery rosną od dołu, nad przyciskami strony telefonu (`Layout.BottomReserve` + wysokość wiersza akcji `PhoneView`). `Screen.BannerSpot : BannerSpot` (nowy enum w osobnym pliku `Hud/BannerSpot.cs`: `Default`, `BelowTitleStats`, `Bottom`). Domyślnie: ekrany z `UsesPhone` → `Bottom`, `TitleScreen` → `BelowTitleStats`, gra (`Game`) → `Default` (pod HUD, jak dziś).
- `TitleView.StatsBottom : float`: dolna krawędź bloku rekord / zadania / seria i klucza ustawień (prawa góra).

- [ ] **Step 1:** Zmiany jak wyżej.
- [ ] **Step 2:** Zrzuty: `shot filter-banner`, `shot banners`, `shot hurtownia`, `shot daily`. Oczekiwane: jeden baner naraz. Na tytule baner pod statystykami i nie przykrywa klucza. W Hurtowni i Codziennej baner nad przyciskami na dole, nagłówek i pierwsze wiersze widoczne.
- [ ] **Step 3:** `--smoke` → PASS (asercje banerów w `SmokeTest` mogą liczyć kolejkę: dopasuj do `MaxActive`, nie wyłączaj ich).
- [ ] **Step 4: Commit** `"v0.22.0 UX A: najwyżej jedno powiadomienie, w miejscu, które nie zasłania treści"`

### Task 4: Pasek zakładek telefonu tylko przy zakładkach, „Koszty” → „Szkolenia”

**Files:**
- Modify: `GODOT/godot/scripts/Phone/PhoneView.cs` (rysowanie paska ikon zakładek, ok. linii 265–330; `OpenSingle(page, icon, …)`)
- Modify: `GODOT/godot/scripts/Screens/ProfileScreen.cs:11` (etykieta piątej zakładki)
- Modify: `GODOT/godot/scripts/Phone/ProfileTabs/TrainingTab.cs:46` (`Sub` „Koszty” → bez dubla)

**Interfaces:**
- Pasek ikon rysuje się i przyjmuje stuknięcia tylko gdy `PhoneView.HasTabs`. Przy `OpenSingle` treść strony zajmuje jego miejsce. Parametr `icon` zostaje w sygnaturze (wołający bez zmian), ale nic nie rysuje.
- Zakładka profilu: `Loc.T("szkolenia")` zamiast `Loc.T("koszty")`. Zakładka „Koszty” telefonu **w grze** (koszty budowy) bez zmian.

- [ ] **Step 1:** Zmiany.
- [ ] **Step 2:** Zrzuty `shot career`, `shot card`, `shot settings-title`, `shot daily`, `shot training`, `shot phone-tasks`. Oczekiwane: na pierwszych czterech brak paska, na `training` i `phone-tasks` pasek z podpisami. W `training` zakładka „Szkolenia”.
- [ ] **Step 3:** `--smoke` + `--monkey 13 400 --touch --portrait`.
- [ ] **Step 4: Commit** `"v0.22.0 UX A: pasek zakładek tylko przy zakładkach, zakładka Szkolenia w telefonie kierownika"`

### Task 5: Między budowami – rozmowa z Kierownikiem Markiem

**Files:**
- Create: `GODOT/godot/scripts/Screens/BetweenRunsScreen.cs`, `GODOT/godot/scripts/Phone/Pages/BetweenRunsPage.cs`, `GODOT/godot/scripts/Phone/Pages/QuickReply.cs` (enum), `GODOT/godot/scripts/Session/EndNotes.cs`
- Modify: `GODOT/godot/scripts/App.cs:78-80` (`RunEnded`), `GODOT/godot/scripts/Screens/ScreenFlow.cs` (dodać `BetweenRuns`; usunąć `EndMessage`, `End`), `GODOT/godot/scripts/Screens/HouseScheduleScreen.cs:44`, `GODOT/godot/scripts/Screens/RecapScreen.cs:31`
- Delete: `GODOT/godot/scripts/Screens/EndMessageScreen.cs`, `GODOT/godot/scripts/Screens/EndScreen.cs`, `GODOT/godot/scripts/Screens/Views/EndView.cs` (+ `.uid`), wpis `ViewSet.End` i węzeł `EndView` w `SceneNodes.cs`
- Modify (debug): `GODOT/godot/scripts/Debug/SmokeTest.cs:540-630`, `Debug/DebugScenes.cs:760-765`, `Debug/RecapStaging.cs:139-145`, `Debug/GoalsStaging.cs:162`
- Modify: `GBA/data/game.json` (`uiGodot`), `GBA/data/lang/en.json`

**Interfaces:**
- Consumes: `Spend.QuickKind/QuickBuy/TreePick/NextReward`, `Recap.UnlockName` (Task 1), `PushBanners` (Task 3).
- Przepływ: `RunEnded` → po wygranej z trasą (`g.RouteCount() > 0`) `Flow.HouseSchedule.Open()`, inaczej `Flow.BetweenRuns.Open()`. Z harmonogramu „Dalej” → `Flow.BetweenRuns.Open()`. Z podsumowania (`RecapScreen`) powrót → `Flow.BetweenRuns.Open()`.
- `BetweenRunsScreen : Screen`: `InRun => false`, `UsesPhone => true`, `Music => "title"`, `Views => ViewSet.Title`. Zachowanie z dzisiejszego `EndScreen.Enter` (zapis rekordu, banery postępu) przenosi się do `EndNotes`, a nie do banerów.
- `EndNotes.Of(GameSession s) : List<string>`: krótkie wiersze z `ProgressBanners.Of`, `GoalBanners.Of`, nowych wątków fabuły, `LastCareerFirst/LastCareerNew`, `LastFilterNew`, `LastReward`, rekordu dnia/tygodnia, stawki inwestora (to, co dziś pokazują `EndScreen` i `EndMessageScreen`).
- `BetweenRunsPage : PhonePage` (rozmowa w stylu `MessagePage`, nadawca „Kierownik Marek”), kolejno:
  1. `miedzy_wygrana` / `miedzy_przegrana`
  2. `miedzy_dosw` (+N, razem)
  3. oferta: `miedzy_kup` (gdy `QuickKind ≥ 0`) albo `miedzy_drzewko` (gdy `TreePick ≥ 0`) albo `miedzy_brakuje` (gdy `NextUnlock ≥ 0`), albo nic
  4. `do_tego` + najwyżej 4 wiersze z `EndNotes`, potem `i_wiecej`
  5. pod rozmową karta „Za następny odbiór: [nazwa]” (istniejący klucz `za_kolejny_odbior_2`) z ikoną broni, gdy `toolWeapon ≥ 0`; bez karty, gdy `NextReward == -1`.
- Szybkie odpowiedzi (`QuickReply`: `Buy`, `Training`, `Next`, `NextPlus`, `Details`, `Menu`) jako przyciski-pigułki w treści strony, lista pionowa, wybór ↑/↓ + A, stuknięcie przez `TapRow`. Widoczne:
  - `Buy` „Kup: {nazwa}” (gdy `QuickKind ≥ 0`), inaczej `Training` „Pokaż Szkolenia” (gdy jest cokolwiek w Szkoleniach)
  - `Next` „Kolejna budowa”
  - `NextPlus` „Kolejna budowa (NG+)” (tylko wygrana, nie dzień/tydzień, jak dziś `CanContinue`)
  - `Details` „Szczegóły budowy”
  - `Menu`
- Klawisze: A = zaznaczona odpowiedź, Start = `Next`, Shop (K) = `Training`, B/Cancel = `Menu`. `PageAction` na dole: jedna, „Kolejna budowa” (Start).
- Akcje:
  - `Buy` → `Spend.QuickBuy` + `S.Save()` + do rozmowy dopisuje `miedzy_kupione` i przelicza ofertę (punkty 3 i odpowiedzi)
  - `Training` → `Flow.Profile.Open(4)`
  - `Next` → `Flow.Career.Available ? Flow.Career.Open() : Flow.ClassSelect.Open()`
  - `NextPlus` → `S.NewGamePlus(); App.Refresh(); Flow.StageCard.Open();`
  - `Details` → `Flow.Recap.Open()`
  - `Menu` → `Flow.Title.Open()`
- Teksty (PL `uiGodot` / EN):

| klucz | PL | EN |
|---|---|---|
| `kierownik_marek` | Kierownik Marek | Site manager Marek |
| `miedzy_wygrana` | Szefie, odbiór zaliczony! Inwestorka zadowolona. | Boss, handover passed! The investor is happy. |
| `miedzy_przegrana` | Szefie, budowa stanęła. Ale doświadczenie zostaje w kieszeni. | Boss, the build stopped. But we keep the experience. |
| `miedzy_dosw` | Mamy +{0} dośw., razem {1}. | We got +{0} XP, {1} in total. |
| `miedzy_kup` | Za {1} dośw. możemy wziąć: {0}. Kupujemy? | For {1} XP we can get: {0}. Buy it? |
| `miedzy_drzewko` | W drzewku Szkoleń czeka wybór: {0}. | A pick is waiting in the Training tree: {0}. |
| `miedzy_brakuje` | Do „{0}” brakuje jeszcze {1} dośw. | “{0}” still needs {1} more XP. |
| `miedzy_kupione` | Zrobione! {0} kupione. | Done! {0} bought. |
| `do_tego` | Do tego: | Also: |
| `i_wiecej` | i {0} więcej w szczegółach | and {0} more in details |
| `kup_x` | Kup: {0} | Buy: {0} |
| `pokaz_szkolenia` | Pokaż Szkolenia | Show Training |
| `kolejna_budowa` | Kolejna budowa | Next build |
| `szczegoly_budowy` | Szczegóły budowy | Build details |

(Istniejące klucze, np. `kolejna_budowa_ng`, `menu`, `za_kolejny_odbior_2`, użyj ponownie. `gen_data.py --check` zgłosi dubel.)

- [ ] **Step 1:** Teksty + `gen_data.py --check` → OK.
- [ ] **Step 2:** `EndNotes`, `QuickReply`, `BetweenRunsPage`, `BetweenRunsScreen`, przepięcia w `App`/`HouseSchedule`/`Recap`, usunięcie starych ekranów i `EndView`.
- [ ] **Step 3:** Sceny zrzutów w `DebugScenes`: `between-win` (wygrana z ofertą „Kup”), `between-death` (przegrana), `between-daily` (budowa dnia), `between-broke` (Xp = 0), `between-tree` (najtańszy węzeł drzewka). Dotychczasowe nazwy `end` i `endmsg` przekierowane na `between-win` (README i skrypty zrzutów ich używają).
- [ ] **Step 4: Test dymny:** przepisz bloki w `SmokeTest.cs` (tydzień ~540, dzień ~600, wygrana ~619) na nowy przepływ. Asercje:
  - tydzień i dzień: `Flow.Current == Flow.BetweenRuns`, brak `NextPlus`, `Next` → `Flow.Current` to `Career` albo `ClassSelect`
  - wygrana: harmonogram (link działa jak dziś) → `BetweenRuns` z `NextPlus`; `Details` → `Recap` (`Page.Count >= 10`) → powrót do `BetweenRuns`
  - zakup: przy `Xp` ustawionym na koszt `QuickKind` + koszt kolejnej, dwa razy `Buy` → `Xp` zmniejszone o oba koszty, profil zapisany (`S.Profile` po `Load` ma ten sam `Xp`)
- [ ] **Step 5:** Build, `--smoke` → PASS. `shot between-win`, `shot between-death`, `shot between-daily`, `shot between-broke`, `shot between-tree`: rozmowa czytelna, odpowiedzi nie nachodzą na tekst i pasek akcji, EN się mieści, na `-fl` telefon wyśrodkowany.
- [ ] **Step 6:** `--monkey 14 900 --touch --portrait` i `--monkey 15 900` (desktop): małpa przechodzi koniec budowy i zaczyna kolejną, bez wyjątków.
- [ ] **Step 7: Commit** `"v0.22.0 UX A: między budowami rozmowa z Kierownikiem Markiem (kup, Szkolenia, kolejna budowa); bez starej planszy końcowej"`

### Task 6: Harmonogram domu z kodem QR, jedno „Zaplanuj swoją budowę”

**Files:**
- Create: `GODOT/godot/assets/ui/qr.png` (+ `.import` z Godota), wycięty kod QR z `assets/ui/end.png`. Dodaj wycinanie do `GODOT/tools/export_godot_assets.py`, żeby przetrwał ponowny eksport.
- Modify: `GODOT/godot/scripts/Phone/Pages/HouseSchedulePage.cs` (kod QR w karcie linku na dole; usunięty wiersz-dubel „Zaplanuj swoją budowę”, zostaje przycisk akcji)

**Interfaces:**
- Karta na dole: kod QR (skala całkowita, min. 2×, ostre piksele) + „planbudowlany.online”. Stuknięcie w kartę = dzisiejszy `TapRow(0)` (Select → link).
- Stuknięcie i Select dalej liczą `HouseScheduleScreen.LinkOpened` (test dymny bez zmian).

- [ ] **Step 1:** Wycięcie QR (skrypt) i zmiany strony.
- [ ] **Step 2:** `shot house` (cztery rozmiary): QR czytelny (zeskanuj z ekranu telefonem, prowadzi do planbudowlany.online), jedno „Zaplanuj swoją budowę”.
- [ ] **Step 3:** `--smoke` → PASS.
- [ ] **Step 4: Commit** `"v0.22.0 UX A: harmonogram domu z kodem QR planbudowlany.online, bez dubla Zaplanuj swoją budowę"`

### Task 7: Tytuł – pogrupowane menu i karta „Twoje postępy” (wariant A)

**Files:**
- Modify: `GODOT/godot/scripts/Screens/TitleScreen.cs` (pozycje, `Choose`, `CoachHole`, `Populate`)
- Modify: `GODOT/godot/scripts/Screens/Views/TitleView.cs` (rysowanie grup, wiersz dwóch przycisków, karta, prostokąty stuknięć)
- Modify: `GBA/data/game.json` (`uiGodot`), `GBA/data/lang/en.json`

**Interfaces:**
- Consumes: `Spend.QuickKind` / `Meta.NextUnlock` (Task 1), `TitleView.StatsBottom` (Task 3), `Layout.Wide` (Task 2).
- Pozycje (`TitleItem`, nowy enum w `Screens/TitleItem.cs`): `Continue` (gdy zapis), `NewRun`, `Daily`, `Weekly`, `Phone`, `Help`. `Daily` i `Weekly` to jeden wiersz, dwa przyciski. ↑/↓ chodzi po wierszach, ←/→ w wierszu.
- Grupy: bez nagłówka dla `Continue`/`NewRun`, potem `dziennie` „Dziennie” / „Daily”, potem `postep_grupa` „Postęp” / „Progress”.
- `Phone` → `Flow.Profile.Open(0)`. Etykieta `telefon_kierownika`: „Telefon kierownika” / „Site manager's phone”.
- Karta (tylko gdy `Runs > 0`) w miejscu paska `Info`: `twoje_postepy` „Twoje postępy” / „Your progress”, wiersz `postepy_linia` „Doświadczenie {0} · Budowy {1}” / „Experience {0} · Builds {1}”, przycisk `wydaj_doswiadczenie` „Wydaj doświadczenie” / „Spend experience” → `Flow.Profile.Open(4)`. Przycisk aktywny (fiolet), gdy `Meta.NextUnlock` ≤ `Xp`, inaczej szary z „brakuje N”. `Note` ma pierwszeństwo nad kartą. `Info` bez `p_profil_k_szkolenia`.
- Etykiety „Codzienna” i „Tydzień” (`tydzien_krotko` „Tydzień” / „Week”). Klawisze P/K zostają.
- `CoachHole`: `phone`/`training`/`respect` → prostokąt `Phone` (dla `training`/`respect` dymek mówi o Szkoleniach w telefonie: istniejące teksty samouczka zostają), `daily` → `Daily`, `help` → `Help`, reszta → `NewRun`.
- `Wide`: menu i karta w kolumnie o szerokości ≤ 420 UI, wyśrodkowane, przyciski min. `Layout.TouchTarget` wysokości.

- [ ] **Step 1:** Teksty + `gen_data.py --check`.
- [ ] **Step 2:** Zmiany w `TitleScreen`/`TitleView`/`TitleItem`.
- [ ] **Step 3:** `shot title`, `shot tutorial-title`, `shot filter-banner`: grupy, karta z przyciskiem, dymek samouczka wskazuje właściwy przycisk, baner pod statystykami. EN się mieści, `-fl` kolumna wyśrodkowana.
- [ ] **Step 4:** `--smoke` (tytuł: indeksy pozycji w `SmokeTest` dopasuj do `TitleItem`) + `--monkey 16 500 --touch --portrait`.
- [ ] **Step 5: Commit** `"v0.22.0 UX A: tytuł – grupy Dziennie/Postęp, Telefon kierownika, karta Twoje postępy z Wydaj doświadczenie"`

### Task 8: Wybór zawodu – siatka kafli z pełnymi nazwami, broń i moc jako bloki

**Files:**
- Modify: `GODOT/godot/scripts/Screens/Views/ClassSelectView.cs` (`DrawContent` ~141, `DrawPortraitCard` ~236, `DrawCard` ~384; nowe metody siatki w tym samym pliku albo wydzielone do `Screens/Views/ClassGrid.cs`, jeśli plik przekroczy ~700 linii)
- Modify: `GODOT/godot/scripts/Screens/ClassSelectScreen.cs` (nawigacja ↑/↓/←/→ po siatce, „Szczegóły”)
- Modify: `GODOT/godot/scripts/Screens/Views/ClassSelectHit.cs` (nowe trafienia: kafel, szczegóły)
- Modify: `GBA/data/game.json` (`uiGodot`), `GBA/data/lang/en.json`

**Interfaces:**
- Kolejność kafli jak dziś (`_order`: odblokowane, potem zablokowane).
- Kafel: portret (wybrany animowany jak dziś), **pełna nazwa** (zawija do 2 linii, bez `Fit`/wielokropka; jeśli 2 linie nie wystarczą w EN, zmniejsz skalę fontu tylko dla tego kafla). Zablokowany: sylwetka z kłódką + krótki warunek: liczba wygranych / koszt dośw. / „Akt 0” / „?” (te same źródła co dzisiejsza cena / opis blokady).
- Układy:
  - **pion telefonu** (`Portrait && !Wide`): siatka 3 kolumny, przewijana pionowo, gdy się nie mieści. Pod nią krótka karta: nazwa, opis, blok broni (ikona, nazwa, styl, obrażenia). Przyciski „Szczegóły” (`szczegoly`) i „Start budowy”. „Szczegóły” otwiera pełną kartę na całą wysokość z „Wróć”.
  - **`Wide` i desktop 1280×720** (`!Portrait || Wide`): siatka 4 kolumny po lewej, pełna karta po prawej.
- Pełna karta: blok **broni** (ikona broni, nazwa, zasięg, obrażenia, kryt; dymek rozpiski #26 jak dziś) i blok **mocy** (ikona, nazwa, opis) jako osobne karty. Statystyki jako wiersze „etykieta · co robi” + wartość, np. `Zdrowie 34 (+4 kask)`, `Siła · obrażenia wręcz 5`, `Szczęście · lepszy sprzęt +3` (opisy z `StatHelp`; brakujące krótkie etykiety dodaj jako klucze `stat_*` w `uiGodot`/EN). Paski statystyk mogą zostać jako tło wiersza. Dalej trudność, pamiątka, tryb inwestora, mistrzostwo jak dziś.
- Zablokowany zawód: przycisk startu szary z napisem warunku (np. „Za 5. wygraną”), nie fioletowy „Zablokowany”.
- Samouczek (`_coach`): kroki `class`/`difficulty`/`keepsake`/`stats`/`investor` dostają prostokąty z nowego układu.

- [ ] **Step 1:** Teksty + `gen_data.py --check`.
- [ ] **Step 2:** Siatka, karta krótka i pełna, nawigacja, trafienia, samouczek.
- [ ] **Step 3:** `shot classselect`, `shot classselect-locked`, `shot stats-class`, `shot tutorial-class`, `shot class-operator`. Oczekiwane: żadna nazwa nie jest ucięta (sprawdź „Kierownik budowy”, „Operator koparki”, „Cieśla-dekarz” w PL i EN), warunki blokady czytelne, na `-fl` i 1280×720 dwa panele.
- [ ] **Step 4:** `--smoke` (wybór zawodu i start budowy w `SmokeTest`) + `--monkey 17 600 --touch --portrait` + `--monkey 18 600`.
- [ ] **Step 5: Commit** `"v0.22.0 UX A: wybór zawodu – siatka z pełnymi nazwami, broń i moc jako osobne bloki, statystyki słowami"`

### Task 9: Podsumowanie budowy bez „10/48” i bez „Wyżej/Niżej”

**Files:**
- Modify: `GODOT/godot/scripts/Phone/Pages/RecapPage.cs` (`Sub`, `Actions`, kolejność pierwszych wierszy)
- Modify: `GODOT/godot/scripts/Phone/PhoneView.cs` (przewijanie listy przeciągnięciem palca, jeśli go brak: dziś jest `Swipe`/`SwipeRepeat`)

**Interfaces:**
- `RecapPage.Sub` = wynik („Wygrana” / „Przegrana”), bez licznika pozycji. Na dotyku `Actions` = tylko „Wróć” (do `BetweenRuns`). Przewijanie przeciągnięciem, na klawiaturze ↑/↓ jak dziś.
- Pierwsze dwa wiersze: wynik budowy i „Zdobyte doświadczenie +N”, dalej bez zmian (najmocniejsze ciosy, oś czasu…).

- [ ] **Step 1:** Zmiany.
- [ ] **Step 2:** `shot recap-win`, `shot recap-death`, `shot recap-death-scroll`, `shot recap-progress`.
- [ ] **Step 3:** `--smoke` (asercje `Recap.Page` z Task 5) + `--monkey 19 600 --touch --portrait`.
- [ ] **Step 4: Commit** `"v0.22.0 UX A: podsumowanie budowy – wynik i doświadczenie na górze, przewijanie palcem"`

### Task 10: HUD z podpisami, legenda kolorów, ustawienia w poziomie w grupach

**Files:**
- Modify: `GODOT/godot/scripts/Hud/HudLayer.cs` (+ plik rysujący górny pasek HUD, w którym są ikony apteczki / pogody / błota / głośnika)
- Modify: `GODOT/godot/scripts/Phone/Pages/MessagePage.cs` (wiersz legendy pod listą informacji, gdy są wiersze w ≥ 2 kolorach)
- Modify: `GODOT/godot/scripts/Phone/Tabs/TasksTab.cs` (wiersz legendy przy „Plac budowy”)
- Modify: `GODOT/godot/scripts/Phone/Pages/SettingsPage.cs` (nagłówki grup Dźwięk / Sterowanie / Gra także w widoku poziomym)
- Modify: `GBA/data/game.json` (`uiGodot`), `GBA/data/lang/en.json`

**Interfaces:**
- HUD: przy ikonie krótki podpis (`hud_apteczka` „Apteczka”, nazwa pogody z danych etapu, nazwa mechaniki aktu z danych). Ikona głośnika usunięta z HUD (dźwięk w ustawieniach). W pionie podpisy w drugim wierszu HUD; nie mogą zasłaniać mapy bardziej niż dziś (+1 wiersz maks.).
- Legenda: kolorowe kwadraty + `legenda_premia` „premia” / „bonus”, `legenda_zagrozenie` „zagrożenie” / „danger”, `legenda_sciezka` „ścieżka” / „path”, `legenda_info` „info” / „info”. Kolory = te, którymi `MessagePage`/`TasksTab` już barwią paski (`Ink.Done`, `Ink.Late`, `Ink.Brand`, `Ink.Dim`; sprawdź w kodzie i użyj tych samych).

- [ ] **Step 1:** Teksty + `gen_data.py --check`.
- [ ] **Step 2:** Zmiany.
- [ ] **Step 3:** `shot game`, `shot weather-rain`, `shot act-mud`, `shot card`, `shot phone-tasks`, `shot settings`, `shot settings-title`.
- [ ] **Step 4:** `--smoke` + `--monkey 20 600 --touch --portrait` + `--monkey 21 600`.
- [ ] **Step 5: Commit** `"v0.22.0 UX A: HUD z podpisami, legenda kolorów w wiadomościach i na placu, ustawienia w grupach także w poziomie"`

### Task 11: Dokumentacja, porządki i zamknięcie pakietu

**Files:**
- Modify: `TODO.md` (sekcja v0.22.0: nowe wiersze projektu A ✅; #71 ✅; #72 🔄 częściowo przez rozmowę między budowami; „Następne (2026-10-09) – UX przepływ po wygranej” ✅ dla Godota; nowe pozycje projektów B–E jako ⬜, w tym pomysł właściciela z 2026-10-23: „detale map – okna w ścianach, piwnica poniżej, w etapach dachowych światło z góry (świetliki)” w projekcie C / oprawa)
- Modify: `GBA/CHANGELOG.md` (wpis v0.22.0 – UX A, Godot)
- Modify: `GODOT/README.md` (nowe sceny zrzutów `between-*`, usunięte `EndScreen`/`EndMessageScreen`), `GODOT/docs/KONCEPCJA.md` (sekcja przepływu ekranów, jeśli opisuje planszę końcową)
- Modify: `docs/superpowers/plans/2026-10-22-ux-xp-flow.md` (na górze: „Zastąpiony przez 2026-10-23-ux-pakiet-a.md”)

- [ ] **Step 1:** Wpisy.
- [ ] **Step 2:** `dotnet test GODOT/LifeLike.sln`, `gen_data.py --check`, build, `--smoke`, `--smoke --lang en` → PASS. `git diff -w --stat` vs `git diff --stat`, `git grep` na sekrety wg CLAUDE.md.
- [ ] **Step 3: Commit** `"v0.22.0 UX A: TODO, CHANGELOG, README po projekcie A"`
- [ ] **Step 4:** Pokaż właścicielowi changelog i zrzuty „przed/po” (pion + składak) i zapytaj „wydajemy?”. Pełny `rogue-test` dopiero po „tak”.
