extends CharacterBody3D
class_name ThirdPersonController
## WoW-style third person character controller
## Movement relative to camera, character rotates to face movement direction

@export_group("Movement")
@export var walk_speed: float = 5.0
@export var sprint_speed: float = 10.0
@export var jump_force: float = 7.0
@export var gravity: float = 20.0
@export var rotation_speed: float = 10.0  # How fast character turns

@export_group("References")
@export var camera_pivot: Node3D

var _is_sprinting: bool = false
var _target_rotation: float = 0.0

func _ready() -> void:
	# Find camera pivot if not assigned
	if not camera_pivot:
		camera_pivot = get_node_or_null("CameraPivot")

func _physics_process(delta: float) -> void:
	# Get input
	var input_dir := Vector2.ZERO
	input_dir.x = Input.get_axis("move_left", "move_right")
	input_dir.y = Input.get_axis("move_forward", "move_back")

	# Get camera-relative direction
	var direction := Vector3.ZERO
	if camera_pivot and input_dir.length() > 0.1:
		var cam_forward = camera_pivot.get_camera_forward() if camera_pivot.has_method("get_camera_forward") else -camera_pivot.global_transform.basis.z
		var cam_right = camera_pivot.get_camera_right() if camera_pivot.has_method("get_camera_right") else camera_pivot.global_transform.basis.x

		# Flatten to horizontal plane
		cam_forward.y = 0
		cam_forward = cam_forward.normalized()
		cam_right.y = 0
		cam_right = cam_right.normalized()

		direction = (cam_right * input_dir.x + cam_forward * -input_dir.y).normalized()

	# Sprint
	_is_sprinting = Input.is_action_pressed("sprint")
	var speed := sprint_speed if _is_sprinting else walk_speed

	# Apply horizontal movement
	if direction.length() > 0.1:
		velocity.x = direction.x * speed
		velocity.z = direction.z * speed

		# Rotate character to face movement direction
		_target_rotation = atan2(direction.x, direction.z)
	else:
		# Decelerate
		velocity.x = move_toward(velocity.x, 0, speed * 0.5)
		velocity.z = move_toward(velocity.z, 0, speed * 0.5)

	# Smooth character rotation
	rotation.y = lerp_angle(rotation.y, _target_rotation, rotation_speed * delta)

	# Gravity
	if not is_on_floor():
		velocity.y -= gravity * delta
	else:
		velocity.y = -0.1

	# Jump
	if Input.is_action_just_pressed("jump") and is_on_floor():
		velocity.y = jump_force

	move_and_slide()

func get_current_speed() -> float:
	return Vector2(velocity.x, velocity.z).length()

func is_sprinting() -> bool:
	return _is_sprinting and is_on_floor() and get_current_speed() > 0.1

func is_moving() -> bool:
	return get_current_speed() > 0.1
