#pragma once
// Filtry ekranu (v0.21.53) na GBA: przekształcenie koloru palety RGB555 (efekt własny palet Butano - działa na każdym
// kolorze tła i sprite'ów po ściemnianiu). Czyste C++ (bez Butano) - testowalne na PC. Te same wzory co shader Godota
// (GODOT/godot/shaders/screen_filter.gdshader), bez efektów zależnych od pozycji piksela (ziarno, winieta, dithering,
// linie, fale) - paleta nie wie, gdzie jest piksel. Kwas: obrót barwy wbudowany w Butano (animowany w main.cpp).
#include <cstdint>

namespace core
{
    enum class filter_mode : uint8_t { none, noir, retro, neon, kwas, protan, deutan, tritan, contrast };

    inline filter_mode filter_mode_of(const char* id)
    {
        struct m { const char* id; filter_mode mode; };
        static constexpr m modes[] = { { "noir", filter_mode::noir }, { "retro", filter_mode::retro }, { "neon", filter_mode::neon },
            { "kwas", filter_mode::kwas }, { "protanopia", filter_mode::protan }, { "deuteranopia", filter_mode::deutan },
            { "tritanopia", filter_mode::tritan }, { "kontrast", filter_mode::contrast } };
        for(const m& x : modes)   // bez strcmp (GBA bez biblioteki C)
        {
            int i = 0;
            while(x.id[i] && x.id[i] == id[i]) ++i;
            if(x.id[i] == id[i]) return x.mode;
        }
        return filter_mode::none;
    }

    namespace filter_detail
    {
        constexpr int one = 4096;   // kanał 0..one (stała przecinkowa)
        inline int clampc(int v) { return v < 0 ? 0 : (v > one ? one : v); }
        inline int luma(int r, int g, int b) { return (r * 1225 + g * 2404 + b * 467) >> 12; }   // 0.299 / 0.587 / 0.114
        inline int mixc(int a, int b, int t) { return a + (((b - a) * t) >> 12); }               // t: 0..one
        inline int smooth(int e0, int e1, int x)
        {
            if(x <= e0) return 0;
            if(x >= e1) return one;
            const int t = int((int64_t(x - e0) << 12) / (e1 - e0));
            return int((int64_t(t) * t >> 12) * (3 * one - 2 * t) >> 12);
        }
        inline void saturate(int& r, int& g, int& b, int s)   // s w 1/4096
        {
            const int l = luma(r, g, b);
            r = l + (((r - l) * s) >> 12); g = l + (((g - l) * s) >> 12); b = l + (((b - l) * s) >> 12);
        }
        inline int contrast(int v, int c, int mid = one / 2) { return mid + (((v - one / 2) * c) >> 12); }

        // Daltonizacja (jak shader): symulacja wady macierzą Machado 2009 (pełna wada, S), błąd e = c - S c przeniesiony
        // na widoczne kanały: o = c + k * E * e, potem kontrast 1.08 - to liniowe, więc jedna macierz 3x3 na tryb
        // (liczona w czasie kompilacji). Protanopia: błąd czerwieni i zieleni do niebieskiego (k 1.0), deuteranopia: klasyczne
        // przeniesienie do zieleni i niebieskiego (k 1.4), tritanopia: błąd niebieskiego na oś czerwień-zieleń (k 1.0).
        struct mat { double m[3][3]; };
        constexpr mat dalton(int mode)   // 0 protan, 1 deutan, 2 tritan
        {
            const mat s = mode == 0 ? mat{ { { 0.152286, 1.052583, -0.204868 }, { 0.114503, 0.786281, 0.099216 }, { -0.003882, -0.048116, 1.051998 } } }
                        : mode == 1 ? mat{ { { 0.367322, 0.860646, -0.227968 }, { 0.280085, 0.672501, 0.047413 }, { -0.011820, 0.042940, 0.968881 } } }
                                    : mat{ { { 1.255528, -0.076749, -0.178779 }, { -0.078411, 0.930809, 0.147602 }, { 0.004733, 0.691367, 0.303900 } } };
            const mat e = mode == 0 ? mat{ { { 1, 0, 0 }, { 0, 1, 0 }, { 0.7, 0.7, 0 } } }
                        : mode == 1 ? mat{ { { 0, 0, 0 }, { 0.7, 1, 0 }, { 0.7, 0, 1 } } }
                                    : mat{ { { 0, 0, 0.5 }, { 0, 0, -0.5 }, { 0, 0, 0 } } };
            const double k = mode == 1 ? 1.4 : 1.0;
            mat r{};
            for(int i = 0; i < 3; ++i) for(int j = 0; j < 3; ++j)
            {
                double err = 0;
                for(int q = 0; q < 3; ++q) err += e.m[i][q] * ((q == j ? 1.0 : 0.0) - s.m[q][j]);
                r.m[i][j] = (i == j ? 1.0 : 0.0) + k * err;
            }
            return r;
        }
        struct imat { int m[3][3]; };
        constexpr imat to_int(const mat& x)
        {
            imat r{};
            for(int i = 0; i < 3; ++i) for(int j = 0; j < 3; ++j) r.m[i][j] = int(x.m[i][j] * one + (x.m[i][j] < 0 ? -0.5 : 0.5));
            return r;
        }
        inline constexpr imat dalton_m[3] = { to_int(dalton(0)), to_int(dalton(1)), to_int(dalton(2)) };
    }

