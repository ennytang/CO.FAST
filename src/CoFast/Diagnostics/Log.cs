using System.Text;

namespace CoFast.Diagnostics;

/// <summary>
/// Simple file log. Clipboard contents and key data are never written (AGENTS.md).
/// </summary>
internal static class Log
{
    private const long MaxBytes = 1_000_000;
    private static readonly object Gate = new();

    public static string FilePath { get; private set; } = string.Empty;

    public static void Initialize(string directory)
    {
        Directory.CreateDirectory(directory);
        FilePath = Path.Combine(directory, "cofast.log");
        RotateIfNeeded();
    }

    public static void Info(string message) => Write("INFO", message);

    public static void Warn(string message) => Write("WARN", message);

    public static void Error(string message, Exception? ex = null) =>
        Write("ERROR", ex is null ? message : $"{message} | {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");

    private static void Write(string level, string message)
    {
        if (string.IsNullOrEmpty(FilePath))
        {
            return;
        }

        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";
        lock (Gate)
        {
            try
            {
                File.AppendAllText(FilePath, line, Encoding.UTF8);
            }
            catch (IOException)
            {
                // A failed log write must not stop the app.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    private static void RotateIfNeeded()
    {
        try
        {
            var info = new FileInfo(FilePath);
            if (info.Exists && info.Length > MaxBytes)
            {
                var old = FilePath + ".1";
                File.Copy(FilePath, old, overwrite: true);
                File.WriteAllText(FilePath, string.Empty);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
