# StS2 Launcher — Step 39.1

Active candidate: **0.0.169 (168)** — Gate-B lifecycle compatibility before the first real Godot `SceneTree` insertion.

Physical **0.0.167** passed Step-39 Gate A and failed closed in the actual-hierarchy Gate-B lifecycle audit before `AddChild`. The observed blockers were the exact two `SteamRemoteStorage` cloud-capability probes reached through `SaveManager.ConstructDefault()` and Sentry wrapper/callback paths reached from automatic `_Ready` / `_Notification` closure.

0.0.169 preserves the existing Step-36/37/38 compatibility work and adds only a launcher-private Step-39.1 lifecycle-admission extension: force those two Steam cloud probes false, seal the already-disabled game-owned `SentryService` and nested helpers inert, and inert compiler-generated void helpers that directly call external `Sentry.*` methods. The derivative is reopened and verified before Gate B.

Gate B remains fail-closed for surviving external Sentry, Steamworks/SteamService, FMOD, Spine, later startup/platform/native, unresolved same-sts2, or Cecil external-resolution edges. Gate C/D semantics are unchanged: one real `SceneTree.Root.AddChild(NGame)`, immediate synchronous render freeze on return, then frozen in-tree confinement.

Authoritative status: `CURRENT-STATUS.md`. Historical design/evidence: `history/steps/STEP-39.1-GATE-B-LIFECYCLE-COMPATIBILITY.md` and the 0.0.167 Step-39 physical Gate-B reports under `history/reports/`.
