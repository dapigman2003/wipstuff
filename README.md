# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Step 27 remains focused on proving one launcher-owned Harmony patch/unpatch boundary on iOS before any StS2 member is reflected or modified.

## Active candidate

**Step 27.0.15 / `0.0.99 (99)` — real-Harmony test compile hardening**

Codemagic 0.0.98 proved the production core still compiles after the Deferred-Cecil normalizer correction, but the newly added real-Harmony host regression itself failed to compile. Its helper used bare `ICustomAttributeProvider` while importing both `System.Reflection` and `Mono.Cecil`, producing CS0104 plus follow-on CS1503 diagnostics. The real Harmony 2.4.2 normalizer regression therefore never executed and no IPA/device evidence was produced.

0.0.99 changes only that test boundary:

- aliases `Mono.Cecil.ICustomAttributeProvider` as `CecilCustomAttributeProvider`;
- uses the alias in the `EditorBrowsableAttribute` surface scan;
- adds static protection forbidding the ambiguous bare helper declaration;
- retains the exact merged `Lib.Harmony 2.4.2` `netstandard2.0` quarantined CI fixture;
- keeps production `ReadingMode.Deferred`, the fail-closed resolver, the exact eleven-instruction `HarmonySharedState::.cctor` rewrite, prepared/source immutability, and Gates S/T unchanged.

The 0.0.98 Codemagic compiler report is preserved in `docs/history/reports/STEP-27.0.14-CODEMAGIC-TEST-COMPILE-FAILURE.txt`.

## iOS detour decision rule

The stop rule from 0.0.98 remains unchanged:

1. Reach and pass T6 with the normalized cctor.
2. If public `PatchProcessor.Patch()` works, continue the Harmony path.
3. If T6 passes but T7/T8 fails, perform one representative patch/unpatch on a launcher-owned post-publish interpreted fixture.
4. If that interpreted target also cannot be patched, stop iterating Harmony internals and propose deterministic ahead-of-load Cecil transforms on derived runtime copies. That would be a master-plan-level architecture change.

The master plan remains unchanged because 0.0.99 is still inside the existing launcher-owned Harmony characterization boundary.

## Build

Workflow: `ios-step-27`

Expected app version: `0.0.99 (99)`

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`

Codemagic must compile and execute the full host suite, including the real Harmony 2.4.2 normalizer regression, before publish. Physical acceptance remains A–Z **26/26**, then OfflineReady PASS and Foundation 5/5.
