using System.Text.Json;

namespace CoFast.Settings;

/// <summary>Reads and writes the settings file. A corrupt file is backed up and defaults are used.</summary>
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

    /// <summary>Path of the backup if the last load backed up a corrupt file; otherwise null.</summary>
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
                ?? throw new JsonException("Empty settings file.");
            settings.Normalize();
            return settings;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or ArgumentException)
        {
            LastBackupPath = $"{FilePath}.corrupt-{DateTime.Now:yyyyMMdd-HHmmss}";
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
