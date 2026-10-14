using CoFast.Native;

namespace CoFast.Dispatch;

/// <summary>Ayrıştırılmış kısayol: önce basılacak değiştirici tuşlar, sonra asıl tuş.</summary>
internal sealed record Hotkey(IReadOnlyList<ushort> Modifiers, ushort Key);

/// <summary>"Ctrl+Shift+L", "Alt+1", "F6" gibi metinleri sanal tuş kodlarına çevirir.</summary>
internal static class HotkeyParser
{
    public static bool TryParse(string? text, out Hotkey? hotkey)
    {
        hotkey = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var parts = text.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return false;
        }

        var modifiers = new List<ushort>();
        for (var i = 0; i < parts.Length - 1; i++)
        {
            var mod = ParseModifier(parts[i]);
            if (mod is null)
            {
                return false;
            }

            if (!modifiers.Contains(mod.Value))
            {
                modifiers.Add(mod.Value);
            }
        }

        var key = ParseKey(parts[^1]);
        if (key is null)
        {
            return false;
        }

        hotkey = new Hotkey(modifiers, key.Value);
        return true;
    }

    private static ushort? ParseModifier(string token) => token.ToLowerInvariant() switch
    {
        "ctrl" or "control" => NativeMethods.VK_CONTROL,
        "shift" => NativeMethods.VK_SHIFT,
        "alt" => NativeMethods.VK_MENU,
        "win" or "windows" => NativeMethods.VK_LWIN,
        _ => null,
    };

    private static ushort? ParseKey(string token)
    {
        if (token.Length == 1)
        {
            var c = char.ToUpperInvariant(token[0]);
            if (c is >= 'A' and <= 'Z' or >= '0' and <= '9')
            {
                return c; // A-Z ve 0-9 için VK kodu ASCII değeri ile aynıdır.
            }
        }

        if (ParseModifier(token) is not null)
        {
            return null; // Yalnız değiştirici tuş geçersiz.
        }

        if (Enum.TryParse<Keys>(token, ignoreCase: true, out var k) && k != Keys.None && (int)k <= 0xFE)
        {
            return (ushort)k;
        }

        return null;
    }
}
