#pragma once
// Scenariusze testowe dla playtestera (tools/playtest). Kompilowane tylko z -DPB_SCENARIO=N:
//   make TARGET=scn1 BUILD=build_scn1 USERFLAGS="-DPB_SCENARIO=1"
// Na starcie pierwszej budowy ustawiają sytuację, do której skrypt klawiszy nie dojdzie na ślepo.
//   1 - boss aktu I tuż obok bohatera (zapowiedź ciosu, unik)
//   2 - trzech wrogów w zasięgu (celownik, zmiana celu, karta wroga pod B)
//   3 - wrogowie wokół bohatera (efekty mocy każdego zawodu; wszystkie zawody odblokowane)
//   4 - paczki sprzętu 3 jakości i skrzynka z narzędziem obok bohatera
//   5 - aktywne stany: zatrucie, poślizg, porażenie
//   6 - porównanie sprzętu: założony kask i rękawice, obok (w prawo) paczki: lepszy kask i gorsze rękawice
//   7 - termos: 2 kawy w termosie, ranny bohater (35% HP), kawa na polu w prawo
//   8 - kryt i unik: markowy sprzęt z cechą Szczęście +1, obok (w lewo) wytrzymała Pleśń atakująca bohatera
//   9 - statystyki: Szkolenia Warsztaty i Kurs BHP II kupione, sprzęt z cechami SIŁ/ZRĘ/INT +1,
//       obok (w prawo) skrzynka z Tabletem z projektem (narzędzie INT)
//  10 - uprawnienia i zlecenia: część odznak zdobyta (premie na budowę), zlecenia w toku,
//       Mocarz o krok od ukończenia (moc R + L+R+SELECT = baner), Stały klient o jedną wygraną (baner na końcu)
//  11 - pamiątki: wszystkie odblokowane, Termos babci po 8 budowach (ranga III, wybrany), Kask ojca po 3 (II)
//  12 - wydarzenia na placu: start od etapu 2 z wydarzeniem; każdy kolejny etap bez bossa ma wydarzenie
//       (po kolei z listy; L+R+SELECT przechodzi dalej)
//  13 - Inspekcja Pracy (boss w środku aktu III, sam) kilka pól od bohatera w pełnym sprzęcie: baner "Wszystko zgodnie
//       z BHP!" (2 tury ogłuszenia), potem Kontrola BHP w krzyż (3 tury na zejście), wezwanie Papierologii;
//       L+R+SELECT = pokonanie, baner "Protokół bez uwag" i harmonogram bez Hurtowni
//  14 - pogoda dnia: pierwszy etap w deszczu (kałuże obok bohatera), każdy kolejny etap z kolejną pogodą z listy
//       (Słonecznie, Upał, Mróz, Wiatr, Deszcz...; L+R+SELECT przechodzi dalej)
//  15 - brygada: 100 zł budżetu, wszyscy fachowcy odblokowani, trzy problemy obok bohatera (ogłuszone na chwilę),
//       zatrucie; telefon -> Zespół -> A = Brygada; kolejne etapy też ze 100 zł (L+R+SELECT)
//  16 - tryb inwestora: profil po pierwszej wygranej, włączone Budżet -30% i Problemy +20% HP, rekord stawki Murarza 3;
//       na wyborze zawodu SELECT = lista modyfikatorów
//  17 - wybór ścieżki: L+R+SELECT kończy etap, harmonogram z dwoma wariantami (lewo/prawo, A); kolejne etapy też
//  18 - materiały i naprawy: po 5 cementu/stali/drewna, deszcz (kałuże obok), bohater o 1 dośw. od awansu, obok
//       Kornik z 1 HP (A = usunięty, awans z napisem "AWANS!"), dalej ogłuszona Pleśń; telefon -> Sprzęt -> A:
//       naprawy Załataj i Kładka
//  19 - codzienna budowa: profil z datą 25.09.2026 i wynikami trzech wcześniejszych dni (R na tytule)
//  20 - wygrana: ostatni etap z bossem, dni poprzednich etapów wypełnione (L+R+SELECT = odbiór, harmonogram domu)
//  21 - porażka: 1 HP, obok przebudzony problem (B = czekaj, koniec budowy z motywacją: rekord, zlecenie, Szkolenia)
//  22 - Respekt: profil z Respektem i częścią rang, 3 wygrane (3 nagrody); tytuł -> SELECT -> Koszty -> SELECT = Respekt,
//       A kupuje, SELECT = Nagrody za odbiór
//  23 - Respekt za etap: profil z 12 Respektu; L+R+SELECT = etap zaliczony, baner "Respekt +2"; telefon -> Koszty
//  24 - nagroda za odbiór: profil bez wygranych, ostatni etap z bossem (L+R+SELECT = odbiór, "Nagroda: Młot udarowy")
//  25 - Dekarz: trzy problemy w linii w prawo (ogłuszone), wiatr (zasięg bez zmian); R = Rynna
//  26 - Tynkarz: trzy problemy w grupie 2 pola w prawo; R = Narzut
//  27 - Operator koparki: wytrzymały problem 3 pola w prawo; R = Taran, potem D-pad w prawo = ciosy (czasem odpychają)
//  28 - nowe narzędzia i sprzęt: obok bohatera (w prawo) skrzynki Młot udarowy i Pistolet do kotew, paczki Buty i Pas;
//       telefon -> Sprzęt (5 slotów)
//  29 - strzelcy (etap Mury parteru): Mostek termiczny 3 pola w prawo (strzela i ucieka), Przeciekająca papa 3 pola w dół
//  30 - dzieli się i wybucha: Woda gruntowa (1 HP) w prawo, Pęknięty pustak (1 HP) w lewo - A w prawo = podział,
//       D-pad w lewo = wybuch (czerwone pola, tura na zejście)
//  31 - łata, rośnie, stoi (etap Tynki, akt III - pył): Wilgoć 3 pola w prawo przy rannej Pleśni, Ugięcie stropu 2 w lewo,
//       Kamień w wykopie 3 w górę
//  32 - odpycha i wraca: Osuwisko skarpy obok w prawo, Dziurawa folia (1 HP, ogłuszona) w lewo
//  33 - akt I, błoto (etap Izolacja fundamentów): pole w prawo to błoto (wejście = tura)
//  34 - akt II, porywy (etap Ściany działowe): poryw za 2 tury (licznik w HUD), B = czekaj
//  35 - akt III, pył (etap Tynki i wylewki): mniejsze pole widzenia, pył w powietrzu, płytki i tynk
//  36 - Katalog usterek: wszystkie problemy znane (tytuł -> SELECT -> Katalog, zachowania pod listą)
//  37 - Akt 0, etap Pozwolenie (pieczątki): trzy dokumenty w prawo od bohatera, dalej zamknięte schody
//       (kłódka, HUD 0/3); D-pad w prawo zbiera dokumenty (banery), komplet otwiera schody, wejście = etap zaliczony
//  38 - Akt 0, boss Decyzja odmowna obok bohatera (w prawo), HP tuż nad połową: A = druga faza Odwołanie (baner, +HP,
//       wezwanie Zaginionego wniosku), dalej Stempel ODMOWA; L+R+SELECT = pokonanie (Pozwolenie wydane, premia za akt)
//  39 - samouczek menu na profilu po pierwszej wygranej (tryb inwestora odblokowany): wszystkie dymki tytułu
//       i wyboru zawodu (A dalej, START przy statystykach); B na tytule -> Jak grać -> "pokaż samouczek jeszcze raz"
//  40 - dymki nowości: Respekt, codzienna budowa, Akt 0 (tytuł), tryb inwestora i nowe zawody (wybór zawodu)
//  41 - nagroda Akt 0: 7 wygranych, ostatni etap z bossem (L+R+SELECT = odbiór, "Nagroda: Akt 0: Papierologia"),
//       potem dymek nowości na tytule i nowa budowa od Aktu 0
//  42 - rozpiska obrażeń (#26): profil z Kursem fachowym, Warsztatami, Kursem BHP II, Respektem (Pewna ręka, Oko
//       fachowca), odznakami Seryjny i Zawodowiec, Szczęśliwą kielnią; w budowie poziom 5, projekt wykonawczy,
//       rękawice z cechą Kryt +5%; obok w prawo paczka markowych rękawic (porównanie), dalej skrzynka z Młotem
//       udarowym (baner "teraz -> po zmianie"); 2 pola w górę ogłuszone Kamień w wykopie i Przekroczony budżet (karta
//       pod B: obrażenia w obie strony); wybór zawodu: karta z krytem, START -> A = strony Obrażenia broni
//  43 - premia po etapie (#27): L+R+SELECT = etap zaliczony, oferta 3 zwykłych premii, 40 zł (SELECT = losuj za 25 zł)
//  44 - oferta 3 rzadkich premii; mając Beton B30 i Szelki - dwie z nich włączą synergię ("Synergia!" na karcie)
//  45 - oferta 3 legendarnych; mając Krzesiwo - Transformator włącza synergię Iskrzenie (A = ekran "Synergia!")
//  46 - lista premii w telefonie: 8 premii i 3 synergie (telefon -> Sprzęt -> góra; góra/dół przewija)
//  47 - elity: po jednej każdej cechy wokół bohatera (ogłuszone), złota paleta; B trzymane = karta z przedrostkiem,
//       "Elita: ...", "Zadasz" z obroną elity (Tarcza)
//  48 - mokry + prąd: Elektryk, obok w prawo mokry Przeciek, za nim mokry Kornik, niżej suchy Kornik; A = Porażenie
//  49 - pył + iskra: Glazurnik w akcie III (pył), obok w prawo zapylony Kornik i dwa obok niego; A = Wybuch pyłu
//  50 - zamróz + uderzenie: Mróz, obok w prawo zmrożony Kamień w wykopie; D-pad w prawo = Pęknięcie
//  51 - mokry + prąd na bohaterze: bohater mokry (ikona w HUD), obok Zwarcie; B = czekaj, porażenie bohatera
//  52 - wydarzenia z wyborem (#30): w prawo trzy pola z SMS-em - Tańszy dostawca (30%: Pleśń), Znaleziony projekt
//       (premia 1 z 3 od razu), Stara ostrzałka (ulepszenie narzędzia); D-pad w prawo = SMS, A = odpowiedzi, A = wynik
//  53 - ulepszenie w Hurtowni (#31): etap Strop z bossem, Kielnia+1, 200 zł i 9 stali; L+R+SELECT = boss pokonany,
//       premia, harmonogram, Hurtownia: pierwszy wiersz "Ulepsz narzędzie" (A = +2 i wybór cechy)
//  54 - narzędzie przy ulepszonym (#31): Kielnia+2 z Przebiciem, obok w prawo skrzynka z Młotem udarowym
//       (okno: porównanie i "ulepszenie przepadnie", A zamienia, B zostaje); telefon -> Sprzęt -> A = rozpiska z Ulepszeniem
//  55 - magazyn za pękniętą ścianą (#32): ściana w prawo od bohatera, klucz w kieszeni, 2 pola dalej ogłuszony problem
//       z kluczem (B trzymane: "Ma klucz do magazynu!"); D-pad w prawo = otwarcie, 3x w prawo = skrzynia; L = mapa
//  56 - drzwi magazynu (#32): drzwi w prawo, bez klucza (podpowiedź "potrzebny klucz"), Operator koparki nie pomoże;
//       L = podgląd mapy ze znacznikiem magazynu; telefon -> Zadania: wiersz Magazyn
//  57 - podsumowanie po porażce (#33): etap Dach, oś czasu 4 etapów (premie, SMS-y, magazyn, elita), bohater mokry
//       z 2 HP obok Zwarcia; B = czekaj -> Mokry + prąd, koniec budowy: SMS, 3 strony podsumowania, baner "Nowa
//       wiadomość" (elita, magazyn, SMS) na ekranie końcowym
//  58 - podsumowanie po wygranej: ostatni etap z bossem, oś czasu całej budowy (L+R+SELECT = odbiór)
//  59 - wyzwanie tygodnia (#34): data 29.09.2026 i wyniki dwóch wcześniejszych tygodni (L na tytule; A = start z zasadą)
//  60 - fabuła i Osiedle (#35): 6 wygranych (6 domów, 4 ozdoby), część wątków odblokowana i nieprzeczytana
//       (tytuł -> SELECT -> Osiedle -> A = Wiadomości, A = wątek)
//  v0.21.51 cz. 2: sekretne zlecenia (#39)
//  61 - dymek Nowość (Na styk) na tytule, strona Sekrety: 3 z 8 wykonane (Bez kofeiny, Mokra robota, Na styk), reszta "???" z podpowiedzią (tytuł ->
//       SELECT -> Odznaki -> A x3); Koszty -> SELECT = Respekt: na dole "???" (Zaprawiony w boju - sekret)
//  62 - baner sekretu w budowie: 5. magazyn otwarty (licznik), L+R+SELECT = etap zaliczony, baner "Sekretne zlecenie!"
//       (Poziomica mistrza), potem na tytule dymek "Nowość"
//  63 - Spawacz (wszystkie sekrety wykonane): trzy ogłuszone problemy w linii w prawo; R = Spaw (iskry i dym), A = wybuch pyłu
//  64 - Geodeta: cały plac odkryty od startu (L = mapa), obok w prawo ogłuszony problem; R = Tyczenie, A = cios z premią
//  65 - Majster: moc innego fachu na etap (ikona w HUD), problemy wokół; R = pożyczona moc, L+R+SELECT = kolejny etap, inna moc
//  66 - narzędzia i wygląd z sekretów: kask w paski i złota kielnia, w prawo skrzynki Młot Zenka i Poziomica mistrza,
//       ogłuszony problem dalej (A po podniesieniu młota = kryt ze złotym błyskiem i odepchnięcie); magazyn na etapie (L = mapa
//       z magazynem dzięki Poziomicy)
//  67 - wygrana z sekretami: ostatni etap z bossem, 1 HP (+2 za awans), bez kawy, szybkie etapy; L+R+SELECT = odbiór, banery 3 sekretów
//       (Bez kofeiny, Na styk, Szybka ekipa), na tytule dymki "Nowość"
//  68 - crash magazynu (v0.21.51): jak 55 (ściana w prawo, klucz w kieszeni), ale pełny HUD - 5 zajętych slotów sprzętu,
//       8 premii, stany (zatrucie, poślizg, mokry), materiały, 5 ogłuszonych elit za bohaterem; D-pad w prawo = otwarcie,
//       3x w prawo = skrzynia: sprzęt do zajętego slotu = okno porównania (dawniej przepełniało stos)
//  v0.21.52 cz. a: tempo postępu (#41-#43, #52)
//  69 - profil w trakcie postępu: 140 dośw., Kondycja 2/4, BHP 1/4, jeden zawód i dwa narzędzia kupione (kolejne drożej),
//       odznaki Przed terminem, Twardziel, Seryjny i zlecenie Mocarz (tytuły, dwa kolory kasku), wybrany tytuł Seryjny
//       pogromca i niebieski kask; tytuł -> SELECT = Koszty (Szkolenia z poziomami), -> = Odznaki, A x4 = Tytuły
//       (SELECT wybiera); wybór zawodu: niebieski kask, SELECT = Wygląd (A = kolejny kolor); w budowie 1 HP i obok
//       przebudzony problem: B = czekaj -> koniec budowy, strona 4/4 podsumowania "Postęp" z paskami
#include "core.h"
#include "meta.h"

