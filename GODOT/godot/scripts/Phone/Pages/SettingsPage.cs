using System;
using System.Collections.Generic;
using Godot;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using LifeLike.Game.Settings;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Ustawienia w stylu aplikacji PlanBudowlany (klucz w rogu ekranu, Esc): głośność muzyki i dźwięków, wibracje,
/// sterowanie dotykiem (gesty + pasek / gałka), ręka paska akcji, wielkość tekstu, Jak grać, w trakcie budowy
/// Zapisz i wyjdź oraz Porzuć budowę (z potwierdzeniem), wersja gry i link planbudowlany.online.
/// Strzałki: wiersz i wartość, Spacja: wykonaj; dotyk: wiersz albo przyciski -/+. Otwarcie nie zużywa tury.
/// </summary>
public sealed class SettingsPage : PhonePage
{
    public const string Url = "https://planbudowlany.online";
    private const int Minus = 100, Plus = 200;

    private readonly bool _inRun;
    private readonly string _version;
    private readonly List<SettingsRow> _rows = new();
    private int _sel;
    private bool _confirm;

    public SettingsPage(bool inRun, string version)
    {
        _inRun = inRun;
        _version = version;
        _rows.AddRange([SettingsRow.Music, SettingsRow.Sound, SettingsRow.Vibration, SettingsRow.Controls, SettingsRow.Hand, SettingsRow.Text, SettingsRow.Help]);
        if (inRun) _rows.AddRange([SettingsRow.SaveExit, SettingsRow.Abandon]);
        _rows.Add(SettingsRow.Link);
    }

    /// <summary>Jak grać, Zapisz i wyjdź, Porzuć budowę - wykonuje ekran ustawień.</summary>
    public Action<SettingsRow> OnAction;

    public override string Title => "Ustawienia";
    public override string Sub => _version;
    public override string Hint => "Strzałki: wybór i wartość  Spacja: wykonaj  Esc: wróć";
    public override bool Closable => true;

    public SettingsRow Selected => _rows[_sel];

    public override bool Input(InputCmd e)
    {
        var v = e.VDir;
        if (v != 0)
        {
            _sel = (_sel + v + _rows.Count) % _rows.Count;
            _confirm = false;
            Sfx.Play("menu");
            return true;
        }
        var h = e.HDir;
        if (h != 0)
        {
            Change(Selected, h);
            return true;
        }
        if (!e.Is(GameAction.A)) return false;
        Activate(Selected);
        return true;
    }

    public override bool TapRow(int index)
    {
        if (index >= Plus) Change(_rows[index - Plus], 1);
        else if (index >= Minus) Change(_rows[index - Minus], -1);
        else
        {
            if (_sel != index) _confirm = false;
            _sel = index;
            Activate(Selected);
        }
        return true;
    }

    /// <summary>Strzałka w lewo/prawo albo -/+: głośność o krok, przełączniki w obie strony.</summary>
    public void Change(SettingsRow row, int d)
    {
        switch (row)
        {
            case SettingsRow.Music:
                GameSettings.Music = Mathf.Clamp(GameSettings.Music + d, 0, GameSettings.VolumeSteps);
                break;
            case SettingsRow.Sound:
                GameSettings.Sound = Mathf.Clamp(GameSettings.Sound + d, 0, GameSettings.VolumeSteps);
                break;
            case SettingsRow.Vibration:
            case SettingsRow.Controls:
            case SettingsRow.Hand:
            case SettingsRow.Text:
                Toggle(row);
                return;
            default:
                return;
        }
        GameSettings.Save();
        Sfx.Play("menu");
        Redraw();
    }

    private void Toggle(SettingsRow row)
    {
        switch (row)
        {
            case SettingsRow.Vibration:
                GameSettings.Vibration = !GameSettings.Vibration;
                if (GameSettings.Vibration) Haptics.Pulse(40);
                break;
            case SettingsRow.Controls:
                GameSettings.Controls = GameSettings.Controls == ControlScheme.Swipe ? ControlScheme.Joystick : ControlScheme.Swipe;
                break;
            case SettingsRow.Hand:
                GameSettings.LeftHanded = !GameSettings.LeftHanded;
                break;
            case SettingsRow.Text:
                GameSettings.LargeText = !GameSettings.LargeText;
                break;
        }
        GameSettings.Save();
        Sfx.Play("menu");
        Redraw();
    }

