using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Tabs;

/// <summary>
/// Sprzęt (tab_gear na GBA): narzędzie z zakresem ciosu i krytem (rozpiska obrażeń #26: I / dotknięcie narzędzia),
/// kask / rękawice / kamizelka / ... z jakością (kolor paska), skutkiem (np. „+2 OBR”) i cechą (pastylka), premie
/// sprzętu oraz szczęście: kryt, unik, wzrok. A / przycisk: Brygada. v0.23.0: broń z rzadkością (kolor), typem ciosu,
/// trybami A i B słowami, odnowieniem B i drugą bronią w kaburze (pas monterski). v0.23.0: Patent nazwą ruchu; gdy trzy
/// sekcje (Narzędzie, Sprzęt, Premie i materiały) nie mieszczą się (telefon w poziomie / ramka na komputerze), strona
/// przewija się sekcjami: strzałki góra/dół, przesunięcie palcem, dotknięcie strzałki „więcej” – nic nie chowa się pod przyciskami.
/// </summary>
public sealed class GearTab : PhonePage
{
    private const int WeaponRow = 100, MoreDown = 101, MoreUp = 102;

    /// <summary>Wysokość paska ze strzałką „więcej niżej / wyżej”.</summary>
    private const int MoreH = 10;
    private readonly CoreGame _g;
    private readonly System.Action _brigade;
    private readonly System.Action _breakdown;
    private readonly System.Action _boons;
    private readonly System.Action _backpack;
    private int _scroll, _maxScroll;

    public GearTab(CoreGame g, System.Action brigade = null, System.Action breakdown = null, System.Action boons = null, System.Action backpack = null)
    {
        _g = g;
        _brigade = brigade;
        _breakdown = breakdown;
        _boons = boons;
        _backpack = backpack;
    }

    public override string Title => Loc.T("sprzet_5");
    public override string Sub => _brigade is null ? Loc.T("na_budowie") : Loc.T("spacja_brygada");
    public override PageAction[] Actions =>
        _brigade is null ? [] : _breakdown is null ? [new(Loc.T("brygada_3"), GameAction.A)]
        : _boons is null ? [new(Loc.T("obrazenia"), GameAction.Info), new(Loc.T("brygada_3"), GameAction.A)]
        : _backpack is null ? [new(Loc.T("obrazenia"), GameAction.Info), new(Loc.T("premie_3"), GameAction.R), new(Loc.T("brygada_3"), GameAction.A)]
        : [new(Loc.T("obrazenia"), GameAction.Info), new(Loc.T("premie_3"), GameAction.R), new(Loc.F("plecak_btn", _g.BackpackCount()), GameAction.Swap), new(Loc.T("brygada_3"), GameAction.A)]; // v0.23.2

    /// <summary>Pierwsza widoczna sekcja (0 = od narzędzia).</summary>
    public int Scroll => _scroll;

    public override void Enter() => _scroll = 0;

    public override bool Input(InputCmd e)
    {
        if (e.VDir != 0 && _maxScroll > 0) // v0.23.0: przewijanie sekcjami (w poziomie się nie mieszczą)
        {
            _scroll = System.Math.Clamp(_scroll + e.VDir, 0, _maxScroll);
            return true;
        }
        if (_breakdown is not null && e.Is(GameAction.Info))
        {
            _breakdown();
            return true;
        }
        if (_backpack is not null && e.Is(GameAction.Swap)) // v0.23.2: plecak (X)
        {
            _backpack();
            return true;
        }
        if (_boons is not null && e.Is(GameAction.R)) // premie po etapach i synergie
        {
            _boons();
            return true;
        }
        if (_brigade is null || !e.Is(GameAction.A)) return false;
        _brigade();
        return true;
    }

    /// <summary>Dotknięcie / klik narzędzia: rozpiska obrażeń.</summary>
    public override bool TapRow(int index)
    {
        if (index is MoreDown or MoreUp)
        {
            _scroll = System.Math.Clamp(_scroll + (index == MoreDown ? 1 : -1), 0, _maxScroll);
            return true;
        }
        if (index != WeaponRow || _breakdown is null) return false;
        _breakdown();
        return true;
    }

