using LifeLike.Game.Session;

namespace LifeLike.Game.Audio;

/// <summary>Dźwięki zdarzeń budowy (jak bn::sound_items w detect_events i przejściach scen na GBA).</summary>
public static class SoundCues
{
    public static void Attach(SessionEvents ev)
    {
        ev.PickedUp += () => Sfx.Play("pickup");
        ev.LevelUp += (_, _) => Sfx.Play("level");
        ev.StageCleared += () => Sfx.Play("stage");
        ev.RunEnded += won => Sfx.Play(won ? "level" : "hurt");
    }
}
