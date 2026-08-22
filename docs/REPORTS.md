# Diagnostic Reports

Current on-device diagnostics write text reports beneath:

`Documents/StS2Launcher/Reports/*.txt`

and are visible through:

`Files → On My iPhone → StS2 Launcher → StS2Launcher → Reports`

Specialized full diagnostics may use stable files directly under `Documents/StS2Launcher/`, for example runtime-binding or framework-frontier reports.

The active Step 27 report is:

`Documents/StS2Launcher/Reports/Step27-ControlledHarmonyPatchExecution.txt`

It records the ordered A–Z result; exact replay of the closed Step-26 state; exact patch API plus bounded AccessTools initializer metadata; launcher-owned target/prefix signatures and baseline counters; explicit AccessTools type initialization; prefix registration; the first exact `PatchProcessor.Patch()` boundary; pre-invocation post-patch integrity; patched direct/reflection behavior; exact prefix unpatch; post-unpatch integrity; restored direct/reflection behavior; final plan/file hashes, OfflineReady, context membership, resolver/native observations; and the explicit no-StS2/no-Godot/no-native-game boundary.

The physically closed Step 26 report remains:

`Documents/StS2Launcher/Reports/Step26-ControlledHarmonyProcessorCreation.txt`

The physically closed Step 25 report remains:

`Documents/StS2Launcher/Reports/Step25-ControlledHarmonyConstruction.txt`

The physically closed Step 24 report remains:

`Documents/StS2Launcher/Reports/Step24-ControlledManagedInitialization.txt`

The physically closed Step 23 regression report remains:

`Documents/StS2Launcher/Reports/Step23-FirstRealGameLoad.txt`

A current verification overwrites its deterministic latest report. Reports are output-only and are never trusted runtime input.

## Step 27 crash checkpoint

Step 27.0.5 adds `Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt`. Unlike the normal end-of-run report, this tiny output-only file is synchronously overwritten and flushed during the run so it can survive an abrupt process termination. Preserve it before the next Step-27 attempt if the app exits without a managed report. It is diagnostic only and is never consumed as trusted runtime input.

- `docs/history/reports/STEP-27.0.5-PHYSICAL-GATE-S-CRASH-CHECKPOINT.txt` — physical 0.0.89 durable crash breadcrumb localizing abrupt termination to Gate S / S1 inside `PatchProcessor.AddPrefix(MethodInfo)` before the first `Patch()` call.
