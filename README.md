# 1620 - Unity Project

## Quick Start

### 1. Create Unity Project

```
Unity Hub → New Project → 3D (URP) → Name: "1620-Graybox"
```

**Requirements:**
- Unity 2022.3 LTS or newer
- Universal Render Pipeline (URP)

### 2. Copy Assets

Copy the contents of this folder into your Unity project:

```
Unity/Assets/Editor/    →  YourProject/Assets/Editor/
Unity/Assets/Scripts/   →  YourProject/Assets/Scripts/
Unity/Assets/Scenes/    →  YourProject/Assets/Scenes/
```

### 3. Generate Graybox Scene

1. Open Unity
2. Menu: **Tools → 1620 → Generate Graybox Scene**
3. Click "Generate Graybox Scene"
4. Save scene as `Scenes/Graybox_Ambush.unity`

### 4. Test the Scene

1. Add a simple character controller (or import Unity Starter Assets)
2. Run around the scene
3. Verify:
   - [ ] Camp feels right size (10-15 people)
   - [ ] Treeline provides cover
   - [ ] River is wide enough to feel dangerous
   - [ ] Sprint times match spec (see below)

## Sprint Timing Targets

| Route | Target Time |
|-------|-------------|
| Treeline → Camp edge | 5-8 seconds |
| Camp center → River | 4-6 seconds |
| Across river | 8-10 seconds |
| Full escape | 15-20 seconds |

## Scene Hierarchy

```
Graybox_Ambush (Scene)
├── Environment
│   ├── Ground
│   ├── River
│   ├── Riverbank
│   └── Trees
│       └── Tree_001 ... Tree_080
├── Camp
│   ├── Tent_01 ... Tent_04
│   ├── Campfire
│   │   └── CampfireLight
│   └── Canoe_01, Canoe_02
├── SpawnPoints
│   ├── Spawn_Trapper_01 ... 04
│   ├── Spawn_Native_01 ... 08
│   └── EscapePoint_Canoes
└── Lighting
    └── Directional Light
```

## Next Steps

After graybox is approved:

1. **Add Character Controller**
   - Option A: Unity Starter Assets (free)
   - Option B: Invector Melee Template ($60)

2. **Test Movement Feel**
   - Sprint speed
   - Turn rate
   - Stamina (if using)

3. **Add Placeholder Combat**
   - Raycast for bow/gun
   - Trigger collider for melee

4. **Import Environment Art**
   - NatureManufacture forests
   - Replace primitives piece by piece

## Controls (Planned)

| Input | Action |
|-------|--------|
| WASD | Move |
| Shift | Sprint |
| Space | Jump/Vault |
| LMB | Attack/Fire |
| RMB | Aim/Block |
| R | Reload (rifle) |
| E | Interact |
| Tab | Inventory |

## Art Replacement Order

When replacing graybox with real assets:

1. **Ground/Terrain** - biggest visual impact
2. **Trees** - defines the space
3. **River/Water** - sets the mood
4. **Fog/Lighting** - atmosphere
5. **Camp props** - detail
6. **Characters** - last (use capsules until then)

---

*See /docs/GRAYBOX_SPEC.md for full specifications*
