namespace LifeLike.Core.Tests;

/// <summary>
/// v0.23.0: noże w prawdziwej walce – Nóż majstra seniora B (dwa ciosy po 75%, OBR 4 pominięta), czyste kłucie
/// (Neutral: 100% na stal i wodę), kłucie omijające 4 OBR zamiast zwykłego 1; tekst rozpiski bez celu („Typ ciosu”).
/// Broń bez rozrzutu (min = max), kryt wyłączony – obrażenia liczone dokładnie.
/// </summary>
public class KnivesCombatTests
{
    private static GameData D => TestData.D;

    private static int Wi(string id) => Array.FindIndex(D.Weapons, w => w.Id == id);

    /// <summary>Plac z bronią w ręku (kopia danych: broń bez rozrzutu i krytu, opcjonalnie inne pola).</summary>
    private static Game Arena(string weapon, Func<WeaponDef, WeaponDef> change = null)
    {
        var wi = Wi(weapon);
        var w = D.Weapons[wi] with { MinDamage = 5, MaxDamage = 5, Crit = 0 };
        if (change is not null) w = change(w);
        var g = new Game(D.WithWeapon(wi, w));
        g.NewRun(0, 77);
        g.Lv.Fill(Tile.Wall);
        for (var y = 1; y <= 14; ++y)
            for (var x = 1; x <= 14; ++x)
                g.Lv[x, y] = Tile.Floor;
        g.EnemiesCount = 0;
        g.PickupsCount = 0;
        g.StairsX = g.StairsY = -1;
        g.Weather = 0;
        g.Hero.X = 7;
        g.Hero.Y = 7;
        g.WeaponOverride = wi;
        g.CritBuff = sbyte.MinValue; // bez krytu
        g.UpdateFov();
        return g;
    }

    /// <summary>Problem obok bohatera z OBR równą def (przez DefDown – także ujemny, by podnieść OBR), dużo HP.</summary>
    private static int Foe(Game g, Func<EnemyDef, bool> pick, int def)
    {
        var id = Array.FindIndex(D.Enemies, e => pick(e) && !e.Slam);
        var i = g.EnemiesCount;
        g.Spawn(id, 8, 7);
        g.Enemies[i].Hp = g.Enemies[i].MaxHp = 1000;
        g.Enemies[i].DefDown = 100;
        g.Enemies[i].DefDownVal = (sbyte)(D.Enemies[id].Defense + g.EnemyEliteDef(i) - def);
        g.UpdateFov();
        Assert.Equal(def, g.EnemyDefense(i));
        return i;
    }

    private static bool Steel(EnemyDef e) => e.Material >= 0 && D.Materials[e.Material].Id == "stal" && e.Elem != Element.Water;

    private static bool Water(EnemyDef e) => e.Elem == Element.Water;

    private static int Loss(Game g, int ei, WeaponMode m)
    {
        var hp = g.Enemies[ei].Hp;
        g.AttackWith(m, ei);
        return hp - g.Enemies[ei].Hp;
    }

    /// <summary>Cios trybu A (100%) w problem bez OBR – „surowe” obrażenia tej broni.</summary>
    private static int Raw(string weapon)
    {
        var g = Arena(weapon);
        var e = Foe(g, Steel, 0);
        return Loss(g, e, g.Weapon.ModeA);
    }

    [Fact]
    public void SeniorKnifeModeBHitsTwiceAt75IgnoringFourDef()
    {
        var raw = Raw("noz_majstra");
        Assert.True(raw > 4);
        var g = Arena("noz_majstra");
        var e = Foe(g, Steel, 4);
        Assert.Equal(2 * HitCalc.Apply(raw, 75), Loss(g, e, g.Weapon.ModeB));
    }

    [Fact]
    public void NeutralWeaponDealsFullDamageToSteelAndWater()
    {
        var raw = Raw("noz_majstra");
        var g = Arena("noz_majstra");
        Assert.Equal(raw, Loss(g, Foe(g, Steel, 0), g.Weapon.ModeA));
        Assert.Equal(raw, Loss(g, Foe(g, Water, 0), g.Weapon.ModeA));
        // ta sama broń bez Neutral – kłucie: stal 125%, woda 75% (tabela hitTypes)
        var n = Arena("noz_majstra", w => w with { Neutral = false });
        var pierce = (int)HitType.Pierce;
        Assert.Equal(HitCalc.Apply(raw, D.HitTypes.MaterialPct[pierce, Array.FindIndex(D.Materials, m => m.Id == "stal")]),
            Loss(n, Foe(n, Steel, 0), n.Weapon.ModeA));
        Assert.Equal(HitCalc.Apply(raw, D.HitTypes.MaterialPct[pierce, 3]), Loss(n, Foe(n, Water, 0), n.Weapon.ModeA));
    }

    [Fact]
    public void PierceFourRemovesFourDefNormalStabOne()
    {
        var raw = Raw("noz_majstra");
        var g = Arena("noz_majstra");
        Assert.Equal(raw - (8 - 4) / 2, Loss(g, Foe(g, Steel, 8), g.Weapon.ModeA));
        var plain = Arena("noz_majstra", w => w with { Pierce = 0 }); // zwykłe kłucie: HitTypes.PierceDef
        Assert.Equal(raw - (8 - D.HitTypes.PierceDef) / 2, Loss(plain, Foe(plain, Steel, 8), plain.Weapon.ModeA));
    }

    [Fact]
    public void BreakdownWithoutTargetListsHitTypes()
    {
        var g = Arena("kielnia");
        var b = g.WeaponBreakdown();
        Assert.False(b.VsEnemy);
        var m = new Message();
        Assert.False(DamageHelp.Line(D, m, b, DmgText.HitMult));
        Assert.Equal(Loc.T("typ_ciosu") + string.Join(", ", g.Weapon.Hit.Select(h => D.HitTypes.Names[(int)h])), m.Text);
    }
}
