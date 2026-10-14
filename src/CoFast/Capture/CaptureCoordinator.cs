using CoFast.Diagnostics;

namespace CoFast.Capture;

/// <summary>
/// PrtScn → (alıntı aracı) → pano akışını izler. Kurulu durumdayken panoya görüntü gelirse
/// <see cref="CaptureReady"/> olayını bir kez tetikler.
/// </summary>
internal sealed class CaptureCoordinator : IDisposable
{
    private readonly KeyboardHook _hook = new();
    private readonly ClipboardWatcher _clipboard = new();
    private readonly System.Windows.Forms.Timer _debounce = new();
    private readonly ArmState _arm;
    private bool _checking;

    public CaptureCoordinator(TimeSpan armTimeout, int debounceMs)
    {
        _arm = new ArmState(armTimeout);
        _debounce.Interval = Math.Max(1, debounceMs);
        _debounce.Tick += OnDebounceTick;
        _hook.PrintScreenPressed += OnPrintScreen;
        _hook.EscapePressed += OnEscape;
        _clipboard.ClipboardUpdated += OnClipboardUpdated;
    }

    /// <summary>PrtScn sonrası panoya görüntü geldi; yapıştırma için hazır.</summary>
    public event EventHandler? CaptureReady;

    public bool Paused { get; set; }

    public void Start()
    {
        _hook.Install();
        _clipboard.Start();
        Log.Info("Yakalama başlatıldı (klavye kancası + pano dinleyicisi).");
    }

    public void UpdateTiming(TimeSpan armTimeout, int debounceMs)
    {
        _arm.Timeout = armTimeout;
        _debounce.Interval = Math.Max(1, debounceMs);
    }

    public void Dispose()
    {
        _debounce.Stop();
        _debounce.Dispose();
        _hook.Dispose();
        _clipboard.Dispose();
    }

    private void OnPrintScreen(object? sender, EventArgs e)
    {
        if (Paused)
        {
            return;
        }

        // Kanca geri çağrısı hızlı dönmeli; sadece durum güncellenir.
        _arm.Arm(DateTime.UtcNow);
    }

    private void OnEscape(object? sender, EventArgs e)
    {
        if (_arm.IsArmed(DateTime.UtcNow))
        {
            _arm.Disarm();
            _debounce.Stop();
            Log.Info("Alıntı Esc ile iptal edildi.");
        }
    }

    private void OnClipboardUpdated(object? sender, EventArgs e)
    {
        if (Paused || !_arm.IsArmed(DateTime.UtcNow))
        {
            return;
        }

        // Alıntı aracı panoyu art arda güncelleyebilir; son güncellemeyi bekle.
        _debounce.Stop();
        _debounce.Start();
    }

    private async void OnDebounceTick(object? sender, EventArgs e)
    {
        _debounce.Stop();
        if (_checking || Paused || !_arm.IsArmed(DateTime.UtcNow))
        {
            return;
        }

        _checking = true;
        try
        {
            if (await ClipboardHelper.ContainsImageAsync())
            {
                _arm.Disarm();
                Log.Info("PrtScn sonrası panoya görüntü geldi.");
                CaptureReady?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            Log.Error("Pano kontrolü başarısız.", ex);
        }
        finally
        {
            _checking = false;
        }
    }
}
