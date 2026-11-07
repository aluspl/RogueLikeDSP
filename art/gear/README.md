# Sprzęt na bohaterze (v0.23, TODO #88)

Generowane skryptem `GBA/tools/gear_sprites.py` z map znaków (pixel art w kodzie) i listy sprzętu z
`GBA/data/game.json` (`equipment.slots`, `equipment.rarities`). Bez nazw i barw cudzych marek.

```
python3 GBA/tools/gear_sprites.py      # z katalogu repozytorium, wymaga Pillow
```

## Pliki

| Plik | Klatka | Układ |
|---|---|---|
| `gear_overlay.png` | 16×16 (GBA) | wiersz = `slot*3 + rzadkość`; kolumna = `kierunek*2 + klatka` (A, B jak `actors.bmp`) |
| `gear_overlay_gba.bmp` | 16×16 | to samo, indeksowane 4bpp z paletą `gear.json` → `palette` (osobny bank palety sprite'a) |
| `gear_overlay_2x.png` | 32×32 (Godot) | wiersz jw.; kolumna = `kierunek*5 + klatka` (walkA, walkAstep, walkB, walkBstep, breath – jak `actors_anim.png`) |
| `gear.json` | – | manifest: wiersze → przedmiot, kolumny, kolejność rysowania, przesunięcia per zawód, paleta, kafle GBA |
| `preview.png`, `preview_deutan.png` | – | 12 zawodów × (bez sprzętu + 3 rzadkości), kierunki, chód i oddech; symulacja deuteranopii |

- **Sloty** (kolejność jak `equipment.slots`): `kask`, `rekawice`, `kamizelka`, `buty`, `pas`; **rzadkości**: Zwykły,
  Solidny, Markowy (= kolejność `items` w slocie).
- **Kierunki**: `down`, `up`, `left`, `right`. Bohater nie ma klatek kierunkowych – `down` = `up` = `right`,
  `left` = odbicie lustrzane. Gdy nakładka jest rysowana tą samą transformacją co bohater (`ActorSprite.Flip`),
  wystarczy kolumna `down`.
- **Klatki**: Godot – statyczna klatka A = `walkA`, B = `walkB`; krok i oddech przesuwają wiersze dokładnie jak
  `export_godot_assets.py`, więc nakładka idzie z sylwetką.
- **Kolejność rysowania**: bohater → buty → kamizelka → pas → rękawice → kask → broń w ręce. Przy kierunku `up`
  broń pod bohaterem i sprzętem (`art/weapons/README.md`).
- **Zawody**: wszystkie 12 mają tę samą sylwetkę (`pixel_art.WORKER_TOP` + nogi A/B) – przesunięcia 0 (skrypt to
  sprawdza). Na GBA (16×16, bez wygładzania) nakładka pasuje co do piksela. W Godocie Scale2x złożonej klatki
  zaokrągla kilka subpikseli na granicy z pikselami zależnymi od zawodu (narzędzie przy dłoni, przyłbica spawacza,
  tyczka geodety): `verifyGodotMismatchPixels` w `gear.json` – 0–105 subpikseli na zawód łącznie dla 15 nakładek
  × 5 klatek (1–2 px w klatce 32×32, niewidoczne w grze).
- Rękawica na prawej dłoni zasłania piksel narzędzia zawodu wrysowanego w `actors.png` (`toolCoversHand`).

## Wygląd

| | Zwykły | Solidny | Markowy |
|---|---|---|---|
| Kask | czapka w kolorze zawodu, jasny obrys skorupy i szare rondo | morski z latarką na czole, pasek w kolorze czapki zawodu | żółty, czarny grzebień i rondo, pasek w kolorze czapki zawodu |
| Rękawice | jasnoszare | morskie | żółte, czarny mankiet |
| Kamizelka (rozpięta – środek w kolorze zawodu) | tylko srebrne pasy odblaskowe na koszuli zawodu | morska pikowana | żółta, biały pas odblaskowy, czarny dół |
| Buty | tylko brązowe podeszwy | morskie, stalowy podnosek | czarne, żółty nosek |
| Pas | skórzany, metalowa sprzączka | morski z kaburą z prawej | czarny, szeroka żółta sprzączka |

v0.23.0 (task 9c, decyzja koordynatora): **Zwykły sprzęt nie przemalowuje postaci** – kolory zawodu (koszula, czapka,
spodnie) zostają, dochodzą tylko elementy. Solidny i Markowy przemalowują, ale kask zostawia pasek w kolorze czapki
zawodu (przezroczyste piksele w wierszu 3 nakładki – widać kolor bohatera), więc zawód da się rozpoznać. Odznaka koloru
kasku (`HelmetTint`) i kask w paski działają tylko bez kasku-sprzętu (z kaskiem gra rysuje zwykłą klatkę zawodu).
Nakładka Godota zawiera tylko barwy nakładki i obrys – kolor zawodu nigdy do niej nie trafia (inaczej pasek kasku
zawodu wzorcowego pokazałby się u innych).

## Paleta (16 kolorów, GBA 4bpp)

`.` 0 przezroczysty, `K` obrys, `W` biel, `l` jasnoszary, `g` szary, `d` ciemnoszary, `G`/`F` szarozielony + cień,
`S`/`s`/`Z` morski / głęboki morski / blik, `Y`/`y` żółty + cień, `N` czerń, `T`/`t` brąz + jasny brąz.
Wartości RGB w `gear.json` → `palette`.

## GBA – limit kafli

Nakładka 16×16 = 4 kafle 8×8. Kask zajmuje tylko 2 górne kafle, reszta (rękawice, kamizelka, pas, buty) 2 dolne:

| Wariant | Kafle VRAM (bohater) | Sprite'y (OAM) |
|---|---|---|
| Wszystkie sloty złożone w jeden sprite 16×16 (CPU składa 5 nakładek przy zmianie sprzętu/klatki) | **4** | +1 |
| Kask 16×8 + dół 16×8 (dół złożony z 4 slotów) | 4 | +2 |
| Każdy slot osobnym sprite'em 16×16 | 20 | +5 |
| Tylko kask + kamizelka (osobne 16×8) | 4 | +2 |

Sprzęt nosi tylko bohater, więc łącznie +4 kafle i +1 sprite przy wariancie złożonym – mieści się w limicie 256
kafli i 128 sprite'ów. Nakładki w ROM: 15 wierszy × 2 klatki × 4 kafle = 120 kafli (ładowane do VRAM tylko
bieżące). Potrzebny osobny bank palety sprite'a (16 kolorów `palette`).
