# Current status

## Active candidate — Step 39.2 / 0.0.170 (170)

**Physical 0.0.169 / Step 39.1:** Gates A/B/C passed. The audited 69-node hierarchy had 23 selected managed node types, 26 immediate callbacks, 557 same-sts2 closure methods, and zero forbidden/unresolved lifecycle references. The first real `SceneTree.Root.AddChild(NGame)` returned successfully with `IsInsideTree=True`, exact `NGame.Instance`, non-null `_window`, parent authority, state 2, and zero rejected/initializer/native escape; rendering then stopped synchronously. Gate D failed only in launcher reflection bookkeeping with `MoreThanOneMatch` while selecting zero-argument `GetParent`. 0.0.170 changes only that Gate-D reflection selector to require the non-generic root-compatible overload; Gate A/B/C and compatibility-image semantics are unchanged.

**Physical 0.0.168 / Step 39.1:** Gate A passed and Gate B again failed closed before AddChild, but the sole reported edge was `NGame._Notification -> LocManager.SmartFormat -> System.Action<Sentry.Scope>..ctor`. This is an audit-classification false positive: Cecil renders the generic argument inside the BCL delegate declaring type FullName, so the prior substring-based `Sentry` fragment matched `System.Action<Sentry.Scope>` even though the declaring type is `System.Action`, not Sentry. 0.0.170 changes only that classifier: exact transformed `SentryService`/nested boundaries remain allowed, actual declaring types in the `Sentry` namespace and other game-owned Sentry-named wrappers remain forbidden, and Steam/FMOD/Spine/startup/platform/native policies remain unchanged. The Step-39.1 compatibility transform itself is unchanged.

Physical **0.0.167 / Step 39.0** reached the real on-device Gate-B hierarchy audit and failed closed **before `SceneTree.Root.AddChild(NGame)`**. Gate A passed: the selected Step-38.2 compatibility authority and all four exact Step-39 PCK preflight resources reverified with zero resolver/native delta. Gate B instantiated a fresh FMOD-neutral real `NGame` hierarchy off-tree with `NGame.Instance == null`, resolved the live `SceneTree.Root`, and then rejected the transitive managed lifecycle closure because automatic `_Ready` / `_Notification` paths reached two deferred desktop/platform surfaces: `SaveManager.ConstructDefault()` referenced `Steamworks.SteamRemoteStorage.IsCloudEnabledForAccount()` / `IsCloudEnabledForApp()`, and save/localization error paths reached the game-owned `SentryService` wrapper plus compiler-generated Sentry callbacks. The off-tree instance was released; Gate C never started; no real SceneTree insertion evidence exists yet.

Physical **0.0.159** remains the Step 36.0.5 4/4 authority. Physical **0.0.161** remains the Step 37.0.1 4/4 authority. Physical **0.0.165 / Step 38.2** remains closed **4/4**: the exact compatibility `_EnterTree()` returned off-tree with `IsInsideTree=false`, state `2`, and zero resolver/initializer/rejected/native escape after the inert `GameStartupWrapper`, Sentry initialize suppression, and bounded GetWindow/FilesDropped/Connect suppression.

**0.0.170 / Step 39.2** is a Gate-D-only correction over the physically exercised 0.0.169 image. The Step-39.1 private compatibility transform is unchanged: the exact two `SteamRemoteStorage` cloud-capability probes in `SaveManager.ConstructDefault()` remain forced false; the game-owned Sentry boundary remains inert and serialization-verified; the actual hierarchy audit remains fail-closed for surviving Steam/Sentry/FM0D/Spine/startup/platform/native edges. The only runtime-source delta is Gate-D reflection selection after successful insertion: instead of `Single(...)` across all zero-argument `GetParent` methods, the launcher selects the non-generic, closed overload whose return type can represent the live `SceneTree.Root`, or fails explicitly with candidate diagnostics.

Gate B then reruns against the actual instantiated hierarchy. The exact `SentryService` wrapper reference is permitted only because the selected compatibility image seals that wrapper inert; any surviving external `Sentry.*`, Steamworks, SteamService, FMOD, Spine, later startup, or platform/native reference still fails closed. Gate C remains exactly one launcher-authorized `SceneTree.Root.AddChild(NGame)`. If Gate C returns, the iOS caller immediately calls `StopRendering()` before recording/advancing. Gate D requires the inserted hierarchy to remain authoritative and in-tree while rendering is frozen.

## Physical sequence for 0.0.170

1. Fresh process → Step 15 A-C.
2. Step 35.0.32 MODEL-BOOTSTRAP → require 4/4.
3. Step 36.0.5 → require 4/4.
4. Step 37.0.1 → require 4/4.
5. **Skip Step 38 in this process.** Physical Step 38.2 is prior evidence only.
6. Step 39.1 A-D once.
7. If Gate C starts, preserve all reports and relaunch; never retry Step 39 in-process.

Still forbidden: explicit `_ExitTree`, RemoveChild/Free after insertion, render restart, `GameStartup`, `InitializePlatform`, main-menu launch, `ExecuteDeferred`, Steam initialization/native Steam API loading, FMOD/Spine/native Sentry extensions, gameplay, state reset, and mutation of the trusted Step-12 install.
