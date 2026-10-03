using LifeLike.Core.Data;

namespace LifeLike.Core;

// v0.21.50 cz. 4: podsumowanie budowy (#33) i zasady wyzwania tygodnia (#34) – port 1:1 z core.h.
public sealed partial class Game
{
    public const int RecapHitsN = 3;

    /// <summary>Ciosy w bohatera (0 = ostatni).</summary>
    public readonly RecapHit[] LastHits = [RecapHit.None, RecapHit.None, RecapHit.None];
    /// <summary>Najmocniejszy cios w bohatera w tej budowie.</summary>
    public RecapHit WorstHit = RecapHit.None;
    /// <summary>Najmocniejszy cios bohatera (obrażenia), w problem (GameData.Enemies), kryt.</summary>
    public short BestHit;
    public sbyte BestHitDef = -1;
    public bool BestHitCrit;
    /// <summary>Problem, po którym czeka wybuch (źródło ciosu).</summary>
    public sbyte BlastSrc = -1;
    /// <summary>Oś czasu: usunięte na etapie, premia po etapie, wydarzenie * 4 + odpowiedź (255 = brak), bity RecapFlag.</summary>
    public readonly byte[] StageKillLog = new byte[MaxStages];
    public readonly sbyte[] StageBoon = [-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1];
    public readonly byte[] StageEventLog = [255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255];
    public readonly byte[] StageFlags = new byte[MaxStages];
    public byte ElitesKilled;
    /// <summary>Kombinacje stanów wywołane przez bohatera.</summary>
    public ushort CombosRun;
    /// <summary>Numer tygodnia wyzwania (0 = zwykła budowa).</summary>
    public ushort WeeklyWeek;

    // ------------------------------------------------------------------ zasady wyzwania tygodnia (#34)
    public bool WeeklyHas(WeeklyRule w)
    {
        if (Bonus.Weekly < 0) return false;
        foreach (var r in D.Weekly[Bonus.Weekly].Rules)
        {
            if (r.Rule == w) return true;
        }
        return false;
    }

    public int WeeklyValue(WeeklyRule w)
    {
        if (Bonus.Weekly < 0) return 0;
        foreach (var r in D.Weekly[Bonus.Weekly].Rules)
        {
            if (r.Rule == w) return r.Value;
        }
        return 0;
    }

    // ------------------------------------------------------------------ podsumowanie budowy (#33)
    /// <summary>Cios w bohatera do podsumowania: ostatnie RecapHitsN (0 = ostatni) i najmocniejszy w budowie.</summary>
    public void LogHit(int src, int elite, RecapKind k, int amount)
    {
        for (var i = RecapHitsN - 1; i > 0; --i) LastHits[i] = LastHits[i - 1];
        LastHits[0] = new RecapHit { Src = (sbyte)src, Elite = (sbyte)elite, Kind = (byte)k, Stage = (sbyte)Stage, Amount = (short)Math.Min(32767, amount) };
        if (LastHits[0].Amount > WorstHit.Amount) WorstHit = LastHits[0];
        NotePaperHit(src);
    }

    public void NoteCombo()
    {
        if (CombosRun < 65535) ++CombosRun;
        StageFlags[Stage] |= RecapFlag.Combo;
    }

    public void ClearTimeline()
    {
        for (var s = 0; s < MaxStages; ++s)
        {
            StageKillLog[s] = 0;
            StageBoon[s] = -1;
            StageEventLog[s] = 255;
            StageFlags[s] = 0;
        }
    }

    /// <summary>Źródło ciosu z przedrostkiem elity („Zbrojony Przeciek”).</summary>
    public static Message RecapSrc(GameData d, Message m, in RecapHit h)
    {
        if (h.Src < 0) return m.Add(Loc.T("wybuch"));
        var ed = d.Enemies[h.Src];
        if (h.Elite >= 0) m.Add(d.Elites[h.Elite].Prefix[ed.Gender]).Add(" ");
        return m.Add(ed.Name);
    }

    /// <summary>Rodzaj ciosu słowami (cios bossa: nazwa uderzenia, np. „Kontrola BHP”).</summary>
    public static Message RecapKindName(GameData d, Message m, in RecapHit h)
    {
        if (h.Kind == (byte)RecapKind.Slam && h.Src >= 0 && d.Enemies[h.Src].SlamName.Length > 0) return m.Add(d.Enemies[h.Src].SlamName);
        return m.Add(d.RecapKindNames[h.Kind < d.RecapKindNames.Length ? h.Kind : 0]);
    }

