"""
Stack Up! — low-poly warehouse modules + props generator (Blender 5 / bpy).

Builds the environment and prop assets (Section 10) and exports each to
Blender/exports/ as GLB + FBX. Modules are built so they tile/snap on a 2 m grid.

Run:
    python3 Blender/scripts/stackup_warehouse_blender5.py
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import stackup_buildlib as lib


def _m():
    return {
        "floor": lib.material("Floor", (0.52, 0.53, 0.56)),
        "wall": lib.material("Wall", (0.72, 0.73, 0.76)),
        "metal": lib.material("RackMetal", (0.28, 0.33, 0.45), metal=0.7, rough=0.4),
        "wood": lib.material("Wood", (0.60, 0.45, 0.25)),
        "card": lib.material("Cardboard", (0.78, 0.62, 0.40)),
        "tape": lib.material("Tape", (0.85, 0.78, 0.55)),
        "plastic": lib.material("Plastic", (0.20, 0.45, 0.70)),
        "yellow": lib.material("Paint", (0.95, 0.78, 0.10)),
        "white": lib.material("White", (0.92, 0.92, 0.92)),
        "screen": lib.material("Screen", (0.05, 0.07, 0.09)),
        "orange": lib.material("Orange", (1.0, 0.48, 0.08)),
        "blue": lib.material("ZoneBlue", (0.20, 0.45, 0.85)),
        "green": lib.material("ZoneGreen", (0.20, 0.70, 0.35)),
        "glass": lib.material("Glass", (0.45, 0.75, 0.85), rough=0.2),
    }


# --- environment ---------------------------------------------------------
def floor_tile():
    m = _m(); r = lib.empty("FloorTile")
    lib.box("Tile", (2.0, 2.0, 0.10), (0, 0, -0.05), m["floor"], r)


def wall_straight():
    m = _m(); r = lib.empty("WallStraight")
    lib.box("Wall", (2.0, 0.2, 2.4), (0, 0, 1.2), m["wall"], r)
    lib.box("Skirt", (2.0, 0.24, 0.2), (0, 0, 0.1), m["yellow"], r)


def wall_corner():
    m = _m(); r = lib.empty("WallCorner")
    lib.box("WallX", (2.0, 0.2, 2.4), (0, 0, 1.2), m["wall"], r)
    lib.box("WallZ", (0.2, 2.0, 2.4), (-0.9, 1.1, 1.2), m["wall"], r)


def wall_door():
    m = _m(); r = lib.empty("WallDoor")
    lib.box("PostL", (0.4, 0.25, 2.4), (-0.8, 0, 1.2), m["wall"], r)
    lib.box("PostR", (0.4, 0.25, 2.4), (0.8, 0, 1.2), m["wall"], r)
    lib.box("Lintel", (2.0, 0.25, 0.5), (0, 0, 2.15), m["wall"], r)
    lib.box("Door", (1.2, 0.06, 1.9), (0, 0, 0.95), m["metal"], r)  # roller door


def rack_bay():
    m = _m(); r = lib.empty("RackBay")
    for x in (-0.85, 0.85):
        for y in (-0.35, 0.35):
            lib.box(f"Upright_{x}_{y}", (0.1, 0.1, 2.4), (x, y, 1.2), m["metal"], r)
    for z in (0.8, 1.6, 2.35):
        lib.box(f"BeamF_{z}", (1.8, 0.08, 0.08), (0, -0.35, z), m["metal"], r)
        lib.box(f"BeamB_{z}", (1.8, 0.08, 0.08), (0, 0.35, z), m["metal"], r)


def rack_shelf():
    m = _m(); r = lib.empty("RackShelf")
    lib.box("Deck", (1.8, 0.8, 0.06), (0, 0, 0), m["metal"], r)


def rack_back_panel():
    m = _m(); r = lib.empty("RackBackPanel")
    lib.box("Panel", (1.8, 0.04, 2.4), (0, 0, 1.2), m["metal"], r)


def dock_lane_marker():
    m = _m(); r = lib.empty("DockLaneMarker")
    lib.box("Lane", (1.4, 3.0, 0.02), (0, 0, 0.011), m["yellow"], r)
    for x in (-0.6, 0.6):
        lib.box(f"Stripe_{x}", (0.08, 3.0, 0.03), (x, 0, 0.02), m["white"], r)


def verification_station():
    m = _m(); r = lib.empty("VerificationStation")
    lib.box("Base", (1.0, 0.7, 0.9), (0, 0, 0.45), m["orange"], r)
    lib.box("Counter", (1.1, 0.8, 0.08), (0, 0, 0.92), m["white"], r)
    lib.box("Post", (0.1, 0.1, 0.7), (0.35, -0.2, 1.25), m["metal"], r)
    lib.box("Screen", (0.5, 0.06, 0.36), (0.0, -0.2, 1.45), m["screen"], r)
    lib.box("Scanner", (0.18, 0.18, 0.1), (-0.35, -0.2, 1.1), m["metal"], r)


def receiving_zone():
    m = _m(); r = lib.empty("ReceivingZoneMarker")
    lib.box("Pad", (2.0, 2.0, 0.02), (0, 0, 0.011), m["blue"], r)
    lib.box("Border", (2.0, 2.0, 0.03), (0, 0, 0.005), m["white"], r)


def staging_zone():
    m = _m(); r = lib.empty("StagingZoneMarker")
    lib.box("Pad", (2.0, 2.0, 0.02), (0, 0, 0.011), m["green"], r)
    lib.box("Border", (2.0, 2.0, 0.03), (0, 0, 0.005), m["white"], r)


# --- props ---------------------------------------------------------------
def tote():
    m = _m(); r = lib.empty("Tote")
    lib.box("Floor", (0.5, 0.35, 0.04), (0, 0, 0.02), m["plastic"], r)
    lib.box("WallF", (0.5, 0.03, 0.22), (0, -0.16, 0.13), m["plastic"], r)
    lib.box("WallB", (0.5, 0.03, 0.22), (0, 0.16, 0.13), m["plastic"], r)
    lib.box("WallL", (0.03, 0.35, 0.22), (-0.235, 0, 0.13), m["plastic"], r)
    lib.box("WallR", (0.03, 0.35, 0.22), (0.235, 0, 0.13), m["plastic"], r)


def pallet():
    m = _m(); r = lib.empty("Pallet")
    for x in (-0.45, 0, 0.45):
        lib.box(f"Block_{x}", (0.12, 1.0, 0.10), (x, 0, 0.05), m["wood"], r)
    for x in (-0.5, -0.25, 0, 0.25, 0.5):
        lib.box(f"TopSlat_{x}", (0.16, 1.0, 0.04), (x, 0, 0.12), m["wood"], r)
    lib.box("BottomSlat", (1.1, 0.16, 0.04), (0, 0, 0.0), m["wood"], r)


def _box(name, s):
    m = _m(); r = lib.empty(name)
    lib.box("Box", (s, s, s), (0, 0, s * 0.5), m["card"], r)
    lib.box("Tape", (s * 0.18, s, s + 0.01), (0, 0, s * 0.5), m["tape"], r)


def box_small():
    _box("BoxSmall", 0.30)


def box_medium():
    _box("BoxMedium", 0.45)


def box_large():
    _box("BoxLarge", 0.60)


def bottle_case():
    m = _m(); r = lib.empty("BottleCase")
    lib.box("Crate", (0.5, 0.35, 0.22), (0, 0, 0.11), m["plastic"], r)
    i = 0
    for x in (-0.16, 0, 0.16):
        for y in (-0.09, 0.09):
            lib.cyl(f"Bottle_{i}", 0.05, 0.18, (x, y, 0.31), m["glass"], r, verts=10)
            i += 1


def bag():
    m = _m(); r = lib.empty("Bag")
    s = lib.sphere("Sack", 0.22, (0, 0, 0.22), m["card"], r, segs=12)
    s.scale = (1.0, 0.8, 1.2)
    lib.cyl("Tie", 0.06, 0.08, (0, 0, 0.42), m["tape"], r, verts=10)


ASSETS = [
    ("FloorTile", floor_tile),
    ("WallStraight", wall_straight),
    ("WallCorner", wall_corner),
    ("WallDoor", wall_door),
    ("RackBay", rack_bay),
    ("RackShelf", rack_shelf),
    ("RackBackPanel", rack_back_panel),
    ("DockLaneMarker", dock_lane_marker),
    ("VerificationStation", verification_station),
    ("ReceivingZoneMarker", receiving_zone),
    ("StagingZoneMarker", staging_zone),
    ("Tote", tote),
    ("Pallet", pallet),
    ("BoxSmall", box_small),
    ("BoxMedium", box_medium),
    ("BoxLarge", box_large),
    ("BottleCase", bottle_case),
    ("Bag", bag),
]

if __name__ == "__main__":
    lib.build_all(ASSETS)
