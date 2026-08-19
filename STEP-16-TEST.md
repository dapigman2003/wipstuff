# Step 16.1 — Physical iPhone Test

Build Codemagic workflow:

```text
ios-step-16-1
```

Expected app:

```text
STEP 16.1 — MANAGED PREPARATION FOUNDATION
Version 0.0.45
```

## Precondition

Keep the existing Step 12-managed StS2 install in its good `OfflineReady` state.

If you have run the Step 15 Godot host in the current app process, force-quit/relaunch before Step 16. The Step 16 button intentionally refuses to run while Step 15 process-global Godot state requires a restart.

## Run

Tap:

```text
Run Gates A–D — Cecil Fixture → IL Rewrite → Real StS2 Metadata
```

The app runs the four gates in order and stops at the first failure.

### Gate A PASS

Expect evidence that:

- Mono.Cecil opened `StS2Launcher.Step16.Fixture`;
- fixture identity is `STEP16_CECIL_FIXTURE_V1`;
- `RewriteMe` IL constant is `7`.

### Gate B PASS

Expect:

- fixture write/reopen succeeded;
- output is under `Step16-ManagedPreparation/`;
- reopened value remains `7`;
- bundled fixture unchanged = YES.

### Gate C PASS

Expect:

- controlled rewrite `RewriteMe 7 → 42`;
- rewritten output reopens successfully;
- fixture identity preserved;
- bundled fixture unchanged = YES;
- real StS2 install modified = NO.

### Gate D PASS

This gate can take longer because it first re-hashes the full Step 13 managed tree and then parses the installed managed assemblies one at a time. The Step 13 precondition file/byte progress is forwarded into the Step 16 detail view, so a long local hash pass should not look idle.

Expect:

- OfflineReady precondition = YES;
- non-zero managed-module candidates and parsed modules;
- both architecture-specific `sts2.dll` receipt entries may be discovered;
- the unique `data_sts2_macos_arm64/sts2.dll` is selected as the primary iPhone/AOT analysis target;
- main assembly identity/version/runtime/type/method/reference telemetry;
- post-inspection candidate SHA-1s reverified = all candidates;
- `sts2.dll` receipt SHA-1 preserved after inspection = YES;
- assembly dependency resolution attempted = NO;
- Steam session consulted = NO;
- network attempted = NO;
- real managed install modified = NO;
- game assembly loaded/executed = NO.

Final target:

```text
MANAGED PREPARATION PASS — 4/4
```

## Regression after Step 16

After 4/4, run:

```text
Verify Offline-Ready Install (Local Only)
```

and confirm it still reports `OFFLINE READY PASS`.

The existing Foundation 5/5 regression should also remain available. There is no need to rerun the Step 15 Godot gates unless a Step 16 build/link change appears to have regressed that subsystem.

## Stop rule

If any Gate A–D fails, stop there and send the full result screen and Codemagic artifacts if the failure happened during build. Do not infer later gates. A user cancellation during Gate D is reported as **CANCELLED**, not as a compatibility failure.
