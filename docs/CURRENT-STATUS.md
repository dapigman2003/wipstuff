# Current status

## Active candidate — Step 38.0 / 0.0.162 (162)

Physical **0.0.159** closed unchanged `ExecuteEssential()` at Step 36.0.5 4/4. The physically validated ModelDb derivative remains frozen at SHA-256 `e9eea7be01d1c7bc77371b43c24b7c60e599c660c6541bde65772ff8c400092d` with the audited `1624/56/47/41` model graph.

Physical **0.0.161** now closes Step 37.0.1 at **4/4**. The exact `game.tscn` authority matched 10,414 bytes / SHA-256 `aaec1e04f689122fd812b83fee09ea6e30e2991cf5851dc599b01b7802320ad8` / PCK MD5 `dc9a89798eb1bb38e23563866e746ac5`; the copied FMOD-neutral derivative loaded as `PackedScene`; and a real `NGame` hierarchy instantiated off-tree with `FmodBankLoader`/`FmodListener2D` as inert `Godot.Node`. Gate C/D had resolver/host/private/initializer/rejected/native deltas of zero and the run ended normally.

**0.0.162 / Step 38.0** advances only `NGame._EnterTree`. Before execution, Gate A Cecil-maps `_EnterTree`, `_Ready`, `GameStartupWrapper`, `GameStartup`, `InitializePlatform`, `LaunchMainMenu`, and `LoadDeferredStartupAssetsAsync` from the exact selected compatibility image and rejects a same-`NGame` `_EnterTree` call closure that reaches later startup/deferred or OneTimeInitialization re-entry.

Gate B re-instantiates the physically proven scene off-tree. Gate C invokes `_EnterTree()` exactly once by reflection while `IsInsideTree == false`; no `AddChild` occurs. Gate D re-proves off-tree/state-2 confinement and zero initializer/rejected/native escape, then frees the node without invoking `_ExitTree`.

## Physical sequence for 0.0.162

1. Fresh process → Step 15 A-C.
2. Step 35.0.32 MODEL-BOOTSTRAP → 4/4.
3. Step 36.0.5 → 4/4.
4. Step 37.0.1 → 4/4.
5. Step 38.0 A-D once. Do not retry in-process after Gate C starts.
6. Preserve Step35/Step36/Step37/Step38 run-correlated reports.

Still forbidden: SceneTree insertion, `_Ready`, `_ExitTree`, `GameStartup`, `InitializePlatform`, main-menu launch, `ExecuteDeferred`, Steam initialization, native FMOD/Spine/Sentry extensions, gameplay, broad native compatibility, and mutation of the trusted Step-12 install.
