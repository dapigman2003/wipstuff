# Step 59.2 — ascension ready-binding forensics — 0.0.196

Physical **0.0.195 (195)** changed the diagnosis materially. Step 59 Gate A passed, but Gate B stopped before any
`NSubmenuStack.Push` or `OpenCharacterSelect` arm because the retained real `NCharacterSelectScreen` reported
`IsNodeReady() == true` while `_ascensionPanel == null`. The same preflight had already read
`_charButtonContainer` successfully, so the failure is no longer a generic `InitializeSingleplayer` prerequisite.

Static review of the trusted original `sts2.dll`
(SHA-256 `e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18`) proves:

- `_ascensionPanel` is a normal private runtime field, **not** a `[Godot.Export]` field;
- `NCharacterSelectScreen._Ready()` assigns `_charButtonContainer` with a `GetNode(...)` lookup;
- immediately afterward it loads the literal `%AscensionPanel`, calls the generic Godot node lookup for
  `NAscensionPanel`, and stores the result into `_ascensionPanel`;
- later in the same `_Ready()` it dereferences `_ascensionPanel` to connect the ascension-level signal.

Therefore the current root cause is localized to the character-select `_Ready` **ascension unique-name/type binding
boundary**. `IsNodeReady()==true` does not prove the managed `_Ready()` body completed successfully; Godot can have
delivered the ready notification while a managed callback failed or left a typed lookup null.

0.0.196 remains diagnostic and performs no direct field repair. Step 59 Gate B now:

1. records all critical character-select fields without failing at the first null;
2. re-verifies the selected compatibility image's `_Ready()` IL ordering around
   `_charButtonContainer -> "%AscensionPanel" -> _ascensionPanel`;
3. inventories the live character-select descendants whose names/types contain `Ascension`, recording runtime type,
   selected/private load-context identity, `IsInsideTree`, `IsNodeReady`, `UniqueNameInOwner`, and owner identity;
4. extracts the exact receipt-backed
   `res://scenes/screens/character_select_screen.tscn` and
   `res://scenes/screens/ascension_panel.tscn` bytes read-only from the sealed PCK and records the relevant TSCN
   declarations;
5. durably writes that forensic map **before Gate C**.

If any required ready binding remains unhealthy, Gate C returns a safe failure before setting the one-shot transition
flag and before invoking either the submenu Push repair or `OpenCharacterSelect`.

This distinguishes the remaining major causes in one physical run:

- **no AscensionPanel descendant** -> nested scene/resource instantiation failure;
- **descendant exists but wrong/base/foreign type** -> managed script/type binding or AssemblyLoadContext identity issue;
- **exact selected `NAscensionPanel` exists but `UniqueNameInOwner`/owner is wrong** -> Godot scene unique-name ownership issue;
- **exact selected node + correct unique-name/owner but field is null** -> managed `GetNode<T>("%AscensionPanel")`
  dispatch/cast behavior must be localized next.

No launcher code writes `_ascensionPanel`, calls `_Ready()` manually, or mutates scene ownership in 0.0.196.