    // Kolor RGB555 (bit 15 pomijany) po filtrze.
    inline uint16_t filter_color(filter_mode mode, uint16_t c)
    {
        using namespace filter_detail;
        if(mode == filter_mode::none || mode == filter_mode::kwas) return c;
        int r = (c & 31) * one / 31, g = ((c >> 5) & 31) * one / 31, b = ((c >> 10) & 31) * one / 31;
        switch(mode)
        {
            case filter_mode::noir:   // czerń i biel z kontrastem; czerwień zagrożeń przygaszona, nie szara
            {
                const int l = clampc(contrast(luma(r, g, b), 5939) + 82);    // x1.45, +0.02
                const int mx = g > b ? g : b;
                const int red = clampc(r - ((mx * 4506) >> 12));               // r - 1.1 max(g, b)
                const int t = (smooth(410, 1434, red) * 3686) >> 12;           // smoothstep(0.1, 0.35) * 0.9
                const int tr = clampc(((l * 4710) >> 12) + 737);
                r = mixc(l, tr, t); g = mixc(l, (l * 1311) >> 12, t); b = mixc(l, (l * 1229) >> 12, t);
                break;
            }
            case filter_mode::retro:   // 4 odcienie zieleni (bez ditheringu - paleta); progi niżej - plansze GBA są ciemne
            {
                const int l = luma(r, g, b);
                static constexpr int pal[4][3] = { { 246, 901, 246 }, { 778, 1556, 778 }, { 2253, 2744, 246 }, { 2499, 3031, 246 } };
                const int lv = l < 500 ? 0 : (l < 1250 ? 1 : (l < 2350 ? 2 : 3));
                r = pal[lv][0]; g = pal[lv][1]; b = pal[lv][2];
                break;
            }
            case filter_mode::neon:   // róż i błękit, mocne nasycenie
            {
                int sr = r, sg = g, sb = b;
                saturate(sr, sg, sb, 7373);   // x1.8
                const int l = luma(r, g, b);
                const int hot = smooth(1843, 3482, l);
                const int hr = mixc(4096, 492, hot), hg = mixc(655, 3891, hot), hb = mixc(2867, 4096, hot);
                const int gt = smooth(164, 2048, l);
                const int k = 2253 + l;   // 0.55 + l
                const int gr = (mixc(614, hr, gt) * k) >> 12, gg = (mixc(82, hg, gt) * k) >> 12, gb = (mixc(1147, hb, gt) * k) >> 12;
                r = (sr + gr) / 2; g = (sg + gg) / 2; b = (sb + gb) / 2;
                break;
            }
            case filter_mode::protan:
            case filter_mode::deutan:
            case filter_mode::tritan:
            {
                const imat& m = dalton_m[int(mode) - int(filter_mode::protan)];
                const int nr = clampc(int((int64_t(m.m[0][0]) * r + int64_t(m.m[0][1]) * g + int64_t(m.m[0][2]) * b) >> 12));
                const int ng = clampc(int((int64_t(m.m[1][0]) * r + int64_t(m.m[1][1]) * g + int64_t(m.m[1][2]) * b) >> 12));
                const int nb = clampc(int((int64_t(m.m[2][0]) * r + int64_t(m.m[2][1]) * g + int64_t(m.m[2][2]) * b) >> 12));
                r = contrast(nr, 4424); g = contrast(ng, 4424); b = contrast(nb, 4424);   // x1.08
                break;
            }
            case filter_mode::contrast:   // nasycenie x1.6, kontrast x1.35, ciemniejsze tło
            {
                const int l = luma(r, g, b);
                saturate(r, g, b, 6554);
                r = clampc(contrast(clampc(r), 5530)); g = clampc(contrast(clampc(g), 5530)); b = clampc(contrast(clampc(b), 5530));
                const int k = mixc(2253, one, smooth(614, 2048, l));
                r = (r * k) >> 12; g = (g * k) >> 12; b = (b * k) >> 12;
                break;
            }
            default:
                break;
        }
        r = clampc(r); g = clampc(g); b = clampc(b);
        return uint16_t(((r * 31 + one / 2) >> 12) | (((g * 31 + one / 2) >> 12) << 5) | (((b * 31 + one / 2) >> 12) << 10));
    }
}
