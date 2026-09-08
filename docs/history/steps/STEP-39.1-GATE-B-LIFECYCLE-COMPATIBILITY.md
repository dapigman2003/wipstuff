# Step 39.1 — Gate-B lifecycle compatibility

Candidate: **0.0.168 (168)**

Physical 0.0.167 reached Step-39 Gate B on a real iPhone. Gate A passed and a fresh real FMOD-neutral NGame hierarchy instantiated off-tree. The transitive same-sts2 lifecycle audit then failed closed before AddChild. The observed blockers were the exact `SaveManager.ConstructDefault()` calls to `Steamworks.SteamRemoteStorage.IsCloudEnabledForAccount()` and `IsCloudEnabledForApp()`, plus game-owned SentryService/error-capture closure from automatic `_Ready` / `_Notification` paths.

0.0.168 does not weaken the Steam/native/startup policy and does not mutate the trusted install. The selected private compatibility derivative forces only those two zero-argument Steam cloud capability probes to `false`, preserving local save/settings construction while Steam remains deferred. Because Sentry is an intentionally disabled subsystem, the game-owned `SentryService` method surface and nested helpers are sealed to inert default-return bodies; compiler-generated void helpers outside that type that directly call external `Sentry.*` methods are also inerted. The serialized derivative is reopened and checked before use.

Gate B remains fail-closed. It permits references to the exact game-owned `SentryService` wrapper only after the compatibility transform has sealed it inert. Any surviving external `Sentry.*`, Steamworks/SteamService, FMOD, Spine, later startup/platform, unresolved same-sts2, or Cecil external-resolution edge still rejects before AddChild.

Gate C and Gate D are unchanged: one real `SceneTree.Root.AddChild(NGame)`, immediate synchronous `StopRendering()` on return, then frozen in-tree confinement. GameStartup, platform initialization, main-menu launch, ExecuteDeferred, Steam initialization/native calls, native GDExtensions, gameplay, explicit exit, and render restart remain outside this candidate.
