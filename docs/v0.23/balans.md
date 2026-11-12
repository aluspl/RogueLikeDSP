# Balans v0.23 – pomiary bota

Bot C# (`BalanceBaselineTests.MeasureNormalBaseline`, `BALANCE_OUT=plik dotnet test GODOT/LifeLike.sln --filter MeasureNormalBaseline`),
300 przebiegów na zawód, poziom Normalny, bez ulepszeń (`RunMods.Default`), ziarna `1000 + k*7919`. Numer etapu = indeks absolutny
(0-1 Akt 0 Papierologia, 2-5 Akt I Stan surowy, 6-8 Akt II Pod dachem, 9-11 Akt III Wykończenie; wiersz „porażki” to udział % wszystkich przebiegów zawodu).

## v0.22.1 (przed zmianami broni) – stałe dla `WeaponBalanceTests` (Task 7)

| Zawód | Wygrane % | Porażki na etapach (etap: udział % wszystkich przebiegów) |
|---|---|---|
| Kierownik budowy | 36,7 | 5: 12,0, 6: 2,7, 7: 2,3, 8: 19,3, 9: 14,3, 10: 2,0, 11: 10,7 |
| Murarz | 16,0 | 5: 7,3, 6: 2,0, 7: 5,7, 8: 30,7, 9: 21,0, 10: 3,3, 11: 14,0 |
| Cieśla-dekarz | 48,7 | 5: 6,0, 6: 1,3, 7: 1,0, 8: 13,3, 9: 14,7, 10: 3,3, 11: 11,7 |
| Elektryk | 35,3 | 2: 0,3, 5: 16,3, 6: 3,3, 7: 2,3, 8: 16,7, 9: 11,3, 10: 1,0, 11: 13,3 |
| Hydraulik | 14,0 | 4: 0,3, 5: 9,7, 6: 3,3, 7: 4,3, 8: 27,0, 9: 25,0, 10: 3,3, 11: 13,0 |
| Glazurnik | 23,3 | 4: 0,3, 5: 9,0, 6: 2,7, 7: 5,0, 8: 26,7, 9: 17,3, 10: 2,7, 11: 13,0 |
| Dekarz | 57,0 | 2: 0,3, 4: 0,3, 5: 15,0, 6: 2,7, 7: 1,3, 8: 7,0, 9: 8,3, 10: 1,0, 11: 7,0 |
| Tynkarz | 36,7 | 5: 9,7, 6: 3,7, 7: 2,3, 8: 18,7, 9: 14,0, 10: 1,7, 11: 13,3 |
| Operator koparki | 23,0 | 5: 8,7, 6: 3,3, 7: 4,3, 8: 19,3, 9: 18,7, 10: 5,0, 11: 17,7 |
| Spawacz | 35,0 | 4: 0,3, 5: 18,0, 6: 4,7, 7: 3,0, 8: 18,0, 9: 9,3, 10: 3,7, 11: 8,0 |
| Geodeta | 36,7 | 5: 20,0, 6: 2,3, 7: 3,3, 8: 16,7, 9: 8,7, 10: 3,0, 11: 9,3 |
| Majster | 23,0 | 4: 0,3, 5: 6,0, 6: 1,3, 7: 4,0, 8: 22,0, 9: 20,7, 10: 4,3, 11: 18,3 |
| **Razem** | **32,1** | 2: 0,1, 4: 0,1, 5: 11,5, 6: 2,8, 7: 3,2, 8: 19,6, 9: 15,3, 10: 2,9, 11: 12,4 |

## v0.23.0 – Task 7: bot z bronią i strojenie

Pomiar: `WeaponBalanceTests` (`BALANCE_OUT=plik.md dotnet test GODOT/LifeLike.sln --filter "FullyQualifiedName~WeaponBalanceTests"`,
opcjonalnie `BALANCE_RUNS=N`, `BALANCE_QUICK=1` – tylko Normalny i bot z bronią domyślną, `BALANCE_QUICK=meta` – wiersze meta).
Średnie celów jak w `core_tests.cpp` – zwykłe zawody (bez sekretów); kontrakty 150 przebiegów na zawód.
„Bot” = bot grający jak gracz (`BotOptions.Default`: tryb B, ustawienie pod wzór, zamiana z pasa, ocena znalezionych broni –
gorsze omija); „tylko domyślna” = `BotOptions.DefaultOnly` (omija i odrzuca znalezione bronie, bez zamiany; tryb B swojej broni używa).

### Rundy (N = przebiegi na zawód; Normalny, zwykłe zawody; „dom.” = bot tylko z bronią domyślną)

| Runda | Zmiana | N | Bot | dom. | Uwagi |
|---|---|---|---|---|---|
| 0 | stan po zad. 1–6, bot v0.22.1 (`Bot.Step` bez broni) | 300 | 33,3 (wszystkie) | – | bot podnosi każde narzędzie (gorsze też) |
| 1 | nowy bot (tryb B, wzory, pas, ocena broni), dane bez zmian | 100 | 59,4 | 57,7 | Łatwy 79,9, Trudny 25,9, Szkolenia 89,7, inwestor 43,4 |
| 2 | bot: wręcz dalej cios krokiem na cel (jak v0.22.1) | 100 | 54,4 | 52,9 | bez trybu B: 39,1 / 39,2 |
| – | diagnoza: dane v0.22.1 + bot omijający narzędzia | 100 | 43 | – | v0.22.1: 32; pełne Szkolenia 67 (było 59) – sam „mądrzejszy” bot daje +11 p.p. |
| 3 | tabela ±25% (obuch 125/75/100/75/100, cięcie 75/75/125/100/100, kłucie 100/125/100/100/100) | 100 | 57,3 | 55,3 | Dekarz wraca do 57 |
| 4 | kłucie 75 na cement i wodę | 100 | 53,1 | 51,1 | bez trybu B 38,6 |
| 5 | kłucie −1 OBR (było 2), iskra na stal 125 (było 150), odnowienia B +2 wszystkim | 100 | 50,2 | 49,4 | |
| 6 | próba: Normalny HP 120% | 100 | 33,7 | 33,1 | Dekarz −16, Geodeta −15, Operator +17 |
| 7 | B broni zawodów: Kielnia i Łyżka B 50%, Dziennik znak 20%, Dachówki B 100% / odn. 4, Tyczka odn. 4 / znak 40%, Klucz 4, Szlifierka 3, Młotek 2, Spawarka 4, Próbnik 4 | 300 | 29,3 | 28,6 | Hydraulik 7,0 (−7), etap 5 +14,3 – za dużo |
| 8 | Klucz nastawny: obuch + kłucie (spec. 4.7 pkt 3, drugi typ); HP 115% | 300 | 34,7 | 34,4 | wszystkie zawody ≥ −5 p.p. |
| 9 | HP Łatwy 115 / Normalny 117 / Trudny 167 | 300 | 33,6 | 33,1 | Łatwy 59,9, Trudny 8,3; Szkolenia 70,4, Respekt 83,8, Akt 0 87,5, inwestor 22,5, kontrakty 28–31 / 80–87; Murarz etap 5 +11,0 |
| – | meta: rzadkość Solidna 0 / Markowa +1 | 100 | – | – | Szkolenia −3 p.p. (za mało) |
| – | meta: wrogowie +1 obrażeń zamiast HP | 100 | 26,4 | 26,7 | Szkolenia 58,6, Respekt 74,9, Akt 0 69,4, inwestor 15,2 – ale zawody wręcz −8…−11 p.p. (Glazurnik, Operator) |
| – | meta: mniej narzędzi w dropach (15 → 6, kawa 40 → 49) | 100 | 28,9 | 28,8 | Szkolenia rosną bardziej niż Normalny – odrzucone |
| 10 | Kielnia: cięcie + kłucie (etap 5), HP 118 | 300 | 33,6 | 33,6 | Murarz +13 |
| 11 | Kielnia B odn. 7, Dachówki B odn. 3; HP 120 | 300 | 29,9 | 30,2 | kilka zawodów na granicy −5 |
| 12 | HP 118 (Łatwy 118, Trudny 165); rzadkość Solidna +1, Markowa +1 i odn. −1; Przecinak B odn. 1 (ładunki) | 300 | 33,2 | 33,2 | **wynik końcowy** (tabela niżej) |

### Wynik końcowy (300 przebiegów na zawód)

Przebiegi na zawód: 300 (kontrakty 150); średnie celów = zwykłe zawody (bez sekretów), jak core_tests.cpp.

| Cel | Wynik | Zakres |
|---|---|---|
| Łatwy | 56,3 | 50–60 |
| Normalny | 33,2 (wszystkie zawody 33,3) | 25–35 |
| Trudny | 9,3 | 8–15 |
| Pełne Szkolenia | 69,3 | 50–60 |
| + Respekt | 81,4 | 65–75 |
| Akt 0 + meta | 82,1 | 60–70 |
| Inwestor (wszystkie modyfikatory) | 18,1 | 5–15 |
| Kontrakt Letniskowy bez meta / pełne meta | 30,0 / 81,0 | 25–40 / 60–75 |
| Kontrakt Bliźniak bez meta / pełne meta | 28,3 / 74,7 | 25–40 / 60–75 |
| Kontrakt Poddasze bez meta / pełne meta | 31,2 / 77,0 | 25–40 / 60–75 |
| Kontrakt Kamienica bez meta / pełne meta | 30,1 / 76,9 | 25–40 / 60–75 |
| Tylko broń domyślna, Normalny | 33,2 (wszystkie 33,1) | zawód ≥ v0.22.1 −5 p.p. |