    public override void Draw(PhonePainter p)
    {
        // v0.23.0: wysokości sekcji (nagłówek + karta + odstęp); gdy całość się nie mieści – przewijanie sekcjami
        int[] rows = [ToolRows(), Slots().Count, _g.D.Materials.Length > 0 ? 4 : 3];
        var h = new float[3];
        for (var i = 0; i < 3; i++) h[i] = PhonePainter.RowH + rows[i] * PhonePainter.RowH + 8 + 4;
        var room = p.Bottom - p.Top + 4;
        _maxScroll = 0;
        if (h[0] + h[1] + h[2] > room)
        {
            _maxScroll = 2;
            for (var k = 1; k <= 2; k++)
            {
                var sum = (float)MoreH;
                for (var i = k; i < 3; i++) sum += h[i];
                if (sum > room) continue;
                _maxScroll = k;
                break;
            }
        }
        _scroll = System.Math.Clamp(_scroll, 0, _maxScroll);
        var y = p.Top;
        if (_scroll > 0)
        {
            More(p, y, false);
            y += MoreH;
        }
        var bottom = p.Bottom + 4 - (_maxScroll > 0 ? MoreH : 0);
        var last = _scroll;
        for (var i = _scroll; i < 3; i++)
        {
            if (i > _scroll && y + h[i] > (i == 2 ? p.Bottom + 4 : bottom)) break;
            y = i switch { 0 => DrawTool(p, y, rows[0]), 1 => DrawGear(p, y), _ => DrawBoons(p, y) } + 4;
            last = i;
        }
        if (last < 2) More(p, bottom, true);
    }

    /// <summary>Pasek ze strzałką „więcej niżej” (down) albo „wyżej” – dotknięcie przewija o sekcję.</summary>
    private static void More(PhonePainter p, float y, bool down)
    {
        var cx = Godot.Mathf.Round((p.Left + p.Right) / 2);
        var cy = y + MoreH / 2f;
        Godot.Vector2[] tri = down
            ? [new(cx - 6, cy - 3), new(cx + 6, cy - 3), new(cx, cy + 3)]
            : [new(cx - 6, cy + 3), new(cx + 6, cy + 3), new(cx, cy - 3)];
        p.C.DrawColoredPolygon(tri, Pal.Brand);
        p.Hit(new Godot.Rect2(p.Left, y - 4, p.Width, MoreH + 8), down ? MoreDown : MoreUp);
    }

    /// <summary>Wiersze karty narzędzia: w poziomie nazwa i rzadkość, w pionie też kryt i tryby (Cios, Patent z nazwą i opisem).</summary>
    private int ToolRows()
    {
        var g = _g;
        var full = PhoneView.Full;
        var belt = g.HasBelt();
        var b = g.WeaponBreakdown();
        var n = (full ? 6 : 2) + (belt ? 1 : 0) + WeaponLook.PerkRows(g.D, b.Perks, b.PerksMaster, full).Count;
        if (belt && g.Weapon2 >= 0 && g.Weapon2Perks != 0)
        {
            var b2 = g.WeaponBreakdown(-1, g.Weapon2, -1, -1, 0, g.Weapon2Rarity, g.Weapon2Perks);
            n += WeaponLook.PerkRows(g.D, b2.Perks, b2.PerksMaster, full).Count;
        }
        return n;
    }

    /// <summary>Sloty: kask, rękawice, kamizelka + buty i pas z nagród za odbiór (jeśli odebrane albo założone).</summary>
    private System.Collections.Generic.List<int> Slots()
    {
        var slots = new System.Collections.Generic.List<int>();
        for (var i = 0; i < _g.D.GearSlotsCount; i++)
        {
            if (((_g.Bonus.GearSlots >> i) & 1) != 0 || _g.Equipped[i] >= 0) slots.Add(i);
        }
        return slots;
    }

