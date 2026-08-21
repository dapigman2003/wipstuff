# Step 26 — Controlled Empty Harmony PatchProcessor Creation

## Starting evidence

Physical Step 25.0.2 / `0.0.82 (82)` closed the exact Harmony API/type-initialization/instance-construction boundary 9/9 and passed OfflineReady + Foundation 5/5.

Upstream Harmony 2.4.2 defines `Harmony.CreateProcessor(MethodBase)` as a thin factory returning `new PatchProcessor(this, original)`. `PatchProcessor(Harmony, MethodBase)` stores only the Harmony instance and original `MethodBase`; actual replacement occurs later in `PatchProcessor.Patch()`. `PatchProcessor` also owns a static `locker = new object()` initializer. Step 26 separates these into explicit gates.

## Objective

Prove that the physically accepted Harmony object can create one **empty** `HarmonyLib.PatchProcessor` targeting a launcher-owned inert host method, without patching anything and without reflecting any StS2 member.

## Gates

### Gates A–I — exact Step 25 replay

Reproduce the complete physically closed Step 25 chain in the Step 26 private context: input/initializer preflight, real `sts2.dll` load closure, exact `0Harmony` module initialization, exact Harmony API resolution, exact Harmony type initialization, one inert `Harmony(string)` construction, and the post-construction isolation audit.

### Gate J — HarmonyProcessorApiResolution

Metadata-audit the exact prepared `0Harmony 2.4.2.0` with rejecting Cecil resolution and require:

- exactly one public `Harmony.CreateProcessor(System.Reflection.MethodBase)` returning `HarmonyLib.PatchProcessor`;
- exact four-instruction factory shape: `ldarg.0`, `ldarg.1`, `new PatchProcessor(this, original)`, `ret`;
- public non-abstract `HarmonyLib.PatchProcessor`;
- exactly one public `.ctor(HarmonyLib.Harmony, System.Reflection.MethodBase)` with the measured field-storage-only body;
- exact private retained fields `instance : HarmonyLib.Harmony` and `original : System.Reflection.MethodBase`;
- exactly one managed `PatchProcessor::.cctor` matching `new System.Object()` → static `locker` → `ret`.

Then resolve only those exact runtime reflection objects. Do **not** initialize or construct `PatchProcessor`.

### Gate K — PatchProcessorTypeInitialization

Re-hash exact `0Harmony`, then explicitly complete only the Gate-J-measured `PatchProcessor::.cctor` using `RuntimeHelpers.RunClassConstructor(PatchProcessor.TypeHandle)`. Require unchanged context membership, zero native attempts, zero rejected/unplanned managed requests, and unchanged target bytes.

### Gate L — LauncherProbeResolution

Resolve exactly the build-time launcher-owned host method:

`System.Int32 StS2Launcher.Core.HarmonyProcessorProbe::Target(System.Int32)`

Require it to reside in the default host context. Do not invoke it and do not reflect any StS2 member.

### Gate M — HarmonyProcessorCreation

Invoke only exact `Harmony.CreateProcessor(MethodBase)` on the retained physically replayed Harmony object, passing the exact Gate-L probe `MethodInfo`.

Require:

- returned runtime type = exact `HarmonyLib.PatchProcessor` in the Step 26 private context;
- exact private `instance` field retains the exact Harmony object;
- exact private `original` field retains the exact launcher probe `MethodBase`;
- no private-context membership change;
- no native attempt;
- no rejected/unplanned managed request;
- unchanged `0Harmony` bytes.

Do **not** invoke `PatchProcessor.Patch()`.

### Gate N — PostProcessorAudit

Re-hash plan/prepared/live bytes, re-prove OfflineReady, verify exact private-context membership and retained processor/Harmony/probe identity, and require zero native/unplanned resolver events.

## Explicitly still out of scope

- `PatchProcessor.Patch`;
- `Harmony.Patch`, `PatchAll`, categories, patch-class discovery, or unpatching;
- `HarmonyMethod` / prefix / postfix / transpiler / finalizer creation;
- StS2 type/member reflection or invocation;
- the StS2 entry point;
- Godot/game startup;
- native game-library loading;
- mutation of trusted live/prepared game bytes.

## Candidate identity

- step: **26.0**
- version: **0.0.83 (83)**
- workflow: **`ios-step-26`**
- IPA: **`artifacts/StS2-Launcher-Step-26.ipa`**
- device report: `Documents/StS2Launcher/Reports/Step26-ControlledHarmonyProcessorCreation.txt`
