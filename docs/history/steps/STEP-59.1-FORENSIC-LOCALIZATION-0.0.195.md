# Step 59.1 — forensic character-select localization — 0.0.195

Physical **0.0.194 (194)** closed Step 58 and reached Step 59 with the retained real `NSingleplayerSubmenu._stack` already bound to the exact retained `NMainMenuSubmenuStack`; navigation repair was skipped and the retained real `_standardButton` was passed to original `OpenCharacterSelect(NButton)`. The original handler still raised `NullReferenceException` while rendering remained frozen.

Static review of trusted `sts2.dll` establishes that the observed cached character-select pre-state is the **normal game preload state**: `NMainMenuSubmenuStack._Ready()` pre-instantiates `NCharacterSelectScreen`, hides it, and adds it as a child; inherited `NSubmenu._stack` remains null until `Push()`.

0.0.195 therefore does **not** add another guessed repair. Step 59 Gate B records a read-only forensic prerequisite snapshot before handler invocation:

- `NCharacterSelectScreen.IsNodeReady()` and critical `_Ready`-bound fields;
- `NAscensionPanel.IsNodeReady()`;
- NGame hotkey/input/remote-cursor/reaction/timeout/root-scene services;
- existing production `SaveManager.Progress`, `Progress.Epochs`, and `Progress.EncounterStats`;
- `RootSceneContainer.CurrentScene` identity relative to the retained real main menu;
- current `_lobby` state.

If original `OpenCharacterSelect` throws, the reflection boundary records the **inner exception type, TargetSite, Source, and original StackTrace** before propagation. It then records a post-failure snapshot including lobby existence/player count and character-select ownership. `ExceptionDispatchInfo` preserves the inner stack instead of `throw inner` resetting it.

No direct character-select field write, no `InitializeSingleplayer()` substage reproduction, no `ExecuteDeferred()` guess, no full `GameStartup()` call, and no Step 60+ behavior are authorized unless the original handler closes cleanly. Step 63 remains unopened.
