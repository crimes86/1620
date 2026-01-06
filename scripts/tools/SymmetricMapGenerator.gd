@tool
extends EditorScript
## Symmetric Map Generator for 1620
## Run via: Script Editor -> File -> Run (Ctrl+Shift+X)
##
## Creates a symmetric faction-based PvP map:
## - Trapper Camp (south)
## - Native Village (north)
## - Contested Hunting Grounds (center)

# Map dimensions (meters)
const MAP_WIDTH: float = 300.0
const MAP_DEPTH: float = 400.0

# Zone depths (from each edge)
const SAFE_ZONE_DEPTH: float = 40.0      # No PvP
const TERRITORY_DEPTH: float = 80.0       # Home advantage
const CONTESTED_DEPTH: float = 160.0      # Center (what's left)

# Environment
const TREE_DENSITY: int = 120
const RIVER_WIDTH: float = 15.0

func _run() -> void:
	print("=== 1620 Symmetric Map Generator ===")
	print("Creating faction-based PvP map...")

	var root = get_scene().get_root() if get_scene() else null
	if not root:
		push_error("No scene open! Create a new 3D scene first.")
		return

	# Clear existing
	_clear_children(root)

	# Create zone structure
	var zones = _create_node(root, "Zones", Node3D)
	var environment = _create_node(root, "Environment", Node3D)
	var trapper_camp = _create_node(root, "TrapperCamp", Node3D)
	var native_village = _create_node(root, "NativeVillage", Node3D)
	var contested = _create_node(root, "ContestedGrounds", Node3D)
	var spawn_points = _create_node(root, "SpawnPoints", Node3D)
	var lighting = _create_node(root, "Lighting", Node3D)

	# Build the world
	_create_ground(environment)
	_create_zone_markers(zones)
	_create_river(environment)
	_create_trapper_camp(trapper_camp)
	_create_native_village(native_village)
	_create_contested_grounds(contested)
	_create_forests(environment)
	_create_spawn_points(spawn_points)
	_setup_lighting(lighting)

	print("Symmetric map generated!")
	print("- Trapper Camp: South (Z: %.0f to %.0f)" % [-MAP_DEPTH/2, -MAP_DEPTH/2 + SAFE_ZONE_DEPTH])
	print("- Native Village: North (Z: %.0f to %.0f)" % [MAP_DEPTH/2 - SAFE_ZONE_DEPTH, MAP_DEPTH/2])
	print("- Contested: Center")

func _create_node(parent: Node, name: String, type: Variant) -> Node:
	var node = type.new()
	node.name = name
	parent.add_child(node)
	node.owner = get_scene().get_root()
	return node

func _clear_children(node: Node) -> void:
	for child in node.get_children():
		if child.name in ["Zones", "Environment", "TrapperCamp", "NativeVillage",
						  "ContestedGrounds", "SpawnPoints", "Lighting"]:
			child.queue_free()

func _create_ground(parent: Node) -> void:
	var ground = CSGBox3D.new()
	ground.name = "Ground"
	ground.size = Vector3(MAP_WIDTH, 0.5, MAP_DEPTH)
	ground.position = Vector3(0, -0.25, 0)

	var mat = StandardMaterial3D.new()
	mat.albedo_color = Color(0.25, 0.35, 0.2)  # Forest green
	ground.material = mat

	parent.add_child(ground)
	ground.owner = get_scene().get_root()

