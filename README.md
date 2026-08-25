# StS2 Launcher iOS — Step 30 Selected Harmony Target Semantic Context Audit

Steps 01–26 are physically closed. Step 27 is closed negative for runtime Harmony/MonoMod replacement. Step 28 is closed positive at **5/5** for deterministic transform-before-load + transformed-only interpreted execution. Step 29 is now closed positive at **4/4** for exact receipt-backed real-StS2 target auditing/selection.

Physical Step 29 selected exactly `MegaCrit.Sts2.Core.Modding.ModManager::TryLoadMod(Mod)` token `0x06007927`, `IL_0D9D Callvirt -> Harmony.PatchAll(Assembly)`, body SHA-256 `50c8c4394082f3c73df414fad8675540cfc00a99ccc4f350b616cec574cdbcbd`.

## Active candidate

**Step 30.0 / `0.0.113 (113)` — selected-target semantic context audit**

Step 30 remains read-only. It re-binds the exact physical Step-29 source evidence, records the selected method's exact bounded IL/control-flow/exception context, and applies the existing product boundary that Harmony/mod compatibility is later and must not block base-game startup. If the selected site remains structurally `ModManager.TryLoadMod(Mod) -> Harmony.PatchAll(Assembly)`, Gate C must defer it and authorize **no rewrite**.

Workflow: `ios-step-30`

Expected IPA: `artifacts/StS2-Launcher-Step-30.ipa`

Next authority: Codemagic static validation → complete host suite → iOS publish → IPA verification → physical Step 30 A–D **4/4 PASS**. Preserve `Step30-SelectedTargetSemanticContextAudit.txt`.
