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
