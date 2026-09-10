# StS2 Launcher — Steps 43–52 guarded direct main-menu ladder

Active candidate: **0.0.182 (182)** — same independently gated Steps 43–52 ladder; engineering changes are intentionally limited to **Steps 49–52**. Step 48 is now physically closed 4/4.

Physical **0.0.175 / Step 42** is closed positive **4/4**. Physical **0.0.178** closed **Step 45 4/4**. Physical **0.0.179** closed **Steps 46.1 and 47 4/4**. Physical **0.0.181** then closed **Step 48 4/4** and advanced Step 49 through exact retained-root binding, exact trivial setter audit, null-property repair, and frozen `RootSceneContainer.AddChild(NMainMenu)` execution.

Step 49 stopped only during post-admission verification because the helper reused the historical Step-39 whole-NGame traversal, whose immutable diagnostic ceiling is 256 nodes. Once the real menu lifecycle expanded the SceneTree, that verification helper hit its ceiling. Rendering stayed frozen and the operation ended normally.

0.0.182 does **not** broaden runtime authorization. It improves only the current 49–52 portion:

**Step 49.1 exact immediate-child RootSceneContainer authority + richer post-AddChild checkpoints → Step 50 audited short render pulse/refreeze with a dedicated 4,096-node menu diagnostic traversal → Step 51 non-invoking single-player frontier map → Step 52 audited 1.5-second render residency/refreeze using the same dedicated menu traversal.**

The historical Step-39 256-node helper is left untouched. RootSceneContainer identity no longer traverses the admitted menu subtree at all: it requires exactly one immediate NGame child named `RootSceneContainer` of exact managed type `MegaCrit.Sts2.Core.Nodes.NSceneContainer`, exact direct parent NGame, `IsInsideTree=true`, and nullable-property consistency. Steps 50/52 use a separate bounded 4,096-node breadth-first traversal solely for menu diagnostics/audits.

Every rung retains independent four-gate results, durable evidence, same-process prerequisite authority, and stop-on-first-failure behavior. Steps 49 and 51 keep rendering frozen. **Steps 50 and 52 alone may start rendering, and both synchronously stop rendering again before evaluating success.**

Original whole `GameStartup`, original `LaunchMainMenu`, single-player handler invocation, `DoCloudSync`, migration mutation, `InitializePlatform`, `LoadDeferredStartupAssetsAsync`, `ExecuteDeferred`, external Steamworks/native Steam, and FMOD/Spine native game extensions remain unopened.

Authoritative status, exact boundaries, report names, and physical sequence: `docs/CURRENT-STATUS.md`.
