# Stack Up! Prototype

**Stack Up!** is a low-poly 3D warehouse operations game where you control cute
robot warehouse workers and fulfil customer orders under time pressure.

Core loop: **Pick → Stack → Verify → Load**

- **Engine:** Unity 6 LTS, Universal Render Pipeline (URP)
- **Dev machine:** macOS
- **Target:** Steam — Windows 64-bit first, with Steam Deck / Proton as an
  important secondary target
- **Input:** keyboard/mouse and controller (Unity Input System)

## Repository structure

```text
stackup-prototype/
  README.md
  .gitignore
  docs/                      # Design + handoff documentation
    CLAUDE_CODE_SPEC.md      # Full project spec (source of truth)
    STEAM_HANDOFF_UNITY.md
    MILESTONE_ISSUES.md
    ART_PIPELINE.md
    STEAM_RELEASE_CHECKLIST.md
  Unity/
    StackUpPrototype/        # The Unity project (open THIS folder in Unity)
      Assets/
      Packages/
      ProjectSettings/
  Blender/
    scripts/                 # Blender asset generator scripts
    exports/                 # Exported meshes for Unity import
  Builds/                    # Local build output (gitignored)
```

## Getting started (macOS)

```bash
git clone https://github.com/KevinBusuttil/stackup-prototype.git
cd stackup-prototype
```

Open the Unity project by pointing Unity Hub at `Unity/StackUpPrototype`
(not the repository root). Use a modern Unity 6 LTS editor.

## Documentation

The authoritative spec is [`docs/CLAUDE_CODE_SPEC.md`](docs/CLAUDE_CODE_SPEC.md).
Milestone tracking lives in [`docs/MILESTONE_ISSUES.md`](docs/MILESTONE_ISSUES.md).

## Status

Early prototype / scaffolding. See the milestones (M0–M6) in the spec for the
development roadmap.
