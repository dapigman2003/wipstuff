# Step 59.9 — 0.0.203 current-ownership pivot and compilation-efficient probes

0.0.203 rejects 0.0.202 as an active runtime candidate after physical Step 54 reproduced a new `NSingleplayerSubmenu.RefreshButtons()` `NullReferenceException`. The 0.0.202 experiment modified shared sts2 methods for Step-59 callsite tracing, so it did not preserve the already-proven pre-Step-59 behavior.

0.0.203 restores `TransformedRealStS2VeryEarlyInitialization.cs` byte-for-byte from the physically proven 0.0.201 candidate and does not add a Step-59 runtime trace bridge or `STEP59TRACE_*` IL markers.

The active architecture is also corrected to match the ownership pivot established by Steps 58–59: legacy Steps 53–57 remain historical/optional diagnostics but are no longer mandatory choreography. After the physically closed path reaches Step 52 and refreezes, Step 58 Gate A directly acquires the current game-owned single-player/character-select ownership surface. It adopts an already-visible `NSingleplayerSubmenu` when present, or invokes the exact game-owned `NMainMenu.OpenSingleplayerSubmenu()` once only if that ownership object must be materialized/activated. It then validates the retained character-select `PackedScene` resource and continues with the existing Step-58 ownership audit.

One 0.0.203 compilation contains multiple separate Step-59 phone experiments: 59D deep diagnostics, 59R controller/singleton rehearsal, 59H isolated `RegisterHotkeys()`, 59U isolated `UpdateControllerButton()`, 59T isolated post-base `NConfirmButton.OnEnable` visual/tween tail, 59E isolated full embark `Enable()`, and the real Step-59 `OpenCharacterSelect` transition. Mutating probes and the real transition require fresh processes.

Step 63 remains unopened.
