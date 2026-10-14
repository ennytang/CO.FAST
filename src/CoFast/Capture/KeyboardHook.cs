using System.ComponentModel;
using System.Runtime.InteropServices;
using CoFast.Native;

namespace CoFast.Capture;

/// <summary>
/// Low-level keyboard hook. Looks only at PrtScn and Esc,
/// never swallows a key and never records a key (AGENTS.md security rules).
/// Must be created on a thread with a message loop (the UI thread).
/// </summary>
internal sealed class KeyboardHook : IDisposable
{
    // Kept in a field; otherwise the GC collects the delegate and the hook crashes.
    private readonly NativeMethods.LowLevelKeyboardProc _proc;
    private IntPtr _hook;

    public KeyboardHook()
    {
        _proc = HookCallback;
    }

    /// <summary>PrtScn was pressed (key down or key up).</summary>
    public event EventHandler? PrintScreenPressed;

    /// <summary>Esc was pressed.</summary>
    public event EventHandler? EscapePressed;

    public bool IsInstalled => _hook != IntPtr.Zero;

    public void Install()
    {
        if (IsInstalled)
        {
            return;
        }

        var module = NativeMethods.GetModuleHandle(null);
        _hook = NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, _proc, module, 0);
        if (_hook == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    public void Uninstall()
    {
        if (_hook != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_hook);
            _hook = IntPtr.Zero;
        }
    }

    public void Dispose() => Uninstall();

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            if (nCode >= 0)
            {
                var data = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
                var injected = (data.flags & NativeMethods.LLKHF_INJECTED) != 0;
                var msg = wParam.ToInt32();
                var isDown = msg is NativeMethods.WM_KEYDOWN or NativeMethods.WM_SYSKEYDOWN;
                var isUp = msg is NativeMethods.WM_KEYUP or NativeMethods.WM_SYSKEYUP;

                // Keys we injected ourselves are ignored.
                if (!injected)
                {
                    // On some systems PrtScn only produces a key-up event; both are accepted.
                    if (data.vkCode == NativeMethods.VK_SNAPSHOT && (isDown || isUp))
                    {
                        PrintScreenPressed?.Invoke(this, EventArgs.Empty);
                    }
                    else if (data.vkCode == NativeMethods.VK_ESCAPE && isDown)
                    {
                        EscapePressed?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }
        catch
        {
            // No exception may escape the hook; the key is always passed on.
        }

        return NativeMethods.CallNextHookEx(_hook, nCode, wParam, lParam);
    }
}
