# Step 59.3 — unique-name provenance forensics — 0.0.198

Physical **0.0.197 (197)** closed the 0.0.196 harness regression and produced the intended Step-59 ready-binding map.

The result materially changes the diagnosis:

- the exact live `AscensionPanel` node exists;
- its runtime type is the exact selected
  `MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NAscensionPanel`;
- it is inside the tree and reports `IsNodeReady()==true`;
- its owner is the retained character-select root;
- **its live `UniqueNameInOwner` is false**;
- the exact receipt-backed parent `character_select_screen.tscn` declares the instanced root with
  `unique_name_in_owner = true`;
- a nested unique node declared inside `ascension_panel.tscn` (`AscensionIcon`) remains
  `UniqueNameInOwner=true` at runtime.

Trusted `sts2.dll` IL independently proves `NCharacterSelectScreen._Ready()` asks Godot for
`GetNode<NAscensionPanel>("%AscensionPanel")` immediately after successfully storing `_charButtonContainer`.
The physical null `_ascensionPanel` therefore has a concrete mechanism: the `%AscensionPanel` unique-name
lookup cannot resolve the live instanced root when that root no longer carries its parent-scene unique flag.

The same physical preflight also shows `_actDropdown == null` and `NGame.TimeoutOverlay == null`, while several
ordinary NGame manager/container bindings are healthy. This raises a systemic hypothesis: **parent-scene
overrides on instantiated subscene roots may be losing `unique_name_in_owner` during resource/scene
instantiation**, while unique flags declared inside the subscene itself survive.

0.0.198 remains diagnostic and does not set any unique-name flag, write `_ascensionPanel`, invoke `_Ready()`
manually, or arm `OpenCharacterSelect` when the preflight is unhealthy. It compares:

1. exact receipt-backed TSCN declarations;
2. the retained `PackedScene.GetState()` serialized/overridden properties;
3. a temporary **off-tree** `PackedScene.Instantiate(GenEditState.Disabled)` clone that is never admitted to the
   SceneTree and is released after inspection;
4. the retained live node;
5. non-generic direct-path and `%Name` lookups against the same object graph.

The comparison is performed for character-select targets (`AscensionPanel`, `ActDropdown`) and for representative
NGame/main-menu nodes, including ordinary unique nodes and instantiated-subscene roots.

Interpretation:

- TSCN true + SceneState missing/false -> resource parser/cache layer;
- SceneState true + temporary off-tree clone false -> PackedScene instantiation/application layer;
- temporary clone true + retained live false -> later tree/owner/lifecycle mutation;
- live flag true + `%Name` lookup fails -> lookup/registration/managed interop layer;
- ordinary unique nodes true while instantiated roots false -> systemic instanced-root override loss.

No gameplay or Step 63 boundary is opened.
