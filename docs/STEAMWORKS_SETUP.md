# Steamworks Setup (M6)

How to turn on the real Steam integration. The game runs fully without any of
this (it uses `MockSteamService`); these steps swap in the real backend.

## 1. Install Steamworks.NET

Package Manager ▸ **Add package from git URL**:

```
https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net#master
```

(or import the Unity plugin release). This provides the
`com.rlabrecque.steamworks.net` assembly that `StackUp.Steam.asmdef` references.

## 2. Enable the define

**Project Settings ▸ Player ▸ Other Settings ▸ Scripting Define Symbols**, add:

```
STEAMWORKS_NET
```

This compiles `Assets/_StackUp/Steam/` (the `StackUp.Steam` assembly). Without the
define that assembly is excluded entirely, so the default project is never
affected by the Steam code.

## 3. App ID

- Put your real app id in `steam_appid.txt` next to the editor/executable for
  local testing (this file is **gitignored**). During development you may use
  `480` (Spacewar) to smoke-test the API.
- `SteamworksService` registers itself before the first scene and calls
  `SteamAPI.Init()`. If Steam isn't running it logs a warning and the locator
  falls back to the mock — the game still runs.

## 4. Configure achievements / stats / leaderboards

In the Steamworks partner site, create entries whose API names **exactly match**
the constants in `Assets/_StackUp/Scripts/Steam/SteamIds.cs`:

- **Achievements:** `FIRST_PICK`, `FIRST_ORDER`, `PERFECT_ORDER`, `COMBO_5`,
  `COMBO_10`, `STACK_MASTER`, `NO_REWORK`, `SPEED_RUNNER`, `ENDLESS_10`,
  `ENDLESS_25`, `DOCK_PERFECT`, `WAREHOUSE_PRO`.
- **Stats (INT):** `total_orders`, `total_picks`, `perfect_orders`,
  `wrong_picks`, `rework_jobs`, `best_combo`, `play_time_seconds`,
  `highest_endless_wave`.
- **Leaderboards:** `endless_high_score`, `endless_highest_wave`, and per level
  `level_1_best_time` … `level_8_best_time` (created on first submit via
  `FindOrCreateLeaderboard`).

Nothing in gameplay changes — `SteamTelemetry` already raises all of these
through `ISteamService`.

## 5. Steam Cloud (save sync)

Saves are written to `Application.persistentDataPath` and listed in
`SaveService.CloudFiles` (`progress.json`, `highscores.json`, `settings.json`).

Use **Steam Auto-Cloud** (Steamworks ▸ App ▸ Cloud): map the root save path and
the file patterns above. Auto-Cloud syncs by path with **no code** required —
this is the recommended approach. (A manual `ISteamRemoteStorage` path can be
added later if you need conflict handling.)

## 6. Verify

- Launch through Steam (or with `steam_appid.txt` present); the Console logs
  `[Steam] initialized.`
- Trigger an achievement (e.g. pick an item → `FIRST_PICK`) and confirm it
  unlocks in the overlay.
- Complete an endless run and confirm a leaderboard entry appears.
