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
#include "bn_regular_bg_items_title.h"
#include "bn_regular_bg_items_end.h"
#include "bn_regular_bg_tiles_items_tiles.h"
#include "bn_bg_palette_items_stage_palettes.h"

#include "core.h"

namespace
{
    // ------------------------------------------------------------------ font z polskimi znakami
    constexpr bn::utf8_character pl_chars[] = {
        "ą", "ć", "ę", "ł", "ń", "ó", "ś", "ź", "ż", "Ą", "Ć", "Ę", "Ł", "Ń", "Ó", "Ś", "Ź", "Ż"
    };
    constexpr bn::span<const bn::utf8_character> pl_chars_span(pl_chars);
    constexpr auto pl_chars_map = bn::utf8_characters_map<pl_chars_span>();
    constexpr bn::sprite_font font(bn::sprite_items::font_8x16, pl_chars_map.reference());

    using text_sprites = bn::vector<bn::sprite_ptr, 48>;
    using page_sprites = bn::vector<bn::sprite_ptr, 80>;   // pełnoekranowe strony menu

    enum class scene { title, class_select, game, schedule, end };

    constexpr int frame_coffee = 15;
    constexpr int frame_fx = 18;

    // ------------------------------------------------------------------ zapis (SRAM)
    struct save_data
    {
        char magic[8];
        int32_t best = 0;
        int32_t runs = 0;
        int32_t wins = 0;
    };
    constexpr char save_magic[8] = "PBRL001";

    save_data load_save()
    {
        save_data s;
        bn::sram::read(s);
        bool ok = true;
        for(int i = 0; i < 8; ++i) if(s.magic[i] != save_magic[i]) ok = false;
        if(! ok)   // pusta/obca pamięć -> domyślne
        {
            s = save_data();
            for(int i = 0; i < 8; ++i) s.magic[i] = save_magic[i];
            bn::sram::write(s);
        }
        return s;
    }

