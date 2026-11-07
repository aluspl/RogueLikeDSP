# Sprite'y broni (v0.23, TODO #84–#86)

Generowane skryptem `GBA/tools/weapon_sprites.py` z map znaków (pixel art w kodzie) i aktualnej listy broni
z `GBA/data/game.json` (`weapons`). Bez nazw i barw cudzych marek.

```
python3 GBA/tools/weapon_sprites.py      # z katalogu repozytorium, wymaga Pillow
```

Wiersz w arkuszach = kolejność broni w `game.json` (brakujące noże z listy zapasowej na końcu);
mapowanie wiersz → id, kierunek, rzadkość, chwyt i animacje jest w `weapons.json`.

## Pliki i rozmiary

| Plik | Klatka | Układ |
|---|---|---|
| `weapons_inhand.png` | 16×16 (skala GBA) | wiersz = broń; kolumna = `rzadkość*4 + kierunek` (16 kolumn) |
| `weapons_inhand_2x.png` | 32×32 (Godot, Scale2x jak `actors.png`) | jw. |
| `weapons_icons.png` | 16×16 (HUD, `Assets.Icon`) | wiersz = broń; kolumna = rzadkość (4) |
| `weapons_icons_2x.png` | 32×32 (menu/telefon, Scale2x) | jw. |
| `weapons_swing.png` | 32×32 (skala GBA) | wiersz = `swingRows[i]` (broń × Markowa/Mistrzowska); kolumna = klatka 0–5 |
| `weapons_swing_2x.png` | 64×64 (Godot) | jw.; do ogłoszeń rysować dalej ×2–×4 bez wygładzania |
| `weapons.json` | – | manifest (rozmiary, palety, wiersze, chwyty, rodzina animacji) |
| `preview.png`, `preview_deutan.png`, `preview_swing.png` | – | podgląd (nie do gry), symulacja deuteranopii |

- **Kierunki** (kolejność kolumn): `down`, `up`, `left`, `right`. Mapa broni jest rysowana w prawo
  (rękojeść po lewej); `left` = odbicie, `up`/`down` = obrót o 90°. Przedmioty bez obrotu (dziennik, tablet,
  dachówki, lasery na statywie) mają w `up`/`down` tę samą klatkę co `right`.
- **Rzadkość** (kolejność kolumn): Zwykła, Solidna, Markowa, Mistrzowska. Bronie Mistrzowskie
  (`master: true` – Młot Zenka, Poziomica mistrza, Pilarka tarczowa, Nóż majstra seniora) mają tylko kolumny
  Mistrzowskiej; pozostałe pola są przezroczyste.
- **Chwyt**: `grip[kierunek]` = piksel rękojeści w klatce 16×16 (`gripGodot` w 32×32). Postawić klatkę tak, by
  ten piksel wypadł na prawej ręce bohatera (w `actors.png` ok. piksel 13,11 GBA / 26,22 Godot). Przy `up`
  broń rysować pod postacią, w pozostałych kierunkach nad.
- **Ikony**: narzędzia ręczne obrócone o 45° (RotSprite), elektronarzędzia i przedmioty bokiem. Znacznik
  niezależny od koloru w lewym dolnym rogu: Solidna 1 kropka, Markowa 2, Mistrzowska iskra.
- **Zamach** (6 klatek, ~60–80 ms/klatkę): rodziny `swing` (cięcie łukiem ze smugą), `slam` (zamach znad głowy,
  gwiazdka uderzenia), `stab` (pchnięcie), `shoot` (odrzut + pocisk: gwóźdź, kotwa, zszywka, silikon, dachówka),
  `beam` (wiązka lasera / impuls prądu / „znacznik” dziennika), `spray` (stożek: tynk, woda, płomień, gorące
  powietrze, iskry). Chwyt w klatce zamachu: piksel (11,20) GBA.

## Paleta (16 kolorów, zgodna z GBA 4bpp)

Indeksy 0–11 są stałe, 12–15 zależą od rzadkości – na GBA wystarczy jedna paleta sprite'a podmieniana
przy zmianie broni, w Godocie są gotowe PNG.

| Idx | Znak | Kolor | Idx | Znak | Kolor |
|---|---|---|---|---|---|
| 0 | `.` | przezroczysty | 8 | `R` | czerwony (laser, dachówka) |
| 1 | `K` | obrys | 9 | `Y` | żółty (iskry, bańka) |
| 2 | `W` | biel | 10 | `C` | cyjan (szkło, woda) |
| 3 | `l` | stal jasna | 11 | `O` | pomarańcz (płomień, nóż majstra) |
| 4 | `g` | stal | 12 | `H` | obudowa główna |
| 5 | `d` | stal ciemna / guma | 13 | `h` | obudowa druga |
| 6 | `T` | drewno | 14 | `A` | kolor rzadkości (poświata, akcenty) |
| 7 | `t` | jasne drewno | 15 | `*` | jasny akcent (iskra Mistrzowskiej) |

| Rzadkość | Obudowa H + h (elektronarzędzia) | A (poświata = `weaponRarities.color`) | `*` |
|---|---|---|---|
| Zwykła | ciemna zieleń `#2c683a` + czerń `#242428` | `#8a8796` | `#c4c2ce` |
| Solidna | morski `#2096a4` + głęboki morski `#124860` | `#2f8fd6` | `#96cef6` |
| Markowa | żółty `#f6c020` + czerń `#242428` | `#6b4eff` | `#b2a0ff` |
| Mistrzowska | czerwień `#d62c28` + biel `#f6f6f6` | `#d9a400` | `#fff4a0` |

Zasady: elektronarzędzia (`kind: power`) mają obudowę w kolorach rzadkości; narzędzia ręczne (`kind: hand`)
zostają z drewna/stali/gumy, a kolor rzadkości trafia na pasek rękojeści, owijkę lub okładkę. Wszystko ma
obrys `K` i 1 px poświaty `A`; Mistrzowska dodatkowo iskrę `*`. Nóż majstra seniora: pomarańczowa, wytarta
rękojeść z wybrzuszeniem pod palec, cienkie zeszlifowane ostrze wygięte ku górze (wg zdjęcia właściciela,
zdjęcie nie jest w repo).

## Nowa broń

1. Dopisz mapę `weapon("id", "power"|"hand", rodzina, (x, y) chwytu, [wiersze...])` w `weapon_sprites.py`
   (max 12×9 pól, żeby z obrysem i poświatą zmieścić się w 16×16; skierowana w prawo).
2. Uruchom skrypt, obejrzyj `preview.png` (i `preview_deutan.png`), popraw mapę.
3. Broń z `game.json` bez mapy jest pomijana z ostrzeżeniem na stderr.
