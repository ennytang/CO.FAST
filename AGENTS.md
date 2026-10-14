# AGENTS.md — CO.FAST için agent kuralları

Bu repoda çalışan her AI agent (Claude Code, Antigravity, Cursor vb.) bu kurallara uyar.

## Kaynak
- Tek doğru kaynak: `docs/MASTER_PLAN.md`. Plan dışı bir değişiklik gerekiyorsa önce planı güncelle, sonra kodu.
- Aşama dışına çıkma. Şu anki aşama: **MVP**. Limit sistemi (v1) ve ayar ekranı / TR-EN (v2) bu aşamada yazılmaz.

## Teknoloji
- C# / .NET 10, `net10.0-windows10.0.19041.0`, WinForms (sadece tray; görünür form yok).
- Win32 erişimi yalnızca `src/CoFast/Native/NativeMethods.cs` üzerinden (`DllImport`). `unsafe` kod yok.
- MVP'de üçüncü parti NuGet paketi yok. FlaUI v1'de eklenecek.
- Kullanıcıya görünen metinler `Localization/Strings.cs` içinde tutulur, kodda gömülü metin yazılmaz.

## Zorunlu güvenlik kuralları
1. Klavye kancası yalnızca `PrtScn` (VK_SNAPSHOT) ve `Esc` tuşlarına bakar. Başka hiçbir tuş saklanmaz, loglanmaz, iletilmez.
2. Kanca tuşu asla yutmaz; her zaman `CallNextHookEx` çağrılır.
3. Uygulama ağa bağlanmaz.
4. Pano içeriği (görüntü veya metin) diske yazılmaz ve loglanmaz.
5. Log dosyasına yalnızca olay türleri, süreç adları ve hata mesajları yazılır.

## UI thread kuralları
- Kanca ve pano dinleyicisi UI thread üzerinde çalışır. UI thread'i `Thread.Sleep` ile bloklama; bekleme gereken yerde `await Task.Delay` kullan. (Uzun bloklama, Windows'un düşük seviyeli kancayı sessizce kaldırmasına yol açar.)

## Çalışma şekli
- Her mantıksal adım ayrı commit. Commit mesajı: `tür(kapsam): açıklama` (ör. `feat(capture): ...`).
- Saf mantık (ayrıştırma, eşleştirme, hesaplama) Win32'den ayrı sınıflarda tutulur ve `tests/CoFast.Tests` içinde test edilir.
- Her adım sonunda: `dotnet build`, `dotnet test`, ve README'deki manuel test listesi.