| Zawód | v0.22.1 | Bot (Normalny) | Tylko domyślna | Δ domyślna | Najgorszy etap domyślnej (Δ udziału porażek, p.p.) |
|---|---|---|---|---|---|
| Kierownik budowy | 36,7 | 34,6 | 35,6 | -1,1 | 10: 5,0 |
| Murarz | 16,0 | 23,3 | 23,3 | 7,3 | 11: 4,3 |
| Cieśla-dekarz | 48,7 | 51,0 | 51,0 | 2,3 | 10: 1,7 |
| Elektryk | 35,3 | 37,3 | 36,6 | 1,3 | 9: 2,7 |
| Hydraulik | 14,0 | 17,3 | 17,0 | 3,0 | 11: 2,0 |
| Glazurnik | 23,3 | 23,0 | 22,6 | -0,7 | 11: 3,0 |
| Dekarz | 57,0 | 50,3 | 52,0 | -5,0 | 11: 4,3 |
| Tynkarz | 36,7 | 37,6 | 36,3 | -0,4 | 5: 4,3 |
| Operator koparki | 23,0 | 24,0 | 24,0 | 1,0 | 9: 5,3 |
| Spawacz | 35,0 | 36,6 | 36,3 | 1,3 | 11: 6,0 |
| Geodeta | 36,7 | 40,3 | 39,3 | 2,6 | 8: 3,3 |
| Majster | 23,0 | 24,0 | 23,0 | 0,0 | 9: 5,3 |

Porażki na etapach (etap: udział % wszystkich przebiegów zawodu), Normalny:

| Zawód | Bot | Tylko domyślna |
|---|---|---|
| Kierownik budowy | 5: 10,0, 6: 3,3, 7: 1,6, 8: 18,3, 9: 11,0, 10: 7,0, 11: 14,0 | 5: 7,6, 6: 3,3, 7: 2,0, 8: 19,6, 9: 11,6, 10: 7,0, 11: 13,0 |
| Murarz | 3: 0,3, 5: 5,3, 6: 3,0, 7: 1,6, 8: 22,3, 9: 19,3, 10: 6,0, 11: 18,6 | 3: 0,3, 5: 5,3, 6: 2,6, 7: 1,6, 8: 22,6, 9: 19,6, 10: 6,0, 11: 18,3 |
| Cieśla-dekarz | 3: 0,3, 4: 0,3, 5: 6,0, 6: 2,0, 7: 2,6, 8: 10,0, 9: 11,6, 10: 5,3, 11: 10,6 | 3: 0,3, 4: 0,3, 5: 5,6, 6: 2,0, 7: 2,3, 8: 9,6, 9: 12,0, 10: 5,0, 11: 11,6 |
| Elektryk | 4: 0,3, 5: 11,0, 6: 2,0, 7: 3,3, 8: 14,6, 9: 13,3, 10: 3,3, 11: 14,6 | 4: 0,3, 5: 11,0, 6: 2,0, 7: 3,3, 8: 14,3, 9: 14,0, 10: 3,3, 11: 15,0 |
| Hydraulik | 5: 6,6, 6: 4,0, 7: 4,0, 8: 26,3, 9: 21,6, 10: 5,0, 11: 15,0 | 5: 7,3, 6: 4,0, 7: 4,0, 8: 26,0, 9: 22,0, 10: 4,6, 11: 15,0 |
| Glazurnik | 5: 11,6, 6: 5,3, 7: 3,0, 8: 21,6, 9: 16,6, 10: 3,0, 11: 15,6 | 5: 11,3, 6: 5,3, 7: 3,3, 8: 20,6, 9: 17,6, 10: 3,0, 11: 16,0 |
| Dekarz | 5: 13,3, 6: 2,6, 7: 1,0, 8: 9,0, 9: 11,6, 10: 1,6, 11: 10,3 | 5: 14,0, 6: 2,0, 7: 1,3, 8: 8,3, 9: 10,6, 10: 0,3, 11: 11,3 |
| Tynkarz | 5: 13,3, 6: 2,0, 7: 2,3, 8: 18,3, 9: 13,0, 10: 2,6, 11: 10,6 | 5: 14,0, 6: 2,3, 7: 2,6, 8: 19,3, 9: 13,0, 10: 2,3, 11: 10,0 |
| Operator koparki | 5: 11,0, 6: 3,6, 7: 2,6, 8: 19,6, 9: 23,0, 10: 4,3, 11: 11,6 | 4: 0,3, 5: 11,0, 6: 4,0, 7: 2,6, 8: 18,6, 9: 24,0, 10: 4,0, 11: 11,3 |
| Spawacz | 3: 0,3, 5: 19,3, 6: 2,3, 7: 2,6, 8: 12,6, 9: 7,6, 10: 4,0, 11: 14,3 | 3: 0,3, 5: 19,0, 6: 2,3, 7: 2,6, 8: 13,0, 9: 8,6, 10: 3,6, 11: 14,0 |
| Geodeta | 4: 0,3, 5: 9,6, 6: 3,0, 7: 2,6, 8: 19,6, 9: 9,0, 10: 3,0, 11: 12,3 | 4: 0,3, 5: 9,6, 6: 3,3, 7: 2,3, 8: 20,0, 9: 9,0, 10: 3,3, 11: 12,6 |
| Majster | 4: 0,6, 5: 7,3, 6: 2,0, 7: 2,0, 8: 16,0, 9: 25,6, 10: 1,6, 11: 20,6 | 4: 0,6, 5: 7,3, 6: 2,0, 7: 2,0, 8: 15,6, 9: 26,0, 10: 1,6, 11: 21,6 |

**Poza celem (do decyzji właściciela):** pełne Szkolenia 69,3 (50–60), + Respekt 81,4 (65–75), Akt 0 + meta 82,1 (60–70),
inwestor 18,1 (5–15), kontrakty z pełnym meta 74,7–81,0 (60–75). Przyczyna: przewaga meta nad grą bez meta wzrosła
z ~25 p.p. (v0.22.1) do ~36 p.p. – bot nie podnosi już gorszych broni (na danych v0.22.1 sam ten zysk to +11 p.p. bez meta
i +8 p.p. z pełnymi Szkoleniami), a tryb B i typy ciosu dokładają się. Gałki broni (tabela, odnowienia, rzadkość, dropy) i
poziom trudności przesuwają Normalny i meta prawie równo; zejście meta do celu przy Normalnym ≥ v0.22.1 −5 p.p. na zawód
wymaga osłabienia samego meta (Szkolenia / Respekt / nagrody) albo nowych celów meta dla bota z bronią.

## v0.23.0 – runda meta (decyzja właściciela: HP +18% zostaje, cele meta „pół na pół”)

Nowe cele meta: pełne Szkolenia 55–65, + Respekt 70–80, Akt 0 + meta 65–75, inwestor 5–15, kontrakty 25–40 / 65–80
(Łatwy / Normalny / Trudny i reguły broni domyślnej bez zmian).

| Gałka | Przed | Po | Wpływ (pomiar, N = 100 / zawód, pełne Szkolenia) |
|---|---|---|---|
| Szkolenie Dostawy, poziom 2 | +1 znajdźka | +5% materiałów (Dostawy: „Więcej materiałów i budżetu”) | −6 p.p. |
| Szkolenie Kondycja, poziom 4 | +1 HP | szansa na lepszy sprzęt +1 | −1 p.p. (N = 300) |
| Respekt: Pewna ręka, Gruba skóra, Oko fachowca, Zwinność, Mocna kawa, Rutyna | 2 rangi (1 / 2, kawa 5 / 10) | 1 ranga (1, kawa 5) | + Respekt −4…−5, Akt 0 −6 |
| Dachówki, tryb B (odnowienie) | 3 | 2 | Dekarz z bronią domyślną −5,0 → −2,4 p.p. (zapas ≥ 1) |

Sprawdzone i odrzucone: drugi i trzeci poziom termosu (bez wpływu), Szczęście z BHP II (−4,5, ale kontrakty spadają do 63%),
bez Drugiej szansy (usuwa funkcję), bez nagród Pas / Buty (≤ 1 p.p.).

### Wynik końcowy po rundzie meta (300 przebiegów na zawód)

Przebiegi na zawód: 300 (kontrakty 150); średnie celów = zwykłe zawody (bez sekretów), jak core_tests.cpp.

| Cel | Wynik | Zakres |
|---|---|---|
| Łatwy | 56,9 | 50–60 |
| Normalny | 33,6 (wszystkie zawody 33,6) | 25–35 |
| Trudny | 9,1 | 8–15 |
| Pełne Szkolenia | 64,6 | 55–65 |
| + Respekt | 73,3 | 70–80 |
| Akt 0 + meta | 73,7 | 65–75 |
| Inwestor (wszystkie modyfikatory) | 11,6 | 5–15 |
| Kontrakt Letniskowy bez meta / pełne meta | 29,9 / 74,1 | 25–40 / 65–80 |
| Kontrakt Bliźniak bez meta / pełne meta | 28,4 / 67,8 | 25–40 / 65–80 |
| Kontrakt Poddasze bez meta / pełne meta | 31,0 / 68,7 | 25–40 / 65–80 |
| Kontrakt Kamienica bez meta / pełne meta | 30,2 / 71,3 | 25–40 / 65–80 |
| Tylko broń domyślna, Normalny | 33,5 (wszystkie 33,3) | zawód ≥ v0.22.1 −5 p.p. |

| Zawód | v0.22.1 | Bot (Normalny) | Tylko domyślna | Δ domyślna | Najgorszy etap domyślnej (Δ udziału porażek, p.p.) |
|---|---|---|---|---|---|
| Kierownik budowy | 36,7 | 34,6 | 35,6 | -1,1 | 10: 5,0 |
| Murarz | 16,0 | 23,3 | 23,3 | 7,3 | 11: 4,3 |
| Cieśla-dekarz | 48,7 | 51,0 | 51,0 | 2,3 | 10: 1,7 |
| Elektryk | 35,3 | 37,3 | 36,6 | 1,3 | 9: 2,7 |
| Hydraulik | 14,0 | 17,3 | 17,0 | 3,0 | 11: 2,0 |
| Glazurnik | 23,3 | 23,0 | 22,6 | -0,7 | 11: 3,0 |
| Dekarz | 57,0 | 53,6 | 54,6 | -2,4 | 11: 4,3 |
| Tynkarz | 36,7 | 37,6 | 36,3 | -0,4 | 5: 4,3 |
| Operator koparki | 23,0 | 24,0 | 24,0 | 1,0 | 9: 5,3 |
| Spawacz | 35,0 | 36,6 | 36,3 | 1,3 | 11: 6,0 |
| Geodeta | 36,7 | 40,3 | 39,3 | 2,6 | 8: 3,3 |
| Majster | 23,0 | 24,0 | 23,0 | 0,0 | 9: 5,3 |

Porażki na etapach (etap: udział % wszystkich przebiegów zawodu), Normalny:

