# v0.23.2 – minimalistyczny, czytelny interfejs (Godot)

Uwagi właściciela po teście v0.23.2 (2026-11-13): interfejs telefonu „mega nieczytelny”, UX/UI i flow pauzy
„tragiczne”. Kierunek: **minimalistyczny i czytelny**, wolno się inspirować innymi grami (ekran postaci i plecaka).
Zakres: tylko Godot (GBA ma własne, ograniczone UI – bez zmian). Wchodzi do v0.23.2 jako poprawka rażącego błędu.

Inspiracje: pauza z Hades / Dead Cells (przyciemniony ekran, kilka dużych przycisków), ekran postaci jako
„paperdoll” (Diablo, Dead Cells, Brotato – bohater w środku, sloty dookoła), plecak jako siatka dużych kafli
z kolorową ramką rzadkości (Brotato, Slay the Spire – szczegóły dopiero po dotknięciu), karta przedmiotu jako
dolny arkusz z 1–3 najważniejszymi liczbami i porównaniem +/−.

## Zasady (dla wszystkich ekranów)
1. **Czytelność:** większy tekst (w pionie na telefonie co najmniej ~1,5× obecnego), więcej powietrza między
   wierszami, jedna kolumna. Treść wypełnia ekran zamiast ściskać się u góry.
2. **Hierarchia:** na każdym ekranie jedna rzecz najważniejsza (duża), reszta drobniej albo pod „Więcej”.
   Bez zrzutu wszystkich statystyk naraz; skróty typu „SIŁ 3 ZRĘ 5 INT 2 SZCZ 4” tylko w szczegółach.
3. **Kolor:** neutralne tło + jeden kolor akcentu + kolory rzadkości. Bez kilku kolorowych etykiet w jednym wierszu.
4. **Dotyk:** na dotyku żadnych podpowiedzi klawiatury („Spacja”, „Tab”, „Enter/Z”, „Strzałki”, „Q/E”) –
   tylko na desktopie. Cele dotyku ≥ 48 px, duży przycisk „Zamknij/Wróć” w stałym miejscu.
5. **Spójność:** jeden sposób otwierania każdej rzeczy; dolne przyciski zawsze: Wróć (lewo) / główna akcja (prawo).

## Ekrany
- **Pauza** (przycisk pauzy / Esc / Start): gra stoi, przyciemnienie, 4–5 dużych przycisków jeden pod drugim:
  **Wznów**, **Postać** (sprzęt + plecak), **Budowa** (zadania, usterki, koszty – dotychczasowy telefon),
  **Ustawienia**, **Wyjdź do menu** (z potwierdzeniem). Na górze tylko etap i HP.
- **Postać (paperdoll):** bohater na środku, dookoła sloty: broń w ręku, kabura, kask, kamizelka, rękawice, buty,
  pas. Pod spodem 4 kluczowe liczby (HP, Obrona, Obrażenia, Kryt), „Więcej statystyk” rozwija resztę.
  Niżej **Plecak** – siatka 6 dużych kafli (3×2) z ikoną i ramką rzadkości, ▲/▼ na rogu kafla, gdy lepsze/słabsze.
- **Karta przedmiotu** (dotknięcie kafla lub slotu): dolny arkusz – nazwa, rzadkość, 1–3 liczby z porównaniem
  do noszonego (+2 zielone, −1 czerwone), cecha jednym zdaniem; przyciski: **Załóż** (1 tura na budowie) /
  **Wyrzuć**; w Hurtowni także **Sprzedaj** (cena) i **PSZOK**. Pełna rozpiska obrażeń pod „Szczegóły”.
- **Karta znaleziska** (paczka/broń z mapy): ten sam układ co karta przedmiotu: nowy vs noszony,
  przyciski **Załóż** / **Do plecaka (n/6)** / **Zostaw**.
- **Budowa (telefon):** zostaje motyw smartfona i aplikacji, ale mniej treści na ekran: zakładki Zadania,
  Usterki, Koszty z większymi wierszami; Start i Sprzęt zastępuje ekran Postać.
- **Hurtownia:** zakładki zostają (Zakupy / Skup / PSZOK / Plecak), ale wiersze i kafle w tym samym stylu.

## Poza zakresem
- GBA, logika gry i balans (bez zmian). Teksty nowe lub zmienione tylko w game.json / en.json.
