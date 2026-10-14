using System.Drawing;
using CoFast.Dispatch;

namespace CoFast.Tests;

public class ClickMathTests
{
    private static readonly Rectangle Bounds = new(100, 200, 1001, 801);

    [Fact]
    public void Center() => Assert.Equal(new Point(600, 600), ClickMath.RelativePoint(Bounds, 0.5, 0.5));

    [Fact]
    public void Corners_StayInside()
    {
        Assert.Equal(new Point(100, 200), ClickMath.RelativePoint(Bounds, 0, 0));
        Assert.Equal(new Point(1100, 1000), ClickMath.RelativePoint(Bounds, 1, 1));
    }

    [Fact]
    public void OutOfRange_IsClamped() =>
        Assert.Equal(new Point(1100, 200), ClickMath.RelativePoint(Bounds, 5, -3));

    [Fact]
    public void NaN_FallsBackToCenter() =>
        Assert.Equal(new Point(600, 600), ClickMath.RelativePoint(Bounds, double.NaN, double.PositiveInfinity));
}
