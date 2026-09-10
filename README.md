# StS2 Launcher — Steps 43–52 guarded direct main-menu ladder

Active candidate: **0.0.181 (181)** — same independently gated Steps 43–52 ladder; current engineering focus is Steps 48–52 only.

Physical **0.0.175 / Step 42** is closed positive **4/4**: exact `NGame.InitPools()` mapped to a 22-method zero-boundary closure, executed once on the real in-tree `NGame`, and returned with state 2, frozen rendering, and zero resolver/host/private/initializer/rejected/native deltas.

Physical **0.0.178** closed **Step 45 4/4**. Physical **0.0.179** then closed **Steps 46.1 and 47 4/4** and proved exact `NMainMenu` PackedScene instantiation succeeds once off-tree. Physical **0.0.180** passed the revised Step-48 Gate A/B and again instantiated exact real `NMainMenu` off-tree, then stopped safely during runtime-guard rehearsal because `NGame.RootSceneContainer` was null. No AddChild or rendering occurred.

0.0.181 does not expand the frontier. It focuses only on improving Steps 48–52:

**Established prerequisite ladder through Step 47 → Step 48.2 exact off-tree `NMainMenu` + retained-root runtime-guard rehearsal/lifecycle audit → Step 49 audited null-only `NGame.RootSceneContainer` repair to the exact retained child + frozen `AddChild(NMainMenu)` → Step 50 audited short render pulse → Step 51 non-invoking single-player handler frontier map → Step 52 audited 1.5-second render residency/refreeze.**

Step 48.2 still does **not** whitelist Steam paths. It now resolves the already-retained exact `/Game/RootSceneContainer` child directly from the NGame hierarchy instead of requiring the nullable `NGame.RootSceneContainer` property. A non-null property must already point to that exact child. Step 49 may repair a null property only after the exact setter is token-matched and proven to be a zero-call, single-`NSceneContainer`-field trivial assignment; any mismatched non-null property fails closed.

Every rung has its own four-gate result and run-correlated evidence. Later rungs remain locked until the prior rung is 4/4 in the same process. Steps 43–49 and 51 keep rendering frozen. **Steps 50 and 52 alone may start rendering, and both synchronously stop rendering again before evaluating success.**

Original whole `GameStartup`, original `LaunchMainMenu`, `DoCloudSync`, migration mutation, `InitializePlatform`, `LoadDeferredStartupAssetsAsync`, `ExecuteDeferred`, external Steamworks/native Steam, and FMOD/Spine native game extensions remain unopened.

Authoritative status, exact boundaries, report names, and physical sequence: `docs/CURRENT-STATUS.md`.
