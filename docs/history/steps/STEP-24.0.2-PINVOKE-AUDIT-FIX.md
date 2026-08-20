# Step 24.0.2 — Reachable P/Invoke Audit Fix

## Trigger

Step 24.0.1 / `0.0.74 (74)` passed canonical static validation **287/287**, compiled successfully on Codemagic, and ran the complete host suite. The suite finished **160/162**. Exactly two Step 24 safety tests failed:

- `GateARejectsReachablePInvokeBeforeAnyStep24ClrLoad`;
- `GateARejectsImplicitTypeInitializerPInvokeBeforeAnyStep24ClrLoad`.

Both failures showed `gateA.Passed == true` when the synthetic automatic-initialization closure contained a reachable P/Invoke. Host testing stopped the pipeline before iOS packaging, so no IPA and no physical Step 24 evidence exist for build 74.

## Root cause

The synthetic fixtures were valid. Cecil represented the native probe as a same-assembly P/Invoke method with no managed `MethodBody`, as expected. Gate A resolved the call successfully, but its traversal set contained only methods with managed bodies. The resolved P/Invoke target was therefore discarded before its `IsPInvokeImpl` / `PInvokeInfo` metadata was inspected.

The implicit type-initializer fixture exposed the identical blind spot one level deeper: the type constructor itself was correctly added to the automatic-initialization closure, but its call to the bodyless P/Invoke stub was skipped after resolution.

## Correction

After a same-assembly method reference resolves, inspect the resolved target **before** applying the managed-body traversal filter:

1. if `IsPInvokeImpl` or `PInvokeInfo` is present, record `P/Invoke reachable` and fail Gate A;
2. if the resolved same-assembly target has no managed IL body for any other reason, fail closed as an unmeasured execution edge;
3. only body-bearing managed methods are then queued for recursive IL traversal.

This preserves the existing bounded Cecil design while closing the exact native-execution hole demonstrated by the host tests.

## Protected behavior

No change to the physically proven Step 23 implementation. No change to Step 24 gate ordering, exact `0Harmony 2.4.2.0` target, private resolver policy, `RuntimeHelpers.RunModuleConstructor` boundary, native resolver refusal, trusted/prepared bytes, or prohibition on Harmony patch APIs/game invocation/Godot startup.

The Master Plan is unchanged because this is a candidate-level implementation correction, not a durable architecture or roadmap change.

## Candidate

- Step: **24.0.2**
- version: **0.0.75 (75)**
- workflow: **`ios-step-24`**
- expected IPA: **`artifacts/StS2-Launcher-Step-24.ipa`**

## Authority

Codemagic must first prove the host suite fully green and produce a verified IPA. Only then does the physical iPhone become the authority for the Step 24 A–D runtime boundary.
