using System.Text;
using CoFast.Targets;

namespace CoFast.Diagnostics;

/// <summary>Text report of open windows, used to tune target profiles.</summary>
internal static class WindowReport
{
    public static string Build(IReadOnlyList<WindowInfo> windows, IReadOnlyList<TargetProfile> profiles)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"CO.FAST diagnostics — {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Windows: {Environment.OSVersion.VersionString}");
        sb.AppendLine();
        sb.AppendLine("Open windows (most recently used first):");
        sb.AppendLine("process | class | title | matching profiles");

        foreach (var w in windows)
        {
            var matches = profiles
                .Where(p => TargetMatcher.Matches(p, w.ProcessName, w.Title))
                .Select(p => p.Id)
                .ToList();
            var matchText = matches.Count == 0 ? "-" : string.Join(", ", matches);
            sb.AppendLine($"{w.ProcessName} | {w.ClassName} | {w.Title} | {matchText}");
        }

        return sb.ToString();
    }
}
