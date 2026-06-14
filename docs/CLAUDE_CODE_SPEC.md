# Stack Up! Prototype — Unity + Steam Handoff Spec for Claude Code

You are working on a Unity game prototype called **Stack Up!**.

Repository:

```text
https://github.com/KevinBusuttil/stackup-prototype
```

Development machine:

```text
macOS
```

Target game engine:

```text
Unity
```

Target release platform:

```text
Steam — Windows 64-bit first, with Steam Deck / Proton compatibility as an important target.
```

---

# 1. Project Vision

**Stack Up!** is a low-poly 3D warehouse operations game where the player controls cute robot warehouse workers and fulfils orders under time pressure.

The core gameplay loop is:

```text
Pick → Stack → Verify → Load
```

The player receives customer orders, navigates a warehouse, picks items from rack slots, stacks them onto totes or pallets, verifies the order at a checking station, and loads it into the correct dock lane.

The game should feel:

* readable
* fast
* satisfying
* slightly chaotic but fair
* suitable for Steam
* playable with keyboard/mouse and controller
* readable on Steam Deck

The art style should be low-poly 3D with cute robot workers, similar to toy-like warehouse robots with screen faces, high-vis vests, caps, hardhats, goggles, tablets, and forklift attachments.

---

# 2. Repository Structure

Set up the repository using this structure:

```text
stackup-prototype/
  README.md
  .gitignore
  docs/
    STEAM_HANDOFF_UNITY.md
    MILESTONE_ISSUES.md
    ART_PIPELINE.md
    STEAM_RELEASE_CHECKLIST.md
    CLAUDE_CODE_SPEC.md
  Unity/
    StackUpPrototype/
      Assets/
      Packages/
      ProjectSettings/
  Blender/
    scripts/
      stackup_robots_blender5_beautified.py
    exports/
  Builds/
    .gitkeep
```

The Unity project should live under:

```text
Unity/StackUpPrototype/
```

Do not place the Unity project directly at the repository root.

---

# 3. macOS Setup Requirements

Assume the project is being developed on macOS.

Recommended local commands:

```bash
cd ~/Projects
git clone https://github.com/KevinBusuttil/stackup-prototype.git
cd stackup-prototype
mkdir -p docs Unity Blender/scripts Blender/exports Builds
touch Builds/.gitkeep
```

Add a root `.gitignore` suitable for Unity, macOS, Blender, and Steam local files.

Required `.gitignore` content:

```gitignore
# macOS
.DS_Store
.AppleDouble
.LSOverride
# Unity
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]ser[Ss]ettings/
MemoryCaptures/
Recordings/
# Unity generated
*.csproj
*.sln
*.suo
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db
# Rider / VS Code
.idea/
.vscode/
# Blender backup files
*.blend1
*.blend2
*.blend@
*.blend~
# Keep exported builds out of git
Builds/*
!Builds/.gitkeep
# Steam local files
steam_appid.txt
```

---

# 4. Unity Project Requirements

Use a modern Unity LTS version.

Recommended:

```text
Unity 2022 LTS or Unity 6 LTS
```

Use:

```text
Universal Render Pipeline (URP)
```

Required packages/systems:

* Unity Input System
* Cinemachine
* TextMeshPro
* URP
* Optional later: Addressables
* Optional later: Steamworks.NET or another Unity-compatible Steamworks wrapper

The Unity folder layout should be:

```text
Assets/
  _StackUp/
    Art/
      Characters/
      Environment/
      Props/
      UI/
      Materials/
    Audio/
      SFX/
      Music/
      Ambience/
    Prefabs/
      Characters/
      Warehouse/
      Items/
      UI/
    Scenes/
      Bootstrap.unity
      MainMenu.unity
      Warehouse_Level_01.unity
      Warehouse_Level_02.unity
    Scripts/
      Core/
      Player/
      Warehouse/
      Orders/
      Inventory/
      Interaction/
      UI/
      Steam/
      Save/
      Camera/
    ScriptableObjects/
      SKUs/
      Levels/
      Orders/
      Cosmetics/
    Settings/
```

---

# 5. Core Game Concept

