# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Step 26.0 / `0.0.83 (83)` passed **14/14** on iPhone, followed by OfflineReady PASS and Foundation 5/5.

## Active Step 27 boundary

Step 27.0 / `0.0.84 (84)` crosses the first real Harmony method-replacement boundary, but only against a deterministic launcher-owned probe.

The candidate replays the complete closed Step 26 chain through Gate N, then:

- **Gate O:** metadata-audits and resolves exact `AddPrefix(MethodInfo)`, `Patch()`, `Unpatch(MethodInfo)`, and `HarmonyMethod(MethodInfo)` surfaces without construction or patching;
- **Gate P:** resolves launcher-owned `HarmonyPatchProbe.Target(int)` + `Prefix(int, ref __result)` without invocation;
- **Gate Q:** proves original probe behavior;
- **Gate R:** registers only the exact launcher prefix descriptor;
- **Gate S:** invokes exactly one `PatchProcessor.Patch()` — the first real patch-engine boundary — without invoking the patched target yet;
- **Gate T:** audits hashes/OfflineReady/context/native/resolver state before patched execution;
- **Gate U:** proves deterministic patched behavior through reflection and direct invocation while the original body is skipped;
- **Gate V:** removes exactly that prefix;
- **Gate W:** audits before restored execution;
- **Gate X:** proves original behavior is restored through both routes;
- **Gate Y:** performs the final full hash/OfflineReady/context/native isolation audit.

StS2 member reflection/patching/invocation, broad Harmony patch discovery, Godot/game startup, and native game-library loading remain forbidden.

## Build

Codemagic workflow: `ios-step-27`

Expected app version: `0.0.84 (84)`.

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`.

After a fully green Codemagic run, install fresh and run Step 27 A–Y. Require **25/25**, then OfflineReady PASS and Foundation 5/5.
