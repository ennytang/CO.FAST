using System.Drawing;

namespace CoFast.Dispatch;

/// <summary>Relative click point math (pure logic, testable).</summary>
internal static class ClickMath
{
    /// <summary>
    /// The screen point inside the window bounds that corresponds to the (rx, ry) ratios.
    /// Ratios are clamped to 0..1; the point stays inside the window.
    /// </summary>
    public static Point RelativePoint(Rectangle bounds, double rx, double ry)
    {
        rx = double.IsFinite(rx) ? Math.Clamp(rx, 0, 1) : 0.5;
        ry = double.IsFinite(ry) ? Math.Clamp(ry, 0, 1) : 0.5;

        var x = bounds.Left + (int)Math.Round((bounds.Width - 1) * rx);
        var y = bounds.Top + (int)Math.Round((bounds.Height - 1) * ry);
        return new Point(x, y);
    }
}