    // ------------------------------------------------------------------ wspólny stan
    struct app
    {
        bn::sprite_text_generator text{font};
        core::game* g = nullptr;
        save_data save;
        uint32_t seed_counter = 1;
        int chosen_class = 1;
        int chosen_diff = data::default_difficulty;
    };

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
        bn::core::update();
    }

    // ------------------------------------------------------------------ ekran tytułowy
    scene run_title(app& a)
    {
        bn::bg_palettes::set_transparent_color(bn::color(3, 5, 8));
        bn::regular_bg_ptr bg = bn::regular_bg_items::title.create_bg(8, 48);   // lewy górny róg obrazu = róg ekranu
        text_sprites prompt, record;
        a.text.set_center_alignment();
        if(a.save.best > 0)
        {
            core::message m; m.add("Rekord: ").add(int(a.save.best));
            a.text.generate(0, 66, m.s, record);
        }
        int frame = 0;
        while(true)
        {
            ++a.seed_counter;
            if((frame++ % 60) == 0) { prompt.clear(); a.text.generate(0, 48, "Naciśnij START", prompt); }
            if((frame % 60) == 40) prompt.clear();
            if(bn::keypad::start_pressed() || bn::keypad::a_pressed()) { wait_release(); return scene::class_select; }
            bn::core::update();
        }
    }

    // ------------------------------------------------------------------ wybór zawodu
    scene run_class_select(app& a)
    {
        bn::bg_palettes::set_transparent_color(bn::color(3, 5, 8));
        text_sprites header, lines, diff_line;
        a.text.set_center_alignment();
        a.text.generate(0, -72, "Wybierz fach", header);
        bn::sprite_ptr hero = bn::sprite_items::actors.create_sprite(0, -30, 0);
        hero.set_double_size_mode(bn::sprite_double_size_mode::ENABLED);
        hero.set_scale(2);

        auto redraw = [&]() {
            lines.clear();
            const core::class_def& c = data::classes[a.chosen_class];
            const core::weapon_def& w = data::weapons[c.weapon];
            hero.set_tiles(bn::sprite_items::actors.tiles_item(), c.frame);
            core::message n; n.add("< ").add(c.name).add(" >");
            a.text.generate(0, 4, n.s, lines);
            a.text.generate(0, 20, clip(c.desc, 29), lines);
            core::message s; s.add("HP ").add(c.max_health).add(" SIŁ ").add(c.strength).add(" ZRĘ ").add(c.agility);
            a.text.generate(0, 38, s.s, lines);
            core::message s2; s2.add("INT ").add(c.intelligence).add(" OBR ").add(c.defense);
            a.text.generate(0, 54, s2.s, lines);
            core::message wl; wl.add(w.name).add(" ").add(w.min_damage).add("-").add(w.max_damage).add(" z").add(w.range);
            a.text.generate(0, 72, clip(wl.s, 29), lines);
        };
        auto redraw_diff = [&]() {
            diff_line.clear();
            a.text.set_center_alignment();
            core::message m; m.add("Poziom (góra/dół): ").add(data::difficulties[a.chosen_diff].name);
            a.text.generate(0, -56, m.s, diff_line);
        };
        redraw();
        redraw_diff();

        while(true)
        {
            if(bn::keypad::up_pressed()) { a.chosen_diff = (a.chosen_diff + data::difficulties_count - 1) % data::difficulties_count; redraw_diff(); }
            if(bn::keypad::down_pressed()) { a.chosen_diff = (a.chosen_diff + 1) % data::difficulties_count; redraw_diff(); }
            if(bn::keypad::left_pressed()) { a.chosen_class = (a.chosen_class + data::classes_count - 1) % data::classes_count; redraw(); }
            if(bn::keypad::right_pressed()) { a.chosen_class = (a.chosen_class + 1) % data::classes_count; redraw(); }
            if(bn::keypad::a_pressed() || bn::keypad::start_pressed())
            {
                a.g->new_run(a.chosen_class, a.seed_counter * 2654435761u + 12345u, a.chosen_diff);
                ++a.save.runs;
                bn::sram::write(a.save);
                wait_release();
                return scene::game;
            }
            if(bn::keypad::b_pressed()) { wait_release(); return scene::title; }
            bn::core::update();
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

        void build(const core::level& lv, int palette)
        {
            for(int y = 0; y < core::map_h; ++y)
                for(int x = 0; x < core::map_w; ++x)
                {
                    int t = 2;                                                     // mur
                    core::tile k = lv.at(x, y);
                    if(k == core::tile::floor) t = 1;
                    else if(k == core::tile::stairs) t = 4;
                    else if(lv.at(x, y + 1) != core::tile::wall) t = 3;            // lico muru nad podłogą
                    for(int dy = 0; dy < 2; ++dy)
                        for(int dx = 0; dx < 2; ++dx)
                        {
                            bn::regular_bg_map_cell& cell = cells[map_item.cell_index(x * 2 + dx, y * 2 + dy)];
                            bn::regular_bg_map_cell_info info(cell);
                            info.set_tile_index(t);
                            info.set_palette_id(palette);
                            info.set_horizontal_flip(false);
                            cell = info.cell();
                        }
                }
        }
    };

    bn::fixed clampf(bn::fixed v, int lim) { return v < -lim ? bn::fixed(-lim) : (v > lim ? bn::fixed(lim) : v); }

    bn::fixed_point world(int x, int y) { return bn::fixed_point(x * 16 + 8 - 256, y * 16 + 8 - 256); }

    // ------------------------------------------------------------------ menu pod SELECT (pauza)
    void wait_page_close()
    {
        while(! (bn::keypad::a_pressed() || bn::keypad::b_pressed() || bn::keypad::select_pressed()))
            bn::core::update();
        wait_release();
    }

    void page_character(app& a)
    {
        const core::game& g = *a.g;
        const core::class_def& c = g.cdef();
        const core::weapon_def& w = g.weapon();
        page_sprites t;
        a.text.set_center_alignment();
        a.text.generate(0, -68, c.name, t);
        a.text.set_left_alignment();
        core::message l[7];
        l[0].add("Poziom: ").add(g.ddef().name);
        if(g.tier > 0) l[0].add(" NG+").add(g.tier);
        l[1].add("HP ").add(g.hero.hp).add("/").add(g.hero.max_hp).add("  OBR ").add(c.defense).add("+").add(g.def_bonus);
        l[2].add("SIŁ ").add(c.strength).add(" ZRĘ ").add(c.agility).add(" INT ").add(c.intelligence);
        l[3].add(w.name).add(" ").add(w.min_damage).add("-").add(w.max_damage).add(" z").add(w.range);
        l[4].add("Premia obrażeń: +").add(g.dmg_bonus);
        l[5].add("Wynik ").add(g.score).add("  Dni ").add(g.turns);
        l[6].add("Usunięte problemy: ").add(g.kills);
        for(int i = 0; i < 7; ++i) a.text.generate(-108, -46 + i * 16, clip(l[i].s, 27), t);
        a.text.set_center_alignment();
        a.text.generate(0, 72, "B: wróć", t);
        wait_page_close();
    }

    void page_schedule(app& a)
    {
        const core::game& g = *a.g;
        page_sprites t;
        a.text.set_center_alignment();
        a.text.generate(0, -68, "Harmonogram budowy", t);
        a.text.set_left_alignment();
        for(int i = 0; i < data::stages_count; ++i)
        {
            core::message m;
            m.add(i < g.stage ? "[x] " : (i == g.stage ? "[>] " : "[ ] ")).add(data::stages[i].name);
            a.text.generate(-100, -44 + i * 16, clip(m.s, 27), t);
        }
        a.text.set_center_alignment();
        core::message s; s.add("Siła problemów: ").add(g.enemy_hp_pct()).add("% HP");
        a.text.generate(0, 44, s.s, t);
        a.text.generate(0, 72, "B: wróć", t);
        wait_page_close();
    }

    void page_controls(app& a)
    {
        page_sprites t;
        a.text.set_center_alignment();
        a.text.generate(0, -68, "Sterowanie", t);
        a.text.set_left_alignment();
        const char* lines[] = { "D-pad: ruch / atak", "Przytrzymaj: szybki ruch", "A: atak narzędziem",
                                "B: czekaj (odpoczynek)", "SELECT: menu" };
        for(int i = 0; i < 5; ++i) a.text.generate(-100, -44 + i * 16, lines[i], t);
        a.text.set_center_alignment();
        a.text.generate(0, 72, "B: wróć", t);
        wait_page_close();
    }

    // Zwraca true, jeśli gracz porzucił budowę.
    bool run_pause(app& a)
    {
        const char* items[] = { "Wznów", "Karta postaci", "Harmonogram", "Sterowanie", "Porzuć budowę" };
        constexpr int items_count = 5;
        int sel = 0;
        bool confirm = false;
        page_sprites t;
        auto redraw = [&]() {
            t.clear();
            a.text.set_center_alignment();
            a.text.generate(0, -68, "Przerwa", t);
            for(int i = 0; i < items_count; ++i)
            {
                core::message m; m.add(i == sel ? "> " : "  ").add(items[i]).add(i == sel ? " <" : "  ");
                a.text.generate(0, -40 + i * 16, m.s, t);
            }
            a.text.generate(0, 72, confirm ? "Na pewno? A: tak  B: nie" : "A: wybierz  B: wróć", t);
        };
        redraw();
        wait_release();
        while(true)
        {
            if(confirm)
            {
                if(bn::keypad::a_pressed()) { wait_release(); return true; }
                if(bn::keypad::b_pressed()) { confirm = false; redraw(); }
            }
            else
            {
                if(bn::keypad::up_pressed()) { sel = (sel + items_count - 1) % items_count; redraw(); }
                if(bn::keypad::down_pressed()) { sel = (sel + 1) % items_count; redraw(); }
                if(bn::keypad::b_pressed() || bn::keypad::select_pressed() || bn::keypad::start_pressed()) { wait_release(); return false; }
                if(bn::keypad::a_pressed())
                {
                    if(sel == 0) { wait_release(); return false; }
                    if(sel == 4) { confirm = true; redraw(); }
                    else
                    {
                        t.clear();
                        wait_release();
                        if(sel == 1) page_character(a);
                        else if(sel == 2) page_schedule(a);
                        else page_controls(a);
                        redraw();
                    }
                }
            }
            bn::core::update();
        }
    }

    scene run_game(app& a)
    {
        core::game& g = *a.g;
        bn::bg_palettes::set_transparent_color(bn::color(1, 1, 3));
        bn::camera_ptr cam = bn::camera_ptr::create(0, 0);

        bn::bg_tiles::set_allow_offset(false);
        bn::unique_ptr<bg_map> map(new bg_map());
        map->build(g.lv, g.stage);
        bn::regular_bg_item item(bn::regular_bg_tiles_items::tiles, bn::bg_palette_items::stage_palettes, map->map_item);
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
        for(int i = 0; i < g.pickups_count; ++i)
        {
            bn::sprite_ptr s = bn::sprite_items::actors.create_sprite(world(g.pickups[i].x, g.pickups[i].y), frame_coffee + g.pickups[i].type);
            s.set_camera(cam);
            s.set_z_order(10);
            pickups.push_back(s);
        }
        bn::vector<bn::sprite_ptr, core::max_enemies> fx;

        text_sprites hud, log;
        a.text.set_bg_priority(0);
        a.text.set_z_order(-100);
        int fx_timer = 0, hurt_timer = 0, hold = 0;

        auto refresh = [&]() {
            hero.set_position(world(g.hero.x, g.hero.y));
            for(int i = 0; i < g.enemies_count; ++i)
            {
                enemies[i].set_visible(g.enemies[i].alive);
                enemies[i].set_position(world(g.enemies[i].x, g.enemies[i].y));
            }
            for(int i = 0; i < g.pickups_count; ++i) pickups[i].set_visible(g.pickups[i].active);
            bn::fixed_point c = world(g.hero.x, g.hero.y);
            cam.set_position(clampf(c.x(), 136), clampf(c.y(), 176));

            hud.clear();
            a.text.set_left_alignment();
            core::message top; top.add("HP ").add(g.hero.hp).add("/").add(g.hero.max_hp);
            a.text.generate(-116, -72, top.s, hud);
            a.text.set_right_alignment();
            core::message st; st.add(clip(data::stages[g.stage].name, 11).c_str()).add(" ").add(g.stage + 1).add("/").add(data::stages_count);
            a.text.generate(116, -72, st.s, hud);

            log.clear();
            a.text.set_left_alignment();
            a.text.generate(-116, 56, clip(g.log[core::log_lines - 2].s, 29), log);
            a.text.generate(-116, 72, clip(g.log[core::log_lines - 1].s, 29), log);

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
            if(g.hero_hit) hurt_timer = 16;
            g.turn_events = 0;
            g.hero_hit = false;
        };
        refresh();

        while(true)
        {
            bool acted = false;
            int dx = 0, dy = 0;
            // ruch: pojedyncze wciśnięcie lub przytrzymanie (auto-powtórzenie)
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
            else if(bn::keypad::a_pressed()) { acted = g.player_attack_nearest(); if(! acted) refresh(); }
            else if(bn::keypad::b_pressed()) acted = g.player_wait();
            // skrót pokazowy/testowy: L+R+SELECT = zalicz etap; samo SELECT = menu
            if(bn::keypad::select_pressed() && bn::keypad::l_held() && bn::keypad::r_held()) { g.debug_skip(); refresh(); }
            else if(bn::keypad::select_pressed())
            {
                bg.set_visible(false);
                hero.set_visible(false);
                for(auto& s : enemies) s.set_visible(false);
                for(auto& s : pickups) s.set_visible(false);
                fx.clear(); hud.clear(); log.clear();
                bool quit = run_pause(a);
                if(quit)
                {
                    if(g.score > a.save.best) { a.save.best = g.score; bn::sram::write(a.save); }
                    return scene::title;
                }
                bg.set_visible(true);
                hero.set_visible(true);
                refresh();
                hold = 0;
                continue;
            }

            if(acted) refresh();

            if(fx_timer > 0 && --fx_timer == 0) fx.clear();
            if(hurt_timer > 0) { --hurt_timer; hero.set_visible((hurt_timer & 2) == 0); }

            if(g.st == core::status::stage_clear) { for(int i = 0; i < 30; ++i) bn::core::update(); return scene::schedule; }
            if(g.st == core::status::dead || g.st == core::status::won)
            {
                hero.set_visible(true);
                for(int i = 0; i < 90; ++i) bn::core::update();
                return scene::end;
            }
            bn::core::update();
        }
    }

    // ------------------------------------------------------------------ harmonogram między etapami
    scene run_schedule(app& a)
    {
        core::game& g = *a.g;
        bn::bg_palettes::set_transparent_color(bn::color(3, 5, 8));
        text_sprites t;
        a.text.set_center_alignment();
        a.text.generate(0, -68, "Harmonogram budowy", t);
        a.text.set_left_alignment();
        for(int i = 0; i < data::stages_count; ++i)
        {
            core::message m;
            m.add(i <= g.stage ? "[x] " : (i == g.stage + 1 ? "[>] " : "[ ] ")).add(data::stages[i].name);
            a.text.generate(-100, -44 + i * 16, clip(m.s, 27), t);
        }
        a.text.set_center_alignment();
        core::message s; s.add("Wynik: ").add(g.score).add("  Dni: ").add(g.turns);
        a.text.generate(0, 44, s.s, t);
        a.text.generate(0, 62, "Kawa: +5 HP   A: dalej", t);
        while(true)
        {
            if(bn::keypad::a_pressed() || bn::keypad::start_pressed()) { g.next_stage(); wait_release(); return scene::game; }
            bn::core::update();
        }
    }

    // ------------------------------------------------------------------ ekran końcowy z QR
    scene run_end(app& a)
    {
        core::game& g = *a.g;
        bool won = g.st == core::status::won;
        if(g.score > a.save.best) a.save.best = g.score;
        if(won) ++a.save.wins;
        bn::sram::write(a.save);

        bn::bg_palettes::set_transparent_color(bn::color(3, 5, 8));
        bn::regular_bg_ptr bg = bn::regular_bg_items::end.create_bg(8, 48);
        text_sprites t;
        a.text.set_center_alignment();
        a.text.generate(0, 34, won ? "ODBIÓR ZALICZONY!" : "BUDOWA WSTRZYMANA", t);
        core::message s; s.add("Wynik ").add(g.score).add("  Rekord ").add(int(a.save.best));
        a.text.generate(0, 52, s.s, t);
        a.text.generate(0, 70, won ? "A: kolejna  START: koniec" : "START: nowa budowa", t);
        while(true)
        {
            if(won && bn::keypad::a_pressed()) { g.new_game_plus(); wait_release(); return scene::game; }
            if(bn::keypad::start_pressed() || (! won && bn::keypad::a_pressed())) { wait_release(); return scene::title; }
            bn::core::update();
        }
    }
}

int main()
{
    bn::core::init();
    app a;
    a.save = load_save();
    bn::unique_ptr<core::game> game(new core::game());
    a.g = game.get();

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
        }
    }
}