namespace debug_scenario
{
    // Wolne pole podłogi w odległości [dmin, dmax] od bohatera (najbliższe), -1 gdy brak.
    inline bool free_cell(const core::game& g, int dmin, int dmax, int& ox, int& oy)
    {
        for(int d = dmin; d <= dmax; ++d)
            for(int y = g.hero.y - d; y <= g.hero.y + d; ++y)
                for(int x = g.hero.x - d; x <= g.hero.x + d; ++x)
                    if(core::cheb(x, y, g.hero.x, g.hero.y) == d && g.lv.at(x, y) == core::tile::floor && ! g.occupied(x, y)
                       && ! g.pickup_at(x, y))
                    { ox = x; oy = y; return true; }
        return false;
    }

    inline void place_enemy(core::game& g, int def, int dmin, int dmax, bool awake, int stun)
    {
        int x, y;
        if(g.enemies_count >= core::max_enemies || ! free_cell(g, dmin, dmax, x, y)) return;
        g.spawn(def, x, y);
        g.enemies[g.enemies_count - 1].awake = awake;
        g.enemies[g.enemies_count - 1].stun = int8_t(stun);
    }

    constexpr int F = data::prelude_stages;   // pierwszy etap budowy bez Aktu 0 (Fundamenty)

    // Nagroda Akt 0 (ostatnia na liście).
    inline int act0_reward()
    {
        for(int i = 0; i < data::rewards_count; ++i) if(data::rewards[i].kind == core::reward_kind::act) return i;
        return data::rewards_count;
    }