    private float DrawTool(PhonePainter p, float top, int rows)
    {
        var g = _g;
        var d = g.D;
        var y = p.Section(top, Loc.T("narzedzie_4"), _breakdown is null ? "" : ButtonNames.Pick(Loc.T("i_rozpiska"), Loc.T("dotknij_rozpiska")));
        var full = PhoneView.Full; // v0.23.0: tryby słowami tylko w pełnym telefonie (w poziomie brak miejsca)
        var belt = g.HasBelt();
        var b = g.WeaponBreakdown();   // zakres ciosu jak w walce (rozpiska #26)
        var perkLines = WeaponLook.PerkRows(d, b.Perks, b.PerksMaster, full); // v0.23.0 (#91): wiersz na profit, pełna nazwa i opis
        var belt2 = belt && g.Weapon2 >= 0 && g.Weapon2Perks != 0; // profity broni w kaburze – też w poziomie (zawijanie w osobnych wierszach)
        var b2 = belt2 ? g.WeaponBreakdown(-1, g.Weapon2, -1, -1, 0, g.Weapon2Rarity, g.Weapon2Perks) : default;
        var perkLines2 = belt2 ? WeaponLook.PerkRows(d, b2.Perks, b2.PerksMaster, full) : [];
        var c0 = p.Card(y, rows); // w poziomie: rzadkość i typ w wierszu krytu (bez trybów)
        var tx = p.TextX(c0);
        var right = c0.End.X - 6;
        var w = g.Weapon;
        var rc = WeaponLook.RarityColor(d, g.WeaponRarity);
        p.Stripe(c0, 0, rc);
        var pw = p.Pill(right, p.RowY(c0, 0), $"z{g.WeaponRange()} {UiText.StatShort(w.ScalesWith)}", PillKind.Group);
        var ix = tx + p.WeaponIcon(tx, p.RowY(c0, 0), w, WeaponSheet.Rarity(g)); // v0.23.0: ikona broni
        p.Text(ix, p.RowY(c0, 0), $"{g.WeaponTitle()} {b.Min}-{b.Max}", Ink.Dark, TextAlign.Left, right - pw - 4 - ix);
        p.Divider(c0, 1);
        var avg = DamageHelp.AddTenths(new Message(), b.Avg10).Text;
        var bState = g.WeaponBCd == 0 ? Loc.T("b_gotowy") : Loc.F("b_za", g.WeaponBCd);
        var row = 1;
        if (full)
        {
            p.Text(tx, p.RowY(c0, 1), Loc.F("kryt_srednio", b.CritMin, b.CritMax, b.CritChance(), avg), Ink.Prog, TextAlign.Left, right - tx);
            row = 2;
            p.Divider(c0, row);
        }
        p.HitRow(c0, 0, WeaponRow);
        p.HitRow(c0, 1, WeaponRow);
        // v0.23.0: rzadkość i typ ciosu (w poziomie z krytem), odnowienie Patentu, Cios i Patent, kabura
        p.Stripe(c0, row, rc);
        pw = p.Pill(right, p.RowY(c0, row), bState, g.WeaponBCd == 0 ? PillKind.Group : PillKind.Gray);
        var kind = $"{WeaponLook.RarityName(d, g.WeaponRarity)} · {WeaponLook.Hits(d, w)}"; // kryt w poziomie – w karcie premii niżej
        p.Text(tx, p.RowY(c0, row), kind, WeaponLook.PhoneInk(d, g.WeaponRarity), TextAlign.Left, right - pw - 4 - tx);
        row++;
        foreach (var line in perkLines)
        {
            p.Divider(c0, row);
            p.Stripe(c0, row, rc);
            p.Text(tx, p.RowY(c0, row), line, WeaponLook.PhoneInk(d, g.WeaponRarity), TextAlign.Left, right - tx);
            row++;
        }
        if (full)
        {
            p.Divider(c0, row);
            p.Text(tx, p.RowY(c0, row), WeaponLook.LineA(w), Ink.Dim, TextAlign.Left, right - tx);
            row++;
            p.Divider(c0, row); // v0.23.0: Patent – nazwa ruchu, pod nią wzór, efekt i odnowienie
            p.Text(tx, p.RowY(c0, row), Loc.T("tryb_fach") + ModeText.BName(w), Ink.Brand, TextAlign.Left, right - tx);
            row++;
            p.Text(tx + 8, p.RowY(c0, row), WeaponLook.Mode(w, w.ModeB, true, g.WeaponBCdDelta()), Ink.Dim, TextAlign.Left, right - tx - 8);
            row++;
        }
        if (belt)
        {
            p.Divider(c0, row);
            if (g.Weapon2 >= 0)
            {
                p.Stripe(c0, row, WeaponLook.RarityColor(d, g.Weapon2Rarity));
                var w2 = d.Weapons[g.Weapon2];
                var lvl = g.Weapon2Lvl > 0 ? $"+{g.Weapon2Lvl}" : "";
                p.Text(tx, p.RowY(c0, row), Loc.F("kabura", $"{w2.Name}{lvl} {w2.MinDamage}-{w2.MaxDamage} · {WeaponLook.Hits(d, w2)}"),
                    WeaponLook.PhoneInk(d, g.Weapon2Rarity), TextAlign.Left, right - tx);
                foreach (var line in perkLines2) // v0.23.0: profity broni w kaburze
                {
                    row++;
                    p.Divider(c0, row);
                    p.Stripe(c0, row, WeaponLook.RarityColor(d, g.Weapon2Rarity));
                    p.Text(tx, p.RowY(c0, row), line, WeaponLook.PhoneInk(d, g.Weapon2Rarity), TextAlign.Left, right - tx);
                }
            }
            else p.Text(tx, p.RowY(c0, row), Loc.T("kabura_pusta_2"), Ink.Dim, TextAlign.Left, right - tx);
        }
        return c0.End.Y;
    }

