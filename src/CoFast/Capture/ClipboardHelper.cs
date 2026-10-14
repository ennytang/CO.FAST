using System.Runtime.InteropServices;

namespace CoFast.Capture;

/// <summary>
/// Accesses the clipboard with short retries while another app holds it locked.
/// Clipboard contents are never written anywhere.
/// </summary>
internal static class ClipboardHelper
{
    public static async Task<bool> ContainsImageAsync(int attempts = 5, int delayMs = 40)
    {
        for (var i = 0; i < attempts; i++)
        {
            try
            {
                return Clipboard.ContainsImage();
            }
            catch (ExternalException)
            {
                await Task.Delay(delayMs);
            }
        }

        return false;
    }

    public static async Task<bool> TrySetTextAsync(string text, int attempts = 5, int delayMs = 40)
    {
        for (var i = 0; i < attempts; i++)
        {
            try
            {
                Clipboard.SetText(text);
                return true;
            }
            catch (ExternalException)
            {
                await Task.Delay(delayMs);
            }
        }

        return false;
    }
}
