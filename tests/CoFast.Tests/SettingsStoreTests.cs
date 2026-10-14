using CoFast.Settings;
using CoFast.Targets;

namespace CoFast.Tests;

public sealed class SettingsStoreTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "cofast-tests-" + Guid.NewGuid().ToString("N"));

    private string FilePath => Path.Combine(_dir, "settings.json");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, recursive: true);
        }
    }

    [Fact]
    public void MissingFile_WritesDefaults()
    {
        var store = new SettingsStore(FilePath);
        var s = store.Load();

        Assert.True(File.Exists(FilePath));
        Assert.Equal(TargetRegistry.ClaudeDesktopId, s.SelectedTargetId);
        Assert.Equal(4, s.Targets.Count);
        Assert.Null(store.LastBackupPath);
    }

    [Fact]
    public void RoundTrip_PreservesValues()
    {
        var store = new SettingsStore(FilePath);
        var s = store.Load();
        s.SelectedTargetId = "antigravity-ide";
        s.Targets[0].FocusStrategy = FocusStrategy.Shortcut;
        s.Targets[0].FocusShortcut = "Ctrl+L";
        store.Save(s);

        var loaded = new SettingsStore(FilePath).Load();
        Assert.Equal("antigravity-ide", loaded.SelectedTargetId);
        Assert.Equal(FocusStrategy.Shortcut, loaded.Targets[0].FocusStrategy);
        Assert.Equal("Ctrl+L", loaded.Targets[0].FocusShortcut);
        Assert.Contains("\"Shortcut\"", File.ReadAllText(FilePath));
    }

    [Fact]
    public void CorruptFile_IsBackedUp_AndDefaultsUsed()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(FilePath, "{ bu json değil");

        var store = new SettingsStore(FilePath);
        var s = store.Load();

        Assert.NotNull(store.LastBackupPath);
        Assert.True(File.Exists(store.LastBackupPath));
        Assert.Equal(4, s.Targets.Count);
    }

    [Fact]
    public void Normalize_ClampsAndCleans()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(FilePath, """
            {
              // yorumlara izin verilir
              "ArmTimeoutSeconds": 99999,
              "Targets": [
                { "Id": "a", "ProcessNames": ["Claude.exe", " ", "x"] },
                { "Id": "" },
              ],
            }
            """);

        var s = new SettingsStore(FilePath).Load();
        Assert.Equal(300, s.ArmTimeoutSeconds);
        Assert.Single(s.Targets);
        Assert.Equal(new[] { "Claude", "x" }, s.Targets[0].ProcessNames.ToArray());
        Assert.Equal("a", s.Targets[0].DisplayName);
    }
}