The player controls a robot worker in a warehouse.

The main player activities are:

```text
Move
Interact
Pick item
Place item
Stack item
Verify order
Load order
Cycle active job
Pause game
```

The required gameplay loop for MVP is:

1. Generate order.
2. Assign pick job.
3. Player moves to rack slot.
4. Player picks required SKU quantity.
5. Player places item into tote or pallet.
6. Player moves to verification station.
7. Verification station compares collected items against order.
8. If correct, order becomes verified.
9. Player moves verified order to dock lane.
10. Dock lane validates order.
11. Order completes.
12. Score is awarded.

---

# 6. Game Modes

Implement the game around two modes.

## 6.1 Campaign Mode

MVP should support 8 campaign levels.

Suggested levels:

1. **First Pick**
   * One SKU
   * One order
   * Pick and load only
2. **More Items**
   * Multiple SKUs
   * Basic order queue
3. **Stacking Intro**
   * Pallet grid introduced
4. **Verification**
   * Verification station required
5. **Time Pressure**
   * Multiple active orders
   * SLA timers
6. **Batch Picking**
   * Efficient routing encouraged
7. **Larger Warehouse**
   * Longer travel distance
   * More racks
8. **Controlled Chaos**
   * Multiple concurrent orders
   * Verification
   * Dock lane management

## 6.2 Endless Mode

Endless mode should:

* spawn waves of orders
* increase SKU variety over time
* increase concurrent orders
* reduce available time
* track high score
* track highest wave
* submit score to Steam leaderboard later

---

# 7. Camera Specification

Use a high-angle 3D perspective camera.

Recommended starting settings:

```text
Pitch: 50–60 degrees
Field of View: 25–40
Follow target: player robot
Camera system: Cinemachine
```

The camera should:

* follow smoothly
* not jitter
* provide good warehouse readability
* support controller-friendly gameplay
* optionally support zoom later

---

# 8. Occluder Fade / Transparency Mechanic

The game must include a visibility system.

When tall scenery blocks the view between the camera and the player, the object should fade.

Implementation:

* Raycast or spherecast from camera to player head marker.
* Detect objects on a `Fadeable` or `Occluder` layer.
* Fade detected objects to around 25–35% opacity.
* Restore opacity smoothly when no longer blocking.
* Avoid fading the player, tote, active item, or important interaction objects.

Required Unity components:

```text
OccluderFadeSystem
FadeableObject
```

Implementation notes:

* Racks and walls must be modular.
* Do not use one giant merged warehouse mesh.
* Use transparent-compatible materials for fadeable objects.
* Cache original material state where possible.
* Avoid excessive material instancing.

---

# 9. Art Direction

Use low-poly 3D.

Characters are cute robot warehouse workers.

Robot variants:

## 9.1 Picker Bot

Visual traits:

* small wheeled base
* rounded robot torso
* screen face
* yellow hi-vis vest
* grey or dark cap
* simple arms

## 9.2 Supervisor Bot

Visual traits:

* orange vest or jacket
* screen face
* tablet accessory
* optional claw arm
* red or orange cap

## 9.3 Forklift Bot

Visual traits:

* green hi-vis vest
* blue hardhat
* goggles
* forklift mast/frame/forks
* slightly bulkier silhouette

---

# 10. Required Art Assets

Minimum MVP assets:

```text
Characters:
  PickerBot
  SupervisorBot
  ForkliftBot
Environment:
  Floor tile
  Wall straight
  Wall corner
  Wall door / dock door
  Rack bay
  Rack shelf
  Rack back panel
  Dock lane marker
  Verification station
  Receiving zone marker
  Staging zone marker
Props:
  Tote
  Pallet
  Box small
  Box medium
  Box large
  Case / bottle case
  Bag / sack
UI:
  SKU icons
  Pick icon
  Stack icon
  Verify icon
  Load icon
  Warning icon
  Success icon
```

Blender-generated assets can be placed under:

```text
Blender/scripts/
Blender/exports/
```

Imported Unity assets should go under:

```text
Unity/StackUpPrototype/Assets/_StackUp/Art/
```

---

# 11. Data Model

