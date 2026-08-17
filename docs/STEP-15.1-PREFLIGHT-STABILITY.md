# Step 15.1 — Godot preflight + runtime stability hardening

Step 15.0.4 had not yet been submitted for another Codemagic/device run when the Step 15 integration was re-audited end-to-end. This release hardens the existing Godot Foundation subsystem before spending another CI/device cycle. It adds no Step 16 or StS2 execution capability.

Runtime version: `0.0.43 (43)`.

## Build / Codemagic hardening

- Pins both Godot tag `4.5.1-stable` and the already-observed immutable commit `f62fdbde15035c5576dad93e586201f4d41ef0cb`.
- Includes Xcode + iPhoneOS SDK identity in the Godot archive cache fingerprint.
- Selects exactly `bin/libgodot.ios.template_release.arm64.a` instead of choosing an arbitrary `libgodot*.a`.
- Keeps the SCons Python virtualenv outside the Codemagic cached directory and removes the legacy cached venv. Codemagic does not support caching symlinks; only the archive/fingerprint are cached now.
- Retries transient Git clone, pip/SCons, .NET installer download, and iOS workload-install boundaries.
- Reads the framework list directly from the app project's `NativeReference`, so the SDK preflight and final .NET link cannot silently drift apart.
- Adds a standalone Xcode `clang++` iPhoneOS native-link preflight against the completed Godot archive before the expensive .NET publish/AOT stage. It uses normal archive selection, `-ObjC -lz`, the exact project framework list, arm64 and the launcher's iOS 18 deployment target.
- Expands final IPA validation to check every managed `DllImport("__Internal")` Step 15 export and audit `otool -L` dependencies for system-or-bundled-only runtime libraries.

The standalone link preflight is specifically intended to catch the same class of errors exposed in 15.0.2–15.0.4 (invalid framework requests, missing app-level glue, undefined symbols, duplicate archive members) before the .NET iOS publish runs.

## Runtime hardening

Upstream `apple_embedded_main` allocates process-global iOS/Godot state before `Main::setup`, and the standalone Godot iOS app exits on setup failure instead of attempting another initialization in the same process. Step 15.1 therefore treats entry into `apple_embedded_main` as a one-attempt-per-process boundary.

- Native telemetry exports `sts2_step15_requires_process_restart`.
- After Godot initialization is entered, another Godot start is refused until force-quit/relaunch, even if Gate B later fails.
- The launcher disables unrelated Steam/Foundation/install controls after a Godot start attempt touches process-global engine state. This prevents accidental reuse of a partially initialized process.
- UIKit layout/bounds are validated before entering Godot, so a zero-size container remains a safe pre-start failure that can be corrected without poisoning the process.
- C# explicitly marshals the smoke-project path as UTF-8.
- Old render/touch markers must be successfully removed before the new session continues; stale evidence cannot produce a false Gate C/D pass.
- The smoke scene waits for `RenderingServer.frame_post_draw` before writing the Gate C render marker, so the marker represents a completed renderer frame rather than only scene-tree processing.
- The stale `Step 13 startup exception` SceneDelegate diagnostic label is corrected to Step 15.

## Scope unchanged

The ordered physical gates remain:

A. native Godot 4.5.1 availability/version;
B. embedded initialization and CADisplayLink stop/restart;
C. Metal-backed smoke-scene rendering;
D. touch + background/foreground/focus forwarding.

Step 15.1 still does not load, rewrite, or execute StS2 managed assemblies, integrate FMOD/Spine, implement game audio, Cloud, Workshop, or Step 16 work.

## Additional preflight findings from the second audit

- The final app now roots every Step 15 `DllImport("__Internal")` bridge function with .NET iOS `ReferenceNativeSymbol` items. This keeps normal archive-member selection (avoiding the PCRE2 force-load collision) while using the platform build system's native-symbol rooting mechanism instead of a hand-written linker `-u` flag.
- `AppDelegate.Window` is explicitly overridden so the Objective-C `window` / `setWindow:` surface Godot queries is guaranteed to exist in the scene-based .NET launcher. The bridge still points it at the existing scene window; it does not create a competing window.
- Gate-C UIKit/native view telemetry is explicitly evaluated on the main thread after asynchronous waits.
- Archive/IPA symbol validation now requires actual defined symbols, not merely matching names, and the native-link preflight derives the deployment target from the project instead of duplicating `18.0`.
- The Godot cache fingerprint now hashes the integration build script that contains the guarded upstream source patches, so changing a patch cannot silently reuse an older engine archive.

### Final second-pass hardening before device test

- Replaced the hand-written `-Wl,-u,_sts2_step15_get_engine_version` NativeReference flag with .NET 9 `ReferenceNativeSymbol` function roots for **every** managed `DllImport("__Internal")` Step 15 bridge entry point. The standalone clang preflight mirrors those roots with `-u`, while the actual .NET iOS build uses its supported build item.
- Gate C's project-owned marker now waits for `RenderingServer.frame_post_draw`, so the marker represents a completed rendering frame rather than only scene-tree processing.
- Removed a `set -o pipefail` / `strings | grep -q` false-negative hazard from final IPA verification by materializing the string table before checking the pinned Godot version marker. App discovery also uses `find -print -quit` rather than `find | head`.
