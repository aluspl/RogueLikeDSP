// PlanBudowlany RogueLike - warstwa GBA (Butano). Logika gry: include/core.h (czyste C++).
#include "bn_core.h"
#include "bn_keypad.h"
#include "bn_memory.h"
#include "bn_sram.h"
#include "bn_string.h"
#include "bn_vector.h"
#include "bn_unique_ptr.h"
#include "bn_bg_tiles.h"
#include "bn_bg_palettes.h"
#include "bn_camera_ptr.h"
#include "bn_sprite_ptr.h"
#include "bn_sprite_double_size_mode.h"
#include "bn_sprite_tiles_item.h"
#include "bn_sprite_font.h"
#include "bn_regular_bg_ptr.h"
#include "bn_regular_bg_item.h"
#include "bn_regular_bg_map_ptr.h"
#include "bn_regular_bg_map_cell_info.h"
#include "bn_sprite_text_generator.h"
#include "bn_utf8_characters_map.h"

#include "bn_sprite_items_actors.h"
#include "bn_sprite_items_font_8x16.h"
#include "bn_sprite_items_hp_bar.h"
#include "bn_sprite_items_particles.h"
#include "bn_sprite_items_mini_hp.h"
#include "bn_sprite_items_truck.h"
#include "bn_sprite_items_houses.h"
#include "bn_sprite_items_ability_icons.h"
#include "bn_sprite_items_menu_icons.h"
#include "bn_sprite_items_actors_elite.h"
#include "bn_random.h"
#include "bn_music.h"
#include "bn_music_items.h"
#include "bn_sound_items.h"
#include "bn_display.h"
#include "bn_bg_palette_ptr.h"
#include "bn_sprite_palette_ptr.h"
#include "bn_bg_palette_color_hbe_ptr.h"
#include "bn_optional.h"
#include "bn_sprite_items_phone_icons.h"
#include "bn_regular_bg_items_phone_chrome.h"
#include "bn_regular_bg_tiles_items_phone_tiles.h"
#include "bn_bg_palette_items_phone_palette.h"
#include "bn_sprite_palette_items_font_dark.h"
#include "bn_sprite_palette_items_font_dim.h"
#include "bn_sprite_palette_items_font_brand.h"
#include "bn_sprite_palette_items_font_prog.h"
#include "bn_sprite_palette_items_font_done.h"
#include "bn_sprite_palette_items_font_late.h"
#include "bn_sprite_palette_items_font_white.h"
#include "bn_sprite_palette_items_font_map_bad.h"
#include "bn_sprite_palette_items_font_map_good.h"
#include "bn_sprite_palette_items_font_map_loot.h"
#include "bn_blending.h"
#include "bn_sprite_palettes.h"
#include "bn_regular_bg_items_title.h"
#include "bn_regular_bg_items_end.h"
#include "bn_regular_bg_tiles_items_tiles.h"
#include "bn_bg_palette_items_stage_palettes_0.h"
#include "bn_bg_palette_items_stage_palettes_1.h"
#include "bn_bg_palette_items_stage_palettes_2.h"
#include "bn_bg_palette_items_stage_palettes_3.h"
#include "bn_bg_palette_items_stage_palettes_4.h"
#include "bn_bg_palette_items_stage_palettes_5.h"
#include "bn_bg_palette_items_stage_palettes_6.h"
#include "bn_bg_palette_items_stage_palettes_7.h"
#include "bn_bg_palette_items_stage_palettes_8.h"
#include "bn_bg_palette_items_stage_palettes_9.h"
#include "bn_bg_palette_items_stage_palettes_10.h"
#include "bn_bg_palette_items_stage_palettes_11.h"

#include "core.h"
#include "meta.h"
#include "phone_tiles.h"
#include "screen_info.h"
#include "font_widths.h"
#ifdef PB_SCENARIO
#include "debug_scenarios.h"   // tylko buildy testowe playtestera
#endif

namespace
{
    // ------------------------------------------------------------------ font z polskimi znakami
    constexpr bn::utf8_character pl_chars[] = {
        "ą", "ć", "ę", "ł", "ń", "ó", "ś", "ź", "ż", "Ą", "Ć", "Ę", "Ł", "Ń", "Ó", "Ś", "Ź", "Ż"
    };
    constexpr bn::span<const bn::utf8_character> pl_chars_span(pl_chars);
    constexpr auto pl_chars_map = bn::utf8_characters_map<pl_chars_span>();
    constexpr bn::sprite_font font(bn::sprite_items::font_8x16, pl_chars_map.reference(), font_widths);   // zmienna szerokość

    using text_sprites = bn::vector<bn::sprite_ptr, 48>;
    using page_sprites = bn::vector<bn::sprite_ptr, 80>;   // pełnoekranowe strony menu

    enum class scene { title, class_select, game, schedule, end, shop, help, hurtownia, prologue, investor, daily, weekly };

    constexpr int frame_coffee = 15;
    constexpr int frame_fx = 18;
    constexpr int frame_lock = 19;
    constexpr int frame_silhouette = 20;   // + indeks zawodu (0-5); zawody z nagród: frame_silhouette_ext + (indeks - 6)
    constexpr int frame_silhouette_ext = 58;
    constexpr int frame_toolbox = 26;
    constexpr int frame_anim_b = 27;     // + klatka zawodu/wroga (0..14) = druga klatka animacji

    // druga klatka: zawody/wrogowie 0-14 -> +27, bossowie aktów 46-47 -> 48-49, Inspekcja 50 -> 51, zawody z nagród 52-54 -> 55-57,
    // problemy etapów 61-80 -> 81-100, Akt 0: problemy i Decyzja odmowna 101-109 -> 110-118
    constexpr int frame_stage_enemy = 61, stage_enemies = 20;
    constexpr int frame_prelude_enemy = 101, prelude_enemies = 9;
    constexpr int frame_document = 119;   // dokumenty Aktu 0 (podpis, mapa, uzgodnienie)
    // v0.21.50 cz. 3: pole wydarzenia, klucz do magazynu, skrzynia, pęknięcie muru (nakładka), drzwi magazynu
    constexpr int frame_event = 122, frame_key = 123, frame_chest = 124, frame_crack = 125, frame_door = 126;
    int anim_b(int frame)
    {
        if(frame >= frame_prelude_enemy) return frame + prelude_enemies;
        if(frame >= frame_stage_enemy) return frame + stage_enemies;
        return frame < 15 ? frame + frame_anim_b : (frame < 48 ? frame + 2 : (frame < 52 ? frame + 1 : frame + 3));
    }
    int silhouette_frame(int cls) { return cls < 6 ? frame_silhouette + cls : frame_silhouette_ext + cls - 6; }
    // Osiedle: dom = wielkość * liczba zawodów + zawód; pusta działka na końcu arkusza
    int house_frame(uint8_t h) { return (h >> 4) * data::classes_count + (h & 15); }
    constexpr int house_empty = 4 * data::classes_count;
    // menu_icons: nagrody za odbiór (narzędzia 15-16, buty 17, pas 18), Respekt 19, mechaniki aktów 20-22, statystyki 23
    constexpr int icon_respect = 19;
    constexpr int icon_act = 20;   // + mechanika aktu - 1 (błoto, porywy, pył)
    constexpr int icon_stats = 23;
    constexpr int icon_stamps = 24;   // mechanika Aktu 0: pieczątki
    constexpr int icon_boon = 25;     // v0.21.50: premia po etapie (paczka z kokardą)
    constexpr int icon_combo = 26;    // v0.21.50: kombinacja stanów (kropla + piorun)
    constexpr int icon_event = 27;    // v0.21.50 cz. 3: wydarzenie z wyborem (SMS z "!")
    constexpr int icon_upgrade = 28;  // ulepszenie narzędzia (klucz płaski i plus)
    constexpr int icon_key = 29;      // klucz do magazynu
    int act_icon_frame(core::act_mechanic m) { return m == core::act_mechanic::stamps ? icon_stamps : icon_act + core::imax(0, int(m) - 1); }
    // Zachowania problemu, np. "strzela z dystansu, ucieka" (karta wroga, Katalog usterek).
    // Stany problemu dla kombinacji (#29), np. "mokry, zapylony" (karta problemu); pusty = brak.
    core::message enemy_states_line(const core::game& g, int ei)
    {
        core::message m;
        if(g.enemy_wet(ei)) m.add("mokry");
        if(g.enemy_dusty(ei)) m.add(m.n ? ", " : "").add("zapylony");
        if(g.enemy_frozen(ei)) m.add(m.n ? ", " : "").add("zmrożony");
        return m;
    }
    // Znaczniki premii, np. "Beton, BHP".
    core::message boon_tags_line(const core::boon_def& bd)
    {
        core::message m;
        for(int t = 0; t < data::boon_tags_count; ++t) if((bd.tags >> t) & 1) m.add(m.n ? ", " : "").add(data::boon_tags[t]);
        return m;
    }
    // Sprite problemu: elita ma złotą paletę (złoty obrys, ciepły odcień).
    void style_enemy(bn::sprite_ptr& s, const core::actor& e)
    {
        if(e.elite >= 0) s.set_palette(bn::sprite_items::actors_elite.palette_item());
        else s.set_palette(bn::sprite_items::actors.palette_item());
    }
    core::message behaviors_line(int def)
    {
        core::message m;
        const int tags = data::enemies[def].tags;
        for(int b = 0; b < core::behaviors_count; ++b)
            if(tags & (1 << b)) m.add(m.n ? ", " : "").add(data::behavior_names[b]);
        return m;
    }
    int reward_icon(const core::reward_def& r)
    {
        if(r.kind == core::reward_kind::tool) return r.index == data::tools_count - 1 ? 16 : 15;
        if(r.kind == core::reward_kind::gear) return data::gear[r.index * 3].stat == core::gear_stat::thermos ? 18 : 17;
        if(r.kind == core::reward_kind::act) return icon_stamps;
        return 14;
    }

    const char* roman(int n) { static const char* r[] = { "I", "II", "III", "IV", "V" }; return r[n < 5 ? n : 4]; }

    constexpr int frame_gear = 42;       // + jakość
    constexpr int frame_reticle = 45;
    int pickup_frame(const core::pickup& p)
    {
        if(p.type == core::tool) return frame_toolbox;
        if(p.type == core::gear_box) return frame_gear + p.arg % 3;
        if(p.type == core::document) return frame_document + p.arg;
        if(p.type == core::event_tile) return frame_event;
        if(p.type == core::store_key) return frame_key;
        if(p.type == core::chest) return frame_chest;
        return frame_coffee + p.type;
    }

    // ------------------------------------------------------------------ przejścia: ściemnianie palet
    constexpr int fade_frames = 8;
    int fade_in_left = 0;

    void set_fade(int step)
    {
        bn::fixed intensity = bn::fixed(step) / fade_frames;
        bn::bg_palettes::set_fade(bn::color(0, 0, 0), intensity);
        bn::sprite_palettes::set_fade(bn::color(0, 0, 0), intensity);
    }

    // Zamiast bn::core::update(): prowadzi rozjaśnianie po zmianie sceny.
    void next_frame()
    {
        if(fade_in_left > 0) set_fade(--fade_in_left);
        bn::core::update();
    }

    // ------------------------------------------------------------------ zapis (SRAM): profil z meta.h
    core::profile load_save()
    {
        core::profile p;
        bn::sram::read(p);
        if(core::profile_fix(p)) bn::sram::write(p);   // pusta pamięć albo migracja z v1
        return p;
    }

    // ------------------------------------------------------------------ wspólny stan
    struct app
    {
        bn::sprite_text_generator text{font};
        core::game* g = nullptr;
        core::profile save;
        uint32_t seed_counter = 1;
        int chosen_class = 1;
        int chosen_diff = data::default_difficulty;
        scene after_help = scene::title;   // dokąd wrócić z ekranu "Jak grać"
        bool has_run = false;              // w SRAM jest przerwana budowa
        int phone_tab = 2;                 // ostatnio otwarta zakładka telefonu (Start)
        int pending_helper = -1;           // brygada: fachowiec wybrany w telefonie (wzywany po powrocie do gry)
    };

    // ------------------------------------------------------------------ zapis budowy w trakcie (SRAM za profilem)
    struct run_marker { char magic[8]; };

    void save_run(app& a)
    {
        bn::unique_ptr<core::run_save> s(new core::run_save());
        core::run_save_make(*s, *a.g);
        bn::sram::write_offset(*s, core::run_save_offset);
        a.has_run = true;
    }

    bool load_run(app& a)
    {
        bn::unique_ptr<core::run_save> s(new core::run_save());
        bn::sram::read_offset(*s, core::run_save_offset);
        if(! core::run_save_valid(*s)) return false;
        bn::memory::copy(s->g, 1, *a.g);
        return true;
    }

    void clear_run(app& a)
    {
        bn::sram::write_offset(run_marker{}, core::run_save_offset);
        a.has_run = false;
    }

    // Ściemnia ekran i przechodzi do sceny s (rozjaśnienie robi frame() w nowej scenie).
    scene leave(scene s)
    {
        for(int i = fade_in_left + 1; i <= fade_frames; ++i) { set_fade(i); bn::core::update(); }
        fade_in_left = fade_frames;
        return s;
    }

    // obcina tekst do n znaków (UTF-8), żeby nie wychodził poza ekran
    bn::string<96> clip(const char* s, int n)
    {
        bn::string<96> out;
        int chars = 0;
        for(const char* p = s; *p && chars < n; ++chars)
        {
            int len = (uint8_t(*p) >= 0xC0) ? 2 : 1;
            for(int k = 0; k < len && *p; ++k) out.push_back(*p++);
        }
        return out;
    }

    constexpr int phone_text_w = 210;   // szerokość wiersza tekstu w telefonie (px)

    // obcina tekst do szerokości w pikselach (font o zmiennej szerokości); ucięty kończy się kropką
    template<class App>
    bn::string<96> fit(App& a, const char* s, int max_px)
    {
        bn::string<96> out = clip(s, 95);
        if(a.text.width(out) <= max_px) return out;
        int dot = a.text.width(".");
        while(! out.empty() && (a.text.width(out) + dot > max_px || out.back() == ' ' || out.back() == ','))
        {
            while(! out.empty() && (uint8_t(out.back()) & 0xC0) == 0x80) out.pop_back();   // bajty kontynuacji UTF-8
            if(! out.empty()) out.pop_back();
        }
        out.push_back('.');
        return out;
    }

    void wait_release()
    {
        next_frame();
    }

    // Nazwa mocy z rangą, np. "Ścianka II".
    core::message ability_label(const core::game& g)
    {
        core::message m; m.add(g.cdef().ability_name);
        int r = g.ability_rank();
        if(r > 1) m.add(r == 2 ? " II" : " III");
        return m;
    }

    // Statystyki: skrót nazwy i zapis "baza+premia" (np. "SIŁ 5+1").
    const char* stat_short(core::stat s) { return s == core::stat::str ? "SIŁ" : (s == core::stat::agi ? "ZRĘ" : "INT"); }

    void add_stat(core::message& m, const char* label, int base, int bonus)
    {
        m.add(label).add(" ").add(base);
        if(bonus > 0) m.add("+").add(bonus);
    }

    // Wiersz statystyk efektywnych bohatera w trakcie budowy: SIŁ, ZRĘ, INT, szczęście.
    core::message hero_stats_line(const core::game& g)
    {
        core::message m;
        const core::stat all[3] = { core::stat::str, core::stat::agi, core::stat::intel };
        for(core::stat st : all) { add_stat(m, stat_short(st), core::class_base_stat(g.cls, st), g.stat_bonus(st)); m.add(" "); }
        add_stat(m, "SZCZ", g.cdef().luck, g.luck() - g.cdef().luck);
        return m;
    }

    // ------------------------------------------------------------------ dźwięk (Maxmod; pliki z tools/make_audio.py)
    enum class song { none, title, game };
    song current_song = song::none;

    void play_song(song s)
    {
        if(s == current_song) return;
        current_song = s;
        if(s == song::title) bn::music_items::music_title.play(bn::fixed(0.45));
        else if(s == song::game) bn::music_items::music_game.play(bn::fixed(0.35));
        else if(bn::music::playing()) bn::music::stop();
    }

    // ------------------------------------------------------------------ efekty palet
    // Gradient fioletu marki na tle ekranu tytułowego i końcowego: HDMA zmienia jeden kolor palety co linię.
    alignas(int) bn::color violet_gradient[bn::display::height()];

    bn::bg_palette_color_hbe_ptr make_gradient(const bn::regular_bg_ptr& bg, int color_index)
    {
        for(int y = 0; y < bn::display::height(); ++y)
        {
            int t = y * 256 / bn::display::height();   // 0..255 od góry do dołu
            int r = 124 - (124 - 44) * t / 256, gr = 98 - (98 - 30) * t / 256, b = 255 - (255 - 150) * t / 256;
            violet_gradient[y] = bn::color(r >> 3, gr >> 3, b >> 3);
        }
        return bn::bg_palette_color_hbe_ptr::create(bg.palette(), color_index, violet_gradient);
    }

    // Trójkątna fala 0..max..0 o okresie 2*max klatek.
    int pulse(int clock, int max) { int p = clock % (2 * max); return p < max ? p : 2 * max - p; }

    bn::color mix(int r1, int g1, int b1, int r2, int g2, int b2, int t, int max)   // RGB 0..255 -> kolor GBA
    {
        return bn::color((r1 + (r2 - r1) * t / max) >> 3, (g1 + (g2 - g1) * t / max) >> 3, (b1 + (b2 - b1) * t / max) >> 3);
    }

    void title_tutorial(app& a);   // samouczek menu na tytule (niżej, przy telefonie)

    // ------------------------------------------------------------------ ekran tytułowy
    scene run_title(app& a)
    {
        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        bn::regular_bg_ptr bg = bn::regular_bg_items::title.create_bg(8, 48);   // lewy górny róg obrazu = róg ekranu
        bn::bg_palette_color_hbe_ptr gradient = make_gradient(bg, screen_info::title_bg_index);
        play_song(song::title);
        title_tutorial(a);   // pierwsze uruchomienie: dymki po kolei; potem dymki nowości (Respekt, budowa dnia, Akt 0)
        text_sprites prompt, record;
        a.text.set_center_alignment();
        if(a.save.best > 0)
        {
            core::message m; m.add("Rekord: ").add(int(a.save.best));
            a.text.set_right_alignment();
            a.text.generate(116, -72, m.s, record);
            a.text.set_center_alignment();
        }
        text_sprites version_text;   // numer wersji w lewym górnym rogu
        a.text.set_left_alignment();
        a.text.generate(-116, -72, data::version, version_text);
        a.text.set_center_alignment();
        text_sprites shop_hint;
        a.text.generate(0, 66, "SELECT: telefon  B: pomoc", shop_hint);
        int frame = 0;
        while(true)
        {
            ++a.seed_counter;
            if((frame++ % 60) == 0) { prompt.clear(); a.text.generate(0, 50, a.has_run ? "START: dalej  A: nowa" : "Naciśnij START", prompt); }
            if(frame % 150 == 0 || frame % 150 == 75)   // podpowiedź na dole na zmianę: telefon i pomoc / budowa dnia
            {
                shop_hint.clear();
                a.text.generate(0, 66, frame % 150 == 0 ? "SELECT: telefon  B: pomoc" : "R: budowa dnia  L: tydzień", shop_hint);
            }
            if((frame % 60) == 40) prompt.clear();
            if(a.has_run && bn::keypad::start_pressed())   // kontynuuj przerwaną budowę
            {
                wait_release();
                if(load_run(a)) return leave(scene::game);
                a.has_run = false;
            }
            else if(bn::keypad::start_pressed() || bn::keypad::a_pressed()) { wait_release(); return leave(scene::class_select); }
            if(bn::keypad::select_pressed()) { wait_release(); return leave(scene::shop); }
            if(bn::keypad::b_pressed()) { a.after_help = scene::title; wait_release(); return leave(scene::help); }
            if(bn::keypad::r_pressed()) { wait_release(); return leave(scene::daily); }
            if(bn::keypad::l_pressed()) { wait_release(); return leave(scene::weekly); }   // wyzwanie tygodnia (#34)
            next_frame();
        }
    }

    // ------------------------------------------------------------------ mapa etapu (dynamiczne tło 64x64 kafli 8x8 = 32x32 pól 16x16)
    struct bg_map
    {
        static constexpr int columns = 64;
        static constexpr int rows = 64;
        alignas(int) bn::regular_bg_map_cell cells[columns * rows];
        bn::regular_bg_map_item map_item;

        bg_map() : map_item(cells[0], bn::size(columns, rows)) { bn::memory::clear(cells); }

        // Miękkie światło: paleta 0 = pełne światło przy bohaterze, 1 = 80%, 2 = 60% (skraj pola widzenia),
        // 3 = pole zapamiętane poza polem widzenia. Każdy etap ma własny zestaw 4 palet.
        static int light_level(const core::game& g, int x, int y)
        {
            if(! g.visible(x, y)) return 3;
            int dx = x - g.hero.x, dy = y - g.hero.y, d2 = dx * dx + dy * dy;
            return d2 <= 10 ? 0 : (d2 <= 24 ? 1 : 2);
        }

        const bool (*highlight)[core::map_w] = nullptr;   // pola w zasięgu broni (celowanie)

        void set(int cx, int cy, int t, int palette, bool hflip = false, bool vflip = false)
        {
            bn::regular_bg_map_cell& cell = cells[map_item.cell_index(cx, cy)];
            bn::regular_bg_map_cell_info info(cell);
            info.set_tile_index(t);
            info.set_palette_id(palette);
            info.set_horizontal_flip(hflip);
            info.set_vertical_flip(vflip);
            cell = info.cell();
        }

        // Kafel pola z uwzględnieniem mgły wojny: 0 = nieznane (czarne).
        // Kafle aktu: akt I 1-10, akt II 11-20, akt III 21-30 (ziemia i bloczki / deski i cegła / płytki i tynk);
        // Akt 0: 31-40 biuro z segregatorami (Pozwolenie), 41-50 wykop z rurami (Przyłącza).
        static int tile_set(const core::game& g) { return g.stage < data::prelude_stages ? 3 + g.stage : data::stages[g.stage].act; }
        static int act_tile(const core::game& g, int t) { return t == 0 ? 0 : t + 10 * tile_set(g); }
        static int tile_of(const core::game& g, int x, int y, int& palette)
        {
            palette = light_level(g, x, y);
            if(! g.explored(x, y)) return 0;
            core::tile k = g.lv.at(x, y);
            if(k == core::tile::floor) return g.lv.at(x, y - 1) == core::tile::wall ? 5 : 1;   // cień muru u góry
            if(k == core::tile::stairs) return 4;
            return g.lv.at(x, y + 1) != core::tile::wall ? 3 : 2;   // lico muru nad podłogą / mur
        }
        void quad(const core::game& g, int x, int y, int t, int pal)   // pole z ćwiartki odbijanej na 4 strony
        {
            t = act_tile(g, t);
            set(x * 2, y * 2, t, pal); set(x * 2 + 1, y * 2, t, pal, true);
            set(x * 2, y * 2 + 1, t, pal, false, true); set(x * 2 + 1, y * 2 + 1, t, pal, true, true);
        }

        // Czy na polu stoi coś, co rzuca cień (bohater, widoczny wróg, znajdźka)?
        static bool casts_shadow(const core::game& g, int x, int y)
        {
            if(g.hero.x == x && g.hero.y == y) return true;
            for(int i = 0; i < g.enemies_count; ++i)
                if(g.enemies[i].alive && g.enemies[i].x == x && g.enemies[i].y == y && g.visible(x, y)) return true;
            for(int i = 0; i < g.pickups_count; ++i)
                if(g.pickups[i].active && g.pickups[i].x == x && g.pickups[i].y == y) return true;
            return false;
        }

        // Widok gry: pole 16x16 = 2x2 kafle 8x8. Cień postaci: dolne kafle pola (ćwiartka + odbicie).
        void build(const core::game& g)
        {
            for(int y = 0; y < core::map_h; ++y)
                for(int x = 0; x < core::map_w; ++x)
                {
                    int pal, t = tile_of(g, x, y, pal);
                    bool flat = t == 1 || t == 5;
                    if(flat && g.danger_cell(x, y)) { quad(g, x, y, 8, pal); continue; }                  // cios bossa / wybuch
                    if(flat && highlight && highlight[y][x]) { quad(g, x, y, 7, pal); continue; }         // ramka pola w zasięgu
                    if(flat && g.puddle(x, y)) { quad(g, x, y, 9, pal); continue; }                       // deszcz: kałuża
                    if(flat && g.mud(x, y)) { quad(g, x, y, 10, pal); continue; }                         // akt I: błoto
                    t = act_tile(g, t);
                    bool top_shadow = t == act_tile(g, 5);
                    int base = top_shadow ? act_tile(g, 1) : t;
                    set(x * 2, y * 2, t, pal); set(x * 2 + 1, y * 2, t, pal);
                    if(flat && g.visible(x, y) && casts_shadow(g, x, y))
                    {
                        set(x * 2, y * 2 + 1, act_tile(g, 6), pal);
                        set(x * 2 + 1, y * 2 + 1, act_tile(g, 6), pal, true);
                    }
                    else { set(x * 2, y * 2 + 1, base, pal); set(x * 2 + 1, y * 2 + 1, base, pal); }
                }
        }

        // Podgląd mapy (L): pole = 1 kafel 8x8, cały etap mieści się prawie na jednym ekranie.
        void build_overview(const core::game& g)
        {
            bn::memory::clear(cells);
            for(int y = 0; y < core::map_h; ++y)
                for(int x = 0; x < core::map_w; ++x) { int pal, t = tile_of(g, x, y, pal); set(x, y, act_tile(g, t), pal); }
        }
    };

    bn::fixed clampf(bn::fixed v, int lim) { return v < -lim ? bn::fixed(-lim) : (v > lim ? bn::fixed(lim) : v); }

    bn::fixed_point world(int x, int y) { return bn::fixed_point(x * 16 + 8 - 256, y * 16 + 8 - 256); }

    // ------------------------------------------------------------------ menu pod SELECT (pauza)
    void wait_page_close()
    {
        while(! (bn::keypad::a_pressed() || bn::keypad::b_pressed() || bn::keypad::select_pressed()))
            next_frame();
        wait_release();
    }

    void page_help(app& a)
    {
        constexpr int pages_count = 12;
        core::message cmb[3];   // kombinacje stanów (#29): "Mokry + prąd! Porażenie"
        for(int k = 0; k < 3 && k < data::combos_count; ++k) cmb[k].add(data::combos[k].short_name).add(" ").add(data::combos[k].name);
        core::message luck1, luck2;   // wzory z danych (sekcja luck)
        luck1.add("SZCZ: kryt ").add(data::crit_base_pct).add("%+").add(data::crit_per_luck_pct).add("%/pkt, unik");
        luck2.add(data::dodge_per_luck_pct).add("%/pkt (maks. ").add(data::dodge_max_pct).add("%), łupy +").add(data::drop_per_luck_pct).add("%.");
        const char* pages[pages_count][7] = {
            { "3 akty (10 etapów), po nagrodzie", "Akt 0. Boss kończy akt.", "D-pad: ruch i atak wręcz",
              "A: atak (trzymaj: celuj)", "B: czekaj (trzymaj: podgląd)", "R: moc zawodu  L: mapa",
              "START: akcje  SELECT: telefon" },
            { "Pogoda dnia: ikona w HUD,", "skutek w telefonie (Zadania).", "Brygada raz na etap za zł:",
              "telefon, Sprzęt, dół (albo", "START i A). Po wygranej:", "SELECT na wyborze zawodu =", "tryb inwestora (stawka)." },
            { "Między etapami wybierz", "ścieżkę (lewo/prawo, A).", "Materiały z problemów:", "Hurtownia i naprawy (Sprzęt,", "dół): Załataj, Kładka.",
              "Tytuł, R: budowa dnia (ustaw", "datę) - dla wszystkich ta sama." },
            { "Respekt za każdy etap (boss", "więcej) zostaje po porażce.", "Telefon profilu, Koszty,", "SELECT: Respekt - stałe",
              "premie z rangami. Każda wygrana", "to nagroda za odbiór: sprzęt,", "narzędzia, nowe zawody." },
            { "Mechanika aktu: 0 pieczątki -", "3 dokumenty otwierają schody,", "I błoto (wejście = tura), II", "porywy spychają, III pył.",
              "Problemy strzelają, dzielą się,", "wybuchają (czerwone pola -", "odejdź!), rosną i wracają." },
            { "SIŁ/ZRĘ/INT: +1 obr. co 2 pkt", "(tylko statystyka broni).", "OBR: -1 obrażeń co 2 pkt.", luck1.s, luck2.s,
              "Wybór zawodu: START = opis,", "telefon: Start, A = premie." },
            { data::damage_help[0], data::damage_help[1], data::damage_help[2], data::damage_help[3], data::damage_help[4],
              data::damage_help[5], "Telefon: Sprzęt, A = rozpiska." },
            { cmb[0].s, data::combos[0].info, cmb[1].s, data::combos[1].info, cmb[2].s, data::combos[2].info,
              "Ikona nad problemem: jego stan." },
            { data::combo_sources[0], data::combo_sources[1], data::combo_sources[2], data::combo_sources[3],
              "Na Tobie też (ikona w HUD):", data::combos[0].hero, data::combos[1].hero },
            { "Po etapie: premia 1 z 3", "(zwykła, rzadka, legendarna).", "2+ z tym samym znacznikiem =",
              "synergia. SELECT: losuj raz.", "Telefon: Sprzęt, góra = premie.", "Złota ramka: elita z cechą,",
              "więcej HP, lepsza nagroda." },
            { data::extras_help[0], data::extras_help[1], data::extras_help[2], data::extras_help[3], data::extras_help[4],
              data::extras_help[5], "Karta problemu (B): Ma klucz." },
            { data::extras_help[6], data::extras_help[7], data::extras_help[8], data::extras_help[9], data::extras_help[10],
              data::extras_help[11], "L: mapa pokazuje magazyn." } };
        for(int pg = 0; pg < pages_count; ++pg)
        {
            page_sprites t;
            a.text.set_center_alignment();
            static const char* const names[5] = { "Kombinacje", "Skąd stany", "Premie i elity", "Wydarzenia", "Magazyn" };   // strony 8-12 (v0.21.50)
            core::message title; title.add(pg >= 7 ? names[pg - 7] : "Jak grać").add(" (").add(pg + 1).add("/").add(pages_count).add(")");
            a.text.generate(0, -70, title.s, t);
            a.text.set_left_alignment();
            for(int i = 0; i < 7; ++i) a.text.generate(-108, -48 + i * 16, fit(a, pages[pg][i], 224).c_str(), t);
            a.text.set_center_alignment();
            a.text.generate(0, 72, "A: dalej", t);
            wait_page_close();
        }
    }

    scene run_help(app& a)
    {
        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        page_help(a);
        if(! core::has_flag(a.save, core::help_seen)) { core::set_flag(a.save, core::help_seen); bn::sram::write(a.save); }
        if(a.after_help == scene::title)   // z tytułu: samouczek menu jeszcze raz (#25)
        {
            page_sprites t;
            a.text.set_center_alignment();
            a.text.generate(0, -70, "Samouczek menu", t);
            a.text.set_left_alignment();
            const char* lines[] = { "Dymki Kierownika Marka", "pokazują po kolei, co jest", "na tytule i na wyborze", "zawodu (klawisz i opis)." };
            for(int i = 0; i < 4; ++i) a.text.generate(-108, -48 + i * 16, lines[i], t);
            a.text.set_center_alignment();
            a.text.generate(0, 40, "A: pokaż samouczek jeszcze raz", t);
            a.text.generate(0, 72, "B: wróć", t);
            wait_release();
            while(! bn::keypad::a_pressed() && ! bn::keypad::b_pressed() && ! bn::keypad::select_pressed()) next_frame();
            if(bn::keypad::a_pressed()) { core::tutorial_reset(a.save); bn::sram::write(a.save); }
            wait_release();
        }
        return leave(a.after_help);
    }

    // ------------------------------------------------------------------ telefon z aplikacją PlanBudowlany
    // Menu w grze wygląda jak aplikacja PlanBudowlany: ramka z paskiem statusu, białe karty, pastylki
    // statusów w kolorach AppColors, dolny pasek zakładek (Zadania, Usterki, Start, Zespół, Koszty).
    struct phone_canvas   // dynamiczna warstwa treści nad ramką (kafelki z graphics/phone_tiles.bmp)
    {
        static constexpr int columns = 32;
        static constexpr int rows = 32;
        alignas(int) bn::regular_bg_map_cell cells[columns * rows];
        bn::regular_bg_map_item map_item;

        phone_canvas() : map_item(cells[0], bn::size(columns, rows)) { clear(); }

        void clear() { bn::memory::clear(cells); }

        void set(int tx, int ty, int tile, bool hflip = false, bool vflip = false)
        {
            if(tx < 0 || ty < 0 || tx >= columns || ty >= rows) return;
            bn::regular_bg_map_cell& cell = cells[map_item.cell_index(tx, ty)];
            bn::regular_bg_map_cell_info info(cell);
            info.set_tile_index(tile);
            info.set_palette_id(0);
            info.set_horizontal_flip(hflip);
            info.set_vertical_flip(vflip);
            cell = info.cell();
        }

        // prostokąt z zaokrąglonymi rogami (kafel lewego górnego rogu odbijany na pozostałe)
        void rounded(int tx, int ty, int tw, int th, int fill, int corner)
        {
            for(int y = 0; y < th; ++y) for(int x = 0; x < tw; ++x) set(tx + x, ty + y, fill);
            set(tx, ty, corner);
            set(tx + tw - 1, ty, corner, true);
            set(tx, ty + th - 1, corner, false, true);
            set(tx + tw - 1, ty + th - 1, corner, true, true);
        }

        // pasek postępu (jeden rząd kafli): value z max
        void bar(int tx, int ty, int tw, int first_tile, int value, int max)
        {
            int px = max > 0 ? core::imin(tw * 8, core::imax(0, value) * tw * 8 / max) : 0;
            for(int x = 0; x < tw; ++x) set(tx + x, ty, first_tile + core::imax(0, core::imin(8, px - x * 8)));
        }
    };

    bn::regular_bg_ptr make_canvas_bg(phone_canvas& c)
    {
        bn::bg_tiles::set_allow_offset(false);
        bn::regular_bg_item item(bn::regular_bg_tiles_items::phone_tiles, bn::bg_palette_items::phone_palette, c.map_item);
        bn::regular_bg_ptr bg = item.create_bg(8, 48);
        bn::bg_tiles::set_allow_offset(true);
        return bg;
    }

    struct phone_screen   // ramka + warstwa treści + ikona aktywnej zakładki
    {
        bn::unique_ptr<phone_canvas> canvas;
        bn::regular_bg_ptr chrome;
        bn::regular_bg_ptr content;
        bn::regular_bg_map_ptr map;
        bn::sprite_ptr icon;

        explicit phone_screen(int tab) :
            canvas(new phone_canvas()),
            chrome(bn::regular_bg_items::phone_chrome.create_bg(8, 48)),
            content(make_canvas_bg(*canvas)),
            map(content.map()),
            icon(bn::sprite_items::phone_icons.create_sprite(phone_tile::tab_x[tab], phone_tile::tab_y, tab))
        {
            chrome.set_priority(3);
            content.set_priority(2);
            icon.set_bg_priority(1);
        }

        void set_tab(int tab)
        {
            icon.set_position(phone_tile::tab_x[tab], phone_tile::tab_y);
            icon.set_tiles(bn::sprite_items::phone_icons.tiles_item(), tab);
        }

        void commit() { map.reload_cells_ref(); }

        void set_visible(bool v) { chrome.set_visible(v); content.set_visible(v); icon.set_visible(v); }
    };

    enum class ink { dark, dim, brand, prog, done, late, white };

    const bn::sprite_palette_item& ink_palette(ink i)
    {
        switch(i)
        {
            case ink::dim:   return bn::sprite_palette_items::font_dim;
            case ink::brand: return bn::sprite_palette_items::font_brand;
            case ink::prog:  return bn::sprite_palette_items::font_prog;
            case ink::done:  return bn::sprite_palette_items::font_done;
            case ink::late:  return bn::sprite_palette_items::font_late;
            case ink::white: return bn::sprite_palette_items::font_white;
            default:         return bn::sprite_palette_items::font_dark;
        }
    }

    // Tekst w pikselach ekranu: (px, py) = lewy górny róg linii 16 px; align -1 lewo, 0 środek, 1 prawo.
    void phone_text(app& a, page_sprites& t, int px, int py, const char* s, ink i, int align = -1)
    {
        a.text.set_palette_item(ink_palette(i));
        a.text.set_bg_priority(1);   // nad kartami (priorytet 2) i ramką (3)
        if(align < 0) a.text.set_left_alignment();
        else if(align > 0) a.text.set_right_alignment();
        else a.text.set_center_alignment();
        a.text.generate(px - 120, py - 72, s, t);
    }

    int utf8_len(const char* s)
    {
        int n = 0;
        for(; *s; ++s) if((uint8_t(*s) & 0xC0) != 0x80) ++n;
        return n;
    }

    enum class pill { prog, late, done, gray, brand, group };

    // Pastylka statusu przyklejona prawą krawędzią do kafla tx_end, 2 kafle wysokości.
    void phone_pill(app& a, phone_canvas& c, page_sprites& t, int tx_end, int ty, const char* s, pill p)
    {
        int tw = utf8_len(s) + 1;
        int tx = tx_end - tw;
        int fill = phone_tile::fill_gray, corner = phone_tile::corner_gray;
        ink i = ink::dim;
        switch(p)
        {
            case pill::prog:  fill = phone_tile::fill_prog_bg; corner = phone_tile::corner_prog_bg; i = ink::prog; break;
            case pill::late:  fill = phone_tile::fill_late_bg; corner = phone_tile::corner_late_bg; i = ink::late; break;
            case pill::done:  fill = phone_tile::fill_done_bg; corner = phone_tile::corner_done_bg; i = ink::done; break;
            case pill::brand: fill = phone_tile::fill_brand; corner = phone_tile::corner_brand; i = ink::white; break;
            case pill::group: fill = phone_tile::fill_group; corner = phone_tile::corner_group; i = ink::brand; break;
            default: break;
        }
        c.rounded(tx, ty, tw, 2, fill, corner);
        phone_text(a, t, tx * 8 + tw * 4, ty * 8, s, i, 0);
    }

