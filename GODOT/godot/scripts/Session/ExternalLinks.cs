using Godot;

namespace LifeLike.Game.Session;

/// <summary>Linki poza grę (planbudowlany.online): w trybach testowych (test dymny, zrzuty, test małpy) tylko liczone.</summary>
public static class ExternalLinks
{
    public static bool Enabled { get; set; } = true;

    /// <summary>Ile razy gra chciała otworzyć przeglądarkę (testy).</summary>
    public static int Opened { get; private set; }

    public static void Open(string url)
    {
        Opened++;
        if (Enabled) OS.ShellOpen(url);
    }
}
