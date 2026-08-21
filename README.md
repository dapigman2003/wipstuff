# StS2 Launcher iOS — Step 26 Controlled Empty Harmony PatchProcessor Creation

Steps 01–25 are physically closed. Step 25.0.2 / `0.0.82 (82)` passed **9/9** on iPhone, followed by OfflineReady PASS and Foundation 5/5.

## Active Step 26 boundary

Step 26.0 / `0.0.83 (83)` tests the smallest Harmony patch-engine object boundary without patching anything.

The candidate replays the complete closed Step 25 chain, then:

- **Gate J:** metadata-audits and resolves exact `Harmony.CreateProcessor(MethodBase)` + `HarmonyLib.PatchProcessor` without type initialization or construction;
- **Gate K:** explicitly completes only the measured `PatchProcessor` static `locker = new object()` initializer;
- **Gate L:** resolves one launcher-owned inert host method, `HarmonyProcessorProbe.Target(int)`, without invoking it;
- **Gate M:** calls only `Harmony.CreateProcessor(MethodBase)` and verifies the returned empty processor retains the exact Harmony object and launcher probe `MethodBase`;
- **Gate N:** re-hashes/audits everything and re-proves OfflineReady.

`PatchProcessor.Patch`, `Harmony.Patch/PatchAll`, `HarmonyMethod` creation, StS2 member reflection/invocation, Godot/game startup, and native game-library loading remain forbidden.

## Build

Codemagic workflow: `ios-step-26`

Expected app version: `0.0.83 (83)`.

Expected IPA: `artifacts/StS2-Launcher-Step-26.ipa`.

After a fully green Codemagic run, install fresh and run Step 26 A–N. Require **14/14**, then OfflineReady PASS and Foundation 5/5.
