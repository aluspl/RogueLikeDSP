using System.Collections.Generic;
using Godot;

namespace LifeLike.Game.Audio;

/// <summary>
/// Dźwięki z GBA (assets/audio/sfx_*.wav) i muzyka (music_*.mp3 wyrenderowana z modułów .mod).
/// Nazwy jak bn::sound_items na GBA: hit, hurt, pickup, level, ability, menu, notify, buy, stage.
/// </summary>
public partial class Sfx : Node
{
    private static Sfx _instance;
    private readonly List<AudioStreamPlayer> _pool = new();
    private readonly Dictionary<string, AudioStream> _streams = new();
    private AudioStreamPlayer _music;
    private string _song = "";
    private int _next;

    public override void _Ready()
    {
        _instance = this;
        for (var i = 0; i < 8; i++)
        {
            var p = new AudioStreamPlayer();
            AddChild(p);
            _pool.Add(p);
        }
        _music = new AudioStreamPlayer();
        AddChild(_music);
    }

    public override void _ExitTree()
    {
        _streams.Clear();
        foreach (var p in _pool)
        {
            p.Stop();
            p.Stream = null;
        }
        _music.Stop();
        _music.Stream = null;
        if (_instance == this) _instance = null;
    }

    private AudioStream Stream(string path)
    {
        if (_streams.TryGetValue(path, out var s)) return s;
        s = ResourceLoader.Exists(path) ? GD.Load<AudioStream>(path) : null;
        _streams[path] = s;
        return s;
    }

    public static readonly string[] Names = ["hit", "hurt", "pickup", "level", "ability", "menu", "notify", "buy", "stage"];

    /// <summary>Czy wszystkie efekty i muzyka dają się wczytać (test dymny); zwraca brakujące.</summary>
    public static string Missing()
    {
        var missing = new List<string>();
        foreach (var n in Names)
        {
            if (!ResourceLoader.Exists($"res://assets/audio/sfx_{n}.wav")) missing.Add(n);
        }
        foreach (var m in new[] { "title", "game" })
        {
            if (!ResourceLoader.Exists($"res://assets/audio/music_{m}.mp3")) missing.Add("music_" + m);
        }
        return string.Join(", ", missing);
    }

    /// <summary>Efekt dźwiękowy; volume jak bn::fixed w sound_item.play(volume) (0..1).</summary>
    public static void Play(string name, float volume = 1f)
    {
        if (_instance is null) return;
        var s = _instance.Stream($"res://assets/audio/sfx_{name}.wav");
        if (s is null) return;
        var p = _instance._pool[_instance._next];
        _instance._next = (_instance._next + 1) % _instance._pool.Count;
        p.Stream = s;
        p.VolumeDb = Mathf.LinearToDb(Mathf.Max(volume, 0.01f)) - 4f;
        p.Play();
    }

    /// <summary>Muzyka: "title", "game" albo "" (cisza) - jak play_song na GBA (głośność 0.45 / 0.35).</summary>
    public static void Music(string song)
    {
        if (_instance is null || _instance._song == song) return;
        _instance._song = song;
        var m = _instance._music;
        m.Stop();
        if (song.Length == 0) return;
        var s = _instance.Stream($"res://assets/audio/music_{song}.mp3");
        if (s is AudioStreamMP3 mp3) mp3.Loop = true;
        if (s is null) return;
        m.Stream = s;
        m.VolumeDb = Mathf.LinearToDb(song == "title" ? 0.45f : 0.35f) - 4f;
        m.Play();
    }
}
