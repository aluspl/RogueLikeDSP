# TODO – PlanBudowlany RogueLike

Stan na 2026-09-29: GBA v0.21.49 wydane, **v0.21.50 gotowe do wydania** (cz. 1: rozpiska obrażeń broni #26; cz. 2: premie po etapie #27, elity #28,
kombinacje stanów #29; cz. 3: wydarzenia z wyborem #30, ulepszanie narzędzia #31, ukryte pomieszczenia #32; cz. 4: podsumowanie
budowy #33, wyzwania tygodnia #34, fabuła odkrywana z budowami #35 – GBA i Godot; balans: Łatwy 53%, Normalny 32%, Trudny 10%,
Szkolenia 54%, + Respekt 70%, z Aktem 0 66%, wszystkie modyfikatory 9%; wyzwania tygodnia 11–35%),
wcześniej v0.21.49 (Respekt, nagrody za odbiór, nowe zawody, balans
Szkoleń; cz. 2: 10 etapów, wrogowie etapów z zachowaniami, mechaniki aktów, opis statystyk; cz. 3: Akt 0 „Papierologia”,
samouczek menu), Godot – logika zgodna z v0.21.49, oprawa z GBA, wersja mobilna na iPhonie. Opis projektów: [`README.md`](README.md).

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
| Grafika | ✅ bogatsza (kafle 32 px, 4 klatki chodu, światło) | ✅ |
| Dźwięk i muzyka | ✅ | ✅ |

## Porządki

- [ ] Stare tagi `v0.2137`–`v0.2140` (błędna numeracja) – usunąć tylko po decyzji właściciela
- [ ] PR #1 `godot-migration` → `master` – merge robi właściciel
- [ ] Archiwum Unity 2017 (`Assets/`, `ProjectSettings/`…) – zostawić czy przenieść do osobnego folderu/brancha

Historia zmian: [`GBA/CHANGELOG.md`](GBA/CHANGELOG.md), postęp Godota: [`GODOT/docs/KONCEPCJA.md`](GODOT/docs/KONCEPCJA.md) (sekcja 8).
