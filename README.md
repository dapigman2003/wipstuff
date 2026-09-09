# StS2 Launcher — Step 41.0

Active candidate: **0.0.173 (173)** — non-invoking GameStartup async frontier map.

Physical **0.0.170 / Step 39** closed real SceneTree admission **4/4**. A second physical **0.0.171 / Step 40** run then closed the controlled real-game render pulse **4/4**: the in-tree hierarchy passed its 68-node / 23-managed-type / 12-callback / 447-method audit, `StartRendering()` returned active, the first continuation arrived at 101.8 ms, `StopRendering()` returned inactive at 103.3 ms, and frozen NGame/state/native confinement held. The earlier 0.0.171 523.5 ms run remains useful timing evidence but is superseded for closure by the later 4/4 run.

**Step 41.0 does not invoke GameStartup.** It requires same-process Step-40 4/4 with rendering frozen, maps exact `NGame.GameStartup`, its `AsyncStateMachineAttribute`, compiler-generated state-machine fields and full `MoveNext` IL, then traverses the same-sts2 closure and records path-qualified platform/Steam/main-menu/deferred/FMOD/Spine/Sentry/native-facing boundaries. Gate D proves the map was runtime-inert. `GameStartupWrapper` remains inert and rendering remains stopped throughout.

Authoritative status and exact device sequence: `docs/CURRENT-STATUS.md`.
