# Art Pipeline

Low-poly art for Stack Up!, generated procedurally in Blender and imported into
Unity. Authoritative requirements: [`CLAUDE_CODE_SPEC.md`](./CLAUDE_CODE_SPEC.md)
Sections 9, 10, 23.

## Style

Low-poly 3D, cute robot warehouse workers (screen faces, hi-vis vests, caps,
hardhats, goggles, forklift attachments). Readable, toy-like, modular scenery.

## Robot variants (Section 9)

- **Picker Bot** — wheeled base, rounded torso, screen face, **yellow** hi-vis vest, grey cap.
- **Supervisor Bot** — **orange** vest, screen face, tablet, claw arm, orange cap.
- **Forklift Bot** — **green** vest, **blue** hardhat, goggles, forklift mast/forks, bulkier.

## Generators (`Blender/scripts/`)

The models are built **procedurally** from Blender primitives — no manual modelling
— so they are reproducible and easy to tweak in code.

| Script | Builds | Assets |
|--------|--------|--------|
| `stackup_buildlib.py` | shared helpers (box/cyl/sphere/material/export) | — |
| `stackup_robots_blender5_beautified.py` | the 3 robot variants | PickerBot, SupervisorBot, ForkliftBot |
| `stackup_warehouse_blender5.py` | environment + props (Section 10) | FloorTile, WallStraight, WallCorner, WallDoor, RackBay, RackShelf, RackBackPanel, DockLaneMarker, VerificationStation, ReceivingZoneMarker, StagingZoneMarker, Tote, Pallet, BoxSmall/Medium/Large, BottleCase, Bag |

### Running the generators

Either inside Blender (5.x), or headless with the standalone `bpy` module.

```bash
# Option A — Blender app (5.x):
blender --background --python Blender/scripts/stackup_robots_blender5_beautified.py
blender --background --python Blender/scripts/stackup_warehouse_blender5.py

# Option B — standalone bpy (no Blender install):
pip install bpy            # matches Blender 5.0
python3 Blender/scripts/stackup_robots_blender5_beautified.py
python3 Blender/scripts/stackup_warehouse_blender5.py
```

Each asset is exported to `Blender/exports/` as **both**:
- `<Name>.glb` — modern glTF (compact; import via the free *glTFast* package).
- `<Name>.fbx` — **imports natively into Unity**, no extra package needed.

Both formats are committed (they're the deliverable). Re-running the scripts
overwrites them.

## Importing into Unity

1. Copy the desired `.fbx` files from `Blender/exports/` into
   `Unity/StackUpPrototype/Assets/_StackUp/Art/` (Characters / Environment / Props).
   - Or use `.glb` after adding `com.unity.cloud.gltfast` via Package Manager.
2. Import settings: **Scale Factor 1**, **Convert Units** on. Models are authored
   in metres at roughly gameplay scale (a robot is ~1.1 m tall, modules tile on a
   2 m grid). Materials import from the embedded Principled BSDF colours; under URP
   use **Assets ▸ Convert ▸ … to URP Materials** if they come in pink.
3. Make a prefab per model under `Assets/_StackUp/Prefabs/` and add colliders
   (BoxCollider for props/modules, CapsuleCollider for robots).

### Wiring into the runtime builder (automatic)

Models placed in `Assets/_StackUp/Resources/StackUpArt/<Name>.fbx` are loaded at
runtime by `PrefabLibrary` and overlaid on the gameplay primitives by
`LevelBootstrap.AttachVisual` (the placeholder mesh is hidden, its collider
kept). Currently wired: **PickerBot** (player), **VerificationStation**,
**DockLaneMarker**. Drop more models in and add an `AttachVisual(...)` call to
extend coverage (racks, pallet, items).

`ModelStyle.Apply` re-tints each model with URP materials by matching the source
material name, so imported FBX never render **pink** and screen/eye faces glow —
no in-editor material conversion needed. Bevelled edges come from the generator
(`_bevel` in `stackup_buildlib.py`).

## Modularity & performance rules (Sections 8, 23)

- Modules tile on a **2 m grid**; racks/walls stay separate objects so the
  occluder-fade system (`FadeableObject`) can fade them individually.
- Keep poly counts low (these are box/cylinder primitives); limit transparent
  materials; avoid excessive realtime lights.

## TODO

- [x] Add Blender robot generator script under `Blender/scripts/`
- [x] Generate environment + prop modules
- [x] Define export settings (GLB + FBX) and naming conventions
- [ ] Wire imported prefabs into `LevelBootstrap` (in-editor)
- [ ] Material/shader pass for URP + fade (transparent-compatible)
- [ ] Author real textures / screen-face emissive (later polish)
