# QA Checklist (M6 #57)

Run before tagging a release candidate. Mark P0 (blocker) issues — none may
remain to ship.

## Build & boot
- [ ] Windows 64-bit build is produced (CI `Build` workflow or `BuildScript.BuildWindows`).
- [ ] Game launches to the **Main Menu** with no errors in `Player.log`.
- [ ] EditMode tests pass (Test Runner / CI `Tests` workflow).
- [ ] No compile errors/warnings spike in the Console on a clean open.

## Core loop (campaign)
- [ ] Level 1 (First Pick): pick → load completes; result screen shows score.
- [ ] Stacking levels: stack onto pallet; **heavy cannot go on fragile**; height limit holds.
- [ ] Verification: verifying short fails + spawns rework; finishing + re-verifying passes.
- [ ] Wrong pick (decoy slot) and wrong dock apply penalties.
- [ ] SLA levels: timer counts down; expiry fails the order with a penalty.
- [ ] Completing a level unlocks the next; best score persists across restarts.
- [ ] Finishing all 8 unlocks **Warehouse Pro**.

## Endless
- [ ] Orders keep spawning; difficulty/wave increases.
- [ ] Running out of lives ends the run; high score + wave persist.

## UI / input
- [ ] All menus navigable with **keyboard, mouse, and controller**.
- [ ] Pause (Esc / Start) freezes gameplay; Resume/Settings/Menu work.
- [ ] Settings: UI scale (100–160%) and volumes change and persist.
- [ ] Readable at **1280×720** (Steam Deck) at default UI scale.

## Audio / feedback
- [ ] SFX play for pick/place/verify/penalty/complete; ambience loops.
- [ ] Music slider affects ambience; SFX slider affects effects.
- [ ] Feedback popups appear and are pooled (no leak/stutter over time).

## Visuals / performance
- [ ] Occluder fade: scenery between camera and player fades and restores.
- [ ] No pink/magenta materials (URP assigned).
- [ ] Holds ~60 FPS on a mid-range PC; acceptable on Steam Deck / Proton.

## Steam (with `STEAMWORKS_NET`)
- [ ] `[Steam] initialized.` logs; overlay opens.
- [ ] Achievements unlock (spot-check FIRST_PICK, FIRST_ORDER, a combo one).
- [ ] Stats increment; leaderboard entries submit (endless + a level time).
- [ ] Cloud save: progress/settings sync across machines.

## Sign-off
- [ ] No open **P0** bugs.
- [ ] Version bumped; release notes drafted.
