# Current project status

**Steps 01–14 are complete and closed on a physical iPhone.**

The latest proven content inventory is Step 14 / `0.0.41 (41)`. On the installed public depot it reported:

- 428 files / 2,323,747,842 bytes;
- 8 broad asset files / 1,911,841,071 bytes;
- 1 Godot content file;
- 370 managed assemblies / 164,655,448 bytes;
- 39 native binaries / 247,031,344 bytes;
- 6 Godot/GodotSharp indicator files;
- 22 FMOD indicator files;
- 9 Spine indicator files;
- 98 dynamic-code/JIT indicator files;
- 273 platform-specific indicator files;
- three broad potential-iOS-blocker signal classes: desktop-native binaries, dynamic-code/JIT, and platform-specific paths/APIs.

The Step 14 evidence is triage only; it did not execute game code.

**Current source candidate:** Step 15.1 — Godot Foundation preflight/stability hardening.

- App version: `0.0.43 (43)`
- Codemagic workflow: `ios-step-15`
- Godot source pin: `4.5.1-stable`
- iOS Godot build: arm64 static archive, native Metal enabled, Vulkan/MoltenVK disabled for this foundation boundary
- Test model: ordered gates A–D; stop at first failure

Step 15 gates:

A. statically linked Godot bridge/version availability;
B. embedded engine initialization + CADisplayLink render-loop stop/start;
C. visible project-owned Godot scene on a Metal rendering layer;
D. physical touch + app focus/background/foreground forwarding.

Step 15 does not load or execute StS2 game content and does not introduce Cecil/FMOD/Spine/game-runtime integration.

**Step 15 is not complete until all four physical-iPhone gates pass and the existing Foundation 5/5 regression passes after relaunch.**


**Step 15.0.1 build note:** first Step 15 Codemagic run successfully built Godot 4.5.1-stable, then failed only in the local archive validator because it assumed unmangled C linkage for the Objective-C++ `apple_embedded_main` symbol and used `grep -q` pipelines under `pipefail`. Both validator issues are corrected; runtime remained 0.0.42 (42).

**Step 15.0.2 build note:** the following Codemagic run passed the Godot build and archive validator, then failed at the .NET/iOS native link with `ld: framework 'AudioUnit' not found`. The Step 15 `NativeReference` framework list had incorrectly requested `AudioUnit` as a standalone link framework. Step 15.0.2 removes that one link item while retaining `AudioToolbox`; runtime remained 0.0.42 (42).

**Step 15.0.3 build note:** the next run reached the final native link and exposed the two Godot iOS export-plugin glue hooks missing from the custom embedded host. The project-owned Godot module now supplies intentional no-op C++ definitions because Step 15 has zero iOS export plugins. Runtime remained 0.0.42 (42).

**Step 15.0.4 build note:** the next run exposed duplicate PCRE2 helper symbols because the combined Godot archive was force-loaded. The NativeReference now uses normal archive-member selection (`ForceLoad=false`, `SmartLink=false`) while retaining `-ObjC` and explicit link roots. Runtime remained 0.0.42 (42).

**Step 15.1 review note:** Before the next physical test, the full Godot/Codemagic integration was preflighted again. Runtime is now `0.0.43 (43)`. Build hardening includes immutable Godot commit/toolchain-aware cache identity, symlink-safe cache contents, deterministic archive selection, a standalone Apple native-link preflight before .NET publish, and broader final IPA export/dependency auditing. Runtime hardening makes any entered `apple_embedded_main` attempt one-per-process and locks unrelated controls until relaunch, validates non-zero UIKit host bounds before initialization, explicitly marshals the project path as UTF-8, and strengthens fresh render-marker evidence. Step 15 remains unproven until Gates A–D pass on the physical iPhone.


**Step 15.1.1 CI note:** the latest Codemagic run built Godot successfully but exposed a language-mode bug in the standalone native-link preflight. Runtime remains `0.0.43 (43)`; the probe now uses an explicit two-stage C++17 compile/link and the physical Step 15 gates remain unproven.
