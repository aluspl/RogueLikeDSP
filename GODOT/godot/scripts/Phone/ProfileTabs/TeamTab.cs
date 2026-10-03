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

    public override string Title => Loc.T("zespol");
    public override string Sub => Loc.F("wygrane_3", Meta.ClassesWon(_d, _p), _d.Classes.Length);
    public override string Hint => Loc.T("q_e_zakladki_esc_wroc");

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
            var ml = Progress.MasteryLevel(_d, _p, i); // v0.21.52 cz. b (#45): poziom mistrzostwa zawodu
            var pill = !unl ? Loc.T("zablok") : ml > 0 ? Loc.F("mistrz_3", ml) : won ? Loc.T("wygrana_2") : Loc.T("dostepny");
            var pw = p.Pill(right, y, pill, !unl ? PillKind.Gray : won || ml >= _d.MasteryLevels.Length ? PillKind.Done : PillKind.Group);
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
        p.Text(x, dc.Position.Y + 4, Loc.T("moc_4") + c.AbilityName, Ink.Dark, TextAlign.Left, right - x);
        var lines = p.F.Wrap(c.AbilityDesc, (int)(right - x));
        if (lines.Count > 0) p.Text(x, dc.Position.Y + 22, lines[0], Ink.Dim, TextAlign.Left, right - x);
        var bottom = unlocked ? c.Desc : LockedHint(s);
        if (unlocked && Progress.MasteryXp(_p, s) > 0) // mistrzostwo: dośw. do kolejnego poziomu i jego nagroda
        {
            var ml = Progress.MasteryLevel(_d, _p, s);
            Progress.MasteryBar(_d, _p, s, out var cur, out var need);
            bottom = need > 0 ? Loc.F("mistrz_dalej", ml, cur, need, Progress.RewardLabel(_d, _d.MasteryLevels[ml], s)) : Loc.F("mistrz_wszystko", ml);
        }
        p.Text(dc.Position.X + 8, dc.Position.Y + 44, bottom, unlocked ? Ink.Dim : Ink.Brand, TextAlign.Left, right - dc.Position.X - 8);
    }

    // Zablokowany zawód: z nagrody za odbiór (za którą wygraną) albo do kupienia w Szkoleniach.
    private string LockedHint(int cls)
    {
        if (Meta.ClassSecret(_d, cls)) // v0.21.51 cz. 2: zawód z sekretnego zlecenia
        {
            var si = Secrets.Of(_d, SecretReward.Cls, cls);
            return Loc.T("sekret_3") + (si >= 0 ? _d.Secrets[si].Hint : "???");
        }
        if (!Meta.ClassReward(_d, cls)) return Loc.F("odblokujesz_w_kosztach_dosw", Meta.ClassCost(_d, _p));
        for (var i = 0; i < _d.Rewards.Length; i++)
            if (_d.Rewards[i].Kind == RewardKind.Cls && _d.Rewards[i].Index == cls) return Loc.F("nagroda_za_wygrana", Meta.RewardWin(_d, _p, i));
        return Loc.T("nagroda_za_odbior");
    }
}