func _create_zone_markers(parent: Node) -> void:
	# Visual zone boundaries (transparent planes)
	var zone_colors = {
		"TrapperSafe": Color(0.2, 0.4, 0.8, 0.15),      # Blue
		"TrapperTerritory": Color(0.2, 0.4, 0.8, 0.08),
		"Contested": Color(0.8, 0.2, 0.2, 0.1),          # Red
		"NativeTerritory": Color(0.2, 0.8, 0.2, 0.08),   # Green
		"NativeSafe": Color(0.2, 0.8, 0.2, 0.15),
	}

	# Trapper Safe Zone (south)
	_create_zone_plane(parent, "TrapperSafeZone",
		Vector3(0, 0.1, -MAP_DEPTH/2 + SAFE_ZONE_DEPTH/2),
		Vector3(MAP_WIDTH, 0.1, SAFE_ZONE_DEPTH),
		zone_colors["TrapperSafe"])

	# Trapper Territory
	_create_zone_plane(parent, "TrapperTerritory",
		Vector3(0, 0.1, -MAP_DEPTH/2 + SAFE_ZONE_DEPTH + TERRITORY_DEPTH/2),
		Vector3(MAP_WIDTH, 0.1, TERRITORY_DEPTH),
		zone_colors["TrapperTerritory"])

	# Contested Grounds (center)
	var contested_start = -MAP_DEPTH/2 + SAFE_ZONE_DEPTH + TERRITORY_DEPTH
	var contested_size = MAP_DEPTH - 2 * (SAFE_ZONE_DEPTH + TERRITORY_DEPTH)
	_create_zone_plane(parent, "ContestedZone",
		Vector3(0, 0.1, contested_start + contested_size/2),
		Vector3(MAP_WIDTH, 0.1, contested_size),
		zone_colors["Contested"])

	# Native Territory
	_create_zone_plane(parent, "NativeTerritory",
		Vector3(0, 0.1, MAP_DEPTH/2 - SAFE_ZONE_DEPTH - TERRITORY_DEPTH/2),
		Vector3(MAP_WIDTH, 0.1, TERRITORY_DEPTH),
		zone_colors["NativeTerritory"])

	# Native Safe Zone (north)
	_create_zone_plane(parent, "NativeSafeZone",
		Vector3(0, 0.1, MAP_DEPTH/2 - SAFE_ZONE_DEPTH/2),
		Vector3(MAP_WIDTH, 0.1, SAFE_ZONE_DEPTH),
		zone_colors["NativeSafe"])

func _create_zone_plane(parent: Node, name: String, pos: Vector3, size: Vector3, color: Color) -> void:
	var zone = CSGBox3D.new()
	zone.name = name
	zone.size = size
	zone.position = pos

	var mat = StandardMaterial3D.new()
	mat.albedo_color = color
	mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	zone.material = mat

	parent.add_child(zone)
	zone.owner = get_scene().get_root()

func _create_river(parent: Node) -> void:
	# River runs east-west through contested zone
	var river = CSGBox3D.new()
	river.name = "River"
	river.size = Vector3(MAP_WIDTH, 0.3, RIVER_WIDTH)
	river.position = Vector3(0, -0.2, 0)  # Center of map

	var mat = StandardMaterial3D.new()
	mat.albedo_color = Color(0.2, 0.4, 0.6)
	mat.metallic = 0.3
	mat.roughness = 0.2
	river.material = mat

	parent.add_child(river)
	river.owner = get_scene().get_root()

	# Riverbanks
	for side in [-1, 1]:
		var bank = CSGBox3D.new()
		bank.name = "Riverbank_%s" % ("South" if side < 0 else "North")
		bank.size = Vector3(MAP_WIDTH, 1.5, 6.0)
		bank.position = Vector3(0, 0.3, side * (RIVER_WIDTH/2 + 3))
		bank.rotation_degrees.x = side * 15.0

		var bank_mat = StandardMaterial3D.new()
		bank_mat.albedo_color = Color(0.3, 0.25, 0.15)
		bank.material = bank_mat

		parent.add_child(bank)
		bank.owner = get_scene().get_root()

func _create_trapper_camp(parent: Node) -> void:
	var camp_z = -MAP_DEPTH/2 + SAFE_ZONE_DEPTH/2

	# Campfire (center)
	_create_campfire(parent, Vector3(0, 0, camp_z))

	# Tents in semicircle
	var tent_mat = StandardMaterial3D.new()
	tent_mat.albedo_color = Color(0.7, 0.6, 0.5)

	for i in range(4):
		var angle = PI + (float(i) / 3.0) * PI  # Semicircle facing north
		var radius = 15.0
		var pos = Vector3(cos(angle) * radius, 1.25, camp_z + sin(angle) * radius * 0.5)

		var tent = CSGBox3D.new()
		tent.name = "Tent_%02d" % (i + 1)
		tent.size = Vector3(4.0, 2.5, 3.0)
		tent.position = pos
		tent.rotation_degrees.y = rad_to_deg(angle) + 90
		tent.material = tent_mat

		parent.add_child(tent)
		tent.owner = get_scene().get_root()

	# Skinning station
	_create_station(parent, "SkinningStation", Vector3(-20, 0, camp_z + 5), Color(0.5, 0.3, 0.2))

	# Crafting table
	_create_station(parent, "CraftingTable", Vector3(20, 0, camp_z + 5), Color(0.4, 0.35, 0.25))

	# Storage cache
	_create_station(parent, "StorageCache", Vector3(0, 0, camp_z - 10), Color(0.35, 0.3, 0.25))

