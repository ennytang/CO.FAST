using System.Text.Json.Serialization;

namespace CoFast.Targets;

/// <summary>How to focus the chat box after the target window comes to the front.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<FocusStrategy>))]
public enum FocusStrategy
{
    /// <summary>Do nothing; focus is already in the chat box when the window comes to the front.</summary>
    None,

    /// <summary>Send a shortcut (e.g. "Ctrl+L").</summary>
    Shortcut,

    /// <summary>Click a relative point in the window (ClickX/ClickY, 0..1).</summary>
    ClickRelative,
}

/// <summary>Profile that identifies an AI app's window and says how to paste into it.</summary>
public sealed class TargetProfile
{
    public string Id { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Process names without ".exe", case-insensitive (e.g. "claude").</summary>
    public List<string> ProcessNames { get; set; } = new();

    /// <summary>Regex the window title must match (empty = any title).</summary>
    public string? TitlePattern { get; set; }

    /// <summary>Windows whose title matches this regex are excluded.</summary>
    public string? TitleExcludePattern { get; set; }

    public FocusStrategy FocusStrategy { get; set; } = FocusStrategy.None;

    /// <summary>Shortcut for FocusStrategy = Shortcut, e.g. "Ctrl+L".</summary>
    public string? FocusShortcut { get; set; }

    /// <summary>Horizontal ratio for FocusStrategy = ClickRelative (0 = left, 1 = right).</summary>
    public double? ClickX { get; set; }

    /// <summary>Vertical ratio for FocusStrategy = ClickRelative (0 = top, 1 = bottom).</summary>
    public double? ClickY { get; set; }

    /// <summary>Priority for v1 failover (lower comes first).</summary>
    public int Priority { get; set; }
}
