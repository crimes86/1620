extends Label
## UI element showing current zone

var _current_zone: ZoneManager.Zone = ZoneManager.Zone.CONTESTED
var _player: Node3D = null

func _ready() -> void:
	# Find player
	await get_tree().process_frame
	_player = get_tree().get_first_node_in_group("player")

	if not _player:
		# Try to find by name
		_player = get_node_or_null("/root/GrayboxAmbush/Player")

	_update_display()

func _process(_delta: float) -> void:
	if not _player:
		return

	var new_zone = ZoneManager.get_zone_at(_player.global_position)
	if new_zone != _current_zone:
		_current_zone = new_zone
		_update_display()

func _update_display() -> void:
	text = ZoneManager.get_zone_name(_current_zone)

	var color = ZoneManager.get_zone_color(_current_zone)
	add_theme_color_override("font_color", color)

	# Show PvP status
	if ZoneManager.is_pvp_zone(_current_zone):
		text += " [PvP]"
	else:
		text += " [Safe]"
