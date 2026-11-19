using System;
using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using LifeLike.Game.Session;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// v0.22.0 (UX A): między budowami rozmowa z Kierownikiem Markiem (zamiast SMS-a końca budowy i planszy końcowej).
/// Dymki jak w komunikatorze, przyklejone do dołu (na małym ekranie najstarsze znikają u góry), pod nimi nagroda za
/// następny odbiór i szybkie odpowiedzi: kup jednym stuknięciem, Szkolenia, kolejna budowa, szczegóły, menu.
/// </summary>
public sealed class BetweenRunsPage : PhonePage
{
    private const int NotesFull = 4, NotesSmall = 2, LineH = 16;

    private readonly GameSession _s;
    private readonly bool _canPlus;
    private readonly List<string> _notes;
    private readonly List<List<string>> _chat = new(); // dymek = wiersze; wiersz zaczynający się od '\u0001' – pogrubiony
    private readonly List<QuickReply> _replies = new();
    private int _sel;

    public BetweenRunsPage(GameSession s, bool canPlus)
    {
        _s = s;
        _canPlus = canPlus;
        _notes = EndNotes.Of(s);
        var g = s.Game;
        Say(g.St == GameStatus.Won ? Loc.T("miedzy_wygrana") : Loc.T("miedzy_przegrana"));
        Say(Loc.F("miedzy_wynik", g.Score));
        Say(Loc.F("miedzy_dosw", s.LastGained, s.Profile.Xp));
        if (_notes.Count > 0) _chat.Add(null); // miejsce na „Do tego:” (liczba wierszy zależy od ekranu)
        Offer();
    }

    /// <summary>Wybrana odpowiedź (klawiatura, pad, stuknięcie).</summary>
    public Action<QuickReply> OnReply;

    public IReadOnlyList<QuickReply> Replies => _replies;

    /// <summary>Wiersze „Do tego:” (test dymny).</summary>
    public IReadOnlyList<string> Notes => _notes;

    /// <summary>Dymki rozmowy jako tekst (test dymny, zrzuty).</summary>
    public IEnumerable<string> Chat
    {
        get
        {
            foreach (var b in _chat)
            {
                if (b is not null) yield return string.Join(" ", b);
            }
        }
    }

    public override string Title => Loc.T("wiadomosci_2");
    public override string Sub => Loc.F("dosw_5", _s.Profile.Xp);
    public override string Hint => Loc.T("miedzy_klawisze");
    public override PageAction[] Actions => [new(Loc.T("kolejna_budowa"), GameAction.Start)];

    /// <summary>Po zakupie: odpowiedź Marka i nowa oferta (następna rzecz albo „brakuje”).</summary>
    public void Bought(string name)
    {
        Say(Loc.F("miedzy_kupione", name));
        Offer();
        Redraw();
    }

    private void Say(string text) => _chat.Add([text]);

    /// <summary>Oferta zakupu i odpowiedzi do niej (punkt 3 rozmowy).</summary>
    private void Offer()
    {
        var d = _s.Data;
        var p = _s.Profile;
        var qc = Spend.QuickKind(d, p, out var qk, out var qi);
        var tree = Spend.TreePick(d, p);
        var nc = Meta.NextUnlock(d, p, out var nk, out var ni);
        if (qc >= 0) Say(Loc.F("miedzy_kup", Recap.UnlockName(d, p, qk, qi), qc));
        else if (tree >= 0) Say(Loc.F("miedzy_drzewko", d.TreeBranches[d.TreeNodes[tree].Branch].Name));
        else if (nc >= 0) Say(Loc.F("miedzy_brakuje", Recap.UnlockName(d, p, nk, ni), nc - p.Xp));
        _replies.Clear();
        if (qc >= 0) _replies.Add(QuickReply.Buy);
        else if (nc >= 0) _replies.Add(QuickReply.Training);
        if (!Layout.Touch) _replies.Add(QuickReply.Next); // dotyk: kolejna budowa to przycisk na dole strony
        if (_canPlus) _replies.Add(QuickReply.NextPlus);
        _replies.Add(QuickReply.Details);
        _replies.Add(QuickReply.Menu);
        _sel = 0;
    }

    private string ReplyLabel(QuickReply r)
    {
        var d = _s.Data;
        var p = _s.Profile;
        return r switch
        {
            QuickReply.Buy => Spend.QuickKind(d, p, out var k, out var i) >= 0 ? Loc.F("kup_x", Recap.UnlockName(d, p, k, i)) : "",
            QuickReply.Training => Loc.T("pokaz_szkolenia"),
            QuickReply.Next => Loc.T("kolejna_budowa"),
            QuickReply.NextPlus => Loc.T("kolejna_budowa_ng"),
            QuickReply.Details => Loc.T("szczegoly_budowy"),
            _ => Loc.T("menu"),
        };
    }

    public override bool Input(InputCmd e)
    {
        if (_replies.Count == 0) return false;
        if (e.Is(GameAction.Up, true) || e.Is(GameAction.Down, true))
        {
            _sel = (_sel + (e.Is(GameAction.Up, true) ? _replies.Count - 1 : 1)) % _replies.Count;
            Redraw();
            return true;
        }
        if (!e.Is(GameAction.A)) return false;
        OnReply?.Invoke(_replies[Math.Clamp(_sel, 0, _replies.Count - 1)]);
        return true;
    }

