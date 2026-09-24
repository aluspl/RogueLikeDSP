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
#include "bn_sprite_palettes.h"
#include "bn_regular_bg_items_title.h"
#include "bn_regular_bg_items_end.h"
#include "bn_regular_bg_tiles_items_tiles.h"
#include "bn_bg_palette_items_stage_palettes.h"

#include "core.h"
#include "meta.h"
#include "phone_tiles.h"

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

    enum class scene { title, class_select, game, schedule, end, shop, help };

    constexpr int frame_coffee = 15;
    constexpr int frame_fx = 18;
    constexpr int frame_lock = 19;
    constexpr int frame_silhouette = 20;   // + indeks zawodu
    constexpr int frame_toolbox = 26;

    int pickup_frame(const core::pickup& p) { return p.type == core::tool ? frame_toolbox : frame_coffee + p.type; }

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

    // ------------------------------------------------------------------ ekran tytułowy
    scene run_title(app& a)
    {
        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        bn::regular_bg_ptr bg = bn::regular_bg_items::title.create_bg(8, 48);   // lewy górny róg obrazu = róg ekranu
        text_sprites prompt, record;
        a.text.set_center_alignment();
        if(a.save.best > 0)
        {
            core::message m; m.add("Rekord: ").add(int(a.save.best));
            a.text.set_right_alignment();
            a.text.generate(116, -72, m.s, record);
            a.text.set_center_alignment();
        }
        text_sprites shop_hint;
        a.text.generate(0, 66, "SELECT: szkolenia  B: pomoc", shop_hint);
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
            if(bn::keypad::left_pressed()) { a.chosen_class = (a.chosen_class + data::classes_count - 1) % data::classes_count; redraw(); }
            if(bn::keypad::right_pressed()) { a.chosen_class = (a.chosen_class + 1) % data::classes_count; redraw(); }
            if((bn::keypad::a_pressed() || bn::keypad::start_pressed()) && core::class_unlocked(a.save, a.chosen_class))
            {
                a.g->new_run(a.chosen_class, a.seed_counter * 2654435761u + 12345u, a.chosen_diff, core::mods(a.save));
                ++a.save.runs;
                bn::sram::write(a.save);
                wait_release();
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

        static constexpr int dim_palettes = 5;   // palety 5..9 = przyciemnione etapy

        void set(int cx, int cy, int t, int palette)
        {
            bn::regular_bg_map_cell& cell = cells[map_item.cell_index(cx, cy)];
            bn::regular_bg_map_cell_info info(cell);
            info.set_tile_index(t);
            info.set_palette_id(palette);
            info.set_horizontal_flip(false);
            cell = info.cell();
        }

        // Kafel pola z uwzględnieniem mgły wojny: 0 = nieznane (czarne).
        static int tile_of(const core::game& g, int x, int y, int& palette)
        {
            palette = g.stage + (g.visible(x, y) ? 0 : dim_palettes);
            if(! g.explored(x, y)) return 0;
            core::tile k = g.lv.at(x, y);
            if(k == core::tile::floor) return 1;
            if(k == core::tile::stairs) return 4;
            return g.lv.at(x, y + 1) != core::tile::wall ? 3 : 2;   // lico muru nad podłogą / mur
        }

        // Widok gry: pole 16x16 = 2x2 kafle 8x8.
        void build(const core::game& g)
        {
            for(int y = 0; y < core::map_h; ++y)
                for(int x = 0; x < core::map_w; ++x)
                {
                    int pal, t = tile_of(g, x, y, pal);
                    for(int dy = 0; dy < 2; ++dy) for(int dx = 0; dx < 2; ++dx) set(x * 2 + dx, y * 2 + dy, t, pal);
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
        const char* lines[] = { "Przejdź 5 etapów budowy,", "na końcu pokonaj Termin.", "Schody = koniec etapu.",
                                "D-pad: ruch i atak wręcz", "A: atak  B: czekaj", "R: moc zawodu",
                                "SELECT: menu  L: mapa" };
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

    constexpr const char* tab_names[] = { "Zadania", "Usterki", "Start", "Zespół", "Koszty" };
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
        core::message sub; sub.add("Etap ").add(g.stage + 1).add("/").add(data::stages_count);
        phone_header(a, ph, t, tab_names[0], sub.s);
        phone_canvas& c = *ph.canvas;
        for(int i = 0; i < data::stages_count; ++i)
        {
            bool done = i < g.stage || (i == g.stage && g.st == core::status::won);
            bool cur = i == g.stage && ! done;
            stripe(c, i, done ? phone_tile::stripe_done : (cur ? phone_tile::stripe_prog : phone_tile::stripe_todo));
            phone_text(a, t, list_x, row_py(i), clip(data::stages[i].name, 15).c_str(), done ? ink::dim : ink::dark);
            phone_pill(a, c, t, pill_end, row_ty(i), done ? "Gotowe" : (cur ? "W trakcie" : "Do zrob."),
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

    void tab_home(app& a, phone_screen& ph, page_sprites& t)   // Start = pulpit postaci
    {
        const core::game& g = *a.g;
        core::message sub; sub.add(g.ddef().name);
        if(g.tier > 0) sub.add(" NG+").add(g.tier);
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

        core::message mc; mc.add("Moc: ").add(g.cdef().ability_name);
        phone_text(a, t, list_x, row_py(3), mc.s, ink::dark);
        core::message cd; cd.add("za ").add(g.ability_cd);
        phone_pill(a, c, t, pill_end, row_ty(3), g.ability_cd == 0 ? "Gotowa" : cd.s, g.ability_cd == 0 ? pill::done : pill::gray);

        const core::weapon_def& w = g.weapon();
        core::message wl; wl.add(w.name).add(" ").add(w.min_damage).add("-").add(w.max_damage).add(" z").add(w.range);
        wl.add(" +").add(g.dmg_bonus);
        phone_text(a, t, list_x, row_py(4), clip(wl.s, 26).c_str(), ink::dim);
        core::message sc; sc.add("Wynik ").add(g.score).add("  Dzień ").add(g.turns);
        phone_text(a, t, list_x, row_py(5), sc.s, ink::dim);
    }

    void tab_team(app& a, phone_screen& ph, page_sprites& t)   // Zespół = zawody
    {
        const core::game& g = *a.g;
        phone_header(a, ph, t, tab_names[3], "Fachowcy");
        phone_canvas& c = *ph.canvas;
        for(int i = 0; i < data::classes_count && i < 6; ++i)
        {
            bool mine = i == g.cls, unl = core::class_unlocked(a.save, i);
            stripe(c, i, mine ? phone_tile::stripe_brand : (unl ? phone_tile::stripe_done : phone_tile::stripe_todo));
            phone_text(a, t, list_x, row_py(i), clip(data::classes[i].name, 16).c_str(), mine ? ink::brand : (unl ? ink::dark : ink::dim));
            phone_pill(a, c, t, pill_end, row_ty(i), mine ? "Ty" : (unl ? "Dostępny" : "Zablok."),
                       mine ? pill::brand : (unl ? pill::done : pill::gray));
        }
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
            case 3: tab_team(a, ph, t); break;
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
                if(d) { a.phone_tab = (a.phone_tab + d + tabs_count) % tabs_count; redraw(); }
                if(bn::keypad::start_pressed()) { sheet = true; sel = 0; redraw(); }
                if(bn::keypad::b_pressed() || bn::keypad::select_pressed()) return finish(pause_result::resume);
            }
            next_frame();
        }
    }

    // Karta etapu na wejściu: nazwa, siła problemów, poziom trudności.
    void stage_card(app& a)
    {
        const core::game& g = *a.g;
        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        page_sprites t;
        a.text.set_center_alignment();
        core::message l1; l1.add("Etap ").add(g.stage + 1).add("/").add(data::stages_count);
        a.text.generate(0, -40, l1.s, t);
        a.text.generate(0, -20, clip(data::stages[g.stage].name, 29), t);
        core::message l3; l3.add("Siła problemów: ").add(g.enemy_hp_pct()).add("% HP");
        a.text.generate(0, 8, l3.s, t);
        core::message l4; l4.add("Trudność: ").add(g.ddef().name);
        if(g.tier > 0) l4.add("  NG+").add(g.tier);
        a.text.generate(0, 26, l4.s, t);
        if(data::stages[g.stage].boss >= 0) a.text.generate(0, 50, "Uwaga: Termin czeka!", t);
        for(int i = 0; i < 100 && ! bn::keypad::a_pressed() && ! bn::keypad::start_pressed(); ++i) next_frame();
        leave(scene::game);   // ściemnij kartę, gra się rozjaśni
    }

    // Powiadomienie push jak z aplikacji PlanBudowlany: baner zjeżdża z góry ekranu.
    struct notice { bn::string<32> title; bn::string<32> body; };

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
            queue.push_back({ bn::string<32>(title), bn::string<32>(body) });
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
        save_run(a);   // autozapis na starcie etapu (albo po wznowieniu)
        bn::bg_palettes::set_transparent_color(bn::color(1, 1, 3));
        bn::camera_ptr cam = bn::camera_ptr::create(0, 0);

        bn::bg_tiles::set_allow_offset(false);
        bn::unique_ptr<bg_map> map(new bg_map());
        map->build(g);
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
        a.text.set_bg_priority(0);
        a.text.set_z_order(-100);
        int fx_timer = 0, hurt_timer = 0, hold = 0;
        bn::vector<floater, core::max_hits> floaters;
        int shake_timer = 0, target_timer = 0;
        bn::fixed_point cam_base;
        push_banner banner;
        int prev_level = g.hero_level, prev_weapon = g.weapon_override, prev_pickups = g.pickups_count;
        int prev_cd = g.ability_cd;
        bool boss_seen = false;
        auto detect_events = [&]() {   // powiadomienia push o ważnych zdarzeniach
            if(g.hero_level > prev_level)
            {
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
            if(g.pickups_count > prev_pickups) banner.push("Coś wypadło!", "Zajrzyj na miejsce usterki");
            if(prev_cd > 0 && g.ability_cd == 0) banner.push("Moc gotowa", g.cdef().ability_name);
            if(! boss_seen && g.boss >= 0 && g.enemies[g.boss].alive && g.visible(g.enemies[g.boss].x, g.enemies[g.boss].y))
            {
                boss_seen = true;
                banner.push("Przypisano Ci usterkę", "Nieprzekraczalny Termin");
            }
            if(g.st == core::status::stage_clear)   // ważniejsze niż kolejka: od razu, zanim zmieni się scena
            {
                banner.hide();
                banner.push("Etap zaliczony", clip(data::stages[g.stage].name, 24).c_str());
            }
            prev_level = g.hero_level; prev_weapon = g.weapon_override; prev_pickups = g.pickups_count; prev_cd = g.ability_cd;
        };

        // pasek HP celu (ostatnio trafiony wróg / boss): 2 segmenty ściśnięte do 16 px
        bn::sprite_ptr tgt_left = bn::sprite_items::hp_bar.create_sprite(0, 0, 0);
        bn::sprite_ptr tgt_right = bn::sprite_items::hp_bar.create_sprite(0, 0, 96);
        for(bn::sprite_ptr* s : { &tgt_left, &tgt_right })
        {
            s->set_camera(cam); s->set_horizontal_scale(bn::fixed(0.5)); s->set_z_order(-20); s->set_visible(false);
        }
        auto update_target_bar = [&]() {
            int ti = target_timer > 0 ? g.last_target : -1;
            if(g.boss >= 0 && g.enemies[g.boss].alive && g.enemies[g.boss].awake) ti = g.boss;
            bool show = ti >= 0 && g.enemies[ti].alive && g.visible(g.enemies[ti].x, g.enemies[ti].y);
            tgt_left.set_visible(show); tgt_right.set_visible(show);
            if(! show) return;
            const core::actor& e = g.enemies[ti];
            int fill = core::imax(1, e.hp * 62 / e.max_hp);
            int color = e.hp * 2 > e.max_hp ? 0 : (e.hp * 4 > e.max_hp ? 1 : 2);
            tgt_left.set_tiles(bn::sprite_items::hp_bar.tiles_item(), color * 32 + core::imin(fill, 31));
            tgt_right.set_tiles(bn::sprite_items::hp_bar.tiles_item(), 96 + color * 32 + core::imax(0, fill - 31));
            bn::fixed_point p = world(e.x, e.y);
            tgt_left.set_position(p.x() - 8, p.y() - 11);
            tgt_right.set_position(p.x() + 8, p.y() - 11);
        };

        auto refresh = [&]() {
            map->build(g);
            bg_map_ptr.reload_cells_ref();
            hero.set_position(world(g.hero.x, g.hero.y));
            for(int i = 0; i < g.enemies_count; ++i)
            {
                enemies[i].set_visible(g.enemies[i].alive && g.visible(g.enemies[i].x, g.enemies[i].y));
                enemies[i].set_position(world(g.enemies[i].x, g.enemies[i].y));
            }
            sync_pickups();
            for(int i = 0; i < g.pickups_count; ++i)
                pickups[i].set_visible(g.pickups[i].active && g.explored(g.pickups[i].x, g.pickups[i].y));
            bn::fixed_point c = world(g.hero.x, g.hero.y);
            cam_base = bn::fixed_point(clampf(c.x(), 136), clampf(c.y(), 176));
            cam.set_position(cam_base);

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
            if(g.ability_cd == 0) a.text.generate(20, -72, "R", hud);   // moc gotowa
            a.text.set_right_alignment();
            core::message st; st.add("Etap ").add(g.stage + 1).add("/").add(data::stages_count).add(" ");
            st.add(clip(g.ddef().name, 1).c_str());
            if(g.tier > 0) st.add("+").add(g.tier);
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
                core::message m; m.add("-").add(g.hits[i].amount);
                bn::fixed_point p = world(g.hits[i].x, g.hits[i].y);
                a.text.generate(p.x(), p.y() - 12, m.s, f.sprites);
                for(bn::sprite_ptr& sp : f.sprites) { sp.set_camera(cam); sp.set_z_order(-60); }
                f.timer = 36;
                if(! g.hits[i].on_hero) target_timer = 90;
            }
            g.hits_count = 0;
            update_target_bar();
            detect_events();
        };
        refresh();

        // Podgląd mapy: trzymaj L (samo L, bez R - L+R+SELECT to skrót pokazowy).
        auto overview = [&]() {
            map->build_overview(g);
            bg_map_ptr.reload_cells_ref();
            for(auto& s : enemies) s.set_visible(false);
            for(auto& s : pickups) s.set_visible(false);
            fx.clear(); log.clear(); floaters.clear();
            tgt_left.set_visible(false); tgt_right.set_visible(false);
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
            refresh();
            hold = 0;
        };

        while(true)
        {
            if(bn::keypad::l_held() && ! bn::keypad::r_held()) { overview(); continue; }
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
            else if(bn::keypad::r_pressed() && ! bn::keypad::l_held()) { acted = g.player_ability(); if(! acted) refresh(); }
            // skrót pokazowy/testowy: L+R+SELECT = zalicz etap; samo SELECT = menu
            if(bn::keypad::select_pressed() && bn::keypad::l_held() && bn::keypad::r_held()) { g.debug_skip(); refresh(); }
            else if(bn::keypad::select_pressed())
            {
                bg.set_visible(false);
                hero.set_visible(false);
                for(auto& s : enemies) s.set_visible(false);
                for(auto& s : pickups) s.set_visible(false);
                fx.clear(); hud.clear(); log.clear(); floaters.clear();
                hp_left.set_visible(false); hp_right.set_visible(false);
                tgt_left.set_visible(false); tgt_right.set_visible(false);
                banner.hide();
                pause_result pr = run_phone(a);
                if(pr == pause_result::save_exit) { save_run(a); return leave(scene::title); }
                if(pr == pause_result::quit)
                {
                    if(g.score > a.save.best) a.save.best = g.score;
                    core::bank_xp(a.save, g);
                    bn::sram::write(a.save);
                    clear_run(a);
                    return leave(scene::shop);
                }
                bg.set_visible(true);
                hero.set_visible(true);
                hp_left.set_visible(true); hp_right.set_visible(true);
                refresh();
                hold = 0;
                continue;
            }

            if(acted) refresh();

            if(fx_timer > 0 && --fx_timer == 0) fx.clear();
            for(int i = 0; i < floaters.size(); )
            {
                floater& f = floaters[i];
                if(--f.timer <= 0) { floaters.erase(floaters.begin() + i); continue; }
                if(f.timer & 1) for(bn::sprite_ptr& sp : f.sprites) sp.set_y(sp.y() - 1);
                ++i;
            }
            if(target_timer > 0 && --target_timer == 0) update_target_bar();
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
            hp_left.set_visible(! banner_on && (! low_hp || (blink & 16)));
            hp_right.set_visible(! banner_on && (! low_hp || (blink & 16)));

            if(g.st == core::status::stage_clear)
            {
                for(int i = 0; i < 70; ++i) { banner.update(a); next_frame(); }
                return leave(scene::schedule);
            }
            if(g.st == core::status::dead || g.st == core::status::won)
            {
                hero.set_visible(true);
                for(int i = 0; i < 90; ++i) next_frame();
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
            if(bn::keypad::a_pressed() || bn::keypad::start_pressed()) { g.next_stage(); wait_release(); return leave(scene::game); }
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
        int gained = core::bank_xp(a.save, g);
        bn::sram::write(a.save);
        clear_run(a);

        bn::bg_palettes::set_transparent_color(bn::color(3, 2, 8));
        bn::regular_bg_ptr bg = bn::regular_bg_items::end.create_bg(8, 48);
        text_sprites t;
        a.text.set_center_alignment();
        a.text.generate(0, 34, won ? "ODBIÓR ZALICZONY!" : "BUDOWA WSTRZYMANA", t);
        core::message s; s.add("Wynik ").add(g.score).add("  Dośw. +").add(gained);
        a.text.generate(0, 52, s.s, t);
        a.text.generate(0, 70, won ? "A: kolejna  START: koniec" : "START: nowa budowa", t);
        while(true)
        {
            if(won && bn::keypad::a_pressed()) { g.new_game_plus(); wait_release(); return leave(scene::game); }
            if(bn::keypad::start_pressed() || (! won && bn::keypad::a_pressed())) { wait_release(); return leave(scene::shop); }
            next_frame();
        }
    }

    // ------------------------------------------------------------------ sklep "Szkolenia" = zakładka Koszty w telefonie
    scene run_shop(app& a)
    {
        phone_screen ph(4);
        bn::sprite_palette_item default_ink = a.text.palette_item();
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

        constexpr int window = 4;   // wiersze 2..5 karty
        int sel = 0, top = 0;
        const char* note = nullptr;
        page_sprites t;
        auto cost_of = [&](const entry& e) {
            return e.k == upgrade ? core::upgrade_cost(a.save, e.i)
                 : e.k == cls ? data::class_cost : (e.k == tool ? data::tools[e.i].cost : data::hard_cost);
        };
        auto redraw = [&]() {
            phone_header(a, ph, t, tab_names[4], "A: kup  B: wyjdź");
            phone_canvas& c = *ph.canvas;
            phone_text(a, t, list_x, row_py(0), "Pozostało", ink::dim);
            core::message xp; xp.add(int(a.save.xp)).add(" dośw.");
            phone_text(a, t, 226, row_py(0), xp.s, ink::done, 1);
            const entry& se = entries[sel];
            core::message tool_desc;
            if(se.k == tool)
            {
                const core::weapon_def& w = data::weapons[data::tools[se.i].weapon];
                tool_desc.add("Narzędzie ").add(w.min_damage).add("-").add(w.max_damage).add(" z").add(w.range);
            }
            const char* desc = note ? note : (se.k == upgrade ? data::upgrades[se.i].desc
                             : (se.k == cls ? "Nowy zawód do wyboru" : (se.k == tool ? tool_desc.s : "Najwyższa trudność")));
            phone_text(a, t, list_x, row_py(1), clip(desc, 26).c_str(), ink::dim);
            for(int r = 0; r < window && top + r < entries.size(); ++r)
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
        };
        redraw();

        while(true)
        {
            int dir = bn::keypad::up_pressed() ? -1 : (bn::keypad::down_pressed() ? 1 : 0);
            if(dir)
            {
                sel = (sel + dir + entries.size()) % entries.size();
                if(sel < top) top = sel;
                if(sel >= top + window) top = sel - window + 1;
                note = nullptr;
                redraw();
            }
            if(bn::keypad::a_pressed())
            {
                const entry e = entries[sel];
                bool ok = e.k == upgrade ? core::buy_upgrade(a.save, e.i)
                        : e.k == cls ? core::buy_class(a.save, e.i)
                        : e.k == tool ? core::buy_tool(a.save, e.i) : core::buy_hard(a.save);
                if(ok)
                {
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
        }
    }
}
