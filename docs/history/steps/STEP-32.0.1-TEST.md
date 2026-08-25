# Step 32.0.1 — Physical iPhone Test

Version: `0.0.116 (116)`

Prerequisites: Codemagic static validation, the complete host suite, iOS publish, and IPA verification must all pass. The host regression `ExactPrewarmJitPrepareMethodFamilyIsRewrittenOnPrivateCopyOnly` must specifically pass through Gate C using the corrected serialization verification model.

Force-quit/relaunch, then run:

`Step 32 A–D — Clone Exact sts2.dll → Rewrite 10 PrepareMethod Calls → Reopen/Verify → Re-Prove Isolation`

Preserve:

`Documents/StS2Launcher/Reports/Step32-RealStS2PrepareMethodRewrite.txt`

Acceptance remains **4/4 PASS**. Gate B must report 6/6 one-argument and 4/4 two-argument sites rewritten. Gate C must report source/transformed PrepareMethod references `10 / 0`, the reopened offset-independent semantic fingerprint matching the exact in-memory pre-write plan, a distinct post-write transformed method-body fingerprint, unchanged source body fingerprint, and preserved assembly identity/MVID. Gate D must re-prove OfflineReady and state that the trusted Step-12 install is unchanged and no real-StS2 CLR load/invocation occurred.

Do not start the game or CLR-load the transformed `sts2.dll` in this candidate.
