# StS2 Launcher — Step 42.0

Active candidate: **0.0.175 (175)** — controlled GameStartup `NGame.InitPools()` boundary; Codemagic compile-only correction of 0.0.174.

Physical **0.0.173 / Step 41** closed the non-invoking GameStartup async frontier map **4/4**: exact `GameStartup` and compiler `MoveNext` were mapped, 691 transitive same-sts2 methods and 46 classified startup/platform boundaries were recorded, unresolved same-sts2 and external Cecil resolution stayed zero, and GameStartup was never invoked.

**Step 42.0 still does not invoke GameStartup.** It keeps rendering frozen and `GameStartupWrapper` inert, maps exact `NGame.InitPools()` plus its own transitive same-sts2 closure, requires zero classified/unresolved boundaries, writes the verified map durably, then invokes exact `InitPools()` once on the retained real in-tree `NGame`. Final confinement requires state 2 and zero resolver/host/private/initializer/rejected/native deltas. Migrations, cloud sync, platform, Steam, main-menu, deferred startup and render restart remain unopened. Once Gate C is armed, do not retry Step 42 in-process.

Authoritative status and exact device sequence: `docs/CURRENT-STATUS.md`.

0.0.174 never reached device runtime: Codemagic Core compilation failed because the Step-42 source used three non-existent shorthand load-context property names and one non-existent diagnostic helper name. 0.0.175 substitutes only the existing proven members `InitializerBearingRequests`, `RejectedManagedRequests`, `NativeLoadAttempts`, and `FormatExceptionDiagnostic`; Step-42 runtime semantics are otherwise unchanged.