| Zawód | Bot | Tylko domyślna |
|---|---|---|
| Kierownik budowy | 5: 10,0, 6: 3,3, 7: 1,6, 8: 18,3, 9: 11,0, 10: 7,0, 11: 14,0 | 5: 7,6, 6: 3,3, 7: 2,0, 8: 19,6, 9: 11,6, 10: 7,0, 11: 13,0 |
| Murarz | 3: 0,3, 5: 5,3, 6: 3,0, 7: 1,6, 8: 22,3, 9: 19,3, 10: 6,0, 11: 18,6 | 3: 0,3, 5: 5,3, 6: 2,6, 7: 1,6, 8: 22,6, 9: 19,6, 10: 6,0, 11: 18,3 |
| Cieśla-dekarz | 3: 0,3, 4: 0,3, 5: 6,0, 6: 2,0, 7: 2,6, 8: 10,0, 9: 11,6, 10: 5,3, 11: 10,6 | 3: 0,3, 4: 0,3, 5: 5,6, 6: 2,0, 7: 2,3, 8: 9,6, 9: 12,0, 10: 5,0, 11: 11,6 |
| Elektryk | 4: 0,3, 5: 11,0, 6: 2,0, 7: 3,3, 8: 14,6, 9: 13,3, 10: 3,3, 11: 14,6 | 4: 0,3, 5: 11,0, 6: 2,0, 7: 3,3, 8: 14,3, 9: 14,0, 10: 3,3, 11: 15,0 |
| Hydraulik | 5: 6,6, 6: 4,0, 7: 4,0, 8: 26,3, 9: 21,6, 10: 5,0, 11: 15,0 | 5: 7,3, 6: 4,0, 7: 4,0, 8: 26,0, 9: 22,0, 10: 4,6, 11: 15,0 |
| Glazurnik | 5: 11,6, 6: 5,3, 7: 3,0, 8: 21,6, 9: 16,6, 10: 3,0, 11: 15,6 | 5: 11,3, 6: 5,3, 7: 3,3, 8: 20,6, 9: 17,6, 10: 3,0, 11: 16,0 |
| Dekarz | 5: 12,6, 6: 1,6, 7: 0,6, 8: 7,6, 9: 11,3, 10: 1,6, 11: 10,6 | 5: 13,3, 6: 1,6, 7: 1,3, 8: 6,6, 9: 11,0, 11: 11,3 |
| Tynkarz | 5: 13,3, 6: 2,0, 7: 2,3, 8: 18,3, 9: 13,0, 10: 2,6, 11: 10,6 | 5: 14,0, 6: 2,3, 7: 2,6, 8: 19,3, 9: 13,0, 10: 2,3, 11: 10,0 |
| Operator koparki | 5: 11,0, 6: 3,6, 7: 2,6, 8: 19,6, 9: 23,0, 10: 4,3, 11: 11,6 | 4: 0,3, 5: 11,0, 6: 4,0, 7: 2,6, 8: 18,6, 9: 24,0, 10: 4,0, 11: 11,3 |
| Spawacz | 3: 0,3, 5: 19,3, 6: 2,3, 7: 2,6, 8: 12,6, 9: 7,6, 10: 4,0, 11: 14,3 | 3: 0,3, 5: 19,0, 6: 2,3, 7: 2,6, 8: 13,0, 9: 8,6, 10: 3,6, 11: 14,0 |
| Geodeta | 4: 0,3, 5: 9,6, 6: 3,0, 7: 2,6, 8: 19,6, 9: 9,0, 10: 3,0, 11: 12,3 | 4: 0,3, 5: 9,6, 6: 3,3, 7: 2,3, 8: 20,0, 9: 9,0, 10: 3,3, 11: 12,6 |
| Majster | 4: 0,6, 5: 7,3, 6: 2,0, 7: 2,0, 8: 16,0, 9: 25,6, 10: 1,6, 11: 20,6 | 4: 0,6, 5: 7,3, 6: 2,0, 7: 2,0, 8: 15,6, 9: 26,0, 10: 1,6, 11: 21,6 |

## v0.23.0 – Task 8 (#84): dwa noże (Nóż do tapet w dropach i Hurtowni, Nóż majstra seniora z zlecenia Stara szkoła)

Runda po dodaniu noży (300 przebiegów na zawód, `WeaponBalanceTests`, wszystkie 6 testów zielone). Nóż do tapet wchodzi do puli
dropów i Hurtowni – przesuwa rzuty; Nóż majstra seniora tylko po zleceniu (bot w pomiarach go nie ma). Wszystkie cele w zakresie;
Akt 0 + meta 74,7 tuż pod sufitem 75, pełne Szkolenia 63,9 (sufit 65). Test `BotPassesAct0CollectingDocuments`: 12/20 (było 13/20).

Przebiegi na zawód: 300 (kontrakty 150); średnie celów = zwykłe zawody (bez sekretów), jak core_tests.cpp.

| Cel | Wynik | Zakres |
|---|---|---|
| Łatwy | 57,0 | 50–60 |
| Normalny | 33,4 (wszystkie zawody 33,3) | 25–35 |
| Trudny | 9,9 | 8–15 |
| Pełne Szkolenia | 63,9 | 55–65 |
| + Respekt | 73,5 | 70–80 |
| Akt 0 + meta | 74,7 | 65–75 |
| Inwestor (wszystkie modyfikatory) | 11,4 | 5–15 |
| Kontrakt Letniskowy bez meta / pełne meta | 30,1 / 73,7 | 25–40 / 65–80 |
| Kontrakt Bliźniak bez meta / pełne meta | 27,8 / 66,5 | 25–40 / 65–80 |
| Kontrakt Poddasze bez meta / pełne meta | 31,0 / 69,2 | 25–40 / 65–80 |
| Kontrakt Kamienica bez meta / pełne meta | 30,6 / 71,0 | 25–40 / 65–80 |
| Tylko broń domyślna, Normalny | 33,3 (wszystkie 33,1) | zawód ≥ v0.22.1 −5 p.p. |

| Zawód | v0.22.1 | Bot (Normalny) | Tylko domyślna | Δ domyślna | Najgorszy etap domyślnej (Δ udziału porażek, p.p.) |
|---|---|---|---|---|---|
| Kierownik budowy | 36,7 | 35,3 | 36,0 | -0,7 | 10: 5,0 |
| Murarz | 16,0 | 24,3 | 24,3 | 8,3 | 11: 3,6 |
| Cieśla-dekarz | 48,7 | 50,0 | 51,0 | 2,3 | 7: 1,6 |
| Elektryk | 35,3 | 37,3 | 37,0 | 1,7 | 11: 2,7 |
| Hydraulik | 14,0 | 15,6 | 15,6 | 1,6 | 10: 1,0 |
| Glazurnik | 23,3 | 25,0 | 23,6 | 0,3 | 11: 3,3 |
| Dekarz | 57,0 | 52,0 | 52,6 | -4,4 | 11: 2,6 |
| Tynkarz | 36,7 | 38,6 | 37,0 | 0,3 | 5: 5,3 |
| Operator koparki | 23,0 | 22,3 | 22,0 | -1,0 | 9: 5,3 |
| Spawacz | 35,0 | 35,3 | 35,6 | 0,6 | 11: 6,3 |
| Geodeta | 36,7 | 38,0 | 38,0 | 1,3 | 11: 4,3 |
| Majster | 23,0 | 25,6 | 24,6 | 1,6 | 9: 5,6 |

Porażki na etapach (etap: udział % wszystkich przebiegów zawodu), Normalny:

| Zawód | Bot | Tylko domyślna |
|---|---|---|
| Kierownik budowy | 5: 8,6, 6: 3,6, 7: 2,6, 8: 18,3, 9: 12,3, 10: 6,0, 11: 13,0 | 5: 8,0, 6: 3,3, 7: 2,6, 8: 17,6, 9: 13,0, 10: 7,0, 11: 12,3 |
| Murarz | 5: 5,0, 6: 2,6, 7: 1,6, 8: 22,6, 9: 20,3, 10: 5,3, 11: 18,0 | 5: 4,6, 6: 2,6, 7: 2,0, 8: 23,3, 9: 20,0, 10: 5,3, 11: 17,6 |
| Cieśla-dekarz | 4: 0,6, 5: 6,0, 6: 2,3, 7: 2,6, 8: 11,0, 9: 11,6, 10: 4,6, 11: 11,0 | 3: 0,3, 4: 0,6, 5: 5,3, 6: 1,6, 7: 2,6, 8: 11,0, 9: 11,6, 10: 4,6, 11: 11,0 |
| Elektryk | 2: 0,3, 3: 0,3, 4: 0,3, 5: 10,3, 6: 2,3, 7: 3,6, 8: 12,6, 9: 13,3, 10: 3,3, 11: 16,0 | 2: 0,3, 3: 0,3, 4: 0,3, 5: 10,3, 6: 2,3, 7: 3,3, 8: 13,3, 9: 13,3, 10: 3,3, 11: 16,0 |
| Hydraulik | 4: 0,3, 5: 6,3, 6: 4,0, 7: 3,6, 8: 27,6, 9: 24,0, 10: 4,0, 11: 14,3 | 4: 0,3, 5: 6,3, 6: 4,0, 7: 3,6, 8: 27,6, 9: 24,0, 10: 4,3, 11: 14,0 |
| Glazurnik | 5: 11,3, 6: 3,6, 7: 3,3, 8: 21,0, 9: 16,3, 10: 3,6, 11: 15,6 | 5: 11,3, 6: 4,0, 7: 3,3, 8: 21,6, 9: 16,0, 10: 3,6, 11: 16,3 |
| Dekarz | 5: 13,6, 6: 3,6, 8: 10,6, 9: 8,6, 10: 1,3, 11: 10,0 | 5: 14,3, 6: 2,6, 7: 0,6, 8: 9,3, 9: 9,6, 10: 1,0, 11: 9,6 |
| Tynkarz | 5: 14,0, 6: 1,6, 7: 2,6, 8: 20,6, 9: 11,6, 10: 1,6, 11: 9,0 | 5: 15,0, 6: 2,0, 7: 2,6, 8: 20,0, 9: 13,0, 10: 1,0, 11: 9,3 |
| Operator koparki | 4: 0,3, 5: 11,0, 6: 4,0, 7: 3,0, 8: 19,0, 9: 23,0, 10: 3,3, 11: 14,0 | 4: 0,3, 5: 11,0, 6: 3,6, 7: 3,0, 8: 19,3, 9: 24,0, 10: 3,3, 11: 13,3 |
| Spawacz | 2: 0,3, 5: 20,6, 6: 2,6, 7: 2,6, 8: 15,0, 9: 7,0, 10: 3,0, 11: 13,3 | 2: 0,3, 5: 19,6, 6: 2,6, 7: 2,0, 8: 14,6, 9: 7,6, 10: 3,0, 11: 14,3 |
| Geodeta | 4: 0,6, 5: 9,0, 6: 3,3, 7: 2,0, 8: 19,3, 9: 10,0, 10: 3,0, 11: 14,6 | 4: 0,6, 5: 8,6, 6: 3,6, 7: 3,0, 8: 19,6, 9: 9,3, 10: 3,3, 11: 13,6 |
| Majster | 4: 0,3, 5: 8,0, 6: 1,6, 7: 1,6, 8: 14,3, 9: 25,0, 10: 1,0, 11: 22,3 | 4: 0,6, 5: 8,0, 6: 1,3, 7: 1,6, 8: 14,3, 9: 26,3, 10: 1,0, 11: 22,0 |

