using CoFast.Targets;

namespace CoFast.Tests;

public class TargetMatcherTests
{
    private static TargetProfile Profile(string id) => TargetRegistry.Defaults().Single(p => p.Id == id);

    [Theory]
    [InlineData("claude", "Claude", true)]
    [InlineData("Claude", "Herhangi bir başlık", true)]
    [InlineData("claude.exe", "Claude", true)]
    [InlineData("chrome", "Claude", false)]
    public void ClaudeDesktop(string process, string title, bool expected) =>
        Assert.Equal(expected, TargetMatcher.Matches(Profile("claude-desktop"), process, title));

    [Theory]
    [InlineData("Antigravity", "Agent Manager", true, false)]
    [InlineData("antigravity", "main.py - KIRAY - Antigravity", false, true)]
    public void Antigravity_SplitsByTitle(string process, string title, bool agent, bool ide)
    {
        Assert.Equal(agent, TargetMatcher.Matches(Profile("antigravity-agent"), process, title));
        Assert.Equal(ide, TargetMatcher.Matches(Profile("antigravity-ide"), process, title));
    }

    [Theory]
    [InlineData("chrome", "Google Gemini - Google Chrome", true)]
    [InlineData("msedge", "ChatGPT - Microsoft Edge", true)]
    [InlineData("firefox", "Claude — Mozilla Firefox", true)]
    [InlineData("chrome", "YouTube - Google Chrome", false)]
    [InlineData("notepad", "ChatGPT notlarım", false)]
    public void Browser(string process, string title, bool expected) =>
        Assert.Equal(expected, TargetMatcher.Matches(Profile("browser-ai"), process, title));

    [Fact]
    public void InvalidRegex_DoesNotThrow_AndDoesNotMatch()
    {
        var p = new TargetProfile { Id = "x", ProcessNames = new() { "app" }, TitlePattern = "([" };
        Assert.False(TargetMatcher.Matches(p, "app", "anything"));
    }

    [Fact]
    public void EmptyProcessList_NeverMatches()
    {
        var p = new TargetProfile { Id = "x" };
        Assert.False(TargetMatcher.Matches(p, "app", "anything"));
    }
}
