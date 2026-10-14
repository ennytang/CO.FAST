using CoFast.Targets;

namespace CoFast.Settings;

/// <summary>Contents of %AppData%\CoFast\settings.json.</summary>
public sealed class AppSettings
{
    public int Version { get; set; } = 1;

    /// <summary>Id of the selected target profile.</summary>
    public string? SelectedTargetId { get; set; } = TargetRegistry.ClaudeDesktopId;

    public bool Paused { get; set; }

    /// <summary>How long to wait after PrtScn for an image to reach the clipboard.</summary>
    public int ArmTimeoutSeconds { get; set; } = 30;

    /// <summary>Delay after the last of several rapid clipboard updates.</summary>
    public int ClipboardDebounceMs { get; set; } = 250;

    /// <summary>Short delay before activating the target while the Snipping Tool closes.</summary>
    public int ActivationDelayMs { get; set; } = 150;

    /// <summary>How long to wait for the target to come to the front.</summary>
    public int FocusTimeoutMs { get; set; } = 1500;

    /// <summary>Delay after the focus strategy, before Ctrl+V.</summary>
    public int PostFocusDelayMs { get; set; } = 150;

    public bool ShowNotifications { get; set; } = true;

    /// <summary>Move the mouse cursor back after a ClickRelative focus.</summary>
    public bool RestoreCursorAfterClick { get; set; } = true;

    public List<TargetProfile> Targets { get; set; } = TargetRegistry.Defaults();

    /// <summary>Clamps loaded values to safe ranges.</summary>
    public void Normalize()
    {
        ArmTimeoutSeconds = Math.Clamp(ArmTimeoutSeconds, 3, 300);
        ClipboardDebounceMs = Math.Clamp(ClipboardDebounceMs, 0, 3000);
        ActivationDelayMs = Math.Clamp(ActivationDelayMs, 0, 3000);
        FocusTimeoutMs = Math.Clamp(FocusTimeoutMs, 200, 10000);
        PostFocusDelayMs = Math.Clamp(PostFocusDelayMs, 0, 3000);
        Targets ??= new List<TargetProfile>();
        Targets.RemoveAll(t => t is null || string.IsNullOrWhiteSpace(t.Id));
        foreach (var t in Targets)
        {
            t.ProcessNames ??= new List<string>();
            t.ProcessNames = t.ProcessNames
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .Select(p => p.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? p[..^4] : p)
                .ToList();
            if (string.IsNullOrWhiteSpace(t.DisplayName))
            {
                t.DisplayName = t.Id;
            }
        }
    }
}