    /// <summary>Cios w bohatera: „Zbrojony Przeciek: -4 (cios)”.</summary>
    public static Message RecapHitLine(GameData d, Message m, in RecapHit h)
    {
        RecapSrc(d, m, h).Add(": -").Add(h.Amount).Add(" (");
        RecapKindName(d, m, h);
        return m.Add(")");
    }

    public string RecapHitText(in RecapHit h) => RecapHitLine(D, new Message(), h).Text;

    /// <summary>„Pokonał Cię: Zbrojony Przeciek” (czasownik wg rodzaju nazwy problemu).</summary>
    public Message RecapKiller(Message m)
    {
        var h = LastHits[0];
        if (h.Src < 0 && h.Amount == 0) return m.Add(Loc.T("budowa_wstrzymana"));
        m.Add(D.RecapVerbs[h.Src >= 0 ? D.Enemies[h.Src].Gender : 0]).Add(Loc.T("cie"));
        return RecapSrc(D, m, h);
    }

    /// <summary>„3/10, Akt I” (etap budowy i akt).</summary>
    public Message RecapWhere(Message m) => m.Add(StageNumber()).Add("/").Add(StagesInRun()).Add(Loc.T("akt")).Add(ActNumeral());

    /// <summary>Etap w toku (porażka albo porzucenie) – dni i usunięte do teraz.</summary>
    public bool RecapCurrent(int s) => s == Stage && (St == GameStatus.Dead || St == GameStatus.Playing);

    public int RecapDays(int s) => D.ScheduleMinDays + (RecapCurrent(s) ? Turns - StageStartTurn : StageDays[s]) / D.ScheduleTurnsPerDay;

    public int RecapKills(int s) => RecapCurrent(s) ? StageKills : StageKillLog[s];

    private static string[] FlagNames => [Loc.T("magazyn"), Loc.T("ulepszenie_2"), Loc.T("elita_2"), Loc.T("boss_pokonany"), Loc.T("kombinacje"), Loc.T("synergia_2"), Loc.T("premia_z_sms")];

    /// <summary>Oś czasu: etap (numer, nazwa; dni i usunięte po prawej), pod nim SMS, co się działo, premia; koniec budowy.</summary>
    public List<RecapLine> RecapTimeline()
    {
        var out_ = new List<RecapLine>();
        for (var s = FirstStage; s <= Stage && s < RouteCount(); ++s)
        {
            var deadHere = RecapCurrent(s) && St == GameStatus.Dead;
            var l = new RecapLine { Ink = deadHere ? LogKind.Bad : LogKind.Info };
            l.Text.Add(s - FirstStage + 1).Add(". ").Add(SDef(s).Name);
            l.Tail.Add(RecapDays(s)).Add(" d., ").Add(RecapKills(s)).Add(Loc.T("usun"));
            out_.Add(l);
            if (StageEventLog[s] != 255)
            {
                var e = new RecapLine { Ink = LogKind.Good };
                e.Text.Add("  SMS: ").Add(D.ChoiceEvents[StageEventLog[s] / 4].Name);
                out_.Add(e);
            }
            var f = new RecapLine { Ink = LogKind.Good };
            var items = 0;
            for (var b = 0; b < FlagNames.Length; ++b)
            {
                if (((StageFlags[s] >> b) & 1) == 0) continue;
                if (items == 3)
                {
                    out_.Add(f);
                    f = new RecapLine { Ink = LogKind.Good };
                    items = 0;
                }
                f.Text.Add(items > 0 ? ", " : "  + ").Add(FlagNames[b]);
                ++items;
            }
            if (items > 0) out_.Add(f);
            if (StageBoon[s] >= 0)
            {
                var bl = new RecapLine { Ink = LogKind.Loot };
                bl.Text.Add(Loc.T("premia_2")).Add(D.Boons[StageBoon[s]].Name);
                out_.Add(bl);
            }
            if (deadHere)
            {
                var dl = new RecapLine { Ink = LogKind.Bad };
                dl.Text.Add(Loc.T("tu_stanela_budowa"));
                out_.Add(dl);
            }
        }
        return out_;
    }
}
