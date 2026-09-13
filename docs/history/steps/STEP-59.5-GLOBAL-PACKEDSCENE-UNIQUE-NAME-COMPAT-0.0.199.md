# Step 59.5 — global PackedScene instanced-root unique-name compatibility — 0.0.199

Physical **0.0.198 (198)** closes the provenance question that Steps 59.2–59.4 were designed to answer.

The exact parent scene data is correct:

- character-select `SceneState` has `AscensionPanel` and `ActDropdown` as `instance=YES:Godot.PackedScene` with
  `unique_name_in_owner=True`;
- game `SceneState` has `RootSceneContainer`, `ReactionWheel`, and `MultiplayerTimeoutOverlay` as instanced roots
  with `unique_name_in_owner=True`;
- main-menu `SceneState` has `MainMenuBg`, `ContinueRunInfo`, and `PatchNotesScreen` as instanced roots with
  `unique_name_in_owner=True`.

A fresh temporary **off-tree** `PackedScene.Instantiate()` already loses those parent-scene overrides:

- `AscensionPanel=false`;
- `ActDropdown=false`;
- `RootSceneContainer=false`;
- `ReactionWheel=false`;
- `MultiplayerTimeoutOverlay=false`.

By contrast, ordinary non-instanced nodes in the same scenes preserve the serialized property:

- `InputManager=true`;
- `HotkeyManager=true`;
- `ReactionContainer=true`;
- `WorldEnvironment=true`;
- main-menu `Submenus=true`.

The retained live instances have the same pattern as the temporary off-tree instances. Therefore:

> the defect occurs during `PackedScene` instantiation/application of parent-scene properties to **roots of
> instanced subscenes**, before SceneTree admission and before StS2 managed `_Ready()` callbacks.

This explains the observed downstream nulls without making them separate game bugs:

- `NGame.TimeoutOverlay == null`;
- `NCharacterSelectScreen._ascensionPanel == null`;
- `NCharacterSelectScreen._actDropdown == null`;
- later `_randomCharacterButton` never reaches its normal completed state.

## 0.0.199 correction

0.0.199 moves from forensic observation to one **central compatibility boundary** in the private Godot managed bridge.

A separately hash-pinned derivative of the exact prepared `GodotSharp` image wraps the managed
`Godot.PackedScene.Instantiate(GenEditState)` return path. Before the new scene root is returned to StS2 or launcher
code, a host callback:

1. reads the same `PackedScene.GetState()` that physical 0.0.198 proved authoritative;
2. considers only SceneState nodes where:
   - `GetNodeInstance(index)` is non-null, so the node is an instanced-subscene root; and
   - the parent scene explicitly stores `unique_name_in_owner=true`;
3. enumerates the newly created runtime hierarchy using `GetChildren()` + `Name`, avoiding the unrelated
   reflection-time `Godot.NodePath` TypeLoad noise observed in 0.0.198;
4. finds the exact runtime node by the serialized relative path;
5. sets only that node's `UniqueNameInOwner` property to `true` when it is currently false;
6. rereads the property and fails closed if the correction did not stick.

The original exact prepared GodotSharp bytes remain unchanged. The derivative preserves assembly identity and MVID
and is selected only as the private runtime compatibility authority for the existing model-bootstrap path.

This is deliberately **not** a screen repair:

- no `_ascensionPanel`, `_actDropdown`, `TimeoutOverlay`, submenu, or other game field is written;
- no `_Ready()` callback is manually replayed;
- no scene owner is changed;
- no ordinary/non-instanced node is modified;
- no TSCN/PCK bytes are modified;
- no trusted install file is mutated;
- the correction applies to NGame, main menu, character select, and later scenes through the same PackedScene
  compatibility rule.

The existing Step-59 forensic snapshot remains active after the compatibility correction. It must prove that the
previously missing bindings are now healthy before the original `OpenCharacterSelect` transition is authorized.
If the compatibility derivative exposes a different earlier lifecycle boundary, the ladder still stops on that first
failure.

This is the first Step-59 correction that belongs directly to the intended long-term launcher architecture: a narrow,
deterministic host compatibility substitution underneath normal StS2/Godot scene ownership.
