# CO.FAST — Master Plan

> The approved plan. This is the single source of truth for the project; changes are made here first.

## 1. Product summary

CO.FAST is a lightweight Windows tray app that runs in the background. It pastes a screenshot taken with PrtScn into the chat box of the open AI app selected from the tray menu (Claude Desktop, Antigravity, Antigravity IDE, or the active AI tab in a browser) and leaves the cursor there. The user types the prompt directly in the target app. In v1, if the target shows a usage-limit warning, the screenshot goes to the next target in priority order, and the primary target is restored once its limit clears. Antigravity account switching is left to Antigravity-Manager.

## 2. Decision table

| # | Topic | Decision | Rationale |
|---|---|---|---|
| 1 | Platform | Windows only | Fastest and most robust development |
| 2 | Trigger | PrtScn + Windows Snipping Tool; the app only listens | No change to existing habits |
| 3 | Prompt box | None; the prompt is typed in the target app | Shortest flow |
| 4 | Targets | Claude Desktop, Antigravity, Antigravity IDE, browser | The tools actually in use |
| 5 | Conversation | Continue the chat already open in the target | Context is preserved |
| 6 | Target selection | Tray menu | Simple, visual |
| 7 | Account switching | Stays in Antigravity-Manager | Don't reinvent the wheel |
| 8 | Limit detection | UI Automation, OCR as fallback | Most reliable approach |
| 9 | Failover | Fixed priority order + notification + automatic return to primary | Predictable |
| 10 | Storage | Screenshots are never written to disk | Privacy |
| 11 | Technology | C# / .NET 10 (LTS), WinForms tray, FlaUI (v1), Windows.Media.Ocr (v1) | Native access to Windows APIs. .NET 10 was chosen because .NET 8 support ends in November 2026 |
| 12 | Packaging | Self-contained single .exe, auto-start via the `Run` registry key | No installer |
| 13 | Scope | Personal, structured to be open-source ready | Can be shared later |
| 14 | MVP | Core flow + target selection + focus + auto-start | Fast and usable |
| 15 | Language | English code, docs, and UI | Open-source readiness |

## 3. System architecture

```
[PrtScn]──► KeyboardHook (WH_KEYBOARD_LL, never swallows the key)
                │  "armed" (30 s window, Esc cancels)
                ▼
     Windows Snipping Tool ──► Clipboard
                │
     ClipboardWatcher (WM_CLIPBOARDUPDATE)
                │  armed + image present + debounce
                ▼
        CaptureCoordinator
                │
     [v1] FailoverManager ──► LimitDetector (UIA → OCR)
                │  suitable target
                ▼
           Dispatcher
     1. Find the window (process name + title)
     2. Restore if minimized, bring to front (ForegroundHelper)
     3. Focus strategy (none / shortcut / relative click)
     4. SendInput Ctrl+V  (Enter is never pressed)
                ▼
     The user types the prompt in the target app
```

Key technical points:
- PrtScn is observed with a low-level hook, not `RegisterHotKey`; otherwise the Snipping Tool would not open.
- Thanks to the "armed" window, images copied independently of PrtScn never trigger anything.
- The foreground lock is bypassed with `AttachThreadInput`. Before pasting, the app verifies that the target really is in front; if not, nothing is pasted.
- The image stays on the clipboard; if automatic pasting fails, the user can press Ctrl+V manually.

## 4. Target and limit system (v1)

```
TargetProfile {
  Id, DisplayName,
  ProcessNames, TitlePattern?, TitleExcludePattern?,
  FocusStrategy: None | Shortcut | ClickRelative,
  FocusShortcut?, ClickX?, ClickY?,
  LimitKeywords (v1), Priority, IsPrimary (v1)
}
```

State machine:

```
Ready ──limit detected──► Limited ──10 min──► Probing
  ▲                                              │
  └──────────────── no limit ────────────────────┘
  (window closed: Closed; reopened: Ready)
```

Failover algorithm:
1. If the selected target is `Ready`, send to it.
2. Before sending, LimitDetector checks it; if a limit is detected, the target becomes `Limited`.
3. Send to the first open `Ready` target in priority order and show a notification.
4. If no target is available, nothing is pasted, a notification is shown, and the image stays on the clipboard.
5. RecoveryScheduler probes `Limited` targets every 10 minutes; when the primary target is `Ready` again, it becomes the target again.

## 5. Folder structure

```
CoFast/
├─ CoFast.sln
├─ README.md
├─ AGENTS.md / CLAUDE.md
├─ docs/MASTER_PLAN.md
├─ src/CoFast/
│  ├─ Program.cs
│  ├─ App/          (TrayApp, TrayIconFactory, AutoStart)
│  ├─ Native/       (NativeMethods)
│  ├─ Capture/      (KeyboardHook, ClipboardWatcher, ArmState, CaptureCoordinator, ClipboardHelper)
│  ├─ Targets/      (TargetProfile, TargetRegistry, TargetMatcher, WindowFinder, WindowInfo)
│  ├─ Dispatch/     (Dispatcher, ForegroundHelper, InputSender, HotkeyParser, ClickMath)
│  ├─ Limits/       [v1]
│  ├─ Settings/     (AppSettings, SettingsStore)
│  ├─ Diagnostics/  (Log, WindowReport)
│  └─ Localization/ (Strings; TR/EN in v2)
└─ tests/CoFast.Tests/
```

## 6. User interface

- Tray menu (MVP): target list, "Other window", Pause, Start with Windows, Diagnostics, Open settings file, Reload settings, Open log file, Exit.
- Tray icon state: normal / paused.
- Notifications: paste failed, target not found, [v1] failover happened.
- [v1] Priority order and target states. [v2] Settings window, language selection.

## 7. Risks

| Risk | Impact | Mitigation |
|---|---|---|
| Cursor not in the chat box (especially in the IDE) | Screenshot lands in the wrong place | Per-target focus strategy |
| Foreground lock | Window does not come to front | AttachThreadInput + verification; cancel if it fails |
| Snipping Tool writes to the clipboard late | Missed screenshot | 30 s armed window |
| Clipboard updated twice | Double paste | Debounce |
| Incomplete UIA tree in Electron apps | Limit missed | OCR fallback |
| UI text changes | Detection breaks | Keywords kept in the settings file |
| Antivirus false positive (keyboard hook) | App blocked | Open source; consider code signing in v2 |
| Terms of service | — | Account rotation is out of scope; only switching between different apps |

## 8. Phases

**MVP** — hook, clipboard listener, Dispatcher, tray target selection, focus strategies, auto-start, JSON settings.
- A PrtScn screenshot is pasted into the selected target's chat box within 1 s.
- An image copied from elsewhere triggers nothing.
- In Claude Desktop, Antigravity, and Antigravity IDE the screenshot lands in the right place.
- If the target is closed or cannot be brought to the front, a notification appears and nothing is pasted.
- Starts automatically with Windows.

**v1 — Limit system** — LimitDetector (UIA + OCR), FailoverManager, RecoveryScheduler.
- A limit warning is detected within 2 s; the next target is used; the primary target is restored when the limit clears; no false positives.

**v2 — Ready to share** — settings window, TR/EN UI, release automation.
- A new user can install and use it in 5 minutes with the README.

## 9. Open questions

1. The real process names of the targets (to be verified with the Diagnostics menu).
2. The best way to focus the chat box in each target.
3. Does Antigravity's chat/agent box accept pasted images?
