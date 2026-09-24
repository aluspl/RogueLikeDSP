// Playtester bez okna: uruchamia ROM w libmgba wg skryptu z stdin i zapisuje zrzuty ekranu (PPM).
// Budowanie/uruchomienie: tools/playtest/run.sh <skrypt> (Docker, debian + libmgba-dev).
//
// Skrypt, jedna komenda na linię (# = komentarz):
//   wait N            N klatek bez klawiszy
//   press KEYS [N]    przytrzymaj KEYS przez N klatek (domyślnie 3), potem 3 klatki puszczone
//   hold KEYS N       jak press, ale bez puszczenia na końcu
//   shot NAZWA        zapisz <out>/NAZWA.ppm
//   repeat N KEYS     N razy press KEYS
// Dźwięk z całej sesji zapisuje się do <out>/audio.wav (32768 Hz, mono).
// KEYS: A B SELECT START RIGHT LEFT UP DOWN R L, łączone "+", np. L+R+SELECT
#include <mgba/core/core.h>
#include <mgba/core/config.h>
#include <mgba/core/log.h>
#include <mgba/core/blip_buf.h>
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

static void quiet_log(struct mLogger* l, int category, enum mLogLevel level, const char* fmt, va_list args)
{
    (void) l; (void) category; (void) level; (void) fmt; (void) args;
}
static struct mLogger quiet_logger = { .log = quiet_log };

static struct mCore* core;
static color_t* buffer;
static unsigned width, height;

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
    for(int i = 0; i < n; ++i) { core->runFrame(core); grab_audio(); }
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

static void shot(const char* dir, const char* name)
{
    char path[512];
    snprintf(path, sizeof path, "%s/%s.ppm", dir, name);
    FILE* f = fopen(path, "wb");
    if(! f) { perror(path); exit(1); }
    fprintf(f, "P6\n%u %u\n255\n", width, height);
    for(unsigned i = 0; i < width * height; ++i)
    {
        uint32_t c = buffer[i];
        unsigned char rgb[3] = { c & 0xFF, (c >> 8) & 0xFF, (c >> 16) & 0xFF };
        fwrite(rgb, 1, 3, f);
    }
    fclose(f);
    printf("zrzut: %s\n", path);
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
        else { fprintf(stderr, "nieznana komenda: %s\n", cmd); return 2; }
    }
    write_audio(argv[2]);
    core->deinit(core);   // zapisuje SRAM do ROM.sav
    return 0;
}
