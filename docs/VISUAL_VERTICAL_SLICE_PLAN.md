# Visual Vertical Slice Plan

The goal of this milestone is **not** to make the whole game beautiful. It is to
establish a credible, repeatable **visual direction** and make the first playable
scene (the Game scene) read as a *charming toy warehouse* instead of a programmer
prototype — while never breaking the Pick → Stack → Verify → Load loop and never
*requiring* imported art to exist.

Authoritative art requirements live in
[`CLAUDE_CODE_SPEC.md`](./CLAUDE_CODE_SPEC.md) (Sections 7–10, 18, 23) and the
pipeline in [`ART_PIPELINE.md`](./ART_PIPELINE.md).

---

## 1. Current visual state

The runtime level is assembled procedurally by
`Assets/_StackUp/Scripts/Core/LevelBootstrap.cs`:

| Element | Before this pass | Notes |
|---|---|---|
| Floor | One grey `Plane`, no markings | flat, featureless |
| Racks | A single coloured `Cube` per slot | reads as a box, not a rack |
| Docks | Green `Cube` pad | no lane language |
| Verification | Amber `Cube` | indistinct from a dock |
| Player | Blue `Capsule` | no robot identity |
| Pallet / items | `Cube`s, SKU-coloured | acceptable, item colour already SKU-driven |
| Lighting | One directional light; **dim flat ambient (~0.21)** | murky, "default scene" look |
| Post FX | `DefaultVolumeProfile` wired globally but **all effects no-op** (Bloom 0, Tonemapping None) | no grade |
| Camera | High-angle follow rig (good), but black background | framed in black |
| HUD | All-text TMP, white on transparent | hard to read over a bright scene |

There is already a **safe, optional art overlay**: `PrefabLibrary` loads
`Resources/StackUpArt/<Name>` and `LevelBootstrap.AttachVisual` overlays it on the
gameplay primitive (hiding the placeholder mesh, keeping the collider).
`ModelStyle` re-tints imported models to URP so they never import pink. Three
robot/station FBX already live in `Resources/StackUpArt`.

### Visual confusion / legacy

- **Legacy 2D stacking prototype still in the project**: `Assets/Scripts/BlockController.cs`
  and `Assets/Scripts/StackGameManager.cs` (outside `_StackUp`), plus
  `Assets/Scenes/SampleScene.unity`.
- `SampleScene.unity` was **enabled in build settings** (now disabled — see §8).

---

## 2. Main graphics risks (ranked)

1. **No clear art direction baked into the scene** — primitives + dim lighting
   read as "engine default". *Highest priority; cheapest to fix.*
2. **Lighting/grade too dark & flat** — kills the "toy" feel and Steam Deck
   readability.
3. **Silhouette confusion** — racks, docks and the verify station are all just
   coloured cubes; the player is a capsule. No recognisable identity.
4. **Floor has no gameplay colour language** — players can't read zones
   (storage / staging / verify / dock) at a glance.
5. **HUD legibility at 720p** — white text over a now-brighter scene needs
   backing.
6. **Project clutter** — legacy 2D prototype + SampleScene in build settings
   create confusion about "what is the game".
7. **Art hook-up coverage was thin** — only 3 of ~15 host types could be
   visually replaced by prefabs.

---

## 3. Target art direction

**Readable, charming, toy-like low-poly warehouse.**

- Low-poly, soft-bevelled shapes; chunky, friendly proportions.
- Bright, slightly cool key + warm fill; **never dark or murky**.
- Saturated but controlled palette; one strong accent (warehouse orange/yellow).
- **Cute robot worker identity** — screen face, glowing eyes, hi-vis vest, cap.
- **Gameplay colour language is consistent and learnable** (see §6).
- Composes well in a high-angle screenshot at **1280×720 / Steam Deck**.

**Explicitly avoid:** realistic warehouse sim, grey industrial realism, dark
lighting, huge asset counts, procedural art with no charm.

### Colour language (single source of truth: `ArtKit`)

| Meaning | Colour | Used by |
|---|---|---|
| Player / "you" | Cyan-blue | PickerBot |
| Storage / racks | Steel blue + orange beams | RackBay |
| Ship / dock | Green + hazard stripes | DockLaneMarker |
| Verify / inspect | Amber + glowing screen | VerificationStation |
| Staging | Warm yellow floor zone | pallet area |
| Item identity | Per-SKU `DisplayColor` | rack stock, pallet items |

---

## 4. Priority visual assets

In rough order of screenshot impact (prefab name → `Resources/StackUpArt`):

1. `PickerBot` *(present)* — the hero; must read as a cute robot.
2. `RackBay` / `RackShelf` — define the aisle.
3. `Pallet`, `BoxSmall/Medium/Large`, `BottleCase`, `Bag`, `Tote` — the things
   you actually carry and stack.
4. `VerificationStation` *(present)*, `DockLaneMarker` *(present)*.
5. `FloorTile`, `WallStraight`, `WallCorner` — frame the space.

All of these already have Blender generators (`Blender/scripts/`) and exports.
Wiring is **drop-in**: place `<Name>.fbx` in `Resources/StackUpArt` and the
matching `AttachArt(...)` call uses it automatically; otherwise the procedural
fallback shows.

---

