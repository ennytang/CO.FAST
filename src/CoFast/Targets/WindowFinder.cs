using System.Diagnostics;
using System.Text;
using CoFast.Native;

namespace CoFast.Targets;

/// <summary>Açık, kullanıcıya görünen üst düzey pencereleri Z sırasıyla (en son kullanılan önce) listeler.</summary>
internal static class WindowFinder
{
    public static List<WindowInfo> EnumerateWindows()
    {
        var result = new List<WindowInfo>();
        var processNames = new Dictionary<uint, string>();
        var ownPid = (uint)Environment.ProcessId;

        NativeMethods.EnumWindows((hwnd, _) =>
        {
            if (!IsUserWindow(hwnd))
            {
                return true;
            }

            NativeMethods.GetWindowThreadProcessId(hwnd, out var pid);
            if (pid == 0 || pid == ownPid)
            {
                return true;
            }

            var title = GetTitle(hwnd);
            if (string.IsNullOrWhiteSpace(title))
            {
                return true;
            }

            if (!processNames.TryGetValue(pid, out var procName))
            {
                procName = GetProcessName(pid);
                processNames[pid] = procName;
            }

            result.Add(new WindowInfo(hwnd, title, procName, pid, GetClassName(hwnd)));
            return true;
        }, IntPtr.Zero);

        return result;
    }

    /// <summary>Profile uyan en son kullanılmış pencere.</summary>
    public static WindowInfo? FindFirst(TargetProfile profile, IReadOnlyList<WindowInfo>? windows = null)
    {
        windows ??= EnumerateWindows();
        return windows.FirstOrDefault(w => TargetMatcher.Matches(profile, w.ProcessName, w.Title));
    }

    public static string GetTitle(IntPtr hwnd)
    {
        var length = NativeMethods.GetWindowTextLength(hwnd);
        if (length <= 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder(length + 1);
        NativeMethods.GetWindowText(hwnd, sb, sb.Capacity);
        return sb.ToString();
    }

    private static string GetClassName(IntPtr hwnd)
    {
        var sb = new StringBuilder(256);
        NativeMethods.GetClassName(hwnd, sb, sb.Capacity);
        return sb.ToString();
    }

    private static string GetProcessName(uint pid)
    {
        try
        {
            using var p = Process.GetProcessById((int)pid);
            return p.ProcessName;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            return string.Empty;
        }
    }

    private static bool IsUserWindow(IntPtr hwnd)
    {
        if (!NativeMethods.IsWindowVisible(hwnd))
        {
            return false;
        }

        if (NativeMethods.GetWindow(hwnd, NativeMethods.GW_OWNER) != IntPtr.Zero)
        {
            return false;
        }

        var exStyle = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE).ToInt64();
        if ((exStyle & NativeMethods.WS_EX_TOOLWINDOW) != 0)
        {
            return false;
        }

        // Başka sanal masaüstündeki veya gizlenmiş UWP pencereleri.
        if (NativeMethods.DwmGetWindowAttribute(hwnd, NativeMethods.DWMWA_CLOAKED, out int cloaked, sizeof(int)) == 0
            && cloaked != 0)
        {
            return false;
        }

        return true;
    }
}
