# Diagnostic Reports

Current on-device diagnostics write text reports beneath:

`Documents/StS2Launcher/Reports/*.txt`

and are visible through:

`Files → On My iPhone → StS2 Launcher → StS2Launcher → Reports`

Specialized full diagnostics may use stable files directly under `Documents/StS2Launcher/`, for example runtime-binding or framework-frontier reports.

## Active Step 28 report

`Documents/StS2Launcher/Reports/Step28-AheadOfLoadManagedTransformation.txt`

It records ordered Gates A–E: OfflineReady/source-fixture admission, source SHA-256 and exact Cecil IL shape, deterministic private `Adjustment() 1 -> 1000` rewrite, source/transformed hash isolation, transformed-image reopen verification, transformed-only private `AssemblyLoadContext` admission, `Adjustment/Target/InvokeTarget` execution results, resolver observations, final source/transformed hashes, final OfflineReady state, and the explicit no-Harmony/no-real-StS2/no-Godot/no-native-game boundary.

A current verification overwrites its deterministic latest report. Reports are output-only and are never trusted runtime input.

## Preserved Step 27 architecture-decision evidence

The final physical Step-27 report is preserved in source history at:

`docs/history/reports/STEP-27.0.24-PHYSICAL-INTERPRETED-PATCH-FAILURE.txt`

It records **19/26, first failure PatchEngineExecution**, after the post-publish interpreted fixture was admitted and baseline direct-call behavior was proven. The exact public `PatchProcessor.Patch()` call then threw `System.NotImplementedException` from `PatchFunctions.UpdateWrapper`. This is the evidence that triggered the Step-28 architecture pivot.

The historical on-device Step-27 report/checkpoint paths remain:

- `Documents/StS2Launcher/Reports/Step27-ControlledHarmonyPatchExecution.txt`
- `Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt`

They remain available as historical regression/evidence tooling but are no longer the active compatibility path.

The physically closed Step 26/25/24/23 reports retain their existing deterministic filenames.
