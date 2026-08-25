# StS2 Launcher iOS — Step 29 Real StS2 Compatibility Target Audit

Steps 01–26 are physically closed. Step 27 is a **closed negative architecture result**: physical `0.0.108 (108)` proved runtime Harmony/MonoMod replacement is not viable on this host. Step 28 is now a **closed positive architecture result**: physical `0.0.111 (111)` passed A–E **5/5**, including transformed execution **1000 / 1041 / 1041**, transformed-only CLR admission, stable source/transformed hashes, and post-execution OfflineReady **428/428**.

Raw Step-28 closure evidence is preserved at `docs/history/reports/STEP-28.0.2-PHYSICAL-CLOSURE.txt`.

## Active candidate

**Step 29.0 / `0.0.112 (112)` — exact receipt-backed real-StS2 compatibility target audit**

The repository does not preserve the old physical Step-17 exact source→target call-site samples. Rather than guess the first semantic patch, Step 29.0 regenerates current evidence from the verified macOS ARM64 `sts2.dll`:

1. re-prove OfflineReady and admit only the exact receipt-backed primary assembly as deferred Cecil metadata;
2. fingerprint concrete compatibility-risk IL call sites without dependency resolution;
3. deterministically select at most one audit candidate with exact method token, IL offset/opcode, target scope/member and source method-body SHA-256;
4. re-hash source bytes and re-prove OfflineReady/isolation.

Step 29 performs **zero Cecil writes**, never CLR-loads or invokes `sts2.dll`, and does not run Harmony/MonoMod detours, Godot/game startup, or native game loading. A selected candidate is evidence for the next iteration, not authorization to rewrite it in this build.

## Build

Workflow: `ios-step-29`

Expected app version: `0.0.112 (112)`

Expected IPA: `artifacts/StS2-Launcher-Step-29.ipa`

Next authority: Codemagic static validation → complete host suite → iOS publish → IPA verification → physical Step 29 A–D **4/4 PASS**. Preserve `Step29-RealStS2CompatibilityTargetAudit.txt` as the authority for the next semantic-transformation design.
