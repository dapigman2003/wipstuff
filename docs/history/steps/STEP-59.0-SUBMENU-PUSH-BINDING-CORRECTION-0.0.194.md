# Step 59.0 — submenu Push binding correction / 0.0.194

Physical **0.0.193 (193)** retained the complete Step-58 4/4 ownership authority and reached Step 59 Gate B with rendering frozen. The observed character-select cache already existed, was inside the live SceneTree, was hidden, had `NMainMenuSubmenuStack` as its direct Godot parent, and still had a null logical `NSubmenu._stack`. The real `OpenCharacterSelect(NButton)` handler was then armed exactly once and failed immediately with `NullReferenceException`; rendering remained stopped and the process ended normally.

The physically closed Step-56 IL already proves that `OpenCharacterSelect(NButton)` does **not** read its explicit `NButton` parameter. Its first operation instead dereferences `NSingleplayerSubmenu`'s inherited `NSubmenu._stack` and calls `GetSubmenuType<NCharacterSelectScreen>()`. Therefore the 0.0.193 null-button argument is not used as root-cause authority.

The earlier direct Step-54 route intentionally called `NMainMenu.OpenSingleplayerSubmenu()` rather than the normal navigation path. That proved the real single-player submenu could exist, enter the tree, become visible and render, but it did not prove the inherited logical `NSubmenu._stack` binding normally established by submenu-stack navigation. The game metadata also exposes `NSubmenu.SetStack`, and `NSubmenu.OnSubmenuOpened` is documented as occurring when a submenu is newly pushed.

## 0.0.194 correction

Step 59 now checks the retained real `NSingleplayerSubmenu` logical stack before character-select execution:

- a non-null foreign stack is an immediate failure;
- an exact retained `NMainMenuSubmenuStack` means no repair is needed;
- a null stack permits exactly one narrowly bounded, game-owned `NSubmenuStack.Push(NSubmenu)` invocation on the retained stack with the retained real `NSingleplayerSubmenu`;
- the Push method is selected from the exact compatibility image as `void Push(NSubmenu)`, its frontier is audited under the retained runtime guards, and the runtime method is rebound by metadata token;
- after Push returns, the single-player submenu's inherited `_stack` must be reference-identical to the retained `NMainMenuSubmenuStack` before any character-select handler call is allowed;
- the original `OpenCharacterSelect(NButton)` then receives the retained real `_standardButton` rather than a synthetic/null placeholder;
- success still requires the exact game-owned `NCharacterSelectScreen` visible/in-tree and logically bound to that same retained stack.

No launcher code writes `_stack` directly. The repair uses the game's own stack operation and is one-shot once armed. Steps 60–62 remain the active-screen audit, ~2-second visible render/refreeze and ~10-second visible render/refreeze. Character choice, confirm/embark/run-start, continuous interactive ownership and Step 63 remain unopened.
