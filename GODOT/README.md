# PlanBudowlany RogueLike – Godot 4 + C#

Wersja Godot gry z demo GBA (`../GBA`): roguelike budowlany, w którym etapy budowy są piętrami lochu,
a wrogami są *problemy budowy*. Kierunek rozwoju: [`docs/KONCEPCJA.md`](docs/KONCEPCJA.md)
(telefon z aplikacją PlanBudowlany jako interfejs, oprawa 2.5D – kolejne kamienie milowe).

**Stan: zgodny z GBA v0.21.52 cz. d (mapa kariery: rdzeń, test złoty i ekrany), cz. c (drzewko Szkoleń, kolekcje, zadania dnia i tygodnia, seria dni), cz. b (inspektor, mistrzostwo, stopnie inwestora), cz. a (tempo postępu), wcześniej v0.21.51 cz. 2 (sekretne zlecenia), v0.21.49 cz. 3** (logika, dane i test złoty z migawki GBA v0.21.49 cz. 3: Akt 0 „Papierologia”
z pieczątkami i Decyzją odmowną, samouczek menu, profil v10; wcześniej 10 etapów, 20 nowych problemów
z zachowaniami, mechaniki aktów, opis statystyk; Respekt za etapy i sklep Respektu,
nagrody za odbiór – Młot udarowy, Pistolet do kotew, buty, pas, zawody Dekarz, Tynkarz, Operator koparki – nowy balans
Szkoleń, profil v8; wcześniej wybór ścieżki, materiały, codzienna budowa, pogoda, brygada, tryb inwestora);
oprawa (grafika, font, dźwięk, telefon, wybór zawodu) jak w GBA.

Nowe w v0.21.52 cz. d (#47): **mapa kariery** – kontrakty `GameData.Career` (`CareerDef`: pierwszy etap w `Stages`, liczba,
Akt 0, odblokowanie `CareerUnlock` – wygrane / poziom inspektora, porywy, bliźniak, boss, nagroda) z własnymi etapami
(`StageDef.Look` – paleta `tiles/stage_N.png` 0–21, `Tiles`, `Twin`; `GameData.StagesCount` = Dom jednorodzinny), `Game.Contract`
i `Game.Career` (`SDef`, `RouteCount`, `PreludeCount`, `StageId`, `MechValue` – Dom z poddaszem: porywy co 4 tury), bliźniak w
`StartStage` (`TwinCarry`: pogoda, wydarzenie i do 3 niedokończonych problemów z pierwszej połowy), `Career` (odblokowanie,
wybór, ogłoszenie, najlepszy etap, pierwsza wygrana: Respekt, tytuł – źródło 5, kask; `MigrateV16`), profil v16 (`PBRL016`),
zapis budowy z kontraktem na końcu (`RunSave.V15Tail` – stary zapis wczytuje się jako Dom). Ekrany: **Mapa kariery**
(`CareerScreen` / `CareerPage`: po pierwszej budowie Nowa budowa na tytule; Spacja / drugie stuknięcie wybiera, Esc z wyboru
zawodu wraca do mapy), karta etapu z nazwą kontraktu, banery Wspólna ściana, Wygrany kontrakt i Nowy kontrakt, Jak grać
str. 11; eksport klatek 184–195 i kafli wyglądów 12–21 (nowe rodzaje: parkiet, bale, kamień, sztukateria). Testy:
`CareerTests`, test złoty 74 przebiegi (każdy kontrakt). Sceny zrzutów (`Debug/CareerStaging`): `career`, `career-locked`,
`career-letnisko`, `career-blizniak`, `career-poddasze`, `career-kamienica`, `career-end`, `help-career`.

Nowe w v0.21.52 cz. c (#46, #49, #50, #51): **drzewko Szkoleń** (`SkillTree`: gałęzie `TreeBranch` z pniem = Szkolenia,
węzły `TreeNode` z opcjami `TreeOption`, `Choose` – pierwszy wybór za koszt, zmiana za `TreeRespecCost`; Siła rozpędu –
`RunMods.FirstHitBonus` w bitach 8-11 `Mastery`, `Game.HeroAttack` dolicza ją w nietknięty problem); **kolekcje**
(`CollectionBook`: `Profile.KillCount` ze znakiem wodnym `KillMark` w `Meta.RecordRun`, komplety aktów / bossów / albumu,
`Check` – baner raz, premia w `Meta.Mods`, tytuły i kask); **zadania dnia i tygodnia** (`DailyTasks`: 3 + 2 z seeda
numeru dnia / tygodnia – data z systemu `GameSession.TodayNumber`, `Metric` z liczników budowy, w tym nowych
`Game.HelpersCalled` i `ShopBuys`, `Bank` ze znakiem wodnym `TaskMark`, Respekt, nagrody `TaskRewards`); **seria dni**
(`DayStreak.Record` w `Daily.Record`, nagrody `StreakRewards`, pamiątka `KeepsakeDef.Streak`); Kask ojca –
`PerkEffect.TakenPct`. Ekrany: Koszty > Tab: **Drzewko** (kolumny gałęzi, strzałki / stuknięcia, Spacja wybiera),
Katalog > Spacja: **Kolekcje / Bossowie / Album**, Odznaki > **Zadania** (seria dni w opisie), tytuł („Zadania 1/3”,
„Seria N dni”), telefon w budowie > Koszty (zadanie dnia na żywo), złote banery (`Session/GoalBanners`: koniec etapu
– zdarzenie `SessionEvents.Goals` – i plansza końcowa), Jak grać str. 10. Profil v15 (384 B, `Goals.MigrateV15`);
zapis budowy bez dwóch nowych liczników (krótszy o 2 bajty) wczytuje się z zerami (`RunSave.V14Tail`). Testy:
`GoalsTests`, test złoty 66 przebiegów (drzewko, zadania, seria dni). Sceny zrzutów (`Debug/GoalsStaging`): `title-goals`,
`tree`, `tree-locked`, `collections`, `bosses`, `album`, `tasks`, `phone-goals`, `goals-end`, `help-goals`.

