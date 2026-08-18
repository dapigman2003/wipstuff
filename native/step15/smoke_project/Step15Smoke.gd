extends Control

const RENDER_MARKER := "user://sts2_step15_render_ready.txt"
const TOUCH_MARKER := "user://sts2_step15_touch_ready.txt"
var tap_count := 0

func _ready() -> void:
    for marker in [RENDER_MARKER, TOUCH_MARKER]:
        if FileAccess.file_exists(marker):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(marker))

    # Do not mark Gate C from _ready() itself. Godot documents frame_post_draw
    # as the point at which a viewport has completed a rendered frame; this makes
    # the marker evidence of the render path, not merely scene-tree processing.
    await RenderingServer.frame_post_draw
    _write_marker(RENDER_MARKER, "frame-post-draw-ready")

func _input(event: InputEvent) -> void:
    if event is InputEventScreenTouch and event.pressed:
        tap_count += 1
        $Panel.color = Color(0.10, 0.42, 0.25, 1)
        $Status.text = "TOUCH RECEIVED BY GODOT\nTap count: %d\nNow background and return to the launcher" % tap_count
        _write_marker(TOUCH_MARKER, str(tap_count))
        get_viewport().set_input_as_handled()

func _write_marker(path: String, text: String) -> void:
    var file := FileAccess.open(path, FileAccess.WRITE)
    if file != null:
        file.store_string(text)
        file.flush()
