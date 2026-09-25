using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Gfx;

namespace LifeLike.Game.Phone.ProfileTabs;

/// <summary>
/// Osiedle (zakładka 2 profilu na GBA): domy z wygranych budów na działkach (dach w kolorze kasku zawodu,
/// wielkość wg wyniku), puste działki czekają na kolejne odbiory.
/// </summary>
public sealed class EstateTab : PhonePage
{
    private readonly GameData _d;
    private readonly Profile _p;

    public EstateTab(GameData d, Profile p)
    {
        _d = d;
        _p = p;
    }

    public override string Title => "Osiedle";
    public override string Sub => $"Domy: {_p.HousesCount}/{Profile.MaxHouses}";
    public override string Hint => "Q/E: zakładki  Esc: wróć";

    public override void Draw(PhonePainter p)
    {
        var y = p.Section(p.Top, "TWOJE UKOŃCZONE BUDOWY");
        const int cols = 4, rows = 3, cellW = 48, cellH = 46;
        var card = p.CardH(y, rows * cellH + 12);
        var x0 = card.Position.X + (card.Size.X - cols * cellW) / 2;
        for (var i = 0; i < Profile.MaxHouses; i++)
        {
            var cx = x0 + (i % cols) * cellW;
            var cy = card.Position.Y + 6 + (i / cols) * cellH;
            p.C.DrawRect(new Rect2(cx + 4, cy + 34, cellW - 8, 4), i < _p.HousesCount ? Pal.EstateBar : Pal.Border);
            var frame = 24;
            if (i < _p.HousesCount) frame = (_p.Houses[i] >> 4) * 6 + (_p.Houses[i] & 15);
            p.Icon(Assets.Houses, frame, Assets.Actor, new Vector2(cx + (cellW - 32) / 2, cy + 4));
        }
        var c2 = p.Card(card.End.Y + 6, 2);
        p.Text(p.TextX(c2), p.RowY(c2, 0), $"Najlepszy wynik: {_p.Best}", Ink.Dark);
        p.Divider(c2, 1);
        p.Text(p.TextX(c2), p.RowY(c2, 1), $"Budowy: {_p.Runs}  Wygrane: {_p.Wins}", Ink.Dim);
    }
}
