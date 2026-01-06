# 1620 Project Guidelines

## Overview

**1620** is a 3D survival MMO set in 1620s colonial America - "The Revenant meets Rust, set during first contact."

Two playable factions:
- **Trappers** (European settlers) - Firearms, metal tools, fortifications
- **Natives** (Wampanoag Confederacy) - Land knowledge, tracking, mobility

## Current Phase: Greybox

Building "The Ambush" scene - a Revenant-style frontier camp raid scenario for scale/flow testing.

## Project Structure

```
1620/
├── project.godot              # Godot 4.x 3D project
├── scenes/
│   ├── main_menu.tscn         # Main menu
│   ├── greybox_ambush.tscn    # The Ambush greybox scene
│   └── player/
│       └── player.tscn        # First-person player
├── scripts/
│   ├── autoload/
│   │   ├── GameManager.gd     # Game state, factions
│   │   └── NetworkManager.gd  # Multiplayer (ENet)
│   ├── player/
│   │   └── FirstPersonController.gd
│   ├── ui/
│   │   └── MainMenu.gd
│   └── tools/
│       └── GrayboxGenerator.gd  # Editor tool to populate scene
└── docs/
    ├── GDD.md                 # Game Design Document
    └── GRAYBOX_SPEC.md        # Greybox specifications
```

## Scene Dimensions (The Ambush)

| Element | Size |
|---------|------|
| Total playable area | 200m x 150m |
| Camp clearing | 40m x 30m |
| River width | 20m |
| Treeline depth | 30m |

## Controls

| Input | Action |
|-------|--------|
| WASD | Move |
| Shift | Sprint |
| Space | Jump |
| Mouse | Look |
| Esc | Release cursor |
| LMB | Attack (future) |
| RMB | Aim (future) |
| E | Interact (future) |

## Networking

Server-authoritative architecture using Godot's built-in ENet:
- Default port: 7777
- Max players: 12 (4 trappers + 8 natives)
- Dedicated server mode: `--server` command line arg

## Sprint Timing Targets

| Route | Target Time |
|-------|-------------|
| Treeline → Camp edge | 5-8 seconds |
| Camp center → River | 4-6 seconds |
| Across river | 8-10 seconds |
| Full escape | 15-20 seconds |

## Development Commands

```bash
# Open in Godot
godot --path "C:\Users\kevin\OneDrive\1620" --editor

# Run greybox directly
godot --path "C:\Users\kevin\OneDrive\1620" res://scenes/greybox_ambush.tscn

# Run as dedicated server
godot --path "C:\Users\kevin\OneDrive\1620" --headless -- --server
```

## Key References

- **The Revenant (2015)** - Tone, survival, combat, opening ambush scene
- **Red Dead Redemption 2** - Atmosphere, hunting, period detail
- **Rust** - Survival, building, PvP tension

## Don't Commit Until Tested

Always test changes in Godot before committing.
