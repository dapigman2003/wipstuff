# Release checklist — Steps 43–52 guarded direct main-menu ladder / 0.0.180

Release identity: display/build `0.0.180 (180)`, IPA `StS2-Launcher-Steps-43-52.ipa`, workflow `ios-canonical`.

Require physical Step-42 4/4 provenance sealed and physical 0.0.178 Step-45 closure preserved.

Require physical 0.0.179 Step-48 evidence sealed and interpreted correctly: Steps 46.1/47 passed; exact `NMainMenu` instantiation started and returned off-tree; the old lifecycle audit failed before AddChild/rendering on conditional `_Ready()` paths; process ended normally.

Require generic four-gate sequence with stable ordinals and separately exposed rungs 43–52. Same-process prior-rung authority and fail-stop behavior are mandatory. Mutating/rendering boundaries must be one-shot where applicable.

Require Step48.1 runtime-guard rehearsal before lifecycle admission: `Godot.OS.GetCmdlineArgs()` must be empty; exact production `SaveManager._instance` must already exist with `_mockInstance` null; exact `SaveManager.get_Instance()` must return that same instance; `PrimaryPlatform` must resolve to exact `NullPlatformUtilStrategy`; exact `PlatformUtil.SetRichPresence` must return with zero context/native/tree drift; exact off-tree `NMainMenu.CheckCommandLineArgs()` must return with menu/root child counts unchanged. The guard-aware auditor may prune only those exact three rehearsed immediate-call surfaces and may not generically whitelist platform/Steam.

Require Step49 one-shot exact frozen `RootSceneContainer.AddChild(NMainMenu)` with exact parent/child-count/in-tree/state/native confinement.

Require Step50 actual in-tree frame/input frontier audit, durable map before `StartRendering`, exactly one short pulse, synchronous `StopRendering` before post-stop telemetry/file I/O, and frozen post-pulse authority.

Require Step51 to map exact `SingleplayerButtonPressed` and `OpenSingleplayerSubmenu` IL/frontiers without invoking either handler and without rendering. Classified frontiers are evidence only; unresolved/external Cecil resolution remains fatal.

Require Step52 fresh in-tree frame/input audit, durable map before rendering, one-shot requested 1500 ms residency with 6000 ms evidence ceiling, synchronous `StopRendering` before post-stop telemetry/file I/O, zero initializer/rejected/native escape, and exact frozen post-pulse authority.

Require rendering calls isolated to Steps 50 and 52; Steps 43–49 and 51 must contain no rendering control calls.

Require no direct whole `GameStartup`, original `LaunchMainMenu`, `DoCloudSync`, migration/archive mutation, `InitializePlatform`, `LoadDeferredStartupAssetsAsync`, `ExecuteDeferred`, external Steamworks/native Steam, or FMOD/Spine native game-extension execution in the active ladder UI/control path.

Require exact version/build `0.0.180/180` in csproj/plist/release presentation/current-release shell constants; Codemagic/build/test/IPA-verifier headings and filenames updated to Steps 43–52; active candidate manifests regenerated; protected manifests unchanged; no proprietary StS2/native payload; exact `history.zip`; final ZIP integrity clean; canonical validator green from both the release tree and a completely fresh extraction.
