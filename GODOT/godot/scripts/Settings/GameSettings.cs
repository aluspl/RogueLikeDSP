using System;
using Godot;

namespace LifeLike.Game.Settings;

/// <summary>
/// Ustawienia gry (klucz w rogu ekranu): głośność muzyki i dźwięków, wibracje, sterowanie dotykiem, ręka paska
/// akcji, wielkość tekstu. Zapis w user://settings.cfg (ConfigFile) - osobno od profilu gracza (profile.sav).
/// Test dymny i zrzuty nie czytają ani nie zapisują pliku (Persist = false).
/// </summary>
public static class GameSettings
{
    public const string Path = "user://settings.cfg";
    public const int VolumeSteps = 10;
    private const string Section = "settings";

    /// <summary>Muzyka i dźwięki: 0..VolumeSteps.</summary>
    public static int Music { get; set; } = 8;
    public static int Sound { get; set; } = VolumeSteps;
    public static bool Vibration { get; set; } = true;
    public static ControlScheme Controls { get; set; } = ControlScheme.Swipe;
    /// <summary>Pasek akcji dla lewej ręki (Atak po lewej, gałka po lewej).</summary>
    public static bool LeftHanded { get; set; }
    /// <summary>Duży tekst: większa skala całkowita interfejsu, gdy ekran na to pozwala.</summary>
    public static bool LargeText { get; set; }
    public static bool Persist { get; set; }

    /// <summary>Po każdej zmianie (głośność muzyki, skala, pasek akcji).</summary>
    public static event Action Changed;

    public static float MusicVolume => Music / (float)VolumeSteps;
    public static float SoundVolume => Sound / (float)VolumeSteps;

    public static void Load()
    {
        Persist = true;
        var cfg = new ConfigFile();
        if (cfg.Load(Path) != Error.Ok) return;
        Music = Mathf.Clamp((int)cfg.GetValue(Section, "music", Music), 0, VolumeSteps);
        Sound = Mathf.Clamp((int)cfg.GetValue(Section, "sound", Sound), 0, VolumeSteps);
        Vibration = (bool)cfg.GetValue(Section, "vibration", Vibration);
        Controls = (int)cfg.GetValue(Section, "controls", (int)Controls) == 1 ? ControlScheme.Joystick : ControlScheme.Swipe;
        LeftHanded = (bool)cfg.GetValue(Section, "left_handed", LeftHanded);
        LargeText = (bool)cfg.GetValue(Section, "large_text", LargeText);
    }

    /// <summary>Zapis (gdy Persist) i powiadomienie obserwatorów.</summary>
    public static void Save()
    {
        if (Persist)
        {
            var cfg = new ConfigFile();
            cfg.SetValue(Section, "music", Music);
            cfg.SetValue(Section, "sound", Sound);
            cfg.SetValue(Section, "vibration", Vibration);
            cfg.SetValue(Section, "controls", (int)Controls);
            cfg.SetValue(Section, "left_handed", LeftHanded);
            cfg.SetValue(Section, "large_text", LargeText);
            cfg.Save(Path);
        }
        Changed?.Invoke();
    }
}
