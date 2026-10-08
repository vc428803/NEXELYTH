"""NEXELYTH Floor Module V1 — Blender 4.x Python.
Run from Blender Scripting workspace. Outputs a .blend and .glb beside this script.
Geometry-first prototype: preview materials are NOT final PBR textures.
"""
import bpy
import math
import random
from pathlib import Path
from mathutils import Vector

SEED = 428803
TILE_SIZE = 2.0
GRID = 4
GAP = 0.018
OUT_DIR = Path(bpy.path.abspath('//')) if bpy.data.filepath else Path.home() / 'Desktop'
random.seed(SEED)

# Only clear objects in this temporary modeling scene.
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)


def material(name, color, roughness=0.92):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value = (*color, 1)
    bsdf.inputs['Roughness'].default_value = roughness
    return mat

stone = [
    material('Stone_ColdGray_A', (0.31, 0.34, 0.35)),
    material('Stone_ColdGray_B', (0.27, 0.30, 0.31)),
    material('Stone_ColdGray_C', (0.35, 0.37, 0.37)),
]
moss = material('Moss_DarkMuted', (0.13, 0.19, 0.13))
soil = material('Soil_Dark', (0.12, 0.12, 0.115))

# One stable square base; prevents visible holes and gives reliable collisions.
bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0, -0.055))
base = bpy.context.object
base.name = 'Floor_Base_Collision_2x2m'
base.dimensions = (TILE_SIZE, TILE_SIZE, 0.10)
bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
base.data.materials.append(soil)
base.hide_render = False

# Create irregular slabs as closed mesh solids, avoiding a toy-like bevel.
slabs = []
cell = TILE_SIZE / GRID
for row in range(GRID):
    for col in range(GRID):
        cx = -1 + (col + 0.5) * cell
        cy = -1 + (row + 0.5) * cell
        x0 = cx - cell/2 + GAP/2
        x1 = cx + cell/2 - GAP/2
        y0 = cy - cell/2 + GAP/2
        y1 = cy + cell/2 - GAP/2
        # Interior vertices only are displaced; outer tile remains exactly 2x2m.
        boundary = [
            (x0 + random.uniform(0, .028), y0 + random.uniform(0, .028)),
            (x1 - random.uniform(0, .028), y0 + random.uniform(0, .028)),
            (x1 - random.uniform(0, .028), y1 - random.uniform(0, .028)),
            (x0 + random.uniform(0, .028), y1 - random.uniform(0, .028)),
        ]
        # One extra point on each edge makes the silhouette less rectangular.
        points = []
        for k in range(4):
            a, b = boundary[k], boundary[(k+1)%4]
            points.append(a)
            t = random.uniform(.38, .62)
            mx, my = a[0]*(1-t)+b[0]*t, a[1]*(1-t)+b[1]*t
            inward = Vector((cx-mx, cy-my)).normalized() * random.uniform(.004, .022)
            points.append((mx+inward.x, my+inward.y))
        z = random.uniform(-.009, .009)
        thickness = random.uniform(.065, .105)
        verts = [(x,y,z) for x,y in points] + [(x,y,z-thickness) for x,y in points]
        n = len(points)
        faces = [tuple(range(n-1,-1,-1)), tuple(range(n,2*n))]
        faces += [(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
        mesh = bpy.data.meshes.new(f'SlabMesh_{row}_{col}')
        mesh.from_pydata(verts, [], faces)
        mesh.update()
        obj = bpy.data.objects.new(f'Floor_Slab_{row:02}_{col:02}', mesh)
        bpy.context.collection.objects.link(obj)
        obj.data.materials.append(random.choice(stone))
        slabs.append(obj)

# Subtle moss strips in a handful of internal seams only; preview geometry.
for i in range(5):
    horizontal = i % 2 == 0
    line = random.randint(1, GRID-1)
    if horizontal:
        px, py = random.uniform(-.65, .65), -1 + line*cell
        sx, sy = random.uniform(.08,.20), .009
    else:
        px, py = -1 + line*cell, random.uniform(-.65,.65)
        sx, sy = .009, random.uniform(.08,.20)
    bpy.ops.mesh.primitive_cube_add(size=1, location=(px,py,.002))
    patch = bpy.context.object
    patch.name = f'Moss_Preview_{i:02}'
    patch.dimensions = (sx, sy, .004)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    patch.data.materials.append(moss)

# Viewport setup: meter units and top-oblique inspection angle.
scene = bpy.context.scene
scene.unit_settings.system = 'METRIC'
scene.unit_settings.scale_length = 1.0
for area in bpy.context.screen.areas:
    if area.type == 'VIEW_3D':
        area.spaces.active.region_3d.view_distance = 4.7
        area.spaces.active.shading.type = 'MATERIAL'

OUT_DIR.mkdir(parents=True, exist_ok=True)
blend_path = OUT_DIR / 'NEXELYTH_Floor_V1.blend'
glb_path = OUT_DIR / 'NEXELYTH_Floor_V1.glb'
bpy.ops.wm.save_as_mainfile(filepath=str(blend_path))
bpy.ops.export_scene.gltf(filepath=str(glb_path), export_format='GLB')
print('DONE:', blend_path)
print('DONE:', glb_path)
print('Triangles:', sum(len(p.vertices)-2 for obj in bpy.context.scene.objects if obj.type=='MESH' for p in obj.data.polygons))
