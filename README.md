# CO.FAST

**Press PrtScn → select an area → the screenshot is pasted into your chosen AI app's chat box → type your prompt.**

CO.FAST is a small Windows tray app that runs in the background. It turns "take a snip → switch to the app → paste" into a single action. There is no extra prompt window: you type your prompt directly in the target app (Claude Desktop, Antigravity, or Gemini / ChatGPT / Claude in your browser).

> Status: **MVP (v0.1)**. Limit detection and automatic failover between targets arrive in v1. Plan: [`docs/MASTER_PLAN.md`](docs/MASTER_PLAN.md)

## Installation

1. Download `CoFast.exe` from the [Releases](../../releases) page, or from the **CoFast-win-x64** artifact of the latest successful [Actions](../../actions) run.
2. Put it in any folder and run it. No installation needed (.NET is bundled; single file).
3. An orange **C** icon appears in the system tray.
4. Click the icon → pick an app from the **Target** list.
5. Optionally enable **Start with Windows**.

> Windows SmartScreen may show an "unrecognized app" warning: **More info → Run anyway**. The app is not code-signed; the full source is in this repository.

## Usage

1. Make sure the target app (e.g. Claude Desktop) is open.
2. Press **PrtScn** and select an area.
3. CO.FAST brings the target window to the front and pastes the screenshot into its chat box.
4. Type your prompt and press Enter.

Notes:
- Only an image that reaches the clipboard **within 30 seconds after PrtScn** is handled. Images you copy from anywhere else never trigger anything.
- If you cancel the snip with **Esc**, CO.FAST cancels too.
- If the target is not open or cannot be brought to the front, you get a notification; the image stays on the clipboard so you can paste it manually with Ctrl+V.
- If the app has several windows, the screenshot goes to the **most recently used** one.
- To target one specific window: menu → **Other window**. (This choice resets when CO.FAST exits.)

### PrtScn setting

On Windows 11, PrtScn opens the Snipping Tool by default: *Settings → Accessibility → Keyboard → "Use the Print screen key to open screen capture"*. If this is off, PrtScn copies the whole screen; CO.FAST works in that case too.

## Target profiles and settings

Settings file: `%AppData%\CoFast\settings.json` (menu → **Open settings file**; after editing, choose **Reload settings**).

Default profiles:

| Id | App | Process | Title rule |
|---|---|---|---|
| `claude-desktop` | Claude Desktop | `claude` | — |
| `antigravity-agent` | Antigravity (Agent Manager) | `antigravity` | title contains "Agent Manager" |
| `antigravity-ide` | Antigravity IDE | `antigravity` | title does **not** contain "Agent Manager" |
| `browser-ai` | Browser | `chrome`, `msedge`, `brave`, `firefox`, `opera`, `vivaldi` | title contains Gemini / ChatGPT / Claude |

> Process names and titles have not been verified on a real machine yet. **Menu → Diagnostics** copies the process names and titles of all open windows, plus which profile each one matches, to the clipboard. If something does not match, fix it in `settings.json`.

### Focus strategy

If the cursor is not in the chat box when the window comes to the front (e.g. the code editor has focus in an IDE), the screenshot lands in the wrong place. Per profile you can set:

```jsonc
{
  "Id": "antigravity-ide",
  "FocusStrategy": "Shortcut",   // None | Shortcut | ClickRelative
  "FocusShortcut": "Ctrl+L"      // shortcut that focuses the chat box
}
```

```jsonc
{
  "Id": "claude-desktop",
  "FocusStrategy": "ClickRelative",
  "ClickX": 0.5,                 // 0 = left, 1 = right
  "ClickY": 0.92                 // 0 = top, 1 = bottom (where the chat box is)
}
```

> Caution: in some apps the chat shortcut **toggles** the panel. If the panel is already open, the shortcut may close it. In that case `ClickRelative` is safer.

Other settings: `ArmTimeoutSeconds` (30), `ClipboardDebounceMs` (250), `ActivationDelayMs` (150), `FocusTimeoutMs` (1500), `PostFocusDelayMs` (150), `ShowNotifications` (true).

## MVP test checklist

| # | Test | Expected |
|---|---|---|
| 1 | Target: Claude Desktop, PrtScn → select area | Screenshot in Claude's chat box, cursor there |
| 2 | Same for Antigravity Agent Manager and IDE | Screenshot in the chat/agent box (the IDE may need a focus strategy) |
| 3 | Gemini tab active in the browser | Screenshot in Gemini's input box |
| 4 | Right-click an image → Copy | Nothing happens |
| 5 | PrtScn → Esc | Nothing happens |
| 6 | PrtScn while the target is closed | Notification; image stays on the clipboard |
| 7 | PrtScn while the target is minimized | Window is restored and the screenshot is pasted |
| 8 | PrtScn while paused | Nothing happens |
| 9 | Start with Windows → restart | CO.FAST starts automatically |

## Security and privacy

- The keyboard hook looks **only at PrtScn and Esc**; no other key is read, stored, or logged. See [`KeyboardHook.cs`](src/CoFast/Capture/KeyboardHook.cs).
- Screenshots are never written to disk; they are only pasted from the clipboard into the target.
- The app never connects to the internet.
- Log: `%AppData%\CoFast\cofast.log` (event types and errors only).

## Development

Requirements: .NET 10 SDK, Windows 10/11.

```powershell
dotnet build CoFast.sln
dotnet test CoFast.sln
dotnet publish src/CoFast/CoFast.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
```

If you work with an AI agent, read [`AGENTS.md`](AGENTS.md) first.

## Roadmap

- **v1:** Limit detection (UI Automation + OCR), automatic failover between targets by priority, return to the primary target when its limit clears.
- **v2:** Settings window, Turkish/English UI, release automation.