    // Lista na karcie: wiersz r (0..5) = 16 px od y 32, kafle od rzędu 4.
    constexpr int row_ty(int r) { return 4 + r * 2; }
    constexpr int row_py(int r) { return 32 + r * 16; }
    constexpr int list_x = 14;   // początek tekstu za paskiem statusu
    constexpr int pill_end = 28; // prawa krawędź pastylek (kafel)
    int utf8_len(const char* s);
    // miejsce (px) na tekst od list_x do pastylki z napisem s przy prawej krawędzi
    int pill_room(const char* s) { return (pill_end - utf8_len(s) - 1) * 8 - list_x - 4; }

    void stripe(phone_canvas& c, int r, int tile) { c.set(1, row_ty(r), tile); c.set(1, row_ty(r) + 1, tile); }
    // Rzadkość premii (#27): zwykła szara, rzadka fioletowa, legendarna złota.
    ink rarity_ink(int r) { return r == 2 ? ink::prog : (r == 1 ? ink::brand : ink::dark); }
    pill rarity_pill(int r) { return r == 2 ? pill::prog : (r == 1 ? pill::group : pill::gray); }
    int rarity_stripe(int r) { return r == 2 ? phone_tile::stripe_prog : (r == 1 ? phone_tile::stripe_brand : phone_tile::stripe_todo); }

    constexpr const char* tab_names[] = { "Zadania", "Usterki", "Start", "Sprzęt", "Koszty" };
    constexpr int tabs_count = 5;

    void phone_header(app& a, phone_screen& ph, page_sprites& t, const char* title, const char* sub)
    {
        ph.canvas->clear();
        t.clear();
        phone_text(a, t, 10, 13, title, ink::dark);
        phone_text(a, t, 230, 13, sub, ink::dim, 1);
        ph.canvas->rounded(1, 4, 28, 12, phone_tile::fill_card, phone_tile::corner_card);
    }

    // ------------------------------------------------------------------ samouczek menu (#25)
    // Dymek Kierownika Marka jak powiadomienie PlanBudowlany nad przyciemnionym ekranem: półprzezroczysta ciemna
    // warstwa z "dziurą" nad omawianym elementem (reszta przygaszona) i karta z tekstem z data::tutorial_steps
    // (te same teksty co w Godocie). A - dalej, B - pomiń resztę, START przy kroku o statystykach - otwiera ich opis.
    struct coach_hole { int8_t tx, ty, tw, th; bool top; };   // podświetlony prostokąt (kafle 8x8); dymek u góry / na dole
    bool same(const char* x, const char* y) { while(*x && *x == *y) { ++x; ++y; } return *x == *y; }   // bez strcmp (Butano bez libc)

    struct coach
    {
        bn::unique_ptr<phone_canvas> dim, card;
        bn::regular_bg_ptr dim_bg, card_bg;
        bn::regular_bg_map_ptr dim_map, card_map;
        bn::sprite_ptr avatar;
        page_sprites t;

        coach() :
            dim(new phone_canvas()), card(new phone_canvas()),
            dim_bg(make_canvas_bg(*dim)), card_bg(make_canvas_bg(*card)),
            dim_map(dim_bg.map()), card_map(card_bg.map()),
            avatar(bn::sprite_items::actors.create_sprite(0, 0, data::classes[0].frame))   // Kierownik Marek
        {
            dim_bg.set_priority(0); dim_bg.set_z_order(10); dim_bg.set_blending_enabled(true);
            card_bg.set_priority(0); card_bg.set_z_order(-10);
            avatar.set_bg_priority(0); avatar.set_z_order(-120);
            bn::blending::set_transparency_alpha(bn::fixed(0.66));
        }

        void text(app& a, int px, int py, const char* s, ink i, int align = -1)
        {
            a.text.set_palette_item(ink_palette(i));
            a.text.set_bg_priority(0);
            a.text.set_z_order(-120);
            if(align < 0) a.text.set_left_alignment();
            else if(align > 0) a.text.set_right_alignment();
            else a.text.set_center_alignment();
            a.text.generate(px - 120, py - 72, s, t);
        }

        int pill(app& a, int tx_end, int ty, const char* s, bool brand)   // pastylka na karcie (szerokość z tekstu); zwraca pierwszy kafel
        {
            int tw = (a.text.width(s) + 15) / 8 + 1, tx = tx_end - tw;
            card->rounded(tx, ty, tw, 2, brand ? phone_tile::fill_brand : phone_tile::fill_group, brand ? phone_tile::corner_brand : phone_tile::corner_group);
            text(a, tx * 8 + tw * 4, ty * 8, s, brand ? ink::white : ink::brand, 0);
            return tx;
        }

        // Karta: awatar i tytuł, pastylka (np. "2/6" albo "Nowość"), 3 linie dymka, klawisz na GBA i A / B.
        void show(app& a, const core::tutorial_step& s, const char* counter, const coach_hole& h, bool link)
        {
            dim->clear(); card->clear(); t.clear();
            for(int y = 0; y < 20; ++y)
                for(int x = 0; x < 30; ++x)
                    if(! (x >= h.tx && x < h.tx + h.tw && y >= h.ty && y < h.ty + h.th)) dim->set(x, y, phone_tile::fill_dark);
            const int ty = h.top ? 0 : 9, py = ty * 8;
            card->rounded(1, ty, 28, 11, phone_tile::fill_card, phone_tile::corner_card);
            card->rounded(2, ty + 3, 26, 6, phone_tile::fill_group, phone_tile::corner_group);   // dymek wiadomości
            avatar.set_position(20 - 120, py + 16 - 80);
            text(a, 32, py + 8, s.title, ink::brand);
            pill(a, 28, ty + 1, counter, false);
            for(int i = 0; i < 3; ++i) text(a, 22, py + 26 + i * 15, s.msg.lines[i], ink::dark);
            int key_end = 0;   // prawy brzeg pastylki klawisza (px)
            if(s.gba[0]) key_end = 8 * (2 + (a.text.width(s.gba) + 15) / 8 + 1);
            if(s.gba[0]) pill(a, key_end / 8, ty + 9, s.gba, true);   // klawisz na GBA
            const char* hint = link ? "START: opis  A: dalej" : "A: dalej  B: pomiń";
            if(226 - a.text.width(hint) < key_end + 6) hint = "A: dalej";   // długi klawisz: skrócona podpowiedź, bez nachodzenia
            text(a, 226, py + 72, hint, ink::dim, 1);
            dim_map.reload_cells_ref();
            card_map.reload_cells_ref();
        }
    };

    // Kroki samouczka ekranu (0 tytuł, 1 wybór zawodu): hole_of(id) - co podświetlić, on_link() - START przy kroku
    // o statystykach (opis statystyk; dymek na chwilę znika, bo telefon potrzebuje dwóch warstw tła).
    template<class HoleFn, class LinkFn>
    void run_tutorial(app& a, int screen, HoleFn hole_of, LinkFn on_link)
    {
        const bn::sprite_palette_item ink0 = a.text.palette_item();
        const int prio0 = a.text.bg_priority(), z0 = a.text.z_order();
        int ids[data::tutorial_steps_count], n = 0;
        for(int i = 0; i < data::tutorial_steps_count; ++i)
            if(data::tutorial_steps[i].screen == screen && core::tutorial_step_shown(a.save, i, false)) ids[n++] = i;
        bn::optional<coach> c;
        for(int k = 0; k < n; ++k)
        {
            const core::tutorial_step& s = data::tutorial_steps[ids[k]];
            const bool link = same(s.id, "stats");
            core::message cnt; cnt.add(k + 1).add("/").add(n);
            if(! c) c.emplace();
            c->show(a, s, cnt.s, hole_of(s.id), link);
            bn::sound_items::sfx_notify.play(bn::fixed(0.6));
            wait_release();
            int res = 0;   // 1 dalej, 2 pomiń
            while(! res)
            {
                if(bn::keypad::a_pressed()) res = 1;
                else if(bn::keypad::b_pressed()) res = 2;
                else if(link && bn::keypad::start_pressed())
                {
                    c.reset();
                    a.text.set_palette_item(ink0); a.text.set_bg_priority(prio0); a.text.set_z_order(z0);
                    on_link();
                    c.emplace();
                    c->show(a, s, cnt.s, hole_of(s.id), link);
                }
                next_frame();
            }
            bn::sound_items::sfx_menu.play();
            if(res == 2) break;
        }
        c.reset();
        a.text.set_palette_item(ink0); a.text.set_bg_priority(prio0); a.text.set_z_order(z0);
        core::tutorial_done(a.save, screen);
        bn::sram::write(a.save);
        wait_release();
    }

    // Dymki przy pierwszym odblokowaniu (Respekt, codzienna budowa, tryb inwestora, Akt 0, nowy zawód) - po jednym.
    template<class HoleFn>
    void run_unlocks(app& a, int screen, HoleFn hole_of)
    {
        const bn::sprite_palette_item ink0 = a.text.palette_item();
        const int prio0 = a.text.bg_priority(), z0 = a.text.z_order();
        int cls = -1;
        for(int u = core::pending_unlock(a.save, screen, cls); u >= 0; u = core::pending_unlock(a.save, screen, cls))
        {
            const core::tutorial_step& s = data::tutorial_unlocks[u];
            {
                coach c;
                c.show(a, s, cls >= 0 ? clip(data::classes[cls].name, 12).c_str() : "Nowość", hole_of(s.id), false);
                bn::sound_items::sfx_notify.play(bn::fixed(0.7));
                wait_release();
                while(! bn::keypad::a_pressed() && ! bn::keypad::b_pressed() && ! bn::keypad::start_pressed()) next_frame();
            }
            core::mark_unlock(a.save, u, cls);
            bn::sram::write(a.save);
            a.text.set_palette_item(ink0); a.text.set_bg_priority(prio0); a.text.set_z_order(z0);
            wait_release();
        }
    }

    void title_tutorial(app& a)
    {
        auto hole = [](const char*) { return coach_hole{ 0, 0, 0, 0, false }; };   // tytuł: klawisz na karcie, logo przygaszone
        if(core::tutorial_pending(a.save, 0)) run_tutorial(a, 0, hole, [] {});
        run_unlocks(a, 0, hole);
    }

    void tab_tasks(app& a, phone_screen& ph, page_sprites& t)   // Zadania = harmonogram budowy
    {
        const core::game& g = *a.g;
        core::message sub; sub.add("Etap ").add(g.stage_number()).add("/").add(g.stages_in_run());
        if(g.stage_path >= 0) sub.add(", ").add(data::paths[g.stage_path].short_name);   // wybrana ścieżka
        else sub.add(", ").add(g.score).add(" pkt");
        phone_header(a, ph, t, tab_names[0], sub.s);
        phone_canvas& c = *ph.canvas;
        int first = core::imax(int(g.first_stage), core::imin(g.stage, data::stages_count - 2));   // okno 2 etapów: bieżący i kolejny
        for(int r = 0; r < 2 && first + r < data::stages_count; ++r)
        {
            int i = first + r;
            bool done = i < g.stage || (i == g.stage && g.st == core::status::won);
            bool cur = i == g.stage && ! done;
            stripe(c, r, done ? phone_tile::stripe_done : (cur ? phone_tile::stripe_prog : phone_tile::stripe_todo));
            core::message m; m.add(i - g.first_stage + 1).add(". ").add(data::stages[i].name);
            const char* state = done ? "Gotowe" : (cur ? "W trakcie" : "Do zrob.");
            phone_text(a, t, list_x, row_py(r), fit(a, m.s, pill_room(state)).c_str(), done ? ink::dim : ink::dark);
            phone_pill(a, c, t, pill_end, row_ty(r), state, done ? pill::done : (cur ? pill::prog : pill::gray));
        }
        {   // mechanika aktu (błoto, porywy, pył)
            const core::act_def& ad = g.adef();
            stripe(c, 2, phone_tile::stripe_late);
            core::message am;
            if(g.gust_in() > 0) am.add("Poryw za ").add(g.gust_in()).add(" t. ").add(core::game::dir_name(g.gust_dir()));
            else if(g.docs_needed() > 0)   // pieczątki: dokumenty otwierają schody
                am.add(g.stairs_locked() ? "Dokumenty " : "Schody otwarte ").add(g.docs_count()).add("/").add(g.docs_needed());
            else am.add(ad.mech_name);
            phone_text(a, t, list_x, row_py(2), fit(a, am.s, pill_room(ad.mech_short)).c_str(), ink::dark);
            phone_pill(a, c, t, pill_end, row_ty(2), ad.mech_short, pill::late);
        }
        {   // v0.21.50 cz. 3: wydarzenie z wyborem (i odpowiedź) albo magazyn na etapie
            core::message em;
            const char* pl = nullptr;
            pill pk = pill::gray;
            if(g.stage_choice >= 0)
            {
                const core::choice_event_def& ev = data::choice_events[g.stage_choice];
                em.add(ev.name).add(": ").add(ev.choices[g.stage_choice_pick].label);
                pl = "SMS"; pk = pill::brand;
            }
            else if(g.has_secret() && (g.explored(g.secret_x, g.secret_y) || g.keys > 0))
            {
                em.add(g.secret_open ? "Magazyn otwarty" : g.secret_def().name);
                if(! g.secret_open) em.add(g.keys > 0 ? ", masz klucz" : (g.secret_def().breakable ? ", klucz lub wybuch" : ", potrzebny klucz"));
                pl = g.secret_open ? "Otwarty" : "Magazyn"; pk = g.secret_open ? pill::done : pill::prog;
            }
            else if(g.keys > 0) { em.add("Klucz do magazynu: ").add(int(g.keys)); pl = "Klucz"; pk = pill::prog; }
            else if(g.bonus.weekly >= 0) { em.add(data::weekly[g.bonus.weekly].name); pl = "Tydzień"; pk = pill::late; }   // wyzwanie tygodnia (#34)
            if(pl)
            {
                stripe(c, 3, pk == pill::done ? phone_tile::stripe_done : phone_tile::stripe_brand);
                phone_text(a, t, list_x, row_py(3), fit(a, em.s, pill_room(pl)).c_str(), ink::dark);
                phone_pill(a, c, t, pill_end, row_ty(3), pl, pk);
            }
            else phone_text(a, t, list_x, row_py(3), "Bez wydarzeń i magazynu", ink::dim);
        }
        if(g.stage_event >= 0)   // wydarzenie na placu na tym etapie
        {
            const core::site_event_def& ev = data::site_events[g.stage_event];
            stripe(c, 4, ev.good ? phone_tile::stripe_done : phone_tile::stripe_late);
            int room = ((pill_end - utf8_len(ev.short_name) - 1) * 8 - list_x) / 7;   // znaki przed pastylką
            phone_text(a, t, list_x, row_py(4), clip(ev.name, room).c_str(), ink::dark);
            phone_pill(a, c, t, pill_end, row_ty(4), ev.short_name, ev.good ? pill::done : pill::late);
        }
        else phone_text(a, t, list_x, row_py(4), "Plac: bez niespodzianek", ink::dim);
        const core::weather_def& wd = g.wdef();   // pogoda dnia
        bool calm = wd.effect == core::weather_effect::none;
        stripe(c, 5, wd.bad ? phone_tile::stripe_late : (calm ? phone_tile::stripe_todo : phone_tile::stripe_done));
        core::message wm; wm.add("Pogoda: ").add(wd.name);
        phone_text(a, t, list_x, row_py(5), fit(a, wm.s, pill_room(wd.short_name)).c_str(), ink::dark);
        phone_pill(a, c, t, pill_end, row_ty(5), wd.short_name, wd.bad ? pill::late : (calm ? pill::gray : pill::done));
    }

    void tab_issues(app& a, phone_screen& ph, page_sprites& t)   // Usterki = problemy budowy
    {
        const core::game& g = *a.g;
        constexpr int types = int(sizeof(data::enemies) / sizeof(data::enemies[0]));
        const core::stage_def& sd = data::stages[g.stage];
        int open = 0, rows = 0;
        struct row { int8_t def; bool open; };
        bn::vector<row, 6> list;
        for(int d = 0; d < types && list.size() < 6; ++d)
        {
            bool in_stage = sd.boss == d;
            for(int k = 0; k < sd.pool_count; ++k) if(sd.pool[k] == d) in_stage = true;
            int alive = 0;
            for(int i = 0; i < g.enemies_count; ++i) if(g.enemies[i].alive && g.enemies[i].def_id == d) ++alive;
            if(! in_stage && g.kills_by_type[d] == 0) continue;
            list.push_back({ int8_t(d), alive > 0 });
            open += alive > 0;
        }
        core::message sub; sub.add("Otwarte: ").add(open);
        phone_header(a, ph, t, tab_names[1], sub.s);
        phone_canvas& c = *ph.canvas;
        for(const row& rw : list)
        {
            stripe(c, rows, rw.open ? phone_tile::stripe_late : phone_tile::stripe_done);
            core::message m; m.add("#").add(rows + 1).add(" ").add(clip(data::enemies[rw.def].name, 9).c_str());
            if(g.kills_by_type[rw.def] > 0) m.add(" x").add(g.kills_by_type[rw.def]);
            phone_text(a, t, list_x, row_py(rows), m.s, ink::dark);
            phone_pill(a, c, t, pill_end, row_ty(rows), rw.open ? "OTWARTA" : "ZAMKNIĘTA", rw.open ? pill::late : pill::done);
            ++rows;
        }
    }

    constexpr core::status_effect hud_statuses[4] = { core::status_effect::poison, core::status_effect::shock, core::status_effect::slip,
                                                      core::status_effect::wet };   // ikona = particle_pool::status_icon + indeks

    // Aktywne stany z turami do końca: jeden - pełna nazwa i skutek, kilka - skróty.
    core::message status_line(const core::game& g)
    {
        core::message m; m.add("Stany: ");
        int n = 0;
        for(core::status_effect s : hud_statuses) n += g.status_turns(s) > 0;
        if(n == 0) return m.add("brak");
        m.as(core::bad);
        bool first = true;
        for(core::status_effect s : hud_statuses)
        {
            int t = g.status_turns(s);
            if(t <= 0) continue;
            const core::status_def& sd = data::statuses[int(s)];
            if(n == 1) return m.add(sd.name).add(" ").add(t).add(" t. (").add(sd.effect).add(")");
            if(! first) m.add(", ");
            m.add(sd.short_name).add(" ").add(t);
            first = false;
        }
        return m.add(" t.");
    }

    void tab_home(app& a, phone_screen& ph, page_sprites& t)   // Start = pulpit postaci
    {
        const core::game& g = *a.g;
        core::message sub; sub.add(g.ddef().name);
        if(g.tier > 0) sub.add(" NG+").add(g.tier);
        sub.add(", ").add(g.cash).add(" zł");
        phone_header(a, ph, t, tab_names[2], sub.s);
        phone_canvas& c = *ph.canvas;
        core::message r0; r0.add("Etap ").add(g.stage_number()).add(": ").add(data::stages[g.stage].name);
        phone_text(a, t, list_x, row_py(0), fit(a, r0.s, pill_room("W trakcie")).c_str(), ink::dark);
        phone_pill(a, c, t, pill_end, row_ty(0), "W trakcie", pill::prog);
        stripe(c, 0, phone_tile::stripe_prog);

        core::message hp; hp.add("HP ").add(g.hero.hp).add("/").add(g.hero.max_hp);
        phone_text(a, t, list_x, row_py(1), hp.s, ink::dark);
        int bar_tile = g.hero.hp * 2 > g.hero.max_hp ? phone_tile::bar_done_0
                     : (g.hero.hp * 4 > g.hero.max_hp ? phone_tile::bar_brand_0 : phone_tile::bar_late_0);
        c.bar(14, row_ty(1), 13, bar_tile, g.hero.hp, g.hero.max_hp);

        core::message lv; lv.add("Poziom ").add(g.hero_level);
        phone_text(a, t, list_x, row_py(2), lv.s, ink::dark);
        int prev = g.hero_level >= 2 ? data::level_thresholds[g.hero_level - 2] : 0;
        if(g.xp_to_next() < 0) c.bar(14, row_ty(2), 13, phone_tile::bar_brand_0, 1, 1);
        else c.bar(14, row_ty(2), 13, phone_tile::bar_brand_0, g.run_xp - prev, data::level_thresholds[g.hero_level - 1] - prev);

        core::message mc; mc.add("Moc: ").add(ability_label(g).s);
        phone_text(a, t, list_x, row_py(3), mc.s, ink::dark);
        core::message cd; cd.add("za ").add(g.ability_cd);
        phone_pill(a, c, t, pill_end, row_ty(3), g.ability_cd == 0 ? "Gotowa" : cd.s, g.ability_cd == 0 ? pill::done : pill::gray);

        core::message sl = status_line(g);
        phone_text(a, t, list_x, row_py(4), fit(a, sl.s, phone_text_w).c_str(), sl.kind == core::bad ? ink::late : ink::dim);
        core::message sc = hero_stats_line(g);   // statystyki efektywne (baza+premie); A - opis i skąd premie
        phone_text(a, t, list_x, row_py(5), fit(a, sc.s, pill_room("A: opis")).c_str(), ink::dim);
        phone_pill(a, c, t, pill_end, row_ty(5), "A: opis", pill::group);
    }

    void mats_line(core::message& m, const core::game& g);

    // Sprzęt: narzędzie z zakresem ciosu i krytem (rozpiska #26 pod A) + kask, rękawice, kamizelka (+ buty i pas z nagród
    // za odbiór) - każdy przedmiot z tym, co daje (np. "+2 OBR"), cecha na pastylce; materiały, gdy jest miejsce
    // (przy 5 slotach tylko w HUD). Dół: Brygada i naprawy.
    void tab_gear(app& a, phone_screen& ph, page_sprites& t)
    {
        const core::game& g = *a.g;
        int slots[core::max_gear_slots], n = 0;
        for(int i = 0; i < data::gear_slots_count; ++i) if(((g.bonus.gear_slots >> i) & 1) || g.equipped[i] >= 0) slots[n++] = i;
        phone_header(a, ph, t, tab_names[3], "A obr. góra prem. dół Bryg.");
        phone_canvas& c = *ph.canvas;
        const core::dmg_breakdown b = g.weapon_breakdown();
        core::message w; g.weapon_title(w).add(" ");   // ulepszone: "Kielnia+2"
        core::add_range(w, b.min, b.max).add(", kryt ");
        core::add_range(w, b.crit_min, b.crit_max).add(" (").add(b.crit_chance()).add("%)");
        stripe(c, 0, phone_tile::stripe_brand);
        phone_text(a, t, list_x, row_py(0), fit(a, w.s, phone_text_w).c_str(), ink::dark);
        for(int k = 0; k < n; ++k)
        {
            int i = slots[k], r = g.equipped[i];
            stripe(c, 1 + k, r < 0 ? phone_tile::stripe_todo : (r == 2 ? phone_tile::stripe_prog : (r == 1 ? phone_tile::stripe_brand : phone_tile::stripe_done)));
            if(r < 0)
            {
                core::message m; m.add(data::gear_slots[i]).add(": brak");
                phone_text(a, t, list_x, row_py(1 + k), m.s, ink::dim);
                continue;
            }
            const core::gear_def& gd = data::gear[i * 3 + r];
            const char* trait = data::gear_traits[g.equipped_trait[i]].short_name;
            core::message eff; core::gear_label(eff, gd);   // co daje: "+2 obrażeń", "+1 OBR", "+8 HP"...
            int room = pill_room(trait), ew = a.text.width(eff.s);
            // pełna nazwa, a gdy się nie mieści obok skutku - nazwa slotu ("Rękawice +2 obrażeń"; jakość = kolor paska)
            bn::string<96> name = a.text.width(gd.name) <= room - ew - 6 ? bn::string<96>(gd.name) : fit(a, data::gear_slots[i], room - ew - 6);
            phone_text(a, t, list_x, row_py(1 + k), name.c_str(), ink::dark);
            phone_text(a, t, list_x + a.text.width(name) + 6, row_py(1 + k), eff.s, ink::brand);
            // pastylka = cecha przedmiotu, kolor = jakość
            phone_pill(a, c, t, pill_end, row_ty(1 + k), trait, r == 2 ? pill::prog : (r == 1 ? pill::group : pill::gray));
        }
        if(n <= 4)
        {
            core::message s1; s1.add("Materiały: ");   // cement, stal, drewno (Hurtownia, naprawy pod dół)
            for(int m = 0; m < data::materials_count; ++m) s1.add(m ? ", " : "").add(data::materials[m].short_name).add(" ").add(g.mats[m]);
            phone_text(a, t, list_x, row_py(n + 1), fit(a, s1.s, phone_text_w).c_str(), ink::dim);
        }
    }

    // Materiały skrótem, np. "C2 S1 D3" (pierwsza litera nazwy + liczba).
    void mats_line(core::message& m, const core::game& g)
    {
        for(int i = 0; i < data::materials_count; ++i)
        {
            char c[2] = { data::materials[i].name[0], 0 };
            m.add(i ? " " : "").add(c).add(g.mats[i]);
        }
    }

    // Brygada i naprawy (zakładka Sprzęt, strona pod A): najemni fachowcy raz na etap za budżet budowy, potem naprawy pola
    // za materiał (Załataj, Kładka). Lista przewijana: 4 wiersze; sel >= brigade_count = naprawa.
    constexpr int brigade_rows = data::brigade_count + data::repairs_count;

    void tab_brigade(app& a, phone_screen& ph, page_sprites& t, int sel)
    {
        const core::game& g = *a.g;
        core::message sub; sub.add(g.cash).add(" zł, ");
        mats_line(sub, g);
        phone_header(a, ph, t, sel < data::brigade_count ? "Brygada" : "Naprawy", sub.s);
        phone_canvas& c = *ph.canvas;
        int top = core::imax(0, core::imin(sel - 1, brigade_rows - 4));
        for(int r = 0; r < 4 && top + r < brigade_rows; ++r)
        {
            int i = top + r;
            bool is_sel = i == sel;
            if(is_sel) stripe(c, r, phone_tile::stripe_brand);
            if(i < data::brigade_count)
            {
                const core::helper_def& hd = data::brigade[i];
                bool unl = (g.bonus.helpers >> i) & 1, here = g.helper_called == i;
                core::message pr; pr.add(g.helper_price(i)).add(" zł");
                const char* pill_s = here ? "Na placu" : (unl ? pr.s : "Zablok.");
                phone_text(a, t, list_x, row_py(r), fit(a, hd.name, pill_room(pill_s)).c_str(), is_sel ? ink::brand : (unl ? ink::dark : ink::dim));
                phone_pill(a, c, t, pill_end, row_ty(r), pill_s, here ? pill::done : (unl && g.cash >= g.helper_price(i) ? pill::group : pill::gray));
            }
            else
            {
                const core::repair_def& rd = data::repairs[i - data::brigade_count];
                core::message pr; pr.add(rd.cost).add(" ").add(data::materials[rd.material].short_name);
                phone_text(a, t, list_x, row_py(r), fit(a, rd.name, pill_room(pr.s)).c_str(), is_sel ? ink::brand : ink::dark);
                phone_pill(a, c, t, pill_end, row_ty(r), pr.s, g.mats[rd.material] >= rd.cost ? pill::prog : pill::gray);
            }
        }
        core::message st;
        ink si = ink::dim;
        if(sel >= data::brigade_count)
        {
            int k = sel - data::brigade_count;
            const core::repair_def& rd = data::repairs[k];
            phone_text(a, t, list_x, row_py(4), fit(a, rd.info, phone_text_w).c_str(), ink::dim);
            switch(g.repair_blocked(k))
            {
                case core::game::repair_ok: st.add("A: napraw (tura)  B: wróć"); si = ink::brand; break;
                case core::game::repair_material: st.add("Brak: ").add(data::materials[rd.material].name).add(" (problemy, paczki)"); si = ink::late; break;
                case core::game::repair_no_target: st.add("Brak problemu w polu widzenia"); si = ink::late; break;
                case core::game::repair_no_room: st.add("Nie ma gdzie postawić"); si = ink::late; break;
                case core::game::repair_no_puddle: st.add("Brak kałuż obok"); si = ink::late; break;
                default: st.add("B: wróć"); break;
            }
            phone_text(a, t, list_x, row_py(5), fit(a, st.s, phone_text_w).c_str(), si);
            return;
        }
        phone_text(a, t, list_x, row_py(4), fit(a, data::brigade[sel].desc, phone_text_w).c_str(), ink::dim);
        if(g.helper_called >= 0)
        {
            st.add("Na tym etapie: ").add(data::brigade[g.helper_called].name);
            if(g.guard_turns > 0) st.add(" (").add(g.guard_turns).add(" t.)");
            if(g.ally_turns > 0) st.add(" (").add(g.ally_turns).add(" t.)");
            si = ink::done;
        }
        else switch(g.helper_blocked(sel))
        {
            case core::game::helper_ok: st.add("A: wezwij (tura)  B: wróć"); si = ink::brand; break;
            case core::game::helper_locked: st.add("Odblokuj w Szkoleniach"); break;
            case core::game::helper_cash: st.add("Za mały budżet"); si = ink::late; break;
            case core::game::helper_no_target: st.add("Nikogo w zasięgu ").add(data::brigade[sel].reach); si = ink::late; break;
            case core::game::helper_no_room: st.add("Brak miejsca obok"); si = ink::late; break;
            default: st.add("B: wróć"); break;
        }
        phone_text(a, t, list_x, row_py(5), fit(a, st.s, phone_text_w).c_str(), si);
    }

    // Premie z tej budowy (#27, zakładka Sprzęt, strona pod górą): wybrane premie (kolor = rzadkość) i aktywne synergie.
    // Lista 4 wiersze (góra/dół), pod nią opis zaznaczonej i znaczniki; B - wróć.
    int boons_items(const core::game& g, int* out)
    {
        int n = 0;
        for(int b = 0; b < data::boons_count; ++b) if(g.has_boon(b)) out[n++] = b;
        for(int k = 0; k < data::synergies_count; ++k) if(g.synergy_active(k)) out[n++] = 100 + k;
        return n;
    }

    void tab_boons(app& a, phone_screen& ph, page_sprites& t, int sel)
    {
        const core::game& g = *a.g;
        int items[64];
        const int n = boons_items(g, items);
        core::message sub; sub.add(g.boons_owned()).add(" premii, B: wróć");
        phone_header(a, ph, t, "Premie", sub.s);
        phone_canvas& c = *ph.canvas;
        if(n == 0)
        {
            phone_text(a, t, list_x, row_py(0), "Brak premii", ink::dim);
            phone_text(a, t, list_x, row_py(1), "Po każdym etapie: 1 z 3", ink::dim);
            phone_text(a, t, list_x, row_py(2), "2+ z tym samym znacznikiem", ink::dim);
            phone_text(a, t, list_x, row_py(3), "= synergia (dodatkowy skutek)", ink::dim);
            return;
        }
        int top = core::imax(0, core::imin(sel - 1, n - 4));
        for(int r = 0; r < 4 && top + r < n; ++r)
        {
            const int it = items[top + r];
            const bool on = top + r == sel;
            if(it >= 100)
            {
                const core::synergy_def& sd = data::synergies[it - 100];
                stripe(c, r, phone_tile::stripe_done);
                phone_text(a, t, list_x, row_py(r), fit(a, sd.name, pill_room("Synergia")).c_str(), on ? ink::brand : ink::done);
                phone_pill(a, c, t, pill_end, row_ty(r), "Synergia", pill::done);
                continue;
            }
            const core::boon_def& bd = data::boons[it];
            stripe(c, r, on ? phone_tile::stripe_late : rarity_stripe(bd.rarity));
            const char* rn = data::boon_rarities[bd.rarity].name;
            phone_text(a, t, list_x, row_py(r), fit(a, bd.name, pill_room(rn)).c_str(), on ? ink::brand : rarity_ink(bd.rarity));
            phone_pill(a, c, t, pill_end, row_ty(r), rn, rarity_pill(bd.rarity));
        }
        const int it = items[sel];
        if(it >= 100)
        {
            phone_text(a, t, list_x, row_py(4), fit(a, data::synergies[it - 100].desc, phone_text_w).c_str(), ink::dark);
            core::message tg; tg.add("Znaczniki: ");
            const uint16_t tags = data::synergies[it - 100].tags;
            bool first = true;
            for(int k = 0; k < data::boon_tags_count; ++k) if((tags >> k) & 1) { tg.add(first ? "" : " + ").add(data::boon_tags[k]); first = false; }
            phone_text(a, t, list_x, row_py(5), fit(a, tg.s, phone_text_w).c_str(), ink::dim);
        }
        else
        {
            phone_text(a, t, list_x, row_py(4), fit(a, data::boons[it].desc, phone_text_w).c_str(), ink::dark);
            core::message tg; tg.add("Znaczniki: ").add(boon_tags_line(data::boons[it]).s);
            phone_text(a, t, list_x, row_py(5), fit(a, tg.s, phone_text_w).c_str(), ink::dim);
        }
    }

    void tab_costs(app& a, phone_screen& ph, page_sprites& t)   // Koszty = Szkolenia (podgląd w trakcie budowy)
    {
        const core::game& g = *a.g;
        phone_header(a, ph, t, tab_names[4], "Szkolenia");
        phone_canvas& c = *ph.canvas;
        int total = core::shop_total_cost(), spent = core::shop_spent(a.save);
        phone_text(a, t, list_x, row_py(0), "WYDANE NA SZKOLENIA", ink::dim);
        core::message sp; sp.add(spent).add(" z ").add(total).add(" dośw.");
        phone_text(a, t, list_x, row_py(1), sp.s, ink::dark);
        c.bar(2, row_ty(2), 26, phone_tile::bar_brand_0, spent, total);
        phone_text(a, t, list_x, row_py(3), "Do wydania po budowie", ink::dim);
        core::message rest; rest.add(int(a.save.xp)).add(" dośw.");
        phone_text(a, t, 226, row_py(3), rest.s, ink::done, 1);
        core::message run; run.add("Z tej budowy: +").add(g.xp() - g.xp_banked).add(" dośw.");
        phone_text(a, t, list_x, row_py(4), run.s, ink::dim);
        core::message rs; rs.add("Respekt ").add(int(a.save.respect));   // Respekt za etapy jest już w profilu
        phone_text(a, t, 226, row_py(4), rs.s, ink::brand, 1);
        int ci = core::next_contract(a.save, g);   // najbliższe zlecenie z postępem na żywo
        if(ci >= 0)
        {
            const core::contract_def& cd = data::contracts[ci];
            core::message cm; cm.add("Zlecenie: ").add(cd.name);
            core::message pm; pm.add(core::imin(cd.target, core::contract_progress_live(a.save, g, ci))).add("/").add(cd.target);
            phone_text(a, t, list_x, row_py(5), fit(a, cm.s, pill_room(pm.s)).c_str(), ink::dim);
            phone_pill(a, c, t, pill_end, row_ty(5), pm.s, pill::prog);
        }
        else phone_text(a, t, list_x, row_py(5), "Kupisz po budowie", ink::dim);
    }

    void draw_tab(app& a, phone_screen& ph, page_sprites& t, int tab)
    {
        switch(tab)
        {
            case 0: tab_tasks(a, ph, t); break;
            case 1: tab_issues(a, ph, t); break;
            case 3: tab_gear(a, ph, t); break;
            case 4: tab_costs(a, ph, t); break;
            default: tab_home(a, ph, t); break;
        }
        ph.set_tab(tab);
        ph.commit();
    }

    void stats_page(app& a, phone_screen& ph, page_sprites& t, const core::game* g, int cls, const core::run_mods& m, int start_page = 0);

    enum class pause_result { resume, quit, save_exit };

