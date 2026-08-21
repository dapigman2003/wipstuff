# Step 27 — Controlled Launcher-Owned Harmony Patch + Unpatch

## Starting evidence

Physical Step 26.0 / `0.0.83 (83)` closed the empty `PatchProcessor` boundary **14/14**, followed by OfflineReady PASS and Foundation 5/5.

That means the next unresolved Harmony boundary is actual method replacement. Step 27 keeps the target entirely launcher-owned so Harmony/AOT/runtime patch-engine behavior can be characterized before any real StS2 member is reflected or patched.

## Objective

Prove one complete, reversible Harmony prefix lifecycle against a deterministic launcher-owned static method:

- original launcher method behavior is measured first;
- exact patch metadata is admitted fail-closed;
- one exact prefix is registered;
- exactly one `PatchProcessor.Patch()` crosses the patch-engine boundary;
- patched behavior is observed through reflection and direct calls;
- exactly that prefix is removed;
- original behavior is observed again;
- hashes, OfflineReady, private-context membership, managed resolution, and native refusal remain intact throughout.

## Launcher-owned probe

`HarmonyPatchProbe.Target(int value)`:

- increments a target-body counter;
- returns `value + 1`;
- is marked `NoInlining | NoOptimization`.

`HarmonyPatchProbe.Prefix(int value, ref int __result)`:

- increments a prefix counter;
- writes `__result = value + 1000`;
- returns `false` so Harmony must skip the original body;
- is marked `NoInlining | NoOptimization`.

The fixed test input is `41`:

- baseline/restored result = `42`;
- patched result = `1041`.

## Gates

### A–N — exact Step 26 replay

Reproduce the complete physically closed Step 26 chain, ending with the exact empty `PatchProcessor` in the same private context used by the new patch gates.

### O — HarmonyPatchApiResolution

Before constructing a patch descriptor, Cecil-audit and runtime-resolve only:

- `PatchProcessor.AddPrefix(MethodInfo)`;
- `PatchProcessor.Patch() -> MethodInfo`;
- `PatchProcessor.Unpatch(MethodInfo)`;
- `HarmonyMethod(MethodInfo)`;
- `PatchProcessor.prefix`;
- `HarmonyMethod.method`.

Require the measured Harmony 2.4.2 structural call flow and reject P/Invoke or metadata drift. No patch method is invoked.

### P — LauncherPatchProbeResolution

Resolve only the exact launcher-owned target/prefix pair in the host/default load context. Require exact types and parameter names `value` and `__result`. No invocation.

### Q — BaselineProbeInvocation

Reset counters. Call `Target(41)` directly and by `MethodInfo.Invoke`. Require both results = 42, target calls = 2, prefix calls = 0.

### R — PrefixRegistration

Invoke only `AddPrefix(MethodInfo)` and verify the resulting exact `HarmonyMethod` retains the exact launcher prefix. Require unchanged 0Harmony bytes/context/native/resolver state and unchanged probe counters. `Patch()` is still not invoked.

### S — PatchEngineExecution

Invoke exactly one `PatchProcessor.Patch()`. Require a replacement `MethodInfo`, unchanged `0Harmony` bytes, unchanged private-context membership, zero native attempts, zero rejected managed requests, and unchanged probe counters. Do not invoke the patched target yet.

### T — PostPatchAudit

Re-hash plan/prepared/live bytes, re-prove OfflineReady, and require exact Step-26 private-context/native/resolver state before any patched invocation.

### U — PatchedProbeInvocation

Invoke target first through reflection, then directly. Require result 1041 on both routes. Prefix count must increase twice; original target-body count must not increase.

### V — ExactPrefixUnpatch

Invoke exactly `PatchProcessor.Unpatch(prefix MethodInfo)` and require same processor identity, unchanged bytes/context, zero native/rejected requests, and unchanged counters.

### W — PostUnpatchAudit

Audit target hash/context/native/resolver state before restored invocation.

### X — RestoredProbeInvocation

Invoke target through reflection and direct routes. Require result 42 on both, original target-body count advances twice, and prefix count does not advance.

### Y — FinalIsolationAudit

Final plan/prepared/live rehash, OfflineReady exact-tree proof, exact context membership, zero native/rejected requests, exact retained object identities, `Harmony.DEBUG=false`, and exact restored-behavior snapshot.

## Explicitly still out of scope

- `Harmony.Patch`, `PatchAll`, categories, patch-class discovery;
- postfix/transpiler/finalizer/inner patch registration;
- any StS2 type/member reflection, patching, or invocation;
- the StS2 entry point;
- Godot/game startup;
- native game-library loading;
- trusted/prepared game-byte mutation.

## Candidate identity

- step: **27.0**
- version: **0.0.84 (84)**
- workflow: **`ios-step-27`**
- IPA: **`artifacts/StS2-Launcher-Step-27.ipa`**
- device report: `Documents/StS2Launcher/Reports/Step27-ControlledHarmonyPatchExecution.txt`

## Physical result — 0.0.84 (84)

The first physical Step 27 run reached **17/25**. Gates A–Q passed. Gate R failed during exact `PatchProcessor.AddPrefix(MethodInfo)` before `Patch()` was called.

The stack established a previously implicit execution boundary:

`AddPrefix(MethodInfo)` → `HarmonyMethod(MethodInfo)` → `HarmonyMethod.ImportMethod` → `HarmonyMethodExtensions.CopyTo` → `HarmonyMethod.HarmonyFields()` → automatic `HarmonyLib.AccessTools::.cctor` → `NullReferenceException`.

No launcher patch was installed and no StS2 member was reflected or invoked. The raw report is preserved at `docs/history/reports/STEP-27.0-PHYSICAL-GATE-R-REPORT.txt`.

The next candidate does not weaken prefix or patch policy. It first metadata-audits the exact `AccessTools` static initializer and gives that automatic initialization its own explicit gate before retrying prefix registration.
