# StS2 Launcher — Steps 43–47 startup ladder

Active candidate: **0.0.176 (176)** — five independently gated startup rungs in one IPA; stop on the first failure.

Physical **0.0.175 / Step 42** is closed positive **4/4**: exact `NGame.InitPools()` mapped to a 22-method zero-boundary closure, executed once on the real in-tree `NGame`, and returned with state 2, frozen rendering, and zero resolver/host/private/initializer/rejected/native deltas.

0.0.176 keeps that safety model but reduces rebuild cycles. After Step 42 4/4, the same process may advance through: **Step 43 Null-platform authority → Step 44 read-only legacy-data guard → Step 45 one-shot local SaveManager initialization → Step 46 exact `LaunchMainMenu(bool)` async state-machine/closure map → Step 47 conditional one-shot live `LaunchMainMenu(skipIntro=true)`**. Every rung has its own four-gate result and run-correlated reports; later rungs stay locked until the prior rung is 4/4.

Steps 43–46 keep rendering frozen. Step 47 can start rendering only after Step 46's complete map is admissible and durably written. Step 47 refreezes rendering on normal failure/incomplete return and leaves rendering active only after 4/4 success with a new `RootSceneContainer` child scene.

Authoritative status, exact boundaries and device sequence: `docs/CURRENT-STATUS.md`.

0.0.174 never reached device runtime: Codemagic Core compilation failed because the Step-42 source used three non-existent shorthand load-context property names and one non-existent diagnostic helper name. 0.0.175 substitutes only the existing proven members `InitializerBearingRequests`, `RejectedManagedRequests`, `NativeLoadAttempts`, and `FormatExceptionDiagnostic`; Step-42 runtime semantics are otherwise unchanged.