    // Wszystkie sekretne zlecenia wykonane (nowe zawody, narzędzia, wygląd, Respekt), bez dymków nowości.
    inline void all_secrets(core::profile& p)
    {
        p.secrets = uint16_t((1 << data::secrets_count) - 1); p.secrets_new = 0;
        p.cosmetic = uint8_t(1 << data::cosmetic_stripes);
    }
    inline int secret_index(core::secret_kind k)
    {
        for(int i = 0; i < data::secrets_count; ++i) if(data::secrets[i].kind == k) return i;
        return 0;
    }

    inline void unlock_all(core::profile& p)
    {
        p.classes = uint8_t(((1 << data::classes_count) - 1) & ~data::reward_classes_mask);
        p.rewards = uint8_t(act0_reward());   // nagrody za odbiór: nowe zawody, narzędzia, buty i pas (Akt 0 tylko w 37-41)
        p.hard = 1;
        core::set_flag(p, core::help_seen);
        p.tutorial = 0x3F; p.classes_seen = 0xFFFF;   // samouczek i dymki nowości już obejrzane (poza 39-40)
    }

    // Profil scenariusza (po unlock_all, przed ekranem tytułowym).
    inline void setup_profile(core::profile& p, int scenario)
    {
        if(scenario < 9) return;
        core::set_flag(p, core::prologue_seen);
        for(int i = 0; i < data::upgrades_count; ++i)
            if(data::upgrades[i].effect == core::upgrade_effect::luck || data::upgrades[i].effect == core::upgrade_effect::craft)
                p.levels[i] = uint8_t(data::upgrades[i].levels);
        p.tools = uint8_t((1 << data::tools_count) - 1);
        if(scenario == 10)
        {
            p.badges = uint16_t((1 << data::badge_bez_usterek) | (1 << data::badge_seryjny) | (1 << data::badge_kolekcjoner)
                                | (1 << data::badge_osiedle));
            p.kills_total = 150; p.brand_total = 2; p.class_wins = 3;
            for(int i = 0; i < data::contracts_count; ++i)
            {
                if(data::contracts[i].kind == core::contract_kind::powers) p.powers_total = uint16_t(data::contracts[i].target - 1);
                if(data::contracts[i].kind == core::contract_kind::wins) p.wins = data::contracts[i].target - 1;   // wygrana = baner na końcu
            }
        }
        if(scenario == 19)
        {
            core::set_daily_date(p, 2026, 9, 25);
            int day = core::daily_number(2026, 9, 25);
            core::record_daily(p, day - 1, 3120, true);
            core::record_daily(p, day - 2, 1480, false);
            core::record_daily(p, day - 4, 2210, false);
        }
        if(scenario == 21) { p.best = 4200; p.xp = 12; p.kills_total = 180; }
        if(scenario == 57) { p.best = 4200; p.respect = 23; p.respect_total = 60; p.runs = 3; p.kills_total = 120; p.story = 1; }
        if(scenario == 59)
        {
            core::set_daily_date(p, 2026, 9, 29);
            const int w = core::weekly_number(2026, 9, 29);
            core::record_weekly(p, w - 1, 2890, true);
            core::record_weekly(p, w - 2, 1320, false);
            p.story = 0xFFFFFFFFu >> (32 - data::story_arc_count);   // bez banera fabuły przy starcie
        }
        if(scenario == 60)
        {
            p.wins = 6; p.runs = 9; p.rewards = 6;
            for(int i = 0; i < 6; ++i) p.houses[p.houses_count++] = uint8_t((i % data::classes_count) | ((i % 4) << 4));
            core::catalog_add(p, data::enemy_betoniarka);
            core::story_check(p, nullptr);
            p.story_new = p.story & ~3u;   // dwa pierwsze przeczytane
        }
        if(scenario == 22)
        {
            p.respect = 180; p.respect_total = 420; p.wins = 3; p.rewards = 3;
            p.respect_ranks[0] = 1; p.respect_ranks[5] = 2; p.respect_ranks[8] = 3; p.respect_ranks[12] = 1;
        }
        if(scenario == 23) { p.respect = 12; p.respect_total = 12; }
        if(scenario == 61)
        {
            p.secrets = uint16_t((1 << secret_index(core::secret_kind::no_coffee_win)) | (1 << secret_index(core::secret_kind::shock_combos))
                                 | (1 << secret_index(core::secret_kind::low_hp_win)));
            p.respect = 40; p.respect_total = 90; p.wins = 3; p.rewards = 3;
            p.secrets_new = uint16_t(1 << secret_index(core::secret_kind::low_hp_win));   // dymek "Nowość" na tytule
        }
        if(scenario >= 63 && scenario <= 66) all_secrets(p);
        if(scenario == 67) { p.wins = 4; p.rewards = 4; p.runs = 6; }
        if(scenario == 69)
        {
            p.xp = 140; p.best = 3100; p.runs = 5; p.wins = 2; p.rewards = 2;
            for(int i = 0; i < data::upgrades_count; ++i)
            {
                if(data::upgrades[i].effect == core::upgrade_effect::hp) p.levels[i] = 2;
                if(data::upgrades[i].effect == core::upgrade_effect::taken_pct) p.levels[i] = 1;
            }
            for(int c = 0; c < data::classes_count; ++c) if(core::class_for_sale(c)) { p.classes = uint8_t(p.classes | (1 << c)); break; }
            p.tools = 0;
            for(int i = 0, n = 0; i < data::tools_count && n < 2; ++i) if(data::tools[i].shop) { p.tools = uint8_t(p.tools | (1 << i)); ++n; }
            p.badges = uint16_t((1 << data::badge_przed_terminem) | (1 << data::badge_twardziel) | (1 << data::badge_seryjny));
            for(int i = 0; i < data::contracts_count; ++i) if(data::contracts[i].kind == core::contract_kind::powers) p.contracts = uint8_t(1 << i);
            p.title = uint8_t(data::badge_seryjny + 1);
            p.helmet = uint8_t(data::badges[data::badge_przed_terminem].cosmetic + 1);
            p.kills_total = 90; p.class_wins = 3;
        }
        if(scenario == 36) { p.catalog = 0xFFFF; p.catalog_hi = 0xFFFFFFFFu; }
        if(scenario == 37 || scenario == 38) { p.rewards = uint8_t(data::rewards_count); p.wins = 8; }   // Akt 0 odebrany
        if(scenario == 39) { p.tutorial = 0; p.wins = 1; p.rewards = 1; p.investor = 0; core::set_flag(p, core::prologue_seen); }
        if(scenario == 40)
        {
            p.tutorial = uint16_t(core::tut_title | core::tut_class); p.classes_seen = 0;
            p.respect_total = 14; p.respect = 14; p.runs = 9; p.wins = 8; p.rewards = uint8_t(data::rewards_count);
        }
        if(scenario == 41) { p.wins = 7; p.rewards = uint8_t(act0_reward()); p.respect = 20; }
        if(scenario == 42)
        {
            for(int i = 0; i < data::upgrades_count; ++i)
                if(data::upgrades[i].effect == core::upgrade_effect::dmg_pct) p.levels[i] = uint8_t(data::upgrades[i].levels);
            for(int i = 0; i < data::respect_count; ++i)
                if(data::respect[i].effect == core::respect_effect::dmg_pct || data::respect[i].effect == core::respect_effect::crit)
                    core::set_respect_rank(p, i, data::respect[i].ranks);
            p.badges = uint16_t((1 << data::badge_seryjny) | (1 << data::badge_zawodowiec));
            for(int i = 0; i < data::contracts_count; ++i) if(data::contracts[i].keepsake >= 0) p.contracts = uint8_t(p.contracts | (1 << i));
            p.keepsake = 3;   // Szczęśliwa kielnia (+2 szczęścia)
        }
        if(scenario == 24) { p.wins = 0; p.rewards = 0; p.respect = 30; }
        if(scenario == 16)
        {
            p.wins = 1;
            p.investor = 0x05;
            p.best_stake[1] = 3;
        }
        if(scenario == 11)
        {
            p.badges = uint16_t(p.badges | (1 << data::badge_bez_usterek));
            for(int i = 0; i < data::contracts_count; ++i) if(data::contracts[i].keepsake >= 0) p.contracts = uint8_t(p.contracts | (1 << i));
            p.keepsake_runs[0] = 8; p.keepsake_runs[1] = 3; p.keepsake_runs[3] = 1;
            p.keepsake = 1;
        }
    }

