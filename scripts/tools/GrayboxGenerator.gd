@tool
extends EditorScript
## Greybox Generator for "The Ambush" scene
## Run via: Script Editor -> File -> Run (Ctrl+Shift+X)
##
## Generates the Revenant-style frontier camp ambush scene
## with primitives for scale/flow testing.

# Scene dimensions (meters)
const SCENE_WIDTH: float = 200.0
const SCENE_DEPTH: float = 150.0
const RIVER_WIDTH: float = 20.0
const TREELINE_DEPTH: float = 30.0
const CAMP_WIDTH: float = 40.0
const CAMP_DEPTH: float = 30.0
const TREE_COUNT: int = 80
const TENT_COUNT: int = 4

func _run() -> void:
	print("=== 1620 Greybox Generator ===")
	print("Generating 'The Ambush' scene...")

	var root = get_scene().get_root() if get_scene() else null
	if not root:
		push_error("No scene open! Create a new 3D scene first.")
		return

	# Create parent nodes
	var environment = _create_or_get_node(root, "Environment", Node3D)
	var camp = _create_or_get_node(root, "Camp", Node3D)
	var spawn_points = _create_or_get_node(root, "SpawnPoints", Node3D)
	var lighting = _create_or_get_node(root, "Lighting", Node3D)

	# Generate environment
	_create_ground(environment)
	_create_river(environment)
	_create_riverbank(environment)
	_create_trees(environment)

	# Generate camp
	_create_tents(camp)
	_create_campfire(camp)
	_create_canoes(camp)

	# Generate spawn points
	_create_trapper_spawns(spawn_points)
	_create_native_spawns(spawn_points)
	_create_escape_points(spawn_points)

	# Setup lighting
	_setup_lighting(lighting)

	print("Greybox scene generated successfully!")
	print("Add a Player scene and hit F5 to test!")

func _create_or_get_node(parent: Node, name: String, type: Variant) -> Node:
	var existing = parent.get_node_or_null(name)
	if existing:
		return existing

	var node = type.new()
	node.name = name
	parent.add_child(node)
	node.owner = get_scene().get_root()
	return node

func _create_ground(parent: Node) -> void:
	var ground = _create_or_get_node(parent, "Ground", CSGBox3D)
	ground.size = Vector3(SCENE_WIDTH, 0.5, SCENE_DEPTH)
	ground.position = Vector3(0, -0.25, 0)

	var mat = StandardMaterial3D.new()
	mat.albedo_color = Color(0.2, 0.35, 0.15)  # Dark green
	ground.material = mat

func _create_river(parent: Node) -> void:
	var river = _create_or_get_node(parent, "River", CSGBox3D)
	river.size = Vector3(SCENE_WIDTH, 0.3, RIVER_WIDTH)
	river.position = Vector3(0, -0.35, -SCENE_DEPTH / 2.0 + RIVER_WIDTH / 2.0)

	var mat = StandardMaterial3D.new()
	mat.albedo_color = Color(0.2, 0.4, 0.6)  # Blue
	mat.metallic = 0.3
	mat.roughness = 0.2
	river.material = mat

func _create_riverbank(parent: Node) -> void:
	var bank = _create_or_get_node(parent, "Riverbank", CSGBox3D)
	bank.size = Vector3(SCENE_WIDTH, 2.0, 8.0)
	bank.position = Vector3(0, 0.5, -SCENE_DEPTH / 2.0 + RIVER_WIDTH + 4.0)
	bank.rotation_degrees.x = 15.0

	var mat = StandardMaterial3D.new()
	mat.albedo_color = Color(0.3, 0.25, 0.15)  # Brown
	bank.material = mat

