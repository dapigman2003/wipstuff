# Step 30 — Physical iPhone Test

Version: `0.0.113 (113)`

Prerequisites: Codemagic static validation, complete host tests, iOS publish, and IPA verification all pass. Use the existing OfflineReady install and force-quit/relaunch before running Step 30.

Run:

`Step 30 A–D — Bind Step-29 Evidence → Inspect Exact Context → Disposition → Re-Prove Isolation`

Preserve:

`Documents/StS2Launcher/Reports/Step30-SelectedTargetSemanticContextAudit.txt`

Required result:

`SELECTED TARGET SEMANTIC CONTEXT AUDIT PASS — 4/4`

Gate C must not authorize a rewrite of the selected PatchAll site. Expected disposition, if the physical Step-29 fingerprint still matches, is:

`DEFER — MOD/HARMONY COMPATIBILITY PATH; NO BASE-GAME REWRITE AUTHORIZED`

If any earlier fingerprint or structural condition changes, stop at that gate and preserve the report rather than adapting the policy in-place.