Nowe w v0.21.52 cz. b (#44, #45, #48, #52): **poziom inspektora** (`Progress`: progi z danych, `InspectorBar`, `Bank`
ze znakiem wodnym `Profile.RunProgress` – NG+ i porzucenie bez podwójnego liczenia, Respekt raz za poziom, nagrody od
poziomu: tytuły `Titles` z `GameData.ProgressTitles`, kolory kasku, wątki SMS `StoryTrigger.Inspector`, ozdoby Osiedla
`DecorDef.Inspector`, druga pamiątka `Meta.SelectedKeepsake2` na randze I); **mistrzostwo zawodu** (`MasteryBit`,
`Game.Mastery`: wariant mocy – siła przez `BoonPower`, tury w `AbilityCooldown`; broń mistrza – kryt tylko z bronią zawodu,
złoty błysk; premia mistrzostwa `BoonDef.Mastery` w ofercie; kask mistrza `Secrets.HelmetCosmetic(…, cls)`);
**stopnie inwestora** (`Progress.StakeRank`, Respekt w `Meta.RecordRun`). Ekrany: pasek inspektora na tytule, „Mistrz N”
z paskiem na karcie zawodu (wariant mocy i broń mistrza w nazwach), strona Wygląd / Tryb inwestora z wierszami Moc
i Pamiątka 2 oraz nagrodą za kolejny stopień, karta POSTĘP z paskami mistrzostwa i inspektora, złote banery nowych
poziomów (`Session/ProgressBanners`), Odznaki > Inspektor, „Mistrz N” w Zespole, ozdoby z inspektora na Osiedlu, Jak grać
str. 9. Profil v14 (240 B, `Progress.MigrateV14`). Testy: `InspectorMasteryTests`, test złoty 62 przebiegi (mistrzostwo
i druga pamiątka). Sceny zrzutów: `inspector`, `help-progress` (profil pokazowy z inspektorem i mistrzostwem: `classselect`,
`looks`, `recap-progress`, `recap-end`).

Nowe w v0.21.52 cz. a (tempo postępu #41–#43, #52): Szkolenia po 4 poziomy z danych (`UpgradeDef.Steps`: działanie,
przyrost, koszt; `RunMods.AddUpgrade`, `UpgradeLabel`, `Meta.UpgradeSummary`) – Koszty: „Kondycja 2/4”, opis „Poziom III:
+1 HP na start (teraz: +2 HP na start)”, na maksimum „Razem”; zawody i narzędzia drożeją z każdym zakupem
(`Meta.ClassCost` / `ToolCost`, `ToolDef.Shop`), Trudny 100. Odznaki i zlecenia: mało doświadczenia, za to **tytuły**
(`Titles`, profil > Odznaki – piąta strona **Tytuły**: Tab / „Wybierz” albo drugie stuknięcie; wybrany na Osiedlu
i w podsumowaniu, banery z tytułem) i **kolory kasku** (`Secrets.HelmetCosmetic` / `CycleHelmet`; wybór zawodu > Tab –
strona Wygląd za modyfikatorami inwestora, także przed pierwszą wygraną; klatki 160–183 z eksportu z kaskiem w kolorze
indeksu D i shader `HelmetTint` na bohaterze, portretach wyboru zawodu i w prologu). Podsumowanie: karta **POSTĘP**
z paskami (najbliższe Szkolenie – dośw./koszt albo „Stać Cię!”; mistrzostwo i inspektor – „wkrótce”, cz. b). Profil v13
(200 B, `Meta.MigrateV13` – zwrot za Szkolenia po starej cenie). Sceny zrzutów: `titles`, `looks`, `recap-progress`;
test dymny wyboru tytułu. Test małpy spędza mniej czasu w Ustawieniach; seria pion + dotyk znalazła i poprawiono błąd
marszu po dotknięciu (`AutoWalk`).

