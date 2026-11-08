namespace LifeLike.Core;

/// <summary>Bity Profile.Tutorial (core::tutorial_flag): główny samouczek tytułu i wyboru zawodu, dymki odblokowań.</summary>
public static class Tutorial
{
    public const ushort Title = 1;
    public const ushort Class = 2;
    public const ushort Respect = 4;
    public const ushort Daily = 8;
    public const ushort Investor = 16;
    public const ushort Act0 = 32;
    /// <summary>v0.23.0: profil zaczął od krótkiego samouczka (dymki Szkoleń, opcji, Jak grać, pamiątki, statystyk osobno).</summary>
    public const ushort Short = 64;
    public const ushort Training = 128;
    public const ushort Options = 256;
    public const ushort Help = 512;
    public const ushort Keepsake = 1024;
    public const ushort Stats = 2048;
}
