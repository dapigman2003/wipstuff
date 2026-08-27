# Step 32.0.1 — Serialized Fingerprint Verification Fix

Version: `0.0.116 (116)`

## Trigger

Codemagic 0.0.115 passed canonical static validation at **996/996**, compiled the production/test projects, built all external fixtures, and executed the complete host suite. Result: **230/231 PASS**. The only failure was `ExactPrewarmJitPrepareMethodFamilyIsRewrittenOnPrivateCopyOnly` at Step-32 Gate C:

`InvalidDataException: Step-32 reopened transformed PrewarmJit does not match the exact in-memory predeclared rewrite.`

The failure occurred after Gate B had successfully materialized the launcher-private transformed image. No iOS publish/device result exists for 0.0.115.

Raw authority: `docs/history/reports/STEP-32.0-CODEMAGIC-HOST-TEST-FAILURE.txt`.

## Root cause

0.0.115 computed two in-memory fingerprints before `ModuleDefinition.Write` and expected both to reproduce after reopening the serialized module:

- the **semantic fingerprint**, which is intentionally offset-independent and represents branch/EH targets by instruction ordinal; and
- the older **method-body fingerprint**, which includes concrete IL byte offsets and branch-target offsets.

The latter is not a valid pre-write serialization invariant when the rewrite inserts four instructions. Cecil finalizes instruction offsets while serializing the method body, so a body hash derived from the pre-write `Instruction.Offset` values cannot be required to equal the post-write/reopened physical body hash.

This is a verification-model defect. It is not evidence that the six one-argument or four two-argument stack-neutral replacements are incorrect.

## Correction

Step 32.0.1 leaves the semantic transformation unchanged.

Gate B still computes the exact offset-independent semantic fingerprint from the in-memory rewritten method before serialization. Gate C reopens the transformed file and requires that semantic fingerprint to match exactly. Gate C also computes and records the **post-write physical method-body SHA-256** as serialized evidence and requires it to differ from the unchanged source body fingerprint, but it no longer pretends that the physical offset-bearing hash can be predicted before `module.Write`.

The remaining Gate-C invariants are unchanged: source/transformed PrepareMethod references `10 / 0`, exact instruction-count delta, exact Pop delta, preserved exception-handler count, assembly identity/MVID preservation, zero resolver requests, and zero CLR admission.

## Regression rule

Static validation must require the explanatory serialized-offset guard, the offset-independent semantic comparison, and the post-write body-hash distinctness check. It must reject any return of `ExpectedTransformedBodySha256` as a pre-write transformation snapshot invariant.

## Architecture status

Unchanged. Step 32 remains the first real-StS2 launcher-private rewrite boundary under the physically closed Step-28 transform-before-load architecture. `MASTER-PLAN.md` does not change.
