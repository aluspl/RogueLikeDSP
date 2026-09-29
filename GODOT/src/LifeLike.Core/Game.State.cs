namespace LifeLike.Core;

// Kopiowanie i serializacja całego stanu (odpowiednik memcpy struktury game w zapisie SRAM).
public sealed partial class Game
{
    public Game Clone()
    {
        var g = new Game(D);
        g.CopyFrom(this);
        return g;
    }

    public void CopyFrom(Game o)
    {
        using var ms = new MemoryStream();
        using (var w = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true)) o.Write(w);
        ms.Position = 0;
        using var r = new BinaryReader(ms);
        Read(r);
    }

    public byte[] ToBytes()
    {
        using var ms = new MemoryStream();
        using (var w = new BinaryWriter(ms)) Write(w);
        return ms.ToArray();
    }

    public void FromBytes(byte[] data)
    {
        using var r = new BinaryReader(new MemoryStream(data));
        Read(r);
    }

    private static void WriteActor(BinaryWriter w, in Actor a)
    {
        w.Write(a.X);
        w.Write(a.Y);
        w.Write(a.Hp);
        w.Write(a.MaxHp);
        w.Write(a.DefId);
        w.Write(a.Alive);
        w.Write(a.Awake);
        w.Write(a.Stun);
        w.Write(a.Flags);
        w.Write(a.Grow);
        w.Write(a.Timer);
        w.Write(a.Elite);
        w.Write(a.Wet);
    }

    private static Actor ReadActor(BinaryReader r) => new()
    {
        X = r.ReadSByte(),
        Y = r.ReadSByte(),
        Hp = r.ReadInt16(),
        MaxHp = r.ReadInt16(),
        DefId = r.ReadSByte(),
        Alive = r.ReadBoolean(),
        Awake = r.ReadBoolean(),
        Stun = r.ReadSByte(),
        Flags = r.ReadByte(),
        Grow = r.ReadSByte(),
        Timer = r.ReadSByte(),
        Elite = r.ReadSByte(),
        Wet = r.ReadSByte(),
    };

    public void Write(BinaryWriter w)
    {
        foreach (var t in Lv.T) w.Write((byte)t);
        foreach (var rm in Lv.Rooms)
        {
            w.Write(rm.X);
            w.Write(rm.Y);
            w.Write(rm.W);
            w.Write(rm.H);
        }
        w.Write(Lv.RoomsCount);
        w.Write(R.S);
        WriteActor(w, Hero);
        foreach (var e in Enemies) WriteActor(w, e);
        w.Write(EnemiesCount);
        foreach (var p in Pickups)
        {
            w.Write(p.X);
            w.Write(p.Y);
            w.Write((byte)p.Type);
            w.Write(p.Active);
            w.Write(p.Arg);
            w.Write(p.Trait);
        }
        w.Write(PickupsCount);
        foreach (var v in new[] { Cls, Stage, Diff, Tier, DefBonus, DmgBonus, Turns, Kills, Score, StageDamage, StageKills, StageStartTurn })
            w.Write(v);
        w.Write(KillsByType);
        w.Write(ToolsFound);
        w.Write(PowersUsed);
        w.Write(BrandFound);
        w.Write(CleanBosses);
        w.Write(BossWakeDamage);
        w.Write(StageEvent);
        w.Write(Weather);
        w.Write(HelperCalled);
        w.Write(GuardTurns);
        w.Write(AllyTurns);
        w.Write(AllyX);
        w.Write(AllyY);
        foreach (var v in new[] { Cash, ActKills, ActBonus }) w.Write(v);
        w.Write(ActCleared);
        w.Write(SlamTimer);
        w.Write(SlamX);
        w.Write(SlamY);
        w.Write(SlamCounter);
        foreach (var s in HeroStatus) w.Write(s);
        foreach (var v in new[]
                 {
                     Bonus.Hp, Bonus.Def, Bonus.Dmg, Bonus.Coffee, Bonus.Pickups, Bonus.Luck, Bonus.Craft, Bonus.Cooldown, Bonus.Sight,
                     Bonus.Thermos, Bonus.ToolPct, Bonus.XpPct, Bonus.Cash, Bonus.Crit, Bonus.Tools, Bonus.Helpers, Bonus.Investor,
                     Bonus.DmgPct, Bonus.TakenPct, Bonus.GearPct, Bonus.Dodge, Bonus.CoffeePct, Bonus.BrigadePct, Bonus.ShopPct,
                     Bonus.MatsPct, Bonus.SecondChance, Bonus.GearSlots, Bonus.Act0, Bonus.Rerolls,
                 })
            w.Write(v);
        foreach (var v in new[] { XpPct, XpBanked, RunXp, HeroLevel, Boss, StairsX, StairsY }) w.Write(v);
        w.Write((byte)St);
        foreach (var m in Log)
        {
            w.Write(m.S);
            w.Write(m.N);
            w.Write((byte)m.Kind);
            w.Write(m.Repeat);
        }
        foreach (var f in Fov) w.Write((byte)f);
        w.Write(TurnEvents);
        w.Write(HeroHit);
        foreach (var h in Hits)
        {
            w.Write(h.X);
            w.Write(h.Y);
            w.Write(h.Amount);
            w.Write(h.OnHero);
            w.Write((byte)h.Kind);
        }
        w.Write(HitsCount);
        w.Write(LastTarget);
        w.Write(AbilityCd);
        foreach (var wl in Walls)
        {
            w.Write(wl.X);
            w.Write(wl.Y);
            w.Write(wl.Turns);
        }
        w.Write(WallsCount);
        foreach (var e in Equipped) w.Write(e);
        foreach (var e in EquippedTrait) w.Write(e);
        w.Write(Thermos);
        w.Write(OfferSlot);
        w.Write(OfferRarity);
        w.Write(OfferTrait);
        w.Write(WeaponOverride);
        w.Write(LogSerial);
        w.Write(SummonCounter);
        w.Write(SummonsUsed);
        w.Write(RunSeed);
        w.Write(StagePath);
        w.Write(NextPath);
        w.Write(Mats);
        w.Write(Bridges);
        foreach (var b in BridgeX) w.Write(b);
        foreach (var b in BridgeY) w.Write(b);
        w.Write(Daily);
        w.Write(DailyDay);
        foreach (var d in StageDays) w.Write(d);
        w.Write(Respect);
        w.Write(DmgCarry);
        w.Write(TakenCarry);
        w.Write(SecondUsed);
        w.Write(BlastX);
        w.Write(BlastY);
        w.Write(BlastTimer);
        w.Write(BlastDmg);
        w.Write(ShotEvents);
        w.Write(FirstStage);
        w.Write(Docs);
        w.Write(Boons);
        foreach (var o in BoonOffer) w.Write(o);
        w.Write(BoonRerolls);
        w.Write(HitCtx);
        w.Write(ComboEvents);
        w.Write(BoonSalt);
        w.Write(PendingEvent);
        w.Write(StageChoice);
        w.Write(StageChoicePick);
        w.Write(ChoiceDone);
        w.Write(EventsSeen);
        w.Write(EventDmg);
        w.Write(EventDef);
        w.Write(WeaponLvl);
        w.Write(WeaponTrait);
        w.Write(TraitPending);
        w.Write(ToolOffer);
        w.Write(ToolOfferPickup);
        w.Write(SecretX);
        w.Write(SecretY);
        w.Write(SecretKind);
        w.Write(SecretDir);
        w.Write(SecretRx);
        w.Write(SecretRy);
        w.Write(SecretRw);
        w.Write(SecretRh);
        w.Write(SecretOpen);
        w.Write(KeyHolder);
        w.Write(Keys);
        w.Write(SecretsFound);
    }

    public void Read(BinaryReader r)
    {
        for (var i = 0; i < Lv.T.Length; i++) Lv.T[i] = (Tile)r.ReadByte();
        for (var i = 0; i < Lv.Rooms.Length; i++)
            Lv.Rooms[i] = new Room { X = r.ReadSByte(), Y = r.ReadSByte(), W = r.ReadSByte(), H = r.ReadSByte() };
        Lv.RoomsCount = r.ReadInt32();
        R.S = r.ReadUInt32();
        Hero = ReadActor(r);
        for (var i = 0; i < Enemies.Length; i++) Enemies[i] = ReadActor(r);
        EnemiesCount = r.ReadInt32();
        for (var i = 0; i < Pickups.Length; i++)
            Pickups[i] = new Pickup { X = r.ReadSByte(), Y = r.ReadSByte(), Type = (PickupType)r.ReadByte(), Active = r.ReadBoolean(), Arg = r.ReadByte(), Trait = r.ReadByte() };
        PickupsCount = r.ReadInt32();
        Cls = r.ReadInt32();
        Stage = r.ReadInt32();
        Diff = r.ReadInt32();
        Tier = r.ReadInt32();
        DefBonus = r.ReadInt32();
        DmgBonus = r.ReadInt32();
        Turns = r.ReadInt32();
        Kills = r.ReadInt32();
        Score = r.ReadInt32();
        StageDamage = r.ReadInt32();
        StageKills = r.ReadInt32();
        StageStartTurn = r.ReadInt32();
        r.Read(KillsByType, 0, KillsByType.Length);
        ToolsFound = r.ReadByte();
        PowersUsed = r.ReadUInt16();
        BrandFound = r.ReadByte();
        CleanBosses = r.ReadByte();
        BossWakeDamage = r.ReadInt32();
        StageEvent = r.ReadSByte();
        Weather = r.ReadSByte();
        HelperCalled = r.ReadSByte();
        GuardTurns = r.ReadSByte();
        AllyTurns = r.ReadSByte();
        AllyX = r.ReadSByte();
        AllyY = r.ReadSByte();
        Cash = r.ReadInt32();
        ActKills = r.ReadInt32();
        ActBonus = r.ReadInt32();
        ActCleared = r.ReadBoolean();
        SlamTimer = r.ReadInt32();
        SlamX = r.ReadSByte();
        SlamY = r.ReadSByte();
        SlamCounter = r.ReadInt32();
        for (var i = 0; i < HeroStatus.Length; i++) HeroStatus[i] = r.ReadSByte();
        Bonus = new RunMods
        {
            Hp = r.ReadInt32(), Def = r.ReadInt32(), Dmg = r.ReadInt32(), Coffee = r.ReadInt32(), Pickups = r.ReadInt32(),
            Luck = r.ReadInt32(), Craft = r.ReadInt32(), Cooldown = r.ReadInt32(), Sight = r.ReadInt32(), Thermos = r.ReadInt32(),
            ToolPct = r.ReadInt32(), XpPct = r.ReadInt32(), Cash = r.ReadInt32(), Crit = r.ReadInt32(), Tools = r.ReadInt32(),
            Helpers = r.ReadInt32(),
            Investor = r.ReadInt32(),
            DmgPct = r.ReadInt32(), TakenPct = r.ReadInt32(), GearPct = r.ReadInt32(), Dodge = r.ReadInt32(), CoffeePct = r.ReadInt32(),
            BrigadePct = r.ReadInt32(), ShopPct = r.ReadInt32(), MatsPct = r.ReadInt32(), SecondChance = r.ReadInt32(),
            GearSlots = r.ReadInt32(), Act0 = r.ReadInt32(), Rerolls = r.ReadInt32(),
        };
        XpPct = r.ReadInt32();
        XpBanked = r.ReadInt32();
        RunXp = r.ReadInt32();
        HeroLevel = r.ReadInt32();
        Boss = r.ReadInt32();
        StairsX = r.ReadInt32();
        StairsY = r.ReadInt32();
        St = (GameStatus)r.ReadByte();
        for (var i = 0; i < Log.Length; i++)
        {
            var m = new Message();
            r.Read(m.S, 0, Message.Len);
            m.N = r.ReadInt32();
            m.Kind = (LogKind)r.ReadByte();
            m.Repeat = r.ReadByte();
            Log[i] = m;
        }
        for (var i = 0; i < Fov.Length; i++) Fov[i] = (Sight)r.ReadByte();
        TurnEvents = r.ReadUInt32();
        HeroHit = r.ReadBoolean();
        for (var i = 0; i < Hits.Length; i++)
            Hits[i] = new Hit { X = r.ReadSByte(), Y = r.ReadSByte(), Amount = r.ReadInt16(), OnHero = r.ReadBoolean(), Kind = (HitKind)r.ReadByte() };
        HitsCount = r.ReadInt32();
        LastTarget = r.ReadInt32();
        AbilityCd = r.ReadInt32();
        for (var i = 0; i < Walls.Length; i++)
            Walls[i] = new TempWall { X = r.ReadSByte(), Y = r.ReadSByte(), Turns = r.ReadSByte() };
        WallsCount = r.ReadInt32();
        for (var i = 0; i < Equipped.Length; i++) Equipped[i] = r.ReadSByte();
        for (var i = 0; i < EquippedTrait.Length; i++) EquippedTrait[i] = r.ReadSByte();
        Thermos = r.ReadInt32();
        OfferSlot = r.ReadSByte();
        OfferRarity = r.ReadSByte();
        OfferTrait = r.ReadSByte();
        WeaponOverride = r.ReadInt32();
        LogSerial = r.ReadInt32();
        SummonCounter = r.ReadInt32();
        SummonsUsed = r.ReadInt32();
        RunSeed = r.ReadUInt32();
        StagePath = r.ReadSByte();
        NextPath = r.ReadSByte();
        r.Read(Mats, 0, Mats.Length);
        Bridges = r.ReadSByte();
        for (var i = 0; i < BridgeX.Length; i++) BridgeX[i] = r.ReadSByte();
        for (var i = 0; i < BridgeY.Length; i++) BridgeY[i] = r.ReadSByte();
        Daily = r.ReadBoolean();
        DailyDay = r.ReadUInt16();
        for (var i = 0; i < StageDays.Length; i++) StageDays[i] = r.ReadUInt16();
        Respect = r.ReadInt32();
        DmgCarry = r.ReadInt32();
        TakenCarry = r.ReadInt32();
        SecondUsed = r.ReadBoolean();
        BlastX = r.ReadSByte();
        BlastY = r.ReadSByte();
        BlastTimer = r.ReadSByte();
        BlastDmg = r.ReadSByte();
        ShotEvents = r.ReadUInt32();
        FirstStage = r.ReadSByte();
        Docs = r.ReadByte();
        Boons = r.ReadUInt64();
        for (var i = 0; i < BoonOffer.Length; i++) BoonOffer[i] = r.ReadSByte();
        BoonRerolls = r.ReadByte();
        HitCtx = r.ReadByte();
        ComboEvents = r.ReadByte();
        BoonSalt = r.ReadByte();
        PendingEvent = r.ReadSByte();
        StageChoice = r.ReadSByte();
        StageChoicePick = r.ReadSByte();
        ChoiceDone = r.ReadByte();
        EventsSeen = r.ReadUInt16();
        EventDmg = r.ReadSByte();
        EventDef = r.ReadSByte();
        WeaponLvl = r.ReadSByte();
        WeaponTrait = r.ReadSByte();
        TraitPending = r.ReadBoolean();
        ToolOffer = r.ReadSByte();
        ToolOfferPickup = r.ReadSByte();
        SecretX = r.ReadSByte();
        SecretY = r.ReadSByte();
        SecretKind = r.ReadSByte();
        SecretDir = r.ReadSByte();
        SecretRx = r.ReadSByte();
        SecretRy = r.ReadSByte();
        SecretRw = r.ReadSByte();
        SecretRh = r.ReadSByte();
        SecretOpen = r.ReadBoolean();
        KeyHolder = r.ReadSByte();
        Keys = r.ReadByte();
        SecretsFound = r.ReadByte();
    }
}
