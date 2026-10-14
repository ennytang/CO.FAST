namespace CoFast.Targets;

/// <summary>Summary of an open top-level window.</summary>
internal sealed record WindowInfo(IntPtr Handle, string Title, string ProcessName, uint ProcessId, string ClassName);
