# Step 25.0.2 — Harmony Constructor Framework-Surface Preservation

## Physical evidence from Step 25.0.1 / 0.0.81

Physical build 81 materially advanced the Step 25 boundary:

- Gate A — InitializationPreflight: **PASS**;
- Gate B — ProvenLoadStateReplay: **PASS**;
- Gate C — DeferredModuleInitialization: **PASS**;
- Gate D — ProvenInitializationAudit: **PASS**;
- Gate E — HarmonyApiResolution: **PASS**;
- Gate F — HarmonyTypeInitialization: **PASS**;
- Gate G — HarmonyTypeInitializationAudit: **PASS**;
- Gate H — HarmonyInstanceConstruction: **FAIL**;
- Gate I did not run.

Gate H invoked only the exact physically measured `HarmonyLib.Harmony::.ctor(System.String)` with the launcher probe ID. Reflection entered the constructor through the Mono interpreter, then failed with:

`System.MissingMethodException: Method not found: System.Version System.Environment.get_Version()`

The measured constructor IL shows `Environment.Version` inside the `Harmony.DEBUG` logging branch. Gate F/G had already physically established `Harmony.DEBUG == false` and no `HARMONY_DEBUG` activation, so that branch should not execute semantically. The missing-member failure nevertheless occurred at constructor invocation. The conservative interpretation is that the interpreted method-import/invocation path requires framework member tokens from the exact constructor body to survive trimming even when the runtime branch remains dormant.

This is a build-time trim-survival issue, not evidence that Step 25 may enable Harmony debug logging, patching, game reflection, or native loading.

## Step 25.0.2 correction

Version: **0.0.82 (82)**.

Keep all physically proven runtime policies unchanged:

- `TrimMode=full`;
- `MtouchInterpreter=-all`;
- the exact 22 Step-22 direct framework roots;
- the physically proven `System.Collections.Concurrent` dynamic-IL preservation root;
- the exact Step 23 and Step 24 runtime boundaries;
- the Step 25 A–I execution code and fail-closed resolver/native policy.

Add one candidate-only build-time preservation anchor, `Step25HarmonyConstructorFrameworkPreservation`, which is called from the already-rooted iOS `AppDelegate.FinishedLaunching`. The method executes no framework probe and no Harmony code. Its `DynamicDependency` attributes preserve the bounded public callable surface of framework types that appear in the exact physically measured `Harmony(string)` constructor IL but are not necessarily visible to the build-time linker from the post-publish Harmony assembly:

- `System.Environment`;
- `System.OperatingSystem`;
- `System.Type`;
- `System.Reflection.Assembly`;
- `System.Reflection.AssemblyName`;
- `System.Reflection.MemberInfo`;
- `System.DateTime`;
- `System.Version`;
- `System.Runtime.CompilerServices.DefaultInterpolatedStringHandler`.

This intentionally batches the tightly related framework-token survival question into one candidate rather than preserving only `Environment.Version` and repeating one physical cycle per next missing token. It is still far narrower than rooting `System.Private.CoreLib`, disabling full trimming, enabling broad interpreter mode, or preserving arbitrary reflection surfaces.

## What remains unproven

Build 81 does **not** close Step 25. Gates A–G are physically established for the current boundary, but Gate H constructor completion and Gate I post-construction audit remain open.

The next physical build must still start from a fresh process and execute A–I in order. A different Gate-H failure after `Environment.Version` would be new evidence; the candidate must not preemptively broaden into patching or game-member work.

## Candidate identity

- step: **25.0.2**
- version: **0.0.82 (82)**
- workflow: **`ios-step-25`**
- IPA: **`artifacts/StS2-Launcher-Step-25.ipa`**
- device report: `Documents/StS2Launcher/Reports/Step25-ControlledHarmonyConstruction.txt`
