# StS2 Launcher — Step 37.0.1

Active candidate: **0.0.161 (161)** — controlled `game.tscn` admission after physical Step-36 closure.

Physical **0.0.159** closed Step 36.0.5 at **4/4**. The dependency-aware ModelDb compatibility authority completed the full unchanged `ExecuteEssential()` call, advanced `OneTimeInitialization` state `1 -> 2`, completed `ModelIdSerializationCache.Init`, `ModelDb.InitIds`, `MessageTypes.Initialize`, and `ActionTypes.Initialize`, then re-proved OfflineReady `428/428` with no initializer-bearing, rejected-managed, or native-load escape.

Codemagic **0.0.160** then stopped before iOS publish on a single C# parser error in the new Gate-D checkpoint interpolation. **0.0.161 / Step 37.0.1** corrects only that syntax; the sealed scene authority, FMOD-neutral derivative, PackedScene load, off-tree instantiation boundary, and caches are unchanged.

Step 37 freezes that successful authority. It extracts the exact receipt-backed `res://scenes/game.tscn` from the mounted PCK, requires the sealed 10,414-byte SHA-256/MD5/layout, and writes only a private compatibility copy. Exactly three scene edits are authorized: `FmodBankLoader` type -> `Node`, remove its `bank_paths` property, and `FmodListener2D` type -> `Node`.

Gate C loads only that copied scene as a `PackedScene` with cache-ignore. Gate D instantiates it **off-tree**, verifies an `NGame` root plus the expected managed hierarchy and inert FMOD placeholder nodes, requires `IsInsideTree == false`, then releases the temporary instance.

`SceneTree.AddChild`, `NGame.GameStartup`, main-menu launch, `ExecuteDeferred`, Steam initialization, FMOD/Spine/Sentry native GDExtensions, gameplay, runtime Harmony/MonoMod, arbitrary resolver fallback, retry, and state reset remain forbidden.

## Physical test sequence

1. Fresh process: Step 15 Gates A-C.
2. Step 35.0.32 **MODEL-BOOTSTRAP** once; require 4/4.
3. Step 36.0.5 once; require 4/4.
4. Step 37.0.1 Gates A-D once. If Gate C or D begins and fails, use a fresh process before retry.
5. Preserve the Step37 checkpoint/static-map/final-report artifacts.

Codemagic workflow key remains `ios-canonical`; existing NuGet, .NET, Godot Step-15, and iOS intermediate cache paths are unchanged.