Use ScriptableObjects for design-time data.

## 11.1 SKU Definition

Create:

```csharp
public enum PackagingType
{
    Box,
    Case,
    Bag,
    BottleCase
}
public enum WeightClass
{
    Light,
    Medium,
    Heavy
}
public enum StackClass
{
    Standard,
    Fragile,
    Liquid
}
[CreateAssetMenu(menuName = "StackUp/SKU")]
public class SkuDefinition : ScriptableObject
{
    public string SkuId;
    public string DisplayName;
    public PackagingType PackagingType;
    public WeightClass WeightClass;
    public StackClass StackClass;
    public Sprite Icon;
    public Color DisplayColor;
    public GameObject VisualPrefab;
}
```

## 11.2 Order Data

Create:

```csharp
[System.Serializable]
public class OrderLine
{
    public string SkuId;
    public int Quantity;
}
public enum OrderState
{
    Pending,
    Picking,
    Picked,
    VerificationFailed,
    Verified,
    Loaded,
    Failed
}
[System.Serializable]
public class CustomerOrder
{
    public string OrderId;
    public int Priority;
    public List<OrderLine> Lines;
    public float DueTimeSeconds;
    public string DockLaneId;
    public OrderState State;
}
```

## 11.3 Job Data

Create:

```csharp
public enum JobType
{
    Pick,
    Stack,
    Verify,
    Load,
    Rework
}
public enum JobState
{
    Pending,
    Active,
    Completed,
    Failed
}
public class Job
{
    public string JobId;
    public JobType Type;
    public string OrderId;
    public string SkuId;
    public int Quantity;
    public string SourceSlotId;
    public string TargetLocationId;
    public JobState State;
}
```

## 11.4 Slot Data

Create:

```csharp
public class SlotData
{
    public string SlotId;
    public Vector3 WorldPosition;
    public string ZoneId;
    public Dictionary<string, int> StockBySku;
    public int Capacity;
}
```

---

# 12. Warehouse Slot System

Rack slot markers should be represented as child transforms.

Example Unity hierarchy:

```text
RackBay_A
  Mesh
  Colliders
  SlotMarkers
    Slot_A01_R01_L01_C01
    Slot_A01_R01_L01_C02
    Slot_A01_R01_L02_C01
```

Create a `WarehouseGrid` system that:

* scans all slot marker transforms
* registers each slot by ID
* stores world position
* tracks stock by SKU
* supports lookup by SKU
* supports finding nearest valid slot
* supports debug visualisation of slot IDs

---

# 13. Core Unity Systems to Implement

## 13.1 GameManager

Responsible for:

* game state
* level start
* level end
* timers
* win/loss
* score
* mode selection
* events

States:

```text
Boot
MainMenu
LevelLoading
Running
Paused
Results
```

## 13.2 PlayerController

Responsible for:

* movement
* controller input
* keyboard input
* collision
* movement animation
* interaction trigger

## 13.3 InteractionSystem

Responsible for:

* detecting nearby interactables
* selecting best interactable
* showing prompt
* calling interaction logic

Create interface:

```csharp
public interface IInteractable
{
    string GetPrompt();
    bool CanInteract(PlayerController player);
    void Interact(PlayerController player);
}
```

## 13.4 OrderManager

Responsible for:

* order generation
* order state
* order queue
* job creation
* job progression
* rework jobs
* order completion

## 13.5 InventorySystem

Responsible for:

* rack stock
* tote contents
* pallet contents
* SKU quantity changes
* stock validation

## 13.6 PalletSystem

Responsible for:

* pallet grid
* placement preview
* stack validation
* stack height
* visual placement

## 13.7 VerificationStation

Responsible for:

* comparing order requirements with collected contents
* showing required vs collected
* passing or failing verification
* generating rework jobs

## 13.8 DockLane

Responsible for:

* accepting verified orders
* checking correct lane
* completing orders
* wrong-lane penalty

## 13.9 SaveService

Responsible for:

* persistent save files
* profile
* settings
* campaign progress
* highscores
* cosmetics later

## 13.10 SteamService

Responsible later for:

* Steam initialisation
* achievements
* stats
* leaderboards
* cloud save support

For early prototype, create an interface and mock implementation so the game does not depend on Steam immediately.

---

# 14. Tote and Pallet Specification

## 14.1 Tote

A tote is a simple item container.

Create:

```csharp
public class ToteInventory
{
    public Dictionary<string, int> Contents;
    public int MaxUnits;
}
```

Rules:

* picking adds items to tote
* tote cannot exceed max units
* HUD shows tote contents
* tote can be verified at station

## 14.2 Pallet

Pallet uses a grid.

Recommended MVP:

```text
Width: 3
Depth: 3
Max height: 4
```

Create:

```csharp
public class PalletGrid
{
    public int Width = 3;
    public int Depth = 3;
    public int MaxHeight = 4;
    public PalletCell[,] Cells;
}
```

Stacking rules:

* Heavy cannot be placed on fragile.
* Stack height cannot exceed limit.
* Invalid placement should be blocked.
* Placement preview should show valid/invalid.

---

# 15. Verification Rules

At verification station:

* compare order lines against collected contents
* if all quantities match, order becomes `Verified`
* if missing or wrong item found, order becomes `VerificationFailed`
* create rework job
* show clear UI feedback

Verification UI should show:

```text
Required SKU
Required Qty
Collected Qty
Missing Qty
Wrong Items
Pass / Fail
```

---

# 16. Loading Rules

Dock lane accepts only:

* verified orders
* matching `DockLaneId`

If player tries to load wrong order or wrong lane:

* block the action
* play warning sound
* show UI message
* optionally apply score penalty

---

# 17. Scoring System

Score should include:

```text
Base points per completed order
Time bonus
Accuracy bonus
Stacking bonus
Combo multiplier
Efficiency bonus
```

Penalties:

```text
Wrong pick
Wrong dock lane
Failed verification
Missed SLA
Illegal stacking attempt
Rework job generated
```

---

# 18. UI Requirements

## 18.1 Screens

Required screens:

```text
Main Menu
Level Select
Endless Mode
Settings
Pause Menu
Results
```

## 18.2 HUD

Required HUD elements:

```text
Active job panel
Current order info
SKU / quantity to pick
Tote or pallet contents
Timer / SLA
Score
Combo indicator
Direction arrow or route indicator
Interaction prompt
```

## 18.3 Controller / Steam Deck UI

All menus must be controller navigable.

Requirements:

* readable at 1280×720
* no tiny text
* no hover-only UI dependency
* UI scale option from 100% to 160%

---

# 19. Controls

## 19.1 Keyboard

```text
WASD: Move
E: Interact
Q / Tab: Cycle active job
Space: Confirm / place
Esc: Pause
```

## 19.2 Controller

```text
Left stick: Move
A: Interact / confirm
B: Cancel
X: Pick / place
Y: Cycle job
Start: Pause
Right stick: optional camera / zoom later
```

Use Unity Input System.

---

# 20. Steam Target Requirements

Steam is the intended commercial target.

Early prototype should not require Steam to run, but architecture must allow Steam integration cleanly.

## 20.1 Required Steam Features Later

```text
Steam initialization
Steam achievements
Steam stats
Steam Cloud saves
Steam leaderboards
Steam overlay compatibility
```

## 20.2 Achievements

Suggested achievements:

| ID            | Name          | Condition                                  |
| ------------- | ------------- | ------------------------------------------ |
| FIRST_PICK    | First Pick    | Pick any item                              |
| FIRST_ORDER   | First Order   | Complete first order                       |
| PERFECT_ORDER | Perfect Order | Complete order with no mistakes            |
| COMBO_5       | Five in a Row | Complete 5 perfect orders consecutively    |
| COMBO_10      | Ten in a Row  | Complete 10 perfect orders consecutively   |
| STACK_MASTER  | Stack Master  | Complete pallet with no illegal placements |
| NO_REWORK     | No Rework     | Finish a level without rework              |
| SPEED_RUNNER  | Speed Runner  | Beat a level target time                   |
| ENDLESS_10    | Wave 10       | Reach endless wave 10                      |
| ENDLESS_25    | Wave 25       | Reach endless wave 25                      |
| DOCK_PERFECT  | Dock Perfect  | Load 5 orders without wrong dock attempt   |
| WAREHOUSE_PRO | Warehouse Pro | Complete campaign                          |