Nowe w v0.21.51 (poprawki po graniu na iPhonie; wydane razem z v0.21.52): **autokafle muru (#36)** – eksport generuje dla palety każdego etapu
wierzch masy muru, lico i 6 nakładek (krawędź wierzchu góra / lewa / prawa, końce lica, róg wewnętrzny), `MapLayer`
dobiera je wg sąsiadów (mur z murem poniżej = ciemny wierzch bez pasów, z podłogą poniżej = lico z wzorem), `FogLayer`
wygasza skraj odkrytej części w ciemność (bez schodków; od cz. 2 pikselowo – patrz niżej); **błoto (#37)** co 14. pole
(dane) jako płaska mokra plama z połyskiem (`fx/mud.png`, 3 warianty); HUD w wąskim pionie: „Etap 2/10, Normalny”
i pełna nazwa etapu w drugim rzędzie; **spójne sterowanie (#38)**: `PageAction` ma rolę (główny / powrót / zwykły),
`PhoneView` rysuje główny przycisk zawsze po prawej, powrót po lewej (jeden przycisk – prawa połowa), `InputCmd.IsConfirm`
(A, Enter) i `IsBack` (B, Esc) w oknach, **blokada wejścia** `ScreenFlow.InputLocked` (0,4 s po otwarciu okna i w czasie
wjazdu telefonu – Main połyka wciśnięcia i dotknięcia), zamiana ulepszonego narzędzia przez zaznaczenie (domyślnie
„Zostaję”); Jak grać – wiersz „Okna”. Test dymny: A / Enter / stuknięcie w kartę tuż po otwarciu premii nic nie robi,
po blokadzie pierwsze stuknięcie tylko zaznacza; A przy zamianie narzędzia bez zaznaczenia zostawia ulepszenie.

Nowe w v0.21.51 cz. 2 (sekretne zlecenia #39, warstwa Godota; wydane razem z v0.21.52): eksport klatek 127–159 z GBA (Spawacz, Geodeta, Majster
z chodem i oddechem, sylwetki, kask w paski dla 12 zawodów; `Assets.AnimB` / `HasWalk` / `Silhouette` / `HeroFrame`),
ikony mocy, narzędzi i wyglądu. Profil > Odznaki – czwarta strona **Sekrety** (Spacja / przycisk zmienia stronę): koperta
z „???” i podpowiedzią, po wykonaniu warunek i nagroda z ikoną (zawód – mały portret). Sesja sprawdza sekrety po etapie,
po porzuceniu i na końcu budowy (`Secrets.Check`), baner „Sekretne zlecenie!” ze złotą ramką (kolejka 8 banerów: nagroda,
sekrety, odznaki, zlecenia, fabuła), notatka na planszy końcowej, na tytule dymek „Nowość” z treścią sekretu. Wybór
zawodu i Zespół: nowe zawody (zablokowane – sylwetka, „Sekret” i podpowiedź), moce Spaw (iskry linią, dym) i Tyczenie
(znak geodety nad oznaczonym problemem), Majster – moc innego fachu na etap: ikona w HUD, na pasku dotyku i w menu akcji,
baner „R: …”, efekt pożyczonej mocy. Wygląd: **Kask w paski** – wiersz w Trybie inwestora (bohater, portrety, prolog),
**Złota kielnia** – złote iskry i poświata z broni przy krycie. **Poziomica mistrza**: na podglądzie mapy magazyn widać
pod mgłą (złota ramka, krąg, ikona; kadr obejmuje magazyn). Koszty > Respekt: „Zaprawiony w boju” jako „???” z pastylką
Sekret do wykonania zlecenia; sekretne narzędzia nie są do kupienia w Szkoleniach. Jak grać – strona 8 Sekrety
(`secretsHelp` z game.json). **Mgła pikselowa:** zamiast rozmytej tekstury 4x4 na pole – mała tekstura danych (pole =
piksel: jasność, znane) bez filtrowania i shader w rozdzielczości piksela grafiki: skraj odkrytej części gaśnie w 3 stopniach
kraty Bayera (75/50/25%, po 2 piksele grafiki), światło przechodzi progami co 0,145 z ditheringiem (pola zapamiętane
jednolite). Sceny zrzutów: `sekrety`, `sekrety-locked`, `sekret-banner`, `sekret-news`, `class-spawacz`, `class-geodeta`,
`class-majster`, `class-sekret-locked`, `power-spaw`, `power-tyczenie`, `power-majster`, `stripes`, `stripes-investor`,
`gold-crit`, `poziomica-map`, `respect-sekret`, `help-secrets`; test dymny (`Debug/SmokeSecrets`): strona Sekrety,
wygrana bez kawy – sekret, baner, notatka i dymek Nowość, start każdym nowym zawodem z mocą, kask w paski z Trybu
inwestora, złoty błysk, Poziomica w dropach i na podglądzie, Respekt zablokowany do sekretu. **Test małpy** – niżej.

Nowe w v0.21.50 (rozpiska obrażeń broni #26, jak w BG3): rdzeń `DmgBreakdown` / `Game.WeaponBreakdown` / `EnemyHit`
/ `DamageHelp` (port 1:1 z `core.h`, testy: zakres = walka na tysiącach rzutów), warstwa `Gfx/DamageRows` (te same
wiersze co na GBA). Wybór zawodu: narzędzie z zakresem i krytem, najechanie myszą / dotknięcie = dymek z rozpiską;
opis statystyk ma 4 strony (wartości, Obrażenia broni, Kryt i obrona, wzory); telefon > Sprzęt: zakres, kryt i
średnia, przy przedmiotach co dają, I / „Obrażenia” / dotknięcie narzędzia = rozpiska; paczka: porównanie ciosu,
krytu i OBR; baner nowego narzędzia „teraz -> po zmianie”; karta problemu „Zadasz 9-12 (kryt 18-24), on Tobie 1,
unik 10%”; Jak grać – strona Obrażenia (`damageHelp` z game.json). Sceny zrzutów: `dmg-class`, `dmg-stats`,
`dmg-gear`, `dmg-phone`, `dmg-crit`, `dmg-offer`, `dmg-tool`, `dmg-enemy`, `help-dmg`; test dymny: Sprzęt > I >
rozpiska > B.

Nowe w v0.21.50 cz. 2 (premie po etapie, elity, kombinacje stanów): rdzeń `Game.Boons` (port 1:1 z `core.h`: oferta
1 z 3 z osobnego generatora, rzadkość zwykła / rzadka / legendarna, znaczniki i synergie, premie zawodów, losowanie
raz na budowę + Druga oferta z Respektu, elity z cechą i nagrodą, mokry + prąd, pył + iskra, zamróz + uderzenie; zapis
PBRUN11, test złoty z nowymi polami). Po zaliczonym etapie ekran **Premia za etap** (`BoonScreen` / `BoonPickPage`):
3 karty z paskiem w kolorze rzadkości, skutkiem, znacznikami i pastylką „Synergia: …”, strzałki / dotknięcie,
Enter bierze, R / „Losuj” losuje jeszcze raz; potem harmonogram (i Hurtownia po akcie), nowa synergia – baner i opis
na harmonogramie. Telefon > Sprzęt > R / „Premie” (`BoonListScreen`): lista premii i strona Synergie (postęp
znaczników). Elity: złota ramka, poświata i odcień na mapie, na karcie problemu „Zbrojony Przeciek”, OBR z Tarczą,
„Elita: …” i „Zadasz” z `ActorBreakdown`; stany nad problemem (kropla, pył, płatek), Mokry w HUD, napis „Mokry + prąd!”
z błyskawicami / wybuchem / odłamkami (`WorldFx.Combos`); rozpiska: wiersz „Premie etapów”; Jak grać – strona 5
Kombinacje (`combos`, `sources` z game.json). Sceny zrzutów: `boon-pick`, `boon-synergy`, `boon-phone`,
`boon-synergies`, `elite-map`, `elite-card`, `combo-shock`, `combo-dust`, `combo-crack`, `help-combos`; test dymny:
losowanie, wybór premii, telefon > Sprzęt > R > synergie > B.

Nowe w v0.21.50 cz. 3 (wydarzenia z wyborem, ulepszanie narzędzia, magazyn): rdzeń `Game.Extras` (port 1:1
z `core.h`: pole wydarzenia z osobnego generatora, odpowiedzi ze skutkami i szansą, ulepszenie narzędzia z cechą
Przebicie / Ostrze / Wyważenie, magazyn za pękniętą ścianą albo drzwiami, klucz, strażnik, skrzynia; `ChoiceText`
– te same teksty skutków co GBA; zapis PBRUN12, test złoty z polem `part3`, testy `UpgradesEventsSecretsTests`).
Wejście na pole z SMS-em otwiera **Wydarzenie** (`EventScreen` / `EventPage`): SMS, odpowiedzi ze skutkami
(strzałki / dotknięcie, Enter), wynik – co zaszło, a co „nie tym razem”; premia z projektu – ekran premii „Premia:
projekt” i powrót na plac. Hurtownia: wiersz „Ulepsz narzędzie” (zł + stal, poziom i koszt w opisie, „maks.”), przy
+2 **Cecha narzędzia** (`TraitScreen`), ostrzeżenie przy „Nowe narzędzie”; nowe narzędzie na polu przy ulepszonym –
**Nowe narzędzie** (`ToolOfferScreen`: porównanie ciosu, „ulepszenie +2 przepadnie!”, Spacja zamieniam / Z zostaję).
Nazwa „Kielnia+2” w Sprzęcie i rozpisce (wiersz Ulepszenie), pęknięcie / drzwi magazynu na polu muru (złota ramka,
gdy możesz otworzyć), klucz i skrzynia na mapie, karta problemu „Ma klucz do magazynu!”, telefon > Zadania: wiersz
wydarzenia (z odpowiedzią) albo magazynu; Jak grać – strona 6 (`extrasHelp`). Sceny zrzutów: `event-map`,
`event-sms`, `event-choices`, `event-result`, `event-boon`, `upgrade-shop`, `upgrade-trait`, `upgrade-gear`,
`tool-swap`, `secret-crack`, `secret-card`, `secret-open`, `secret-door`, `secret-map`, `tasks-extras`,
`help-extras`; test dymny: wydarzenie (strzałka, odpowiedź, wynik), ulepszenie z cechą w Hurtowni, zostawienie
ulepszonego narzędzia, drzwi bez klucza, pęknięta ściana z kluczem i skrzynia.

Nowe w v0.21.50 cz. 4 (podsumowanie budowy #33, wyzwania tygodnia #34, fabuła #35): rdzeń `Game.Recap` (port 1:1
z `core.h`: ostatnie ciosy w bohatera z rodzajem i elitą, najmocniejsze ciosy, oś czasu etapów – dni, usunięte, premia,
SMS, magazyn, ulepszenie, elita, boss, kombinacje, synergia), `Recap` (rada – pierwsza pasująca z `recap.tips` – i najbliższy
cel: najtańsza ranga Respektu albo Szkolenie), `Weekly` (tydzień od poniedziałku `weekly.epoch`, seed z numeru tygodnia,
zasady z `weekly.list`: zawód, bez kawy – kawa na wynos, elity %, pogoda, bez Hurtowni, materiały %, HP %, ciosy %,
budżet; wyniki 3 tygodni), `Story` (wątki `story.arc` za kamienie milowe, nowe / przeczytane, ozdoby Osiedla `estate.decor`);
profil v11 (188 bajtów, migracja z v10), zapis PBRUN13, test złoty z polem `part4` i 6 budowami tygodnia, testy
`RecapWeeklyStoryTests`. Po SMS-ie końca budowy (i harmonogramie domu po wygranej) **Podsumowanie** (`RecapScreen` /
`RecapPage`): jedna przewijana strona – co zatrzymało budowę („Pokonało Cię: Zwarcie”, etap i akt), ostatnie ciosy,
najmocniejsze ciosy, oś czasu, nagrody (doświadczenie, Respekt, zlecenie, rekord dnia / tygodnia), najbliższy cel i rada;
góra/dół albo dotknięcie górnej / dolnej połowy przewija. Tytuł: **Wyzwanie tygodnia** (`WeeklyScreen` / `WeeklyPage`:
zasady, zawód, wyniki tygodni, „Wyślij wynik” – `ILeaderboard.WeeklyBoardId` / `SubmitWeekly`, zaślepka bez sieci),
zasada w telefonie > Zadania, bez NG+. Profil > Osiedle: ozdoby rosną z wygranymi, **Wiadomości** (archiwum wątków,
„Nowa” do przeczytania, zablokowane z podpowiedzią), baner „Nowa wiadomość” na planszy końcowej; Jak grać – strona 7.
Sceny zrzutów: `recap-endmsg`, `recap-death`, `recap-death-scroll`, `recap-win`, `recap-end`, `weekly`, `weekly-card`,
`weekly-run`, `story-archive`, `story-thread`, `estate-grow`, `help-meta`; test dymny: wyzwanie tygodnia (start z zasadą,
porażka, podsumowanie z przewijaniem, bez NG+, tabela tygodnia), podsumowanie po wygranej, Wiadomości (wątek przeczytany).

Nowe w v0.21.49 cz. 3 (warstwa Godota): **Akt 0 „Papierologia”** z nagrody za odbiór - kafle biura z regałami
segregatorów (Działka i pozwolenie) i wykopu z rurą (Przyłącza), 8 nowych problemów i boss Decyzja odmowna (klatki
101-118, chód jak reszta), dokumenty do zebrania (klatki 119-121, złota poświata), kłódka na zamkniętych schodach,
ikona pieczątek z licznikiem „n/3” w HUD (zielony po komplecie), wiersz „Dokumenty n/3, schody zamknięte/otwarte”
w Zadaniach, banery „Dokument: …”, „Komplet! Schody otwarte” i „Druga faza: Odwołanie!” (czerwony błysk, wstrząs,
wezwanie); numer etapu i aktu z rdzenia („Akt 0, 1/12”, bez Aktu 0 „Etap 1/10”), harmonogram i harmonogram domu od
pierwszego etapu budowy, nagroda „Akt 0” z ikoną pieczątki. **Samouczek menu (#25)** (`scripts/Guide`: Coach,
CoachView): przy pierwszym uruchomieniu tytuł i wybór zawodu przyciemniają się, podświetlony element (pozycja menu,
klucz opcji, pasek zawodów, trudność, pamiątka, statystyki, tryb inwestora, start) i dymek jak powiadomienie
PlanBudowlany od Kierownika Marka (teksty `tutorial` z game.json - te same co na GBA, licznik kroków); Spacja / Enter
/ dotknięcie = dalej, Esc / B / „Pomiń” = pomiń, przy statystykach I / „Statystyki” otwiera ich opis (samouczek trwa
dalej po powrocie). Potem jeden dymek przy pierwszym odblokowaniu: Respekt, codzienna budowa, Akt 0 (tytuł), tryb
inwestora i nowy zawód (wybór zawodu). Jak grać z tytułu: Tab / „Samouczek jeszcze raz”. Sceny zrzutów:
`tutorial-title`, `tutorial-class`, `tutorial-stats`, `tutorial-unlock`, `tutorial-act0`, `help-tutorial`, `act0-card`,
`act0-stamps`, `act0-stairs-open`, `act0-boss-phase`; test dymny przechodzi cały samouczek na świeżym profilu (z opisem
statystyk), dymki nowości (każdy raz), powtórkę z Jak grać oraz Akt 0 (bot zbiera dokumenty, druga faza bossa,
dalej Fundamenty).

Nowe w v0.21.49 cz. 2 (warstwa Godota): 10 etapów z kaflami wg aktu (akt I ziemia i bloczki / izolacja, akt II
deski, dachówka, cegła, akt III płytki, tynk, instalacje), 20 nowych problemów etapów (klatki 61-100, oddech jak
reszta) - pojawiają się w trakcie etapu (podział, powrót), strzały z dystansu (iskry od strzelca), wybuch (czerwone
pola jak cios bossa, eksplozja), błoto w akcie I (plama na podłodze), porywy w akcie II (ikona z licznikiem w HUD,
pył w stronę porywu), pył w akcie III (drobinki w powietrzu), baner mechaniki na starcie aktu, wiersz mechaniki
w Zadaniach i na karcie etapu, zachowania („Cechy: ...”) na karcie problemu i w Katalogu. Opis statystyk (#19): na
wyborze zawodu dymek nad statystyką (mysz albo dotknięcie), I / przycisk „i” = strona Statystyki (wartości i co dają,
Spacja: wzory), w telefonie Start > Spacja / „Opis statystyk” = skąd są premie; Jak grać ma 3 strony (sterowanie,
akty i zachowania problemów, statystyki). Sceny zrzutów: `behaviors`, `act-mud`, `act-gust`, `act-dust`,
`stats-class`, `stats-tip`, `stats-phone`, `catalog-tags`, `help-acts`, `help-stats`; test dymny gra etap każdego aktu,
pokaz zachowań (podział, wybuch, strzał), statystyki z telefonu i wyboru zawodu oraz 3 strony Jak grać.

Nowe w v0.21.49 (warstwa Godota): telefon profilu, zakładka Koszty – strony Szkolenia / Respekt / Nagrody (Tab albo
przycisk strony na dotyku; Spacja/„Kup” kupuje rangę), 9 zawodów na wyborze zawodu (zawody z nagród: „za N. wygraną”),
banery „Respekt +N”, „Nagroda: …” i „Druga szansa!”, efekty mocy Rynna, Narzut i Taran, 5 slotów sprzętu (buty, pas),
ceny brygady i Hurtowni z rabatem. Sceny zrzutów: `respect`, `rewards`, `classselect-locked`, `class-dekarz`,
`class-tynkarz`, `class-operator`, `gear5`, `respect-banner`, `second-chance`; test dymny kupuje rangę Respektu,
odbiera nagrodę po wygranej i gra każdym nowym zawodem.

Nowe w v0.21.48 (warstwa Godota): mapka wyboru ścieżki na harmonogramie (strzałki / dotknięcie, Enter), materiały
w HUD, w Sprzęcie i w Hurtowni (towary za materiały), naprawy Załataj i Kładka na stronie Brygada i naprawy,
Codzienna budowa w menu tytułu (data z systemu, zawód i modyfikatory dnia, wyniki ostatnich dni, „Wyślij wynik”
przez `ILeaderboard` – na razie lokalna zaślepka, Game Center później, bez kodu sieciowego), harmonogram domu po
wygranej (dom z Osiedla, daty, dni, koszty i link planbudowlany.online), wyraźny awans (poświata, gwiazdki, napis
„AWANS! Poziom N” i co się poprawiło) oraz na końcu budowy rekord, najbliższe zlecenie i najbliższy zakup.

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
| Okna (v0.21.51, wszędzie tak samo): wybierz / dalej – wróć / zostaw | Spacja, Enter – Z, Esc | A, Start – B | przycisk po prawej – po lewej |
| Paczka sprzętu: A zakładam / B zostawiam | Spacja, Enter / Z, Esc | A / B | |
| Dalej (wiadomości, harmonogram); Hurtownia: kup zaznaczone / dalej | Enter / Spacja; Z / Esc = dalej z Hurtowni | Start / A; B | |
| Nowe narzędzie przy ulepszonym: zaznacz / zatwierdź / zostaję | strzałki, Spacja/Enter, Z/Esc | D-pad, A, B | wiersz, „Wybierz”, „Zostaję” |
| Powiadomienie push: otwórz jego zakładkę w telefonie | | | lewy klik / dotknięcie |
| Prolog: pomiń | Spacja / Enter | A / Start | klik |
| Tytuł: menu (Nowa budowa, Profil, Szkolenia, Jak grać) | ↑/↓ + Enter | D-pad + Start | |
| Wybór zawodu: zawód / trudność / pamiątka / start / wróć | ←/→, ↑/↓, Q/E, Enter, Esc | D-pad, LB/RB, Start, B | |
| Profil w telefonie (odznaki, zlecenia, pamiątki; Spacja zmienia stronę) | P | X | |
| Brygada (menu akcji: Spacja bez kierunku; telefon: Sprzęt → Spacja) | Enter, Spacja | Start, A | |
| Tryb inwestora na wyborze zawodu (po pierwszej wygranej; Spacja włącza modyfikator) | Tab | Select | |
| Harmonogram: wybór ścieżki kolejnego etapu | ←/→ (↑/↓), Enter | D-pad, Start | dotknięcie gałęzi |
| Naprawy (telefon: Sprzęt → Brygada i naprawy, Spacja) | Spacja | A | dotknięcie |
| Codzienna budowa (menu tytułu): start / wyślij wynik / wróć | Spacja, Tab, Esc | A, Select, B | przyciski |
| Harmonogram domu po wygranej: planbudowlany.online / dalej | Tab / Spacja, Enter | Select / A, Start | przyciski |
| Szkolenia (zakładka Koszty w telefonie profilu, Spacja kupuje) | K | Y | |
| Opis statystyk na wyborze zawodu (dymek: mysz nad statystyką) | I | R3 | „i” na karcie |
| Opis statystyk w trakcie budowy (telefon: Start) | Spacja | A | „Opis statystyk” |
| Rozpiska obrażeń broni (telefon: Sprzęt; na wyborze zawodu dymek nad narzędziem) | I | R3 | „Obrażenia” / dotknięcie narzędzia |
| Ustawienia (także klucz w prawym górnym rogu tytułu i mapy) | Esc | | klik na klucz |
| Samouczek menu: dalej / pomiń / opis statystyk | Spacja, Enter / Esc / I | A / B / R3 | przyciski dymka |
| Jak grać z tytułu: samouczek jeszcze raz | Tab | Select | „Samouczek jeszcze raz” |

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
| Telefon → **Sprzęt → Brygada** | fachowiec raz na etap (przycisk „Wezwij”, zużywa turę) i naprawy za materiały („Napraw”) |
| Harmonogram | dotknięcie gałęzi mapki = ścieżka, przycisk „Dalej: …” |
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
- Test dymny bez okna (bot gra kilka etapów przez warstwę Godota, wybiera drugą ścieżkę, łata drogę deskami, gra
  budowę dnia i przechodzi harmonogram domu po wygranej; kod 0 = OK):
  `godot-mono --headless --path GODOT/godot -- --smoke`
- **Test małpy** (v0.21.51 cz. 2): `godot-mono --headless --path GODOT/godot -- --monkey SEED KROKI [--touch --portrait]`
  – bot przez prawdziwe wejście (klawisze przez `Input.ParseInputEvent`, mysz / dotyk przez `Viewport.PushInput`, więc
  działa `GestureTracker`, przyciski ekranowe i blokada wejścia) wykonuje KROKI losowych akcji: klawisze z mapy sterowania,
  stuknięcia (40% w przyciski, wiersze list i zakładki z widoków `ITapTargets`, reszta gdziekolwiek), przesunięcia (też
  z palcem trzymanym dalej), przytrzymania, trzymane A/B ze strzałkami, prawy klik; na mapie krok bota i rzadko skrót
  etapu (jak L+R+SELECT), żeby dojść do premii, wydarzeń, Hurtowni, podsumowania i końca budowy. Połowa seedów zaczyna
  od pustego profilu (samouczek), połowa od bogatego (wszystkie sekrety, wygląd, inwestor). Czeka na koniec blokady wejścia
  jak człowiek (10% akcji wpada w blokadę). Błąd = wyjątek albo błąd w logu Godota (`MonkeyLogger`), błąd rysowania
  (`DrawErrors`) albo zawieszenie (ten sam ekran, strona telefonu, samouczek i stan budowy przez 400 akcji i ≥ 6 s); wtedy
  „MONKEY FAIL” ze śladem 40 ostatnich akcji i kod 1, inaczej „MONKEY OK” z liczbą odwiedzin ekranów. Linki do przeglądarki
  w testach tylko się liczą (`ExternalLinks`). Wiele seedów równolegle, np.
  `for s in $(seq 1 50); do godot-mono --headless --path GODOT/godot -- --monkey $s 3000 > /tmp/m_$s.log & done`.
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
  bohatera), `investor` (tryb inwestora nad wyborem zawodu), `schedule-path` (wybór ścieżki), `materials` (HUD
  z materiałami), `repairs` (Brygada i naprawy), `hurtownia-mats` (towary za materiały), `daily` (codzienna budowa),
  `house` (harmonogram domu po wygranej), `levelup` (wyraźny awans), `death` (SMS po porażce: co zostaje),
  `behaviors` (strzał, podział, wybuch), `act-mud`, `act-gust`, `act-dust` (mechaniki aktów), `stats-class`, `stats-tip`,
  `stats-phone` (opis statystyk), `catalog-tags` (Katalog z zachowaniami), `help-acts`, `help-stats` (strony Jak grać),
  `tutorial-title`, `tutorial-class`, `tutorial-stats` (samouczek menu), `tutorial-unlock`, `tutorial-act0` (dymki
  nowości), `help-tutorial`, `act0-card`, `act0-stamps`, `act0-stairs-open`, `act0-boss-phase` (Akt 0), `dmg-class`, `dmg-stats`, `dmg-gear`,
  `dmg-phone`, `dmg-crit`, `dmg-offer`, `dmg-tool`, `dmg-enemy`, `help-dmg` (rozpiska obrażeń broni); sekretne zlecenia
  (v0.21.51 cz. 2) – lista `SecretStaging.Names` (opis wyżej).
  Sceny ustawiają stan ręcznie (profil pokazowy, skrót zaliczenia etapu jak L+R+SELECT na GBA); zrzuty i test dymny
  działają bez dźwięku.

### Ustawienia lokalne (klucze, zespół, urządzenie)

Repo jest publiczne, więc ścieżki do kluczy, identyfikatory kluczy API, zespół Apple i UDID urządzenia są tylko
w `GODOT/.env.local` (w .gitignore; wzór bez wartości: `GODOT/.env.example`). Skrypty z `GODOT/tools` czytają zmienne
z otoczenia albo z tego pliku. Same klucze (`.p8`, keystore, konto serwisowe) leżą poza repo; hasła dopisuje się
ręcznie do `.env.local`. Pole zespołu w `godot/export_presets.cfg` jest puste – skrypty wstawiają je tylko na czas eksportu.

### iPhone (iOS)

`GODOT/tools/ios_deploy.sh` – wersja z `GBA/data/game.json` do presetu, `dotnet build`, eksport projektu Xcode
(`godot-mono --headless --export-debug iOS`), `xcodebuild` z automatycznym podpisem (klucz API App Store Connect),
instalacja i uruchomienie przez `xcrun devicectl` (USB albo Wi-Fi). Opcje: `--export` (tylko projekt Xcode),
`--no-launch`; zmienne `UDID`, `ASC_KEY`, `ASC_KEY_ID`, `ASC_ISSUER_ID`, `TEAM_ID`, `BUNDLE_ID`. Wyniki w `GODOT/build/ios`.
Wymaga szablonów eksportu 4.7.2.stable.mono, Xcode i: `rendering/textures/vram_compression/import_etc2_astc=true`
w project.godot (bez tego eksport iOS kończy się po cichu) oraz `godot/LifeLike.Game.sln` obok project.godot.
Ikona `godot/icon.png` (1024x1024) powstaje w `tools/export_godot_assets.py`.

### TestFlight

`GODOT/tools/testflight_upload.sh` – eksport release, archiwum, podpis lokalnym certyfikatem „Apple Distribution”
z profilem App Store (tworzy/instaluje go `tools/asc_profile.py` przez App Store Connect API) i wysyłka do App Store Connect.
Numer buildu = data i godzina. `--no-upload` – tylko .ipa w `GODOT/build/testflight`.

### Android i Google Play

`GODOT/tools/android_release.sh` – AAB release przez gradle (szablon buildu instaluje się sam, JDK 17, Android SDK),
podpisany kluczem uploadu z `ANDROID_KEYSTORE_PATH` / `ANDROID_KEY_ALIAS` / `ANDROID_KEYSTORE_PASSWORD`;
`--upload` wysyła go przez `tools/play_upload.py` (konto serwisowe `PLAY_SERVICE_ACCOUNT_JSON`) na ścieżkę internal.
Pierwszy AAB nowej aplikacji trzeba wgrać ręcznie w Play Console.

Przy pierwszym uruchomieniu z terminala najpierw `dotnet build GODOT/godot/LifeLike.Game.csproj`
i `godot-mono --headless --path GODOT/godot --import` (import grafik i dźwięków z `godot/assets`).

## Grafika, font i dźwięk z GBA

`python3 GODOT/tools/export_godot_assets.py` (Pillow; do muzyki `openmpt123` i `ffmpeg` z libmp3lame) czyta – tylko
czyta – `GBA/graphics/*.bmp`, `GBA/include/font_widths.h` i `GBA/audio/*` i zapisuje do `godot/assets/`:
`sprites/` (postacie, wrogowie, w tym problemy etapów 61-100, bossowie, znajdźki, paczki, celownik – klatki 32x32 powiększone algorytmem Scale2x
z 16x16, kolejność klatek jak `actors.bmp`; białe sylwetki do błysku; cząsteczki 16x16; domy Osiedla; ikony menu
i mocy), `ui/` (ikony telefonu aktywne i nieaktywne, ikony HUD 16x16 i szare do ładowania mocy, plansze tytułu
i końca z przezroczystym tłem), `tiles/stage_N.png` (12 etapów, w tym Akt 0 - biuro z segregatorami i wykop z rurą, podłoga wg aktu: 4 warianty podłogi, podłoga z cieniem muru, mur,
lico muru, schody – rysowane w 32x32 w paletach etapów z GBA, bogatsze niż kafle 8x8), `fx/` (cień, pole ciosu i wybuchu, błoto,
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

Odtworzenie plików (z tej samej wersji nagłówków GBA co `golden/game.json`; obecnie migawka v0.21.52 cz. d (wcześniej v0.21.51, v0.21.49 cz. 2) – 10 etapów,
zachowania problemów, mechaniki aktów, bot omija błoto i czerwone pola wybuchu – 37 przebiegów (w tym nowe zawody, pełny Respekt, nagrody za odbiór, Druga szansa) – bot „smart”
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
LaunchOptions.cs   argumenty --seed / --smoke / --screenshot --scene / --monkey SEED KROKI / --touch / --portrait / --size
Input/             GameAction (A, B, L, R, START, SELECT, strzałki...), InputCmd (zdarzenie jako akcje, wciśnięte
                   i puszczone), GameInput (mapa klawiszy i pada, Translate, Press/Release dla przycisków
                   ekranowych, IsHeld), ButtonNames (A/B/START... w tekstach z game.json -> klawisze)
Session/           GameSession (budowa + profil: start, budowa dnia, etap, NG+, odznaki, zlecenia, bankowanie, zapis),
                   ILeaderboard + LocalLeaderboard (tabela wyników dnia: zaślepka pod Game Center),
                   SessionEvents (LevelUp, PickedUp, Dropped, ToolFound, GearEquipped, AbilityReady, BossSpotted,
                   StageCleared, RunEnded, Achievements), TurnWatcher (wykrywa zdarzenia tury), GodotDataSource
Screens/           Screen (Enter / Exit / HandleInput / Process + deklaracja warstw i muzyki), ScreenFlow (maszyna
                   stanów), ekrany: Title, Profile, ClassSelect, Prologue, PrologueMessage, Help, Game, Phone,
                   StageCard, Schedule (wybór ścieżki), Hurtownia, Offer, EndMessage, HouseSchedule, End, Daily; Play/ (tryby mapy: ActionMenu, Aiming,
                   EnemyLook, PlayCommands, TouchPlay - gesty na mapie, AutoWalk + PathFinder -
                   marsz po dotknięciu pola); Settings (ustawienia nad bieżącym ekranem); Views/ (TitleView, ClassSelectView, EndView, PrologueView, PrologueStage)
World/             WorldView (sprite'y, synchronizacja), WorldFx (trafienia, moce, awans, konfetti), WorldCamera,
                   warstwy: MapLayer, OverlayLayer, FogLayer, FxLayer, MarksLayer, ActorSprite
Touch/             GestureTracker (dotyk -> gesty: Down, Drag, Swipe, SwipeRepeat, LongPress, Tap, Up), Gesture,
                   TouchControls (warstwa: ActionBar - pasek akcji, VirtualStick - gałka), BarButton, TouchIcon,
                   ITapTargets (prostokąty przycisków dla testu małpy)
Settings/          GameSettings (user://settings.cfg, Changed), ControlScheme, Haptics (wibracje przy dźwiękach)
Hud/               SettingsButton (klucz ustawień), HudLayer (HudTop, HudLog, EnemyCard, ScreenTint), PushBanners + PushBanner (rysowanie, trafienie
                   kliknięciem), BannerFeed (treść i zakładka telefonu z SessionEvents)
Phone/             PhoneView (telefon), PhonePage, PhonePainter, PhoneTabs, Backdrop; Tabs/ (w grze), ProfileTabs/,
                   Pages/ (wiadomość, harmonogram z radą, Hurtownia, paczka, Jak grać)
Gfx/               Pal (tokeny kolorów PlanBudowlany i GBA), Ink, Layout (rozmiar UI, skale HUD i mapy, bezpieczny
                   obszar - jedyne miejsce), ScaledLayer (warstwa z własną skalą), Assets, PixelFont (skala
                   ułamkowa), Ui, UiText, DrawHook, DrawErrors
Audio/             Sfx (dźwięki i muzyka), SoundCues (dźwięki zdarzeń sesji)
Guide/             Coach (samouczek menu i dymki nowości: kroki, flagi w profilu), CoachView (przyciemnienie, podświetlenie,
                   dymek Kierownika Marka), CoachHit
Debug/             DebugRunner (--smoke / --screenshot / --monkey), SmokeTest (+ SmokeSecrets), ScreenshotRunner, DebugScenes,
                   DemoStaging, RecapStaging, SecretStaging, DemoProfile, MonkeyTest + MonkeyLogger (test małpy)
```

Przepływ: `Main` tłumaczy zdarzenie na `InputCmd` -> `ScreenFlow.Current.HandleInput` -> ekran woła akcję rdzenia
(np. `PlayerMove`) -> `App.AfterAction` -> `Refresh` (widok mapy zużywa trafienia tury, `TurnWatcher` zgłasza
zdarzenia -> banery / dźwięki / efekty) -> `GameSession.Resolve` (etap zaliczony, koniec budowy, paczka) -> kolejny ekran.

Skalowanie: `Layout` liczy wszystko od widocznego prostokąta okna – `ContentScale` (piksele okna na piksel UI,
całkowita), `HudScale` (1.5x, zaokrąglone tak, by piksel był całkowitą liczbą pikseli okna), `WorldZoom` (~9 pól
w pionie), `SafeArea` (wycięcia ekranu na telefonie). Widoki układają się od `Size`/`UiSize` (kotwice), więc układ
pionowy (telefon) wymaga tylko własnych proporcji elementów, bez zmian w logice ekranów. Przyciski ekranowe
wstrzykują akcje przez `GameInput.Press/Release` (ta sama ścieżka co klawiatura, łącznie z trzymaniem A/B).
