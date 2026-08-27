# Step 27.0.24 — Single Post-Publish Interpreted Harmony Patch/Unpatch Decision Experiment

## Physical evidence entering this candidate

Physical `0.0.107 (107)` removed the two known publish-time member-trimming failures by switching the launcher host to `MtouchLink=None + TrimMode=copy`. The normalized `HarmonySharedState` boundary remained viable, and the first exact public `PatchProcessor.Patch()` call then failed with `System.NotImplementedException` surfaced from `HarmonyLib.PatchFunctions.UpdateWrapper`.

That stack does not prove whether the unsupported operation occurs while generating Harmony's replacement method or while installing the later MonoMod detour. It does prove that the failure is no longer the `Enumerable.Union<T>` / `DebuggableAttribute` trimming class seen in 0.0.105/106.

The exact physical report is retained at `docs/history/reports/STEP-27.0.23-PHYSICAL-NOTIMPLEMENTED-PATCHENGINE.txt`.

## Stop-rule decision

Step 27 already defined a bounded stop rule: once normalized `HarmonySharedState` succeeds but the real patch boundary does not, perform exactly one representative experiment against a method that is not part of the launcher's build-time AOT graph. If that interpreted target still cannot patch, stop iterating Harmony internals and pivot Step 28 to deterministic ahead-of-load managed IL transformation.

0.0.108 is that single experiment. It does not force MonoMod backend environment variables, patch Harmony again, add another framework preservation exception, or touch any StS2 member.

## Representative interpreted fixture

A dedicated launcher-owned project, `fixtures/StS2Launcher.Step27.InterpretedPatchFixture`, is built by CI before iOS publish but is deliberately absent from the iOS project/content/reference graph. `scripts/build-ios.sh` copies only the resulting DLL into `Step27InterpretedPatchFixture/` after `dotnet publish` has completed. The final IPA verifier requires exactly one byte-identical DLL in that data-only directory plus a passing SHA-256 manifest.

The fixture exposes only:

- `Target(int) -> int`, which increments `TargetCalls` and returns `value + 1`;
- `InvokeTarget(int) -> int`, whose managed IL directly calls `Target` inside the same post-publish assembly;
- `Prefix(int value, ref int __result) -> bool`, which increments `PrefixCalls`, sets `__result = value + 1000`, and returns false;
- `ResetCounters()` plus two public integer counters.

Gate P admits the exact fixture bytes with Deferred/read-only Cecil metadata, requires an IL-only unsigned framework-only assembly with no module initializer, loads those bytes into the Step-27 private context, and resolves the exact members. It then creates a fresh `PatchProcessor` for the interpreted `Target` using the already-audited public `Harmony.CreateProcessor(MethodBase)` factory and verifies the processor retains the exact Harmony instance and target MethodBase.

This also corrects an experimental-harness mismatch discovered during the 0.0.108 review: the historical Gate-M processor was intentionally created against the Step-26 retention probe, while Gate P later resolved a separate patch probe. Prior Patch() attempts failed before behavior observation, so this never produced a false patched-behavior PASS, but the final decision experiment must bind its processor to the exact target under test. The correction uses a fresh public `CreateProcessor(MethodBase)` call; it does not mutate PatchProcessor's private `original` field.

## Required physical behavior

Gate Q resets the interpreted counters and proves two baseline routes: direct MethodInfo invocation of `Target(41)` and MethodInfo invocation of `InvokeTarget(41)`, whose own interpreted IL directly calls Target. Both must return 42 and establish `TargetCalls=2`, `PrefixCalls=0`.

Gate S assigns exactly one parameterless `HarmonyMethod` descriptor carrying only the interpreted Prefix MethodInfo. Gate T invokes public `PatchProcessor.Patch()` exactly once after the already-proven shared-state and framework preflights. If it returns, Gate V must show both interpreted routes return 1041 while `TargetCalls` remains 2 and `PrefixCalls` rises to 2, proving skip-original behavior.

Gate W then calls exact `PatchProcessor.Unpatch(MethodInfo)` once for that prefix. Gate Y must restore 42 on both routes with `TargetCalls=4` and no additional prefix calls.

## Decision after the physical run

- If the interpreted fixture patches and unpatches correctly, Harmony remains viable for the representative post-publish managed execution model and Step 27 can proceed to physical closure.
- If the interpreted fixture cannot patch, no further Harmony-internal workaround release is admitted. Step 28 becomes the ahead-of-load managed IL transformation pivot and `MASTER-PLAN.md` is revised for that architecture at that time.

No StS2 type/member is reflected, patched, or invoked by this candidate.
