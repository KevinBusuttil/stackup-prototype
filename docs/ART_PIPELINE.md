# Art Pipeline

Status: **placeholder** — to be expanded during milestone **M3** and ongoing.
Authoritative requirements live in [`CLAUDE_CODE_SPEC.md`](./CLAUDE_CODE_SPEC.md)
Sections 9, 10, and 23.

## Style

Low-poly 3D. Cute robot warehouse workers (screen faces, hi-vis vests, caps,
hardhats, goggles, tablets, forklift attachments). Readable, toy-like.

## Robot variants (Section 9)

- **Picker Bot** — wheeled base, rounded torso, screen face, yellow hi-vis vest, grey/dark cap.
- **Supervisor Bot** — orange vest/jacket, screen face, tablet, optional claw arm, red/orange cap.
- **Forklift Bot** — green hi-vis vest, blue hardhat, goggles, forklift mast/forks, bulkier.

## Blender → Unity flow

1. Author/generate assets in Blender. Generator scripts live under
   `Blender/scripts/` (e.g. `stackup_robots_blender5_beautified.py`).
2. Export to `Blender/exports/` (glTF/`.glb` or `.fbx` preferred for Unity).
   `.blend` working files are fine to keep; Blender backup files
   (`*.blend1`, `*.blend2`, etc.) are gitignored.
3. Import into Unity under
   `Unity/StackUpPrototype/Assets/_StackUp/Art/` in the matching subfolder
   (`Characters/`, `Environment/`, `Props/`, `UI/`, `Materials/`).

## Required MVP assets (Section 10)

- **Characters:** PickerBot, SupervisorBot, ForkliftBot
- **Environment:** floor tile, walls (straight/corner/door), rack bay/shelf/back
  panel, dock lane marker, verification station, receiving + staging zone markers
- **Props:** tote, pallet, box (small/medium/large), case/bottle case, bag/sack
- **UI:** SKU icons, pick/stack/verify/load icons, warning + success icons

## Modularity & performance rules (Sections 8, 23)

- Racks and walls **must be modular** — no single merged warehouse mesh
  (the occluder-fade system fades individual objects).
- Fadeable objects need transparent-compatible materials.
- Keep poly counts low; limit transparent materials; avoid excessive realtime lights.

## TODO

- [ ] Add Blender robot generator script under `Blender/scripts/`
- [ ] Define export settings / naming conventions
- [ ] Establish material/shader conventions for URP + fade
- [ ] Document import settings (scale, colliders, LODs)
