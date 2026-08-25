# Step 32 — First Real StS2 PrepareMethod Rewrite

Version: `0.0.115 (115)`

## Purpose

Materialize the first real-game semantic rewrite using the physically closed Step-28 transform-before-load mechanism and the exact Step-31 `PrewarmJit()` evidence. This boundary writes **only a launcher-private transformed copy** and deliberately stops before any real-StS2 CLR admission/execution.

## Predeclared semantic change

Exact method: `System.Void MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit()`

Token: `0x06007D05`

Source body SHA-256: `7f25b7bd955c407fc69306cf26af2162223353f5606560458066aed085e72ab9`

Ten exact `RuntimeHelpers.PrepareMethod` calls are suppressed and nothing else is intentionally changed:

- six `PrepareMethod(RuntimeMethodHandle)` calls: replace `Call` with one `Pop`;
- four `PrepareMethod(RuntimeMethodHandle, RuntimeTypeHandle[])` calls: insert one `Pop` immediately before the original call and replace the original `Call` with `Pop`.

The original calls return `void`; therefore the replacement consumes exactly the same one or two stack arguments and leaves no value. Reflection `GetMethod`, `get_MethodHandle`, generic-instantiation-array construction, loop structure, and exception handling remain present. The rewrite does not remove discovery side effects and does not add a launcher-assembly dependency.

## Gates

### Gate A — SourceAdmissionAndPrivateClone

Re-prove OfflineReady; verify exact receipt SHA-1/SHA-256/byte count, assembly identity/MVID, method token/body fingerprint, all ten offsets/signatures, and zero incoming branches to the selected call instructions. Require no `sts2` CLR identity resident. Create a launcher-private source clone and prove trusted/source hashes remain identical.

### Gate B — DeterministicStackNeutralRewrite

Using Mono.Cecil `ReadingMode.Deferred` with a rejecting resolver, perform only the predeclared six one-pop + four two-pop replacements. Write a new private transformed `sts2.dll`. Require zero dependency-resolution requests, unchanged trusted/source hashes, a distinct transformed hash, zero remaining in-memory `PrepareMethod` references, and exact replacement counts.

### Gate C — TransformedImageVerification

Reopen source and transformed images before CLR admission. Re-prove source body fingerprint and ten source calls; require zero transformed `PrepareMethod` references. Compare the reopened transformed method against the exact pre-write semantic fingerprint, including instruction operands, branch targets by ordinal, and exception-handler boundaries. Require source/transformed exception-handler counts to match and the exact `Pop` delta.

### Gate D — FinalIsolationAudit

Re-hash receipt-backed source, private source clone, and transformed image; re-prove OfflineReady; require no real-StS2 CLR load/invocation and no Harmony/MonoMod/Godot/native activity. A 4/4 PASS authorizes only a **separate later transformed-real-StS2 CLR admission/execution boundary**.

## Stop rules

Any source identity drift, resolver request, branch-targeted selected call, call-signature drift, unexpected extra semantic diff, source mutation, CLR-resident `sts2`, or OfflineReady failure stops the step immediately.
