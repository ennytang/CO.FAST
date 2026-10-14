using CoFast.Diagnostics;

namespace CoFast.Capture;

/// <summary>
/// Watches the PrtScn → (Snipping Tool) → clipboard flow. If an image reaches the clipboard while armed,
/// raises <see cref="CaptureReady"/> once.
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

    /// <summary>An image reached the clipboard after PrtScn; ready to paste.</summary>
    public event EventHandler? CaptureReady;

    public bool Paused { get; set; }

    public void Start()
    {
        _hook.Install();
        _clipboard.Start();
        Log.Info("Capture started (keyboard hook + clipboard listener).");
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

        // The hook callback must return quickly; only state is updated.
        _arm.Arm(DateTime.UtcNow);
    }

    private void OnEscape(object? sender, EventArgs e)
    {
        if (_arm.IsArmed(DateTime.UtcNow))
        {
            _arm.Disarm();
            _debounce.Stop();
            Log.Info("Snip cancelled with Esc.");
        }
    }

    private void OnClipboardUpdated(object? sender, EventArgs e)
    {
        if (Paused || !_arm.IsArmed(DateTime.UtcNow))
        {
            return;
        }

        // The Snipping Tool may update the clipboard several times; wait for the last update.
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
                Log.Info("Image reached the clipboard after PrtScn.");
                CaptureReady?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            Log.Error("Clipboard check failed.", ex);
        }
        finally
        {
            _checking = false;
        }
    }
}