    inline int trait_index(core::trait_effect e)
    {
        for(int i = 0; i < data::gear_traits_count; ++i) if(data::gear_traits[i].effect == e) return i;
        return 0;
    }

    inline int tool_index(const char* weapon_name)
    {
        for(int i = 0; i < data::tools_count; ++i)
        {
            const char* a = data::weapons[data::tools[i].weapon].name;
            const char* b = weapon_name;
            while(*a && *a == *b) { ++a; ++b; }
            if(*a == *b) return i;
        }
        return 0;
    }

    // Scenariusz 12: wymusza wydarzenie na placu (kolejne z listy), jeśli los go nie dał.
    inline void force_event(core::game& g)
    {
        static int next = 0;
        if(g.stage_event >= 0 || g.stage == g.first_stage || data::stages[g.stage].boss >= 0) return;
        g.apply_event(next++ % data::site_events_count);
    }

    // Scenariusz 14: wymusza pogodę (kolejna z listy, od deszczu).
    inline void force_weather(core::game& g)
    {
        static int next = data::weather_count - 1;
        g.weather = int8_t(next % data::weather_count);
        next = (next + 1) % data::weather_count;
        g.push(core::message().add("Pogoda: ").add(g.wdef().name).add(" (").add(g.wdef().short_name).add(")").as(g.wdef().bad ? core::bad : core::good));
    }

    // Wołane po przejściu na kolejny etap (harmonogram, Hurtownia).
    inline void after_next_stage(core::game& g, int scenario)
    {
        if(scenario == 12) force_event(g);
        if(scenario == 14) { force_weather(g); g.update_fov(); }
        if(scenario == 15) g.cash = 100;
    }

    // Zawód z mocą e (nowe zawody z nagród za odbiór).
    inline int class_of(core::ability_effect e)
    {
        for(int c = 0; c < data::classes_count; ++c) if(data::classes[c].ability == e) return c;
        return 0;
    }

    // Podłoga w prostokącie wokół bohatera (scenariusze mocy potrzebują miejsca w linii).
    inline void clear_area(core::game& g, int dx0, int dy0, int dx1, int dy1)
    {
        for(int y = g.hero.y + dy0; y <= g.hero.y + dy1; ++y)
            for(int x = g.hero.x + dx0; x <= g.hero.x + dx1; ++x)
                if(x >= 1 && y >= 1 && x < core::map_w - 1 && y < core::map_h - 1 && g.lv.t[y][x] == core::tile::wall) g.lv.t[y][x] = core::tile::floor;
    }

    inline void place_at(core::game& g, int def, int dx, int dy, int stun)
    {
        int x = g.hero.x + dx, y = g.hero.y + dy;
        if(g.enemies_count >= core::max_enemies || g.lv.at(x, y) != core::tile::floor || g.occupied(x, y)) return;
        g.spawn(def, x, y);
        g.enemies[g.enemies_count - 1].awake = true;
        g.enemies[g.enemies_count - 1].stun = int8_t(stun);
    }

    // Premia po nazwie (data::boons), -1 gdy brak.
    inline int boon_index(const char* name)
    {
        for(int b = 0; b < data::boons_count; ++b)
        {
            const char* x = data::boons[b].name;
            const char* y = name;
            while(*x && *x == *y) { ++x; ++y; }
            if(*x == *y) return b;
        }
        return -1;
    }
    inline void give_boon(core::game& g, const char* name)
    {
        int b = boon_index(name);
        if(b >= 0) { g.boon_offer[0] = int8_t(b); g.pick_boon(0); }
    }
    inline void offer(core::game& g, const char* b0, const char* b1, const char* b2)
    {
        g.boon_offer[0] = int8_t(boon_index(b0)); g.boon_offer[1] = int8_t(boon_index(b1)); g.boon_offer[2] = int8_t(boon_index(b2));
    }

    // Wołane przed ekranem premii po etapie (scenariusze 43-45: oferta danej rzadkości).
    inline void before_boon_pick(core::game& g, int scenario)
    {
        if(scenario == 43) { offer(g, "Koniczyna", "Szczęśliwa moneta", "Termos z bufetu"); g.cash = 40; }
        if(scenario == 44) offer(g, "Hartowana kielnia", "Instrukcja BHP", "Tarcza tnąca");
        if(scenario == 45) offer(g, "Transformator", "Młot mistrza", "Anioł stróż");
    }

