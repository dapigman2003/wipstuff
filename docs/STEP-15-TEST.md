# Step 15 physical-iPhone test — Godot Foundation

Step 15 is the first accelerated subsystem build. Gates are ordered. **Stop at the first failing gate and report that screen; do not infer that later gates work.**

## Build/install

Build Codemagic workflow `ios-step-15`, install the resulting IPA, and confirm the launcher shows:

```text
STEP 15 — GODOT FOUNDATION
Version 0.0.42
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
