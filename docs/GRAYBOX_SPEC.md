# Graybox Spec: "The Ambush" Scene

## Overview

Block out the Revenant opening scene using Unity primitives. No art, no gameplay - just geometry to nail scale and flow.

**Time estimate:** 1-2 days
**Engine:** Unity 2022.3+ LTS
**Scene name:** `Scenes/Graybox_Ambush.unity`

---

## Scene Dimensions

Based on real-world frontier camp scale and gameplay needs:

| Element | Size (Unity units = meters) |
|---------|----------------------------|
| **Total playable area** | 200m x 150m |
| **Camp clearing** | 40m x 30m |
| **River width** | 20m |
| **Treeline depth** | 30m+ (fades to boundary) |
| **Visibility range** | ~100m (fog will limit) |

```
200m
◄─────────────────────────────────────────────►

▲   ┌─────────────────────────────────────────┐
│   │░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░│  ░ = Treeline (30m deep)
│   │░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░│
│   │                                         │
│   │         ┌───────────────┐               │
│   │         │               │               │
150m│         │  CAMP (40x30) │               │
│   │         │               │               │
│   │         └───────────────┘               │
│   │                                         │
│   │▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓│  ▓ = Riverbank
│   │═════════════════════════════════════════│  ═ = River (20m wide)
▼   └─────────────────────────────────────────┘
```

---

## Unity Setup

### 1. Create Project
```
Unity Hub → New Project → 3D (URP) → "1620-Graybox"
```

### 2. Scene Hierarchy

```
Graybox_Ambush (Scene)
├── Environment
│   ├── Ground
│   ├── River
│   ├── Riverbank
│   └── Trees (parent)
│       ├── Tree_01
│       ├── Tree_02
│       └── ... (50-100 cylinders)
├── Camp
│   ├── Tent_01
│   ├── Tent_02
│   ├── Tent_03
│   ├── Campfire
│   ├── Canoe_01
│   └── Canoe_02
├── SpawnPoints
│   ├── Spawn_Trapper_01
│   ├── Spawn_Trapper_02
│   ├── Spawn_Native_01
│   └── Spawn_Native_02
├── Lighting
│   ├── Directional Light (dawn angle)
│   └── Campfire Light (point, orange)
└── Camera
    └── Main Camera (or Cinemachine)
```

---

## Primitives Reference

### Ground
- **Type:** Plane
- **Scale:** (200, 1, 150)
- **Material:** Dark green (unlit)
- **Position:** (0, 0, 0)

### River
- **Type:** Plane
- **Scale:** (200, 1, 20)
- **Material:** Blue (unlit)
- **Position:** (0, -0.5, -55) — slightly below ground
- **Note:** Player should "splash" into it (future: water physics)

### Riverbank
- **Type:** Cube, stretched and rotated
- **Scale:** (200, 2, 10)
- **Rotation:** (15, 0, 0) — sloped
- **Material:** Brown (unlit)
- **Position:** (0, 0, -40)

### Trees
- **Type:** Cylinder (trunk) + Sphere (canopy)
- **Trunk:** Scale (1, 8, 1), brown material
- **Canopy:** Scale (6, 6, 6), dark green material
- **Position canopy:** 6m above trunk base
- **Placement:** Random scatter in treeline zones, ~50-100 trees
- **Spacing:** 3-8m apart (dense enough for cover)

```csharp
// Quick tree scatter script
for (int i = 0; i < 80; i++) {
    float x = Random.Range(-90f, 90f);
    float z = Random.Range(50f, 75f); // North treeline
    SpawnTree(new Vector3(x, 0, z));
}
```

### Tents
- **Type:** Cube (stretched into wedge shape) or use ProBuilder
- **Scale:** (4, 2.5, 3)
- **Material:** Tan/beige (unlit)
- **Count:** 3-5 tents
- **Placement:** Clustered in camp center, irregular spacing

### Campfire
- **Type:** Cube (0.5, 0.3, 0.5) + Point Light
- **Light settings:**
  - Color: Orange (255, 150, 50)
  - Intensity: 2
  - Range: 15m
- **Position:** Center of camp

### Canoes
- **Type:** Cube (stretched)
- **Scale:** (1.5, 0.5, 6)
- **Material:** Brown (unlit)
- **Count:** 2-3
- **Position:** Near riverbank, angled toward water

---

## Spawn Points

Use empty GameObjects with Gizmo icons:

### Trapper Spawns (Defenders)
- Inside or near tents
- 4-6 spawn points
- Tag: `SpawnTrapper`

### Native Spawns (Attackers)
- In treeline, hidden
- 8-10 spawn points (outnumber trappers)
- Tag: `SpawnNative`

### Escape Points
- At canoes
- At river edge
- Tag: `EscapePoint`

---

## Lighting Setup

**Time of day:** Dawn (The Revenant ambush)

### Directional Light (Sun)
- **Rotation:** (15, -30, 0) — low sun, long shadows
- **Color:** Warm orange (255, 220, 180)
- **Intensity:** 0.8

### Ambient
- **Source:** Gradient
- **Sky:** Dark blue
- **Equator:** Purple/gray
- **Ground:** Dark brown

### Fog (Critical for atmosphere)
- **Enable:** Yes
- **Mode:** Exponential Squared
- **Density:** 0.015
- **Color:** Light gray/blue (180, 190, 200)

---

## Testing Checklist

Run around the graybox and verify:

### Scale
- [ ] Camp feels like 10-15 people could live here
- [ ] River feels dangerous to cross (wide enough)
- [ ] Treeline feels threatening (can't see through easily)
- [ ] Sprint from trees to camp takes 5-8 seconds

### Sightlines
- [ ] Attackers can approach unseen through trees
- [ ] Defenders have clear sight within camp
- [ ] River provides escape but you're exposed crossing it
- [ ] Some trees provide cover during retreat

### Flow
- [ ] Natural choke points exist
- [ ] Multiple attack routes through trees
- [ ] Escape to river is obvious but risky
- [ ] Camp layout creates interesting combat spaces

### Timing (with basic character controller)
- [ ] Tree cover → Camp edge: 5-8 seconds sprint
- [ ] Camp center → River: 4-6 seconds sprint
- [ ] Across river: 8-10 seconds (swimming/wading)
- [ ] Full escape (camp → far riverbank): 15-20 seconds

---

## Success Criteria

The graybox is DONE when:

1. **You can walk around** and it "feels" like a frontier camp
2. **Attackers have concealment** in the treeline
3. **Defenders have reaction time** but are surrounded
4. **Escape is possible** but dangerous
5. **Scale matches** real-world (person is ~2m tall, trees ~10-15m)
6. **A screenshot looks like** a plausible game level (even as boxes)

---

## Next Steps After Graybox

1. **Add character controller** (Invector or Unity Starter Assets)
2. **Test movement feel** - sprint, walk, turn speed
3. **Block out combat ranges** - bow range, rifle range, melee range
4. **Add placeholder enemies** - capsules that stand in spawn points
5. **Record video** of a "playthrough" running the escape route

---

## Reference Images

Study these for scale and layout:

- The Revenant (2015) - opening ambush scene
- Frontier camp paintings by Alfred Jacob Miller
- Lewis & Clark expedition camp recreations
- Red Dead Redemption 2 trapper camps

---

*Document created: December 2024*
