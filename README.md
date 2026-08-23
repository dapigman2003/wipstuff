# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Step 27 has localized the current iOS/AOT compatibility frontier to `HarmonyLib.HarmonySharedState::.cctor` before any `PatchProcessor.Patch()` or launcher-target invocation.

## Active candidate

**Step 27.0.11 / `0.0.95 (95)`** converts the physical `0.0.94 (94)` diagnosis into a bounded compatibility fix. The 0.0.94 checkpoint proved that the dedicated Step-27 load context successfully resolved requested `netstandard, Version=2.0.0.0` to host `netstandard, Version=2.1.0.0` while the original HarmonySharedState initializer was running, but the process still terminated before T6.

- Gate A first requires the exact receipt-backed Harmony 2.4.2 patch-engine metadata fingerprint, then creates a **byte-distinct in-memory runtime image** in which only `HarmonySharedState::.cctor` is normalized.
- The normalized initializer is audited as exactly 11 IL instructions: initialize the three Harmony state dictionaries, set `methodAddressRef = null`, set `actualVersion = 102`, and return.
- The source/live/prepared Harmony files are never rewritten. Their persisted length/SHA checks remain authoritative.
- Gate B loads the normalized bytes only for the exact verified `0Harmony, Version=2.4.2.0` private identity; all other assemblies continue to load from their verified prepared files.
- Gate S remains the bounded `HarmonyMethod()` descriptor path and never invokes `PatchProcessor.AddPrefix(MethodInfo)`.
- Gate T5a re-verifies the retained runtime-image SHA and requires no pre-existing generated patch-engine assemblies.
- Gate T5b executes exactly one `RuntimeHelpers.RunClassConstructor(HarmonySharedState.TypeHandle)` against the normalized direct-state initializer.
- T6 requires all three dictionaries non-null, `methodAddressRef == null`, `actualVersion == 102`, no generated `HarmonySharedState`/`ILGeneratorProxy` assembly, unchanged prepared bytes, and unchanged launcher-probe counters.
- Only after T6 may the existing single public `PatchProcessor.Patch()` acceptance call execute. The launcher target remains uninvoked until Gate V.
- `TrimMode=full`, `MtouchInterpreter=-all`, the fresh-process rule, and all StS2/Godot/native-game prohibitions remain unchanged.

The master document is unchanged; this is a bounded Step-27 runtime-compatibility correction.

## Build

Workflow: `ios-step-27`

Expected app version: `0.0.95 (95)`

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`

Require **26/26**, then OfflineReady PASS and Foundation 5/5.
