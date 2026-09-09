# StS2 Launcher — Step 41.0

Active candidate: **0.0.173 (173)** — exact GameStartup async frontier map, non-invoking.

Physical **0.0.171 / Step 40** is now closed positive 4/4: a successful run completed the real render pulse at 103.3 ms, refroze rendering, retained the real NGame authority/state, and produced zero initializer/rejected/native deltas.

Step 41.0 keeps the Step-39.1 compatibility image and inert `GameStartupWrapper` unchanged. It never restarts rendering and never calls GameStartup. Instead it maps exact `NGame.GameStartup` metadata/IL, its compiler-generated async state machine and `MoveNext`, then maps the transitive same-sts2 startup closure with path-qualified boundary classification.

Still closed: GameStartup execution, InitializePlatform, Steam/native Steam, main-menu launch, ExecuteDeferred, native game GDExtensions, gameplay startup, explicit `_ExitTree`, RemoveChild/Free.

Authoritative status: `CURRENT-STATUS.md`.
