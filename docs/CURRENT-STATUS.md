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

**Current source candidate:** Step 15.0.1 — Godot Foundation archive-validation hotfix (runtime Step 15 unchanged).

- App version: `0.0.42 (42)`
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


**Step 15.0.1 build note:** first Step 15 Codemagic run successfully built Godot 4.5.1-stable, then failed only in the local archive validator because it assumed unmangled C linkage for the Objective-C++ `apple_embedded_main` symbol and used `grep -q` pipelines under `pipefail`. Both validator issues are corrected; runtime remains 0.0.42 (42).
