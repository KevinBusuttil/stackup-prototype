# Steam Release Checklist

Status: **placeholder** — to be completed during milestone **M6**.
See [`CLAUDE_CODE_SPEC.md`](./CLAUDE_CODE_SPEC.md) Sections 20–23 for details.

## Build

- [ ] Windows 64-bit build produced from `Unity/StackUpPrototype`
- [ ] Steam Deck / Proton smoke test passes
- [ ] 60 FPS on mid-range Windows PC; acceptable Steam Deck performance
- [ ] No shader compilation hitches during gameplay
- [ ] `Builds/` output is gitignored (only `Builds/.gitkeep` tracked)
- [ ] `steam_appid.txt` present locally for testing and gitignored

## Steamworks integration

- [ ] Real `SteamService` replaces `MockSteamService` behind `ISteamService`
- [ ] Steam initialization succeeds; overlay works
- [ ] Achievements unlock (all IDs from spec Section 20.2)
- [ ] Stats report correctly (spec Section 20.3)
- [ ] Leaderboards submit (endless high score, endless wave, level best times)
- [ ] Steam Cloud sync works for `profile.json`, `progress.json`,
      `settings.json`, `highscores.json`

## Controller / Steam Deck UX

- [ ] All menus controller navigable (no hover-only dependencies)
- [ ] Readable at 1280×720, no tiny text
- [ ] UI scale option 100%–160% works

## Audio

- [ ] Core SFX present (pick, place, scan, verify pass/fail, warning,
      order complete, countdown, ambience)

## QA

- [ ] QA checklist completed
- [ ] No P0 bugs remain
- [ ] Release candidate build approved