    // Telefon pod SELECT. L/R lub strzałki: zakładki; START: menu akcji; B/SELECT: powrót do gry.
    pause_result run_phone(app& a, bool brigade_page = false)
    {
        phone_screen ph(a.phone_tab);
        page_sprites t;
        bn::sprite_palette_item default_ink = a.text.palette_item();
        const char* actions[] = { "Wróć do gry", "Jak grać", "Zapisz i wyjdź", "Porzuć budowę" };
        constexpr int actions_count = 4;
        bool sheet = false, confirm = false, brigade = brigade_page;   // brigade: strona Brygada w zakładce Zespół (A)
        bool boons = false;   // strona Premie w zakładce Sprzęt (góra)
        int sel = 0, bsel = 0, psel = 0;
        auto redraw = [&]() {
            if(! sheet && boons && a.phone_tab == 3) { tab_boons(a, ph, t, psel); ph.set_tab(3); ph.commit(); return; }
            if(! sheet && brigade && a.phone_tab == 3) { tab_brigade(a, ph, t, bsel); ph.set_tab(3); ph.commit(); return; }
            if(! sheet) { draw_tab(a, ph, t, a.phone_tab); return; }
            phone_header(a, ph, t, "Menu", "START");
            for(int i = 0; i < actions_count; ++i)
            {
                if(i == sel) stripe(*ph.canvas, i, phone_tile::stripe_brand);
                phone_text(a, t, list_x, row_py(i), actions[i], i == sel ? ink::brand : ink::dark);
            }
            phone_text(a, t, list_x, row_py(5), confirm ? "Na pewno? A: tak  B: nie" : "A: wybierz  B: wróć",
                       confirm ? ink::late : ink::dim);
            ph.commit();
        };
        auto finish = [&](pause_result r) {
            t.clear();
            a.text.set_palette_item(default_ink);
            wait_release();
            return r;
        };
        redraw();
        wait_release();
        while(true)
        {
            if(sheet)
            {
                if(confirm)
                {
                    if(bn::keypad::a_pressed()) return finish(pause_result::quit);
                    if(bn::keypad::b_pressed()) { confirm = false; redraw(); }
                }
                else
                {
                    if(bn::keypad::up_pressed()) { sel = (sel + actions_count - 1) % actions_count; redraw(); }
                    if(bn::keypad::down_pressed()) { sel = (sel + 1) % actions_count; redraw(); }
                    if(bn::keypad::b_pressed()) { sheet = false; redraw(); }
                    if(bn::keypad::a_pressed())
                    {
                        if(sel == 0) return finish(pause_result::resume);
                        if(sel == 2) return finish(pause_result::save_exit);
                        if(sel == 3) { confirm = true; redraw(); }
                        else
                        {
                            t.clear();
                            ph.set_visible(false);
                            a.text.set_palette_item(default_ink);
                            wait_release();
                            page_help(a);
                            ph.set_visible(true);
                            redraw();
                        }
                    }
                }
            }
            else
            {
                int d = (bn::keypad::r_pressed() || bn::keypad::right_pressed()) ? 1
                      : ((bn::keypad::l_pressed() || bn::keypad::left_pressed()) ? -1 : 0);
                if(d) { a.phone_tab = (a.phone_tab + d + tabs_count) % tabs_count; brigade = false; boons = false; redraw(); bn::sound_items::sfx_menu.play(); }
                else if(boons && a.phone_tab == 3)   // Premie: góra/dół przewija, B wraca do Sprzętu
                {
                    int items[64];
                    const int n = boons_items(*a.g, items);
                    int v = bn::keypad::up_pressed() ? -1 : (bn::keypad::down_pressed() ? 1 : 0);
                    if(v && n > 0) { psel = (psel + v + n) % n; redraw(); bn::sound_items::sfx_menu.play(); }
                    if(bn::keypad::b_pressed()) { boons = false; redraw(); bn::sound_items::sfx_menu.play(); next_frame(); continue; }
                }
                else if(brigade && a.phone_tab == 3)   // Brygada: góra/dół wybór, A wezwij, B wróć do Sprzętu
                {
                    int v = bn::keypad::up_pressed() ? -1 : (bn::keypad::down_pressed() ? 1 : 0);
                    if(v) { bsel = (bsel + v + brigade_rows) % brigade_rows; redraw(); bn::sound_items::sfx_menu.play(); }
                    if(bn::keypad::a_pressed())
                    {
                        bool ok = bsel < data::brigade_count ? a.g->helper_blocked(bsel) == core::game::helper_ok
                                                             : a.g->repair_blocked(bsel - data::brigade_count) == core::game::repair_ok;
                        if(ok) { a.pending_helper = bsel; return finish(pause_result::resume); }   // wezwanie / naprawa po powrocie do gry
                        bn::sound_items::sfx_hurt.play();
                    }
                    if(bn::keypad::b_pressed()) { brigade = false; redraw(); bn::sound_items::sfx_menu.play(); next_frame(); continue; }
                }
                else if(a.phone_tab == 3 && bn::keypad::down_pressed()) { brigade = true; bsel = 0; redraw(); bn::sound_items::sfx_menu.play(); }
                else if(a.phone_tab == 3 && bn::keypad::up_pressed()) { boons = true; psel = 0; redraw(); bn::sound_items::sfx_menu.play(); }
                else if(a.phone_tab == 3 && bn::keypad::a_pressed())   // Sprzęt: A = rozpiska obrażeń broni (#26)
                {
                    stats_page(a, ph, t, a.g, a.g->cls, a.g->bonus, 1);
                    redraw();
                    next_frame();
                    continue;
                }
                else if(a.phone_tab == 2 && bn::keypad::a_pressed())   // Start: A = statystyki i skąd są premie
                {
                    stats_page(a, ph, t, a.g, a.g->cls, a.g->bonus);
                    redraw();
                    next_frame();
                    continue;
                }
                if(bn::keypad::start_pressed()) { sheet = true; sel = 0; redraw(); }
                if(bn::keypad::b_pressed() || bn::keypad::select_pressed()) return finish(pause_result::resume);
            }
            next_frame();
        }
    }

    const char* gear_stat_name(core::gear_stat s)
    {
        switch(s)
        {
            case core::gear_stat::def:     return "Obrona";
            case core::gear_stat::dmg:     return "Obrażenia";
            case core::gear_stat::dodge:   return "Unik %";
            case core::gear_stat::thermos: return "Termos";
            default:                       return "Max HP";
        }
    }

    // ------------------------------------------------------------------ v0.21.50 cz. 3: wydarzenia, ulepszenie narzędzia
    // Cecha ulepszonego narzędzia (+2): 3 karty (nazwa, skrót, opis), góra/dół, A - wybór. Rysuje na podanym telefonie
    // (Hurtownia albo osobne okno po wydarzeniu).
    void trait_loop(app& a, phone_screen& ph, page_sprites& t)
    {
        core::game& g = *a.g;
        int sel = 0;
        auto redraw = [&]() {
            core::message title; title.add("   ").add(g.weapon().name).add("+").add(g.weapon_lvl);
            phone_header(a, ph, t, title.s, "Wybierz cechę");
            phone_canvas& c = *ph.canvas;
            for(int k = 0; k < data::tool_traits_count && k < 3; ++k)
            {
                const core::tool_trait_def& td = data::tool_traits[k];
                const bool on = k == sel;
                if(on) c.rounded(1, row_ty(2 * k), 28, 4, phone_tile::fill_group, phone_tile::corner_group);
                stripe(c, 2 * k, on ? phone_tile::stripe_brand : phone_tile::stripe_todo);
                stripe(c, 2 * k + 1, on ? phone_tile::stripe_brand : phone_tile::stripe_todo);
                phone_text(a, t, list_x, row_py(2 * k), fit(a, td.name, pill_room(td.short_name)).c_str(), on ? ink::brand : ink::dark);
                phone_pill(a, c, t, pill_end, row_ty(2 * k), td.short_name, on ? pill::prog : pill::gray);
                phone_text(a, t, list_x, row_py(2 * k + 1), fit(a, td.desc, phone_text_w).c_str(), on ? ink::dark : ink::dim);
            }
            ph.commit();
        };
        redraw();
        bn::sound_items::sfx_notify.play(bn::fixed(0.7));
        wait_release();
        while(g.trait_pending)
        {
            int d = bn::keypad::up_pressed() ? -1 : (bn::keypad::down_pressed() ? 1 : 0);
            if(d) { sel = (sel + d + data::tool_traits_count) % data::tool_traits_count; redraw(); bn::sound_items::sfx_menu.play(); }
            if(bn::keypad::a_pressed() && g.choose_trait(sel)) bn::sound_items::sfx_level.play();
            next_frame();
        }
        wait_release();
    }
    void trait_dialog(app& a)
    {
        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        phone_screen ph(3);
        ph.icon.set_visible(false);
        bn::sprite_ptr icon = bn::sprite_items::menu_icons.create_sprite(-100, -59, icon_upgrade);
        icon.set_bg_priority(1);
        bn::sprite_palette_item default_ink = a.text.palette_item();
        page_sprites t;
        trait_loop(a, ph, t);
        t.clear();
        a.text.set_palette_item(default_ink);
    }

    // Wydarzenie z wyborem (#30): SMS (nadawca, 3 linie), potem odpowiedzi (2-3, skutki pod spodem), na końcu wynik
    // (co zaszło, a co "nie tym razem"). A - dalej / wybór, góra/dół - odpowiedź.
    void event_dialog(app& a)
    {
        core::game& g = *a.g;
        if(g.pending_event < 0) return;
        const core::choice_event_def& ev = data::choice_events[g.pending_event];
        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        phone_screen ph(2);
        ph.icon.set_visible(false);
        bn::sprite_ptr icon = bn::sprite_items::menu_icons.create_sprite(-100, -59, icon_event);
        icon.set_bg_priority(1);
        bn::sprite_palette_item default_ink = a.text.palette_item();
        page_sprites t;
        int page = 0, sel = 0;
        auto redraw = [&]() {
            phone_canvas& c = *ph.canvas;
            if(page == 0)   // SMS
            {
                phone_header(a, ph, t, "   Wiadomości", ev.name);
                phone_text(a, t, list_x, row_py(0), ev.msg.from, ink::dark);
                phone_pill(a, c, t, pill_end, row_ty(0), "teraz", pill::gray);
                c.rounded(2, row_ty(1), 26, 6, phone_tile::fill_group, phone_tile::corner_group);
                for(int i = 0; i < 3; ++i) phone_text(a, t, 22, row_py(1 + i), ev.msg.lines[i], ink::dark);
                phone_text(a, t, list_x, row_py(4), "Wydarzenie: wybierz odpowiedź", ink::brand);
                phone_text(a, t, 226, row_py(5), "A: odpowiedz", ink::brand, 1);
            }
            else if(page == 1)   // odpowiedzi
            {
                phone_header(a, ph, t, "   Odpowiedź", "góra/dół, A: wybierz");
                for(int k = 0; k < ev.choices_count; ++k)
                {
                    const core::event_choice& ch = ev.choices[k];
                    const bool on = k == sel;
                    if(on) c.rounded(1, row_ty(2 * k), 28, 4, phone_tile::fill_group, phone_tile::corner_group);
                    stripe(c, 2 * k, on ? phone_tile::stripe_brand : phone_tile::stripe_todo);
                    stripe(c, 2 * k + 1, on ? phone_tile::stripe_brand : phone_tile::stripe_todo);
                    phone_text(a, t, list_x, row_py(2 * k), ch.label, on ? ink::brand : ink::dark);
                    core::message m; core::choice_label(m, ch);
                    phone_text(a, t, list_x, row_py(2 * k + 1), fit(a, m.s, phone_text_w).c_str(), on ? ink::dark : ink::dim);
                }
                if(ev.choices_count < 3) phone_text(a, t, list_x, row_py(5), ev.name, ink::dim);
            }
            else   // wynik
            {
                const core::event_choice& ch = ev.choices[g.stage_choice_pick];
                phone_header(a, ph, t, "   Wynik", ev.name);
                core::message m0; m0.add("Odpowiedź: ").add(ch.label);
                phone_text(a, t, list_x, row_py(0), fit(a, m0.s, phone_text_w).c_str(), ink::dark);
                stripe(c, 1, phone_tile::stripe_done);
                phone_text(a, t, list_x, row_py(1), fit(a, ch.result, phone_text_w).c_str(), ink::done);
                if(ch.outs == 0) phone_text(a, t, list_x, row_py(2), "Bez skutków", ink::dim);
                for(int i = 0; i < ch.outs && i < 3; ++i)
                {
                    const bool done = (g.choice_done >> i) & 1;
                    core::message m;
                    if(! done) m.add("Nie tym razem: ");
                    core::choice_out o = ch.out[i];
                    o.chance = 100;   // szansa już rozstrzygnięta
                    core::choice_out_label(m, o);
                    const bool bad = o.effect == core::choice_effect::spawn || o.effect == core::choice_effect::status || o.value < 0;
                    stripe(c, 2 + i, ! done ? phone_tile::stripe_todo : (bad ? phone_tile::stripe_late : phone_tile::stripe_done));
                    phone_text(a, t, list_x, row_py(2 + i), fit(a, m.s, phone_text_w).c_str(), ! done ? ink::dim : (bad ? ink::late : ink::dark));
                }
                phone_text(a, t, 226, row_py(5), "A: dalej", ink::brand, 1);
            }
            ph.commit();
        };
        redraw();
        bn::sound_items::sfx_notify.play(bn::fixed(0.7));
        wait_release();
        while(true)
        {
            if(page == 1)
            {
                int d = bn::keypad::up_pressed() ? -1 : (bn::keypad::down_pressed() ? 1 : 0);
                if(d) { sel = (sel + d + ev.choices_count) % ev.choices_count; redraw(); bn::sound_items::sfx_menu.play(); }
            }
            if(bn::keypad::a_pressed() || (page != 1 && bn::keypad::start_pressed()))
            {
                if(page == 1) { g.choose_event(sel); bn::sound_items::sfx_buy.play(); }
                if(page == 2) break;
                ++page;
                wait_release();
                redraw();
            }
            next_frame();
        }
        t.clear();
        a.text.set_palette_item(default_ink);
        wait_release();
    }

    // Narzędzie na polu przy ulepszonym (#31): porównanie, ostrzeżenie o utracie ulepszenia. A - zamieniam, B - zostaję.
    void tool_offer_dialog(app& a)
    {
        core::game& g = *a.g;
        if(! g.has_tool_offer()) return;
        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        phone_screen ph(3);
        bn::sprite_palette_item default_ink = a.text.palette_item();
        page_sprites t;
        const int w = data::tools[g.tool_offer].weapon;
        phone_header(a, ph, t, "Nowe narzędzie", data::weapons[w].name);
        phone_canvas& c = *ph.canvas;
        const core::dmg_breakdown now = g.weapon_breakdown(), next = g.weapon_breakdown(-1, w);
        core::message m0; m0.add("Teraz: "); g.weapon_title(m0);
        core::add_range(m0.add(" "), now.min, now.max);
        stripe(c, 0, phone_tile::stripe_brand);
        phone_text(a, t, list_x, row_py(0), fit(a, m0.s, phone_text_w).c_str(), ink::dark);
        core::message m1; m1.add("Nowe: ").add(data::weapons[w].name).add(" ");
        core::add_range(m1, next.min, next.max).add(", zasięg ").add(data::weapons[w].range);
        stripe(c, 1, phone_tile::stripe_prog);
        phone_text(a, t, list_x, row_py(1), fit(a, m1.s, phone_text_w).c_str(), ink::dark);
        core::message m2; core::compare_line(m2, now, next);
        if(a.text.width(m2.s) > phone_text_w) { m2 = core::message(); core::compare_line(m2, now, next, true); }
        phone_text(a, t, list_x, row_py(2), fit(a, m2.s, phone_text_w).c_str(), next.avg10 > now.avg10 ? ink::done : ink::late);
        core::message m3; m3.add("Uwaga: ulepszenie +").add(g.weapon_lvl).add(" przepadnie!");
        stripe(c, 3, phone_tile::stripe_late);
        phone_text(a, t, list_x, row_py(3), m3.s, ink::late);
        if(g.weapon_trait >= 0)
        {
            core::message m4; m4.add("Też cecha: ").add(data::tool_traits[g.weapon_trait].name).add(" (").add(data::tool_traits[g.weapon_trait].short_name).add(")");
            phone_text(a, t, list_x, row_py(4), fit(a, m4.s, phone_text_w).c_str(), ink::dim);
        }
        phone_text(a, t, list_x, row_py(5), "A: zamieniam  B: zostaję", ink::dark);
        ph.commit();
        bn::sound_items::sfx_notify.play(bn::fixed(0.7));
        wait_release();
        while(g.has_tool_offer())
        {
            if(bn::keypad::a_pressed()) { g.accept_tool(); bn::sound_items::sfx_buy.play(); }
            else if(bn::keypad::b_pressed()) { g.decline_tool(); bn::sound_items::sfx_menu.play(); }
            next_frame();
        }
        t.clear();
        a.text.set_palette_item(default_ink);
        wait_release();
    }

    // Paczka sprzętu przy zajętym slocie: porównanie obecny / nowy z cechami. A: zakładam, B: zostawiam.
    void gear_offer_dialog(app& a)
    {
        core::game& g = *a.g;
        phone_screen ph(3);
        bn::sprite_palette_item default_ink = a.text.palette_item();
        page_sprites t;
        const int slot = g.offer_slot;
        phone_header(a, ph, t, "Paczka sprzętu", data::gear_slots[slot]);
        phone_canvas& c = *ph.canvas;
        auto item_rows = [&](int row, int rarity, int trait, bool is_new) {
            const core::gear_def& gd = data::gear[slot * 3 + rarity];
            stripe(c, row, is_new ? phone_tile::stripe_prog : phone_tile::stripe_todo);
            stripe(c, row + 1, is_new ? phone_tile::stripe_prog : phone_tile::stripe_todo);
            phone_text(a, t, list_x, row_py(row), clip(gd.name, 17).c_str(), ink::dark);
            phone_pill(a, c, t, pill_end, row_ty(row), data::gear_rarities[rarity],
                       rarity == 2 ? pill::prog : (rarity == 1 ? pill::group : pill::gray));
            core::message m; m.add(is_new ? "Nowy: " : "Teraz: ").add(gear_stat_name(gd.stat)).add(" +").add(gd.value)
                                .add(", ").add(data::gear_traits[trait].short_name);
            phone_text(a, t, list_x, row_py(row + 1), fit(a, m.s, phone_text_w).c_str(), is_new ? ink::brand : ink::dim);
        };
        item_rows(0, g.equipped[slot], g.equipped_trait[slot], false);
        item_rows(2, g.offer_rarity, g.offer_trait, true);
        // wiersz 4: porównanie (rozpiska #26) - cios, kryt, obrona; kilka zmian na zmianę co ~1,5 s, bez zmian - cecha
        const core::dmg_breakdown now = g.weapon_breakdown(), next = g.weapon_breakdown(-1, -1, slot, g.offer_rarity, g.offer_trait);
        core::message cmp[3]; int cmp_n = 0;
        if(now.min != next.min || now.max != next.max || now.avg10 != next.avg10) { core::compare_line(cmp[cmp_n], now, next); if(a.text.width(cmp[cmp_n].s) > phone_text_w) { cmp[cmp_n] = core::message(); core::compare_line(cmp[cmp_n], now, next, true); } ++cmp_n; }
        if(now.crit_chance() != next.crit_chance() || now.crit_max != next.crit_max) core::compare_crit(cmp[cmp_n++], now, next);
        const core::gear_def& gold = data::gear[slot * 3 + g.equipped[slot]], &gnew = data::gear[slot * 3 + g.offer_rarity];
        if(gold.stat == core::gear_stat::def && gnew.value != gold.value)
        {
            int d0 = g.hero_defense(), d1 = d0 - gold.value + gnew.value;
            cmp[cmp_n++].add("OBR ").add(d0).add(" -> ").add(d1).add(": z ciosu -").add(d0 / 2).add(" -> -").add(d1 / 2);
        }
        const bool neutral = cmp_n == 0;   // bez wpływu na walkę: jak dawniej cecha nowego przedmiotu
        if(neutral) cmp[cmp_n++].add("Cecha: ").add(data::gear_traits[g.offer_trait].name);
        page_sprites t4;
        auto draw_cmp = [&](int i) {
            t4.clear();
            phone_text(a, t4, list_x, row_py(4), fit(a, cmp[i].s, phone_text_w).c_str(), neutral ? ink::dim : ink::brand);
        };
        draw_cmp(0);
        core::message km; km.add("A: zakładam  B: zostawiam (+").add(data::gear_decline_xp + g.offer_rarity).add(")");
        phone_text(a, t, list_x, row_py(5), km.s, g.offer_is_better() ? ink::done : ink::dark);
        ph.commit();
        bn::sound_items::sfx_notify.play(bn::fixed(0.7));
        wait_release();
        int clock = 0;
        while(g.has_offer())
        {
            if(bn::keypad::a_pressed()) { g.accept_offer(); bn::sound_items::sfx_buy.play(); }
            else if(bn::keypad::b_pressed()) { g.decline_offer(); bn::sound_items::sfx_menu.play(); }
            else if(cmp_n > 1 && ++clock % 90 == 0) draw_cmp((clock / 90) % cmp_n);
            next_frame();
        }
        t4.clear();
        t.clear();
        a.text.set_palette_item(default_ink);
        wait_release();
    }

    // Rozpiska obrażeń broni (#26) jako lista wierszy telefonu: broń, statystyka, premie (tylko te, które coś dają),
    // cios, kryt z częściami, moc, obrona i unik (w trakcie budowy). Zwraca liczbę wierszy.
    struct dmg_rows { core::message line[20]; ink k[20]; int stripe[20]; int n = 0; };

    // Wiersz rozpiski; dłuższy niż telefon dzieli się po ostatnim ", " / ": " / " + ", które się mieści - reszta
    // w kolejnym wierszu z wcięciem.
    void push_row(app& a, dmg_rows& out, const core::message& l, ink k, int stripe_tile)
    {
        if(out.n >= 20) return;
        int cut = -1;
        if(a.text.width(l.s) > phone_text_w)
            for(int p = 1; p + 2 < l.n; ++p)
                if(l.s[p] == ' ' && (l.s[p - 1] == ',' || l.s[p - 1] == ':' || (l.s[p + 1] == '+' && l.s[p + 2] == ' ')))
                {
                    bn::string<96> pre(l.s, p);
                    if(a.text.width(pre) <= phone_text_w) cut = p;
                }
        if(cut < 0 || out.n >= 19) { out.line[out.n] = l; out.k[out.n] = k; out.stripe[out.n++] = stripe_tile; return; }
        core::message first; first.kind = l.kind;
        bn::string<96> pre(l.s, cut);
        first.add(pre.c_str());
        out.line[out.n] = first; out.k[out.n] = k; out.stripe[out.n++] = stripe_tile;
        core::message rest; rest.add("   ").add(l.s + cut + 1);
        out.line[out.n] = rest; out.k[out.n] = k; out.stripe[out.n++] = -1;
    }

    // Rozpiska obrażeń broni (#26) jako lista wierszy telefonu: broń, statystyka, premie (tylko te, które coś dają),
    // cios, kryt z częściami, moc, obrona i unik (w trakcie budowy).
    void breakdown_rows(app& a, dmg_rows& out, const core::dmg_breakdown& b, const core::game* g)
    {
        using core::dmg_text;
        const dmg_text order[] = { dmg_text::weapon, dmg_text::stat, dmg_text::stat_parts, dmg_text::profile, dmg_text::run,
                                   dmg_text::gear, dmg_text::upgrade, dmg_text::pct, dmg_text::boon, dmg_text::enemy, dmg_text::total, dmg_text::power, dmg_text::crit,
                                   dmg_text::crit_parts, dmg_text::crit_extra };
        for(dmg_text k : order)
        {
            core::message l;
            bool shown = core::dmg_line(l, b, k);
            bool always = k == dmg_text::weapon || k == dmg_text::stat || k == dmg_text::total || k == dmg_text::crit
                       || k == dmg_text::crit_parts;
            if(! shown && ! always) continue;
            ink i = k == dmg_text::weapon ? ink::brand : (k == dmg_text::total ? ink::done
                  : (k == dmg_text::crit ? ink::prog : (k == dmg_text::stat || k == dmg_text::profile || k == dmg_text::run
                                                        || k == dmg_text::gear || k == dmg_text::pct || k == dmg_text::boon
                                                        || k == dmg_text::upgrade ? ink::dark : ink::dim)));
            int st = k == dmg_text::weapon ? phone_tile::stripe_brand : (k == dmg_text::total ? phone_tile::stripe_done
                   : (k == dmg_text::crit ? phone_tile::stripe_prog : -1));
            push_row(a, out, l, i, st);
        }
        if(g)   // obrona i unik: co zmniejsza ciosy problemów
        {
            const core::game& G = *g;
            core::message d; d.add("OBR ").add(G.hero_defense()).add(": -").add(G.hero_defense() / 2).add(" z ciosu problemu");
            if(G.bonus.taken_pct) d.add(", -").add(G.bonus.taken_pct).add("%");
            push_row(a, out, d, ink::dark, -1);
            core::message u; u.add("Unik ").add(G.dodge_pct()).add("% (SZCZ ").add(G.luck()).add(" x ").add(data::dodge_per_luck_pct).add("%");
            if(G.gear_bonus(core::gear_stat::dodge)) u.add(", buty +").add(G.gear_bonus(core::gear_stat::dodge)).add("%");
            const int pd = G.bonus.dodge + G.boon_sum(core::boon_effect::dodge);
            if(pd) u.add(", premie +").add(pd).add("%");
            u.add(")");
            push_row(a, out, u, ink::dim, -1);
        }
    }

    // Rozpiska obrażeń: z bieżącej budowy (g) albo dla zawodu przed budową; źródła premii z profilu, gdy się zgadzają.
    core::dmg_breakdown hero_breakdown(app& a, const core::game* g, int cls, const core::run_mods& m)
    {
        core::dmg_breakdown b = g ? g->weapon_breakdown() : core::class_breakdown(cls, m);
        core::run_mods parts[core::mods_sources];
        core::mods_parts(a.save, parts);
        b.set_sources(parts);
        return b;
    }

    // Opis statystyk (#19): strona 1 - wartości i co dają (na wyborze zawodu) albo skąd są premie (w trakcie budowy),
    // strony 2+ - rozpiska obrażeń broni (#26, jak w BG3), ostatnia - wzory w prostych słowach.
    // A / prawo / dół: następna strona, lewo / góra: poprzednia, B / START / SELECT: wróć. start_page 1 = obrażenia.
    void stats_page(app& a, phone_screen& ph, page_sprites& t, const core::game* g, int cls, const core::run_mods& m, int start_page)
    {
        bn::sprite_palette_item default_ink = a.text.palette_item();
        const core::class_def& c = data::classes[cls];
        const core::weapon_def& w = g ? g->weapon() : data::weapons[c.weapon];
        const core::stat stats[3] = { core::stat::str, core::stat::agi, core::stat::intel };
        dmg_rows dr;
        breakdown_rows(a, dr, hero_breakdown(a, g, cls, m), g);
        const int dmg_pages = (dr.n + 5) / 6, pages = dmg_pages + 2;
        int page = start_page < pages ? start_page : 0;
        auto redraw = [&]() {
            core::message sub;
            if(page == 0) sub.add(c.name);
            else if(page == pages - 1) sub.add("A: strona  B: wróć");
            else sub.add(page).add("/").add(dmg_pages).add("  A: dalej");
            phone_header(a, ph, t, page == 0 ? "Statystyki" : (page == pages - 1 ? "Jak działają" : "Obrażenia broni"), sub.s);
            phone_canvas& cv = *ph.canvas;
            if(page > 0 && page < pages - 1)
            {
                for(int r = 0; r < 6; ++r)
                {
                    int i = (page - 1) * 6 + r;
                    if(i >= dr.n) break;
                    if(dr.stripe[i] >= 0) stripe(cv, r, dr.stripe[i]);
                    phone_text(a, t, list_x, row_py(r), fit(a, dr.line[i].s, phone_text_w).c_str(), dr.k[i]);
                }
                ph.commit();
                return;
            }
            for(int r = 0; r < 6; ++r)
            {
                core::message l;
                ink k = ink::dark;
                if(page == pages - 1)   // wzory: statystyka broni, obrona, szczęście (3 wiersze), HP
                {
                    const core::stat_kind ws = w.scales_with == core::stat::str ? core::stat_kind::str
                                             : (w.scales_with == core::stat::agi ? core::stat_kind::agi : core::stat_kind::intel);
                    const core::stat_kind rk[6] = { ws, core::stat_kind::def, core::stat_kind::luck, core::stat_kind::luck, core::stat_kind::luck,
                                                    core::stat_kind::hp };
                    core::stat_rule(l, rk[r], r >= 2 && r <= 4 ? r - 2 : 0);
                    k = r == 0 ? ink::brand : (r == 3 || r == 4 ? ink::dim : ink::dark);
                    if(r == 0) stripe(cv, r, phone_tile::stripe_brand);
                }
                else if(! g)   // wybór zawodu: wartość (baza + premie z profilu) i co daje
                {
                    const core::stat_kind kinds[6] = { core::stat_kind::hp, core::stat_kind::str, core::stat_kind::agi, core::stat_kind::intel,
                                                       core::stat_kind::def, core::stat_kind::luck };
                    int base = r == 0 ? c.max_health : (r <= 3 ? core::class_base_stat(cls, stats[r - 1]) : (r == 4 ? c.defense : c.luck));
                    int bonus = r == 0 ? m.hp : (r <= 3 ? core::mods_stat_bonus(m, cls, stats[r - 1]) : (r == 4 ? m.def : m.luck));
                    bool wstat = r >= 1 && r <= 3 && stats[r - 1] == w.scales_with;
                    l.add(core::stat_kind_name(kinds[r])).add(" ").add(base + bonus).add(": ");
                    core::stat_effect(l, kinds[r], base + bonus, wstat);
                    k = wstat ? ink::brand : (r >= 1 && r <= 3 ? ink::dim : ink::dark);
                    if(wstat) stripe(cv, r, phone_tile::stripe_brand);
                }
                else   // w trakcie budowy: skąd są premie
                {
                    const core::game& G = *g;
                    int ws = G.hero_stat(w.scales_with), wb = core::class_base_stat(cls, w.scales_with);
                    switch(r)
                    {
                        case 0:
                            l.add("Broń: ").add(stat_short(w.scales_with)).add(" ").add(ws).add(" = +").add(ws / 2).add(" obrażeń");
                            k = ink::brand; stripe(cv, r, phone_tile::stripe_brand);
                            break;
                        case 1:
                            l.add("zawód ").add(wb);
                            if(core::mods_stat_bonus(G.bonus, cls, w.scales_with)) l.add(", Warsztaty +").add(core::mods_stat_bonus(G.bonus, cls, w.scales_with));
                            if(G.trait_bonus(core::stat_trait(w.scales_with))) l.add(", sprzęt +").add(G.trait_bonus(core::stat_trait(w.scales_with)));
                            k = ink::dim;
                            break;
                        case 2:
                            l.add("Obrażenia +").add(G.dmg_bonus);
                            if(G.gear_bonus(core::gear_stat::dmg)) l.add(", rękawice +").add(G.gear_bonus(core::gear_stat::dmg));
                            if(G.bonus.dmg_pct) l.add(", +").add(G.bonus.dmg_pct).add("%");
                            break;
                        case 3:
                        {
                            int def = G.hero_defense();
                            l.add("OBR ").add(def).add(" = zawód ").add(c.defense);
                            if(G.def_bonus) l.add(", premie +").add(G.def_bonus);
                            if(G.gear_bonus(core::gear_stat::def)) l.add(", kask +").add(G.gear_bonus(core::gear_stat::def));
                            l.add(": -").add(def / 2);
                            if(G.bonus.taken_pct) l.add(", -").add(G.bonus.taken_pct).add("%");
                            break;
                        }
                        case 4:
                            l.add("SZCZ ").add(G.luck()).add(": kryt ").add(G.crit_pct()).add("%, unik ").add(G.dodge_pct()).add("%, łupy +")
                             .add(data::drop_per_luck_pct * G.luck()).add("%");
                            break;
                        default:
                        {
                            int lvl = data::hp_per_level * (G.hero_level - 1), gear = G.gear_bonus(core::gear_stat::hp);
                            l.add("HP ").add(G.hero.max_hp).add(" = zawód ").add(c.max_health);
                            if(G.bonus.hp) l.add(", Szkol. +").add(G.bonus.hp);
                            if(lvl) l.add(", poziomy +").add(lvl);
                            if(gear) l.add(", kamizelka +").add(gear);
                            int rest = G.hero.max_hp - c.max_health - G.bonus.hp - lvl - gear;
                            if(rest > 0) l.add(", inne +").add(rest);
                            break;
                        }
                    }
                }
                phone_text(a, t, list_x, row_py(r), fit(a, l.s, phone_text_w).c_str(), k);
            }
            ph.commit();
        };
        redraw();
        bn::sound_items::sfx_menu.play();
        wait_release();
        while(true)
        {
            int d = (bn::keypad::a_pressed() || bn::keypad::right_pressed() || bn::keypad::down_pressed()) ? 1
                  : ((bn::keypad::left_pressed() || bn::keypad::up_pressed()) ? -1 : 0);
            if(d) { page = (page + d + pages) % pages; redraw(); bn::sound_items::sfx_menu.play(); }
            if(bn::keypad::b_pressed() || bn::keypad::start_pressed() || bn::keypad::select_pressed()) break;
            next_frame();
        }
        t.clear();
        a.text.set_palette_item(default_ink);
        wait_release();
    }

    // Wiadomość w telefonie (fabuła): nadawca, dymek z 3 liniami, 2 wiersze informacji. A/START: dalej.
    void phone_message(app& a, const core::story_msg& m, const char* sub, const char* info1, ink info1_ink, const char* info2)
    {
        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        phone_screen ph(2);
        ph.icon.set_visible(false);
        bn::sprite_palette_item default_ink = a.text.palette_item();
        page_sprites t;
        phone_header(a, ph, t, "Wiadomości", sub);
        phone_canvas& c = *ph.canvas;
        phone_text(a, t, list_x, row_py(0), m.from, ink::dark);
        phone_pill(a, c, t, pill_end, row_ty(0), "teraz", pill::gray);
        c.rounded(2, row_ty(1), 26, 6, phone_tile::fill_group, phone_tile::corner_group);   // dymek wiadomości
        for(int i = 0; i < 3; ++i) phone_text(a, t, 22, row_py(1 + i), m.lines[i], ink::dark);
        phone_text(a, t, list_x, row_py(4), info1, info1_ink);
        phone_text(a, t, list_x, row_py(5), info2, ink::dim);
        phone_text(a, t, 226, row_py(5), "A: dalej", ink::brand, 1);
        ph.commit();
        bn::sound_items::sfx_notify.play(bn::fixed(0.7));
        wait_release();
        for(int i = 0; i < 600 && ! bn::keypad::a_pressed() && ! bn::keypad::start_pressed(); ++i) next_frame();
        t.clear();
        a.text.set_palette_item(default_ink);
    }

    // Wejście na etap: wiadomość od inwestorki / kierownika + siła problemów i trudność.
    void stage_card(app& a)
    {
        const core::game& g = *a.g;
        core::message sub; sub.add("Akt ").add(g.act_numeral()).add(", ").add(g.stage_number()).add("/").add(g.stages_in_run());
        bool boss = data::stages[g.stage].boss >= 0;
        core::message i1;
        if(boss) i1.add("Uwaga: ").add(clip(data::enemies[data::stages[g.stage].boss].name, 18).c_str()).add("!");
        else i1.add(clip(data::stages[g.stage].name, 12).c_str()).add(": problemy ").add(g.enemy_hp_pct()).add("%");
        auto card_line = [&](bool prefix) {   // pogoda dnia i mechanika aktu, wybrana ścieżka, NG+
            core::message m;
            if(prefix) m.add("Pogoda: ");
            m.add(g.wdef().name).add(", ").add(g.adef().mech_short);
            if(g.stage_path >= 0) m.add(", ").add(data::paths[g.stage_path].short_name);
            if(g.tier > 0) m.add(", NG+").add(g.tier);
            return m;
        };
        core::message i2 = card_line(true);
        if(a.text.width(i2.s) > 150) i2 = card_line(false);   // za długo (np. Pieczątki): bez "Pogoda:"
        phone_message(a, g.stage_story(), sub.s, i1.s, boss ? ink::late : ink::dim, fit(a, i2.s, 150).c_str());
        if(g.stage_event >= 0 && g.turns == g.stage_start_turn)   // wydarzenie na placu: drugi SMS (nie po wznowieniu w trakcie)
        {
            const core::site_event_def& ev = data::site_events[g.stage_event];
            leave(scene::game);
            phone_message(a, ev.msg, "Plac budowy", ev.info, ev.good ? ink::done : ink::late, ev.name);
        }
        leave(scene::game);   // ściemnij telefon, gra się rozjaśni
    }

    // Powiadomienie push jak z aplikacji PlanBudowlany: baner zjeżdża z góry ekranu.
    struct notice { bn::string<64> title; bn::string<64> body; };

    struct push_banner
    {
        static constexpr int slide = 8, hold = 80, height = 32;
        bn::unique_ptr<phone_canvas> canvas;
        bn::regular_bg_ptr bg;
        bn::sprite_ptr icon;
        bn::vector<bn::sprite_ptr, 24> text;
        bn::vector<bn::fixed, 24> text_y;
        bn::vector<notice, 4> queue;
        int timer = 0;   // 0 = brak banera

        push_banner() :
            canvas(new phone_canvas()),
            bg(make_canvas_bg(*canvas)),
            icon(bn::sprite_items::phone_icons.create_sprite(-96, -64, 5))
        {
            canvas->rounded(1, 0, 28, 4, phone_tile::fill_card, phone_tile::corner_card);
            bn::regular_bg_map_ptr map = bg.map();
            map.reload_cells_ref();
            bg.set_priority(0);
            bg.set_visible(false);
            icon.set_bg_priority(0);
            icon.set_visible(false);
        }

        bool active() const { return timer > 0; }

        void push(const char* title, const char* body)
        {
            if(queue.full()) queue.erase(queue.begin());
            queue.push_back({ bn::string<64>(clip(title, 23).c_str()), bn::string<64>(clip(body, 23).c_str()) });   // 23 znaki obok ikony
        }

        void start(app& a)
        {
            const notice& n = queue.front();
            text.clear(); text_y.clear();
            a.text.set_bg_priority(0);
            a.text.set_left_alignment();
            a.text.set_palette_item(bn::sprite_palette_items::font_dark);
            a.text.generate(36 - 120, 0 - 72, n.title, text);
            a.text.set_palette_item(bn::sprite_palette_items::font_dim);
            a.text.generate(36 - 120, 15 - 72, n.body, text);
            a.text.set_palette_item(bn::sprite_items::font_8x16.palette_item());
            for(bn::sprite_ptr& sp : text) text_y.push_back(sp.y());
            queue.erase(queue.begin());
            timer = slide * 2 + hold;
            bg.set_visible(true); icon.set_visible(true);
            bn::sound_items::sfx_notify.play(bn::fixed(0.7));
        }

        // Zwraca true, gdy baner jest na ekranie (HUD wtedy schowany).
        bool update(app& a)
        {
            if(timer == 0 && ! queue.empty()) start(a);
            if(timer == 0) return false;
            --timer;
            int t = slide * 2 + hold - timer;   // klatka od startu
            int off = t < slide ? height - t * height / slide : (timer < slide ? height - timer * height / slide : 0);
            bg.set_y(48 - off);
            icon.set_y(-64 - off);
            for(int i = 0; i < text.size(); ++i) text[i].set_y(text_y[i] - off);
            if(timer == 0) { bg.set_visible(false); icon.set_visible(false); text.clear(); }
            return true;
        }

        void hide() { timer = 0; queue.clear(); bg.set_visible(false); icon.set_visible(false); text.clear(); }

        bool busy() const { return timer > 0 || ! queue.empty(); }
    };

