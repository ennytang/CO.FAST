namespace CoFast.Targets;

/// <summary>
/// İlk çalıştırmada ayar dosyasına yazılan varsayılan hedef profilleri.
/// Süreç adları ve odak stratejileri Tanılama çıktısıyla doğrulanıp ayar dosyasından düzeltilebilir.
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
            DisplayName = "Tarayıcı (Gemini / ChatGPT / Claude sekmesi)",
            ProcessNames = new() { "chrome", "msedge", "brave", "firefox", "opera", "vivaldi" },
            TitlePattern = "(?i)gemini|chatgpt|claude",
            FocusStrategy = FocusStrategy.None,
            Priority = 4,
        },
    };
}
