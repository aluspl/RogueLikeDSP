// Playtester bez okna: uruchamia ROM w libmgba wg skryptu z stdin i zapisuje zrzuty ekranu (PPM).
// Budowanie/uruchomienie: tools/playtest/run.sh <skrypt> (Docker, debian + libmgba-dev).
//
// Skrypt, jedna komenda na linię (# = komentarz):
//   wait N            N klatek bez klawiszy
//   press KEYS [N]    przytrzymaj KEYS przez N klatek (domyślnie 3), potem 3 klatki puszczone
//   hold KEYS N       jak press, ale bez puszczenia na końcu
//   shot NAZWA        zapisz <out>/NAZWA.ppm
//   repeat N KEYS     N razy press KEYS
//   status            wypisz klatkę i ile klatek temu gra dała puls (buildy z PB_DEBUG_STATS: co 128 klatek)
//   monkey SEED N     N klatek losowych klawiszy (ziarno SEED; d-pad, A, B, START, SELECT, L, R, kombinacje, czasem
//                     L+R+SELECT); co 10 klatek sprawdza ekran błędu Butano (tryb 3), co 60 - zawieszenie (ten sam obraz
//                     przez 2400 klatek), a w buildach z PB_DEBUG_STATS też puls gry (brak przez 600 klatek = HANG).
//                     Błąd/zawieszenie: <out>/crash.ppm, ostatnie zrzuty ring_*.ppm, koniec z kodem 3 / 4 / 5.
//                     Wszystkie wciśnięcia trafiają do <out>/inputs.log (to samo ziarno + ROM + skrypt = ten sam przebieg).
// Log mGBA z gry (BN_LOG, kategoria "GBA Debug") wypisuje się na stdout jako "log[f=klatka] ...".
// Dźwięk z całej sesji zapisuje się do <out>/audio.wav (32768 Hz, mono).
// KEYS: A B SELECT START RIGHT LEFT UP DOWN R L, łączone "+", np. L+R+SELECT
#include <mgba/core/core.h>
#include <mgba/core/config.h>
#include <mgba/core/log.h>
#include <mgba/core/blip_buf.h>
#include <mgba/internal/arm/arm.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

static const char* key_names[] = { "A", "B", "SELECT", "START", "RIGHT", "LEFT", "UP", "DOWN", "R", "L" };

static uint32_t parse_keys(const char* s)
{
    uint32_t mask = 0;
    char buf[128];
    strncpy(buf, s, sizeof buf - 1);
    buf[sizeof buf - 1] = 0;
    for(char* tok = strtok(buf, "+"); tok; tok = strtok(NULL, "+"))
    {
        int found = 0;
        for(int i = 0; i < 10; ++i)
            if(strcmp(tok, key_names[i]) == 0) { mask |= 1u << i; found = 1; }
        if(! found) { fprintf(stderr, "nieznany klawisz: %s\n", tok); exit(2); }
    }
    return mask;
}

static struct mCore* core;
static color_t* buffer;
static unsigned width, height;
static long frame_no = 0;
static int max_items = 0, max_sprites = 0, max_stack = 0, stack_size = 0;
static unsigned scenes_seen = 0;
static long last_heartbeat = 0;

static void quiet_log(struct mLogger* l, int category, enum mLogLevel level, const char* fmt, va_list args)
{
    (void) l; (void) level;
    const char* name = mLogCategoryName(category);
    if(! name || strcmp(name, "GBA Debug") != 0) return;
    char msg[256];
    vsnprintf(msg, sizeof msg, fmt, args);
    if(strcmp(msg, "PBHB") == 0) { last_heartbeat = frame_no; return; }
    int items, sprites, stack, of, sc;
    if(sscanf(msg, "PBSTAT items %d sprites %d stack %d of %d", &items, &sprites, &stack, &of) == 4)
    {
        if(items > max_items) max_items = items;
        if(sprites > max_sprites) max_sprites = sprites;
        if(stack > max_stack) max_stack = stack;
        stack_size = of;
    }
    if(sscanf(msg, "PBSCENE %d", &sc) == 1 && sc >= 0 && sc < 32) scenes_seen |= 1u << sc;
    printf("log[f=%ld] %s\n", frame_no, msg);
}
static struct mLogger quiet_logger = { .log = quiet_log };

