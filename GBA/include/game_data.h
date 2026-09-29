// WYGENEROWANE przez tools/gen_data.py z data/game.json - nie edytuj ręcznie.
#pragma once
#include "core_types.h"

namespace data {

inline constexpr core::weapon_def weapons[] = {
    { "Dziennik budowy", 2, 4, 2, core::stat::intel, core::element::none },
    { "Kielnia", 4, 6, 1, core::stat::str, core::element::none },
    { "Gwoździarka", 2, 3, 3, core::stat::agi, core::element::none },
    { "Próbnik napięcia", 3, 5, 2, core::stat::intel, core::element::power },
    { "Klucz nastawny", 4, 7, 1, core::stat::str, core::element::none },
    { "Szlifierka", 4, 7, 1, core::stat::agi, core::element::spark },
    { "Łom", 3, 6, 1, core::stat::str, core::element::none },
    { "Wkrętarka", 2, 5, 2, core::stat::agi, core::element::none },
    { "Poziomica laserowa", 2, 4, 4, core::stat::intel, core::element::none },
    { "Młot wyburzeniowy", 5, 8, 1, core::stat::str, core::element::none },
    { "Tablet z projektem", 3, 5, 2, core::stat::intel, core::element::none },
    { "Miernik laserowy", 2, 5, 3, core::stat::intel, core::element::none },
    { "Dachówki", 2, 3, 3, core::stat::agi, core::element::none },
    { "Agregat tynkarski", 2, 4, 2, core::stat::str, core::element::none },
    { "Łyżka koparki", 4, 7, 1, core::stat::str, core::element::none },
    { "Młot udarowy", 5, 9, 1, core::stat::str, core::element::none },
    { "Pistolet do kotew", 3, 5, 3, core::stat::agi, core::element::spark },
};

inline constexpr core::class_def classes[] = {
    { "Kierownik budowy", "Trzyma harmonogram w ryzach.", 30, 3, 3, 5, 3, 2, 0, 0, "Odprawa", "Ogłusza widocznych", core::ability_effect::stun, 15, core::class_passive::none },
    { "Murarz", "Twardy jak pustak.", 34, 5, 2, 1, 4, 0, 1, 1, "Ścianka", "Mur przed wrogiem", core::ability_effect::wall, 15, core::class_passive::none },
    { "Cieśla-dekarz", "Gwoździe wbija z daleka.", 30, 4, 4, 1, 3, 2, 2, 2, "Seria", "Gwoździe we wszystkich", core::ability_effect::volley, 12, core::class_passive::none },
    { "Elektryk", "Wie, gdzie jest faza.", 24, 2, 4, 5, 2, 1, 3, 3, "Łańcuch", "Prąd skacze po celach", core::ability_effect::chain, 12, core::class_passive::none },
    { "Hydraulik", "Żaden przeciek mu nie straszny.", 34, 4, 3, 3, 3, 2, 4, 4, "Zawór", "Odpycha wrogów i leczy", core::ability_effect::flush, 20, core::class_passive::none },
    { "Glazurnik", "Precyzja co do fugi.", 30, 3, 5, 2, 3, 4, 5, 5, "Wirówka", "Tnie wszystkich dookoła", core::ability_effect::spin, 10, core::class_passive::none },
    { "Dekarz", "Wiatr mu nie przeszkadza.", 26, 3, 5, 2, 2, 3, 12, 52, "Rynna", "Bije całą linię", core::ability_effect::line, 12, core::class_passive::windproof },
    { "Tynkarz", "Tynk na cały pokój.", 30, 4, 3, 2, 3, 2, 13, 53, "Narzut", "Tynk na obszar 3x3", core::ability_effect::splash, 12, core::class_passive::none },
    { "Operator koparki", "Wolny, ale pcha wszystko.", 32, 5, 1, 1, 4, 0, 14, 54, "Taran", "Szarża i odepchnięcie", core::ability_effect::ram, 16, core::class_passive::push },
};

inline constexpr core::enemy_def enemies[] = {
    { "Przeciek", "Kapie tam, gdzie nie powinno", 6, 1, 3, 0, 7, 10, 6, false, core::status_effect::slip, 14, 2, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 0, 0, 0, 0, 0, "", core::element::water, 0 },
    { "Zwarcie", "Iskrzy przy każdej okazji", 5, 2, 4, 0, 8, 12, 7, false, core::status_effect::shock, 17, 1, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 1, 0, 0, 0, 0, "", core::element::power, 2 },
    { "Pleśń", "Lubi wilgoć i zimne ściany", 9, 1, 2, 1, 5, 10, 8, false, core::status_effect::poison, 24, 3, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 2, 0, 0, 0, 0, "", core::element::none, 1 },
    { "Kornik", "Drąży więźbę po cichu", 7, 1, 3, 1, 6, 10, 9, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 2, 0, 0, 0, 0, "", core::element::none, 0 },
    { "Papierologia", "Brakuje jednej pieczątki", 12, 1, 2, 2, 5, 15, 10, false, core::status_effect::paper, 35, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 2, 0, 0, 0, 0, "", core::element::none, 1 },
    { "Opóźniona dostawa", "Będzie jutro. Na pewno.", 10, 2, 4, 1, 7, 15, 11, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 1, 0, 0, 0, 0, "", core::element::none, 1 },
    { "Ulewa", "Zawsze tuż przed dachem", 8, 2, 3, 0, 9, 12, 12, false, core::status_effect::slip, 28, 3, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 0, 0, 0, 0, 0, "", core::element::water, 1 },
    { "Przekroczony budżet", "Rośnie szybciej niż mury", 14, 2, 5, 2, 7, 25, 13, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 1, 0, 0, 0, 0, "", core::element::none, 0 },
    { "Nieprzekraczalny Termin", "Nieprzesuwalny. Podobno.", 42, 4, 6, 3, 12, 200, 14, true, core::status_effect::paper, 28, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", -1, 0, 0, 0, 0, "", core::element::none, 0 },
    { "Zepsuta Betoniarka", "Kręci się, ale nie tam", 28, 3, 5, 2, 10, 150, 46, true, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", -1, 0, 0, 0, 0, "", core::element::none, 1 },
    { "Nawałnica", "Leje jak z cebra", 32, 3, 5, 2, 12, 180, 47, true, core::status_effect::slip, 35, 3, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", -1, 0, 0, 0, 0, "", core::element::water, 1 },
    { "Inspekcja Pracy", "Sprawdza kask i barierki", 30, 3, 5, 2, 12, 170, 50, true, core::status_effect::paper, 30, 0, core::slam_shape::cross, "Kontrola BHP", 4, 6, 2, 2, 60, "Protokół bez uwag", -1, 0, 0, 0, 0, "", core::element::none, 1 },
    { "Woda gruntowa", "Wybija tam, gdzie kopiesz. I obok", 8, 1, 2, 0, 6, 12, 61, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 0, 2, 0, 0, 0, "", core::element::water, 1 },
    { "Kamień w wykopie", "Nie ruszy się. Koparka też nie", 16, 2, 3, 3, 4, 14, 62, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 0, 64, 0, 0, 0, "", core::element::none, 0 },
    { "Osuwisko skarpy", "Zjeżdża prosto na ciebie", 10, 1, 3, 1, 6, 12, 63, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 0, 128, 0, 0, 0, "", core::element::none, 2 },
    { "Dziurawa folia", "Załatana? Tylko tak wygląda", 7, 1, 2, 0, 6, 12, 64, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 1, 256, 0, 0, 0, "", core::element::water, 1 },
    { "Krzywy mur", "Pion? Jaki pion?", 12, 1, 3, 2, 5, 14, 65, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 0, 192, 0, 0, 0, "", core::element::none, 0 },
    { "Mostek termiczny", "Ucieka ciepło. I sam ucieka", 7, 1, 2, 0, 8, 14, 66, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 1, 33, 0, 0, 0, "", core::element::none, 0 },
    { "Ugięcie stropu", "Z każdym dniem trochę niżej", 12, 1, 3, 1, 5, 15, 67, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 1, 80, 0, 0, 0, "", core::element::none, 2 },
    { "Brak zbrojenia", "Beton bez stali pęka na pół", 10, 2, 3, 1, 6, 14, 68, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 1, 2, 0, 0, 0, "", core::element::none, 0 },
    { "Przeciekająca papa", "Kapie z góry, i to celnie", 8, 1, 3, 0, 8, 14, 69, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 2, 1, 0, 0, 0, "", core::element::water, 1 },
    { "Zapchana rynna", "Zaraz się przeleje. Na ciebie", 9, 1, 2, 0, 6, 14, 70, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 1, 8, 0, 0, 0, "", core::element::none, 1 },
    { "Pęknięty pustak", "Rozsypie się przy pierwszej okazji", 8, 2, 3, 1, 6, 14, 71, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 0, 8, 0, 0, 0, "", core::element::none, 0 },
    { "Zła wymiarówka", "Im dłużej, tym gorzej pasuje", 10, 1, 3, 1, 6, 15, 72, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 2, 16, 0, 0, 0, "", core::element::none, 1 },
    { "Nieszczelna ramka", "Wieje z każdej szczeliny", 13, 1, 3, 2, 7, 15, 73, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 2, 65, 0, 0, 0, "", core::element::none, 1 },
    { "Przeciąg", "Drzwi trzaskają same", 8, 1, 3, 0, 8, 14, 74, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 2, 160, 0, 0, 0, "", core::element::none, 0 },
    { "Zapowietrzenie", "Odpowietrzysz, a ono wraca", 8, 1, 3, 0, 7, 14, 75, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 1, 288, 0, 0, 0, "", core::element::none, 2 },
    { "Brak uziemienia", "Dotknij obudowy. Albo nie", 9, 2, 3, 0, 7, 15, 76, false, core::status_effect::shock, 12, 1, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 1, 8, 0, 0, 0, "", core::element::power, 0 },
    { "Rysa skurczowa", "Jedna rysa, dwie rysy, dziesięć", 10, 1, 3, 1, 6, 14, 77, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 0, 2, 0, 0, 0, "", core::element::none, 1 },
    { "Wilgoć w ścianie", "Karmi pleśń i sąsiadów", 11, 1, 2, 1, 6, 15, 78, false, core::status_effect::poison, 15, 2, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 0, 4, 0, 0, 0, "", core::element::water, 1 },
    { "Odpryski płytek", "Lecą przy każdym cięciu", 9, 2, 3, 0, 8, 15, 79, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 0, 1, 0, 0, 0, "", core::element::none, 2 },
    { "Poprawki na odbiorze", "Zamknięte? Inspektor ma inne zdanie", 12, 2, 4, 1, 7, 18, 80, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 1, 256, 0, 0, 0, "", core::element::none, 2 },
    { "Brakujący podpis", "Uciekł, zanim ktoś podpisał", 7, 1, 2, 0, 7, 12, 101, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 2, 32, 0, 0, 0, "", core::element::none, 0 },
    { "Zaginiony wniosek", "Zgubiony. Znajdzie się. Dwa razy", 8, 1, 2, 0, 6, 12, 102, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 2, 256, 0, 0, 0, "", core::element::none, 0 },
    { "Termin na odwołanie", "Tyka i rośnie z każdym dniem", 9, 1, 2, 0, 6, 13, 103, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 2, 16, 0, 0, 0, "", core::element::none, 0 },
    { "Niezgodność z planem", "Rzuca uwagami z daleka", 7, 1, 2, 0, 8, 13, 104, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 2, 1, 0, 0, 0, "", core::element::none, 1 },
    { "Pieczątka nie ta", "Stempluje i odsyła na początek", 10, 1, 3, 1, 5, 13, 105, false, core::status_effect::paper, 20, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 2, 128, 0, 0, 0, "", core::element::none, 1 },
    { "Pęknięta rura", "Jedna dziura, zaraz dwie", 8, 1, 2, 0, 6, 12, 106, false, core::status_effect::slip, 15, 2, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 1, 2, 0, 0, 0, "", core::element::water, 1 },
    { "Brak ciśnienia", "Stoi i dopompowuje innych", 12, 1, 2, 2, 4, 13, 107, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 1, 68, 0, 0, 0, "", core::element::none, 0 },
    { "Kolizja z kablem", "Koparka trafiła w kabel. Iskry!", 7, 1, 3, 0, 7, 13, 108, false, core::status_effect::shock, 12, 1, core::slam_shape::square, "", -1, 0, 0, 0, 0, "", 1, 8, 0, 0, 0, "", core::element::power, 1 },
    { "Decyzja odmowna", "Stos pism z pieczątką ODMOWA", 34, 3, 5, 2, 12, 140, 109, true, core::status_effect::paper, 25, 0, core::slam_shape::square, "Stempel ODMOWA", 33, 6, 3, 0, 40, "Pozwolenie wydane", -1, 0, 50, 30, 1, "Odwołanie", core::element::none, 1 },
};

inline constexpr core::stage_def stages[] = {
    { "Pozwolenie", { 32, 34, 35, 36 }, 4, 6, -1, 130, 2, 3, 15 },
    { "Przyłącza", { 37, 38, 39, 33 }, 4, 6, 40, 134, 1, 3, 28 },
    { "Fundamenty", { 12, 13, 0, 3 }, 4, 5, -1, 107, 0, 0, 48 },
    { "Izolacja fundamentów", { 14, 15, 12, 0 }, 4, 5, -1, 117, 0, 0, 22 },
    { "Mury parteru", { 16, 17, 4, 5 }, 4, 6, -1, 126, 0, 0, 62 },
    { "Strop", { 18, 19, 5, 7 }, 4, 6, 9, 138, 1, 0, 36 },
    { "Dach", { 20, 21, 6, 3 }, 4, 8, -1, 150, 2, 1, 55 },
    { "Ściany działowe", { 22, 23, 4, 5 }, 4, 7, -1, 159, 3, 1, 30 },
    { "Okna i drzwi", { 24, 25, 6, 5 }, 4, 8, 10, 169, 4, 1, 41 },
    { "Instalacje", { 26, 27, 1, 0 }, 4, 8, 11, 181, 5, 2, 46 },
    { "Tynki i wylewki", { 28, 29, 2, 0 }, 4, 9, -1, 194, 5, 2, 38 },
    { "Wykończenie i odbiór", { 30, 31, 2, 7 }, 4, 7, 8, 207, 5, 2, 72 },
};

inline constexpr core::difficulty_def difficulties[] = {
    { "Łatwy", 100, -1, 50 },
    { "Normalny", 100, 0, 100 },
    { "Trudny", 145, 0, 150 },
};

inline constexpr core::upgrade_def upgrades[] = {
    { "Kondycja", "+2 HP na start", core::upgrade_effect::hp, 2, 2, { 10, 20, 0, 0 }, 35, 0 },
    { "Szkolenie BHP", "-2% otrzymanych obrażeń", core::upgrade_effect::taken_pct, 2, 1, { 15, 0, 0, 0 }, 30, 15 },
    { "Kurs fachowy", "+2% obrażeń", core::upgrade_effect::dmg_pct, 2, 1, { 20, 0, 0, 0 }, 40, 20 },
    { "Lepszy termos", "Kawa leczy +1 HP", core::upgrade_effect::coffee, 1, 1, { 10, 0, 0, 0 }, 20, 10 },
    { "Dostawy", "+1 znajdźka na etap", core::upgrade_effect::pickups, 1, 1, { 15, 0, 0, 0 }, 30, 0 },
    { "Kurs BHP II", "+1 szczęścia", core::upgrade_effect::luck, 1, 1, { 20, 0, 0, 0 }, 40, 0 },
    { "Warsztaty", "+1 do statystyki broni", core::upgrade_effect::craft, 1, 1, { 20, 0, 0, 0 }, 40, 0 },
};

inline constexpr core::story_msg story_stages[] = {
    { "Anna Nowak", { "Najpierw papiery! Mapa,", "warunki zabudowy,", "pozwolenie. Bez tego nic." } },
    { "Kierownik Marek", { "Pozwolenie prawie jest.", "Teraz przyłącza: woda,", "prąd, kanaliza. Uważaj!" } },
    { "Anna Nowak", { "Działka nasza! Liczę na", "mocne fundamenty. Uważaj", "na wodę i błoto w dole." } },
    { "Kierownik Marek", { "Fundament stoi. Teraz", "folia i izolacja - bez", "dziur, bo woda wróci." } },
    { "Kierownik Marek", { "Stal jest, papierów brak.", "Papierologia już czeka.", "Trzymaj się planu!" } },
    { "Kierownik Marek", { "Szalunki stoją, beton", "jedzie. Oby dostawa", "dojechała na czas!" } },
    { "Anna Nowak", { "Prognoza: ulewa. Zdążysz", "z dachem przed deszczem?", "Kornik też nie śpi." } },
    { "Anna Nowak", { "Dach jest! Teraz ściany", "działowe - chcę duży", "salon i małą spiżarnię." } },
    { "Kierownik Marek", { "Stan surowy zamknięty!", "Okna na wymiar, drzwi", "też. Ulewa nie odpuszcza." } },
    { "Kierownik Marek", { "Inspekcja Pracy w drodze!", "Kask, szelki, barierki -", "papiery też sprawdzą." } },
    { "Kierownik Marek", { "Tynki schną tydzień.", "Pleśń tylko na to czeka.", "Wietrz i nie odpuszczaj." } },
    { "Anna Nowak", { "Już widzę nasz salon.", "Tylko ten Termin... Dasz", "radę, mamy harmonogram!" } },
};
inline constexpr core::story_msg story_win = { "Anna Nowak", { "Mamy klucze! Plan", "pokonał chaos budowy.", "Dziękujemy za wszystko!" } };
inline constexpr core::story_msg story_lose = { "Kierownik Marek", { "Budowa stoi. Spokojnie -", "z lepszym planem pójdzie.", "Wracamy na plac?" } };
inline constexpr core::story_msg story_ngplus = { "Anna Nowak", { "Znajomi też chcą dom.", "Bierzesz kolejną budowę?", "Termin już się szykuje." } };
inline constexpr core::story_msg story_prologue = { "Anna Nowak", { "Witaj na budowie! Plan", "jest, ekipa też. Tylko", "te problemy... Do dzieła!" } };
inline constexpr const char* prologue_captions[] = { "Działka przy ul. Budowlanej 7...", "...a problemy już czekają." };

inline constexpr core::act_def acts[] = {   // mechanika aktu: błoto, porywy wiatru, pył
    { "Stan surowy", 10, 2, core::act_mechanic::mud, 14, "Błoto w wykopie", "Błoto", "Wejście w błoto = tura", "I", false },
    { "Pod dachem", 10, 2, core::act_mechanic::gust, 6, "Porywy na wysokości", "Porywy", "Poryw co 6 tur spycha", "II", false },
    { "Wykończenie", 10, 2, core::act_mechanic::dust, 2, "Pył z szlifowania", "Pył", "Pył: widzenie -2", "III", false },
    { "Papierologia", 4, 2, core::act_mechanic::stamps, 3, "Pieczątki i kolejka", "Pieczątki", "3 dokumenty = schody", "0", true },
};
inline constexpr int prelude_stages = 2;   // etapy aktu wstępnego (Akt 0) - z nagrody za odbiór
inline constexpr const char* documents[] = { "Podpis", "Mapa", "Uzgodnienie" };   // pieczątki: dokumenty etapu
inline constexpr int documents_count = 3;
inline constexpr int behavior_ranged_reach = 3;
inline constexpr int behavior_split_hp_pct = 50;
inline constexpr int behavior_heal_value = 3;
inline constexpr int behavior_heal_every = 2;
inline constexpr int behavior_blast_damage = 4;
inline constexpr int behavior_blast_radius = 1;
inline constexpr int behavior_blast_delay = 2;
inline constexpr int behavior_grow_every = 4;
inline constexpr int behavior_grow_hp = 2;
inline constexpr int behavior_grow_max = 4;
inline constexpr int behavior_flee_cooldown = 3;
inline constexpr int behavior_return_turns = 4;
inline constexpr int behavior_return_hp_pct = 50;
inline constexpr int behavior_push_cooldown = 3;
inline constexpr const char* behavior_names[] = { "strzela z dystansu", "dzieli się", "łata innych", "wybucha", "rośnie", "ucieka", "nie rusza się", "odpycha", "wraca raz" };   // indeks = bit zachowania
inline constexpr core::shop_item_def hurtownia[] = {
    { "Ulepsz narzędzie", "+1 obrażeń (maks. +3), od +2 cecha", 0, core::shop_effect::upgrade, -1, 0 },
    { "Kawa z ekspresu", "Pełne HP", 30, core::shop_effect::heal, -1, 0 },
    { "Paczka sprzętu", "Losowy sprzęt, min. solidny", 50, core::shop_effect::gear, -1, 0 },
    { "Nowe narzędzie", "Losowe odblokowane narzędzie", 40, core::shop_effect::tool, -1, 0 },
    { "Siłownia", "+3 max HP na tę budowę", 45, core::shop_effect::maxhp, -1, 0 },
    { "Energetyk", "Moc od razu gotowa", 20, core::shop_effect::ability, -1, 0 },
    { "Zbrojenie", "+1 obrona na tę budowę", 0, core::shop_effect::def, 1, 4 },
    { "Wylewka", "+3 max HP na tę budowę", 0, core::shop_effect::maxhp, 0, 4 },
    { "Deskowanie", "2 kawy do termosu", 0, core::shop_effect::thermos, 2, 3 },
};
inline constexpr core::status_def statuses[] = {   // indeks = core::status_effect
    { "", "", "" },
    { "Zatrucie", "Zatr.", "-1 HP/turę" },
    { "Porażenie", "Poraż.", "tracisz turę" },
    { "Poślizg", "Pośl.", "ruch o 2 pola" },
    { "Papierologia", "Papier.", "moc później" },
    { "Mokry", "Mokry", "prąd boli bardziej" },
};
inline constexpr int paper_delay = 3;
inline constexpr int acts_count = 4;
inline constexpr int hurtownia_count = 9;
inline constexpr int slam_every = 4;
inline constexpr int slam_damage_bonus = 3;
inline constexpr int slam_radius = 1;
inline constexpr int slam_delay = 2;
inline constexpr int slam_cross_reach = 2;
inline constexpr int slam_cross_delay = 3;
inline constexpr int cash_per_score = 5;
inline constexpr int enemy_przeciek = 0;
inline constexpr int enemy_zwarcie = 1;
inline constexpr int enemy_plesn = 2;
inline constexpr int enemy_kornik = 3;
inline constexpr int enemy_papierologia = 4;
inline constexpr int enemy_dostawa = 5;
inline constexpr int enemy_ulewa = 6;
inline constexpr int enemy_budzet = 7;
inline constexpr int enemy_termin = 8;
inline constexpr int enemy_betoniarka = 9;
inline constexpr int enemy_nawalnica = 10;
inline constexpr int enemy_inspekcja = 11;
inline constexpr int enemy_woda = 12;
inline constexpr int enemy_kamien = 13;
inline constexpr int enemy_osuwisko = 14;
inline constexpr int enemy_folia = 15;
inline constexpr int enemy_krzywy_mur = 16;
inline constexpr int enemy_mostek = 17;
inline constexpr int enemy_ugiecie = 18;
inline constexpr int enemy_zbrojenie = 19;
inline constexpr int enemy_papa = 20;
inline constexpr int enemy_rynna = 21;
inline constexpr int enemy_pustak = 22;
inline constexpr int enemy_wymiarowka = 23;
inline constexpr int enemy_ramka = 24;
inline constexpr int enemy_przeciag = 25;
inline constexpr int enemy_zapowietrzenie = 26;
inline constexpr int enemy_uziemienie = 27;
inline constexpr int enemy_rysa = 28;
inline constexpr int enemy_wilgoc = 29;
inline constexpr int enemy_odpryski = 30;
inline constexpr int enemy_poprawki = 31;
inline constexpr int enemy_podpis = 32;
inline constexpr int enemy_wniosek = 33;
inline constexpr int enemy_termin_odw = 34;
inline constexpr int enemy_niezgodnosc = 35;
inline constexpr int enemy_pieczatka = 36;
inline constexpr int enemy_rura = 37;
inline constexpr int enemy_cisnienie = 38;
inline constexpr int enemy_kabel = 39;
inline constexpr int enemy_decyzja = 40;

inline constexpr core::badge_def badges[] = {   // perk = uprawnienie: trwała premia na każdą budowę
    { "Bez usterek", "Etap bez żadnych obrażeń", 20, { core::perk_effect::hp, 2 } },
    { "Przed terminem", "Termin pokonany w 150 tur", 30, { core::perk_effect::cooldown, 1 } },
    { "Seryjny", "8 problemów na jednym etapie", 15, { core::perk_effect::dmg, 1 } },
    { "Zawodowiec", "Poziom postaci 5", 20, { core::perk_effect::crit, 5 } },
    { "Twardziel", "Wygrana na Trudnym", 40, { core::perk_effect::def, 1 } },
    { "Pełny zespół", "Wygrana każdym zawodem", 60, { core::perk_effect::cash, 20 } },
    { "Kolekcjoner", "Znajdź wszystkie narzędzia", 30, { core::perk_effect::tool_pct, 10 } },
    { "Katalog usterek", "Pokonaj każdy problem", 30, { core::perk_effect::luck, 1 } },
    { "Osiedle", "Zbuduj 5 domów", 50, { core::perk_effect::xp_pct, 10 } },
};

inline constexpr int badges_count = 9;
inline constexpr int enemies_count = 41;
inline constexpr int badge_bez_usterek = 0;
inline constexpr int badge_przed_terminem = 1;
inline constexpr int badge_seryjny = 2;
inline constexpr int badge_zawodowiec = 3;
inline constexpr int badge_twardziel = 4;
inline constexpr int badge_pelny_zespol = 5;
inline constexpr int badge_kolekcjoner = 6;
inline constexpr int badge_katalog = 7;
inline constexpr int badge_osiedle = 8;

inline constexpr core::keepsake_def keepsakes[] = {   // pamiątki: wybierane na start budowy, ranga rośnie z budowami
    { "Termos babci", "Stary, ale trzyma ciepło", core::perk_effect::thermos, { 1, 2, 3 }, -1, true },
    { "Kask ojca", "Pamięta niejedną budowę", core::perk_effect::def, { 1, 2, 3 }, 0, false },
    { "Szczęśliwa kielnia", "Nigdy nie zawiodła", core::perk_effect::luck, { 2, 3, 4 }, -1, false },
    { "Stara poziomica", "Wszystko widać jak na dłoni", core::perk_effect::sight, { 1, 2, 3 }, -1, false },
    { "Notes kierownika", "Wszystko zapisane, nic nie ginie", core::perk_effect::cooldown, { 1, 2, 3 }, -1, false },
};
inline constexpr int keepsakes_count = 5;
inline constexpr int keepsake_rank_runs[] = { 3, 8 };

inline constexpr core::contract_def contracts[] = {   // zlecenia: długofalowe cele z licznikami w profilu
    { "Trzy fachy", "Wygraj 3 różnymi zawodami", core::contract_kind::class_wins, 3, 40, -1 },
    { "Czysta robota", "Boss aktu bez obrażeń w walce", core::contract_kind::clean_boss, 1, 30, 3 },
    { "Markowy styl", "Zbierz 5 markowych przedmiotów", core::contract_kind::brand, 5, 30, 2 },
    { "Mocarz", "Użyj mocy zawodu 100 razy", core::contract_kind::powers, 100, 40, 4 },
    { "Pogromca usterek", "Usuń 200 problemów", core::contract_kind::kills, 200, 60, -1 },
    { "Stały klient", "Wygraj 5 budów", core::contract_kind::wins, 5, 50, -1 },
};
inline constexpr int contracts_count = 6;

inline constexpr core::site_event_def site_events[] = {   // wydarzenia na placu: SMS na starcie etapu
    { "Dostawa spóźniona", "-2 znajdź.", "Mniej znajdziek na etapie", { "Kierownik Marek", { "Hurtownia dzwoniła:", "dostawa będzie jutro.", "Mniej materiału na placu." } }, core::event_effect::fewer_pickups, 2, false },
    { "Premia od inwestora", "+20 zł", "Budżet budowy +20 zł", { "Anna Nowak", { "Dobrze idzie! Przelewam", "premię na budowę. Kupcie", "coś w Hurtowni." } }, core::event_effect::cash, 20, true },
    { "Inspekcja nadzoru", "Bez ran", "Etap bez obrażeń: +10 dośw.", { "Kierownik Marek", { "Dziś inspektor nadzoru.", "Etap bez obrażeń =", "premia. Uważaj na siebie!" } }, core::event_effect::inspection, 10, true },
    { "Ulewa w nocy", "Poślizg", "Ciosy: +25% szans na poślizg", { "Anna Nowak", { "W nocy lało jak z cebra.", "Na placu błoto po kostki.", "Uważaj, ślisko!" } }, core::event_effect::rain, 25, false },
    { "Ekipa na kawie", "Termos", "Termos pełny", { "Kierownik Marek", { "Ekipa zrobiła kawę", "dla wszystkich. Termos", "masz pełny!" } }, core::event_effect::thermos, 0, true },
};
inline constexpr int site_events_count = 5;
inline constexpr int site_event_chance_pct = 45;

inline constexpr core::weather_def weather[] = {   // pogoda dnia: losowana na starcie etapu
    { "Słonecznie", "Pogodnie", "Bez wpływu na etap", core::weather_effect::none, 0, 40, false, 4095 },
    { "Upał", "Moc +1 t.", "Moc odnawia się 1 turę dłużej", core::weather_effect::heat, 1, 16, true, 4095 },
    { "Mróz", "Wolniejsi", "Problemy stoją co 3. turę", core::weather_effect::frost, 3, 14, false, 511 },
    { "Wiatr", "Zasięg -1", "Broń z dystansu: zasięg -1", core::weather_effect::wind, 1, 14, true, 496 },
    { "Deszcz", "Kałuże", "Wejście w kałużę = poślizg", core::weather_effect::rain, 9, 16, true, 511 },
};
inline constexpr int weather_count = 5;
inline constexpr bool weather_no_bad_stack = true;

inline constexpr core::helper_def brigade[] = {   // brygada: najemni fachowcy (raz na etap)
    { "Geodeta", "Mapa etapu i schody", core::helper_effect::reveal, 0, 0, 0, 8, 0, -1 },
    { "BHP-owiec", "Bez stanów, obrona +2", core::helper_effect::safety, 2, 8, 0, 10, 0, -1 },
    { "Pompa do betonu", "Beton wokół: -6 HP", core::helper_effect::pump, 6, 0, 2, 15, 30, -1 },
    { "Elektryk-kolega", "Pomaga 10 tur, cios -3", core::helper_effect::ally, 3, 10, 1, 15, 35, 3 },
};
inline constexpr int brigade_count = 4;
inline constexpr int start_helpers_mask = 3;

inline constexpr core::investor_def investor[] = {   // tryb inwestora: modyfikatory po pierwszej wygranej
    { "Budżet -30%", "Mniej zł za problemy i akty", core::investor_effect::cash_pct, -30, 10, 1 },
    { "Bez przerwy na kawę", "Między etapami bez +5 HP", core::investor_effect::no_break, 0, 10, 1 },
    { "Problemy +20% HP", "Każdy problem ma +20% HP", core::investor_effect::enemy_hp, 20, 15, 2 },
    { "Hurtownia zamknięta", "Między aktami bez zakupów", core::investor_effect::no_shop, 0, 15, 2 },
    { "Kontrola częściej", "Boss uderza co 3 tury, nie 4", core::investor_effect::slam, 1, 10, 1 },
    { "Termin goni", "Problemy biją o 1 mocniej", core::investor_effect::enemy_dmg, 1, 20, 3 },
};
inline constexpr int investor_count = 6;

inline constexpr core::material_def materials[] = {   // materiały: cement, stal, drewno
    { "Cement", "Cem." },
    { "Stal", "Stal" },
    { "Drewno", "Drew." },
};
inline constexpr core::repair_def repairs[] = {   // naprawy pola za materiał
    { "Załataj", "Deski w poprzek drogi", "Mur 3 pola przed problemem, 6 tur", core::repair_effect::patch, 2, 1, 6 },
    { "Kładka", "Kałuże bez poślizgu", "Kałuże w zasięgu 2: bez poślizgu", core::repair_effect::bridge, 1, 1, 2 },
};
inline constexpr int materials_count = 3;
inline constexpr int repairs_count = 2;
inline constexpr int material_drop_pct = 35;
inline constexpr int material_boss_drop = 2;
inline constexpr int material_gear_box = 1;
inline constexpr int material_max = 9;

inline constexpr core::path_def paths[] = {   // wybór ścieżki: wariant kolejnego etapu
    { "Szybko i drogo", "Szybko", "-2 problemy, -15 zł, -1 znajdźka", -2, -1, -15, 0, false, false },
    { "Tanio, ryzyko", "Tanio", "+1 problem, +2 znajdźki, zła pogoda", 1, 2, 0, 0, true, false },
    { "Po terminie", "Po term.", "Bez wydarzenia na placu, -10 zł", 0, 0, -10, 0, false, true },
    { "Z zapasem", "Zapas", "+1 problem, +3 materiały na start", 1, 0, 0, 3, false, false },
};
inline constexpr int paths_count = 4;

inline constexpr int daily_epoch[] = { 2026, 1, 1 };   // codzienna budowa: dzień nr 1
inline constexpr int daily_default_date[] = { 2026, 10, 1 };
inline constexpr int daily_difficulty = 1;
inline constexpr int daily_investor_mods = 2;
inline constexpr int daily_history = 5;

inline constexpr int schedule_min_days = 4;   // harmonogram domu: dni etapu = min + tury / turnsPerDay
inline constexpr int schedule_turns_per_day = 3;
inline constexpr const char* schedule_url = "planbudowlany.online";

inline constexpr core::tool_def tools[] = {
    { 6, 0, false },
    { 7, 20, false },
    { 8, 30, false },
    { 9, 40, false },
    { 10, 25, false },
    { 11, 35, false },
    { 15, 0, true },
    { 16, 0, true },
};

inline constexpr int tools_count = 8;
inline constexpr int start_tools_mask = 1;
inline constexpr int drop_chance_pct = 25;
inline constexpr int drop_weights[] = { 40, 5, 5, 15, 35 };

inline constexpr int crit_base_pct = 5;
inline constexpr int crit_per_luck_pct = 3;
inline constexpr int crit_multiplier = 2;
inline constexpr int drop_per_luck_pct = 2;
inline constexpr int rarity_per_luck = 3;
inline constexpr int dodge_per_luck_pct = 2;
inline constexpr int dodge_max_pct = 20;

inline constexpr int thermos_capacity = 3;
inline constexpr int coffee_heal = 8;
inline constexpr int bot_drink_below_pct = 40;

inline constexpr core::gear_def gear[] = {   // indeks = slot * 3 + jakość
    { "Kask budowlany", core::gear_stat::def, 1 },
    { "Kask z latarką", core::gear_stat::def, 2 },
    { "Kask markowy", core::gear_stat::def, 3 },
    { "Rękawice robocze", core::gear_stat::dmg, 1 },
    { "Rękawice wzmacniane", core::gear_stat::dmg, 2 },
    { "Rękawice markowe", core::gear_stat::dmg, 3 },
    { "Kamizelka odblaskowa", core::gear_stat::hp, 4 },
    { "Kamizelka ocieplana", core::gear_stat::hp, 8 },
    { "Kamizelka markowa", core::gear_stat::hp, 12 },
    { "Buty robocze", core::gear_stat::dodge, 3 },
    { "Buty z podnoskiem", core::gear_stat::dodge, 5 },
    { "Buty markowe", core::gear_stat::dodge, 8 },
    { "Pas narzędziowy", core::gear_stat::thermos, 1 },
    { "Pas z kaburą", core::gear_stat::thermos, 1 },
    { "Pas markowy", core::gear_stat::thermos, 2 },
};
inline constexpr const char* gear_slots[] = { "Kask", "Rękawice", "Kamizelka", "Buty", "Pas" };
inline constexpr const char* gear_rarities[] = { "Zwykły", "Solidny", "Markowy" };
inline constexpr core::trait_def gear_traits[] = {   // cechy sprzętu (losowane do każdego przedmiotu)
    { "Szczęście +1", "Szcz.+1", core::trait_effect::luck, 1 },
    { "Kryt +5%", "Kryt+5%", core::trait_effect::crit, 5 },
    { "Odporność na zatrucie", "Bez zatr.", core::trait_effect::poison_res, 1 },
    { "Widzenie +1", "Wzrok+1", core::trait_effect::sight, 1 },
    { "Odnowienie mocy -1", "Moc -1t", core::trait_effect::cooldown, 1 },
    { "Siła +1", "SIŁ+1", core::trait_effect::str, 1 },
    { "Zręczność +1", "ZRĘ+1", core::trait_effect::agi, 1 },
    { "Inteligencja +1", "INT+1", core::trait_effect::intel, 1 },
    { "Bez poślizgu", "Bez pośl.", core::trait_effect::slip_res, 1 },
};
inline constexpr int gear_traits_count = 9;
inline constexpr int gear_decline_xp = 1;
inline constexpr int gear_slots_count = 5;
inline constexpr int gear_reward_mask = 24;   // sloty z nagród
inline constexpr int gear_base_mask = 7;   // pełny sprzęt (BHP)
inline constexpr int gear_solid_from = 70;
inline constexpr int gear_brand_from = 94;
inline constexpr int gear_stage_bonus = 5;

inline constexpr int upgrades_count = 7;
inline constexpr int xp_per_kill = 1;
inline constexpr int xp_per_stage = 5;
inline constexpr int xp_boss = 20;
inline constexpr int start_classes_mask = 19;
inline constexpr int reward_classes_mask = 448;
inline constexpr int class_cost = 25;
inline constexpr int hard_cost = 40;

inline constexpr int level_thresholds[] = { 10, 25, 45, 70 };
inline constexpr int max_hero_level = 5;
inline constexpr int hp_per_level = 2;
inline constexpr int dmg_levels_mask = 32;
inline constexpr int def_levels_mask = 8;

inline constexpr core::respect_def respect[] = {   // Respekt: stałe ulepszenia z rangami
    { "Pewna ręka", "Obrażenia", core::respect_effect::dmg_pct, 2, { 1, 2, 0, 0, 0 }, { 15, 40, 0, 0, 0 } },
    { "Gruba skóra", "Otrzymane obrażenia", core::respect_effect::taken_pct, 2, { 1, 2, 0, 0, 0 }, { 15, 40, 0, 0, 0 } },
    { "Dobre źródła", "Szansa na lepszy sprzęt", core::respect_effect::gear_pct, 5, { 1, 2, 3, 4, 5 }, { 6, 12, 20, 32, 48 } },
    { "Oko fachowca", "Szansa na kryt", core::respect_effect::crit, 2, { 1, 2, 0, 0, 0 }, { 15, 40, 0, 0, 0 } },
    { "Zwinność", "Unik (łącznie maks. 20%)", core::respect_effect::dodge, 2, { 1, 2, 0, 0, 0 }, { 15, 40, 0, 0, 0 } },
    { "Mocna kawa", "Kawa leczy więcej", core::respect_effect::coffee_pct, 2, { 5, 10, 0, 0, 0 }, { 10, 30, 0, 0, 0 } },
    { "Duży termos", "Termos: miejsce na kawę", core::respect_effect::thermos, 1, { 1, 0, 0, 0, 0 }, { 40, 0, 0, 0, 0 } },
    { "Rutyna", "Moc odnawia się szybciej", core::respect_effect::cooldown, 2, { 1, 2, 0, 0, 0 }, { 30, 60, 0, 0, 0 } },
    { "Oszczędności", "Budżet na start", core::respect_effect::cash, 5, { 10, 20, 30, 40, 50 }, { 5, 10, 16, 25, 36 } },
    { "Nauka", "Doświadczenie", core::respect_effect::xp_pct, 5, { 4, 8, 12, 16, 20 }, { 6, 12, 20, 32, 48 } },
    { "Znajomości", "Brygada taniej", core::respect_effect::brigade_pct, 5, { 6, 12, 18, 24, 30 }, { 5, 10, 16, 25, 36 } },
    { "Czujność", "Pole widzenia", core::respect_effect::sight, 1, { 1, 0, 0, 0, 0 }, { 40, 0, 0, 0, 0 } },
    { "Rabat", "Hurtownia taniej", core::respect_effect::shop_pct, 5, { 5, 10, 15, 20, 25 }, { 5, 10, 16, 25, 36 } },
    { "Zapasy", "Więcej materiałów", core::respect_effect::mats_pct, 5, { 10, 20, 30, 40, 50 }, { 5, 10, 16, 25, 36 } },
    { "Druga szansa", "Raz na budowę: 1 HP zamiast końca", core::respect_effect::second_chance, 1, { 1, 0, 0, 0, 0 }, { 120, 0, 0, 0, 0 } },
    { "Druga oferta", "Darmowe losowanie", core::respect_effect::reroll, 1, { 1, 0, 0, 0, 0 }, { 30, 0, 0, 0, 0 } },
};
inline constexpr int push_chance_pct = 20;   // Operator koparki: cios wręcz odpycha
inline constexpr int respect_count = 16;
inline constexpr int respect_stage = 2;   // Respekt za etap: zwykły, boss w środku aktu, boss aktu, ostatni
inline constexpr int respect_boss = 4;
inline constexpr int respect_act_boss = 6;
inline constexpr int respect_final = 10;

inline constexpr core::reward_def rewards[] = {   // nagrody za odbiór: każda wygrana odblokowuje kolejną
    { core::reward_kind::tool, 6, "Młot udarowy", "Narzędzie w dropach" },
    { core::reward_kind::cls, 6, "Dekarz", "Nowy zawód: z dystansu" },
    { core::reward_kind::gear, 3, "Buty robocze", "Nowy sprzęt: unik" },
    { core::reward_kind::tool, 7, "Pistolet do kotew", "Narzędzie w dropach" },
    { core::reward_kind::cls, 7, "Tynkarz", "Nowy zawód: obszar" },
    { core::reward_kind::gear, 4, "Pas narzędziowy", "Nowy sprzęt: termos" },
    { core::reward_kind::cls, 8, "Operator koparki", "Nowy zawód: taran" },
    { core::reward_kind::act, 3, "Akt 0: Papierologia", "Nowy akt przed budową" },
};
inline constexpr int rewards_count = 8;

inline constexpr core::tutorial_step tutorial_steps[] = {   // samouczek menu: tytuł (0) i wybór zawodu (1)
    { "new", "Nowa budowa", { "Kierownik Marek", { "Tu zaczynasz budowę domu", "Nowaków: zawód, trudność", "i na plac. Powodzenia!" } }, "START / A", 0, false, false },
    { "phone", "Profil w telefonie", { "Kierownik Marek", { "Twój telefon: odznaki,", "zlecenia, pamiątki,", "Katalog usterek, Osiedle." } }, "SELECT", 0, false, false },
    { "training", "Szkolenia", { "Kierownik Marek", { "Za doświadczenie z budów", "kupisz Szkolenia: HP,", "nowe zawody, narzędzia." } }, "SELECT, Koszty", 0, false, false },
    { "respect", "Respekt", { "Kierownik Marek", { "Respekt dostajesz za", "każdy etap i zostaje po", "porażce. Stałe premie!" } }, "SELECT, Koszty", 0, false, false },
    { "daily", "Codzienna budowa", { "Kierownik Marek", { "Budowa dnia: ten sam", "plac i zawód dla", "wszystkich. Pobij wynik!" } }, "R", 0, false, false },
    { "help", "Jak grać", { "Kierownik Marek", { "Sterowanie, akty,", "statystyki. Tam też", "powtórzysz samouczek." } }, "B", 0, false, false },
    { "options", "Opcje", { "Kierownik Marek", { "Dźwięk, wibracje,", "sterowanie i wielkość", "tekstu są pod kluczem." } }, "", 0, true, false },
    { "class", "Zawód", { "Kierownik Marek", { "Każdy fach ma broń, moc", "i statystyki. Nowe fachy", "kupisz albo wygrasz." } }, "lewo / prawo", 1, false, false },
    { "difficulty", "Trudność", { "Kierownik Marek", { "Łatwy, Normalny, Trudny:", "silniejsze problemy,", "więcej punktów." } }, "góra / dół", 1, false, false },
    { "keepsake", "Pamiątka", { "Kierownik Marek", { "Jedna pamiątka na", "budowę. Im częściej ją", "bierzesz, tym mocniejsza." } }, "L / R", 1, false, false },
    { "stats", "Statystyki", { "Kierownik Marek", { "Co daje SIŁ, ZRĘ, INT,", "OBR i SZCZ - wzory", "w prostych słowach." } }, "START", 1, false, false },
    { "investor", "Tryb inwestora", { "Kierownik Marek", { "Utrudnienia za więcej", "doświadczenia. Stawka", "to Twój rekord zawodu." } }, "SELECT", 1, false, true },
    { "go", "Na plac!", { "Kierownik Marek", { "Wybierz fach i ruszamy.", "Będę pisał po drodze.", "Powodzenia, szefie!" } }, "A", 1, false, false },
};
inline constexpr core::tutorial_step tutorial_unlocks[] = {   // dymki przy pierwszym odblokowaniu (kolejność = core::tutorial_unlock)
    { "respect", "Pierwszy Respekt!", { "Kierownik Marek", { "Respekt zostaje na stałe.", "Wydasz go w telefonie:", "Koszty, strona Respekt." } }, "SELECT, Koszty", 0, false, false },
    { "daily", "Codzienna budowa", { "Kierownik Marek", { "Po pierwszej budowie", "spróbuj budowy dnia:", "ten sam plac dla każdego." } }, "R", 0, false, false },
    { "investor", "Tryb inwestora", { "Kierownik Marek", { "Pierwszy odbiór! Na", "wyborze zawodu włączysz", "utrudnienia za dośw." } }, "SELECT", 1, false, false },
    { "act0", "Akt 0: Papierologia", { "Kierownik Marek", { "Najpierw papiery! Nowe", "budowy zaczną się od", "pozwoleń i przyłączy." } }, "START / A", 0, false, false },
    { "class", "Nowy zawód", { "Kierownik Marek", { "Nowy fach w ekipie!", "Czeka na pasku zawodów.", "Wypróbuj go na placu." } }, "lewo / prawo", 1, false, false },
};
inline constexpr int tutorial_steps_count = 13;
inline constexpr int tutorial_unlocks_count = 5;

inline constexpr core::boon_rarity_def boon_rarities[] = {   // premie: rzadkość i waga losowania
    { "zwykła", 70 },
    { "rzadka", 24 },
    { "legendarna", 6 },
};
inline constexpr const char* boon_tags[] = { "Woda", "Prąd", "Beton", "BHP", "Szczęście", "Kawa", "Brygada", "Materiały", "Iskra" };   // znaczniki premii
inline constexpr core::boon_def boons[] = {   // premie po etapie: 1 z 3 (rzadkość, znaczniki, skutek, zawód)
    { "Hartowana kielnia", "+1 obrażeń", 1, 4, core::boon_effect::dmg, 1, -1 },
    { "Beton B30", "+1 OBR", 1, 4, core::boon_effect::def, 1, -1 },
    { "Płyta warstwowa", "+4 max HP", 0, 8, core::boon_effect::max_hp, 4, -1 },
    { "Zbrojona rękawica", "+1 obrażeń", 1, 128, core::boon_effect::dmg, 1, -1 },
    { "Młot mistrza", "+10% obrażeń", 2, 4, core::boon_effect::dmg_pct, 10, -1 },
    { "Szelki asekuracyjne", "Unik +4%", 0, 8, core::boon_effect::dodge, 4, -1 },
    { "Apteczka na placu", "+4 HP na start etapu", 0, 40, core::boon_effect::regen_stage, 4, -1 },
    { "Kask z latarką", "Widzenie +1", 0, 8, core::boon_effect::sight, 1, -1 },
    { "Instrukcja BHP", "Stany krócej o 1 t.", 1, 8, core::boon_effect::status_res, 1, -1 },
    { "Anioł stróż", "+8 max HP", 2, 8, core::boon_effect::max_hp, 8, -1 },
    { "Podwójne espresso", "Kawa leczy +3 HP", 0, 32, core::boon_effect::coffee, 3, -1 },
    { "Termos z bufetu", "Termos +1 miejsce", 0, 32, core::boon_effect::thermos, 1, -1 },
    { "Druga zmiana", "Moc -2 t. odnowienia", 1, 32, core::boon_effect::cooldown, 2, -1 },
    { "Drożdżówka", "+1 HP za usunięty problem", 2, 32, core::boon_effect::kill_heal, 1, -1 },
    { "Koniczyna", "+1 SZCZ", 0, 16, core::boon_effect::luck, 1, -1 },
    { "Szczęśliwa moneta", "Kryt +4%", 0, 16, core::boon_effect::crit, 4, -1 },
    { "Złota podkowa", "+2 SZCZ", 2, 16, core::boon_effect::luck, 2, -1 },
    { "Wąż ogrodowy", "Ciosy moczą problem", 0, 1, core::boon_effect::wet_hits, 3, -1 },
    { "Suchy lód", "Ciosy zmrażają problem", 1, 1, core::boon_effect::frost_hits, 1, -1 },
    { "Hydrofor", "+6% obrażeń", 1, 3, core::boon_effect::dmg_pct, 6, -1 },
    { "Przedłużacz", "Ciosy z prądem", 0, 2, core::boon_effect::electric, 1, -1 },
    { "Transformator", "+2 obrażeń", 2, 258, core::boon_effect::dmg, 2, -1 },
    { "Krzesiwo", "Ciosy krzeszą iskry", 0, 256, core::boon_effect::spark, 1, -1 },
    { "Tarcza tnąca", "Kryt +6%", 1, 256, core::boon_effect::crit, 6, -1 },
    { "Numer do ekipy", "Brygada -25% ceny", 0, 64, core::boon_effect::brigade_pct, 25, -1 },
    { "Premia od inwestora", "+40 zł od razu", 0, 64, core::boon_effect::cash, 40, -1 },
    { "Brygadzista", "Kryt +6%", 1, 80, core::boon_effect::crit, 6, -1 },
    { "Paleta materiałów", "+2 każdego materiału", 0, 128, core::boon_effect::mats, 2, -1 },
    { "Karta hurtowni", "Hurtownia -25% ceny", 0, 128, core::boon_effect::shop_pct, 25, -1 },
    { "Skład na placu", "Materiały +60% częściej", 1, 128, core::boon_effect::mats_pct, 60, -1 },
    { "Twarda odprawa", "Odprawa ogłusza +1 t.", 1, 8, core::boon_effect::power, 1, 0 },
    { "Harmonogram", "Moc -3 t. odnowienia", 0, 32, core::boon_effect::cooldown, 3, 0 },
    { "Gruba ścianka", "Ścianka stoi +3 t.", 0, 4, core::boon_effect::power, 3, 1 },
    { "Pełny magazynek", "Seria: zasięg +1", 1, 128, core::boon_effect::power, 1, 2 },
    { "Mocny łańcuch", "Łańcuch: +1 cel", 1, 2, core::boon_effect::power, 1, 3 },
    { "Gumowe rękawice", "+2 OBR", 0, 10, core::boon_effect::def, 2, 3 },
    { "Mocny strumień", "Zawór leczy +4 HP", 0, 1, core::boon_effect::power, 4, 4 },
    { "Szybka wirówka", "Wirówka +2 obrażeń", 1, 256, core::boon_effect::power, 2, 5 },
    { "Długa rynna", "Rynna: +2 pola", 0, 1, core::boon_effect::power, 2, 6 },
    { "Gęsty tynk", "Narzut ogłusza +1 t.", 1, 4, core::boon_effect::power, 1, 7 },
    { "Ciężka łyżka", "Taran +2 obrażeń", 1, 4, core::boon_effect::power, 2, 8 },
};
inline constexpr core::synergy_def synergies[] = {   // synergie: 2+ premie z tym samym znacznikiem
    { "Przepięcie", "Ciosy z prądem, porażenie dalej", 3, core::synergy_effect::conduct, 1 },
    { "Zbrojenie", "+1 OBR za każdą premię Beton", 4, core::synergy_effect::armor, 1 },
    { "Espresso", "Kawa ładuje moc (-3 t.)", 32, core::synergy_effect::espresso, 3 },
    { "Pełne BHP", "Bez zatrucia i porażenia", 8, core::synergy_effect::safety, 0 },
    { "Fart", "+2 SZCZ", 16, core::synergy_effect::luck, 2 },
    { "Stała ekipa", "Brygada -50% ceny", 64, core::synergy_effect::brigade, 50 },
    { "Magazyn", "Materiały 2x częściej", 128, core::synergy_effect::stock, 100 },
    { "Iskrzenie", "Wybuch pyłu +3 i szerzej", 256, core::synergy_effect::sparks, 3 },
};
inline constexpr int boons_count = 41;
inline constexpr int boon_tags_count = 9;
inline constexpr int synergies_count = 8;
inline constexpr int boon_reroll_cost = 25;
inline constexpr int synergy_at = 2;
inline constexpr int boon_luck_rare = 2;
inline constexpr int boon_luck_legend = 1;

inline constexpr core::elite_def elites[] = {   // elity: cecha, przedrostek nazwy wg rodzaju
    { "Tarcza", { "Zbrojony", "Zbrojona", "Zbrojone" }, "+4 OBR", core::elite_effect::shield, 4 },
    { "Szybki", { "Szybki", "Szybka", "Szybkie" }, "2 ruchy na turę", core::elite_effect::fast, 2 },
    { "Regeneracja", { "Uparty", "Uparta", "Uparte" }, "+1 HP co turę", core::elite_effect::regen, 1 },
    { "Wybuchowy", { "Wybuchowy", "Wybuchowa", "Wybuchowe" }, "Wybucha po usunięciu", core::elite_effect::explode, 0 },
    { "Wzywa pomoc", { "Zaraźliwy", "Zaraźliwa", "Zaraźliwe" }, "Przy 50% HP wzywa pomoc", core::elite_effect::summon, 50 },
};
inline constexpr int elites_count = 5;
inline constexpr int elite_act_pct[] = { 7, 11, 15, 5 };   // szansa na elitę wg aktu (data::acts)
inline constexpr int elite_diff_pct[] = { -3, 0, 5 };
inline constexpr int elite_tier_pct = 4;
inline constexpr int elite_hp_pct = 150;
inline constexpr int elite_dmg = 1;
inline constexpr int elite_respect = 1;
inline constexpr int elite_mats = 2;
inline constexpr int elite_gear_min = 1;

inline constexpr core::combo_def combos[] = {   // kombinacje stanów (kolejność = core::combo_effect)
    { "Porażenie", "Mokry + prąd!", "Prąd w mokry: +3 i na mokrych obok", "Ty mokry + prąd: +2 i tracisz turę", core::combo_effect::shock_area, 3, 1, 2 },
    { "Wybuch pyłu", "Pył + iskra!", "Iskra w zapylony: wybuch 4 wokół", "Wybuch w pyle: +2 dla Ciebie", core::combo_effect::dust_blast, 4, 1, 2 },
    { "Pęknięcie", "Zamróz + uderzenie!", "Cios wręcz w zmrożony: +50%", "", core::combo_effect::crack, 50, 0, 0 },
};
inline constexpr const char* combo_sources[] = { "Mokry: kałuże, Deszcz, woda, Zawór", "Prąd: Próbnik, Łańcuch, Zwarcie", "Pył: akt III; iskra: Szlifierka", "Zamróz: Mróz, Suchy lód" };   // Jak grać: skąd stany
inline constexpr int combos_count = 3;
inline constexpr int wet_turns = 3;
inline constexpr int hero_wet_turns = 2;

inline constexpr core::choice_event_def choice_events[] = {   // wydarzenia z wyborem: pole z SMS-em na etapie
    { "Betoniarka sąsiada", { "Sąsiad Zenek", { "Mam wolną betoniarkę.", "Za dychę pożyczę na dziś.", "Beton sam się zmiesza!" } }, { { "Pożycz za 20 zł", "Betoniarka kręci: mocne ciosy!", { { core::choice_effect::cash, -20, 100, -1 }, { core::choice_effect::stage_dmg, 2, 100, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 2 }, { "Odmów", "Zenek wzrusza ramionami.", { { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 0 }, { "", "", { { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 0 } }, 2 },
    { "Tańszy dostawca", { "Kierownik Marek", { "Nowy dostawca da taniej.", "Materiał trochę wilgotny,", "może złapać pleśń." } }, { { "Bierz tanio", "Materiał już na placu.", { { core::choice_effect::mats, 2, 100, -1 }, { core::choice_effect::spawn, 2, 30, 2 }, { core::choice_effect::cash, 0, 0, -1 } }, 2 }, { "Zostań przy starym", "Bez ryzyka, bez zysku.", { { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 0 }, { "", "", { { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 0 } }, 2 },
    { "Nadgodziny", { "Kierownik Marek", { "Zostaniesz po godzinach?", "Inwestor dopłaci, ale", "będziesz padnięty." } }, { { "Zostaję", "Nocka na budowie się opłaca.", { { core::choice_effect::xp, 15, 100, -1 }, { core::choice_effect::cash, 10, 100, -1 }, { core::choice_effect::hp, -4, 100, -1 } }, 3 }, { "Idę do domu", "Wyspany fachowiec to skarb.", { { core::choice_effect::hp, 4, 100, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 1 }, { "", "", { { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 0 } }, 2 },
    { "Znaleziony projekt", { "Anna Nowak", { "Znalazłam stary projekt", "z poprawkami architekta.", "Warto go przejrzeć!" } }, { { "Czytaj całą noc", "Nowy pomysł na budowę!", { { core::choice_effect::boon, 1, 100, -1 }, { core::choice_effect::hp, -4, 100, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 2 }, { "Oddaj Markowi", "Marek dziękuje.", { { core::choice_effect::xp, 6, 100, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 1 }, { "", "", { { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 0 } }, 2 },
    { "Zagubiony kask", { "Kierownik Marek", { "Ktoś zostawił nowy kask", "przy kontenerze. Weźmiesz", "czy oddasz właścicielowi?" } }, { { "Biorę", "Kask jak nowy.", { { core::choice_effect::gear, 1, 100, 0 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 1 }, { "Oddaję", "Cała ekipa to widziała.", { { core::choice_effect::respect, 1, 100, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 1 }, { "Sprzedaję", "Kupiec się znalazł.", { { core::choice_effect::cash, 12, 100, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 1 } }, 3 },
    { "Ekipa obok prosi", { "Sąsiad Zenek", { "Chłopaki z sąsiedniej", "budowy nie dają rady", "z szalunkiem. Pomożesz?" } }, { { "Pomogę", "Szalunek stoi, jest wdzięczność.", { { core::choice_effect::hp, -3, 100, -1 }, { core::choice_effect::cash, 15, 100, -1 }, { core::choice_effect::respect, 1, 100, -1 } }, 3 }, { "Nie dziś", "Każdy ma swoją budowę.", { { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 0 }, { "", "", { { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 0 } }, 2 },
    { "Automat z kawą", { "Kierownik Marek", { "Na placu stanął automat", "z kawą. 5 zł za kubek,", "ale bywa kapryśny." } }, { { "Kup 2 kawy", "Kawa do termosu.", { { core::choice_effect::cash, -10, 100, -1 }, { core::choice_effect::coffee, 2, 75, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 2 }, { "Szkoda kasy", "Termos poczeka.", { { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 0 }, { "", "", { { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 0 } }, 2 },
    { "Stara ostrzałka", { "Sąsiad Zenek", { "Mam w garażu ostrzałkę.", "Podostrzę ci narzędzie,", "tylko uważaj na palce!" } }, { { "Ostrz narzędzie", "Narzędzie jak brzytwa.", { { core::choice_effect::upgrade, 1, 100, -1 }, { core::choice_effect::hp, -3, 35, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 2 }, { "Nie trzeba", "Zenek odkłada ostrzałkę.", { { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 0 }, { "", "", { { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 0 } }, 2 },
    { "Stal przed czasem", { "Anna Nowak", { "Hurtownia przywiozła stal", "tydzień za wcześnie.", "Przyjmiesz czy odeślesz?" } }, { { "Przyjmij", "Stal na placu.", { { core::choice_effect::mats, 3, 100, 1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 1 }, { "Odeślij", "Hurtownia oddaje za transport.", { { core::choice_effect::cash, 15, 100, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 1 }, { "Weź połowę", "Trochę stali, trochę zł.", { { core::choice_effect::mats, 1, 100, 1 }, { core::choice_effect::cash, 7, 100, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 2 } }, 3 },
    { "Szybka kontrola BHP", { "Kierownik Marek", { "Za chwilę kontrola BHP.", "Ubierzesz się porządnie", "czy robisz dalej?" } }, { { "Pełne BHP", "Kontrola zadowolona.", { { core::choice_effect::stage_def, 1, 100, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 1 }, { "Robię dalej", "Szybciej, ale ryzykownie.", { { core::choice_effect::xp, 8, 100, -1 }, { core::choice_effect::hp, -3, 50, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 2 }, { "", "", { { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 0 } }, 2 },
    { "Energetyk od Marka", { "Kierownik Marek", { "Masz energetyka na drogę.", "Da kopa, ale potem", "serce wali jak młot." } }, { { "Wypij od razu", "Moc gotowa!", { { core::choice_effect::power, 1, 100, -1 }, { core::choice_effect::hp, -2, 100, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 2 }, { "Do termosu", "Na czarną godzinę.", { { core::choice_effect::coffee, 1, 100, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 1 }, { "", "", { { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 0 } }, 2 },
    { "Premia za tempo", { "Anna Nowak", { "Jak skończycie ten etap", "szybko, dorzucę premię.", "Tylko bez fuszerki!" } }, { { "Przyspieszam", "Tempo! Ale mniej ostrożnie.", { { core::choice_effect::cash, 25, 100, -1 }, { core::choice_effect::stage_def, -1, 100, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 2 }, { "Spokojnie", "Dokładność przede wszystkim.", { { core::choice_effect::hp, 4, 100, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 1 }, { "", "", { { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 }, { core::choice_effect::cash, 0, 0, -1 } }, 0 } }, 2 },
};
inline constexpr int choice_events_count = 12;
inline constexpr int choice_event_chance_pct = 55;

inline constexpr core::tool_level_def tool_levels[] = {   // koszt kolejnych poziomów ulepszenia narzędzia
    { 40, 1, 3 },
    { 60, 1, 4 },
    { 80, 1, 5 },
};
inline constexpr core::tool_trait_def tool_traits[] = {   // cecha ulepszonego narzędzia (wybór na poziomie traitAt)
    { "Przebicie", "OBR -2", "Ignoruje 2 OBR problemu", core::tool_trait_effect::pierce, 2 },
    { "Ostrze", "kryt +5%", "Kryt +5%", core::tool_trait_effect::crit, 5 },
    { "Wyważenie", "rzut min +1", "Najsłabszy rzut +1", core::tool_trait_effect::steady, 1 },
};
inline constexpr int tool_upgrade_max = 3;
inline constexpr int tool_upgrade_dmg = 1;
inline constexpr int tool_trait_at = 2;
inline constexpr int tool_traits_count = 3;

inline constexpr core::secret_kind_def secret_kinds[] = {   // ukryte pomieszczenie: pęknięta ściana / drzwi
    { "Pęknięta ściana", "klucz, Operator lub wybuch", true },
    { "Drzwi magazynu", "tylko klucz", false },
};
inline constexpr int secret_kinds_count = 2;
inline constexpr int secret_chance_pct = 35;
inline constexpr int secret_guard_pct = 50;
inline constexpr int chest_respect = 2;
inline constexpr int chest_mats = 1;
inline constexpr int chest_cash = 10;
inline constexpr int chest_gear_min = 1;

inline constexpr const char* recap_kind_names[] = { "cios", "z dystansu", "cios bossa", "wybuch", "mokry + prąd", "wybuch pyłu" };   // = core::recap_kind
inline constexpr const char* recap_verbs[] = { "Pokonał", "Pokonała", "Pokonało" };   // Pokonał / Pokonała / Pokonało Cię
inline constexpr core::recap_tip_def recap_tips[] = {   // rada w podsumowaniu: pierwsza pasująca
    { core::recap_tip::shock, { "Mokry + prąd boli: wyjdź", "z kałuży przed Zwarciem" } },
    { core::recap_tip::slam, { "Czerwone pola bossa: masz", "2 tury, zejdź po skosie" } },
    { core::recap_tip::blast, { "Wybuchowy problem: po", "usunięciu odejdź o 2 pola" } },
    { core::recap_tip::coffee, { "Kawa została w termosie:", "pij przy 40% HP (START)" } },
    { core::recap_tip::ranged, { "Strzelec trafia w linii:", "podchodź po skosie" } },
    { core::recap_tip::elite, { "Złota ramka = elita:", "cecha na karcie pod B" } },
    { core::recap_tip::boss, { "Przed bossem: pełny", "termos i gotowa moc (R)" } },
    { core::recap_tip::no_combo, { "Mokry + prąd zadaje +3:", "spróbuj premii Przepięcie" } },
    { core::recap_tip::won, { "Czas na więcej: tryb", "inwestora albo Trudny" } },
    { core::recap_tip::any, { "Harmonogram ma radę", "kierownika po każdym etapie" } },
};
inline constexpr int recap_tips_count = 10;

inline constexpr core::weekly_def weekly[] = {   // wyzwania tygodnia: kolejne tygodnie po kolei z listy
    { "Tylko Glazurnik, bez kawy", "Glazurnik bez kawy", { "Bez kawy, za to HP +25%.", "Kawa idzie na wynos (zł)" }, { { core::weekly_rule::cls, 5 }, { core::weekly_rule::no_coffee, 0 }, { core::weekly_rule::hp_pct, 25 } }, 3 },
    { "Elity x2", "Elity x2", { "Dwa razy więcej elit -", "i dwa razy więcej łupów" }, { { core::weekly_rule::elite_pct, 200 }, { core::weekly_rule::cash, 0 }, { core::weekly_rule::cash, 0 } }, 1 },
    { "Mokry tydzień", "Mokry tydzień", { "Deszcz na każdym etapie:", "kałuże, poślizg, mokro" }, { { core::weekly_rule::weather, 4 }, { core::weekly_rule::cash, 0 }, { core::weekly_rule::cash, 0 } }, 1 },
    { "Bez Hurtowni, x2 materiały", "Bez Hurtowni", { "Hurtownia zamknięta,", "materiały 2x częściej" }, { { core::weekly_rule::no_shop, 0 }, { core::weekly_rule::mats_pct, 100 }, { core::weekly_rule::cash, 0 } }, 2 },
    { "Szklany kask", "Szklany kask", { "HP -50%, ale Twoje", "ciosy +15%" }, { { core::weekly_rule::hp_pct, -50 }, { core::weekly_rule::dmg_pct, 15 }, { core::weekly_rule::cash, 0 } }, 2 },
    { "Kierownik na placu", "Tylko Kierownik", { "Tylko Kierownik, więcej", "elit, HP -25%" }, { { core::weekly_rule::cls, 0 }, { core::weekly_rule::elite_pct, 175 }, { core::weekly_rule::hp_pct, -25 } }, 3 },
};
inline constexpr int weekly_count = 6;
inline constexpr int weekly_epoch[] = { 2026, 1, 5 };   // tydzień nr 1 (poniedziałek)
inline constexpr int weekly_difficulty = 1;
inline constexpr int weekly_history = 3;
inline constexpr int weekly_coffee_cash = 5;

inline constexpr core::story_thread story_arc[] = {   // fabuła odkrywana z budowami: wątki SMS-ów (archiwum Wiadomości)
    { "Pierwsza budowa", "Rozegraj 1 budowę", core::story_trigger::runs, 1, { { "Kierownik Marek", { "Pierwsza budowa za nami.", "Notuję w PlanBudowlany,", "co poszło nie tak." } }, { "Anna Nowak", { "A ja notuję, kto wypił", "moją kawę z termosu.", "Marek, patrzę na Ciebie." } } }, 2 },
    { "Złota ramka", "Usuń elitę", core::story_trigger::elite, 0, { { "Kierownik Marek", { "Ten problem miał złotą", "ramkę. Takie to elity -", "trudniejsze, ale płacą." } }, { "", { "", "", "" } } }, 1 },
    { "Betoniarka", "Pokonaj Zepsutą Betoniarkę", core::story_trigger::boss, 9, { { "Sąsiad Zenek", { "Słyszałem huk. To moja", "stara betoniarka? Nie", "płaczę. No, trochę." } }, { "Kierownik Marek", { "Stan surowy bez niej", "idzie szybciej. Zenek,", "kupisz nową w Hurtowni." } } }, 2 },
    { "Stary magazyn", "Otwórz ukryty magazyn", core::story_trigger::secret, 0, { { "Sąsiad Zenek", { "Znalazłeś mój magazyn?", "Bierz, co chcesz. Tylko", "oddaj taczkę z 1998." } }, { "", { "", "", "" } } }, 1 },
    { "Szybka decyzja", "Odpowiedz na SMS na placu", core::story_trigger::event, 0, { { "Kierownik Marek", { "Na placu co chwilę coś.", "Odpisuj szybko, ale", "czytaj drobny druk!" } }, { "", { "", "", "" } } }, 1 },
    { "Synergia", "Połącz premie w synergię", core::story_trigger::synergy, 0, { { "Anna Nowak", { "Marek mówi, że premie", "się łączą. Jak my dwoje", "z kredytem. Brawo!" } }, { "", { "", "", "" } } }, 1 },
    { "Po burzy", "Pokonaj Nawałnicę", core::story_trigger::boss, 10, { { "Anna Nowak", { "Po Nawałnicy dach suchy!", "Dzieci pytają, czy mogą", "już malować pokoje." } }, { "Kierownik Marek", { "Mogą. Ale najpierw", "tynki. I okna. I drzwi.", "Pędzle trzymam u siebie." } } }, 2 },
    { "Protokół", "Pokonaj Inspekcję Pracy", core::story_trigger::boss, 11, { { "Kierownik Marek", { "Inspekcja: bez uwag!", "Pierwszy raz w mojej", "karierze. Oprawię to." } }, { "", { "", "", "" } } }, 1 },
    { "Klucze", "Wygraj budowę", core::story_trigger::wins, 1, { { "Anna Nowak", { "Pierwsza noc w nowym", "domu! Cisza, ciepło,", "nic nie cieknie." } }, { "Sąsiad Zenek", { "Sąsiad Zenek. Też chcę", "taki dom. Masz wolny", "termin? Mam działkę obok." } } }, 2 },
    { "Na czas", "Pokonaj Termin", core::story_trigger::boss, 8, { { "Anna Nowak", { "Termin pokonany! W", "harmonogramie wszystko", "na zielono. Pierwszy raz!" } }, { "", { "", "", "" } } }, 1 },
    { "Stała ekipa", "Rozegraj 5 budów", core::story_trigger::runs, 5, { { "Kierownik Marek", { "5 budów. Ekipa zna", "już Twoje tempo. Kawa", "stoi gotowa o 6:00." } }, { "", { "", "", "" } } }, 1 },
    { "Osiedle rośnie", "Wygraj 3 budowy", core::story_trigger::wins, 3, { { "Sąsiad Zenek", { "Na ulicy już 3 domy.", "Robi się osiedle! Stawiam", "ławkę pod lipą." } }, { "Anna Nowak", { "Zenek postawił ławkę.", "W sobotę grill dla", "całej ekipy!" } } }, 2 },
    { "Budowa dnia", "Zagraj budowę dnia", core::story_trigger::daily, 0, { { "Kierownik Marek", { "Budowa dnia jest jak", "poranna odprawa: ta sama", "dla każdej ekipy." } }, { "", { "", "", "" } } }, 1 },
    { "Zasady tygodnia", "Zagraj wyzwanie tygodnia", core::story_trigger::weekly, 0, { { "Inwestorka Ewa", { "Tu Ewa, inwestorka.", "Co tydzień nowe zasady.", "Zobaczymy, kto da radę." } }, { "", { "", "", "" } } }, 1 },
    { "Papierologia", "Odblokuj Akt 0", core::story_trigger::act0, 0, { { "Inwestorka Ewa", { "Nowa działka, nowe", "papiery. Akt 0 czeka.", "Pieczątki w dłoń!" } }, { "", { "", "", "" } } }, 1 },
    { "Odwołanie", "Pokonaj Decyzję odmowną", core::story_trigger::boss, 40, { { "Kierownik Marek", { "Decyzja odmowna?", "Odwołanie wygrane!", "Urzędnik aż wstał." } }, { "", { "", "", "" } } }, 1 },
    { "Dziesiąta budowa", "Rozegraj 10 budów", core::story_trigger::runs, 10, { { "Anna Nowak", { "10 budów! Marek robi", "z Twoich notatek plan", "w aplikacji dla innych." } }, { "", { "", "", "" } } }, 1 },
    { "Nasze osiedle", "Wygraj 5 budów", core::story_trigger::wins, 5, { { "Inwestorka Ewa", { "5 domów, jedno osiedle.", "Chcę tu plac zabaw.", "Budżet? Znajdzie się." } }, { "Anna Nowak", { "Plac zabaw! Dzieci już", "rysują plan. Na kartce,", "ale z harmonogramem." } } }, 2 },
    { "Dom za domem", "Wygraj 10 budów", core::story_trigger::wins, 10, { { "Kierownik Marek", { "10 odbiorów. Plan", "pokonał chaos. Dzięki", "- bez Ciebie ani rusz." } }, { "Anna Nowak", { "Ulica ma nazwę: Planowa.", "Tabliczkę wieszamy", "w sobotę. Przyjdź!" } } }, 2 },
};
inline constexpr int story_arc_count = 19;

inline constexpr core::decor_def estate_decor[] = {   // ozdoby Osiedla (klatki w houses.bmp za pustą działką)
    { "Lipa", 1 },
    { "Ławka Zenka", 3 },
    { "Latarnia", 4 },
    { "Plac zabaw", 5 },
    { "Tablica PlanBudowlany", 8 },
    { "Fontanna", 10 },
};
inline constexpr int estate_decor_count = 6;

inline constexpr const char* version = "v0.21.51";   // numer wersji (ekran tytułowy, changelog)

inline constexpr const char* damage_help[] = {   // Jak grać: obrażenia broni w prostych słowach (rozpiska #26)
    "Cios = rzut broni + premie:",
    "SIŁ/ZRĘ/INT broni +1 co 2 pkt,",
    "Szkolenia, odznaki, poziom,",
    "projekty, rękawice; potem +%.",
    "OBR problemu: -1 co 2 pkt (min. 1).",
    "Kryt mnoży cios, SZCZ go podnosi.",
};
inline constexpr int damage_help_count = 6;

inline constexpr const char* extras_help[] = {   // Jak grać: wydarzenia z wyborem, ulepszanie narzędzia, magazyn
    "Pole z SMS-em: wydarzenie.",
    "Odpowiedź: ryzyko albo zysk,",
    "szansa (np. 30%) raz.",
    "Hurtownia: ulepsz (zł+stal):",
    "+1 obrażeń za poziom, maks. +3,",
    "od +2 cecha. Zmiana: strata!",
    "Pęknięta ściana / drzwi:",
    "magazyn. Klucz ma problem",
    "(często elita). Ścianę kruszy",
    "Operator albo wybuch, drzwi -",
    "tylko klucz. Skrzynia: sprzęt,",
    "Respekt. Czasem śpi tam elita!",
};
inline constexpr int extras_help_count = 12;

inline constexpr const char* meta_help[] = {   // Jak grać: podsumowanie budowy, wyzwanie tygodnia, fabuła
    "Po budowie: podsumowanie -",
    "co zatrzymało, oś czasu,",
    "najbliższy cel i rada.",
    "Wyzwanie tygodnia: seed i",
    "zasady tygodnia, osobny wynik.",
    "Fabuła: SMS-y w Osiedlu (A).",
};
inline constexpr int meta_help_count = 6;

inline constexpr const char* tips[] = {   // rady kierownika na ekranie harmonogramu między etapami
    "Przytrzymaj B: podgląd problemów",
    "Przytrzymaj A: celownik i zasięg",
    "Kawa z termosu leczy - menu START",
    "Czerwone pola? Masz 2 tury na unik",
    "Moc pod R: ikona pulsuje = gotowa",
    "Paczka: A zakładam, B zostawiam",
    "Przytrzymaj L: mapa odkrytego placu",
    "Telefon (SELECT): Koszty i Zlecenia",
    "Co 4 tury czekania (B) +1 HP",
    "Między etapami wybierz ścieżkę",
    "Drewno: Załataj drogę problemom",
    "Stal: Kładka nad kałużami",
    "Czerwone pola po wybuchu: odejdź",
    "Błoto w akcie I kosztuje turę",
    "START na wyborze: opis statystyk",
    "Akt 0: 3 dokumenty otwierają schody",
    "Po etapie: wybierz 1 z 3 premii",
    "Mokry + prąd = porażenie obok",
    "Złota ramka: elita, lepsza nagroda",
};
inline constexpr int tips_count = 19;

inline constexpr int classes_count = 9;
inline constexpr int stages_count = 12;
inline constexpr int difficulties_count = 3;
inline constexpr int default_difficulty = 1;
inline constexpr int ng_hp_pct_per_tier = 20;
inline constexpr int ng_dmg_bonus_per_tier = 1;
inline constexpr int ng_score_pct_per_tier = 50;

}
