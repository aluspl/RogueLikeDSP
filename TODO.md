# TODO – PlanBudowlany RogueLike

Stan na 2026-09-25: GBA v0.21.45 (wydane), Godot – logika zgodna z v0.21.45, oprawa z GBA, refaktor na ekrany,
wersja mobilna w toku. Opis projektów: [`README.md`](README.md).

Legenda: ✅ zrobione · 🔄 w toku · ⬜ do zrobienia · — nie dotyczy

## W toku i następne

| Zadanie | GODOT (MOBILE) | GBA |
|---|---|---|
| Pion na telefonie, marginesy pod wyspę i pasek Home, skalowanie bez czarnych pasów | 🔄 | — |
| Sterowanie jedną ręką: przesuwanie palcem, stuknięcie w pole/wroga, pasek ikon akcji (Atak, Moc, Termos, Czekaj, Telefon) | 🔄 | — |
| Opcje pod ikoną klucza (głośność, wibracje, joystick, pasek pod lewą/prawą rękę, wielkość tekstu, Zapisz i wyjdź) | 🔄 | ⬜ (opcje dźwięku w menu) |
| Przycisk planbudowlany.online na tytule | 🔄 | ✅ (QR na ekranie końcowym) |
| Instalacja na urządzeniu jedną komendą (`GODOT/tools/ios_deploy.sh`) | 🔄 | — |
| Test ręczny na sprzęcie i poprawki po graniu | ⬜ iPhone | ⬜ Miyoo, dźwięk |
| Android: eksport i instalacja (podpis bez sekretów w repo) | ⬜ | — |
| Wydania do pobrania w Releases | ⬜ .ipa / .apk / desktop | ✅ ROM co wersję |
| Oprawa 2.5D: kamera 3/4, dynamiczne światło (latarka czołowa), pogoda na etapie Dach | ⬜ | — |
| Mapy tematyczne etapów (szalunki, otwory okienne, krokwie, bruzdy) i elementy otoczenia (rusztowania, betoniarka, palety) | ⬜ | ⬜ (prostsza wersja) |
| Zadania etapu jako cele poboczne (odbiór częściowy, dodatkowe doświadczenie) | ⬜ | ⬜ |
| Drzewka ulepszeń mocy zawodów w zakładce Koszty | ⬜ | ⬜ |
| Muzyka: pętle bez przerwy | ⬜ (MP3 → OGG) | ✅ |
| Portret Kierownika na wyborze zawodu lekko przesunięty | — | ⬜ |
| Czytelniejszy komunikat „Brak celu w zasięgu 1” (`core.h` + port C#) | ⬜ | ⬜ |
| Balans po nowych funkcjach (bot: Normalny ~54%, pełne Szkolenia ~69%) | wspólny rdzeń | ⬜ |

## Nowe pomysły (2026-09-25, „zrób wszystko”)

Kolejność: A – rdzeń i GBA (po bossie Inspekcja Pracy), B – Godot i mobile (po wersji mobilnej), C – 2.5D.

| # | Pomysł | Etap | GODOT (MOBILE) | GBA |
|---|---|---|---|---|
| 0 | Boss Inspekcja Pracy (podkładka z pieczątką, akt III, Kontrola BHP) | – | 🔄 rdzeń | 🔄 |
| 1 | Codzienna budowa: seed dnia, tabela wyników | A/B | ⬜ (data z systemu) | ⬜ (seed z daty wpisanej ręcznie – GBA nie ma zegara) |
| 2 | Brygada: najemny fachowiec raz na etap (Geodeta – mapa, Pompa do betonu – obszar…) | A | ⬜ | ⬜ |
| 3 | Wybór ścieżki między etapami (harmonogram z rozgałęzieniami) | A | ⬜ | ⬜ |
| 4 | Materiały (cement, stal, drewno) – Hurtownia i naprawy pól | A | ⬜ | ⬜ |
| 5 | Pogoda dnia (upał, mróz, wiatr) jako modyfikator etapu | A | ⬜ | ⬜ |
| 6 | Tryb inwestora: modyfikatory trudności za dodatkowe doświadczenie (jak Heat w Hadesie) | A | ⬜ | ⬜ |
| 7 | Powiadomienia systemowe („Nowa codzienna budowa”) | B | ⬜ | — |
| 8 | Widżet / Live Activity z postępem budowy | B (natywne rozszerzenie iOS) | ⬜ | — |
| 9 | Udostępnianie wyniku (obrazek + link planbudowlany.online) | B | ⬜ | ⬜ (kod QR z wynikiem) |
| 10 | Game Center / Google Play Games: osiągnięcia z odznak, tabela wyników | B (konfiguracja w App Store Connect) | ⬜ | — |
| 11 | 2.5D: kamera 3/4, latarka czołowa, cienie ścian | C | ⬜ | — |
| 12 | Ulewa jako pogoda na etapie Dach (deszcz, kałuże, poślizg) | C | ⬜ | ⬜ (prostszy efekt) |
| 13 | Po wygranej: harmonogram domu w stylu aplikacji + „Zaplanuj swoją budowę” | A/B | ⬜ | ⬜ |

## Zgodność funkcji

| Funkcja | GODOT (MOBILE) | GBA |
|---|---|---|
| Logika gry (etapy, akty, bossowie, moce, sprzęt, szczęście, termos, stany, Hurtownia) | ✅ 1:1 (test złoty) | ✅ |
| Profil: Szkolenia, odznaki z uprawnieniami, zlecenia, pamiątki, katalog, Osiedle | ✅ | ✅ |
| Wydarzenia na placu, rady kierownika | ✅ | ✅ |
| Telefon PlanBudowlany (zakładki, powiadomienia push) | ✅ | ✅ |
| Wybór zawodu z paskiem portretów, zablokowane na końcu | ✅ | ✅ |
| Celowanie (przytrzymanie A) i karta wroga (przytrzymanie B) | ✅ klawiatura · 🔄 dotyk | ✅ |
| Prolog przy pierwszej budowie | ✅ | ✅ |
| Grafika | ✅ bogatsza (kafle 32 px, 4 klatki chodu, światło) | ✅ |
| Dźwięk i muzyka | ✅ | ✅ |

## Porządki

- [ ] Stare tagi `v0.2137`–`v0.2140` (błędna numeracja) – usunąć tylko po decyzji właściciela
- [ ] PR #1 `godot-migration` → `master` – merge robi właściciel
- [ ] Archiwum Unity 2017 (`Assets/`, `ProjectSettings/`…) – zostawić czy przenieść do osobnego folderu/brancha

Historia zmian: [`GBA/CHANGELOG.md`](GBA/CHANGELOG.md), postęp Godota: [`GODOT/docs/KONCEPCJA.md`](GODOT/docs/KONCEPCJA.md) (sekcja 8).
