namespace CoFast.Targets;

/// <summary>
/// Default target profiles written to the settings file on first run.
/// Process names and focus strategies can be verified with the Diagnostics output and fixed in the settings file.
/// </summary>
internal static class TargetRegistry
{
    public const string ClaudeDesktopId = "claude-desktop";

    public static List<TargetProfile> Defaults() => new()
    {
        new TargetProfile
        {
            Id = ClaudeDesktopId,
            DisplayName = "Claude Desktop",
            ProcessNames = new() { "claude" },
            FocusStrategy = FocusStrategy.None,
            Priority = 1,
        },
        new TargetProfile
        {
            Id = "antigravity-agent",
            DisplayName = "Antigravity (Agent Manager)",
            ProcessNames = new() { "antigravity" },
            TitlePattern = "(?i)agent\\s*manager",
            FocusStrategy = FocusStrategy.None,
            Priority = 2,
        },
        new TargetProfile
        {
            Id = "antigravity-ide",
            DisplayName = "Antigravity IDE",
            ProcessNames = new() { "antigravity" },
            TitleExcludePattern = "(?i)agent\\s*manager",
            FocusStrategy = FocusStrategy.None,
            Priority = 3,
        },
        new TargetProfile
        {
            Id = "browser-ai",
            DisplayName = "Browser (Gemini / ChatGPT / Claude tab)",
            ProcessNames = new() { "chrome", "msedge", "brave", "firefox", "opera", "vivaldi" },
            TitlePattern = "(?i)gemini|chatgpt|claude",
            FocusStrategy = FocusStrategy.None,
            Priority = 4,
        },
    };
}
