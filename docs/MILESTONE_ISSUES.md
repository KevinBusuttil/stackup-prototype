# Milestone Issues

Status: **placeholder** — derived from [`CLAUDE_CODE_SPEC.md`](./CLAUDE_CODE_SPEC.md)
Sections 24 (milestones) and 25 (issue list). Use this as the tracking checklist
when creating GitHub issues.

## Milestones overview

| Milestone | Theme                          |
| --------- | ------------------------------ |
| M0        | Repository and Unity setup     |
| M1        | Vertical slice                 |
| M2        | Core gameplay loop             |
| M3        | Content and progression        |
| M4        | Steam architecture             |
| M5        | Polish and Steam Deck readiness|
| M6        | Release preparation (later)    |

## Issues

### M0 — Repository and Unity Setup
- [x] 1. Create Unity project skeleton — under `Unity/StackUpPrototype/`
- [x] 2. Add Unity/macOS `.gitignore`
- [~] 3. Configure URP — packages + global settings in place; verify the URP
      asset is the active render pipeline in-editor (project ships a 2D URP template)
- [x] 4. Configure Unity Input System — package enabled (`activeInputHandler: 2`)
- [x] 5. Create base folder structure — `Assets/_StackUp/` tree
- [x] 6. Create ScriptableObject data types — Section 11 model (`StackUp` namespace)
- [x] 7. Create initial Bootstrap scene — `Bootstrap.unity` + `GameManager`, build index 0
- [x] 8. Add Windows build instructions — `docs/BUILD_WINDOWS.md`

### M1 — Vertical Slice
- [x] 9. Implement PlayerController movement — Input System (keyboard + gamepad)
- [~] 10. Implement high-angle Cinemachine camera — `HighAngleCameraRig` (custom,
      dependency-free; swap to Cinemachine later)
- [x] 11. Create small test warehouse scene — `Warehouse_Level_01` + `LevelBootstrap`
      (builds floor/rack/dock/player at runtime)
- [x] 12. Implement WarehouseGrid slot registry
- [x] 13. Implement basic SKU stock data — `SlotMarker` stock + `SkuCatalog`
- [x] 14. Implement basic order generation — `OrderManager`
- [x] 15. Implement rack pick interaction — `RackSlot` + `IInteractable`/`PlayerInteractor`
- [x] 16. Implement tote inventory — `ToteInventory` + `Tote`
- [x] 17. Implement dock loading interaction — `DockLane`
- [x] 18. Implement basic HUD — `HUD` (job, tote, score, prompt)
- [x] 19. Implement level result screen — `ResultScreen`

### M2 — Core Gameplay Loop
- [x] 20. Implement pallet grid system — `PalletGrid` (3×3×4)
- [x] 21. Implement placement preview — `Pallet` ghost (green valid / red invalid)
- [x] 22. Implement stacking validation rules — height limit + heavy-on-fragile
- [x] 23. Implement verification station — `VerificationStation` + `VerificationResult`
- [x] 24. Implement rework jobs — `OrderManager` rework `Job`s on failed verify
- [x] 25. Implement scoring system — `ScoreSystem` (bonuses, combo, penalties)
- [x] 26. Implement order queue — `OrderManager` pending queue
- [x] 27. Implement multiple active orders — concurrent + cycle (Q/Tab/Y)
- [x] 28. Implement wrong pick / wrong dock penalties — `ScoreSystem` penalties
      wired in `RackSlot` / `DockLane`

> M2 demonstrator scene: `Warehouse_Level_02.unity` (stacking + verification +
> 2 concurrent orders). `Warehouse_Level_01.unity` remains the simple M1 slice.

### M3 — Content and Progression
- [x] 29. Implement campaign level select — `MainMenuController` (lock state + best score)
- [x] 30. Implement level progression save — `SaveService` (JSON: progress + highscores)
- [x] 31. Create 8 campaign level definitions — `LevelLibrary` / `LevelConfig`
- [x] 32. Implement endless mode — `OrderManager` waves + `LevelLibrary.Endless`
- [~] 33. Create SKU pool and sample item prefabs — SKU pool done; item prefabs are
      primitives (real prefabs need art)
- [ ] 34. Import robot worker variants — deferred (needs Blender models)
- [ ] 35. Create modular warehouse prefabs — deferred (needs art); primitives for now
- [x] 36. Improve HUD for job queue and SLA timers — HUD shows orders, SLA countdown, wave

> Flow: Bootstrap → MainMenu (Campaign / Endless) → Game scene (data-driven by
> `LevelConfig`). Levels unlock as you complete them; endless tracks high score + wave.
> #34/#35 are art tasks deferred until the Blender pipeline lands.

### M4 — Steam Architecture
- [ ] 37. Create SteamService abstraction
- [ ] 38. Create MockSteamService
- [ ] 39. Add achievement event hooks
- [ ] 40. Add stat event hooks
- [ ] 41. Add leaderboard interface
- [ ] 42. Make save system Steam Cloud-compatible

### M5 — Polish and Steam Deck Readiness
- [ ] 43. Implement occluder fade system
- [ ] 44. Add controller-first menu navigation
- [ ] 45. Implement settings menu
- [ ] 46. Add UI scale option
- [ ] 47. Add core SFX
- [ ] 48. Add feedback effects
- [ ] 49. Perform Steam Deck readability pass
- [ ] 50. Perform performance pass

### M6 — Release Preparation (Later)
- [ ] 51. Integrate Steamworks
- [ ] 52. Implement Steam achievements
- [ ] 53. Implement Steam stats
- [ ] 54. Implement Steam leaderboards
- [ ] 55. Implement Steam Cloud save sync
- [ ] 56. Create Steam build branch workflow
- [ ] 57. Create QA checklist
- [ ] 58. Produce release candidate build
