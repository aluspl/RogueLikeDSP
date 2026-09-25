// WYGENEROWANE przez tools/gen_data.py z data/game.json - nie edytuj ręcznie.
#pragma once
#include "core_types.h"

namespace data {

inline constexpr core::weapon_def weapons[] = {
    { "Dziennik budowy", 2, 4, 2, core::stat::intel },
    { "Kielnia", 3, 5, 1, core::stat::str },
    { "Gwoździarka", 2, 3, 3, core::stat::agi },
    { "Próbnik napięcia", 3, 5, 2, core::stat::intel },
    { "Klucz nastawny", 3, 6, 1, core::stat::str },
    { "Szlifierka", 3, 7, 1, core::stat::agi },
    { "Łom", 3, 6, 1, core::stat::str },
    { "Wkrętarka", 2, 5, 2, core::stat::agi },
    { "Poziomica laserowa", 2, 4, 4, core::stat::intel },
    { "Młot wyburzeniowy", 5, 8, 1, core::stat::str },
    { "Tablet z projektem", 3, 5, 2, core::stat::intel },
    { "Miernik laserowy", 2, 5, 3, core::stat::intel },
};

inline constexpr core::class_def classes[] = {
    { "Kierownik budowy", "Trzyma harmonogram w ryzach.", 30, 3, 3, 5, 3, 2, 0, 0, "Odprawa", "Ogłusza widocznych", core::ability_effect::stun, 15 },
    { "Murarz", "Twardy jak pustak.", 32, 5, 2, 1, 4, 0, 1, 1, "Ścianka", "Mur przed wrogiem", core::ability_effect::wall, 15 },
    { "Cieśla-dekarz", "Gwoździe wbija z daleka.", 30, 4, 4, 1, 3, 2, 2, 2, "Seria", "Gwoździe we wszystkich", core::ability_effect::volley, 12 },
    { "Elektryk", "Wie, gdzie jest faza.", 24, 2, 4, 5, 2, 1, 3, 3, "Łańcuch", "Prąd skacze po celach", core::ability_effect::chain, 12 },
    { "Hydraulik", "Żaden przeciek mu nie straszny.", 32, 4, 3, 3, 3, 2, 4, 4, "Zawór", "Odpycha wrogów i leczy", core::ability_effect::flush, 20 },
    { "Glazurnik", "Precyzja co do fugi.", 30, 3, 5, 2, 3, 4, 5, 5, "Wirówka", "Tnie wszystkich dookoła", core::ability_effect::spin, 10 },
};

inline constexpr core::enemy_def enemies[] = {
    { "Przeciek", "Kapie tam, gdzie nie powinno", 6, 1, 3, 0, 6, 10, 6, false, core::status_effect::slip, 14, 2, core::slam_shape::square, "", -1, 0, 0, 0, 0, "" },
    { "Zwarcie", "Iskrzy przy każdej okazji", 5, 2, 4, 0, 7, 12, 7, false, core::status_effect::shock, 17, 1, core::slam_shape::square, "", -1, 0, 0, 0, 0, "" },
    { "Pleśń", "Lubi wilgoć i zimne ściany", 9, 1, 2, 1, 4, 10, 8, false, core::status_effect::poison, 24, 3, core::slam_shape::square, "", -1, 0, 0, 0, 0, "" },
    { "Kornik", "Drąży więźbę po cichu", 7, 1, 3, 1, 5, 10, 9, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "" },
    { "Papierologia", "Brakuje jednej pieczątki", 12, 1, 2, 2, 4, 15, 10, false, core::status_effect::paper, 35, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "" },
    { "Opóźniona dostawa", "Będzie jutro. Na pewno.", 10, 2, 4, 1, 6, 15, 11, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "" },
    { "Ulewa", "Zawsze tuż przed dachem", 8, 2, 3, 0, 8, 12, 12, false, core::status_effect::slip, 28, 3, core::slam_shape::square, "", -1, 0, 0, 0, 0, "" },
    { "Przekroczony budżet", "Rośnie szybciej niż mury", 14, 2, 5, 2, 6, 25, 13, false, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "" },
    { "Nieprzekraczalny Termin", "Nieprzesuwalny. Podobno.", 40, 3, 6, 3, 12, 200, 14, true, core::status_effect::paper, 28, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "" },
    { "Zepsuta Betoniarka", "Kręci się, ale nie tam", 30, 3, 5, 2, 10, 150, 46, true, core::status_effect::none, 0, 0, core::slam_shape::square, "", -1, 0, 0, 0, 0, "" },
    { "Nawałnica", "Leje jak z cebra", 36, 3, 6, 2, 12, 180, 47, true, core::status_effect::slip, 35, 3, core::slam_shape::square, "", -1, 0, 0, 0, 0, "" },
    { "Inspekcja Pracy", "Sprawdza kask i barierki", 30, 3, 5, 2, 12, 170, 50, true, core::status_effect::paper, 30, 0, core::slam_shape::cross, "Kontrola BHP", 4, 6, 2, 2, 60, "Protokół bez uwag" },
};

inline constexpr core::stage_def stages[] = {
    { "Fundamenty", { 0, 3, -1, -1 }, 2, 5, -1, 108, 0, 0 },
    { "Mury parteru", { 4, 5, 3, -1 }, 3, 6, -1, 110, 0, 0 },
    { "Strop", { 5, 7, 3, -1 }, 3, 6, 9, 112, 0, 0 },
    { "Dach", { 6, 3, 0, -1 }, 3, 7, -1, 115, 0, 1 },
    { "Okna i drzwi", { 5, 6, 4, -1 }, 3, 7, 10, 118, 0, 1 },
    { "Instalacje", { 1, 0, 2, -1 }, 3, 7, 11, 121, 0, 2 },
    { "Tynki i wylewki", { 2, 0, 1, -1 }, 3, 8, -1, 124, 0, 2 },
    { "Wykończenie i odbiór", { 2, 7, -1, -1 }, 2, 6, 8, 128, 0, 2 },
};

inline constexpr core::difficulty_def difficulties[] = {
    { "Łatwy", 80, -1, 50 },
    { "Normalny", 100, 0, 100 },
    { "Trudny", 125, 0, 150 },
};

inline constexpr core::upgrade_def upgrades[] = {
    { "Kondycja", "+4 HP na start", core::upgrade_effect::hp, 4, 3, { 10, 20, 35, 0 } },
    { "Szkolenie BHP", "+1 obrona", core::upgrade_effect::def, 1, 2, { 15, 30, 0, 0 } },
    { "Kurs fachowy", "+1 obrażenia", core::upgrade_effect::dmg, 1, 2, { 20, 40, 0, 0 } },
    { "Lepszy termos", "Kawa leczy +4 HP", core::upgrade_effect::coffee, 4, 2, { 10, 20, 0, 0 } },
    { "Dostawy", "+1 znajdźka na etap", core::upgrade_effect::pickups, 1, 2, { 15, 30, 0, 0 } },
    { "Kurs BHP II", "+1 szczęścia", core::upgrade_effect::luck, 1, 2, { 20, 40, 0, 0 } },
    { "Warsztaty", "+1 do statystyki broni", core::upgrade_effect::craft, 1, 2, { 20, 40, 0, 0 } },
};

inline constexpr core::story_msg story_stages[] = {
    { "Anna Nowak", { "Działka nasza! Liczę na", "mocne fundamenty. Uważaj", "na wilgoć w wykopie." } },
    { "Kierownik Marek", { "Stal jest, papierów brak.", "Papierologia już czeka.", "Trzymaj się planu!" } },
    { "Kierownik Marek", { "Szalunki stoją, beton", "jedzie. Oby dostawa", "dojechała na czas!" } },
    { "Anna Nowak", { "Prognoza: ulewa. Zdążysz", "z dachem przed deszczem?", "Kornik też nie śpi." } },
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

inline constexpr core::act_def acts[] = {
    { "Stan surowy", 10, 2 },
    { "Pod dachem", 10, 2 },
    { "Wykończenie", 10, 2 },
};
inline constexpr core::shop_item_def hurtownia[] = {
    { "Kawa z ekspresu", "Pełne HP", 30, core::shop_effect::heal },
    { "Paczka sprzętu", "Losowy sprzęt, min. solidny", 50, core::shop_effect::gear },
    { "Nowe narzędzie", "Losowe odblokowane narzędzie", 40, core::shop_effect::tool },
    { "Siłownia", "+3 max HP na tę budowę", 45, core::shop_effect::maxhp },
    { "Energetyk", "Moc od razu gotowa", 20, core::shop_effect::ability },
};
inline constexpr core::status_def statuses[] = {   // indeks = core::status_effect
    { "", "", "" },
    { "Zatrucie", "Zatr.", "-1 HP/turę" },
    { "Porażenie", "Poraż.", "tracisz turę" },
    { "Poślizg", "Pośl.", "ruch o 2 pola" },
    { "Papierologia", "Papier.", "moc później" },
};
inline constexpr int paper_delay = 3;
inline constexpr int acts_count = 3;
inline constexpr int hurtownia_count = 5;
inline constexpr int slam_every = 4;
inline constexpr int slam_damage_bonus = 2;
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
inline constexpr int enemies_count = 12;
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

inline constexpr core::tool_def tools[] = {
    { 6, 0 },
    { 7, 20 },
    { 8, 30 },
    { 9, 40 },
    { 10, 25 },
    { 11, 35 },
};

inline constexpr int tools_count = 6;
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
};
inline constexpr const char* gear_slots[] = { "Kask", "Rękawice", "Kamizelka" };
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
};
inline constexpr int gear_traits_count = 8;
inline constexpr int gear_decline_xp = 1;
inline constexpr int gear_slots_count = 3;
inline constexpr int gear_solid_from = 70;
inline constexpr int gear_brand_from = 94;
inline constexpr int gear_stage_bonus = 5;

inline constexpr int upgrades_count = 7;
inline constexpr int xp_per_kill = 1;
inline constexpr int xp_per_stage = 5;
inline constexpr int xp_boss = 20;
inline constexpr int start_classes_mask = 19;
inline constexpr int class_cost = 25;
inline constexpr int hard_cost = 40;

inline constexpr int level_thresholds[] = { 10, 25, 45, 70 };
inline constexpr int max_hero_level = 5;
inline constexpr int hp_per_level = 2;
inline constexpr int dmg_levels_mask = 32;
inline constexpr int def_levels_mask = 8;

inline constexpr const char* version = "v0.21.46";   // numer wersji (ekran tytułowy, changelog)

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
};
inline constexpr int tips_count = 9;

inline constexpr int classes_count = 6;
inline constexpr int stages_count = 8;
inline constexpr int difficulties_count = 3;
inline constexpr int default_difficulty = 1;
inline constexpr int ng_hp_pct_per_tier = 20;
inline constexpr int ng_dmg_bonus_per_tier = 1;
inline constexpr int ng_score_pct_per_tier = 50;

}
