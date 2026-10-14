using System.Text.Json;

namespace CoFast.Settings;

/// <summary>Ayar dosyasını okur ve yazar. Bozuk dosya yedeklenir, varsayılanlar kullanılır.</summary>
internal sealed class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
    };

    public SettingsStore(string filePath)
    {
        FilePath = filePath;
    }

    public string FilePath { get; }

    /// <summary>Son okumada bozuk dosya yedeklendiyse yedeğin yolu; aksi hâlde null.</summary>
    public string? LastBackupPath { get; private set; }

    public static string DefaultDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoFast");

    public AppSettings Load()
    {
        LastBackupPath = null;

        if (!File.Exists(FilePath))
        {
            var fresh = new AppSettings();
            fresh.Normalize();
            Save(fresh);
            return fresh;
        }

        try
        {
            var json = File.ReadAllText(FilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions)
                ?? throw new JsonException("Boş ayar dosyası.");
            settings.Normalize();
            return settings;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or ArgumentException)
        {
            LastBackupPath = $"{FilePath}.bozuk-{DateTime.Now:yyyyMMdd-HHmmss}";
            File.Copy(FilePath, LastBackupPath, overwrite: true);
            var fresh = new AppSettings();
            fresh.Normalize();
            Save(fresh);
            return fresh;
        }
    }

    public void Save(AppSettings settings)
    {
        var dir = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var tmp = FilePath + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(settings, JsonOptions));
        File.Move(tmp, FilePath, overwrite: true);
    }
}
