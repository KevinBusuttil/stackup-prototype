# Steam Handoff — Unity

Status: **placeholder** — to be expanded during milestone **M4 (Steam Architecture)** and **M6 (Release Preparation)**.

This document describes how the Unity prototype is wired so that Steam can be
integrated later without rewriting gameplay systems. See
[`CLAUDE_CODE_SPEC.md`](./CLAUDE_CODE_SPEC.md) Section 20 for the authoritative
requirements.

## Goal

The early prototype must run **without Steam**. All Steam-facing calls go
through a service abstraction (`ISteamService`) with a `MockSteamService`
implementation, so the real Steamworks integration can be dropped in later.

## Architecture

- `ISteamService` — interface for init, achievements, stats, leaderboards,
  cloud save (lives under `Assets/_StackUp/Scripts/Steam/`).
- `MockSteamService` — no-op / local implementation used during development.
- `SteamService` (later) — real Steamworks.NET-backed implementation.

Gameplay systems never reference Steamworks directly; they only call
`ISteamService`.

## Features to integrate later

- Steam initialization (`SteamAPI.Init`, app id, `steam_appid.txt` for dev)
- Achievements (see spec Section 20.2)
- Stats (see spec Section 20.3)
- Cloud saves (save files in spec Section 21 must be cloud-compatible)
- Leaderboards (see spec Section 20.4)
- Steam overlay compatibility

## Build target

- Windows 64-bit first.
- Steam Deck / Proton compatibility is an important secondary target
  (test UI readability at 1280×720 from the start).

## Architecture (M4 — implemented)

- `ISteamService` (`Assets/_StackUp/Scripts/Steam/`) — init, achievements, stats,
  leaderboards, callbacks.
- `MockSteamService` — in-memory default; the game runs fully without Steam.
- `SteamServices` — static locator; `SteamServices.Init()` is called from
  `GameManager.Awake` (defaults to the mock). M6 passes a real implementation.
- `SteamIds` — central achievement/stat/leaderboard keys (mirror these in the
  Steamworks partner config).
- `SteamTelemetry` — the only translator from gameplay events to `ISteamService`.
  Gameplay systems raise plain C# events and never reference Steam.
- Save files (`SaveService.CloudFiles`) sit at the persistentDataPath root, ready
  to map to Steam Cloud.

## TODO

- [x] Define `ISteamService` interface (M4)
- [x] Add `MockSteamService` (M4)
- [x] Document achievement/stat trigger points (M4)
- [ ] Document Steamworks.NET setup steps (M6)
- [ ] Document `steam_appid.txt` usage and that it is gitignored
- [ ] Document Steam build branch workflow (M6)
