using System;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Hurtownia między aktami (run_hurtownia na GBA): budżet budowy w nagłówku, premia za akt, lista towarów
/// z ceną w pastylce (fiolet - stać Cię, szara - za drogo; towary za materiały: „4x Stal”), opis zaznaczonego,
/// posiadane materiały; Spacja kupuje, Enter - dalej.
/// </summary>
public sealed class HurtowniaPage : PhonePage
{
    private const int Window = 6;
    private readonly CoreGame _g;
    private readonly ListState _list = new();
    private string _note = "";

    public HurtowniaPage(CoreGame g) => _g = g;

    public override string Title => "Hurtownia";
    public override string Sub => $"Budżet: {_g.Cash} zł";
    public override string Hint => "Spacja/Enter: kup zaznaczone  Z/Esc: dalej";
    public override PageAction[] Actions => [new("Kup", GameAction.A), new("Dalej", GameAction.B, PageRole.Back)];

    public override bool TapRow(int index)
    {
        if (index == _list.Sel) Buy();
        else _note = "";
        _list.Sel = index;
        return true;
    }

    public int Sel
    {
        get => _list.Sel;
        set => _list.Sel = value;
    }

    /// <summary>Zakup udany (ekran: cecha narzędzia przy +2).</summary>
    public System.Action Bought { get; set; }

    public void Buy()
    {
        var it = _g.D.Hurtownia[_list.Sel];
        var upgrade = it.Effect == LifeLike.Core.Data.ShopEffect.Upgrade;
        if (_g.HurtowniaBuy(_list.Sel))
        {
            Sfx.Play("buy");
            _note = upgrade ? $"Ulepszone: {_g.WeaponTitle()}" : "Kupione! Enter: dalej";
            Bought?.Invoke();
        }
        else if (upgrade)
        {
            _note = _g.WeaponLvl >= _g.D.ToolUpgradeMax ? "Maksymalne ulepszenie" : "Za mało zł albo stali";
        }
        else
        {
            _note = it.Material >= 0 ? $"Za mało: {_g.D.Materials[it.Material].Name}" : "Za mały budżet";
        }
    }

    /// <summary>Opis zaznaczonego: ulepszenie – poziom i koszt, nowe narzędzie – ostrzeżenie o utracie ulepszenia.</summary>
    private (string Text, Ink Ink) Describe(int i)
    {
        var it = _g.D.Hurtownia[i];
        if (it.Effect == LifeLike.Core.Data.ShopEffect.Upgrade)
        {
            if (_g.WeaponLvl >= _g.D.ToolUpgradeMax) return ($"{_g.WeaponTitle()}: maksymalne ulepszenie", Ink.Brand);
            var cost = LifeLike.Core.ChoiceText.ToolLevelLabel(_g.D, new LifeLike.Core.Message(), _g.WeaponLvl, _g.UpgradePrice()).Text;
            var trait = _g.WeaponLvl + 1 == _g.D.ToolTraitAt ? ", wybór cechy" : "";
            return ($"{_g.WeaponTitle()} -> +{_g.WeaponLvl + 1}: {cost}{trait}. {it.Desc}", Ink.Brand);
        }
        if (it.Effect == LifeLike.Core.Data.ShopEffect.Tool && _g.WeaponLvl > 0) return ($"Uwaga: {_g.WeaponTitle()} przepadnie! {it.Desc}", Ink.Late);
        return (it.Desc, Ink.Dim);
    }

    public override bool Input(InputCmd e)
    {
        var v = e.VDir;
        if (v != 0)
        {
            _list.Move(v, _g.D.Hurtownia.Length, Window);
            _note = "";
            Sfx.Play("menu");
            return true;
        }
        if (e.IsConfirm) // v0.21.51: A i Enter tak samo - kup zaznaczone (wyjście: B / Esc)
        {
            Buy();
            return true;
        }
        return false;
    }

    public override void Draw(PhonePainter p)
    {
        var d = _g.D;
        var c0 = p.Card(p.Top, 1);
        var tx = p.TextX(c0);
        var right = c0.End.X - 6;
        var bonus = $"Premia za akt {_g.ActNumeral()}: +{_g.ActBonus} zł";
        p.Stripe(c0, 0, _note.Length > 0 ? Pal.Brand : Pal.Done);
        p.Text(tx, p.RowY(c0, 0), _note.Length > 0 ? _note : bonus, _note.Length > 0 ? Ink.Brand : Ink.Done, TextAlign.Left, right - tx);
        var n = d.Hurtownia.Length;
        _list.Clamp(n, Window);
        var rows = Math.Min(Window, n - _list.Top);
        var card = p.Card(c0.End.Y + 6, rows);
        for (var r = 0; r < rows; r++)
        {
            var i = _list.Top + r;
            var it = d.Hurtownia[i];
            var y = p.RowY(card, r);
            var sel = i == _list.Sel;
            if (sel) p.Selected(card, r);
            else if (r > 0) p.Divider(card, r);
            p.HitRow(card, r, i);
            var price = it.Effect == LifeLike.Core.Data.ShopEffect.Upgrade && _g.WeaponLvl >= d.ToolUpgradeMax ? "maks."
                      : it.Effect == LifeLike.Core.Data.ShopEffect.Upgrade ? $"{_g.HurtowniaPrice(i)} zł + {d.ToolLevels[_g.WeaponLvl].Count}x {d.Materials[d.ToolLevels[_g.WeaponLvl].Material].Short}"
                      : it.Material >= 0 ? $"{it.MatCost}x {d.Materials[it.Material].Short}" : $"{_g.HurtowniaPrice(i)} zł";
            var pw = p.Pill(right, y, price, _g.HurtowniaCan(i) ? PillKind.Group : PillKind.Gray);
            var nx = tx;
            if (it.Material >= 0)
            {
                MaterialIcon.Draw(p.C, it.Material, new Godot.Vector2(tx, y + (PhonePainter.RowH - MaterialIcon.Size) / 2));
                nx += MaterialIcon.Size + 4;
            }
            p.Text(nx, y, it.Name, sel ? Ink.Brand : Ink.Dark, TextAlign.Left, right - pw - 4 - nx);
        }
        var dc = p.Card(card.End.Y + 6, 3);
        var (desc, ink) = Describe(_list.Sel);
        var lines = p.F.Wrap(desc, (int)(right - tx));
        for (var k = 0; k < 2 && k < lines.Count; k++) p.Text(tx, p.RowY(dc, k), lines[k], ink);
        p.Divider(dc, 2);
        var info = $"HP {_g.Hero.Hp}/{_g.Hero.MaxHp}";
        for (var m = 0; m < d.Materials.Length; m++) info += $"  {d.Materials[m].Short} {_g.Mats[m]}";
        if (d.Materials.Length == 0) info += $"  {_g.Weapon.Name}";
        p.Text(tx, p.RowY(dc, 2), info, Ink.Dim, TextAlign.Left, right - tx);
    }
}
