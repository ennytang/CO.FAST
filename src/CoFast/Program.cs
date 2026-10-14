using CoFast.App;
using CoFast.Diagnostics;
using CoFast.Localization;
using CoFast.Settings;

namespace CoFast;

internal static class Program
{
    private const string MutexName = @"Local\CoFast.SingleInstance";

    [STAThread]
    private static void Main()
    {
        using var mutex = new Mutex(initiallyOwned: true, MutexName, out var createdNew);
        if (!createdNew)
        {
            MessageBox.Show(Strings.NotifyAlreadyRunning, Strings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var dataDir = SettingsStore.DefaultDirectory;
        Log.Initialize(dataDir);
        Log.Info($"CO.FAST başlıyor. Sürüm {typeof(Program).Assembly.GetName().Version}");

        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => Log.Error("UI thread hatası.", e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            Log.Error("İşlenmemiş hata.", e.ExceptionObject as Exception);

        ApplicationConfiguration.Initialize();

        var store = new SettingsStore(Path.Combine(dataDir, "settings.json"));
        using var app = new TrayApp(store);
        Application.Run(app);

        Log.Info("CO.FAST kapandı.");
    }
}
