using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;

namespace LifeLike.Game.Phone.ProfileTabs;

/// <summary>Zespół (zakładka 3 profilu na GBA): zawody z pastylką Wygrana / Dostępny / Zablok. i opisem mocy zaznaczonego.</summary>
public sealed class TeamTab : PhonePage
{
    private readonly GameData _d;
    private readonly Profile _p;
    private readonly ListState _list = new();

    public TeamTab(GameData d, Profile p)
    {
        _d = d;
        _p = p;
    }

    public override string Title => "Zespół";
    public override string Sub => $"Wygrane {Meta.ClassesWon(_d, _p)}/{_d.Classes.Length}";
    public override string Hint => "Q/E: zakładki  Esc: wróć";

    public override bool Input(InputCmd e)
    {
        var v = e.VDir;
        if (v == 0) return false;
        _list.Move(v, _d.Classes.Length, _d.Classes.Length);
        return true;
    }

    public override void Draw(PhonePainter p)
    {
        var n = _d.Classes.Length;
        var card = p.Card(p.Top, n);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        for (var i = 0; i < n; i++)
        {
            var y = p.RowY(card, i);
            var sel = i == _list.Sel;
            if (sel) p.Selected(card, i);
            else if (i > 0) p.Divider(card, i);
            var won = Meta.ClassWon(_p, i);
            var unl = Meta.ClassUnlocked(_d, _p, i);
            var pw = p.Pill(right, y, won ? "Wygrana" : unl ? "Dostępny" : "Zablok.", won ? PillKind.Done : unl ? PillKind.Group : PillKind.Gray);
            p.Text(tx, y, _d.Classes[i].Name, sel ? Ink.Brand : unl ? Ink.Dark : Ink.Dim, TextAlign.Left, right - pw - 4 - tx);
        }
        var s = _list.Sel;
        var c = _d.Classes[s];
        var dc = p.CardH(card.End.Y + 6, 66);
        var unlocked = Meta.ClassUnlocked(_d, _p, s);
        var photo = new Rect2(dc.Position.X + 8, dc.Position.Y + 8, 32, 32);
        p.C.DrawStyleBox(Ui.Box(Pal.Group, 5), photo);
        p.Icon(Assets.Actors, unlocked ? Assets.HeroFrame(_d, _p, s) : Assets.Silhouette(s), Assets.Actor, photo.Position);
        var x = photo.End.X + 8;
        p.Text(x, dc.Position.Y + 4, "Moc: " + c.AbilityName, Ink.Dark, TextAlign.Left, right - x);
        var lines = p.F.Wrap(c.AbilityDesc, (int)(right - x));
        if (lines.Count > 0) p.Text(x, dc.Position.Y + 22, lines[0], Ink.Dim, TextAlign.Left, right - x);
        p.Text(dc.Position.X + 8, dc.Position.Y + 44, unlocked ? c.Desc : LockedHint(s), unlocked ? Ink.Dim : Ink.Brand, TextAlign.Left, right - dc.Position.X - 8);
    }

    // Zablokowany zawód: z nagrody za odbiór (za którą wygraną) albo do kupienia w Szkoleniach.
    private string LockedHint(int cls)
    {
        if (Meta.ClassSecret(_d, cls)) // v0.21.51 cz. 2: zawód z sekretnego zlecenia
        {
            var si = Secrets.Of(_d, SecretReward.Cls, cls);
            return "Sekret: " + (si >= 0 ? _d.Secrets[si].Hint : "???");
        }
        if (!Meta.ClassReward(_d, cls)) return $"Odblokujesz w Kosztach: {_d.ClassCost} dośw.";
        for (var i = 0; i < _d.Rewards.Length; i++)
            if (_d.Rewards[i].Kind == RewardKind.Cls && _d.Rewards[i].Index == cls) return $"Nagroda za {Meta.RewardWin(_d, _p, i)}. wygraną";
        return "Nagroda za odbiór";
    }
}
