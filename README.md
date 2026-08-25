# StS2 Launcher iOS — Step 31 PrepareMethod Semantic Context Audit

Steps 01–26 are physically closed. Step 27 is closed negative for runtime Harmony/MonoMod replacement. Step 28 is closed positive at **5/5** for deterministic transform-before-load + transformed-only interpreted execution. Step 29 is closed positive at **4/4** for exact real-StS2 target auditing. Step 30 is closed positive at **4/4** and formally deferred the selected Harmony/mod-loading site from the base-game frontier.

The next exact non-mod family from Step-29 evidence is `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit()` token `0x06007D05`, body SHA-256 `7f25b7bd955c407fc69306cf26af2162223353f5606560458066aed085e72ab9`, containing ten physically audited `RuntimeHelpers.PrepareMethod` sites.

## Active candidate

**Step 31.0 / `0.0.114 (114)` — PrepareMethod semantic context audit**

Step 31 is read-only. It re-binds the exact receipt-backed source/method/site evidence, records per-site IL/control-flow/exception context, and may classify the family as eligible for a later explicitly predeclared rewrite design. It performs **zero real-StS2 writes and zero real-StS2 CLR execution**.

Workflow: `ios-step-31`

Expected IPA: `artifacts/StS2-Launcher-Step-31.ipa`

Next authority: Codemagic static validation → complete host suite → iOS publish → IPA verification → physical Step 31 A–D **4/4 PASS**. Preserve `Step31-PrepareMethodSemanticContextAudit.txt`.
