# Step 27.0.1 — Explicit HarmonyLib.AccessTools Type Initialization

## Trigger

Physical Step 27.0 / `0.0.84 (84)` passed Gates A–Q and failed at Gate R while calling exact `PatchProcessor.AddPrefix(MethodInfo)`. `Patch()` had not run. The inner failure was `TypeInitializationException` for `HarmonyLib.AccessTools`, caused while `HarmonyMethod(MethodInfo)` imported the launcher prefix metadata.

This showed that prefix-description construction crosses an automatic type-initialization boundary that Step 27.0 had not separated from `AddPrefix`.

## Correction

Step 27.0.1 / `0.0.85 (85)` keeps the same launcher-only patch objective but makes the new boundary explicit:

- Gate O additionally Cecil-audits `HarmonyLib.AccessTools::.cctor` and requires a bounded static BindingFlags-only field-initialization shape for exact public static readonly `all` and `allDeclared`.
- Gate O runtime-resolves the exact AccessTools type, initializer, and fields but does not read the fields or initialize the type.
- New Gate R calls only `RuntimeHelpers.RunClassConstructor(HarmonyLib.AccessTools.TypeHandle)`, then verifies exact BindingFlags values and unchanged hashes/context/native/resolver/probe state.
- Prefix registration moves to Gate S.
- The first real `PatchProcessor.Patch()` boundary moves to Gate T without changing its implementation or intent.
- Later audit/invoke/unpatch/restore/final gates shift through Gate Z.

No speculative framework root, Harmony fork, altered 0Harmony byte, StS2 reflection, or policy relaxation is introduced. If the explicit AccessTools initializer fails again, its measured Cecil IL plus direct RunClassConstructor failure becomes the next causal evidence.

## External-source cross-check

Upstream Harmony 2.4.2 source defines `AccessTools.all` as the combined public/nonpublic/instance/static/get/set field/property BindingFlags set and `allDeclared` as that value plus `DeclaredOnly`. This external source is advisory only; Gate O still measures the exact receipt-backed `0Harmony 2.4.2.0` actually prepared on-device before Gate R may execute it.

## Candidate identity

- step: **27.0.1**
- version: **0.0.85 (85)**
- workflow: **`ios-step-27`**
- expected summary: **26/26 PASS**

## Physical result — 0.0.85 (85)

Physical build 85 failed safely at Gate O, 14/26, before `RunClassConstructor` or any patching. The receipt-backed AccessTools initializer disproved this candidate's BindingFlags-only assumption and exposed a runtime-detection/cache initializer that also string-resolves `RuntimeInformation`, reflects `FrameworkDescription`, and constructs a handler dictionary and `ReaderWriterLockSlim`. The immediate follow-on candidate interpreted the first evidence as 56 instructions; physical build 86 later corrected the exact count to 57. The raw report is preserved at `docs/history/reports/STEP-27.0.1-PHYSICAL-GATE-O-REPORT.txt`. Step 27.0.2 replaces the incorrect metadata model; no Step-27 execution fact from build 84 is revoked.
