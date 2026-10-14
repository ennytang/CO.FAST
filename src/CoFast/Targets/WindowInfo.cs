namespace CoFast.Targets;

/// <summary>Açık bir üst düzey pencerenin özeti.</summary>
internal sealed record WindowInfo(IntPtr Handle, string Title, string ProcessName, uint ProcessId, string ClassName);
