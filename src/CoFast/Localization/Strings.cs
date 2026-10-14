namespace CoFast.Localization;

/// <summary>
/// Kullanıcıya görünen tüm metinler. v2'de TR/EN kaynak dosyalarına taşınacak.
/// </summary>
internal static class Strings
{
    public const string AppName = "CO.FAST";

    // Tray menüsü
    public const string MenuTargetHeader = "Hedef";
    public const string MenuNotOpenSuffix = " (kapalı)";
    public const string MenuOtherWindow = "Diğer pencere";
    public const string MenuNoWindows = "(açık pencere yok)";
    public const string MenuPause = "Duraklat";
    public const string MenuStartWithWindows = "Windows ile başlat";
    public const string MenuDiagnostics = "Tanılama: açık pencereleri kopyala";
    public const string MenuOpenSettings = "Ayarlar dosyasını aç";
    public const string MenuReloadSettings = "Ayarları yeniden yükle";
    public const string MenuOpenLog = "Log dosyasını aç";
    public const string MenuExit = "Çıkış";

    // Durum
    public const string TooltipActive = "CO.FAST — hedef: {0}";
    public const string TooltipPaused = "CO.FAST — duraklatıldı";
    public const string NoTargetSelected = "seçilmedi";

    // Bildirimler
    public const string NotifyStarted = "Çalışıyor. PrtScn ile alıntı al; hedef: {0}";
    public const string NotifyAlreadyRunning = "CO.FAST zaten çalışıyor (tray ikonuna bak).";
    public const string NotifyNoTarget = "Hedef seçilmedi. Tray menüsünden bir hedef seç. Görüntü panoda.";
    public const string NotifyTargetNotOpen = "\"{0}\" açık değil. Görüntü panoda, elle yapıştırabilirsin.";
    public const string NotifyFocusFailed = "\"{0}\" öne getirilemedi. Görüntü panoda, elle yapıştırabilirsin.";
    public const string NotifyPasteFailed = "Yapıştırma gönderilemedi. Görüntü panoda.";
    public const string NotifyDiagnosticsCopied = "Açık pencerelerin listesi panoya kopyalandı ({0} pencere).";
    public const string NotifySettingsReloaded = "Ayarlar yeniden yüklendi.";
    public const string NotifySettingsInvalid = "Ayar dosyası okunamadı; varsayılanlar kullanılıyor. Yedek: {0}";
    public const string NotifyHookFailed = "Klavye kancası kurulamadı. PrtScn dinlenemiyor. Log: {0}";
    public const string NotifyAutoStartFailed = "Otomatik başlatma ayarı değiştirilemedi.";
    public const string NotifyUnexpectedError = "Beklenmeyen hata. Ayrıntılar log dosyasında.";
}
