using System.Text.Json.Serialization;

namespace CoFast.Targets;

/// <summary>Hedef pencere öne geldikten sonra chat kutusuna nasıl odaklanılacağı.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<FocusStrategy>))]
public enum FocusStrategy
{
    /// <summary>Hiçbir şey yapma; pencere öne gelince odak zaten chat kutusunda.</summary>
    None,

    /// <summary>Bir kısayol gönder (ör. "Ctrl+L").</summary>
    Shortcut,

    /// <summary>Pencerenin göreli bir noktasına tıkla (ClickX/ClickY, 0..1).</summary>
    ClickRelative,
}

/// <summary>Bir AI uygulamasının penceresini tanımlayan ve nasıl yapıştırılacağını söyleyen profil.</summary>
public sealed class TargetProfile
{
    public string Id { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Süreç adları, ".exe" olmadan, büyük/küçük harf duyarsız (ör. "claude").</summary>
    public List<string> ProcessNames { get; set; } = new();

    /// <summary>Pencere başlığının eşleşmesi gereken regex (boşsa her başlık).</summary>
    public string? TitlePattern { get; set; }

    /// <summary>Pencere başlığı bu regex ile eşleşirse profil dışında kalır.</summary>
    public string? TitleExcludePattern { get; set; }

    public FocusStrategy FocusStrategy { get; set; } = FocusStrategy.None;

    /// <summary>FocusStrategy = Shortcut için kısayol, ör. "Ctrl+L".</summary>
    public string? FocusShortcut { get; set; }

    /// <summary>FocusStrategy = ClickRelative için yatay oran (0 = sol, 1 = sağ).</summary>
    public double? ClickX { get; set; }

    /// <summary>FocusStrategy = ClickRelative için dikey oran (0 = üst, 1 = alt).</summary>
    public double? ClickY { get; set; }

    /// <summary>v1 hedef geçişi için öncelik (küçük olan önce).</summary>
    public int Priority { get; set; }
}
