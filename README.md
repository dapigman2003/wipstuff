# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Step 27 has localized the current iOS/AOT compatibility frontier to `HarmonyLib.HarmonySharedState::.cctor` before any `PatchProcessor.Patch()` or launcher-target invocation.

## Active candidate

**Step 27.0.13 / `0.0.97 (97)`** is a host-test scope correction on top of the 0.0.95/0.0.96 HarmonySharedState compatibility candidate. Codemagic proved 0.0.96 now compiles and runs the host suite: **209/211 tests passed**. The two failures were not production runtime failures; Gate A had begun applying the exact real-`0Harmony` patch-engine fingerprint requirement to randomized minimal synthetic Harmony-like fixtures that are intentionally used only to replay Gates A–N.

- The public production path remains pinned to exact `0Harmony, Version=2.4.2.0` and still requires the full original patch-engine metadata fingerprint before any compatibility rewrite is admitted.
- For that canonical production target, Gate A still creates a **byte-distinct in-memory runtime image** in which only `HarmonySharedState::.cctor` is normalized. The eleven emitted instructions continue to use `CecilOpCodes = Mono.Cecil.Cil.OpCodes`.
- Internal randomized synthetic host fixtures now use a byte-identical passthrough runtime image instead of being forced through the production-only patch-engine audit. This restores their intended A–N gate tests without relaxing the public production constructor.
- The normalized production initializer remains exactly 11 IL instructions: initialize the three Harmony state dictionaries, set `methodAddressRef = null`, set `actualVersion = 102`, and return.
- Source/live/prepared Harmony files are never rewritten. Persisted length/SHA checks remain authoritative.
- Gate B uses the normalized bytes for the exact production Harmony identity; Gate S remains the bounded `HarmonyMethod()` descriptor path; Gate T5b still executes exactly one `RuntimeHelpers.RunClassConstructor(HarmonySharedState.TypeHandle)` against the normalized production image.
- T6 still requires all three dictionaries non-null, `methodAddressRef == null`, `actualVersion == 102`, no generated `HarmonySharedState`/`ILGeneratorProxy` assembly, unchanged prepared bytes, and unchanged launcher-probe counters.
- Only after T6 may the existing single public `PatchProcessor.Patch()` acceptance call execute. The launcher target remains uninvoked until Gate V.
- `TrimMode=full`, `MtouchInterpreter=-all`, the fresh-process rule, and all StS2/Godot/native-game prohibitions remain unchanged.

The master document is unchanged; this is a bounded Step-27 host-test/provenance correction with no intended production runtime behavior change from 0.0.96.

## Build

Workflow: `ios-step-27`

Expected app version: `0.0.97 (97)`

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`

Require **26/26**, then OfflineReady PASS and Foundation 5/5.
