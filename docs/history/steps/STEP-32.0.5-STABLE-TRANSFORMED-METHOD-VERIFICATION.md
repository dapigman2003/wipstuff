# Step 32.0.5 — Stable Transformed Method Verification

Candidate: `0.0.120 (120)`

## Physical trigger

Physical `0.0.119 (119)` advanced Step 32 to **2/4**. Gate A passed on the exact receipt-backed source. Gate B then completed the first real-StS2 private semantic Cecil write: all 6 one-argument and 4 two-argument `RuntimeHelpers.PrepareMethod` sites were replaced as predeclared, the three audited external constant type/storage requirements were accepted, exactly nine write-time resolution requests stayed inside the exact System.Runtime/Sentry scopes, no external dependency bytes were opened, and the trusted/source images were not mutated.

Gate C failed immediately after reopening the transformed module with:

`Step-32 transformed PrewarmJit method identity/body drifted.`

The 0.0.119 verifier located the transformed method by the physical Step-31 **source MethodDef token** `0x06007D05`, then required that token occupant to still be `System.Void MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit()`. The report proves only that this post-write token lookup did not yield the expected body identity. It does not prove that the semantic rewrite drifted, because the semantic fingerprint, Constant-table fingerprint, instruction-count, EH-topology, PrepareMethod-count, and Pop-count checks had not yet run.

## Correction

Keep token `0x06007D05` authoritative for **source admission and pre-write binding**. Gate A and Gate B continue to require the exact Step-31 token, source type/signature, body fingerprint, 117-instruction/2-EH shape, ten exact PrepareMethod offsets/signatures, and zero incoming branches.

After `module.Write`, Gate C no longer treats the source MethodDef RID as transformed semantic identity. It instead requires exactly one method matching both:

- declaring type `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization`;
- full signature `System.Void MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit()`.

The reopened transformed method must still pass every stronger post-write invariant already present:

- exact pre-write transformed semantic fingerprint;
- zero `RuntimeHelpers.PrepareMethod` references;
- expected instruction count;
- unchanged exception-handler count/topology fingerprint inputs;
- exact `Pop` delta from the 6+4 rewrite;
- source/transformed Constant-table semantic fingerprint equality;
- transformed physical body fingerprint differs from the source body;
- assembly identity and MVID remain the physical Step-31 values;
- zero Cecil resolution during reopen;
- zero CLR admission.

Gate C additionally reports the transformed method token, whether `0x06007D05` survived serialization, and the method occupying the old source token. Token preservation is diagnostic only after write; stable type/signature plus the exact semantic fingerprint are the transformed identity contract.

## Non-changes

Step 32.0.5 does **not** change:

- the 6 + 4 PrepareMethod-to-Pop semantics;
- the exact audited System.Runtime/Sentry constant-metadata resolver authority;
- the source DLL identity or Gate-A source token requirement;
- trusted-install immutability;
- the no-real-StS2-CLR-load rule;
- Harmony/MonoMod retirement;
- trimming/linking policy;
- Godot/game/native-loading authorization.

Step 32 remains open until a physical candidate passes A–D **4/4**.
