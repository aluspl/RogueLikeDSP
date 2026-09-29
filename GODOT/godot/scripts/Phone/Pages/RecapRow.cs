using Godot;
using LifeLike.Game.Gfx;

namespace LifeLike.Game.Phone.Pages;

/// <summary>Wiersz strony podsumowania: tekst, dopisek w pastylce, kolor, pasek z lewej, nagłówek sekcji.</summary>
public sealed record RecapRow(string Text, string Tail, Ink Ink, Color Stripe, bool Header = false);