#define AUDIO_RATE 32768
static short* audio = NULL;
static size_t audio_len = 0, audio_cap = 0;

static void grab_audio(void)
{
    blip_t* left = core->getAudioChannel(core, 0);
    int avail = blip_samples_avail(left);
    if(avail <= 0) return;
    if(audio_len + avail > audio_cap) { audio_cap = (audio_len + avail) * 2; audio = realloc(audio, audio_cap * sizeof(short)); }
    audio_len += blip_read_samples(left, audio + audio_len, avail, 0);
    blip_clear(core->getAudioChannel(core, 1));
}

static void frames(uint32_t keys, int n)
{
    core->setKeys(core, keys);
    for(int i = 0; i < n; ++i) { core->runFrame(core); grab_audio(); ++frame_no; }
}

static void write_audio(const char* dir)
{
    char path[512];
    snprintf(path, sizeof path, "%s/audio.wav", dir);
    FILE* f = fopen(path, "wb");
    if(! f) return;
    unsigned data = (unsigned) (audio_len * 2), rate = AUDIO_RATE, byte_rate = AUDIO_RATE * 2, riff = 36 + data;
    unsigned short fmt = 1, ch = 1, align = 2, bits = 16;
    unsigned fmt_len = 16;
    fwrite("RIFF", 1, 4, f); fwrite(&riff, 4, 1, f); fwrite("WAVEfmt ", 1, 8, f); fwrite(&fmt_len, 4, 1, f);
    fwrite(&fmt, 2, 1, f); fwrite(&ch, 2, 1, f); fwrite(&rate, 4, 1, f); fwrite(&byte_rate, 4, 1, f);
    fwrite(&align, 2, 1, f); fwrite(&bits, 2, 1, f); fwrite("data", 1, 4, f); fwrite(&data, 4, 1, f);
    fwrite(audio, 2, audio_len, f);
    fclose(f);
    printf("dźwięk: %s (%.1f s)\n", path, audio_len / (double) AUDIO_RATE);
}

static void write_ppm(const char* dir, const char* name, const color_t* pixels)
{
    char path[512];
    snprintf(path, sizeof path, "%s/%s.ppm", dir, name);
    FILE* f = fopen(path, "wb");
    if(! f) { perror(path); exit(1); }
    fprintf(f, "P6\n%u %u\n255\n", width, height);
    for(unsigned i = 0; i < width * height; ++i)
    {
        uint32_t c = pixels[i];
        unsigned char rgb[3] = { c & 0xFF, (c >> 8) & 0xFF, (c >> 16) & 0xFF };
        fwrite(rgb, 1, 3, f);
    }
    fclose(f);
    printf("zrzut: %s\n", path);
}

static void shot(const char* dir, const char* name) { write_ppm(dir, name, buffer); }

// ------------------------------------------------------------------ monkey: losowe klawisze
static uint32_t rng_state;
static uint32_t rnd(void) { rng_state ^= rng_state << 13; rng_state ^= rng_state >> 17; rng_state ^= rng_state << 5; return rng_state; }
static int rnd_range(int lo, int hi) { return lo + (int) (rnd() % (uint32_t) (hi - lo + 1)); }

enum { K_A = 1, K_B = 2, K_SELECT = 4, K_START = 8, K_RIGHT = 16, K_LEFT = 32, K_UP = 64, K_DOWN = 128, K_R = 256, K_L = 512 };

static void keys_text(uint32_t k, char* out, size_t n)
{
    out[0] = 0;
    if(! k) { snprintf(out, n, "-"); return; }
    for(int i = 0; i < 10; ++i)
        if(k & (1u << i)) { if(out[0]) strncat(out, "+", n - strlen(out) - 1); strncat(out, key_names[i], n - strlen(out) - 1); }
}

static uint32_t frame_hash(void)
{
    uint32_t h = 2166136261u;
    for(unsigned i = 0; i < width * height; i += 3) { h ^= buffer[i]; h *= 16777619u; }
    return h;
}

