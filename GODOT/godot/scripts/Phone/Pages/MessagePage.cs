using System.Collections.Generic;
using Godot;
using LifeLike.Core.Data;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Wiadomości w telefonie (phone_message na GBA): nadawca z „teraz”, dymek SMS, pod spodem wiersze informacji
/// (siła problemów, boss, poziom trudności, wydarzenie na placu). Kilka SMS-ów to jedna rozmowa.
/// </summary>
public sealed class MessagePage : PhonePage
{
    private readonly string _sub;
    private readonly List<StoryMsg> _msgs = new();
    private readonly List<(string Text, Ink Ink)> _info = new();
    private readonly string _hint;

    public MessagePage(string sub, string hint = "Enter: dalej")
    {
        _sub = sub;
        _hint = hint;
    }

    public MessagePage Add(StoryMsg m)
    {
        _msgs.Add(m);
        return this;
    }

    public MessagePage Info(string text, Ink ink)
    {
        if (!string.IsNullOrEmpty(text)) _info.Add((text, ink));
        return this;
    }

    public override string Title => "Wiadomości";
    public override string Sub => _sub;
    public override string Hint => _hint;

    /// <summary>Przycisk dotykowy z podpowiedzi („Enter: do roboty” -> „Do roboty”).</summary>
    public override PageAction[] Actions
    {
        get
        {
            var label = _hint.Contains(':') ? _hint[(_hint.IndexOf(':') + 1)..].Trim() : "Dalej";
            if (label.Length > 0) label = char.ToUpperInvariant(label[0]) + label[1..];
            return [new(label, GameAction.Start)];
        }
    }

    public override void Draw(PhonePainter p)
    {
        var y = p.Top;
        foreach (var m in _msgs)
        {
            var text = string.Join(" ", m.Lines).Trim();
            var lines = p.F.Wrap(text, (int)p.Width - 34);
            var card = p.CardH(y, 26 + lines.Count * 16 + 8);
            p.Icon(Assets.PhoneIcons, 5, Assets.Icon, new Vector2(card.Position.X + 6, card.Position.Y + 5));
            p.Text(card.Position.X + 26, card.Position.Y + 3, m.From, Ink.Dark, TextAlign.Left, card.Size.X - 80);
            p.Pill(card.End.X - 6, card.Position.Y + 3, "teraz", PillKind.Gray);
            var bubble = new Rect2(card.Position.X + 8, card.Position.Y + 24, card.Size.X - 16, lines.Count * 16 + 6);
            p.C.DrawStyleBox(Ui.Box(Pal.Group, 8), bubble);
            for (var i = 0; i < lines.Count; i++) p.F.Draw(p.C, new Vector2(bubble.Position.X + 8, bubble.Position.Y + 2 + i * 16), lines[i], Ink.Dark);
            y = card.End.Y + 6;
        }
        if (_info.Count == 0) return;
        var tx = p.Left + 12;
        var width = (int)(p.Right - 6 - tx);
        var wrapped = new List<List<string>>();
        var rows = 0;
        var room = (int)((p.Bottom - y - 8) / PhonePainter.RowH); // wiersze, które mieszczą się nad podpowiedzią
        var extra = room - _info.Count;                           // tyle informacji może zająć drugi wiersz
        foreach (var (text, _) in _info)
        {
            var l = p.F.Wrap(text, width);
            if (l.Count > 1 && extra > 0) l = l.GetRange(0, 2);
            else l = [p.F.Fit(text, width)];
            if (l.Count > 1) extra--;
            wrapped.Add(l);
            rows += l.Count;
        }
        var ic = p.Card(y, rows);
        var r = 0;
        for (var i = 0; i < _info.Count; i++)
        {
            if (i > 0) p.Divider(ic, r);
            p.C.DrawRect(new Rect2(ic.Position.X + 4, p.RowY(ic, r) + 2, 3, wrapped[i].Count * PhonePainter.RowH - 4), _info[i].Ink.Fill);
            foreach (var line in wrapped[i]) p.Text(tx, p.RowY(ic, r++), line, _info[i].Ink);
        }
    }
}
