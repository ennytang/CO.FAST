using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using CoFast.Native;

namespace CoFast.App;

/// <summary>Ikon dosyası gerektirmeden çalışma anında tray ikonu çizer.</summary>
internal static class TrayIconFactory
{
    public static Icon Create(bool paused)
    {
        var fill = paused ? Color.FromArgb(120, 120, 120) : Color.FromArgb(234, 88, 12);

        using var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.Clear(Color.Transparent);
            using var brush = new SolidBrush(fill);
            g.FillEllipse(brush, 1, 1, 30, 30);
            using var font = new Font("Segoe UI", 15f, FontStyle.Bold, GraphicsUnit.Pixel);
            using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("C", font, Brushes.White, new RectangleF(0, 0, 32, 32), format);
        }

        var handle = bmp.GetHicon();
        try
        {
            using var temp = Icon.FromHandle(handle);
            return (Icon)temp.Clone();
        }
        finally
        {
            NativeMethods.DestroyIcon(handle);
        }
    }
}
