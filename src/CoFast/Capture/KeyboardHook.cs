using System.ComponentModel;
using System.Runtime.InteropServices;
using CoFast.Native;

namespace CoFast.Capture;

/// <summary>
/// Düşük seviyeli klavye kancası. Yalnızca PrtScn ve Esc tuşlarına bakar,
/// hiçbir tuşu yutmaz ve hiçbir tuşu kaydetmez (AGENTS.md güvenlik kuralları).
/// Mesaj döngüsü olan thread'de (UI thread) oluşturulmalıdır.
/// </summary>
internal sealed class KeyboardHook : IDisposable
{
    // Delege alan olarak tutulur; aksi hâlde GC toplar ve kanca çöker.
    private readonly NativeMethods.LowLevelKeyboardProc _proc;
    private IntPtr _hook;

    public KeyboardHook()
    {
        _proc = HookCallback;
    }

    /// <summary>PrtScn'e basıldı (tuş inişi veya kalkışı).</summary>
    public event EventHandler? PrintScreenPressed;

    /// <summary>Esc'e basıldı.</summary>
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

                // Kendi gönderdiğimiz (enjekte) tuşlar yok sayılır.
                if (!injected)
                {
                    // PrtScn bazı sistemlerde yalnızca kalkış olayı üretir; ikisi de kabul edilir.
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
            // Kanca içinde istisna kaçmamalı; tuş her durumda iletilir.
        }

        return NativeMethods.CallNextHookEx(_hook, nCode, wParam, lParam);
    }
}
