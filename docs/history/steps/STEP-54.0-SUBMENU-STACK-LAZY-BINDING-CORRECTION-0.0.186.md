# Step 54.0 submenu-stack lazy-binding correction / 0.0.186

Physical 0.0.185 retained the frozen Step-52/Step-53 authority and passed Step 54 Gate A. Gate B then failed before the one-shot open was armed because the candidate looked for `_singleplayerSubmenu` directly on `NMainMenu`. The runtime reported `MissingFieldException` for `NMainMenu._singleplayerSubmenu`; rendering remained inactive and the run ended normally.

The supplied game metadata resolves the ownership model: `NMainMenu` exposes `SubmenuStack`, whose runtime type is `NMainMenuSubmenuStack`; that class owns `_singleplayerSubmenu`. Its game documentation states that submenus are lazily spawned when requested. Therefore requiring a non-null submenu instance before `OpenSingleplayerSubmenu()` is both structurally wrong and incompatible with the lazy lifecycle.

0.0.186 makes the narrow correction consistent with those two evidence sources:

- Step 54 Gate B still token-matches the exact zero-argument `OpenSingleplayerSubmenu() -> NSingleplayerSubmenu` method from Step 53.
- Gate B binds exact `NMainMenu.SubmenuStack` with runtime type `NMainMenuSubmenuStack`, then binds exact instance field `NMainMenuSubmenuStack._singleplayerSubmenu : NSingleplayerSubmenu`.
- The field is allowed to be null before the open. Any pre-existing instance is recorded without being created or made visible by the launcher.
- Gate C re-verifies stack identity and the field's pre-invocation identity, then arms the one-shot and invokes only `OpenSingleplayerSubmenu()` once while rendering stays frozen.
- After return, the method return value must be non-null exact `NSingleplayerSubmenu` and reference-identical to `NMainMenu.SubmenuStack._singleplayerSubmenu`. If an instance existed before the call, its identity must remain unchanged.
- The retained submenu must then be inside the tree, `Visible=true`, and `IsVisibleInTree=true`, with the existing context/native no-drift checks unchanged.

No broader frontier is authorized. `SingleplayerButtonPressed` remains uninvoked, Step 55 remains the only new render rung, character select remains non-invoking/read-only through Step 57, and Step 58 remains unopened.