func _create_trees(parent: Node) -> void:
	var trees = _create_or_get_node(parent, "Trees", Node3D)

	# Clear existing trees
	for child in trees.get_children():
		child.queue_free()

	var trunk_mat = StandardMaterial3D.new()
	trunk_mat.albedo_color = Color(0.4, 0.25, 0.15)

	var canopy_mat = StandardMaterial3D.new()
	canopy_mat.albedo_color = Color(0.15, 0.3, 0.1)

	# North treeline
	for i in range(TREE_COUNT):
		var x = randf_range(-SCENE_WIDTH / 2.0 + 10.0, SCENE_WIDTH / 2.0 - 10.0)
		var z = randf_range(SCENE_DEPTH / 2.0 - TREELINE_DEPTH, SCENE_DEPTH / 2.0 - 5.0)
		_create_tree(trees, Vector3(x, 0, z), trunk_mat, canopy_mat, i)

	# Side trees
	for i in range(TREE_COUNT / 4):
		# Left side
		var x_left = randf_range(-SCENE_WIDTH / 2.0, -SCENE_WIDTH / 2.0 + 20.0)
		var z_left = randf_range(-SCENE_DEPTH / 4.0, SCENE_DEPTH / 2.0 - TREELINE_DEPTH)
		_create_tree(trees, Vector3(x_left, 0, z_left), trunk_mat, canopy_mat, TREE_COUNT + i)

		# Right side
		var x_right = randf_range(SCENE_WIDTH / 2.0 - 20.0, SCENE_WIDTH / 2.0)
		var z_right = randf_range(-SCENE_DEPTH / 4.0, SCENE_DEPTH / 2.0 - TREELINE_DEPTH)
		_create_tree(trees, Vector3(x_right, 0, z_right), trunk_mat, canopy_mat, TREE_COUNT + TREE_COUNT / 4 + i)

func _create_tree(parent: Node, pos: Vector3, trunk_mat: Material, canopy_mat: Material, index: int) -> void:
	var tree = Node3D.new()
	tree.name = "Tree_%03d" % (index + 1)
	tree.position = pos
	parent.add_child(tree)
	tree.owner = get_scene().get_root()

	# Trunk (cylinder)
	var trunk = CSGCylinder3D.new()
	trunk.name = "Trunk"
	trunk.radius = 0.5
	trunk.height = 8.0
	trunk.position = Vector3(0, 4.0, 0)
	trunk.material = trunk_mat
	tree.add_child(trunk)
	trunk.owner = get_scene().get_root()

	# Canopy (sphere)
	var canopy = CSGSphere3D.new()
	canopy.name = "Canopy"
	canopy.radius = 3.0
	canopy.position = Vector3(0, 9.0, 0)
	canopy.material = canopy_mat
	tree.add_child(canopy)
	canopy.owner = get_scene().get_root()

func _create_tents(parent: Node) -> void:
	var tent_mat = StandardMaterial3D.new()
	tent_mat.albedo_color = Color(0.7, 0.6, 0.5)  # Tan

	var camp_center_z = 10.0  # Slightly north of center

	for i in range(TENT_COUNT):
		var tent = CSGBox3D.new()
		tent.name = "Tent_%02d" % (i + 1)
		tent.size = Vector3(4.0, 2.5, 3.0)
		tent.material = tent_mat

		var angle = (float(i) / float(TENT_COUNT)) * PI * 2.0
		var radius = CAMP_WIDTH / 4.0
		var x = cos(angle) * radius + randf_range(-3.0, 3.0)
		var z = sin(angle) * radius + camp_center_z + randf_range(-3.0, 3.0)

		tent.position = Vector3(x, 1.25, z)
		tent.rotation_degrees.y = randf_range(0, 360)

		parent.add_child(tent)
		tent.owner = get_scene().get_root()

