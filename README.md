# StS2 Launcher — Steps 43–52 guarded direct main-menu ladder

Active candidate: **0.0.180 (180)** — ten independently gated startup/main-menu rungs in one IPA; stop on the first failure.

Physical **0.0.175 / Step 42** is closed positive **4/4**: exact `NGame.InitPools()` mapped to a 22-method zero-boundary closure, executed once on the real in-tree `NGame`, and returned with state 2, frozen rendering, and zero resolver/host/private/initializer/rejected/native deltas.

Physical **0.0.178** closed **Step 45 4/4**. Physical **0.0.179** then closed **Steps 46.1 and 47 4/4** and proved exact `NMainMenu` PackedScene instantiation succeeds once off-tree. Step 48 stopped safely before SceneTree admission because the static lifecycle frontier over-expanded conditional `_Ready()` branches into multiplayer/Steam and platform paths; no AddChild or rendering occurred.

0.0.180 keeps the successful direct-resource route and replaces that over-broad Step-48 audit with an exact runtime-guard rehearsal before any lifecycle admission:

**Step 43 Null-platform authority → Step 44 read-only legacy-data guard → Step 45 local SaveManager initialization → Step 46.1 non-invoking LaunchMainMenu frontier map → Step 47 exact main-menu resource preparation → Step 48.1 one-shot off-tree `NMainMenu` + runtime-guard rehearsal/lifecycle audit → Step 49 frozen `RootSceneContainer.AddChild(NMainMenu)` → Step 50 audited short render pulse → Step 51 non-invoking single-player handler frontier map → Step 52 audited 1.5-second render residency/refreeze.**

Step 48.1 does **not** whitelist Steam paths. It requires an empty Godot command line, the already-created production `SaveManager` singleton, exact Null-platform routing, a no-drift `SetRichPresence` rehearsal, and an off-tree no-drift `CheckCommandLineArgs()` rehearsal. Only those exact rehearsed calls may be treated as runtime-guarded frontiers; all other forbidden boundaries still fail closed.

Every rung has its own four-gate result and run-correlated evidence. Later rungs remain locked until the prior rung is 4/4 in the same process. Steps 43–49 and 51 keep rendering frozen. **Steps 50 and 52 alone may start rendering, and both synchronously stop rendering again before evaluating success.**

Original whole `GameStartup`, original `LaunchMainMenu`, `DoCloudSync`, migration mutation, `InitializePlatform`, `LoadDeferredStartupAssetsAsync`, `ExecuteDeferred`, external Steamworks/native Steam, and FMOD/Spine native game extensions remain unopened.

Authoritative status, exact boundaries, report names, and physical sequence: `docs/CURRENT-STATUS.md`.