func _create_native_village(parent: Node) -> void:
	var village_z = MAP_DEPTH/2 - SAFE_ZONE_DEPTH/2

	# Central fire pit
	_create_campfire(parent, Vector3(0, 0, village_z))

	# Wigwams/Longhouses in semicircle
	var structure_mat = StandardMaterial3D.new()
	structure_mat.albedo_color = Color(0.5, 0.4, 0.3)

	for i in range(4):
		var angle = (float(i) / 3.0) * PI  # Semicircle facing south
		var radius = 15.0
		var pos = Vector3(cos(angle) * radius, 1.5, village_z + sin(angle) * radius * 0.5)

		# Wigwam (dome shape approximated with sphere)
		var wigwam = CSGSphere3D.new()
		wigwam.name = "Wigwam_%02d" % (i + 1)
		wigwam.radius = 3.0
		wigwam.position = pos
		wigwam.material = structure_mat

		parent.add_child(wigwam)
		wigwam.owner = get_scene().get_root()

	# Drying rack
	_create_station(parent, "DryingRack", Vector3(-20, 0, village_z - 5), Color(0.45, 0.35, 0.25))

	# Medicine lodge
	_create_station(parent, "MedicineLodge", Vector3(20, 0, village_z - 5), Color(0.4, 0.3, 0.2))

	# Storage pit
	_create_station(parent, "StoragePit", Vector3(0, 0, village_z + 10), Color(0.3, 0.25, 0.2))

func _create_station(parent: Node, name: String, pos: Vector3, color: Color) -> void:
	var station = CSGBox3D.new()
	station.name = name
	station.size = Vector3(3.0, 1.5, 2.0)
	station.position = pos + Vector3(0, 0.75, 0)

	var mat = StandardMaterial3D.new()
	mat.albedo_color = color
	station.material = mat

	parent.add_child(station)
	station.owner = get_scene().get_root()

func _create_campfire(parent: Node, pos: Vector3) -> void:
	var fire_node = Node3D.new()
	fire_node.name = "Campfire"
	fire_node.position = pos
	parent.add_child(fire_node)
	fire_node.owner = get_scene().get_root()

	var base = CSGCylinder3D.new()
	base.name = "FirePit"
	base.radius = 1.5
	base.height = 0.3
	base.position = Vector3(0, 0.15, 0)

	var fire_mat = StandardMaterial3D.new()
	fire_mat.albedo_color = Color(0.8, 0.3, 0.1)
	fire_mat.emission_enabled = true
	fire_mat.emission = Color(1.0, 0.5, 0.2)
	fire_mat.emission_energy_multiplier = 2.0
	base.material = fire_mat

	fire_node.add_child(base)
	base.owner = get_scene().get_root()

	var light = OmniLight3D.new()
	light.name = "FireLight"
	light.position = Vector3(0, 2.0, 0)
	light.light_color = Color(1.0, 0.6, 0.3)
	light.light_energy = 2.0
	light.omni_range = 20.0

	fire_node.add_child(light)
	light.owner = get_scene().get_root()

func _create_contested_grounds(parent: Node) -> void:
	# Beaver dam
	_create_resource_point(parent, "BeaverDam", Vector3(-40, 0, 20), Color(0.4, 0.3, 0.2), 5.0)
	_create_resource_point(parent, "BeaverDam2", Vector3(50, 0, -30), Color(0.4, 0.3, 0.2), 4.0)

	# Elk grazing area marker
	_create_resource_point(parent, "ElkGrazing", Vector3(0, 0, 40), Color(0.35, 0.4, 0.25), 8.0)

	# Fox den
	_create_resource_point(parent, "FoxDen", Vector3(-60, 0, -10), Color(0.5, 0.35, 0.2), 3.0)

	# Deer trail marker
	_create_resource_point(parent, "DeerTrail", Vector3(30, 0, 0), Color(0.3, 0.35, 0.25), 2.0)

func _create_resource_point(parent: Node, name: String, pos: Vector3, color: Color, radius: float) -> void:
	var point = CSGCylinder3D.new()
	point.name = name
	point.radius = radius
	point.height = 0.2
	point.position = pos + Vector3(0, 0.1, 0)

	var mat = StandardMaterial3D.new()
	mat.albedo_color = color
	point.material = mat

	parent.add_child(point)
	point.owner = get_scene().get_root()

