# Step 32 — Physical iPhone Test

Version: `0.0.115 (115)`

Prerequisites: Codemagic static validation, complete host tests, iOS publish, and IPA verification all pass. Use the existing OfflineReady install and force-quit/relaunch before Step 32.

Run:

`Step 32 A–D — Clone Exact sts2.dll → Rewrite 10 PrepareMethod Calls → Reopen/Verify → Re-Prove Isolation`

Preserve:

`Documents/StS2Launcher/Reports/Step32-RealStS2PrepareMethodRewrite.txt`

Acceptance is **4/4 PASS**. Gate B must report 6/6 one-argument and 4/4 two-argument sites rewritten. Gate C must report source/transformed PrepareMethod references `10 / 0`, exact reopened semantic fingerprint match, unchanged source body fingerprint, and preserved assembly identity/MVID. Gate D must re-prove OfflineReady 428/428 on the current physical baseline and state that the trusted Step-12 install is unchanged and no real-StS2 CLR load/invocation occurred.

Do not start the game, load the transformed `sts2.dll` into the CLR, or combine this test with Godot/native loading. Those are later boundaries.
