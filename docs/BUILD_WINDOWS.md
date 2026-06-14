# Windows Build Instructions

How to produce a Windows 64-bit build of **Stack Up!** for Steam. Covers
milestone **M0** issue *"Add Windows build instructions"*. See
[`CLAUDE_CODE_SPEC.md`](./CLAUDE_CODE_SPEC.md) Sections 20 & 23 for targets.

## Prerequisites

- **Unity 6 LTS** (project is on `6000.4.11f1` — see
  `Unity/StackUpPrototype/ProjectSettings/ProjectVersion.txt`).
- **Windows Build Support (IL2CPP)** module installed via Unity Hub.
  - On macOS you can produce a Windows build, but IL2CPP cross-compilation to
    Windows requires building **on Windows** (or CI). For local macOS iteration,
    use the macOS or Mono Windows target; produce the shipping IL2CPP Windows
    build on a Windows machine / CI runner.
- The Unity project lives in `Unity/StackUpPrototype/` — open **that** folder in
  Unity Hub, not the repository root.

## Player settings (target: Steam, Windows 64-bit)

In **File → Build Settings → Windows** (Player Settings):

- **Architecture:** x86_64
- **Scripting Backend:** IL2CPP (for shipping); Mono is fine for quick local tests
- **Api Compatibility Level:** .NET Standard 2.1
- **Fullscreen Mode:** Windowed or Fullscreen Window (Steam Deck friendly)
- **Default resolution:** ensure UI is readable at **1280×720** (Steam Deck)
- **Color Space:** Linear (URP)

## Build via the Editor

1. Open `Unity/StackUpPrototype/` in Unity 6.
2. `File → Build Settings…`
3. Add the scenes to **Scenes In Build** in load order
   (e.g. `Bootstrap`, `MainMenu`, `Warehouse_Level_01`, …).
4. Select **Windows** platform, set **Architecture = x86_64**, click
   **Switch Platform** if needed.
5. Click **Build** and output to `Builds/Windows/` at the repository root.
   - `Builds/` is gitignored (only `Builds/.gitkeep` is tracked) — build
     artifacts must not be committed.

## Build from the command line (CI-friendly)

```bash
# Run on a machine/agent with Unity 6 + Windows Build Support installed.
"<Unity>/Unity" \
  -quit -batchmode -nographics \
  -projectPath "Unity/StackUpPrototype" \
  -buildWindows64Player "../../Builds/Windows/StackUp.exe" \
  -logFile -
```

For repeatable builds, prefer a custom `BuildScript` invoked with
`-executeMethod BuildScript.BuildWindows` (to be added in M6 alongside the Steam
build workflow — see issue *"Create Steam build branch workflow"*).

## Steam notes (later — M6)

- `steam_appid.txt` is required next to the executable for local Steam testing
  and is **gitignored** (see root `.gitignore`).
- Verify the Steam overlay loads and the build runs under **Proton on Steam
  Deck** (an important secondary target).
- Target **60 FPS** on a mid-range Windows PC; keep transparent materials and
  realtime lights limited; avoid shader compilation hitches during gameplay.

## Verifying a build

- Launches to the Bootstrap/Main Menu scene without errors.
- Keyboard/mouse **and** controller both navigate menus.
- Readable at 1280×720.
- No errors in the player log (`%USERPROFILE%\AppData\LocalLow\<Company>\StackUp\Player.log`).
