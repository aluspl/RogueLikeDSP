using Godot;
using LifeLike.Game.Gfx;

namespace LifeLike.Game.Phone.Pages;

/// <summary>Wiersz strony podsumowania: tekst, dopisek w pastylce, kolor, pasek z lewej, nagłówek sekcji; v0.21.52: pasek
/// postępu (Bar 0..1, -1 = zwykły wiersz) w kolorze BarColor.</summary>
public sealed record RecapRow(string Text, string Tail, Ink Ink, Color Stripe, bool Header = false, float Bar = -1f, Color BarColor = default);
