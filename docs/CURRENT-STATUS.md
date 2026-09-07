# Current status

## Active candidate — Step 37.0 / 0.0.160 (160)

Physical **0.0.159 / Step 36.0.5** is closed positive **4/4**. The exact dependency-aware ModelDb derivative completed unchanged `ExecuteEssential()`, state `1 -> 2`, and the full post-ModelDb sequence `ModelIdSerializationCache.Init -> ModelDb.InitIds -> MessageTypes.Initialize -> ActionTypes.Initialize`. Final isolation re-proved OfflineReady `428/428`; initializer-bearing requests, rejected managed requests, and native-load attempts remained zero.

The physically validated ModelDb derivative is now frozen at SHA-256 `e9eea7be01d1c7bc77371b43c24b7c60e599c660c6541bde65772ff8c400092d` with the audited `1624/56/47/41` model graph and same-instance canonical-order strategy.

**0.0.160 / Step 37.0** opens the next boundary without invoking deferred/game startup. The exact PCK `res://scenes/game.tscn` is sealed at 10,414 bytes, SHA-256 `aaec1e04f689122fd812b83fee09ea6e30e2991cf5851dc599b01b7802320ad8`, PCK MD5 `dc9a89798eb1bb38e23563866e746ac5`. A private copy receives exactly three edits: neutralize `FmodBankLoader`, remove its desktop `bank_paths`, and neutralize `FmodListener2D`.

Step 37 then loads the copy as `Godot.PackedScene` and, only after that succeeds without resolver/native escape, instantiates it off-tree. The expected root is `MegaCrit.Sts2.Core.Nodes.NGame`; the audit requires the expected managed AudioManager/SceneContainer/AssetLoader hierarchy, inert FMOD placeholders, `IsInsideTree == false`, unchanged OneTimeInitialization state 2, and no initializer-bearing/rejected/native escape.

The macOS app inventory contains desktop FMOD/Sentry/Spine frameworks only; no iOS/iphone/XCFramework payload is shipped in this app. Those binaries remain analysis-only and are not part of the launcher archive.

ExecuteDeferred, `NGame.GameStartup`, `LaunchMainMenu`, SceneTree insertion, Steam initialization, native GDExtension loading, gameplay, runtime Harmony/MonoMod, retry, and state reset remain forbidden.

## Physical sequence for 0.0.160

1. Fresh process: Step 15 Gates A-C.
2. Step 35.0.32 MODEL-BOOTSTRAP once; require 4/4.
3. Step 36.0.5 once; require 4/4.
4. Step 37.0 A-D once; if Gate C/D begins and fails, do not retry in-process.
5. Preserve Step35/Step36/Step37 run-correlated reports.
