# Release Checklist — Step 28.0.1

## Source / policy

- Steps 01–26 remain closed/protected.
- Preserve the exact physical Step-27.0.24 / 0.0.108 negative report and closure note.
- Preserve the 0.0.109 Codemagic Core compile failure as compile-only evidence; do not reinterpret it as a Step-28 runtime failure.
- Runtime Harmony/MonoMod replacement is retired from the active architecture; Step 28 production code must not invoke Harmony patch APIs.
- `MtouchLink=None`, `TrimMode=copy`, and `MtouchInterpreter=-all` remain active; broad `UseInterpreter=true` and NativeAOT remain prohibited.
- The Step-28 source fixture is launcher-owned, IL-only, framework-only, and absent from the iOS/test ProjectReference graph.
- Build/copy ordering must prove the Step-28 fixture enters the `.app` only after `dotnet publish`.
- Gate A requires OfflineReady, exact source SHA-256/IL, a private source clone, and no already-loaded Step-28 fixture identity.
- The Gate-A OfflineReady progress bridge uses the private `CallbackProgress<T> : IProgress<T>` adapter; its constructor/forwarding behavior is statically guarded.
- Gate B rewrites only the private transformed copy: `Adjustment() 1 -> 1000`.
- Gate C reopens source/transformed images and verifies source remains 1, transformed is 1000, and direct-call topology is unchanged.
- Gate D loads only transformed bytes into a dedicated private ALC and requires `Adjustment()==1000`, `Target(41)==1041`, `InvokeTarget(41)==1041`.
- Gate E re-hashes all images, re-proves OfflineReady, and requires exactly one Step-28 fixture identity in the dedicated context with no unexpected private dependency fallback.
- Source/live game bytes remain immutable.
- No real StS2 member reflection/rewrite/invocation; no Godot/game startup; no native game-library loading.
- `MASTER-PLAN.md` remains unchanged for 0.0.110 because this is a compile correction only; update it only if architecture, methodology, major roadmap, or end-state assumptions actually change.

## Build identity

- step/candidate: **Step 28.0.1**
- version: `0.0.110 (110)`
- workflow: `ios-step-28`
- IPA: `artifacts/StS2-Launcher-Step-28.ipa`
- TRX: `artifacts/test-results/step28.trx`
- top launcher banner: **Step 28.0.1**, bundle-derived **Version 0.0.110**, preserved Step-27 negative closure, preserved 0.0.109 compile stop, and unchanged transformed-only execution architecture.

## Pre-device authority

- Canonical static validation: expected **850/850 PASS**.
- Core/test compilation: PASS.
- Complete host regression suite: PASS.
- iOS publish: PASS.
- IPA verification: PASS.
- If any stage fails, preserve the raw artifact, classify the first failing boundary, make the smallest correction, bump candidate identity, and do not change Step-28 semantics unless evidence requires it.

## Device-run discipline

- Force-quit/relaunch before the run.
- Gate A must report that the Step-28 fixture identity is not CLR-loaded.
- Gate B/C must report distinct transformed SHA-256 while source/bundle SHA-256 stay unchanged.
- Gate D must report 1000 / 1041 / 1041 for Adjustment / Target / InvokeTarget.
- After Gate D, force-quit before any retry because the Step-28 fixture identity remains process-resident.
- Preserve `Step28-AheadOfLoadManagedTransformation.txt` after the run.

## Authority

Physical iPhone remains final runtime authority. Step 28 closes only when A–E reach **5/5 PASS** and Gate E re-proves OfflineReady.
