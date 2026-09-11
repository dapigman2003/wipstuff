# Step 56.0 NButton signature correction / 0.0.187

Physical 0.0.186 reached Step 56 from a retained Step-55 4/4 rendered/refrozen `NSingleplayerSubmenu` authority. Step 56 Gate A passed, proving the same-process Step-55 closure and frozen renderer. Gate B then failed before any character-select invocation because the candidate required zero-argument `void OpenCharacterSelect()`, while the selected real `sts2.dll` reports one parameter and a `void` return. The run ended normally with rendering still frozen; `OpenCharacterSelect` was never invoked.

The supplied trusted game assembly (`sts2.dll` SHA-256 `e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18`) resolves the exact method signature as:

`MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NSingleplayerSubmenu.OpenCharacterSelect(MegaCrit.Sts2.Core.Nodes.GodotExtensions.NButton) -> System.Void`

The parameter metadata name is `_`; it is the Godot button callback argument. 0.0.187 makes only the narrow signature correction:

- Step 56 Gate B requires exactly one parameter of exact managed type `MegaCrit.Sts2.Core.Nodes.GodotExtensions.NButton` and exact return type `System.Void`.
- The exact method token is retained and the static map records the parameter type alongside token/return/IL evidence.
- Step 56 Gate C rechecks the exact same signature before token comparison and frontier mapping, so the runtime map cannot silently drift to an overload or different callback contract.
- `OpenCharacterSelect` remains strictly non-invoking. Step 56 still only maps IL/frontiers/resource literals while rendering is frozen.
- Step 57 remains read-only PCK inspection only. No `ResourceLoader`, `PackedScene`, character-select admission, rendering, or Step 58 behavior is added.

Because physical 0.0.186 passed Step 56 Gate A from exact Step-55 closure, physical authority is now recorded through **Step 55 4/4/refrozen**. The one-button physically closed path intentionally remains capped at Step 52; Steps 53–55 must still be re-run in the same fresh process before Step 56.
