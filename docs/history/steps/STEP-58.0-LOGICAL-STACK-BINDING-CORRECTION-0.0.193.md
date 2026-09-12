# Step 58.0 logical-stack binding correction — 0.0.193

Physical 0.0.192 reached Step 58 Gate A and stopped safely at Gate B because the cached real `NCharacterSelectScreen` did not have `NSubmenu._stack` reference-identical to the retained `NMainMenuSubmenuStack`. Step 58 itself invoked no handler and never restarted rendering.

The previous ownership check conflated two independent facts: a cached submenu may already be instantiated/attached to the live SceneTree, while its logical `NSubmenu._stack` binding is established by the submenu-stack push path. 0.0.193 therefore treats a null `_stack` as a legitimate pre-push state, rejects any non-null foreign stack, and records tree-parent identity only as evidence. A transition is considered complete only when the real character-select screen is visible/in-tree **and** `_stack` is the exact retained main-menu stack.

If the transition is incomplete, Step 59 still delegates exactly once to the original `OpenCharacterSelect(NButton)` handler under the previously audited frontier. After that handler returns, `_stack` must be the exact retained stack. No launcher-owned `InitializeSingleplayer` or `Push` reproduction is reintroduced. Steps 60–62 remain active-screen audit plus 2-second and 10-second render/refreeze trials; Step 63 remains unopened.