    // Scenariusze 55-56: etap z magazynem danego rodzaju, ściana w prawo od pokoju; bohater przed nią, bez strażnika.
    inline void secret_stage(core::game& g, int kind)
    {
        const uint32_t base = g.run_seed;
        for(uint32_t k = 1; k < 4000; ++k)
        {
            g.run_seed = base + k * 7919u;
            g.start_stage(F + 1);
            if(g.has_secret() && g.secret_kind == kind && g.secret_dir == 0) break;
        }
        g.hero.x = int8_t(g.secret_front_x()); g.hero.y = int8_t(g.secret_front_y());
        for(int i = 0; i < g.enemies_count; ++i)   // bez strażnika i bez problemów tuż obok
            if(g.in_secret(g.enemies[i].x, g.enemies[i].y) || core::cheb(g.enemies[i].x, g.enemies[i].y, g.hero.x, g.hero.y) <= 3)
                g.enemies[i].alive = false;
        g.key_holder = -1; g.stage_event = -1;   // bez SMS-a z placu (skrypt: jedna wiadomość)
        for(int i = 0; i < g.pickups_count; ++i) if(g.pickups[i].type == core::event_tile) g.pickups[i].active = false;
        for(int y = g.hero.y - 3; y <= g.hero.y + 3; ++y)   // odkryty kawałek obok (ściana widoczna)
            for(int x = g.hero.x - 3; x <= g.hero.x + 3; ++x) if(g.lv.in(x, y) && ! g.in_secret(x, y)) g.fov[y][x] = core::remembered;
    }
    // Pole wydarzenia (po nazwie) na polu (dx, 0) od bohatera.
    inline void event_at(core::game& g, const char* name, int dx)
    {
        for(int e = 0; e < data::choice_events_count; ++e)
        {
            const char* x = data::choice_events[e].name; const char* y = name;
            while(*x && *x == *y) { ++x; ++y; }
            if(*x == *y && g.pickups_count < core::max_pickups)
                g.pickups[g.pickups_count++] = { int8_t(g.hero.x + dx), g.hero.y, core::event_tile, true, uint8_t(e) };
        }
    }