    public override bool TapRow(int index)
    {
        if (index < 0 || index >= _replies.Count) return false;
        _sel = index;
        OnReply?.Invoke(_replies[index]);
        return true;
    }

    public override void Draw(PhonePainter p)
    {
        var f = p.F;
        var full = PhoneView.Full;
        var w = (int)p.Width - 24;
        var rh = PhonePainter.RowH + (full ? 8 : 2);
        var repliesH = _replies.Count * (rh + 4);
        var nr = Spend.NextReward(_s.Data, _s.Profile, out var weapon);
        var rewardH = nr >= 0 ? PhonePainter.RowH + 8 + 6 : 0;
        var bubbles = new List<List<string>>();
        foreach (var b in _chat) bubbles.Add(b is null ? NotesBubble(f, w, full) : Wrap(f, b, w));
        var heights = bubbles.ConvertAll(b => b.Count * LineH + 8);
        var senderH = LineH + 6;

        // całość mieści się – od góry; inaczej rozmowa przyklejona do odpowiedzi na dole, najstarsze dymki znikają u góry
        var chatH = senderH;
        foreach (var h in heights) chatH += h + 6;
        var fits = p.Top + chatH + rewardH + repliesH <= p.Bottom;
        var bottom = (fits ? p.Top + chatH : p.Bottom - repliesH - rewardH) - 6;
        var first = bubbles.Count;
        var used = 0f;
        while (first > 0 && bottom - used - heights[first - 1] >= p.Top + senderH)
        {
            used += heights[first - 1] + 6;
            first--;
        }
        var by = bottom - used + 6;
        if (first < bubbles.Count)
        {
            p.Icon(Assets.PhoneIcons, 5, Assets.Icon, new Vector2(p.Left + 2, by - LineH - 4));
            p.Text(p.Left + 22, by - LineH - 4, Loc.T("kierownik_marek"), Ink.Dark);
        }
        for (var i = first; i < bubbles.Count; i++)
        {
            var bubble = new Rect2(p.Left, by, Mathf.Min(p.Width - 16, Widest(f, bubbles[i]) + 16), heights[i]);
            p.C.DrawStyleBox(Ui.Box(Pal.Card, 10, Pal.Border), bubble);
            for (var l = 0; l < bubbles[i].Count; l++)
            {
                var line = bubbles[i][l];
                var bold = line.Length > 0 && line[0] == '\u0001';
                f.Draw(p.C, new Vector2(bubble.Position.X + 8, bubble.Position.Y + 4 + l * LineH), bold ? line[1..] : line, bold ? Ink.Brand : Ink.Dark, TextAlign.Left, 1, bold);
            }
            by = bubble.End.Y + 6;
        }
        var y = bottom + 6;

        // nagroda za następny odbiór
        if (nr >= 0)
        {
            var reward = _s.Data.Rewards[nr];
            var name = weapon >= 0 && _s.Data.Weapons[weapon].Name != reward.Name ? $"{reward.Name} ({_s.Data.Weapons[weapon].Name})" : reward.Name;
            var card = p.CardH(y, PhonePainter.RowH + 8);
            p.Stripe(card, 0, Pal.Done);
            p.Text(p.TextX(card), p.RowY(card, 0) + PhonePainter.TextDy, Loc.F("za_kolejny_odbior_2", name), Ink.Done, TextAlign.Left, card.Size.X - 20);
            y = card.End.Y + 6;
        }

        // szybkie odpowiedzi
        for (var i = 0; i < _replies.Count; i++)
        {
            var r = new Rect2(p.Left + (full ? 24 : 12), y + i * (rh + 4), p.Width - (full ? 24 : 12), rh);
            var primary = _replies[i] is QuickReply.Buy or QuickReply.Training;
            var on = i == _sel && !Layout.Touch;
            p.C.DrawStyleBox(Ui.Box(primary ? Pal.Brand : Pal.Card, (int)(rh / 2f), on ? Pal.Text : Pal.Brand), r);
            f.Draw(p.C, new Vector2(r.GetCenter().X, Mathf.Round(r.GetCenter().Y - 8)), f.Fit(ReplyLabel(_replies[i]), (int)r.Size.X - 16), primary ? Ink.OnBrand : Ink.Brand, TextAlign.Center);
            p.Hit(r, i);
        }
    }

    private static List<string> Wrap(PixelFont f, List<string> b, int w)
    {
        var lines = new List<string>();
        foreach (var t in b) lines.AddRange(f.Wrap(t, w));
        return lines;
    }

    private static float Widest(PixelFont f, List<string> lines)
    {
        var m = 0f;
        foreach (var l in lines) m = Mathf.Max(m, l.Length > 0 && l[0] == '\u0001' ? f.Measure(l[1..], 1, true) : f.Measure(l));
        return m;
    }

    /// <summary>„Do tego:” + najwyżej 4 wiersze (2 w małym telefonie), reszta „i N więcej w szczegółach”.</summary>
    private List<string> NotesBubble(PixelFont f, int w, bool full)
    {
        var max = full ? NotesFull : NotesSmall;
        var lines = new List<string> { "\u0001" + Loc.T("do_tego") };
        for (var i = 0; i < _notes.Count && i < max; i++) lines.Add(f.Fit("- " + _notes[i], w));
        if (_notes.Count > max) lines.Add(Loc.F("i_wiecej", _notes.Count - max));
        return lines;
    }
}
