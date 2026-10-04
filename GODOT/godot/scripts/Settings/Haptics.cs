using Godot;

namespace LifeLike.Game.Settings;

/// <summary>Wibracje telefonu (Input.VibrateHandheld) przy trafieniu, obrażeniach i powiadomieniu; wyłączane w ustawieniach.</summary>
public static class Haptics
{
    /// <summary>Krótka wibracja dla dźwięku zdarzenia (te same miejsca co dźwięki z GBA).</summary>
    public static void ForSound(string sound)
    {
        var ms = sound switch
        {
            "hurt" => 45,
            "hit" => 18,
            "notify" => 22,
            "level" => 60,
            "ability" => 30,
            _ => 0,
        };
        if (ms > 0) Pulse(ms);
    }

    public static void Pulse(int ms)
    {
        if (!GameSettings.Vibration || !OS.HasFeature("mobile")) return;
        Godot.Input.VibrateHandheld(ms, 0.6f);
    }
}
