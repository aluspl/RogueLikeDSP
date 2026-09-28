using Godot;
using LifeLike.Core;

namespace LifeLike.Game.Gfx;

/// <summary>
/// Wiersz rozpiski obrażeń broni (#26) w telefonie i dymkach: tekst z DamageHelp, kolor, pasek po lewej
/// (Stripe = kolor paska, przezroczysty = bez). Kind = składnik (DmgText); Defense = obrona / unik bohatera.
/// </summary>
public sealed record DamageRow(string Text, Ink Ink, Color Stripe, DmgText Kind, bool Defense);
