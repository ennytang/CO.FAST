using CoFast.Diagnostics;
using CoFast.Native;
using CoFast.Settings;
using CoFast.Targets;

namespace CoFast.Dispatch;

internal enum DispatchStatus
{
    Pasted,
    TargetNotOpen,
    FocusFailed,
    PasteFailed,
}

internal sealed record DispatchResult(DispatchStatus Status, string TargetName);

/// <summary>
/// Pastes the clipboard image into the target window:
/// find window → bring to front → focus strategy → Ctrl+V. Enter is never pressed.
/// </summary>
internal sealed class Dispatcher
{
    private readonly Func<AppSettings> _settings;

    public Dispatcher(Func<AppSettings> settings)
    {
        _settings = settings;
    }

    /// <summary>Sends to the most recently used window that matches the profile.</summary>
    public async Task<DispatchResult> DispatchAsync(TargetProfile profile)
    {
        var window = WindowFinder.FindFirst(profile);
        if (window is null)
        {
            Log.Info($"Target not open: {profile.DisplayName}");
            return new DispatchResult(DispatchStatus.TargetNotOpen, profile.DisplayName);
        }

        return await DispatchToWindowAsync(window.Handle, profile.DisplayName, profile);
    }

    /// <summary>Sends to a specific window (no focus strategy without a profile).</summary>
    public async Task<DispatchResult> DispatchToWindowAsync(IntPtr hwnd, string name, TargetProfile? profile)
    {
        var s = _settings();

        if (!NativeMethods.IsWindow(hwnd))
        {
            return new DispatchResult(DispatchStatus.TargetNotOpen, name);
        }

        if (s.ActivationDelayMs > 0)
        {
            await Task.Delay(s.ActivationDelayMs);
        }

        if (!await ForegroundHelper.ActivateAsync(hwnd, s.FocusTimeoutMs))
        {
            Log.Warn($"Could not bring window to front: {name}");
            return new DispatchResult(DispatchStatus.FocusFailed, name);
        }

        if (profile is not null)
        {
            await ApplyFocusStrategyAsync(hwnd, profile, s);

            // Don't paste if the focus strategy brought another window to the front.
            if (!ForegroundHelper.IsForeground(hwnd))
            {
                Log.Warn($"Target not in front after focus strategy: {name}");
                return new DispatchResult(DispatchStatus.FocusFailed, name);
            }
        }

        if (s.PostFocusDelayMs > 0)
        {
            await Task.Delay(s.PostFocusDelayMs);
        }

        if (!InputSender.SendPaste())
        {
            Log.Warn($"Could not send Ctrl+V: {name}");
            return new DispatchResult(DispatchStatus.PasteFailed, name);
        }

        Log.Info($"Pasted: {name}");
        return new DispatchResult(DispatchStatus.Pasted, name);
    }

    private static async Task ApplyFocusStrategyAsync(IntPtr hwnd, TargetProfile profile, AppSettings s)
    {
        switch (profile.FocusStrategy)
        {
            case FocusStrategy.Shortcut:
                if (HotkeyParser.TryParse(profile.FocusShortcut, out var hotkey) && hotkey is not null)
                {
                    InputSender.SendHotkey(hotkey);
                    await Task.Delay(80);
                }
                else
                {
                    Log.Warn($"Invalid focus shortcut: '{profile.FocusShortcut}' ({profile.DisplayName})");
                }

                break;

            case FocusStrategy.ClickRelative:
                if (profile.ClickX is double rx && profile.ClickY is double ry)
                {
                    var hadCursor = NativeMethods.GetCursorPos(out var old);
                    var bounds = ForegroundHelper.GetVisibleBounds(hwnd);
                    var point = ClickMath.RelativePoint(bounds, rx, ry);
                    InputSender.LeftClickAt(point.X, point.Y);
                    await Task.Delay(80);
                    if (s.RestoreCursorAfterClick && hadCursor)
                    {
                        NativeMethods.SetCursorPos(old.X, old.Y);
                    }
                }
                else
                {
                    Log.Warn($"ClickX/ClickY missing for ClickRelative ({profile.DisplayName})");
                }

                break;

            case FocusStrategy.None:
            default:
                break;
        }
    }
}
