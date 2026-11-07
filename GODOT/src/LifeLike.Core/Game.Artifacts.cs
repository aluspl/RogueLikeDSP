using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// v0.23.0: artefakty – mechanika „co N-ty cios” (WeaponDef.Proc), artefakt zamiast Markowej broni bazowej z dropu
/// (GameData.ArtifactChancePct, bez powtórki w budowie), ściany otwarte Patentem (sekretne zlecenie Łomu Ostatniej Deski).
/// </summary>
public sealed partial class Game
{
    /// <summary>Ciosy bronią w ręku od ostatniego „co N-tego” (zeruje się przy zmianie broni).</summary>
    public byte ProcCount;

    /// <summary>Artefakty, które w tej budowie już wypadły albo trafiły do ręki (bity GameData.Artifacts) – drugi raz nie wypadną.</summary>
    public ushort ArtifactsRun;

    /// <summary>Ściany magazynów otwarte Patentem w tej budowie (sekretne zlecenie).</summary>
    public byte PatentWalls;

    /// <summary>Liczy cios bronią w ręku; zwraca mechanikę, gdy to ten N-ty (null = zwykły cios albo broń bez mechaniki).</summary>
    private ProcDef CountProc()
    {
        var p = Weapon.Proc;
        if (p is null) return null;
        if (p.Every <= 1)
        {
            ProcCount = 0;
            return p;
        }
        if (++ProcCount < p.Every) return null;
        ProcCount = 0;
        return p;
    }

    /// <summary>Ściana magazynu otwarta Patentem (licznik sekretnego zlecenia).</summary>
    private void PatentWall()
    {
        if (PatentWalls < 255) ++PatentWalls;
    }

    /// <summary>Licznik od nowa – nowa broń w ręku.</summary>
    private void ResetProc() => ProcCount = 0;

    /// <summary>Oznacza artefakt (broń) jako obecny w tej budowie.</summary>
    private void MarkArtifact(int weapon)
    {
        var i = ArtifactCase.Index(D, weapon);
        if (i >= 0 && i < 16) ArtifactsRun = (ushort)(ArtifactsRun | (1 << i));
    }

    /// <summary>
    /// Artefakt, którym może okazać się Markowa broń weapon z dropu: najpierw z bazą weapon, potem z bazą = broń zawodu bohatera;
    /// tylko narzędzia z flagą Artifact, jeszcze nie w tej budowie. Zwraca pozycję w Gablocie albo -1.
    /// </summary>
    private int ArtifactFor(int weapon)
    {
        var cls = -1;
        for (var i = 0; i < D.Artifacts.Length && i < 16; ++i)
        {
            if (((ArtifactsRun >> i) & 1) != 0) continue;
            var t = ArtifactCase.Tool(D, i);
            if (t < 0 || !D.Tools[t].Artifact) continue;
            var b = D.ArtifactBase(D.Artifacts[i]);
            if (b == weapon) return i;
            if (b == CDef.Weapon && cls < 0) cls = i;
        }
        return cls;
    }

    /// <summary>Drop: Markowa broń bazowa z szansą ArtifactChancePct zamienia się w artefakt (Mistrzowska, stałe profity). False = bez zmian.</summary>
    private bool RollArtifact(ref int tool, ref int rarity, ref int perks)
    {
        if (rarity != MaxRolledWeaponRarity || D.ArtifactChancePct <= 0) return false;
        var a = ArtifactFor(D.Tools[tool].Weapon);
        if (a < 0 || R.Range(1, 100) > D.ArtifactChancePct) return false;
        tool = ArtifactCase.Tool(D, a);
        rarity = D.WeaponRarities.Length - 1;
        perks = D.MasterPerkMask(D.Artifacts[a]);
        ArtifactsRun = (ushort)(ArtifactsRun | (1 << a));
        return true;
    }

    /// <summary>Ogień z podpalonych celów przeskakuje na n sąsiadów (żywych, nietrafionych, jeszcze się niepalących) – te same tury i obrażenia.</summary>
    private void SpreadFire(int n, ReadOnlySpan<int> hit)
    {
        for (var i = 0; i < hit.Length && n > 0; ++i)
        {
            var s = Enemies[hit[i]];
            if (!s.Alive || s.Dot <= 0 || s.DotKind == 2) continue;
            for (var j = 0; j < EnemiesCount && n > 0; ++j)
            {
                ref var e = ref Enemies[j];
                if (!e.Alive || e.Dot > 0 || hit.Contains(j) || Cheb(s.X, s.Y, e.X, e.Y) > 1) continue;
                e.Dot = s.Dot;
                e.DotDmg = s.DotDmg;
                e.DotKind = 1;
                e.Awake = true;
                --n;
                Push(EnemyName(Msg(D.Statuses[(int)StatusEffect.Burn].Name).Add(": "), j).As(LogKind.Good));
            }
        }
    }
}
