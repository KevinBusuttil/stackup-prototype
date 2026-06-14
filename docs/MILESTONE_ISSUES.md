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
- [ ] 9. Implement PlayerController movement
- [ ] 10. Implement high-angle Cinemachine camera
- [ ] 11. Create small test warehouse scene
- [ ] 12. Implement WarehouseGrid slot registry
- [ ] 13. Implement basic SKU stock data
- [ ] 14. Implement basic order generation
- [ ] 15. Implement rack pick interaction
- [ ] 16. Implement tote inventory
- [ ] 17. Implement dock loading interaction
- [ ] 18. Implement basic HUD
- [ ] 19. Implement level result screen

### M2 — Core Gameplay Loop
- [ ] 20. Implement pallet grid system
- [ ] 21. Implement placement preview
- [ ] 22. Implement stacking validation rules
- [ ] 23. Implement verification station
- [ ] 24. Implement rework jobs
- [ ] 25. Implement scoring system
- [ ] 26. Implement order queue
- [ ] 27. Implement multiple active orders
- [ ] 28. Implement wrong pick / wrong dock penalties

### M3 — Content and Progression
- [ ] 29. Implement campaign level select
- [ ] 30. Implement level progression save
- [ ] 31. Create 8 campaign level definitions
- [ ] 32. Implement endless mode
- [ ] 33. Create SKU pool and sample item prefabs
- [ ] 34. Import robot worker variants
- [ ] 35. Create modular warehouse prefabs
- [ ] 36. Improve HUD for job queue and SLA timers

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
