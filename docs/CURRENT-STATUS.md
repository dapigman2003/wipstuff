# Current status

## Active candidate — Step 39.1 / 0.0.169 (169)

**Physical 0.0.168 / Step 39.1:** Gate A passed and Gate B again failed closed before AddChild, but the sole reported edge was `NGame._Notification -> LocManager.SmartFormat -> System.Action<Sentry.Scope>..ctor`. This is an audit-classification false positive: Cecil renders the generic argument inside the BCL delegate declaring type FullName, so the prior substring-based `Sentry` fragment matched `System.Action<Sentry.Scope>` even though the declaring type is `System.Action`, not Sentry. 0.0.169 changes only that classifier: exact transformed `SentryService`/nested boundaries remain allowed, actual declaring types in the `Sentry` namespace and other game-owned Sentry-named wrappers remain forbidden, and Steam/FMOD/Spine/startup/platform/native policies remain unchanged. The Step-39.1 compatibility transform itself is unchanged.

Physical **0.0.167 / Step 39.0** reached the real on-device Gate-B hierarchy audit and failed closed **before `SceneTree.Root.AddChild(NGame)`**. Gate A passed: the selected Step-38.2 compatibility authority and all four exact Step-39 PCK preflight resources reverified with zero resolver/native delta. Gate B instantiated a fresh FMOD-neutral real `NGame` hierarchy off-tree with `NGame.Instance == null`, resolved the live `SceneTree.Root`, and then rejected the transitive managed lifecycle closure because automatic `_Ready` / `_Notification` paths reached two deferred desktop/platform surfaces: `SaveManager.ConstructDefault()` referenced `Steamworks.SteamRemoteStorage.IsCloudEnabledForAccount()` / `IsCloudEnabledForApp()`, and save/localization error paths reached the game-owned `SentryService` wrapper plus compiler-generated Sentry callbacks. The off-tree instance was released; Gate C never started; no real SceneTree insertion evidence exists yet.

Physical **0.0.159** remains the Step 36.0.5 4/4 authority. Physical **0.0.161** remains the Step 37.0.1 4/4 authority. Physical **0.0.165 / Step 38.2** remains closed **4/4**: the exact compatibility `_EnterTree()` returned off-tree with `IsInsideTree=false`, state `2`, and zero resolver/initializer/rejected/native escape after the inert `GameStartupWrapper`, Sentry initialize suppression, and bounded GetWindow/FilesDropped/Connect suppression.

**0.0.169 / Step 39.1** keeps the Step-39 Gate-B policy fail-closed and changes only the selected private compatibility image needed to remove the exact 0.0.167 blockers. `SaveManager.ConstructDefault()` must contain exactly the two observed zero-argument `SteamRemoteStorage` cloud-capability probes; the transform substitutes those calls with `false`, preserving local save/settings construction while keeping Steam initialization and native Steam API use deferred. The game-owned `MegaCrit.Sts2.Core.Debug.SentryService` method surface and its nested helpers are replaced with inert default-return bodies because Sentry is already intentionally disabled on iOS. Compiler-generated helper methods outside `SentryService` that directly reference external `Sentry.*` methods are inerted only when they are compiler-generated and return `void`. Serialization is reopened and verified: the two Steam probes must be absent, the Sentry wrapper/helper bodies must remain inert, and external compiler-generated Sentry callbacks must have no surviving external Sentry method reference.

Gate B then reruns against the actual instantiated hierarchy. The exact `SentryService` wrapper reference is permitted only because the selected compatibility image seals that wrapper inert; any surviving external `Sentry.*`, Steamworks, SteamService, FMOD, Spine, later startup, or platform/native reference still fails closed. Gate C remains exactly one launcher-authorized `SceneTree.Root.AddChild(NGame)`. If Gate C returns, the iOS caller immediately calls `StopRendering()` before recording/advancing. Gate D requires the inserted hierarchy to remain authoritative and in-tree while rendering is frozen.

## Physical sequence for 0.0.169

1. Fresh process → Step 15 A-C.
2. Step 35.0.32 MODEL-BOOTSTRAP → require 4/4.
3. Step 36.0.5 → require 4/4.
4. Step 37.0.1 → require 4/4.
5. **Skip Step 38 in this process.** Physical Step 38.2 is prior evidence only.
6. Step 39.1 A-D once.
7. If Gate C starts, preserve all reports and relaunch; never retry Step 39 in-process.

Still forbidden: explicit `_ExitTree`, RemoveChild/Free after insertion, render restart, `GameStartup`, `InitializePlatform`, main-menu launch, `ExecuteDeferred`, Steam initialization/native Steam API loading, FMOD/Spine/native Sentry extensions, gameplay, state reset, and mutation of the trusted Step-12 install.
