using System;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Gfx;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Tabs;

/// <summary>Zadania = harmonogram budowy: etapy z pastylkami Gotowe / W trakcie / Do zrob., wydarzenie na placu, pogoda dnia,
/// mechanika aktu i wybrana ścieżka (tab_tasks na GBA).</summary>
public sealed class TasksTab : PhonePage
{
    private readonly CoreGame _g;

    public TasksTab(CoreGame g) => _g = g;

    public override string Title => "Zadania";
    public override string Sub => $"Etap {_g.StageNumber()}/{_g.StagesInRun()}, {_g.Score} pkt";

    public override void Draw(PhonePainter p)
    {
        var d = _g.D;
        var y = p.Section(p.Top, "HARMONOGRAM", $"Akt {_g.ActNumeral()}");
        var f0 = _g.FirstStage;   // bez Aktu 0: od Fundamentów
        var n = _g.RouteCount() - f0;
        var rows = PhoneView.Full ? n : Math.Min(n, 5);   // wąski telefon (poziomo): okno etapów wokół bieżącego
        var first = f0 + Math.Max(0, Math.Min(_g.Stage - f0 - 1, n - rows));
        var card = p.Card(y, rows);
        for (var r = 0; r < rows; r++)
        {
            var i = first + r;
            var done = i < _g.Stage || (i == _g.Stage && _g.St == GameStatus.Won);
            var cur = i == _g.Stage && !done;
            var ry = p.RowY(card, r);
            if (r > 0) p.Divider(card, r);
            p.Stripe(card, r, done ? Pal.Done : cur ? Pal.Prog : Pal.Todo);
            var pill = done ? "Gotowe" : cur ? "W trakcie" : "Do zrob.";
            var pw = p.Pill(card.End.X - 6, ry, pill, done ? PillKind.Done : cur ? PillKind.Prog : PillKind.Gray);
            p.Text(p.TextX(card), ry, $"{i - f0 + 1}. {_g.SDef(i).Name}", done ? Ink.Dim : Ink.Dark, TextAlign.Left, card.End.X - 12 - pw - p.TextX(card));
        }
        y = card.End.Y + 4;
        y = p.Section(y, "PLAC BUDOWY");
        var hasPath = _g.StagePath >= 0 && _g.StagePath < d.Paths.Length;
        var extra = ExtraRow(out var extraText, out var extraPill, out var extraKind);
        var c2 = p.Card(y, (hasPath ? 4 : 3) + (extra ? 1 : 0));
        var r0 = p.RowY(c2, 0);
        if (_g.CurrentEvent is { } ev)
        {
            p.Stripe(c2, 0, ev.Good ? Pal.Done : Pal.Late);
            var pw = p.Pill(c2.End.X - 6, r0, ev.Short, ev.Good ? PillKind.Done : PillKind.Late);
            p.Text(p.TextX(c2), r0, ev.Name, Ink.Dark, TextAlign.Left, c2.End.X - 12 - pw - p.TextX(c2));
        }
        else
        {
            p.Text(p.TextX(c2), r0, "Plac: bez niespodzianek", Ink.Dim);
        }
        var r1 = p.RowY(c2, 1);
        p.Divider(c2, 1);
        var wd = _g.WDef; // pogoda dnia
        var calm = wd.Effect == WeatherEffect.None;
        p.Stripe(c2, 1, wd.Bad ? Pal.Late : calm ? Pal.Todo : Pal.Done);
        var wp = p.Pill(c2.End.X - 6, r1, wd.Short, wd.Bad ? PillKind.Late : calm ? PillKind.Gray : PillKind.Done);
        p.Text(p.TextX(c2), r1, $"Pogoda: {wd.Name}", Ink.Dark, TextAlign.Left, c2.End.X - 12 - wp - p.TextX(c2));
        var ad = _g.ADef; // mechanika aktu: błoto, porywy (za ile tur i w którą stronę), pył
        var r2 = p.RowY(c2, 2);
        p.Divider(c2, 2);
        p.Stripe(c2, 2, Pal.Late);
        var ap = p.Pill(c2.End.X - 6, r2, ad.MechShort, PillKind.Late);
        var gin = _g.GustIn();
        var mech = gin > 0 ? $"Poryw za {gin} t. {CoreGame.DirName(_g.GustDir())}"
            : _g.DocsNeeded() > 0 ? $"Dokumenty {_g.DocsCount()}/{_g.DocsNeeded()}, schody " + (_g.StairsLocked() ? "zamknięte" : "otwarte")
            : ad.MechName;
        p.Text(p.TextX(c2), r2, mech, Ink.Dark, TextAlign.Left, c2.End.X - 12 - ap - p.TextX(c2));
        if (extra) // wydarzenie z wyborem (z odpowiedzią) albo magazyn na etapie
        {
            var re = hasPath ? 4 : 3;
            var ry = p.RowY(c2, re);
            p.Divider(c2, re);
            p.Stripe(c2, re, extraKind == PillKind.Done ? Pal.Done : Pal.Brand);
            var ep = p.Pill(c2.End.X - 6, ry, extraPill, extraKind);
            p.Text(p.TextX(c2), ry, extraText, Ink.Dark, TextAlign.Left, c2.End.X - 12 - ep - p.TextX(c2));
        }
        if (!hasPath) return;
        var path = d.Paths[_g.StagePath]; // ścieżka wybrana na harmonogramie
        var r3 = p.RowY(c2, 3);
        p.Divider(c2, 3);
        p.Stripe(c2, 3, Pal.Brand);
        var pp = p.Pill(c2.End.X - 6, r3, path.Short, PillKind.Group);
        p.Text(p.TextX(c2), r3, $"Ścieżka: {path.Desc}", Ink.Dark, TextAlign.Left, c2.End.X - 12 - pp - p.TextX(c2));
    }

    /// <summary>Wiersz v0.21.50 cz. 3: wydarzenie etapu i odpowiedź, a bez niego magazyn (odkryty), klucz w kieszeni albo zasada
    /// wyzwania tygodnia (cz. 4).</summary>
    private bool ExtraRow(out string text, out string pill, out PillKind kind)
    {
        var g = _g;
        if (g.StageChoice >= 0)
        {
            var ev = g.D.ChoiceEvents[g.StageChoice];
            text = $"{ev.Name}: {ev.Choices[g.StageChoicePick].Label}";
            pill = "SMS";
            kind = PillKind.Brand;
            return true;
        }
        if (g.HasSecret && (g.Explored(g.SecretX, g.SecretY) || g.Keys > 0))
        {
            text = g.SecretOpen ? "Magazyn otwarty" : g.SecretDef.Name + (g.Keys > 0 ? ", masz klucz" : g.SecretDef.Breakable ? ", klucz lub wybuch" : ", potrzebny klucz");
            pill = g.SecretOpen ? "Otwarty" : "Magazyn";
            kind = g.SecretOpen ? PillKind.Done : PillKind.Prog;
            return true;
        }
        if (g.Keys > 0)
        {
            text = $"Klucz do magazynu: {g.Keys}";
            pill = "Klucz";
            kind = PillKind.Prog;
            return true;
        }
        if (g.Bonus.Weekly >= 0) // v0.21.50 cz. 4: wyzwanie tygodnia (#34)
        {
            text = g.D.Weekly[g.Bonus.Weekly].Name;
            pill = "Tydzień";
            kind = PillKind.Late;
            return true;
        }
        text = pill = "";
        kind = PillKind.Gray;
        return false;
    }
}
