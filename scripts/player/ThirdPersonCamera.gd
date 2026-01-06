extends Node3D
class_name ThirdPersonCamera
## WoW-style third person camera
## - Right-click drag to rotate camera
## - Scroll wheel to zoom
## - Camera collision to prevent clipping

@export_group("Orbit")
@export var min_distance: float = 2.0
@export var max_distance: float = 15.0
@export var default_distance: float = 8.0
@export var zoom_speed: float = 1.0

@export_group("Rotation")
@export var mouse_sensitivity: float = 0.003
@export var min_pitch: float = -80.0  # Looking up
@export var max_pitch: float = 80.0   # Looking down

@export_group("Collision")
@export var collision_margin: float = 0.3
@export var collision_mask: int = 1  # World layer

@export_group("Smoothing")
@export var rotation_smoothing: float = 15.0
@export var zoom_smoothing: float = 10.0

@onready var camera: Camera3D = $Camera3D
@onready var spring_arm: SpringArm3D = $SpringArm3D

var _target_distance: float
var _current_distance: float
var _yaw: float = 0.0  # Horizontal rotation
var _pitch: float = -20.0  # Vertical rotation (start looking slightly down)
var _target_yaw: float = 0.0
var _target_pitch: float = -20.0

var _is_rotating: bool = false

func _ready() -> void:
	_target_distance = default_distance
	_current_distance = default_distance

	# Setup spring arm for collision
	if spring_arm:
		spring_arm.spring_length = default_distance
		spring_arm.margin = collision_margin
		spring_arm.collision_mask = collision_mask

func _unhandled_input(event: InputEvent) -> void:
	# Right-click to rotate camera
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_RIGHT:
			_is_rotating = event.pressed
			if _is_rotating:
				Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
			else:
				Input.mouse_mode = Input.MOUSE_MODE_VISIBLE

		# Scroll wheel zoom
		elif event.button_index == MOUSE_BUTTON_WHEEL_UP:
			_target_distance = max(min_distance, _target_distance - zoom_speed)
		elif event.button_index == MOUSE_BUTTON_WHEEL_DOWN:
			_target_distance = min(max_distance, _target_distance + zoom_speed)

	# Mouse motion for rotation
	if event is InputEventMouseMotion and _is_rotating:
		_target_yaw -= event.relative.x * mouse_sensitivity
		_target_pitch -= event.relative.y * mouse_sensitivity
		_target_pitch = clamp(_target_pitch, deg_to_rad(min_pitch), deg_to_rad(max_pitch))

func _process(delta: float) -> void:
	# Smooth rotation
	_yaw = lerp_angle(_yaw, _target_yaw, rotation_smoothing * delta)
	_pitch = lerp(_pitch, _target_pitch, rotation_smoothing * delta)

	# Smooth zoom
	_current_distance = lerp(_current_distance, _target_distance, zoom_smoothing * delta)

	# Apply rotation to this node (pivot point)
	rotation.y = _yaw
	rotation.x = _pitch

	# Apply distance to spring arm
	if spring_arm:
		spring_arm.spring_length = _current_distance

## Get the forward direction the camera is facing (for movement)
func get_camera_forward() -> Vector3:
	return -global_transform.basis.z

## Get the right direction relative to camera
func get_camera_right() -> Vector3:
	return global_transform.basis.x

## Get yaw angle for character rotation
func get_yaw() -> float:
	return _yaw