func _create_campfire(parent: Node) -> void:
	var campfire = _create_or_get_node(parent, "Campfire", Node3D)
	campfire.position = Vector3(0, 0, 10.0)

	# Fire base
	var base = CSGBox3D.new()
	base.name = "FireBase"
	base.size = Vector3(1.0, 0.3, 1.0)
	base.position = Vector3(0, 0.15, 0)

	var fire_mat = StandardMaterial3D.new()
	fire_mat.albedo_color = Color(0.8, 0.3, 0.1)
	fire_mat.emission_enabled = true
	fire_mat.emission = Color(1.0, 0.5, 0.2)
	fire_mat.emission_energy_multiplier = 2.0
	base.material = fire_mat

	campfire.add_child(base)
	base.owner = get_scene().get_root()

	# Light
	var light = OmniLight3D.new()
	light.name = "FireLight"
	light.position = Vector3(0, 2.0, 0)
	light.light_color = Color(1.0, 0.6, 0.3)
	light.light_energy = 2.0
	light.omni_range = 15.0

	campfire.add_child(light)
	light.owner = get_scene().get_root()

func _create_canoes(parent: Node) -> void:
	var canoe_mat = StandardMaterial3D.new()
	canoe_mat.albedo_color = Color(0.5, 0.35, 0.2)  # Brown

	for i in range(2):
		var canoe = CSGBox3D.new()
		canoe.name = "Canoe_%02d" % (i + 1)
		canoe.size = Vector3(1.5, 0.5, 6.0)
		canoe.position = Vector3(-15.0 + i * 10.0, 0.25, -SCENE_DEPTH / 2.0 + RIVER_WIDTH + 12.0)
		canoe.rotation_degrees.y = -20.0 + randf_range(-10.0, 10.0)
		canoe.material = canoe_mat

		parent.add_child(canoe)
		canoe.owner = get_scene().get_root()

func _create_trapper_spawns(parent: Node) -> void:
	var camp_center_z = 10.0

	for i in range(4):
		var spawn = Marker3D.new()
		spawn.name = "Spawn_Trapper_%02d" % (i + 1)
		spawn.position = Vector3(
			randf_range(-CAMP_WIDTH / 4.0, CAMP_WIDTH / 4.0),
			0,
			camp_center_z + randf_range(-CAMP_DEPTH / 4.0, CAMP_DEPTH / 4.0)
		)
		spawn.set_meta("spawn_type", "trapper")

		parent.add_child(spawn)
		spawn.owner = get_scene().get_root()

func _create_native_spawns(parent: Node) -> void:
	for i in range(8):
		var spawn = Marker3D.new()
		spawn.name = "Spawn_Native_%02d" % (i + 1)
		spawn.position = Vector3(
			randf_range(-SCENE_WIDTH / 3.0, SCENE_WIDTH / 3.0),
			0,
			SCENE_DEPTH / 2.0 - randf_range(5.0, TREELINE_DEPTH - 5.0)
		)
		spawn.set_meta("spawn_type", "native")

		parent.add_child(spawn)
		spawn.owner = get_scene().get_root()

func _create_escape_points(parent: Node) -> void:
	var escape = Marker3D.new()
	escape.name = "EscapePoint_Canoes"
	escape.position = Vector3(-10.0, 0, -SCENE_DEPTH / 2.0 + RIVER_WIDTH + 10.0)
	escape.set_meta("spawn_type", "escape")

	parent.add_child(escape)
	escape.owner = get_scene().get_root()

func _setup_lighting(parent: Node) -> void:
	# Directional light (dawn sun)
	var sun = _create_or_get_node(parent, "Sun", DirectionalLight3D)
	sun.rotation_degrees = Vector3(-15, -30, 0)
	sun.light_color = Color(1.0, 0.85, 0.7)  # Warm orange
	sun.light_energy = 0.8
	sun.shadow_enabled = true

	# WorldEnvironment for fog
	var world_env = _create_or_get_node(parent, "WorldEnvironment", WorldEnvironment)

	var env = Environment.new()
	env.background_mode = Environment.BG_COLOR
	env.background_color = Color(0.4, 0.45, 0.5)

	# Fog
	env.fog_enabled = true
	env.fog_light_color = Color(0.7, 0.75, 0.8)
	env.fog_density = 0.015
	env.fog_aerial_perspective = 0.5

	# Ambient
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color(0.3, 0.35, 0.4)
	env.ambient_light_energy = 0.5

	world_env.environment = env