func _create_forests(parent: Node) -> void:
	var trees = Node3D.new()
	trees.name = "Trees"
	parent.add_child(trees)
	trees.owner = get_scene().get_root()

	var trunk_mat = StandardMaterial3D.new()
	trunk_mat.albedo_color = Color(0.4, 0.25, 0.15)

	var canopy_mat = StandardMaterial3D.new()
	canopy_mat.albedo_color = Color(0.15, 0.3, 0.1)

	var tree_idx = 0

	# Dense forest in Native territory (north)
	for i in range(TREE_DENSITY / 2):
		var x = randf_range(-MAP_WIDTH/2 + 10, MAP_WIDTH/2 - 10)
		var z = randf_range(MAP_DEPTH/2 - SAFE_ZONE_DEPTH - TERRITORY_DEPTH, MAP_DEPTH/2 - SAFE_ZONE_DEPTH - 5)
		_create_tree(trees, Vector3(x, 0, z), trunk_mat, canopy_mat, tree_idx)
		tree_idx += 1

	# Sparse trees in contested grounds
	for i in range(TREE_DENSITY / 4):
		var x = randf_range(-MAP_WIDTH/2 + 20, MAP_WIDTH/2 - 20)
		var z = randf_range(-CONTESTED_DEPTH/4, CONTESTED_DEPTH/4)
		# Avoid river
		if abs(z) > RIVER_WIDTH:
			_create_tree(trees, Vector3(x, 0, z), trunk_mat, canopy_mat, tree_idx)
			tree_idx += 1

	# Light woods in Trapper territory (south)
	for i in range(TREE_DENSITY / 4):
		var x = randf_range(-MAP_WIDTH/2 + 10, MAP_WIDTH/2 - 10)
		var z = randf_range(-MAP_DEPTH/2 + SAFE_ZONE_DEPTH + 5, -MAP_DEPTH/2 + SAFE_ZONE_DEPTH + TERRITORY_DEPTH)
		_create_tree(trees, Vector3(x, 0, z), trunk_mat, canopy_mat, tree_idx)
		tree_idx += 1

func _create_tree(parent: Node, pos: Vector3, trunk_mat: Material, canopy_mat: Material, index: int) -> void:
	var tree = Node3D.new()
	tree.name = "Tree_%03d" % (index + 1)
	tree.position = pos
	parent.add_child(tree)
	tree.owner = get_scene().get_root()

	var height = randf_range(6.0, 10.0)

	var trunk = CSGCylinder3D.new()
	trunk.name = "Trunk"
	trunk.radius = 0.4
	trunk.height = height
	trunk.position = Vector3(0, height/2, 0)
	trunk.material = trunk_mat
	tree.add_child(trunk)
	trunk.owner = get_scene().get_root()

	var canopy = CSGSphere3D.new()
	canopy.name = "Canopy"
	canopy.radius = randf_range(2.5, 4.0)
	canopy.position = Vector3(0, height + 1.5, 0)
	canopy.material = canopy_mat
	tree.add_child(canopy)
	canopy.owner = get_scene().get_root()

func _create_spawn_points(parent: Node) -> void:
	# Trapper spawns (in safe zone)
	var trapper_z = -MAP_DEPTH/2 + SAFE_ZONE_DEPTH/2
	for i in range(4):
		var spawn = Marker3D.new()
		spawn.name = "Spawn_Trapper_%02d" % (i + 1)
		spawn.position = Vector3(randf_range(-15, 15), 0, trapper_z + randf_range(-5, 5))
		spawn.set_meta("faction", "trapper")
		parent.add_child(spawn)
		spawn.owner = get_scene().get_root()

	# Native spawns (in safe zone)
	var native_z = MAP_DEPTH/2 - SAFE_ZONE_DEPTH/2
	for i in range(4):
		var spawn = Marker3D.new()
		spawn.name = "Spawn_Native_%02d" % (i + 1)
		spawn.position = Vector3(randf_range(-15, 15), 0, native_z + randf_range(-5, 5))
		spawn.set_meta("faction", "native")
		parent.add_child(spawn)
		spawn.owner = get_scene().get_root()

func _setup_lighting(parent: Node) -> void:
	var sun = DirectionalLight3D.new()
	sun.name = "Sun"
	sun.rotation_degrees = Vector3(-45, -30, 0)
	sun.light_color = Color(1.0, 0.95, 0.9)
	sun.light_energy = 1.0
	sun.shadow_enabled = true
	parent.add_child(sun)
	sun.owner = get_scene().get_root()

	var world_env = WorldEnvironment.new()
	world_env.name = "WorldEnvironment"
	parent.add_child(world_env)
	world_env.owner = get_scene().get_root()

	var env = Environment.new()
	env.background_mode = Environment.BG_COLOR
	env.background_color = Color(0.5, 0.6, 0.7)
	env.fog_enabled = true
	env.fog_light_color = Color(0.7, 0.75, 0.8)
	env.fog_density = 0.008
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color(0.4, 0.45, 0.5)
	env.ambient_light_energy = 0.6
	world_env.environment = env
