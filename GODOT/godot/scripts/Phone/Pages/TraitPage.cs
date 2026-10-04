using LifeLike.Core;
using Godot;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Cecha ulepszonego narzędzia przy +2 (v0.21.50 cz. 3, trait_loop na GBA): 3 karty – Przebicie, Ostrze, Wyważenie
/// (nazwa, skrót w pastylce, opis). Strzałki / dotknięcie wybierają, Enter / drugie dotknięcie bierze.
/// </summary>
public sealed class TraitPage : PhonePage
{
    private readonly CoreGame _g;

    public TraitPage(CoreGame g) => _g = g;

    public int Sel { get; set; }

    public System.Action Picked { get; set; }

    public override string Title => _g.WeaponTitle();
    public override string Sub => Loc.T("wybierz_ceche");
    public override string Hint => Loc.T("strzalki_wybor_enter_biore");
    public override PageAction[] Actions => [new(Loc.T("biore"), GameAction.A)];

    public override bool TapRow(int index)
    {
        if (index < 0 || index >= _g.D.ToolTraits.Length) return false;
        if (index == Sel) Picked?.Invoke();
        else Sel = index;
        return true;
    }

    public override bool Input(InputCmd e)
    {
        var d = e.VDir != 0 ? e.VDir : e.HDir;
        if (d == 0) return false;
        var n = _g.D.ToolTraits.Length;
        Sel = (Sel + d + n) % n;
        Sfx.Play("menu");
        return true;
    }

    public override void Draw(PhonePainter p)
    {
        var y = p.Section(p.Top, Loc.T("cecha_od") + _g.D.ToolTraitAt, Loc.F("ulepszenie_4", _g.WeaponLvl));
        for (var k = 0; k < _g.D.ToolTraits.Length; k++)
        {
            var td = _g.D.ToolTraits[k];
            var sel = k == Sel;
            var card = p.Card(y, 2);
            p.C.DrawRect(new Rect2(card.Position, new Vector2(4, card.Size.Y)), sel ? Pal.Brand : Pal.Todo);
            if (sel)
            {
                p.C.DrawRect(new Rect2(card.Position.X + 4, card.Position.Y, card.Size.X - 4, card.Size.Y), Pal.Group);
                p.C.DrawRect(card, Pal.Brand, false, 2);
            }
            p.Hit(card, k);
            var tx = p.TextX(card);
            var right = card.End.X - 6;
            var pw = p.Pill(right, p.RowY(card, 0), td.Short, sel ? PillKind.Prog : PillKind.Gray);
            p.Bold(tx, p.RowY(card, 0) + PhonePainter.TextDy, p.F.Fit(td.Name, (int)(right - pw - 6 - tx)), sel ? Ink.Brand : Ink.Dark);
            p.Text(tx, p.RowY(card, 1), td.Desc, sel ? Ink.Dark : Ink.Dim, TextAlign.Left, right - tx);
            y = card.End.Y + 5;
        }
    }
}