    private void Activate(SettingsRow row)
    {
        switch (row)
        {
            case SettingsRow.Music:
            case SettingsRow.Sound:
                var cur = row == SettingsRow.Music ? GameSettings.Music : GameSettings.Sound;
                Change(row, cur >= GameSettings.VolumeSteps ? -GameSettings.VolumeSteps : 1); // Spacja: +1, po maksimum od zera
                return;
            case SettingsRow.Link:
                OS.ShellOpen(Url);
                return;
            case SettingsRow.Abandon when !_confirm:
                _confirm = true;
                Sfx.Play("hurt", 0.4f);
                Redraw();
                return;
            case SettingsRow.Help:
            case SettingsRow.SaveExit:
            case SettingsRow.Abandon:
                _confirm = false;
                OnAction?.Invoke(row);
                return;
            default:
                Toggle(row);
                return;
        }
    }

    public override void Draw(PhonePainter p)
    {
        var y = p.Section(p.Top, "DŹWIĘK");
        y = Group(p, y, 0, 3) + 4;
        y = p.Section(y, "STEROWANIE");
        y = Group(p, y, 3, 3) + 4;
        y = p.Section(y, "GRA");
        var n = _rows.Count - 7;
        y = Group(p, y, 6, n) + 4;
        Group(p, y, _rows.Count - 1, 1);
    }

    private float Group(PhonePainter p, float y, int from, int count)
    {
        var card = p.Card(y, count);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        for (var k = 0; k < count; k++)
        {
            var i = from + k;
            var row = _rows[i];
            var ry = p.RowY(card, k);
            var sel = i == _sel;
            if (sel) p.Selected(card, k);
            else if (k > 0) p.Divider(card, k);
            var ink = sel ? Ink.Brand : Ink.Dark;
            switch (row)
            {
                case SettingsRow.Music:
                case SettingsRow.Sound:
                {
                    var val = row == SettingsRow.Music ? GameSettings.Music : GameSettings.Sound;
                    var plus = p.Pill(right, ry, "+", PillKind.Group);
                    var bw = Mathf.Min(90, (right - tx) * 0.35f);
                    var barX = right - plus - 6 - bw;
                    p.Bar(barX, ry, bw, val, GameSettings.VolumeSteps, Pal.Brand);
                    var minusR = barX - 6;
                    var minus = p.Pill(minusR, ry, "-", PillKind.Group);
                    p.Hit(new Rect2(right - plus - 10, ry - 4, plus + 16, PhonePainter.RowH + 8), Plus + i);
                    p.Hit(new Rect2(minusR - minus - 6, ry - 4, minus + 14, PhonePainter.RowH + 8), Minus + i);
                    p.Text(tx, ry, row == SettingsRow.Music ? "Muzyka" : "Dźwięki", ink, TextAlign.Left, minusR - minus - 6 - tx);
                    break;
                }
                case SettingsRow.Link:
                    p.Icon(Assets.TouchIcons, Touch.TouchIcon.Link, Assets.Icon, new Vector2(tx - 2, ry + (PhonePainter.RowH - 16) / 2));
                    p.Text(tx + 18, ry, "planbudowlany.online", Ink.Brand);
                    p.Text(right, ry, ">", Ink.Dim, TextAlign.Right);
                    break;
                default:
                {
                    var (label, value, kind) = Describe(row);
                    var pw = value.Length > 0 ? p.Pill(right, ry, value, kind) : p.Text(right, ry, ">", Ink.Dim, TextAlign.Right);
                    var labelInk = row == SettingsRow.Abandon ? Ink.Late : ink;
                    p.Text(tx, ry, label, labelInk, TextAlign.Left, right - pw - 6 - tx);
                    break;
                }
            }
            p.HitRow(card, k, i);
        }
        return card.End.Y;
    }

    private (string Label, string Value, PillKind Kind) Describe(SettingsRow row) => row switch
    {
        SettingsRow.Vibration => ("Wibracje", GameSettings.Vibration ? "Wł." : "Wył.", GameSettings.Vibration ? PillKind.Done : PillKind.Gray),
        SettingsRow.Controls => ("Sterowanie", GameSettings.Controls == ControlScheme.Swipe ? "Gesty + pasek" : "Gałka + pasek", PillKind.Group),
        SettingsRow.Hand => ("Pasek akcji", GameSettings.LeftHanded ? "Lewa ręka" : "Prawa ręka", PillKind.Group),
        SettingsRow.Text => ("Tekst", GameSettings.LargeText ? "Duży" : "Normalny", GameSettings.LargeText ? PillKind.Brand : PillKind.Group),
        SettingsRow.Help => ("Jak grać", "", PillKind.Gray),
        SettingsRow.SaveExit => ("Zapisz i wyjdź", "", PillKind.Gray),
        SettingsRow.Abandon => (_confirm ? "Na pewno porzucić? Jeszcze raz" : "Porzuć budowę", _confirm ? "Tak" : "", PillKind.Late),
        _ => ("", "", PillKind.Gray),
    };
}
