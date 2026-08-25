# Step 31 — Physical iPhone Test

Version: `0.0.114 (114)`

Prerequisites: Codemagic static validation, complete host tests, iOS publish, and IPA verification all pass. Use the existing OfflineReady install and force-quit/relaunch before running Step 31.

Run:

`Step 31 A–D — Bind PrewarmJit Evidence → Inspect 10 PrepareMethod Sites → Disposition → Re-Prove Isolation`

Preserve:

`Documents/StS2Launcher/Reports/Step31-PrepareMethodSemanticContextAudit.txt`

Required result:

`PREPAREMETHOD SEMANTIC CONTEXT AUDIT PASS — 4/4`

Gate C may record rewrite-design eligibility only if the exact source/method/body/site fingerprints still match. It must still say **NO WRITE AUTHORIZED**. If any fingerprint or site shape drifts, stop at the first failing gate and preserve the report rather than adapting the evidence in-place.
