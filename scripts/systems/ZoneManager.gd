extends Node
class_name ZoneManager
## Manages map zones and faction territories

enum Zone {
	TRAPPER_SAFE,      # No PvP, Trapper spawn
	TRAPPER_TERRITORY, # PvP with Trapper advantage
	CONTESTED,         # Full PvP, no advantage
	NATIVE_TERRITORY,  # PvP with Native advantage
	NATIVE_SAFE        # No PvP, Native spawn
}

# Map dimensions (must match SymmetricMapGenerator)
const MAP_DEPTH: float = 400.0
const SAFE_ZONE_DEPTH: float = 40.0
const TERRITORY_DEPTH: float = 80.0

signal zone_changed(player: Node, old_zone: Zone, new_zone: Zone)
signal entered_safe_zone(player: Node, faction: String)
signal entered_contested(player: Node)

## Get zone at world position
static func get_zone_at(pos: Vector3) -> Zone:
	var z = pos.z
	var half_depth = MAP_DEPTH / 2.0

	# South to North
	if z < -half_depth + SAFE_ZONE_DEPTH:
		return Zone.TRAPPER_SAFE
	elif z < -half_depth + SAFE_ZONE_DEPTH + TERRITORY_DEPTH:
		return Zone.TRAPPER_TERRITORY
	elif z > half_depth - SAFE_ZONE_DEPTH:
		return Zone.NATIVE_SAFE
	elif z > half_depth - SAFE_ZONE_DEPTH - TERRITORY_DEPTH:
		return Zone.NATIVE_TERRITORY
	else:
		return Zone.CONTESTED

## Check if zone allows PvP
static func is_pvp_zone(zone: Zone) -> bool:
	return zone not in [Zone.TRAPPER_SAFE, Zone.NATIVE_SAFE]

## Get faction that owns a zone (or null for contested)
static func get_zone_owner(zone: Zone) -> String:
	match zone:
		Zone.TRAPPER_SAFE, Zone.TRAPPER_TERRITORY:
			return "trapper"
		Zone.NATIVE_SAFE, Zone.NATIVE_TERRITORY:
			return "native"
		_:
			return ""

## Check if player has home advantage in zone
static func has_home_advantage(zone: Zone, faction: String) -> bool:
	var owner = get_zone_owner(zone)
	return owner == faction and zone in [Zone.TRAPPER_TERRITORY, Zone.NATIVE_TERRITORY]

## Get damage multiplier for attacker in zone
static func get_damage_multiplier(zone: Zone, attacker_faction: String) -> float:
	if has_home_advantage(zone, attacker_faction):
		return 1.2  # 20% bonus in home territory
	return 1.0

## Get zone display name
static func get_zone_name(zone: Zone) -> String:
	match zone:
		Zone.TRAPPER_SAFE:
			return "Trapper Camp (Safe)"
		Zone.TRAPPER_TERRITORY:
			return "Trapper Territory"
		Zone.CONTESTED:
			return "Contested Grounds"
		Zone.NATIVE_TERRITORY:
			return "Native Territory"
		Zone.NATIVE_SAFE:
			return "Native Village (Safe)"
		_:
			return "Unknown"

## Get zone color for UI
static func get_zone_color(zone: Zone) -> Color:
	match zone:
		Zone.TRAPPER_SAFE:
			return Color(0.2, 0.4, 0.8)  # Blue
		Zone.TRAPPER_TERRITORY:
			return Color(0.3, 0.5, 0.9)
		Zone.CONTESTED:
			return Color(0.8, 0.2, 0.2)  # Red
		Zone.NATIVE_TERRITORY:
			return Color(0.3, 0.8, 0.3)  # Green
		Zone.NATIVE_SAFE:
			return Color(0.2, 0.7, 0.2)
		_:
			return Color.WHITE
