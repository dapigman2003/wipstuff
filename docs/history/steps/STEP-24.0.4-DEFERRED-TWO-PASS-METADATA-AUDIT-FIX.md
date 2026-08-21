# Step 24.0.4 — Deferred Two-Pass Metadata Audit Fix

## Physical evidence that caused this correction

Step 24.0.3 / `0.0.76 (76)` reached a physical iPhone and again failed safely at **Gate A — InitializationPreflight**, stage `prepared target classification`:

`AssemblyResolutionException: Failed to resolve assembly: 'GodotSharp, Version=4.5.1.0, Culture=neutral, PublicKeyToken=null'`

The result was **0/4**. Gate B never ran, no Step 24 private CLR context was created, and the accepted Step 23.4.3 boundary remains the latest physically closed runtime state.

This repeated the build-75 exception even after Step 24.0.3 removed the explicit `MethodReference.Resolve()` call from the initializer traversal. Therefore the previous diagnosis was incomplete: some other part of the broad Cecil classification path could still cause external metadata resolution or eager materialization.

## Correction

Step 24.0.4 / `0.0.77 (77)` keeps the Step 24 execution policy unchanged and narrows Gate A's metadata behavior further.

1. **Two-pass classification.** Gate A first scans every exact prepared plan member only for module-initializer presence. It does not traverse method bodies during this whole-plan pass.
2. **Target-only closure audit.** Only after exactly one initializer-bearing dependency has been selected and verified as `0Harmony 2.4.2.0` does Gate A traverse the automatic-initialization call closure.
3. **Deferred Cecil reading.** The Step 24 reader now uses `ReadingMode.Deferred`, matching the established metadata-only pattern used by earlier physically proven inspection boundaries, so unrelated method bodies are not eagerly materialized.
4. **Explicit rejecting metadata resolver.** Both Cecil assembly and metadata resolution are bound to a Step 24 rejecting resolver. Any remaining implicit resolver request fails with a Step-24-specific message identifying the audited file and requested metadata rather than silently widening the metadata environment.
5. **No `ModuleDefinition.LookupToken` for call traversal.** MethodDef operands are handled directly; MemberRef/MethodSpec operands are matched only against definitions already present in the target module.
6. **More precise physical diagnostics.** Gate A stage text now includes the exact prepared relative path being classified, switches to an explicit `target automatic-initialization closure audit` stage before traversing `0Harmony`, and preserves the full exception text/stack in a failure report.

## What this does not change

- `GodotSharp` is not added to a resolver or allowlist.
- Genuine reachable `GodotSharp` execution remains a prohibited non-framework edge.
- Gate B remains the accepted Step 23 load-state replay.
- Gate C remains the sole new execution boundary and still admits exactly `0Harmony` before `RuntimeHelpers.RunModuleConstructor`.
- Native loading, Harmony APIs/patching, game reflection/invocation, Godot startup, and trusted/prepared/live mutation remain forbidden.

## Candidate identity

- step: **24.0.4**
- version: **0.0.77 (77)**
- workflow: **`ios-step-24`**
- expected IPA: **`artifacts/StS2-Launcher-Step-24.ipa`**

## Interpretation of the next physical run

- If the shallow whole-plan pass still attempts resolution, the report should identify the exact prepared file and the rejecting resolver should identify the requested assembly/type/member.
- If shallow classification passes but the target closure audit attempts resolution, the report should explicitly identify the `0Harmony` audit stage and requested metadata.
- If a real non-framework call is reachable, Gate A should stop with the existing prohibited-edge + audited-IL evidence.
- Only a clean Gate A may proceed to the already-defined B–D sequence.
