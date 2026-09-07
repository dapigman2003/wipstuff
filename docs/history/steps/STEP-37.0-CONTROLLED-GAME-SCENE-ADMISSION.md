# Step 37.0 — controlled game-scene admission

Physical 0.0.159 closed Step 36.0.5 at 4/4. Unchanged `ExecuteEssential()` returned with `OneTimeInitialization` state `1 -> 2`, and the original post-ModelDb sequence (`ModelIdSerializationCache.Init`, `ModelDb.InitIds`, `MessageTypes.Initialize`, `ActionTypes.Initialize`) completed with final OfflineReady 428/428 and zero initializer-bearing, rejected-managed, or native-load escape.

0.0.160 freezes that physically proven authority and advances only to the first controlled Godot game-scene boundary.

## Exact sealed scene authority

`res://scenes/game.tscn` is extracted directly from the already receipt-backed PCK at runtime and must match:

- bytes: `10,414`
- SHA-256: `aaec1e04f689122fd812b83fee09ea6e30e2991cf5851dc599b01b7802320ad8`
- PCK MD5: `dc9a89798eb1bb38e23563866e746ac5`
- PCK format: `3`
- Godot engine: `4.5.1`
- PCK flags: `0x00000002`
- directory entries: `12,328`

## Bounded compatibility derivative

Only a launcher-owned copied scene is mutated. The trusted Step-12 install and the PCK are never edited.

Exactly three edits are authorized:

1. `FmodBankLoader` node type -> built-in `Node`
2. remove the `bank_paths` property from that inert node
3. `FmodListener2D` node type -> built-in `Node`

The node names and positions are preserved. `NGame`, `NAudioManager`, `NSceneContainer`, `NAssetLoader`, reaction-wheel, multiplayer-timeout and other scene dependencies remain unchanged.

## Gates

- **A** — require same-process Step-36.0.5 closure, extract/verify sealed `game.tscn`, structural audit, no scene load.
- **B** — create/read-back/hash the private three-edit derivative, no Godot load.
- **C** — exact prepared GodotSharp `ResourceLoader.Load(..., "PackedScene", CacheMode.Ignore)`, no instantiation.
- **D** — `PackedScene.Instantiate(GenEditState.Disabled)` off-tree; require managed `NGame` root, expected hierarchy, inert FMOD node types, `IsInsideTree == false`, state remains 2, zero initializer-bearing/rejected/native escape; release temporary instance.

## Still forbidden

No `AddChild`, `_EnterTree`/`_Ready` startup authorization, `NGame.GameStartup`, `LaunchMainMenu`, `ExecuteDeferred`, Steam initialization, native FMOD/Spine/Sentry GDExtension loading, gameplay, or broad native compatibility is authorized by Step 37.
