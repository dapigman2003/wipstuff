# StS2 Launcher — Step 39.1

Active candidate: **0.0.169 (168)** — Gate-B lifecycle compatibility for the first real Godot `SceneTree` admission.

Physical **0.0.167 / Step 39.0** passed Gate A on-device, instantiated a fresh FMOD-neutral real `NGame` hierarchy off-tree, resolved the live `SceneTree.Root`, and then failed closed in Gate B before `AddChild`. The actual hierarchy's automatic lifecycle closure reached the two Steam Cloud capability probes in `SaveManager.ConstructDefault()` and game-owned Sentry/error-capture paths. The off-tree instance was released and no real SceneTree insertion occurred.

0.0.169 keeps Gate B fail-closed. In the launcher-private compatibility derivative only, the exact `SteamRemoteStorage.IsCloudEnabledForAccount()` and `IsCloudEnabledForApp()` probes in `SaveManager.ConstructDefault()` are forced to `false`; Steam initialization/native Steam remains deferred. The already-disabled game-owned `SentryService` surface and its nested helpers are replaced by inert default-return bodies, and compiler-generated void helpers that directly call external `Sentry.*` methods are inerted. The serialized image is reopened and verified before use.

Gate B then reruns against the real off-tree hierarchy. The exact sealed game-owned `SentryService` wrapper is permitted, while surviving external Sentry, Steamworks/SteamService, FMOD, Spine, later startup/platform, unresolved same-sts2, or Cecil external-resolution edges still fail before insertion.

Gate C remains exactly one `SceneTree.Root.AddChild(NGame)`. If it returns, the iOS caller synchronously stops rendering before recording/advancing. Gate D verifies frozen in-tree confinement. `RemoveChild`, `Free`, explicit `_ExitTree`, render restart, `GameStartup`, platform initialization, main-menu launch, `ExecuteDeferred`, Steam initialization/native calls, native FMOD/Spine/Sentry extensions, and gameplay remain forbidden.

See `docs/CURRENT-STATUS.md` for the exact physical sequence and `docs/history/steps/STEP-39.1-GATE-B-LIFECYCLE-COMPATIBILITY.md` for the candidate rationale.
