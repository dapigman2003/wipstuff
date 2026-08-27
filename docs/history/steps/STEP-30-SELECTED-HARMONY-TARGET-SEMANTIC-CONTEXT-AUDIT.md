# Step 30 — Selected Harmony Target Semantic Context Audit

## Purpose

Physical Step 29 selected one exact real-StS2 site, but the selected call occurs in `MegaCrit.Sts2.Core.Modding.ModManager::TryLoadMod(Mod)`. The project already separates Workshop/Harmony-mod compatibility from the base-game startup objective, so priority rank alone is not sufficient authorization to rewrite this site.

Step 30.0 / `0.0.113 (113)` is therefore a second read-only evidence boundary. It binds the exact physical Step-29 source/token/IL/target/body fingerprint, records the selected method's bounded IL/control-flow/exception context, and applies the predeclared product-scope disposition. It performs **zero real-game writes and zero real-game CLR execution**.

## Gates

### Gate A — SelectedEvidenceBindingAndOfflineReady

Re-prove OfflineReady, select the same receipt-backed ARM64 `sts2.dll`, and require the exact physical Step-29 source SHA-1/SHA-256/byte count/MVID/assembly identity. Re-find method token `0x06007927`, `IL_0D9D Callvirt -> Harmony.PatchAll(Assembly)`, and body SHA-256 `50c8...dbcbd`. Use `ReadingMode.Deferred` plus a rejecting resolver and require zero resolver requests / no CLR-resident `sts2`.

### Gate B — ExactSemanticContextAudit

Inspect only the exact selected method and report its method/body shape, selected call stack contract, a bounded 14-instruction-before/after IL window, branches targeting the selected instruction, exception regions covering it, nearby string literals, Harmony-reference count, and dynamic-assembly-load count. No external Cecil resolution, write, or CLR execution is allowed.

### Gate C — DeterministicDisposition

If and only if the selected site remains structurally `ModManager.TryLoadMod(Mod) -> Harmony.PatchAll(Assembly)`, record:

`DEFER — MOD/HARMONY COMPATIBILITY PATH; NO BASE-GAME REWRITE AUTHORIZED`

This is not a runtime-reachability claim. It is a product-scope decision: the selected Harmony mod-loading path is not allowed to become the first base-game semantic rewrite merely because Step 29 ranked Harmony runtime-patch calls first. The predeclared behavior change for this site is **NONE**.

The next evidence frontier after a physical Step-30 pass is the highest-priority non-mod family already present in Step-29 evidence: `RuntimeHelpers.PrepareMethod` calls in `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit()`. That family must receive its own exact semantic audit before any write.

### Gate D — FinalIsolationAudit

Re-hash the source, re-prove OfflineReady, require zero CLR-resident `sts2`, zero Cecil writes, zero resolver requests, no runtime Harmony/MonoMod patching, and no Godot/game/native loading.

## Close condition

Physical Step 30 closes only at **A–D / 4/4 PASS**. Preserve `Step30-SelectedTargetSemanticContextAudit.txt`.
