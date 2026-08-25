# Step 29.0 — Physical Closure

Physical `0.0.112 (112)` closed the read-only real-StS2 compatibility target-audit boundary at **4/4 PASS**.

The preserved raw report is `docs/history/reports/STEP-29.0-PHYSICAL-CLOSURE.txt`.

## Decisive evidence

- exact receipt-backed source: `SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll`;
- source SHA-256: `e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18`;
- MVID: `518e4758-52d7-47c2-b776-471a0e29e49d`;
- OfflineReady passed before and after the audit at `428/428` files;
- Cecil used `ReadingMode.Deferred` with zero dependency-resolution requests;
- zero Cecil writes and zero real-StS2 CLR load/invocation occurred;
- Gate B inspected 48,970 methods / 1,395,517 IL instructions / 326,145 concrete method-reference sites and found 50 bounded compatibility candidates;
- Gate C deterministically selected the highest-priority exact site:
  - source: `MegaCrit.Sts2.Core.Modding.ModManager::TryLoadMod(MegaCrit.Sts2.Core.Modding.Mod)`;
  - token: `0x06007927`;
  - site: `IL_0D9D Callvirt`;
  - target: `[0Harmony] System.Void HarmonyLib.Harmony::PatchAll(System.Reflection.Assembly)`;
  - source method-body SHA-256: `50c8c4394082f3c73df414fad8675540cfc00a99ccc4f350b616cec574cdbcbd`.

## Scope decision

Step 29 selected **audit evidence only**. It did not authorize a rewrite. The next candidate must bind this exact physical evidence to the same source and inspect the method's surrounding semantics before any real-game Cecil write.
