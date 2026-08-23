# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Step 27 has been stuck because several recent candidates failed **before** the intended patch-engine frontier: a compile ambiguity, synthetic-fixture overreach, and now a Mono.Cecil eager-read regression. Step 27.0.14 addresses both the immediate physical failure and that recurrence pattern.

## Active candidate

**Step 27.0.14 / `0.0.98 (98)` — Deferred Cecil normalization + real Harmony CI gate**

Physical 0.0.97 failed 0/26 at Gate A while creating the normalized Harmony runtime image. The normalizer used `ReadingMode.Immediate`, causing Mono.Cecil to eagerly deserialize unrelated custom-attribute constructor arguments and attempt forbidden external resolution of `System.ComponentModel.EditorBrowsableState`. The established metadata-only auditors already use `ReadingMode.Deferred` and do not have this problem.

0.0.98 therefore:

- uses `ReadingMode.Deferred` for both the source normalization read and the normalized-image audit;
- keeps `Step27MetadataOnlyResolver` fail-closed rather than adding framework resolution exceptions;
- relies on Cecil's writer behavior for Deferred modules, which completes metadata reading with custom-attribute argument resolution disabled;
- keeps the exact production-only `0Harmony 2.4.2` fingerprint gate and exact 11-instruction `HarmonySharedState::.cctor` replacement;
- keeps source/live/prepared Harmony bytes immutable and hash-authoritative;
- adds exact upstream `Lib.Harmony 2.4.2` as a quarantined **host-test fixture only**, then executes the real production normalizer against its `netstandard2.0/0Harmony.dll` in Codemagic;
- requires that real fixture to preserve the `EditorBrowsableAttribute` surface, remain byte-for-byte unchanged, and produce the expected byte-distinct normalized image;
- preserves the 0.0.97 byte-identical passthrough only for internal randomized synthetic replay targets.

Gate S and Gate T patch behavior are otherwise unchanged. T5b still runs exactly one normalized `HarmonySharedState` cctor; T6 validates direct state; only then may T7 enter the single public `PatchProcessor.Patch()` call. No StS2 member is reflected, patched, or invoked.

## iOS detour decision rule

Research against Microsoft iOS interpreter guidance, Apple JIT-memory restrictions, Harmony/MonoMod internals, the Android StS2 launchers, PPSSPP, and UTM shows an important distinction: `MtouchInterpreter=-all` can keep dynamic managed IL executable through the interpreter, but that does not by itself prove that Harmony/MonoMod can perform its native runtime method detour on iOS.

So the project now has a fixed stop rule instead of open-ended Harmony iteration:

1. 0.0.98 must first reach and pass T6 with the normalized cctor.
2. If public `PatchProcessor.Patch()` then works, continue the current Harmony path.
3. If T6 passes but T7/T8 fails, perform one representative patch/unpatch on a launcher-owned **post-publish interpreted fixture**, not another build-time AOT method.
4. If that interpreted target also cannot be patched, stop iterating Harmony internals and move to deterministic ahead-of-load Cecil transforms on derived runtime copies. That would be a major architecture change and would trigger a master-plan update.

The current master document remains unchanged because 0.0.98 is still inside its existing launcher-owned Harmony characterization boundary.

## Build

Workflow: `ios-step-27`

Expected app version: `0.0.98 (98)`

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`

Codemagic must compile/run the host suite—including the real Harmony 2.4.2 normalizer regression—before publish. Physical acceptance remains A–Z **26/26**, then OfflineReady PASS and Foundation 5/5.
