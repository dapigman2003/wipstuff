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


## Preserved Step 28 compile evidence

The 0.0.109 Codemagic host/build output is preserved at:

`docs/history/reports/STEP-28.0-CODEMAGIC-CORE-COMPILE-FAILURE.txt`

It records canonical static validation 845/845 PASS, successful construction of all external managed fixtures, and the first blocking Core compiler diagnostic: `AheadOfLoadManagedTransformation.cs(88,23): error CS0246` for missing `CallbackProgress<>`. No MSTest, iOS publish, IPA, or physical runtime result followed from that candidate.

## Preserved Step 27 architecture-decision evidence

The final physical Step-27 report is preserved in source history at:

`docs/history/reports/STEP-27.0.24-PHYSICAL-INTERPRETED-PATCH-FAILURE.txt`

It records **19/26, first failure PatchEngineExecution**, after the post-publish interpreted fixture was admitted and baseline direct-call behavior was proven. The exact public `PatchProcessor.Patch()` call then threw `System.NotImplementedException` from `PatchFunctions.UpdateWrapper`. This is the evidence that triggered the Step-28 architecture pivot.

The historical on-device Step-27 report/checkpoint paths remain:

- `Documents/StS2Launcher/Reports/Step27-ControlledHarmonyPatchExecution.txt`
- `Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt`

They remain available as historical regression/evidence tooling but are no longer the active compatibility path.

The physically closed Step 26/25/24/23 reports retain their existing deterministic filenames.

- `docs/history/reports/STEP-28.0.1-CODEMAGIC-HOST-TEST-FAILURE.txt` — preserved raw Codemagic 0.0.110 host output; compile succeeded, 216/217 host tests passed, and Step-28 Gate A failed before rewrite/load on Cecil eager `System.Runtime` resolution.
