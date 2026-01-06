extends Node
## GameManager - Global game state and utilities
## Autoload singleton for 1620

enum Faction { NONE, TRAPPER, NATIVE }
enum GameState { MENU, LOADING, PLAYING, PAUSED }

signal game_state_changed(new_state: GameState)
signal faction_selected(faction: Faction)

var current_state: GameState = GameState.MENU
var local_faction: Faction = Faction.NONE
var is_dedicated_server: bool = false

func _ready() -> void:
	# Check if running as dedicated server
	is_dedicated_server = "--server" in OS.get_cmdline_user_args()

	if is_dedicated_server:
		print("[GameManager] Running as dedicated server")
		DisplayServer.window_set_title("1620 - Dedicated Server")

func change_state(new_state: GameState) -> void:
	if current_state == new_state:
		return

	var old_state = current_state
	current_state = new_state
	print("[GameManager] State: %s -> %s" % [GameState.keys()[old_state], GameState.keys()[new_state]])
	game_state_changed.emit(new_state)

func select_faction(faction: Faction) -> void:
	local_faction = faction
	print("[GameManager] Faction selected: %s" % Faction.keys()[faction])
	faction_selected.emit(faction)

func get_faction_name(faction: Faction) -> String:
	match faction:
		Faction.TRAPPER:
			return "Trapper"
		Faction.NATIVE:
			return "Native"
		_:
			return "None"
