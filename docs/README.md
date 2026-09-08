# StS2 Launcher — Step 39.2

Active candidate: **0.0.170 (170)** — Gate-D frozen inserted-tree confinement correction.

Physical **0.0.169 / Step 39.1** closed the unknown real-insertion question: Gate B passed on the actual 69-node hierarchy, and `SceneTree.Root.AddChild(NGame)` returned successfully on-device with exact singleton/parent/window/state authority and zero rejected/initializer/native escape. Rendering then stopped synchronously as designed.

The only failure was after that successful insertion/freeze, inside Gate-D launcher bookkeeping: reflection selected zero-argument `GetParent` with `Single(...)`, but Godot exposes more than one matching method shape. **0.0.170 changes only that selector** to require a non-generic, closed method whose return type can represent the live `SceneTree.Root`.

No compatibility-image semantics are changed from the physically proven 0.0.169 run. Gate B remains fail-closed; GameStartup, platform initialization, main-menu/deferred startup, Steam/native GDExtensions, explicit `_ExitTree`, RemoveChild/Free, and render restart remain unauthorized.

Authoritative status: `CURRENT-STATUS.md`. Historical design/evidence remains under `history/`.
