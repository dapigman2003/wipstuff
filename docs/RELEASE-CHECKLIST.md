## Active 0.0.175 / Step 42.0 checks

Require the 0.0.174 Codemagic compile failure to be sealed as provenance and verify the compile-only correction uses `InitializerBearingRequests`, `RejectedManagedRequests`, `NativeLoadAttempts`, and `FormatExceptionDiagnostic` with no shorthand identifiers remaining.

Require physical Step-41 4/4 provenance, exact version/build 0.0.175/175, Step42 source/gates/tests/UI/reports, zero-boundary pre-invocation InitPools map, one-shot MethodInfo invocation, frozen post-invocation confinement, no StartRendering/StopRendering or GameStartup/migration/cloud/platform/Steam/main-menu/deferred call sites in the Step42 UI/Core, clean source archive, and fresh-extraction validator pass.

# Release checklist — Step 42.0 / 0.0.175

Release identity: display/build `0.0.175 (175)`, IPA `StS2-Launcher-Step-42.ipa`, workflow `ios-canonical`.

Before device use, require canonical static validation green; physical Step-39 and Step-40 4/4 provenance sealed; no proprietary StS2 payload; unchanged native Step-15 host; Step41 contains no render-start/stop calls and no GameStartup/platform/main-menu/deferred invocation; exact async-state-machine mapping + path-qualified closure classifier present; deferred/rejecting Cecil only; durable Step41 checkpoint/static-map/report plumbing present; and clean-extraction revalidation of the final ZIP.
