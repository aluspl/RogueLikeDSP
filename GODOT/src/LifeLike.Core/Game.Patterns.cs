using LifeLike.Core.Data;

namespace LifeLike.Core;

// v0.23.0: atak wieloceowy – wzory pól trybu ataku (HitPattern) stosowane do problemów.
public sealed partial class Game
{
    /// <summary>Zasięg wzoru trybu (z wiatrem dla zasięgu &gt; 1).</summary>
    public int ModeRange(WeaponMode m) => WindRange(m.Range);

    /// <summary>
    /// Czy cel ei leży na polu wzoru trybu w swoim kierunku. Single/Push/Throw3x3: dzisiejszy test zasięgu (odległość Czebyszewa),
    /// Shot: w zasięgu, a w linii – bez ściany po drodze; wzory Line*, Arc, Around, Cross, Spray: cel trafiony wzorem.
    /// </summary>
    public bool InPattern(WeaponMode m, int ei)
    {
        var e = Enemies[ei];
        var d = Cheb(Hero.X, Hero.Y, e.X, e.Y);
        if (d == 0) return false;
        var rg = ModeRange(m);
        switch (m.Pattern)
        {
            case PatternKind.Single:
            case PatternKind.Push:
            case PatternKind.Throw3x3:
                return d <= rg;
            case PatternKind.Shot:
                if (d > rg) return false;
                if (!OnLine(e.X, e.Y)) return true;
                break;
        }
        Span<int> hit = stackalloc int[HitPattern.MaxCells];
        var n = Targets(m, ei, hit);
        if (m.Pattern == PatternKind.Shot) return n > 0;
        for (var i = 0; i < n; ++i)
        {
            if (hit[i] == ei) return true;
        }
        return false;
    }

    private bool OnLine(int tx, int ty) => Hero.X == tx || Hero.Y == ty || Math.Abs(tx - Hero.X) == Math.Abs(ty - Hero.Y);

    /// <summary>
    /// Żywe problemy trafione trybem m w kierunku celu ei (kolejność od bohatera): promienie zatrzymują się na pierwszej ścianie,
    /// Shot daje pierwszy cel, Line* przebija. Zwraca liczbę zapisaną w hit.
    /// </summary>
    private int Targets(WeaponMode m, int ei, Span<int> hit)
    {
        int tx = Enemies[ei].X, ty = Enemies[ei].Y;
        int dx = Math.Sign(tx - Hero.X), dy = Math.Sign(ty - Hero.Y);
        var pat = m.Pattern;
        var hits = 0;
        if (pat == PatternKind.Single || pat == PatternKind.Push || (pat == PatternKind.Shot && !OnLine(tx, ty)))
        {
            hit[hits++] = ei; // dzisiejsze zachowanie: cel w zasięgu dostaje cios
            return hits;
        }
        Span<int> xs = stackalloc int[HitPattern.MaxCells];
        Span<int> ys = stackalloc int[HitPattern.MaxCells];
        int n;
        if (pat == PatternKind.Throw3x3) n = HitPattern.Area3x3(tx, ty, xs, ys);
        else n = HitPattern.Cells(pat, ModeRange(m), Hero.X, Hero.Y, dx, dy, xs, ys);
        var ray = HitPattern.IsRay(pat);
        var prev = 0;
        var blocked = false;
        for (var i = 0; i < n; ++i)
        {
            var d = Cheb(Hero.X, Hero.Y, xs[i], ys[i]);
            if (d <= prev) blocked = false; // nowy promień
            prev = d;
            if (ray && blocked) continue;
            if (!Lv.Passable(xs[i], ys[i])) // tylko ściana blokuje (schody nie)
            {
                if (ray) blocked = true;
                continue;
            }
            // wzory nieprzebijające ścian (natrysk, łuk, krzyż): dalsze pola za ścianą przed bohaterem nie są trafiane
            if (!ray && d >= 2 && pat is PatternKind.Spray or PatternKind.Arc or PatternKind.Cross && !Lv.Passable(Hero.X + dx, Hero.Y + dy)) continue;
            var j = EnemyAt(xs[i], ys[i]);
            if (j < 0 || hits >= hit.Length) continue;
            hit[hits++] = j;
            if (pat == PatternKind.Shot) break; // pierwszy cel w linii
        }
        return hits;
    }

    /// <summary>
    /// Atak trybem m w kierunku celu ei: trafia każdego żyjącego problemu na polach wzoru (promienie zatrzymują się na pierwszej
    /// ścianie, Shot trafia pierwszy cel, Line* przebija), obrażenia ×m.DmgPct/100. Zwraca liczbę trafionych.
    /// </summary>
    public int AttackWith(WeaponMode m, int ei)
    {
        if (ei < 0 || ei >= EnemiesCount || !Enemies[ei].Alive) return 0;
        Span<int> hit = stackalloc int[HitPattern.MaxCells];
        var hits = Targets(m, ei, hit);
        var dealt = 0;
        for (var i = 0; i < hits; ++i)
        {
            if (St != GameStatus.Playing || !Enemies[hit[i]].Alive) continue;
            HeroAttack(hit[i], m.DmgPct);
            ++dealt;
        }
        return dealt;
    }
}
