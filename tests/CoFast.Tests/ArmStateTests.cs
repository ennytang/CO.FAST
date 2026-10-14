using CoFast.Capture;

namespace CoFast.Tests;

public class ArmStateTests
{
    private static readonly DateTime T0 = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void NotArmed_ByDefault()
    {
        var s = new ArmState(TimeSpan.FromSeconds(30));
        Assert.False(s.IsArmed(T0));
    }

    [Fact]
    public void Armed_WithinTimeout()
    {
        var s = new ArmState(TimeSpan.FromSeconds(30));
        s.Arm(T0);
        Assert.True(s.IsArmed(T0.AddSeconds(29)));
    }

    [Fact]
    public void Expires_AfterTimeout()
    {
        var s = new ArmState(TimeSpan.FromSeconds(30));
        s.Arm(T0);
        Assert.False(s.IsArmed(T0.AddSeconds(31)));
        Assert.False(s.IsArmed(T0.AddSeconds(1)));
    }

    [Fact]
    public void Disarm_Clears()
    {
        var s = new ArmState(TimeSpan.FromSeconds(30));
        s.Arm(T0);
        s.Disarm();
        Assert.False(s.IsArmed(T0));
    }

    [Fact]
    public void Rearm_ExtendsWindow()
    {
        var s = new ArmState(TimeSpan.FromSeconds(30));
        s.Arm(T0);
        s.Arm(T0.AddSeconds(20));
        Assert.True(s.IsArmed(T0.AddSeconds(45)));
    }
}
