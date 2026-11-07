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

    /// <summary>
    /// v0.23.0: problemy, które trafiłby tryb m skierowany na cel ei (0, gdy cel poza wzorem) – bez ataku; do podglądu i bota.
    /// </summary>
    public int PatternTargets(WeaponMode m, int ei, Span<int> hit)
    {
        if (ei < 0 || ei >= EnemiesCount || !Enemies[ei].Alive || Cheb(Hero.X, Hero.Y, Enemies[ei].X, Enemies[ei].Y) > ModeRange(m) || !InPattern(m, ei)) return 0;
        return Targets(m, ei, hit);
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
        // stan celów sprzed ciosu: pozycje, mokry, zapylony (do łańcucha i wybuchu pyłu)
        Span<int> px = stackalloc int[hits], py = stackalloc int[hits];
        Span<bool> wasWet = stackalloc bool[hits], wasDusty = stackalloc bool[hits];
        int n1 = 0, n2 = 0, f1 = -1, f2 = -1; // v0.23.0: ilu celów dotknął efekt 1 / 2 i pierwszy z nich (dziennik)
        for (var i = 0; i < hits; ++i)
        {
            px[i] = Enemies[hit[i]].X;
            py[i] = Enemies[hit[i]].Y;
            wasWet[i] = EnemyWet(hit[i]);
            wasDusty[i] = EnemyDusty(hit[i]);
            if (ModeEffectBefore(m.Effect, m.Value, m.Turns, hit[i], m.SteelPct) && n1++ == 0) f1 = hit[i];
            if (ModeEffectBefore(m.Effect2, m.Value2, m.Turns2, hit[i], m.SteelPct) && n2++ == 0) f2 = hit[i];
        }
        var dealt = 0;
        for (var i = 0; i < hits; ++i)
        {
            if (St != GameStatus.Playing || !Enemies[hit[i]].Alive) continue;
            for (var k = 0; k < m.Hits && St == GameStatus.Playing && Enemies[hit[i]].Alive; ++k) // v0.23.0 (#84): kilka ciosów w cel
            {
                if (m.DmgPct > 0) HeroAttack(hit[i], m.DmgPct);
            }
            ++dealt;
        }
        if (St != GameStatus.Playing) return dealt;
        var knock = Weapon.Knockback && ReferenceEquals(m, Weapon.ModeA); // Młot Zenka odpycha już w HeroAttack
        for (var i = 0; i < hits; ++i)
        {
            if (ModeEffectAfter(m.Effect, m.Value, hit[i], knock) && n1++ == 0) f1 = hit[i];
            if (ModeEffectAfter(m.Effect2, m.Value2, hit[i], knock) && n2++ == 0) f2 = hit[i];
        }
        NoteEffect(m.Effect, f1, n1);
        NoteEffect(m.Effect2, f2, n2);
        var h = hit[..hits];
        ModeEffectOnce(m.Effect, m.Value, m.Turns, m, h, px, py, wasWet, wasDusty);
        ModeEffectOnce(m.Effect2, m.Value2, m.Turns2, m, h, px, py, wasWet, wasDusty);
        return dealt;
    }
}
