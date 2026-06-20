"""
Shared low-poly model-building helpers for the Stack Up! Blender pipeline.

Runs under both the standalone `bpy` pip module (`python3 script.py`) and inside
Blender (`blender --background --python script.py`). Exports GLB (modern, compact)
and FBX (imports natively into Unity).

See docs/ART_PIPELINE.md.
"""
import bpy
import os
import math
import addon_utils


def export_dir():
    here = os.path.dirname(os.path.abspath(__file__))
    d = os.path.abspath(os.path.join(here, "..", "exports"))
    os.makedirs(d, exist_ok=True)
    return d


def reset_scene():
    """Empty the scene and purge orphan data so each asset starts clean."""
    if bpy.context.object and bpy.context.object.mode != "OBJECT":
        bpy.ops.object.mode_set(mode="OBJECT")
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for coll in (bpy.data.meshes, bpy.data.materials, bpy.data.objects):
        for block in list(coll):
            if block.users == 0:
                coll.remove(block)


def material(name, color, rough=0.65, metal=0.0):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = (color[0], color[1], color[2], 1.0)
        if "Roughness" in bsdf.inputs:
            bsdf.inputs["Roughness"].default_value = rough
        if "Metallic" in bsdf.inputs:
            bsdf.inputs["Metallic"].default_value = metal
    return m


def _finish(obj, mat, parent):
    obj.data.materials.clear()
    obj.data.materials.append(mat)
    _bevel(obj)
    if parent:
        obj.parent = parent
    return obj


def _bevel(obj, width=0.012, segments=1):
    """Soft low-poly edges — much nicer silhouette than hard 90-degree cubes."""
    try:
        m = obj.modifiers.new(name="Bevel", type="BEVEL")
        m.width = width
        m.segments = segments
        m.clamp_overlap = True
        m.limit_method = "ANGLE"
        m.angle_limit = math.radians(40)
    except Exception:
        pass


def box(name, size, loc, mat, parent=None):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=loc)
    o = bpy.context.active_object
    o.name = name
    o.scale = (size[0], size[1], size[2])
    return _finish(o, mat, parent)


def cyl(name, radius, depth, loc, mat, parent=None, verts=16, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=radius, depth=depth, location=loc)
    o = bpy.context.active_object
    o.name = name
    o.rotation_euler = (math.radians(rot[0]), math.radians(rot[1]), math.radians(rot[2]))
    return _finish(o, mat, parent)


def sphere(name, radius, loc, mat, parent=None, segs=12):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segs, ring_count=max(4, segs // 2), radius=radius, location=loc)
    o = bpy.context.active_object
    o.name = name
    return _finish(o, mat, parent)


def empty(name, loc=(0, 0, 0)):
    e = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(e)
    e.location = loc
    return e


def _ensure_exporters():
    for addon in ("io_scene_fbx", "io_scene_gltf2"):
        try:
            addon_utils.enable(addon, default_set=False, persistent=True)
        except Exception:
            pass


def export(name):
    """Selects everything and writes <name>.glb and <name>.fbx into Blender/exports/."""
    _ensure_exporters()
    out = export_dir()
    bpy.ops.object.select_all(action="SELECT")

    glb = os.path.join(out, name + ".glb")
    bpy.ops.export_scene.gltf(filepath=glb, export_format="GLB", use_selection=True, export_apply=True)
    print("  exported", glb)

    try:
        fbx = os.path.join(out, name + ".fbx")
        bpy.ops.export_scene.fbx(filepath=fbx, use_selection=True, apply_unit_scale=True,
                                 bake_space_transform=True, use_mesh_modifiers=True)
        print("  exported", fbx)
    except Exception as e:  # FBX exporter not available in this bpy build
        print("  (fbx export skipped:", e, ")")


def build_all(assets):
    """assets: list of (name, builder_fn). Builds and exports each in a clean scene."""
    for name, builder in assets:
        print("building", name)
        reset_scene()
        builder()
        export(name)
    print("done:", len(assets), "assets ->", export_dir())
