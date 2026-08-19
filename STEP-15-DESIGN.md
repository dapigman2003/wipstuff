# Step 15 design — embedded Godot Foundation

## Why this is a grouped step

After Step 14, the project moves from one-capability-per-release to one-related-subsystem-per-release with ordered gates. Step 15 groups the minimum Godot-native foundation because each gate has a clear observable boundary and later gates are never considered proven after an earlier failure.

## Source/build strategy

Codemagic clones the exact upstream Godot `4.5.1-stable` tag and uses SCons to build an arm64 iOS `template_release` static archive with native Metal enabled. Vulkan/MoltenVK is disabled for this boundary; OpenGLES fallback objects remain compiled for upstream iOS archive compatibility, while the smoke project and runtime arguments explicitly select Metal.

The upstream iOS archive contains its normal standalone `main()` entry point. This project already has a proven .NET/UIKit app entry point, so the build script renames only that one upstream `main()` definition before compilation. It retains `apple_embedded_main`, which performs Godot's embedded setup path.

Godot's Apple-embedded `DisplayServer` resolves its rendering view through the static `GDTAppDelegateService.viewController` that the normal standalone app delegate sets. Because the launcher keeps its already-proven .NET/UIScene delegates, the build applies one additional guarded patch to the pinned Godot source: it adds an embedded-host-only setter for that existing static slot. The bridge sets it to the project-owned `GDTViewController` before renderer setup and points the inherited `UIApplicationDelegate.window` property at the launcher's already-existing scene window. It does not create or replace the launcher window.

A project-owned Godot custom module exports a narrow C ABI consumed through `DllImport("__Internal")`. The module owns only host diagnostics/control: version, initialization, render-loop stop/restart, Metal/setup state, marker checks, and lifecycle counters.

## Lifecycle strategy

The normal Godot iOS app delegate forwards focus/background events into `OS_AppleEmbedded`. Step 15 keeps the existing .NET UIKit delegate and registers native UIKit notifications in the bridge, forwarding the same four events:

- focus out;
- enter background;
- exit background;
- focus in.

## Rendering/input proof

The smoke project is repository-owned GDScript content. `_ready()` writes a render marker. A real `InputEventScreenTouch` writes a separate touch marker and visibly changes the scene. The native host additionally requires Godot renderer setup completion and a Metal-named rendering layer.

## Safety/scope

No downloaded StS2 content is copied into the IPA or passed to Godot. Step 15 does not load real game assemblies or native libraries. It is an engine-host proof only.