    // Banery: nowe odznaki (bitmaska z check_badges) i ukończone zlecenia (z check_contracts).
    void push_achievements(push_banner& banner, int badges_got, int contracts_got)
    {
        for(int i = 0; i < data::badges_count; ++i)
            if(badges_got & (1 << i))
            {
                core::message t; t.add("Odznaka: ").add(data::badges[i].name);
                core::message b; b.add("+").add(data::badges[i].xp).add(" dośw.");
                banner.push(t.s, b.s);
            }
        for(int i = 0; i < data::contracts_count; ++i)
            if(contracts_got & (1 << i))
            {
                const core::contract_def& c = data::contracts[i];
                core::message t; t.add("Zlecenie: ").add(c.name);
                core::message b;
                if(c.keepsake >= 0) b.add("+").add(c.xp).add(", ").add(data::keepsakes[c.keepsake].name);   // pamiątka odblokowana
                else b.add("Wykonane! +").add(c.xp).add(" dośw.");
                banner.push(t.s, b.s);
            }
    }

    // Cząsteczki (pył, iskry, konfetti, gwiazdki). Mała pula; bez wolnych sprite'ów efekt jest pomijany.
    struct particle
    {
        bn::sprite_ptr sp;
        bn::fixed x, y, vx, vy, gravity;
        int age, life, frame, frames;
    };

    struct particle_pool
    {
        enum : int { dust = 0, spark = 3, confetti = 5, star = 9, ring = 10, brick = 12, nail = 13, bolt = 14,
                     drop = 16, plus = 17, zzz = 18, alert = 19, marker = 20, status_icon = 21,
                     state_wet = 24, state_dusty = 25, state_frozen = 26,       // 21-24: stany bohatera (24 = mokry)
                     map_secret = 27, map_chest = 28 };                          // v0.21.50 cz. 3: podgląd mapy - magazyn, skrzynia
        bn::vector<particle, 24> list;
        bn::random rnd;
        bn::camera_ptr cam;

        explicit particle_pool(const bn::camera_ptr& c) : cam(c) {}

        bn::fixed rand(int lo16, int hi16) { return bn::fixed(rnd.get_int(lo16, hi16 + 1)) / 16; }   // w 1/16 px

        void spawn(bn::fixed x, bn::fixed y, bn::fixed vx, bn::fixed vy, bn::fixed gravity, int life, int frame, int frames = 1)
        {
            if(list.full()) return;
            bn::optional<bn::sprite_ptr> sp = bn::sprite_items::particles.create_sprite_optional(x, y, frame);
            if(! sp) return;
            sp->set_camera(cam);
            sp->set_z_order(-30);
            list.push_back({ bn::move(*sp), x, y, vx, vy, gravity, 0, life, frame, frames });
        }

        void burst(bn::fixed_point p, int count, int frame, int frames, int speed16, int life)
        {
            for(int i = 0; i < count; ++i)
                spawn(p.x(), p.y(), rand(-speed16, speed16), rand(-speed16, speed16 / 2), bn::fixed(0.08), life, frame, frames);
        }

        void update()
        {
            for(int i = 0; i < list.size(); )
            {
                particle& p = list[i];
                if(++p.age >= p.life) { list.erase(list.begin() + i); continue; }
                p.vy += p.gravity;
                p.x += p.vx; p.y += p.vy;
                p.sp.set_position(p.x, p.y);
                int f = p.frame + bn::min(p.frames - 1, p.age * p.frames / p.life);
                if(p.frames > 1) p.sp.set_tiles(bn::sprite_items::particles.tiles_item(), f);
                ++i;
            }
        }
    };

    // Unosząca się liczba obrażeń nad polem.
    struct floater
    {
        bn::vector<bn::sprite_ptr, 4> sprites;
        int timer = 0;
    };

    void boon_pick(app& a, bool mid_stage = false);   // premia 1 z 3 (niżej); mid_stage - z wydarzenia (#30)

    scene run_game(app& a)
    {
        core::game& g = *a.g;
        stage_card(a);
        play_song(song::game);
        save_run(a);   // autozapis na starcie etapu (albo po wznowieniu)
        bn::bg_palettes::set_transparent_color(bn::color(1, 1, 3));
        bn::camera_ptr cam = bn::camera_ptr::create(0, 0);

        bn::bg_tiles::set_allow_offset(false);
        bn::unique_ptr<bg_map> map(new bg_map());
        map->build(g);
        const bn::bg_palette_item* stage_pals[] = { &bn::bg_palette_items::stage_palettes_0, &bn::bg_palette_items::stage_palettes_1,
                                                    &bn::bg_palette_items::stage_palettes_2, &bn::bg_palette_items::stage_palettes_3,
                                                    &bn::bg_palette_items::stage_palettes_4, &bn::bg_palette_items::stage_palettes_5,
                                                    &bn::bg_palette_items::stage_palettes_6, &bn::bg_palette_items::stage_palettes_7,
                                                    &bn::bg_palette_items::stage_palettes_8, &bn::bg_palette_items::stage_palettes_9,
                                                    &bn::bg_palette_items::stage_palettes_10, &bn::bg_palette_items::stage_palettes_11 };
        static_assert(data::stages_count == int(sizeof(stage_pals) / sizeof(stage_pals[0])));
        bn::regular_bg_item item(bn::regular_bg_tiles_items::tiles, *stage_pals[g.stage], map->map_item);
        bn::regular_bg_ptr bg = item.create_bg(0, 0);
        bn::regular_bg_map_ptr bg_map_ptr = bg.map();
        bn::bg_tiles::set_allow_offset(true);
        bg.set_camera(cam);

        bn::sprite_ptr hero = bn::sprite_items::actors.create_sprite(0, 0, data::classes[g.cls].frame);
        hero.set_camera(cam);
        bn::vector<bn::sprite_ptr, core::max_enemies> enemies;
        bn::vector<bool, core::max_enemies> enemy_gold;   // sprite ma złotą paletę elity
        for(int i = 0; i < g.enemies_count; ++i)
        {
            bn::sprite_ptr s = bn::sprite_items::actors.create_sprite(0, 0, data::enemies[g.enemies[i].def_id].frame);
            s.set_camera(cam);
            style_enemy(s, g.enemies[i]);
            enemies.push_back(s);
            enemy_gold.push_back(g.enemies[i].elite >= 0);
        }
        bn::vector<bn::sprite_ptr, core::max_pickups> pickups;
        auto sync_pickups = [&]() {   // dropy pojawiają się w trakcie etapu
            for(int i = pickups.size(); i < g.pickups_count; ++i)
            {
                bn::sprite_ptr s = bn::sprite_items::actors.create_sprite(world(g.pickups[i].x, g.pickups[i].y), pickup_frame(g.pickups[i]));
                s.set_camera(cam);
                s.set_z_order(10);
                pickups.push_back(s);
            }
        };
        sync_pickups();
        bn::vector<bn::sprite_ptr, core::max_enemies> fx;
        // Pieczątki (Akt 0): kłódka na schodach, dopóki brakuje dokumentów.
        bn::sprite_ptr stairs_lock = bn::sprite_items::actors.create_sprite(world(core::imax(0, g.stairs_x), core::imax(0, g.stairs_y)), frame_lock);
        stairs_lock.set_camera(cam);
        stairs_lock.set_z_order(5);
        stairs_lock.set_visible(false);
        // Magazyn (#32): pęknięcie albo drzwi na polu muru, dopóki zamknięty i odkryty.
        bn::sprite_ptr secret_sprite = bn::sprite_items::actors.create_sprite(world(core::imax(0, g.secret_x), core::imax(0, g.secret_y)),
                                                                              g.secret_def().breakable ? frame_crack : frame_door);
        secret_sprite.set_camera(cam);
        secret_sprite.set_z_order(5);
        secret_sprite.set_visible(false);

        text_sprites hud, log;
        bn::sprite_ptr hp_left = bn::sprite_items::hp_bar.create_sprite(-82, -72, 0);
        bn::sprite_ptr hp_right = bn::sprite_items::hp_bar.create_sprite(-50, -72, 96);
        hp_left.set_bg_priority(0); hp_right.set_bg_priority(0);
        hp_left.set_z_order(-100); hp_right.set_z_order(-100);
        int blink = 0;
        // Ikona mocy (R) pod HUD: szara z odliczaniem, gdy się ładuje; pulsuje, gdy gotowa.
        bn::sprite_ptr power_icon = bn::sprite_items::ability_icons.create_sprite(106, -52, g.cls);
        power_icon.set_bg_priority(0);
        power_icon.set_z_order(-100);
        bn::sprite_palette_ptr power_palette = power_icon.palette();
        text_sprites power_text;
        int shown_cd = -1;
        // Aktywne stany w tym samym rzędzie (lewa strona): ikona + liczba tur do końca.
        bn::vector<bn::sprite_ptr, 4> status_icons;
        for(int k = 0; k < 4; ++k)
        {
            bn::sprite_ptr s = bn::sprite_items::particles.create_sprite(-112 + k * 22, -52, particle_pool::status_icon + k);
            s.set_bg_priority(0);
            s.set_z_order(-100);
            s.set_visible(false);
            status_icons.push_back(s);
        }
        text_sprites status_text;
        int shown_status = -1, status_count = 0;
        // Termos w HUD: ikona + liczba kaw (np. 2/3).
        bn::sprite_ptr thermos_icon = bn::sprite_items::menu_icons.create_sprite(50, -52, 1);
        thermos_icon.set_bg_priority(0);
        thermos_icon.set_z_order(-100);
        text_sprites thermos_text;
        int shown_thermos = -1;
        // Pogoda dnia w HUD: ikona między stanami a termosem (menu_icons 6-10).
        constexpr int frame_weather = 6;
        bn::sprite_ptr weather_icon = bn::sprite_items::menu_icons.create_sprite(28, -52, frame_weather + int(g.wdef().effect));
        weather_icon.set_bg_priority(0);
        weather_icon.set_z_order(-100);
        // Mechanika aktu w HUD pod ikoną mocy (menu_icons 20-22: błoto, porywy, pył); przy porywach tury do kolejnego.
        const int act_mech = int(g.adef().mechanic);
        bn::sprite_ptr act_icon = bn::sprite_items::menu_icons.create_sprite(106, -33, act_icon_frame(g.adef().mechanic));
        act_icon.set_bg_priority(0);
        act_icon.set_z_order(-100);
        act_icon.set_visible(act_mech > 0);
        text_sprites act_text;
        int shown_gust = -1;
        // Materiały w HUD (cement, stal, drewno): małe ikony z liczbą, widoczne, gdy coś masz (menu_icons 11-13).
        constexpr int frame_material = 11, mat_x0 = -46, mat_dx = 22;
        bn::vector<bn::sprite_ptr, 3> mat_icons;
        for(int m = 0; m < data::materials_count; ++m)
        {
            bn::sprite_ptr sp = bn::sprite_items::menu_icons.create_sprite(mat_x0 + m * mat_dx, -52, frame_material + m);
            sp.set_bg_priority(0);
            sp.set_z_order(-100);
            sp.set_visible(false);
            mat_icons.push_back(sp);
        }
        text_sprites mat_text;
        int shown_mats = -1;
        auto hide_status_hud = [&]() {
            for(auto& s : mat_icons) s.set_visible(false);
            mat_text.clear(); shown_mats = -1;
            for(auto& s : status_icons) s.set_visible(false);
            status_text.clear(); shown_status = -1;
            thermos_icon.set_visible(false); thermos_text.clear(); shown_thermos = -1;
            weather_icon.set_visible(false);
            act_icon.set_visible(false); act_text.clear(); shown_gust = -1;
        };
        a.text.set_bg_priority(0);
        a.text.set_z_order(-100);
        int fx_timer = 0, hurt_timer = 0, hold = 0;
        bn::vector<floater, core::max_hits> floaters;
        int shake_timer = 0, target_timer = 0;
        bn::fixed_point cam_base;
        push_banner banner;
        particle_pool fx_particles(cam);

        // Półprzezroczyste ciemne paski pod HUD (u góry) i dziennikiem (u dołu, tylko gdy są komunikaty).
        // GBA ma 4 warstwy tła: mapa, baner, paski - telefon (2 warstwy) wymaga chwilowego zwolnienia pasków.
        bn::unique_ptr<phone_canvas> strips(new phone_canvas());
        bn::optional<bn::regular_bg_ptr> strip_bg;
        bn::optional<bn::regular_bg_map_ptr> strip_map;
        bool strips_bottom = false;
        auto create_strips = [&]() {
            strip_bg = make_canvas_bg(*strips);
            strip_map = strip_bg->map();
            strip_bg->set_priority(1);
            strip_bg->set_blending_enabled(true);
            bn::blending::set_transparency_alpha(bn::fixed(0.55));
        };
        auto release_strips = [&]() { strip_map.reset(); strip_bg.reset(); };
        auto draw_strips = [&](bool bottom) {
            strips_bottom = bottom;
            strips->clear();
            for(int x = 0; x < 30; ++x) { strips->set(x, 0, phone_tile::fill_dark); strips->set(x, 1, phone_tile::fill_dark); }
            if(bottom) for(int y = 16; y < 20; ++y) for(int x = 0; x < 30; ++x) strips->set(x, y, phone_tile::fill_dark);
            if(strip_map) strip_map->reload_cells_ref();
        };
        create_strips();
        draw_strips(false);
        int log_timer = 0, log_seen = g.log_serial;

        // Celowanie: przytrzymanie A pokazuje zasięg i celownik, strzałki zmieniają cel, puszczenie atakuje.
        bool aiming = false;
        int aim_frames = 0, aim_sel = 0, aim_count = 0, range_flash = 0;
        int8_t aim_targets[core::max_enemies];
        // Znacznik oznaczonego celu: strzałka nad wrogiem, którego trafi krótkie A (albo wybranym przy celowaniu).
        bn::sprite_ptr target_marker = bn::sprite_items::particles.create_sprite(0, 0, particle_pool::marker);
        target_marker.set_camera(cam);
        target_marker.set_z_order(-45);
        target_marker.set_visible(false);
        int marked = -1;
        // Ikona aktywnego stanu nad bohaterem (zatrucie / porażenie / poślizg).
        bn::sprite_ptr status_sprite = bn::sprite_items::particles.create_sprite(0, 0, particle_pool::status_icon);
        status_sprite.set_camera(cam);
        status_sprite.set_z_order(-45);
        status_sprite.set_visible(false);
        bool looking = false;
        int look_frames = 0, look_sel = 0, look_count = 0, look_shown = -1;
        int8_t look_list[core::max_enemies];
        static bool range_cells[core::map_h][core::map_w];
        bn::sprite_ptr reticle = bn::sprite_items::actors.create_sprite(0, 0, frame_reticle);
        reticle.set_camera(cam);
        reticle.set_z_order(-40);
        reticle.set_visible(false);
        auto show_range = [&](bool on) {
            if(on)
            {
                for(int y = 0; y < core::map_h; ++y)
                    for(int x = 0; x < core::map_w; ++x)
                        range_cells[y][x] = g.visible(x, y) && core::cheb(g.hero.x, g.hero.y, x, y) <= g.weapon_range()
                                            && ! (x == g.hero.x && y == g.hero.y);
                map->highlight = range_cells;
            }
            else map->highlight = nullptr;
            map->build(g);
            bg_map_ptr.reload_cells_ref();
        };
        int prev_level = g.hero_level, prev_weapon = g.weapon_override, prev_pickups = g.pickups_count;
        int prev_cd = g.ability_cd;
        int prev_active = 0;
        int8_t prev_equipped[core::max_gear_slots];
        for(int i = 0; i < core::max_gear_slots; ++i) prev_equipped[i] = g.equipped[i];
        for(int i = 0; i < g.pickups_count; ++i) prev_active += g.pickups[i].active;
        bool boss_seen = false;
        bool boss_engaged = g.boss_wake_damage >= 0;   // Inspekcja: baner "zgodnie z BHP" przy pełnym sprzęcie
        if(g.stage == g.first_stage && g.tier == 0 && g.turns == 0)   // podpowiedź na start budowy: moc pod R, zabrana pamiątka
        {
            core::message t; t.add("R: ").add(g.cdef().ability_name);
            banner.push(t.s, g.cdef().ability_desc);
            int k = core::selected_keepsake(a.save);
            if(k >= 0)
            {
                // ranga liczona z budów przed tą (start_run już policzył bieżącą)
                int runs = a.save.keepsake_runs[k] - 1;
                int rank = 1 + (runs >= data::keepsake_rank_runs[0]) + (runs >= data::keepsake_rank_runs[1]);
                core::message kt; kt.add(data::keepsakes[k].name).add(" ").add(roman(rank - 1));   // pamiątka z rangą
                core::message kb; core::perk_label(kb, { data::keepsakes[k].effect, data::keepsakes[k].values[rank - 1] });
                banner.push(kt.s, kb.s);
            }
        }
        if(g.turns == g.stage_start_turn && (g.stage == g.first_stage || data::stages[g.stage - 1].act != data::stages[g.stage].act)
           && g.adef().mechanic != core::act_mechanic::none)   // nowy akt: jego mechanika (błoto, porywy, pył, pieczątki)
        {
            core::message t; t.add("Akt ").add(g.act_numeral()).add(": ").add(g.adef().mech_short);
            banner.push(t.s, g.adef().mech_info);
        }
        bool second_seen = g.second_used;   // Druga szansa: baner raz
        int prev_keys = g.keys, prev_wlvl = g.weapon_lvl;   // v0.21.50 cz. 3: klucz, ulepszenie narzędzia
        bool prev_secret = g.secret_open;
        int prev_chests = 0;
        for(int i = 0; i < g.pickups_count; ++i) prev_chests += g.pickups[i].active && g.pickups[i].type == core::chest;
        int prev_docs = g.docs;             // pieczątki: nowy dokument = baner
        bool phase_seen = g.boss >= 0 && (g.enemies[g.boss].flags & core::actor_phase);   // druga faza bossa: baner raz
        int flash_timer = 0;
        bn::color flash_color;
        int levelup_pending = 0;   // awans w tej turze: napis nad bohaterem i błysk (obsługa w pętli, gdzie są efekty)
        text_sprites levelup_text;
        int levelup_timer = 0;
        auto detect_events = [&]() {   // powiadomienia push o ważnych zdarzeniach
            int active_pickups = 0;
            for(int i = 0; i < g.pickups_count; ++i) active_pickups += g.pickups[i].active;
            if(active_pickups < prev_active) bn::sound_items::sfx_pickup.play();   // zebrana znajdźka
            prev_active = active_pickups;
            if(g.hero_level > prev_level)
            {
                bn::sound_items::sfx_level.play();
                int old_rank = 1 + (prev_level >= 3) + (prev_level >= 5);
                for(int k = 0; k < 8; ++k)   // gwiazdki awansu dookoła bohatera
                {
                    static constexpr int8_t dir[8][2] = { { 2, 0 }, { 1, 1 }, { 0, 2 }, { -1, 1 }, { -2, 0 }, { -1, -1 }, { 0, -2 }, { 1, -1 } };
                    fx_particles.spawn(world(g.hero.x, g.hero.y).x(), world(g.hero.x, g.hero.y).y(), bn::fixed(dir[k][0]) / 2, bn::fixed(dir[k][1]) / 2, 0, 28, particle_pool::star);
                }
                core::message t; t.add("Awans! Poziom ").add(g.hero_level);
                core::message b; b.add("+").add(data::hp_per_level).add(" max HP");
                if(data::def_levels_mask & (1 << g.hero_level)) b.add(", +1 obrona");
                if(data::dmg_levels_mask & (1 << g.hero_level)) b.add(", +1 obrażenia");
                if(g.ability_rank() > old_rank) b.add(", moc ").add(roman(g.ability_rank() - 1));
                banner.push(t.s, b.s);
                levelup_pending = g.hero_level;
            }
            if(g.weapon_override != prev_weapon && g.weapon_override >= 0)   // porównanie ciosu (rozpiska #26)
            {
                const core::dmg_breakdown was = g.weapon_breakdown(-1, prev_weapon >= 0 ? prev_weapon : g.cdef().weapon), now = g.weapon_breakdown();
                core::message t; t.add("Nowe: ").add(g.weapon().name);
                core::message b; core::add_range(b, was.min, was.max).add(" -> ");
                core::add_range(b, now.min, now.max).add(" (śr. ");
                core::add_tenths(b, now.avg10 - was.avg10, true).add(")");
                banner.push(t.s, b.s);
            }
            if(g.pickups_count > prev_pickups) banner.push("Coś wypadło!", "Sprawdź miejsce usterki");
            for(int i = 0; i < data::gear_slots_count; ++i)
                if(g.equipped[i] != prev_equipped[i] && g.equipped[i] >= 0)
                {
                    const core::gear_def& gd = data::gear[i * 3 + g.equipped[i]];
                    core::message t; t.add("Sprzęt: ").add(data::gear_rarities[g.equipped[i]]);
                    core::message b; b.add(gd.name).add(" +").add(gd.value);
                    banner.push(t.s, b.s);
                    prev_equipped[i] = g.equipped[i];
                }
            if(prev_cd > 0 && g.ability_cd == 0) banner.push("Moc gotowa", g.cdef().ability_name);
            if(! boss_seen && g.boss >= 0 && g.enemies[g.boss].alive && g.visible(g.enemies[g.boss].x, g.enemies[g.boss].y))
            {
                boss_seen = true;
                banner.push("Przypisano Ci usterkę", data::enemies[g.enemies[g.boss].def_id].name);
            }
            if(! boss_engaged && g.boss_wake_damage >= 0)
            {
                boss_engaged = true;
                const core::enemy_def& bd = data::enemies[g.enemies[g.boss].def_id];
                if(bd.gear_stun > 0 && g.full_gear()) banner.push("Wszystko zgodnie z BHP!", "Kontrola wstrzymana");
            }
            if(g.st == core::status::stage_clear)   // ważniejsze niż kolejka: od razu, zanim zmieni się scena
            {
                bn::sound_items::sfx_stage.play();
                banner.hide();
                banner.push("Etap zaliczony", clip(data::stages[g.stage].name, 24).c_str());
                if(g.boss >= 0 && ! g.enemies[g.boss].alive && data::enemies[g.enemies[g.boss].def_id].reward_cash > 0)
                {
                    const core::enemy_def& bd = data::enemies[g.enemies[g.boss].def_id];   // nagroda bossa, np. Protokół bez uwag
                    core::message b; b.add("Premia +").add(bd.reward_cash).add(" zł");
                    banner.push(bd.reward_title, b.s);
                }
            }
            if(g.st == core::status::stage_clear || g.st == core::status::won)   // odznaki i zlecenia
            {
                int got = core::check_badges(a.save, g);   // też Respekt za etap - od razu w profilu
                int done = core::check_contracts(a.save);
                core::message rt; rt.add("Respekt +").add(g.stage_respect());
                core::message rb; rb.add("Razem: ").add(int(a.save.respect)).add(" (Koszty)");
                banner.push(rt.s, rb.s);
                push_achievements(banner, got, done);
                bn::sram::write(a.save);   // liczniki zleceń przeniesione do profilu
            }
            if(g.docs != prev_docs)   // Akt 0: dokument zebrany, komplet otwiera schody
            {
                for(int i = 0; i < data::documents_count; ++i)
                    if(((g.docs >> i) & 1) && ! ((prev_docs >> i) & 1))
                    {
                        core::message t; t.add("Dokument: ").add(data::documents[i]);
                        core::message b; b.add("Pieczątki ").add(g.docs_count()).add("/").add(g.docs_needed());
                        if(! g.stairs_locked()) b = core::message(), b.add("Komplet! Schody otwarte");
                        banner.push(t.s, b.s);
                    }
                prev_docs = g.docs;
            }
            if(! phase_seen && g.boss >= 0 && (g.enemies[g.boss].flags & core::actor_phase) && g.enemies[g.boss].alive)   // druga faza
            {
                phase_seen = true;
                const core::enemy_def& bd = data::enemies[g.enemies[g.boss].def_id];
                core::message t; t.add("Druga faza: ").add(bd.phase_name).add("!");
                core::message b; b.add(bd.name).add(" +").add(g.enemies[g.boss].max_hp * bd.phase_heal / 100).add(" HP");
                banner.push(t.s, b.s);
                flash_color = bn::color(31, 8, 6);
                flash_timer = 10;
                shake_timer = 8;
            }
            if(g.keys > prev_keys) banner.push("Klucz do magazynu!", g.has_secret() ? g.secret_def().name : "Szukaj magazynu");
            prev_keys = g.keys;
            if(g.secret_open && ! prev_secret)   // magazyn otwarty: pył i baner
            {
                banner.push("Magazyn otwarty!", "W środku skrzynia");
                bn::fixed_point w = world(g.secret_x, g.secret_y);
                for(int k = 0; k < 6; ++k) fx_particles.spawn(w.x(), w.y(), fx_particles.rand(-20, 20), fx_particles.rand(-20, 4), bn::fixed(0.06), 22, particle_pool::dust, 3);
                flash_color = bn::color(31, 24, 10); flash_timer = 8; shake_timer = 4;
                bn::sound_items::sfx_ability.play();
            }
            prev_secret = g.secret_open;
            {
                int chests = 0;
                for(int i = 0; i < g.pickups_count; ++i) chests += g.pickups[i].active && g.pickups[i].type == core::chest;
                if(chests < prev_chests)
                {
                    core::message b; b.add("Respekt +").add(data::chest_respect).add(", +").add(g.income(data::chest_cash)).add(" zł");
                    banner.push("Skrzynia!", b.s);
                    bn::fixed_point h = world(g.hero.x, g.hero.y);
                    for(int k = 0; k < 6; ++k) fx_particles.spawn(h.x() + fx_particles.rand(-96, 96), h.y(), 0, fx_particles.rand(-24, -8), 0, 40, particle_pool::star);
                }
                prev_chests = chests;
            }
            if(g.weapon_lvl > prev_wlvl)
            {
                core::message t; t.add("Ulepszenie: "); g.weapon_title(t);
                core::message b; b.add("+").add(g.upgrade_dmg()).add(" obrażeń");
                if(g.weapon_trait >= 0) b.add(", ").add(data::tool_traits[g.weapon_trait].name);
                banner.push(t.s, b.s);
            }
            prev_wlvl = g.weapon_lvl;
            if(g.second_used && ! second_seen)   // Druga szansa z Respektu
            {
                second_seen = true;
                banner.push("Druga szansa!", "Zostaje 1 HP - uważaj");
                flash_color = bn::color(31, 27, 10);
                flash_timer = 10;
            }
            prev_level = g.hero_level; prev_weapon = g.weapon_override; prev_pickups = g.pickups_count; prev_cd = g.ability_cd;
        };

        // Mini paski HP nad widocznymi wrogami, którzy Cię ścigają albo są ranni (1 sprite 8x8 na wroga).
        bn::vector<bn::optional<bn::sprite_ptr>, core::max_enemies> mini_bars(g.enemies_count);
        auto update_mini_bars = [&]() {
            for(int i = 0; i < g.enemies_count; ++i)
            {
                const core::actor& e = g.enemies[i];
                bool show = e.alive && g.visible(e.x, e.y) && (e.awake || e.hp < e.max_hp);
                if(! show) { if(mini_bars[i]) mini_bars[i]->set_visible(false); continue; }
                int fill = core::imax(1, e.hp * 6 / e.max_hp);
                int color = e.hp * 2 > e.max_hp ? 0 : (e.hp * 4 > e.max_hp ? 1 : 2);
                if(! mini_bars[i])
                {
                    mini_bars[i] = bn::sprite_items::mini_hp.create_sprite_optional(0, 0, color * 7 + fill);
                    if(! mini_bars[i]) continue;   // brak wolnych sprite'ów - bez paska
                    mini_bars[i]->set_camera(cam);
                    mini_bars[i]->set_z_order(-20);
                }
                mini_bars[i]->set_tiles(bn::sprite_items::mini_hp.tiles_item(), color * 7 + fill);
                mini_bars[i]->set_visible(true);
            }
        };
        // Stany problemów dla kombinacji (#29): mała ikona obok (mokry, zapylony, zmrożony - na zmianę), elita: złota paleta.
        bn::vector<bn::optional<bn::sprite_ptr>, core::max_enemies> state_icons(g.enemies_count);
        auto state_frame = [&](int i, int clock) {
            int st[3], n = 0;
            if(g.enemy_wet(i)) st[n++] = particle_pool::state_wet;
            if(g.enemy_dusty(i)) st[n++] = particle_pool::state_dusty;
            if(g.enemy_frozen(i)) st[n++] = particle_pool::state_frozen;
            return n ? st[(clock / 40) % n] : -1;
        };
        auto update_state_icons = [&]() {
            for(int i = 0; i < g.enemies_count; ++i)
            {
                const core::actor& e = g.enemies[i];
                if(i < enemies.size() && enemy_gold[i] != (e.elite >= 0))
                {
                    style_enemy(enemies[i], e);
                    enemy_gold[i] = e.elite >= 0;
                }
                int f = e.alive && g.visible(e.x, e.y) ? state_frame(i, 0) : -1;
                if(f < 0) { if(state_icons[i]) state_icons[i]->set_visible(false); continue; }
                if(! state_icons[i])
                {
                    state_icons[i] = bn::sprite_items::particles.create_sprite_optional(0, 0, f);
                    if(! state_icons[i]) continue;   // brak wolnych sprite'ów - bez ikony
                    state_icons[i]->set_camera(cam);
                    state_icons[i]->set_z_order(-20);
                }
                state_icons[i]->set_visible(true);
            }
        };
        auto hide_mini_bars = [&]() {
            for(auto& mb : mini_bars) if(mb) mb->set_visible(false);
            for(auto& si : state_icons) if(si) si->set_visible(false);
        };
        uint32_t prev_awake = 0;

        // Animacja: płynny ruch między polami (4 px/klatkę) i 2 klatki "oddechu"/kroku.
        bn::fixed_point hero_cur = world(g.hero.x, g.hero.y), hero_dst = hero_cur;
        bn::vector<bn::fixed_point, core::max_enemies> enemy_cur, enemy_dst;
        for(int i = 0; i < g.enemies_count; ++i) { enemy_cur.push_back(world(g.enemies[i].x, g.enemies[i].y)); enemy_dst.push_back(enemy_cur.back()); }
        int anim_clock = 0, hero_shown = -1;
        // Pomocnik z brygady: sprite fachowca obok bohatera (tylko, gdy pomaga).
        bn::optional<bn::sprite_ptr> ally_sprite;
        bn::fixed_point ally_cur, ally_dst;
        bool ally_hidden = false;
        bn::vector<int8_t, core::max_enemies> enemy_shown(g.enemies_count, -1);
        bool snap_next = true;
        auto approach = [](bn::fixed_point& c, const bn::fixed_point& d) {
            bn::fixed dx = d.x() - c.x(), dy = d.y() - c.y();
            c.set_x(c.x() + (dx > 4 ? bn::fixed(4) : (dx < -4 ? bn::fixed(-4) : dx)));
            c.set_y(c.y() + (dy > 4 ? bn::fixed(4) : (dy < -4 ? bn::fixed(-4) : dy)));
        };
        bn::bg_palette_ptr map_palette = bg.palette();
        bn::sprite_palette_ptr actors_palette = hero.palette();
        auto palette_fx = [&]() {
            if(anim_clock % 3 == 0)   // poświata schodów (kolory 6-7 w paletach światła 0-2)
            {
                int p = pulse(anim_clock / 3, 12);
                static constexpr int light[3] = { 100, 80, 60 };
                for(int l = 0; l < 3; ++l)
                {
                    int f = light[l];
                    map_palette.set_color(l * 16 + 6, mix(245 * f / 100, 211 * f / 100, 61 * f / 100, 255, 250, 200 * f / 100, p, 16));
                    map_palette.set_color(l * 16 + 7, mix(180 * f / 100, 120 * f / 100, 20 * f / 100, 255, 190, 60, p, 16));
                }
            }
            if(anim_clock % 8 == 0)   // mieniąca się woda (Przeciek): kolor cyjan palety postaci
                actors_palette.set_color(14, mix(90, 208, 230, 190, 245, 255, pulse(anim_clock / 8, 4), 4));
            bool alarm = g.hero.hp * 4 <= g.hero.max_hp && g.hero.hp > 0;   // niskie HP: pulsująca czerwień
            map_palette.set_fade(bn::color(31, 2, 2), alarm ? bn::fixed(pulse(anim_clock, 30)) / 120 : bn::fixed(0));
        };

        auto animate = [&]() {
            ++anim_clock;
            palette_fx();
            bool hero_moving = hero_cur != hero_dst;
            approach(hero_cur, hero_dst);
            hero.set_position(hero_cur);
            bool phase = (anim_clock / 24) & 1;
            {
                int active[4], n = 0;
                for(int k = 0; k < 4; ++k) if(g.status_turns(hud_statuses[k]) > 0) active[n++] = k;
                status_sprite.set_visible(n > 0);
                if(n > 0)
                {
                    status_sprite.set_tiles(bn::sprite_items::particles.tiles_item(), particle_pool::status_icon + active[(anim_clock / 40) % n]);
                    status_sprite.set_position(hero_cur.x() + 7, hero_cur.y() - 12);
                }
            }
            int hf = (phase || hero_moving) ? anim_b(data::classes[g.cls].frame) : data::classes[g.cls].frame;
            if(hf != hero_shown) { hero.set_tiles(bn::sprite_items::actors.tiles_item(), hf); hero_shown = hf; }
            if(g.act_is(core::act_mechanic::dust) && anim_clock % 18 == 0 && ! ally_hidden)   // akt III: pył wisi w powietrzu
                fx_particles.spawn(hero_cur.x() + fx_particles.rand(-1600, 1600), hero_cur.y() + fx_particles.rand(-1100, 1100),
                                   fx_particles.rand(-4, 4), fx_particles.rand(-3, 1), 0, 60, particle_pool::dust + 1, 2);
            for(int i = 0; i < g.enemies_count && i < enemies.size(); ++i)
            {
                approach(enemy_cur[i], enemy_dst[i]);
                enemies[i].set_position(enemy_cur[i]);
                if(mini_bars[i]) mini_bars[i]->set_position(enemy_cur[i].x(), enemy_cur[i].y() - 11);
                if(state_icons[i] && state_icons[i]->visible())
                {
                    state_icons[i]->set_position(enemy_cur[i].x() + 7, enemy_cur[i].y() - 6);
                    int f = state_frame(i, anim_clock + i * 13);
                    if(f >= 0) state_icons[i]->set_tiles(bn::sprite_items::particles.tiles_item(), f);
                }
                if(i == marked)
                    target_marker.set_position(enemy_cur[i].x(), enemy_cur[i].y() - 19 - (((anim_clock / 10) & 1) ? 1 : 0));
                int base = data::enemies[g.enemies[i].def_id].frame;
                int ef = ((anim_clock / 20 + i) & 1) ? anim_b(base) : base;
                if(ef != enemy_shown[i]) { enemies[i].set_tiles(bn::sprite_items::actors.tiles_item(), ef); enemy_shown[i] = int8_t(ef); }
            }
            if(g.ally_turns > 0 && ! ally_hidden)   // pomocnik z brygady
            {
                int base = data::brigade[g.helper_called].frame;
                if(! ally_sprite)
                {
                    ally_sprite = bn::sprite_items::actors.create_sprite_optional(ally_dst, base);
                    if(ally_sprite) { ally_sprite->set_camera(cam); ally_cur = ally_dst; }
                }
                if(ally_sprite)
                {
                    bool moving = ally_cur != ally_dst;
                    approach(ally_cur, ally_dst);
                    ally_sprite->set_position(ally_cur);
                    ally_sprite->set_tiles(bn::sprite_items::actors.tiles_item(), (phase || moving) ? anim_b(base) : base);
                }
            }
            else if(ally_sprite) ally_sprite.reset();
            for(int i = 0; i < pickups.size(); ++i)   // znajdźki lekko podskakują
                pickups[i].set_y(world(g.pickups[i].x, g.pickups[i].y).y() - (((anim_clock / 16 + i) & 1) ? 1 : 0));
            cam_base = bn::fixed_point(clampf(hero_cur.x(), 136), clampf(hero_cur.y(), 176));
            if(shake_timer == 0) cam.set_position(cam_base);
        };

        // Efekty cząsteczkowe mocy zawodów + błysk ekranu w kolorze mocy.
        auto ability_fx = [&]() {
            bn::fixed_point h = world(g.hero.x, g.hero.y);
            static constexpr int8_t dir[8][2] = { { 2, 0 }, { 1, 1 }, { 0, 2 }, { -1, 1 }, { -2, 0 }, { -1, -1 }, { 0, -2 }, { 1, -1 } };
            switch(g.cdef().ability)
            {
                case core::ability_effect::stun:   // kręgi megafonu + "z" nad ogłuszonymi
                    flash_color = bn::color(20, 16, 31);
                    for(int k = 0; k < 8; ++k)
                        fx_particles.spawn(h.x(), h.y(), bn::fixed(dir[k][0]) * bn::fixed(0.9), bn::fixed(dir[k][1]) * bn::fixed(0.9), 0, 22,
                                           particle_pool::ring, 2);
                    for(int i = 0; i < g.enemies_count; ++i)
                        if(g.enemies[i].alive && g.enemies[i].stun > 0 && g.visible(g.enemies[i].x, g.enemies[i].y))
                        {
                            bn::fixed_point e = world(g.enemies[i].x, g.enemies[i].y);
                            fx_particles.spawn(e.x() + 4, e.y() - 8, bn::fixed(0.2), bn::fixed(-0.4), 0, 40, particle_pool::zzz);
                        }
                    break;
                case core::ability_effect::wall:   // cegły wyskakują z ziemi + pył zaprawy
                    flash_color = bn::color(31, 16, 6);
                    for(int i = 0; i < g.walls_count; ++i)
                    {
                        bn::fixed_point w = world(g.walls[i].x, g.walls[i].y);
                        fx_particles.spawn(w.x(), w.y() + 4, fx_particles.rand(-8, 8), bn::fixed(-1.6), bn::fixed(0.15), 20, particle_pool::brick);
                        fx_particles.spawn(w.x() - 4, w.y() + 6, bn::fixed(-0.3), bn::fixed(-0.2), 0, 16, particle_pool::dust, 3);
                        fx_particles.spawn(w.x() + 4, w.y() + 6, bn::fixed(0.3), bn::fixed(-0.2), 0, 16, particle_pool::dust, 3);
                    }
                    break;
                case core::ability_effect::volley:   // gwoździe lecą do celów
                    flash_color = bn::color(31, 28, 8);
                    for(int i = 0; i < g.hits_count; ++i)
                    {
                        bn::fixed_point t = world(g.hits[i].x, g.hits[i].y);
                        fx_particles.spawn(h.x(), h.y(), (t.x() - h.x()) / 10, (t.y() - h.y()) / 10, 0, 10, particle_pool::nail);
                    }
                    break;
                case core::ability_effect::chain:   // łuk elektryczny: bohater -> cel 1 -> cel 2 -> cel 3
                {
                    flash_color = bn::color(12, 28, 31);
                    bn::fixed_point from = h;
                    for(int i = 0; i < g.hits_count; ++i)
                    {
                        if(g.hits[i].on_hero) continue;
                        bn::fixed_point to = world(g.hits[i].x, g.hits[i].y);
                        for(int k = 1; k <= 3; ++k)
                            fx_particles.spawn(from.x() + (to.x() - from.x()) * k / 4, from.y() + (to.y() - from.y()) * k / 4,
                                               0, 0, 0, 12, particle_pool::bolt, 2);
                        from = to;
                    }
                    break;
                }
                case core::ability_effect::flush:   // strumień: krople na boki i zielone plusy
                    flash_color = bn::color(8, 30, 16);
                    for(int k = 0; k < 6; ++k)
                        fx_particles.spawn(h.x() + fx_particles.rand(-112, 112), h.y() + 4, 0, fx_particles.rand(-24, -8), 0, 26,
                                           k & 1 ? particle_pool::plus : particle_pool::drop);
                    break;
                case core::ability_effect::spin:   // wirujący krąg iskier
                    flash_color = bn::color(31, 31, 31);
                    for(int k = 0; k < 8; ++k)
                        fx_particles.spawn(h.x() + dir[k][0] * 6, h.y() + dir[k][1] * 6,
                                           bn::fixed(-dir[k][1]) * bn::fixed(0.8), bn::fixed(dir[k][0]) * bn::fixed(0.8), 0, 16,
                                           particle_pool::spark, 2);
                    break;
                case core::ability_effect::line:   // Rynna: dachówki lecą linią do trafionych
                    flash_color = bn::color(31, 14, 8);
                    for(int i = 0; i < g.hits_count; ++i)
                    {
                        if(g.hits[i].on_hero) continue;
                        bn::fixed_point t = world(g.hits[i].x, g.hits[i].y);
                        for(int k = 0; k < 2; ++k)
                            fx_particles.spawn(h.x(), h.y() - 4 * k, (t.x() - h.x()) / 12, (t.y() - h.y()) / 12, 0, 12, particle_pool::brick);
                    }
                    break;
                case core::ability_effect::splash:   // Narzut: tynk chlapie na obszar wokół celu
                    flash_color = bn::color(30, 30, 28);
                    for(int i = 0; i < g.hits_count; ++i)
                    {
                        if(g.hits[i].on_hero) continue;
                        bn::fixed_point t = world(g.hits[i].x, g.hits[i].y);
                        for(int k = 0; k < 4; ++k)
                            fx_particles.spawn(t.x(), t.y(), bn::fixed(dir[k * 2][0]) * bn::fixed(0.6), bn::fixed(dir[k * 2][1]) * bn::fixed(0.6),
                                               0, 18, particle_pool::dust, 3);
                    }
                    break;
                case core::ability_effect::ram:   // Taran: kurz spod gąsienic i gwiazdki przy uderzeniu
                    flash_color = bn::color(31, 24, 4);
                    for(int k = 0; k < 6; ++k)
                        fx_particles.spawn(h.x() + fx_particles.rand(-96, 96), h.y() + 6, fx_particles.rand(-8, 8), bn::fixed(-0.3), 0, 20, particle_pool::dust, 3);
                    for(int i = 0; i < g.hits_count; ++i)
                        if(! g.hits[i].on_hero)
                        {
                            bn::fixed_point t = world(g.hits[i].x, g.hits[i].y);
                            for(int k = 0; k < 3; ++k) fx_particles.spawn(t.x(), t.y() - 8, fx_particles.rand(-16, 16), bn::fixed(-0.6), bn::fixed(0.05), 24, particle_pool::star);
                        }
                    break;
                default: break;
            }
            flash_timer = 6;
        };

        // Brygada: baner i efekty wezwania fachowca.
        auto brigade_fx = [&](int h) {
            const core::helper_def& hd = data::brigade[h];
            banner.push(hd.name, hd.desc);   // fachowiec na placu (nazwa i skutek, 23 znaki)
            bn::sound_items::sfx_ability.play();
            bn::fixed_point p = world(g.hero.x, g.hero.y);
            static constexpr int8_t dir[8][2] = { { 2, 0 }, { 1, 1 }, { 0, 2 }, { -1, 1 }, { -2, 0 }, { -1, -1 }, { 0, -2 }, { 1, -1 } };
            switch(hd.effect)
            {
                case core::helper_effect::reveal: flash_color = bn::color(16, 24, 31); break;
                case core::helper_effect::pump:   // beton rozlewa się wokół bohatera
                    flash_color = bn::color(24, 24, 22);
                    for(int k = 0; k < 8; ++k)
                        fx_particles.spawn(p.x(), p.y() + 4, bn::fixed(dir[k][0]) * bn::fixed(0.8), bn::fixed(dir[k][1]) * bn::fixed(0.8), 0, 24,
                                           particle_pool::dust, 3);
                    break;
                case core::helper_effect::safety:
                    flash_color = bn::color(8, 30, 16);
                    for(int k = 0; k < 6; ++k)
                        fx_particles.spawn(p.x() + fx_particles.rand(-128, 128), p.y() + 4, 0, fx_particles.rand(-20, -8), 0, 26, particle_pool::plus);
                    break;
                default:
                    flash_color = bn::color(12, 28, 31);
                    if(g.ally_turns > 0)
                    {
                        bn::fixed_point q = world(g.ally_x, g.ally_y);
                        for(int k = -1; k <= 1; k += 2) fx_particles.spawn(q.x() + k * 4, q.y() + 6, bn::fixed(k) / 3, bn::fixed(-0.3), 0, 16, particle_pool::dust, 3);
                    }
                    break;
            }
            flash_timer = 6;
        };

        // Wybrane w telefonie: fachowiec z brygady albo naprawa za materiał (h >= brigade_count). Zużywa turę.
        auto do_pending = [&](int h) {
            if(h < data::brigade_count)
            {
                if(! g.call_helper(h)) return false;
                brigade_fx(h);
                return true;
            }
            int k = h - data::brigade_count;
            int walls0 = g.walls_count;
            if(! g.player_repair(k)) return false;
            const core::repair_def& rd = data::repairs[k];
            banner.push(rd.name, rd.desc);
            bn::sound_items::sfx_ability.play();
            flash_color = bn::color(31, 22, 10);
            flash_timer = 6;
            for(int i = walls0; i < g.walls_count; ++i)   // deski wyskakują z ziemi
            {
                bn::fixed_point w = world(g.walls[i].x, g.walls[i].y);
                fx_particles.spawn(w.x(), w.y() + 4, fx_particles.rand(-8, 8), bn::fixed(-1.4), bn::fixed(0.15), 20, particle_pool::brick);
            }
            if(rd.effect == core::repair_effect::bridge)   // kładka: pył wokół bohatera
            {
                bn::fixed_point p = world(g.hero.x, g.hero.y);
                for(int kk = 0; kk < 6; ++kk)
                    fx_particles.spawn(p.x() + fx_particles.rand(-128, 128), p.y() + 6, 0, fx_particles.rand(-12, -4), 0, 20, particle_pool::dust, 3);
            }
            return true;
        };

        // Nowe problemy w trakcie etapu (podział): sprite'y, pozycje i paski dla dodatkowych miejsc.
        uint32_t prev_alive = 0;
        for(int i = 0; i < g.enemies_count; ++i) if(g.enemies[i].alive) prev_alive |= 1u << i;
        int prev_blast = g.blast_timer, prev_blast_x = g.blast_x, prev_blast_y = g.blast_y, prev_turns = g.turns;
        auto sync_enemies = [&]() {
            while(enemies.size() < g.enemies_count)
            {
                int i = enemies.size();
                bn::sprite_ptr sp = bn::sprite_items::actors.create_sprite(world(g.enemies[i].x, g.enemies[i].y), data::enemies[g.enemies[i].def_id].frame);
                sp.set_camera(cam);
                style_enemy(sp, g.enemies[i]);
                enemies.push_back(sp);
                enemy_gold.push_back(g.enemies[i].elite >= 0);
                state_icons.push_back(bn::optional<bn::sprite_ptr>());
                enemy_cur.push_back(world(g.enemies[i].x, g.enemies[i].y)); enemy_dst.push_back(enemy_cur.back());
                enemy_shown.push_back(-1);
                mini_bars.push_back(bn::optional<bn::sprite_ptr>());
            }
        };
        // Efekty zachowań i mechanik: strzały z dystansu, wybuch, poryw wiatru, nowe problemy z podziału / powrotu.
        auto behavior_fx = [&]() {
            bn::fixed_point h = world(g.hero.x, g.hero.y);
            for(int i = 0; i < g.enemies_count; ++i)
            {
                const core::actor& e = g.enemies[i];
                if(g.shot_events & (1u << i))   // strzał: iskry lecą od strzelca do bohatera
                {
                    bn::fixed_point p = world(e.x, e.y);
                    for(int k = 1; k <= 3; ++k)
                        fx_particles.spawn(p.x() + (h.x() - p.x()) * k / 4, p.y() + (h.y() - p.y()) * k / 4, (h.x() - p.x()) / 24, (h.y() - p.y()) / 24,
                                           0, 10 + k * 3, particle_pool::spark, 2);
                }
                bool alive_now = e.alive, was = prev_alive & (1u << i);
                if(alive_now && ! was)   // podział albo powrót: pojawia się z kurzem, bez przesuwania z dawnego miejsca
                {
                    enemy_cur[i] = enemy_dst[i];
                    bn::fixed_point p = world(e.x, e.y);
                    if(g.visible(e.x, e.y)) for(int k = -1; k <= 1; k += 2) fx_particles.spawn(p.x() + k * 4, p.y() + 6, bn::fixed(k) / 3, bn::fixed(-0.3), 0, 18, particle_pool::dust, 3);
                }
                if(alive_now) prev_alive |= 1u << i; else prev_alive &= ~(1u << i);
            }
            g.shot_events = 0;
            if(prev_blast > 0 && g.blast_timer == 0 && prev_blast_x >= 0)   // wybuch spadł
            {
                bn::fixed_point b = world(prev_blast_x, prev_blast_y);
                fx_particles.burst(b, 8, particle_pool::spark, 2, 40, 20);
                for(int k = 0; k < 4; ++k) fx_particles.spawn(b.x(), b.y(), fx_particles.rand(-24, 24), fx_particles.rand(-24, 8), 0, 22, particle_pool::dust, 3);
                flash_color = bn::color(31, 16, 4); flash_timer = 8; shake_timer = 6;
                bn::sound_items::sfx_hurt.play();
            }
            prev_blast = g.blast_timer; prev_blast_x = g.blast_x; prev_blast_y = g.blast_y;
            if(g.turns != prev_turns && g.act_is(core::act_mechanic::gust))   // poryw wiatru: pył w stronę porywu
            {
                int v = g.adef().mech_value, t = g.turns - g.stage_start_turn;
                if(t > 0 && t % v == 0)
                {
                    int d = (t / v + g.pattern_stage()) & 3;
                    for(int k = 0; k < 6; ++k)
                        fx_particles.spawn(h.x() - core::game::gust_vec[d][0] * 40 + fx_particles.rand(-64, 64), h.y() - core::game::gust_vec[d][1] * 40 + fx_particles.rand(-64, 64),
                                           bn::fixed(core::game::gust_vec[d][0]) * 3, bn::fixed(core::game::gust_vec[d][1]) * 3, 0, 24, particle_pool::dust, 3);
                    bn::sound_items::sfx_menu.play();
                }
            }
            prev_turns = g.turns;
        };
        auto refresh = [&]() {
            sync_enemies();
            map->build(g);
            bg_map_ptr.reload_cells_ref();
            bn::fixed_point old_dst = hero_dst;
            hero_dst = world(g.hero.x, g.hero.y);
            if(! snap_next && old_dst != hero_dst)   // pył spod butów
                for(int k = -1; k <= 1; k += 2)
                    fx_particles.spawn(old_dst.x() + k * 3, old_dst.y() + 6, bn::fixed(k) / 4, bn::fixed(-0.25), 0, 18,
                                       particle_pool::dust, 3);
            for(int i = 0; i < g.enemies_count; ++i)
            {
                enemies[i].set_visible(g.enemies[i].alive && g.visible(g.enemies[i].x, g.enemies[i].y));
                enemy_dst[i] = world(g.enemies[i].x, g.enemies[i].y);
            }
            if(g.ally_turns > 0) ally_dst = world(g.ally_x, g.ally_y);
            behavior_fx();
            if(snap_next)
            {
                hero_cur = hero_dst;
                ally_cur = ally_dst;
                for(int i = 0; i < g.enemies_count; ++i) enemy_cur[i] = enemy_dst[i];
                snap_next = false;
            }
            sync_pickups();
            for(int i = 0; i < g.pickups_count; ++i)
                pickups[i].set_visible(g.pickups[i].active && g.explored(g.pickups[i].x, g.pickups[i].y));
            stairs_lock.set_visible(g.stairs_locked() && g.explored(g.stairs_x, g.stairs_y) && ! (g.hero.x == g.stairs_x && g.hero.y == g.stairs_y));
            secret_sprite.set_visible(g.secret_closed() && g.explored(g.secret_x, g.secret_y));
            animate();

            hud.clear();
            a.text.set_left_alignment();
            a.text.generate(-116, -72, "HP", hud);
            int fill = core::imax(0, g.hero.hp) * 62 / g.hero.max_hp;
            if(g.hero.hp > 0 && fill == 0) fill = 1;
            int color = g.hero.hp * 2 > g.hero.max_hp ? 0 : (g.hero.hp * 4 > g.hero.max_hp ? 1 : 2);
            hp_left.set_tiles(bn::sprite_items::hp_bar.tiles_item(), color * 32 + core::imin(fill, 31));
            hp_right.set_tiles(bn::sprite_items::hp_bar.tiles_item(), 96 + color * 32 + core::imax(0, fill - 31));
            core::message num; num.add(g.hero.hp).add("/").add(g.hero.max_hp);
            a.text.generate(-30, -72, num.s, hud);
            a.text.set_right_alignment();
            core::message st; st.add("Etap ").add(g.stage_number()).add("/").add(g.stages_in_run()).add(" ");
            st.add(clip(g.ddef().name, 1).c_str());
            if(g.tier > 0) st.add("+").add(g.tier);
            a.text.generate(116, -72, st.s, hud);

            if(g.log_serial != log_seen)   // nowe komunikaty: pokaż na chwilę, kolor wg rodzaju
            {
                log_seen = g.log_serial;
                log_timer = 150;
                log.clear();
                a.text.set_left_alignment();
                for(int k = 0; k < 2; ++k)
                {
                    const core::message& m = g.log[core::log_lines - 2 + k];
                    if(m.n == 0) continue;
                    switch(m.kind)
                    {
                        case core::bad:  a.text.set_palette_item(bn::sprite_palette_items::font_map_bad); break;
                        case core::good: a.text.set_palette_item(bn::sprite_palette_items::font_map_good); break;
                        case core::loot: a.text.set_palette_item(bn::sprite_palette_items::font_map_loot); break;
                        default:         a.text.set_palette_item(bn::sprite_items::font_8x16.palette_item()); break;
                    }
                    core::message line; line.add(fit(a, m.s, m.repeat > 1 ? 208 : 232).c_str());
                    if(m.repeat > 1) line.add(" x").add(m.repeat);
                    a.text.generate(-116, 56 + k * 16, line.s, log);
                }
                a.text.set_palette_item(bn::sprite_items::font_8x16.palette_item());
                draw_strips(true);
            }

            fx.clear();
            for(int i = 0; i < g.enemies_count; ++i)
                if(g.turn_events & (1u << i))
                {
                    bn::sprite_ptr s = bn::sprite_items::actors.create_sprite(world(g.enemies[i].x, g.enemies[i].y), frame_fx);
                    s.set_camera(cam);
                    s.set_z_order(-10);
                    fx.push_back(s);
                }
            fx_timer = fx.empty() ? 0 : 10;
            if(g.hero_hit) { hurt_timer = 16; shake_timer = 8; }
            g.turn_events = 0;
            g.hero_hit = false;

            // liczby obrażeń
            a.text.set_center_alignment();
            for(int i = 0; i < g.hits_count; ++i)
            {
                if(floaters.full()) floaters.erase(floaters.begin());
                floaters.push_back(floater());
                floater& f = floaters.back();
                const core::hit& h = g.hits[i];
                core::message m;
                if(h.kind == core::hit_dodge) m.add("Unik!");
                else if(h.kind == core::hit_crit) m.add("KRYT! -").add(h.amount);
                else m.add("-").add(h.amount);
                bn::fixed_point p = world(g.hits[i].x, g.hits[i].y);
                a.text.set_palette_item(h.kind == core::hit_dodge ? bn::sprite_palette_items::font_map_good
                                      : h.kind == core::hit_crit ? bn::sprite_palette_items::font_map_loot
                                      : h.on_hero ? bn::sprite_palette_items::font_map_bad : bn::sprite_items::font_8x16.palette_item());
                if(h.kind == core::hit_crit) { flash_color = bn::color(31, 27, 8); flash_timer = 5; }   // krótki żółty błysk
                a.text.generate(p.x(), p.y() - 12, m.s, f.sprites);
                a.text.set_palette_item(bn::sprite_items::font_8x16.palette_item());
                for(bn::sprite_ptr& sp : f.sprites) { sp.set_camera(cam); sp.set_z_order(-60); }
                f.timer = 36;
                if(! g.hits[i].on_hero) target_timer = 90;
                fx_particles.burst(p, g.hits[i].on_hero ? 3 : 5, particle_pool::spark, 2, 24, 16);   // iskry
                if(i == 0 || g.hits[i].on_hero != g.hits[i - 1].on_hero)
                    (g.hits[i].on_hero ? bn::sound_items::sfx_hurt : bn::sound_items::sfx_hit).play();
            }
            g.hits_count = 0;
            if(g.combo_events)   // kombinacja stanów (#29): baner z zapowiedzią i błysk w kolorze kombinacji
            {
                for(int k = 0; k < data::combos_count; ++k)
                {
                    const core::combo_def& cd = data::combos[k];
                    if(g.combo_events & (1u << k))
                    {
                        core::message b; b.add(cd.name).add(" +").add(cd.value).add(cd.effect == core::combo_effect::crack ? "%" : "");
                        if(cd.radius > 0) b.add(" obok");
                        banner.push(cd.short_name, b.s);
                    }
                    if(g.combo_events & (8u << k)) { core::message b; b.add(cd.name).add(": -").add(cd.hero_value).add(" HP"); banner.push(cd.short_name, b.s); }
                }
                flash_color = (g.combo_events & 0x09) ? bn::color(12, 24, 31) : ((g.combo_events & 0x12) ? bn::color(31, 18, 6) : bn::color(24, 30, 31));
                flash_timer = 8;
                g.combo_events = 0;
            }
            update_mini_bars();
            update_state_icons();
            {
                int8_t tt[core::max_enemies];
                marked = g.targets_in_range(tt, core::max_enemies) > 0 ? tt[0] : -1;
                target_marker.set_visible(marked >= 0);
            }
            for(int i = 0; i < g.enemies_count; ++i)   // "!" nad wrogiem, który Cię właśnie zauważył
            {
                const core::actor& e = g.enemies[i];
                bool aw = e.alive && e.awake;
                if(aw && ! (prev_awake & (1u << i)) && g.visible(e.x, e.y))
                {
                    bn::fixed_point p = world(e.x, e.y);
                    fx_particles.spawn(p.x(), p.y() - 14, 0, bn::fixed(-0.15), 0, 40, particle_pool::alert);
                }
                if(aw) prev_awake |= 1u << i; else prev_awake &= ~(1u << i);
            }
            detect_events();
        };
        refresh();

        // Podgląd mapy: trzymaj L (samo L, bez R - L+R+SELECT to skrót pokazowy).
        auto overview = [&]() {
            map->build_overview(g);
            bg_map_ptr.reload_cells_ref();
            for(auto& s : enemies) s.set_visible(false);
            for(auto& s : pickups) s.set_visible(false);
            stairs_lock.set_visible(false);
            secret_sprite.set_visible(false);
            fx.clear(); log.clear(); floaters.clear(); fx_particles.list.clear(); levelup_text.clear(); levelup_timer = 0;
            hide_mini_bars(); target_marker.set_visible(false); status_sprite.set_visible(false);
            power_icon.set_visible(false); power_text.clear(); shown_cd = -1; hide_status_hud();
            ally_hidden = true; ally_sprite.reset();
            a.text.set_left_alignment();
            a.text.generate(-116, 72, "Podgląd mapy (puść L)", log);
            // magazyn (#32): znacznik ściany / drzwi, gdy odkryte, i skrzynia (odkryta, jeszcze nieotwarta)
            bn::vector<bn::sprite_ptr, 2> marks;
            auto mark = [&](int x, int y, int frame) {
                if(bn::optional<bn::sprite_ptr> sp = bn::sprite_items::particles.create_sprite_optional(x * 8 + 4 - 256, y * 8 + 4 - 256, frame))
                {
                    sp->set_camera(cam); sp->set_z_order(-50);
                    marks.push_back(bn::move(*sp));
                }
            };
            if(g.has_secret() && g.explored(g.secret_x, g.secret_y)) mark(g.secret_x, g.secret_y, particle_pool::map_secret);
            for(int i = 0; i < g.pickups_count && ! marks.full(); ++i)
                if(g.pickups[i].active && g.pickups[i].type == core::chest && g.explored(g.pickups[i].x, g.pickups[i].y))
                    mark(g.pickups[i].x, g.pickups[i].y, particle_pool::map_chest);
            bn::fixed_point hp(g.hero.x * 8 + 4 - 256, g.hero.y * 8 + 4 - 256);
            hero.set_position(hp);
            hero.set_scale(bn::fixed(0.5));
            cam.set_position(clampf(hp.x() + 128, 8) - 128, clampf(hp.y() + 128, 48) - 128);
            int t = 0;
            while(bn::keypad::l_held() && ! bn::keypad::r_held())
            {
                hero.set_visible((++t & 16) == 0);
                next_frame();
            }
            marks.clear();
            hero.remove_affine_mat();
            hero.set_visible(true);
            ally_hidden = false;
            snap_next = true;
            refresh();
            hold = 0;
        };

        // Pełnoekranowe okno (telefon, porównanie sprzętu): chowa mapę i HUD, zwalnia warstwę pasków.
        auto suspend_view = [&]() {
            bg.set_visible(false);
            hero.set_visible(false);
            for(auto& sp : enemies) sp.set_visible(false);
            for(auto& sp : pickups) sp.set_visible(false);
            stairs_lock.set_visible(false);
            secret_sprite.set_visible(false);
            fx.clear(); hud.clear(); log.clear(); floaters.clear(); levelup_text.clear(); levelup_timer = 0;
            hp_left.set_visible(false); hp_right.set_visible(false);
            hide_mini_bars(); target_marker.set_visible(false); status_sprite.set_visible(false);
            for(auto& mb : mini_bars) mb.reset();      // zwalnia kafle sprite'ów (telefon i Jak grać potrzebują ich na tekst)
            for(auto& si : state_icons) si.reset();
            const int shared = data::classes[g.cls].frame;   // ukryte postaci i znajdźki na klatce bohatera: jedne kafle zamiast wielu
            for(int i = 0; i < enemies.size(); ++i) { enemies[i].set_tiles(bn::sprite_items::actors.tiles_item(), shared); enemy_shown[i] = -1; }
            for(auto& sp : pickups) sp.set_tiles(bn::sprite_items::actors.tiles_item(), shared);
            power_icon.set_visible(false); power_text.clear(); shown_cd = -1; hide_status_hud();
            ally_hidden = true; ally_sprite.reset();
            release_strips();
            banner.hide();
            fx_particles.list.clear();
        };
        // Menu akcji pod START: ikony wokół bohatera - góra Atak, prawo Moc, dół Termos, lewo Czekaj.
        // Strzałka wybiera (druga raz tą samą strzałką albo A - wykonuje), START/B zamyka.
        bool menu_open = false;
        int menu_sel = -1;
        bn::vector<bn::sprite_ptr, 5> menu_sprites;
        static constexpr int8_t menu_dir[4][2] = { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 } };
        auto menu_label = [&]() {
            log.clear();
            a.text.set_left_alignment();
            a.text.set_palette_item(bn::sprite_palette_items::font_map_loot);
            core::message m;
            switch(menu_sel)
            {
                case 0: m.add("Atak: najbliższy cel (z").add(g.weapon_range()).add(")"); break;
                case 1:
                    m.add("Moc: ").add(ability_label(g).s);
                    if(g.ability_cd > 0) m.add(" - za ").add(g.ability_cd).add(" t.");
                    break;
                case 2: m.add("Termos ").add(g.thermos).add("/").add(g.thermos_cap()).add(": kawa +").add(g.coffee_heal()).add(" HP"); break;
                case 3: m.add("Czekaj turę"); break;
                default: m.add("Akcje: strzałka, A: Brygada"); break;
            }
            a.text.generate(-116, 56, fit(a, m.s, 232).c_str(), log);
            a.text.set_palette_item(bn::sprite_items::font_8x16.palette_item());
            a.text.generate(-116, 72, menu_sel < 0 ? "START/B: zamknij" : "A: wykonaj  START/B: zamknij", log);
            draw_strips(true);
            log_timer = 0;
            if(menu_sprites.size() == 5)
            {
                menu_sprites[4].set_visible(menu_sel >= 0);
                if(menu_sel >= 0) menu_sprites[4].set_position(menu_sprites[menu_sel].position());
            }
        };
        auto open_menu = [&]() {
            menu_open = true; menu_sel = -1;
            menu_sprites.clear();
            bn::fixed_point h = world(g.hero.x, g.hero.y);
            for(int k = 0; k < 4; ++k)
            {
                bn::fixed_point p(h.x() + menu_dir[k][0] * 22, h.y() + menu_dir[k][1] * 22);
                bn::optional<bn::sprite_ptr> sp = k == 1 ? bn::sprite_items::ability_icons.create_sprite_optional(p, g.cls)
                    : bn::sprite_items::menu_icons.create_sprite_optional(p, k == 0 ? 0 : (k == 2 ? 1 : 2));
                if(! sp) { menu_sprites.clear(); break; }
                sp->set_camera(cam); sp->set_z_order(-70);
                menu_sprites.push_back(bn::move(*sp));
            }
            if(menu_sprites.size() == 4)
                if(bn::optional<bn::sprite_ptr> ring = bn::sprite_items::menu_icons.create_sprite_optional(0, 0, 3))
                {
                    ring->set_camera(cam); ring->set_z_order(-71); ring->set_visible(false);
                    menu_sprites.push_back(bn::move(*ring));
                }
            target_marker.set_visible(false);
            menu_label();
        };
        auto close_menu = [&]() {
            menu_open = false;
            menu_sprites.clear();
            log.clear(); draw_strips(false); log_seen = g.log_serial;
        };

