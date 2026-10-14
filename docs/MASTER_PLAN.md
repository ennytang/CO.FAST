# CO.FAST — Master Plan

> Onaylanan plan. Projedeki tek doğru kaynak budur; değişiklikler önce burada yapılır.

## 1. Ürün özeti

CO.FAST, Windows'ta arka planda çalışan hafif bir tray uygulamasıdır. PrtScn ile alınan ekran alıntısını, tray menüsünden seçilmiş açık AI uygulamasının (Claude Desktop, Antigravity, Antigravity IDE veya tarayıcıdaki aktif AI sekmesi) chat kutusuna otomatik yapıştırır ve imleci orada bırakır. Kullanıcı promptunu doğrudan hedef uygulamada yazar. v1'de hedef limit uyarısı verirse alıntı öncelik sırasındaki bir sonraki hedefe gider; limit kalkınca ana hedefe dönülür. Antigravity hesap geçişi Antigravity-Manager'a bırakılmıştır.

## 2. Karar tablosu

| # | Konu | Karar | Gerekçe |
|---|---|---|---|
| 1 | Platform | Sadece Windows | En hızlı ve en sağlam geliştirme |
| 2 | Tetikleme | PrtScn + Windows alıntı aracı, uygulama sadece dinler | Alışkanlık değişmez |
| 3 | Prompt kutusu | Yok, prompt hedef uygulamada yazılır | En kısa akış |
| 4 | Hedefler | Claude Desktop, Antigravity, Antigravity IDE, tarayıcı | Kullanılan araçlar |
| 5 | Sohbet | Hedefte açık sohbete devam | Bağlam korunur |
| 6 | Hedef seçimi | Tray menüsü | Basit, görsel |
| 7 | Hesap geçişi | Antigravity-Manager'da kalır | Tekerleği yeniden icat etmemek |
| 8 | Limit tespiti | UI Automation, sonuç yoksa OCR | En güvenilir yol |
| 9 | Geçiş | Sabit öncelik sırası + bildirim + ana hedefe otomatik dönüş | Öngörülebilir |
| 10 | Saklama | Görüntü diske yazılmaz | Gizlilik |
| 11 | Teknoloji | C# / .NET 10 (LTS), WinForms tray, FlaUI (v1), Windows.Media.Ocr (v1) | Windows API'lerine doğal erişim. .NET 8 desteği Kasım 2026'da bittiği için LTS olan .NET 10 seçildi |
| 12 | Paketleme | Self-contained tek .exe, `Run` anahtarıyla otomatik başlatma | Kurulumsuz |
| 13 | Kapsam | Kişisel, açık kaynağa hazır düzen | Sonradan paylaşılabilir |
| 14 | MVP | Çekirdek akış + hedef seçimi + odaklama + otomatik başlatma | Hızlı ve kullanılabilir |

## 3. Sistem mimarisi

```
[PrtScn]──► KeyboardHook (WH_KEYBOARD_LL, tuşu yutmaz)
                │  "armed" (30 sn pencere, Esc iptal)
                ▼
     Windows alıntı aracı ──► Pano
                │
     ClipboardWatcher (WM_CLIPBOARDUPDATE)
                │  armed + görüntü var + debounce
                ▼
        CaptureCoordinator
                │
     [v1] FailoverManager ──► LimitDetector (UIA → OCR)
                │  uygun hedef
                ▼
           Dispatcher
     1. Pencereyi bul (süreç adı + başlık)
     2. Küçültülmüşse geri getir, öne al (ForegroundHelper)
     3. Odak stratejisi (yok / kısayol / göreli tıklama)
     4. SendInput Ctrl+V  (Enter'a basılmaz)
                ▼
     Kullanıcı promptu hedef uygulamada yazar
```

Kritik teknik noktalar:
- PrtScn, `RegisterHotKey` ile değil düşük seviyeli kanca ile dinlenir; aksi hâlde alıntı aracı açılmaz.
- "Armed" penceresi sayesinde PrtScn'den bağımsız kopyalanan görüntüler hiçbir şeyi tetiklemez.
- Odak kısıtı `AttachThreadInput` ile aşılır. Yapıştırmadan önce hedefin gerçekten öne geldiği doğrulanır; gelmezse yapıştırma yapılmaz.
- Görüntü panoda kalır; otomatik yapıştırma başarısız olursa elle Ctrl+V yapılabilir.

## 4. Hedef ve limit sistemi (v1)

```
TargetProfile {
  Id, DisplayName,
  ProcessNames, TitlePattern?, TitleExcludePattern?,
  FocusStrategy: None | Shortcut | ClickRelative,
  FocusShortcut?, ClickX?, ClickY?,
  LimitKeywords (v1), Priority, IsPrimary (v1)
}
```

Durum makinesi:

```
Ready ──limit tespit──► Limited ──10 dk──► Probing
  ▲                                         │
  └────────── limit yok ────────────────────┘
  (pencere kapanınca Closed; tekrar açılınca Ready)
```

Geçiş algoritması:
1. Seçili hedef `Ready` ise ona gönder.
2. Gönderimden önce LimitDetector kontrol eder; limit görülürse hedef `Limited` olur.
3. Öncelik sırasında açık ve `Ready` olan ilk hedefe gönderilir, bildirim çıkar.
4. Uygun hedef yoksa yapıştırma yapılmaz, bildirim çıkar, görüntü panoda kalır.
5. RecoveryScheduler `Limited` hedefleri 10 dakikada bir yoklar; ana hedef `Ready` olunca ona dönülür.

## 5. Klasör yapısı

```
CoFast/
├─ CoFast.sln
├─ README.md
├─ AGENTS.md / CLAUDE.md
├─ docs/MASTER_PLAN.md
├─ src/CoFast/
│  ├─ Program.cs
│  ├─ App/        (TrayApp, TrayIconFactory, Notifier, AutoStart)
│  ├─ Native/     (NativeMethods)
│  ├─ Capture/    (KeyboardHook, ClipboardWatcher, ArmState, CaptureCoordinator, ClipboardHelper)
│  ├─ Targets/    (TargetProfile, TargetRegistry, TargetMatcher, WindowFinder, WindowInfo)
│  ├─ Dispatch/   (Dispatcher, ForegroundHelper, InputSender, HotkeyParser, ClickMath)
│  ├─ Limits/     [v1]
│  ├─ Settings/   (AppSettings, SettingsStore)
│  ├─ Diagnostics/(Log, WindowReport)
│  └─ Localization/ (Strings; v2'de TR/EN)
└─ tests/CoFast.Tests/
```

## 6. Arayüz

- Tray menüsü (MVP): hedef listesi, "Diğer pencere…", Duraklat/Devam, Windows ile başlat, Tanılama, Ayarlar dosyasını aç, Çıkış.
- Tray ikonu durumu: normal / duraklatılmış.
- Bildirimler: yapıştırma başarısız, hedef bulunamadı, [v1] limit geçişi.
- [v1] Öncelik sırası ve hedef durumları. [v2] Ayar ekranı, dil seçimi.

## 7. Riskler

| Risk | Etki | Önlem |
|---|---|---|
| İmleç chat kutusunda değil (özellikle IDE) | Görüntü yanlış yere gider | Hedef bazında odak stratejisi |
| Odak çalma kısıtı | Pencere öne gelmez | AttachThreadInput + doğrulama; olmazsa iptal |
| Alıntı aracı panoya geç yazar | Kaçırılan alıntı | 30 sn armed penceresi |
| Pano çift güncelleme | Çift yapıştırma | Debounce |
| Electron uygulamalarında UIA ağacı eksik | Limit kaçırılır | OCR yedeği |
| Arayüz metinleri değişir | Tespit bozulur | Anahtar kelimeler ayar dosyasında |
| Antivirüs yanlış alarmı (klavye kancası) | Uygulama engellenir | Açık kaynak; v2'de imzalama değerlendirilir |
| Kullanım şartları | — | Hesap rotasyonu kapsam dışı; yalnızca uygulamalar arası geçiş |

## 8. Aşamalar

**MVP** — kanca, pano izleyici, Dispatcher, tray hedef seçimi, odak stratejileri, otomatik başlatma, JSON ayarlar.
- PrtScn alıntısı seçili hedefin chat kutusuna 1 sn içinde yapıştırılıyor.
- Başka yerden kopyalanan görüntü hiçbir şeyi tetiklemiyor.
- Claude Desktop, Antigravity, Antigravity IDE'de görüntü doğru alana gidiyor.
- Hedef kapalı / öne gelmiyorsa bildirim çıkıyor, yapıştırma yapılmıyor.
- Windows açılınca otomatik başlıyor.

**v1 — Limit sistemi** — LimitDetector (UIA + OCR), FailoverManager, RecoveryScheduler.
- Limit uyarısı 2 sn içinde tespit ediliyor; sıradaki hedefe gidiliyor; limit kalkınca ana hedefe dönülüyor; yanlış pozitif yok.

**v2 — Paylaşıma hazırlık** — ayar ekranı, TR/EN arayüz, release otomasyonu.
- Yeni bir kullanıcı README ile 5 dakikada kurup kullanabiliyor.

## 9. Açık sorular

1. Hedeflerin gerçek süreç adları (Tanılama menüsüyle doğrulanacak).
2. Her hedefte chat kutusuna odaklanmanın en iyi yolu.
3. Antigravity chat/agent kutusu görüntü yapıştırmayı destekliyor mu?