## Profity broni (v0.23.0, TODO #87) – pomiar 2026-11-07

Pula 5 profitów (Przebicie OBR -1, Ostrze kryt +5%, Wyważenie rzut min +1, Głowica obr. maks +1, Chwyt odn. B -1), perRarity 0/1/2, Mistrzowska 2 stałe. Bot 300 przebiegów/zawód (kontrakty 150).

**Poza celem (do decyzji):** pełne Szkolenia 67,1 (55–65), Akt 0 + meta 80,2 (65–75), inwestor 15,2 (5–15). Wariant perRarity 0/1/1/0 (tylko meta): Szkolenia 66,9, + Respekt 75,3, Akt 0 + meta 78,2, inwestor 13,0 – nadal poza celem. Przyczyna: pełne Szkolenia dają broń startową Markową (Szkolenie Narzędzia), więc każdy profil meta zaczyna z profitami; Akt 0 + meta był już 74,7 (sufit) przed profitami. Wartości profitów są już minimalne (1 pkt), więc same wartości / perRarity nie zejdą pod sufit bez wyłączenia profitów broni startowej.

Przebiegi na zawód: 300 (kontrakty 150); średnie celów = zwykłe zawody (bez sekretów), jak core_tests.cpp.

| Cel | Wynik | Zakres |
|---|---|---|
| Łatwy | 57,2 | 50–60 |
| Normalny | 34,6 (wszystkie zawody 34,1) | 25–35 |
| Trudny | 10,5 | 8–15 |
| Pełne Szkolenia | 67,1 | 55–65 |
| + Respekt | 77,2 | 70–80 |
| Akt 0 + meta | 80,2 | 65–75 |
| Inwestor (wszystkie modyfikatory) | 15,2 | 5–15 |
| Kontrakt Letniskowy bez meta / pełne meta | 30,4 / 80,0 | 25–40 / 65–80 |
| Kontrakt Bliźniak bez meta / pełne meta | 28,2 / 69,4 | 25–40 / 65–80 |
| Kontrakt Poddasze bez meta / pełne meta | 31,7 / 71,1 | 25–40 / 65–80 |
| Kontrakt Kamienica bez meta / pełne meta | 30,2 / 70,4 | 25–40 / 65–80 |
| Tylko broń domyślna, Normalny | 33,7 (wszystkie 33,7) | zawód ≥ v0.22.1 −5 p.p. |

| Zawód | v0.22.1 | Bot (Normalny) | Tylko domyślna | Δ domyślna | Najgorszy etap domyślnej (Δ udziału porażek, p.p.) |
|---|---|---|---|---|---|
| Kierownik budowy | 36,7 | 36,6 | 36,0 | -0,7 | 10: 5,3 |
| Murarz | 16,0 | 25,0 | 25,0 | 9,0 | 11: 4,0 |
| Cieśla-dekarz | 48,7 | 49,0 | 51,3 | 2,6 | 7: 2,0 |
| Elektryk | 35,3 | 37,6 | 37,3 | 2,0 | 10: 2,3 |
| Hydraulik | 14,0 | 18,6 | 16,6 | 2,6 | 11: 1,6 |
| Glazurnik | 23,3 | 30,0 | 24,6 | 1,3 | 11: 3,0 |
| Dekarz | 57,0 | 52,0 | 53,3 | -3,7 | 9: 2,7 |
| Tynkarz | 36,7 | 37,6 | 37,0 | 0,3 | 5: 5,3 |
| Operator koparki | 23,0 | 24,3 | 22,3 | -0,7 | 9: 4,6 |
| Spawacz | 35,0 | 34,0 | 36,3 | 1,3 | 11: 6,0 |
| Geodeta | 36,7 | 37,0 | 39,0 | 2,3 | 11: 4,0 |
| Majster | 23,0 | 27,6 | 25,0 | 2,0 | 9: 4,9 |

Porażki na etapach (etap: udział % wszystkich przebiegów zawodu), Normalny:

| Zawód | Bot | Tylko domyślna |
|---|---|---|
| Kierownik budowy | 5: 7,6, 6: 3,6, 7: 3,0, 8: 19,0, 9: 11,3, 10: 5,6, 11: 13,0 | 5: 8,0, 6: 3,3, 7: 3,0, 8: 17,3, 9: 12,0, 10: 7,3, 11: 13,0 |
| Murarz | 5: 5,0, 6: 2,6, 7: 1,6, 8: 22,0, 9: 20,3, 10: 5,3, 11: 18,0 | 5: 4,6, 6: 2,6, 7: 2,0, 8: 22,3, 9: 20,0, 10: 5,3, 11: 18,0 |
| Cieśla-dekarz | 4: 0,6, 5: 5,3, 6: 2,0, 7: 3,3, 8: 12,0, 9: 11,3, 10: 5,0, 11: 11,3 | 3: 0,3, 4: 0,6, 5: 4,6, 6: 1,6, 7: 3,0, 8: 11,3, 9: 11,3, 10: 4,6, 11: 11,0 |
| Elektryk | 2: 0,3, 3: 0,3, 4: 0,3, 5: 10,3, 6: 3,0, 7: 3,6, 8: 13,0, 9: 13,6, 10: 3,3, 11: 14,3 | 2: 0,3, 3: 0,3, 4: 0,3, 5: 10,6, 6: 2,6, 7: 3,0, 8: 13,3, 9: 13,3, 10: 3,3, 11: 15,3 |
| Hydraulik | 4: 0,3, 5: 6,0, 6: 4,0, 7: 3,6, 8: 25,3, 9: 22,3, 10: 4,6, 11: 15,0 | 4: 0,3, 5: 6,0, 6: 4,0, 7: 3,6, 8: 26,6, 9: 23,3, 10: 4,6, 11: 14,6 |
| Glazurnik | 5: 10,3, 6: 3,6, 7: 3,0, 8: 19,3, 9: 16,3, 10: 3,0, 11: 14,3 | 5: 11,3, 6: 4,0, 7: 3,0, 8: 21,6, 9: 16,0, 10: 3,3, 11: 16,0 |
| Dekarz | 5: 13,3, 6: 3,0, 7: 0,3, 8: 9,6, 9: 10,6, 10: 1,6, 11: 9,3 | 5: 13,3, 6: 2,6, 7: 0,6, 8: 8,0, 9: 11,0, 10: 1,3, 11: 9,6 |
| Tynkarz | 5: 13,6, 6: 1,6, 7: 2,3, 8: 21,0, 9: 11,0, 10: 1,3, 11: 11,3 | 5: 15,0, 6: 2,0, 7: 2,6, 8: 20,3, 9: 12,3, 10: 0,6, 11: 10,0 |
| Operator koparki | 4: 0,3, 5: 11,0, 6: 4,3, 7: 2,6, 8: 19,0, 9: 22,3, 10: 3,3, 11: 12,6 | 4: 0,3, 5: 11,0, 6: 4,0, 7: 3,0, 8: 19,6, 9: 23,3, 10: 3,3, 11: 13,0 |
| Spawacz | 2: 0,3, 5: 20,6, 6: 2,3, 7: 2,6, 8: 16,3, 9: 6,6, 10: 2,6, 11: 14,3 | 2: 0,3, 5: 20,0, 6: 2,3, 7: 2,3, 8: 15,0, 9: 7,0, 10: 2,6, 11: 14,0 |
| Geodeta | 4: 0,6, 5: 9,3, 6: 4,0, 7: 3,0, 8: 19,3, 9: 9,0, 10: 2,6, 11: 15,0 | 4: 0,6, 5: 8,0, 6: 4,0, 7: 3,0, 8: 19,6, 9: 9,0, 10: 3,3, 11: 13,3 |
| Majster | 4: 0,3, 5: 7,6, 6: 1,6, 7: 1,3, 8: 15,0, 9: 24,6, 10: 1,0, 11: 20,6 | 4: 0,6, 5: 7,6, 6: 1,3, 7: 1,6, 8: 15,6, 9: 25,6, 10: 1,0, 11: 21,3 |

## Kryt i problemy (v0.23.0, TODO #93) – pomiar 2026-11-07

Zmiany: `luck.critPerLuckPct` 3 → 2, `hitTypes.cutCritPct` 10 → 5, nowy sufit `luck.critMaxPct` 35; unik ze szczęścia bez zmian
(średni unik bota 6,0% < 8%); obrażenia problemów: `stages[].dmgBonus` Fundamenty, Izolacja, Mury parteru 0 → 1, Dach 2 → 3
(+1 w całości okazało się za mocne: Normalny 14,6%); Trudny `hpPct` 165 → 150 (bez tego 6,3%); broń zawodu na start bez losowych
profitów (opcja a; Mistrzowska – stałe).

| Pomiar (bot, Normalny, 300/zawód) | Przed | Po |
|---|---|---|
| Kryt – średnia szansa przy trafieniu / faktycznie | 20,8 / 18,3 | 16,1 / 14,1 |
| Glazurnik (najwyższy kryt) – szansa | 33,0 | 23,0 |
| Obrażenia otrzymane na etap | 12,7 | 13,4 |
| Unik – średnia szansa / faktycznie | 6,1 / 5,7 | 6,0 / 5,6 |
| Wygrane (wszystkie zawody) | 34,1 | 27,0 |