        auto resume_view = [&]() {
            ally_hidden = false;
            for(int i = 0; i < pickups.size(); ++i) pickups[i].set_tiles(bn::sprite_items::actors.tiles_item(), pickup_frame(g.pickups[i]));
            bg.set_visible(true);
            hero.set_visible(true);
            hp_left.set_visible(true); hp_right.set_visible(true);
            create_strips();
            draw_strips(strips_bottom);
            snap_next = true;
            refresh();
            hold = 0;
        };

        // Awans: duży napis "AWANS! Poziom N" wyskakuje nad bohaterem (~1.5 s), złoty błysk, pierścień i unoszące się gwiazdki.
        auto levelup_fx = [&]() {
            if(! levelup_pending) return;
            core::message m; m.add("AWANS! Poziom ").add(levelup_pending);
            levelup_pending = 0;
            levelup_text.clear();
            bn::fixed_point h = world(g.hero.x, g.hero.y);
            a.text.set_center_alignment();
            a.text.set_palette_item(bn::sprite_palette_items::font_map_loot);
            a.text.generate(h.x(), h.y() - 26, m.s, levelup_text);
            a.text.set_palette_item(bn::sprite_items::font_8x16.palette_item());
            for(bn::sprite_ptr& sp : levelup_text) { sp.set_camera(cam); sp.set_z_order(-80); }
            levelup_timer = 90;
            flash_color = bn::color(31, 27, 10);
            flash_timer = 10;
            static constexpr int8_t dir[8][2] = { { 2, 0 }, { 1, 1 }, { 0, 2 }, { -1, 1 }, { -2, 0 }, { -1, -1 }, { 0, -2 }, { 1, -1 } };
            for(int k = 0; k < 8; ++k)   // pierścień iskier + gwiazdki unoszące się w górę
                fx_particles.spawn(h.x(), h.y(), bn::fixed(dir[k][0]) * bn::fixed(0.9), bn::fixed(dir[k][1]) * bn::fixed(0.9), 0, 20, particle_pool::ring, 2);
            for(int k = 0; k < 6; ++k)
                fx_particles.spawn(h.x() + fx_particles.rand(-160, 160), h.y() + 6, 0, fx_particles.rand(-24, -10), 0, 44, particle_pool::star);
        };
        auto levelup_update = [&]() {
            if(levelup_timer <= 0) return;
            --levelup_timer;
            int age = 90 - levelup_timer;
            for(bn::sprite_ptr& sp : levelup_text)
            {
                if(age < 8) sp.set_y(sp.y() - 2);                 // wyskok
                else if((age & 3) == 0) sp.set_y(sp.y() - 1);     // powolne unoszenie
                sp.set_visible(levelup_timer > 20 || (levelup_timer & 2));   // miganie przed zniknięciem
            }
            if(levelup_timer == 0) levelup_text.clear();
        };