    private float DrawGear(PhonePainter p, float top)
    {
        var g = _g;
        var d = g.D;
        var slots = Slots();
        var bag = _backpack is null ? "" : ButtonNames.Pick(Loc.F("x_plecak", _g.BackpackCount(), _g.BackpackSlots()), Loc.F("plecak_n", _g.BackpackCount(), _g.BackpackSlots())) + "  "; // v0.23.2
        var y = p.Section(top, Loc.T("sprzet_8"), $"{bag}{CountEquipped()}/{slots.Count}");
        var c1 = p.Card(y, slots.Count);
        var tx = p.TextX(c1);
        var right = c1.End.X - 6;
        for (var k = 0; k < slots.Count; k++)
        {
            var i = slots[k];
            var r = p.RowY(c1, k);
            if (k > 0) p.Divider(c1, k);
            var rar = g.Equipped[i];
            p.Stripe(c1, k, rar < 0 ? Pal.Todo : rar == 2 ? Pal.Prog : rar == 1 ? Pal.Brand : Pal.Done);
            if (rar < 0)
            {
                p.Text(tx, r, Loc.F("brak_6", d.GearSlots[i]), Ink.Dim);
                continue;
            }
            var gd = d.Gear[i * 3 + rar];
            // co daje (np. „+2 OBR”, „+3 obrażeń”); nazwa przedmiotu, a gdy się nie mieści - nazwa slotu
            var eff = DamageHelp.GearLabel(gd);
            var tr = g.EquippedTrait[i];
            var nameW = p.F.Measure(d.GearTraits[tr].Name) + 10; // v0.23.0 (#91): cecha pełną nazwą, skrót gdy zabraknie miejsca na nazwę przedmiotu
            var free = right - nameW - 8 - tx - p.F.Measure(eff) - 6;
            var useFull = free - System.Math.Min(p.F.Measure(gd.Name), p.F.Measure(d.GearSlots[i])) >= 0; // v0.23.0: pełna cecha także przy nazwie slotu („Pas … Szczęście +1”)
            var pw = p.Pill(right, r, useFull ? d.GearTraits[tr].Name : d.GearTraits[tr].Short, rar == 2 ? PillKind.Prog : rar == 1 ? PillKind.Group : PillKind.Gray);
            var room = right - pw - 8 - tx - p.F.Measure(eff) - 6;
            var name = p.F.Measure(gd.Name) <= room ? gd.Name : p.F.Fit(d.GearSlots[i], (int)room);
            var nx = tx + p.Text(tx, r, name, Ink.Dark) + 6;
            p.Text(nx, r, eff, Ink.Brand);
        }
        return c1.End.Y;
    }

    private float DrawBoons(PhonePainter p, float top)
    {
        var g = _g;
        var d = g.D;
        var boonsRight = _boons is null ? "" : ButtonNames.Pick(Loc.F("r_premie", g.BoonsOwned()), Loc.F("premie_7", g.BoonsOwned()));
        var y = p.Section(top, Loc.T("premie_i_materialy"), boonsRight);
        var c2 = p.Card(y, d.Materials.Length > 0 ? 4 : 3);
        var tx = p.TextX(c2);
        var right = c2.End.X - 6;
        p.Text(tx, p.RowY(c2, 0), Loc.F("obrona_obraz_hp", g.GearBonus(GearStat.Def), g.GearBonus(GearStat.Dmg), g.GearBonus(GearStat.Hp)), Ink.Dim, TextAlign.Left, right - tx);
        p.Divider(c2, 1);
        p.Text(tx, p.RowY(c2, 1), Loc.F("kryt_unik_wzrok", g.CritPct(), g.DodgePct(), g.SightRadius()), Ink.Dim, TextAlign.Left, right - tx);
        p.Divider(c2, 2);
        p.Text(tx, p.RowY(c2, 2), Loc.F("szczescie_termos", g.Luck(), g.Thermos, g.ThermosCap()), Ink.Dim, TextAlign.Left, right - tx);
        if (d.Materials.Length == 0) return c2.End.Y;
        p.Divider(c2, 3); // materiały: Hurtownia i naprawy (Brygada)
        var mx = tx;
        for (var m = 0; m < d.Materials.Length; m++)
        {
            MaterialIcon.Draw(p.C, m, new Godot.Vector2(mx, p.RowY(c2, 3) + (PhonePainter.RowH - MaterialIcon.Size) / 2));
            mx += MaterialIcon.Size + 3;
            mx += p.Text(mx, p.RowY(c2, 3), $"{d.Materials[m].Name} {g.Mats[m]}", g.Mats[m] > 0 ? Ink.Dark : Ink.Dim) + 8;
        }
        return c2.End.Y;
    }

    private int CountEquipped()
    {
        var n = 0;
        for (var i = 0; i < _g.D.GearSlotsCount; i++)
        {
            if (_g.Equipped[i] >= 0) n++;
        }
        return n;
    }
}