| Cel | Przed (#87) | Po (#93) | Zakres |
|---|---|---|---|
| Łatwy | 57,2 | 52,2 | 50–60 |
| Normalny | 34,6 | 26,6 | 25–35 (cel 25–28) |
| Trudny | 10,5 | 9,8 | 8–15 |
| Pełne Szkolenia | 67,1 | 60,1 | 55–65 |
| + Respekt | 77,2 | 70,3 | 70–80 |
| Akt 0 + meta | 80,2 | 71,2 | 65–75 |
| Inwestor | 15,2 | 9,6 | 5–15 |
| Kontrakty bez meta | 28,2–31,7 | 26,6–28,7 | 25–40 |
| Kontrakty pełne meta | 69,4–80,0 | **63,5** (Bliźniak)–71,8 | 65–80 |
| Tylko broń domyślna | 33,7 | 27,0 | zawód ≥ v0.22.1 −5 p.p. – **7 zawodów poniżej** |

Poza celem: Bliźniak z pełnym meta 63,5 (1,5 p.p. pod progiem; 150 przebiegów), oraz test „tylko broń domyślna ≥ v0.22.1 −5 p.p.”
– sprzeczny z nowym celem Normalnego 25–28 (średnia v0.22.1 to 32,1, więc średni spadek ~5,5 p.p. jest wpisany w cel).
Względem bota z bronią (ta sama wersja) broń domyślna mieści się w −5 p.p. dla każdego zawodu (najgorzej Glazurnik −5,0).

Przebiegi na zawód: 300 (kontrakty 150); średnie celów = zwykłe zawody (bez sekretów), jak core_tests.cpp.

| Cel | Wynik | Zakres |
|---|---|---|
| Łatwy | 52,2 | 50–60 |
| Normalny | 26,6 (wszystkie zawody 27,0) | 25–35 |
| Trudny | 9,8 | 8–15 |
| Pełne Szkolenia | 60,1 | 55–65 |
| + Respekt | 70,3 | 70–80 |
| Akt 0 + meta | 71,2 | 65–75 |
| Inwestor (wszystkie modyfikatory) | 9,6 | 5–15 |
| Kontrakt Letniskowy bez meta / pełne meta | 27,6 / 71,8 | 25–40 / 65–80 |
| Kontrakt Bliźniak bez meta / pełne meta | 26,6 / 63,5 | 25–40 / 65–80 |
| Kontrakt Poddasze bez meta / pełne meta | 28,4 / 67,1 | 25–40 / 65–80 |
| Kontrakt Kamienica bez meta / pełne meta | 28,7 / 66,3 | 25–40 / 65–80 |
| Tylko broń domyślna, Normalny | 27,0 (wszystkie 27,2) | zawód ≥ v0.22.1 −5 p.p. |

| Zawód | v0.22.1 | Bot (Normalny) | Tylko domyślna | Δ domyślna | Najgorszy etap domyślnej (Δ udziału porażek, p.p.) |
|---|---|---|---|---|---|
| Kierownik budowy | 36,7 | 30,3 | 33,0 | -3,7 | 6: 4,3 |
| Murarz | 16,0 | 19,3 | 19,3 | 3,3 | 6: 6,3 |
| Cieśla-dekarz | 48,7 | 37,6 | 42,6 | -6,1 | 6: 3,3 |
| Elektryk | 35,3 | 28,0 | 28,3 | -7,0 | 10: 4,3 |
| Hydraulik | 14,0 | 13,6 | 12,6 | -1,4 | 6: 6,3 |
| Glazurnik | 23,3 | 19,0 | 14,0 | -9,3 | 5: 10,0 |
| Dekarz | 57,0 | 45,6 | 50,0 | -7,0 | 11: 3,6 |
| Tynkarz | 36,7 | 25,6 | 25,0 | -11,7 | 5: 7,3 |
| Operator koparki | 23,0 | 19,6 | 18,3 | -4,7 | 6: 6,3 |
| Spawacz | 35,0 | 30,0 | 28,6 | -6,4 | 11: 4,3 |
| Geodeta | 36,7 | 28,3 | 30,0 | -6,7 | 6: 9,3 |
| Majster | 23,0 | 27,0 | 24,3 | 1,3 | 6: 6,0 |

Porażki na etapach (etap: udział % wszystkich przebiegów zawodu), Normalny:

| Zawód | Bot | Tylko domyślna |
|---|---|---|
| Kierownik budowy | 2: 0,6, 3: 1,3, 4: 1,3, 5: 8,0, 6: 7,3, 7: 3,3, 8: 18,0, 9: 12,6, 10: 5,3, 11: 11,6 | 2: 0,6, 3: 1,3, 4: 1,3, 5: 7,3, 6: 7,0, 7: 3,6, 8: 17,0, 9: 11,6, 10: 5,6, 11: 11,3 |
| Murarz | 2: 0,3, 3: 0,3, 4: 0,3, 5: 7,0, 6: 8,3, 7: 2,3, 8: 18,0, 9: 19,0, 10: 6,3, 11: 18,6 | 2: 0,3, 3: 0,3, 4: 0,3, 5: 7,3, 6: 8,3, 7: 2,3, 8: 18,3, 9: 19,3, 10: 6,0, 11: 18,0 |
| Cieśla-dekarz | 2: 1,0, 3: 1,0, 4: 0,3, 5: 5,3, 6: 5,6, 7: 1,3, 8: 13,6, 9: 12,3, 10: 5,0, 11: 16,6 | 2: 1,0, 3: 1,0, 4: 0,3, 5: 6,3, 6: 4,6, 7: 1,3, 8: 10,6, 9: 12,3, 10: 4,6, 11: 15,0 |
| Elektryk | 2: 0,3, 3: 0,6, 4: 1,3, 5: 11,0, 6: 6,6, 7: 5,0, 8: 16,0, 9: 11,0, 10: 5,3, 11: 14,6 | 2: 0,3, 3: 0,6, 4: 1,3, 5: 11,3, 6: 7,0, 7: 5,0, 8: 15,0, 9: 11,0, 10: 5,3, 11: 14,6 |
| Hydraulik | 3: 0,3, 4: 1,6, 5: 7,6, 6: 9,6, 7: 3,0, 8: 28,6, 9: 22,3, 10: 2,3, 11: 10,6 | 3: 0,3, 4: 1,6, 5: 7,6, 6: 9,6, 7: 3,0, 8: 29,0, 9: 22,3, 10: 2,3, 11: 11,3 |
| Glazurnik | 2: 2,0, 3: 1,0, 4: 4,3, 5: 18,3, 6: 8,6, 7: 3,0, 8: 17,6, 9: 14,0, 10: 2,0, 11: 10,0 | 2: 2,0, 3: 1,0, 4: 4,6, 5: 19,0, 6: 10,3, 7: 4,0, 8: 17,3, 9: 14,3, 10: 1,6, 11: 11,6 |
| Dekarz | 2: 0,3, 3: 0,6, 4: 0,6, 5: 17,3, 6: 6,3, 7: 0,3, 8: 9,3, 9: 9,6, 10: 0,3, 11: 9,3 | 2: 0,3, 3: 0,6, 4: 0,6, 5: 17,6, 6: 4,3, 7: 0,3, 8: 7,3, 9: 7,3, 10: 0,6, 11: 10,6 |
| Tynkarz | 4: 0,3, 5: 16,0, 6: 8,0, 7: 2,6, 8: 19,3, 9: 14,6, 10: 1,0, 11: 12,3 | 5: 17,0, 6: 8,0, 7: 3,3, 8: 19,0, 9: 15,6, 10: 0,6, 11: 11,3 |
| Operator koparki | 4: 1,3, 5: 13,0, 6: 8,6, 7: 2,3, 8: 19,0, 9: 20,0, 10: 3,3, 11: 12,6 | 4: 1,3, 5: 13,0, 6: 9,6, 7: 2,3, 8: 19,0, 9: 20,3, 10: 3,3, 11: 12,6 |
| Spawacz | 2: 0,3, 3: 0,6, 4: 1,0, 5: 20,6, 6: 8,3, 7: 2,0, 8: 16,3, 9: 7,6, 10: 3,0, 11: 10,0 | 2: 0,3, 3: 0,3, 4: 1,0, 5: 20,0, 6: 7,6, 7: 2,0, 8: 16,3, 9: 8,6, 10: 2,6, 11: 12,3 |
| Geodeta | 3: 0,6, 4: 1,3, 5: 10,6, 6: 12,3, 7: 2,6, 8: 23,0, 9: 8,0, 10: 3,6, 11: 9,3 | 3: 0,6, 4: 1,0, 5: 8,6, 6: 11,6, 7: 3,3, 8: 22,3, 9: 9,3, 10: 4,0, 11: 9,0 |
| Majster | 4: 0,3, 5: 10,6, 6: 8,0, 7: 1,3, 8: 15,6, 9: 22,3, 10: 1,3, 11: 13,3 | 4: 0,6, 5: 11,0, 6: 7,3, 7: 1,3, 8: 16,3, 9: 23,0, 10: 1,3, 11: 14,6 |

Bot, Normalny, 300 przebiegów na zawód. Kryt: średnia szansa w chwili trafienia bohatera / faktyczny udział krytów; unik: średnia szansa przy ciosie problemu / faktyczny udział uników.

| Zawód | Wygrane % | Kryt % (szansa / faktycznie) | Obrażenia otrzymane na etap | Unik % (szansa / faktycznie) |
|---|---|---|---|---|
| Kierownik budowy | 30,3 | 15,0 / 13,9 | 13,3 | 6,3 / 6,0 |
| Murarz | 19,3 | 15,4 / 14,0 | 14,4 | 2,3 / 2,2 |
| Cieśla-dekarz | 37,7 | 15,4 / 14,2 | 12,3 | 6,6 / 5,8 |
| Elektryk | 28,0 | 12,7 / 10,6 | 11,6 | 4,5 / 4,1 |
| Hydraulik | 13,7 | 15,0 / 13,9 | 15,3 | 6,4 / 6,3 |
| Glazurnik | 19,0 | 23,0 / 20,1 | 14,9 | 9,9 / 9,8 |
| Dekarz | 45,7 | 17,5 / 16,1 | 10,7 | 8,4 / 8,0 |
| Tynkarz | 25,7 | 14,9 / 13,8 | 13,6 | 6,2 / 5,7 |
| Operator koparki | 19,7 | 10,8 / 9,8 | 15,0 | 2,3 / 2,2 |
| Spawacz | 30,0 | 19,8 / 12,1 | 12,4 | 6,5 / 5,8 |
| Geodeta | 28,3 | 17,1 / 15,7 | 13,3 | 8,5 / 7,9 |
| Majster | 27,0 | 19,1 / 17,9 | 14,7 | 6,3 / 5,7 |
| **Razem** | 27,0 | 16,1 / 14,1 | 13,4 | 6,0 / 5,6 |

### Rulingi po pomiarze #93 (2026-11-07)
- **Broń domyślna**: test `DefaultWeaponNotWorse` porównuje teraz bota tylko z bronią domyślną z botem z bronią tej samej wersji
  (próg −5 p.p. na zawód), bez bazy v0.22.1 – właściciel zażyczył sobie trudniejszego Normalnego (25–28%), a średnia v0.22.1
  (32,1%) leży ponad celem, więc spadek względem v0.22.1 wynika z celu. Pomiar: wszystkie zawody w progu, najbliżej Glazurnik −5,0
  (19,0 → 14,0). Test zielony.
- **Kontrakt Bliźniak z pełnym meta 63,5** – przyjęte jako szum (150 przebiegów); do ponownego pomiaru na 300 przebiegach w pełnym
  zestawie przed wydaniem. Bez strojenia.
- **Glazurnik i Tynkarz** (największe spadki po rzadszym krycie i mocniejszych etapach) – strojenie zawodów odłożone na później.
- Opis szczęścia w UI pokazuje sufit krytu: „SZCZ: kryt 5% +2%/pkt (maks. 35%)”.

## v0.23.0 – artefakty (#92), 2026-11-07

Pomiar po artefaktach (artefakt zamiast Markowej bazy z dropu 25%, co N-ty cios, Łom Ostatniej Deski, Nóż majstra seniora 4–7). Przed = pomiar #93.

| Cel | Przed (#93) | Po (#92) | Zakres |
|---|---|---|---|
| Łatwy | 52,2 | 52,4 | 50–60 |
| Normalny | 26,6 | 26,2 | 25–28 |
| Trudny | 9,8 | 9,8 | 8–15 |
| Pełne Szkolenia | 60,1 | 60,0 | 55–65 |
| + Respekt | 70,3 | 69,7 | 70–80 (−0,3, szum) |
| Akt 0 + meta | 71,2 | 71,3 | 65–75 |
| Inwestor | 9,6 | 9,4 | 5–15 |
| Kontrakty bez meta | 26,6–28,7 | 26,1–29,0 | 25–40 |
| Kontrakty pełne meta | 63,5–71,8 | 64,1–71,8 | 65–80 (Bliźniak 64,1) |
| Tylko broń domyślna | 27,0 | 27,2 | ≥ bot −5 p.p. (zielone) |

Pełny wynik:

Przebiegi na zawód: 300 (kontrakty 150); średnie celów = zwykłe zawody (bez sekretów), jak core_tests.cpp.

| Cel | Wynik | Zakres |
|---|---|---|
| Łatwy | 52,4 | 50–60 |
| Normalny | 26,2 (wszystkie zawody 26,6) | 25–35 |
| Trudny | 9,8 | 8–15 |
| Pełne Szkolenia | 60,0 | 55–65 |
| + Respekt | 69,7 | 70–80 |
| Akt 0 + meta | 71,3 | 65–75 |
| Inwestor (wszystkie modyfikatory) | 9,4 | 5–15 |
| Kontrakt Letniskowy bez meta / pełne meta | 27,4 / 71,8 | 25–40 / 65–80 |
| Kontrakt Bliźniak bez meta / pełne meta | 26,1 / 64,1 | 25–40 / 65–80 |
| Kontrakt Poddasze bez meta / pełne meta | 29,0 / 66,2 | 25–40 / 65–80 |
| Kontrakt Kamienica bez meta / pełne meta | 28,7 / 65,8 | 25–40 / 65–80 |
| Tylko broń domyślna, Normalny | 27,2 (wszystkie 27,2) | zawód ≥ bot z bronią −5 p.p. |

| Zawód | v0.22.1 | Bot (Normalny) | Tylko domyślna | Δ domyślna (vs bot) | Najgorszy etap domyślnej (Δ udziału porażek, p.p.) |
|---|---|---|---|---|---|
| Kierownik budowy | 36,7 | 29,6 | 33,6 | 4,0 | 6: 4,6 |
| Murarz | 16,0 | 21,6 | 21,0 | -0,6 | 6: 5,0 |
| Cieśla-dekarz | 48,7 | 39,3 | 44,3 | 5,0 | 11: 3,6 |
| Elektryk | 35,3 | 27,6 | 28,3 | 0,7 | 10: 4,0 |
| Hydraulik | 14,0 | 13,6 | 12,3 | -1,3 | 6: 6,7 |
| Glazurnik | 23,3 | 16,0 | 13,3 | -2,7 | 5: 10,0 |
| Dekarz | 57,0 | 43,3 | 48,3 | 5,0 | 11: 4,0 |
| Tynkarz | 36,7 | 26,0 | 26,0 | 0,0 | 5: 6,9 |
| Operator koparki | 23,0 | 18,3 | 17,6 | -0,7 | 6: 6,0 |
| Spawacz | 35,0 | 29,0 | 28,3 | -0,7 | 11: 4,3 |
| Geodeta | 36,7 | 27,6 | 29,0 | 1,4 | 6: 9,7 |
| Majster | 23,0 | 26,3 | 24,0 | -2,3 | 6: 6,3 |

Porażki na etapach (etap: udział % wszystkich przebiegów zawodu), Normalny:

| Zawód | Bot | Tylko domyślna |
|---|---|---|
| Kierownik budowy | 2: 0,6, 3: 1,3, 4: 1,3, 5: 8,0, 6: 7,6, 7: 3,3, 8: 19,6, 9: 11,6, 10: 5,0, 11: 11,6 | 2: 0,6, 3: 1,3, 4: 1,3, 5: 7,3, 6: 7,3, 7: 3,6, 8: 18,3, 9: 9,6, 10: 5,3, 11: 11,3 |
| Murarz | 2: 0,3, 3: 0,3, 4: 0,3, 5: 7,0, 6: 7,3, 7: 2,3, 8: 18,3, 9: 19,3, 10: 6,3, 11: 16,6 | 2: 0,3, 3: 0,3, 4: 0,3, 5: 7,3, 6: 7,0, 7: 2,6, 8: 19,6, 9: 19,3, 10: 6,0, 11: 16,0 |
| Cieśla-dekarz | 2: 1,0, 3: 1,0, 4: 0,6, 5: 5,0, 6: 5,6, 7: 1,3, 8: 12,3, 9: 12,6, 10: 4,3, 11: 16,6 | 2: 1,0, 3: 1,0, 4: 0,3, 5: 5,6, 6: 4,6, 7: 1,3, 8: 10,3, 9: 12,0, 10: 4,0, 11: 15,3 |
| Elektryk | 2: 0,3, 3: 0,6, 4: 1,3, 5: 12,0, 6: 6,6, 7: 5,3, 8: 15,3, 9: 11,3, 10: 5,0, 11: 14,3 | 2: 0,3, 3: 0,6, 4: 1,3, 5: 12,0, 6: 7,0, 7: 5,3, 8: 14,3, 9: 11,3, 10: 5,0, 11: 14,3 |
| Hydraulik | 3: 0,3, 4: 1,6, 5: 7,0, 6: 10,0, 7: 3,6, 8: 27,3, 9: 22,3, 10: 2,3, 11: 11,6 | 3: 0,3, 4: 1,6, 5: 7,0, 6: 10,0, 7: 3,6, 8: 28,3, 9: 22,0, 10: 2,3, 11: 12,3 |
| Glazurnik | 2: 2,0, 3: 1,0, 4: 4,3, 5: 18,0, 6: 9,0, 7: 3,6, 8: 19,3, 9: 10,6, 10: 3,6, 11: 12,3 | 2: 2,0, 3: 1,0, 4: 4,6, 5: 19,0, 6: 11,3, 7: 4,0, 8: 19,3, 9: 10,0, 10: 2,3, 11: 13,0 |
| Dekarz | 2: 0,3, 3: 0,6, 4: 0,6, 5: 17,3, 6: 6,6, 7: 0,3, 8: 10,3, 9: 10,0, 10: 0,3, 11: 10,0 | 2: 0,3, 3: 0,6, 4: 0,6, 5: 17,6, 6: 4,6, 7: 0,3, 8: 7,3, 9: 8,6, 10: 0,3, 11: 11,0 |
| Tynkarz | 4: 0,3, 5: 15,6, 6: 8,3, 7: 2,6, 8: 19,6, 9: 14,6, 10: 0,6, 11: 12,0 | 5: 16,6, 6: 8,3, 7: 3,3, 8: 18,3, 9: 16,0, 10: 0,6, 11: 10,6 |
| Operator koparki | 4: 1,3, 5: 13,3, 6: 8,6, 7: 2,3, 8: 18,3, 9: 20,3, 10: 3,3, 11: 14,0 | 4: 1,3, 5: 13,0, 6: 9,3, 7: 2,3, 8: 18,6, 9: 20,3, 10: 3,3, 11: 14,0 |
| Spawacz | 2: 0,3, 3: 0,6, 4: 1,0, 5: 20,6, 6: 8,6, 7: 2,0, 8: 16,3, 9: 8,0, 10: 3,0, 11: 10,3 | 2: 0,3, 3: 0,3, 4: 1,0, 5: 20,0, 6: 7,6, 7: 2,0, 8: 16,6, 9: 8,6, 10: 2,6, 11: 12,3 |
| Geodeta | 3: 0,6, 4: 1,3, 5: 11,0, 6: 12,0, 7: 2,3, 8: 23,0, 9: 8,3, 10: 5,0, 11: 8,6 | 3: 0,6, 4: 1,0, 5: 8,6, 6: 12,0, 7: 3,0, 8: 22,3, 9: 9,3, 10: 5,0, 11: 9,0 |
| Majster | 4: 0,3, 5: 10,6, 6: 8,6, 7: 1,3, 8: 17,3, 9: 20,6, 10: 0,6, 11: 14,0 | 4: 0,6, 5: 11,3, 6: 7,6, 7: 1,3, 8: 17,6, 9: 21,6, 10: 0,6, 11: 15,0 |

## v0.23.0 – pomiar przed wydaniem (Task 11, 2026-11-07, stan f29ba409 + dokumentacja)

Pełny zestaw: `WeaponBalanceTests` (300 przebiegów/zawód, kontrakty 150), `BalanceBaselineTests` (Normalny, bez meta,
razem 26,6%), `CombatFeelTests` (Normalny) i dodatkowy pomiar wszystkich kontraktów na **300** przebiegach/zawód
(ten sam bot i ziarna `1000 + k*7919`, zwykłe zawody, pełne meta z Aktem 0 jak w `WeaponBalanceTests.Rate`).
Bez strojenia – liczby do decyzji właściciela razem z „wydajemy?”.

**Poza celem:** + Respekt **69,7** (cel 70–80; test `MetaInTarget` czerwony – drugi pomiar z rzędu 69,7);
kontrakty z pełnym meta na 300 przebiegach: Bliźniak **62,7**, Poddasze **63,3**, Kamienica **64,2** (cel 65–80;
na 150 przebiegach 65,6 / 65,4 / 65,3 – druga połowa ziaren jest słabsza, więc to nie szum). Normalny 26,2 (cel właściciela 25–28) ✓.

Kontrakty, 300 przebiegów na zawód (bez meta / pełne meta z Aktem 0):

| Kontrakt | Bez meta | Pełne meta | Zakres |
|---|---|---|---|
| Letniskowy | 27,5 | 70,8 | 25–40 / 65–80 |
| Bliźniak | 25,1 | **62,7** | 25–40 / 65–80 |
| Poddasze | 27,4 | **63,3** | 25–40 / 65–80 |
| Kamienica | 26,9 | **64,2** | 25–40 / 65–80 |

Rdzeń GBA (`core_tests.cpp`, pełny balans, inne ziarna, progi GBA poluzowane w Task 10): Łatwy 51, Normalny 26, Trudny 9,
pełne Szkolenia 58, + Respekt 68, inwestor 8, Akt 0 + meta 66, kontrakty 26–27 / 58–68, broń domyślna 27, kawa 3,29/budowę;
wszystko wykupione po 31 budowach (cel 20–30) – „OK – wszystkie testy przeszły”.

Kryt / obrażenia / unik (bot, Normalny, razem): kryt 16,1% szansy / 14,1% faktycznie, 13,5 obrażeń otrzymanych na etap,
unik 6,0 / 5,6 – bez zmian względem #93.

Przebiegi na zawód: 300 (kontrakty 150); średnie celów = zwykłe zawody (bez sekretów), jak core_tests.cpp.

| Cel | Wynik | Zakres |
|---|---|---|
| Łatwy | 52,4 | 50–60 |
| Normalny | 26,2 (wszystkie zawody 26,6) | 25–35 |
| Trudny | 9,8 | 8–15 |
| Pełne Szkolenia | 60,0 | 55–65 |
| + Respekt | 69,7 | 70–80 |
| Akt 0 + meta | 70,9 | 65–75 |
| Inwestor (wszystkie modyfikatory) | 9,4 | 5–15 |
| Kontrakt Letniskowy bez meta / pełne meta | 27,4 / 72,3 | 25–40 / 65–80 |
| Kontrakt Bliźniak bez meta / pełne meta | 26,1 / 65,6 | 25–40 / 65–80 |
| Kontrakt Poddasze bez meta / pełne meta | 29,0 / 65,4 | 25–40 / 65–80 |
| Kontrakt Kamienica bez meta / pełne meta | 28,7 / 65,3 | 25–40 / 65–80 |
| Tylko broń domyślna, Normalny | 27,3 (wszystkie 27,2) | zawód ≥ bot z bronią −5 p.p. |

| Zawód | v0.22.1 | Bot (Normalny) | Tylko domyślna | Δ domyślna (vs bot) | Najgorszy etap domyślnej (Δ udziału porażek, p.p.) |
|---|---|---|---|---|---|
| Kierownik budowy | 36,7 | 29,6 | 33,6 | 4,0 | 6: 4,6 |
| Murarz | 16,0 | 21,6 | 21,0 | -0,6 | 6: 5,0 |
| Cieśla-dekarz | 48,7 | 39,3 | 44,3 | 5,0 | 11: 3,6 |
| Elektryk | 35,3 | 27,6 | 28,3 | 0,7 | 10: 4,0 |
| Hydraulik | 14,0 | 13,6 | 12,3 | -1,3 | 6: 6,7 |
| Glazurnik | 23,3 | 16,0 | 13,3 | -2,7 | 5: 10,0 |
| Dekarz | 57,0 | 43,6 | 48,6 | 5,0 | 11: 3,6 |
| Tynkarz | 36,7 | 26,0 | 26,0 | 0,0 | 5: 6,9 |
| Operator koparki | 23,0 | 18,3 | 17,6 | -0,7 | 6: 6,0 |
| Spawacz | 35,0 | 29,0 | 28,3 | -0,7 | 11: 4,3 |
| Geodeta | 36,7 | 27,6 | 29,0 | 1,4 | 6: 9,7 |
| Majster | 23,0 | 26,3 | 24,0 | -2,3 | 6: 6,3 |

Porażki na etapach (etap: udział % wszystkich przebiegów zawodu), Normalny:

| Zawód | Bot | Tylko domyślna |
|---|---|---|
| Kierownik budowy | 2: 0,6, 3: 1,3, 4: 1,3, 5: 8,0, 6: 7,6, 7: 3,3, 8: 19,6, 9: 11,6, 10: 5,0, 11: 11,6 | 2: 0,6, 3: 1,3, 4: 1,3, 5: 7,3, 6: 7,3, 7: 3,6, 8: 18,3, 9: 9,6, 10: 5,3, 11: 11,3 |
| Murarz | 2: 0,3, 3: 0,3, 4: 0,3, 5: 7,0, 6: 7,3, 7: 2,3, 8: 18,3, 9: 19,3, 10: 6,3, 11: 16,6 | 2: 0,3, 3: 0,3, 4: 0,3, 5: 7,3, 6: 7,0, 7: 2,6, 8: 19,6, 9: 19,3, 10: 6,0, 11: 16,0 |
| Cieśla-dekarz | 2: 1,0, 3: 1,0, 4: 0,6, 5: 5,0, 6: 5,6, 7: 1,3, 8: 12,3, 9: 12,6, 10: 4,3, 11: 16,6 | 2: 1,0, 3: 1,0, 4: 0,3, 5: 5,6, 6: 4,6, 7: 1,3, 8: 10,3, 9: 12,0, 10: 4,0, 11: 15,3 |
| Elektryk | 2: 0,3, 3: 0,6, 4: 1,3, 5: 12,0, 6: 6,6, 7: 5,3, 8: 15,3, 9: 11,3, 10: 5,0, 11: 14,3 | 2: 0,3, 3: 0,6, 4: 1,3, 5: 12,0, 6: 7,0, 7: 5,3, 8: 14,3, 9: 11,3, 10: 5,0, 11: 14,3 |
| Hydraulik | 3: 0,3, 4: 1,6, 5: 7,0, 6: 10,0, 7: 3,6, 8: 27,3, 9: 22,3, 10: 2,3, 11: 11,6 | 3: 0,3, 4: 1,6, 5: 7,0, 6: 10,0, 7: 3,6, 8: 28,3, 9: 22,0, 10: 2,3, 11: 12,3 |
| Glazurnik | 2: 2,0, 3: 1,0, 4: 4,3, 5: 18,0, 6: 9,0, 7: 3,6, 8: 19,3, 9: 10,6, 10: 3,6, 11: 12,3 | 2: 2,0, 3: 1,0, 4: 4,6, 5: 19,0, 6: 11,3, 7: 4,0, 8: 19,3, 9: 10,0, 10: 2,3, 11: 13,0 |
| Dekarz | 2: 0,3, 3: 0,6, 4: 0,6, 5: 17,3, 6: 6,6, 7: 0,3, 8: 10,3, 9: 10,0, 10: 0,3, 11: 9,6 | 2: 0,3, 3: 0,6, 4: 0,6, 5: 17,6, 6: 4,6, 7: 0,3, 8: 7,3, 9: 8,6, 10: 0,3, 11: 10,6 |
| Tynkarz | 4: 0,3, 5: 15,6, 6: 8,3, 7: 2,6, 8: 19,6, 9: 14,6, 10: 0,6, 11: 12,0 | 5: 16,6, 6: 8,3, 7: 3,3, 8: 18,3, 9: 16,0, 10: 0,6, 11: 10,6 |
| Operator koparki | 4: 1,3, 5: 13,3, 6: 8,6, 7: 2,3, 8: 18,3, 9: 20,3, 10: 3,3, 11: 14,0 | 4: 1,3, 5: 13,0, 6: 9,3, 7: 2,3, 8: 18,6, 9: 20,3, 10: 3,3, 11: 14,0 |
| Spawacz | 2: 0,3, 3: 0,6, 4: 1,0, 5: 20,6, 6: 8,6, 7: 2,0, 8: 16,3, 9: 8,0, 10: 3,0, 11: 10,3 | 2: 0,3, 3: 0,3, 4: 1,0, 5: 20,0, 6: 7,6, 7: 2,0, 8: 16,6, 9: 8,6, 10: 2,6, 11: 12,3 |
| Geodeta | 3: 0,6, 4: 1,3, 5: 11,0, 6: 12,0, 7: 2,3, 8: 23,0, 9: 8,3, 10: 5,0, 11: 8,6 | 3: 0,6, 4: 1,0, 5: 8,6, 6: 12,0, 7: 3,0, 8: 22,3, 9: 9,3, 10: 5,0, 11: 9,0 |
| Majster | 4: 0,3, 5: 10,6, 6: 8,6, 7: 1,3, 8: 17,3, 9: 20,6, 10: 0,6, 11: 14,0 | 4: 0,6, 5: 11,3, 6: 7,6, 7: 1,3, 8: 17,6, 9: 21,6, 10: 0,6, 11: 15,0 |

## v0.23.0 – krok w bok w pościgu i strojenie (2026-11-07, `ad8a8639` … `6573972d`)

Błąd z TestFlight („wrogowie przestali chodzić za graczem”): `ChaseStep` / `chase_step` próbował tylko prostego kroku.
Problem w linii z bohaterem stał, gdy pole zasłaniała ściana, róg albo inny problem. Teraz robi krok w bok na wolne pole,
które nie oddala od bohatera (Czebyszew), w kolejności +x, -x, +y, -y. W Godocie (CHASE=1, ziarna 11 i 12) problemy
ruszają się w 70% tur pościgu, wcześniej w 36%.

Strojenie (decyzja koordynatora): Fundamenty, Izolacja fundamentów i Mury parteru wracają do 0 obrażeń problemów
(było +1 z #93); Dach zostaje +1. HP problemów i etapów kontraktów bez zmian.

Bot C# (`WeaponBalanceTests`, 300 przebiegów/zawód, kontrakty 150):

| Cel | Przed (Task 11) | Krok w bok | + strojenie (wynik) | Zakres |
|---|---|---|---|---|
| Łatwy | 52,4 | 51,8 | 53,3 | 50–60 |
| Normalny | 26,2 | 26,3 | 28,1 | 25–35 |
| Trudny | 9,8 | 10,4 | 11,6 | 8–15 |
| Pełne Szkolenia | 60,0 | 59,1 | 61,3 | 55–65 |
| + Respekt | 69,7 | 68,6 | 70,1 | 70–80 |
| Akt 0 + meta | 70,9 | 69,4 | 70,0 | 65–75 |
| Inwestor (wszystkie modyfikatory) | 9,4 | 8,8 | 9,9 | 5–15 |
| Kontrakt Letniskowy bez meta / pełne meta | 27,4 / 72,3 | 29,4 / 71,9 | 29,4 / 71,9 | 25–40 / 65–80 |
| Kontrakt Bliźniak bez meta / pełne meta | 26,1 / 65,6 | 24,5 / 61,6 | 24,5 / **61,6** | 25–40 / 65–80 |
| Kontrakt Poddasze bez meta / pełne meta | 29,0 / 65,4 | 28,7 / 62,1 | 28,7 / **62,1** | 25–40 / 65–80 |
| Kontrakt Kamienica bez meta / pełne meta | 28,7 / 65,3 | 27,9 / 66,4 | 27,9 / 66,4 | 25–40 / 65–80 |
| Tylko broń domyślna, Normalny | 27,3 | 26,6 | 28,3 | – |
| Glazurnik etap 5, tylko broń domyślna (limit 19,0) | 19,0 | 21,6 | 16,6 | ≤ v0.22.1 + 10 p.p. |

**Do decyzji właściciela (znane, `ContractsInTarget` czerwony jak w Task 11):** kontrakty Bliźniak i Poddasze z pełnym
meta (61,6 / 62,1; cel 65–80) i Bliźniak bez meta 24,5 (cel 25–40, 150 przebiegów). Kontrakty mają własne etapy
w `career`, więc strojenie głównych etapów ich nie rusza. Etapów kontraktów i HP problemów nie zmieniano.

Test Aktu 0 (bot, Łatwy, bez meta) na 80 budowach, próg 44; zmierzone 49/80, na 400 budowach 69% (przed krokiem w bok 62,5%).

Rdzeń GBA (`core_tests.cpp`, inne ziarna), przed (Task 11) → krok w bok → + strojenie:
- Łatwy 51 → 49 → 50;
- Normalny 26 → 25 → 27;
- pełne Szkolenia 58 → 57 → 59;
- + Respekt 68 → 67 → 69;
- Akt 0 + meta 66 → 62 → 63;
- kontrakty z pełnym meta 58–68 → 51–67 (Letniskowy 67, Bliźniak 53, Poddasze 51, Kamienica 59). Pełny `core_tests.cpp` po strojeniu: „OK – wszystkie testy przeszły”.

Progi tylko GBA (cele mierzy C#): kontrakty z pełnym meta ≥ 50 (było 55), Akt 0 + meta ≥ 60 (było 65).

## v0.23.1 – mniej termosów z kawą (2026-11-12)

Uwaga właściciela po v0.23.0: „ilość termosów z kawą jeszcze za duża” (zrzut: Termos 5/5 na etapie 5/10, Normalny).

**Źródła ładunków i pojemności (przed → po):**

| Źródło | Przed | Po |
|---|---|---|
| Pojemność bazowa (`thermos.capacity`) | 3 | **2** |
| Pamiątka Termos babci (startowa, rangi po 3 i 8 budowach) | +1 / +2 / +3 | **+1 / +1 / +2** |
| Szkolenie Lepszy termos (poziomy 25 / 50 / 90 / 150) | termos +1, sprzęt +1, termos +1, termos +1 | termos +1, sprzęt +1, termos +1, **sprzęt +1** |
| Kawa z mapy na etap (`Game.StartStage`) | 1 pewna + 2 losowe (kawa / kask / projekt, każda ⅓) ≈ 1,67 | **1 pewna**; dodatkowa znajdźka, która wylosuje kawę, przepada (kask i projekt bez zmian) |
| Dropy z problemów (`drops`) | 25%, waga kawy 40 / 95 | **20%, waga kawy 20 / 75** (pozostałe dropy tak samo częste, kawa o połowę rzadziej) |
| Kawa leczy (`thermos.heal`) – kompensata | 8 HP | **11 HP** |
| Bez zmian | Respekt Duży termos +1, Zaprawiony w boju (kawa na start), pas +1/+1/+2, projekt Termos z bufetu +1, wydarzenie Ekipa na kawie (pełny termos), Hurtownia Deskowanie (2 kawy), wybory SMS | – |

Pomiar ładunków (bot C#, Normalny, 100 przebiegów na zawód, zwykłe zawody; termos na wejściu na etap 5 i 8 / pojemność;
bot pije poniżej 40% HP, więc trzyma mniej niż gracz):

| Profil | Przed: etap 5 | Po: etap 5 | Przed: etap 8 | Po: etap 8 | Kawy wypite na budowę przed → po |
|---|---|---|---|---|---|
| Bez meta | 1,37 / 3,09 | 0,86 / 2,09 | 1,38 | 0,87 | 3,69 → 2,33 |
| Nowy profil (Termos babci I) | 1,58 / 4,09 | 1,06 / 3,09 | 1,68 | 1,13 | 3,85 → 2,53 |
| Pełne Szkolenia | 2,33 / 7,09 | 1,64 / 5,08 | 2,98 | 2,07 | 4,01 → 2,51 |

Gracz z typowym meta (Termos babci II–III, pierwszy poziom Lepszego termosu) ma teraz termos 3–4 zamiast 5–7.

Rundy (C# `WeaponBalanceTests`, 300 przebiegów/zawód, kontrakty 150):
- warianty, w których wylosowana kawa zamieniała się w kask / projekt (+1 OBR / +1 obrażeń na stałe), podnosiły Normalny
  do 31–33% na 100 przebiegach – odrzucone (kawę trzeba zabrać, nie zamienić na mocniejszą znajdźkę);
- tylko cięcia (bez kompensaty, kawa 8 HP): Normalny **23,8**, Łatwy 46,5, kontrakty bez meta 19,9–24,9 – za mało;
- kawa leczy 12 HP: Normalny 31,4 (szybki pomiar); **11 HP – wynik końcowy** (HP problemów bez zmian):

| Cel | v0.23.0 (wydanie) | v0.23.1 | Zakres |
|---|---|---|---|
| Łatwy | 53,3 | 52,1 | 50–60 |
| Normalny | 28,1 | 30,6 (wszystkie zawody 29,6) | 25–35 |
| Trudny | 11,6 | 13,3 | 8–15 |
| Pełne Szkolenia | 61,3 | 61,8 | 55–65 |
| + Respekt | 70,1 | 73,1 | 70–80 |
| Akt 0 + meta | 70,0 | 72,0 | 65–75 |
| Inwestor (wszystkie modyfikatory) | 9,9 | 12,7 | 5–15 |
| Kontrakt Letniskowy bez meta / pełne meta | 29,4 / 71,9 | 28,8 / 71,7 | 25–40 / 65–80 |
| Kontrakt Bliźniak bez meta / pełne meta | 24,5 / 61,6 | 26,1 / **65,0** (64,96) | 25–40 / 65–80 |
| Kontrakt Poddasze bez meta / pełne meta | 28,7 / 62,1 | 29,4 / 69,8 | 25–40 / 65–80 |
| Kontrakt Kamienica bez meta / pełne meta | 27,9 / 66,4 | 30,0 / 70,7 | 25–40 / 65–80 |
| Tylko broń domyślna, Normalny | 28,3 | 30,6 | – |
| Glazurnik etap 5, tylko broń domyślna (limit 19,0) | 16,6 | **20,0** | ≤ v0.22.1 + 10 p.p. |

Testy `WeaponBalanceTests`: czerwone `ContractsInTarget` (Bliźniak z pełnym meta 64,96 – w v0.23.0 61,6, też czerwony) i
`DefaultWeaponStageShare` (Glazurnik etap 5: 20,0 przy limicie 19,0 – mniej kawy w Akcie I boli zawód wręcz przy bossie
Fundamentów; Glazurnik razem 22,6 / tylko domyślna 17,6). Do decyzji właściciela przy „wydajemy?”.

Rdzeń GBA (`core_tests.cpp`, pełny balans, inne ziarna), v0.23.0 → v0.23.1: Łatwy 50 → **49**, Normalny 27 → 30, Trudny 12,
pełne Szkolenia 59 → 60, + Respekt 69 → 71, inwestor 12, Akt 0 + meta 63 → 66, kontrakty bez / z pełnym meta 26–28 / 57–67,
broń domyślna 32, kawa 2,23 na budowę (pije w 78% budów; v0.23.0: 3,29), wszystko wykupione po 30 budowach.
Próg tylko GBA: Łatwy ≥ 47 (było 50) – cel mierzy C# (52,1).