        while(true)
        {
            levelup_fx();
            levelup_update();
            if(bn::keypad::l_held() && ! bn::keypad::r_held()) { overview(); continue; }
            bool acted = false;
            int dx = 0, dy = 0;
            // ruch: pojedyncze wciśnięcie lub przytrzymanie (auto-powtórzenie)
            if(aiming)   // celowanie trwa: strzałki wybierają cel, puszczenie A atakuje
            {
                ++aim_frames;
                if(aim_frames == 8) { show_range(true); reticle.set_visible(aim_count > 0); }
                if(aim_count > 1 && (bn::keypad::right_pressed() || bn::keypad::down_pressed())) aim_sel = (aim_sel + 1) % aim_count;
                if(aim_count > 1 && (bn::keypad::left_pressed() || bn::keypad::up_pressed())) aim_sel = (aim_sel + aim_count - 1) % aim_count;
                if(aim_count > 0)
                {
                    const core::actor& e = g.enemies[aim_targets[aim_sel]];
                    reticle.set_position(world(e.x, e.y));
                    marked = aim_targets[aim_sel];
                    reticle.set_visible(aim_frames >= 8 && ((aim_frames / 6) & 1) == 0 ? true : aim_frames >= 8);
                }
                if(! bn::keypad::a_held())
                {
                    aiming = false;
                    reticle.set_visible(false);
                    if(aim_count > 0) { show_range(false); acted = g.player_attack(aim_targets[aim_sel]); }
                    else { show_range(true); range_flash = 20; }   // brak celu: zasięg tylko mignie
                }
                if(acted) refresh();
                animate(); fx_particles.update(); banner.update(a);
                next_frame();
                continue;
            }
            if(range_flash > 0 && --range_flash == 0) show_range(false);
            if(menu_open)
            {
                int pick = -1;
                for(int k = 0; k < 4; ++k)
                {
                    bool pr = k == 0 ? bn::keypad::up_pressed() : (k == 1 ? bn::keypad::right_pressed()
                            : (k == 2 ? bn::keypad::down_pressed() : bn::keypad::left_pressed()));
                    if(! pr) continue;
                    if(menu_sel == k) pick = k;
                    else { menu_sel = k; menu_label(); bn::sound_items::sfx_menu.play(); }
                }
                if(bn::keypad::a_pressed() && menu_sel >= 0) pick = menu_sel;
                else if(bn::keypad::a_pressed())   // A bez kierunku: Brygada w telefonie
                {
                    close_menu();
                    suspend_view();
                    int tab = a.phone_tab;
                    a.phone_tab = 3;
                    pause_result pr = run_phone(a, true);
                    a.phone_tab = tab;
                    if(pr == pause_result::save_exit) { save_run(a); return leave(scene::title); }
                    if(pr == pause_result::quit)
                    {
                        if(g.score > a.save.best) a.save.best = g.score;
                        core::check_badges(a.save, g);
                        core::check_contracts(a.save);
                        core::bank_xp(a.save, g);
                        bn::sram::write(a.save);
                        clear_run(a);
                        return leave(scene::shop);
                    }
                    resume_view();
                    if(pr == pause_result::resume && a.pending_helper >= 0)
                    {
                        int h = a.pending_helper;
                        a.pending_helper = -1;
                        if(do_pending(h)) refresh();
                    }
                    continue;
                }
                if(pick < 0 && (bn::keypad::start_pressed() || bn::keypad::b_pressed())) { close_menu(); wait_release(); refresh(); }
                else if(pick >= 0)
                {
                    close_menu();
                    switch(pick)
                    {
                        case 0:
                            acted = g.player_attack_nearest();
                            if(! acted) { show_range(true); range_flash = 20; }
                            break;
                        case 1:
                            acted = g.player_ability();
                            if(acted) { bn::sound_items::sfx_ability.play(); ability_fx(); }
                            break;
                        case 2:
                            acted = g.player_drink();
                            if(acted) bn::sound_items::sfx_pickup.play();
                            break;
                        default:
                            acted = g.player_wait();
                            break;
                    }
                    refresh();
                    wait_release();
                }
                animate(); fx_particles.update(); banner.update(a);
                next_frame();
                continue;
            }
            if(looking)   // B: krótko = czekaj turę; przytrzymaj = podgląd wrogów (bez zużycia tury)
            {
                ++look_frames;
                if(look_frames == 10)
                {
                    look_count = 0;
                    for(int d = 1; d <= g.sight_radius() + 1; ++d)
                        for(int i = 0; i < g.enemies_count; ++i)
                            if(g.enemies[i].alive && g.visible(g.enemies[i].x, g.enemies[i].y)
                               && core::cheb(g.hero.x, g.hero.y, g.enemies[i].x, g.enemies[i].y) == d)
                                look_list[look_count++] = int8_t(i);
                }
                if(look_frames >= 10 && look_count > 0)
                {
                    if(bn::keypad::right_pressed() || bn::keypad::down_pressed()) { look_sel = (look_sel + 1) % look_count; look_shown = -1; }
                    if(bn::keypad::left_pressed() || bn::keypad::up_pressed()) { look_sel = (look_sel + look_count - 1) % look_count; look_shown = -1; }
                    const core::actor& e = g.enemies[look_list[look_sel]];
                    reticle.set_position(world(e.x, e.y));
                    reticle.set_visible(true);
                    int look_key = look_list[look_sel] + 32 * ((look_frames / 100) % 12);   // druga linia na zmianę: obrażenia / opis / zachowania
                    if(look_shown != look_key)   // karta wroga w miejscu dziennika
                    {
                        look_shown = look_key;
                        const core::enemy_def& ed = data::enemies[e.def_id];
                        log.clear();
                        a.text.set_left_alignment();
                        a.text.set_palette_item(bn::sprite_palette_items::font_map_loot);
                        const int ei = look_list[look_sel];
                        core::message l1; g.enemy_name(l1, ei);   // elita: przedrostek ("Zbrojony Przeciek")
                        l1.add("  HP ").add(e.hp).add("/").add(e.max_hp);
                        if(g.enemy_defense(ei)) l1.add("  OBR ").add(g.enemy_defense(ei));
                        a.text.generate(-116, 56, fit(a, l1.s, 232).c_str(), log);
                        a.text.set_palette_item(bn::sprite_items::font_8x16.palette_item());
                        // druga linia na zmianę: obrażenia w obie strony (rozpiska #26, obrona elity), elita, stany, opis, zachowania
                        core::message bl = behaviors_line(e.def_id), sl = enemy_states_line(g, ei);
                        const core::dmg_breakdown vb = g.actor_breakdown(ei);
                        const core::hit_range vh = g.enemy_hit(ei);
                        core::message vs; core::versus_line(vs, vb, vh);
                        const bool split = a.text.width(vs.s) > 232;   // za długie: "Zadasz..." i "on Tobie..." osobno
                        int list[7], phases = 0;                       // 0 obrażenia, 1 on Tobie, 2 opis, 3 cechy, 4 elita, 5 stany, 6 klucz
                        list[phases++] = 0;
                        if(split) list[phases++] = 1;
                        if(ei == g.key_holder) list[phases++] = 6;
                        if(e.elite >= 0) list[phases++] = 4;
                        if(sl.n > 0) list[phases++] = 5;
                        list[phases++] = 2;
                        if(bl.n > 0) list[phases++] = 3;
                        const int phase = list[(look_key >> 5) % phases];
                        core::message l2;
                        if(phase == 0) { if(split) core::versus_hero(l2, vb); else l2 = vs; }
                        else if(phase == 1) { core::add_range(l2.add("On Tobie "), vh.min, vh.max).add(", unik ").add(g.dodge_pct()).add("%"); }
                        else if(phase == 3) l2.add("Cechy: ").add(bl.s);
                        else if(phase == 4) l2.add("Elita: ").add(data::elites[e.elite].name).add(" - ").add(data::elites[e.elite].info);
                        else if(phase == 5) l2.add("Stan: ").add(sl.s);
                        else if(phase == 6) l2.add("Ma klucz do magazynu!");
                        else l2.add(ed.desc);
                        a.text.generate(-116, 72, fit(a, l2.s, 232).c_str(), log);
                        draw_strips(true);
                        log_timer = 0;
                    }
                }
                else if(look_frames >= 10 && look_shown != -2)
                {
                    look_shown = -2;
                    log.clear();
                    a.text.set_left_alignment();
                    a.text.generate(-116, 72, "Nikogo w polu widzenia", log);
                    draw_strips(true);
                }
                if(! bn::keypad::b_held())
                {
                    looking = false;
                    reticle.set_visible(false);
                    if(look_frames < 10) acted = g.player_wait();
                    else { log.clear(); draw_strips(false); log_seen = g.log_serial; look_shown = -1; }
                }
                if(acted) refresh();
                animate(); fx_particles.update(); banner.update(a);
                next_frame();
                continue;
            }
            bool any_dir = bn::keypad::left_held() || bn::keypad::right_held() || bn::keypad::up_held() || bn::keypad::down_held();
            bool pressed = bn::keypad::left_pressed() || bn::keypad::right_pressed() || bn::keypad::up_pressed() || bn::keypad::down_pressed();
            hold = any_dir ? hold + 1 : 0;
            if(pressed || (hold > 14 && hold % 6 == 0))
            {
                if(bn::keypad::left_held()) dx = -1;
                else if(bn::keypad::right_held()) dx = 1;
                else if(bn::keypad::up_held()) dy = -1;
                else if(bn::keypad::down_held()) dy = 1;
                if(dx || dy) acted = g.player_move(dx, dy);
                if(! acted && g.log_serial != log_seen) refresh();   // bez tury, ale z komunikatem (np. zamknięty magazyn)
            }
            else if(bn::keypad::a_pressed() && ! aiming)
            {
                aiming = true; aim_frames = 0; aim_sel = 0;
                aim_count = g.targets_in_range(aim_targets, core::max_enemies);
            }
            else if(bn::keypad::b_pressed() && ! looking) { looking = true; look_frames = 0; look_sel = 0; }
            else if(bn::keypad::start_pressed()) { open_menu(); next_frame(); continue; }
            else if(bn::keypad::r_pressed() && ! bn::keypad::l_held())
            {
                acted = g.player_ability();
                if(acted) { bn::sound_items::sfx_ability.play(); ability_fx(); }
                else refresh();
            }
            // skrót pokazowy/testowy: L+R+SELECT = zalicz etap; samo SELECT = menu
            if(bn::keypad::select_pressed() && bn::keypad::l_held() && bn::keypad::r_held()) { g.debug_skip(); snap_next = true; refresh(); }
            else if(bn::keypad::select_pressed())
            {
                suspend_view();
                pause_result pr = run_phone(a);
                if(pr == pause_result::save_exit) { save_run(a); return leave(scene::title); }
                if(pr == pause_result::quit)
                {
                    if(g.score > a.save.best) a.save.best = g.score;
                    core::check_badges(a.save, g);   // liczniki zleceń z przerwanej budowy też się liczą
                    core::check_contracts(a.save);
                    core::bank_xp(a.save, g);
                    bn::sram::write(a.save);
                    clear_run(a);
                    return leave(scene::shop);
                }
                resume_view();
                if(a.pending_helper >= 0)   // brygada wybrana w telefonie: wezwanie zużywa turę
                {
                    int h = a.pending_helper;
                    a.pending_helper = -1;
                    if(do_pending(h)) refresh();
                }
                continue;
            }

            if(acted) refresh();
            // v0.21.50 cz. 3: wydarzenie z wyborem, premia z wydarzenia, cecha narzędzia, narzędzie przy ulepszonym
            if(g.st == core::status::playing && (g.pending_event >= 0 || g.has_boon_offer() || g.trait_pending || g.has_tool_offer()))
            {
                suspend_view();
                if(g.pending_event >= 0) event_dialog(a);
                if(g.has_boon_offer()) boon_pick(a, true);
                if(g.trait_pending) trait_dialog(a);
                if(g.has_tool_offer()) tool_offer_dialog(a);
                resume_view();
                continue;
            }
            if(g.has_offer())   // paczka sprzętu: okno porównania (jak telefon - zwalnia paski)
            {
                suspend_view();
                gear_offer_dialog(a);
                resume_view();
                continue;
            }
            animate();
            fx_particles.update();
            if(log_timer > 0 && --log_timer == 0) { log.clear(); draw_strips(false); }   // komunikaty znikają
            if(fx_timer > 0 && --fx_timer == 0) fx.clear();
            for(int i = 0; i < floaters.size(); )
            {
                floater& f = floaters[i];
                if(--f.timer <= 0) { floaters.erase(floaters.begin() + i); continue; }
                if(f.timer & 1) for(bn::sprite_ptr& sp : f.sprites) sp.set_y(sp.y() - 1);
                ++i;
            }
            if(flash_timer > 0 && fade_in_left == 0 && shake_timer == 0)   // błysk mocy
            {
                --flash_timer;
                bn::bg_palettes::set_fade(flash_color, bn::fixed(flash_timer) / 16);
            }
            // wstrząs i czerwony błysk po otrzymaniu obrażeń
            if(shake_timer > 0)
            {
                --shake_timer;
                cam.set_position(cam_base.x() + (shake_timer == 0 ? 0 : ((shake_timer & 1) ? 2 : -2)), cam_base.y());
                if(fade_in_left == 0)
                    bn::bg_palettes::set_fade(bn::color(31, 4, 4), shake_timer > 4 ? bn::fixed(0.3) : bn::fixed(0));
            }
            if(hurt_timer > 0) { --hurt_timer; hero.set_visible((hurt_timer & 2) == 0); }
            bool low_hp = g.hero.hp * 4 <= g.hero.max_hp;
            ++blink;
            bool banner_on = banner.update(a);
            for(bn::sprite_ptr& sp : hud) sp.set_visible(! banner_on);
            if(g.ability_cd != shown_cd)   // odliczanie przy ikonie mocy
            {
                shown_cd = g.ability_cd;
                power_text.clear();
                a.text.set_palette_item(bn::sprite_items::font_8x16.palette_item());
                a.text.set_bg_priority(0);
                a.text.set_right_alignment();
                if(shown_cd > 0) { core::message m; m.add(shown_cd); a.text.generate(96, -52, m.s, power_text); }
                else a.text.generate(96, -52, "R", power_text);
                power_palette.set_grayscale_intensity(shown_cd > 0 ? bn::fixed(1) : bn::fixed(0));
            }
            {
                int key = 0;
                for(int k = 0; k < 4; ++k) key = key * 64 + core::imax(0, g.status_turns(hud_statuses[k]));
                if(key != shown_status)
                {
                    shown_status = key;
                    status_text.clear();
                    status_count = 0;
                    a.text.set_palette_item(bn::sprite_palette_items::font_map_bad);
                    a.text.set_bg_priority(0);
                    a.text.set_left_alignment();
                    for(int k = 0; k < 4; ++k)
                    {
                        int turns_left = g.status_turns(hud_statuses[k]);
                        if(turns_left <= 0) continue;
                        int x = -112 + status_count * 22;
                        status_icons[status_count].set_tiles(bn::sprite_items::particles.tiles_item(), particle_pool::status_icon + k);
                        status_icons[status_count].set_x(x);
                        core::message m; m.add(turns_left);
                        a.text.generate(x + 5, -52, m.s, status_text);
                        ++status_count;
                    }
                    a.text.set_palette_item(bn::sprite_items::font_8x16.palette_item());
                }
                for(int k = 0; k < 4; ++k) status_icons[k].set_visible(! banner_on && k < status_count);
                for(bn::sprite_ptr& sp : status_text) sp.set_visible(! banner_on);
                if(g.thermos != shown_thermos)
                {
                    shown_thermos = g.thermos;
                    thermos_text.clear();
                    a.text.set_palette_item(bn::sprite_items::font_8x16.palette_item());
                    a.text.set_bg_priority(0);
                    a.text.set_left_alignment();
                    core::message m; m.add(g.thermos).add("/").add(g.thermos_cap());
                    a.text.generate(59, -52, m.s, thermos_text);
                }
                int mkey = g.mats[0] * 10000 + g.mats[1] * 100 + g.mats[2];
                if(mkey != shown_mats)   // materiały: liczby obok ikon
                {
                    shown_mats = mkey;
                    mat_text.clear();
                    a.text.set_palette_item(bn::sprite_items::font_8x16.palette_item());
                    a.text.set_bg_priority(0);
                    a.text.set_left_alignment();
                    for(int m = 0; m < data::materials_count; ++m)
                    {
                        core::message mm; mm.add(g.mats[m]);
                        a.text.generate(mat_x0 + m * mat_dx + 2, -52, mm.s, mat_text);
                    }
                }
                bool any_mat = mkey > 0 && status_count <= 2;   // trzy stany zajmują cały rząd
                for(auto& sp : mat_icons) sp.set_visible(! banner_on && any_mat);
                for(bn::sprite_ptr& sp : mat_text) sp.set_visible(! banner_on && any_mat);
                thermos_icon.set_visible(! banner_on);
                weather_icon.set_visible(! banner_on);
                act_icon.set_visible(! banner_on && act_mech > 0);
                int gin = g.gust_in();
                int akey = g.docs_needed() > 0 ? 100 + g.docs_count() : gin;   // pieczątki: dokumenty zebrane / potrzebne
                if(akey != shown_gust)   // porywy: tury do kolejnego (ostatnia tura - na czerwono)
                {
                    shown_gust = akey;
                    act_text.clear();
                    if(g.docs_needed() > 0)
                    {
                        a.text.set_palette_item(g.stairs_locked() ? bn::sprite_items::font_8x16.palette_item() : bn::sprite_palette_items::font_map_good);
                        a.text.set_bg_priority(0);
                        a.text.set_right_alignment();
                        core::message m; m.add(g.docs_count()).add("/").add(g.docs_needed());
                        a.text.generate(96, -33, m.s, act_text);
                        a.text.set_palette_item(bn::sprite_items::font_8x16.palette_item());
                    }
                    else if(gin > 0)
                    {
                        a.text.set_palette_item(gin == 1 ? bn::sprite_palette_items::font_map_bad : bn::sprite_items::font_8x16.palette_item());
                        a.text.set_bg_priority(0);
                        a.text.set_right_alignment();
                        core::message m; m.add(gin);
                        a.text.generate(96, -33, m.s, act_text);
                        a.text.set_palette_item(bn::sprite_items::font_8x16.palette_item());
                    }
                }
                for(bn::sprite_ptr& sp : act_text) sp.set_visible(! banner_on);
                for(bn::sprite_ptr& sp : thermos_text) sp.set_visible(! banner_on);
            }
            bool ready = g.ability_cd == 0;
            power_icon.set_visible(! banner_on);
            power_icon.set_y(ready && ((blink / 8) & 1) ? -53 : -52);   // gotowa moc "podskakuje"
            for(bn::sprite_ptr& sp : power_text) sp.set_visible(! banner_on && (! ready || ((blink / 16) & 1)));
            hp_left.set_visible(! banner_on && (! low_hp || (blink & 16)));
            hp_right.set_visible(! banner_on && (! low_hp || (blink & 16)));

            if(g.st == core::status::stage_clear)   // czeka też na banery odznak i zleceń (A pomija)
            {
                for(int i = 0; i < 70 || (banner.busy() && i < 420 && ! bn::keypad::a_pressed()); ++i)
                { banner.update(a); animate(); fx_particles.update(); next_frame(); }
                return leave(scene::schedule);
            }
            if(g.st == core::status::dead || g.st == core::status::won)
            {
                hero.set_visible(true);
                for(int i = 0; i < 90 || (banner.busy() && i < 480 && ! bn::keypad::a_pressed()); ++i)   // też banery odznak
                {
                    if(g.st == core::status::won && (i % 6) == 0)   // konfetti na odbiór budowy
                        for(int k = 0; k < 2; ++k)
                            fx_particles.spawn(cam_base.x() + fx_particles.rand(-110 * 16, 110 * 16), cam_base.y() - 84,
                                               fx_particles.rand(-8, 8), fx_particles.rand(8, 20), bn::fixed(0.02), 80,
                                               particle_pool::confetti + fx_particles.rnd.get_int(4));
                    banner.update(a); animate(); fx_particles.update(); next_frame();
                }
                return leave(scene::end);
            }
            next_frame();
        }
    }

    // Kolejny etap (po harmonogramie albo Hurtowni).
    void advance_stage(app& a)
    {
        a.g->next_stage();
#ifdef PB_SCENARIO
        debug_scenario::after_next_stage(*a.g, PB_SCENARIO);
#endif
    }

    // ------------------------------------------------------------------ premia po etapie (#27): 1 z 3
    // Trzy karty (rzadkość kolorem: zwykła szara, rzadka fioletowa, legendarna złota), opis i znaczniki; "Synergia!",
    // gdy wybór włączy synergię. Góra/dół - wybór, A - bierzesz, SELECT - losuj jeszcze raz (raz płatne, Druga oferta
    // za darmo). Po wyborze nowa synergia: potwierdzenie z opisem (A - dalej). Bez pomijania.

    void boon_pick(app& a, bool mid_stage)
    {
        core::game& g = *a.g;
        if(! g.has_boon_offer()) return;
#ifdef PB_SCENARIO
        debug_scenario::before_boon_pick(g, PB_SCENARIO);
#endif
        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        phone_screen ph(3);
        ph.icon.set_visible(false);
        bn::sprite_ptr boon_icon = bn::sprite_items::menu_icons.create_sprite(-100, -59, icon_boon);
        boon_icon.set_bg_priority(1);
        bn::sprite_palette_item default_ink = a.text.palette_item();
        page_sprites t;
        int sel = 0, synergy = -1, picked = -1;
        auto redraw = [&]() {
            if(synergy >= 0)   // nowa synergia: potwierdzenie
            {
                phone_header(a, ph, t, "   Synergia!", "A: dalej");
                phone_canvas& c = *ph.canvas;
                const core::synergy_def& sd = data::synergies[synergy];
                stripe(c, 0, phone_tile::stripe_done);
                core::message m0; m0.add("Synergia: ").add(sd.name);
                phone_text(a, t, list_x, row_py(0), m0.s, ink::done);
                phone_text(a, t, list_x, row_py(1), fit(a, sd.desc, phone_text_w).c_str(), ink::dark);
                core::message tg; tg.add("Znaczniki: ");
                for(int k = 0; k < data::boon_tags_count; ++k) if((sd.tags >> k) & 1) tg.add(tg.n > 11 ? ", " : "").add(data::boon_tags[k]);
                phone_text(a, t, list_x, row_py(2), fit(a, tg.s, phone_text_w).c_str(), ink::dim);
                const core::boon_def& bd = data::boons[picked];
                stripe(c, 4, rarity_stripe(bd.rarity));
                core::message m4; m4.add("Premia: ").add(bd.name);
                phone_text(a, t, list_x, row_py(4), fit(a, m4.s, phone_text_w).c_str(), rarity_ink(bd.rarity));
                phone_text(a, t, list_x, row_py(5), fit(a, bd.desc, phone_text_w).c_str(), ink::dim);
                ph.commit();
                return;
            }
            core::message sub;
            if(g.can_reroll()) { sub.add("SEL: losuj "); if(g.reroll_price() > 0) sub.add(g.reroll_price()).add(" zł"); else sub.add("za darmo"); }
            else sub.add("A: wybierz");
            phone_header(a, ph, t, mid_stage ? "   Premia: projekt" : "   Premia za etap", sub.s);
            phone_canvas& c = *ph.canvas;
            const uint16_t now = g.synergy_mask();
            for(int k = 0; k < 3; ++k)
            {
                const int b = g.boon_offer[k];
                if(b < 0) continue;
                const core::boon_def& bd = data::boons[b];
                const bool on = k == sel;
                const int r0 = 2 * k;
                if(on) c.rounded(1, row_ty(r0), 28, 4, phone_tile::fill_group, phone_tile::corner_group);
                stripe(c, r0, rarity_stripe(bd.rarity)); stripe(c, r0 + 1, rarity_stripe(bd.rarity));
                const char* rn = data::boon_rarities[bd.rarity].name;
                phone_text(a, t, list_x, row_py(r0), fit(a, bd.name, pill_room(rn)).c_str(), rarity_ink(bd.rarity));
                phone_pill(a, c, t, pill_end, row_ty(r0), rn, rarity_pill(bd.rarity));
                const bool syn = core::game::synergy_mask_in(g.boons | (uint64_t(1) << b)) != now;
                core::message d; d.add(bd.desc);
                core::message tg = boon_tags_line(bd);
                const int room = syn ? pill_room("Synergia!") : phone_text_w;
                if(a.text.width(d.s) + a.text.width(tg.s) + 10 <= room) d.add("  ").add(tg.s);
                phone_text(a, t, list_x, row_py(r0 + 1), fit(a, d.s, room).c_str(), on ? ink::dark : ink::dim);
                if(syn) phone_pill(a, c, t, pill_end, row_ty(r0 + 1), "Synergia!", pill::done);
            }
            ph.commit();
        };
        redraw();
        wait_release();
        while(true)
        {
            if(synergy >= 0)
            {
                if(bn::keypad::a_pressed() || bn::keypad::start_pressed() || bn::keypad::b_pressed()) break;
                next_frame();
                continue;
            }
            int d = bn::keypad::up_pressed() ? -1 : (bn::keypad::down_pressed() ? 1 : 0);
            if(d) { sel = (sel + d + 3) % 3; redraw(); bn::sound_items::sfx_menu.play(); }
            if(bn::keypad::select_pressed())
            {
                if(g.reroll_boons()) { bn::sound_items::sfx_buy.play(); sel = 0; redraw(); }
                else bn::sound_items::sfx_hurt.play();
            }
            if(bn::keypad::a_pressed() && g.boon_offer[sel] >= 0)
            {
                const uint16_t before = g.synergy_mask();
                picked = g.boon_offer[sel];
                g.pick_boon(sel);
                bn::sound_items::sfx_level.play();
                const uint16_t now = g.synergy_mask();
                for(int k = 0; k < data::synergies_count && synergy < 0; ++k)
                    if(((now >> k) & 1) && ! ((before >> k) & 1)) synergy = k;
                if(synergy < 0) break;
                wait_release();
                redraw();
            }
            next_frame();
        }
        t.clear();
        a.text.set_palette_item(default_ink);
        wait_release();
        if(! mid_stage) leave(scene::schedule);   // ściemnij przed harmonogramem
    }

    // ------------------------------------------------------------------ harmonogram między etapami: wybór ścieżki
    // Telefon z aplikacją: zaliczony etap, kolejny etap i dwa warianty do wyboru (rozgałęzienie jak mapa w Slay the
    // Spire, kolejność etapów stała). Lewo/prawo - wariant, A/START - dalej (Hurtownia na końcu aktu).
    scene run_schedule(app& a)
    {
        core::game& g = *a.g;
        boon_pick(a);   // najpierw premia 1 z 3 (#27), potem ścieżka i Hurtownia
        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        phone_screen ph(0);
        ph.icon.set_visible(false);
        bn::sprite_palette_item default_ink = a.text.palette_item();
        page_sprites t;
        const int next = g.stage + 1;
        int sel = g.next_path;
        auto redraw = [&]() {
            phone_header(a, ph, t, "Harmonogram", "<>: wybór  A: dalej");
            phone_canvas& c = *ph.canvas;
            // zaliczony etap i kolejny (węzeł)
            stripe(c, 0, phone_tile::stripe_done);
            core::message m0; m0.add(g.stage_number()).add(". ").add(data::stages[g.stage].name);
            phone_text(a, t, list_x, row_py(0), fit(a, m0.s, pill_room("Gotowe")).c_str(), ink::dim);
            phone_pill(a, c, t, pill_end, row_ty(0), "Gotowe", pill::done);
            stripe(c, 1, phone_tile::stripe_prog);
            core::message m1; m1.add(g.stage_number() + 1).add(". ").add(data::stages[next].name);
            const char* rest = g.investor_has(core::investor_effect::no_break) ? "Bez przerwy" : "Kawa +5 HP";
            phone_text(a, t, list_x, row_py(1), fit(a, m1.s, pill_room(rest)).c_str(), ink::dark);
            phone_pill(a, c, t, pill_end, row_ty(1), rest, pill::gray);
            // rozgałęzienie: pień z węzła etapu, poprzeczka, dwa narożniki w dół do kart wariantów
            c.set(15, 8, phone_tile::line_tee_up);
            for(int x = 8; x < 22; ++x) if(x != 15) c.set(x, 8, phone_tile::line_h);
            c.set(7, 8, phone_tile::line_corner); c.set(22, 8, phone_tile::line_corner, true);
            for(int k = 0; k < 2; ++k)
            {
                const core::path_def& pd = data::paths[g.path_offer(k)];
                int tx = k == 0 ? 1 : 16;
                bool on = k == sel;
                c.rounded(tx, 9, 13, 3, on ? phone_tile::fill_brand : phone_tile::fill_group, on ? phone_tile::corner_brand : phone_tile::corner_group);
                phone_text(a, t, tx * 8 + 52, 9 * 8 + 4, fit(a, pd.name, 98).c_str(), on ? ink::white : ink::brand, 0);
            }
            const core::path_def& ps = data::paths[g.path_offer(sel)];
            phone_text(a, t, list_x, 97, fit(a, ps.desc, phone_text_w).c_str(), ink::dark);
            // rada kierownika (data::tips) - kolejna z każdym etapem, bez losowania (nie rusza RNG gry)
            phone_text(a, t, list_x, 112, fit(a, data::tips[(g.stage + g.tier * 3) % data::tips_count], phone_text_w).c_str(), ink::dim);
            ph.commit();
        };
        redraw();
        wait_release();
        while(true)
        {
            int d = bn::keypad::left_pressed() ? -1 : (bn::keypad::right_pressed() ? 1 : 0);
            if(d) { sel = (sel + d + 2) % 2; g.choose_path(sel); redraw(); bn::sound_items::sfx_menu.play(); }
            if(bn::keypad::a_pressed() || bn::keypad::start_pressed())
            {
                g.choose_path(sel);
                t.clear();
                a.text.set_palette_item(default_ink);
                wait_release();
                if(g.act_cleared && ! g.shop_closed()) return leave(scene::hurtownia);   // koniec aktu: zakupy (tryb inwestora: zamknięta)
                advance_stage(a);
                return leave(scene::game);
            }
            next_frame();
        }
    }

    // ------------------------------------------------------------------ po wygranej: harmonogram domu w stylu aplikacji
    // Zdjęcie domu (Osiedle), etapy z datą rozpoczęcia, kosztem i znacznikiem "gotowe", razem dni i koszt, na dole
    // zaproszenie do planbudowlany.online (kod QR na kolejnym ekranie). Góra/dół przewija, A - dalej.
    void house_page(app& a)
    {
        const core::game& g = *a.g;
        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        phone_screen ph(0);
        ph.icon.set_visible(false);
        bn::sprite_palette_item default_ink = a.text.palette_item();
        page_sprites t;
        int size = core::imin(3, g.score / 1000);
        bn::sprite_ptr house = bn::sprite_items::houses.create_sprite(list_x + 8 - 120, row_py(0) + 8 - 80, size * data::classes_count + g.cls);
        house.set_bg_priority(1);
        int y, m, d;
        core::daily_date(a.save, y, m, d);   // dzień odbioru: data z ekranu budowy dnia (GBA bez zegara)
        const int end_day = core::days_from_civil(y, m, d);
        int top = g.first_stage;   // bez Aktu 0: od Fundamentów
        auto date = [](core::message& msg, int day) {
            int yy, mm, dd; core::civil_from_days(day, yy, mm, dd);
            msg.add(dd < 10 ? "0" : "").add(dd).add(".").add(mm < 10 ? "0" : "").add(mm);
        };
        auto redraw = [&]() {
            core::message sub; sub.add(core::schedule_total_days(g)).add(" dni, ").add(core::schedule_total_cost(g)).add(" tys. zł");
            phone_header(a, ph, t, "Twój dom", sub.s);
            phone_canvas& c = *ph.canvas;
            core::message r0; r0.add("Odbiór ");
            date(r0, end_day);
            r0.add(".").add(y);
            phone_text(a, t, list_x + 20, row_py(0), fit(a, r0.s, pill_room("Gotowe") - 20).c_str(), ink::dark);
            phone_pill(a, c, t, pill_end, row_ty(0), "Gotowe", pill::done);
            for(int r = 0; r < 4 && top + r < data::stages_count; ++r)
            {
                int i = top + r;
                stripe(c, r + 1, phone_tile::stripe_done);
                core::message sm; date(sm, core::schedule_start_day(g, i, end_day));
                sm.add(" ").add(data::stages[i].name);
                core::message cm; cm.add(data::stages[i].cost).add(" tys.");
                core::message dm; dm.add(core::schedule_days(g, i)).add(" dni");
                phone_text(a, t, list_x, row_py(r + 1), fit(a, sm.s, pill_room(cm.s) - a.text.width(dm.s) - 6).c_str(), ink::dark);
                phone_text(a, t, (pill_end - utf8_len(cm.s) - 1) * 8 - 4, row_py(r + 1), dm.s, ink::dim, 1);
                phone_pill(a, c, t, pill_end, row_ty(r + 1), cm.s, pill::done);
            }
            phone_text(a, t, list_x, row_py(5), "Twoja budowa:", ink::dim);
            phone_text(a, t, 226, row_py(5), data::schedule_url, ink::brand, 1);
            ph.commit();
        };
        redraw();
        bn::sound_items::sfx_notify.play(bn::fixed(0.7));
        wait_release();
        for(int f = 0; ! bn::keypad::a_pressed() && ! bn::keypad::start_pressed(); ++f)
        {
            int v = bn::keypad::up_pressed() ? -1 : (bn::keypad::down_pressed() ? 1 : 0);
            if(f > 0 && f % 150 == 0 && ! v) v = top + 4 < data::stages_count ? 1 : g.first_stage - top;   // sama przewija listę etapów
            if(v)
            {
                top = core::imax(int(g.first_stage), core::imin(data::stages_count - 4, top + v));
                redraw();
            }
            next_frame();
        }
        t.clear();
        a.text.set_palette_item(default_ink);
        wait_release();
    }

    // ------------------------------------------------------------------ podsumowanie budowy (#33): 3 strony w telefonie
    // 1 - co zatrzymało budowę (albo odbiór), ostatnie ciosy, najmocniejszy cios; 2 - oś czasu etapów (góra/dół);
    // 3 - nagrody z budowy, zlecenie, najbliższy cel i rada. A / prawo - dalej, B / lewo - wstecz, START - pomiń.
    ink recap_ink(uint8_t k) { return k == core::bad ? ink::late : (k == core::good ? ink::done : (k == core::loot ? ink::brand : ink::dark)); }

    void recap_pages(app& a, int gained)
    {
        core::game& g = *a.g;
        const bool won = g.st == core::status::won;
        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        phone_screen ph(2);
        ph.icon.set_visible(false);
        bn::sprite_palette_item default_ink = a.text.palette_item();
        page_sprites t;
        static core::recap_line lines[64];
        const int n_lines = g.recap_timeline(lines, 64);
        int page = 0, top = 0;
        auto draw = [&]() {
            phone_canvas& c = *ph.canvas;
            core::message sub; sub.add(page + 1).add("/3").add(page == 1 ? "  ^v  A: dalej" : "  A: dalej");
            static const char* titles[3] = { "Podsumowanie", "Oś czasu", "Nagrody i cele" };
            phone_header(a, ph, t, titles[page], sub.s);
            if(page == 0)
            {
                core::message w; g.recap_where(w);
                if(won)
                {
                    stripe(c, 0, phone_tile::stripe_done);
                    phone_text(a, t, list_x, row_py(0), "Odbiór zaliczony!", ink::done);
                    core::message dd; dd.add(core::schedule_total_days(g)).add(" dni");
                    phone_pill(a, c, t, pill_end, row_ty(0), dd.s, pill::done);
                }
                else
                {
                    stripe(c, 0, phone_tile::stripe_late);
                    core::message k; g.recap_killer(k);
                    phone_text(a, t, list_x, row_py(0), fit(a, k.s, phone_text_w).c_str(), ink::late);
                }
                phone_text(a, t, list_x, row_py(1), fit(a, data::stages[g.stage].name, pill_room(w.s)).c_str(), ink::dark);
                phone_pill(a, c, t, pill_end, row_ty(1), w.s, won ? pill::done : pill::late);
                int r = 2;
                if(won)
                {
                    core::message tot; tot.add("Usunięte ").add(g.kills).add(", elity ").add(int(g.elites_killed)).add(", kombinacje ").add(int(g.combos_run));
                    phone_text(a, t, list_x, row_py(r++), fit(a, tot.s, phone_text_w).c_str(), ink::dark);
                    if(g.worst_hit.amount > 0)
                    {
                        phone_text(a, t, list_x, row_py(r++), "Najmocniej oberwałeś:", ink::dim);
                        core::message wh; core::game::recap_hit_line(wh, g.worst_hit);
                        phone_text(a, t, list_x, row_py(r++), fit(a, wh.s, phone_text_w).c_str(), ink::dark);
                    }
                }
                else
                {
                    phone_text(a, t, list_x, row_py(r++), "Ostatnie ciosy:", ink::dim);
                    for(int i = 0; i < core::recap_hits_n && r < 5; ++i)
                    {
                        const core::recap_hit& h = g.last_hits[i];
                        if(h.amount <= 0) break;
                        if(i == 0) stripe(c, r, phone_tile::stripe_late);
                        core::message hm; core::game::recap_hit_line(hm, h);
                        phone_text(a, t, list_x, row_py(r++), fit(a, hm.s, phone_text_w).c_str(), i == 0 ? ink::late : ink::dark);
                    }
                }
                if(g.best_hit > 0)
                {
                    core::message bh; bh.add("Twój cios: ").add(int(g.best_hit)).add(g.best_hit_crit ? " (kryt) w " : " w ").add(data::enemies[g.best_hit_def].name);
                    phone_text(a, t, list_x, row_py(5), fit(a, bh.s, phone_text_w).c_str(), ink::brand);
                }
            }
            else if(page == 1)
            {
                for(int r = 0; r < 6 && top + r < n_lines; ++r)
                {
                    const core::recap_line& l = lines[top + r];
                    const bool head = l.text.s[0] != ' ';
                    if(head) stripe(c, r, l.ink == core::bad ? phone_tile::stripe_late : phone_tile::stripe_done);
                    const int room = l.tail.n ? pill_room(l.tail.s) : phone_text_w;
                    phone_text(a, t, list_x, row_py(r), fit(a, l.text.s, room).c_str(), recap_ink(l.ink));
                    if(l.tail.n) phone_pill(a, c, t, pill_end, row_ty(r), l.tail.s, pill::gray);
                }
            }
            else
            {
                core::message xm; xm.add("Dośw. +").add(gained).add(", Respekt +").add(g.respect);
                core::message rm; rm.add("masz ").add(int(a.save.respect));
                phone_text(a, t, list_x, row_py(0), fit(a, xm.s, pill_room(rm.s)).c_str(), ink::dark);
                phone_pill(a, c, t, pill_end, row_ty(0), rm.s, pill::brand);
                int ci = core::next_contract(a.save, g);
                if(ci >= 0)
                {
                    core::message cm; cm.add("Zlecenie ").add(data::contracts[ci].name);
                    core::message pm; pm.add(core::imin(data::contracts[ci].target, core::contract_progress(a.save, ci))).add("/").add(data::contracts[ci].target);
                    phone_text(a, t, list_x, row_py(1), fit(a, cm.s, pill_room(pm.s)).c_str(), ink::dark);
                    phone_pill(a, c, t, pill_end, row_ty(1), pm.s, pill::prog);
                }
                core::message lead, name;
                if(core::recap_goal(a.save, lead, name))
                {
                    phone_text(a, t, list_x, row_py(2), fit(a, lead.s, phone_text_w).c_str(), ink::dim);
                    stripe(c, 3, phone_tile::stripe_brand);
                    phone_text(a, t, list_x, row_py(3), fit(a, name.s, phone_text_w).c_str(), ink::brand);
                }
                const core::recap_tip_def& tip = data::recap_tips[core::recap_tip_index(g)];
                stripe(c, 4, phone_tile::stripe_prog); stripe(c, 5, phone_tile::stripe_prog);
                phone_text(a, t, list_x, row_py(4), fit(a, tip.lines[0], phone_text_w).c_str(), ink::prog);
                phone_text(a, t, list_x, row_py(5), fit(a, tip.lines[1], phone_text_w).c_str(), ink::prog);
            }
            ph.commit();
        };
        draw();
        wait_release();
        while(true)
        {
            if(bn::keypad::start_pressed()) break;
            if(bn::keypad::a_pressed() || bn::keypad::right_pressed())
            {
                if(page == 2) break;
                ++page; draw(); bn::sound_items::sfx_menu.play();
            }
            else if((bn::keypad::b_pressed() || bn::keypad::left_pressed()) && page > 0) { --page; draw(); bn::sound_items::sfx_menu.play(); }
            else if(page == 1 && bn::keypad::down_pressed() && top + 6 < n_lines) { ++top; draw(); }
            else if(page == 1 && bn::keypad::up_pressed() && top > 0) { --top; draw(); }
            next_frame();
        }
        t.clear();
        a.text.set_palette_item(default_ink);
        wait_release();
    }

    // ------------------------------------------------------------------ ekran końcowy z QR
    scene run_end(app& a)
    {
        core::game& g = *a.g;
        bool won = g.st == core::status::won;
        int prev_best = a.save.best;
        bool record = g.score > prev_best;
        if(g.score > a.save.best) a.save.best = g.score;
        bool daily_record = g.daily && core::record_daily(a.save, g.daily_day, g.score, won);   // codzienna budowa: wynik dnia
        int reward = won ? core::record_win(a.save) : -1;   // nagroda za odbiór: każda wygrana odblokowuje kolejną
        if(won) core::add_house(a.save, g);
        int stake = core::investor_stake(g.bonus.investor);
        bool stake_record = won && stake > 0 && stake > core::best_stake(a.save, g.cls);
        int badges_got = core::check_badges(a.save, g);   // katalog, narzędzia, Osiedle, liczniki zleceń, rekord stawki
        int contracts_got = core::check_contracts(a.save);
        int gained = core::bank_xp(a.save, g);
        bool weekly_record = g.weekly_week && core::record_weekly(a.save, g.weekly_week, g.score, won);   // wyzwanie tygodnia (#34)
        uint32_t story_got = core::story_check(a.save, &g);   // fabuła (#35): nowe wątki SMS za kamienie milowe
        bn::sram::write(a.save);
        clear_run(a);
        play_song(song::none);
        (won ? bn::sound_items::sfx_level : bn::sound_items::sfx_hurt).play();
        {
            core::message i1; i1.add("Wynik ").add(g.score).add("  Dośw. +").add(gained);
            core::message i2;
            if(g.daily) i2.add("Budowa dnia: ").add(daily_record ? "rekord dnia!" : "zapisana");
            else if(g.weekly_week) i2.add("Tydzień: ").add(weekly_record ? "rekord tygodnia!" : "zapisany");
            else if(reward >= 0) i2.add("Nagroda: ").add(data::rewards[reward].name).add("!");
            else if(stake > 0) i2.add("Stawka ").add(stake).add(stake_record ? " - rekord!" : "").add(won ? ", dom!" : "");
            else if(record && prev_best > 0) i2.add("Nowy rekord! (było ").add(prev_best).add(")");
            else i2.add(won ? "Dom na Osiedlu!" : "Dośw. zostaje");
            phone_message(a, won ? data::story_win : data::story_lose, won ? "Odbiór" : "Budowa", i1.s, ink::dark, fit(a, i2.s, 150).c_str());
            leave(scene::end);
            if(won) { house_page(a); leave(scene::end); }   // harmonogram domu w stylu aplikacji PlanBudowlany
            recap_pages(a, gained);   // podsumowanie budowy (#33)
            leave(scene::end);
        }
        // Co dała ta budowa (motywacja do kolejnej próby): rekord, najbliższe zlecenie, najbliższe do kupienia w Szkoleniach.
        bn::vector<core::message, 5> motiv;
        {
            core::message m;
            if(record && prev_best > 0) m.add("Nowy rekord: ").add(g.score).add("!");
            else m.add("Rekord: ").add(int(a.save.best));
            if(daily_record) { m = core::message(); m.add("Rekord dnia: ").add(g.score).add("!"); }
            if(weekly_record) { m = core::message(); m.add("Rekord tygodnia: ").add(g.score).add("!"); }
            motiv.push_back(m);
            int ci = core::next_contract(a.save, g);
            if(ci >= 0)
            {
                core::message cm; cm.add("Zlecenie ").add(data::contracts[ci].name).add(": ")
                                    .add(core::imin(data::contracts[ci].target, core::contract_progress(a.save, ci))).add("/").add(data::contracts[ci].target);
                motiv.push_back(cm);
            }
            core::message rm; rm.add("Respekt z budowy +").add(g.respect).add(", masz ").add(int(a.save.respect));
            motiv.push_back(rm);
            int nr = a.save.rewards;   // kolejna nagroda za odbiór
            if(nr < data::rewards_count)
            {
                core::message nm; nm.add("Za kolejny odbiór: ").add(data::rewards[nr].name);
                if(data::rewards[nr].kind == core::reward_kind::soon) nm = core::message(), nm.add(data::rewards[nr].name).add(" - wkrótce");
                motiv.push_back(nm);
            }
            int kind = -1, idx = -1, cost = core::next_unlock(a.save, kind, idx);
            if(cost >= 0)
            {
                core::message um;
                const char* name = kind == 0 ? data::upgrades[idx].name : (kind == 1 ? data::classes[idx].name
                                 : (kind == 2 ? data::weapons[data::tools[idx].weapon].name : (kind == 3 ? data::brigade[idx].name : "Trudny")));
                if(a.save.xp >= cost) um.add("Stać Cię: ").add(name).add(" (").add(cost).add(")");
                else um.add(name).add(": brakuje ").add(cost - int(a.save.xp)).add(" dośw.");
                motiv.push_back(um);
            }
        }

        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        bn::regular_bg_ptr bg = bn::regular_bg_items::end.create_bg(8, 48);
        bn::bg_palette_color_hbe_ptr gradient = make_gradient(bg, screen_info::end_bg_index);
        text_sprites t;
        a.text.set_center_alignment();
        a.text.generate(0, 30, won ? "ODBIÓR ZALICZONY!" : "BUDOWA WSTRZYMANA", t);
        core::message s; s.add("Wynik ").add(g.score).add("  Dośw. +").add(gained);
        a.text.generate(0, 46, s.s, t);
        const bool ngplus = won && ! g.daily && ! g.weekly_week;   // budowa dnia / tygodnia: bez kolejnej budowy (NG+)
        const char* keys = ngplus ? "A: kolejna  START: koniec" : "START: nowa budowa";
        text_sprites rot;   // wiersz na zmianę: motywacja i klawisze
        int rot_i = -1;
        if(stake > 0)   // tryb inwestora: stawka w wierszu na zmianę (w rogu zasłaniała kod QR)
        {
            core::message sm; sm.add("Stawka ").add(stake).add(stake_record ? " - rekord!" : "");
            motiv.push_back(sm);
        }
        push_banner banner;   // odznaki i zlecenia zdobyte na koniec budowy (np. Stały klient)
        if(reward >= 0)
        {
            core::message rt; rt.add("Nagroda: ").add(data::rewards[reward].name);
            if(utf8_len(rt.s) > 23) rt = core::message(), rt.add(data::rewards[reward].name).add("!");   // Akt 0: Papierologia
            banner.push(rt.s, data::rewards[reward].desc);
        }
        push_achievements(banner, badges_got, contracts_got);
        for(int i = 0; i < data::story_arc_count; ++i)   // fabuła: nowy wątek w Wiadomościach (telefon profilu, Osiedle)
            if((story_got >> i) & 1) banner.push("Nowa wiadomość", data::story_arc[i].name);
        for(int f = 0; ; ++f)
        {
            banner.update(a);
            int ri = (f / 120) % (motiv.size() + 1);
            if(ri != rot_i)
            {
                rot_i = ri;
                rot.clear();
                a.text.set_center_alignment();
                bn::sprite_palette_item ink0 = a.text.palette_item();
                if(ri < motiv.size()) a.text.set_palette_item(bn::sprite_palette_items::font_map_loot);
                a.text.generate(0, 62, ri < motiv.size() ? fit(a, motiv[ri].s, 232).c_str() : keys, rot);
                a.text.set_palette_item(ink0);
            }
            if(ngplus && bn::keypad::a_pressed()) { g.new_game_plus(); wait_release(); return leave(scene::game); }
            if(bn::keypad::start_pressed() || (! ngplus && bn::keypad::a_pressed())) { wait_release(); return leave(scene::shop); }
            next_frame();
        }
    }

    // ------------------------------------------------------------------ prolog: wjazd na działkę (pierwsza budowa)
    // Pickup wjeżdża na plac, bohater wysiada, kamera jedzie przez działkę z porozrzucanymi problemami budowy,
    // na koniec SMS od inwestorki. A/START pomija.
    scene run_prologue(app& a)
    {
        const core::game& g = *a.g;
        {
            bn::bg_palettes::set_transparent_color(bn::color(1, 1, 3));
            bn::camera_ptr cam = bn::camera_ptr::create(-136, 8 * 16 + 8 - 256);
            bn::bg_tiles::set_allow_offset(false);
            bn::unique_ptr<bg_map> map(new bg_map());
            for(int y = 0; y < core::map_h; ++y)   // plac: droga dojazdowa z lewej, płot u góry i u dołu
                for(int x = 0; x < core::map_w; ++x)
                {
                    int t = 0;
                    if(y >= 4 && y <= 13) t = 1;
                    else if((y == 3 || y == 14) && x >= 4) t = y == 3 ? 3 : 2;
                    for(int dy = 0; dy < 2; ++dy) for(int dx = 0; dx < 2; ++dx) map->set(x * 2 + dx, y * 2 + dy, t, 0);
                }
            bn::regular_bg_item item(bn::regular_bg_tiles_items::tiles, bn::bg_palette_items::stage_palettes_2, map->map_item);   // Fundamenty
            bn::regular_bg_ptr bg = item.create_bg(0, 0);
            bn::bg_tiles::set_allow_offset(true);
            bg.set_camera(cam);

            bn::sprite_ptr truck = bn::sprite_items::truck.create_sprite(world(-2, 9).x(), world(0, 9).y());
            truck.set_camera(cam);
            bn::sprite_ptr hero = bn::sprite_items::actors.create_sprite(world(7, 10), data::classes[g.cls].frame);
            hero.set_camera(cam);
            hero.set_visible(false);
            struct prop { int8_t def, x, y; };
            const prop props[] = { { int8_t(data::enemy_przeciek), 11, 6 }, { int8_t(data::enemy_zwarcie), 14, 11 },
                                   { int8_t(data::enemy_papierologia), 17, 7 }, { int8_t(data::enemy_plesn), 20, 11 },
                                   { int8_t(data::enemy_kornik), 22, 6 }, { int8_t(data::enemy_budzet), 25, 9 },
                                   { int8_t(data::enemy_ulewa), 27, 5 }, { int8_t(data::enemy_termin), 29, 10 } };
            constexpr int props_count = int(sizeof(props) / sizeof(props[0]));
            bn::vector<bn::sprite_ptr, props_count> enemies;
            bool alerted[props_count] = {};
            for(const prop& p : props)
            {
                bn::sprite_ptr sp = bn::sprite_items::actors.create_sprite(world(p.x, p.y), data::enemies[p.def].frame);
                sp.set_camera(cam);
                enemies.push_back(sp);
            }
            particle_pool dust(cam);
            text_sprites caption;
            a.text.set_bg_priority(0);
            auto show_caption = [&](const char* s) {
                caption.clear();
                a.text.set_center_alignment();
                a.text.set_palette_item(bn::sprite_items::font_8x16.palette_item());
                a.text.generate(0, 64, s, caption);
            };
            play_song(song::game);

            for(int t = 0; t < 420; ++t)
            {
                if(bn::keypad::a_pressed() || bn::keypad::start_pressed()) break;
                if(t < 80)   // wjazd pickupa z pyłem spod kół
                {
                    truck.set_x(world(-2, 9).x() + t * (world(5, 9).x() - world(-2, 9).x()) / 80);
                    truck.set_tiles(bn::sprite_items::truck.tiles_item(), (t / 4) & 1);
                    if(t % 5 == 0)
                        dust.spawn(truck.x() - 16, truck.y() + 6, bn::fixed(-0.5), bn::fixed(-0.2), 0, 20, particle_pool::dust, 3);
                }
                if(t == 90)
                {
                    hero.set_visible(true);
                    for(int k = -1; k <= 1; k += 2) dust.spawn(hero.x() + k * 4, hero.y() + 6, bn::fixed(k) / 3, bn::fixed(-0.3), 0, 16, particle_pool::dust, 3);
                    show_caption(data::prologue_captions[0]);
                }
                if(t >= 110 && t < 330)   // przejazd kamery przez działkę
                    cam.set_x(-136 + (t - 110) * (120 + 136) / 220);
                if(t == 220) show_caption(data::prologue_captions[1]);
                for(int i = 0; i < props_count; ++i)
                {
                    int base = data::enemies[props[i].def].frame;
                    enemies[i].set_tiles(bn::sprite_items::actors.tiles_item(), ((t / 20 + i) & 1) ? anim_b(base) : base);
                    if(! alerted[i] && enemies[i].x() < cam.x() + 90 && t > 110)
                    {
                        alerted[i] = true;
                        dust.spawn(enemies[i].x(), enemies[i].y() - 14, 0, bn::fixed(-0.15), 0, 50, particle_pool::alert);
                    }
                }
                dust.update();
                next_frame();
            }
            caption.clear();
            leave(scene::prologue);   // ściemnij plac przed wiadomością
        }
        core::set_flag(a.save, core::prologue_seen);
        bn::sram::write(a.save);
        phone_message(a, data::story_prologue, "Budowa", "Twój pierwszy plac budowy", ink::dim, data::classes[g.cls].name);
        if(! core::has_flag(a.save, core::help_seen)) { a.after_help = scene::game; return leave(scene::help); }
        return leave(scene::game);
    }

    // ------------------------------------------------------------------ Hurtownia między aktami (budżet budowy)
    scene run_hurtownia(app& a)
    {
        core::game& g = *a.g;
        phone_screen ph(4);
        bn::sprite_palette_item default_ink = a.text.palette_item();
        page_sprites t;
        int sel = 0, top = 0;
        const char* note = nullptr;
        bn::sound_items::sfx_notify.play(bn::fixed(0.7));
        auto redraw = [&]() {
            core::message sub; sub.add(g.cash).add(" zł, ");
            mats_line(sub, g);
            phone_header(a, ph, t, "Hurtownia", sub.s);
            phone_canvas& c = *ph.canvas;
            core::message b; b.add("Premia za akt ").add(g.act_numeral()).add(": +").add(g.act_bonus).add(" zł");
            phone_text(a, t, list_x, row_py(0), note ? note : b.s, note ? ink::brand : ink::done);
            for(int r = 0; r < 4 && top + r < data::hurtownia_count; ++r)   // 4 wiersze, przewijane
            {
                const core::shop_item_def& it = data::hurtownia[top + r];
                bool is_sel = top + r == sel;
                if(is_sel) stripe(c, r + 1, phone_tile::stripe_brand);
                phone_text(a, t, list_x, row_py(r + 1), clip(it.name, 16).c_str(), is_sel ? ink::brand : ink::dark);
                core::message pr;   // cena w zł albo w materiale (np. "4 Stal"); ulepszenie: zł (stal w opisie), "maks."
                if(it.effect == core::shop_effect::upgrade && g.weapon_lvl >= data::tool_upgrade_max) pr.add("maks.");
                else if(it.material >= 0) pr.add(it.mat_cost).add(" ").add(data::materials[it.material].short_name);
                else pr.add(g.hurtownia_price(top + r)).add(" zł");
                phone_pill(a, c, t, pill_end, row_ty(r + 1), pr.s, g.hurtownia_can(top + r) ? (it.material >= 0 ? pill::prog : pill::group) : pill::gray);
            }
            core::message dm; ink dk = ink::dim;   // opis; ulepszenie: poziom i koszt, nowe narzędzie: ostrzeżenie (#31)
            const core::shop_item_def& si = data::hurtownia[sel];
            if(si.effect == core::shop_effect::upgrade)
            {
                g.weapon_title(dm);
                if(g.weapon_lvl >= data::tool_upgrade_max) dm.add(": maks. ulepszenie");
                else { dm.add(" -> +").add(g.weapon_lvl + 1).add(": "); core::tool_level_label(dm, g.weapon_lvl, g.upgrade_price()); }
                dk = ink::brand;
            }
            else if(si.effect == core::shop_effect::tool && g.weapon_lvl > 0)
            {
                dm.add("Uwaga: "); g.weapon_title(dm).add(" przepadnie!");
                dk = ink::late;
            }
            else dm.add(si.desc);
            phone_text(a, t, list_x, row_py(5), fit(a, dm.s, phone_text_w).c_str(), dk);
            ph.commit();
        };
        redraw();
        wait_release();
        while(true)
        {
            int n = data::hurtownia_count;
            int dir = bn::keypad::up_pressed() ? -1 : (bn::keypad::down_pressed() ? 1 : 0);
            if(dir)
            {
                sel = (sel + dir + n) % n;
                if(sel < top) top = sel;
                if(sel >= top + 4) top = sel - 3;
                note = nullptr; redraw(); bn::sound_items::sfx_menu.play();
            }
            if(bn::keypad::a_pressed())
            {
                const bool up = data::hurtownia[sel].effect == core::shop_effect::upgrade;
                if(g.hurtownia_buy(sel))
                {
                    bn::sound_items::sfx_buy.play();
                    note = up ? "Ulepszone! START: dalej" : "Kupione! START: dalej";
                    if(g.trait_pending) trait_loop(a, ph, t);   // +2: wybór cechy narzędzia
                }
                else if(up) note = g.weapon_lvl >= data::tool_upgrade_max ? "Maksymalne ulepszenie" : "Za mało zł albo stali";
                else note = data::hurtownia[sel].material >= 0 ? "Za mało materiału" : "Za mały budżet";
                redraw();
            }
            if(bn::keypad::b_pressed() || bn::keypad::start_pressed())
            {
                t.clear();
                a.text.set_palette_item(default_ink);
                wait_release();
                advance_stage(a);
                return leave(scene::game);
            }
            next_frame();
        }
    }

    // ------------------------------------------------------------------ codzienna budowa (R na tytule)
    // GBA nie ma zegara: gracz ustawia datę (pamiętana w profilu). Seed, zawód i modyfikatory dnia z daty - dla
    // wszystkich takie same; najlepsze wyniki ostatnich dni w profilu. Lewo/prawo - pole daty, góra/dół - zmiana, A - start.
    scene run_daily(app& a)
    {
        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        phone_screen ph(0);
        ph.icon.set_visible(false);
        bn::sprite_palette_item default_ink = a.text.palette_item();
        page_sprites t;
        bn::sprite_ptr cal = bn::sprite_items::menu_icons.create_sprite(14 + 8 - 120, row_py(0) + 8 - 80, 14);   // kalendarz
        cal.set_bg_priority(1);
        int y, m, d;
        core::daily_date(a.save, y, m, d);
        int field = 0;   // 0 dzień, 1 miesiąc, 2 rok
        auto redraw = [&]() {
            int day = core::daily_number(y, m, d);
            uint32_t seed = core::daily_seed(day);
            core::message sub; sub.add("nr ").add(day).add(", A: start");
            phone_header(a, ph, t, "Budowa dnia", sub.s);
            phone_canvas& c = *ph.canvas;
            phone_text(a, t, list_x + 18, row_py(0), "Data:", ink::dim);
            core::message dd, mm, yy; dd.add(d < 10 ? "0" : "").add(d); mm.add(m < 10 ? "0" : "").add(m); yy.add(y);
            phone_pill(a, c, t, 14, row_ty(0), dd.s, field == 0 ? pill::brand : pill::group);
            phone_pill(a, c, t, 18, row_ty(0), mm.s, field == 1 ? pill::brand : pill::group);
            phone_pill(a, c, t, 24, row_ty(0), yy.s, field == 2 ? pill::brand : pill::group);
            phone_text(a, t, 226, row_py(0), "<>^v", ink::dim, 1);
            core::message cl; cl.add("Zawód dnia: ").add(data::classes[core::daily_class(seed)].name);
            phone_text(a, t, list_x, row_py(1), fit(a, cl.s, phone_text_w).c_str(), ink::dark);
            int mask = core::daily_investor(seed), row = 2;   // modyfikatory dnia: po jednym w wierszu (2-3)
            for(int i = 0; i < data::investor_count && row < 4; ++i)
                if((mask >> i) & 1)
                {
                    stripe(c, row, phone_tile::stripe_late);
                    core::message mo; mo.add("Dziś: ").add(data::investor[i].name);
                    phone_text(a, t, list_x, row_py(row++), fit(a, mo.s, phone_text_w).c_str(), ink::late);
                }
            if(row == 2) phone_text(a, t, list_x, row_py(row++), "Dziś bez utrudnień", ink::dim);
            int best = core::daily_best(a.save, day);
            core::message bm;
            if(best >= 0) bm.add("Twój wynik dnia: ").add(best);
            else bm.add("Dziś jeszcze bez wyniku");
            bool won_day = best >= 0 && core::daily_won(a.save, day);
            phone_text(a, t, list_x, row_py(4), fit(a, bm.s, won_day ? pill_room("Wygrana") : phone_text_w).c_str(), ink::dark);
            if(won_day) phone_pill(a, c, t, pill_end, row_ty(4), "Wygrana", pill::done);
            core::message hist; hist.add("Ostatnio: ");   // inne dni z profilu, od najnowszego
            int shown = 0, last = 1 << 20;
            for(int k = 0; k < data::daily_history; ++k)
            {
                int pick = -1;
                for(int i = 0; i < data::daily_history; ++i)
                    if(a.save.daily_day[i] > 0 && a.save.daily_day[i] != day && a.save.daily_day[i] < last && (pick < 0 || a.save.daily_day[i] > a.save.daily_day[pick])) pick = i;
                if(pick < 0) break;
                last = a.save.daily_day[pick];
                int py, pm, pd; core::civil_from_days(core::days_from_civil(data::daily_epoch[0], data::daily_epoch[1], data::daily_epoch[2]) + last - 1, py, pm, pd);
                hist.add(shown ? ", " : "").add(pd).add(".").add(pm < 10 ? "0" : "").add(pm).add(" ").add(int(a.save.daily_score[pick]));
                ++shown;
            }
            if(! shown) hist = core::message().add("SELECT: wyzwanie tygodnia");
            phone_text(a, t, list_x, row_py(5), fit(a, hist.s, phone_text_w).c_str(), ink::dim);
            ph.commit();
        };
        redraw();
        wait_release();
        auto finish = [&]() {
            core::set_daily_date(a.save, y, m, d);
            bn::sram::write(a.save);
            t.clear();
            a.text.set_palette_item(default_ink);
            wait_release();
        };
        while(true)
        {
            int h = bn::keypad::left_pressed() ? -1 : (bn::keypad::right_pressed() ? 1 : 0);
            if(h) { field = (field + h + 3) % 3; redraw(); bn::sound_items::sfx_menu.play(); }
            int v = bn::keypad::up_pressed() ? 1 : (bn::keypad::down_pressed() ? -1 : 0);
            if(v)
            {
                if(field == 0) d = (d - 1 + v + core::days_in_month(y, m)) % core::days_in_month(y, m) + 1;
                else if(field == 1) m = (m - 1 + v + 12) % 12 + 1;
                else y = core::imax(data::daily_epoch[0], core::imin(2099, y + v));
                d = core::imin(d, core::days_in_month(y, m));
                redraw(); bn::sound_items::sfx_menu.play();
            }
            if(bn::keypad::a_pressed() || bn::keypad::start_pressed())
            {
                finish();
                core::start_daily(*a.g, core::daily_number(y, m, d));
                core::start_run(a.save);
                bn::sram::write(a.save);
                return leave(scene::game);
            }
            if(bn::keypad::b_pressed()) { finish(); return leave(scene::title); }
            if(bn::keypad::select_pressed()) { finish(); return leave(scene::weekly); }   // wyzwanie tygodnia z tej samej daty
            next_frame();
        }
    }

    // ------------------------------------------------------------------ wyzwanie tygodnia (#34, L na tytule)
    // Tydzień z daty budowy dnia (GBA bez zegara; góra/dół = tydzień dalej / wcześniej). Seed z numeru tygodnia, zasady
    // z listy po kolei, osobne wyniki ostatnich tygodni w profilu. A - start, SELECT - budowa dnia, B - tytuł.
    scene run_weekly(app& a)
    {
        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        phone_screen ph(0);
        ph.icon.set_visible(false);
        bn::sprite_palette_item default_ink = a.text.palette_item();
        page_sprites t;
        bn::sprite_ptr cal = bn::sprite_items::menu_icons.create_sprite(14 + 8 - 120, row_py(0) + 8 - 80, 14);   // kalendarz
        cal.set_bg_priority(1);
        int y, m, d;
        core::daily_date(a.save, y, m, d);
        int week = core::weekly_number(y, m, d);
        auto redraw = [&]() {
            const int wi = core::weekly_index(week);
            const core::weekly_def& wd = data::weekly[wi];
            core::message title; title.add("Tydzień nr ").add(week);
            phone_header(a, ph, t, title.s, "A: start");
            phone_canvas& c = *ph.canvas;
            int fy, fm, fd, ly, lm, ld;
            core::civil_from_days(core::weekly_first_day(week), fy, fm, fd);
            core::civil_from_days(core::weekly_first_day(week) + 6, ly, lm, ld);
            core::message dm; dm.add(fd).add(".").add(fm < 10 ? "0" : "").add(fm).add(" - ").add(ld).add(".").add(lm < 10 ? "0" : "").add(lm).add(".").add(ly);
            phone_text(a, t, list_x + 18, row_py(0), dm.s, ink::dark);
            phone_text(a, t, 226, row_py(0), "^v", ink::dim, 1);
            stripe(c, 1, phone_tile::stripe_late);
            phone_text(a, t, list_x, row_py(1), fit(a, wd.name, phone_text_w).c_str(), ink::late);
            phone_text(a, t, list_x, row_py(2), fit(a, wd.desc[0], phone_text_w).c_str(), ink::dark);
            phone_text(a, t, list_x, row_py(3), fit(a, wd.desc[1], phone_text_w).c_str(), ink::dark);
            core::message cl; cl.add("Zawód: ").add(data::classes[core::weekly_class(week)].name);
            int best = core::weekly_best(a.save, week);
            core::message bm;
            if(best >= 0) bm.add("Wynik: ").add(best);
            else bm.add("Bez wyniku");
            phone_text(a, t, list_x, row_py(4), fit(a, cl.s, 120).c_str(), ink::dim);
            const bool won_week = best >= 0 && core::weekly_won(a.save, week);
            phone_pill(a, c, t, pill_end, row_ty(4), bm.s, won_week ? pill::done : (best >= 0 ? pill::prog : pill::gray));
            core::message hist; hist.add("Ostatnio: ");   // inne tygodnie z profilu
            int shown = 0;
            for(int i = 0; i < data::weekly_history; ++i)
                if(a.save.weekly_week[i] > 0 && a.save.weekly_week[i] != week)
                {
                    hist.add(shown ? ", " : "").add("t.").add(int(a.save.weekly_week[i])).add(" ").add(int(a.save.weekly_score[i]));
                    ++shown;
                }
            if(! shown) hist = core::message().add("SELECT: budowa dnia");
            phone_text(a, t, list_x, row_py(5), fit(a, hist.s, phone_text_w).c_str(), ink::dim);
            ph.commit();
        };
        redraw();
        wait_release();
        auto finish = [&]() {
            if(core::weekly_number(y, m, d) != week) core::civil_from_days(core::weekly_first_day(week), y, m, d);   // inny tydzień: jego poniedziałek
            core::set_daily_date(a.save, y, m, d);   // data wspólna z budową dnia
            bn::sram::write(a.save);
            t.clear();
            a.text.set_palette_item(default_ink);
            wait_release();
        };
        while(true)
        {
            int v = bn::keypad::up_pressed() ? 1 : (bn::keypad::down_pressed() ? -1 : 0);
            if(v && week + v >= 1 && core::weekly_first_day(week + v) <= core::days_from_civil(2099, 12, 25))
            {
                week += v;
                redraw(); bn::sound_items::sfx_menu.play();
            }
            if(bn::keypad::a_pressed() || bn::keypad::start_pressed())
            {
                finish();
                core::start_weekly(*a.g, week);
                core::start_run(a.save);
                bn::sram::write(a.save);
                return leave(scene::game);
            }
            if(bn::keypad::b_pressed()) { finish(); return leave(scene::title); }
            if(bn::keypad::select_pressed()) { finish(); return leave(scene::daily); }
            next_frame();
        }
    }

    // ------------------------------------------------------------------ telefon profilu (z tytułu i po budowie)
    // Te same ikony zakładek, inne treści: Odznaki, Katalog usterek, Osiedle, Zespół, Koszty (sklep Szkolenia).
    // ------------------------------------------------------------------ wybór zawodu
    // U góry pasek portretów (odblokowane zawody najpierw, potem zablokowane jako sylwetki z kłódką),
    // pod nim karta wybranego zawodu (moc, broń, statystyki, trudność), na dole pamiątka.
    // Strzałki lewo/prawo przechodzą po wszystkich; zablokowany można obejrzeć, ale nie wystartować.
    namespace class_ui
    {
        constexpr int oy = 2;                       // cała warstwa kafli przesunięta 2 px w dół
        constexpr int card_ty = 5, card_th = 11;    // karta: kafle 1..28 x 5..15 (y 42..130)
        constexpr int row1 = 50, row2 = 66, row3 = 82, row4 = 98, row5 = 113;   // wiersze tekstu karty (px)
        constexpr int keep_row = 130, keep_row2 = 144;                          // pod kartą
        constexpr int slot_w = 40;                  // 6 portretów po 40 px = cały ekran (więcej zawodów: pasek się przewija)
        constexpr int slots_visible = 6;
        constexpr int small_cy = 22, big_cy = 18;   // środki portretów (px)
        constexpr int frame_small_lock = 4, frame_arrows = 5;   // klatki menu_icons
        constexpr int slide_px = 12, slide_step = 3;

        int slot_cx(int s) { return s * slot_w + slot_w / 2; }
    }

    scene run_class_select(app& a)
    {
        using namespace class_ui;
        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        const bn::sprite_palette_item default_ink = a.text.palette_item();
        const int default_prio = a.text.bg_priority();
        a.text.set_bg_priority(1);   // nad kartą (warstwa kafli ma priorytet 2)

        // kolejność na pasku: odblokowane (jak w danych), potem zablokowane
        int order[data::classes_count], count = 0;
        for(int pass = 0; pass < 2; ++pass)
            for(int i = 0; i < data::classes_count; ++i)
                if(core::class_unlocked(a.save, i) == (pass == 0)) order[count++] = i;
        auto slot_of = [&](int cls) { for(int s = 0; s < count; ++s) if(order[s] == cls) return s; return 0; };
        if(! core::class_unlocked(a.save, a.chosen_class)) a.chosen_class = order[0];   // startowe zawody zawsze są
        int sel = slot_of(a.chosen_class);
        int first = core::imax(0, core::imin(sel - 2, count - slots_visible));   // pierwszy widoczny portret na pasku

        bn::unique_ptr<phone_canvas> canvas(new phone_canvas());
        bn::regular_bg_ptr bg = make_canvas_bg(*canvas);
        bg.set_y(bg.y() + oy);
        bg.set_priority(2);
        bn::regular_bg_map_ptr map = bg.map();

        // portrety: mały na każdym miejscu, duży (x2) na wybranym
        bn::vector<bn::sprite_ptr, data::classes_count> small, locks;
        for(int s = 0; s < count; ++s)
        {
            int c = order[s];
            bool unl = core::class_unlocked(a.save, c);
            bn::sprite_ptr p = bn::sprite_items::actors.create_sprite(slot_cx(s) - 120, small_cy - 80 + oy,
                                                                       unl ? data::classes[c].frame : silhouette_frame(c));
            p.set_bg_priority(1);
            small.push_back(bn::move(p));
            if(! unl)
            {
                bn::sprite_ptr l = bn::sprite_items::menu_icons.create_sprite(slot_cx(s) - 120 + 6, small_cy - 80 + oy + 5, frame_small_lock);
                l.set_bg_priority(1);
                l.set_z_order(-1);
                locks.push_back(bn::move(l));
            }
        }
        bn::sprite_ptr big = bn::sprite_items::actors.create_sprite(0, 0, 0);
        big.set_double_size_mode(bn::sprite_double_size_mode::ENABLED);
        big.set_bg_priority(1);
        big.set_z_order(-2);
        big.set_scale(2);
        bn::sprite_ptr big_lock = bn::sprite_items::menu_icons.create_sprite(0, 0, frame_small_lock);
        big_lock.set_bg_priority(1);
        big_lock.set_z_order(-3);
        bn::sprite_ptr ability = bn::sprite_items::ability_icons.create_sprite(18 - 120, row2 + 8 - 80, 0);
        ability.set_bg_priority(1);
        bn::sprite_ptr arrows = bn::sprite_items::menu_icons.create_sprite(0, 0, frame_arrows);
        arrows.set_bg_priority(1);
        bn::sprite_ptr info = bn::sprite_items::menu_icons.create_sprite(220 - 120, row3 + 8 - 80 + oy, icon_stats);   // START: opis statystyk
        info.set_bg_priority(1);

        page_sprites card_t;          // tekst karty (przesuwa się przy zmianie zawodu)
        text_sprites pill_t, keep_t;  // napisy na pastylce (kafle stoją) i pod kartą

        // tekst w px ekranu (lewy górny róg linii 16 px); zwraca szerokość
        auto put = [&](auto& t, int px, int py, const char* s, const bn::sprite_palette_item& pal, int align = -1) {
            a.text.set_palette_item(pal);
            if(align < 0) a.text.set_left_alignment();
            else if(align > 0) a.text.set_right_alignment();
            else a.text.set_center_alignment();
            a.text.generate(px - 120, py - 72, s, t);
            return a.text.width(s);
        };
        auto pill_at = [&](int tx_end, int ty, const char* s, pill p) {   // jak phone_pill, z przesunięciem oy
            int tw = utf8_len(s) + 1, tx = tx_end - tw;
            int fill = phone_tile::fill_gray, corner = phone_tile::corner_gray;
            ink i = ink::dim;
            if(p == pill::done) { fill = phone_tile::fill_done_bg; corner = phone_tile::corner_done_bg; i = ink::done; }
            else if(p == pill::late) { fill = phone_tile::fill_late_bg; corner = phone_tile::corner_late_bg; i = ink::late; }
            else if(p == pill::group) { fill = phone_tile::fill_group; corner = phone_tile::corner_group; i = ink::brand; }
            canvas->rounded(tx, ty, tw, 2, fill, corner);
            put(pill_t, tx * 8 + tw * 4, ty * 8 + oy, s, ink_palette(i), 0);
            return tx;
        };

        int clock = 0, pop = 0, slide = 0, shake = 0;

        auto draw_strip = [&]() {
            if(sel < first) first = sel;
            if(sel >= first + slots_visible) first = sel - slots_visible + 1;
            for(int ty = 0; ty < card_ty; ++ty) for(int tx = 0; tx < 30; ++tx) canvas->set(tx, ty, phone_tile::empty);
            int li = 0;
            for(int s = 0; s < count; ++s)
            {
                bool unl = core::class_unlocked(a.save, order[s]), shown = s >= first && s < first + slots_visible;
                int v = s - first;
                if(shown && s == sel) canvas->rounded(v * 5, 0, 5, 4, phone_tile::fill_brand, phone_tile::corner_brand);
                else if(shown) canvas->rounded(v * 5 + 1, 1, 3, 3, unl ? phone_tile::fill_group : phone_tile::fill_gray,
                                               unl ? phone_tile::corner_group : phone_tile::corner_gray);
                small[s].set_position(slot_cx(v) - 120, small_cy - 80 + oy);
                small[s].set_visible(shown && s != sel);
                if(! unl)
                {
                    locks[li].set_position(slot_cx(v) - 120 + 6, small_cy - 80 + oy + 5);
                    locks[li++].set_visible(shown && s != sel);
                }
            }
            if(first > 0) canvas->set(0, 2, phone_tile::line_h);                          // za paskiem są jeszcze zawody
            if(first + slots_visible < count) canvas->set(29, 2, phone_tile::line_h);
            const int c = order[sel];
            bool unl = core::class_unlocked(a.save, c);
            big.set_tiles(bn::sprite_items::actors.tiles_item(), unl ? data::classes[c].frame : silhouette_frame(c));
            big.set_position(slot_cx(sel - first) - 120, big_cy - 80 + oy);   // też przed pętlą (samouczek), inaczej środek ekranu pod kartą
            big_lock.set_position(slot_cx(sel - first) - 120 + 12, big_cy - 80 + oy + 8);
            big_lock.set_visible(! unl);
        };

        auto draw_diff = [&](bool locked) {   // pastylka trudności w pierwszym wierszu karty (albo "Zablokowany")
            for(int tx = 14; tx < 28; ++tx) { canvas->set(tx, 6, phone_tile::fill_card); canvas->set(tx, 7, phone_tile::fill_card); }
            pill_t.clear();
            if(locked) { pill_at(28, 6, "Zablokowany", pill::late); arrows.set_visible(false); return; }
            int d = a.chosen_diff;
            pill p = d == 0 ? pill::done : (d == data::difficulties_count - 1 ? pill::late : pill::group);
            int tx = pill_at(28, 6, data::difficulties[d].name, p);
            arrows.set_position(tx * 8 - 8 - 120, row1 + 8 - 80);
            arrows.set_visible(true);
        };

        auto draw_card = [&]() {
            card_t.clear();
            const int ci = order[sel];
            const core::class_def& c = data::classes[ci];
            const core::weapon_def& w = data::weapons[c.weapon];
            bool locked = ! core::class_unlocked(a.save, ci);
            put(card_t, 16, row1, c.name, ink_palette(ink::dark));
            draw_diff(locked);
            // moc: ikona, nazwa (fiolet) i jednolinijkowy opis
            ability.set_tiles(bn::sprite_items::ability_icons.tiles_item(), ci);
            ability.set_position(18 - 120, row2 + 8 - 80);
            int x = 29 + put(card_t, 29, row2, c.ability_name, ink_palette(ink::brand));
            put(card_t, x + 4, row2, fit(a, c.ability_desc, 229 - x - 4).c_str(), ink_palette(ink::dim));
            // broń: nazwa i zakres ciosu z premiami profilu, kryt (rozpiska #26), zasięg, statystyka skalowania (fiolet);
            // co się nie mieści, skraca się ("zasięg" -> "z.", "kryt" -> "kr", bez zasięgu)
            const core::dmg_breakdown bd = core::class_breakdown(ci, core::mods(a.save));
            core::message wn; wn.add(w.name).add(" "); core::add_range(wn, bd.min, bd.max);
            core::message tag; tag.add("(").add(stat_short(w.scales_with)).add(")");
            core::message ex[4];
            core::add_range(ex[0].add(", kryt "), bd.crit_min, bd.crit_max).add(" (").add(bd.crit_chance()).add("%), zasięg ").add(w.range);
            core::add_range(ex[1].add(", kryt "), bd.crit_min, bd.crit_max).add(" (").add(bd.crit_chance()).add("%), z. ").add(w.range);
            core::add_range(ex[2].add(", kr "), bd.crit_min, bd.crit_max).add(" ").add(bd.crit_chance()).add("%, z. ").add(w.range);
            core::add_range(ex[3].add(", kr "), bd.crit_min, bd.crit_max).add(" ").add(bd.crit_chance()).add("%");
            int room = 212 - 16 - a.text.width(wn.s) - 4 - a.text.width(tag.s), e = 0;   // do przycisku "i" (opis statystyk)
            while(e < 3 && a.text.width(ex[e].s) > room) ++e;
            x = 16 + put(card_t, 16, row3, wn.s, ink_palette(ink::dark));
            x += put(card_t, x, row3, fit(a, ex[e].s, room).c_str(), ink_palette(ink::dim));
            put(card_t, x + 4, row3, tag.s, ink_palette(ink::brand));
            // statystyki efektywne: zawód + Szkolenia, uprawnienia, pamiątka - "baza+premia"
            const core::run_mods m = core::mods(a.save);
            struct cell { const char* label; int base, bonus; bool weapon; };
            const cell cells[6] = {
                { "HP", c.max_health, m.hp, false },
                { "SIŁ", c.strength, core::mods_stat_bonus(m, ci, core::stat::str), w.scales_with == core::stat::str },
                { "ZRĘ", c.agility, core::mods_stat_bonus(m, ci, core::stat::agi), w.scales_with == core::stat::agi },
                { "INT", c.intelligence, core::mods_stat_bonus(m, ci, core::stat::intel), w.scales_with == core::stat::intel },
                { "OBR", c.defense, m.def, false },
                { "SZCZ", c.luck, m.luck, false } };
            for(int i = 0; i < 6; ++i)
            {
                const cell& ce = cells[i];
                int cx = 16 + (i % 3) * 72, cy = i < 3 ? row4 : row5;
                cx += put(card_t, cx, cy, ce.label, ink_palette(ce.weapon ? ink::brand : ink::dim)) + 4;
                core::message v; v.add(ce.base);
                cx += put(card_t, cx, cy, v.s, ink_palette(ink::dark));
                if(ce.bonus > 0) { core::message b; b.add("+").add(ce.bonus); put(card_t, cx, cy, b.s, ink_palette(ink::done)); }
            }
        };

        auto draw_keep = [&]() {   // pod kartą: pamiątka (L/R) albo jak odblokować zawód
            keep_t.clear();
            const int ci = order[sel];
            if(! core::class_unlocked(a.save, ci) && core::class_reward(ci))   // zawód z nagrody za odbiór
            {
                int ri = 0;
                while(ri < data::rewards_count && ! (data::rewards[ri].kind == core::reward_kind::cls && data::rewards[ri].index == ci)) ++ri;
                put(keep_t, 8, keep_row, "Nagroda za odbiór budowy", bn::sprite_items::font_8x16.palette_item());
                core::message m; m.add("Za ").add(core::reward_win(a.save, ri)).add(". wygraną, masz ").add(int(a.save.wins));
                put(keep_t, 8, keep_row2, m.s, bn::sprite_palette_items::font_map_loot);
                return;
            }
            if(! core::class_unlocked(a.save, ci))
            {
                put(keep_t, 8, keep_row, "Odblokuj w Kosztach (telefon)", bn::sprite_items::font_8x16.palette_item());
                core::message m; m.add("Koszt ").add(data::class_cost).add(" dośw., masz ").add(int(a.save.xp));
                put(keep_t, 8, keep_row2, m.s, a.save.xp >= data::class_cost ? bn::sprite_palette_items::font_map_good
                                                                              : bn::sprite_palette_items::font_map_loot);
                return;
            }
            int x = 8 + put(keep_t, 8, keep_row, "Pamiątka L/R: ", bn::sprite_items::font_8x16.palette_item());
            int k = core::selected_keepsake(a.save);
            if(k < 0)
            {
                put(keep_t, x, keep_row, "brak", bn::sprite_palette_items::font_map_loot);
                put(keep_t, 8, keep_row2, "więcej pamiątek w Zleceniach", bn::sprite_palette_items::font_map_good);
                return;
            }
            core::message n; n.add(data::keepsakes[k].name).add(" ").add(roman(core::keepsake_rank(a.save, k) - 1));
            put(keep_t, x, keep_row, fit(a, n.s, 232 - x).c_str(), bn::sprite_palette_items::font_map_loot);
            core::message e; core::perk_label(e, core::keepsake_perk(a.save, k));
            bool inv = core::investor_unlocked(a.save);
            put(keep_t, 8, keep_row2, fit(a, e.s, inv ? 128 : 224).c_str(), bn::sprite_palette_items::font_map_good);
            if(inv)   // tryb inwestora: stawka (SELECT - modyfikatory)
            {
                core::message sm; sm.add("SELECT: stawka ").add(core::investor_stake(core::investor_mask(a.save)));
                put(keep_t, 232, keep_row2, sm.s, bn::sprite_palette_items::font_map_loot, 1);
            }
        };

        auto redraw_all = [&]() {
            canvas->clear();
            canvas->rounded(1, card_ty, 28, card_th, phone_tile::fill_card, phone_tile::corner_card);
            draw_strip();
            draw_card();
            draw_keep();
            map.reload_cells_ref();
        };
        auto move_card = [&](int dx) {
            for(bn::sprite_ptr& s : card_t) s.set_x(s.x() + dx);
            ability.set_x(ability.x() + dx);
        };
        auto open_stats = [&]() {   // opis statystyk wybranego zawodu (co daje każda i wzory)
            card_t.clear(); pill_t.clear(); keep_t.clear();
            bg.set_visible(false);
            for(bn::sprite_ptr& sp : small) sp.set_visible(false);
            for(bn::sprite_ptr& sp : locks) sp.set_visible(false);
            big.set_visible(false); big_lock.set_visible(false); ability.set_visible(false); arrows.set_visible(false); info.set_visible(false);
            {
                phone_screen ph(2);
                ph.icon.set_visible(false);
                page_sprites st;
                stats_page(a, ph, st, nullptr, order[sel], core::mods(a.save));
            }
            a.text.set_bg_priority(1);
            bg.set_visible(true); big.set_visible(true); ability.set_visible(true); info.set_visible(true);
            redraw_all();
        };
        redraw_all();
        {   // samouczek menu: pierwsze wejście - dymki po kolei; potem nowości (tryb inwestora, nowy zawód z nagrody)
            auto hole = [](const char* id) {
                auto is = [&](const char* k) { return same(id, k); };
                if(is("difficulty")) return coach_hole{ 13, 5, 16, 4, false };
                if(is("keepsake")) return coach_hole{ 0, 16, 30, 4, true };
                if(is("stats")) return coach_hole{ 1, 10, 28, 6, true };
                if(is("investor")) return coach_hole{ 14, 17, 16, 3, true };
                return coach_hole{ 0, 0, 30, 5, false };   // zawód, na plac, nowy zawód: pasek portretów
            };
            if(core::tutorial_pending(a.save, 1)) run_tutorial(a, 1, hole, open_stats);
            run_unlocks(a, 1, hole);
            a.text.set_bg_priority(1);
        }

        while(true)
        {
            ++clock;
            int ddir = bn::keypad::up_pressed() ? -1 : (bn::keypad::down_pressed() ? 1 : 0);
            if(ddir)
            {
                do a.chosen_diff = (a.chosen_diff + ddir + data::difficulties_count) % data::difficulties_count;
                while(! core::difficulty_unlocked(a.save, a.chosen_diff));
                if(core::class_unlocked(a.save, order[sel])) { draw_diff(false); map.reload_cells_ref(); }
                bn::sound_items::sfx_menu.play();
            }
            int kdir = bn::keypad::r_pressed() ? 1 : (bn::keypad::l_pressed() ? -1 : 0);
            if(kdir)
            {
                core::cycle_keepsake(a.save, kdir);
                draw_card(); draw_keep(); map.reload_cells_ref();   // pamiątka zmienia statystyki
                bn::sound_items::sfx_menu.play();
            }
            int cdir = bn::keypad::right_pressed() ? 1 : (bn::keypad::left_pressed() ? -1 : 0);
            if(cdir)
            {
                sel = (sel + cdir + count) % count;
                a.chosen_class = order[sel];
                redraw_all();
                slide = cdir * slide_px; move_card(slide);   // karta wjeżdża z kierunku ruchu
                pop = 4;
                bn::sound_items::sfx_menu.play();
            }
            if(bn::keypad::start_pressed())   // opis statystyk wybranego zawodu (co daje każda i wzory)
            {
                open_stats();
                next_frame();
                continue;
            }
            if(bn::keypad::a_pressed())
            {
                if(core::class_unlocked(a.save, a.chosen_class))
                {
                    a.text.set_palette_item(default_ink);
                    a.text.set_bg_priority(default_prio);
                    a.g->new_run(a.chosen_class, a.seed_counter * 2654435761u + 12345u, a.chosen_diff, core::mods(a.save));
#ifdef PB_SCENARIO
                    debug_scenario::apply(*a.g, PB_SCENARIO);
#endif
                    core::start_run(a.save);   // licznik budów i budów z pamiątką (ranga)
                    bn::sram::write(a.save);
                    wait_release();
                    if(! core::has_flag(a.save, core::prologue_seen)) return leave(scene::prologue);
                    if(! core::has_flag(a.save, core::help_seen)) { a.after_help = scene::game; return leave(scene::help); }
                    return leave(scene::game);
                }
                shake = 12;   // zablokowany: portret kręci głową
                bn::sound_items::sfx_hurt.play();
            }
            if(bn::keypad::b_pressed())
            {
                a.text.set_palette_item(default_ink);
                a.text.set_bg_priority(default_prio);
                wait_release();
                return leave(scene::title);
            }
            if(bn::keypad::select_pressed() && core::investor_unlocked(a.save))   // tryb inwestora
            {
                a.text.set_palette_item(default_ink);
                a.text.set_bg_priority(default_prio);
                wait_release();
                return leave(scene::investor);
            }

            // animacje: wjazd karty, "wyskok" i kołysanie wybranego portretu, przebieranie nogami
            if(slide) { int st = slide > 0 ? -core::imin(slide_step, slide) : core::imin(slide_step, -slide); move_card(st); slide += st; }
            const int c = order[sel];
            bool unl = core::class_unlocked(a.save, c);
            big.set_scale(bn::fixed(2) - bn::fixed(pop) / 8);
            if(pop) --pop;
            int bob = unl ? pulse(clock / 12, 1) : 0;
            int sx = shake ? ((shake & 2) ? 2 : -2) : 0;
            if(shake) --shake;
            big.set_position(slot_cx(sel - first) - 120 + sx, big_cy - 80 + oy - bob);
            if(unl && clock % 24 == 0)
                big.set_tiles(bn::sprite_items::actors.tiles_item(), (clock / 24) % 2 ? anim_b(data::classes[c].frame) : data::classes[c].frame);
            next_frame();
        }
    }

    // ------------------------------------------------------------------ tryb inwestora (SELECT na wyborze zawodu)
    // Modyfikatory trudności po pierwszej wygranej: A włącza/wyłącza, każdy daje % doświadczenia i punkty stawki.
    scene run_investor(app& a)
    {
        phone_screen ph(4);
        ph.icon.set_visible(false);
        bn::sprite_palette_item default_ink = a.text.palette_item();
        page_sprites t;
        int sel = 0, top = 0;
        auto redraw = [&]() {
            int mask = core::investor_mask(a.save);
            core::message sub; sub.add("Stawka ").add(core::investor_stake(mask));
            phone_header(a, ph, t, "Tryb inwestora", sub.s);
            phone_canvas& c = *ph.canvas;
            for(int r = 0; r < 4 && top + r < data::investor_count; ++r)
            {
                int i = top + r;
                const core::investor_def& d = data::investor[i];
                bool on = (mask >> i) & 1, is_sel = i == sel;
                stripe(c, r, is_sel ? phone_tile::stripe_brand : (on ? phone_tile::stripe_done : phone_tile::stripe_todo));
                core::message pm; pm.add(on ? "WŁ +" : "+").add(d.stake);
                phone_text(a, t, list_x, row_py(r), fit(a, d.name, pill_room(pm.s)).c_str(), is_sel ? ink::brand : (on ? ink::dark : ink::dim));
                phone_pill(a, c, t, pill_end, row_ty(r), pm.s, on ? pill::done : pill::gray);
            }
            core::message dm; dm.add(data::investor[sel].desc).add(", +").add(data::investor[sel].xp_pct).add("%");
            phone_text(a, t, list_x, row_py(4), fit(a, dm.s, phone_text_w).c_str(), ink::dim);
            core::message xm; xm.add("Dośw. +").add(core::investor_xp(mask)).add("%  rekord ").add(core::best_stake(a.save, a.chosen_class));
            phone_text(a, t, list_x, row_py(5), fit(a, xm.s, 150).c_str(), ink::dark);
            phone_text(a, t, 226, row_py(5), "A: wł/wył", ink::brand, 1);
            ph.commit();
        };
        redraw();
        wait_release();
        while(true)
        {
            int n = data::investor_count;
            int dir = bn::keypad::up_pressed() ? -1 : (bn::keypad::down_pressed() ? 1 : 0);
            if(dir)
            {
                sel = (sel + dir + n) % n;
                if(sel < top) top = sel;
                if(sel >= top + 4) top = sel - 3;
                redraw(); bn::sound_items::sfx_menu.play();
            }
            if(bn::keypad::a_pressed()) { core::toggle_investor(a.save, sel); redraw(); bn::sound_items::sfx_buy.play(); }
            if(bn::keypad::b_pressed() || bn::keypad::start_pressed() || bn::keypad::select_pressed())
            {
                bn::sram::write(a.save);
                t.clear();
                a.text.set_palette_item(default_ink);
                wait_release();
                return leave(scene::class_select);
            }
            next_frame();
        }
    }

    scene run_shop(app& a, int tab = 4)
    {
        phone_screen ph(tab);
        bn::sprite_palette_item default_ink = a.text.palette_item();
        page_sprites t;
        bn::vector<bn::sprite_ptr, core::max_houses + 8> houses;   // domy i ozdoby Osiedla
        int arch = 0, asel = 0, atop = 0, amsg = 0;   // Osiedle: 0 domy, 1 Wiadomości (archiwum fabuły #35), 2 wątek
        const char* profile_tabs[] = { "Odznaki", "Katalog", "Osiedle", "Zespół", "Koszty" };

        // --- sklep (zakładka Koszty)
        enum kind : uint8_t { upgrade, cls, hard, tool, helper };
        struct entry { kind k; int8_t i; };
        bn::vector<entry, core::max_upgrades + 8 + 8 + 8 + 1> entries;
        auto rebuild = [&]() {
            entries.clear();
            for(int i = 0; i < data::upgrades_count; ++i) entries.push_back({ upgrade, int8_t(i) });
            for(int i = 0; i < data::classes_count; ++i)   // zawody i narzędzia z nagród za odbiór nie są na sprzedaż
                if(! core::class_unlocked(a.save, i) && ! core::class_reward(i)) entries.push_back({ cls, int8_t(i) });
            for(int i = 0; i < data::tools_count; ++i)
                if(! core::tool_unlocked(a.save, i) && ! data::tools[i].reward) entries.push_back({ tool, int8_t(i) });
            for(int i = 0; i < data::brigade_count; ++i)
                if(! core::helper_unlocked(a.save, i)) entries.push_back({ helper, int8_t(i) });
            if(! core::difficulty_unlocked(a.save, data::difficulties_count - 1)) entries.push_back({ hard, 0 });
        };
        rebuild();
        auto cost_of = [&](const entry& e) {
            return e.k == upgrade ? core::upgrade_cost(a.save, e.i)
                 : e.k == cls ? data::class_cost : (e.k == tool ? data::tools[e.i].cost : (e.k == helper ? data::brigade[e.i].cost : data::hard_cost));
        };

        int sel = 0, top = 0;
        const char* note = nullptr;
        // Zakładka Odznaki ma strony przełączane A: Odznaki / Zlecenia.
        const char* badge_pages[] = { "Odznaki", "Zlecenia", "Pamiątki" };
        constexpr int badge_pages_count = 3;
        int page = 0;
        // Zakładka Koszty ma strony przełączane SELECT: Szkolenia (doświadczenie) / Respekt / Nagrody za odbiór.
        const char* cost_pages[] = { "Szkolenia", "Respekt", "Nagrody" };
        constexpr int cost_pages_count = 3;
        int kpage = 0;
        auto list_size = [&]() {
            switch(tab)
            {
                case 0: return page == 2 ? data::keepsakes_count : (page == 1 ? data::contracts_count : data::badges_count);
                case 1: return data::enemies_count;
                case 3: return data::classes_count;
                case 4: return kpage == 2 ? data::rewards_count : (kpage == 1 ? data::respect_count : entries.size());
                default: return 0;
            }
        };

        auto list_window = [&]() { return tab == 4 || tab == 0 || tab == 1 ? 4 : 5; };   // widoczne wiersze listy

        auto draw_list_row = [&](int r, int i, bool is_sel) {   // zakładki 0, 1, 3
            phone_canvas& c = *ph.canvas;
            if(is_sel) stripe(c, r, phone_tile::stripe_brand);
            ink name_ink = is_sel ? ink::brand : ink::dark;
            if(tab == 0 && page == 2)   // pamiątka: nazwa + ranga
            {
                bool unl = core::keepsake_unlocked(a.save, i);
                phone_text(a, t, list_x, row_py(r), clip(data::keepsakes[i].name, 18).c_str(), unl || is_sel ? name_ink : ink::dim);
                core::message rk; rk.add("Ranga ").add(roman(core::keepsake_rank(a.save, i) - 1));
                bool chosen = core::selected_keepsake(a.save) == i;
                phone_pill(a, c, t, pill_end, row_ty(r), unl ? rk.s : "Zablok.", ! unl ? pill::gray : (chosen ? pill::prog : pill::group));
            }
            else if(tab == 0 && page == 1)   // zlecenie: nazwa + postęp licznika
            {
                bool done = core::contract_done(a.save, i);
                int pr = core::imin(core::contract_progress(a.save, i), data::contracts[i].target);
                phone_text(a, t, list_x, row_py(r), clip(data::contracts[i].name, 16).c_str(), done || is_sel ? name_ink : ink::dark);
                core::message pm; pm.add(pr).add("/").add(data::contracts[i].target);
                phone_pill(a, c, t, pill_end, row_ty(r), done ? "Wykonane" : pm.s, done ? pill::done : (pr > 0 ? pill::prog : pill::gray));
            }
            else if(tab == 0)
            {
                bool got = a.save.badges & (1u << i);
                core::message xp; xp.add("+").add(data::badges[i].xp).add(" dośw.");   // nagroda za zdobycie
                const char* pill_s = got ? "Zdobyta" : xp.s;
                phone_text(a, t, list_x, row_py(r), fit(a, data::badges[i].name, pill_room(pill_s)).c_str(), got || is_sel ? name_ink : ink::dim);
                phone_pill(a, c, t, pill_end, row_ty(r), pill_s, got ? pill::done : pill::gray);
            }
            else if(tab == 1)
            {
                bool known = core::catalog_has(a.save, i);
                core::message m; m.add("#").add(i + 1).add(" ").add(known ? clip(data::enemies[i].name, 14).c_str() : "???");
                phone_text(a, t, list_x, row_py(r), m.s, known || is_sel ? name_ink : ink::dim);
                phone_pill(a, c, t, pill_end, row_ty(r), known ? "ZAMKNIĘTA" : "NIEZNANA", known ? pill::done : pill::gray);
            }
            else
            {
                bool won = core::class_won(a.save, i), unl = core::class_unlocked(a.save, i);
                phone_text(a, t, list_x, row_py(r), clip(data::classes[i].name, 16).c_str(), unl || is_sel ? name_ink : ink::dim);
                phone_pill(a, c, t, pill_end, row_ty(r), won ? "Wygrana" : (unl ? "Dostępny" : "Zablok."),
                           won ? pill::done : (unl ? pill::group : pill::gray));
            }
        };

        auto redraw = [&]() {
            houses.clear();
            phone_canvas& c = *ph.canvas;
            core::message sub;
            if(tab == 4 && kpage == 2) sub.add("Wygrane ").add(int(a.save.wins)).add("  SELECT");
            else if(tab == 4) sub.add("A: kup  SELECT: ").add(cost_pages[(kpage + 1) % cost_pages_count]);
            else if(tab == 0)
            {
                int n = 0, total = page == 2 ? data::keepsakes_count : (page == 1 ? data::contracts_count : data::badges_count);
                for(int i = 0; i < total; ++i)
                    n += page == 2 ? core::keepsake_unlocked(a.save, i) : ((page == 1 ? a.save.contracts : a.save.badges) >> i) & 1;
                sub.add(n).add("/").add(total).add("  A: ").add(badge_pages[(page + 1) % badge_pages_count]);
            }
            else if(tab == 1) sub.add(core::catalog_count(a.save)).add("/").add(data::enemies_count);
            else if(tab == 2) sub.add("Domy: ").add(int(a.save.houses_count)).add("/").add(core::max_houses);
            else { int n = core::classes_won(a.save);
                   sub.add("Wygrane ").add(n).add("/").add(data::classes_count); }
            phone_header(a, ph, t, tab == 0 ? badge_pages[page] : (tab == 4 ? cost_pages[kpage] : profile_tabs[tab]), sub.s);

            if(tab == 2 && arch == 1)   // Wiadomości: wątki fabuły (odblokowane kamieniami milowymi), zablokowane z podpowiedzią
            {
                core::message sb; sb.add(core::story_count(a.save)).add("/").add(data::story_arc_count).add("  A: czytaj");
                phone_header(a, ph, t, "Wiadomości", sb.s);
                for(int r = 0; r < 6 && atop + r < data::story_arc_count; ++r)
                {
                    const int i = atop + r;
                    const bool unl = core::story_unlocked(a.save, i), is_sel = i == asel;
                    if(is_sel) stripe(c, r, phone_tile::stripe_brand);
                    if(unl)
                    {
                        const bool nw = core::story_unread(a.save, i);
                        phone_text(a, t, list_x, row_py(r), fit(a, data::story_arc[i].name, pill_room(nw ? "Nowa" : "SMS")).c_str(), is_sel ? ink::brand : ink::dark);
                        phone_pill(a, c, t, pill_end, row_ty(r), nw ? "Nowa" : "SMS", nw ? pill::brand : pill::gray);
                    }
                    else
                    {
                        phone_text(a, t, list_x, row_py(r), fit(a, is_sel ? data::story_arc[i].hint : "???", pill_room("Zablok.")).c_str(), ink::dim);
                        phone_pill(a, c, t, pill_end, row_ty(r), "Zablok.", pill::gray);
                    }
                }
                ph.commit();
                return;
            }
            if(tab == 2 && arch == 2)   // wątek: SMS po SMS-ie (A dalej)
            {
                const core::story_thread& th = data::story_arc[asel];
                const core::story_msg& m = th.msgs[amsg];
                core::message sb; sb.add(amsg + 1).add("/").add(th.msgs_count).add("  B: wróć");
                phone_header(a, ph, t, fit(a, th.name, 120).c_str(), sb.s);
                phone_text(a, t, list_x, row_py(0), m.from, ink::dark);
                phone_pill(a, c, t, pill_end, row_ty(0), "SMS", pill::gray);
                c.rounded(2, row_ty(1), 26, 6, phone_tile::fill_group, phone_tile::corner_group);   // dymek wiadomości
                for(int i = 0; i < 3; ++i) phone_text(a, t, 22, row_py(1 + i), m.lines[i], ink::dark);
                phone_text(a, t, 226, row_py(5), amsg + 1 < th.msgs_count ? "A: dalej" : "A: lista", ink::brand, 1);
                ph.commit();
                return;
            }
            if(tab == 2)   // Osiedle: domy z wygranych budów, 6 x 2 działki, między nimi ozdoby (rosną z wygranymi)
            {
                core::message wm; wm.add("Wiadomości ").add(core::story_count(a.save)).add("/").add(data::story_arc_count);
                const int unread = core::story_unread_count(a.save);
                core::message pm; if(unread) pm.add(unread).add(" nowe"); else pm.add("A");
                phone_text(a, t, list_x, row_py(0), wm.s, ink::dim);
                phone_pill(a, c, t, pill_end, row_ty(0), pm.s, unread ? pill::brand : pill::gray);
                for(int k = 0; k < core::estate_decor(a.save); ++k)   // ozdoby w przerwach między domami
                {
                    bn::sprite_ptr ds = bn::sprite_items::houses.create_sprite(48 + (k % 5) * 36 - 120, 64 + (k / 5) * 28 - 80, house_empty + 1 + k);
                    ds.set_bg_priority(1);
                    houses.push_back(ds);
                }
                for(int i = 0; i < core::max_houses; ++i)
                {
                    int frame = house_empty;
                    if(i < a.save.houses_count) frame = house_frame(a.save.houses[i]);
                    bn::sprite_ptr hs = bn::sprite_items::houses.create_sprite(30 + (i % 6) * 36 - 120, 64 + (i / 6) * 28 - 80, frame);
                    hs.set_bg_priority(1);
                    houses.push_back(hs);
                }
                core::message best; best.add("Najlepszy wynik: ").add(int(a.save.best));
                phone_text(a, t, list_x, row_py(5), best.s, ink::dim);
                ph.commit();
                return;
            }
            if(tab == 4 && kpage == 1)   // Respekt: stałe premie z rangami za Respekt z ukończonych etapów
            {
                phone_text(a, t, list_x, row_py(0), "Masz", ink::dim);
                core::message rv; rv.add(int(a.save.respect)).add(" Respektu");
                phone_text(a, t, 226, row_py(0), rv.s, ink::done, 1);
                const core::respect_def& rd = data::respect[sel];
                int rank = core::respect_rank(a.save, sel);
                core::message dm;
                if(note) dm.add(note);
                else
                {
                    if(rank > 0) core::respect_label(dm, rd.effect, core::respect_value(a.save, sel));
                    else dm.add(rd.desc).add(": brak");
                    if(rank < rd.ranks) { dm.add(rank > 0 ? " > " : ", ranga I: "); core::respect_label(dm, rd.effect, rd.values[rank]); }
                }
                phone_text(a, t, list_x, row_py(1), fit(a, dm.s, phone_text_w).c_str(), note ? ink::brand : ink::dim);
                for(int r = 0; r < 4 && top + r < data::respect_count; ++r)
                {
                    int i = top + r;
                    bool is_sel = i == sel;
                    core::message m; m.add(data::respect[i].name).add(" ").add(core::respect_rank(a.save, i)).add("/").add(data::respect[i].ranks);
                    if(is_sel) stripe(c, r + 2, phone_tile::stripe_brand);
                    int cost = core::respect_cost(a.save, i);
                    core::message cm; cm.add(cost);
                    const char* pill_s = cost < 0 ? "MAX" : cm.s;
                    phone_text(a, t, list_x, row_py(r + 2), fit(a, m.s, pill_room(pill_s)).c_str(), is_sel ? ink::brand : ink::dark);
                    phone_pill(a, c, t, pill_end, row_ty(r + 2), pill_s, cost < 0 ? pill::done : (cost <= a.save.respect ? pill::group : pill::gray));
                }
                ph.commit();
                return;
            }
            if(tab == 4 && kpage == 2)   // Nagrody za odbiór: każda wygrana odblokowuje kolejną
            {
                int nr = a.save.rewards;
                core::message nm;
                if(nr < core::rewards_available()) nm.add("Za wygraną: ").add(data::rewards[nr].name);
                else nm.add("Wszystko odebrane - więcej wkrótce");
                phone_text(a, t, list_x, row_py(0), fit(a, nm.s, phone_text_w).c_str(), ink::brand);
                for(int r = 0; r < 4 && top + r < data::rewards_count; ++r)
                {
                    int i = top + r;
                    const core::reward_def& rw = data::rewards[i];
                    bool is_sel = i == sel, got = core::reward_owned(a.save, i), soon = rw.kind == core::reward_kind::soon;
                    if(is_sel) stripe(c, r + 1, phone_tile::stripe_brand);
                    core::message pm;
                    if(got) pm.add("Odebrana");
                    else if(soon) pm.add("Wkrótce");
                    else if(i == nr) pm.add("Następna");
                    else pm.add(core::reward_win(a.save, i)).add(". wygr.");
                    phone_text(a, t, list_x + 18, row_py(r + 1), fit(a, rw.name, pill_room(pm.s) - 18).c_str(),
                               is_sel ? ink::brand : (got ? ink::dark : ink::dim));
                    phone_pill(a, c, t, pill_end, row_ty(r + 1), pm.s, got ? pill::done : (i == nr ? pill::prog : pill::gray));
                    // ikona nagrody: portret zawodu (sylwetka, gdy zablokowany) albo ikona narzędzia / sprzętu
                    bn::optional<bn::sprite_ptr> ic;
                    int iy = row_py(r + 1) + 8 - 80, ix = list_x + 8 - 120;
                    if(rw.kind == core::reward_kind::cls)
                        ic = bn::sprite_items::actors.create_sprite_optional(ix, iy, got ? data::classes[rw.index].frame : silhouette_frame(rw.index));
                    else ic = bn::sprite_items::menu_icons.create_sprite_optional(ix, iy, reward_icon(rw));
                    if(ic) { ic->set_bg_priority(1); houses.push_back(bn::move(*ic)); }
                }
                const core::reward_def& rs = data::rewards[sel];
                core::message dm; dm.add(rs.desc);
                if(rs.kind == core::reward_kind::tool)
                {
                    const core::weapon_def& w = data::weapons[data::tools[rs.index].weapon];
                    dm = core::message(); dm.add(w.name).add(" ").add(w.min_damage).add("-").add(w.max_damage).add(" z").add(w.range)
                                           .add(", ").add(stat_short(w.scales_with));
                }
                else if(rs.kind == core::reward_kind::gear)
                {
                    const core::gear_def& gd = data::gear[rs.index * 3 + 2];
                    dm = core::message(); dm.add(data::gear_slots[rs.index]).add(": ").add(gear_stat_name(gd.stat)).add(" do +").add(gd.value);
                }
                else if(rs.kind == core::reward_kind::cls) { dm = core::message(); dm.add(data::classes[rs.index].ability_name).add(": ").add(data::classes[rs.index].ability_desc); }
                phone_text(a, t, list_x, row_py(5), fit(a, dm.s, phone_text_w).c_str(), ink::dim);
                ph.commit();
                return;
            }
            if(tab == 4)   // Koszty = sklep Szkolenia
            {
                phone_text(a, t, list_x, row_py(0), "Pozostało", ink::dim);
                core::message xp; xp.add(int(a.save.xp)).add(" dośw.");
                phone_text(a, t, 226, row_py(0), xp.s, ink::done, 1);
                const entry& se = entries[sel];
                core::message tool_desc;
                if(se.k == tool)
                {
                    const core::weapon_def& w = data::weapons[data::tools[se.i].weapon];
                    tool_desc.add("Narzędzie ").add(w.min_damage).add("-").add(w.max_damage).add(" z").add(w.range)
                             .add(", ").add(stat_short(w.scales_with));
                }
                const char* desc = note ? note : (se.k == upgrade ? data::upgrades[se.i].desc
                                 : (se.k == cls ? "Nowy zawód do wyboru" : (se.k == tool ? tool_desc.s
                                 : (se.k == helper ? data::brigade[se.i].desc : "Najwyższa trudność"))));
                phone_text(a, t, list_x, row_py(1), fit(a, desc, phone_text_w).c_str(), ink::dim);
                for(int r = 0; r < 4 && top + r < entries.size(); ++r)
                {
                    const entry& e = entries[top + r];
                    bool is_sel = top + r == sel;
                    core::message m;
                    if(e.k == upgrade) m.add(data::upgrades[e.i].name).add(" ").add(a.save.levels[e.i]).add("/").add(data::upgrades[e.i].levels);
                    else if(e.k == cls) m.add("Zawód: ").add(data::classes[e.i].name);
                    else if(e.k == tool) m.add(data::weapons[data::tools[e.i].weapon].name);
                    else if(e.k == helper) m.add("Brygada: ").add(data::brigade[e.i].name);
                    else m.add("Trudność: ").add(data::difficulties[data::difficulties_count - 1].name);
                    if(is_sel) stripe(c, r + 2, phone_tile::stripe_brand);
                    phone_text(a, t, list_x, row_py(r + 2), clip(m.s, 17).c_str(), is_sel ? ink::brand : ink::dark);
                    int cost = cost_of(e);
                    core::message cm; cm.add(cost);
                    phone_pill(a, c, t, pill_end, row_ty(r + 2), cost < 0 ? "MAX" : cm.s,
                               cost < 0 ? pill::done : (cost <= a.save.xp ? pill::group : pill::gray));
                }
                ph.commit();
                return;
            }
            // listy: Odznaki (4 wiersze + opis + uprawnienie), Katalog, Zespół (5 wierszy + opis zaznaczonego)
            for(int r = 0; r < list_window() && top + r < list_size(); ++r) draw_list_row(r, top + r, top + r == sel);
            const char* desc = "";
            if(tab == 0 && page == 2)
            {
                const core::keepsake_def& kd = data::keepsakes[sel];
                bool unl = core::keepsake_unlocked(a.save, sel);
                core::message e; core::perk_label(e, core::keepsake_perk(a.save, sel));
                int runs = a.save.keepsake_runs[sel], rank = core::keepsake_rank(a.save, sel);
                if(unl) e.add(", budowy: ").add(runs);
                phone_text(a, t, list_x, row_py(4), fit(a, unl ? e.s : kd.desc, phone_text_w).c_str(), unl ? ink::dark : ink::dim);
                core::message u;
                if(! unl)
                {
                    if(kd.badge >= 0) u.add("Odznaka: ").add(data::badges[kd.badge].name);
                    else for(int i = 0; i < data::contracts_count; ++i)
                        if(data::contracts[i].keepsake == sel) { u.add("Zlecenie: ").add(data::contracts[i].name); break; }
                }
                else if(rank < 3) u.add("Ranga ").add(roman(rank)).add(" po ").add(data::keepsake_rank_runs[rank - 1]).add(" bud.");
                else u.add(kd.desc);
                phone_text(a, t, list_x, row_py(5), fit(a, u.s, phone_text_w).c_str(), unl ? ink::dim : ink::brand);
                ph.commit();
                return;
            }
            if(tab == 0 && page == 1)
            {
                const core::contract_def& cd = data::contracts[sel];
                phone_text(a, t, list_x, row_py(4), fit(a, cd.desc, phone_text_w).c_str(), ink::dim);
                core::message rm; rm.add("Nagroda: +").add(cd.xp);
                if(cd.keepsake >= 0) rm.add(", ").add(data::keepsakes[cd.keepsake].name);
                else rm.add(" dośw.");
                phone_text(a, t, list_x, row_py(5), fit(a, rm.s, phone_text_w).c_str(), core::contract_done(a.save, sel) ? ink::done : ink::dim);
                ph.commit();
                return;
            }
            if(tab == 0)
            {
                bool got = a.save.badges & (1u << sel);
                phone_text(a, t, list_x, row_py(4), fit(a, data::badges[sel].desc, phone_text_w).c_str(), ink::dim);
                core::message pm; pm.add("Premia: ");
                core::perk_label(pm, data::badges[sel].bonus);
                phone_text(a, t, list_x, row_py(5), fit(a, pm.s, phone_text_w).c_str(), got ? ink::done : ink::dim);
                ph.commit();
                return;
            }
            else if(tab == 1)   // Katalog: zachowania i opis (po pokonaniu)
            {
                bool known = core::catalog_has(a.save, sel);
                core::message bl = behaviors_line(sel);
                if(known && bl.n > 0) { core::message m; m.add("Cechy: ").add(bl.s); phone_text(a, t, list_x, row_py(4), fit(a, m.s, phone_text_w).c_str(), ink::late); }
                desc = known ? data::enemies[sel].desc : "Pokonaj, żeby poznać";
            }
            else desc = data::classes[sel].ability_desc;
            phone_text(a, t, list_x, row_py(5), fit(a, desc, phone_text_w).c_str(), ink::dim);
            ph.commit();
        };
        ph.set_tab(tab);
        redraw();

        while(true)
        {
            if(tab == 2 && arch > 0)   // Wiadomości (fabuła #35)
            {
                if(arch == 1)
                {
                    int dir = bn::keypad::up_pressed() ? -1 : (bn::keypad::down_pressed() ? 1 : 0);
                    if(dir)
                    {
                        asel = (asel + dir + data::story_arc_count) % data::story_arc_count;
                        if(asel < atop) atop = asel;
                        if(asel >= atop + 6) atop = asel - 5;
                        redraw();
                    }
                    if(bn::keypad::a_pressed() && core::story_unlocked(a.save, asel))
                    {
                        arch = 2; amsg = 0;
                        if(core::story_unread(a.save, asel)) { core::story_mark_read(a.save, asel); bn::sram::write(a.save); }
                        bn::sound_items::sfx_notify.play(bn::fixed(0.6));
                        redraw();
                    }
                    else if(bn::keypad::b_pressed()) { arch = 0; redraw(); bn::sound_items::sfx_menu.play(); }
                }
                else if(bn::keypad::a_pressed())
                {
                    if(amsg + 1 < data::story_arc[asel].msgs_count) ++amsg; else arch = 1;
                    redraw(); bn::sound_items::sfx_menu.play();
                }
                else if(bn::keypad::b_pressed()) { arch = 1; redraw(); bn::sound_items::sfx_menu.play(); }
                next_frame();
                continue;
            }
            if(tab == 2 && bn::keypad::a_pressed())   // Osiedle -> Wiadomości
            {
                arch = 1;
                for(int i = 0; i < data::story_arc_count; ++i) if(core::story_unread(a.save, i)) { asel = i; break; }
                atop = core::imax(0, core::imin(asel, data::story_arc_count - 6));
                redraw(); bn::sound_items::sfx_menu.play();
                wait_release();
                continue;
            }
            int d = (bn::keypad::r_pressed() || bn::keypad::right_pressed()) ? 1
                  : ((bn::keypad::l_pressed() || bn::keypad::left_pressed()) ? -1 : 0);
            if(d)
            {
                tab = (tab + d + tabs_count) % tabs_count;
                sel = top = 0; note = nullptr;
                ph.set_tab(tab);
                redraw();
                bn::sound_items::sfx_menu.play();
            }
            int n = list_size(), window = list_window();
            int dir = bn::keypad::up_pressed() ? -1 : (bn::keypad::down_pressed() ? 1 : 0);
            if(dir && n > 0)
            {
                sel = (sel + dir + n) % n;
                if(sel < top) top = sel;
                if(sel >= top + window) top = sel - window + 1;
                note = nullptr;
                redraw();
            }
            if(tab == 0 && bn::keypad::a_pressed())   // Odznaki <-> Zlecenia
            {
                page = (page + 1) % badge_pages_count;
                sel = top = 0;
                redraw();
                bn::sound_items::sfx_menu.play();
            }
            if(tab == 4 && bn::keypad::select_pressed())   // Szkolenia -> Respekt -> Nagrody
            {
                kpage = (kpage + 1) % cost_pages_count;
                sel = top = 0; note = nullptr;
                redraw();
                bn::sound_items::sfx_menu.play();
            }
            else if(tab == 4 && kpage == 1 && bn::keypad::a_pressed())
            {
                if(core::buy_respect(a.save, sel))
                {
                    bn::sound_items::sfx_buy.play();
                    bn::sram::write(a.save);
                    note = "Kupione!";
                }
                else note = core::respect_cost(a.save, sel) < 0 ? "Maksymalna ranga" : "Za mało Respektu - kończ etapy";
                redraw();
            }
            else if(tab == 4 && kpage == 0 && bn::keypad::a_pressed())
            {
                const entry e = entries[sel];
                bool ok = e.k == upgrade ? core::buy_upgrade(a.save, e.i)
                        : e.k == cls ? core::buy_class(a.save, e.i)
                        : e.k == tool ? core::buy_tool(a.save, e.i)
                        : e.k == helper ? core::buy_helper(a.save, e.i) : core::buy_hard(a.save);
                if(ok)
                {
                    bn::sound_items::sfx_buy.play();
                    bn::sram::write(a.save);
                    note = "Kupione!";
                    rebuild();
                    if(sel >= entries.size()) sel = entries.size() - 1;
                    if(top > sel) top = sel;
                }
                else note = (e.k == upgrade && core::upgrade_cost(a.save, e.i) < 0) ? "Maksymalny poziom" : "Za mało doświadczenia";
                redraw();
            }
            if(bn::keypad::b_pressed() || bn::keypad::start_pressed())
            {
                t.clear();
                houses.clear();
                a.text.set_palette_item(default_ink);
                wait_release();
                return leave(scene::title);
            }
            next_frame();
        }
    }
}

int main()
{
    bn::core::init();
    app a;
    a.save = load_save();
#ifdef PB_SCENARIO
    debug_scenario::unlock_all(a.save);
    debug_scenario::setup_profile(a.save, PB_SCENARIO);
#endif
    bn::unique_ptr<core::game> game(new core::game());
    a.g = game.get();
    a.has_run = load_run(a);   // tylko sprawdzenie; start i tak idzie przez tytuł

    scene s = scene::title;
    while(true)
    {
        switch(s)
        {
            case scene::title:        s = run_title(a); break;
            case scene::class_select: s = run_class_select(a); break;
            case scene::game:         s = run_game(a); break;
            case scene::schedule:     s = run_schedule(a); break;
            case scene::end:          s = run_end(a); break;
            case scene::shop:         s = run_shop(a); break;
            case scene::help:         s = run_help(a); break;
            case scene::hurtownia:    s = run_hurtownia(a); break;
            case scene::prologue:     s = run_prologue(a); break;
            case scene::investor:     s = run_investor(a); break;
            case scene::daily:        s = run_daily(a); break;
            case scene::weekly:       s = run_weekly(a); break;
            default: break;
        }
    }
}
