# Documentation index — active Step 42.0 / 0.0.175

Physical Step 41 is closed 4/4. Active Step 42 keeps rendering frozen, audits exact `NGame.InitPools()` to a zero-boundary same-sts2 closure, writes the map before execution, invokes `InitPools()` exactly once, and proves post-invocation frozen state/native confinement. See `CURRENT-STATUS.md`.

Active candidate: **0.0.175 (175)** — controlled GameStartup `NGame.InitPools()` boundary.

Physical **0.0.171 / Step 40** is now closed positive 4/4: a successful run completed the real render pulse at 103.3 ms, refroze rendering, retained the real NGame authority/state, and produced zero initializer/rejected/native deltas.

Physical Step 41.0 keeps the Step-39.1 compatibility image and inert `GameStartupWrapper` unchanged and is closed 4/4. Active Step 42.0 reuses that authority to audit and invoke only exact `NGame.InitPools()` once while rendering remains frozen.

Still closed: GameStartup execution as a whole, migrations/cloud sync, InitializePlatform/platform identity, Steam/native Steam, main-menu launch, ExecuteDeferred, native game GDExtensions, gameplay startup, explicit `_ExitTree`, RemoveChild/Free.

Authoritative status: `CURRENT-STATUS.md`.
