using System.Drawing;
using System.Runtime.InteropServices;
using CoFast.Native;

namespace CoFast.Dispatch;

/// <summary>
/// Windows'un "arka plandaki uygulama pencere öne getiremez" kısıtını
/// AttachThreadInput (gerekirse Alt dokunuşu) ile aşar.
/// </summary>
internal static class ForegroundHelper
{
    public static bool IsForeground(IntPtr hwnd) => NativeMethods.GetForegroundWindow() == hwnd;

    /// <summary>Pencereyi öne getirmeyi dener ve öne gelene kadar bekler.</summary>
    public static async Task<bool> ActivateAsync(IntPtr hwnd, int timeoutMs)
    {
        if (!NativeMethods.IsWindow(hwnd))
        {
            return false;
        }

        if (NativeMethods.IsIconic(hwnd))
        {
            NativeMethods.ShowWindow(hwnd, NativeMethods.SW_RESTORE);
        }

        if (IsForeground(hwnd))
        {
            return true;
        }

        TryActivate(hwnd);
        if (await WaitForegroundAsync(hwnd, Math.Min(400, timeoutMs)))
        {
            return true;
        }

        // İkinci deneme: Alt dokunuşu Windows'un odak kilidini açar.
        InputSender.TapKey(NativeMethods.VK_MENU);
        TryActivate(hwnd);
        return await WaitForegroundAsync(hwnd, Math.Max(100, timeoutMs - 400));
    }

    /// <summary>Pencerenin görünen sınırları (gölge hariç).</summary>
    public static Rectangle GetVisibleBounds(IntPtr hwnd)
    {
        var size = Marshal.SizeOf<NativeMethods.RECT>();
        if (NativeMethods.DwmGetWindowAttribute(hwnd, NativeMethods.DWMWA_EXTENDED_FRAME_BOUNDS, out NativeMethods.RECT r, size) != 0)
        {
            NativeMethods.GetWindowRect(hwnd, out r);
        }

        return Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom);
    }

    private static void TryActivate(IntPtr hwnd)
    {
        var fg = NativeMethods.GetForegroundWindow();
        var fgThread = fg == IntPtr.Zero ? 0u : NativeMethods.GetWindowThreadProcessId(fg, out _);
        var ownThread = NativeMethods.GetCurrentThreadId();
        var attached = false;

        try
        {
            if (fgThread != 0 && fgThread != ownThread)
            {
                attached = NativeMethods.AttachThreadInput(ownThread, fgThread, true);
            }

            NativeMethods.BringWindowToTop(hwnd);
            NativeMethods.SetForegroundWindow(hwnd);
        }
        finally
        {
            if (attached)
            {
                NativeMethods.AttachThreadInput(ownThread, fgThread, false);
            }
        }
    }

    private static async Task<bool> WaitForegroundAsync(IntPtr hwnd, int timeoutMs)
    {
        var deadline = Environment.TickCount64 + timeoutMs;
        while (Environment.TickCount64 < deadline)
        {
            if (IsForeground(hwnd))
            {
                return true;
            }

            await Task.Delay(25);
        }

        return IsForeground(hwnd);
    }
}
