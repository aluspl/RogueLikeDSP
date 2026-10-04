# TODO – PlanBudowlany RogueLike

Stan na 2026-10-04: **v0.21.54 wydane – ostatnia wersja serii 0.21** (GitHub, Drive, TestFlight, Google Play).
Kolejne iteracje to seria **0.22** (otwarta wersja: v0.22.0). Zasada: pomysły i poprawki trafiają do otwartej wersji,
po skończonym pakiecie changelog i pytanie „wydajemy?”; pełny zestaw testów (skill `rogue-test`) raz przed wydaniem.
Historia zmian: [`GBA/CHANGELOG.md`](GBA/CHANGELOG.md). Opis projektów: [`README.md`](README.md).

Legenda: ✅ zrobione · 🔄 w toku · ⬜ do zrobienia · — nie dotyczy

## W toku i następne

| Zadanie | GODOT (MOBILE) | GBA |
|---|---|---|
| Pion na telefonie, marginesy pod wyspę i pasek Home, skalowanie bez czarnych pasów | ✅ | — |
| Sterowanie jedną ręką: przesuwanie palcem, stuknięcie w pole/wroga, pasek ikon akcji (Atak, Moc, Termos, Czekaj, Telefon) | ✅ | — |
| Opcje pod ikoną klucza (głośność, wibracje, joystick, pasek pod lewą/prawą rękę, wielkość tekstu, Zapisz i wyjdź) | ✅ | ⬜ (opcje dźwięku w menu) |
| Przycisk planbudowlany.online na tytule | ✅ | ✅ (QR na ekranie końcowym) |
| Instalacja na urządzeniu jedną komendą (`GODOT/tools/ios_deploy.sh`) | ✅ | — |
| Test ręczny na sprzęcie i poprawki po graniu | 🔄 iPhone (zainstalowane) | ⬜ Miyoo, dźwięk |
| Android: eksport i instalacja (podpis bez sekretów w repo) | ⬜ | — |
| Wydania do pobrania w Releases | ⬜ .ipa / .apk / desktop | ✅ ROM co wersję |
| Oprawa 2.5D: kamera 3/4, dynamiczne światło (latarka czołowa), pogoda na etapie Dach | ⬜ | — |
| Mapy tematyczne etapów (szalunki, otwory okienne, krokwie, bruzdy) i elementy otoczenia (rusztowania, betoniarka, palety) | ⬜ | ⬜ (prostsza wersja) |
| Zadania etapu jako cele poboczne (odbiór częściowy, dodatkowe doświadczenie) | ⬜ | ⬜ |
| Drzewka ulepszeń mocy zawodów w zakładce Koszty | ⬜ | ⬜ |
| Muzyka: pętle bez przerwy | ⬜ (MP3 → OGG) | ✅ |
| Portret Kierownika na wyborze zawodu lekko przesunięty | — | ⬜ |
| Czytelniejszy komunikat „Brak celu w zasięgu 1” (`core.h` + port C#) | ⬜ | ⬜ |
| Bot balansu bez oscylacji (cel po odległości ścieżki, 0 utkniętych przebiegów) i trudniejszy balans v0.21.48 (bot: Łatwy 60%, Normalny 30%, Trudny 12%, pełne Szkolenia 89% – cel 50–60% jeszcze nie osiągnięty, wszystkie modyfikatory 10%) | ✅ wspólny rdzeń | ✅ |
| Balans v0.21.49 (9 zawodów, bot): Normalny 32%, pełne Szkolenia 57%, + pełny Respekt 74%, + wszystkie modyfikatory 10% | ✅ wspólny rdzeń | ✅ |
| Balans v0.21.49 cz. 2 (10 etapów, nowi wrogowie, mechaniki aktów): Łatwy 58%, Normalny 30%, Trudny 12%, pełne Szkolenia 57%, + pełny Respekt 71%, + wszystkie modyfikatory 10%; kawa 2,6/budowę (80% budów), bez kawy 17% | ✅ wspólny rdzeń | ✅ |
| Balans v0.21.49 cz. 3 (Akt 0): bez meta (Akt 0 zablokowany) 30%, pełne Szkolenia + pełny Respekt 71%, z Aktem 0 (wszystkie nagrody) 66% | ✅ wspólny rdzeń | ✅ |
| Balans v0.21.50 cz. 3 (wydarzenia, ulepszenia, magazyn): Łatwy 53%, Normalny 32%, Trudny 10%, pełne Szkolenia 54%, + pełny Respekt 70%, z Aktem 0 66%, + wszystkie modyfikatory 9% | ✅ wspólny rdzeń | ✅ |
| Balans v0.21.50 cz. 4: bez zmian (podsumowanie tylko zapisuje), wyzwania tygodnia (bot): Glazurnik bez kawy 11%, Elity x2 31%, Mokry tydzień 35%, Bez Hurtowni 18%, Szklany kask 24%, Kierownik na placu 33% | ✅ wspólny rdzeń | ✅ |
| Balans v0.21.51 cz. 1 (błoto co 14. pole): Łatwy 54%, Normalny 32%, Trudny 10%, pełne Szkolenia 54%, + pełny Respekt 69%, z Aktem 0 67%, + wszystkie modyfikatory 10%; wyzwania tygodnia 10–36% | ✅ wspólny rdzeń | ✅ |

## Nowe pomysły (2026-09-25, „zrób wszystko”)

Kolejność: A – rdzeń i GBA (po bossie Inspekcja Pracy), B – Godot i mobile (po wersji mobilnej), C – 2.5D.

| # | Pomysł | Etap | GODOT (MOBILE) | GBA |
|---|---|---|---|---|
| 0 | Boss Inspekcja Pracy (podkładka z pieczątką, akt III, Kontrola BHP) | – | ✅ | ✅ |
| 1 | Codzienna budowa: seed dnia, tabela wyników | A/B | ✅ (data z systemu; wyślij wynik – zaślepka pod Game Center) | ✅ (data ustawiana ręcznie, pamiętana w profilu) |
| 2 | Brygada: najemny fachowiec raz na etap (Geodeta – mapa, Pompa do betonu – obszar…) | A | ✅ | ✅ |
| 3 | Wybór ścieżki między etapami (harmonogram z rozgałęzieniami; może później połączyć się z #21) | A | ✅ | ✅ |
| 4 | Materiały (cement, stal, drewno) – Hurtownia i naprawy pól (Załataj, Kładka) | A | ✅ | ✅ |
| 5 | Pogoda dnia (upał, mróz, wiatr) jako modyfikator etapu | A | ✅ | ✅ |
| 6 | Tryb inwestora: modyfikatory trudności za dodatkowe doświadczenie (jak Heat w Hadesie) | A | ✅ | ✅ |
| 7 | Powiadomienia systemowe („Nowa codzienna budowa”) | B | ⬜ | — |
| 8 | Widżet / Live Activity z postępem budowy | B (natywne rozszerzenie iOS) | ⬜ | — |
| 9 | Udostępnianie wyniku (obrazek + link planbudowlany.online) | B | ⬜ | ⬜ (kod QR z wynikiem) |
| 10 | Game Center / Google Play Games: osiągnięcia z odznak, tabela wyników | B (konfiguracja w App Store Connect) | ⬜ | — |
| 11 | 2.5D: kamera 3/4, latarka czołowa, cienie ścian | C | ⬜ | — |
| 12 | Ulewa jako pogoda na etapie Dach (deszcz, kałuże, poślizg) | C | ⬜ | ⬜ (prostszy efekt) |
| 13 | Po wygranej: harmonogram domu w stylu aplikacji + „Zaplanuj swoją budowę” | A/B | ✅ | ✅ |
| 14 | Więcej etapów (np. 10–12) i wyraźna różnorodność między aktami – własne kafle/paleta, zestaw problemów, mechanika aktu (np. akt I wykop i błoto, akt II wysokość i wiatr, akt III instalacje i terminy) | A3, v0.21.49: 10 etapów (Izolacja fundamentów, Ściany działowe), kafle aktów, błoto / porywy / pył | ✅ | ✅ |
| 15 | Wyraźny awans na poziom – poświata i napis „AWANS!” | A | ✅ | ✅ |
| 16 | Przygotowanie pod synchronizację w chmurze – warstwa zapisu (profil + budowa) z lokalną implementacją i miejscem na iCloud / Game Center saved games i Google Play Games Saved Games | A3 | ⬜ | — |
| 17 | Przedmioty (kawa, termos) mają znaczenie – po zmianie balansu sprawdzić, czy są używane (bot: statystyka użycia kawy). v0.21.49: bot pije 2 kawy na budowę (74% budów), bez kawy Normalny 20% zamiast 32%, z pełnymi Szkoleniami 39% zamiast 57% – kawa ma znaczenie | A3 | ✅ (wspólny rdzeń) | ✅ |
| 18 | Więcej różnych przedmiotów – nowe jednorazowe (np. apteczka, energetyk, taśma naprawcza, plan awaryjny), nowe elementy sprzętu i cechy, rzadkie przedmioty unikalne | A3 | ⬜ | ⬜ |
| 19 | Opis statystyk – co robi każda statystyka i jak (wzór w prostych słowach, np. „SIŁ: +1 obrażeń co 2 pkt dla broni SIŁ”), na wyborze zawodu, w telefonie (Start/Sprzęt) i w Jak grać | A3, v0.21.49: wybór zawodu (GBA START, Godot podpowiedzi), telefon Start → skąd premie, Jak grać | ✅ | ✅ |
| 20 | Akt 0 „Papierologia” przed stanem surowym – etapy Działka i pozwolenie, Przyłącza; problemy papierowe i sieciowe (Brakujący podpis, Zaginiony wniosek, Termin na odwołanie, Niezgodność z planem, Pieczątka nie ta, Pęknięta rura, Brak ciśnienia, Kolizja z kablem) – nigdy urzędnicy; boss „Decyzja odmowna” z drugą fazą Odwołanie; mechanika pieczątki (3 dokumenty otwierają schody) | A3, v0.21.49: nagroda za 8. odbiór, potem każda budowa od Aktu 0 | ✅ | ✅ |
| 21 | Bonus między etapami – po każdym etapie wybór 1 z 3 premii na bieżącą budowę (np. +2 max HP, moc -1 t., kryt +5%, kawa +2 HP, brygada -5 zł), rzadkość premii (może się połączyć z wyborem ścieżki #3) | A3 | ⬜ | ⬜ |
| 22 | Respekt – stała waluta za ukończenie każdego etapu (więcej za bossów i akty), zapisana w profilu (nie przepada przy śmierci); wydawana na stałe ulepszenia procentowe z rangami (lista niżej) – v0.21.49: telefon profilu → Koszty → SELECT = Respekt | A3 | ✅ | ✅ |
| 23 | Odblokowania za kolejne przejścia (jak Slay the Spire): każda wygrana odblokowuje coś nowego – lepsze narzędzie, element sprzętu, nowy zawód (np. Dekarz, Tynkarz, Operator koparki), nowy akt/etap, nowy tryb; lista nagród po kolei widoczna w profilu – v0.21.49: Młot udarowy, Dekarz, Buty robocze, Pistolet do kotew, Tynkarz, Pas narzędziowy, Operator koparki, Akt 0 „Papierologia”; strona Nagrody (Koszty → SELECT → SELECT) | A3 | ✅ | ✅ |

| 24 | Wrogowie pasujący do etapu – każdy etap ma własny zestaw problemów (2–3 nowe na etap), np. Fundamenty: Woda gruntowa, Osuwisko skarpy, Kamień w wykopie; Mury: Krzywy mur, Pęknięty pustak, Mostek termiczny; Strop: Ugięcie stropu, Brak zbrojenia; Dach: Przeciekająca papa, Wichura, Zapchana rynna, Oblodzenie; Okna i drzwi: Nieszczelna ramka, Zła wymiarówka, Przeciąg; Instalacje: Zwarcie (jest), Zapowietrzenie, Kolizja rur, Brak uziemienia; Tynki i wylewki: Rysa skurczowa, Wilgoć w ścianie, Pęcherz tynku; Wykończenie: Fuga nie ta, Odpryski płytek, Poprawki na odbiorze. Każdy z własnym zachowaniem (powolny ale twardy, dzieli się, ucieka, strzela z dystansu, leczy innych, wybucha) – razem z #14 | A3, v0.21.49: 20 nowych (2 na etap), 9 zachowań z danych | ✅ | ✅ |
| 25 | Samouczek menu przy pierwszym uruchomieniu – podświetlanie po kolei elementów tytułu i wyboru zawodu (Nowa budowa, Profil/telefon, Szkolenia, Respekt, Codzienna budowa, klucz/opcje, trudność, pamiątka, statystyki, tryb inwestora) z dymkiem Kierownika Marka, dalej A/stuknięcie, pomiń; dymki przy pierwszym odblokowaniu (Respekt, budowa dnia, tryb inwestora, Akt 0, nowy zawód); flagi w profilu v10, powtórka w Jak grać | A3, v0.21.49 | ✅ | ✅ |

## Respekt – lista do testów (#22)

Stałe ulepszenia z rangami (zakres do przetestowania; przesadzone skreślimy po testach). ✅ = zrobione w v0.21.49
(GBA i Godot), w nawiasie rangi po testach bota. Bot reaguje na każdą premię bojową (+1–2 pkt wygranych za 1–2%),
dlatego premie bojowe są małe – cel „pełne Szkolenia + pełny Respekt 65–75%” (wynik 74%). Premie bez wpływu na bota
(nie kupuje, nie używa mocy ani brygady) zostały w pełnym zakresie.

- ✅ obrażenia +1–20% → Pewna ręka +1/+2% (+5% dawało już +5 pkt wygranych)
- ✅ otrzymane obrażenia -1–20% → Gruba skóra -1/-2%
- ✅ szansa na lepszy sprzęt +1–20% → Dobre źródła +1…+5 do rzutu jakości
- ✅ kryt +1–10% → Oko fachowca +1/+2%
- ✅ leczenie kawy +5–50% → Mocna kawa +5/+10% (+25% dawało +7 pkt)
- ✅ odnowienie mocy -1–2 t. → Rutyna -1/-2 t.
- ✅ budżet na start +5–50 zł → Oszczędności +10…+50 zł
- ✅ doświadczenie +2–20% → Nauka +4…+20%
- ✅ brygada -5–30% ceny → Znajomości -6…-30%
- ✅ termos +1 miejsce → Duży termos
- ✅ unik +1–5% → Zwinność +1/+2% (łącznie z szczęściem i butami maks. 20%)
- ✅ zasięg widzenia +1 → Czujność
- ✅ tańsza Hurtownia -5–25% → Rabat
- ✅ więcej materiałów +10–50% → Zapasy
- ✅ druga szansa 1×/budowę (1 HP zamiast końca) → Druga szansa (najmocniejsza: +5 pkt wygranych)
- ⬜ start z przedmiotem – pominięte, na razie nie ma przedmiotów do zabrania (czeka na #18)

## v0.21.50 – regrywalność (cel: wszystko w jednym wydaniu)

| # | Pomysł | Część | GODOT (MOBILE) | GBA |
|---|---|---|---|---|
| 26 | Rozpiska obrażeń broni (jak BG3): od–do, kryt, wpływ statystyk, porównanie, karta wroga – v0.21.50 cz. 1 | 1 | ✅ | ✅ |
| 27 | Premia 1 z 3 po etapie (rzadkość, znaczniki, synergie, premie zawodów) – dawne #21 – v0.21.50 cz. 2 | 2 | ✅ | ✅ |
| 28 | Wzmocnione problemy (elity) z cechą i lepszą nagrodą – v0.21.50 cz. 2 | 2 | ✅ | ✅ |
| 29 | Kombinacje stanów (mokry + prąd = porażenie, pył + iskra = wybuch) – v0.21.50 cz. 2 | 2 | ✅ | ✅ |
| 30 | Wydarzenia z wyborem w trakcie etapu (SMS: ryzyko/nagroda) – v0.21.50 cz. 3: 12 wydarzeń, pole na etapie, skutki z szansą | 3 | ✅ | ✅ |
| 31 | Ulepszanie narzędzia w trakcie budowy (Hurtownia, materiały) – v0.21.50 cz. 3: do +3, od +2 cecha, ostrzeżenie przy zmianie | 3 | ✅ | ✅ |
| 32 | Ukryte pomieszczenia (klucz, magazyn ze skrzynią) – v0.21.50 cz. 3: pęknięta ściana / drzwi, klucz od problemu, strażnik | 3 | ✅ | ✅ |
| 33 | Podsumowanie po śmierci (co zabiło, oś czasu, najbliższy cel) – v0.21.50 cz. 4: też po wygranej; ostatnie ciosy, najmocniejsze ciosy, oś czasu etapów, nagrody, cel i rada; GBA 3 strony, Godot jedna przewijana | 4 | ✅ | ✅ |
| 34 | Wyzwania tygodnia (seed + zasady, osobne wyniki) – v0.21.50 cz. 4: 6 zasad z danych, wyniki 3 tygodni w profilu v11, Godot: zaślepka tabeli tygodnia (`ILeaderboard.WeeklyBoardId`) | 4 | ✅ | ✅ (tydzień z daty budowy dnia) |
| 35 | Fabuła odkrywana z kolejnymi budowami (SMS-y, Osiedle) – v0.21.50 cz. 4: 19 wątków za kamienie milowe, Wiadomości w Osiedlu, 6 ozdób Osiedla | 4 | ✅ | ✅ |

## v0.21.51 – poprawki po graniu na iPhonie (nie wydane osobno – część wydania v0.21.52)

| # | Pomysł | Część | GODOT (MOBILE) | GBA |
|---|---|---|---|---|
| 36 | Autokafle ścian: wierzch masy muru bez pasów, lico tylko nad podłogą, krawędzie i końce wg sąsiadów, światło z lewej góry, miękkie przejście w nieodkrytą ciemność (Godot: kafle z eksportu dla palety etapu) | 1 | ✅ | ✅ (lżej: wierzch + dolna połowa lica) |
| 37 | Błoto: dwa razy rzadziej (co 14. pole, z danych) i jako płaska mokra plama z połyskiem zamiast ciemnych dziur | 1 | ✅ (3 warianty) | ✅ |
| 38 | Spójne sterowanie i blokada wejścia: A / „Wybierz” zawsze po prawej, B / „Wróć” po lewej, Enter/Spacja = A, Esc/Z = B, 0,4 s blokady po otwarciu okna (GBA ~20 klatek), nieodwracalne wybory przez zaznaczenie (zamiana narzędzia: domyślnie „Zostaję”), zasada w Jak grać; HUD: pełna nazwa etapu w drugim rzędzie | 1 | ✅ | ✅ |
| 39 | „Sekretne zlecenia” – ukryte cele profilu (widoczne jako „???” z podpowiedzią), odblokowujące ukryte zawody (np. Spawacz, Geodeta, Majster „złota rączka”), unikalne bronie (Złota kielnia, Młot Zenka, Poziomica mistrza) i kosmetykę (kolory kasku); warunki nietypowe (np. wygraj bez kawy, pokonaj Termin samą brygadą, znajdź 5 magazynów w jednej budowie, wygraj każdym zawodem, przejdź Akt 0 bez obrażeń od Papierologii) | 2 | ✅ (rdzeń, ekrany, grafika, sceny zrzutów, test dymny) | ✅ (rdzeń, ekrany, grafika, scenariusze 61–67) |

### Gdzie skończyliśmy (2026-09-29, przerwane limitem sesji)

Zrobione i w repo (cz. 2): rdzeń sekretów GBA (8 zleceń z `secrets`, liczniki budowy, profil v12 196 B z migracją v11,
zawody Spawacz / Geodeta / Majster, Młot Zenka, Poziomica mistrza, Respekt „Zaprawiony w boju”, wygląd, PBRUN14;
balans nowych zawodów 35/36/23%), port C# + test złoty (55 przebiegów) + SecretsTests, ekrany GBA (strona Sekrety,
banery, dymek Nowość, kask w paski, złoty błysk kryta, Poziomica na mapie, moc Majstra w HUD, pixel art klatki 127–159,
Jak grać str. 15, scenariusze 61–67, kolejka banerów końca budowy).

Godot (2026-10-02, przerwane limitem): ekrany sekretów, eksport klatek 127–159, mgła pikselowa, test małpy `--monkey`
– zrobione i w repo; test dymny OK, `dotnet test` 217/217, build 0 ostrzeżeń; małpa 50 seedów x 3000 akcji
(klawiatura + mysz) bez błędów, seria pion + dotyk (50 x 3000) przerwana – do powtórzenia; zrzuty sekretów desktop + pion
obejrzane. Do zrobienia po stronie Godota: dokończyć serię małpy w pionie, ew. mniej czasu małpy w Ustawieniach (~45% akcji).

Zostało do wydania v0.21.51:
1. ✅ Godot – ekrany sekretów: strona Sekrety w profilu („???” + podpowiedź / warunek i nagroda), banery i dymek Nowość,
   nowe zawody na wyborze zawodu (portrety, moce Spaw / Tyczenie / Złota rączka, HUD mocy Majstra), bronie, wygląd
   (kask w paski, złoty błysk), Poziomica na podglądzie mapy; eksport nowych klatek (127–159) w `export_godot_assets.py`;
   test dymny i zrzuty.
2. ✅ Godot – mgła: krawędź z cz. 1 zbyt rozmyta; zrobić stopniowane / ditherowane zanikanie w rozdzielczości kafla (porównanie przed/po).
3. Dokumentacja: CHANGELOG v0.21.51 (cz. 2), READMEs, #39 ✅ w obu kolumnach.
4. Wydanie: GitHub Release + Drive, karta SD (`sudo GBA/tools/sd_copy.sh`), iPhone (`GODOT/tools/ios_deploy.sh`);
   TestFlight i Google Play dopiero po sygnale sesji PB-platnosci (priorytet: wydanie PlanBudowlany iOS 0.9.2).

### Gdzie skończyliśmy (2026-10-02)

v0.21.51 dokończone (CHANGELOG cz. 2, seria małpy pion + dotyk – poprawka AutoWalk) i v0.21.52 cz. a (tempo postępu
#41–#43, #52) w GBA i Godocie: balans Łatwy 54%, Normalny 32%, Trudny 10%, pełne Szkolenia 57%, + Respekt 72%,
modyfikatory 13%, Akt 0 68%; wszystko wykupione po ~23 budowach. Nie wydane – jedno wydanie po cz. b
(#44 inspektor, #45 mistrzostwo, #48 stopnie inwestora; potem #46, #47, #49–#51).

### Gdzie skończyliśmy (2026-10-03)

v0.21.52 cz. b (#44 inspektor, #45 mistrzostwo, #48 stopnie inwestora, #52 paski) w GBA i Godocie: profil v14 (240 B),
balans bez zmian celów (mistrzostwo + inspektor 70%), tempo: inspektor maks. po ~88 budowach, mistrzostwo 10 po ~22
budowach zawodem. Nie wydane – dalej cz. c (#46 drzewko Szkoleń, #49 kolekcje, #50 zadania dnia/tygodnia, #51 seria dni;
#47 mapa kariery), potem jedno wydanie v0.21.52.

### Gdzie skończyliśmy (2026-10-03, cz. c)

v0.21.52 cz. c (#46 drzewko Szkoleń, #49 kolekcje, #50 zadania dnia i tygodnia, #51 seria dni) w GBA i Godocie: profil v15
(384 B, zapis budowy GBA od 512 – przy migracji przerwana budowa przenosi się spod 256), Kask ojca -5/8/10% otrzymanych
obrażeń zamiast +OBR; balans: Szkolenia z najlepszymi wyborami drzewka 58% (sam pień 50%), + Respekt 73%, modyfikatory 12%,
Akt 0 67%, mistrzostwo i inspektor 70%; wszystko wykupione po ~25 budowach. Nie wydane – dalej cz. d (#47 mapa kariery:
nowe budynki z własnymi etapami, problemami i bossami), potem jedno wydanie v0.21.52.

### Gdzie skończyliśmy (2026-10-03, v0.21.53 cz. 2)

v0.21.53 cz. 2 (#40 język angielski) w GBA i Godocie: polski w `game.json` (dane, „ui” – 799 tekstów interfejsu GBA,
„uiGodot” – 661 Godota), angielski w `GBA/data/lang/en.json` (1356 tekstów danych, tłumaczenie pisane, nie dosłowne);
GBA – `core::ltext`, `UI(klucz)`, wybór języka przy pierwszym uruchomieniu i w Zespole (SELECT), profil v17 bajt 378;
Godot – `Loc`, `LangOverlay`, Ustawienia > Język / Language. `gen_data.py --check`: kompletność, font, szerokość px,
polskie napisy na sztywno (GBA i C#). Monkey po angielsku bez błędów. Dalej: wydanie v0.21.53 (GitHub Release + Drive,
karta SD, iPhone; karta sklepu bez „English is coming soon”).

### Gdzie skończyliśmy (2026-10-03, v0.21.53)

v0.21.53 (#53 filtry ekranu, #54 tryby dla daltonistów) w GBA i Godocie: sekcja `screenFilters` (warunki odblokowania
i teksty interfejsu w danych), profil v17 (filtr i ogłoszone filtry w wyrównaniu v16), Godot – shader na teksturze ekranu
z ustawieniami (siła, filtr na telefonie, ograniczony ruch), GBA – efekt własny palet (`screen_filter.h`), scenariusz 76.
Tryby dla daltonistów sprawdzone symulacją wady na zrzutach (pole ciosu przy protanopii dE 9 -> 41). Dalej w tym samym
wydaniu: obsługa języków PL/EN (#40) – osobno.

### Gdzie skończyliśmy (2026-10-03, cz. d)

v0.21.52 cz. d (#47 mapa kariery) w GBA i Godocie: 4 nowe kontrakty – Domek letniskowy (6 etapów, drewno, boss Zawilgocony
strych; 1 wygrana), Bliźniak (10, wspólna ściana – pogoda, wydarzenie i do 3 problemów z 1. połowy na 2.; boss Pęknięta
dylatacja; 3 wygrane), Dom z poddaszem (12, porywy co 4 tury; boss Zerwana połać; inspektor 8), Kamienica (10, Grzyb domowy,
Stara instalacja; boss Pęknięty strop; inspektor 12); nagroda za pierwszą wygraną (Respekt, tytuł, kask), wątki SMS, kolekcja
Bossowie kariery; profil v16, zapis budowy PBRUN16. Balans (bot, Normalny bez meta / pełne meta): Dom 33/68%, Domek 35/75%,
Bliźniak 28/68%, Poddasze 27/67%, Kamienica 26/70%. Węzeł drzewka Twardziel → Hartowany. v0.21.51 nie wychodzi osobno –
cały jego changelog jest w sekcji v0.21.52. Dalej: wydanie v0.21.52 (GitHub Release + Drive, karta SD, iPhone).

## Następne wydania

| # | Pomysł | GODOT (MOBILE) | GBA |
|---|---|---|---|
| 41 | v0.21.52 – Szkolenia z 4–5 poziomami (mniejsze przyrosty, rosnąca cena), pełne odblokowanie ~20–30 budów; zwrot dośw. za stare poziomy | ✅ (cz. a: 4 poziomy, 25–200, bot: 23 budowy, profil v13 ze zwrotem) | ✅ |
| 42 | v0.21.52 – zawody, narzędzia, trudność drożeją z każdym zakupem (np. 40/80/120/160) | ✅ (zawody 60/100/150, narzędzia 60–150, Trudny 100) | ✅ |
| 43 | v0.21.52 – odznaki i zlecenia: mniej dośw., więcej unikalnych nagród (tytuły, pamiątki, kosmetyka) | ✅ (5–15 dośw., 15 tytułów, 5 kolorów kasku) | ✅ |
| 44 | v0.21.52 – poziom inspektora (konto gracza, pasek dośw., nagroda co poziom: SMS, dekoracja Osiedla, kolor kasku, slot pamiątki) | ✅ (cz. b: 35 poziomów, bot: maks. po ~88 budowach, profil v14) | ✅ |
| 45 | v0.21.52 – mistrzostwo zawodu 1–10 (alternatywna moc, wariant broni, unikalna premia, złoty kask zawodu) | ✅ (cz. b: wariant mocy 3, broń mistrza 5, premia 7, kask 10; bot: 10 po ~22 budowach zawodem) | ✅ |
| 46 | v0.21.52 – drzewko Szkoleń: gałęzie Fach / BHP / Logistyka z wyborem węzłów | ✅ (cz. c: pień = Szkolenia, 6 węzłów 1 z 2, zmiana za 20 dośw.; strona Drzewko w Kosztach) | ✅ (Koszty > SELECT: Drzewko) |
| 47 | v0.21.52 – mapa kariery: kolejne zlecenia (domek letniskowy, bliźniak, dom z poddaszem, kamienica) z innymi etapami/wrogami/bossami | ✅ (cz. d: CareerScreen, 4 kontrakty z etapami, wyglądem i bossem, bliźniak – wspólna ściana, poddasze – porywy co 4 tury; profil v16) | ✅ (tytuł > A: Mapa kariery; scenariusze 72–75) |
| 48 | v0.21.52 – stopnie inwestora z nagrodą za każdy nowy poziom (kosmetyka, tytuły) | ✅ (cz. b: stawki 1–10: Respekt, tytuły, kaski) | ✅ |
| 49 | v0.21.52 – kolekcje: liczniki katalogu, karty bossów, album Osiedla; komplet = drobna premia | ✅ (cz. c: 6 kompletów – akty x10, karty bossów, album; premia / tytuł / kask) | ✅ (Katalog > A: Kolekcje, Bossowie, Album) |
| 50 | v0.21.52 – zadania dnia i tygodnia (np. 3 elity, 2× brygada) za Respekt/kosmetykę | ✅ (cz. c: 3 + 2 z seeda daty systemu, Respekt, nagrody za 5 / 15 / 40 wykonanych) | ✅ (z daty z codziennej budowy) |
| 51 | v0.21.52 – seria dni codziennej budowy (3 dni pamiątka, 7 dni kolor kasku) | ✅ (cz. c: 3 – Kalendarz majstra, 7 – kask, 14 – tytuł) | ✅ (tylko kolejny dzień wpisanej daty) |
| 52 | v0.21.52 – koniec budowy z paskami postępu (Szkolenie, mistrzostwo, inspektor) | ✅ (karta POSTĘP: trzy paski, „Poziom N!”) | ✅ (strona 4/4 Postęp) |
| 53 | v0.21.53 – filtry ekranu do odblokowania: Noir (inspektor 5), Retro LCD (kolekcja Stan surowy), Neon nocy (Kamienica albo sekret Szybka ekipa), Kwas (inspektor 20 albo sekret Mokra robota), „???” z podpowiedzią, baner | ✅ (shader: ziarno, winieta, dithering, kratka, linie, przesunięcie kanałów, fale; Ustawienia > Filtr ekranu z siłą, filtrem na telefonie i ograniczonym ruchem; Wygląd na wyborze zawodu) | ✅ (efekt palet Butano – same kolory, bez efektów zależnych od piksela; Kwas: obrót barwy; Wygląd > Ekran; profil v17) |
| 54 | v0.21.53 – tryby dla daltonistów: Protanopia, Deuteranopia, Tritanopia (daltonizacja), Wysoki kontrast – zawsze dostępne; wzory zamiast samego koloru | ✅ (paski na polach ciosu i wybuchu, litery rzadkości Z/R/L; sprawdzone symulacją wady na zrzutach) | ✅ (ta sama macierz daltonizacji; pola ciosu zawsze z wzorem, rzadkość słowem) |
| 40 | Obsługa wielu języków (PL/EN): wszystkie teksty w `game.json` jako słowniki `pl`/`en` (fabuła, opisy, samouczek, Jak grać), wybór języka w opcjach (Godot: klucz / domyślnie z języka systemu; GBA: opcja w telefonie profilu, zapis w profilu), font z pełnym zestawem znaków (GBA: kontrola `gen_data.py --check` dla obu języków; teksty EN krótsze/dłuższe – dopasowanie `fit()`), nazwy wrogów i przedmiotów po angielsku, sklepy (App Store / Play) z opisem EN | ✅ (v0.21.53 cz. 2: `Loc` + `LangOverlay`, Ustawienia > Język / Language, domyślnie język systemu; `en.json`: 1356 tekstów danych, 661 interfejsu) | ✅ (wybór przy 1. uruchomieniu, Zespół > SELECT, profil bajt 378; 799 tekstów interfejsu, `gen_data.py --check`: kompletność, font, szerokość px, napisy na sztywno) |

## Pomysły: gra + aplikacja PlanBudowlany i głębia grafiki (2026-10-03, do decyzji)

Zasada nadrzędna: **nic z poniższego nie daje przewagi w walce** – tylko kosmetyka, wygoda, fabuła i statystyki.
Balans (bot: Normalny 25–35% itd.) zostaje nietknięty, a gracze bez konta PB nie tracą nic z rozgrywki.

### A. Konto PlanBudowlany w grze (Godot / mobile)
| # | Pomysł | Uwagi |
|---|---|---|
| 55 | Opcjonalne logowanie kontem PlanBudowlany (OAuth / login jak w aplikacji mobilnej, endpointy z dev-api `/scalar`) | tylko Godot; gra działa w pełni bez logowania; token w pęku kluczy telefonu |
| 56 | Kosmetyka dla posiadaczy konta: „Kask PlanBudowlany” (fioletowo-pomarańczowy), tytuł „Inwestor” | po samym zalogowaniu |
| 57 | Kosmetyka dla płatnej subskrypcji: złoty kask PB, filtr ekranu „Plan” (fioletowa siatka techniczna), ozdoba Osiedla „Biuro PB”, tytuł „Inwestor Premium” | sprawdzane przez API subskrypcji; tylko wygląd |
| 58 | Synchronizacja profilu gry przez konto PB (zamiast/obok iCloud i Google Play Games) | zapis profilu jako blob przy koncie; konflikt = nowszy postęp |
| 59 | Most fabularny: po wygranej „Twój dom z gry” jako przykładowy harmonogram do otwarcia w aplikacji PB (link głęboki do szablonu) | marketing bez nachalności |
| 60 | Odwrotnie: w aplikacji PB małe „osiągnięcie” za ukończenie etapu budowy → odblokowuje w grze ozdobę Osiedla (np. „Prawdziwy fundament”) | tylko kosmetyka; wymaga zmian w API i aplikacji (sesja PB) |
| 61 | Statystyki „Budowa w grze vs prawdziwa” na profilu (dni, koszty) – zabawne porównanie | tylko wyświetlanie |

### B. Głębia grafiki w Godot (GBA bez zmian)
| # | Pomysł | Uwagi do balansu |
|---|---|---|
| 62 | ✅ v0.21.54 Wyższe ściany: lico ściany na 1,5–2 pola wysokości, zasłanianie postaci za ścianą półprzezroczystością | logika siatki i pola widzenia bez zmian – tylko rysowanie; mur 1,5 pola w obu widokach, prześwituje przed widoczną podłogą i mocniej przed postaciami / polami ciosu |
| 63 | ✅ prototyp v0.21.54 Widok 3/4 (lekko z góry, jak w Hades / Into the Breach): przesunięte w pionie wiersze, cienie rzucane, warstwy (podłoga, przedmioty, postaci, wierzch ścian) | sterowanie i stuknięcia nadal po siatce; test „małpy” w obu widokach; Ustawienia > Widok mapy (domyślnie Płaski), wszystkie akty i kontrakty; dalej: osobne kafle 3/4 rysowane ręcznie zamiast ściśniętych |
| 64 | Opcja izometryczna (romby) jako tryb eksperymentalny w ustawieniach | duży koszt grafiki – najpierw prototyp jednego aktu |
| 65 | ✅ v0.21.54 Światło dynamiczne: latarka czołowa, kałuże odbijające światło, iskry przy kombinacjach | pole widzenia liczone jak dziś; światło tylko wizualnie; Ustawienia > Efekty świetlne |
| 66 | ✅ v0.21.54 Paralaksa tła poza mapą (rusztowania, dźwig, niebo zależne od pogody) | tylko tło; widać przez ciemność nieznanych pól |

Kolejność proponowana: #62 → #63 (prototyp na jednym akcie, porównanie zrzutów) → #65; #55–#57 po uzgodnieniu API z sesją PB.
Stan v0.21.54: #62, #65, #66 zrobione, #63 jako prototyp w ustawieniach (wszystkie akty), #64 (izometria) – otwarte.

## v0.22.0 (otwarta wersja)

| # | Zadanie | GODOT (MOBILE) | GBA |
|---|---|---|---|
| 67 | Schody na mapie: zamiast żółtej skrzynki w ramce – czytelne schody w dół (stopnie z głębią, ciemny otwór, delikatna poświata / strzałka „Wyjście”), dopasowane do wyższych ścian i widoku 3/4, zamknięte (Akt 0) z kłódką | 🔄 | ⬜ (sprawdzić czytelność) |
| 68 | Dziennik nad mapą: w pionie teksty dziennika giną pod ścianami (półprzezroczyste, słabo czytelne) – pasek tła / cień pod tekstem | 🔄 | — |
| 69 | HUD w pionie: ucięte „Normal..” przy długiej nazwie etapu – skrót trudności (N/Ł/T) albo ikona | 🔄 | — |
| 70 | Drzewko Szkoleń – przebudowa widoku: zamiast 3 ciasnych kolumn z czerwonym „od pnia N” – lista gałęzi (karta na gałąź) z paskiem pnia, węzły jako karty ze stanem (zablokowany / do wyboru 1 z 2 / wybrany), ceną i opisem; jasne „wybierz jedno z dwóch” | ⬜ | ⬜ (GBA: ten sam porządek na stronach) |
| 71 | Harmonogram domu po wygranej: uporządkować (dubel „Zaplanuj swoją budowę” w karcie i przycisku, pusta przestrzeń, krótsze wiersze) | ⬜ | ⬜ |
| 72 | Po wygranej zamiast „Kolejna budowa (NG+)” – ekran „Co dalej”: od razu zakupy Szkoleń/Respektu (co Cię stać), najbliższe odblokowania, nowy kontrakt kariery, wyższa stawka inwestora, zadania dnia; NG+ jako opcja dodatkowa | ⬜ | ⬜ |
| 73 | Trudność wg gracza: wygrana „bez żadnych problemów” – przegląd balansu Normalnego (czy bot ≈ człowiek), podpowiedź „Spróbuj Trudnego / trybu inwestora” po wygranej, ewentualnie twardszy Normalny | ⬜ | ⬜ |

> Stan #67–#69 (2026-10-04, praca wstrzymana na prośbę): WIP w commicie – wersja v0.22.0 w danych, początek nowych kafli schodów w eksporcie i zmiany w HUD/dzienniku Godota; nie sprawdzone zrzutami ani testami, do dokończenia.

## Zgodność funkcji

| Funkcja | GODOT (MOBILE) | GBA |
|---|---|---|
| Logika gry (etapy, akty, bossowie, moce, sprzęt, szczęście, termos, stany, Hurtownia) | ✅ 1:1 (test złoty) | ✅ |
| Profil: Szkolenia, odznaki z uprawnieniami, zlecenia, pamiątki, katalog, Osiedle | ✅ | ✅ |
| Wydarzenia na placu, rady kierownika | ✅ | ✅ |
| Telefon PlanBudowlany (zakładki, powiadomienia push) | ✅ | ✅ |
| Wybór zawodu z paskiem portretów, zablokowane na końcu | ✅ | ✅ |
| Celowanie (przytrzymanie A) i karta wroga (przytrzymanie B) | ✅ klawiatura i dotyk | ✅ |
| Prolog przy pierwszej budowie | ✅ | ✅ |
| Grafika | ✅ bogatsza (kafle 32 px, 4 klatki chodu, światło; v0.21.54: wysokie ściany, widok 3/4, latarka, paralaksa tła) | ✅ |
| Dźwięk i muzyka | ✅ | ✅ |

## Porządki

- [ ] Stare tagi `v0.2137`–`v0.2140` (błędna numeracja) – usunąć tylko po decyzji właściciela
- [ ] PR #1 `godot-migration` → `master` – merge robi właściciel
- [ ] Archiwum Unity 2017 (`Assets/`, `ProjectSettings/`…) – zostawić czy przenieść do osobnego folderu/brancha

Historia zmian: [`GBA/CHANGELOG.md`](GBA/CHANGELOG.md), postęp Godota: [`GODOT/docs/KONCEPCJA.md`](GODOT/docs/KONCEPCJA.md) (sekcja 8).
