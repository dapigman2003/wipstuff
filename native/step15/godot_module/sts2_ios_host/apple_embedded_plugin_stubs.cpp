// Step 15 custom embedded host does not use Godot iOS export plugins.
//
// Godot's iOS API layer always calls these two app-level plugin glue hooks.
// A normal Godot-generated Xcode project supplies implementations that invoke
// the initialization/deinitialization functions for the plugins selected by
// the export preset. This launcher does not use that generated app wrapper and
// deliberately has zero iOS plugins in the Step 15 smoke project, so the
// correct host implementations are no-ops.
//
// Keep C++ linkage: Godot's platform/ios API declares these functions as C++
// functions (the linker therefore expects mangled C++ symbols rather than C-linkage symbols).

__attribute__((visibility("default")))
void godot_apple_embedded_plugins_initialize() {
}

__attribute__((visibility("default")))
void godot_apple_embedded_plugins_deinitialize() {
}