## 20.3 Steam Stats

Suggested stats:

```text
Total orders completed
Total picks
Perfect orders
Wrong picks
Rework jobs
Best combo
Total play time
Highest endless wave
```

## 20.4 Steam Leaderboards

Required leaderboards later:

```text
Endless high score
Endless highest wave
Best completion time per campaign level
```

---

# 21. Save System

Save files should use Unity persistent data path.

Recommended files:

```text
profile.json
progress.json
settings.json
highscores.json
```

Save:

```text
Campaign progress
Unlocked levels
Best scores
Best times
Settings
Cosmetics unlocked later
```

Save system should be Steam Cloud-compatible later.

---

# 22. Audio Requirements

Minimum SFX:

```text
Robot movement hum
Pick sound
Place sound
Scan beep
Verification pass
Verification fail
Wrong action warning
Order complete
Countdown warning
Warehouse ambience loop
```

---

# 23. Performance Requirements

Target:

```text
60 FPS on mid-range Windows PC
Acceptable Steam Deck / Proton performance
```

Optimisation requirements:

* low-poly assets
* object pooling for items and popups
* avoid excessive realtime lights
* keep transparent materials limited
* use URP carefully
* avoid shader compilation hitches during gameplay

---

# 24. Development Milestones

## M0 — Repository and Unity Setup

Tasks:

* create repo structure
* create Unity project under `Unity/StackUpPrototype`
* configure URP
* configure Input System
* configure TextMeshPro
* create folder structure
* add `.gitignore`
* create bootstrap scene
* create build instructions

Acceptance:

* Unity project opens on macOS
* project enters Play Mode
* initial scene loads
* repo does not track Unity Library/Temp files

---

## M1 — Vertical Slice

Tasks:

* implement player robot movement
* implement high-angle camera
* create small warehouse scene
* create rack slots
* implement WarehouseGrid
* implement basic OrderManager
* implement one SKU / one order
* implement picking from slot
* implement tote inventory
* implement loading into dock lane

Acceptance:

* player can start level
* order appears
* player picks item
* player loads correct item
* order completes
* score/result appears

---

## M2 — Core Gameplay Loop

Tasks:

* implement pallet grid
* implement stacking placement preview
* implement stacking validation
* implement verification station
* implement rework jobs
* implement scoring penalties
* implement order queue
* implement multiple active orders

Acceptance:

* Pick → Stack → Verify → Load works
* wrong/missing items fail verification
* rework can be completed
* stacking rules work
* scoring works

---

## M3 — Content and Progression

Tasks:

* create 8 campaign level definitions
* create level select screen
* implement level unlock/save
* create endless mode
* create SKU pool
* create warehouse asset prefabs
* import robot variants
* create HUD polish pass

Acceptance:

* 8 levels playable
* Endless mode playable
* progress saves
* UI shows job queue and timers
* assets are visually coherent

---

## M4 — Steam Architecture

Tasks:

* create SteamService interface
* create MockSteamService
* add achievement trigger points
* add stats trigger points
* prepare leaderboard submission interface
* prepare cloud-save-compatible file layout

Acceptance:

* game runs without Steam
* Steam-related calls go through service abstraction
* later Steamworks integration can be added without rewriting gameplay systems

---

## M5 — Polish and Steam Deck Readiness

Tasks:

* implement occluder fade
* implement controller-first UI navigation
* implement settings screen
* add UI scale
* add core audio
* improve feedback
* performance pass
* test at 1280×720

Acceptance:

* all menus controller navigable
* UI readable at 720p
* occluder fade works
* audio feedback works
* performance is acceptable

---

## M6 — Release Preparation Later

Tasks:

* integrate Steamworks
* implement achievements
* implement leaderboards
* implement Steam Cloud
* create Steam build workflow
* create QA checklist
* create release candidate

Acceptance:

* build works in Steam test environment
* achievements unlock
* leaderboard submits
* save sync works
* no P0 bugs remain

---

# 25. GitHub Issue List to Create

Create these issues in GitHub.

## M0 Issues

1. Create Unity project skeleton
2. Add Unity/macOS `.gitignore`
3. Configure URP
4. Configure Unity Input System
5. Create base folder structure
6. Create ScriptableObject data types
7. Create initial Bootstrap scene
8. Add Windows build instructions

## M1 Issues

9. Implement PlayerController movement
10. Implement high-angle Cinemachine camera
11. Create small test warehouse scene
12. Implement WarehouseGrid slot registry
13. Implement basic SKU stock data
14. Implement basic order generation
15. Implement rack pick interaction
16. Implement tote inventory
17. Implement dock loading interaction
18. Implement basic HUD
19. Implement level result screen

## M2 Issues

20. Implement pallet grid system
21. Implement placement preview
22. Implement stacking validation rules
23. Implement verification station
24. Implement rework jobs
25. Implement scoring system
26. Implement order queue
27. Implement multiple active orders
28. Implement wrong pick / wrong dock penalties

## M3 Issues

29. Implement campaign level select
30. Implement level progression save
31. Create 8 campaign level definitions
32. Implement endless mode
33. Create SKU pool and sample item prefabs
34. Import robot worker variants
35. Create modular warehouse prefabs
36. Improve HUD for job queue and SLA timers

## M4 Issues

37. Create SteamService abstraction
38. Create MockSteamService
39. Add achievement event hooks
40. Add stat event hooks
41. Add leaderboard interface
42. Make save system Steam Cloud-compatible

## M5 Issues

43. Implement occluder fade system
44. Add controller-first menu navigation
45. Implement settings menu
46. Add UI scale option
47. Add core SFX
48. Add feedback effects
49. Perform Steam Deck readability pass
50. Perform performance pass

## M6 Issues

51. Integrate Steamworks
52. Implement Steam achievements
53. Implement Steam stats
54. Implement Steam leaderboards
55. Implement Steam Cloud save sync
56. Create Steam build branch workflow
57. Create QA checklist
58. Produce release candidate build

---

# 26. Immediate Claude Code Tasks

Start by doing the following:

1. Inspect the repository.
2. Create the documentation structure under `docs/`.
3. Add this full spec as:

```text
docs/CLAUDE_CODE_SPEC.md
```

4. Add a Unity/macOS `.gitignore`.
5. Add placeholder docs:

```text
docs/STEAM_HANDOFF_UNITY.md
docs/MILESTONE_ISSUES.md
docs/ART_PIPELINE.md
docs/STEAM_RELEASE_CHECKLIST.md
```

6. If Unity project does not exist yet, create the folder placeholder:

```text
Unity/StackUpPrototype/
```

7. Do not generate Unity binary files manually.
8. Do not commit `Library`, `Temp`, `Obj`, `Builds`, or local Unity files.

---

# 27. Coding Style

Use clean, simple C#.

Prefer:

* small classes
* ScriptableObjects for static config
* event-driven communication where useful
* interfaces for interaction and Steam integration
* no hard-coded magic values where config is better
* comments only where they clarify behaviour

Initial systems should favour clarity over optimisation.

---

# 28. Important Design Decisions

For MVP:

* Use Unity, not Godot.
* Use low-poly 3D, not sprite-based 2D.
* Use high-angle camera, not first person.
* Use robot workers, not human workers.
* Use modular warehouse prefabs.
* Use slot markers for rack slots.
* Use blocked illegal placement rather than allowing broken pallets.
* Use mock Steam service first; integrate real Steamworks later.
* Target Windows Steam first, with Steam Deck readability from the start.

---

# 29. Definition of Done for Early Prototype

The early prototype is done when:

* Unity project opens on macOS.
* Player robot moves.
* Camera follows at high angle.
* One warehouse scene exists.
* One order is generated.
* Player can pick one item from a rack.
* Player can place it into tote.
* Player can load it at dock.
* Order completes.
* Basic score/result appears.
* Code is committed cleanly.
* Documentation exists in `docs/`.
