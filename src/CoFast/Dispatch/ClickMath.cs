using System.Drawing;

namespace CoFast.Dispatch;

/// <summary>Göreli tıklama noktası hesabı (saf mantık, test edilebilir).</summary>
internal static class ClickMath
{
    /// <summary>
    /// Pencere sınırları içinde (rx, ry) oranlarına karşılık gelen ekran noktası.
    /// Oranlar 0..1 aralığına çekilir; nokta pencerenin içinde kalır.
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
