"""
Stack Up! — low-poly robot worker generator (Blender 5 / bpy).

Builds the three robot variants (Section 9) and exports them to
Blender/exports/ as GLB + FBX.

Run:
    python3 Blender/scripts/stackup_robots_blender5_beautified.py
  or in Blender:
    blender --background --python Blender/scripts/stackup_robots_blender5_beautified.py
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import stackup_buildlib as lib


def _common_mats():
    return {
        "body": lib.material("Body", (0.82, 0.84, 0.88)),
        "dark": lib.material("Dark", (0.12, 0.12, 0.14)),
        "screen": lib.material("Screen", (0.05, 0.07, 0.09)),
        "eye": lib.material("Eye", (0.25, 0.9, 1.0), rough=0.3),
        "tyre": lib.material("Tyre", (0.08, 0.08, 0.09)),
    }


def _face(parent, mats, eye_color=None):
    # screen panel + two glowing eyes, on the +Y face
    lib.box("Screen", (0.42, 0.05, 0.30), (0, 0.205, 0.78), mats["screen"], parent)
    eye = lib.material("EyeC", eye_color, rough=0.3) if eye_color else mats["eye"]
    lib.box("EyeL", (0.10, 0.04, 0.06), (-0.10, 0.235, 0.82), eye, parent)
    lib.box("EyeR", (0.10, 0.04, 0.06), (0.10, 0.235, 0.82), eye, parent)
    lib.box("Mouth", (0.16, 0.04, 0.03), (0, 0.235, 0.72), eye, parent)


def _wheeled_base(parent, mats):
    lib.cyl("Base", 0.30, 0.16, (0, 0, 0.10), mats["dark"], parent, verts=20)
    lib.cyl("WheelL", 0.12, 0.10, (-0.26, 0, 0.10), mats["tyre"], parent, verts=16, rot=(0, 90, 0))
    lib.cyl("WheelR", 0.12, 0.10, (0.26, 0, 0.10), mats["tyre"], parent, verts=16, rot=(0, 90, 0))


def picker_bot():
    m = _common_mats()
    vest = lib.material("HiVisYellow", (1.0, 0.83, 0.10))
    cap = lib.material("Cap", (0.20, 0.20, 0.24))
    root = lib.empty("PickerBot")

    _wheeled_base(root, m)
    lib.box("Torso", (0.50, 0.40, 0.62), (0, 0, 0.55), m["body"], root)
    lib.box("Vest", (0.54, 0.44, 0.34), (0, 0, 0.46), vest, root)
    lib.box("Head", (0.46, 0.42, 0.34), (0, 0, 0.80), m["body"], root)
    _face(root, m)
    lib.box("Cap", (0.50, 0.44, 0.10), (0, 0, 1.00), cap, root)
    lib.box("CapPeak", (0.30, 0.18, 0.04), (0, 0.28, 0.98), cap, root)
    lib.cyl("ArmL", 0.05, 0.42, (-0.30, 0, 0.55), m["body"], root, verts=10)
    lib.cyl("ArmR", 0.05, 0.42, (0.30, 0, 0.55), m["body"], root, verts=10)
    lib.cyl("Antenna", 0.015, 0.22, (0.16, 0, 1.16), m["dark"], root, verts=6)
    lib.sphere("AntennaTip", 0.04, (0.16, 0, 1.29), m["eye"], root, segs=8)


def supervisor_bot():
    m = _common_mats()
    vest = lib.material("HiVisOrange", (1.0, 0.48, 0.08))
    cap = lib.material("CapOrange", (0.85, 0.30, 0.08))
    tablet = lib.material("Tablet", (0.10, 0.12, 0.16))
    root = lib.empty("SupervisorBot")

    _wheeled_base(root, m)
    lib.box("Torso", (0.52, 0.42, 0.64), (0, 0, 0.56), m["body"], root)
    lib.box("Vest", (0.56, 0.46, 0.36), (0, 0, 0.46), vest, root)
    lib.box("Head", (0.46, 0.42, 0.34), (0, 0, 0.82), m["body"], root)
    _face(root, m, eye_color=(1.0, 0.7, 0.2))
    lib.box("Cap", (0.50, 0.44, 0.12), (0, 0, 1.02), cap, root)
    # tablet held out front on an arm
    lib.cyl("ArmR", 0.05, 0.40, (0.30, 0.10, 0.55), m["body"], root, verts=10, rot=(30, 0, 0))
    lib.box("Tablet", (0.26, 0.04, 0.18), (0.30, 0.34, 0.66), tablet, root)
    lib.cyl("ArmL", 0.05, 0.42, (-0.30, 0, 0.55), m["body"], root, verts=10)
    # small claw on left arm
    lib.box("Claw", (0.10, 0.10, 0.06), (-0.30, 0, 0.33), m["dark"], root)


def forklift_bot():
    m = _common_mats()
    vest = lib.material("HiVisGreen", (0.16, 0.62, 0.22))
    hardhat = lib.material("Hardhat", (0.10, 0.32, 0.80))
    goggles = lib.material("Goggles", (0.15, 0.18, 0.20), rough=0.2)
    forks = lib.material("Forks", (0.85, 0.70, 0.10), metal=0.6, rough=0.4)
    root = lib.empty("ForkliftBot")

    # bulkier tracked base
    lib.box("Base", (0.72, 0.62, 0.26), (0, 0, 0.16), m["dark"], root)
    lib.cyl("TrackL", 0.14, 0.66, (-0.34, 0, 0.16), m["tyre"], root, verts=18, rot=(0, 90, 0))
    lib.cyl("TrackR", 0.14, 0.66, (0.34, 0, 0.16), m["tyre"], root, verts=18, rot=(0, 90, 0))
    lib.box("Torso", (0.62, 0.50, 0.60), (0, 0, 0.62), m["body"], root)
    lib.box("Vest", (0.66, 0.54, 0.34), (0, 0, 0.52), vest, root)
    lib.box("Head", (0.48, 0.44, 0.34), (0, 0, 0.92), m["body"], root)
    _face(root, m, eye_color=(0.4, 1.0, 0.6))
    # hardhat (dome + brim) and goggles
    lib.sphere("Hardhat", 0.27, (0, 0, 1.10), hardhat, root, segs=12)
    lib.box("HatBrim", (0.50, 0.50, 0.05), (0, 0, 1.04), hardhat, root)
    lib.box("Goggles", (0.42, 0.06, 0.10), (0, 0.225, 0.96), goggles, root)
    # forklift mast + forks at the front
    lib.box("MastL", (0.06, 0.06, 0.9), (-0.18, 0.40, 0.45), m["dark"], root)
    lib.box("MastR", (0.06, 0.06, 0.9), (0.18, 0.40, 0.45), m["dark"], root)
    lib.box("ForkL", (0.08, 0.50, 0.05), (-0.18, 0.66, 0.06), forks, root)
    lib.box("ForkR", (0.08, 0.50, 0.05), (0.18, 0.66, 0.06), forks, root)


ASSETS = [
    ("PickerBot", picker_bot),
    ("SupervisorBot", supervisor_bot),
    ("ForkliftBot", forklift_bot),
]

if __name__ == "__main__":
    lib.build_all(ASSETS)
