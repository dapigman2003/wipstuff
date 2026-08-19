# Step 15 physical-iPhone test — Godot Foundation

Step 15 is the first accelerated subsystem build. Gates are ordered. **Stop at the first failing gate and report that screen; do not infer that later gates work.**

## Build/install

Build Codemagic workflow `ios-step-15`, install the resulting IPA, and confirm the launcher shows:

```text
STEP 15.1 — GODOT FOUNDATION HARDENING
Version 0.0.43
```

The first CI build may be longer because Codemagic source-builds Godot 4.5.1-stable. No StS2 game content is packaged into the IPA.

## Gates A–C

Tap:

```text
Run Gates A–C — Native → Engine Init → Metal Render
```

Expected ordered results:

### Gate A — NativeAvailability

PASS requires the statically linked `__Internal` bridge to resolve and report:

```text
Godot 4.5.1-stable
```

If Gate A fails, stop.

### Gate B — EngineInitializeRenderLoop

PASS requires:

- `apple_embedded_main` initialization of the project-owned smoke project;
- the embedded host reports the initialized Godot session active;
- CADisplayLink rendering initially active;
- stop-rendering succeeds;
- start-rendering succeeds again.

If Gate B fails, stop and force-quit before any later test.

### Gate C — MetalRender

PASS requires all of:

- Godot renderer setup finished;
- native rendering layer identifies as Metal-backed;
- render loop active;
- project-owned `user://sts2_step15_render_ready.txt` marker observed;
- a visible panel reading `GODOT 4.5.1 / METAL — STEP 15 SMOKE SCENE` appears inside the launcher.

Expected launcher state after Gate C:

```text
GODOT FOUNDATION IN PROGRESS — 3/4
```

If Gate C fails, stop. Do not test Gate D.

## Gate D — touch + lifecycle

After A–C pass:

1. Tap inside the visible Godot smoke panel. It should turn green and show `TOUCH RECEIVED BY GODOT`.
2. Send the app to the background once (Home/app switcher).
3. Return to the launcher.
4. Tap `Verify Gate D — Touch + Background / Foreground`.

PASS requires:

- touch marker: YES;
- `background >= 1`;
- `foreground >= 1`;
- `focusOut >= 1`;
- `focusIn >= 1`.

Final expected summary:

```text
GODOT FOUNDATION PASS — 4/4
```

## Regression after Step 15

Once Step 15 reaches 4/4, **force-quit and relaunch the launcher** before running unrelated regression controls. This deliberately avoids asking the current build to tear down and recreate the embedded Godot engine in-process before that lifecycle has its own dedicated proof.

Then run:

```text
Run Foundation 5/5 Regression
```

Expected:

```text
FOUNDATION PASS — 5/5
```

Optionally rerun the Step 14 inventory; it must still work and must not show that Step 15 modified the managed StS2 installation.

## Explicit non-goals

Step 15 does not test StS2 startup, StS2 managed assemblies, Cecil rewriting, StS2 desktop-native libraries, FMOD, Spine, in-game Steamworks, audio, saves, Cloud, or Workshop.

### Step 15.0.3 build hotfix

The custom embedded host now supplies the two app-level Godot iOS plugin glue hooks (`godot_apple_embedded_plugins_initialize` / `deinitialize`) as intentional no-ops. A normal Godot-exported Xcode app generates these hooks from selected iOS plugins; Step 15 has no iOS plugins and does not use Godot's generated app wrapper. The archive validator now requires both C++ definitions before the .NET/iOS publish stage. Runtime version remains 0.0.42 (42).


### Step 15.0.4 native-link hotfix

The combined Godot iOS archive must be linked with normal archive-member selection, not `-force_load`. The Step 15.0.3 final link forced every Godot object member into the executable and therefore pulled mutually exclusive PCRE2 16-bit and 32-bit helper objects that both define `__pcre2_ckd_smul`. Step 15.0.4 sets the Godot NativeReference to `ForceLoad=false`, `SmartLink=false`, retains `-ObjC`, and adds validation against reintroducing force-loading. Runtime/device Gate A-D behavior is unchanged.

### Step 15.1 preflight/runtime hardening

Before the next physical run, the complete Step 15 integration was re-audited. Version `0.0.43 (43)` adds a standalone Apple native-link preflight before .NET publish, immutable Godot commit/toolchain-aware cache validation, symlink-safe Codemagic caching, deterministic archive selection, broader IPA dependency/export verification, explicit one-Godot-start-attempt-per-process safety, non-zero UIKit host bounds checks, UTF-8 path marshaling, and stronger fresh-session render-marker semantics.

The device gate sequence remains A → B → C → D. If Gate B or C fails after native initialization has been entered, **force-quit and relaunch before any retry or unrelated launcher regression**. The UI now enforces this lock instead of merely documenting it.
