extends CharacterBody3D
class_name FirstPersonController
## First-Person Character Controller for 1620 greybox testing
## WASD movement + Mouse look + Sprint + Jump

@export_group("Movement")
@export var walk_speed: float = 5.0
@export var sprint_speed: float = 10.0
@export var jump_force: float = 7.0
@export var gravity: float = 20.0

@export_group("Look")
@export var mouse_sensitivity: float = 0.002
@export var max_look_angle: float = 80.0

@onready var camera: Camera3D = $Camera3D

var _vertical_rotation: float = 0.0
var _is_sprinting: bool = false

func _ready() -> void:
	# Capture mouse
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED

func _unhandled_input(event: InputEvent) -> void:
	# Mouse look
	if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		# Horizontal rotation (body)
		rotate_y(-event.relative.x * mouse_sensitivity)

		# Vertical rotation (camera only)
		_vertical_rotation -= event.relative.y * mouse_sensitivity
		_vertical_rotation = clamp(_vertical_rotation, deg_to_rad(-max_look_angle), deg_to_rad(max_look_angle))
		camera.rotation.x = _vertical_rotation

	# Toggle mouse capture with Escape
	if event.is_action_pressed("ui_cancel"):
		if Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
			Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
		else:
			Input.mouse_mode = Input.MOUSE_MODE_CAPTURED

func _physics_process(delta: float) -> void:
	# Get input direction
	var input_dir := Vector2.ZERO
	input_dir.x = Input.get_axis("move_left", "move_right")
	input_dir.y = Input.get_axis("move_forward", "move_back")

	# Convert to world direction
	var direction := (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()

	# Sprint check
	_is_sprinting = Input.is_action_pressed("sprint")
	var speed := sprint_speed if _is_sprinting else walk_speed

	# Apply horizontal movement
	if direction:
		velocity.x = direction.x * speed
		velocity.z = direction.z * speed
	else:
		velocity.x = move_toward(velocity.x, 0, speed)
		velocity.z = move_toward(velocity.z, 0, speed)

	# Apply gravity
	if not is_on_floor():
		velocity.y -= gravity * delta
	else:
		velocity.y = -0.1  # Small downward force to keep grounded

	# Jump
	if Input.is_action_just_pressed("jump") and is_on_floor():
		velocity.y = jump_force

	move_and_slide()

func get_current_speed() -> float:
	return Vector2(velocity.x, velocity.z).length()

func is_sprinting() -> bool:
	return _is_sprinting and is_on_floor() and get_current_speed() > 0.1
