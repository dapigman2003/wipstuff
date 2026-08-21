# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Step 26.0 / `0.0.83 (83)` passed **14/14** on iPhone, followed by OfflineReady PASS and Foundation 5/5.

Physical Step 27.0 / `0.0.84 (84)` then reached **17/25**: Gates A–Q passed and Gate R failed before `Patch()` because `HarmonyMethod(MethodInfo)` implicitly triggered `HarmonyLib.AccessTools::.cctor`.

## Active Step 27 boundary

Step 27.0.1 / `0.0.85 (85)` keeps the same launcher-only replacement objective but makes the newly observed `AccessTools` initializer explicit.

The candidate replays the complete closed Step 26 chain through Gate N, then:

- **Gate O:** metadata-audits/resolves exact `AddPrefix`, `Patch`, `Unpatch`, `HarmonyMethod`, plus the exact bounded `AccessTools::.cctor`/BindingFlags surface without executing it;
- **Gate P:** resolves launcher-owned `HarmonyPatchProbe.Target(int)` + `Prefix(int, ref __result)` without invocation;
- **Gate Q:** proves original probe behavior;
- **Gate R:** explicitly completes only `AccessTools::.cctor` and verifies `all`/`allDeclared`;
- **Gate S:** registers only the exact launcher prefix descriptor;
- **Gate T:** invokes exactly one `PatchProcessor.Patch()` — the first real patch-engine boundary — without invoking the patched target yet;
- **Gate U:** audits hashes/OfflineReady/context/native/resolver state before patched execution;
- **Gate V:** proves deterministic patched behavior through reflection and direct invocation while the original body is skipped;
- **Gate W:** removes exactly that prefix;
- **Gate X:** audits before restored execution;
- **Gate Y:** proves original behavior is restored through both routes;
- **Gate Z:** performs the final full hash/OfflineReady/context/native isolation audit.

StS2 member reflection/patching/invocation, broad Harmony patch discovery, Godot/game startup, and native game-library loading remain forbidden.

## Build

Codemagic workflow: `ios-step-27`

Expected app version: `0.0.85 (85)`.

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`.

After a fully green Codemagic run, install fresh and run Step 27 A–Z. Require **26/26**, then OfflineReady PASS and Foundation 5/5.
