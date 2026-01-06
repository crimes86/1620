extends Control
## Main Menu for 1620 Greybox

const GREYBOX_SCENE = "res://scenes/greybox_ambush.tscn"

func _ready() -> void:
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE

func _on_play_pressed() -> void:
	print("[Menu] Starting greybox...")
	GameManager.change_state(GameManager.GameState.LOADING)
	get_tree().change_scene_to_file(GREYBOX_SCENE)

func _on_host_pressed() -> void:
	print("[Menu] Starting server and loading greybox...")
	var error = NetworkManager.start_server()
	if error == OK:
		GameManager.change_state(GameManager.GameState.LOADING)
		get_tree().change_scene_to_file(GREYBOX_SCENE)
	else:
		push_error("Failed to start server!")

func _on_join_pressed() -> void:
	# TODO: Add IP input dialog
	print("[Menu] Connecting to localhost...")
	var error = NetworkManager.connect_to_server("127.0.0.1")
	if error == OK:
		# Wait for connection then load scene
		await get_tree().create_timer(1.0).timeout
		GameManager.change_state(GameManager.GameState.LOADING)
		get_tree().change_scene_to_file(GREYBOX_SCENE)
	else:
		push_error("Failed to connect!")

func _on_quit_pressed() -> void:
	print("[Menu] Quitting...")
	get_tree().quit()
