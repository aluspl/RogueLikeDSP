using LifeLike.Core.Data;

namespace LifeLike.Core;

// Materiały (cement, stal, drewno), naprawy pola (Załataj, Kładka), wybór ścieżki między etapami i dni etapów
// do harmonogramu domu – port 1:1 z GBA/include/core.h (v0.21.48).
public sealed partial class Game
{
    /// <summary>Kładka: zasięg (pola) z danych naprawy "bridge".</summary>
    public int BridgeReach()
    {
        foreach (var r in D.Repairs)
        {
            if (r.Effect == RepairEffect.Bridge) return r.Value;
        }
        return 0;
    }

    public void AddMaterial(int m, int n = 1)
    {
        int v = Math.Min(D.MaterialMax, Mats[m] + n), got = v - Mats[m];
        Mats[m] = (byte)v;
        if (got > 0) Push(Msg(D.Materials[m].Name).Add(" +").Add(got).As(LogKind.Loot));
    }

    public bool PuddleNear(int reach)
    {
        for (var y = Hero.Y - reach; y <= Hero.Y + reach; ++y)
        {
            for (var x = Hero.X - reach; x <= Hero.X + reach; ++x)
            {
                if (Puddle(x, y) || Mud(x, y)) return true; // Kładka działa też na błoto
            }
        }
        return false;
    }

    /// <summary>Czy naprawę k można teraz zrobić (bez skutków ubocznych – telefon i bot).</summary>
    public RepairBlock RepairBlocked(int k)
    {
        var rd = D.Repairs[k];
        if (St != GameStatus.Playing) return RepairBlock.Busy;
        if (Mats[rd.Material] < rd.Cost) return RepairBlock.Material;
        if (rd.Effect == RepairEffect.Patch)
        {
            if (NearestVisibleEnemy() < 0) return RepairBlock.NoTarget;
            if (!WallPossible(1)) return RepairBlock.NoRoom;
        }
        if (rd.Effect == RepairEffect.Bridge && (Bridges >= MaxBridges || !PuddleNear(rd.Value))) return RepairBlock.NoPuddle;
        return RepairBlock.Ok;
    }

    /// <summary>
    /// Naprawa za materiał (zużywa turę): Załataj – mur z desek przed najbliższym problemem, Kładka – kałuże wokół
    /// bez poślizgu do końca etapu (i koniec poślizgu).
    /// </summary>
    public bool PlayerRepair(int k)
    {
        var rd = D.Repairs[k];
        switch (RepairBlocked(k))
        {
            case RepairBlock.Ok: break;
            case RepairBlock.Material:
                Push(Msg(rd.Name).Add(": brak - ").Add(D.Materials[rd.Material].Name));
                return false;
            case RepairBlock.NoTarget:
                Push(Msg(rd.Name).Add(": brak problemu w polu widzenia"));
                return false;
            case RepairBlock.NoRoom:
                Push(Msg(rd.Name).Add(": nie ma gdzie"));
                return false;
            case RepairBlock.NoPuddle:
                Push(Msg(rd.Name).Add(": brak kałuż obok"));
                return false;
            default: return false;
        }
        if (ShockedTurn()) return true;
        Mats[rd.Material] = (byte)(Mats[rd.Material] - rd.Cost);
        if (rd.Effect == RepairEffect.Patch)
        {
            WallTowardEnemy(1, rd.Value);
        }
        else
        {
            BridgeX[Bridges] = Hero.X;
            BridgeY[Bridges] = Hero.Y;
            ++Bridges;
            HeroStatus[(int)StatusEffect.Slip] = 0;
        }
        Push(Msg(rd.Name).Add(": ").Add(rd.Desc).As(LogKind.Good));
        EndTurn();
        return true;
    }

    /// <summary>
    /// Oferta ścieżek na kolejny etap: dwie różne ścieżki zależne od seeda budowy i etapu (bez losowania z RNG gry).
    /// </summary>
    public int PathOffer(int k)
    {
        var n = (uint)D.Paths.Length;
        var h = (RunSeed ^ (unchecked((uint)(PatternStage() + 1 + Tier * 16)) * 2654435761u)) * 2246822519u;
        h ^= h >> 15;
        var a = (int)(h % n);
        if (k == 0) return a;
        return (a + 1 + (int)((h >> 8) % (n - 1))) % (int)n;
    }

    public void ChoosePath(int k) => NextPath = (sbyte)(k & 1);

    /// <summary>Etap zaliczony: ile tur trwał (harmonogram domu po wygranej).</summary>
    public void FinishStage()
    {
        StageDays[Stage] = (ushort)Math.Min(65535, Turns - StageStartTurn);
        var got = StageRespect();
        Respect += got;
        Push(Msg("Respekt +").Add(got).As(LogKind.Loot));
        if (Stage < D.Stages.Length - 1) RollBoons(); // premia 1 z 3 przed harmonogramem (nie po odbiorze)
    }

    /// <summary>Respekt za bieżący etap: zwykły, boss w środku aktu, boss aktu, ostatni; mnożnik jak wynik (trudność, NG+).</summary>
    public int StageRespect()
    {
        var sd = D.Stages[Stage];
        var b = Stage == D.Stages.Length - 1 ? D.RespectFinal
            : (sd.Boss < 0 ? D.RespectStage : (D.Stages[Stage + 1].Act == sd.Act ? D.RespectBoss : D.RespectActBoss));
        return Math.Max(1, b * ScorePct() / 100);
    }
}
