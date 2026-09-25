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

    enum class scene { title, class_select, game, schedule, end, shop, help, hurtownia, prologue };

    constexpr int frame_coffee = 15;
    constexpr int frame_fx = 18;
    constexpr int frame_lock = 19;
    constexpr int frame_silhouette = 20;   // + indeks zawodu
    constexpr int frame_toolbox = 26;
    constexpr int frame_anim_b = 27;     // + klatka zawodu/wroga (0..14) = druga klatka animacji

    int anim_b(int frame) { return frame < 15 ? frame + frame_anim_b : frame + 2; }   // bossowie aktów: 46-47 -> 48-49

    const char* roman(int n) { static const char* r[] = { "I", "II", "III", "IV", "V" }; return r[n < 5 ? n : 4]; }

    constexpr int frame_gear = 42;       // + jakość
    constexpr int frame_reticle = 45;
    int pickup_frame(const core::pickup& p)
    {
        if(p.type == core::tool) return frame_toolbox;
        if(p.type == core::gear_box) return frame_gear + p.arg % 3;
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

    // ------------------------------------------------------------------ ekran tytułowy
    scene run_title(app& a)
    {
        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        bn::regular_bg_ptr bg = bn::regular_bg_items::title.create_bg(8, 48);   // lewy górny róg obrazu = róg ekranu
        bn::bg_palette_color_hbe_ptr gradient = make_gradient(bg, screen_info::title_bg_index);
        play_song(song::title);
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
            next_frame();
        }
    }

    // ------------------------------------------------------------------ wybór zawodu
    scene run_class_select(app& a)
    {
        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        text_sprites header, lines, diff_line;
        a.text.set_center_alignment();
        a.text.generate(0, -72, "Wybierz fach", header);
        bn::sprite_ptr hero = bn::sprite_items::actors.create_sprite(0, -30, 0);
        hero.set_double_size_mode(bn::sprite_double_size_mode::ENABLED);
        hero.set_scale(2);
        bn::sprite_ptr lock = bn::sprite_items::actors.create_sprite(20, -20, frame_lock);
        lock.set_z_order(-1);

        auto redraw = [&]() {
            lines.clear();
            const core::class_def& c = data::classes[a.chosen_class];
            const core::weapon_def& w = data::weapons[c.weapon];
            bool locked = ! core::class_unlocked(a.save, a.chosen_class);
            hero.set_tiles(bn::sprite_items::actors.tiles_item(), locked ? frame_silhouette + a.chosen_class : c.frame);
            lock.set_visible(locked);
            core::message n; n.add("< ").add(c.name).add(" >");
            a.text.generate(0, 4, n.s, lines);
            core::message ld; ld.add("Zablokowany: ").add(data::class_cost).add(" dośw.");
            core::message ab; ab.add("Moc R: ").add(c.ability_name);
            a.text.generate(0, 20, locked ? ld.s : ab.s, lines);
            // statystyki efektywne: zawód + Szkolenia (Warsztaty, BHP) - "baza+premia"
            const core::run_mods m = core::mods(a.save);
            const int ci = a.chosen_class;
            core::message s; add_stat(s, "HP", c.max_health, m.hp);
            s.add(" "); add_stat(s, "SIŁ", c.strength, core::mods_stat_bonus(m, ci, core::stat::str));
            s.add(" "); add_stat(s, "ZRĘ", c.agility, core::mods_stat_bonus(m, ci, core::stat::agi));
            a.text.generate(0, 38, s.s, lines);
            core::message s2; add_stat(s2, "INT", c.intelligence, core::mods_stat_bonus(m, ci, core::stat::intel));
            s2.add(" "); add_stat(s2, "OBR", c.defense, m.def);
            s2.add(" "); add_stat(s2, "SZCZ", c.luck, m.luck);
            a.text.generate(0, 54, s2.s, lines);
            core::message wl; wl.add(w.name).add(" ").add(w.min_damage).add("-").add(w.max_damage).add(" z").add(w.range)
                                   .add(" (").add(stat_short(w.scales_with)).add(")");
            a.text.generate(0, 72, clip(wl.s, 29), lines);
        };
        auto redraw_diff = [&]() {
            diff_line.clear();
            a.text.set_center_alignment();
            core::message m; m.add("Trudność (góra/dół): ").add(data::difficulties[a.chosen_diff].name);
            a.text.generate(0, -56, m.s, diff_line);
        };
        redraw();
        redraw_diff();

        while(true)
        {
            int ddir = bn::keypad::up_pressed() ? -1 : (bn::keypad::down_pressed() ? 1 : 0);
            if(ddir)
            {
                do a.chosen_diff = (a.chosen_diff + ddir + data::difficulties_count) % data::difficulties_count;
                while(! core::difficulty_unlocked(a.save, a.chosen_diff));
                redraw_diff();
            }
            if(bn::keypad::left_pressed()) { a.chosen_class = (a.chosen_class + data::classes_count - 1) % data::classes_count; redraw(); bn::sound_items::sfx_menu.play(); }
            if(bn::keypad::right_pressed()) { a.chosen_class = (a.chosen_class + 1) % data::classes_count; redraw(); bn::sound_items::sfx_menu.play(); }
            if((bn::keypad::a_pressed() || bn::keypad::start_pressed()) && core::class_unlocked(a.save, a.chosen_class))
            {
                a.g->new_run(a.chosen_class, a.seed_counter * 2654435761u + 12345u, a.chosen_diff, core::mods(a.save));
#ifdef PB_SCENARIO
                debug_scenario::apply(*a.g, PB_SCENARIO);
#endif
                ++a.save.runs;
                bn::sram::write(a.save);
                wait_release();
                if(! core::has_flag(a.save, core::prologue_seen)) return leave(scene::prologue);
                if(! core::has_flag(a.save, core::help_seen)) { a.after_help = scene::game; return leave(scene::help); }
                return leave(scene::game);
            }
            if(bn::keypad::b_pressed()) { wait_release(); return leave(scene::title); }
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
        static int tile_of(const core::game& g, int x, int y, int& palette)
        {
            palette = light_level(g, x, y);
            if(! g.explored(x, y)) return 0;
            core::tile k = g.lv.at(x, y);
            if(k == core::tile::floor) return g.lv.at(x, y - 1) == core::tile::wall ? 5 : 1;   // cień muru u góry
            if(k == core::tile::stairs) return 4;
            return g.lv.at(x, y + 1) != core::tile::wall ? 3 : 2;   // lico muru nad podłogą / mur
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
                    if((t == 1 || t == 5) && g.slam_cell(x, y))   // zapowiedziany cios bossa
                    {
                        set(x * 2, y * 2, 8, pal); set(x * 2 + 1, y * 2, 8, pal, true);
                        set(x * 2, y * 2 + 1, 8, pal, false, true); set(x * 2 + 1, y * 2 + 1, 8, pal, true, true);
                        continue;
                    }
                    if((t == 1 || t == 5) && highlight && highlight[y][x])   // ramka pola w zasięgu
                    {
                        set(x * 2, y * 2, 7, pal); set(x * 2 + 1, y * 2, 7, pal, true);
                        set(x * 2, y * 2 + 1, 7, pal, false, true); set(x * 2 + 1, y * 2 + 1, 7, pal, true, true);
                        continue;
                    }
                    bool top_shadow = t == 5;
                    int base = top_shadow ? 1 : t;
                    set(x * 2, y * 2, t, pal); set(x * 2 + 1, y * 2, t, pal);
                    if((t == 1 || t == 5) && g.visible(x, y) && casts_shadow(g, x, y))
                    {
                        set(x * 2, y * 2 + 1, 6, pal);
                        set(x * 2 + 1, y * 2 + 1, 6, pal, true);
                    }
                    else { set(x * 2, y * 2 + 1, base, pal); set(x * 2 + 1, y * 2 + 1, base, pal); }
                }
        }

        // Podgląd mapy (L): pole = 1 kafel 8x8, cały etap mieści się prawie na jednym ekranie.
        void build_overview(const core::game& g)
        {
            bn::memory::clear(cells);
            for(int y = 0; y < core::map_h; ++y)
                for(int x = 0; x < core::map_w; ++x) { int pal, t = tile_of(g, x, y, pal); set(x, y, t, pal); }
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
        page_sprites t;
        a.text.set_center_alignment();
        a.text.generate(0, -70, "Jak grać", t);
        a.text.set_left_alignment();
        const char* lines[] = { "8 etapów w 3 aktach, każdy", "kończy boss. Schody = dalej.", "D-pad: ruch i atak wręcz",
                                "A: atak (trzymaj: celuj)", "B: czekaj (trzymaj: podgląd)", "R: moc zawodu  L: mapa",
                                "START: akcje  SELECT: telefon" };
        for(int i = 0; i < 7; ++i) a.text.generate(-108, -48 + i * 16, lines[i], t);
        a.text.set_center_alignment();
        a.text.generate(0, 72, "A: dalej", t);
        wait_page_close();
    }

    scene run_help(app& a)
    {
        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        page_help(a);
        if(! core::has_flag(a.save, core::help_seen)) { core::set_flag(a.save, core::help_seen); bn::sram::write(a.save); }
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

    void stripe(phone_canvas& c, int r, int tile) { c.set(1, row_ty(r), tile); c.set(1, row_ty(r) + 1, tile); }

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

    void tab_tasks(app& a, phone_screen& ph, page_sprites& t)   // Zadania = harmonogram budowy
    {
        const core::game& g = *a.g;
        core::message sub; sub.add("Etap ").add(g.stage + 1).add("/").add(data::stages_count).add(", ").add(g.score).add(" pkt");
        phone_header(a, ph, t, tab_names[0], sub.s);
        phone_canvas& c = *ph.canvas;
        int first = core::imax(0, core::imin(g.stage - 2, data::stages_count - 5));   // okno 5 etapów wokół bieżącego
        for(int r = 0; r < 5 && first + r < data::stages_count; ++r)
        {
            int i = first + r;
            bool done = i < g.stage || (i == g.stage && g.st == core::status::won);
            bool cur = i == g.stage && ! done;
            stripe(c, r, done ? phone_tile::stripe_done : (cur ? phone_tile::stripe_prog : phone_tile::stripe_todo));
            core::message m; m.add(i + 1).add(". ").add(data::stages[i].name);
            phone_text(a, t, list_x, row_py(r), clip(m.s, 15).c_str(), done ? ink::dim : ink::dark);
            phone_pill(a, c, t, pill_end, row_ty(r), done ? "Gotowe" : (cur ? "W trakcie" : "Do zrob."),
                       done ? pill::done : (cur ? pill::prog : pill::gray));
        }
        phone_text(a, t, list_x, row_py(5), "Postęp", ink::dim);
        c.bar(9, row_ty(5), 18, phone_tile::bar_brand_0, g.stage, data::stages_count);
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

    constexpr core::status_effect hud_statuses[3] = { core::status_effect::poison, core::status_effect::shock, core::status_effect::slip };

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
        core::message r0; r0.add("Etap ").add(g.stage + 1).add(": ").add(data::stages[g.stage].name);
        phone_text(a, t, list_x, row_py(0), clip(r0.s, 15).c_str(), ink::dark);
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
        phone_text(a, t, list_x, row_py(4), clip(sl.s, 40).c_str(), sl.kind == core::bad ? ink::late : ink::dim);
        core::message sc = hero_stats_line(g);   // statystyki efektywne (baza+premie); kryt i unik w zakładce Sprzęt
        phone_text(a, t, list_x, row_py(5), clip(sc.s, 34).c_str(), ink::dim);
    }

    void tab_gear(app& a, phone_screen& ph, page_sprites& t)   // Sprzęt: narzędzie + kask, rękawice, kamizelka
    {
        const core::game& g = *a.g;
        phone_header(a, ph, t, tab_names[3], "Na budowie");
        phone_canvas& c = *ph.canvas;
        core::message w; w.add(g.weapon().name).add(" ").add(g.weapon().min_damage).add("-").add(g.weapon().max_damage)
                               .add(" z").add(g.weapon().range).add(" +").add(g.dmg_bonus);
        stripe(c, 0, phone_tile::stripe_brand);
        phone_text(a, t, list_x, row_py(0), clip(w.s, 26).c_str(), ink::dark);
        for(int i = 0; i < data::gear_slots_count; ++i)
        {
            int r = g.equipped[i];
            core::message m;
            if(r >= 0) m.add(data::gear[i * 3 + r].name);
            else m.add(data::gear_slots[i]).add(": brak");
            stripe(c, 1 + i, r < 0 ? phone_tile::stripe_todo : (r == 2 ? phone_tile::stripe_prog : (r == 1 ? phone_tile::stripe_brand : phone_tile::stripe_done)));
            phone_text(a, t, list_x, row_py(1 + i), clip(m.s, 17).c_str(), r >= 0 ? ink::dark : ink::dim);
            if(r >= 0)   // pastylka = cecha przedmiotu, kolor = jakość
                phone_pill(a, c, t, pill_end, row_ty(1 + i), data::gear_traits[g.equipped_trait[i]].short_name,
                           r == 2 ? pill::prog : (r == 1 ? pill::group : pill::gray));
        }
        core::message s1; s1.add("Obrona +").add(g.gear_bonus(core::gear_stat::def)).add("  Obraż. +").add(g.gear_bonus(core::gear_stat::dmg))
                                .add("  HP +").add(g.gear_bonus(core::gear_stat::hp));
        phone_text(a, t, list_x, row_py(4), clip(s1.s, 34).c_str(), ink::dim);
        core::message s2; s2.add("Kryt ").add(g.crit_pct()).add("%  Unik ").add(g.dodge_pct()).add("%  Wzrok ").add(g.sight_radius());
        phone_text(a, t, list_x, row_py(5), clip(s2.s, 34).c_str(), ink::dim);
    }

    void tab_costs(app& a, phone_screen& ph, page_sprites& t)   // Koszty = Szkolenia (podgląd w trakcie budowy)
    {
        const core::game& g = *a.g;
        phone_header(a, ph, t, tab_names[4], "Szkolenia");
        phone_canvas& c = *ph.canvas;
        int total = core::shop_total_cost(), spent = core::shop_spent(a.save);
        phone_text(a, t, list_x, row_py(0), "CAŁKOWITY KOSZT", ink::dim);
        core::message sp; sp.add(spent).add(" dośw.");
        phone_text(a, t, list_x, row_py(1), sp.s, ink::dark);
        c.bar(2, row_ty(2), 26, phone_tile::bar_brand_0, spent, total);
        core::message bud; bud.add("Budżet ").add(total);
        phone_text(a, t, list_x, row_py(3), bud.s, ink::dim);
        core::message rest; rest.add("Pozostało ").add(int(a.save.xp));
        phone_text(a, t, 226, row_py(3), rest.s, ink::done, 1);
        core::message run; run.add("Z tej budowy: +").add(g.xp() - g.xp_banked);
        phone_text(a, t, list_x, row_py(4), run.s, ink::dim);
        phone_text(a, t, list_x, row_py(5), "Kupisz po budowie", ink::dim);
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

    enum class pause_result { resume, quit, save_exit };

    // Telefon pod SELECT. L/R lub strzałki: zakładki; START: menu akcji; B/SELECT: powrót do gry.
    pause_result run_phone(app& a)
    {
        phone_screen ph(a.phone_tab);
        page_sprites t;
        bn::sprite_palette_item default_ink = a.text.palette_item();
        const char* actions[] = { "Wróć do gry", "Jak grać", "Zapisz i wyjdź", "Porzuć budowę" };
        constexpr int actions_count = 4;
        bool sheet = false, confirm = false;
        int sel = 0;
        auto redraw = [&]() {
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
                if(d) { a.phone_tab = (a.phone_tab + d + tabs_count) % tabs_count; redraw(); bn::sound_items::sfx_menu.play(); }
                if(bn::keypad::start_pressed()) { sheet = true; sel = 0; redraw(); }
                if(bn::keypad::b_pressed() || bn::keypad::select_pressed()) return finish(pause_result::resume);
            }
            next_frame();
        }
    }

    const char* gear_stat_name(core::gear_stat s)
    {
        return s == core::gear_stat::def ? "Obrona" : (s == core::gear_stat::dmg ? "Obrażenia" : "Max HP");
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
            phone_text(a, t, list_x, row_py(row + 1), clip(m.s, 34).c_str(), is_new ? ink::brand : ink::dim);
        };
        item_rows(0, g.equipped[slot], g.equipped_trait[slot], false);
        item_rows(2, g.offer_rarity, g.offer_trait, true);
        core::message cm; cm.add("Cecha: ").add(data::gear_traits[g.offer_trait].name);
        phone_text(a, t, list_x, row_py(4), clip(cm.s, 34).c_str(), ink::dim);
        core::message km; km.add("A: zakładam  B: zostawiam (+").add(data::gear_decline_xp + g.offer_rarity).add(")");
        phone_text(a, t, list_x, row_py(5), km.s, g.offer_is_better() ? ink::done : ink::dark);
        ph.commit();
        bn::sound_items::sfx_notify.play(bn::fixed(0.7));
        wait_release();
        while(g.has_offer())
        {
            if(bn::keypad::a_pressed()) { g.accept_offer(); bn::sound_items::sfx_buy.play(); }
            else if(bn::keypad::b_pressed()) { g.decline_offer(); bn::sound_items::sfx_menu.play(); }
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
        core::message sub; sub.add("Akt ").add(roman(data::stages[g.stage].act)).add(", ").add(g.stage + 1).add("/").add(data::stages_count);
        bool boss = data::stages[g.stage].boss >= 0;
        core::message i1;
        if(boss) i1.add("Uwaga: ").add(clip(data::enemies[data::stages[g.stage].boss].name, 18).c_str()).add("!");
        else i1.add(clip(data::stages[g.stage].name, 12).c_str()).add(": problemy ").add(g.enemy_hp_pct()).add("%");
        core::message i2; i2.add(g.ddef().name);
        if(g.tier > 0) i2.add(" NG+").add(g.tier);
        phone_message(a, g.stage_story(), sub.s, i1.s, boss ? ink::late : ink::dim, i2.s);
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
                core::message b; b.add("Wykonane! +").add(c.xp).add(" dośw.");
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
                     drop = 16, plus = 17, zzz = 18, alert = 19, marker = 20, status_icon = 21 };
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
                                                    &bn::bg_palette_items::stage_palettes_6, &bn::bg_palette_items::stage_palettes_7 };
        static_assert(data::stages_count <= 8);
        bn::regular_bg_item item(bn::regular_bg_tiles_items::tiles, *stage_pals[g.stage], map->map_item);
        bn::regular_bg_ptr bg = item.create_bg(0, 0);
        bn::regular_bg_map_ptr bg_map_ptr = bg.map();
        bn::bg_tiles::set_allow_offset(true);
        bg.set_camera(cam);

        bn::sprite_ptr hero = bn::sprite_items::actors.create_sprite(0, 0, data::classes[g.cls].frame);
        hero.set_camera(cam);
        bn::vector<bn::sprite_ptr, core::max_enemies> enemies;
        for(int i = 0; i < g.enemies_count; ++i)
        {
            bn::sprite_ptr s = bn::sprite_items::actors.create_sprite(0, 0, data::enemies[g.enemies[i].def_id].frame);
            s.set_camera(cam);
            enemies.push_back(s);
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
        bn::vector<bn::sprite_ptr, 3> status_icons;
        for(int k = 0; k < 3; ++k)
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
        auto hide_status_hud = [&]() {
            for(auto& s : status_icons) s.set_visible(false);
            status_text.clear(); shown_status = -1;
            thermos_icon.set_visible(false); thermos_text.clear(); shown_thermos = -1;
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
                        range_cells[y][x] = g.visible(x, y) && core::cheb(g.hero.x, g.hero.y, x, y) <= g.weapon().range
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
        int8_t prev_equipped[4];
        for(int i = 0; i < 4; ++i) prev_equipped[i] = g.equipped[i];
        for(int i = 0; i < g.pickups_count; ++i) prev_active += g.pickups[i].active;
        bool boss_seen = false;
        if(g.stage == 0 && g.tier == 0)   // podpowiedź na start budowy: moc pod R
        {
            core::message t; t.add("R: ").add(g.cdef().ability_name);
            banner.push(t.s, g.cdef().ability_desc);
        }
        auto detect_events = [&]() {   // powiadomienia push o ważnych zdarzeniach
            int active_pickups = 0;
            for(int i = 0; i < g.pickups_count; ++i) active_pickups += g.pickups[i].active;
            if(active_pickups < prev_active) bn::sound_items::sfx_pickup.play();   // zebrana znajdźka
            prev_active = active_pickups;
            if(g.hero_level > prev_level)
            {
                bn::sound_items::sfx_level.play();
                int old_rank = 1 + (prev_level >= 3) + (prev_level >= 5);
                if(g.ability_rank() > old_rank)
                {
                    core::message t; t.add("Moc: ").add(ability_label(g).s);
                    banner.push(t.s, "Silniejsza, szybciej gotowa");
                }
                for(int k = 0; k < 8; ++k)   // gwiazdki awansu dookoła bohatera
                {
                    static constexpr int8_t dir[8][2] = { { 2, 0 }, { 1, 1 }, { 0, 2 }, { -1, 1 }, { -2, 0 }, { -1, -1 }, { 0, -2 }, { 1, -1 } };
                    fx_particles.spawn(world(g.hero.x, g.hero.y).x(), world(g.hero.x, g.hero.y).y(), bn::fixed(dir[k][0]) / 2, bn::fixed(dir[k][1]) / 2, 0, 28, particle_pool::star);
                }
                core::message t; t.add("Awans! Poziom ").add(g.hero_level);
                core::message b; b.add("+").add(data::hp_per_level).add(" HP");
                if(data::def_levels_mask & (1 << g.hero_level)) b.add(", +1 obrona");
                if(data::dmg_levels_mask & (1 << g.hero_level)) b.add(", +1 obrażenia");
                banner.push(t.s, b.s);
            }
            if(g.weapon_override != prev_weapon && g.weapon_override >= 0)
            {
                const core::weapon_def& w = g.weapon();
                core::message b; b.add(w.name).add(" ").add(w.min_damage).add("-").add(w.max_damage);
                banner.push("Nowe narzędzie", clip(b.s, 24).c_str());
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
            if(g.st == core::status::stage_clear)   // ważniejsze niż kolejka: od razu, zanim zmieni się scena
            {
                bn::sound_items::sfx_stage.play();
                banner.hide();
                banner.push("Etap zaliczony", clip(data::stages[g.stage].name, 24).c_str());
            }
            if(g.st == core::status::stage_clear || g.st == core::status::won)   // odznaki i zlecenia
            {
                int got = core::check_badges(a.save, g);
                int done = core::check_contracts(a.save);
                push_achievements(banner, got, done);
                bn::sram::write(a.save);   // liczniki zleceń przeniesione do profilu
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
        auto hide_mini_bars = [&]() { for(auto& mb : mini_bars) if(mb) mb->set_visible(false); };
        uint32_t prev_awake = 0;

        // Animacja: płynny ruch między polami (4 px/klatkę) i 2 klatki "oddechu"/kroku.
        bn::fixed_point hero_cur = world(g.hero.x, g.hero.y), hero_dst = hero_cur;
        bn::vector<bn::fixed_point, core::max_enemies> enemy_cur, enemy_dst;
        for(int i = 0; i < g.enemies_count; ++i) { enemy_cur.push_back(world(g.enemies[i].x, g.enemies[i].y)); enemy_dst.push_back(enemy_cur.back()); }
        int anim_clock = 0, hero_shown = -1;
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
                int active[3], n = 0;
                const core::status_effect order[3] = { core::status_effect::poison, core::status_effect::shock, core::status_effect::slip };
                for(int k = 0; k < 3; ++k) if(g.status_turns(order[k]) > 0) active[n++] = k;
                status_sprite.set_visible(n > 0);
                if(n > 0)
                {
                    status_sprite.set_tiles(bn::sprite_items::particles.tiles_item(), particle_pool::status_icon + active[(anim_clock / 40) % n]);
                    status_sprite.set_position(hero_cur.x() + 7, hero_cur.y() - 12);
                }
            }
            int hf = data::classes[g.cls].frame + ((phase || hero_moving) ? frame_anim_b : 0);
            if(hf != hero_shown) { hero.set_tiles(bn::sprite_items::actors.tiles_item(), hf); hero_shown = hf; }
            for(int i = 0; i < g.enemies_count; ++i)
            {
                approach(enemy_cur[i], enemy_dst[i]);
                enemies[i].set_position(enemy_cur[i]);
                if(mini_bars[i]) mini_bars[i]->set_position(enemy_cur[i].x(), enemy_cur[i].y() - 11);
                if(i == marked)
                    target_marker.set_position(enemy_cur[i].x(), enemy_cur[i].y() - 19 - (((anim_clock / 10) & 1) ? 1 : 0));
                int base = data::enemies[g.enemies[i].def_id].frame;
                int ef = ((anim_clock / 20 + i) & 1) ? anim_b(base) : base;
                if(ef != enemy_shown[i]) { enemies[i].set_tiles(bn::sprite_items::actors.tiles_item(), ef); enemy_shown[i] = int8_t(ef); }
            }
            for(int i = 0; i < pickups.size(); ++i)   // znajdźki lekko podskakują
                pickups[i].set_y(world(g.pickups[i].x, g.pickups[i].y).y() - (((anim_clock / 16 + i) & 1) ? 1 : 0));
            cam_base = bn::fixed_point(clampf(hero_cur.x(), 136), clampf(hero_cur.y(), 176));
            if(shake_timer == 0) cam.set_position(cam_base);
        };

        // Efekty cząsteczkowe mocy zawodów + błysk ekranu w kolorze mocy.
        int flash_timer = 0;
        bn::color flash_color;
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
            }
            flash_timer = 6;
        };

        auto refresh = [&]() {
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
            if(snap_next)
            {
                hero_cur = hero_dst;
                for(int i = 0; i < g.enemies_count; ++i) enemy_cur[i] = enemy_dst[i];
                snap_next = false;
            }
            sync_pickups();
            for(int i = 0; i < g.pickups_count; ++i)
                pickups[i].set_visible(g.pickups[i].active && g.explored(g.pickups[i].x, g.pickups[i].y));
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
            core::message st; st.add("Etap ").add(g.stage + 1).add("/").add(data::stages_count).add(" ");
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
                    core::message line; line.add(clip(m.s, 30).c_str());
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
            update_mini_bars();
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
            fx.clear(); log.clear(); floaters.clear(); fx_particles.list.clear();
            hide_mini_bars(); target_marker.set_visible(false); status_sprite.set_visible(false);
            power_icon.set_visible(false); power_text.clear(); shown_cd = -1; hide_status_hud();
            a.text.set_left_alignment();
            a.text.generate(-116, 72, "Podgląd mapy (puść L)", log);
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
            hero.remove_affine_mat();
            hero.set_visible(true);
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
            fx.clear(); hud.clear(); log.clear(); floaters.clear();
            hp_left.set_visible(false); hp_right.set_visible(false);
            hide_mini_bars(); target_marker.set_visible(false); status_sprite.set_visible(false);
            power_icon.set_visible(false); power_text.clear(); shown_cd = -1; hide_status_hud();
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
                case 0: m.add("Atak: najbliższy cel (z").add(g.weapon().range).add(")"); break;
                case 1: m.add("Moc: ").add(ability_label(g).s);
                        if(g.ability_cd > 0) m.add(" - za ").add(g.ability_cd).add(" t."); break;
                case 2: m.add("Termos ").add(g.thermos).add("/").add(g.thermos_cap()).add(": kawa +").add(g.coffee_heal()).add(" HP"); break;
                case 3: m.add("Czekaj turę"); break;
                default: m.add("Akcje: wybierz strzałką"); break;
            }
            a.text.generate(-116, 56, clip(m.s, 34).c_str(), log);
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
            bg.set_visible(true);
            hero.set_visible(true);
            hp_left.set_visible(true); hp_right.set_visible(true);
            create_strips();
            draw_strips(strips_bottom);
            snap_next = true;
            refresh();
            hold = 0;
        };

        while(true)
        {
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
                    if(look_shown != look_list[look_sel])   // karta wroga w miejscu dziennika
                    {
                        look_shown = look_list[look_sel];
                        const core::enemy_def& ed = data::enemies[e.def_id];
                        log.clear();
                        a.text.set_left_alignment();
                        a.text.set_palette_item(bn::sprite_palette_items::font_map_loot);
                        core::message l1; l1.add(ed.name).add("  HP ").add(e.hp).add("/").add(e.max_hp);
                        l1.add("  obr. ").add(ed.min_damage + g.enemy_dmg_bonus()).add("-").add(ed.max_damage + g.enemy_dmg_bonus());
                        a.text.generate(-116, 56, clip(l1.s, 34).c_str(), log);
                        a.text.set_palette_item(bn::sprite_items::font_8x16.palette_item());
                        a.text.generate(-116, 72, ed.desc, log);
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
                continue;
            }

            if(acted) refresh();
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
                for(int k = 0; k < 3; ++k) key = key * 128 + core::imax(0, g.status_turns(hud_statuses[k]));
                if(key != shown_status)
                {
                    shown_status = key;
                    status_text.clear();
                    status_count = 0;
                    a.text.set_palette_item(bn::sprite_palette_items::font_map_bad);
                    a.text.set_bg_priority(0);
                    a.text.set_left_alignment();
                    for(int k = 0; k < 3; ++k)
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
                for(int k = 0; k < 3; ++k) status_icons[k].set_visible(! banner_on && k < status_count);
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
                thermos_icon.set_visible(! banner_on);
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
                for(int i = 0; i < 90; ++i)
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

    // ------------------------------------------------------------------ harmonogram między etapami
    scene run_schedule(app& a)
    {
        core::game& g = *a.g;
        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        text_sprites t;
        a.text.set_center_alignment();
        a.text.generate(0, -68, "Harmonogram budowy", t);
        a.text.set_left_alignment();
        int first = core::imax(0, core::imin(g.stage - 1, data::stages_count - 5));   // okno 5 etapów
        for(int r = 0; r < 5 && first + r < data::stages_count; ++r)
        {
            int i = first + r;
            core::message m;
            m.add(i <= g.stage ? "[x] " : (i == g.stage + 1 ? "[>] " : "[ ] ")).add(i + 1).add(". ").add(data::stages[i].name);
            a.text.generate(-104, -44 + r * 16, clip(m.s, 27), t);
        }
        a.text.set_center_alignment();
        core::message s; s.add("Wynik: ").add(g.score).add("  Dni: ").add(g.turns);
        a.text.generate(0, 44, s.s, t);
        a.text.generate(0, 62, "Kawa: +5 HP   A: dalej", t);
        while(true)
        {
            if(bn::keypad::a_pressed() || bn::keypad::start_pressed())
            {
                wait_release();
                if(g.act_cleared) return leave(scene::hurtownia);   // koniec aktu: zakupy przed kolejnym
                g.next_stage();
                return leave(scene::game);
            }
            next_frame();
        }
    }

    // ------------------------------------------------------------------ ekran końcowy z QR
    scene run_end(app& a)
    {
        core::game& g = *a.g;
        bool won = g.st == core::status::won;
        if(g.score > a.save.best) a.save.best = g.score;
        if(won) ++a.save.wins;
        if(won) core::add_house(a.save, g);
        int badges_got = core::check_badges(a.save, g);   // katalog, narzędzia, Osiedle, liczniki zleceń
        int contracts_got = core::check_contracts(a.save);
        int gained = core::bank_xp(a.save, g);
        bn::sram::write(a.save);
        clear_run(a);
        play_song(song::none);
        (won ? bn::sound_items::sfx_level : bn::sound_items::sfx_hurt).play();
        {
            core::message i1; i1.add("Wynik ").add(g.score).add("  Dośw. +").add(gained);
            phone_message(a, won ? data::story_win : data::story_lose, won ? "Odbiór" : "Budowa", i1.s, ink::dark,
                          won ? "Dom na Osiedlu!" : "Dośw. zostaje");
            leave(scene::end);
        }

        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        bn::regular_bg_ptr bg = bn::regular_bg_items::end.create_bg(8, 48);
        bn::bg_palette_color_hbe_ptr gradient = make_gradient(bg, screen_info::end_bg_index);
        text_sprites t;
        a.text.set_center_alignment();
        a.text.generate(0, 34, won ? "ODBIÓR ZALICZONY!" : "BUDOWA WSTRZYMANA", t);
        core::message s; s.add("Wynik ").add(g.score).add("  Dośw. +").add(gained);
        a.text.generate(0, 52, s.s, t);
        a.text.generate(0, 70, won ? "A: kolejna  START: koniec" : "START: nowa budowa", t);
        push_banner banner;   // odznaki i zlecenia zdobyte na koniec budowy (np. Stały klient)
        push_achievements(banner, badges_got, contracts_got);
        while(true)
        {
            banner.update(a);
            if(won && bn::keypad::a_pressed()) { g.new_game_plus(); wait_release(); return leave(scene::game); }
            if(bn::keypad::start_pressed() || (! won && bn::keypad::a_pressed())) { wait_release(); return leave(scene::shop); }
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
            bn::regular_bg_item item(bn::regular_bg_tiles_items::tiles, bn::bg_palette_items::stage_palettes_0, map->map_item);
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
            core::message sub; sub.add("Budżet: ").add(g.cash).add(" zł");
            phone_header(a, ph, t, "Hurtownia", sub.s);
            phone_canvas& c = *ph.canvas;
            core::message b; b.add("Premia za akt ").add(roman(data::stages[g.stage].act)).add(": +").add(g.act_bonus).add(" zł");
            phone_text(a, t, list_x, row_py(0), note ? note : b.s, note ? ink::brand : ink::done);
            for(int r = 0; r < 4 && top + r < data::hurtownia_count; ++r)   // 4 wiersze, przewijane
            {
                const core::shop_item_def& it = data::hurtownia[top + r];
                bool is_sel = top + r == sel;
                if(is_sel) stripe(c, r + 1, phone_tile::stripe_brand);
                phone_text(a, t, list_x, row_py(r + 1), clip(it.name, 16).c_str(), is_sel ? ink::brand : ink::dark);
                core::message pr; pr.add(it.price).add(" zł");
                phone_pill(a, c, t, pill_end, row_ty(r + 1), pr.s, g.cash >= it.price ? pill::group : pill::gray);
            }
            phone_text(a, t, list_x, row_py(5), clip(data::hurtownia[sel].desc, 26).c_str(), ink::dim);
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
                if(g.hurtownia_buy(sel)) { bn::sound_items::sfx_buy.play(); note = "Kupione! START: dalej"; }
                else note = "Za mały budżet";
                redraw();
            }
            if(bn::keypad::b_pressed() || bn::keypad::start_pressed())
            {
                t.clear();
                a.text.set_palette_item(default_ink);
                wait_release();
                g.next_stage();
                return leave(scene::game);
            }
            next_frame();
        }
    }

    // ------------------------------------------------------------------ telefon profilu (z tytułu i po budowie)
    // Te same ikony zakładek, inne treści: Odznaki, Katalog usterek, Osiedle, Zespół, Koszty (sklep Szkolenia).
    scene run_shop(app& a, int tab = 4)
    {
        phone_screen ph(tab);
        bn::sprite_palette_item default_ink = a.text.palette_item();
        page_sprites t;
        bn::vector<bn::sprite_ptr, core::max_houses> houses;
        const char* profile_tabs[] = { "Odznaki", "Katalog", "Osiedle", "Zespół", "Koszty" };

        // --- sklep (zakładka Koszty)
        enum kind : uint8_t { upgrade, cls, hard, tool };
        struct entry { kind k; int8_t i; };
        bn::vector<entry, core::max_upgrades + 8 + 8 + 1> entries;
        auto rebuild = [&]() {
            entries.clear();
            for(int i = 0; i < data::upgrades_count; ++i) entries.push_back({ upgrade, int8_t(i) });
            for(int i = 0; i < data::classes_count; ++i)
                if(! core::class_unlocked(a.save, i)) entries.push_back({ cls, int8_t(i) });
            for(int i = 0; i < data::tools_count; ++i)
                if(! core::tool_unlocked(a.save, i)) entries.push_back({ tool, int8_t(i) });
            if(! core::difficulty_unlocked(a.save, data::difficulties_count - 1)) entries.push_back({ hard, 0 });
        };
        rebuild();
        auto cost_of = [&](const entry& e) {
            return e.k == upgrade ? core::upgrade_cost(a.save, e.i)
                 : e.k == cls ? data::class_cost : (e.k == tool ? data::tools[e.i].cost : data::hard_cost);
        };

        int sel = 0, top = 0;
        const char* note = nullptr;
        // Zakładka Odznaki ma strony przełączane A: Odznaki / Zlecenia.
        const char* badge_pages[] = { "Odznaki", "Zlecenia" };
        constexpr int badge_pages_count = 2;
        int page = 0;
        auto list_size = [&]() {
            switch(tab)
            {
                case 0: return page == 1 ? data::contracts_count : data::badges_count;
                case 1: return data::enemies_count;
                case 3: return data::classes_count;
                case 4: return entries.size();
                default: return 0;
            }
        };

        auto list_window = [&]() { return tab == 4 || tab == 0 ? 4 : 5; };   // widoczne wiersze listy

        auto draw_list_row = [&](int r, int i, bool is_sel) {   // zakładki 0, 1, 3
            phone_canvas& c = *ph.canvas;
            if(is_sel) stripe(c, r, phone_tile::stripe_brand);
            ink name_ink = is_sel ? ink::brand : ink::dark;
            if(tab == 0 && page == 1)   // zlecenie: nazwa + postęp licznika
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
                phone_text(a, t, list_x, row_py(r), clip(data::badges[i].name, 16).c_str(), got || is_sel ? name_ink : ink::dim);
                core::message xp; xp.add("+").add(data::badges[i].xp);
                phone_pill(a, c, t, pill_end, row_ty(r), got ? "Zdobyta" : xp.s, got ? pill::done : pill::gray);
            }
            else if(tab == 1)
            {
                bool known = a.save.catalog & (1u << i);
                core::message m; m.add("#").add(i + 1).add(" ").add(known ? clip(data::enemies[i].name, 14).c_str() : "???");
                phone_text(a, t, list_x, row_py(r), m.s, known || is_sel ? name_ink : ink::dim);
                phone_pill(a, c, t, pill_end, row_ty(r), known ? "ZAMKNIĘTA" : "NIEZNANA", known ? pill::done : pill::gray);
            }
            else
            {
                bool won = a.save.class_wins & (1u << i), unl = core::class_unlocked(a.save, i);
                phone_text(a, t, list_x, row_py(r), clip(data::classes[i].name, 16).c_str(), unl || is_sel ? name_ink : ink::dim);
                phone_pill(a, c, t, pill_end, row_ty(r), won ? "Wygrana" : (unl ? "Dostępny" : "Zablok."),
                           won ? pill::done : (unl ? pill::group : pill::gray));
            }
        };

        auto redraw = [&]() {
            houses.clear();
            phone_canvas& c = *ph.canvas;
            core::message sub;
            if(tab == 4) sub.add("A: kup  B: wyjdź");
            else if(tab == 0)
            {
                int n = 0, total = page == 1 ? data::contracts_count : data::badges_count;
                for(int i = 0; i < total; ++i) n += ((page == 1 ? a.save.contracts : a.save.badges) >> i) & 1;
                sub.add(n).add("/").add(total).add("  A: ").add(badge_pages[(page + 1) % badge_pages_count]);
            }
            else if(tab == 1) { int n = 0; for(int i = 0; i < data::enemies_count; ++i) n += (a.save.catalog >> i) & 1;
                                sub.add(n).add("/").add(data::enemies_count); }
            else if(tab == 2) sub.add("Domy: ").add(int(a.save.houses_count)).add("/").add(core::max_houses);
            else { int n = 0; for(int i = 0; i < data::classes_count; ++i) n += (a.save.class_wins >> i) & 1;
                   sub.add("Wygrane ").add(n).add("/").add(data::classes_count); }
            phone_header(a, ph, t, tab == 0 ? badge_pages[page] : profile_tabs[tab], sub.s);

            if(tab == 2)   // Osiedle: domy z wygranych budów, 6 x 2 działki
            {
                phone_text(a, t, list_x, row_py(0), "Twoje ukończone budowy", ink::dim);
                for(int i = 0; i < core::max_houses; ++i)
                {
                    int frame = 24;
                    if(i < a.save.houses_count) frame = (a.save.houses[i] >> 4) * 6 + (a.save.houses[i] & 15);
                    bn::sprite_ptr hs = bn::sprite_items::houses.create_sprite(30 + (i % 6) * 36 - 120, 64 + (i / 6) * 28 - 80, frame);
                    hs.set_bg_priority(1);
                    houses.push_back(hs);
                }
                core::message best; best.add("Najlepszy wynik: ").add(int(a.save.best));
                phone_text(a, t, list_x, row_py(5), best.s, ink::dim);
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
                                 : (se.k == cls ? "Nowy zawód do wyboru" : (se.k == tool ? tool_desc.s : "Najwyższa trudność")));
                phone_text(a, t, list_x, row_py(1), clip(desc, 26).c_str(), ink::dim);
                for(int r = 0; r < 4 && top + r < entries.size(); ++r)
                {
                    const entry& e = entries[top + r];
                    bool is_sel = top + r == sel;
                    core::message m;
                    if(e.k == upgrade) m.add(data::upgrades[e.i].name).add(" ").add(a.save.levels[e.i]).add("/").add(data::upgrades[e.i].levels);
                    else if(e.k == cls) m.add("Zawód: ").add(data::classes[e.i].name);
                    else if(e.k == tool) m.add(data::weapons[data::tools[e.i].weapon].name);
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
            if(tab == 0 && page == 1)
            {
                const core::contract_def& cd = data::contracts[sel];
                phone_text(a, t, list_x, row_py(4), clip(cd.desc, 30).c_str(), ink::dim);
                core::message rm; rm.add("Nagroda: +").add(cd.xp).add(" dośw.");
                phone_text(a, t, list_x, row_py(5), clip(rm.s, 34).c_str(), core::contract_done(a.save, sel) ? ink::done : ink::dim);
                ph.commit();
                return;
            }
            if(tab == 0)
            {
                bool got = a.save.badges & (1u << sel);
                phone_text(a, t, list_x, row_py(4), clip(data::badges[sel].desc, 30).c_str(), ink::dim);
                core::message pm; pm.add("Premia: ");
                core::perk_label(pm, data::badges[sel].bonus);
                phone_text(a, t, list_x, row_py(5), clip(pm.s, 34).c_str(), got ? ink::done : ink::dim);
                ph.commit();
                return;
            }
            else if(tab == 1) desc = (a.save.catalog & (1u << sel)) ? data::enemies[sel].desc : "Pokonaj, żeby poznać";
            else desc = data::classes[sel].ability_desc;
            phone_text(a, t, list_x, row_py(5), clip(desc, 26).c_str(), ink::dim);
            ph.commit();
        };
        ph.set_tab(tab);
        redraw();

        while(true)
        {
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
            if(tab == 4 && bn::keypad::a_pressed())
            {
                const entry e = entries[sel];
                bool ok = e.k == upgrade ? core::buy_upgrade(a.save, e.i)
                        : e.k == cls ? core::buy_class(a.save, e.i)
                        : e.k == tool ? core::buy_tool(a.save, e.i) : core::buy_hard(a.save);
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
        }
    }
}
