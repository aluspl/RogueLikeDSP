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
