# CO.FAST

**PrtScn'e bas → alanı seç → görüntü seçtiğin AI uygulamasının chat kutusuna yapıştırılır → promptunu yaz.**

CO.FAST, Windows'ta arka planda çalışan küçük bir tray uygulamasıdır. "Alıntı al → uygulamaya geç → yapıştır" adımlarını tek harekete indirir. Ekstra bir prompt penceresi yoktur; promptu doğrudan hedef uygulamada (Claude Desktop, Antigravity, tarayıcıdaki Gemini/ChatGPT/Claude) yazarsın.

> Durum: **MVP (v0.1)**. Limit tespiti ve hedefler arası otomatik geçiş v1'de gelecek. Plan: [`docs/MASTER_PLAN.md`](docs/MASTER_PLAN.md)

## Kurulum

1. [Releases](../../releases) sayfasından veya son başarılı [Actions](../../actions) çalışmasının **CoFast-win-x64** çıktısından `CoFast.exe` dosyasını indir.
2. İstediğin bir klasöre koy ve çalıştır. Kurulum gerekmez (.NET dahil, tek dosya).
3. Tray'de turuncu **C** ikonu belirir.
4. İkona tıkla → **Hedef** listesinden bir uygulama seç.
5. İstersen **Windows ile başlat** seçeneğini aç.

> Windows SmartScreen "tanınmayan uygulama" uyarısı verebilir: **Ek bilgi → Yine de çalıştır**. Uygulama imzalı değildir; kaynak kod bu repodadır.

## Kullanım

1. Hedef uygulama (ör. Claude Desktop) açık olsun.
2. **PrtScn**'e bas, alanı seç.
3. CO.FAST hedef pencereyi öne getirir ve görüntüyü chat kutusuna yapıştırır.
4. Promptunu yaz, Enter'a bas.

Notlar:
- Yalnızca **PrtScn'den sonraki 30 saniye** içinde panoya gelen görüntü işlenir. Başka yerden kopyaladığın görüntüler hiçbir şeyi tetiklemez.
- Alıntıyı **Esc** ile iptal edersen CO.FAST da iptal olur.
- Hedef açık değilse veya öne getirilemezse bildirim çıkar; görüntü panoda kalır, elle Ctrl+V yapabilirsin.
- Aynı uygulamanın birden fazla penceresi varsa **en son kullandığın** pencereye gider.
- Belirli bir pencereyi hedeflemek için: menü → **Diğer pencere**. (Bu seçim uygulama kapanınca sıfırlanır.)

### PrtScn ayarı

Windows 11'de PrtScn varsayılan olarak alıntı aracını açar: *Ayarlar → Erişilebilirlik → Klavye → "Ekran alıntısı için Print Screen tuşunu kullan"*. Bu ayar kapalıysa PrtScn tüm ekranı kopyalar; CO.FAST bu durumda da çalışır.

## Hedef profilleri ve ayarlar

Ayar dosyası: `%AppData%\CoFast\settings.json` (menü → **Ayarlar dosyasını aç**, düzenledikten sonra **Ayarları yeniden yükle**).

Varsayılan profiller:

| Id | Uygulama | Süreç | Başlık kuralı |
|---|---|---|---|
| `claude-desktop` | Claude Desktop | `claude` | — |
| `antigravity-agent` | Antigravity (Agent Manager) | `antigravity` | başlıkta "Agent Manager" |
| `antigravity-ide` | Antigravity IDE | `antigravity` | başlıkta "Agent Manager" **yok** |
| `browser-ai` | Tarayıcı | `chrome`, `msedge`, `brave`, `firefox`, `opera`, `vivaldi` | başlıkta Gemini / ChatGPT / Claude |

> Süreç adları ve başlıklar henüz gerçek makinede doğrulanmadı. **Menü → Tanılama** açık pencerelerin süreç adlarını, başlıklarını ve hangi profile uyduklarını panoya kopyalar. Uymayan bir şey varsa `settings.json`'dan düzelt.

### Odak stratejisi

Pencere öne geldiğinde imleç chat kutusunda değilse (ör. IDE'de kod editörü odaktaysa) görüntü yanlış yere gider. Her profil için:

```jsonc
{
  "Id": "antigravity-ide",
  "FocusStrategy": "Shortcut",   // None | Shortcut | ClickRelative
  "FocusShortcut": "Ctrl+L"      // chat kutusuna odaklayan kısayol
}
```

```jsonc
{
  "Id": "claude-desktop",
  "FocusStrategy": "ClickRelative",
  "ClickX": 0.5,                 // 0 = sol, 1 = sağ
  "ClickY": 0.92                 // 0 = üst, 1 = alt (chat kutusunun konumu)
}
```

> Dikkat: Bazı uygulamalarda chat kısayolu paneli **açıp kapatır** (toggle). Panel zaten açıksa kısayol onu kapatabilir. Böyle bir durumda `ClickRelative` daha güvenlidir.

Diğer ayarlar: `ArmTimeoutSeconds` (30), `ClipboardDebounceMs` (250), `ActivationDelayMs` (150), `FocusTimeoutMs` (1500), `PostFocusDelayMs` (150), `ShowNotifications` (true).

## MVP test listesi

| # | Test | Beklenen |
|---|---|---|
| 1 | Hedef: Claude Desktop, PrtScn → alan seç | Görüntü Claude'un chat kutusunda, imleç orada |
| 2 | Aynısı Antigravity Agent Manager ve IDE için | Görüntü chat/agent kutusunda (IDE için odak stratejisi gerekebilir) |
| 3 | Tarayıcıda Gemini sekmesi aktifken | Görüntü Gemini kutusunda |
| 4 | Bir resme sağ tık → Kopyala | Hiçbir şey olmaz |
| 5 | PrtScn → Esc | Hiçbir şey olmaz |
| 6 | Hedef kapalıyken PrtScn | Bildirim; görüntü panoda |
| 7 | Hedef küçültülmüşken PrtScn | Pencere geri gelir, görüntü yapıştırılır |
| 8 | Duraklat açıkken PrtScn | Hiçbir şey olmaz |
| 9 | Windows ile başlat → yeniden başlat | Uygulama otomatik açılır |

## Güvenlik ve gizlilik

- Klavye kancası **yalnızca PrtScn ve Esc** tuşlarına bakar; başka tuş okunmaz, saklanmaz, loglanmaz. Bkz. [`KeyboardHook.cs`](src/CoFast/Capture/KeyboardHook.cs).
- Ekran görüntüleri diske yazılmaz; yalnızca panodan hedefe yapıştırılır.
- Uygulama internete bağlanmaz.
- Log: `%AppData%\CoFast\cofast.log` (yalnızca olay türleri ve hatalar).

## Geliştirme

Gereksinim: .NET 10 SDK, Windows 10/11.

```powershell
dotnet build CoFast.sln
dotnet test CoFast.sln
dotnet publish src/CoFast/CoFast.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
```

AI agent ile çalışıyorsan önce [`AGENTS.md`](AGENTS.md) dosyasını oku.

## Yol haritası

- **v1:** Limit tespiti (UI Automation + OCR), öncelik sırasına göre hedefler arası otomatik geçiş, limit kalkınca ana hedefe dönüş.
- **v2:** Ayar ekranı, Türkçe/İngilizce arayüz, release otomasyonu.
