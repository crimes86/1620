# 1620 Core Game Loop

## Overview

Persistent server where Trappers and Natives coexist. Neither faction has an inherent advantage. Conflict emerges naturally over contested resources.

## Map Layout (Symmetric)

```
                        NORTH
    ════════════════════════════════════════════
    ║                                          ║
    ║   ┌─────────────────────────────────┐    ║
    ║   │      NATIVE VILLAGE             │    ║  NATIVE SAFE ZONE
    ║   │   (Spawn, Crafting, Storage)    │    ║  No PvP
    ║   └─────────────────────────────────┘    ║
    ║                                          ║
    ║   ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░    ║  NATIVE TERRITORY
    ║   ░░░ Dense Forest (Native Home) ░░░    ║  PvP enabled, Natives advantage
    ║   ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░    ║
    ║                                          ║
    ╠══════════════════════════════════════════╣
    ║                                          ║
    ║   ╔═════════════════════════════════╗    ║
    ║   ║    CONTESTED HUNTING GROUNDS    ║    ║  NEUTRAL ZONE
    ║   ║   Best resources, most danger   ║    ║  Full PvP, no advantage
    ║   ║   Beaver dams, elk herds, etc   ║    ║
    ║   ╚═════════════════════════════════╝    ║
    ║                                          ║
    ╠══════════════════════════════════════════╣
    ║                                          ║
    ║   ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓    ║  TRAPPER TERRITORY
    ║   ▓▓▓ Open Meadows (Trapper Home) ▓▓▓    ║  PvP enabled, Trappers advantage
    ║   ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓    ║
    ║                                          ║
    ║   ┌─────────────────────────────────┐    ║
    ║   │        TRAPPER CAMP             │    ║  TRAPPER SAFE ZONE
    ║   │   (Spawn, Crafting, Storage)    │    ║  No PvP
    ║   └─────────────────────────────────┘    ║
    ║   ═══════════════════════════════════    ║
    ║   ~~~~~~~~~~~ RIVER ~~~~~~~~~~~~~~~~~    ║  (Boundary / escape route)
    ════════════════════════════════════════════
                        SOUTH
```

## Player Progression Loop

### Phase 1: SPAWN (0-2 min)
- Spawn at faction camp with basic gear
- **Trappers**: Knife, basic clothes, empty trap
- **Natives**: Tomahawk, leather clothes, bow (3 arrows)

### Phase 2: GATHER (2-10 min)
Safe-ish activities near camp:
- **Trappers**: Set traps near camp, chop wood, tend fire
- **Natives**: Hunt small game, gather herbs, craft arrows

Resources near camps are **low-tier** but **safe**:
- Rabbits, squirrels (low value pelts)
- Basic materials (wood, fiber, stone)

### Phase 3: UPGRADE (5-15 min)
Use gathered resources to improve:
- Better weapons (iron knife → steel knife)
- Better traps (snare → leg-hold)
- Armor/clothing upgrades
- Consumables (food, medicine)

**Gate**: Player should feel "ready" after ~15 min of play

### Phase 4: VENTURE (15+ min)
Move toward contested grounds:
- Better resources (beaver, fox, deer)
- Higher risk (enemy players)
- Rewards scale with distance from camp

### Phase 5: ENGAGE
Encounters in contested zone:
- Spot enemy player
- Fight, flee, or negotiate?
- Winner takes loot
- Loser respawns at camp (some gear loss)

### Phase 6: RETURN
Bring loot back to camp:
- Process carcasses at skinning station
- Store pelts in cache
- Craft upgrades
- Sell to NPC vendor?

**Loop repeats** - each cycle player gets slightly stronger

---

## Anti-Griefing Mechanics

### 1. Safe Zones (No PvP)
- Each faction's camp is 100% safe
- Can't be attacked, can't attack
- Provides: Spawn, storage, crafting, vendors

### 2. Territory Advantage
In your own territory (outside safe zone):
- 20% damage bonus
- Faster movement
- Can see enemy outlines at range?
- NPCs/guards assist if nearby

### 3. Contested Zone Balance
- No faction bonuses
- Best resources
- Respawn timer increases with deaths
- "Revenge" mechanic: recently killed players harder to kill again?

### 4. Gear-Based Protection (Optional)
Players with very different "power levels":
- Strong player gets reduced rewards for killing weak
- Weak player loses less on death
- Encourages fighting equals

### 5. Raid Cooldowns
- After a raid on enemy camp (if we allow it):
  - 30-min cooldown before raiding again
  - Defenders get temporary buff
  - Alert goes out to all faction members

---

## Resource Distribution

| Zone | Resources | Risk | Reward |
|------|-----------|------|--------|
| **Faction Camp** | None (safety) | None | N/A |
| **Home Territory** | Low-tier (rabbit, squirrel, wood) | Low (home advantage) | Low |
| **Contested Grounds** | High-tier (beaver, fox, deer, elk) | High (full PvP) | High |
| **Enemy Territory** | Mid-tier + raid opportunities | Very High | Variable |

---

## Session Flow Example

### Trapper Session (30 min)
1. **0:00** - Spawn at Trapper Camp, grab basic gear
2. **0:05** - Set 2 snares near camp, chop some wood
3. **0:10** - Check traps, got a rabbit! Skin it
4. **0:12** - Craft leg-hold trap from iron scraps
5. **0:15** - Venture toward contested grounds
6. **0:18** - Spot beaver dam, set leg-hold trap
7. **0:20** - See Native player in distance, hide
8. **0:22** - They leave, check trap - got a beaver!
9. **0:25** - Heading back, ambushed by Native!
10. **0:26** - Fight! I win, take their fox pelt
11. **0:28** - Back at camp, process beaver, store pelts
12. **0:30** - Log off with progress saved

### Native Session (30 min)
1. **0:00** - Spawn at Native Village
2. **0:03** - Hunt rabbits with bow near village
3. **0:08** - Craft more arrows from gathered materials
4. **0:12** - Prepare for hunting expedition
5. **0:15** - Move toward contested hunting grounds
6. **0:18** - Track deer, line up shot...
7. **0:19** - Hit! Deer wounded, chase it
8. **0:21** - Finish deer, start field dressing
9. **0:23** - Trapper player spots me, attacks!
10. **0:24** - I'm killed, respawn at village
11. **0:25** - Lost the deer hide, keep my bow
12. **0:28** - Quick hunt near village to recover
13. **0:30** - Log off, ready to try again

---

## Key Design Pillars

1. **Symmetric Factions** - Neither side has mechanical advantage
2. **Progression Gates** - Can't just rush enemy camp immediately
3. **Risk/Reward Scaling** - Better loot = more danger
4. **Session-Friendly** - Meaningful progress in 30-60 min
5. **Emergent Conflict** - PvP happens naturally, not forced
6. **Persistent World** - Your traps stay when you log off

---

## Open Questions

1. **Permadeath vs Respawn?** - Lean toward respawn with gear loss
2. **Offline Raiding?** - Can your camp be attacked when offline?
3. **Faction Balance?** - What if 10 Trappers vs 2 Natives?
4. **Time of Day?** - Real-time or accelerated? (affects hunting)
5. **Storage Limits?** - Unlimited hoarding or carry capacity?

---

*Document created: January 2026*
