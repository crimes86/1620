extends Node
## NetworkManager - Server-authoritative multiplayer for 1620
## Autoload singleton

signal server_started
signal server_stopped
signal client_connected(peer_id: int)
signal client_disconnected(peer_id: int)
signal connection_failed

const DEFAULT_PORT: int = 7777
const MAX_PLAYERS: int = 12  # 4 trappers + 8 natives

var peer: ENetMultiplayerPeer = null
var is_server: bool = false
var is_client: bool = false
var connected_peers: Array[int] = []

func _ready() -> void:
	multiplayer.peer_connected.connect(_on_peer_connected)
	multiplayer.peer_disconnected.connect(_on_peer_disconnected)
	multiplayer.connected_to_server.connect(_on_connected_to_server)
	multiplayer.connection_failed.connect(_on_connection_failed)
	multiplayer.server_disconnected.connect(_on_server_disconnected)

	# Auto-start server if dedicated server mode
	if GameManager.is_dedicated_server:
		start_server()

func start_server(port: int = DEFAULT_PORT) -> Error:
	if peer != null:
		push_warning("[Network] Already connected, disconnect first")
		return ERR_ALREADY_IN_USE

	peer = ENetMultiplayerPeer.new()
	var error = peer.create_server(port, MAX_PLAYERS)

	if error != OK:
		push_error("[Network] Failed to start server: %s" % error_string(error))
		peer = null
		return error

	multiplayer.multiplayer_peer = peer
	is_server = true
	is_client = false

	print("[Network] Server started on port %d (max %d players)" % [port, MAX_PLAYERS])
	server_started.emit()
	return OK

func stop_server() -> void:
	if peer == null:
		return

	peer.close()
	peer = null
	multiplayer.multiplayer_peer = null
	is_server = false
	connected_peers.clear()

	print("[Network] Server stopped")
	server_stopped.emit()

func connect_to_server(address: String, port: int = DEFAULT_PORT) -> Error:
	if peer != null:
		push_warning("[Network] Already connected, disconnect first")
		return ERR_ALREADY_IN_USE

	peer = ENetMultiplayerPeer.new()
	var error = peer.create_client(address, port)

	if error != OK:
		push_error("[Network] Failed to connect: %s" % error_string(error))
		peer = null
		return error

	multiplayer.multiplayer_peer = peer
	is_client = true
	is_server = false

	print("[Network] Connecting to %s:%d..." % [address, port])
	return OK

func disconnect_from_server() -> void:
	if peer == null:
		return

	peer.close()
	peer = null
	multiplayer.multiplayer_peer = null
	is_client = false
	connected_peers.clear()

	print("[Network] Disconnected")

func _on_peer_connected(id: int) -> void:
	connected_peers.append(id)
	print("[Network] Peer connected: %d (%d/%d)" % [id, connected_peers.size(), MAX_PLAYERS])
	client_connected.emit(id)

func _on_peer_disconnected(id: int) -> void:
	connected_peers.erase(id)
	print("[Network] Peer disconnected: %d (%d/%d)" % [id, connected_peers.size(), MAX_PLAYERS])
	client_disconnected.emit(id)

func _on_connected_to_server() -> void:
	print("[Network] Connected to server!")
	is_client = true

func _on_connection_failed() -> void:
	print("[Network] Connection failed!")
	peer = null
	multiplayer.multiplayer_peer = null
	is_client = false
	connection_failed.emit()

func _on_server_disconnected() -> void:
	print("[Network] Server disconnected!")
	peer = null
	multiplayer.multiplayer_peer = null
	is_client = false

func get_player_count() -> int:
	return connected_peers.size()

func is_connected() -> bool:
	return peer != null and (is_server or is_client)
