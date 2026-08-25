# Step 31 — PrepareMethod Semantic Context Audit

## Purpose

Physical Step 30 closed 4/4 and formally deferred the selected Harmony/mod-loading site. Step 29 had already recorded the next highest-priority non-mod compatibility family: ten direct `RuntimeHelpers.PrepareMethod` call sites inside `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit()`.

Step 31.0 / `0.0.114 (114)` is a read-only evidence boundary. It binds the exact receipt-backed source, `PrewarmJit()` token `0x06007D05`, method-body SHA-256 `7f25b7bd955c407fc69306cf26af2162223353f5606560458066aed085e72ab9`, and the ten physical Step-29 call offsets. It then records exact per-site IL/control-flow/exception context before any real-game semantic write is designed.

## Exact physical evidence carried forward

PrepareMethod offsets:

- `IL_003D` — `PrepareMethod(RuntimeMethodHandle)`
- `IL_0052` — `PrepareMethod(RuntimeMethodHandle)`
- `IL_007A` — `PrepareMethod(RuntimeMethodHandle, RuntimeTypeHandle[])`
- `IL_00A2` — `PrepareMethod(RuntimeMethodHandle, RuntimeTypeHandle[])`
- `IL_00CA` — `PrepareMethod(RuntimeMethodHandle, RuntimeTypeHandle[])`
- `IL_00F2` — `PrepareMethod(RuntimeMethodHandle, RuntimeTypeHandle[])`
- `IL_0136` — `PrepareMethod(RuntimeMethodHandle)`
- `IL_014C` — `PrepareMethod(RuntimeMethodHandle)`
- `IL_0162` — `PrepareMethod(RuntimeMethodHandle)`
- `IL_0178` — `PrepareMethod(RuntimeMethodHandle)`

All are `[System.Runtime] System.Runtime.CompilerServices.RuntimeHelpers::PrepareMethod(...)` calls recorded by physical Step 29.

## Gates

### Gate A — EvidenceBindingAndOfflineReady

Re-prove OfflineReady and require the exact receipt-backed ARM64 `sts2.dll` SHA-1/SHA-256/byte count, MVID, `PrewarmJit()` method token/body fingerprint, and all ten exact PrepareMethod offsets/signatures. Cecil uses `ReadingMode.Deferred` with a rejecting resolver; resolver requests and CLR-resident `sts2` are blocking.

### Gate B — ExactPrepareMethodSemanticContextAudit

For the exact `PrewarmJit()` method, report method-body shape and all ten PrepareMethod sites. For each site, record a bounded IL window, incoming branch sources, covering exception regions, opcode/signature/argument count, plus method-wide strings and related RuntimeHelpers/Harmony/mod reference counts. No dependency resolution, Cecil write, or CLR execution is allowed.

### Gate C — DeterministicDisposition

If and only if the exact physical method/body/site set remains intact, record:

`BASE-GAME COMPATIBILITY FAMILY CONFIRMED — ELIGIBLE FOR EXPLICIT REWRITE DESIGN; NO WRITE AUTHORIZED`

This is a design-frontier disposition only. It does not claim runtime reachability and predeclares **no behavior change** in Step 31.

### Gate D — FinalIsolationAudit

Re-hash the primary source, re-prove OfflineReady, require no CLR-resident `sts2`, no Cecil writes, no resolver requests, no Harmony/MonoMod runtime patching, and no Godot/game/native loading.

## Close condition

Physical Step 31 closes only at **A–D / 4/4 PASS**. Preserve `Step31-PrepareMethodSemanticContextAudit.txt`.

A pass allows the following candidate to *design* one narrowly bounded ahead-of-load transformation for the exact fingerprinted method/sites. That later candidate must explicitly predeclare stack/control-flow semantics and still transform only a launcher-private copy before CLR admission.
