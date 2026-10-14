namespace CoFast.Localization;

/// <summary>
/// All user-facing text. Will move to TR/EN resource files in v2.
/// </summary>
internal static class Strings
{
    public const string AppName = "CO.FAST";

    // Tray menu
    public const string MenuTargetHeader = "Target";
    public const string MenuNotOpenSuffix = " (closed)";
    public const string MenuOtherWindow = "Other window";
    public const string MenuNoWindows = "(no open windows)";
    public const string MenuPause = "Pause";
    public const string MenuStartWithWindows = "Start with Windows";
    public const string MenuDiagnostics = "Diagnostics: copy open windows";
    public const string MenuOpenSettings = "Open settings file";
    public const string MenuReloadSettings = "Reload settings";
    public const string MenuOpenLog = "Open log file";
    public const string MenuExit = "Exit";

    // Status
    public const string TooltipActive = "CO.FAST — target: {0}";
    public const string TooltipPaused = "CO.FAST — paused";
    public const string NoTargetSelected = "none";

    // Notifications
    public const string NotifyStarted = "Running. Take a snip with PrtScn; target: {0}";
    public const string NotifyAlreadyRunning = "CO.FAST is already running (check the tray icon).";
    public const string NotifyNoTarget = "No target selected. Pick one from the tray menu. The image is on the clipboard.";
    public const string NotifyTargetNotOpen = "\"{0}\" is not open. The image is on the clipboard; you can paste it manually.";
    public const string NotifyFocusFailed = "Could not bring \"{0}\" to the front. The image is on the clipboard; you can paste it manually.";
    public const string NotifyPasteFailed = "Could not send the paste. The image is on the clipboard.";
    public const string NotifyDiagnosticsCopied = "List of open windows copied to the clipboard ({0} windows).";
    public const string NotifySettingsReloaded = "Settings reloaded.";
    public const string NotifySettingsInvalid = "Settings file could not be read; using defaults. Backup: {0}";
    public const string NotifyHookFailed = "Keyboard hook could not be installed. PrtScn cannot be detected. Log: {0}";
    public const string NotifyAutoStartFailed = "Could not change the start-with-Windows setting.";
    public const string NotifyUnexpectedError = "Unexpected error. Details are in the log file.";
}
