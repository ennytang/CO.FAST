using System.Runtime.InteropServices;
using CoFast.Native;

namespace CoFast.Dispatch;

/// <summary>SendInput ile sentetik klavye ve fare olayları gönderir.</summary>
internal static class InputSender
{
    private static readonly int InputSize = Marshal.SizeOf<NativeMethods.INPUT>();

    public static bool SendPaste() =>
        SendHotkey(new Hotkey(new[] { NativeMethods.VK_CONTROL }, NativeMethods.VK_V));

    public static bool SendHotkey(Hotkey hotkey)
    {
        var inputs = new List<NativeMethods.INPUT>();
        foreach (var m in hotkey.Modifiers)
        {
            inputs.Add(Key(m, up: false));
        }

        inputs.Add(Key(hotkey.Key, up: false));
        inputs.Add(Key(hotkey.Key, up: true));

        for (var i = hotkey.Modifiers.Count - 1; i >= 0; i--)
        {
            inputs.Add(Key(hotkey.Modifiers[i], up: true));
        }

        return Send(inputs.ToArray());
    }

    /// <summary>Tek bir tuşa basıp bırakır.</summary>
    public static bool TapKey(ushort vk) => Send(new[] { Key(vk, up: false), Key(vk, up: true) });

    /// <summary>İmleci verilen ekran noktasına götürüp sol tıklar.</summary>
    public static bool LeftClickAt(int x, int y)
    {
        if (!NativeMethods.SetCursorPos(x, y))
        {
            return false;
        }

        return Send(new[] { Mouse(NativeMethods.MOUSEEVENTF_LEFTDOWN), Mouse(NativeMethods.MOUSEEVENTF_LEFTUP) });
    }

    private static bool Send(NativeMethods.INPUT[] inputs)
    {
        var sent = NativeMethods.SendInput((uint)inputs.Length, inputs, InputSize);
        return sent == inputs.Length;
    }

    private static NativeMethods.INPUT Key(ushort vk, bool up) => new()
    {
        type = NativeMethods.INPUT_KEYBOARD,
        U = new NativeMethods.InputUnion
        {
            ki = new NativeMethods.KEYBDINPUT
            {
                wVk = vk,
                dwFlags = up ? NativeMethods.KEYEVENTF_KEYUP : 0,
            },
        },
    };

    private static NativeMethods.INPUT Mouse(uint flags) => new()
    {
        type = NativeMethods.INPUT_MOUSE,
        U = new NativeMethods.InputUnion
        {
            mi = new NativeMethods.MOUSEINPUT { dwFlags = flags },
        },
    };
}
