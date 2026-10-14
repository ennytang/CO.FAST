using System.Diagnostics;
using CoFast.Capture;
using CoFast.Diagnostics;
using CoFast.Dispatch;
using CoFast.Localization;
using CoFast.Native;
using CoFast.Settings;
using CoFast.Targets;

namespace CoFast.App;

/// <summary>Tray ikonu, menü ve yakalama → yapıştırma akışının bağlandığı yer.</summary>
internal sealed class TrayApp : ApplicationContext
{
    private const int TooltipMaxLength = 127;

    private readonly SettingsStore _store;
    private readonly NotifyIcon _tray;
    private readonly ContextMenuStrip _menu = new();
    private readonly CaptureCoordinator _capture;
    private readonly Dispatcher _dispatcher;

    private AppSettings _settings;
    private Icon? _icon;
    private bool _busy;

    // "Diğer pencere" ile seçilen, yalnızca bu oturumda geçerli hedef.
    private IntPtr _pinnedWindow = IntPtr.Zero;
    private string _pinnedName = string.Empty;

    public TrayApp(SettingsStore store)
    {
        _store = store;
        _settings = _store.Load();

        _menu.Items.Add(Strings.AppName); // Boş menü açılmaz; Opening içinde yeniden kurulur.
        _menu.Opening += (_, _) => RebuildMenu();

        _tray = new NotifyIcon
        {
            ContextMenuStrip = _menu,
            Visible = true,
        };
        _tray.MouseClick += OnTrayClick;

        _dispatcher = new Dispatcher(() => _settings);
        _capture = new CaptureCoordinator(TimeSpan.FromSeconds(_settings.ArmTimeoutSeconds), _settings.ClipboardDebounceMs)
        {
            Paused = _settings.Paused,
        };
        _capture.CaptureReady += OnCaptureReady;

        UpdateTrayState();

        if (_store.LastBackupPath is not null)
        {
            Notify(string.Format(Strings.NotifySettingsInvalid, _store.LastBackupPath), ToolTipIcon.Warning, force: true);
        }

        try
        {
            _capture.Start();
            Notify(string.Format(Strings.NotifyStarted, CurrentTargetName()), ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            Log.Error("Yakalama başlatılamadı.", ex);
            Notify(string.Format(Strings.NotifyHookFailed, Log.FilePath), ToolTipIcon.Error, force: true);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _capture.Dispose();
            _tray.Visible = false;
            _tray.Dispose();
            _menu.Dispose();
            _icon?.Dispose();
        }

        base.Dispose(disposing);
    }

    // --- Akış ---------------------------------------------------------------

    private async void OnCaptureReady(object? sender, EventArgs e)
    {
        if (_busy)
        {
            return;
        }

        _busy = true;
        try
        {
            DispatchResult result;
            if (_pinnedWindow != IntPtr.Zero && NativeMethods.IsWindow(_pinnedWindow))
            {
                result = await _dispatcher.DispatchToWindowAsync(_pinnedWindow, _pinnedName, profile: null);
            }
            else
            {
                ClearPin();
                var profile = SelectedProfile();
                if (profile is null)
                {
                    Notify(Strings.NotifyNoTarget, ToolTipIcon.Warning);
                    return;
                }

                result = await _dispatcher.DispatchAsync(profile);
            }

            ReportResult(result);
        }
        catch (Exception ex)
        {
            Log.Error("Yapıştırma akışında hata.", ex);
            Notify(Strings.NotifyUnexpectedError, ToolTipIcon.Error);
        }
        finally
        {
            _busy = false;
        }
    }

    private void ReportResult(DispatchResult result)
    {
        switch (result.Status)
        {
            case DispatchStatus.TargetNotOpen:
                Notify(string.Format(Strings.NotifyTargetNotOpen, result.TargetName), ToolTipIcon.Warning);
                break;
            case DispatchStatus.FocusFailed:
                Notify(string.Format(Strings.NotifyFocusFailed, result.TargetName), ToolTipIcon.Warning);
                break;
            case DispatchStatus.PasteFailed:
                Notify(Strings.NotifyPasteFailed, ToolTipIcon.Warning);
                break;
            case DispatchStatus.Pasted:
            default:
                break; // Başarı sessizdir; imleç zaten hedefte.
        }
    }

    // --- Menü ---------------------------------------------------------------

    private void OnTrayClick(object? sender, MouseEventArgs e)
    {
        // Sol tık da menüyü açsın. NotifyIcon'un kendi (iç) yöntemi menünün
        // dışına tıklanınca kapanmasını sağlar; bulunamazsa düz Show kullanılır.
        if (e.Button == MouseButtons.Left)
        {
            var method = typeof(NotifyIcon).GetMethod(
                "ShowContextMenu",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (method is not null)
            {
                method.Invoke(_tray, null);
            }
            else
            {
                _menu.Show(Cursor.Position);
            }
        }
    }

    private void RebuildMenu()
    {
        _menu.SuspendLayout();
        _menu.Items.Clear();

        var windows = SafeEnumerateWindows();

        var header = new ToolStripMenuItem(Strings.MenuTargetHeader) { Enabled = false };
        _menu.Items.Add(header);

        foreach (var profile in _settings.Targets.OrderBy(t => t.Priority).ThenBy(t => t.DisplayName))
        {
            var isOpen = WindowFinder.FindFirst(profile, windows) is not null;
            var item = new ToolStripMenuItem(profile.DisplayName + (isOpen ? string.Empty : Strings.MenuNotOpenSuffix))
            {
                Checked = _pinnedWindow == IntPtr.Zero && profile.Id == _settings.SelectedTargetId,
                Tag = profile.Id,
            };
            item.Click += (_, _) => SelectProfile(profile.Id);
            _menu.Items.Add(item);
        }

        var other = new ToolStripMenuItem(Strings.MenuOtherWindow);
        if (windows.Count == 0)
        {
            other.DropDownItems.Add(new ToolStripMenuItem(Strings.MenuNoWindows) { Enabled = false });
        }

        foreach (var w in windows)
        {
            var text = $"{w.ProcessName} — {Truncate(w.Title, 60)}";
            var item = new ToolStripMenuItem(text) { Checked = w.Handle == _pinnedWindow };
            var handle = w.Handle;
            var name = $"{w.ProcessName}: {Truncate(w.Title, 40)}";
            item.Click += (_, _) => PinWindow(handle, name);
            other.DropDownItems.Add(item);
        }

        other.Checked = _pinnedWindow != IntPtr.Zero;
        _menu.Items.Add(other);
        _menu.Items.Add(new ToolStripSeparator());

        var pause = new ToolStripMenuItem(Strings.MenuPause) { Checked = _settings.Paused };
        pause.Click += (_, _) => TogglePause();
        _menu.Items.Add(pause);

        var autoStart = new ToolStripMenuItem(Strings.MenuStartWithWindows) { Checked = SafeAutoStartEnabled() };
        autoStart.Click += (_, _) => ToggleAutoStart();
        _menu.Items.Add(autoStart);
        _menu.Items.Add(new ToolStripSeparator());

        var diag = new ToolStripMenuItem(Strings.MenuDiagnostics);
        diag.Click += async (_, _) => await CopyDiagnosticsAsync();
        _menu.Items.Add(diag);

        var openSettings = new ToolStripMenuItem(Strings.MenuOpenSettings);
        openSettings.Click += (_, _) => OpenFile(_store.FilePath);
        _menu.Items.Add(openSettings);

        var reload = new ToolStripMenuItem(Strings.MenuReloadSettings);
        reload.Click += (_, _) => ReloadSettings();
        _menu.Items.Add(reload);

        var openLog = new ToolStripMenuItem(Strings.MenuOpenLog);
        openLog.Click += (_, _) => OpenFile(Log.FilePath);
        _menu.Items.Add(openLog);
        _menu.Items.Add(new ToolStripSeparator());

        var exit = new ToolStripMenuItem(Strings.MenuExit);
        exit.Click += (_, _) => ExitThread();
        _menu.Items.Add(exit);

        _menu.ResumeLayout();
    }

    private void SelectProfile(string id)
    {
        ClearPin();
        _settings.SelectedTargetId = id;
        SaveSettings();
        UpdateTrayState();
        Log.Info($"Hedef seçildi: {id}");
    }

    private void PinWindow(IntPtr handle, string name)
    {
        _pinnedWindow = handle;
        _pinnedName = name;
        UpdateTrayState();
        Log.Info("Hedef olarak belirli bir pencere seçildi.");
    }

    private void ClearPin()
    {
        if (_pinnedWindow != IntPtr.Zero)
        {
            _pinnedWindow = IntPtr.Zero;
            _pinnedName = string.Empty;
            UpdateTrayState();
        }
    }

    private void TogglePause()
    {
        _settings.Paused = !_settings.Paused;
        _capture.Paused = _settings.Paused;
        SaveSettings();
        UpdateTrayState();
    }

    private void ToggleAutoStart()
    {
        try
        {
            AutoStart.SetEnabled(!AutoStart.IsEnabled());
        }
        catch (Exception ex)
        {
            Log.Error("Otomatik başlatma değiştirilemedi.", ex);
            Notify(Strings.NotifyAutoStartFailed, ToolTipIcon.Error, force: true);
        }
    }

    private async Task CopyDiagnosticsAsync()
    {
        var windows = SafeEnumerateWindows();
        var report = WindowReport.Build(windows, _settings.Targets);
        if (await ClipboardHelper.TrySetTextAsync(report))
        {
            Notify(string.Format(Strings.NotifyDiagnosticsCopied, windows.Count), ToolTipIcon.Info, force: true);
        }
    }

    private void ReloadSettings()
    {
        _settings = _store.Load();
        _capture.Paused = _settings.Paused;
        _capture.UpdateTiming(TimeSpan.FromSeconds(_settings.ArmTimeoutSeconds), _settings.ClipboardDebounceMs);
        UpdateTrayState();
        if (_store.LastBackupPath is not null)
        {
            Notify(string.Format(Strings.NotifySettingsInvalid, _store.LastBackupPath), ToolTipIcon.Warning, force: true);
        }
        else
        {
            Notify(Strings.NotifySettingsReloaded, ToolTipIcon.Info, force: true);
        }
    }

    // --- Yardımcılar --------------------------------------------------------

    private TargetProfile? SelectedProfile() =>
        _settings.Targets.FirstOrDefault(t => t.Id == _settings.SelectedTargetId);

    private string CurrentTargetName()
    {
        if (_pinnedWindow != IntPtr.Zero)
        {
            return _pinnedName;
        }

        return SelectedProfile()?.DisplayName ?? Strings.NoTargetSelected;
    }

    private void UpdateTrayState()
    {
        var old = _icon;
        _icon = TrayIconFactory.Create(_settings.Paused);
        _tray.Icon = _icon;
        old?.Dispose();

        var text = _settings.Paused
            ? Strings.TooltipPaused
            : string.Format(Strings.TooltipActive, CurrentTargetName());
        _tray.Text = Truncate(text, TooltipMaxLength);
    }

    private void Notify(string text, ToolTipIcon icon, bool force = false)
    {
        Log.Info($"Bildirim: {text}");
        if (_settings.ShowNotifications || force)
        {
            _tray.ShowBalloonTip(3000, Strings.AppName, text, icon);
        }
    }

    private void SaveSettings()
    {
        try
        {
            _store.Save(_settings);
        }
        catch (Exception ex)
        {
            Log.Error("Ayarlar kaydedilemedi.", ex);
        }
    }

    private static List<WindowInfo> SafeEnumerateWindows()
    {
        try
        {
            return WindowFinder.EnumerateWindows();
        }
        catch (Exception ex)
        {
            Log.Error("Pencereler listelenemedi.", ex);
            return new List<WindowInfo>();
        }
    }

    private static bool SafeAutoStartEnabled()
    {
        try
        {
            return AutoStart.IsEnabled();
        }
        catch (Exception ex)
        {
            Log.Error("Otomatik başlatma durumu okunamadı.", ex);
            return false;
        }
    }

    private static void OpenFile(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception)
        {
            // .json için ilişkili uygulama yoksa Not Defteri ile aç.
            Process.Start(new ProcessStartInfo("notepad.exe", $"\"{path}\"") { UseShellExecute = true });
        }
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..(max - 1)] + "…";
}
