using CoFast.Targets;

namespace CoFast.Settings;

/// <summary>%AppData%\CoFast\settings.json içeriği.</summary>
public sealed class AppSettings
{
    public int Version { get; set; } = 1;

    /// <summary>Seçili hedef profilin Id'si.</summary>
    public string? SelectedTargetId { get; set; } = TargetRegistry.ClaudeDesktopId;

    public bool Paused { get; set; }

    /// <summary>PrtScn'den sonra panoya görüntü gelmesi için beklenen en uzun süre.</summary>
    public int ArmTimeoutSeconds { get; set; } = 30;

    /// <summary>Pano art arda güncellenirse son güncellemeden sonra beklenecek süre.</summary>
    public int ClipboardDebounceMs { get; set; } = 250;

    /// <summary>Alıntı aracı kapanırken hedefi öne getirmeden önceki kısa bekleme.</summary>
    public int ActivationDelayMs { get; set; } = 150;

    /// <summary>Hedefin öne gelmesi için beklenen en uzun süre.</summary>
    public int FocusTimeoutMs { get; set; } = 1500;

    /// <summary>Odak stratejisinden sonra, Ctrl+V'den önceki bekleme.</summary>
    public int PostFocusDelayMs { get; set; } = 150;

    public bool ShowNotifications { get; set; } = true;

    /// <summary>ClickRelative stratejisinden sonra fare imlecini eski yerine götür.</summary>
    public bool RestoreCursorAfterClick { get; set; } = true;

    public List<TargetProfile> Targets { get; set; } = TargetRegistry.Defaults();

    /// <summary>Okunan değerleri güvenli aralıklara çeker.</summary>
    public void Normalize()
    {
        ArmTimeoutSeconds = Math.Clamp(ArmTimeoutSeconds, 3, 300);
        ClipboardDebounceMs = Math.Clamp(ClipboardDebounceMs, 0, 3000);
        ActivationDelayMs = Math.Clamp(ActivationDelayMs, 0, 3000);
        FocusTimeoutMs = Math.Clamp(FocusTimeoutMs, 200, 10000);
        PostFocusDelayMs = Math.Clamp(PostFocusDelayMs, 0, 3000);
        Targets ??= new List<TargetProfile>();
        Targets.RemoveAll(t => t is null || string.IsNullOrWhiteSpace(t.Id));
        foreach (var t in Targets)
        {
            t.ProcessNames ??= new List<string>();
            t.ProcessNames = t.ProcessNames
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .Select(p => p.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? p[..^4] : p)
                .ToList();
            if (string.IsNullOrWhiteSpace(t.DisplayName))
            {
                t.DisplayName = t.Id;
            }
        }
    }
}
