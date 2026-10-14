using System.Text.RegularExpressions;

namespace CoFast.Targets;

/// <summary>Decides whether a window matches a target profile (pure logic, testable).</summary>
internal static class TargetMatcher
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);

    public static bool Matches(TargetProfile profile, string processName, string title)
    {
        if (profile.ProcessNames.Count == 0)
        {
            return false;
        }

        var proc = Normalize(processName);
        if (!profile.ProcessNames.Any(p => string.Equals(Normalize(p), proc, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(profile.TitlePattern) && !SafeIsMatch(title, profile.TitlePattern))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(profile.TitleExcludePattern) && SafeIsMatch(title, profile.TitleExcludePattern))
        {
            return false;
        }

        return true;
    }

    /// <summary>Treats an invalid regex as "no match" instead of an error.</summary>
    public static bool SafeIsMatch(string input, string pattern)
    {
        try
        {
            return Regex.IsMatch(input, pattern, RegexOptions.None, RegexTimeout);
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    private static string Normalize(string name)
    {
        var n = name.Trim();
        return n.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? n[..^4] : n;
    }
}
