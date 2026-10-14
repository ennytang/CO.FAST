using System.Text;
using CoFast.Targets;

namespace CoFast.Diagnostics;

/// <summary>Hedef profillerini ayarlamak için açık pencerelerin metin raporu.</summary>
internal static class WindowReport
{
    public static string Build(IReadOnlyList<WindowInfo> windows, IReadOnlyList<TargetProfile> profiles)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"CO.FAST tanılama — {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Windows: {Environment.OSVersion.VersionString}");
        sb.AppendLine();
        sb.AppendLine("Açık pencereler (en son kullanılan önce):");
        sb.AppendLine("süreç | sınıf | başlık | eşleşen profiller");

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
