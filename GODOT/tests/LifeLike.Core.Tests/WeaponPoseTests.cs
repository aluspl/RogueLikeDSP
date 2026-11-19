namespace LifeLike.Core.Tests;

/// <summary>v0.23.0: broń w ręce bohatera – kierunek z ruchu / ataku, kolumna arkusza (rzadkość * 4 + kierunek), odbicie
/// postaci, broń pod postacią przy „up”, krótki wymach (klatki obrotu).</summary>
public class WeaponPoseTests
{
    [Theory]
    [InlineData(1, 0, WeaponPose.Right)]
    [InlineData(-1, 0, WeaponPose.Left)]
    [InlineData(0, 1, WeaponPose.Down)]
    [InlineData(0, -1, WeaponPose.Up)]
    [InlineData(-1, 1, WeaponPose.Left)]   // skos: poziomo (bohater ma tylko odbicie lewo/prawo)
    [InlineData(2, -1, WeaponPose.Right)]
    public void DirectionFromDelta(int dx, int dy, int dir) => Assert.Equal(dir, WeaponPose.Dir(dx, dy, WeaponPose.Down));

    [Fact]
    public void NoDeltaKeepsPreviousDirection() => Assert.Equal(WeaponPose.Up, WeaponPose.Dir(0, 0, WeaponPose.Up));

    [Fact]
    public void LeftFlipsHeroRightUnflipsVerticalKeeps()
    {
        Assert.True(WeaponPose.Flip(WeaponPose.Left, false));
        Assert.False(WeaponPose.Flip(WeaponPose.Right, true));
        Assert.True(WeaponPose.Flip(WeaponPose.Up, true));
        Assert.False(WeaponPose.Flip(WeaponPose.Down, false));
    }

    [Fact]
    public void ColumnIsRarityTimesFourPlusLocalDirection()
    {
        Assert.Equal(0, WeaponPose.Column(0, false, WeaponPose.Down));
        Assert.Equal(2 * 4 + 1, WeaponPose.Column(2, false, WeaponPose.Up));
        // w lewo bohater jest odbity – broń rysowana w jego transformacji, więc z kolumny „right”
        Assert.Equal(1 * 4 + WeaponPose.Right, WeaponPose.Column(1, false, WeaponPose.Left));
        Assert.Equal(WeaponPose.Right, WeaponPose.LocalDir(WeaponPose.Left));
    }

    [Fact]
    public void MasterWeaponsUseOnlyMasterColumns()
    {
        Assert.Equal(3 * 4 + WeaponPose.Down, WeaponPose.Column(0, true, WeaponPose.Down));
        Assert.Equal(3 * 4 + WeaponPose.Right, WeaponPose.Column(2, true, WeaponPose.Right));
    }

    [Fact]
    public void WeaponBehindHeroOnlyWhenFacingUp()
    {
        Assert.True(WeaponPose.Behind(WeaponPose.Up));
        Assert.False(WeaponPose.Behind(WeaponPose.Down));
        Assert.False(WeaponPose.Behind(WeaponPose.Left));
        Assert.False(WeaponPose.Behind(WeaponPose.Right));
    }

    [Fact]
    public void FrameOriginPutsGripOnHand()
    {
        var (x, y) = WeaponPose.Origin(10, 4);
        Assert.Equal((WeaponPose.HandX - 10, WeaponPose.HandY - 4), (x, y));
        Assert.Equal((24, 22), (WeaponPose.HandX, WeaponPose.HandY));
    }

    [Fact]
    public void SwingIsThreeShortRotationStepsThenRest()
    {
        Assert.Equal(-60f, WeaponPose.SwingAngle(0f, false));
        Assert.Equal(30f, WeaponPose.SwingAngle(WeaponPose.SwingStep * 1.5f, false));
        Assert.Equal(10f, WeaponPose.SwingAngle(WeaponPose.SwingStep * 2.5f, false));
        Assert.Equal(0f, WeaponPose.SwingAngle(WeaponPose.SwingStep * 3f, false));
        Assert.Equal(0f, WeaponPose.SwingAngle(-1f, false));
        Assert.True(WeaponPose.SwingStep * 3 <= 0.2f); // nie spowalnia tury
    }

    [Fact]
    public void ReducedMotionHasNoSwing() => Assert.Equal(0f, WeaponPose.SwingAngle(0.01f, true));
}
