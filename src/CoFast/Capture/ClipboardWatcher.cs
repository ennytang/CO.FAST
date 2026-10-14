using CoFast.Native;

namespace CoFast.Capture;

/// <summary>
/// Görünmez, yalnızca mesaj alan bir pencere ile pano değişikliklerini dinler.
/// </summary>
internal sealed class ClipboardWatcher : NativeWindow, IDisposable
{
    private bool _listening;

    public event EventHandler? ClipboardUpdated;

    public void Start()
    {
        if (_listening)
        {
            return;
        }

        if (Handle == IntPtr.Zero)
        {
            CreateHandle(new CreateParams { Parent = NativeMethods.HWND_MESSAGE });
        }

        _listening = NativeMethods.AddClipboardFormatListener(Handle);
        if (!_listening)
        {
            throw new InvalidOperationException("AddClipboardFormatListener başarısız oldu.");
        }
    }

    public void Stop()
    {
        if (_listening && Handle != IntPtr.Zero)
        {
            NativeMethods.RemoveClipboardFormatListener(Handle);
        }

        _listening = false;
    }

    public void Dispose()
    {
        Stop();
        if (Handle != IntPtr.Zero)
        {
            DestroyHandle();
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == NativeMethods.WM_CLIPBOARDUPDATE)
        {
            ClipboardUpdated?.Invoke(this, EventArgs.Empty);
        }

        base.WndProc(ref m);
    }
}