#define RING 6
static int monkey(const char* dir, uint32_t seed, long n)
{
    rng_state = seed * 2654435761u + 0x9E3779B9u;
    if(! rng_state) rng_state = 1;
    char path[512];
    snprintf(path, sizeof path, "%s/inputs.log", dir);
    FILE* log = fopen(path, "a");
    static color_t* ring[RING];
    for(int i = 0; i < RING; ++i) if(! ring[i]) ring[i] = calloc(width * height, sizeof(color_t));
    long ring_frame[RING] = { 0 };
    int ring_pos = 0;
    long end = frame_no + n, next_ring = frame_no + 300, next_check = frame_no + 10, next_hash = frame_no + 60;
    uint32_t last_hash = 0;
    long static_frames = 0;
    const long softlock_limit = 2400;
    int result = 0;
    while(frame_no < end && ! result)
    {
        // akcja: klawisze i czas trzymania
        int w = rnd_range(0, 999);
        uint32_t k = 0;
        int hold = rnd_range(1, 8);
        if(w < 440) { static const uint32_t d[4] = { K_RIGHT, K_LEFT, K_UP, K_DOWN }; k = d[rnd() % 4]; if(rnd() % 6 == 0) hold = rnd_range(15, 45); }
        else if(w < 640) k = K_A;
        else if(w < 730) { k = K_B; if(rnd() % 5 == 0) hold = rnd_range(30, 90); }   // B trzymane: karta wroga
        else if(w < 780) k = K_START;
        else if(w < 825) k = K_SELECT;
        else if(w < 865) { k = K_L; if(rnd() % 4 == 0) hold = rnd_range(20, 60); }   // L trzymane: mapa
        else if(w < 915) k = K_R;
        else if(w < 927) k = K_L | K_R | K_SELECT;                                    // pominięcie etapu (buildy testowe)
        else if(w < 942) k = (1u << (rnd() % 10)) | (1u << (rnd() % 10));             // losowa para
        if(w >= 942) hold = rnd_range(20, 120);                                        // bez klawiszy
        int release = rnd_range(1, 8);
        if(log) { char t[64]; keys_text(k, t, sizeof t); fprintf(log, "%ld %s %d %d\n", frame_no, t, hold, release); }
        for(int phase = 0; phase < 2 && ! result; ++phase)
        {
            int len = phase == 0 ? hold : release;
            core->setKeys(core, phase == 0 ? k : 0);
            for(int i = 0; i < len && ! result; ++i)
            {
                core->runFrame(core); grab_audio(); ++frame_no;
                if(audio_len > (size_t) AUDIO_RATE * 30) audio_len = 0;   // długie przebiegi: bez pełnego nagrania
                if(frame_no >= next_ring)
                {
                    memcpy(ring[ring_pos], buffer, width * height * sizeof(color_t));
                    ring_frame[ring_pos] = frame_no; ring_pos = (ring_pos + 1) % RING; next_ring = frame_no + 300;
                }
                if(frame_no >= next_check)
                {
                    next_check = frame_no + 10;
                    if((core->busRead16(core, 0x04000000) & 7) == 3)   // ekran błędu Butano (bitmapa, tryb 3)
                    {
                        for(int f = 0; f < 4; ++f) core->runFrame(core);   // dokończ rysowanie komunikatu
                        printf("MONKEY CRASH seed=%u frame=%ld\n", seed, frame_no);
                        result = 3;
                    }
                }
                if(! result && last_heartbeat > 0 && frame_no - last_heartbeat > 600)   // build z PB_DEBUG_STATS: gra przestała dawać puls
                {
                    printf("MONKEY HANG seed=%u frame=%ld (bez pulsu gry od %ld klatek), PC %08x\n", seed, frame_no, frame_no - last_heartbeat,
                           (unsigned) ((struct ARMCore*) core->cpu)->gprs[ARM_PC]);
                    result = 5;
                }
                if(! result && frame_no >= next_hash)
                {
                    next_hash = frame_no + 60;
                    uint32_t h = frame_hash();
                    static_frames = h == last_hash ? static_frames + 60 : 0;
                    last_hash = h;
                    if(static_frames >= softlock_limit)
                    {
                        printf("MONKEY SOFTLOCK seed=%u frame=%ld (obraz bez zmian %ld klatek, ostatni puls gry %ld klatek temu)\n",
                               seed, frame_no, static_frames, frame_no - last_heartbeat);
                        printf("PC:");   // gdzie kręci się procesor (arm-none-eabi-addr2line -f -C -e ROM.elf ADRES)
                        for(int f = 0; f < 8; ++f) { core->runFrame(core); printf(" %08x", (unsigned) ((struct ARMCore*) core->cpu)->gprs[ARM_PC]); }
                        printf("\n");
                        result = 4;
                    }
                }
            }
        }
    }
    core->setKeys(core, 0);
    if(log) fclose(log);
    if(result)
    {
        shot(dir, "crash");
        for(int i = 0; i < RING; ++i)
        {
            int idx = (ring_pos + i) % RING;
            if(! ring_frame[idx]) continue;
            char name[64]; snprintf(name, sizeof name, "ring_%d_f%ld", i, ring_frame[idx]);
            write_ppm(dir, name, ring[idx]);
        }
    }
    printf("MONKEY %s seed=%u frames=%ld max_items=%d max_sprites=%d max_stack=%d/%d scenes=0x%x\n",
           result == 3 ? "END-CRASH" : result == 4 ? "END-SOFTLOCK" : result == 5 ? "END-HANG" : "OK", seed, frame_no, max_items, max_sprites, max_stack, stack_size, scenes_seen);
    return result;
}