    // Wołane raz, na wejściu na pierwszy etap budowy.
    inline void apply(core::game& g, int scenario)
    {
        if(scenario == 48 || scenario == 49)   // kombinacje: Elektryk (prąd) / Glazurnik (iskra)
        {
            core::run_mods m = g.bonus;
            g.new_run(scenario == 48 ? class_of(core::ability_effect::chain) : class_of(core::ability_effect::spin), g.run_seed, g.diff, m);
        }
        if(scenario >= 63 && scenario <= 65)   // v0.21.51 cz. 2: zawód z sekretu niezależnie od wyboru na ekranie zawodu
        {
            const core::ability_effect e[3] = { core::ability_effect::weld, core::ability_effect::mark, core::ability_effect::borrow };
            core::run_mods m = g.bonus;
            g.new_run(class_of(e[scenario - 63]), g.run_seed, g.diff, m);
        }
        if(scenario >= 25 && scenario <= 27)   // nowy zawód niezależnie od wyboru na ekranie zawodu
        {
            const core::ability_effect e[3] = { core::ability_effect::line, core::ability_effect::splash, core::ability_effect::ram };
            core::run_mods m = g.bonus;
            g.new_run(class_of(e[scenario - 25]), g.run_seed, g.diff, m);
        }
        switch(scenario)
        {
            case 1:
            {
                int boss_stage = F;
                while(data::stages[boss_stage].boss < 0) ++boss_stage;
                g.start_stage(boss_stage);
                core::actor& b = g.enemies[g.boss];
                int x, y;
                if(free_cell(g, 3, 4, x, y)) { b.x = int8_t(x); b.y = int8_t(y); }
                b.awake = true;
                g.slam_counter = data::slam_every - 1;   // zapowie cios przy najbliższej turze
                break;
            }
            case 2:
                g.enemies_count = 0;
                place_enemy(g, data::enemy_papierologia, 1, 1, false, 20);
                place_enemy(g, data::enemy_kornik, 2, 2, false, 20);
                place_enemy(g, data::enemy_plesn, 2, 3, false, 20);
                break;
            case 3:
                g.enemies_count = 0;
                for(int i = 0; i < 4; ++i) place_enemy(g, data::enemy_papierologia, 1, 2, false, 40);
                place_enemy(g, data::enemy_kornik, 3, 4, true, 0);
                break;
            case 4:
            {
                g.pickups_count = 0;
                const int args[4] = { 0 * 3 + 0, 1 * 3 + 1, 2 * 3 + 2, 1 };
                for(int i = 0; i < 4; ++i)
                {
                    int x, y;
                    if(! free_cell(g, 1, 2, x, y)) break;
                    g.pickups[g.pickups_count++] = { int8_t(x), int8_t(y), uint8_t(i < 3 ? core::gear_box : core::tool),
                                                     true, uint8_t(args[i]) };
                }
                break;
            }
            case 5:
                g.apply_status(core::status_effect::poison, 6);
                g.apply_status(core::status_effect::slip, 6);
                g.apply_status(core::status_effect::shock, 1);
                break;
            case 6:
            {
                g.pickups_count = 0;
                g.equip(0, 0, 0);                                   // kask budowlany, Szczęście +1
                g.equip(1, 2, 4);                                   // rękawice markowe, Odnowienie mocy -1
                g.pickups[g.pickups_count++] = { int8_t(g.hero.x + 1), g.hero.y, core::gear_box, true, uint8_t(0 * 3 + 2), 3 };
                if(g.lv.at(g.hero.x + 2, g.hero.y) == core::tile::floor)
                    g.pickups[g.pickups_count++] = { int8_t(g.hero.x + 2), g.hero.y, core::gear_box, true, uint8_t(1 * 3 + 1), 1 };
                break;
            }
            case 7:
                g.pickups_count = 0;
                g.thermos = 2;
                g.hero.hp = int16_t(g.hero.max_hp * 35 / 100);
                g.pickups[g.pickups_count++] = { int8_t(g.hero.x + 1), g.hero.y, core::coffee, true };
                break;
            case 8:
            {
                for(int i = 0; i < data::gear_slots_count; ++i) g.equip(i, 2, 0);   // szczęście +3: kryt i unik
                g.enemies_count = 0;
                g.spawn(data::enemy_plesn, g.hero.x - 1, g.hero.y);
                core::actor& e = g.enemies[0];
                e.hp = e.max_hp = 300; e.awake = true;
                g.hero.max_hp = g.hero.hp = 300;
                break;
            }
            case 9:
                g.pickups_count = 0;
                g.equip(0, 1, trait_index(core::trait_effect::str));
                g.equip(1, 1, trait_index(core::trait_effect::agi));
                g.equip(2, 1, trait_index(core::trait_effect::intel));
                g.pickups[g.pickups_count++] = { int8_t(g.hero.x + 1), g.hero.y, core::tool, true, uint8_t(tool_index("Tablet z projektem")) };
                break;
            case 10:
                g.enemies_count = 0;
                place_enemy(g, data::enemy_kornik, 2, 3, true, 0);
                break;
            case 12:
                g.start_stage(F + 1);
                force_event(g);
                break;
            case 14:
                force_weather(g);   // deszcz: kałuże widać wokół bohatera
                break;
            case 15:
                g.cash = 100;
                g.bonus.helpers = (1 << data::brigade_count) - 1;
                g.enemies_count = 0;
                place_enemy(g, data::enemy_papierologia, 1, 1, true, 3);
                place_enemy(g, data::enemy_kornik, 2, 2, true, 3);
                place_enemy(g, data::enemy_plesn, 2, 2, true, 3);
                g.apply_status(core::status_effect::poison, 6);
                break;
            case 13:
            {
                int st = F;
                while(data::stages[st].boss != data::enemy_inspekcja) ++st;
                g.start_stage(st);
                for(int i = 0; i < g.boss; ++i) g.enemies[i].alive = false;          // sam boss (bez problemów etapu)
                for(int i = 0; i < data::gear_slots_count; ++i) g.equip(i, 0, 0);   // pełny sprzęt: kask, rękawice, kamizelka
                core::actor& b = g.enemies[g.boss];
                int x, y;
                if(free_cell(g, 3, 3, x, y)) { b.x = int8_t(x); b.y = int8_t(y); }
                b.awake = true;
                g.slam_counter = data::slam_every - 1;   // Kontrola BHP zaraz po ogłuszeniu
                g.summon_counter = data::enemies[data::enemy_inspekcja].summon_every - 1;
                break;
            }
            case 18:
            {
                for(int m = 0; m < data::materials_count; ++m) g.mats[m] = 5;
                for(int i = 0; i < data::weather_count; ++i) if(data::weather[i].effect == core::weather_effect::rain) g.weather = int8_t(i);
                g.run_xp = data::level_thresholds[0] - 1;
                g.enemies_count = 0;
                place_enemy(g, data::enemy_kornik, 1, 1, true, 5);
                g.enemies[0].hp = 1;
                place_enemy(g, data::enemy_plesn, 3, 3, true, 30);
                break;
            }
            case 20:
            {
                g.start_stage(data::stages_count - 1);
                for(int s = g.first_stage; s < data::stages_count - 1; ++s) g.stage_days[s] = uint16_t(18 + (s * 7) % 11);
                g.score = 4800;
                g.turns = 240;
                g.stage_start_turn = 200;
                break;
            }
            case 23:
                g.enemies_count = 0;
                place_enemy(g, data::enemy_kornik, 3, 4, false, 0);
                break;
            case 24:
            {
                g.start_stage(data::stages_count - 1);
                for(int s = g.first_stage; s < data::stages_count - 1; ++s) g.stage_days[s] = uint16_t(15 + (s * 5) % 9);
                g.score = 3900;
                g.respect = 24;
                break;
            }
            case 25:
            {
                clear_area(g, -1, -2, 6, 2);
                g.enemies_count = 0;
                for(int k = 2; k <= 4; ++k) place_at(g, k == 3 ? data::enemy_kornik : data::enemy_przeciek, k, 0, 30);
                for(int i = 0; i < data::weather_count; ++i) if(data::weather[i].effect == core::weather_effect::wind) g.weather = int8_t(i);
                g.push(core::message().add("Pogoda: ").add(g.wdef().name).add(" (").add(g.wdef().short_name).add(")").as(core::bad));
                break;
            }
            case 26:
            {
                clear_area(g, -1, -2, 5, 2);
                g.enemies_count = 0;
                place_at(g, data::enemy_kornik, 2, 0, 30);
                place_at(g, data::enemy_przeciek, 3, 1, 30);
                place_at(g, data::enemy_plesn, 3, -1, 30);
                place_at(g, data::enemy_kornik, -1, 2, 30);
                break;
            }
            case 27:
            {
                clear_area(g, -1, -2, 6, 2);
                g.enemies_count = 0;
                place_at(g, data::enemy_budzet, 3, 0, 30);
                if(g.enemies_count > 0) g.enemies[0].hp = g.enemies[0].max_hp = 80;   // wytrzyma szarżę i kilka ciosów (odepchnięcie)
                place_at(g, data::enemy_kornik, -3, 2, 40);   // dalej niż cel szarży
                break;
            }
            case 28:
            {
                clear_area(g, -1, -1, 5, 1);
                g.pickups_count = 0;
                int udarowy = -1, kotwy = -1, boots = -1, belt = -1, slip = trait_index(core::trait_effect::slip_res);
                for(int i = 0; i < data::tools_count; ++i) if(data::tools[i].reward) (udarowy < 0 ? udarowy : kotwy) = i;
                for(int i = 0; i < data::gear_slots_count; ++i)
                {
                    if(data::gear[i * 3].stat == core::gear_stat::dodge) boots = i;
                    if(data::gear[i * 3].stat == core::gear_stat::thermos) belt = i;
                }
                g.pickups[g.pickups_count++] = { int8_t(g.hero.x + 1), g.hero.y, core::tool, true, uint8_t(udarowy) };
                g.pickups[g.pickups_count++] = { int8_t(g.hero.x + 2), g.hero.y, core::gear_box, true, uint8_t(boots * 3 + 2), uint8_t(slip) };
                g.pickups[g.pickups_count++] = { int8_t(g.hero.x + 3), g.hero.y, core::gear_box, true, uint8_t(belt * 3 + 1), 0 };
                g.pickups[g.pickups_count++] = { int8_t(g.hero.x + 4), g.hero.y, core::tool, true, uint8_t(kotwy) };
                g.enemies_count = 0;
                break;
            }
            case 29:
                g.start_stage(F + 2);
                g.enemies_count = 0;
                clear_area(g, -1, -1, 4, 4);
                place_at(g, data::enemy_mostek, 3, 0, 0);
                place_at(g, data::enemy_papa, 0, 3, 0);
                break;
            case 30:
                g.enemies_count = 0;
                clear_area(g, -3, -2, 3, 2);
                place_at(g, data::enemy_woda, 1, 0, 6);
                place_at(g, data::enemy_pustak, -1, 0, 6);
                for(int i = 0; i < g.enemies_count; ++i) g.enemies[i].hp = 1;
                break;
            case 31:
                g.start_stage(F + 8);
                g.enemies_count = 0;
                clear_area(g, -3, -3, 4, 3);
                place_at(g, data::enemy_wilgoc, 3, 0, 0);
                place_at(g, data::enemy_plesn, 3, 2, 40);
                if(g.enemies_count > 1) g.enemies[1].hp = int16_t(g.enemies[1].max_hp - 7);
                place_at(g, data::enemy_ugiecie, -2, 0, 0);
                place_at(g, data::enemy_kamien, 0, -3, 0);
                break;
            case 32:
                g.enemies_count = 0;
                clear_area(g, -2, -1, 3, 1);
                place_at(g, data::enemy_osuwisko, 1, 0, 0);
                place_at(g, data::enemy_folia, -1, 0, 8);
                if(g.enemies_count > 1) g.enemies[1].hp = 1;
                break;
            case 33:
            {
                g.start_stage(F + 1);
                g.enemies_count = 0;
                int best = 999, hx = g.hero.x, hy = g.hero.y;
                for(int y = 1; y < core::map_h - 1; ++y)   // najbliższe błoto z wolnym polem po lewej
                    for(int x = 2; x < core::map_w - 1; ++x)
                        if(g.mud(x, y) && g.lv.at(x - 1, y) == core::tile::floor && ! g.mud(x - 1, y) && core::cheb(x, y, hx, hy) < best)
                        { best = core::cheb(x, y, hx, hy); g.hero.x = int8_t(x - 1); g.hero.y = int8_t(y); }
                break;
            }
            case 34:
                g.start_stage(F + 5);
                g.enemies_count = 0;
                g.stage_start_turn = g.turns - (data::acts[1].mech_value - 2);   // poryw za 2 tury
                break;
            case 35:
                g.start_stage(F + 8);
                for(int i = 0; i < g.enemies_count; ++i) g.enemies[i].stun = 60;
                break;
            case 37:   // Akt 0: dokumenty w linii w prawo, zamknięte schody za nimi
            {
                clear_area(g, -1, -1, 7, 1);
                g.enemies_count = 0;
                int k = 0;
                for(int i = 0; i < g.pickups_count; ++i)
                    if(g.pickups[i].type == core::document) { g.pickups[i].x = int8_t(g.hero.x + 1 + k); g.pickups[i].y = g.hero.y; ++k; }
                    else g.pickups[i].active = false;
                g.lv.t[g.stairs_y][g.stairs_x] = core::tile::floor;
                g.stairs_x = g.hero.x + 5; g.stairs_y = g.hero.y;
                g.lv.t[g.stairs_y][g.stairs_x] = core::tile::stairs;
                place_at(g, data::enemy_niezgodnosc, 3, -1, 40);   // ogłuszony strzelec obok (do obejrzenia)
                break;
            }
            case 38:   // Akt 0: Decyzja odmowna obok, HP tuż nad progiem drugiej fazy
            {
                g.start_stage(g.first_stage + 1);
                for(int i = 0; i < g.boss; ++i) g.enemies[i].alive = false;
                clear_area(g, -2, -2, 3, 2);
                core::actor& b = g.enemies[g.boss];
                b.x = int8_t(g.hero.x + 1); b.y = g.hero.y;
                b.awake = true; b.stun = 2;
                const core::enemy_def& bd = data::enemies[b.def_id];
                b.hp = int16_t(b.max_hp * bd.phase_pct / 100 + 1);
                g.hero.max_hp = g.hero.hp = 120;
                break;
            }
            case 41:
            {
                g.start_stage(data::stages_count - 1);
                for(int s = g.first_stage; s < data::stages_count - 1; ++s) g.stage_days[s] = uint16_t(16 + (s * 3) % 7);
                g.score = 4100;
                break;
            }
            case 42:
            {
                clear_area(g, -2, -3, 3, 1);
                g.enemies_count = 0; g.pickups_count = 0;
                while(g.hero_level < 5) g.gain_xp(10);                   // poziom 5: +1 obrażeń, moc III
                ++g.dmg_bonus;                                            // projekt wykonawczy
                g.equip(0, 0, trait_index(core::trait_effect::luck));    // kask, Szczęście +1
                g.equip(1, 1, trait_index(core::trait_effect::crit));    // rękawice wzmacniane, Kryt +5%
                g.pickups[g.pickups_count++] = { int8_t(g.hero.x + 1), g.hero.y, core::gear_box, true, uint8_t(1 * 3 + 2),
                                                 uint8_t(trait_index(core::trait_effect::str)) };
                g.pickups[g.pickups_count++] = { int8_t(g.hero.x + 2), g.hero.y, core::tool, true, uint8_t(tool_index("Młot udarowy")) };
                place_at(g, data::enemy_kamien, 0, -2, 90);
                place_at(g, data::enemy_budzet, 1, -2, 90);
                break;
            }
            case 44: give_boon(g, "Beton B30"); give_boon(g, "Szelki asekuracyjne"); break;
            case 45: give_boon(g, "Krzesiwo"); break;
            case 46:
                for(const char* n : { "Beton B30", "Hartowana kielnia", "Wąż ogrodowy", "Przedłużacz", "Podwójne espresso",
                                      "Termos z bufetu", "Złota podkowa", "Szczęśliwa moneta" })
                    give_boon(g, n);
                break;
            case 47:   // elity: każda cecha, ogłuszone wokół bohatera
            {
                clear_area(g, -2, -2, 2, 2);
                g.enemies_count = 0;
                const int defs[5] = { data::enemy_przeciek, data::enemy_kornik, data::enemy_plesn, data::enemy_zwarcie, data::enemy_woda };
                const int8_t pos[5][2] = { { 1, 0 }, { 2, -1 }, { -1, -2 }, { 0, 2 }, { -2, 1 } };
                for(int t = 0; t < 5 && t < data::elites_count; ++t)
                {
                    place_at(g, defs[t], pos[t][0], pos[t][1], 90);
                    g.make_elite(g.enemies_count - 1, t);
                }
                break;
            }
            case 48:
            {
                clear_area(g, -1, -1, 3, 2);
                g.enemies_count = 0;
                place_at(g, data::enemy_przeciek, 1, 0, 90);
                place_at(g, data::enemy_kornik, 2, 0, 90);
                place_at(g, data::enemy_kornik, 1, 1, 90);
                for(int i = 0; i < g.enemies_count; ++i) g.enemies[i].hp = g.enemies[i].max_hp = 40;
                g.enemies[1].wet = 9;   // mokry obok celu; [2] suchy
                break;
            }
            case 49:
            {
                g.start_stage(F + 8);
                clear_area(g, -1, -2, 3, 2);
                g.enemies_count = 0; g.boss = -1;
                place_at(g, data::enemy_kornik, 1, 0, 90);
                place_at(g, data::enemy_kornik, 2, -1, 90);
                place_at(g, data::enemy_kornik, 2, 1, 90);
                for(int i = 0; i < g.enemies_count; ++i) { g.enemies[i].hp = g.enemies[i].max_hp = 40; g.enemies[i].flags = core::actor_dusty; }
                break;
            }
            case 50:
            {
                clear_area(g, -1, -1, 2, 1);
                g.enemies_count = 0;
                for(int w = 0; w < data::weather_count; ++w) if(data::weather[w].effect == core::weather_effect::frost) g.weather = int8_t(w);
                place_at(g, data::enemy_kamien, 1, 0, 90);
                g.enemies[0].flags = core::actor_frozen;
                break;
            }
            case 52:
            {
                clear_area(g, -1, -1, 4, 1);
                g.enemies_count = 0; g.pickups_count = 0; g.key_holder = -1;
                event_at(g, "Tańszy dostawca", 1);
                event_at(g, "Znaleziony projekt", 2);
                event_at(g, "Stara ostrzałka", 3);
                g.weapon_lvl = 1;   // ostrzałka: +2 i wybór cechy
                break;
            }
            case 53:
            {
                g.start_stage(F + 3);   // Strop: boss aktu I, potem Hurtownia
                g.cash = 200; g.mats[1] = 9; g.weapon_lvl = 1;
                break;
            }
            case 54:
            {
                clear_area(g, -1, -1, 2, 1);
                g.enemies_count = 0; g.pickups_count = 0; g.key_holder = -1;
                g.weapon_lvl = 2; g.weapon_trait = 0;
                g.pickups[g.pickups_count++] = { int8_t(g.hero.x + 1), g.hero.y, core::tool, true, uint8_t(tool_index("Młot udarowy")) };
                place_at(g, data::enemy_kamien, 0, -1, 90);
                break;
            }
            case 55:
            {
                secret_stage(g, 0);
                g.keys = 1;
                int x = -1, y = -1;   // wolne pole 2-3 od bohatera, poza magazynem
                for(int yy = g.hero.y - 3; yy <= g.hero.y + 3 && x < 0; ++yy)
                    for(int xx = g.hero.x - 3; xx <= g.hero.x; ++xx)
                        if(core::cheb(xx, yy, g.hero.x, g.hero.y) >= 2 && g.lv.at(xx, yy) == core::tile::floor && ! g.occupied(xx, yy)
                           && ! g.pickup_at(xx, yy) && ! g.in_secret(xx, yy)) { x = xx; y = yy; break; }
                if(x >= 0 && g.enemies_count < core::max_enemies)
                {
                    g.spawn(data::enemy_kornik, x, y);
                    g.enemies[g.enemies_count - 1].awake = true; g.enemies[g.enemies_count - 1].stun = 90;
                    g.key_holder = int8_t(g.enemies_count - 1);
                }
                break;
            }
            case 56:
            {
                secret_stage(g, 1);
                break;
            }
            case 51:
            {
                clear_area(g, -1, -1, 1, 1);
                g.enemies_count = 0;
                g.hero.max_hp = g.hero.hp = 60;
                g.apply_status(core::status_effect::wet, 6);
                place_at(g, data::enemy_zwarcie, 1, 0, 0);
                break;
            }
            case 57:
            case 58:
            {
                const int last = scenario == 57 ? F + 4 : data::stages_count - 1;
                g.start_stage(last);
                g.boons = 0;
                for(int s = g.first_stage; s < last; ++s)
                {
                    g.stage_days[s] = uint16_t(14 + (s * 7) % 11);
                    g.stage_kill_log[s] = uint8_t(5 + s % 4);
                    g.stage_boon[s] = int8_t((s * 5) % data::boons_count);
                    g.boons |= uint64_t(1) << g.stage_boon[s];
                    if(s % 3 == 1) g.stage_event_log[s] = uint8_t(((s / 3) % data::choice_events_count) * 4 + s % 2);
                }
                g.stage_flags[g.first_stage + 1] = core::recap_secret | core::recap_elite;
                g.stage_flags[g.first_stage + 3] = core::recap_boss | core::recap_combo | core::recap_upgrade | core::recap_synergy;
                g.kills = 30; g.elites_killed = 2; g.combos_run = 5; g.secrets_found = 1;
                g.best_hit = 21; g.best_hit_def = data::enemy_betoniarka; g.best_hit_crit = true;
                g.worst_hit = { int8_t(data::enemy_betoniarka), -1, uint8_t(core::recap_kind::slam), int8_t(g.first_stage + 3), 8 };
                g.respect = 14; g.score = 3100; g.turns = 260; g.stage_start_turn = 240;
                if(scenario == 57)
                {
                    g.log_hit(data::enemy_kornik, 2, core::recap_kind::melee, 4);
                    g.enemies_count = 0;
                    clear_area(g, -1, -1, 1, 1);
                    g.hero.hp = 3;
                    g.bonus.second_chance = 0;
                    g.apply_status(core::status_effect::wet, 6);
                    place_at(g, data::enemy_zwarcie, 1, 0, 0);
                }
                break;
            }
            case 62:   // 5. magazyn: licznik w budowie, na końcu etapu baner sekretu
            {
                g.secrets_found = uint8_t(data::secrets[secret_index(core::secret_kind::storerooms)].value);
                break;
            }
            case 63:   // Spawacz: trzy problemy w linii w prawo
            case 65:   // Majster: pożyczona moc (ten sam układ - linia i obok)
            {
                clear_area(g, -1, -2, 5, 2);
                g.enemies_count = 0;
                for(int k = 1; k <= 3; ++k) place_at(g, data::enemy_kornik, k + 1, 0, 90);
                if(scenario == 65) { place_at(g, data::enemy_plesn, 0, -1, 90); place_at(g, data::enemy_kornik, 1, 1, 90); }
                for(int i = 0; i < g.enemies_count; ++i) g.enemies[i].hp = g.enemies[i].max_hp = 60;
                break;
            }
            case 64:   // Geodeta: plac odkryty od startu, problem obok w prawo
            {
                clear_area(g, -1, -1, 2, 1);
                g.enemies_count = 0;
                place_at(g, data::enemy_kornik, 1, 0, 3);
                g.enemies[0].hp = g.enemies[0].max_hp = 60;
                break;
            }
            case 66:   // Młot Zenka, Poziomica mistrza, kryt ze złotym błyskiem
            {
                secret_stage(g, 0);
                clear_area(g, -3, -2, -1, 2);
                const int t0 = tool_index("Młot Zenka"), t1 = tool_index("Poziomica mistrza");
                g.pickups[g.pickups_count++] = { int8_t(g.hero.x - 1), g.hero.y, core::tool, true, uint8_t(t0) };
                g.pickups[g.pickups_count++] = { int8_t(g.hero.x - 1), int8_t(g.hero.y + 1), core::tool, true, uint8_t(t1) };
                place_at(g, data::enemy_kornik, -2, 0, 90);
                if(g.enemies_count > 0) g.enemies[g.enemies_count - 1].hp = g.enemies[g.enemies_count - 1].max_hp = 80;
                g.bonus.crit = 100;   // każdy cios kryt (pokaz złotego błysku)
                break;
            }
            case 68:   // crash magazynu: skrzynia przy pełnym HUD, sprzęt ze skrzyni do zajętego slotu (okno porównania)
            {
                secret_stage(g, 0);
                g.keys = 1;
                for(int s = 0; s < data::gear_slots_count; ++s) g.equip(s, 0, 0);
                for(const char* n : { "Beton B30", "Hartowana kielnia", "Wąż ogrodowy", "Przedłużacz", "Podwójne espresso",
                                      "Termos z bufetu", "Złota podkowa", "Szczęśliwa moneta" })
                    give_boon(g, n);
                g.apply_status(core::status_effect::poison, 9);
                g.apply_status(core::status_effect::slip, 9);
                g.apply_status(core::status_effect::wet, 9);
                for(int m = 0; m < data::materials_count; ++m) g.mats[m] = 4;
                const int defs[5] = { data::enemy_przeciek, data::enemy_kornik, data::enemy_plesn, data::enemy_zwarcie, data::enemy_woda };
                const int8_t pos[5][2] = { { -2, -1 }, { -2, 1 }, { -3, 0 }, { -1, -2 }, { -1, 2 } };
                for(int t = 0; t < 5 && t < data::elites_count; ++t)
                {
                    const int before = g.enemies_count;
                    place_at(g, defs[t], pos[t][0], pos[t][1], 120);
                    if(g.enemies_count > before) g.make_elite(g.enemies_count - 1, t);
                }
                break;
            }
            case 67:   // wygrana: 2 HP, bez kawy, szybkie etapy
            {
                g.start_stage(data::stages_count - 1);
                for(int s = g.first_stage; s < data::stages_count; ++s) g.stage_days[s] = 3;
                g.stage_start_turn = g.turns;
                g.hero.hp = 1;   // awans za bossa: +2 HP = 3 (Na styk: 1-3 HP)
                g.enemies_count = g.boss + 1;
                break;
            }
            case 21:
            case 69:   // v0.21.52: porażka - koniec budowy ze stroną Postęp
            {
                g.hero.hp = 1;
                g.score = 900;
                g.enemies_count = 0;
                place_enemy(g, data::enemy_budzet, 1, 1, true, 0);
                break;
            }
            default:
                break;
        }
        g.update_fov();
    }
}