## 5. Lighting / post-processing recommendations

Implemented now (all in code / the existing volume profile — no new assets):

- **Three-point-ish lighting**: warm key (soft shadows, strength 0.65) + cool
  fill from behind; bright **Trilight gradient ambient** so shadowed faces stay
  readable.
- **Soft sky camera background** instead of black.
- **Enable URP post on the runtime camera** and give the (previously no-op)
  `DefaultVolumeProfile` a *gentle* toy grade: Neutral tonemapping, +exposure
  0.08, +contrast 6, +saturation 10, mild Bloom (threshold 1.05, intensity 0.35).

Keep avoiding: heavy bloom, SSAO/SSR, many realtime shadow-casting lights,
expensive DoF. Steam Deck is the budget ceiling.

---

## 6. HUD / UI style direction

Direction: **warehouse handheld scanner / terminal** — dark translucent panels,
monospace-ish readouts, colour-coded order state, chunky controller prompts.

Implemented now: translucent backing panels behind the order list and score so
white text stays legible over the brighter scene.

Still to do (documented, not yet built):

- SKU **icons / colour chips** next to order lines (not just text IDs).
- Order **state colour-coding** (READY = green, REWORK = red, in-progress = amber)
  and per-line progress bars.
- **Controller prompt glyphs** (currently `[E]` only) — show gamepad button.
- Order "cards" with a panel/border per order instead of a flat list.
- A consistent UI font + 720p type scale audit.

---

## 7. First visual slice checklist

- [x] Central palette + colour language (`ArtKit`).
- [x] Lighting pass (key + fill + gradient ambient, soft shadows).
- [x] Camera background + URP post enabled + gentle grade.
- [x] Floor markings (aisle line + storage/staging/verify zone tints).
- [x] Rack placeholder reads as a rack (uprights + beams + shelves + stock box).
- [x] Dock placeholder reads as a lane (hazard stripes + border).
- [x] Verification placeholder reads as a scanner (plinth + glowing screen).
- [x] Player placeholder reads as a robot (body + screen + eyes + cap + wheels).
- [x] Prefab-or-procedural for **all** main hosts (so imported art drops in).
- [x] HUD backing panels for 720p legibility.
- [x] Build hygiene: SampleScene disabled in build settings.
- [x] Enclosing **warehouse shell** (walls + lit clerestory windows) so the space
      feels like a room, not objects on a plane.
- [x] **Background rack aisles** (deep storage rows on three sides) for depth.
- [x] **Ground clutter** (loose pallets/crates, safety bollards, cones) +
      expanded floor markings (cross aisle, hazard safety lane).
- [ ] Import remaining Blender props into `Resources/StackUpArt` (RackBay, Pallet,
      boxes, walls) — *manual Unity step.*
- [ ] HUD order-state colour-coding + SKU chips.
- [ ] Optional: hanging zone signage + a subtle skybox.

---

## 8. What NOT to work on yet

- New **gameplay** features (explicitly out of scope).
- Real textures / normal maps / PBR materials — flat-shaded low-poly is the look.
- Character animation / rigging — static posed bots are fine for the slice.
- Cinemachine, custom render features, decal projectors, baked GI.
- A large asset library — keep the count small and modular (2 m grid).
- **Do not delete** the legacy 2D prototype yet (see below) — only de-risk it.

### Recommended cleanup (low risk)

- `Assets/Scenes/SampleScene.unity` — **disabled in build settings** (done). It is
  the stock Unity template scene, not part of the game.
- `Assets/Scripts/BlockController.cs` + `StackGameManager.cs` — the old 2D
  stacking prototype. **Recommendation:** move to `Assets/_Archive/` (kept out of
  any assembly/build) or delete in a dedicated, clearly-labelled commit. Not
  removed here because deletion should be an explicit, reviewed decision.

---

## 9. Acceptance criteria — "graphics milestone done"

1. One scene (Game) looks **coherent in a screenshot** — consistent palette &
   lighting, no "default engine" murk.
2. The **player robot has a recognisable identity** (robot, not a capsule).
3. The **warehouse aisle is visually readable** — racks look like racks, zones
   are colour-coded.
4. **Pickable items are visually distinct** (per-SKU colour, distinct shapes).
5. **Dock and verification zones are clear** and not confusable with each other.
6. **HUD is readable at 720p** / Steam Deck.
7. The game **still runs with no imported art** (procedural fallbacks).
8. **No gameplay regression** — colliders, interaction, orders all unchanged.

---

## 10. Recommended next tasks (in order)

1. Import the remaining Blender exports into `Resources/StackUpArt` (RackBay,
   RackShelf, Pallet, Tote, BoxSmall/Medium/Large, BottleCase, Bag, FloorTile,
   WallStraight, WallCorner) and verify the `AttachArt` swaps.
2. Wire **pallet items + rack stock** to SKU prop prefabs
   (`ArtKit.PrefabForPackaging`) once the box props are imported.
3. Build a **room shell** (walls/corners on the 2 m grid) to frame screenshots.
4. HUD pass: order cards, state colour-coding, SKU chips, gamepad glyphs.
5. Archive/remove the legacy 2D prototype in a dedicated commit.
6. Tune the volume profile against a real captured frame on a Deck-resolution
   target; add a subtle skybox if desired.