int main(int argc, char** argv)
{
    if(argc < 3) { fprintf(stderr, "użycie: playtest ROM KATALOG_ZRZUTÓW < skrypt\n"); return 2; }
    mLogSetDefaultLogger(&quiet_logger);
    core = mCoreFind(argv[1]);
    if(! core || ! core->init(core)) { fprintf(stderr, "nie można uruchomić rdzenia\n"); return 1; }
    mCoreInitConfig(core, NULL);
    core->desiredVideoDimensions(core, &width, &height);
    buffer = malloc(width * height * BYTES_PER_PIXEL);
    core->setVideoBuffer(core, buffer, width);
    if(! mCoreLoadFile(core, argv[1])) { fprintf(stderr, "nie można wczytać ROM-u\n"); return 1; }
    mCoreAutoloadSave(core);   // ROM.sav obok ROM-u (SRAM między uruchomieniami)
    core->reset(core);
    core->setAudioBufferSize(core, 4096);
    blip_set_rates(core->getAudioChannel(core, 0), core->frequency(core), AUDIO_RATE);
    blip_set_rates(core->getAudioChannel(core, 1), core->frequency(core), AUDIO_RATE);

    char line[256];
    while(fgets(line, sizeof line, stdin))
    {
        char cmd[32] = "", a1[128] = "", a2[128] = "";
        int n = sscanf(line, "%31s %127s %127s", cmd, a1, a2);
        if(n <= 0 || cmd[0] == '#') continue;
        if(strcmp(cmd, "wait") == 0) frames(0, atoi(a1));
        else if(strcmp(cmd, "press") == 0) { frames(parse_keys(a1), n > 2 ? atoi(a2) : 3); frames(0, 3); }
        else if(strcmp(cmd, "hold") == 0) frames(parse_keys(a1), atoi(a2));
        else if(strcmp(cmd, "repeat") == 0) { uint32_t k = parse_keys(a2); for(int i = atoi(a1); i > 0; --i) { frames(k, 3); frames(0, 3); } }
        else if(strcmp(cmd, "shot") == 0) shot(argv[2], a1);
        else if(strcmp(cmd, "status") == 0)
            printf("status: klatka %ld, puls gry %ld klatek temu, PC %08x\n", frame_no, frame_no - last_heartbeat,
                   (unsigned) ((struct ARMCore*) core->cpu)->gprs[ARM_PC]);
        else if(strcmp(cmd, "monkey") == 0)
        {
            int r = monkey(argv[2], (uint32_t) strtoul(a1, NULL, 10), atol(a2));
            if(r) { write_audio(argv[2]); core->deinit(core); return r; }
        }
        else { fprintf(stderr, "nieznana komenda: %s\n", cmd); return 2; }
    }
    write_audio(argv[2]);
    core->deinit(core);   // zapisuje SRAM do ROM.sav
    return 0;
}
